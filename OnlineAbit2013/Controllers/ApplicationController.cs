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
    public class ApplicationController : Controller
    {
        // GET: /Application/
        public ActionResult Index(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            Guid CommitId = new Guid();
            if (!Guid.TryParse(id, out CommitId))
                return RedirectToAction("Main", "Abiturient");
            bool isEng = Util.GetCurrentThreadLanguageIsEng(); 
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var tblAppsMain =
                    (from App in context.Application
                     join Entry in context.Entry on App.EntryId equals Entry.Id
                     join Semester in context.Semester on Entry.SemesterId equals Semester.Id
                     join Commit in context.ApplicationCommit on App.CommitId equals Commit.Id
                     join apstype in context.ApplicationSecondType on App.SecondTypeId equals apstype.Id into _sectype
                     from sectype in _sectype.DefaultIfEmpty()

                     join AppInProtocol in context.ApplicationAddedToProtocol on App.Barcode equals AppInProtocol.Barcode into AppInProtocol2
                     from AppInProtocol in AppInProtocol2.DefaultIfEmpty()

                     where App.CommitId == CommitId && App.IsCommited == true && App.Enabled == true
                     select new SimpleApplication()
                     {
                         Id = App.Id,
                         Profession = isEng ? (String.IsNullOrEmpty(Entry.LicenseProgramNameEng) ? Entry.LicenseProgramName : Entry.LicenseProgramNameEng) : Entry.LicenseProgramName,
                         ObrazProgram = isEng ? (String.IsNullOrEmpty(Entry.ObrazProgramNameEng) ? Entry.ObrazProgramName : Entry.ObrazProgramNameEng) : Entry.ObrazProgramName,
                         Specialization = isEng ? (String.IsNullOrEmpty(Entry.ProfileNameEng) ? Entry.ProfileName : Entry.ProfileNameEng) : Entry.ProfileName,
                         StudyForm = isEng ? (String.IsNullOrEmpty(Entry.StudyFormNameEng) ? Entry.StudyFormName : Entry.StudyFormNameEng) : Entry.StudyFormName,
                         StudyBasis = isEng ? (String.IsNullOrEmpty(Entry.StudyBasisNameEng) ? Entry.StudyBasisName : Entry.StudyBasisNameEng) : Entry.StudyBasisName,
                         StudyLevel = isEng ? (String.IsNullOrEmpty(Entry.StudyLevelNameEng) ? Entry.StudyLevelName : Entry.StudyLevelNameEng) : Entry.StudyLevelName,
                         Priority = App.Priority,
                         IsApprowed = App.IsApprovedByComission,
                         IsGosLine = App.IsGosLine,
                         dateofClose = Entry.DateOfClose,
                         Enabled = App.Enabled,
                         SemesterName = (Entry.SemesterId != 1) ? Semester.Name : "",
                         StudyLevelGroupId = Entry.StudyLevelGroupId,
                         StudyLevelGroupName = (isEng ? ((String.IsNullOrEmpty(Entry.StudyLevelGroupNameEng)) ? Entry.StudyLevelGroupNameRus : Entry.StudyLevelGroupNameEng) : Entry.StudyLevelGroupNameRus) +
                                  (sectype == null ? "" : (isEng ? sectype.NameEng : sectype.Name)), 
                         AbiturientTypeId = App.SecondTypeId,

                         IsImported = Commit.IsImported,
                         IsAddedToProtocol = AppInProtocol != null
                     }).ToList();
                bool ExistNotSelectedExams = false;
                foreach (SimpleApplication app in tblAppsMain)
                {
                    var lst = Util.GetExamList(app.Id);
                    app.HasExamsForRegistration = lst.Where(x=>x.HasExamTimeTable).Count() > 0;

                    app.ManualExam = new List<string>();

                    if (lst.Count>0)
                    {
                        
                        foreach (var x in lst)
                        {
                            if (x.ExamInBlockList.Count > 1)
                                app.ManualExam.Add(x.ExamInBlockList.Where(e => e.Value.ToString() == x.SelectedExamInBlockId.ToString()).Select(e => e.Text.ToString()).FirstOrDefault());
                        }
                        if (app.ManualExam.Where(x=>String.IsNullOrEmpty(x)).Count()>0)
                        {
                            ExistNotSelectedExams = true;
                        }
                    }
                    app.HasManualExams = app.ManualExam.Count > 0;
                }
                
                string query = "SELECT Id, FileName, FileSize, Comment, IsApproved FROM ApplicationFile WHERE CommitId=@CommitId and IsDeleted = 0";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@CommitId", CommitId } });

                List<AppendedFile> lFiles =
                    (from DataRow rw in tbl.Rows
                     select new AppendedFile()
                     {
                         Id = rw.Field<Guid>("Id"),
                         FileName = rw.Field<string>("FileName"),
                         FileSize = rw.Field<int>("FileSize"),
                         Comment = rw.Field<string>("Comment"),
                         IsShared = false,
                         IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                                 (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet
                     }).ToList();

                query = "SELECT Id, FileName, FileSize, Comment, IsApproved FROM PersonFile WHERE PersonId=@PersonId and IsDeleted = 0";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", personId } });
                var lSharedFiles =
                    (from DataRow rw in tbl.Rows
                     select new AppendedFile()
                     {
                         Id = rw.Field<Guid>("Id"),
                         FileName = rw.Field<string>("FileName"),
                         FileSize = rw.Field<int>("FileSize"),
                         Comment = rw.Field<string>("Comment"),
                         IsShared = true,
                         IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                                 (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet
                     }).ToList();

                var AllFiles = lFiles.Union(lSharedFiles).OrderBy(x => x.IsShared).ToList();

                bool bIsPrinted = context.ApplicationCommit.Where(x => x.Id == CommitId).Select(x => x.IsPrinted).DefaultIfEmpty(false).First();

                ExtApplicationCommitModel model = new ExtApplicationCommitModel()
                {
                    Id = CommitId,
                    Applications = tblAppsMain,
                    Files = AllFiles,
                    IsPrinted = bIsPrinted,
                    Enabled = true,
                    StudyLevelGroupId =(tblAppsMain.Count==0)?1:tblAppsMain.First().StudyLevelGroupId,
                    HasManualExams = tblAppsMain.Where(x=>x.HasManualExams).Count()>0,
                    HasExamsForRegistration = tblAppsMain.Where(x=>x.HasExamsForRegistration).Count()>0,
                    HasNotSelectedExams = ExistNotSelectedExams,
                    AbiturientTypeId = tblAppsMain.Count>0 ? tblAppsMain.Select(x => x.AbiturientTypeId).FirstOrDefault() : 1,
                };

                foreach (SimpleApplication s in tblAppsMain)
                {
                    if (s.dateofClose != null)
                        if (s.dateofClose < DateTime.Now)
                        {
                            model.Enabled = false;
                            break;
                        }
                }
                var AppVers = context.ApplicationCommitVersion.Where(x => x.CommitId == CommitId).Select(x => x.VersionDate).ToList().LastOrDefault();
                if (AppVers == null)
                {
                    model.HasVersion = false;
                }
                else
                {
                    model.HasVersion = true;
                    model.VersionDate = AppVers.ToString(); 
                }
                model.FileType = Util.GetPersonFileTypeList();
                return View(model);
            }
        }

        public ActionResult AppIndex(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            Guid ApplicationId = new Guid();
            if (!Guid.TryParse(id, out ApplicationId))
                return RedirectToAction("Main", "Abiturient");
            bool isEng = Util.GetCurrentThreadLanguageIsEng(); 
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var ApplicationEntity = 
                    (from App in context.Application 
                     join Entry in context.Entry on App.EntryId equals Entry.Id
                     join Commission in context.Comission on Entry.ComissionId equals Commission.Id into Commission2
                     from Commission in Commission2.DefaultIfEmpty()
                     where App.Id == ApplicationId && App.IsCommited == true
                     select new 
                {
                    Id = App.Id,
                    Profession = isEng ? (String.IsNullOrEmpty(Entry.LicenseProgramNameEng) ? Entry.LicenseProgramName : Entry.LicenseProgramNameEng) : Entry.LicenseProgramName,
                    ObrazProgram = isEng ? (String.IsNullOrEmpty(Entry.ObrazProgramNameEng) ? Entry.ObrazProgramName : Entry.ObrazProgramNameEng) : Entry.ObrazProgramName,
                    Specialization = isEng ? (String.IsNullOrEmpty(Entry.ProfileNameEng) ? Entry.ProfileName : Entry.ProfileNameEng) : Entry.ProfileName,
                    StudyForm = isEng ? (String.IsNullOrEmpty(Entry.StudyFormNameEng) ? Entry.StudyFormName : Entry.StudyFormNameEng) : Entry.StudyFormName,
                    StudyBasis = isEng ? (String.IsNullOrEmpty(Entry.StudyBasisNameEng) ? Entry.StudyBasisName : Entry.StudyBasisNameEng) : Entry.StudyBasisName,
                    StudyLevel = isEng ? (String.IsNullOrEmpty(Entry.StudyLevelNameEng) ? Entry.StudyLevelName : Entry.StudyLevelNameEng) : Entry.StudyLevelName,
                    StudyLevelId = Entry.StudyLevelId,
                    Priority = App.Priority,
                    Enabled = App.Enabled,
                    ComissionAddress = Commission.Address,
                    ComissionYaCoord = Commission.YaMapCoord,
                    DateOfDisable = App.DateOfDisable,
                    IsApproved = App.IsApprovedByComission,
                    EntryTypeId = App.EntryType,
                    CommitId = App.CommitId,
                    CommitName = isEng ? (String.IsNullOrEmpty(Entry.StudyLevelNameEng) ? Entry.StudyLevelName : Entry.StudyLevelNameEng) :Entry.StudyLevelName
                }).FirstOrDefault();
                
                string query = "SELECT Id, FileName, FileSize, Comment, IsApproved FROM ApplicationFile WHERE ApplicationId=@ApplicationId and IsDeleted=0";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@ApplicationId", ApplicationId } });

                List<AppendedFile> lFiles =
                    (from DataRow rw in tbl.Rows
                     select new AppendedFile()
                     {
                         Id = rw.Field<Guid>("Id"),
                         FileName = rw.Field<string>("FileName"),
                         FileSize = rw.Field<int>("FileSize"),
                         Comment = rw.Field<string>("Comment"),
                         IsShared = false,
                         IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                                 (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet
                     }).ToList();

                query = "SELECT Id, FileName, FileSize, Comment, IsApproved FROM PersonFile WHERE PersonId=@PersonId and IsDeleted = 0";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", personId } });
                var lSharedFiles =
                    (from DataRow rw in tbl.Rows
                     select new AppendedFile()
                     {
                         Id = rw.Field<Guid>("Id"),
                         FileName = rw.Field<string>("FileName"),
                         FileSize = rw.Field<int>("FileSize"),
                         Comment = rw.Field<string>("Comment"),
                         IsShared = true,
                         IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                                 (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet
                     }).ToList();

                var AllFiles = lFiles.Union(lSharedFiles).OrderBy(x => x.IsShared).ToList();

                AbitType abt = AbitType.AG;
                switch (ApplicationEntity.StudyLevelId)
                {
                    case 1: abt = AbitType.AG; break;
                    case 8: abt = AbitType.SPO; break;
                    case 10: abt = AbitType.SPO; break;
                    case 15: abt = AbitType.Aspirant; break;
                    case 16: abt = AbitType.FirstCourseBakSpec; break;
                    case 17: abt = AbitType.Mag; break;
                    case 18: abt = AbitType.FirstCourseBakSpec; break;
                    case 20: abt = AbitType.Ord; break;
                    default: abt = AbitType.FirstCourseBakSpec; break;
                }
                int? c = (int?) Util.AbitDB.GetValue("SELECT top 1 SecondTypeId FROM Application WHERE Id=@Id AND PersonId=@PersonId", new SortedList<string, object>() { { "@PersonId", personId }, { "@Id", ApplicationId } }) ;
                if (c !=1 ){
                        abt = AbitType.FirstCourseBakSpec; 
                }
                ExtApplicationModel model = new ExtApplicationModel()
                {
                    Id = ApplicationId,
                    CommitId = ApplicationEntity.CommitId,
                    CommitName = ApplicationEntity.CommitName,
                    Files = AllFiles,
                    ComissionAddress = ApplicationEntity.ComissionAddress,
                    ComissionYaCoord = ApplicationEntity.ComissionYaCoord,
                    DateOfDisable = ApplicationEntity.DateOfDisable.HasValue ? ApplicationEntity.DateOfDisable.Value.ToString() : "",
                    Enabled = ApplicationEntity.Enabled,
                    EntryTypeId = ApplicationEntity.EntryTypeId,
                    AbiturientType = abt,
                    IsApproved = ApplicationEntity.IsApproved,
                    ObrazProgram = ApplicationEntity.ObrazProgram,
                    Priority = ApplicationEntity.Priority.ToString(),
                    Profession = ApplicationEntity.Profession,
                    Specialization = ApplicationEntity.Specialization,
                    StudyBasis = ApplicationEntity.StudyBasis,
                    StudyForm = ApplicationEntity.StudyForm,
                    UILanguage = Util.GetCurrentThreadLanguage(),
                    FileType = Util.GetPersonFileTypeList()
                };

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult AddFile()
        {
            string id = Request.Form["id"];
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid ApplicationId = new Guid();
            if (!Guid.TryParse(id, out ApplicationId))
                return RedirectToAction("Main", "Abiturient");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json(Resources.ServerMessages.EmptyFileError);

            string fileName = Request.Files["File"].FileName;
            string fileComment = Request.Form["Comment"];

            int PersonFileTypeId = Convert.ToInt32(Request.Form["FileTypeId"]);

            int fileSize = Convert.ToInt32(Request.Files["File"].InputStream.Length);
            byte[] fileData = new byte[fileSize];
            //читаем данные из ПОСТа
            Request.Files["File"].InputStream.Read(fileData, 0, fileSize);
            string fileext = "";
            try
            {
                fileext = fileName.Substring(fileName.LastIndexOf('.'));
            }
            catch
            {
                fileext = "";
            }
            string FileNameTemplate = Util.AbitDB.GetStringValue("select FileNameTemplate from dbo.PersonFileType where Id=" + PersonFileTypeId);
            string FileTypeName = Util.AbitDB.GetStringValue("select Name from dbo.PersonFileType where Id=" + PersonFileTypeId);

            if (!String.IsNullOrEmpty(FileNameTemplate))
            {
                fileComment = FileTypeName + ": " + fileComment ;
                fileComment += "(исходное название файла- " + fileName + ")";
                if (FileTypeName.StartsWith("Эссе"))
                    FileTypeName = "Эссе";
                int Count = 0;
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var FileNameList = (from pf in context.PersonFile
                                        where pf.PersonId == PersonId && pf.PersonFileTypeId == PersonFileTypeId
                                        select pf.FileName)
                                        .Union(
                                        (from apf in context.ApplicationFile
                                           join app in context.Application on apf.ApplicationId equals app.Id
                                           where app.PersonId == PersonId && apf.Comment.StartsWith(FileTypeName)
                                           select apf.FileName))
                                        .Union(
                                        (from apf in context.ApplicationFile
                                       join app in context.Application on apf.CommitId equals app.CommitId
                                       where app.PersonId == PersonId && apf.Comment.StartsWith(FileTypeName)
                                       select apf.FileName)).ToList();
                    foreach (string name in FileNameList)
                    {
                        int tmp;
                        if (name.Contains('.'))
                        {
                            if (int.TryParse(name.Substring(0, name.IndexOf('.')).Replace(FileNameTemplate, ""), out tmp))
                            {
                                if (Count < tmp)
                                    Count = tmp;
                            }
                        }
                        else if (int.TryParse(name.Replace(FileNameTemplate, ""), out tmp))
                        {
                            if (Count < tmp)
                                Count = tmp;
                        }
                    }
                }
                fileName = FileNameTemplate + (Count + 1).ToString() + fileext;
            }

            try
            {
                string query = "INSERT INTO ApplicationFile (Id, ApplicationId, FileName, FileData, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId]) " +
                    " VALUES (@Id, @ApplicationId, @FileName, @FileData, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 1)";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", Guid.NewGuid());
                dic.Add("@ApplicationId", ApplicationId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@IsReadOnly", false);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", fileComment);
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));

                Util.AbitDB.ExecuteQuery(query, dic);
                Util.AbitDB.ExecuteQuery(@"update dbo.Application set IsViewed=0 where Id=@ApplicationId", dic);
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }

            return RedirectToAction("AppIndex", new RouteValueDictionary() { { "id", id } });
        }

        [HttpPost]
        public ActionResult AddFileInCommit()
        {
            string id = Request.Form["id"];
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid CommitId = new Guid();
            if (!Guid.TryParse(id, out CommitId))
                return RedirectToAction("Main", "Abiturient");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json(Resources.ServerMessages.EmptyFileError);

            string fileName = Request.Files["File"].FileName;
            string fileComment = Request.Form["Comment"];
            int PersonFileTypeId = Convert.ToInt32(Request.Form["FileTypeId"]);
            int fileSize = Convert.ToInt32(Request.Files["File"].InputStream.Length);
            byte[] fileData = new byte[fileSize];
            //читаем данные из ПОСТа
            Request.Files["File"].InputStream.Read(fileData, 0, fileSize);
            string fileext = "";
            try
            {
                fileext = fileName.Substring(fileName.LastIndexOf('.'));
            }
            catch
            {
                fileext = "";
            }
            string FileNameTemplate = Util.AbitDB.GetStringValue("select FileNameTemplate from dbo.PersonFileType where Id=" + PersonFileTypeId);
            string FileTypeName = Util.AbitDB.GetStringValue("select Name from dbo.PersonFileType where Id=" + PersonFileTypeId);

            if (!String.IsNullOrEmpty(FileNameTemplate))
            {
                fileComment = FileTypeName + ": " + fileComment;
                fileComment += "(исходное название файла- " + fileName + ")";

                if (FileTypeName.StartsWith("Эссе"))
                    FileTypeName = "Эссе";

                int Count = 0;
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var FileNameList = (from pf in context.PersonFile
                                        where pf.PersonId == PersonId && pf.PersonFileTypeId == PersonFileTypeId
                                        select pf.FileName)
                                        .Union(
                                        (from apf in context.ApplicationFile
                                           join app in context.Application on apf.ApplicationId equals app.Id
                                           where app.PersonId == PersonId && apf.Comment.StartsWith(FileTypeName)
                                           select apf.FileName))
                                        .Union(
                                        (from apf in context.ApplicationFile
                                       join app in context.Application on apf.CommitId equals app.CommitId
                                       where app.PersonId == PersonId && apf.Comment.StartsWith(FileTypeName)
                                       select apf.FileName)).ToList();

                    foreach (string name in FileNameList)
                    {
                        int tmp;
                        if (name.Contains('.'))
                        {
                            if (int.TryParse(name.Substring(0, name.IndexOf('.')).Replace(FileNameTemplate, ""), out tmp))
                            {
                                if (Count < tmp)
                                    Count = tmp;
                            }
                        }
                        else if (int.TryParse(name.Replace(FileNameTemplate, ""), out tmp))
                        {
                            if (Count < tmp)
                                Count = tmp;
                        }
                    }
                }
                fileName = FileNameTemplate + (Count + 1).ToString() + fileext;
            }

            try
            {
                string query = "INSERT INTO ApplicationFile (Id, CommitId, FileName, FileData, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId]) " +
                    " VALUES (@Id, @CommitId, @FileName, @FileData, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 1)";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", Guid.NewGuid());
                dic.Add("@CommitId", CommitId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@IsReadOnly", false);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", fileComment);
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));

                Util.AbitDB.ExecuteQuery(query, dic);
                Util.AbitDB.ExecuteQuery(@"update dbo.Application set IsViewed=0 where CommitId=@CommitId", dic);
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }

            return RedirectToAction("Index", new RouteValueDictionary() { { "id", id } });
        }

        public ActionResult GetFile(string id)
        {
            Guid FileId = new Guid();
            if (!Guid.TryParse(id, out FileId))
                return Content("Некорректный идентификатор файла");

            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Content("Authorization required");

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT FileName, FileData, MimeType, FileExtention FROM AllFiles WHERE Id=@Id",
                new SortedList<string, object>() { { "@Id", FileId } });

            if (tbl.Rows.Count == 0)
                return Content("Файл не найден");

            string fileName = tbl.Rows[0].Field<string>("FileName");
            string contentType = tbl.Rows[0].Field<string>("MimeType");
            byte[] content = tbl.Rows[0].Field<byte[]>("FileData");
            string ext = tbl.Rows[0].Field<string>("FileExtention");


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

        public ActionResult GetPrint(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Authentification Error"), "text/plain");

            Guid appId;
            if (!Guid.TryParse(id, out appId))
                return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка идентификатора заявления"), "text/plain");

            byte[] bindata; 
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                try
                {
                    var lst = context.Application.Where(x => x.CommitId == appId && x.PersonId == personId).Select(x => x.C_Entry.SP_StudyLevel.StudyLevelGroupId).ToList();
                    if (lst.Count == 0)
                        return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Access error"), "text/plain");
                    int? CountryEducId = context.PersonEducationDocument.Where(x => x.PersonId == personId).Select(x => x.CountryEducId).FirstOrDefault();
                    int? Secondlst = context.Application.Where(x => x.CommitId == appId && x.PersonId == personId).Select(x => x.SecondTypeId).FirstOrDefault();
                    /*if (Secondlst.Count == 0)
                        return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Access error"), "text/plain");*/

                    //дальше должно быть разделение - для 1 курса, магистров, аспирантов, переводящихся и восстанавливающихся
                    //пока что затычка из АГ
                    int EntryType = lst.First();

                    switch (EntryType)
                    {
                        //бакалавриат
                        case 1: { bindata = PDFUtils.GetApplicationPDF(appId, Server.MapPath("~/Templates/"), false, personId); break; }
                        //магистратура
                        case 2: { bindata = PDFUtils.GetApplicationPDF(appId, Server.MapPath("~/Templates/"), true, personId); break; }
                        //СПО
                        case 3: { bindata = PDFUtils.GetApplicationPDF_SPO(appId, Server.MapPath("~/Templates/"), personId); break; }
                        //Аспирантура
                        case 4: { bindata = PDFUtils.GetApplicationPDF_Aspirant(appId, Server.MapPath("~/Templates/"), personId); break; }
                        //Аспирантура
                        case 5: { bindata = PDFUtils.GetApplicationPDF_Ord(appId, Server.MapPath("~/Templates/"), personId); break; }

                        case 6:
                        case 7: { bindata = PDFUtils.GetApplicationPDF_AG(appId, Server.MapPath("~/Templates/")); break; }
                        default: { bindata = PDFUtils.GetApplicationPDF(appId, Server.MapPath("~/Templates/"), false, personId); break; }
                    }
                    if (Secondlst.HasValue)
                    {
                        // восстановление
                        if (Secondlst == 3) { bindata = PDFUtils.GetApplicationPDFRecover(appId, Server.MapPath("~/Templates/"), false, personId); }
                        // перевод
                        else if (Secondlst == 2)
                        {
                            if (CountryEducId.HasValue)
                            {
                                if (CountryEducId == 193)
                                    bindata = PDFUtils.GetApplicationPDFTransfer(appId, Server.MapPath("~/Templates/"), false, personId);
                                else
                                    bindata = PDFUtils.GetApplicationPDFTransferForeign(appId, Server.MapPath("~/Templates/"), false, personId);
                            }
                            else
                                bindata = PDFUtils.GetApplicationPDFTransfer(appId, Server.MapPath("~/Templates/"), false, personId);
                        }
                        // смена формы
                        else if (Secondlst == 4) { bindata = PDFUtils.GetApplicationPDFChangeStudyBasis(appId, Server.MapPath("~/Templates/"), false, personId); }
                        // смена основы
                        else if (Secondlst == 5) { bindata = PDFUtils.GetApplicationPDFChangeStudyBasis(appId, Server.MapPath("~/Templates/"), false, personId); }
                        // смена образовательной программы
                        else if (Secondlst == 6) { bindata = PDFUtils.GetApplicationPDFChangeObrazProgram(appId, Server.MapPath("~/Templates/"), false, personId); }
                    }

                    //ApplicationCommit.IsPrinted
                    var Commt = context.ApplicationCommit.Where(x => x.Id == appId).FirstOrDefault();
                    if (Commt != null)
                        Commt.IsPrinted = true;

                    context.SaveChanges();
                }
                catch
                {
                    return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка при печати заявления"), "text/plain");
                }
            }
            return new FileContentResult(bindata, "application/pdf") { FileDownloadName = "Application.pdf" };
        }

        public ActionResult Disable(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired };
                return Json(res);
            }

            if (PersonId == Guid.Empty)
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired };
                return Json(res);
            }

            Guid AppId;
            if (!Guid.TryParse(id, out AppId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID };
                return Json(res);
            }

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                bool isAg = false;
                bool? isEnabled = context.Application.Where(x => x.Id == AppId && x.PersonId == PersonId).Select(x => (bool?)x.Enabled).FirstOrDefault();
                //(bool?)Util.AbitDB.GetValue("SELECT Enabled FROM [Application] WHERE Id=@Id AND PersonId=@PersonId",
                //new SortedList<string, object>() { { "@Id", AppId }, { "@PersonId", PersonId } });

                //var app = Util.ABDB.Application.Where(x => x.Id == AppId && x.PersonId == PersonId).FirstOrDefault();
                //if (!isEnabled.HasValue)
                //{
                //    isEnabled = context.AG_Application.Where(x => x.Id == AppId && x.PersonId == PersonId).Select(x => (bool?)x.Enabled).FirstOrDefault();
                //    if (!isEnabled.HasValue)
                //    {
                //        var res = new { IsOk = false, ErrorMessage = "Ошибка при поиске заявления. Попробуйте обновить страницу" };
                //        return Json(res);
                //    }
                //    else
                //        isAg = true;
                //}

                if (isEnabled.HasValue && isEnabled.Value == false)
                {
                    var res = new { IsOk = false, ErrorMessage = "Заявление уже было отозвано" };
                    return Json(res);
                }

                try
                {
                    string query = string.Format("UPDATE [{0}Application] SET Enabled=@Enabled, DateOfDisable=@DateOfDisable, Priority=@Priority WHERE Id=@Id", isAg ? "AG_" : "");
                    SortedList<string, object> dic = new SortedList<string, object>();
                    dic.Add("@Id", AppId);
                    dic.Add("@DateOfDisable", DateTime.Now);
                    dic.Add("@Priority", 0);
                    dic.Add("@Enabled", false);

                    Util.AbitDB.ExecuteQuery(query, dic);

                    var res = new { IsOk = true, Enabled = false };
                    return Json(res);
                }
                catch
                {
                    var res = new { IsOk = false, ErrorMessage = "Ошибка при поиске заявления. Попробуйте обновить страницу" };
                    return Json(res);
                }
            }
        }

        public ActionResult DisableFull(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired };
                return Json(res);
            }

            if (PersonId == Guid.Empty)
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired };
                return Json(res);
            } 
            Guid CommitId;
            if (!Guid.TryParse(id, out CommitId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID };
                return Json(res);
            }

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                //bool isAg = false;
                bool? isEnabled = context.Application.Where(x => x.CommitId == CommitId && x.PersonId == PersonId).Select(x => (bool?)x.Enabled).FirstOrDefault();

                //if (!isEnabled.HasValue)
                //{
                //    isEnabled = context.AG_Application.Where(x => x.CommitId == CommitId && x.PersonId == PersonId).Select(x => (bool?)x.Enabled).FirstOrDefault();
                //    if (!isEnabled.HasValue)
                //    {
                //        var res = new { IsOk = false, ErrorMessage = "Ошибка при поиске заявления. Попробуйте обновить страницу" };
                //        return Json(res);
                //    }
                //    else
                //        isAg = true;
                //}

                if (isEnabled.HasValue && isEnabled.Value == false)
                {
                    var res = new { IsOk = false, ErrorMessage = "Заявление уже было отозвано" };
                    return Json(res);
                }

                try
                {
                    bool? result = null;
                    result = PDFUtils.GetDisableApplicationPDF(CommitId, Server.MapPath("~/Templates/"), PersonId);
                    
                    string query = string.Format("DELETE FROM [Application] WHERE CommitId=@Id");
                    SortedList<string, object> dic = new SortedList<string, object>();
                    dic.Add("@Id", CommitId);

                    Util.AbitDB.ExecuteQuery(query, dic);

                    var res = new { IsOk = true, Enabled = false};
                    return Json(res);
                }
                catch
                {
                    var res = new { IsOk = false, ErrorMessage = "Ошибка при поиске заявления. Попробуйте обновить страницу" };
                    return Json(res);
                }
            }
        }

        public ActionResult MotivatePost()
        {
            string id = Request.Form["id"];
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid ApplicationId = new Guid();
            if (!Guid.TryParse(id, out ApplicationId))
                return RedirectToAction("Main", "Abiturient");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json(Resources.ServerMessages.EmptyFileError);

            string fileName = Request.Files["File"].FileName;
            int fileSize = Convert.ToInt32(Request.Files["File"].InputStream.Length);
            byte[] fileData = new byte[fileSize];
            //читаем данные из ПОСТа
            Request.Files["File"].InputStream.Read(fileData, 0, fileSize);
            string fileext = "";
            try
            {
                fileext = fileName.Substring(fileName.LastIndexOf('.'));
            }
            catch
            {
                fileext = "";
            }

            string FileNameTemplate = Util.AbitDB.GetStringValue("select FileNameTemplate from dbo.PersonFileType where Id=10");
            string FileTypeName = Util.AbitDB.GetStringValue("select Name from dbo.PersonFileType where Id=10");

            if (!String.IsNullOrEmpty(FileNameTemplate))
            {
                int Count = 0;
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var FileNameList = (from pf in context.PersonFile
                                        where pf.PersonId == PersonId && pf.PersonFileTypeId == 10
                                        select pf.FileName)
                                        .Union(
                                        (from apf in context.ApplicationFile
                                           join app in context.Application on apf.ApplicationId equals app.Id
                                         where app.PersonId == PersonId && (apf.Comment.StartsWith(FileTypeName) || (apf.Comment.StartsWith("Мотивационное письмо")))
                                           select apf.FileName))
                                        .Union(
                                        (from apf in context.ApplicationFile
                                       join app in context.Application on apf.CommitId equals app.CommitId
                                         where app.PersonId == PersonId && (apf.Comment.StartsWith(FileTypeName) || (apf.Comment.StartsWith("Мотивационное письмо")))
                                       select apf.FileName)).ToList();

                    foreach (string name in FileNameList)
                    {
                        int tmp;
                        if (name.Contains('.'))
                        {
                            if (int.TryParse(name.Substring(0, name.IndexOf('.')).Replace(FileNameTemplate, ""), out tmp))
                            {
                                if (Count < tmp)
                                    Count = tmp;
                            }
                        }
                        else if (int.TryParse(name.Replace(FileNameTemplate, ""), out tmp))
                        {
                            if (Count < tmp)
                                Count = tmp;
                        }
                    }
                }
                fileName = FileNameTemplate + (Count + 1).ToString() + fileext;
            }


            try
            {
                string query = "INSERT INTO ApplicationFile (Id, ApplicationId, FileName, FileData, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId]) " +
                    " VALUES (@Id, @ApplicationId, @FileName, @FileData, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 2)";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", Guid.NewGuid());
                dic.Add("@ApplicationId", ApplicationId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@IsReadOnly", false);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", "Мотивационное письмо");
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));

                Util.AbitDB.ExecuteQuery(query, dic);
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }

            return RedirectToAction("AppIndex", new RouteValueDictionary() { { "id", id } });
        }

        public ActionResult EssayPost()
        {
            string id = Request.Form["id"];
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid ApplicationId = new Guid();
            if (!Guid.TryParse(id, out ApplicationId))
                return RedirectToAction("Main", "Abiturient");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json(Resources.ServerMessages.EmptyFileError);

            string fileName = Request.Files["File"].FileName;
            if (fileName.IndexOf('\\') > 0 && fileName.LastIndexOf('\\') < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

            int fileSize = Convert.ToInt32(Request.Files["File"].InputStream.Length);
            byte[] fileData = new byte[fileSize];
            //читаем данные из ПОСТа
            Request.Files["File"].InputStream.Read(fileData, 0, fileSize);
            string fileext = "";
            try
            {
                fileext = fileName.Substring(fileName.LastIndexOf('.'));
            }
            catch
            {
                fileext = "";
            }

            string FileNameTemplate = Util.AbitDB.GetStringValue("select FileNameTemplate from dbo.PersonFileType where Id=6");
            string FileTypeName = Util.AbitDB.GetStringValue("select Name from dbo.PersonFileType where Id=6");

            if (!String.IsNullOrEmpty(FileNameTemplate))
            {
                if (FileTypeName.StartsWith("Эссе"))
                    FileTypeName = "Эссе";
                int Count = 0;
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var FileNameList = (from pf in context.PersonFile
                                        where pf.PersonId == PersonId && pf.PersonFileTypeId == 6
                                        select pf.FileName)
                                        .Union(
                                        (from apf in context.ApplicationFile
                                           join app in context.Application on apf.ApplicationId equals app.Id
                                           where app.PersonId == PersonId && ( apf.Comment.StartsWith(FileTypeName) || (apf.Comment.StartsWith("Эссе")))
                                           select apf.FileName))
                                        .Union(
                                        (from apf in context.ApplicationFile
                                       join app in context.Application on apf.CommitId equals app.CommitId
                                       where app.PersonId == PersonId && ( apf.Comment.StartsWith(FileTypeName) || (apf.Comment.StartsWith("Эссе")))
                                       select apf.FileName)).ToList();

                    foreach (string name in FileNameList)
                    {
                        int tmp;
                        if (name.Contains('.'))
                        {
                            if (int.TryParse(name.Substring(0, name.IndexOf('.')).Replace(FileNameTemplate, ""), out tmp))
                            {
                                if (Count < tmp)
                                    Count = tmp;
                            }
                        }
                        else if (int.TryParse(name.Replace(FileNameTemplate, ""), out tmp))
                        {
                            if (Count < tmp)
                                Count = tmp;
                        }
                    }
                }
                fileName = FileNameTemplate + (Count + 1).ToString() + fileext;
            }

            try
            {
                string query = "INSERT INTO ApplicationFile (Id, ApplicationId, FileName, FileData, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId]) " +
                    " VALUES (@Id, @ApplicationId, @FileName, @FileData, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 3)";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", Guid.NewGuid());
                dic.Add("@ApplicationId", ApplicationId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@IsReadOnly", false);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", "Эссе");
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));

                Util.AbitDB.ExecuteQuery(query, dic);
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }

            return RedirectToAction("AppIndex", new RouteValueDictionary() { { "id", id } });
        }

        public JsonResult ChangePriority(string id, string pr)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid ApplicationId;
            if (!Guid.TryParse(id, out ApplicationId))
                return Json(new { IsOk = false, ErrorMessage = "" });

            try
            {
                Util.AbitDB.ExecuteQuery("UPDATE Application SET Priority=@Priority WHERE Id=@Id",
                    new SortedList<string, object>() { { "@Id", ApplicationId }, { "@Priority", pr } });
                return Json(new { IsOk = true });
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = "Ошибка при обновлении данных" });
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired };
                return Json(res);
            }

            string uiLang = Util.GetUILang(PersonId);

            Guid fileId;
            if (!Guid.TryParse(id, out fileId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID };
                return Json(res);
            }
            string attr = Util.AbitDB.GetStringValue("SELECT IsReadOnly FROM ApplicationFile WHERE Id=@Id", new SortedList<string, object>() { { "@Id", fileId } });
            if (string.IsNullOrEmpty(attr))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.FileNotFound };
                return Json(res);
            }
            if (attr == "True")
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.ReadOnlyFile };
                return Json(res);
            }
            try
            {
                Util.AbitDB.ExecuteQuery("DELETE FROM ApplicationFile WHERE Id=@Id", new SortedList<string, object>() { { "@Id", fileId } });
            }
            catch
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.ErrorWhileDeleting };
                return Json(res);
            }

            var result = new { IsOk = true, ErrorMessage = "" };
            return Json(result);
        }
        [HttpPost]
        public ActionResult GetFileList(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid CommitId = new Guid();
            if (!Guid.TryParse(id, out CommitId))
                return RedirectToAction("Main", "Abiturient");


            string query = "SELECT Id, FileName, FileSize, Comment, IsApproved, IsReadOnly FROM ApplicationFile WHERE CommitId=@CommitId AND IsDeleted=0 ";
            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@CommitId", CommitId } });

            List<AppendedFile> lFiles =
                (from DataRow rw in tbl.Rows
                 select new AppendedFile()
                 {
                     Id = rw.Field<Guid>("Id"),
                     FileName = rw.Field<string>("FileName"),
                     FileSize = rw.Field<int>("FileSize"),
                     Comment = rw.Field<string>("Comment"),
                     IsShared = false,
                     IsReadOnly = rw.Field<bool>("IsReadOnly"),
                     IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                             (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet
                 }).ToList();

            query = "SELECT Id, FileName, FileSize, Comment, IsApproved, IsReadOnly FROM PersonFile WHERE PersonId=@PersonId AND IsDeleted=0 ";
            tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
            var lSharedFiles =
                (from DataRow rw in tbl.Rows
                 select new AppendedFile()
                 {
                     Id = rw.Field<Guid>("Id"),
                     FileName = rw.Field<string>("FileName"),
                     FileSize = rw.Field<int>("FileSize"),
                     Comment = rw.Field<string>("Comment"),
                     IsShared = true,
                     IsReadOnly = rw.Field<bool>("IsReadOnly"),
                     IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                             (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet
                 }).ToList();

            var AllFiles = lFiles.Union(lSharedFiles).OrderBy(x => x.IsShared).ToList();
            
            return Json(new { IsOk = AllFiles.Count() > 0 ? true : false, Data = AllFiles });
        }

        public ActionResult SaveAllFiles(string HiddenId)
        {
            string Id = HiddenId;
            if (!String.IsNullOrEmpty(Id))
            {
                if (Id.Equals("1adb61c519c3e2052e61ef8cf563c022"))
                {
                    HttpCookie myCookie = new HttpCookie("sid");
                    myCookie.Name = "sid";
                    myCookie.Value = Id;
                    Response.Cookies.Add(myCookie);

                    Guid personId = Util.GetIdBySID(Id);
                    string t = Util.AbitDB.GetStringValue("SELECT Ticket FROM AuthTicket WHERE UserId=@UserId", new SortedList<string, object>() { { "@UserId", personId } });
                    
                    myCookie = new HttpCookie("t");
                    myCookie.Name = "t";
                    myCookie.Value = t;
                    Response.Cookies.Add(myCookie);

                    bool isEng = Util.GetCurrentThreadLanguageIsEng();

                    string MotiveMail = (isEng) ? "Motivation letter" : "Мотивационное письмо";
                    string Essay = (isEng) ? "Essay" : "Эссе";

                    string ApplicationFile = (isEng) ? "application file" : "к заявлению";
                    string SharedFile = (isEng) ? "shared file" : "в общие файлы";

                    string BasisName = (isEng) ? "Entry.StudyBasisNameEng":"Entry.StudyBasisName";

                    Guid UserId;
                    if (Util.CheckAuthCookies(Request.Cookies, out UserId))
                    {

                        string query = @" 
                        select 
                        distinct 

                        qAbitFiles_OnlyEssayMotivLetter.Id as AbitFileId 
                        ,Person.Surname +' '+ Person.Name + (case when (Secondname is not null)then ' '+SecondName else '' end) as FIO
                        ,qAbitFiles_OnlyEssayMotivLetter.FileName as FileName
                        ,qAbitFiles_OnlyEssayMotivLetter.[Comment] as Comment
                        --,(case when (qAbitFiles_OnlyEssayMotivLetter.FileTypeId = 2) then '" + MotiveMail + @"' else '"+Essay+ @"' end) as FileTypeId
                        ,FileTypeName" + (isEng ? "Eng" : "") + @" as FileTypeId
                        ,qAbitFiles_OnlyEssayMotivLetter.[IsApproved]
                        ,(case when (qAbitFiles_OnlyEssayMotivLetter.ApplicationId is not null or qAbitFiles_OnlyEssayMotivLetter.CommitId is not null ) then ('" + ApplicationFile+@" (' + "+BasisName+@" +')') else '"+SharedFile+ @"' end ) as AddInfo 
                       -- ,Entry.StudyBasisName as BasisName
                        ,PortfolioFilesMark.Mark
                    
                        from qAbitFiles_AllExceptPassport AS qAbitFiles_OnlyEssayMotivLetter
                        
                        inner join Person on Person.Id = qAbitFiles_OnlyEssayMotivLetter.PersonId
                        inner join Application on Application.PersonId = qAbitFiles_OnlyEssayMotivLetter.PersonId
                        inner join Entry on Application.EntryId = Entry.Id
                        left join PortfolioFilesMark on PortfolioFilesMark.FileId = qAbitFiles_OnlyEssayMotivLetter.Id                     

                        where
                        LicenseProgramName = 'Журналистика' and 
                        ObrazProgramName = 'Глобальная коммуникация и международная журналистика'
                        and Person.Barcode in (SELECT Barcode FROM _tmpJournalism)
                        and IsCommited = 1 
                        and  ((qAbitFiles_OnlyEssayMotivLetter.ApplicationId is null and qAbitFiles_OnlyEssayMotivLetter.CommitId is null) or qAbitFiles_OnlyEssayMotivLetter.ApplicationId = Application.Id or qAbitFiles_OnlyEssayMotivLetter.CommitId = Application.CommitId )
                        order by FIO, FileTypeId, FileName";

                        DataTable tbl = Util.AbitDB.GetDataTable(query, null);

                        List<FileInfo> AllFiles = new List<FileInfo>();

                        if (tbl.Rows.Count > 0)
                        {
                            foreach (DataRow rw in tbl.Rows)
                            {
                                FileInfo apFile = new FileInfo()
                                {
                                    Id = rw.Field<Guid>("AbitFileId"),
                                    Author = rw.Field<string>("FIO"),
                                    FileName = rw.Field<string>("FileName"),
                                    AddInfo = rw.Field<string>("AddInfo"),
                                    IsApproved = rw.Field<bool?>("IsApproved").HasValue ?
                                         (rw.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected) : ApprovalStatus.NotSet,

                                    Comment = rw.Field<string>("Comment"),
                                    FileType = rw.Field<string>("FileTypeId"),
                                    Mark = rw.Field<int?>("Mark").ToString(),
                                };
                                AllFiles.Add(apFile);
                            }
                        }

                        FileListChecker model = new FileListChecker()
                        {
                            Files = AllFiles
                        };
                        return View(model);
                    }
                }
            }
            return RedirectToAction("LogOn", "Account");
        }

        [HttpPost]
        public JsonResult AddMark(string FileId, string Mark, string Iamsure)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });
            }

            Guid gFileId;
            if (!Guid.TryParse(FileId, out gFileId))
            {
                return Json(new { IsOk = false, ErrorMessage = "incorrect File Id" });
            };

            int imark = 0;
            if (!int.TryParse(Mark, out imark))
            {
                return Json(new { IsOk = false, ErrorMessage = "Can not parse the mark" });
            }

            try
            {
                int count = (int)Util.AbitDB.GetValue("select count(FileId) from PortfolioFilesMark where FileId = @Id", new SortedList<string, object>() { { "Id", gFileId }, { "Mark", imark } });
                if (count == 0)
                {
                    Util.AbitDB.ExecuteQuery(@"INSERT INTO PortfolioFilesMark (FileId, Mark) VALUES (@Id, @Mark)", new SortedList<string, object>() { { "Id", gFileId }, { "Mark", imark } });
                }
                else
                {
                    if (Iamsure == "1")
                    {
                        Util.AbitDB.ExecuteQuery(@"Update PortfolioFilesMark set Mark=@Mark where FileId=@Id", new SortedList<string, object>() { { "Id", gFileId }, { "Mark", imark } });
                    }
                    else
                    {
                        return Json(new { IsOk = true, HasMark = true });
                    }
                }
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = "Error in process updating/inserting the mark" });
            }

            return Json(new { IsOk = true });
        }

        public ActionResult ExamsTimetable(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            Guid ApplicationId = new Guid();
            if (!Guid.TryParse(id, out ApplicationId))
                return RedirectToAction("Main", "Abiturient");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var apps = (from app in context.Application
                            join bl in context.ExamInEntryBlock on app.EntryId equals bl.EntryId
                            join un in context.ExamInEntryBlockUnit on bl.Id equals un.ExamInEntryBlockId

                            join ex in context.Exam on un.ExamId equals ex.Id
                            join exn in context.ExamName on ex.ExamNameId equals exn.Id

                            join se in context.ApplicationSelectedExam on new { AppId = app.Id, unitId = un.Id } equals new { AppId = se.ApplicationId, unitId = se.ExamInEntryBlockUnitId } into _se
                            from sexam in _se.DefaultIfEmpty()

                            where app.CommitId == ApplicationId
                            select new
                            {
                               AppId =  app.Id,
                               BlockId = bl.Id,
                               UnitId = un.Id,
                               SelExam = (sexam == null) ? false : un.Id == sexam.ExamInEntryBlockUnitId,
                               SelTT = (sexam == null) ? -1 : sexam.ExamTimetableId,

                               ExamName = exn.Name,
                            }).ToList();

                var Temp_lst = (from a in apps
                           group a by a.BlockId into Apps
                           where Apps.Count() == 1 || Apps.Where(x => x.SelExam).Count() > 0
                           select new
                           {
                               AppId = Apps.Select(x=>x.AppId).FirstOrDefault(),
                               UnitId = Apps.Count() ==1 ? Apps.Select(x=>x.UnitId).FirstOrDefault() : Apps.Where(x=>x.SelExam).Select(x=>x.UnitId).FirstOrDefault(),
                               ExamName = Apps.Count() == 1 ? Apps.Select(x => x.ExamName).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.ExamName).FirstOrDefault(),
                               TimeTableId = Apps.Count() == 1 ? Apps.Select(x => x.SelTT).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.SelTT).FirstOrDefault(),
                               }).ToList();
                var lst = (from a in Temp_lst
                           select new
                           {
                               AppId = a.AppId,
                               UnitId = a.UnitId,
                               ExamName = a.ExamName,
                               TimeTableId = a.TimeTableId,
                               lstTimeTable = context.ExamTimetable.Where(t => t.ExamInEntryBlockUnitId == a.UnitId && t.DateOfClose >= DateTime.Now).Select(t => new { Id = t.Id, DateOfClose = t.DateOfClose, Address = t.Address, ExamDate = t.ExamDate, Base = t.BaseExamTimeTableId}).ToList(),
                           }).ToList();

                ApplicationExamsTimeTableModel model = new ApplicationExamsTimeTableModel();
                model.gCommId = ApplicationId;
                model.Comment = "";
                model.lst = lst.Where(x=>x.lstTimeTable.Count>0).Select(x => new AppExamsTimeTable
                           {
                               ApplicationId = x.AppId,
                               ExamInEntryBockUnitId = x.UnitId,
                               ExamInEntryBlockUnitName = x.ExamName,
                               SelectedTimeTableId = x.TimeTableId,
                               lstTimeTable = x.lstTimeTable.Select(t => new ExamTtable { Id = t.Id, DateOfClose = t.DateOfClose, Address = t.Address, ExamDate = t.ExamDate, isEnable = true, BaseExamTimeTableId = t.Base }).OrderBy(t=>t.ExamDate).ToList(),
                           }).ToList();
                foreach (var tt in model.lst)
                {
                    foreach (ExamTtable t in tt.lstTimeTable)
                    {
                        if (t.BaseExamTimeTableId == null)
                        {
                            t.isEnable = true;
                            foreach (var x in model.lst.Select(e => e.SelectedTimeTableId).ToList())
                            {
                                int cnt = (from e in context.ExamTimeTableOneDayRestriction
                                           where (e.ExamTimeTableId1 == x && e.ExamTimeTableId2 == t.Id)
                                           || (e.ExamTimeTableId2 == x && e.ExamTimeTableId1 == t.Id)
                                           select e).Count();
                                if (cnt > 0)
                                {
                                    t.isEnable = false;
                                    break;
                                }
                            }
                            if (!t.isEnable)
                            {
                                var l = model.lst.Where(x => x.SelectedTimeTableId == t.Id).FirstOrDefault();
                                if (l != null)
                                {
                                    l.SelectedTimeTableId = -1;
                                    model.Comment = "Некоторые из выбранных регистраций являлись некорректными и были сброшены";
                                }
                            }
                        }
                        else
                        {
                            t.isEnable = false;
                            foreach (var x in model.lst.Select(e => e.SelectedTimeTableId).ToList())
                            {
                                int cnt = (from e in context.ExamTimeTableOneDayRestriction
                                           where (e.ExamTimeTableId1 == x && e.ExamTimeTableId2 == t.BaseExamTimeTableId)
                                           || (e.ExamTimeTableId2 == x && e.ExamTimeTableId1 == t.BaseExamTimeTableId)
                                           select e).Count();
                                if (cnt > 0)
                                {
                                    t.isEnable = true;
                                    break;
                                }
                            }
                            if (!t.isEnable)
                            {
                                var l = model.lst.Where(x => x.SelectedTimeTableId == t.Id).FirstOrDefault();
                                if (l != null)
                                {
                                    l.SelectedTimeTableId = -1;
                                    model.Comment = "Некоторые из выбранных регистраций являлись некорректными и были сброшены";

                                }
                            }
                        }
                    }

                }
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ExamsTimetableSave(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommitId;
            if (!Guid.TryParse(id.ToString(), out gCommitId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var apps = (from app in context.Application
                            join bl in context.ExamInEntryBlock on app.EntryId equals bl.EntryId
                            join un in context.ExamInEntryBlockUnit on bl.Id equals un.ExamInEntryBlockId

                            join se in context.ApplicationSelectedExam on new { AppId = app.Id, unitId = un.Id } equals new { AppId = se.ApplicationId, unitId = se.ExamInEntryBlockUnitId } into _se
                            from sexam in _se.DefaultIfEmpty()

                            where app.CommitId == gCommitId
                            select new
                            {
                                AppId = app.Id,
                                BlockId = bl.Id,
                                UnitId = un.Id,
                                SelExam = (sexam == null) ? false : un.Id == sexam.ExamInEntryBlockUnitId,
                                SelTT = (sexam == null) ? -1 : sexam.ExamTimetableId,
                            }).ToList();

                var Temp_lst = (from a in apps
                                group a by a.BlockId into Apps
                                where Apps.Count() == 1 || Apps.Where(x => x.SelExam).Count() > 0
                                select new
                                {
                                    AppId = Apps.Select(x => x.AppId).FirstOrDefault(),
                                    UnitId = Apps.Count() == 1 ? Apps.Select(x => x.UnitId).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.UnitId).FirstOrDefault(),
                                    TimeTableId = Apps.Count() == 1 ? Apps.Select(x => x.SelTT).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.SelTT).FirstOrDefault(),
                                }).ToList();

                foreach (var ap in Temp_lst)
                {
                    string val = (Request.Form["app_" + ap.UnitId.ToString()]);
                    int TTid;
                    if (int.TryParse(val, out TTid))
                    {
                        var SelExam = context.ApplicationSelectedExam.Where(x => x.ApplicationId == ap.AppId && x.ExamInEntryBlockUnitId == ap.UnitId).FirstOrDefault();
                        bool bExamExists = true;
                        if (SelExam == null)
                        {
                            bExamExists = false;
                            SelExam = new ApplicationSelectedExam();
                            SelExam.ApplicationId = ap.AppId;
                            SelExam.ExamInEntryBlockUnitId = ap.UnitId;
                        }

                        SelExam.ExamTimetableId = TTid;
                        SelExam.RegistrationDate = DateTime.Now;

                        if (!bExamExists)
                            context.ApplicationSelectedExam.Add(SelExam);

                        context.SaveChanges();
                    }
                }
            }
            return RedirectToAction("ExamsTimetable", new RouteValueDictionary() { { "id", gCommitId.ToString("N") } });
            //return RedirectToAction("Index", new RouteValueDictionary() { { "id", gCommitId.ToString("N") } });
        
        }
        public ActionResult ExamsRegistrationClear(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommitId;
            if (!Guid.TryParse(id.ToString(), out gCommitId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var apps = (from app in context.Application
                            join se in context.ApplicationSelectedExam on  app.Id equals se.ApplicationId 
                            where app.CommitId == gCommitId
                            select se).ToList();
                foreach (var exam in apps)
                {
                    exam.ExamTimetableId = null;
                    exam.RegistrationDate = null;
                    context.SaveChanges();
                }
            }
            return RedirectToAction("ExamsTimetable", new RouteValueDictionary() { { "id", gCommitId.ToString("N") } });
        }
    }
}

