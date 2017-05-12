using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineAbit2013.Models;
using System.Data;
using System.Web.Routing;
using OnlineAbit2013.EMDX;

namespace OnlineAbit2013.Controllers
{
    public class SupportController : Controller
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

            if (!CheckUserGroup(personId))
                return RedirectToAction("Main", "Abiturient");

            return null;
        }
        

        public ActionResult Index(string id)
        {
            Guid personId = Guid.Empty;
           
            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            SupportDialogList model = new SupportDialogList();
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
                                   HasAnswer = d.HasAnswer,
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
                        if (model.Photolst.Where(x => x.UserId == mes.UserId).Count() == 0)
                        {
                            model.Photolst.Add(new OperatorProfilePhoto()
                            {
                                UserId = mes.UserId,
                                Photo = InboxSupportFunction.GetPhoto(mes.UserId),
                            });
                        }
                    }
                    d.isRead = context.InboxMessage.Where(x => x.DialogId == d.Id && x.UserId != personId && !x.IsRead).Count() == 0;
                }
                model.MyDialogList = dialogs;
                var newdialogs = (from d in context.InboxDialog
                                  join s in context.InboxDialogStatus on d.StatusId equals s.Id
                                  where d.StatusId == 1
                                  select new ShortDialog
                                  {
                                      Id = d.Id,
                                      Theme = d.Theme,
                                      StatusId = d.StatusId,
                                      StatusName = s.Name,
                                      HasAnswer = d.HasAnswer,
                                  }).ToList();
                foreach (var d in newdialogs)
                {
                    d.LastMes = new DialogMessage();
                    var mes = context.InboxMessage.Where(x => x.DialogId == d.Id).OrderByDescending(x => x.DateTime).Select(x => x).FirstOrDefault();
                    if (mes != null)
                    {
                        d.LastMes.UserId = mes.UserId;
                        d.LastMes.Author = context.Person.Where(x => x.UserId == mes.UserId).Select(x => x.Name).FirstOrDefault();
                        d.isRead = mes.IsRead;
                        d.LastMes.Text = mes.Text;
                        d.isRead = mes.IsRead;
                        d.LastMes.time = mes.DateTime;
                        if (model.Photolst.Where(x => x.UserId == mes.UserId).Count() == 0)
                        {
                            model.Photolst.Add(new OperatorProfilePhoto()
                            {
                                UserId = mes.UserId,
                                Photo = InboxSupportFunction.GetPhoto(mes.UserId),
                            });
                        }
                    }
                    else
                    {
 
                    }
                }
                model.NewDialogList = newdialogs;
            } 
            return View(model);
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

            SupportDialog model = new SupportDialog();
            model.Partial.Photolst = new List<OperatorProfilePhoto>();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                model.IsNew = (context.InboxDialogUsers.Where(x => x.DialogId == gDialogId).Count() == 1);
                model.IsMine = (context.InboxDialogUsers.Where(x => x.DialogId == gDialogId && x.UserId == personId).Count() >0);
                
                model.DialogId = gDialogId.ToString();
                model.Theme = context.InboxDialog.Where(x=>x.Id == gDialogId).Select(x=>x.Theme).FirstOrDefault();
                model.Partial.Messages = (from d in context.InboxDialog
                              join m in context.InboxMessage on d.Id equals m.DialogId
                              join p in context.Person on m.UserId equals p.Id
                              where d.Id == gDialogId
                              select new DialogMessage
                              {
                                  Id = m.Id,
                                  Mine = m.UserId == personId,
                                  UserId = m.UserId,
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
            Guid personId = Guid.Empty;
            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            Guid gDialogId;
            if (!Guid.TryParse(Model.DialogId, out gDialogId))
                return Json(OK);
            
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                InboxDialog Dialog = context.InboxDialog.Where(x => x.Id == gDialogId).FirstOrDefault();
                if (Dialog == null)
                    return Json(OK);

                Guid MessageId = Guid.NewGuid();
                context.InboxMessage.Add(new InboxMessage()
                {
                    Id = MessageId,
                    UserId = personId,
                    Text = HttpUtility.HtmlEncode(Model.PartialNewMessage.NewMessage),
                    DialogId = gDialogId,
                });

                Dialog.HasAnswer = true;
                Dialog.StatusId = 2;
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
            return RedirectToAction("Dialog", "Support", new RouteValueDictionary() { { "id", gDialogId } });
        }

        public ActionResult SubmitQuestion(string dId)
        {
            Guid personId = Guid.Empty;

            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            Guid gDialogId;
            if (!Guid.TryParse(dId, out gDialogId))
                return Json(new { OK = false, Error = "Некорректный guid" });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                InboxDialog Dialog = context.InboxDialog.Where(x => x.Id == gDialogId).FirstOrDefault();
                if (Dialog == null)
                    return Json(new { OK = false, Error = "Вопрос не найден" });

                if (context.InboxDialogUsers.Where(x => x.DialogId == gDialogId).Count() == 1)
                {
                    Dialog.StatusId = 2; // просмотрено
                    context.InboxDialogUsers.Add(new InboxDialogUsers()
                        {
                            DialogId = gDialogId,
                            UserId = personId,
                        });
                    context.SaveChanges();
                    return Json(new { OK = true, Error = "" });
                }
                else
                    return Json(new { OK = false, Error = "На этот вопрос уже отвечает другой оператор" });
            }
        }

        public ActionResult GetCountMessages()
        {
            Guid personId = Guid.Empty;

            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                int MyCnt = (from x in context.InboxDialog
                               join u in context.InboxDialogUsers on x.Id equals u.DialogId
                               where u.UserId == personId && !x.HasAnswer
                               select x).Count();

                int NewCnt = (from x in context.InboxDialog
                             where x.StatusId == 1 && !x.HasAnswer
                             select x).Count();

                return Json(new { MyCnt = MyCnt, NewCnt = NewCnt, All = MyCnt + NewCnt });
            }
        }

        public ActionResult ProfilePhoto()
        {
            Guid personId = Guid.Empty;

            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            SupportOperator model = new SupportOperator();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                model.Name = context.Person.Where(x => x.Id == personId).Select(x => x.Name).FirstOrDefault();
                Guid? Photo = context.SupportOperatorProfile.Where(x => x.PersonId == personId).Select(x => x.ProfilePhotoId).FirstOrDefault();
                model.Photolst = new List<ProfilePhotoDictionary>();
                var c = context.SupportProfilePhoto.Select(x => x).ToList();
                int i = 0;
                foreach (var img in c)
                    model.Photolst.Add(new ProfilePhotoDictionary()
                        {
                            Id = img.Id.ToString(),
                            Img = img.Name,
                            Selected = img.Id == Photo,
                        });
            }
            return View(model);
        }
        public ActionResult SaveOperatorProfilePhoto(string id)
        {
            Guid personId = Guid.Empty;

            RedirectToRouteResult red = Check(ref personId);
            if (red != null)
                return red;

            Guid gPhotoId;
            if (!Guid.TryParse(id, out gPhotoId))
                return Json(false);

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                bool isNew = false;
                var Photo = context.SupportOperatorProfile.Where(x => x.PersonId == personId).Select(x => x).FirstOrDefault();
                if (Photo == null)
                {
                    isNew = true;
                    Photo = new SupportOperatorProfile();
                    Photo.PersonId = personId;
                }
                Photo.ProfilePhotoId = gPhotoId;

                if (isNew)
                    context.SupportOperatorProfile.Add(Photo);
                context.SaveChanges();
                return Json(true);
            }
        }
    }
}
