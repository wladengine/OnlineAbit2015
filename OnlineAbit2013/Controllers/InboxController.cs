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
 
//using System.IO;



namespace OnlineAbit2013.Controllers
{
    public class InboxController : Controller
    {
        public bool CheckUserGroup(Guid personId)
        {
            DataTable tblGroup = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.SupportOperatorsGroupId } });
            if (tblGroup.Rows.Count == 0)
                return false;
            else
                return true;
        }

        public RedirectToRouteResult Check(ref Guid personId)
        {
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            if (CheckUserGroup(personId))
                return RedirectToAction("Index", "Support");

            return null;
        }

        public ActionResult Index(string id)
        {
            Guid personId = Guid.Empty;
            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;
            
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
                            Photo = InboxSupportFunction.GetPhoto(mes.UserId),
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
            Guid personId = Guid.Empty;
            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            NewDialog model = new NewDialog();
            model.Files = new List<HttpPostedFileBase>();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SendQuestion(NewDialog model)
        {
            Guid personId = Guid.Empty;
            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

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
                        Theme = HttpUtility.HtmlEncode(model.Theme.Trim()),
                        HasAnswer = false,
                        StatusId = 1,
                    });
                    context.SaveChanges();
                    Guid MessageId = Guid.NewGuid();
                    context.InboxMessage.Add(new InboxMessage()
                    {
                        Id = MessageId,
                        Text = HttpUtility.HtmlEncode(model.Text.Trim()),
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
            Guid personId = Guid.Empty;
            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            Guid gDialogId;
            if (!Guid.TryParse(id, out gDialogId))
                return RedirectToAction("Index");

            Dialog model = new Dialog();
            model.Partial.Photolst = new List<OperatorProfilePhoto>();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                model.DialogId = gDialogId.ToString();
                model.Theme = context.InboxDialog.Where(x=>x.Id == gDialogId).Select(x=>x.Theme).FirstOrDefault();
                model.Partial.Messages = (from d in context.InboxDialog
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
                foreach (var mes in model.Partial.Messages)
                {
                    if (model.Partial.Photolst.Where(x => x.UserId == mes.UserId).Count() == 0)
                    {
                        model.Partial.Photolst.Add(new OperatorProfilePhoto()
                        { 
                            UserId = mes.UserId,
                            Photo = InboxSupportFunction.GetPhoto(mes.UserId),
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

        [ValidateInput(false)]
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
                    Text = HttpUtility.HtmlEncode(Model.PartialNewMessage.NewMessage),
                    DialogId = gDialogId,
                });

                Dialog.HasAnswer = false;
                context.SaveChanges();

                foreach (var file in Model.PartialNewMessage.Files)
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
            Guid FileId = Guid.Empty;
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
        public ActionResult GetPhotoProfile(string sid)
        {
            InboxSupportFunction x = new InboxSupportFunction();
            return x.GetFile(sid);
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
                

                return Json(new { MyCnt = MyCnt});
            }
        }
        public ActionResult CheckNewMessages(string gId)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return Json(new { OK = false, Error = "Некорректный guid" });

            Guid dialogId;
            if (!Guid.TryParse(gId, out dialogId))
                return Json(new { OK = false, Error = "Некорректный guid" });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                 var msg = (from u in context.InboxDialogUsers
                           join x in context.InboxDialog on u.DialogId equals x.Id
                           join m in context.InboxMessage on x.Id equals m.DialogId
                           join p in context.Person on m.UserId equals p.Id
                           where u.UserId == personId
                           && m.UserId != personId && !m.IsRead
                           && x.Id == dialogId
                           select new 
                           {
                               Id = m.Id,
                               UserId = m.UserId,
                               Mine = m.UserId == personId,
                               Author = p.Name,
                               Text = m.Text,
                               date = m.DateTime,
                               isRead = m.IsRead,
                           }).OrderBy(x=>x.date).ToList()
                             .Select(x => new
                             {
                                 Id = x.Id,
                                 UserId = x.UserId,
                                 Mine = x.Mine,
                                 Author = x.Author,
                                 Text = x.Text,
                                 date = x.date.ToShortDateString(),
                                 time = x.date.ToShortTimeString(),
                                 isRead = x.isRead,
                                 Files = (from f in context.InboxMessageFile
                                          where f.MessageId == x.Id
                                          select new FileInfo
                                          {
                                              Id = f.Id,
                                              FileName = f.FileName,
                                          }).ToList(),
                                 Photo = InboxSupportFunction.GetPhoto(x.UserId),
                             }).ToList();
                
                return Json(new { MyCnt = msg });
            }
        }
        public ActionResult SetIsRead(string gId)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return Json(new { OK = false, Error = "Некорректный guid" });

            Guid dialogId;
            if (!Guid.TryParse(gId, out dialogId))
                return Json(new { OK = false, Error = "Некорректный guid" });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var msg = (from u in context.InboxDialogUsers
                                           join x in context.InboxDialog on u.DialogId equals x.Id
                                           join m in context.InboxMessage on x.Id equals m.DialogId
                                           join p in context.Person on m.UserId equals p.Id
                                           where u.UserId == personId
                                           && m.UserId != personId && !m.IsRead
                                           && x.Id == dialogId
                                           select m.Id).ToList();
                foreach (var mes in msg)
                {
                    var m = context.InboxMessage.Where(x => x.Id == mes).FirstOrDefault();
                    if (m != null)
                        m.IsRead = true;
                    context.SaveChanges();
                }
                context.SaveChanges();

                return Json(new { ok = true });
            }
        }
    }

    public class InboxSupportFunction : Controller
    {
        public static string GetPhoto(Guid userid)
        {
            DataTable tbl = Util.AbitDB.GetDataTable(@"
SELECT top 1 PersonFile.ShortId, FileExtention FROM dbo.PersonFile WHERE PersonId=@PersonId and PersonFileTypeId=14 and ISDeleted = 0
UNION ALL
SELECT top 1 ApplicationFile.ShortId, FileExtention FROM dbo.ApplicationFile 
INNER JOIN dbo.Application AP ON AP.Id = ApplicationFile.ApplicationId
WHERE AP.PersonId=@PersonId and FileTypeId=18 and ApplicationFile.ISDeleted = 0
UNION ALL
SELECT top 1 ApplicationFile.ShortId, FileExtention FROM dbo.ApplicationFile 
INNER JOIN dbo.Application AP ON AP.CommitId = ApplicationFile.CommitId
WHERE AP.PersonId=@PersonId and FileTypeId=18 and ApplicationFile.ISDeleted = 0
",
            new SortedList<string, object>() { { "@PersonId", userid } });

            if (tbl.Rows.Count > 0)
            {
                return "/images/"+tbl.Rows[0].Field<string>("ShortId") + tbl.Rows[0].Field<string>("FileExtention");
            }
            else return GetPhotoProfile(userid);
        }
        public static string GetPhotoProfile(Guid userid)
        {
            DataTable tbl = Util.AbitDB.GetDataTable(
@"select Name from dbo.SupportOperatorProfile Profile 
join dbo.SupportProfilePhoto Photo on Profile.ProfilePhotoId = Photo.Id
where PersonId = @PersonId", new SortedList<string, object>() { { "@PersonId", userid } });
            if (tbl.Rows.Count > 0)
            {
                return "../../"+tbl.Rows[0].Field<string>("Name");
            }
            else
                return "../../Content/themes/base/images/user_no_photo.png";
        }

        public ActionResult GetFile(string sid)
        {
            SortedList<string, object> slParams = new SortedList<string, object>();
            sid = sid.Substring(0, sid.IndexOf('.'));
            slParams.Add("@sId", sid);

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT Id, FileName, MimeType, FileExtention FROM AllFiles WHERE ShortId=@sId", slParams);
            if (tbl.Rows.Count ==0)
                return Content("Ошибка при чтении файла");

            Guid fId = tbl.Rows[0].Field<Guid>("Id");
            slParams.Add("@Id", fId);
            string fileName = tbl.Rows[0].Field<string>("FileName");
            string contentType = tbl.Rows[0].Field<string>("MimeType");
            string ext = tbl.Rows[0].Field<string>("FileExtention");

            byte[] content = (byte[])Util.AbitDB.GetValue("SELECT FileData FROM FileStorage WHERE Id=@Id", slParams);
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
                            return File(stream.ToArray(), contentType, fileName);
                        else
                            return File(stream.ToArray(), contentType);
                    }
                    catch
                    {
                        return Content("Ошибка при чтении файла");
                    }
                }
            }
        }
    }

}
