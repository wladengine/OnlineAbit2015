using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineAbit2013.Models;
using System.Data;
using System.Web.Routing;

namespace OnlineAbit2013.Controllers
{
    public class CommunicationController : Controller
    {
        public ActionResult Index()
        {
            Guid personId;
            if (!Util.CheckAuthCookies(Request.Cookies, out personId))
                return RedirectToAction("LogOn", "Account");

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", personId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
            if (tbl.Rows.Count==0)
                return RedirectToAction("Main", "Abiturient");

            GlobalCommunicationModelApplicantList model = new GlobalCommunicationModelApplicantList();
            model.ApplicantList = new List<GlobalCommunicationApplicantShort>();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                List<Guid> EntryList = (from En in context.Entry
                                        where En.LicenseProgramId == 971 && En.ObrazProgramId == 2500 && En.SemesterId == 1
                                        select En.Id).ToList();

                var lst = (from abit in context.Abiturient
                           join pers in context.Person on abit.PersonId equals pers.Id
                           where EntryList.Contains(abit.EntryId)
                           select new GlobalCommunicationApplicantShort
                           {
                               Number = pers.Barcode.ToString(),

                               Surname = pers.Surname,
                               Name = pers.Name,
                               SecondName = pers.SecondName,

                               isComplete = false,
                               Status = "",
                               PortfolioAssessmentRu = 0,
                               OverallResults = 0,
                               InterviewAssessmentDe = 0,
                               InterviewAssessmentCommon = 0,
                               InterviewAssessmentRu = 0,
                               PortfolioAssessmentCommon = 0,
                               PortfolioAssessmentDe = 0
                           }).Distinct().ToList();
                model.ApplicantList = lst;
            }
            return View(model);
        }

        public ActionResult ApplicantCard(string id)
        {
            GlobalCommunicationApplicant model = new GlobalCommunicationApplicant();
            int iNumber;
            if (!int.TryParse(id, out iNumber))
            {
                return RedirectToAction("Index");
            }
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                List<Guid> EntryList = (from En in context.Entry
                                        where En.LicenseProgramId == 971 && En.ObrazProgramId == 2500 && En.SemesterId == 1
                                        select En.Id).ToList();

                var person = (from abit in context.Abiturient
                           join pers in context.Person on abit.PersonId equals pers.Id
                           join pCont in context.PersonContacts on pers.Id equals pCont.PersonId
                           join us in context.User on pers.Id equals us.Id
                           where EntryList.Contains(abit.EntryId) && pers.Barcode == iNumber
                           select new
                           {
                               personId = pers.Id,
                               Number = pers.Barcode.ToString(),

                               Surname = pers.Surname,
                               Name = pers.Name,
                               SecondName = pers.SecondName,
                               pers.Sex,
                               Nationality = pers.Nationality.Name,
                               pers.BirthPlace,
                               pers.BirthDate,
                               
                               pCont.Code,
                               pCont.Country.IsRussia,
                               Country = pCont.Country.Name,
                               countryeng = pCont.Country.NameEng,
                               Region = pCont.Region.Name,
                               pCont.City,
                               pCont.Street,
                               pCont.House,
                               pCont.Korpus,
                               pCont.Flat, 

                               VisaCountry = pers.PersonVisaInfo.Country.Name,
                               VisaPostAddress = pers.PersonVisaInfo.PostAddress,
                               VisaTown = pers.PersonVisaInfo.Town,

                               pers.PassportValid, 
                               us.Email, 
                           }).FirstOrDefault();
                if (person == null)
                    return RedirectToAction("Index");

                model.Number = iNumber.ToString();
                model.Sex = person.Sex;

                model.Surname = person.Surname;
                model.Name = person.Name;
                model.SecondName = person.SecondName;

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
                    (person.House == string.Empty ? "" : "дом " + person.House),
                    (person.Korpus == string.Empty ? "" : "корп. " + person.Korpus),
                    (person.Flat == string.Empty ? "" : "кв. " + person.Flat));
                  
                model.Email = person.Email;
                
                model.PosstalAddress = Address;
                model.VisaApplicationPlace = person.VisaCountry + " "+person.VisaTown + " " + person.VisaPostAddress + " ";
                model.PassportValid = person.PassportValid.HasValue ? (person.PassportValid.Value == DateTime.MinValue ? "" : person.PassportValid.Value.ToShortDateString()) : "";

                DataTable tbl = Util.AbitDB.GetDataTable("SELECT top 1 FileName, FileData, MimeType, FileExtention FROM PersonFile WHERE PersonId=@PersonId and (FileExtention='.jpeg' or FileExtention='.jpg')",
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
            }
            return View(model);
        }
    }
}