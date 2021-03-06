﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineAbit2013.Models;
using System.Data;
using System.Web.Routing;
using OnlineAbit2013.EMDX;
using SharpRaven;
using SharpRaven.Data;

namespace OnlineAbit2013.Controllers
{
    public class ApplicationController : Controller
    {
        protected override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            var ravenClient = new RavenClient(Util.SentryDSNHost);
            var sEvent = new SentryEvent(filterContext.Exception);
            if (filterContext.Exception is System.Data.Entity.Validation.DbEntityValidationException)
                sEvent.Extra = ((System.Data.Entity.Validation.DbEntityValidationException)filterContext.Exception).EntityValidationErrors;

            ravenClient.Capture(sEvent);
            
            base.OnException(filterContext);
        }
        // GET: /Application/
        public ActionResult Index(string id)
        {
            try
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
                             IsGosLine = Entry.IsForeign,
                             IsCrimea = Entry.IsCrimea,
                             dateofClose = Entry.DateOfClose,
                             Enabled = App.Enabled,
                             SemesterName = (Entry.SemesterId != 1) ? Semester.Name : "",
                             StudyLevelGroupId = Entry.StudyLevelGroupId,
                             StudyLevelGroupName = (isEng ? ((String.IsNullOrEmpty(Entry.StudyLevelGroupNameEng)) ? Entry.StudyLevelGroupNameRus : Entry.StudyLevelGroupNameEng) : Entry.StudyLevelGroupNameRus) +
                                      (sectype == null ? "" : (isEng ? sectype.NameEng : sectype.Name)),
                             AbiturientTypeId = App.SecondTypeId,
                             IsImported = Commit.IsImported,
                             IsAddedToProtocol = AppInProtocol != null,
                             CampaignYear = Entry.CampaignYear,

                             HasSeparateObrazPrograms = false,
                             HasNotSpecifiedInnerPriorities = false,

                         }).ToList();
                    bool ExistNotSelectedExams = false;
                    foreach (SimpleApplication app in tblAppsMain)
                    {
                        var lst = Util.GetExamList(app.Id);
                        //есть внутренние приоритеты?
                        app.HasSeparateObrazPrograms = context.ApplicationDetails.Where(x => x.ApplicationId == app.Id).Count() > 0;
                        // записи в таблице ApplicationDetails сделаны автоматически, или пользователем?
                        app.HasNotSpecifiedInnerPriorities = context.ApplicationDetails.Where(x => x.ApplicationId == app.Id && !(x.ByUser ?? true)).Count() > 0;
                        List<string> sDetails = new List<string>();
                        var details = (from det in context.ApplicationDetails
                                       where det.ApplicationId == app.Id
                                       orderby det.InnerEntryInEntryPriority
                                       select new
                                       {
                                           ObProgram = det.InnerEntryInEntry.SP_ObrazProgram.Name,
                                           Profile = det.InnerEntryInEntry.SP_Profile.Name
                                       }).ToList();
                        // сократим запись насколько возможно
                        if (details.Where(x => x.ObProgram != details.First().ObProgram).Count() == 0)
                            sDetails = details.Select(x => x.Profile).ToList();
                        else if (details.Where(x => x.ObProgram != x.Profile).Count() == 0)
                            sDetails = details.Select(x => x.Profile).ToList();
                        else if (details.Where(x => details.Where(t => t.ObProgram == x.ObProgram).Count() > 1).Count() == 0)
                            sDetails = details.Select(x => x.ObProgram).ToList();
                        else
                            sDetails = details.Select(x => x.ObProgram + " (" + x.Profile + ")").ToList();


                        app.InnerPrioritiesMessage = (app.HasSeparateObrazPrograms ? (app.HasNotSpecifiedInnerPriorities ?
                            "<span class='red'>Укажите приоритеты образовательных программ и профилей </span> <a href='/Abiturient/PriorityChangerApplication?AppId='" + app.Id.ToString() + ">" :
                            string.Join(", ", sDetails.ToArray()))
                            : "");

                        app.HasExamsForRegistration = lst.Where(x => x.HasExamTimeTable).Count() > 0;
                        app.ManualExam = new List<string>();

                        if (lst.Count > 0)
                        {
                            foreach (var x in lst)
                            {
                                if (x.ExamInBlockList.Count > 1)
                                    app.ManualExam.Add(x.ExamInBlockList.Where(e => e.Value.ToString() == x.SelectedExamInBlockId.ToString()).Select(e => e.Text.ToString()).FirstOrDefault());
                            }
                            if (app.ManualExam.Where(x => String.IsNullOrEmpty(x)).Count() > 0)
                                ExistNotSelectedExams = true;
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

                    //заявления, которым требуется портфолио
                    var PortfolioApplicationsInCommit =
                        (from App in context.Application
                         join ExinEntBlock in context.ExamInEntryBlock on App.EntryId equals ExinEntBlock.EntryId
                         join ExUnit in context.ExamInEntryBlockUnit on ExinEntBlock.Id equals ExUnit.ExamInEntryBlockId
                         join Ex in context.Exam on ExUnit.ExamId equals Ex.Id
                         where App.CommitId == CommitId && Ex.IsPortfolio == true
                         select new
                         {
                             ApplicationId = App.Id
                         }).Distinct().ToList();

                    foreach (var App in PortfolioApplicationsInCommit)
                    {
                        int ind = tblAppsMain.FindIndex(x => x.Id == App.ApplicationId);
                        if (ind >= 0)
                        {
                            //проверяем, есть ли файлы (эссе, м.п.) по данному конкурсу
                            //int cntFiles = AllFiles.Where(x => x.FileType)
                        }
                    }

                    ExtApplicationCommitModel model = new ExtApplicationCommitModel()
                    {
                        Id = CommitId,
                        Applications = tblAppsMain,
                        Files = AllFiles,
                        IsPrinted = bIsPrinted,
                        Enabled = true,
                        StudyLevelGroupId = (tblAppsMain.Count == 0) ? 1 : tblAppsMain.First().StudyLevelGroupId,
                        HasManualExams = tblAppsMain.Where(x => x.HasManualExams).Count() > 0,
                        HasExamsForRegistration = tblAppsMain.Where(x => x.HasExamsForRegistration).Count() > 0,
                        HasNotSelectedExams = ExistNotSelectedExams,
                        AbiturientTypeId = tblAppsMain.Count > 0 ? tblAppsMain.Select(x => x.AbiturientTypeId).FirstOrDefault() : 1,
                    };

                    foreach (SimpleApplication s in tblAppsMain)
                    {
                        if (s.CampaignYear < Util.iPriemYear)
                            continue;

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
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult AppIndex(string id)
        {
            try
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
                             CommitName = isEng ? (String.IsNullOrEmpty(Entry.StudyLevelNameEng) ? Entry.StudyLevelName : Entry.StudyLevelNameEng) : Entry.StudyLevelName
                         }).FirstOrDefault();
                    if (ApplicationEntity == null)
                        return RedirectToAction("Main", "Abiturient");
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
                    int? c = (int?)Util.AbitDB.GetValue("SELECT top 1 SecondTypeId FROM Application WHERE Id=@Id AND PersonId=@PersonId", new SortedList<string, object>() { { "@PersonId", personId }, { "@Id", ApplicationId } });
                    if (c != 1)
                    {
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
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        [HttpPost]
        public ActionResult AddFile()
        {
            try
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
                    string query = "INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);" +
                        "\n INSERT INTO ApplicationFile (Id, ApplicationId, FileName, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId], FileHash) " +
                        " VALUES (@Id, @ApplicationId, @FileName, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 1, @FileHash)";
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

                    string sFileHash = Util.SHA1Byte(fileData);
                    dic.Add("@FileHash", sFileHash);

                    Util.AbitDB.ExecuteQuery(query, dic);
                    Util.AbitDB.ExecuteQuery(@"update dbo.Application set IsViewed=0 where Id=@ApplicationId", dic);
                }
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Json("Ошибка при записи файла");
                }

                return RedirectToAction("AppIndex", new RouteValueDictionary() { { "id", id } });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        [HttpPost]
        public ActionResult AddFileInCommit()
        {
            try
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
                    string query = "INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);" +
                        "\n INSERT INTO ApplicationFile (Id, CommitId, FileName, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId], FileHash) " +
                        " VALUES (@Id, @CommitId, @FileName, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, @FileTypeId, @FileHash);";
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

                    string sFileHash = Util.SHA1Byte(fileData);
                    dic.Add("@FileHash", sFileHash);

                    int iFileTypeId = (int)Util.AbitDB.GetValue("SELECT ISNULL(ApplicationFileTypeId, 1) FROM PersonFileType where Id=" + PersonFileTypeId);
                    dic.Add("@FileTypeId", iFileTypeId);

                    Util.AbitDB.ExecuteQuery(query, dic);
                    Util.AbitDB.ExecuteQuery(@"update dbo.Application set IsViewed=0 where CommitId=@CommitId", dic);
                }
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Json("Ошибка при записи файла");
                }

                return RedirectToAction("Index", new RouteValueDictionary() { { "id", id } });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult GetFile(string id)
        {
            try
            {
                Guid FileId = new Guid();
                if (!Guid.TryParse(id, out FileId))
                    return Content("Некорректный идентификатор файла");

                Guid PersonId;
                if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                    return Content("Authorization required");

                SortedList<string, object> slParams = new SortedList<string, object>();
                slParams.Add("@Id", FileId);

                DataTable tbl = Util.AbitDB.GetDataTable("SELECT FileName, MimeType, FileExtention FROM AllFiles WHERE Id=@Id", slParams);

                if (tbl.Rows.Count == 0)
                    return Content("Файл не найден");

                string fileName = tbl.Rows[0].Field<string>("FileName");
                string contentType = tbl.Rows[0].Field<string>("MimeType");
                string ext = tbl.Rows[0].Field<string>("FileExtention");

                byte[] content = (byte[])Util.AbitDB.GetValue("SELECT FileData FROM FileStorage WHERE Id=@Id", slParams);

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
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Content("Ошибка при чтении файла");
                }
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult GetPrint(string id)
        {
            try
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
                    catch (Exception exception)
                    {
                        var ravenClient = new RavenClient(Util.SentryDSNHost);
                        ravenClient.Capture(new SentryEvent(exception));
                        return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка при печати заявления"), "text/plain");
                    }
                }
                return new FileContentResult(bindata, "application/pdf") { FileDownloadName = "Application.pdf" };
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult Disable(string id)
        {
            try
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
                    catch (Exception exception)
                    {
                        var ravenClient = new RavenClient(Util.SentryDSNHost);
                        ravenClient.Capture(new SentryEvent(exception));
                        var res = new { IsOk = false, ErrorMessage = "Ошибка при поиске заявления. Попробуйте обновить страницу" };
                        return Json(res);
                    }
                }
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult DisableFull(string id)
        {
            try
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
                    bool? isEnabled = context.Application.Where(x => x.CommitId == CommitId && x.PersonId == PersonId).Select(x => (bool?)x.Enabled).FirstOrDefault();

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

                        query = string.Format("DELETE FROM [ApplicationSelectedTimetable] WHERE CommitId=@Id");
                        Util.AbitDB.ExecuteQuery(query, dic);

                        var res = new { IsOk = true, Enabled = false };
                        return Json(res);
                    }
                    catch (Exception exception)
                    {
                        var ravenClient = new RavenClient(Util.SentryDSNHost);
                        ravenClient.Capture(new SentryEvent(exception));
                        var res = new { IsOk = false, ErrorMessage = "Ошибка при поиске заявления. Попробуйте обновить страницу" };
                        return Json(res);
                    }
                }
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult MotivatePost()
        {
            try
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
                    string query = " INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);" +
                        "\n INSERT INTO ApplicationFile (Id, ApplicationId, FileName, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId], FileHash) " +
                        "\n VALUES (@Id, @ApplicationId, @FileName, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 2, @FileHash);";
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

                    string sFileHash = Util.SHA1Byte(fileData);
                    dic.Add("@FileHash", sFileHash);

                    Util.AbitDB.ExecuteQuery(query, dic);
                }
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Json("Ошибка при записи файла");
                }

                return RedirectToAction("AppIndex", new RouteValueDictionary() { { "id", id } });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult EssayPost()
        {
            try
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
                                             where app.PersonId == PersonId && (apf.Comment.StartsWith(FileTypeName) || (apf.Comment.StartsWith("Эссе")))
                                             select apf.FileName))
                                            .Union(
                                            (from apf in context.ApplicationFile
                                             join app in context.Application on apf.CommitId equals app.CommitId
                                             where app.PersonId == PersonId && (apf.Comment.StartsWith(FileTypeName) || (apf.Comment.StartsWith("Эссе")))
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
                    string query = " INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);" +
                        "\n INSERT INTO ApplicationFile (Id, ApplicationId, FileName, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId], FileHash) " +
                        "\n VALUES (@Id, @ApplicationId, @FileName, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 3, @FileHash);";
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

                    string sFileHash = Util.SHA1Byte(fileData);
                    dic.Add("@FileHash", sFileHash);

                    Util.AbitDB.ExecuteQuery(query, dic);
                }
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Json("Ошибка при записи файла");
                }

                return RedirectToAction("AppIndex", new RouteValueDictionary() { { "id", id } });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public JsonResult ChangePriority(string id, string pr)
        {
            try
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
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Json(new { IsOk = false, ErrorMessage = "Ошибка при обновлении данных" });
                }
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(string id)
        {
            try
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
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.ErrorWhileDeleting };
                    return Json(res);
                }

                var result = new { IsOk = true, ErrorMessage = "" };
                return Json(result);
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }
        [HttpPost]
        public ActionResult GetFileList(string id)
        {
            try
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
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        public ActionResult SaveAllFiles(string HiddenId)
        {
            try
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

                        string BasisName = (isEng) ? "Entry.StudyBasisNameEng" : "Entry.StudyBasisName";

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
                        --,(case when (qAbitFiles_OnlyEssayMotivLetter.FileTypeId = 2) then '" + MotiveMail + @"' else '" + Essay + @"' end) as FileTypeId
                        ,FileTypeName" + (isEng ? "Eng" : "") + @" as FileTypeId
                        ,qAbitFiles_OnlyEssayMotivLetter.[IsApproved]
                        ,(case when (qAbitFiles_OnlyEssayMotivLetter.ApplicationId is not null or qAbitFiles_OnlyEssayMotivLetter.CommitId is not null ) then ('" + ApplicationFile + @" (' + " + BasisName + @" +')') else '" + SharedFile + @"' end ) as AddInfo 
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
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        [HttpPost]
        public JsonResult AddMark(string FileId, string Mark, string Iamsure)
        {
            try
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
                catch (Exception exception)
                {
                    var ravenClient = new RavenClient(Util.SentryDSNHost);
                    ravenClient.Capture(new SentryEvent(exception));
                    return Json(new { IsOk = false, ErrorMessage = "Error in process updating/inserting the mark" });
                }

                return Json(new { IsOk = true });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }
       
        public ActionResult ExamsTimetable(string id)
        {
            try
            {
                Guid personId;
                if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                    return RedirectToAction("LogOn", "Account");

                Guid CommitId = new Guid();
                if (!Guid.TryParse(id, out CommitId))
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

                                where app.CommitId == CommitId
                                select new
                                {
                                    AppId = app.Id,
                                    BlockId = bl.Id,
                                    UnitId = un.Id,
                                    ExamId = un.ExamId,
                                    SelExam = (sexam == null) ? false : un.Id == sexam.ExamInEntryBlockUnitId,
                                    ExamName = exn.Name,
                                }).ToList();

                    // получили список юнитов в блоках 
                    var Temp_lst = (from a in apps
                                    group a by a.BlockId into Apps
                                    // если в блоке только 1 юнит (выбирать не из чего) или есть выбранный юнит
                                    where Apps.Count() == 1 || Apps.Where(x => x.SelExam).Count() > 0
                                    select new
                                    {
                                        UnitId = Apps.Count() == 1 ? Apps.Select(x => x.UnitId).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.UnitId).FirstOrDefault(),
                                        ExamId = Apps.Count() == 1 ? Apps.Select(x => x.ExamId).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.ExamId).FirstOrDefault(),
                                        ExamName = Apps.Count() == 1 ? Apps.Select(x => x.ExamName).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.ExamName).FirstOrDefault(),
                                    }).ToList();
                    var lst = (from a in Temp_lst
                               group a by a.ExamId into _a
                               select new
                               {
                                   ExamId = _a.Key,
                                   ExamName = _a.First().ExamName,
                                   Units = _a.Select(x => x.UnitId).ToList(),
                               }).Distinct().ToList();

                    ApplicationExamsTimeTableModel model = new ApplicationExamsTimeTableModel();
                    model.gCommId = CommitId;
                    model.Comment = "";

                    model.lst = new List<AppExamsTimeTable>();
                    foreach (var ex in lst)
                    {
                        // все расписание по выбранному экзамену
                        List<ExamTtable> tt = (from x in context.ExamInEntryBlockUnitTimetable
                                               join un in context.ExamInEntryBlockUnit on x.ExamInEntryBlockUnitId equals un.Id
                                               join bTT in context.ExamBaseTimetable on x.ExamBaseTimetableId equals bTT.Id

                                               join se in context.ApplicationSelectedTimetable on
                                                                          new { CommId = CommitId, bTTId = bTT.Id } equals
                                                                          new { CommId = se.CommitId, bTTId = se.ExamBaseTimetableId } into _se
                                               from sexam in _se.DefaultIfEmpty()

                                               where ex.ExamId == un.ExamId
                                               && ex.Units.Contains(un.Id)
                                               && bTT.DateOfClose >= DateTime.Now
                                               select new ExamTtable
                                               {
                                                   Id = bTT.Id,
                                                   ExamDate = bTT.ExamDate,
                                                   Address = bTT.Address,
                                                   isSelected = (sexam != null),
                                                   isEnable = true,
                                                   DateOfClose = bTT.DateOfClose,
                                               }).Distinct().ToList();
                        if (tt.Count != 0)
                        {
                            model.lst.Add(new AppExamsTimeTable()
                            {
                                ExamId = ex.ExamId,
                                ExamName = ex.ExamName,
                                lstTimeTable = tt,
                            });
                        }
                    }

                    return View(model);
                }
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }

        [HttpPost]
        public ActionResult ExamsTimetableSave(string id)
        {
            try
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
                                    ExamId = un.ExamId,
                                    SelExam = (sexam == null) ? false : un.Id == sexam.ExamInEntryBlockUnitId,
                                    SelTT = (sexam == null) ? -1 : sexam.ExamTimetableId,
                                }).ToList();

                    var Temp_lst = (from a in apps
                                    group a by a.BlockId into Apps
                                    where Apps.Count() == 1 || Apps.Where(x => x.SelExam).Count() > 0
                                    select new
                                    {
                                        ExamId = Apps.Count() == 1 ? Apps.Select(x => x.ExamId).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.ExamId).FirstOrDefault(),
                                        TimeTableId = Apps.Count() == 1 ? Apps.Select(x => x.SelTT).FirstOrDefault() : Apps.Where(x => x.SelExam).Select(x => x.SelTT).FirstOrDefault(),
                                    }).Distinct().ToList();

                    foreach (var ap in Temp_lst)
                    {
                        string val = (Request.Form["app_" + ap.ExamId.ToString()]);
                        int TTid;
                        if (int.TryParse(val, out TTid))
                        {
                            List<ApplicationSelectedTimetable> SelExams = (from x in context.ApplicationSelectedTimetable
                                                                           join bTT in context.ExamBaseTimetable on x.ExamBaseTimetableId equals bTT.Id
                                                                           where x.CommitId == gCommitId && bTT.ExamId == ap.ExamId
                                                                           select x).ToList();
                            ApplicationSelectedTimetable SelExam = (SelExams == null || SelExams.Count == 0) ? new ApplicationSelectedTimetable() : SelExams.First();

                            bool bExamExists = true;
                            if (SelExams == null || SelExams.Count == 0)
                            {
                                bExamExists = false;
                                SelExam.CommitId = gCommitId;
                            }

                            SelExam.ExamBaseTimetableId = TTid;
                            SelExam.RegistrationDate = DateTime.Now;

                            if (SelExams != null)
                                context.ApplicationSelectedTimetable.RemoveRange(SelExams.Where(x => x.Id != SelExam.Id).ToList());

                            if (!bExamExists)
                                context.ApplicationSelectedTimetable.Add(SelExam);

                            context.SaveChanges();
                        }
                    }
                }
                return RedirectToAction("ExamsTimetable", new RouteValueDictionary() { { "id", gCommitId.ToString("N") } });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }
        public ActionResult ExamsRegistrationClear(string id)
        {
            try
            {
                Guid PersonId;
                if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                    return RedirectToAction("LogOn", "Account");

                Guid gCommitId;
                if (!Guid.TryParse(id.ToString(), out gCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var apps = (from se in context.ApplicationSelectedTimetable
                                where se.CommitId == gCommitId
                                select se).ToList();
                    context.ApplicationSelectedTimetable.RemoveRange(apps);
                    context.SaveChanges();
                }
                return RedirectToAction("ExamsTimetable", new RouteValueDictionary() { { "id", gCommitId.ToString("N") } });
            }
            catch (Exception exception)
            {
                var ravenClient = new RavenClient(Util.SentryDSNHost);
                ravenClient.Capture(new SentryEvent(exception));
                throw;
            }
        }
    }
}