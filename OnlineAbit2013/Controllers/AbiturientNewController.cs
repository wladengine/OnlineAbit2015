using OnlineAbit2013.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineAbit2013.Controllers
{
    public class AbiturientNewController : Controller
    {
        #region UFMS
        public ActionResult RusLangExam_ufms(RuslangExamModelPersonList model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                model.Enable = false;
                return View("RusLangExam_ufms", model);
            }
            if (PersonId != Guid.Parse("65C7E9CE-FDBD-4382-82E0-782FB59CB2C0"))
            {
                model.Enable = false;
                return View("RusLangExam_ufms", model);
            }

            model.Enable = true;
            model.PersonList = new List<RuslangExamModelPerson>();
            if (!string.IsNullOrEmpty(model.findstring))
            {
                using (RuslangExamEntities context = new RuslangExamEntities())
                {

                    var prefix = (from l in context.LevelName
                                  select l.PrefixSertificate).Distinct().ToList();

                    string sPrefix = "";
                    string sNum = model.findstring;

                    if (model.findstring.Length > 9)
                    {
                        foreach (var x in prefix)
                        {
                            if (sNum.StartsWith(x))
                            {
                                sNum = sNum.Substring(x.Length, sNum.Length - x.Length);
                                sPrefix = x;
                                break;
                            }
                        }
                    }

                    while (sNum.StartsWith("0"))
                        sNum = sNum.Substring(1, sNum.Length - 1);

                    long Num = 0;
                    if (long.TryParse(sNum, out Num))
                    {
                        if (String.IsNullOrEmpty(sPrefix))
                        {
                            var Sert2014 = (from sert in context.Sertificate
                                            join pers in context.Person on sert.ParentId equals pers.Id
                                            join passp in context.Passport on pers.Id equals passp.ParentId
                                            join sx in context.Sex on passp.sex_id equals sx.Id
                                            join nat in context.Nationality on passp.nationality_id equals nat.Id
                                            join lvl in context.Level on sert.Level_id equals lvl.Id
                                            join lvlName in context.LevelName on lvl.RealLevel equals lvlName.Id
                                            where sert.SertificateNumber2014 == Num
                                            select new
                                            {
                                                personId = pers.Id,
                                                SertificateId = sert.Id,
                                                passp.FIO,
                                                Nationality = nat.Name,
                                                Sex = sx.Name,
                                                sert.SertificateNumber2014,
                                                lvlName.PrefixSertificate,
                                            }).Distinct().ToList();

                            foreach (var s in Sert2014)
                            {
                                string FullNum = s.SertificateNumber2014.ToString();
                                while (s.PrefixSertificate.Length + FullNum.Length < 12)
                                    FullNum = "0" + FullNum;
                                model.PersonList.Add(new RuslangExamModelPerson() { Id = s.SertificateId, Name = s.FIO + "(" + s.PrefixSertificate + FullNum + ")" });
                            }


                            var Sert = (from sert in context.Sertificate
                                        join pers in context.Person on sert.ParentId equals pers.Id
                                        join passp in context.Passport on pers.Id equals passp.ParentId
                                        join sx in context.Sex on passp.sex_id equals sx.Id
                                        join nat in context.Nationality on passp.nationality_id equals nat.Id
                                        where sert.SertificateNumber == Num
                                        select new
                                        {
                                            personId = pers.Id,
                                            SertificateId = sert.Id,
                                            passp.FIO,
                                            sert.SertificateNumber,
                                            Nationality = nat.Name,
                                            Sex = sx.Name,
                                        }).Distinct().ToList();
                            foreach (var s in Sert)
                            {
                                string FullNum = s.SertificateNumber.ToString();
                                while (FullNum.Length < 7)
                                    FullNum = "0" + FullNum;
                                model.PersonList.Add(new RuslangExamModelPerson() { Id = s.SertificateId, Name = s.FIO + "(" + FullNum + ")" });
                            }
                            var SertComplex = (from sert in context.Sertificate
                                               join pers in context.Person on sert.ParentId equals pers.Id
                                               join passp in context.Passport on pers.Id equals passp.ParentId
                                               join sx in context.Sex on passp.sex_id equals sx.Id
                                               join nat in context.Nationality on passp.nationality_id equals nat.Id
                                               join lvl in context.Level on sert.Level_id equals lvl.Id
                                               join lvlName in context.LevelName on lvl.RealLevel equals lvlName.Id
                                               where sert.SertificateNumber2014Complex == Num
                                               select new
                                               {
                                                   personId = pers.Id,
                                                   SertificateId = sert.Id,
                                                   passp.FIO,
                                                   sert.SertificateNumber2014Complex,
                                                   Nationality = nat.Name,
                                                   Sex = sx.Name,
                                                   lvlName.PrefixSertificate,
                                               }).Distinct().ToList();
                            foreach (var s in SertComplex)
                            {
                                string FullNum = s.SertificateNumber2014Complex.ToString();
                                while (s.PrefixSertificate.Length + FullNum.Length < 12)
                                    FullNum = "0" + FullNum;
                                model.PersonList.Add(new RuslangExamModelPerson() { Id = s.SertificateId, Name = s.FIO + "(" + s.PrefixSertificate + FullNum + ")" });
                            }
                        }
                        else
                        {
                            var SertComplex = (from sert in context.Sertificate
                                               join pers in context.Person on sert.ParentId equals pers.Id
                                               join passp in context.Passport on pers.Id equals passp.ParentId
                                               join sx in context.Sex on passp.sex_id equals sx.Id
                                               join nat in context.Nationality on passp.nationality_id equals nat.Id
                                               join lvl in context.Level on sert.Level_id equals lvl.Id
                                               join lvlName in context.LevelName on lvl.RealLevel equals lvlName.Id
                                               where
                                               (sert.SertificateNumber2014Complex == Num || sert.SertificateNumber2014 == Num)
                                               && lvlName.PrefixSertificate.Trim() == sPrefix
                                               select new
                                               {
                                                   personId = pers.Id,
                                                   SertificateId = sert.Id,
                                                   passp.FIO,
                                                   sert.SertificateNumber2014Complex,
                                                   sert.SertificateNumber2014,
                                                   Nationality = nat.Name,
                                                   Sex = sx.Name,
                                                   lvlName.PrefixSertificate,
                                                   lvlName.PublicLevelName,
                                               }).Distinct().ToList();
                            foreach (var s in SertComplex)
                            {
                                if (s.PublicLevelName.ToLower().Contains("комплек"))
                                {
                                    string FullNum = s.SertificateNumber2014Complex.ToString();
                                    while (s.PrefixSertificate.Length + FullNum.Length < 12)
                                        FullNum = "0" + FullNum;
                                    model.PersonList.Add(new RuslangExamModelPerson() { Id = s.SertificateId, Name = s.FIO + "(" + s.PrefixSertificate + FullNum + ")" });
                                }
                                else
                                {
                                    string FullNum = s.SertificateNumber2014.ToString();
                                    while (s.PrefixSertificate.Length + FullNum.Length < 12)
                                        FullNum = "0" + FullNum;
                                    model.PersonList.Add(new RuslangExamModelPerson() { Id = s.SertificateId, Name = s.FIO + "(" + s.PrefixSertificate + FullNum + ")" });
                                }
                            }
                        }
                    }
                }
            }
            return View("RusLangExam_ufms", model);
        }

        public ActionResult ufms(string HiddenId)
        {
            string Id = HiddenId;
            if (!String.IsNullOrEmpty(Id))
            {
                if (Id.Equals("0ceebbda2bee50e227710e7322152ef2"))
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

                    return RedirectToAction("RusLangExam_ufms", "AbiturientNew");
                }
            }
            return RedirectToAction("LogOn", "Account");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetUFMS(string sertId)
        {
            long _sertId = long.Parse(sertId);

            using (RuslangExamEntities context = new RuslangExamEntities())
            {
                var s = (from p in context.Person
                         join pas in context.Passport on p.Id equals pas.ParentId
                         join pastype in context.PassportType on pas.passporttype_id equals pastype.Id
                         join nat in context.Nationality on pas.nationality_id equals nat.Id
                         join sert in context.Sertificate on p.Id equals sert.ParentId
                         join lvl in context.Level on sert.Level_id equals lvl.Id
                         join lvlName in context.LevelName on lvl.RealLevel equals lvlName.Id
                         join sex in context.Sex on pas.sex_id equals sex.Id
                         where sert.Id == _sertId
                         select new
                         {
                             p.Id,
                             pas.FIO,
                             pas.BirthDate,
                             document = ((pastype.Id == 1) ? "" : pastype.Name) + " " + (String.IsNullOrEmpty(pas.Seria) ? "" : pas.Seria + " ") + (String.IsNullOrEmpty(pas.Number) ? "" : pas.Number),
                             Nationality = nat.Name,
                             sert.SertificateNumber2014,
                             sert.SertificateNumber,
                             sert.SertificateNumber2014Complex,
                             lvlName.PrefixSertificate,
                             lvlName.PublicLevelName,
                             Sex = sex.Name
                         }).Distinct().FirstOrDefault();

                if (s == null)
                    return Json(new { NoFree = true });

                var Abit = (from PrintV in context.PrintViewSertificate
                            where PrintV.SertificateId == _sertId
                            select PrintV).FirstOrDefault();

                string FIO = s.FIO;
                string Nationality = s.Nationality;
                string Level = s.PublicLevelName;
                string BirthDate = s.BirthDate.ToShortDateString();
                string Document = s.document;
                string FullNum = "";

                if (!String.IsNullOrEmpty(Abit.SertificateNumber.ToString()))
                {
                    FullNum = Abit.SertificateNumber.ToString();
                    while (FullNum.Length < 7)
                        FullNum = "0" + FullNum;
                }
                else
                    if (!String.IsNullOrEmpty(Abit.SertificateNumber2014.ToString()))
                    {
                        FullNum = Abit.SertificateNumber2014.ToString();
                        while (s.PrefixSertificate.Length + FullNum.Length < 12)
                            FullNum = "0" + FullNum;
                        FullNum = s.PrefixSertificate + FullNum;

                    }
                    else if (!String.IsNullOrEmpty(Abit.SertificateNumber2014Complex.ToString()))
                    {
                        FullNum = Abit.SertificateNumber2014Complex.ToString();
                        while (s.PrefixSertificate.Length + FullNum.Length < 12)
                            FullNum = "0" + FullNum;
                        FullNum = s.PrefixSertificate + FullNum;
                    }

                string Number = FullNum;

                string Date = Abit.ExamenDate.Value.ToShortDateString();
                string Sex = s.Sex;
                return Json(new { fio = FIO, level = Level, sex = Sex, nationality = Nationality, date = Date, birthdate = BirthDate, document = Document, number = Number });
            }
        }
        #endregion
    }
}
