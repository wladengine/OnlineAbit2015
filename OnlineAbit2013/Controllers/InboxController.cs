using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineAbit2013.Models;
using System.Data;
using System.Web.Routing;
using OnlineAbit2013.EMDX;
using System.Drawing; 

namespace OnlineAbit2013.Controllers
{
    public class InboxController : Controller
    {
        public ActionResult Index(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");
            
            InboxDialogList model = new InboxDialogList();
            model.Photolst = new List<OperatorProfilePhoto>();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var dialogs = (from u in context.InboxDialogUsers
                               join d in context.InboxDialog on u.DialogId equals d.Id

                               join s in context.InboxDialogStatus on d.StatusId equals s.Id into _s
                               from s in _s.DefaultIfEmpty()

                               where u.UserId == personId
                               select new ShortDialog
                               {
                                   Id = d.Id,
                                   Theme = d.Theme,
                                   StatusId = d.StatusId,
                                   StatusName = s.Name,
                               }).ToList();
                foreach (var d in dialogs)
                {
                    d.LastMes = new DialogMessage();
                    var mes = context.InboxMessage.Where(x => x.DialogId == d.Id).OrderByDescending(x => x.DateTime).Select(x => x).FirstOrDefault();
                    if (mes != null)
                    {
                        d.LastMes.UserId = mes.UserId;
                        d.LastMes.Author = context.Person.Where(x => x.UserId == mes.UserId).Select(x => x.Name).FirstOrDefault();
                        d.LastMes.Text = mes.Text;
                        d.LastMes.time = mes.DateTime;
                        d.LastMes.HasFiles = context.InboxMessageFile.Where(x => x.MessageId == mes.Id).Count() > 0;
                        if (model.Photolst.Where(x => x.UserId == mes.UserId).Count() == 0)
                        {
                            model.Photolst.Add(new OperatorProfilePhoto() { 
                            UserId = mes.UserId,
                            bPhoto = InboxSupportFunction.GetPhoto(mes.UserId),
                            imgPhoto = InboxSupportFunction.GetPhotoProfile(mes.UserId) 
                        });
                        }
                        d.HasAnswer = mes.UserId != personId; 
                    }
                    d.isRead = context.InboxMessage.Where(x => x.DialogId == d.Id && x.UserId != personId && !x.IsRead).Count() == 0;
                }
                model.DialogList = dialogs;
            } 
            return View(model);
        }

        public ActionResult CreateDialog()
        {
            NewDialog model = new NewDialog();
            model.Files = new List<HttpPostedFileBase>();
            return View(model);
        }

        [HttpPost]
        public ActionResult SendQuestion(NewDialog model)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            if (String.IsNullOrEmpty(model.Theme) || String.IsNullOrEmpty(model.Text) || String.IsNullOrEmpty(model.Theme.Trim()) || String.IsNullOrEmpty(model.Text.Trim()))
            {
                model.Error = "Введите тему и сообщение";
                return View("createdialog",model);
            }
            else
            {
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    Guid DialogId = Guid.NewGuid();
                    context.InboxDialog.Add(new InboxDialog()
                    {
                        Id = DialogId,
                        Theme = model.Theme.Trim(),
                        HasAnswer = false,
                        StatusId = 1,
                    });
                    context.SaveChanges();
                    Guid MessageId = Guid.NewGuid();
                    context.InboxMessage.Add(new InboxMessage()
                    {
                        Id = MessageId,
                        Text = model.Text.Trim(),
                        UserId = personId,
                        DialogId = DialogId,
                    });
                    context.SaveChanges();
                    context.InboxDialogUsers.Add(new InboxDialogUsers()
                        {
                            DialogId = DialogId,
                            UserId = personId,
                        });
                    context.SaveChanges();

                    foreach (var file in model.Files)
                    {
                        if (file == null || file.ContentLength == 0)
                            continue;
                        var fileName = System.IO.Path.GetFileName(file.FileName);
                        int lastSlashPos = 0;
                        lastSlashPos = fileName.LastIndexOfAny(new char[] { '\\', '/' });
                        if (lastSlashPos > 0)
                            fileName = fileName.Substring(lastSlashPos);

                        int fileSize = file.ContentLength;
                        byte[] fileData = new byte[fileSize];

                        file.InputStream.Read(fileData, 0, fileSize);
                        string fileext = "";
                        try
                        {
                            fileext = fileName.Substring(fileName.LastIndexOf('.'));
                        }
                        catch
                        {
                            fileext = "";
                        }

                        try
                        {
                            Guid FileId = Guid.NewGuid();
                            string query = "INSERT INTO dbo.InboxMessageFile (Id, MessageId, FileName, FileSize,MimeType, FileExtention, LoadDate) " +
                                " VALUES (@Id, @MessageId, @FileName, @FileSize, @MimeType, @FileExtention, @LoadDate);"
                                + "\n INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);";
                            SortedList<string, object> dic = new SortedList<string, object>();
                            dic.Add("@Id", FileId);
                            dic.Add("@MessageId", MessageId);
                            dic.Add("@FileName", fileName);
                            dic.Add("@FileData", fileData);
                            dic.Add("@FileSize", fileSize);
                            dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));
                            dic.Add("@FileExtention", fileext);
                            dic.Add("@LoadDate", DateTime.Now);

                            Util.AbitDB.ExecuteQuery(query, dic);
                        }
                        catch
                        {
                            return Json("Ошибка при записи файла");
                        }
                    }

                    return RedirectToAction("Dialog", "Inbox", new RouteValueDictionary() { { "id", DialogId } });
                }
            }
        }

        public ActionResult Dialog(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            Guid gDialogId;
            if (!Guid.TryParse(id, out gDialogId))
                return RedirectToAction("Index");

            Dialog model = new Dialog();
            model.Photolst = new List<OperatorProfilePhoto>();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                model.DialogId = gDialogId.ToString();
                model.Theme = context.InboxDialog.Where(x=>x.Id == gDialogId).Select(x=>x.Theme).FirstOrDefault();
                model.Messages = (from d in context.InboxDialog
                              join m in context.InboxMessage on d.Id equals m.DialogId
                              join p in context.Person on m.UserId equals p.Id
                              where d.Id == gDialogId
                              select new DialogMessage
                              {
                                  Id = m.Id,
                                  UserId = m.UserId,
                                  Mine = m.UserId == personId,
                                  Author = p.Name,
                                  Text = m.Text,
                                  time = m.DateTime,
                                  isRead = m.IsRead,
                              }).OrderByDescending(x=>x.time).ToList(); 
                foreach (var mes in model.Messages)
                {
                    if (model.Photolst.Where(x=>x.UserId == mes.UserId).Count()==0)
                    {
                        model.Photolst.Add(new OperatorProfilePhoto() { 
                            UserId = mes.UserId,
                            bPhoto = InboxSupportFunction.GetPhoto(mes.UserId),
                            imgPhoto = InboxSupportFunction.GetPhotoProfile(mes.UserId) 
                        });
                    }
                    mes.Files = (from f in context.InboxMessageFile 
                                 where f.MessageId == mes.Id
                                 select new FileInfo {
                                     Id = f.Id,
                                     FileName = f.FileName,
                                 }).ToList();
                }
                var readmes = context.InboxMessage.Where(x => x.DialogId == gDialogId && x.UserId != personId).Select(x => x);
                foreach (var mes in readmes)
                    mes.IsRead = true;
                context.SaveChanges();
            } 
            return View(model);
        }

        [HttpPost]
        public ActionResult SendMessage(Dialog Model)
        {
            var OK = false;
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return Json(OK);

            Guid gDialogId;
            if (!Guid.TryParse(Model.DialogId, out gDialogId))
                return Json(OK);
            
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                InboxDialog Dialog = context.InboxDialog.Where(x => x.Id == gDialogId).FirstOrDefault();
                if (Dialog == null)
                    return Json(OK);

                if (context.InboxDialogUsers.Where(x=>x.DialogId == gDialogId && x.UserId == personId).Count()==0)
                    return Json(OK);
                Guid MessageId = Guid.NewGuid();
                context.InboxMessage.Add(new InboxMessage()
                {
                    Id = MessageId,
                    UserId = personId,
                    Text = Model.NewMessage,
                    DialogId = gDialogId,
                });

                Dialog.HasAnswer = false;
                context.SaveChanges();

                foreach (var file in Model.Files)
                {
                    if (file == null || file.ContentLength == 0)
                        continue;
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    int lastSlashPos = 0;
                    lastSlashPos = fileName.LastIndexOfAny(new char[] { '\\', '/' });
                    if (lastSlashPos > 0)
                        fileName = fileName.Substring(lastSlashPos);

                    int fileSize = file.ContentLength;
                    byte[] fileData = new byte[fileSize];

                    file.InputStream.Read(fileData, 0, fileSize);
                    string fileext = "";
                    try
                    {
                        fileext = fileName.Substring(fileName.LastIndexOf('.'));
                    }
                    catch
                    {
                        fileext = "";
                    }

                    try
                    {
                        Guid FileId = Guid.NewGuid();
                        string query = "INSERT INTO dbo.InboxMessageFile (Id, MessageId, FileName, FileSize,MimeType, FileExtention, LoadDate) " +
                            " VALUES (@Id, @MessageId, @FileName, @FileSize, @MimeType, @FileExtention, @LoadDate);"
                            + "\n INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);";
                        SortedList<string, object> dic = new SortedList<string, object>();
                        dic.Add("@Id", FileId);
                        dic.Add("@MessageId", MessageId);
                        dic.Add("@FileName", fileName);
                        dic.Add("@FileData", fileData);
                        dic.Add("@FileSize", fileSize);
                        dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));
                        dic.Add("@FileExtention", fileext);
                        dic.Add("@LoadDate", DateTime.Now);

                        Util.AbitDB.ExecuteQuery(query, dic);
                    }
                    catch
                    {
                        return Json("Ошибка при записи файла");
                    }
                }
            }
            return RedirectToAction("Dialog", "Inbox", new RouteValueDictionary() { { "id", gDialogId } });
        } 

        public ActionResult GetFile(string id)
        {
            Guid FileId = new Guid();
            if (!Guid.TryParse(id, out FileId))
                return Content("Некорректный идентификатор файла");

            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Content("Authorization required");

            SortedList<string, object> slParams = new SortedList<string, object>();
            slParams.Add("@PersonId", PersonId);
            slParams.Add("@Id", FileId);

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT FileName, MimeType, FileExtention FROM InboxMessageFile WHERE Id=@Id", slParams);

            if (tbl.Rows.Count == 0)
                return Content("Файл не найден");

            string fileName = tbl.Rows[0].Field<string>("FileName");
            string contentType = tbl.Rows[0].Field<string>("MimeType");
            string ext = tbl.Rows[0].Field<string>("FileExtention");

            byte[] content = (byte[])Util.AbitDB.GetValue("SELECT FileData FROM FileStorage WHERE Id=@Id", slParams);
            if (content == null)
                return Content("Файл не найден");

            if (string.IsNullOrEmpty(contentType))
            {
                if (string.IsNullOrEmpty(ext))
                    contentType = "application/octet-stream";
                else
                    contentType = Util.GetMimeFromExtention(ext);
            }
            bool openMenu = true;
            if (ext.IndexOf("jpg", StringComparison.OrdinalIgnoreCase) != -1)
                openMenu = false;
            if (ext.IndexOf("jpeg", StringComparison.OrdinalIgnoreCase) != -1)
                openMenu = false;
            if (ext.IndexOf("gif", StringComparison.OrdinalIgnoreCase) != -1)
                openMenu = false;
            if (ext.IndexOf("png", StringComparison.OrdinalIgnoreCase) != -1)
                openMenu = false;

            try
            {
                if (openMenu)
                    return File(content, contentType, fileName);
                else
                    return File(content, contentType);
            }
            catch
            {
                return Content("Ошибка при чтении файла");
            }
        }

        public ActionResult GetCountMessages()
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return Json(new { OK = false, Error = "Некорректный guid" });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                int MyCnt = (from u in context.InboxDialogUsers
                             join x in context.InboxDialog on u.DialogId equals  x.Id 
                             join m in context.InboxMessage on x.Id equals m.DialogId
                             where u.UserId == personId
                             && m.UserId != personId && !m.IsRead
                             select x.Id).Distinct().Count();


                return Json(new { MyCnt = MyCnt });
            }
        }
    }

    public static class InboxSupportFunction
    {
        public static string GetPhoto(Guid userid)
        {
            DataTable tbl = Util.AbitDB.GetDataTable(@"
SELECT top 1 PersonFile.Id FROM dbo.PersonFile WHERE PersonId=@PersonId and PersonFileTypeId=14 
UNION ALL
SELECT top 1 ApplicationFile.Id FROM dbo.ApplicationFile 
INNER JOIN dbo.Application AP ON AP.Id = ApplicationFile.ApplicationId
WHERE AP.PersonId=@PersonId and FileTypeId=18
UNION ALL
SELECT top 1 ApplicationFile.Id FROM dbo.ApplicationFile 
INNER JOIN dbo.Application AP ON AP.CommitId = ApplicationFile.CommitId
WHERE AP.PersonId=@PersonId and FileTypeId=18 
",
            new SortedList<string, object>() { { "@PersonId", userid } });

            if (tbl.Rows.Count > 0)
            {
                Guid fileId = tbl.Rows[0].Field<Guid>("Id");
                DataTable t_fd = Util.AbitDB.GetDataTable("select FileData from dbo.FileStorage where Id =@Id", new SortedList<string, object>() { { "@Id", fileId } });
                if (t_fd.Rows.Count == 0)
                    return "";
                byte[] content = t_fd.Rows[0].Field<byte[]>("FileData");

                using (var ms = new System.IO.MemoryStream(content))
                {
                    Image img = Image.FromStream(ms);
                    int w = img.Width;
                    int h = img.Height;
                    System.Drawing.Rectangle cropRect;
                    if (w > h)
                        cropRect = new System.Drawing.Rectangle(w / 2 - h / 2, 0, h, h);
                    else
                        cropRect = new System.Drawing.Rectangle(0, h / 2 - w / 2, w, w);

                    Bitmap source = img as Bitmap;
                    Bitmap target = new Bitmap(120, 120);
                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                        g.DrawImage(source,
                                    new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                    }
                    using (var stream = new System.IO.MemoryStream())
                    {
                        target.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return Convert.ToBase64String(stream.ToArray());
                    }
                }
            }
            else  return "";
        }
   
        public static string GetPhotoProfile(Guid userid)
        {
            DataTable tbl = Util.AbitDB.GetDataTable(
@"select Name from dbo.SupportOperatorProfile Profile 
join dbo.SupportProfilePhoto Photo on Profile.ProfilePhotoId = Photo.Id
where PersonId = @PersonId", new SortedList<string, object>() { { "@PersonId", userid } });
            if (tbl.Rows.Count > 0)
            {
                return tbl.Rows[0].Field<string>("Name");
            }
            else
                return "";
        }
    }

}
