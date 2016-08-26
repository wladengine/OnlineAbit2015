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
    public class CommunicationController : Controller
    {
        public List<Guid> GetEntryList(bool rf)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                List<Guid> EntryList = (from En in context.Entry
                                        where En.LicenseProgramId == 971 && En.ObrazProgramId == 2500 && En.SemesterId == 1
                                        && (rf ? !En.IsForeign : true)
                                        select En.Id).ToList();
                return EntryList;
            }
        }
        public List<string> GetSortOrder(string sort)
        {
            List<string> sSortResult = new List<string>();

            if (!String.IsNullOrEmpty(sort))
            {
                List<string> Sortlst = sort.Split('_').ToList();
                Dictionary<string, string> SortResult = new Dictionary<string, string>();

                for (int i = Sortlst.Count() - 1; i >= 0; i--)
                {
                    string s = Sortlst[i];

                    if (String.IsNullOrEmpty(s))
                        continue;

                    string Number = s.Substring(0, s.Length - 1);
                    if (!SortResult.ContainsKey(Number))
                    {
                        SortResult.Add(Number, s[s.Length - 1].ToString());
                        if (s[s.Length - 1].ToString() != "n")
                            sSortResult.Add(s);
                    }
                }
            }
            return sSortResult;
        }
        public ActionResult Index(string sort, string rf)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");
            
            DataTable tbl = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
            if (tbl.Rows.Count == 0)
                return RedirectToAction("Main", "Abiturient");
            List<string> sSortResult = GetSortOrder(sort);
            GlobalCommunicationModelApplicantList model = GetModelList(sSortResult, rf);
            return View(model);
        }

        public GlobalCommunicationModelApplicantList GetModelList(List<string> SortOrder, string rf)
        {
            bool RFPriem;
            if (String.IsNullOrEmpty(rf) || (rf.ToLower() != Boolean.TrueString.ToLower() && rf.ToLower() != Boolean.FalseString.ToLower()))
                RFPriem = false;
            else
                RFPriem = bool.Parse(rf);

            GlobalCommunicationModelApplicantList model = new GlobalCommunicationModelApplicantList();
            model.ApplicantList = new List<GlobalCommunicationApplicantShort>();
            model.RFPriem = RFPriem;
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                List<Guid> EntryList = GetEntryList(RFPriem);

                var lst = (from abit in context.Abiturient
                           join pers in context.Person on abit.PersonId equals pers.Id
                           join us in context.User on abit.PersonId equals us.Id

                           join prt in context.PortfolioFilesMark on pers.Id equals prt.PersonId into _port
                           from port in _port.DefaultIfEmpty()

                           where EntryList.Contains(abit.EntryId) && abit.IsCommited 
                          
                           select new GlobalCommunicationApplicantShort
                           {
                               Number = pers.Barcode,

                               FIO = String.IsNullOrEmpty(pers.SurnameEng) ? 
                               (pers.Surname + " " + pers.Name + (String.IsNullOrEmpty(pers.SecondName)? "" : " " + pers.SecondName)).Trim() : 
                               ((pers.SurnameEng ?? "") + " " + (pers.NameEng ?? "") + " " + (pers.SecondNameEng ?? "")).Trim(),
                               
                               FIOEng = !String.IsNullOrEmpty(pers.SurnameEng) ? 
                               (pers.Surname + " " + pers.Name + (String.IsNullOrEmpty(pers.SecondName)? "" : " " + pers.SecondName)).Trim(): 
                               ((pers.SurnameEng ?? "") + " " + (pers.NameEng ?? "") + " " + (pers.SecondNameEng ?? "")).Trim(),

                               Email = us.Email,

                               isComplete = (port == null) ? false : port.IsComplete,

                               PortfolioAssessmentRu = (port==null)? "-" : (port.RuPortfolioPts.HasValue ? port.RuPortfolioPts.ToString() : "-"),
                               PortfolioAssessmentDe = (port == null) ? "-" : (port.DePortfolioPts.HasValue ? port.DePortfolioPts.ToString() : "-"),
                               PortfolioAssessmentCommon = (port == null) ? "-" : ((port.RuPortfolioPts.HasValue && port.DePortfolioPts.HasValue) ? ((port.RuPortfolioPts + port.DePortfolioPts)/2).ToString() : "-"),

                               Interview = (port == null) ? false: (port.Interview ?? false),

                               InterviewAssessmentDe = (port == null) ? "-" : (port.RuInterviewPts.HasValue ? port.RuInterviewPts.ToString() : "-"),
                               InterviewAssessmentRu = (port == null) ? "-" : (port.DeInterviewPts.HasValue ? port.DeInterviewPts.ToString() : "-"),
                               InterviewAssessmentCommon = (port == null) ? "-" : ((port.RuInterviewPts.HasValue && port.DeInterviewPts.HasValue) ? ((port.RuInterviewPts + port.DeInterviewPts)/2).ToString() : "-"),

                               OverallResults = (port == null) ? "-" : ((port.RuPortfolioPts.HasValue && port.DePortfolioPts.HasValue && port.RuInterviewPts.HasValue && port.DeInterviewPts.HasValue)
                               ? ((port.RuPortfolioPts + port.DePortfolioPts + port.RuInterviewPts + port.DeInterviewPts) / 2).ToString() : "-"),

                               StatusId = (port == null) ? 7: port.StatusId,
                               Status = (port == null) ? "X" : port.PortfolioStatus.ShortName,
                               StatusAlt = (port == null) ? "X – incomplete application" : port.PortfolioStatus.Name,
                           }).Distinct().ToList();
                for (int i = SortOrder.Count - 1; i >= 0; i-- )
                {
                    string s = SortOrder[i];
                    switch (s)
                    {
                        case "1d": { lst = lst.OrderByDescending(x => x.Number).ToList(); break; }
                        case "1u": { lst = lst.OrderBy(x => x.Number).ToList(); break; }

                        case "2d": { lst = lst.OrderByDescending(x => x.FIO).ToList(); break; }
                        case "2u": { lst = lst.OrderBy(x => x.FIO).ToList(); break; }

                        case "3d": { lst = lst.OrderByDescending(x => x.isComplete).ToList(); break; }
                        case "3u": { lst = lst.OrderBy(x => x.isComplete).ToList(); break; }

                        case "4d": { lst = lst.OrderByDescending(x => x.PortfolioAssessmentRu).ToList(); break; }
                        case "4u": { lst = lst.OrderBy(x => x.PortfolioAssessmentRu).ToList(); break; }

                        case "5d": { lst = lst.OrderByDescending(x => x.PortfolioAssessmentDe).ToList(); break; }
                        case "5u": { lst = lst.OrderBy(x => x.PortfolioAssessmentDe).ToList(); break; }

                        case "6d": { lst = lst.OrderByDescending(x => x.PortfolioAssessmentCommon).ToList(); break; }
                        case "6u": { lst = lst.OrderBy(x => x.PortfolioAssessmentCommon).ToList(); break; }

                        case "7d": { lst = lst.OrderByDescending(x => x.Interview).ToList(); break; }
                        case "7u": { lst = lst.OrderBy(x => x.Interview).ToList(); break; }

                        case "8d": { lst = lst.OrderByDescending(x => x.InterviewAssessmentRu).ToList(); break; }
                        case "8u": { lst = lst.OrderBy(x => x.InterviewAssessmentRu).ToList(); break; }

                        case "9d": { lst = lst.OrderByDescending(x => x.InterviewAssessmentDe).ToList(); break; }
                        case "9u": { lst = lst.OrderBy(x => x.InterviewAssessmentDe).ToList(); break; }

                        case "10d": { lst = lst.OrderByDescending(x => x.InterviewAssessmentCommon).ToList(); break; }
                        case "10u": { lst = lst.OrderBy(x => x.InterviewAssessmentCommon).ToList(); break; }

                        case "11d": { lst = lst.OrderByDescending(x => x.OverallResults).ToList(); break; }
                        case "11u": { lst = lst.OrderBy(x => x.OverallResults).ToList(); break; }

                        case "12d": { lst = lst.OrderByDescending(x => x.StatusId).ToList(); break; }
                        case "12u": { lst = lst.OrderBy(x => x.StatusId).ToList(); break; }
                    }
                }
                model.ApplicantList = lst;
                model.SortOrder = "";
                for (int i = SortOrder.Count - 1;  i >= 0; i--)
                {
                    model.SortOrder += "_"+SortOrder[i] + "_";
                }
                model.BarcodeList = string.Join(",", model.ApplicantList.Select(x => x.Number).ToList());
            }
            return model;
        }

        public ActionResult ApplicantCard(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            DataTable tblGroup = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
            if (tblGroup.Rows.Count == 0)
                return RedirectToAction("Main", "Abiturient");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ApplicantCard()
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            DataTable tblGroup = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
            if (tblGroup.Rows.Count == 0)
                return RedirectToAction("Main", "Abiturient");

            string sort = Request.Form["SortOrder"];
            string rfpriem = Request.Form["rfpriem"];

            string id = Request.Form["Barcode"];
            string BarcodeList = Request.Form["BarcodeList"];

            GlobalCommunicationApplicant model = new GlobalCommunicationApplicant();
            model.SortOrder = sort;
            model.RFPriem = rfpriem;
            int iNumber;
            if (!int.TryParse(id, out iNumber))
            {
                return RedirectToAction("Index");
            }
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                List<Guid> EntryList = GetEntryList(false);
                bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

                #region GetAccess
                model.isRussian = (from x in context.GroupUsers
                                   where x.PersonId == personId && x.GroupId == Util.GlobalCommunicationGroupId_Ru
                                   select x).Count() > 0;
                model.isGermany = (from x in context.GroupUsers
                                   where x.PersonId == personId && x.GroupId == Util.GlobalCommunicationGroupId_De
                                   select x).Count() > 0;
                #endregion
                #region CommonInfo
                var abitinfo = (from abit in context.Application
                           join pers in context.Person on abit.PersonId equals pers.Id
                           join pCont in context.PersonContacts on pers.Id equals pCont.PersonId
                           join us in context.User on pers.Id equals us.Id
                           where EntryList.Contains(abit.EntryId) && pers.Barcode == iNumber && abit.IsCommited
                           select new
                           {
                               personId = pers.Id,
                               Number = pers.Barcode.ToString(),

                               Surname = pers.Surname,
                               Name = pers.Name,
                               SecondName = pers.SecondName,

                               FIOEng = ((pers.SurnameEng ?? "") + " " + (pers.NameEng ?? "") + " " + (pers.SecondNameEng ?? "")).Trim(),
                               pers.Sex,
                               Nationality = bIsEng ?  pers.Nationality.NameEng : pers.Nationality.Name,
                               pers.BirthPlace,
                               pers.BirthDate,
                               
                               pCont.Code,
                               pCont.Country.IsRussia,
                               Country = bIsEng ? pCont.Country.NameEng : pCont.Country.Name,
                               Region = pCont.Region.Name,
                               pCont.City,
                               pCont.Street,
                               pCont.House,
                               pCont.Korpus,
                               pCont.Flat, 

                               VisaCountry = bIsEng ? pers.PersonVisaInfo.Country.NameEng : pers.PersonVisaInfo.Country.Name,
                               VisaPostAddress = pers.PersonVisaInfo.PostAddress,
                               VisaTown = pers.PersonVisaInfo.Town,

                               pers.PassportValid, 
                               us.Email, 
                               HasFee = (abit.C_Entry.StudyBasisId == 1),
                               HasNoFee = (abit.C_Entry.StudyBasisId == 2),
                           }).ToList();
                
                if (abitinfo == null)
                    return RedirectToAction("Index");
                var person = abitinfo.First();

                model.Number = iNumber.ToString();
                model.Sex = person.Sex;

                model.Surname = person.Surname;
                model.Name = person.Name;
                model.SecondName = person.SecondName;
                model.FioEng = person.FIOEng;
                model.Nationality = person.Nationality;
                model.DateOfBirth = person.BirthDate.Value.ToShortDateString();
                model.PlaceOfBirth = person.BirthPlace;
                model.CountryOfBirth = "";

                string Address = string.Format("{0} {1}{2}", 
                    (person.Code) ?? "",
                    (person.IsRussia ? ((person.Region + ", ") ?? "") : person.Country + ", "),
                    (person.City + ", ") ?? "") 
                    +
                    string.Format("{0} {1} {2} {3}", 
                    person.Street ?? "",
                    (string.IsNullOrEmpty(person.House) ? "" : (bIsEng ? "" : "дом ") + person.House),
                    (string.IsNullOrEmpty(person.Korpus) ? "" : (bIsEng ? "":"корп. ") + person.Korpus),
                    (string.IsNullOrEmpty(person.Flat) ? "" : (bIsEng ? "" : "кв. ") + person.Flat));
                  
                model.Email = person.Email;
                
                model.PosstalAddress = Address;
                model.VisaApplicationPlace = person.VisaCountry + " "+person.VisaTown + " " + person.VisaPostAddress + " ";
                model.PassportValid = person.PassportValid.HasValue ? (person.PassportValid.Value == DateTime.MinValue ? "" : person.PassportValid.Value.ToShortDateString()) : "";
                model.HasFee = abitinfo.Where(x => x.HasFee).Count() > 0;
                model.HasNoFee = abitinfo.Where(x => x.HasNoFee).Count() > 0;

                #endregion
                #region Photo
                DataTable tbl = Util.AbitDB.GetDataTable(@"
SELECT top 1 FileName, FileData, MimeType, FileExtention FROM PersonFile WHERE PersonId=@PersonId and PersonFileTypeId=14 
UNION ALL
SELECT top 1 FileName, FileData, MimeType, FileExtention FROM ApplicationFile 
INNER JOIN Application AP ON AP.Id = ApplicationFile.ApplicationId
WHERE AP.PersonId=@PersonId and FileTypeId=18
UNION ALL
SELECT top 1 FileName, FileData, MimeType, FileExtention FROM ApplicationFile 
INNER JOIN Application AP ON AP.CommitId = ApplicationFile.CommitId
WHERE AP.PersonId=@PersonId and FileTypeId=18 
",
                new SortedList<string, object>() { { "@PersonId", person.personId } });

                if (tbl.Rows.Count > 0)
                {
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
                    model.Photo = Convert.ToBase64String(content);
                }
                #endregion
                #region Certificates
                var cert_lst = (from x in context.PersonLanguageCertificates
                                where x.PersonId == person.personId
                                select new CommunicateCertificateInfo
                                {
                                    TypeName = bIsEng ? x.LanguageCertificatesType.NameEng : x.LanguageCertificatesType.Name,
                                    Number = x.Number,
                                    BoolType = x.LanguageCertificatesType.BoolType,
                                    Result = x.LanguageCertificatesType.BoolType ? Resources.PersonalOffice_Step5.CertificatePassed : x.ResultValue.ToString(),
                                }).ToList();
                model.Certificates = cert_lst;
                #endregion
                #region PortfolioMarks
                var portf = (from x in context.PortfolioFilesMark
                            where x.PersonId == person.personId
                            select x).FirstOrDefault();

                double? RuPort = (portf != null) ?  portf.RuPortfolioPts : null;
                double? DePort = (portf != null) ? portf.DePortfolioPts : null;

                model.RuPortfolioPts = (RuPort != null) ?  RuPort.ToString() : "-"  ;
                model.DePortfolioPts = (DePort != null) ?   DePort.ToString() : "-" ; 
                model.CommonPortfolioPts = (RuPort.HasValue && DePort.HasValue) ? ((RuPort + DePort) / 2).ToString() :  "-";

                model.Interview = (portf != null) ? (portf.Interview ?? false) : false;

                double? RuInt= (portf != null) ? portf.RuInterviewPts : null;
                double? DeInt = (portf != null) ? portf.DeInterviewPts : null;

                model.RuInterviewPts = (RuInt != null) ? RuInt.ToString() : "-";
                model.DeInterviewPts = (DeInt != null) ? DeInt.ToString() : "-";
                model.CommonInterviewPts = (RuInt.HasValue && DeInt.HasValue) ? ((RuInt + DeInt) / 2).ToString() : "-";

                double? Overall = ((RuPort + DePort) / 2 + ((RuInt + DeInt) / 2));
                model.OverallPts = (Overall!= null) ? Overall.ToString() : "-";
                model.StatusId =  (portf != null) ? portf.StatusId : 7;
                model.StatusList = context.PortfolioStatus.Select(m => new SelectListItem() { Value = m.Id.ToString(), Text = m.Name, Selected = (model.StatusId == m.Id) }).ToList();
                model.IsComplete = (portf != null) ? portf.IsComplete : false;
                #endregion
                #region Files

                var lstFiles_PersonFile =
                    (from x in context.PersonFile
                     join type in context.PersonFileType on x.PersonFileTypeId equals type.Id
                     where x.PersonId == person.personId
                     && type.IndexInAppCard > 0
                     
                     select new CommunicationFile()
                     {
                         Id = x.Id,
                         FileName = (x.IsDeleted ? (bIsEng ? "DELETED " : "ФАЙЛ УДАЛЁН ") : "") + x.FileName,
                         IsPersonFile = true,
                         Comment = x.Comment,
                         filetype = bIsEng ? type.NameEng : type.Name,
                         index = type.IndexInAppCard,
                     }).ToList();

                //var lstFiles_InApplication =
                //    (from x in context.ApplicationFile
                //     join ap in context.Application on x.ApplicationId equals ap.Id
                //     where x.FileTypeId == 2 && EntryList.Contains(ap.EntryId) && ap.PersonId == person.personId
                //     select new CommunicationFile()
                //     {
                //         Id = x.Id,
                //         FileName = x.FileName,
                //         IsPersonFile = false,
                //         Comment = x.Comment,
                //         filetype = context.PersonFileType.Where(t => t.Id == 10).Select(t => bIsEng ? t.NameEng : t.Name).FirstOrDefault(),
                //         index = context.PersonFileType.Where(t => t.Id == 10).Select(t => t.IndexInAppCard).FirstOrDefault(),
                //     }).ToList();

                //var lstFiles_InCommit =
                //    (from x in context.ApplicationFile
                //     join ap in context.Application on x.CommitId equals ap.CommitId
                //     where x.FileTypeId == 2 && EntryList.Contains(ap.EntryId) && ap.PersonId == person.personId
                //     select new CommunicationFile()
                //     {
                //         Id = x.Id,
                //         FileName = x.FileName,
                //         IsPersonFile = false,
                //         Comment = x.Comment,
                //         filetype = context.PersonFileType.Where(t => t.Id == 10).Select(t => bIsEng ? t.NameEng : t.Name).FirstOrDefault(),
                //         index = context.PersonFileType.Where(t => t.Id == 10).Select(t => t.IndexInAppCard).FirstOrDefault(),
                //     }).ToList();

                var lstAppFiles_InApplication =
                    (from x in context.ApplicationFile
                     join ap in context.Application on x.ApplicationId equals ap.Id
                     join AppFileType in context.FileType on x.FileTypeId equals AppFileType.Id
                     join PFT in context.PersonFileType on AppFileType.Id equals PFT.ApplicationFileTypeId
                     where PFT.IndexInAppCard > 0 && EntryList.Contains(ap.EntryId) && ap.PersonId == person.personId
                     
                     select new CommunicationFile()
                     {
                         Id = x.Id,
                         FileName = (x.IsDeleted ? (bIsEng ? "DELETED " : "ФАЙЛ УДАЛЁН ") : "") + x.FileName,
                         IsPersonFile = false,
                         Comment = x.Comment,
                         filetype = bIsEng ? PFT.NameEng : PFT.Name,
                         index = PFT.IndexInAppCard,
                     }).ToList();

                var lstAppFiles_InCommit =
                    (from x in context.ApplicationFile
                     join ap in context.Application on x.CommitId equals ap.CommitId
                     join AppFileType in context.FileType on x.FileTypeId equals AppFileType.Id
                     join PFT in context.PersonFileType on AppFileType.Id equals PFT.ApplicationFileTypeId
                     where PFT.IndexInAppCard > 0 && EntryList.Contains(ap.EntryId) && ap.PersonId == person.personId
                     
                     select new CommunicationFile()
                     {
                         Id = x.Id,
                         FileName = (x.IsDeleted ? (bIsEng ? "DELETED " : "ФАЙЛ УДАЛЁН ") : "") + x.FileName,
                         IsPersonFile = false,
                         Comment = x.Comment,
                         filetype = bIsEng ? PFT.NameEng : PFT.Name,
                         index = PFT.IndexInAppCard,
                     }).ToList();

                var files = lstFiles_PersonFile.Union(lstAppFiles_InApplication).Union(lstAppFiles_InCommit).Distinct().ToList();

                var blockFiles =
                    (from f in files
                     group f by f.filetype into block
                     select new CommunicationFilesBlock
                     {
                          BlockIndex = block.Select(x=>x.index).First(),
                          BlockName = block.Select(x=>x.filetype).First(),
                          lst = block.ToList(),
                     }).ToList();
                model.lstFiles = blockFiles;
                #endregion
                #region BarcodeList
                model.NexNumber = null; 
                model.PrevNumber = null;
                model.BarcodeList = BarcodeList;
                if (!String.IsNullOrEmpty(BarcodeList))
                {
                    List<int> lstBarcode = new List<int>();
                    List<string> Sortlst = BarcodeList.Split(',').ToList();
                    foreach (string s in Sortlst)
                        lstBarcode.Add(int.Parse(s));
                    int index = lstBarcode.IndexOf(iNumber);

                    if (index == 0)
                        model.PrevNumber = lstBarcode[lstBarcode.Count - 1].ToString();
                    else
                        model.PrevNumber = lstBarcode[index - 1].ToString();

                    if (index == lstBarcode.Count - 1)
                        model.NexNumber = lstBarcode[0].ToString();
                    else
                        model.NexNumber = lstBarcode[index + 1].ToString();
                }
                #endregion
            }
            return View(model);
        }

        public void Interview(int pBarcode, bool bResult, OnlinePriemEntities context)
        {
            var person = (from per in context.Person
                          where per.Barcode == pBarcode
                          select per).FirstOrDefault();
            var Prtf = (from p in context.PortfolioFilesMark
                        where p.PersonId == person.Id
                        select p).FirstOrDefault();
            if (Prtf != null)
                Prtf.Interview = bResult;
            else
                context.PortfolioFilesMark.Add(new PortfolioFilesMark() { PersonId = person.Id, Interview = bResult });
            context.SaveChanges();
        }
        public void IsComplete(int pBarcode, bool bResult, OnlinePriemEntities context)
        {
            var person = (from per in context.Person
                          where per.Barcode == pBarcode
                          select per).FirstOrDefault();
            var Prtf = (from p in context.PortfolioFilesMark
                        where p.PersonId == person.Id
                        select p).FirstOrDefault();
            if (Prtf != null)
                Prtf.IsComplete = bResult;
            else
                context.PortfolioFilesMark.Add(new PortfolioFilesMark() { PersonId = person.Id, IsComplete = bResult });
            context.SaveChanges();
        }

        public ActionResult ChangeBoolValue(string Barcode, string result, string type)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");

            int pBarcode;
            if (!int.TryParse(Barcode, out pBarcode))
                return Json("Некорректное значение");

            bool bResult;
            if (result.ToString() == "1")
                bResult = true;
            else if (result.ToString() == "0")
                bResult = false;
            else
                return Json("Некорректное значение");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var person = (from per in context.Person
                              where per.Barcode == pBarcode
                              select per).FirstOrDefault();
                if (person != null)
                {
                    var Prtf = (from p in context.PortfolioFilesMark
                                where p.PersonId == person.Id
                                select p).FirstOrDefault();
                    
                    switch (type)
                    {
                        case "Interview": { Interview(pBarcode, bResult, context); break; }
                        case "IsComplete": { IsComplete(pBarcode, bResult, context); break; }
                        default: { break; }
                    } 
                    return Json("");
                }
                else
                    return Json("Некорректное значение");

            }
        }
        public ActionResult UpdateRuPts(string Barcode, string RuPort, string RuInt)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");

            int pBarcode;
            if (!int.TryParse(Barcode, out pBarcode))
                return Json("Некорректное значение");

            double tmp;
            double? iRuPort, iRuInt;

            if (!string.IsNullOrEmpty(RuPort))
                RuPort = RuPort.Replace('.', ',');
            if (!string.IsNullOrEmpty(RuInt))
                RuInt = RuInt.Replace('.', ',');

            if (double.TryParse(RuPort, out tmp))
                iRuPort =  tmp;
            else
                iRuPort = null;

            if (double.TryParse(RuInt, out tmp))
                iRuInt = tmp;
            else
                iRuInt = null;

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var person = (from per in context.Person
                              where per.Barcode == pBarcode
                              select per).FirstOrDefault();
                if (person != null)
                {
                    var Prtf = (from p in context.PortfolioFilesMark
                                where p.PersonId == person.Id
                                select p).FirstOrDefault();
                    if (Prtf != null)
                    {
                        Prtf.RuPortfolioPts = iRuPort;
                        Prtf.RuInterviewPts = iRuInt;
                    }
                    else
                    {
                        context.PortfolioFilesMark.Add(new PortfolioFilesMark()
                        {
                            PersonId = person.Id,
                            RuPortfolioPts = iRuPort,
                            RuInterviewPts = iRuInt,
                        });
                    }
                    context.SaveChanges();
                }
                else
                    return Json("Некорректное значение");
                return Json("");
            }
        }
        public ActionResult UpdateDePts(string Barcode, string DePort, string DeInt)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");

            int pBarcode;
            if (!int.TryParse(Barcode, out pBarcode))
                return Json("Некорректное значение");

            double tmp;
            double?  iDePort, iDeInt;

            if (double.TryParse(DePort, out tmp))
                iDePort = tmp;
            else
                iDePort = null;

            if (double.TryParse(DeInt, out tmp))
                iDeInt = tmp;
            else
                iDeInt = null;

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var person = (from per in context.Person
                              where per.Barcode == pBarcode
                              select per).FirstOrDefault();
                if (person != null)
                {
                    var Prtf = (from p in context.PortfolioFilesMark
                                where p.PersonId == person.Id
                                select p).FirstOrDefault();
                    if (Prtf != null)
                    {
                        Prtf.DePortfolioPts = iDePort;
                        Prtf.DeInterviewPts = iDeInt;
                    }
                    else
                    {
                        context.PortfolioFilesMark.Add(new PortfolioFilesMark()
                        {
                            PersonId = person.Id,
                            DePortfolioPts = iDePort,
                            DeInterviewPts = iDeInt,
                        });
                    }
                    context.SaveChanges();
                }
                else
                    return Json("Некорректное значение");
                return Json("");
            }
        }

        public ActionResult UpdateStatus(string Barcode, string StatusId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");

            int pBarcode;
            if (!int.TryParse(Barcode, out pBarcode))
                return Json("Некорректное значение");

            int iStatus;

            if (!int.TryParse(StatusId, out iStatus))
                return Json("Некорректное значение");
            
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var person = (from per in context.Person
                              where per.Barcode == pBarcode
                              select per).FirstOrDefault();
                if (person != null)
                {
                    var Prtf = (from p in context.PortfolioFilesMark
                                where p.PersonId == person.Id
                                select p).FirstOrDefault();
                    bool bIsNew = false;
                    if (Prtf == null)
                    {
                        Prtf = new PortfolioFilesMark();
                        Prtf.PersonId = person.Id;
                        bIsNew = true;
                    }

                    Prtf.StatusId = iStatus;

                    if (bIsNew)
                        context.PortfolioFilesMark.Add(Prtf);

                    context.SaveChanges();
                }
                else
                    return Json("Некорректное значение");
                return Json("");
            }
        }

        public ActionResult Statistics(string id)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            CommunicationStat model = new CommunicationStat();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                List<Guid> EntryList = GetEntryList(false);
                bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

                var lst_tmp = (from abit in context.Application
                               join pers in context.Person on abit.PersonId equals pers.Id

                               join prt in context.PortfolioFilesMark on pers.Id equals prt.PersonId into _port
                               from port in _port.DefaultIfEmpty()

                               where EntryList.Contains(abit.EntryId) && abit.IsCommited
                               select new
                               {
                                   Number = pers.Barcode,
                                   isComplete = (port == null) ? false : port.IsComplete,
                                   male = pers.Sex,

                                   isActive = (port == null) ? false : port.PortfolioStatus.ShortName == "A",
                                   isWaiting = (port == null) ? false : port.PortfolioStatus.ShortName == "W",

                                   isRusslian = pers.Nationality.IsRussia,
                                   nationality = bIsEng ? pers.Nationality.NameEng : pers.Nationality.Name,
                                   isGerman = pers.NationalityId == 90,

                                   Interview = (port == null) ? false : port.Interview,
                                   RuPort = (port == null) ? 0 : port.RuPortfolioPts,
                                   DePort = (port == null) ? 0 : port.DePortfolioPts,
                                   RuInt = (port == null) ? 0 : port.RuInterviewPts,
                                   DeInt = (port == null) ? 0 : port.DeInterviewPts,

                                   HasFee = (abit.C_Entry.StudyBasisId == 1),
                                   HasNoFee = (abit.C_Entry.StudyBasisId == 2),
                               }).ToList();

                var lst = (from ls in lst_tmp
                           group ls by ls.Number into l
                           select new
                           {
                               Number = l.Key,
                               l.First().isComplete,
                               isAccepted = l.First().isActive,
                               l.First().isWaiting,
                               l.First().isRusslian,
                               l.First().isGerman,
                               l.First().nationality,
                               l.First().RuPort,
                               l.First().DePort,
                               l.First().RuInt,
                               l.First().DeInt,
                               l.First().male,
                               l.First().Interview,
                               HasFee = l.Where(x => x.HasFee).Count() > 0,
                               HasNoFee = l.Where(x => x.HasNoFee).Count() > 0,
                           }).Distinct().ToList();
                model.Add("Number of applicants", lst.Count().ToString());
                model.Add("Number of countries", lst.Select(x=>x.nationality).Distinct().Count().ToString());
                model.Add("<hr/>", "<hr/>");

                int Compl = (lst.Count() != 0) ? (int)(100 * lst.Where(x => x.isComplete).Count() / lst.Count()) : 0;
                model.Add("Complete/Incomplete(%)", Compl.ToString() + "% / " + (100 - Compl).ToString() + "%");

                int Male = (lst.Count() != 0) ? (int)(100 * lst.Where(x => x.male).Count() / lst.Count()) : 0;
                model.Add("Male/Female(%)", Male.ToString() + "% / " + (100 - Male).ToString() + "%");

                int PaidCnt = (lst_tmp.Count != 0) ? (int)(100.0 * lst_tmp.Where(x => x.HasNoFee).Count() / lst_tmp.Count()) : 0;
                model.Add("Paid/Free(%)", (lst_tmp.Count != 0) ? PaidCnt.ToString() + "% /" + (100 - PaidCnt).ToString() + "%" : "-");
                model.Add("<hr/>", "<hr/>");

                var Nationalities = (from l in lst
                                     group l by l.nationality into ls
                                     select new
                                         {
                                             Name = ls.Key,
                                             Cnt = ls.Count(),
                                             isComplete = ls.Where(x=>x.isComplete).Count(),
                                             isNotComplete= ls.Where(x=>!x.isComplete).Count(),
                                         }).ToList();
                string sNationlities = "<table> <tr> <td>Country</td><td>Complete</td><td>Incomplete</td><td>Total</td></tr>";
                foreach (var x in Nationalities)
                {
                    sNationlities += "<tr><td>" + x.Name + "</td><td> " + x.isComplete.ToString() + "</td><td> " + x.isNotComplete.ToString() + "</td><td> " + x.Cnt.ToString() + "</tr>";
                }
                sNationlities += "</table>";
                model.Add("List of nationalities and no. of applications", sNationlities);
                model.Add("<hr/>", "<hr/>");

                int isRussian = (lst.Count() != 0) ? (int)(100 * lst.Where(x => x.isRusslian).Count() / lst.Count()) : 0;
                model.Add("Russian(%)", isRussian.ToString() + "%");

                int isGerman = (lst.Count() != 0) ? (int)(100 * lst.Where(x => x.isGerman).Count() / lst.Count()) : 0;
                model.Add("German(%)", isGerman.ToString() + "%");
                model.Add("Other(%)", (100 - isRussian - isGerman).ToString() + "%");
                model.Add("<hr/>", "<hr/>");

                int PortAvg = (lst.Count() != 0) ? (int)(lst.Select(x => x.RuPort + x.DePort).Sum() / (2 * lst.Count())) : 0;
                model.Add("Average results of portfolio", PortAvg.ToString());
                model.Add("<hr/>", "<hr/>");

                int Invitation = (lst.Count() != 0) ? (int)(100 * lst.Where(x => x.Interview??false).Count() / lst.Count()) : 0;
                model.Add("Invitation to interviews(%)", Invitation.ToString() + "%");

                int IntAvg = (lst.Count() != 0) ? (int)(lst.Select(x => x.RuInt + x.DeInt).Sum() / (2 * lst.Count())) : 0;
                model.Add("Average results of interview", (Invitation == 0) ? "-" : IntAvg.ToString());
                model.Add("<hr/>", "<hr/>");

                int isAccpeted = lst.Where(x => x.isAccepted).Count();
                model.Add("accepted applicants (status=a)", isAccpeted.ToString());

                int AcceptedMale = (isAccpeted != 0) ? (int)(100 * lst.Where(x => x.male && x.isAccepted).Count() / isAccpeted) : 0;
                model.Add("accepted: Male/Female(%)", (isAccpeted != 0) ? (AcceptedMale.ToString() + "% / " + (100 - AcceptedMale).ToString() + "%") : "-");

                int AcceptedFree = (isAccpeted != 0) ? (int)(100 * lst.Where(x => x.HasFee && x.isAccepted).Count() / isAccpeted) : 0;
                model.Add("accepted: Free/Paid(%)", (isAccpeted != 0) ? (AcceptedFree.ToString() + "% / " + (100 - AcceptedFree).ToString() + "%") : "-");
                model.Add("<hr/>", "<hr/>");

                int isWaiting = lst.Where(x => x.isWaiting).Count();
                model.Add("Waiting list(status = w)", isWaiting.ToString());
                int WaitingMale = (isWaiting != 0) ? (int)(100 * lst.Where(x => x.male && x.isWaiting).Count() / isWaiting) : 0;
                model.Add("Waiting: Male/Female(%)", (isWaiting != 0) ? (WaitingMale.ToString() + "% / " + (100 - WaitingMale).ToString() + "%") : "-");
            }
            return View(model);
        }
 
        public ActionResult GetPrint(string Barcode)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Authentification Error"), "text/plain");

            int iBarcode;
            if (!int.TryParse(Barcode, out iBarcode))
                return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка идентификатора"), "text/plain");

            byte[] bindata;
            string filename = "ApplicationCard";
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                try
                {
                    var Abiturient = context.Person.Where(x => x.Barcode == iBarcode).Select(x => x).FirstOrDefault();
                    if (Abiturient == null)
                        return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка идентификатора"), "text/plain");
                    filename = Abiturient.Surname + " " + Abiturient.Name + " " + Abiturient.Barcode.ToString();
                    if (!String.IsNullOrEmpty(Abiturient.SurnameEng))
                        filename = Abiturient.SurnameEng + " " + Abiturient.NameEng + " " + Abiturient.Barcode.ToString();
                    bindata = PDFUtils.GetCommunicationAppCard(Server.MapPath("~/Templates/"), Abiturient.Id);
                }
                catch
                {
                    return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка при печати заявления"), "text/plain");
                }
            }
            return new FileContentResult(bindata, "application/pdf") { FileDownloadName = filename + ".pdf" };
        }

        public ActionResult PrintListToPDF(string sort, string rf)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
            if (tbl.Rows.Count == 0)
                return RedirectToAction("Main", "Abiturient");

            List<string> sSortResult = new List<string>();

            if (!String.IsNullOrEmpty(sort))
            {
                List<string> Sortlst = sort.Split('_').ToList();
                Dictionary<string, string> SortResult = new Dictionary<string, string>();

                for (int i = Sortlst.Count() - 1; i >= 0; i--)
                {
                    string s = Sortlst[i];

                    if (String.IsNullOrEmpty(s))
                        continue;

                    string Number = s.Substring(0, s.Length - 1);
                    if (!SortResult.ContainsKey(Number))
                    {
                        SortResult.Add(Number, s[s.Length - 1].ToString());
                        if (s[s.Length - 1].ToString() != "n")
                            sSortResult.Add(s);
                    }
                }
            }
            GlobalCommunicationModelApplicantList model = GetModelList(sSortResult, rf);
            byte[] bindata;
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                try
                {
                    bindata = PDFUtils.GetCommunicationAbitList(Server.MapPath("~/Templates/"), model);
                }
                catch
                {
                    return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка при печати заявления"), "text/plain");
                }
            }
            return new FileContentResult(bindata, "application/pdf") { FileDownloadName = "ApplicationCard.pdf" };
        }

        public ActionResult PrintListToXLS(string sort, string rf)
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
            if (tbl.Rows.Count == 0)
                return RedirectToAction("Main", "Abiturient");

            List<string> sSortResult = new List<string>();

            if (!String.IsNullOrEmpty(sort))
            {
                List<string> Sortlst = sort.Split('_').ToList();
                Dictionary<string, string> SortResult = new Dictionary<string, string>();

                for (int i = Sortlst.Count() - 1; i >= 0; i--)
                {
                    string s = Sortlst[i];

                    if (String.IsNullOrEmpty(s))
                        continue;

                    string Number = s.Substring(0, s.Length - 1);
                    if (!SortResult.ContainsKey(Number))
                    {
                        SortResult.Add(Number, s[s.Length - 1].ToString());
                        if (s[s.Length - 1].ToString() != "n")
                            sSortResult.Add(s);
                    }
                }
            }
            GlobalCommunicationModelApplicantList model = GetModelList(sSortResult, rf);
            model.ApplicantList = model.ApplicantList.ToList();

            byte[] bindata;
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                try
                {
                    bindata = XLSUtils.PrintAllToExcel2007(model, "list", "AbiturientList.xlsx");
                }
                catch
                {
                    return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("Ошибка при печати заявления"), "text/plain");
                }
            }
            return new FileContentResult(bindata, "application/xlsx") { FileDownloadName = "AbiturientList.xlsx" };
        }

    }
}