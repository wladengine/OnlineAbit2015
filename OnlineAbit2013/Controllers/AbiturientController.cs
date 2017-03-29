using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OnlineAbit2013.Models;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Net.Mail;
using System.Data.Entity.Core.Objects;
using OnlineAbit2013.EMDX;

namespace OnlineAbit2013.Controllers
{
    public class AbiturientController : Controller
    {
        int maxBlockTransfer = 1;
        int maxBlockChangeStudyFormBasis = 1;
        int maxBlockRecover = 1;

        public ActionResult OpenPersonalAccount()
        {
            Request.Cookies.SetThreadCultureByCookies();
            return View("PersonStartPage");
        }

        [HttpPost]
        public ActionResult OpenPersonalAccount(OpenPersonalAccountModel model)
        {
            Guid UserId;
            Util.CheckAuthCookies(Request.Cookies, out UserId);

            string param = Request.Form["Val"];
            string val = Request.Form["val_h"];
            int res = 0;

            switch (val)
            {
                case "1": { res = 1; break; } //Поступление на 1 курс гражданам РФ
                case "2": { res = 2; break; } //Поступление на 1 курс иностранным гражданам
                case "3": { res = 3; break; } //Перевод из российского университета в СПбГУ
                case "4": { res = 4; break; } //Перевод из иностранного университета в СПбГУ
                case "5": { res = 5; break; } //Восстановление в СПбГУ
                case "6": { res = 6; break; } //Перевод с платной формы обучения на бюджетную
                case "7": { res = 7; break; } //Смена образовательной программы
                case "8": { res = 8; break; } //Поступление в Академическую Гимназию
                case "9": { res = 9; break; } //Поступление в СПО
                case "10": { res = 10; break; } //Поступление в аспирантуру гражданам РФ
                case "11": { res = 11; break; } //Поступление в аспирантуру иностранным гражданам
                default: { res = 1; break; }
            }

            //создаём запись человека в базе
            string query = "SELECT COUNT(*) FROM Person WHERE Id=@Id";
            int cnt = (int)Util.AbitDB.GetValue(query, new SortedList<string, object>() { { "@Id", UserId } });
            if (cnt == 0)
            {
                query = "INSERT INTO Person(Id, UserId, AbiturientTypeId) VALUES (@Id, @Id, @Type)";
                Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Id", UserId }, { "@Type", res} });
            }
            
            switch (res)
            {
                case 1: { return RedirectToAction("Index"); }
                case 2: { return RedirectToAction("Index", "ForeignAbiturient"); }
                case 3: { return RedirectToAction("Index", "Transfer"); }
                case 4: { return RedirectToAction("Index", "TransferForeign"); }
                case 5: { return RedirectToAction("Index", "Recover"); }
                case 6: { return RedirectToAction("Index", "ChangeStudyForm"); }
                case 7: { return RedirectToAction("Index", "ChangeObrazProgram"); }
                case 8: { return RedirectToAction("Index", "AG"); }
                case 9: { return RedirectToAction("Index", "SPO"); }
                case 10: { return RedirectToAction("Index", "Aspirant"); }
                case 11: { return RedirectToAction("Index", "ForeignAspirant"); }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Index(string step)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            if (Util.CheckIsNew(PersonId))
                return RedirectToAction("OpenPersonalAccount");

            int stage = 0;
            if (!int.TryParse(step, out stage))
                stage = 1;
            
            bool isEng = Util.GetCurrentThreadLanguageIsEng(); 

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                string query;
                var Person = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (Person == null)//paranoia
                    return RedirectToAction("OpenPersonalAccount");

                if (Person.RegistrationStage == 0)
                    stage = 1;
                else if (Person.RegistrationStage < stage)
                    stage = Person.RegistrationStage;

                PersonalOffice model = new PersonalOffice() { Lang = "ru", Stage = stage != 0 ? stage : 1, Enabled = !Util.CheckPersonReadOnlyStatus(PersonId) };
                model.ConstInfo = new Constants();
                model.ConstInfo = Util.getConstInfo();

                if (model.Stage == 1)
                {
                    #region Stage 1
                    model.PersonInfo = new InfoPerson();
                    model.ContactsInfo = new ContactsPerson();
                    var PersonContacts = Person.PersonContacts;
                    if (PersonContacts == null)
                        PersonContacts = new PersonContacts();
                    model.PersonInfo.Surname = Server.HtmlDecode(Person.Surname);
                    model.PersonInfo.Name = Server.HtmlDecode(Person.Name);
                    model.PersonInfo.SecondName = Server.HtmlDecode(Person.SecondName);

                    model.PersonInfo.SurnameEng = Server.HtmlDecode(Person.SurnameEng);
                    model.PersonInfo.NameEng = Server.HtmlDecode(Person.NameEng);
                    model.PersonInfo.SecondNameEng = Server.HtmlDecode(Person.SecondNameEng);

                    model.PersonInfo.Sex = Person.Sex ? "M" : "F";
                    model.PersonInfo.CountryOfBirth = Person.CountryOfBirth.ToString();
                    model.PersonInfo.Nationality = Person.NationalityId.ToString();
                    
                    model.ContactsInfo.CountryId = PersonContacts.CountryId.ToString();

                    model.PersonInfo.BirthPlace = Server.HtmlDecode(Person.BirthPlace);
                    model.PersonInfo.BirthDate = Person.BirthDate.HasValue ? Person.BirthDate.Value.ToString("dd.MM.yyyy") : "";
                    model.PersonInfo.NationalityList = Util.GetCountryList();
                    model.ContactsInfo.CountryList   = Util.GetCountryList();
                    model.PersonInfo.HasRussianNationality = Person.HasRussianNationality;
                    model.PersonInfo.SexList = new List<SelectListItem>()
                    {
                        new SelectListItem () {Text = isEng ? "Male" : "Мужской", Value = "M" },
                        new SelectListItem () {Text = isEng ? "Female" : "Женский", Value = "F" }
                    };
                    #endregion
                    return View("PersonalOffice_Page1", model);
                }
                else if (model.Stage == 2)
                {
                    #region Stage 2
                    model.PersonInfo = new InfoPerson();
                    model.PassportInfo = new PassportPerson();
                    model.res = Util.GetRess(PersonId);
                    string strTblPsp;
                    int defaultPsp = 1;
                    switch (model.res)
                    {
                        case 1: { strTblPsp = "SELECT Id, Name, NameEng FROM PassportType "; break; }
                        case 2: { strTblPsp = "SELECT Id, Name, NameEng FROM PassportType "; break; }
                        case 3: { strTblPsp = "SELECT Id, Name, NameEng FROM PassportType "; break; }
                        case 4: { strTblPsp = "SELECT Id, Name, NameEng FROM PassportType WHERE IsApprovedForeign=1"; defaultPsp = 2; break; }
                        default: { strTblPsp = "SELECT Id, Name, NameEng FROM PassportType "; break; }
                    }
                    DataTable tblPsp = Util.AbitDB.GetDataTable(strTblPsp, null);
                    model.PassportInfo.PassportTypeList =
                        (from DataRow rw in tblPsp.Rows
                         select new SelectListItem() { Value = rw.Field<int>("Id").ToString(), Text = isEng? rw.Field<string>("NameEng") : rw.Field<string>("Name") }).
                        ToList();
                    model.PassportInfo.PassportType = (Person.PassportTypeId ?? defaultPsp).ToString();
                    model.PassportInfo.PassportSeries = Server.HtmlDecode(Person.PassportSeries);
                    model.PassportInfo.PassportNumber = Server.HtmlDecode(Person.PassportNumber);
                    model.PassportInfo.PassportAuthor = Server.HtmlDecode(Person.PassportAuthor);
                    model.PassportInfo.PassportDate = Person.PassportDate.HasValue ? Person.PassportDate.Value.ToString("dd.MM.yyyy") : "";
                    model.PassportInfo.PassportCode = Server.HtmlDecode(Person.PassportCode);
                    if (model.res == 4)
                    {
                        model.PassportInfo.PassportValid = Person.PassportValid.HasValue ? Person.PassportValid.Value.ToString("dd.MM.yyyy") : "";

                        model.VisaInfo = new VisaInfo();
                        DataTable tblCountr =
                            Util.AbitDB.GetDataTable(
                                string.Format("SELECT Id, Name, NameEng FROM [Country] ORDER BY LevelOfUsing DESC, {0}", isEng ? "NameEng" : "Name"),
                                null);
                        model.VisaInfo.CountryList = (from DataRow rw in tblCountr.Rows
                                                      select new SelectListItem()
                                                      {
                                                          Value = rw.Field<int>("Id").ToString(),
                                                          Text = rw.Field<string>(isEng ? "NameEng" : "Name")
                                                      }).ToList();

                        var PersonVisaInfo = Person.PersonVisaInfo;
                        if (PersonVisaInfo == null)
                            PersonVisaInfo = new PersonVisaInfo();
                        model.VisaInfo.CountryId = PersonVisaInfo.CountryId.ToString();
                        model.VisaInfo.PostAddress = PersonVisaInfo.PostAddress;
                        model.VisaInfo.Town = PersonVisaInfo.Town;
                    }
                    model.PersonInfo.SNILS = Person.SNILS ?? "";
                    model.Files = Util.GetFileList(PersonId, "1");
                    model.FileTypes = Util.GetPersonFileTypeList();
                    #endregion
                    return View("PersonalOffice_Page2", model);
                }
                else if (model.Stage == 3)
                {
                    #region Stage 3
                    model.ContactsInfo = new ContactsPerson();
                    var PersonContacts = Person.PersonContacts;
                    if (PersonContacts == null)
                        PersonContacts = new PersonContacts();

                    model.res = Util.GetRess(PersonId);
                    model.ContactsInfo.MainPhone = Server.HtmlDecode(PersonContacts.Phone);
                    model.ContactsInfo.SecondPhone = Server.HtmlDecode(PersonContacts.Mobiles);
                    model.ContactsInfo.AddEmail = Server.HtmlDecode(PersonContacts.AddEmail);

                    model.ContactsInfo.CountryId = PersonContacts.CountryId.ToString();
                    model.ContactsInfo.RegionId = PersonContacts.RegionId.ToString();

                    model.ContactsInfo.PostIndex = Server.HtmlDecode(PersonContacts.Code);
                    model.ContactsInfo.City = Server.HtmlDecode(PersonContacts.City);
                    model.ContactsInfo.Street = Server.HtmlDecode(PersonContacts.Street);
                    model.ContactsInfo.House = Server.HtmlDecode(PersonContacts.House);
                    model.ContactsInfo.Korpus = Server.HtmlDecode(PersonContacts.Korpus);
                    model.ContactsInfo.Flat = Server.HtmlDecode(PersonContacts.Flat);

                    model.ContactsInfo.CountryRealId = PersonContacts.CountryRealId.ToString();
                    model.ContactsInfo.RegionRealId = PersonContacts.RegionRealId.ToString();
                    model.ContactsInfo.PostIndexReal = Server.HtmlDecode(PersonContacts.CodeReal);
                    model.ContactsInfo.CityReal = Server.HtmlDecode(PersonContacts.CityReal);
                    model.ContactsInfo.StreetReal = Server.HtmlDecode(PersonContacts.StreetReal);
                    model.ContactsInfo.HouseReal = Server.HtmlDecode(PersonContacts.HouseReal);
                    model.ContactsInfo.KorpusReal = Server.HtmlDecode(PersonContacts.KorpusReal);
                    model.ContactsInfo.FlatReal = Server.HtmlDecode(PersonContacts.FlatReal);

                    model.ContactsInfo.CountryList = Util.GetCountryList();

                    query = "SELECT Id, Name FROM Region WHERE RegionNumber IS NOT NULL ORDER BY Distance, Name";
                    model.ContactsInfo.RegionList =
                        (from DataRow rw in Util.AbitDB.GetDataTable(query, null).Rows
                         select new SelectListItem()
                         {
                             Value = rw.Field<int>("Id").ToString(),
                             Text = rw.Field<string>("Name")
                         }).ToList();
                    #endregion
                    return View("PersonalOffice_Page3", model);
                }
                else if (model.Stage == 4)
                {
                    #region Stage 4
                    string temp_str;
                    model.Messages = new List<PersonalMessage>();
                    if (!isEng)
                    {
                        temp_str = "<li>Для <b>перевода в СПбГУ</b> (из другого университета) выберите 'ВУЗ' в поле 'Тип образовательного учреждения' и 'Перевод в СПбГУ' в поле 'Тип поступления'<br>"
                                        + "<br><li>Для <b>восстановления в СПбГУ</b> выберите 'ВУЗ' в поле 'Тип образовательного учреждения' и 'Восстановление в СПбГУ' в поле 'Тип поступления'<br>"
                                         + "<br><li>Для <b>смены образовательной программы, формы и основы обучения</b> выберите 'ВУЗ' в поле 'Тип образовательного учреждения' и 'Перевод внутри СПбГУ' в поле 'Тип поступления'<br>"
                                         + "<br>В остальных случаях выбирайте  'тип образовательного учреждения' в соответствии с имеющимся у вас образованием.";
                        model.Messages.Add(new PersonalMessage() { Id = "0", Type = MessageType.TipMessage, Text = temp_str });
                    }

                    model.EducationInfo = new EducationPerson();
                    model.EducationInfo.QualificationList = Util.GetQualificationList(isEng);
                    
                    query = "SELECT Id, Name, NameEng FROM SchoolTypeAll";
                    DataTable _tblT = Util.AbitDB.GetDataTable(query, null);
                    var SchoolTypeList =
                        (from DataRow rw in _tblT.Rows
                         select new SelectListItem()
                         {
                             Value = rw.Field<int>("Id").ToString(),
                             Text = (isEng ?
                                        (string.IsNullOrEmpty(rw.Field<string>("NameEng")) ? rw.Field<string>("Name") : rw.Field<string>("NameEng"))
                                        : rw.Field<string>("Name"))
                         }).ToList();

                    model.EducationInfo.RegionList = Util.RegionsAll
                        .Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value }).ToList();
                    model.EducationInfo.SchoolTypeList = SchoolTypeList;
                    model.EducationInfo.SchoolExitClassList = context.SchoolExitClass.OrderBy(x => x.IntValue)
                            .Select(x => new { x.Id, x.Name, x.NameEng }).ToList()
                            .Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = isEng ? x.NameEng : x.Name })
                            .ToList();
                    model.EducationInfo.VuzAdditionalTypeList = context.VuzAdditionalType
                            .Select(x => new { x.Id, Name = isEng ? x.NameEng : x.Name }).ToList()
                            .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() })
                            .ToList();
                    model.EducationInfo.StudyFormList = Util.GetStudyFormBaseList();
                    model.EducationInfo.CountryList = Util.GetCountryList();


                    //-----------------EducationDocuments-------------------------
                    model.EducationInfo.EducationDocumentsMaxCount = Util.getConstInfo().EducationDocumentsMaxCount ?? 5;

                    var PersonEducationDocumentList = Person.PersonEducationDocument.ToList();
                    model.EducationInfo.EducationDocuments = new List<EducationDocumentPerson>();
                    foreach (var PersonEducationDocument in PersonEducationDocumentList)
                    {
                        var EPD = new EducationDocumentPerson();

                        EPD.sId = PersonEducationDocument.Id.ToString();
                        EPD.SchoolTypeId = PersonEducationDocument.SchoolTypeId.ToString();

                        EPD.SchoolTypeList = (from DataRow rw in _tblT.Rows
                                              select new SelectListItem()
                                              {
                                                  Value = rw.Field<int>("Id").ToString(),
                                                  Text = (isEng ?
                                                             (string.IsNullOrEmpty(rw.Field<string>("NameEng")) ? rw.Field<string>("Name") : rw.Field<string>("NameEng"))
                                                             : rw.Field<string>("Name"))
                                              }).ToList().SetValue(EPD.SchoolTypeId.ToString());

                        EPD.SchoolName = Server.HtmlDecode(PersonEducationDocument.SchoolName);
                        EPD.SchoolNumber = Server.HtmlDecode(PersonEducationDocument.SchoolNum);
                        EPD.SchoolExitYear = Server.HtmlDecode(PersonEducationDocument.SchoolExitYear);
                        EPD.SchoolCity = Server.HtmlDecode(PersonEducationDocument.SchoolCity);
                        EPD.AvgMark = PersonEducationDocument.AvgMark.HasValue ? PersonEducationDocument.AvgMark.Value.ToString() : "";
                        EPD.IsExcellent = PersonEducationDocument.IsExcellent;
                        EPD.IsEqual = PersonEducationDocument.IsEqual;
                        EPD.EqualityDocumentNumber = PersonEducationDocument.EqualDocumentNumber;

                        EPD.RegionList = Util.RegionsAll
                            .Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value })
                            .ToList().SetValue(PersonEducationDocument.RegionEducId.ToString());
                        EPD.RegionEducId = Server.HtmlDecode(PersonEducationDocument.RegionEducId.ToString());

                        // добавить сортировку по Name
                        EPD.SchoolExitClassList = context.SchoolExitClass.OrderBy(x => x.IntValue)
                            .Select(x => new { x.Id, x.Name, x.NameEng }).ToList()
                            .Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = isEng ? x.NameEng : x.Name })
                            .ToList().SetValue(PersonEducationDocument.SchoolExitClassId.ToString());
                        EPD.SchoolExitClassId = Server.HtmlDecode(PersonEducationDocument.SchoolExitClassId.ToString());

                        EPD.VuzAdditionalTypeList = context.VuzAdditionalType
                            .Select(x => new { x.Id, Name = isEng ? x.NameEng : x.Name }).ToList()
                            .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() })
                            .ToList().SetValue(PersonEducationDocument.VuzAdditionalTypeId.ToString());
                        EPD.VuzAdditionalTypeId = Server.HtmlDecode(PersonEducationDocument.VuzAdditionalTypeId.ToString());

                        var PersonHighEducationInfo = PersonEducationDocument.PersonHighEducationInfo;
                        if (PersonHighEducationInfo == null)
                            PersonHighEducationInfo = new OnlineAbit2013.EMDX.PersonHighEducationInfo();

                        EPD.HEExitYear = Server.HtmlDecode(PersonHighEducationInfo.ExitYear.ToString());
                        EPD.HEEntryYear = Server.HtmlDecode(PersonHighEducationInfo.EntryYear.ToString());
                        EPD.Series = Server.HtmlDecode(PersonEducationDocument.Series);
                        EPD.Number = Server.HtmlDecode(PersonEducationDocument.Number);
                        EPD.ProgramName = Server.HtmlDecode(PersonHighEducationInfo.ProgramName);
                        EPD.DiplomTheme = Server.HtmlDecode(PersonHighEducationInfo.DiplomaTheme);
                        
                        EPD.StudyFormId = PersonHighEducationInfo.StudyFormId.ToString();
                        EPD.StudyFormList = Util.GetStudyFormBaseList().SetValue(PersonHighEducationInfo.StudyFormId.ToString());

                        EPD.QualificationList = Util.GetQualificationList(isEng).SetValue(PersonHighEducationInfo.QualificationId.ToString());
                        EPD.PersonQualification = PersonHighEducationInfo.QualificationId.ToString();

                        EPD.CountryList = Util.GetCountryList().SetValue(PersonEducationDocument.CountryEducId.ToString());
                        EPD.CountryEducId = PersonEducationDocument.CountryEducId.ToString();

                        model.EducationInfo.EducationDocuments.Add(EPD);
                    }
                    //------END--------EducationDocuments------------END----------
                    #endregion
                    return View("PersonalOffice_Page4", model);
                }
                else if (model.Stage == 5)
                {
                    #region Stage 5
                    if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                        return RedirectToAction("LogOn", "Account");
                    
                    var AddInfo = Person.PersonAddInfo;
                    if (AddInfo == null)
                        AddInfo = new PersonAddInfo();

                    var AddEducInfo = new AdditionalEducationInfoPerson();

                    if (model.Messages == null)
                        model.Messages = new List<PersonalMessage>();
                    AddEducInfo.StartEnglish = AddInfo.StartEnglish;
                    AddEducInfo.EnglishMark = AddInfo.EnglishMark.ToString();
                    AddEducInfo.HasTRKI = AddInfo.HasTRKI;
                    AddEducInfo.TRKICertificateNumber = AddInfo.TRKICertificateNumber;
                    AddEducInfo.LanguageId = (AddInfo.LanguageId ?? 1).ToString();
                    AddEducInfo.LanguageList = Util.GetLanguageList();

                    model.AddEducationInfo = AddEducInfo;
                    
                    model.EducationInfo = new EducationPerson();
                    model.EducationInfo.StudyFormList = Util.GetStudyFormList();
                    model.EducationInfo.StudyBasisList = Util.GetStudyBasisList();
                    if (Person.NationalityId != 193)
                        model.AddEducationInfo.HasForeignNationality = true;
                    else
                        model.AddEducationInfo.HasForeignNationality = false;

                    

                    #region CurrentEducation
                    if (Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && (x.VuzAdditionalTypeId == 2 || x.VuzAdditionalTypeId == 4)).Count() > 0)
                    {
                        if (!isEng)
                        {
                            string temp_str = "Будьте внимательны! Для поступления в СПбГУ на программы бакалавриата, магистратуры и аспирантуры необходимо выбирать 'основной прием' на странице 'Образование' (4 стр. анкеты). ";
                         
                            model.Messages.Add(new PersonalMessage() { Id = "0", Type = MessageType.TipMessage, Text = temp_str });
                        }
                        model.AddEducationInfo.HasTransfer = true;
                        model.CurrentEducation = new CurrentEducation();
                        PersonCurrentEducation CurrentEducation = Person.PersonCurrentEducation;
                        if (CurrentEducation == null)
                            CurrentEducation = new PersonCurrentEducation();

                        model.CurrentEducation.EducationTypeList = Util.SchoolTypesAll
                                .Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value })
                                .ToList();

                        query = "SELECT Id, Name FROM Semester WHERE IsIGA = 0";
                        DataTable _tblT = Util.AbitDB.GetDataTable(query, null);
                        model.CurrentEducation.SemesterList =
                            (from DataRow rw in _tblT.Rows
                             select new SelectListItem()
                             {
                                 Value = rw.Field<int>("Id").ToString(),
                                 Text = rw.Field<string>("Name")
                             }).ToList();

                        query = "SELECT Id, Name FROM SP_StudyLevel";
                        _tblT = Util.AbitDB.GetDataTable(query, null);
                        model.CurrentEducation.StudyLevelList =
                            (from DataRow rw in _tblT.Rows
                             select new SelectListItem()
                             {
                                 Value = rw.Field<int>("Id").ToString(),
                                 Text = rw.Field<string>("Name")
                             }).ToList();

                        model.CurrentEducation.StudyLevelId = CurrentEducation.StudyLevelId.ToString();
                        model.CurrentEducation.StudyFormId = CurrentEducation.StudyFormId ?? 1;
                        model.CurrentEducation.SemesterId = CurrentEducation.SemesterId.ToString();
                        model.CurrentEducation.StudyBasisId = CurrentEducation.StudyBasisId ?? 1;
                        model.CurrentEducation.HiddenLicenseProgramId = CurrentEducation.LicenseProgramId.ToString();
                        model.CurrentEducation.HiddenObrazProgramId = CurrentEducation.ObrazProgramId.ToString();
                        model.CurrentEducation.ProfileName = CurrentEducation.ProfileName;
                        

                        model.CurrentEducation.HasAccreditation = CurrentEducation.HasAccreditation;
                        model.CurrentEducation.AccreditationDate = CurrentEducation.AccreditationDate.HasValue ? CurrentEducation.AccreditationDate.Value.ToShortDateString() : "";
                        model.CurrentEducation.AccreditationNumber = CurrentEducation.AccreditationNumber;
                        model.CurrentEducation.EducationTypeId = CurrentEducation.EducTypeId.ToString();
                        model.CurrentEducation.EducationName = CurrentEducation.EducName;
                        model.CurrentEducation.HasScholarship = CurrentEducation.HasScholarship;
                    }
                    #endregion
                    #region ChangeStudyFormReason
                    if (Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && (x.VuzAdditionalTypeId == 2)).Count() > 0)
                    {
                        model.AddEducationInfo.HasReason = true;
                        model.ChangeStudyFormReason = new PersonChangeStudyFormReason();
                        PersonChangeStudyFormReason ChangeStudyFormReason = Person.PersonChangeStudyFormReason;
                        if (ChangeStudyFormReason == null)
                            ChangeStudyFormReason = new PersonChangeStudyFormReason();
                        model.ChangeStudyFormReason.Reason = ChangeStudyFormReason.Reason;
                    }
                    #endregion
                    #region DisorderInfo
                    if (Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && (x.VuzAdditionalTypeId == 3)).Count() > 0)
                    {
                        model.AddEducationInfo.HasRecover = true;
                        model.DisorderInfo = new DisorderedSPBUEducation();
                        if (Person.PersonDisorderInfo != null)
                        {
                            model.DisorderInfo.YearOfDisorder = Person.PersonDisorderInfo.YearOfDisorder;
                            model.DisorderInfo.EducationProgramName = Person.PersonDisorderInfo.EducationProgramName;
                            model.DisorderInfo.IsForIGA = Person.PersonDisorderInfo.IsForIGA;
                        }
                        else
                        {
                            model.DisorderInfo.YearOfDisorder = "";
                            model.DisorderInfo.EducationProgramName = "";
                            model.DisorderInfo.IsForIGA = false;
                        }
                    }
                    #endregion
                    #region AddEducationInfo
                    if (Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 1).Count() > 0)
                    {
                        bool HasEge = Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 1).Select(x =>
                            new
                            {
                                ExitClass = x.SchoolExitClassId.HasValue ? (x.Country.IsRussia ?
                                x.SchoolExitClass.IntValue > 10 :
                                x.SchoolExitClass.IntValue > 9) : false,
                            }).ToList().Where(x=>x.ExitClass).Count() >0;

                        model.AddEducationInfo.HasEGE = HasEge;
                        string qEgeMarks = "SELECT EgeMark.Id, EgeCertificate.Number, EgeExam.Name, EgeMark.Value, EgeMark.IsSecondWave, EgeMark.IsInUniversity FROM Person " +
                            " INNER JOIN EgeCertificate ON EgeCertificate.PersonId = Person.Id INNER JOIN EgeMark ON EgeMark.EgeCertificateId=EgeCertificate.Id " +
                            " INNER JOIN EgeExam ON EgeExam.Id=EgeMark.EgeExamId WHERE Person.Id=@Id";
                        DataTable tblEge = Util.AbitDB.GetDataTable(qEgeMarks, new SortedList<string, object>() { { "@Id", PersonId } });

                        model.EducationInfo.EgeMarks = new List<EgeMarkModel>();
                        model.EducationInfo.EgeMarks =
                            (from DataRow rw in tblEge.Rows
                             select new EgeMarkModel()
                             {
                                 Id = rw.Field<Guid>("Id"),
                                 CertificateNum = rw.Field<string>("Number"),
                                 ExamName = rw.Field<string>("Name"),
                                 Value = rw.Field<bool>("IsSecondWave") ? ("Сдаю во второй волне") : (rw.Field<bool>("IsInUniversity") ? "Сдаю в СПбГУ" : rw.Field<int?>("Value").ToString())
                             }).ToList();
                    }
                    #endregion

                    #region Certificates
                    model.FileTypes = model.FileTypes = Util.GetPersonFileTypeList().Where(x => x.Value == "5").ToList();
                    model.CertificatesVisible = !(model.AddEducationInfo.HasTransfer || model.AddEducationInfo.HasRecover);
                    if (Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 1 && x.SchoolExitClass.IntValue < 10).Count() > 0)
                    {
                        model.CertificatesVisible = false;
                    }
                    model.Certificates = new CertificatesInfo();
                    model.Certificates.CertTypeList = Util.GetCertificatesTypeList();
                    model.Files = Util.GetFileList(PersonId, "5");
                    var certs = context.PersonLanguageCertificates.Where(x => x.PersonId == PersonId).Select(x => new CertificateInfo()
                        {
                            Id = x.Id,
                            Name = isEng ? x.LanguageCertificatesType.NameEng : x.LanguageCertificatesType.Name ,
                            Number = x.Number,
                            BoolResult = x.ResultBool,
                            ValueResult = x.ResultValue,
                            BoolType = x.LanguageCertificatesType.BoolType,
                            ValueType = x.LanguageCertificatesType.ValueType,
                        }).ToList();
                    model.Certificates.Certs = certs;

                    #endregion
                    #endregion
                    return View("PersonalOffice_Page5", model);
                }
                else if (model.Stage == 6)
                {
                    #region Stage 6
                    #region WorkInfo
                    model.WorkInfo = new WorkPerson();
                    query = "SELECT Id, Name, NameEng FROM ScienceWorkType";
                    DataTable tbl = Util.AbitDB.GetDataTable(query, null);

                    model.WorkInfo.ScWorks =
                        (from DataRow rw in tbl.Rows
                         select new SelectListItem()
                         {
                             Value = rw.Field<int>("Id").ToString(),
                             Text = rw.Field<string>(isEng ? "NameEng" : "Name")
                         }).ToList();

                    string qPSW = "SELECT PersonScienceWork.Id, ScienceWorkType.Name, PersonScienceWork.WorkYear, ScienceWorkType.NameEng, PersonScienceWork.WorkInfo FROM PersonScienceWork " +
                        " INNER JOIN ScienceWorkType ON ScienceWorkType.Id=PersonScienceWork.WorkTypeId WHERE PersonScienceWork.PersonId=@Id";
                    DataTable tblPSW = Util.AbitDB.GetDataTable(qPSW, new SortedList<string, object>() { { "@Id", PersonId } });

                    model.WorkInfo.pScWorks =
                        (from DataRow rw in tblPSW.Rows
                         select new ScienceWorkInformation()
                         {
                             Id = rw.Field<Guid>("Id"),
                             ScienceWorkType = rw.Field<string>(isEng ? "NameEng" : "Name"),
                             ScienceWorkInfo = rw.Field<string>("WorkInfo"),
                             ScienceWorkYear = rw.Field<string>("WorkYear")
                         }).ToList();

                    string qPW = "SELECT Id, WorkPlace, Stage, WorkProfession, WorkSpecifications FROM PersonWork WHERE PersonId=@Id";
                    DataTable tblPW = Util.AbitDB.GetDataTable(qPW, new SortedList<string, object>() { { "@Id", PersonId } });

                    model.WorkInfo.pWorks =
                        (from DataRow rw in tblPW.Rows
                         select new WorkInformationModel()
                         {
                             Id = rw.Field<Guid>("Id"),
                             Place = rw.Field<string>("WorkPlace"),
                             Stag = rw.Field<string>("Stage"),
                             Level = rw.Field<string>("WorkProfession"),
                             Duties = rw.Field<string>("WorkSpecifications")
                         }).ToList();
                    #endregion
                    #region Olympiads
                    model.PrivelegeInfo = new PersonPrivileges();
                    model.PrivelegeInfo.pOlympiads = context.Olympiads.Where(x => x.PersonId == PersonId)
                        .Select(x => new OlympiadInformation()
                        {
                            Id = x.Id,
                            OlympYear = x.OlympYear,
                            OlympType = x.OlympType.Name,
                            OlympName = x.OlympName.Name,
                            OlympSubject = x.OlympSubject.Name,
                            OlympValue = x.OlympValue.Name,
                            DocumentSeries = x.DocumentSeries,
                            DocumentNumber = x.DocumentNumber,
                            DocumentDate = x.DocumentDate.HasValue ? x.DocumentDate.Value : DateTime.Now
                        }).ToList();

                    #region PersonPrivileges
                    query = "SELECT Distinct OlympYear FROM OlympBook";
                    DataTable _tbl = Util.AbitDB.GetDataTable(query, null);
                    model.PrivelegeInfo.OlympYearList =
                        (from DataRow rw in _tbl.Rows
                         select new SelectListItem()
                         {
                             Value = rw.Field<int>("OlympYear").ToString(),
                             Text = rw.Field<int>("OlympYear").ToString()
                         }).ToList();

                    query = "SELECT Id, Name, NameEng FROM OlympValue";
                    _tbl = Util.AbitDB.GetDataTable(query, null);
                    model.PrivelegeInfo.OlympValueList =
                        (from DataRow rw in _tbl.Rows
                         select new SelectListItem()
                         {
                             Value = rw.Field<int>("Id").ToString(),
                             Text = (isEng ?
                                        (string.IsNullOrEmpty(rw.Field<string>("NameEng")) ? rw.Field<string>("Name") : rw.Field<string>("NameEng"))
                                        : rw.Field<string>("Name"))
                         }).ToList();
                    #endregion

                    query = "SELECT Id, Name, NameEng FROM SportQualification";
                    _tbl = Util.AbitDB.GetDataTable(query, null);
                    model.PrivelegeInfo.SportQualificationList =
                        (from DataRow rw in _tbl.Rows
                         select new SelectListItem()
                         {
                             Value = rw.Field<int>("Id").ToString(),
                             Text = (isEng ?
                                        (string.IsNullOrEmpty(rw.Field<string>("NameEng")) ? rw.Field<string>("Name") : rw.Field<string>("NameEng"))
                                        : rw.Field<string>("Name"))
                         }).ToList();

                    PersonSportQualification PersonSportQualification = Person.PersonSportQualification;
                    if (PersonSportQualification == null)
                        PersonSportQualification = new OnlineAbit2013.EMDX.PersonSportQualification();
                    else
                    {
                        model.PrivelegeInfo.SportQualificationId = PersonSportQualification.SportQualificationId.ToString();
                        model.PrivelegeInfo.SportQualificationLevel = PersonSportQualification.SportQualificationLevel ?? "";
                        model.PrivelegeInfo.OtherSportQualification = PersonSportQualification.OtherSportQualification ?? "";
                    }
                    #endregion
                    #endregion
                    return View("PersonalOffice_Page6", model);
                }
                else //if (model.Stage == 7)
                {
                    #region Stage 7
                    if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                        return RedirectToAction("LogOn", "Account");

                    var AddInfo = Person.PersonAddInfo;
                    if (AddInfo == null)
                        AddInfo = new PersonAddInfo();

                    model.AddInfo = new AdditionalInfoPerson()
                    {
                        FZ_152Agree = false,
                        ExtraInfo = Server.HtmlDecode(AddInfo.AddInfo),
                        HasPrivileges = AddInfo.HasPrivileges ?? false,
                        HostelAbit = AddInfo.HostelAbit ?? false,
                        HostelEduc = AddInfo.HostelEduc,
                        ContactPerson = Server.HtmlDecode(AddInfo.Parents),

                        VisibleParentBlock = (DateTime.Now.Year - Person.BirthDate.Value.Year) < 18,
                        Parent_Surname = AddInfo.Parent_Surname,
                        Parent_Name  = AddInfo.Parent_Name,
                        Parent_SecondName = AddInfo.Parent_SecondName,
                        Parent_Email = AddInfo.Parent_Email,
                        Parent_Phone = AddInfo.Parent_Phone,
                        Parent_Work = AddInfo.Parent_Work,
                        Parent_WorkPosition = AddInfo.Parent_WorkPosition,
                        Parent2_Surname = AddInfo.Parent2_Surname,
                        Parent2_Name = AddInfo.Parent2_Name,
                        Parent2_SecondName = AddInfo.Parent2_SecondName,
                        Parent2_Email = AddInfo.Parent2_Email,
                        Parent2_Phone = AddInfo.Parent2_Phone,
                        Parent2_Work = AddInfo.Parent2_Work,
                        Parent2_WorkPosition = AddInfo.Parent2_WorkPosition,
                        ReturnDocumentTypeId = Server.HtmlDecode((AddInfo.ReturnDocumentTypeId ?? 1).ToString()),
                        ReturnDocumentTypeList = isEng ?
                            context.ReturnDocumentType.Select(x => new { x.Id, x.NameEng }).ToList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.NameEng }).ToList() :
                            context.ReturnDocumentType.Select(x => new { x.Id, x.Name }).ToList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name }).ToList(),
                        NeedSpecialConditions = AddInfo.NeedSpecialConditions,
                    };
                    #endregion
                    return View("PersonalOffice_Page7", model);
                }
                //return View("PersonalOffice_Page", model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NextStep(PersonalOffice model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var Person = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                int iRegStage = Person.RegistrationStage;

                if (iRegStage == 0)
                    iRegStage = 1;

                if (Util.CheckPersonReadOnlyStatus(PersonId))
                {
                    if (++(model.Stage) <= 6)
                        return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", model.Stage } });
                    else
                        return RedirectToAction("Main", "Abiturient");
                }

                if (model.Stage == 1)
                {
                    #region Stage1
                    DateTime bdate;
                    if (!DateTime.TryParse(model.PersonInfo.BirthDate, CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat, DateTimeStyles.None, out bdate))
                        bdate = DateTime.Now.Date;

                    if (bdate.Date > DateTime.Now.Date)
                        bdate = DateTime.Now.Date;

                    int NationalityId = 193;
                    if (!int.TryParse(model.PersonInfo.Nationality, out NationalityId))
                        NationalityId = 193;

                    int CountryOfBirthId = 193;
                    if (!int.TryParse(model.PersonInfo.CountryOfBirth, out CountryOfBirthId))
                        CountryOfBirthId = 193;

                    int iCountryId = 0;
                    if (!int.TryParse(model.ContactsInfo.CountryId, out iCountryId))
                        iCountryId = 193;//Russia

                    bool bIns = false;
                    var PersonContacts = Person.PersonContacts;
                    if (PersonContacts == null)
                    {
                        PersonContacts = new PersonContacts();
                        PersonContacts.PersonId = PersonId;
                        bIns = true;
                    }

                    Person.Surname = model.PersonInfo.Surname;
                    Person.Name = model.PersonInfo.Name;
                    Person.SecondName = model.PersonInfo.SecondName;
                    Person.BirthDate = bdate;
                    Person.BirthPlace = model.PersonInfo.BirthPlace;
                    Person.CountryOfBirth = CountryOfBirthId;
                    Person.NationalityId = NationalityId;
                    Person.Sex = model.PersonInfo.Sex == "M" ? true : false;
                    Person.RegistrationStage = iRegStage < 2 ? 2 : iRegStage;
                    PersonContacts.CountryId = iCountryId;

                    Person.SurnameEng = model.PersonInfo.SurnameEng;
                    Person.NameEng = model.PersonInfo.NameEng;
                    Person.SecondNameEng = model.PersonInfo.SecondNameEng;

                    if (iCountryId != 193)
                    {
                        PersonContacts.RegionId = context.Country.Where(x => x.Id == iCountryId).Select(x => x.RegionId).DefaultIfEmpty(1).First();
                    }

                    Person.HasRussianNationality = (NationalityId==193)? true :model.PersonInfo.HasRussianNationality;

                    if (bIns)
                        context.PersonContacts.Add(PersonContacts);
                    context.SaveChanges();

                    if (Person.RegistrationStage == 100)
                        UpdateFZ152Log(Person, context);

                    #endregion
                }
                else if (model.Stage == 2)
                {
                    #region Stage2
                    int iPassportType = 1;
                    if (!int.TryParse(model.PassportInfo.PassportType, out iPassportType))
                        iPassportType = 1;
                    int iVisaCountryId;

                    DateTime? dtPassportDate, dtPassportValid;
                    try
                    {
                        dtPassportDate = Convert.ToDateTime(model.PassportInfo.PassportDate,
                            System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                    }
                    catch { dtPassportDate = DateTime.Now; }

                    try
                    {
                        dtPassportValid = Convert.ToDateTime(model.PassportInfo.PassportValid,
                            System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                    }
                    catch { dtPassportValid = null; }

                    if (dtPassportDate.Value.Date > DateTime.Now.Date)
                        dtPassportDate = DateTime.Now.Date;

                    Person.SNILS = model.PersonInfo.SNILS;

                    Person.PassportTypeId = iPassportType;
                    Person.PassportSeries = model.PassportInfo.PassportSeries;
                    Person.PassportNumber = model.PassportInfo.PassportNumber;
                    Person.PassportAuthor = model.PassportInfo.PassportAuthor;
                    Person.PassportDate = dtPassportDate;
                    Person.PassportCode = model.PassportInfo.PassportCode;
                    Person.PassportValid = dtPassportValid;
                    try
                    {
                        int.TryParse(model.VisaInfo.CountryId, out iVisaCountryId);
                        bool bIns = false;
                        var PersonVisaInfo = Person.PersonVisaInfo;
                        if (PersonVisaInfo == null)
                        {
                            PersonVisaInfo = new OnlineAbit2013.EMDX.PersonVisaInfo();
                            PersonVisaInfo.PersonId = PersonId;
                            bIns = true;
                        }
                        if (iVisaCountryId > 0)//not null or something wrong
                        {
                            PersonVisaInfo.CountryId = iVisaCountryId;
                            PersonVisaInfo.PostAddress = model.VisaInfo.PostAddress;
                            PersonVisaInfo.Town = model.VisaInfo.Town;
                        }
                        if (bIns)
                            context.PersonVisaInfo.Add(PersonVisaInfo);
                    }
                    catch { }

                    Person.RegistrationStage = iRegStage < 3 ? 3 : iRegStage;

                    context.SaveChanges();
                    if (Request.Form["SubmitSave"] != null)
                    {
                        return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", model.Stage } });
                    }

                    if (Person.RegistrationStage == 100)
                        UpdateFZ152Log(Person, context);

                    #endregion
                }
                else if (model.Stage == 3)
                {
                    #region Stage3
                    int iCountryId = 0;
                    if (!int.TryParse(model.ContactsInfo.CountryId, out iCountryId))
                        iCountryId = 193;//Russia

                    int iRegionId = 0;
                    if (!int.TryParse(model.ContactsInfo.RegionId, out iRegionId))
                        iRegionId = 0;//unnamed

                    int? altRegionId = context.Country.Where(x => x.Id == iCountryId).Select(x => x.RegionId).FirstOrDefault();
                    if (altRegionId.HasValue && iRegionId == 0)
                    {
                        if (iCountryId != 193)
                            iRegionId = altRegionId.Value;//RegionValue
                        else
                            iRegionId = 3;//Russia
                    }
                    else
                        if (iCountryId != 193)
                            iRegionId = 11;//Далн. зарубеж.


                    int iCountryRealId = 0;
                    if (!int.TryParse(model.ContactsInfo.CountryRealId, out iCountryRealId))
                        iCountryRealId = 193;//Russia

                    int iRegionRealId = 0;
                    if (!int.TryParse(model.ContactsInfo.RegionRealId, out iRegionRealId))
                        iRegionRealId = 0;//unnamed

                    int? altRegionRealId = context.Country.Where(x => x.Id == iCountryRealId).Select(x => x.RegionId).FirstOrDefault();
                    if (altRegionRealId.HasValue && iRegionRealId == 0)
                    {
                        if (iCountryRealId != 193)
                            iRegionRealId = altRegionRealId.Value;//RegionValue
                        else
                            iRegionRealId = 3;//Russia
                    }
                    else
                        if (iCountryRealId != 193)
                            iRegionRealId = 11;//Далн. зарубеж.

                    bool bIns = false;
                    var PersonContacts = Person.PersonContacts;
                    if (PersonContacts == null)
                    {
                        PersonContacts = new PersonContacts();
                        PersonContacts.PersonId = PersonId;
                        bIns = true;
                    }

                    string sCity = model.ContactsInfo.City;
                    string sStreet = model.ContactsInfo.Street;
                    string sHouse = model.ContactsInfo.House;
                    string sRegionKladrCode = Util.GetRegionKladrCodeByRegionId(model.ContactsInfo.RegionId);
                    PersonContacts.KladrCode = Util.GetKladrCodeByAddress(sRegionKladrCode, sCity, sStreet, sHouse);

                    PersonContacts.Phone = model.ContactsInfo.MainPhone;
                    PersonContacts.Mobiles = model.ContactsInfo.SecondPhone;
                    PersonContacts.AddEmail = model.ContactsInfo.AddEmail;

                    PersonContacts.RegionId = iRegionId;
                    PersonContacts.Code = model.ContactsInfo.PostIndex;
                    PersonContacts.City = sCity;
                    PersonContacts.Street = sStreet;
                    PersonContacts.House = sHouse;
                    PersonContacts.Korpus = model.ContactsInfo.Korpus;
                    PersonContacts.Flat = model.ContactsInfo.Flat;
                    PersonContacts.CodeReal = model.ContactsInfo.PostIndexReal;
                    PersonContacts.CityReal = model.ContactsInfo.CityReal;
                    PersonContacts.StreetReal = model.ContactsInfo.StreetReal;
                    PersonContacts.HouseReal = model.ContactsInfo.HouseReal;
                    PersonContacts.KorpusReal = model.ContactsInfo.KorpusReal;
                    PersonContacts.FlatReal = model.ContactsInfo.FlatReal;
                    PersonContacts.RegionRealId = iRegionRealId;
                    PersonContacts.CountryRealId = iCountryRealId;

                    if (bIns)
                        context.PersonContacts.Add(PersonContacts);

                    Person.RegistrationStage = iRegStage < 4 ? 4 : iRegStage;

                    context.SaveChanges();

                    if (Person.RegistrationStage == 100)
                        UpdateFZ152Log(Person, context);

                    #endregion
                }
                else if (model.Stage == 4)//образование
                {
                    #region Stage4
                    //проходим по POST-переменным по очереди
                    for (int i = 0; i < Util.getConstInfo().EducationDocumentsMaxCount; i++)
                    {
                        #region InitVariables
                        //-----------------------------
                        string sId = Request.Form["_sId_" + i];
                        int iId;
                        int.TryParse(sId, out iId);

                        //-----------------------------
                        string sIsEnabled = Request.Form["_isEnabled_" + i];
                        bool bIsEnabled = false;
                        if ("1".Equals(sIsEnabled, StringComparison.OrdinalIgnoreCase))
                            bIsEnabled = true;
                        else
                            bIsEnabled = false;

                        //если помечено к удалению и есть iId, то это синоним удаления
                        if (!bIsEnabled && iId != 0)
                        {
                            context.PersonEducationDocument_delete(PersonId, iId);
                            continue;
                        }
                        //если помечено к удалению и нет iId, то это синоним не открытого листа
                        else if (!bIsEnabled)
                            continue;

                        //-----------------------------
                        string sSchoolTypeId = Request.Form["SchoolTypeId_" + i];
                        int iSchoolTypeId;
                        if (!int.TryParse(sSchoolTypeId, out iSchoolTypeId))
                            iSchoolTypeId = 1;

                        //-----------------------------
                        string SchoolName = Request.Form["SchoolName_" + i];
                        if (string.IsNullOrEmpty(SchoolName))
                            continue;

                        //-----------------------------
                        string sSchoolExitYear = Request.Form["SchoolExitYear_" + i];
                        if (string.IsNullOrEmpty(sSchoolExitYear))
                            continue;

                        int SchoolExitYear;
                        if (!int.TryParse(sSchoolExitYear, out SchoolExitYear))
                            SchoolExitYear = DateTime.Now.Year;

                        //-----------------------------
                        string sCountryEducId = Request.Form["CountryEducId_" + i];
                        int iCountryEducId;
                        if (!int.TryParse(sCountryEducId, out iCountryEducId))
                            iCountryEducId = 1;

                        //-----------------------------
                        string sRegionEducId = Request.Form["RegionEducId_" + i];
                        int iRegionEducId;
                        if (!int.TryParse(sRegionEducId, out iRegionEducId))
                            iRegionEducId = 1;

                        //-----------------------------
                        string sVuzAdditionalTypeId = Request.Form["VuzAdditionalTypeId_" + i];
                        int iVuzAddTypeId;
                        if (!int.TryParse(sVuzAdditionalTypeId, out iVuzAddTypeId))
                            iVuzAddTypeId = 1;

                        //-----------------------------
                        string sSchoolExitClassId = Request.Form["SchoolExitClassId_" + i];
                        int iSchoolExitClassId;
                        if (!int.TryParse(sSchoolExitClassId, out iSchoolExitClassId))
                            iSchoolExitClassId = 1;

                        //-----------------------------
                        string sAvgMark = Request.Form["AvgMark_" + i];
                        double avgBall;
                        double.TryParse(sAvgMark, out avgBall);

                        //----------------------------
                        string sPersonStudyForm = Request.Form["PersonStudyForm_" + i];
                        int iPersonStudyForm = 0;
                        int.TryParse(sPersonStudyForm, out iPersonStudyForm);

                        //----------------------------
                        string sPersonQualification = Request.Form["PersonQualification_" + i];
                        int iPersonQualification = 0;
                        int.TryParse(sPersonQualification, out iPersonQualification);

                        //----------------------------
                        
                        string SchoolNumber = Request.Form["SchoolNumber_" + i];
                        string SchoolCity = Request.Form["SchoolCity_" + i];

                        //----------------------------
                        string sIsExcellent = Request.Form["IsExcellent_" + i].Split(',').First();
                        string sIsEqual = Request.Form["IsEqual_" + i].Split(',').First();

                        bool bIsExcellent = false;
                        bool bIsEqual = false;

                        bool.TryParse(sIsExcellent, out bIsExcellent);
                        bool.TryParse(sIsEqual, out bIsEqual);
                        //----------------------------

                        string EqualityDocumentNumber = Request.Form["EqualityDocumentNumber_" + i];
                        string Series = Request.Form["Series_" + i];
                        string Number = Request.Form["Number_" + i];
                        string sHEEntryYear = Request.Form["HEEntryYear_" + i];
                        string DiplomTheme = Request.Form["DiplomTheme_" + i];
                        string ProgramName = Request.Form["ProgramName_" + i];

                        #endregion
                        //-----------------PersonEducationDocument---------------------
                        #region PersonEducationDocument
                        bool bIns = false;
                        if (context.PersonEducationDocument.Where(x => x.Id == iId && x.PersonId == PersonId).Count() == 0)
                            bIns = true;
                        
                        if (iCountryEducId != 193)
                            iRegionEducId = context.Country.Where(x => x.Id == iCountryEducId).Select(x => x.RegionId).FirstOrDefault();

                        if (bIns)
                        {
                            ObjectParameter idParam = new ObjectParameter("id", typeof(int));
                            context.PersonEducationDocument_insert(PersonId, iSchoolTypeId, iCountryEducId, iRegionEducId, iSchoolTypeId == 4 ? (int?)iVuzAddTypeId : null,
                                SchoolCity, SchoolName, SchoolNumber, SchoolExitYear.ToString(),
                                iSchoolTypeId == 1 ? (int?)iSchoolExitClassId : null, Series, Number, bIsEqual, EqualityDocumentNumber, avgBall, bIsExcellent, idParam);
                            bIns = false;
                            iId = (int)idParam.Value;
                        }
                        else
                        {
                            context.PersonEducationDocument_update(PersonId, iSchoolTypeId, iCountryEducId, iRegionEducId, iSchoolTypeId == 4 ? (int?)iVuzAddTypeId : null,
                                SchoolCity, SchoolName, SchoolNumber, SchoolExitYear.ToString(),
                                iSchoolTypeId == 1 ? (int?)iSchoolExitClassId : null, Series, Number, bIsEqual, EqualityDocumentNumber, avgBall, bIsExcellent, iId);
                        }
                        #endregion
                        //-----------------PersonHighEducationInfo---------------------
                        #region PersonHighEducationInfo
                        if (iSchoolTypeId == 4)
                        {
                            var PersonHighEducationInfo = context.PersonHighEducationInfo.Where(x => x.EducationDocumentId == iId).FirstOrDefault();
                            if (PersonHighEducationInfo == null)
                            {
                                PersonHighEducationInfo = new PersonHighEducationInfo();
                                PersonHighEducationInfo.EducationDocumentId = iId;
                                bIns = true;
                            }

                            int iHEEntryYear;
                            int.TryParse(sHEEntryYear, out iHEEntryYear);
                            if (!string.IsNullOrEmpty(DiplomTheme) && DiplomTheme.Length > 4000)
                                DiplomTheme = DiplomTheme.Substring(0, 4000);
                            PersonHighEducationInfo.DiplomaTheme = DiplomTheme;
                            if (!string.IsNullOrEmpty(ProgramName) && ProgramName.Length > 1000)
                                ProgramName = ProgramName.Substring(0, 1000);

                            PersonHighEducationInfo.ProgramName = ProgramName;
                            PersonHighEducationInfo.EntryYear = (iHEEntryYear == 0 ? null : (int?)iHEEntryYear);
                            if (iPersonStudyForm != 0)
                                PersonHighEducationInfo.StudyFormId = iPersonStudyForm;
                            if (iPersonQualification != 0)
                                PersonHighEducationInfo.QualificationId = iPersonQualification;

                            if (iSchoolTypeId == 4)
                            {
                                int iEntryYear;
                                int.TryParse(sHEEntryYear, out iEntryYear);
                                PersonHighEducationInfo.EntryYear = iEntryYear != 0 ? (int?)iEntryYear : null;
                                PersonHighEducationInfo.ExitYear = SchoolExitYear != 0 ? SchoolExitYear : DateTime.Now.Year;
                            }

                            if (bIns)
                            {
                                context.PersonHighEducationInfo.Add(PersonHighEducationInfo);
                                bIns = false;
                            }
                        }
                        #endregion
                    }
                    //--------------------------------------
                    Person.RegistrationStage = iRegStage < 5 ? 5 : iRegStage;
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        //return View(ex.ToString());
                    }
                    #endregion
                }
                else if (model.Stage == 5)
                {
                    #region Stage5
                    bool bIns = false;
                    var PersonAddInfo = Person.PersonAddInfo;
                    if (PersonAddInfo == null)
                    {
                        PersonAddInfo = new PersonAddInfo();
                        PersonAddInfo.PersonId = PersonId;
                        bIns = true;
                    }

                    int iLanguageId;
                    if (!int.TryParse(model.AddEducationInfo.LanguageId, out iLanguageId))
                        iLanguageId = 1;

                    double EnglishMark;
                    if (!double.TryParse(model.AddEducationInfo.EnglishMark, out EnglishMark))
                        EnglishMark = 0.0;

                    PersonAddInfo.LanguageId = iLanguageId;
                    PersonAddInfo.EnglishMark = EnglishMark;
                    PersonAddInfo.StartEnglish = model.AddEducationInfo.StartEnglish;
                    PersonAddInfo.HasTRKI = model.AddEducationInfo.HasTRKI;
                    PersonAddInfo.TRKICertificateNumber = model.AddEducationInfo.TRKICertificateNumber;
                    
                    if (bIns)
                        context.PersonAddInfo.Add(PersonAddInfo);
                    #region PersonDisorderInfo
                    bool bHasDisorder = Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && (x.VuzAdditionalTypeId == 3)).Count() > 0;
                    if (bHasDisorder && model.DisorderInfo != null)
                    {
                        bIns = false;
                        PersonDisorderInfo PersonDisorderEducation = Person.PersonDisorderInfo;
                        if (PersonDisorderEducation == null)
                        {
                            PersonDisorderEducation = new PersonDisorderInfo();
                            PersonDisorderEducation.PersonId = PersonId;
                            bIns = true;
                        }
                        PersonDisorderEducation.IsForIGA = model.DisorderInfo.IsForIGA;
                        PersonDisorderEducation.YearOfDisorder = model.DisorderInfo.YearOfDisorder;
                        PersonDisorderEducation.EducationProgramName = model.DisorderInfo.EducationProgramName;
                        if (bIns)
                            context.PersonDisorderInfo.Add(PersonDisorderEducation);
                    }
                    #endregion
                    bool bHasCurrentEducation = Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && (x.VuzAdditionalTypeId == 2 || x.VuzAdditionalTypeId == 4)).Count() > 0;
                    //-----------------PersonCurrentEducation---------------------
                    #region PersonCurrentEducation
                    if (bHasCurrentEducation)
                    {
                        bIns = false;
                        PersonCurrentEducation PersonCurrentEducation = Person.PersonCurrentEducation;
                        int iEducTypeId = 1;
                        if (!int.TryParse(model.CurrentEducation.EducationTypeId, out iEducTypeId))
                            iEducTypeId = 1;//default value
                        int iSemesterId = 1;
                        if (!int.TryParse(model.CurrentEducation.SemesterId, out iSemesterId))
                            iSemesterId = 1;//default value
                        int iStudyLevelId = 16;
                        if (!int.TryParse(model.CurrentEducation.StudyLevelId, out iStudyLevelId))
                            iStudyLevelId = 16;//default value
                        DateTime? dtAccreditation;
                        DateTime tmp;
                        if (!DateTime.TryParse(model.CurrentEducation.AccreditationDate, out tmp))
                            dtAccreditation = null;
                        else
                            dtAccreditation = tmp;
                        int iLicenseProgramId = 1;
                        if (!int.TryParse(model.CurrentEducation.HiddenLicenseProgramId, out iLicenseProgramId))
                            iLicenseProgramId = 1;//default value
                        int? iObrazProgramId;
                        int _iObrazProgramId = 1;
                        if (!int.TryParse(model.CurrentEducation.HiddenObrazProgramId, out _iObrazProgramId))
                            iObrazProgramId = null;//default value
                        else
                            iObrazProgramId = _iObrazProgramId;

                        if (PersonCurrentEducation == null)
                        {
                            PersonCurrentEducation = new PersonCurrentEducation();
                            PersonCurrentEducation.PersonId = PersonId;
                            bIns = true;
                        }
                        PersonCurrentEducation.EducTypeId = iEducTypeId;
                        PersonCurrentEducation.SemesterId = iSemesterId;
                        PersonCurrentEducation.AccreditationDate = dtAccreditation;
                        PersonCurrentEducation.AccreditationNumber = model.CurrentEducation.AccreditationNumber;

                        PersonCurrentEducation.EducName = model.CurrentEducation.EducationName;
                        PersonCurrentEducation.HasAccreditation = model.CurrentEducation.HasAccreditation;
                        PersonCurrentEducation.HasScholarship = model.CurrentEducation.HasScholarship;
                        PersonCurrentEducation.StudyLevelId = iStudyLevelId;
                        PersonCurrentEducation.StudyFormId = model.CurrentEducation.StudyFormId;
                        PersonCurrentEducation.StudyBasisId = model.CurrentEducation.StudyBasisId;

                        PersonCurrentEducation.LicenseProgramId = iLicenseProgramId;
                        PersonCurrentEducation.ObrazProgramId = iObrazProgramId;

                        bool bHas = Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && (x.VuzAdditionalTypeId == 2 || x.VuzAdditionalTypeId == 3)).Count() > 0;
                        int iCountryEducId = Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4).Select(x => x.CountryEducId).DefaultIfEmpty(193).First();

                        PersonCurrentEducation.CountryId = bHas ? 193 : iCountryEducId;
                        PersonCurrentEducation.ProfileName = model.CurrentEducation.ProfileName;

                        if (bIns)
                        {
                            context.PersonCurrentEducation.Add(PersonCurrentEducation);
                            bIns = false;
                        }

                        bool bHasChangeStudyForm = Person.PersonEducationDocument.Where(x => x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2).Count() > 0;
                        if (bHasChangeStudyForm)
                        {
                            bIns = false;
                            PersonChangeStudyFormReason ChangeStudyFormReason = Person.PersonChangeStudyFormReason;
                            if (ChangeStudyFormReason == null)
                            {
                                ChangeStudyFormReason = new PersonChangeStudyFormReason();
                                ChangeStudyFormReason.PersonId = PersonId;
                                bIns = true;
                            }

                            ChangeStudyFormReason.Reason = model.ChangeStudyFormReason.Reason;
                            if (bIns)
                            {
                                context.PersonChangeStudyFormReason.Add(ChangeStudyFormReason);
                                bIns = false;
                            }
                        }
                    }
                    #endregion

                    #region ChangeStudyFormReason
                    if (model.AddEducationInfo.HasReason)
                    {
                        bIns = false;
                        PersonChangeStudyFormReason ChangeStudyFormReason = Person.PersonChangeStudyFormReason;
                        if (ChangeStudyFormReason == null)
                        {
                            ChangeStudyFormReason = new PersonChangeStudyFormReason();
                            ChangeStudyFormReason.PersonId = PersonId;
                        }
                        ChangeStudyFormReason.Reason = model.ChangeStudyFormReason.Reason;

                        if (bIns)
                            context.PersonChangeStudyFormReason.Add(ChangeStudyFormReason);
                    }
                    #endregion

                    if (iRegStage < 6)
                        Person.RegistrationStage = 6;

                    context.SaveChanges();

                    #endregion
                }
                else if (model.Stage == 6)
                {
                    #region Stage6
                    bool bIns = false;
                    var PersonSportQualification = Person.PersonSportQualification;
                    if (PersonSportQualification == null)
                    {
                        PersonSportQualification = new PersonSportQualification();
                        bIns = true;
                        PersonSportQualification.PersonId = PersonId;
                    }

                    int iSportQualificationId = 0;
                    int.TryParse(model.PrivelegeInfo.SportQualificationId, out iSportQualificationId);

                    PersonSportQualification.OtherSportQualification = model.PrivelegeInfo.OtherSportQualification;
                    PersonSportQualification.SportQualificationId = iSportQualificationId;
                    PersonSportQualification.SportQualificationLevel = model.PrivelegeInfo.SportQualificationLevel;

                    if (bIns)
                        context.PersonSportQualification.Add(PersonSportQualification);

                    if (iRegStage < 7)
                        Person.RegistrationStage = 7;

                    context.SaveChanges();

                    #endregion
                }
                else if (model.Stage == 7)
                {
                    #region Stage7
                    if (!model.AddInfo.FZ_152Agree)
                    {
                        ModelState.AddModelError("AddInfo_FZ_152Agree", "Вы должны принять условия");
                        return View("PersonalOffice", model);
                    }
                    bool bIns = false;
                    var PersonAddInfo = Person.PersonAddInfo;
                    if (PersonAddInfo == null)
                    {
                        PersonAddInfo = new PersonAddInfo();
                        PersonAddInfo.PersonId = PersonId;
                        bIns = true;
                    }

                    int iReturnDocumentTypeId;
                    if (!int.TryParse(model.AddInfo.ReturnDocumentTypeId, out iReturnDocumentTypeId))
                        iReturnDocumentTypeId = 1;

                    PersonAddInfo.AddInfo = model.AddInfo.ExtraInfo;
                    PersonAddInfo.Parents = model.AddInfo.ContactPerson;
                    PersonAddInfo.HasPrivileges = model.AddInfo.HasPrivileges;
                    PersonAddInfo.NeedSpecialConditions = model.AddInfo.NeedSpecialConditions;
                    PersonAddInfo.HostelAbit = model.AddInfo.HostelAbit;
                    PersonAddInfo.HostelEduc = model.AddInfo.HostelEduc;
                    PersonAddInfo.ReturnDocumentTypeId = iReturnDocumentTypeId;
                    PersonAddInfo.Parent_Surname = model.AddInfo.Parent_Surname;
                    PersonAddInfo.Parent_Name = model.AddInfo.Parent_Name;
                    PersonAddInfo.Parent_SecondName = model.AddInfo.Parent_SecondName;
                    PersonAddInfo.Parent_Phone = model.AddInfo.Parent_Phone;
                    PersonAddInfo.Parent_Email = model.AddInfo.Parent_Email;
                    PersonAddInfo.Parent_Work = model.AddInfo.Parent_Work;
                    PersonAddInfo.Parent_WorkPosition = model.AddInfo.Parent_WorkPosition;
                    PersonAddInfo.Parent2_Surname = model.AddInfo.Parent2_Surname;
                    PersonAddInfo.Parent2_Name = model.AddInfo.Parent2_Name;
                    PersonAddInfo.Parent2_SecondName = model.AddInfo.Parent2_SecondName;
                    PersonAddInfo.Parent2_Phone = model.AddInfo.Parent2_Phone;
                    PersonAddInfo.Parent2_Email = model.AddInfo.Parent2_Email;
                    PersonAddInfo.Parent2_Work = model.AddInfo.Parent2_Work;
                    PersonAddInfo.Parent2_WorkPosition = model.AddInfo.Parent2_WorkPosition;

                    if (Person.RegistrationStage <= 7)
                        Person.RegistrationStage = 100;

                    UpdateFZ152Log(Person, context);

                    if (bIns)
                        context.PersonAddInfo.Add(PersonAddInfo);

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                }

                if (model.Stage < 7)
                {
                    model.Stage++;
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", model.Stage } });
                }
                else
                    return RedirectToAction("Main", "Abiturient");
            }
        }

        private void UpdateFZ152Log(Person Person, OnlinePriemEntities context)
        {
            FZ_152_AgreeLog logEntry = new FZ_152_AgreeLog();
            //Личные сведения
            logEntry.PersonId = Person.Id;
            logEntry.Surname = Person.Surname;
            logEntry.Name = Person.Name;
            logEntry.SecondName = Person.SecondName ?? "";
            logEntry.BirthDate = Person.BirthDate ?? DateTime.Now;
            logEntry.BirthPlace = Person.BirthPlace;
            logEntry.SurnameEng = Person.SurnameEng;
            logEntry.NameEng = Person.NameEng;
            logEntry.SecondNameEng = Person.SecondNameEng;

            //Документы
            logEntry.PassportSeries = Person.PassportSeries;
            logEntry.PassportNumber = Person.PassportNumber;
            logEntry.PassportDate = Person.PassportDate;
            logEntry.PassportAuthor = Person.PassportAuthor;
            logEntry.SNILS = Person.SNILS;

            //Контактные данные
            logEntry.Phone = Person.PersonContacts.Phone;
            logEntry.Mobiles = Person.PersonContacts.Mobiles;
            logEntry.Code = Person.PersonContacts.Code;
            logEntry.City = Person.PersonContacts.City;
            logEntry.Street = Person.PersonContacts.Street;
            logEntry.House = Person.PersonContacts.House;
            logEntry.Korpus = Person.PersonContacts.Korpus;
            logEntry.Flat = Person.PersonContacts.Flat;

            logEntry.CodeReal = Person.PersonContacts.CodeReal;
            logEntry.CityReal = Person.PersonContacts.CityReal;
            logEntry.StreetReal = Person.PersonContacts.StreetReal;
            logEntry.HouseReal = Person.PersonContacts.HouseReal;
            logEntry.KorpusReal = Person.PersonContacts.KorpusReal;
            logEntry.FlatReal = Person.PersonContacts.FlatReal;
            
            //Прочие личные данные
            logEntry.Parents = Person.PersonAddInfo.Parents;
            logEntry.AddInfo = Person.PersonAddInfo.AddInfo;
            logEntry.Parent_Surname = Person.PersonAddInfo.Parent_Surname;
            logEntry.Parent_Name = Person.PersonAddInfo.Parent_Name;
            logEntry.Parent_SecondName = Person.PersonAddInfo.Parent_SecondName;
            logEntry.Parent_Phone = Person.PersonAddInfo.Parent_Phone;
            logEntry.Parent_Email = Person.PersonAddInfo.Parent_Email;
            logEntry.Parent_Work = Person.PersonAddInfo.Parent_Work;
            logEntry.Parent_WorkPosition = Person.PersonAddInfo.Parent_WorkPosition;

            //Служебные поля
            logEntry.TimeStamp = DateTime.Now;
            logEntry.Request_Agent = Request.UserAgent;
            logEntry.Request_IP = Request.UserHostAddress;
            logEntry.Request_Path = Request.Path;
            
            context.FZ_152_AgreeLog.Add(logEntry);
            context.SaveChanges();
        }

        public ActionResult Main()
        {
            if (Request.Url.AbsoluteUri.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == -1 && Util.bUseRedirection &&
                Request.Url.AbsoluteUri.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) == -1)
                return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));

            //Validation
            Guid PersonID;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonID))
                return RedirectToAction("LogOn", "Account");

            if (Util.CheckIsNew(PersonID))
            {
                if (Util.CreateNew(PersonID))
                    return RedirectToAction("Index");
            }

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonID).FirstOrDefault();
                if (PersonInfo == null)
                    return RedirectToAction("Index");

                int regStage = PersonInfo.RegistrationStage;
                if (regStage < 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", regStage.ToString() } });

                bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

                SimplePerson model = new SimplePerson();
                model.Applications = new List<SimpleApplicationPackage>();
                model.Files = new List<AppendedFile>();

                string query = "SELECT Surname, Name, SecondName, RegistrationStage FROM PERSON WHERE Id=@Id";
                SortedList<string, object> dic = new SortedList<string, object>() { { "@Id", PersonID } };
                DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
                if (tbl.Rows.Count != 1)
                    return RedirectToAction("Index");

                model.Name = Server.HtmlEncode(PersonInfo.Name);
                model.Surname = Server.HtmlEncode(PersonInfo.Surname);
                model.SecondName = Server.HtmlEncode(PersonInfo.SecondName);

                var Applications = context.Abiturient.Where(x => x.PersonId == PersonID && x.Enabled == true && x.IsCommited == true && x.CampaignYear == Util.iPriemYear)
                    .Select(x => new { x.CommitId, x.StudyLevelGroupNameRus, x.StudyLevelGroupNameEng, x.EntryType, x.SecondTypeId, x.IsApprovedByComission }).Distinct();
                var SecondTypeList = context.ApplicationSecondType.Select(x => new { x.Id, Name = bIsEng ? x.NameEng : x.Name });

                foreach (var app in Applications)
                {
                    model.Applications.Add(new SimpleApplicationPackage()
                    {
                        Id = app.CommitId,
                        isApproved = app.IsApprovedByComission,
                        StudyLevel = bIsEng ? app.StudyLevelGroupNameEng : app.StudyLevelGroupNameRus +
                                     SecondTypeList.Where(x => x.Id == app.SecondTypeId).Select(x => x.Name).FirstOrDefault(),

                    });
                }

                string temp_str;
                if (bIsEng)
                    temp_str = "To submit an application select the link <a href=\"" + Util.ServerAddress + "/Abiturient/NewApplication\">\"New application\"</a>";
                else
                    temp_str = "Для подачи заявления нажмите кнопку <a href=\"" + Util.ServerAddress + "/Abiturient/NewApplication\">\"Подать новое заявление\"</a>";
                model.Messages = Util.GetNewPersonalMessages(PersonID);
                if (model.Applications.Count == 0)
                {
                    model.Messages.Add(new PersonalMessage() { Id = "0", Type = MessageType.TipMessage, Text = temp_str });
                }

                //model.Messages.Add(new PersonalMessage() { Id = "0", Type = MessageType.TipMessage, Text = "Уважаемые абитуриенты! Не забудьте проверить свой выбор \"Нуждаюсь в общежитии на время обучения\" на <a href=\"https://cabinet.spbu.ru/Abiturient?step=6\">последней странице анкеты</a>" });
                return View("Main", model);
            }
        }

        public ActionResult page404(params string[] errors)
        {
            return View("page404");
        }

        #region NewApplication

        #region NewApplication - MENU By Types 
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_AG(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                if (PersonInfo.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                var lst = Util.GetEnableLevelList(new List<int>() { 6, 7 }, PersonInfo);

                if (lst.Count == 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = "Невозможно подать заявление в Академическую Гимназию (не соответствует уровень образования)";
                }
                else
                {
                    model.Enabled = true;
                    model.MaxBlocks = lst.First().MaxBlocks;
                }
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();

                return View("NewApplication_AG", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_Mag(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                if (PersonInfo.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");
 
                model.Enabled = true;

                var lst = Util.GetEnableLevelList(new List<int>(){2}, PersonInfo);
                if (lst.Count == 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать заявление в магистратуру (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data";
                }
                else
                {
                    model.MaxBlocks = lst.First().MaxBlocks;
                    int VuzAddType = PersonInfo.PersonEducationDocument.OrderByDescending(x => x.VuzAdditionalTypeId).Select(x => x.VuzAdditionalTypeId).FirstOrDefault() ?? 1;
                    if ((int)VuzAddType != 1)
                    {
                        model.Enabled = false;
                        model.HasError = true;
                        model.ErrorMessage = (!bIsEng) ?
                           "Невозможно подать заявление в магистратуру (смените тип поступления в Анкете)" :
                           "Change your Entry Type in Questionnaire Data";
                    }
                }
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                return View("NewApplication_Mag", model);
            }
        }
 
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_1kurs(params string[] errors)
         {
             if (errors != null && errors.Length > 0)
             {
                 foreach (string er in errors)
                     ModelState.AddModelError("", er);
             }
             Guid PersonId;
             if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                 return RedirectToAction("LogOn", "Account");
             bool bIsEng = Util.GetCurrentThreadLanguageIsEng();
             using (OnlinePriemEntities context = new OnlinePriemEntities())
             {
                 var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                 if (PersonInfo == null)//а что это могло бы значить???
                     return RedirectToAction("Index");

                 if (PersonInfo.RegistrationStage != 100)
                     return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                 if (PersonInfo.PersonEducationDocument == null)
                     return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                 Mag_ApplicationModel model = new Mag_ApplicationModel();
                 model.Applications = new List<Mag_ApplicationSipleEntity>();
                 model.CommitId = Guid.NewGuid().ToString("N");
                 bool isForeign = Util.GetRess(PersonId) == 4;
                 model.Enabled = true;

                 var lst = Util.GetEnableLevelList(new List<int>() { 1 }, PersonInfo, isForeign);
                 if (lst.Count == 0)
                 {
                     model.Enabled = false;
                     model.HasError = true;
                     model.ErrorMessage = (!bIsEng) ?
                         "Невозможно подать заявление на первый курс (не соответствует уровень образования)" :
                         "Change your previous education degree in Questionnaire Data";
                 }
                 else
                 {
                     model.MaxBlocks = lst.First().MaxBlocks; 
                     int VuzAddType = PersonInfo.PersonEducationDocument.OrderByDescending(x => x.VuzAdditionalTypeId).Select(x => x.VuzAdditionalTypeId).FirstOrDefault() ?? 1;
                     if ((int)VuzAddType != 1)
                     {
                         model.Enabled = false;
                         model.HasError = true;
                         model.ErrorMessage = (!bIsEng) ? 
                            "Невозможно подать заявление на первый курс (смените тип поступления в Анкете)":
                            "Change your Entry Type in Questionnaire Data";
                     }
                 }
                 model.StudyFormList = Util.GetStudyFormList();
                 model.StudyBasisList = Util.GetStudyBasisList();
                 return View("NewApplication_1kurs", model);
             }
         }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_SPO(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });
                
                if (PersonInfo.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");
                model.Enabled = true;
                var lst = Util.GetEnableLevelList(new List<int>() { 3 }, PersonInfo);
                if (lst.Count() == 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Подача заявления в СПО доступна только для людей, уже закончивших 9 классов школы":
                        "Change your previous education degree in Questionnaire Data";
                }
                else
                {
                    model.MaxBlocks = lst.First().MaxBlocks;
                    int VuzAddType = PersonInfo.PersonEducationDocument.OrderByDescending(x => x.VuzAdditionalTypeId).Select(x => x.VuzAdditionalTypeId).FirstOrDefault() ?? 1;
                    if ((int)VuzAddType != 1)
                    {
                        model.Enabled = false;
                        model.HasError = true;
                        model.ErrorMessage = (!bIsEng) ?
                            "Невозможно подать заявление в СПО (смените тип поступления в Анкете)" :
                        "Change your Entry Type in Questionnaire Data";
                    }
                }

                string query = "SELECT DISTINCT StudyFormId, StudyFormName FROM Entry WHERE StudyLevelGroupId = 3 ORDER BY 1";
                DataTable tbl = Util.AbitDB.GetDataTable(query, null);
                model.StudyFormList =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         Value = rw.Field<int>("StudyFormId"),
                         Text = rw.Field<string>("StudyFormName")
                     }).AsEnumerable()
                    .Select(x => new SelectListItem() { Text = x.Text, Value = x.Value.ToString() })
                    .ToList();

                query = "SELECT DISTINCT StudyBasisId, StudyBasisName FROM Entry WHERE StudyLevelGroupId = 3 ORDER BY 1";
                tbl = Util.AbitDB.GetDataTable(query, null);
                model.StudyBasisList =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         Value = rw.Field<int>("StudyBasisId"),
                         Text = rw.Field<string>("StudyBasisName")
                     }).AsEnumerable()
                     .Select(x => new SelectListItem() { Text = x.Text, Value = x.Value.ToString() })
                     .ToList();
                return View("NewApplication_SPO", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_Aspirant(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                if (PersonInfo.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });
                
                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                model.Enabled = true;
                var lst = Util.GetEnableLevelList(new List<int>() { 4 }, PersonInfo);
                if (lst.Count == 0)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в аспирантуру (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data";
                }
                else
                {
                    model.MaxBlocks = lst.First().MaxBlocks;
                    int? VuzAddType = PersonInfo.PersonEducationDocument.OrderByDescending(x => x.VuzAdditionalTypeId).Select(x => x.VuzAdditionalTypeId).FirstOrDefault() ?? 1;
                    if ((int)VuzAddType != 1)
                    {
                        model.Enabled = false;
                        model.HasError = true;
                        model.ErrorMessage = (!bIsEng) ?
                            "Невозможно подать заявление в аспирантуру (смените тип поступления в Анкете)" :
                            "Change your Entry Type in Questionnaire Data";
                    }
                }
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                  
                return View("NewApplication_Aspirant", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_Ord(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                if (PersonInfo.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                model.Enabled = true;
                var lst = Util.GetEnableLevelList(new List<int>() { 5 }, PersonInfo);
                if (lst.Count == 0)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в ординатуру (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data";
                }
                else
                {
                    model.MaxBlocks = lst.First().MaxBlocks;
                    int? VuzAddType = PersonInfo.PersonEducationDocument.OrderByDescending(x => x.VuzAdditionalTypeId).Select(x => x.VuzAdditionalTypeId).FirstOrDefault() ?? 1;
                    if ((int)VuzAddType != 1)
                    {
                        model.Enabled = false;
                        model.HasError = true;
                        model.ErrorMessage = (!bIsEng)? 
                            "Невозможно подать заявление в ординатуру (смените тип поступления в Анкете)":
                            "Change your Entry Type in Questionnaire Data";
                    }
                }
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                return View("NewApplication_Ord", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_Recover(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                var PersonDisOrderInfo = context.PersonDisorderInfo.Where(x => x.PersonId == PersonId).FirstOrDefault();
                if (PersonDisOrderInfo == null)
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", 5 } });

                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 3
                                     select x).FirstOrDefault();
                if (PersonEducDoc != null)
                {
                    model.MaxBlocks = maxBlockRecover;
                    model.Enabled = true;
                }
                else
                    model.Enabled = false;

                model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                model.SemestrList = Util.GetSemesterList();

                return View("NewApplication_Recover", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_Transfer(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });


                var PersonCurEducInfo = context.PersonCurrentEducation.Where(x => x.PersonId == PersonId).FirstOrDefault();
                if (PersonCurEducInfo == null)
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", 5 } });


                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 4
                                     select x).FirstOrDefault();
                if (PersonEducDoc != null)
                {
                    model.MaxBlocks = maxBlockTransfer;
                    model.Enabled = true;
                }
                else
                    model.Enabled = false;
               
                model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                model.SemestrList = Util.GetSemesterList();
                return View("NewApplication_Transfer", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_ChangeStudyForm(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                var PersonCurEducInfo = context.PersonCurrentEducation.Where(x => x.PersonId == PersonId).FirstOrDefault();
                if (PersonCurEducInfo == null)
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", 5 } });
                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                     select x).FirstOrDefault();
                if (PersonEducDoc != null)
                {
                    model.MaxBlocks = maxBlockChangeStudyFormBasis;
                    model.Enabled = true;
                }
                else
                    model.Enabled = false;

                model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                model.SemestrList = Util.GetSemesterList();

                return View("NewApplication_ChangeStudyForm", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_ChangeObrazProgram(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                var PersonCurEducInfo = context.PersonCurrentEducation.Where(x => x.PersonId == PersonId).FirstOrDefault();
                if (PersonCurEducInfo == null)
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", 5 } });
                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                model.CommitId = Guid.NewGuid().ToString("N");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                     select x).FirstOrDefault();
                if (PersonEducDoc != null)
                {
                    model.MaxBlocks = maxBlockTransfer;
                    model.Enabled = true;
                }
                else
                    model.Enabled = false;

                model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                model.StudyFormList = Util.GetStudyFormList();
                model.StudyBasisList = Util.GetStudyBasisList();
                model.SemestrList = Util.GetSemesterList();
                model.SemestrId = (int?)Util.AbitDB.GetValue("SELECT S.NextSemesterId FROM PersonCurrentEducation P INNER JOIN Semester S ON S.Id = P.SemesterId WHERE PersonId=@PersonId", new SortedList<string, object>() { { "@PersonId", PersonId } }) ?? 3;
                for (int i = 0; i < model.SemestrList.Count ; i++)
                {
                    if (model.SemestrList[i].Value == model.SemestrId.ToString())
                        model.SemestrList[i].Selected = true;
                }
                return View("NewApplication_ChangeObrazProgram", model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult NewApplication_ChangeStudyBasis(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                var PersonCurEducInfo = context.PersonCurrentEducation.Where(x => x.PersonId == PersonId).FirstOrDefault();
                if (PersonCurEducInfo == null)
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", 5 } });

                Mag_ApplicationModel model = new Mag_ApplicationModel();
                model.Applications = new List<Mag_ApplicationSipleEntity>();
                Guid gComm = Guid.NewGuid();
                model.CommitId = gComm.ToString("N");

                bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                     select x).FirstOrDefault();
                if (PersonEducDoc != null)
                {
                    model.MaxBlocks = maxBlockChangeStudyFormBasis;
                    model.Enabled = true;
                }
                else
                    model.Enabled = false;

                if (context.Application.Where(x => x.PersonId == PersonId && x.IsCommited == true && x.SecondTypeId == 5 && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    return RedirectToAction("Main", "Abiturient");
                }

                var PersonCurrentEduc = context.PersonCurrentEducation.Where(x => x.PersonId == PersonId).FirstOrDefault();

                if (PersonCurrentEduc == null)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", "4" } });
                if (!PersonCurrentEduc.ObrazProgramId.HasValue)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", "4" } });

                int ActualSemesterId = Util.GetActualSemester(PersonCurrentEduc.SemesterId);

                var EntryList =
                    (from Ent in context.Entry
                     join SPStudyLevel in context.SP_StudyLevel on Ent.StudyLevelId equals SPStudyLevel.Id
                     where  PersonCurrentEduc.LicenseProgramId == Ent.LicenseProgramId &&
                            Ent.ObrazProgramId == PersonCurrentEduc.ObrazProgramId &&
                            Ent.StudyFormId == PersonCurrentEduc.StudyFormId &&
                            Ent.StudyBasisId == 1 &&
                            Ent.CampaignYear == Util.iPriemYear &&
                            Ent.StudyLevelId == PersonCurrentEduc.StudyLevelId &&
                            Ent.IsParallel == false &&
                            Ent.IsReduced == false &&
                            Ent.IsSecond == false &&
                            Ent.SemesterId == ActualSemesterId &&
                            Ent.IsUsedForPriem
                     select new
                     {
                         EntryId = Ent.Id,
                         EntryTypeId = Ent.StudyLevelGroupId,
                         DateOfStart = Ent.DateOfStart,
                         DateOfClose = Ent.DateOfClose
                     }).FirstOrDefault();

                if (EntryList == null)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", "4" } });

                if (EntryList.DateOfClose.HasValue && EntryList.DateOfClose < DateTime.Now)
                    return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Приём заявлений уже закрыт" } });
                if (EntryList.DateOfStart.HasValue && EntryList.DateOfStart > DateTime.Now)
                    return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Приём заявлений ещё не начался" } });

                Guid appId = Guid.NewGuid();
                context.Application.Add(new Application()
                {
                    Id = appId,
                    PersonId = PersonId,
                    EntryId = EntryList.EntryId,
                    HostelEduc = false,
                    Priority = 1,
                    Enabled = true,
                    EntryType = EntryList.EntryTypeId,
                    DateOfStart = DateTime.Now,
                    CommitId = gComm,
                    IsGosLine = false,
                    IsCommited = true,
                    SecondTypeId = 5
                });
                context.SaveChanges(); 
                return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", gComm } });
            }
        }
        #endregion

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ChangeApplication(string Id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            bool NewId = false;
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)
                    return RedirectToAction("Index");

                if (PersonInfo.PersonEducationDocument == null)
                    return RedirectToAction("Index");

                Guid gComm = Guid.Parse(Id);
                bool isPrinted = (bool)Util.AbitDB.GetValue("SELECT IsPrinted FROM ApplicationCommit WHERE Id=@Id ", new SortedList<string, object>() { { "@Id", gComm } });
                if (isPrinted)
                {
                    int NotEnabledApplication = (int)Util.AbitDB.GetValue(@"select count (Application.Id) from Application
                                         inner join Entry on Entry.Id = EntryId
                                         where CommitId = @Id
                                         and Entry.DateOfClose < GETDATE()", new SortedList<string, object>() { { "@Id", gComm } });
                    if (NotEnabledApplication == 0)
                        return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", Guid.Parse(Id) } });
                    else
                        NewId = true;
                }
                int SecondTypeId = context.Application.Where(x => x.CommitId == gComm && x.PersonId == PersonId).Select(x => x.SecondTypeId).FirstOrDefault();
                 // поступление
                if (SecondTypeId == 1)
                {
                    #region secondtype=1
                    int EntryTypeId = PersonInfo.Application.Where(x => x.CommitId == gComm).Select(x => x.EntryType).FirstOrDefault();

                    Mag_ApplicationModel model = new Mag_ApplicationModel();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.CommitId = Id;
                    model.Enabled = true;

                    var lst = Util.GetEnableLevelList(new List<int>() { EntryTypeId }, PersonInfo);
                    if (lst.Count == 0)
                    {
                        model.Enabled = false;
                    }
                    else
                        model.MaxBlocks = lst.First().MaxBlocks;

                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    Guid CommitId = Guid.Parse(Id);

                    var CommId = context.Application.Where(x => x.PersonId == PersonId && x.IsCommited == true && x.CommitId == CommitId).Select(x => x.CommitId);
                    if (CommId.Count() > 0)
                    {
                        model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    }
                    if (NewId)
                    {
                        model.OldCommitId = CommitId.ToString();
                        gComm = Guid.NewGuid();
                        model.CommitId = gComm.ToString();
                        model.ProjectJuly = true;
                        Util.CopyApplicationsInAnotherCommit(CommitId, gComm, PersonId);
                        model.Applications = Util.GetApplicationListInCommit(gComm, PersonId);
                    }
                    else
                        model.ProjectJuly = false;
                    switch (EntryTypeId)
                    {
                        case 1: { return View("NewApplication_1kurs", model); }
                        case 2: { return View("NewApplication_Mag", model); }
                        case 3: { return View("NewApplication_SPO", model); }
                        case 4: { return View("NewApplication_Aspirant", model); }
                        case 6:
                        case 7: { return View("NewApplication_AG", model); }
                        default: { return RedirectToAction("Index", "Application"); }
                    }
                #endregion
                } 
                    // восстановление
                else if (SecondTypeId == 3)
                {
                    #region Secondtype=3
                    Mag_ApplicationModel model = new Mag_ApplicationModel();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.CommitId = Id;
                    model.Enabled = true;

                    var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                         where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 3
                                         select x).FirstOrDefault();
                    if (PersonEducDoc != null)
                    {
                        model.MaxBlocks = maxBlockRecover;
                        model.Enabled = true;
                    }
                    else
                        model.Enabled = false;

                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.SemestrList = Util.GetSemesterList();
                    Guid CommitId = Guid.Parse(Id);
                    var CommId = context.Application.Where(x => x.PersonId == PersonId && x.IsCommited == true && x.CommitId == CommitId && x.SecondTypeId == SecondTypeId).Select(x => x.CommitId);
                    if (CommId.Count() > 0)
                    {
                        model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    }
                    return View("NewApplication_Recover", model);
                    #endregion
                }
                    // перевод
                else
                {
                    #region Seconstype = 2,4,5,6
                    Mag_ApplicationModel model = new Mag_ApplicationModel();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.CommitId = Id;

                    var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                         where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                         select x).FirstOrDefault();
                    if (PersonEducDoc != null)
                    {
                        
                        model.Enabled = true;
                    }
                    else
                        model.Enabled = false;

                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.SemestrList = Util.GetSemesterList();
                    Guid CommitId = Guid.Parse(Id);
                    var CommId = context.Application.Where(x => x.PersonId == PersonId && x.IsCommited == true && x.CommitId == CommitId && x.SecondTypeId == SecondTypeId).Select(x => x.CommitId);
                    if (CommId.Count() > 0)
                    {
                        model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    }
                    switch (SecondTypeId)
                    {
                        case 2: { model.MaxBlocks = maxBlockTransfer; return View("NewApplication_Transfer", model); }
                        case 4: { model.MaxBlocks = maxBlockChangeStudyFormBasis; return View("NewApplication_ChangeStudyForm", model); }
                        case 5: { model.MaxBlocks = maxBlockChangeStudyFormBasis; return View("NewApplication_ChangeStudyBasis", model); }
                        case 6: { model.MaxBlocks = maxBlockTransfer; return View("NewApplication_ChangeObrazProgram", model); }
                        default: { model.MaxBlocks = 1; return RedirectToAction("Index", "Application"); }
                    }
                    #endregion
                }
            }
        }
        public ActionResult NewApplication(params string[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (string er in errors)
                    ModelState.AddModelError("", er);
            }
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            bool bisEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                ApplicationModel model = new ApplicationModel();
                model.IsForeign = Util.GetRess(PersonId) == 4;

                if (PersonInfo.PersonEducationDocument == null)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", "4" } });

                var ScTypeList = (from sctype in context.PersonEducationDocument
                                  join highEduc in context.PersonHighEducationInfo on sctype.Id equals highEduc.EducationDocumentId into highEduc
                                  from hEduc in highEduc.DefaultIfEmpty()
                                  where sctype.PersonId == PersonId
                                  select new
                                     {
                                         sctype.SchoolTypeId,
                                         sctype.VuzAdditionalTypeId,
                                         sctype.SchoolExitClassId,
                                         qualification = (hEduc == null) ? -1 : hEduc.QualificationId
                                     }).ToList();

                if (ScTypeList.Count > 0)
                {
                    if (ScTypeList.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId.HasValue).Count() > 0)
                    {
                        var iScTypeId = ScTypeList.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId.HasValue).First();
                        model.EntryType = 2;
                        model.VuzAddType = iScTypeId.VuzAdditionalTypeId.Value;
                        model.ExitClassId = iScTypeId.qualification;
                    }
                    else
                    {
                        List<sp_level> lst = Util.GetEnableLevelList(null, PersonInfo, model.IsForeign);
                        model.AbitTypeList = lst.Select(x => x.type.Value).Distinct().ToList();
                    }
                }
                else //если образований не введено
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", "4" } });

                model.StudyForms = Util.GetStudyFormList(); 
                model.StudyBasises = Util.GetStudyBasisList();
                bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

                //выборка активных заявлений
                model.Applications = new List<SimpleApplicationPackage>();
                var Applications = context.Abiturient.Where(x => x.PersonId == PersonId && x.Enabled == true && x.IsCommited == true && x.CampaignYear == Util.iPriemYear)
                    .Select(x => new { x.CommitId, x.StudyLevelGroupNameEng, x.StudyLevelGroupNameRus, x.EntryType, x.SecondTypeId,  x.IsApprovedByComission }).Distinct();

                var SecondTypeList = context.ApplicationSecondType.Select(x => new { x.Id, Name = bIsEng ? x.NameEng : x.Name });
                foreach (var app in Applications)
                {
                    model.Applications.Add(new SimpleApplicationPackage()
                    {
                        Id = app.CommitId,
                        isApproved = app.IsApprovedByComission,
                        PriemType = app.EntryType.ToString(),
                        StudyLevel = bIsEng ? app.StudyLevelGroupNameEng : app.StudyLevelGroupNameRus +
                                     SecondTypeList.Where(x=>x.Id == app.SecondTypeId).Select(x=>x.Name).FirstOrDefault(),
                    });
                }

                if (model.VuzAddType == 2)
                {
                    var PersonCurrentEduc = context.PersonCurrentEducation.Where(x => x.PersonId == PersonId).FirstOrDefault();
                    if (PersonCurrentEduc == null)
                        model.Message = "Данные некорректны (не найден учебный план). Вернитесь в анкету и проверьте правильность данных";
                    else
                    {
                        int qw = PersonCurrentEduc.LicenseProgramId;
                        model.LicenseProgramName = Util.AbitDB.GetStringValue("select top 1 LicenseProgramName from Entry where LicenseProgramId=@Id", new SortedList<string, object>() { { "@Id", qw } });
                        if (!PersonCurrentEduc.ObrazProgramId.HasValue)
                            model.Message = "Данные некорректны (не найден учебный план). Вернитесь в анкету и проверьте правильность данных";
                        else
                        {
                            qw = PersonCurrentEduc.ObrazProgramId.Value;
                            model.ObrazProgramName = Util.AbitDB.GetStringValue("select top 1 ObrazProgramName from Entry  where ObrazProgramId=@Id", new SortedList<string, object>() { { "@Id", qw } });

                            //Выбираем следующий семестр относительно указанного в данных
                            int ActualSemesterId = Util.GetActualSemester(PersonCurrentEduc.SemesterId);

                            var EntryList =
                                (from Ent in context.Entry
                                 join SPStudyLevel in context.SP_StudyLevel on Ent.StudyLevelId equals SPStudyLevel.Id
                                 where PersonCurrentEduc.LicenseProgramId == Ent.LicenseProgramId &&
                                        Ent.ObrazProgramId == PersonCurrentEduc.ObrazProgramId &&
                                        Ent.StudyFormId == PersonCurrentEduc.StudyFormId &&
                                        Ent.StudyBasisId == PersonCurrentEduc.StudyBasisId &&
                                        Ent.CampaignYear == Util.iPriemYear &&
                                        Ent.StudyLevelId == PersonCurrentEduc.StudyLevelId &&
                                        Ent.IsParallel == false &&
                                        Ent.IsReduced == false &&
                                        Ent.IsSecond == false &&
                                        (Ent.SemesterId == ActualSemesterId || Ent.SemesterId == ActualSemesterId - 1)
                                 select new
                                 {
                                     EntryId = Ent.Id,
                                 }).FirstOrDefault();

                            if (EntryList == null)
                            {
                                model.Message = "Данные некорректны (не найден учебный план). Вернитесь в анкету и проверьте правильность данных";
                            }
                        }
                    }
                }
                return View("NewApplication", model);
            }
        }
        [HttpPost]
        public ActionResult NewApplicationSelect()
        { 
            string val = Request.Form["val_h"]; 
            switch (val)
            {
                case "1": { return RedirectToAction("NewApplication_1kurs", "Abiturient"); } //Поступление на 1 курс гражданам РФ
                case "2": { return RedirectToAction("NewApplication_Mag", "Abiturient"); } //Поступление в магистратуру
                case "3": { return RedirectToAction("NewApplication_ChangeStudyBasis", "Abiturient"); } //Перевод ОСНОВА
                case "4": { return RedirectToAction("NewApplication_Transfer", "Abiturient"); } //Перевод  в СПбГУ
                case "5": { return RedirectToAction("NewApplication_Recover", "Abiturient"); } //Восстановление в СПбГУ
                case "6": { return RedirectToAction("NewApplication_ChangeStudyForm", "Abiturient"); } //Перевод ФОРМА
                case "7": { return RedirectToAction("NewApplication_ChangeObrazProgram", "Abiturient"); } //Смена образовательной программы
                case "8": { return RedirectToAction("NewApplication_AG", "Abiturient"); } //Поступление в Академическую Гимназию
                case "9": { return RedirectToAction("NewApplication_SPO", "Abiturient"); } //Поступление в СПО
                case "10": { return RedirectToAction("NewApplication_Aspirant", "Abiturient"); } //Поступление в аспирантуру гражданам РФ
                case "11": { return RedirectToAction("NewApplication_Ord", "Abiturient"); } //Поступление в ординатуру
                default: { return RedirectToAction("page404", "Abiturient"); }
            }  
        }

        #region NewApplication - POST By Types
        [HttpPost]
        public ActionResult NewApp()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            bool isForeign = Util.CheckIsForeign(PersonId);

            string faculty = Request.Form["lFaculty"];
            string profession = Request.Form["lProfession"];
            string obrazprogram = Request.Form["lObrazProgram"];
            string sform = Request.Form["StudyFormId"];
            string sbasis = Request.Form["StudyBasisId"];
            string isSecond = Request.Form["IsSecondHidden"];
            string isReduced = Request.Form["IsReducedHidden"];
            string isParallel = Request.Form["IsParallelHidden"];
            bool needHostel = string.IsNullOrEmpty(Request.Form["NeedHostel"]) ? false : true;

            int iStudyFormId = Util.ParseSafe(sform);
            int iStudyBasisId = Util.ParseSafe(sbasis);
            int iFacultyId = Util.ParseSafe(faculty);
            int iProfession = Util.ParseSafe(profession);
            int iObrazProgram = Util.ParseSafe(obrazprogram);
            Guid ProfileId = Guid.Empty;
            if (!string.IsNullOrEmpty(Request.Form["lSpecialization"]))
                Guid.TryParse(Request.Form["lSpecialization"], out ProfileId);

            int iEntry = Util.ParseSafe(Request.Form["EntryType"]);
            bool bIsSecond = isSecond == "1" ? true : false;
            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;

            DateTime timeX = new DateTime(2014, 6, 20, 10, 0, 0);//20-06-2013 10:00:00
            if (iEntry != 2 && DateTime.Now < timeX )
            {
                return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Подача заявлений на 1 курс начнётся 20 июня в 10:00 МСК" } });
            }

            //------------------Проверка на дублирование заявлений---------------------------------------------------------------------
            string query = "SELECT qEntry.Id FROM qEntry INNER JOIN SP_StudyLevel ON SP_StudyLevel.Id=qEntry.StudyLevelId WHERE LicenseProgramId=@LicenseProgramId " +
                " AND ObrazProgramId=@ObrazProgramId AND StudyFormId=@SFormId AND StudyBasisId=@SBasisId AND IsSecond=@IsSecond AND IsParallel=@IsParallel AND IsReduced=@IsReduced " +
                (ProfileId == Guid.Empty ? " AND ProfileId IS NULL " : " AND ProfileId=@ProfileId ") + (iFacultyId == 0 ? "" : " AND FacultyId=@FacultyId ") + 
                " AND SemesterId=@SemesterId AND CampaignYear=@CampaignYear";

            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@LicenseProgramId", iProfession);
            dic.Add("@ObrazProgramId", iObrazProgram);
            dic.Add("@SFormId", iStudyFormId);
            dic.Add("@SBasisId", iStudyBasisId);
            dic.Add("@IsSecond", bIsSecond);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@SemesterId", 1);//only 1 semester (1kurs)
            dic.Add("@CampaignYear", Util.iPriemYear);
            if (ProfileId != Guid.Empty)
                dic.Add("@ProfileId", ProfileId);
            if (iFacultyId != 0)
                dic.Add("@FacultyId", iFacultyId);

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            if (tbl.Rows.Count > 1)
                return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Неоднозначный выбор учебного плана (" + tbl.Rows.Count + ")" } });
            if (tbl.Rows.Count == 0)
                return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Не найден учебный план" } });

            Guid EntryId = tbl.Rows[0].Field<Guid>("Id");

            query = "SELECT DateOfClose FROM [Entry] WHERE Id=@Id";
            DateTime DateOfClose = (DateTime)Util.AbitDB.GetValue(query, new SortedList<string, object>() { { "@Id", EntryId } });

            if (DateTime.Now > DateOfClose)
                return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Подача заявлений на данное направление прекращена " + DateOfClose.ToString("dd.MM.yyyy") } });

            query = "SELECT EntryId FROM [Application] WHERE PersonId=@PersonId AND Enabled='True' AND EntryId IS NOT NULL";
            tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
            var eIds =
                from DataRow rw in tbl.Rows
                select rw.Field<Guid>("EntryId");
            if (eIds.Contains(EntryId))
                return RedirectToAction("NewApplication", new RouteValueDictionary() { { "errors", "Заявление на данную программу уже подано" } });

            DataTable tblPriors = Util.AbitDB.GetDataTable("SELECT Priority FROM [Application] WHERE PersonId=@PersonId AND Enabled=@Enabled",
                new SortedList<string, object>() { { "@PersonId", PersonId }, { "@Enabled", true } });
            int? PriorMax =
                (from DataRow rw in tblPriors.Rows
                 select rw.Field<int?>("Priority")).Max();

            Guid appId = Guid.NewGuid();
            query = "INSERT INTO [Application] (Id, PersonId, EntryId, HostelEduc, Enabled, Priority, EntryType, DateOfStart) " +
                "VALUES (@Id, @PersonId, @EntryId, @HostelEduc, @Enabled, @Priority, @EntryType, @DateOfStart)";
            SortedList<string, object> prms = new SortedList<string, object>()
            {
                { "@Id", appId },
                { "@PersonId", PersonId },
                { "@EntryId", EntryId },
                { "@HostelEduc", needHostel },
                { "@Enabled", true },
                { "@Priority", PriorMax.HasValue ? PriorMax.Value + 1 : 1 },
                { "@EntryType", iEntry },
                { "@DateOfStart", DateTime.Now }
            };

            Util.AbitDB.ExecuteQuery(query, prms);

            query = "SELECT Person.Surname, Person.Name, Person.SecondName, Entry.LicenseProgramCode, Entry.LicenseProgramName, Entry.ObrazProgramName " +
                " FROM [Application] INNER JOIN Person ON Person.Id=[Application].PersonId " +
                " INNER JOIN Entry ON Application.EntryId=Entry.Id WHERE Application.Id=@AppId";
            DataTable Tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@AppId", appId } });
            var fileInfo =
                (from DataRow rw in Tbl.Rows
                 select new
                 {
                     Surname = rw.Field<string>("Surname"),
                     Name = rw.Field<string>("Name"),
                     SecondName = rw.Field<string>("SecondName"),
                     ProfessionCode = rw.Field<string>("LicenseProgramCode"),
                     Profession = rw.Field<string>("LicenseProgramName"),
                     ObrazProgram = rw.Field<string>("ObrazProgramName")
                 }).FirstOrDefault();

            return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", appId.ToString("N") } });
        }
        [HttpPost]
        public ActionResult NewApp_AG(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            //20 июня 2017 года 17:00 - закрытие конкурсов
            if (DateTime.Now >= new DateTime(2017, 6, 20, 17, 0, 0))
            {
                if (!bIsEng)
                    return RedirectToAction("NewApplication_AG", new RouteValueDictionary() { { "errors", "Приём документов в АГ СПбГУ ЗАКРЫТ" } });
                else
                    return RedirectToAction("NewApplication_AG", new RouteValueDictionary() { { "errors", "Entry is closed" } });
            }

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            string sOldCommitId = Request.Form["OldCommitId"];
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(sOldCommitId))
            {
                if (!Guid.TryParse(sOldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID);
            }

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)
                    return RedirectToAction("Index");

                if (PersonInfo.RegistrationStage != 100)
                    return RedirectToAction("Index", new RouteValueDictionary() { { "step", (PersonInfo.RegistrationStage).ToString() } });

                var lst = Util.GetEnableLevelList(new List<int>() { 6, 7 }, PersonInfo);
                if (lst.Count == 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в Академическую Гимназию (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data." ;
                     return View("NewApplication_AG", model);
                }

                if (!OldCommitId.Equals(Guid.Empty))
                {
                    var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                    foreach (var AppId in Ids)
                    {
                        var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                        if (App == null)
                            continue;
                        App.IsCommited = false;
                    }
                    context.SaveChanges();
                    Util.DifferenceBetweenCommits(OldCommitId, CommitId, PersonId);
                    bool? result = PDFUtils.GetDisableApplicationPDF(OldCommitId, Server.MapPath("~/Templates/"), PersonId);
                    // печать заявления об отзыве (проверить isDeleted и возможно переставить код выше)
                    Util.CommitApplication(CommitId, PersonId, context);
                }

                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                    "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные." :
                    "To submit new application you should cancel your active application.";
                    return View("NewApplication_AG", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Count() == 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_AG", model);
                }

                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("ApplicationExams", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }

        [HttpPost]
        public ActionResult NewApp_Mag(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);
            string sOldCommitId = Request.Form["OldCommitId"];
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(sOldCommitId))
            {
                if (!Guid.TryParse(sOldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID);
            }

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");
                var lst = Util.GetEnableLevelList(new List<int>() {2}, PersonInfo);
                if (lst.Count == 0)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать заявление в магистратуру (не соответствует уровень образования)" :
                    "Change your previous education degree in Questionnaire Data";
                    return View("NewApplication_Mag", model);
                }
                else if (PersonInfo.PersonEducationDocument.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId != null).Count() > 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в магистратуру (смените тип поступления в Анкете)" :
                        "Change your Entry Type in Questionnaire Data";
                    return View("NewApplication_Mag", model);
                }
                if (!OldCommitId.Equals(Guid.Empty))
                {
                    var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                    foreach (var AppId in Ids)
                    {
                        var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                        if (App == null)
                            continue;
                        App.IsCommited = false;
                    }
                    context.SaveChanges();
                    Util.CopyApplicationFiles(OldCommitId, CommitId, PersonId);
                    Util.DifferenceBetweenCommits(OldCommitId, CommitId, PersonId);
                    bool? result = PDFUtils.GetDisableApplicationPDF(OldCommitId, Server.MapPath("~/Templates/"), PersonId);
                    // печать заявления об отзыве (проверить isDeleted и возможно переставить код выше)
                    Util.CommitApplication(CommitId, PersonId, context);
                }

                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && x.C_Entry.StudyLevelId == 17 && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные." :
                        "To submit new application you should cancel your active application.";
                    return View("NewApplication_Mag", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_Mag", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_Asp(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);
            string sOldCommitId = Request.Form["OldCommitId"];
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(sOldCommitId))
            {
                if (!Guid.TryParse(sOldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID);
            }
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");
                var lst = Util.GetEnableLevelList(new List<int>() { 4 }, PersonInfo);
                if (lst.Count == 0)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в аспирантуру (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data";
                    return View("NewApplication_Aspirant", model);
                }
                else if (PersonInfo.PersonEducationDocument.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId != null).Count() > 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в аспирантуру (смените тип поступления в Анкете)" :
                        "Change your Entry Type in Questionnaire Data";
                    return View("NewApplication_Aspirant", model);
                }
                if (!OldCommitId.Equals(Guid.Empty))
                {
                    var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                    foreach (var AppId in Ids)
                    {
                        var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                        if (App == null)
                            continue;
                        App.IsCommited = false;
                    }
                    context.SaveChanges(); 
                    Util.CopyApplicationFiles(OldCommitId, CommitId, PersonId);
                    Util.DifferenceBetweenCommits(OldCommitId, CommitId, PersonId);
                    bool? result = PDFUtils.GetDisableApplicationPDF(OldCommitId, Server.MapPath("~/Templates/"), PersonId);
                    // печать заявления об отзыве (проверить isDeleted и возможно переставить код выше)
                    Util.CommitApplication(CommitId, PersonId, context);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && x.C_Entry.StudyLevelId == 15 && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng)?
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные.":
                        "To submit new application you should cancel your active application.";  
                    return View("NewApplication_Aspirant", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng)?
                        "Невозможно подать пустое заявление":
                        "You can not submit empty application."; 
                    return View("NewApplication_Aspirant", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_Ord(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            string sOldCommitId = Request.Form["OldCommitId"];
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(sOldCommitId))
            {
                if (!Guid.TryParse(sOldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID);
            }
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");
                var lst = Util.GetEnableLevelList(new List<int>() { 5 }, PersonInfo);
                if (lst.Count == 0)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать заявление в ординатуру (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data";
                    return View("NewApplication_Ord", model);
                }
                else if (PersonInfo.PersonEducationDocument.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId != null).Count() > 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление в ординатуру (смените тип поступления в Анкете)" :
                        "Change your Entry Type in Questionnaire Data";
                    return View("NewApplication_Ord", model);
                }

                if (!OldCommitId.Equals(Guid.Empty))
                {
                    var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                    foreach (var AppId in Ids)
                    {
                        var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                        if (App == null)
                            continue;
                        App.IsCommited = false;
                    }
                    context.SaveChanges();
                    Util.DifferenceBetweenCommits(OldCommitId, CommitId, PersonId);
                    bool? result = PDFUtils.GetDisableApplicationPDF(OldCommitId, Server.MapPath("~/Templates/"), PersonId);
                    // печать заявления об отзыве (проверить isDeleted и возможно переставить код выше)
                    Util.CommitApplication(CommitId, PersonId, context);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && x.C_Entry.StudyLevelId == 15 && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные." :
                        "To submit new application you should cancel your active application.";
                    return View("NewApplication_Ord", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_Ord", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_1kurs(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            //if (DateTime.Now >= new DateTime(2014, 6, 23, 0, 0, 0))
            //    return RedirectToAction("NewApplication_AG", new RouteValueDictionary() { { "errors", "Приём документов в АГ СПбГУ ЗАКРЫТ" } });

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);
            string sOldCommitId = Request.Form["OldCommitId"];
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(sOldCommitId))
            {
                if (!Guid.TryParse(sOldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID);
            }
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");
                bool isForeign = Util.GetRess(PersonId) == 4;
                var lst = Util.GetEnableLevelList(new List<int>() { 1 }, PersonInfo, isForeign);
                if (lst.Count == 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление на первый курс (не соответствует уровень образования)" :
                        "Change your previous education degree in Questionnaire Data";
                    return View("NewApplication_1kurs", model);
                }
                else if (PersonInfo.PersonEducationDocument.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId != null).Count() > 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление на первый курс (смените тип поступления в Анкете)" :
                        "Change your Entry Type in Questionnaire Data";
                    return View("NewApplication_1kurs", model);
                }
                if (!OldCommitId.Equals(Guid.Empty))
                {
                    var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                    foreach (var AppId in Ids)
                    {
                        var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                        if (App == null)
                            continue;
                        App.IsCommited = false;
                    }
                    context.SaveChanges();
                    Util.DifferenceBetweenCommits(OldCommitId, CommitId, PersonId);
                    bool? result = PDFUtils.GetDisableApplicationPDF(OldCommitId, Server.MapPath("~/Templates/"), PersonId);
                    // печать заявления об отзыве (проверить isDeleted и возможно переставить код выше)
                    Util.CommitApplication(CommitId, PersonId, context);
                }

                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && (x.C_Entry.StudyLevelId == 16 || x.C_Entry.StudyLevelId == 18) && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng)?
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные.":
                        "To submit new application you should cancel your active application.";  
                    return View("NewApplication_1kurs", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng)?
                        "Невозможно подать пустое заявление":
                        "You can not submit empty application."; 
                    return View("NewApplication_1kurs", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", "Abiturient", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_SPO(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);
            string sOldCommitId = Request.Form["OldCommitId"];
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(sOldCommitId))
            {
                if (!Guid.TryParse(sOldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID);
            }
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");
                var lst = Util.GetEnableLevelList(new List<int>() { 3 }, PersonInfo);
                if (lst.Count == 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Подача заявления в СПО доступна только для людей, уже закончивших 9 классов школы" :
                        "Change your previous education degree in Questionnaire Data";
                    return View("NewApplication_SPO", model);
                }
                else if (PersonInfo.PersonEducationDocument.Where(x => x.VuzAdditionalTypeId != 1 && x.VuzAdditionalTypeId != null).Count() > 0)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng)?
                            "Невозможно подать заявление в СПО (смените тип поступления в Анкете)":
                            "Change your Entry Type in Questionnaire Data";
                    return View("NewApplication_SPO", model);
                }
                if (!OldCommitId.Equals(Guid.Empty))
                {
                    var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                    foreach (var AppId in Ids)
                    {
                        var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                        if (App == null)
                            continue;
                        App.IsCommited = false;
                    }
                    context.SaveChanges();
                    Util.DifferenceBetweenCommits(OldCommitId, CommitId, PersonId);
                    bool? result = PDFUtils.GetDisableApplicationPDF(OldCommitId, Server.MapPath("~/Templates/"), PersonId);
                    // печать заявления об отзыве (проверить isDeleted и возможно переставить код выше)
                    Util.CommitApplication(CommitId, PersonId, context);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && (x.C_Entry.StudyLevelId == 10 || x.C_Entry.StudyLevelId == 8) && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные." :
                        "To submit new application you should cancel your active application.";
                    return View("NewApplication_SPO", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = lst.First().MaxBlocks;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_SPO", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_Recover(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                var PersonDisOrderInfo = context.PersonDisorderInfo.Where(x => x.PersonId == PersonId).FirstOrDefault();
                if (PersonDisOrderInfo == null)
                    return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", 5 } });

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 3
                                     select x).FirstOrDefault();
                if (PersonEducDoc == null)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление на восстановление (не соответствует уровень образования и/или смените тип поступления в Анкете)" :
                        "Change your previous education degree in Questionnaire Data And (Or) change your Entry Type in Questionnaire Data";
                    return View("NewApplication_Recover", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = maxBlockRecover;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_Recover", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_Transfer(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 4
                                     select x).FirstOrDefault();
                if (PersonEducDoc == null)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать заявление на перевод (не соответствует уровень образования и/или смените тип поступления в Анкете)" :
                        "Change your previous education degree (or change your Entry Type) in Questionnaire Data";
                    return View("NewApplication_Transfer", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = maxBlockTransfer;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_Transfer", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_ChangeStudyForm(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                     select x).FirstOrDefault();
                if (PersonEducDoc == null)
                {
                    model.Enabled = false;
                    model.HasError = true;
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление на перевод (не соответствует уровень образования и/или смените тип поступления в Анкете)" :
                        "Change your previous education degree (or change your Entry Type) in Questionnaire Data";
                    return View("NewApplication_Transfer", model);
                }

                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && x.SecondTypeId == 4 && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = maxBlockChangeStudyFormBasis;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные.":
                        "To submit new application you should cancel your active application.";
                    return View("NewApplication_ChangeStudyForm", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = maxBlockChangeStudyFormBasis;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_ChangeStudyForm", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_ChangeObrazProgram(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            //if (DateTime.Now >= new DateTime(2014, 6, 23, 0, 0, 0))
            //    return RedirectToAction("NewApplication_AG", new RouteValueDictionary() { { "errors", "Приём документов в АГ СПбГУ ЗАКРЫТ" } });

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                     select x).FirstOrDefault();
                if (PersonEducDoc == null)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление на перевод (не соответствует уровень образования и/или смените тип поступления в Анкете)" :
                        "Change your previous education degree (or Change your Entry Type) in Questionnaire Data";
                    return View("NewApplication_ChangeObrazProgram", model);
                }

                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = maxBlockTransfer;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать пустое заявление" :
                        "You can not submit empty application.";
                    return View("NewApplication_ChangeObrazProgram", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        [HttpPost]
        public ActionResult NewApp_ChangeStudyBasis(Mag_ApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            //if (DateTime.Now >= new DateTime(2014, 6, 23, 0, 0, 0))
            //    return RedirectToAction("NewApplication_AG", new RouteValueDictionary() { { "errors", "Приём документов в АГ СПбГУ ЗАКРЫТ" } });

            string sCommitId = Request.Form["CommitId"];
            Guid CommitId;
            if (!Guid.TryParse(sCommitId, out CommitId))
                return Json(Resources.ServerMessages.IncorrectGUID);

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                var PersonEducDoc = (from x in PersonInfo.PersonEducationDocument
                                     where x.SchoolTypeId == 4 && x.VuzAdditionalTypeId == 2
                                     select x).FirstOrDefault();
                if (PersonEducDoc == null)
                {
                    // окончил не вуз
                    model.Enabled = false;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Невозможно подать заявление на перевод (не соответствует уровень образования и/или смените тип поступления в Анкете)" :
                        "Change your previous education degree (or Change your Entry Type) in Questionnaire Data";
                    return View("NewApplication_ChangeObrazProgram", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId != CommitId && x.IsCommited == true && x.SecondTypeId == 5 && x.C_Entry.CampaignYear == Util.iPriemYear).Count() > 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.Applications = Util.GetApplicationListInCommit(CommitId, PersonId);
                    model.MaxBlocks = maxBlockChangeStudyFormBasis;
                    model.HasError = true;
                    model.ErrorMessage = (!bIsEng) ?
                        "Уже существует активное заявление. Для создания нового заявления необходимо удалить уже созданные." :
                        "To submit new application you should cancel your active application.";
                    return View("NewApplication_ChangeStudyBasis", model);
                }
                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == CommitId && !x.IsDeleted).Select(x => x.Id).Count() == 0)
                {
                    model.SemestrList = Util.GetSemesterList();
                    model.StudyFormList = Util.GetStudyFormList();
                    model.StudyBasisList = Util.GetStudyBasisList();
                    model.StudyLevelGroupList = Util.GetStudyLevelGroupList();
                    model.Applications = new List<Mag_ApplicationSipleEntity>();
                    model.MaxBlocks = maxBlockChangeStudyFormBasis;
                    model.HasError = true;
                    model.Enabled = true;
                    model.ErrorMessage = (!bIsEng) ? 
                        "Невозможно подать пустое заявление":
                        "You can not submit empty application.";
                    return View("NewApplication_ChangeStudyBasis", model);
                }
                Util.CommitApplication(CommitId, PersonId, context);
            }
            return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", CommitId.ToString() } });
        }
        #endregion

        #region Priorities&InnerPriorities
        public ActionResult PriorityChanger(string ComId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommitId;
            if (!Guid.TryParse(ComId, out gCommitId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            Guid gComm = gCommitId;
            Guid VersionId = Guid.NewGuid();
            bool bisEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                bool bIsAG = (int)Util.AbitDB.GetValue("SELECT COUNT(*) FROM AG_Application WHERE [CommitId]=@Id ", new SortedList<string, object>() { { "@Id", gCommitId } }) > 0;
                if (!bIsAG)
                {
                    bool isPrinted = (bool)Util.AbitDB.GetValue("SELECT IsPrinted FROM ApplicationCommit WHERE Id=@Id ", new SortedList<string, object>() { { "@Id", gCommitId } });
                    if (isPrinted)
                    {
                        int NotEnabledApplication = (int)Util.AbitDB.GetValue(@"select count (Application.Id) from Application
                                        inner join Entry on Entry.Id = EntryId
                                        where CommitId = @Id
                                        and Entry.DateOfClose < GETDATE()", new SortedList<string, object>() { { "@Id", gCommitId } });
                        if (NotEnabledApplication == 0)
                        {
                            return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", gCommitId } });
                        }
                        else
                        {
                            gComm = Guid.NewGuid();
                            Util.CopyApplicationsInAnotherCommit(gCommitId, gComm, PersonId);
                        }
                    }
                }

                var apps =
                    (from App in context.Application
                     join Entry in context.Entry on App.EntryId equals Entry.Id
                     join Semester in context.Semester on Entry.SemesterId equals Semester.Id

                     join apstype in context.ApplicationSecondType on App.SecondTypeId equals apstype.Id into _sectype
                     from sectype in _sectype.DefaultIfEmpty()

                     where App.PersonId == PersonId && App.CommitId == gComm && App.Enabled == true
                     select new SimpleApplication()
                     {
                         Id = App.Id,
                         Priority = App.Priority,
                         StudyForm = (bisEng ? ((String.IsNullOrEmpty(Entry.StudyFormNameEng)) ? Entry.StudyFormName : Entry.StudyFormNameEng) : Entry.StudyFormName),
                         StudyBasis = bisEng ? ((String.IsNullOrEmpty(Entry.StudyBasisNameEng)) ? Entry.StudyBasisName : Entry.StudyBasisNameEng) : Entry.StudyBasisName,
                         Profession = Entry.LicenseProgramCode + " " + (bisEng ? ((String.IsNullOrEmpty(Entry.LicenseProgramNameEng)) ? Entry.LicenseProgramName : Entry.LicenseProgramNameEng) : Entry.LicenseProgramName),
                         ObrazProgram = Entry.ObrazProgramCrypt + " " + (bisEng ? ((String.IsNullOrEmpty(Entry.ObrazProgramNameEng)) ? Entry.ObrazProgramName : Entry.ObrazProgramNameEng) : Entry.ObrazProgramName),
                         Specialization = (bisEng ? ((String.IsNullOrEmpty(Entry.ProfileNameEng)) ? Entry.ProfileName : Entry.ProfileNameEng) : Entry.ProfileName),
                         HasManualExams = false,
                         HasSeparateObrazPrograms = context.InnerEntryInEntry.Where(x => x.EntryId == App.EntryId).Count() > 0,
                         InnerEntryInEntryId = context.InnerEntryInEntry.Where(x => x.EntryId == App.EntryId).Count() == 1 ? context.InnerEntryInEntry.Where(x => x.EntryId == App.EntryId).Select(x => x.Id).FirstOrDefault() : Guid.Empty,
                         EntryId = App.EntryId,
                         IsGosLine = Entry.IsForeign && Entry.StudyBasisId == 1,
                         IsCrimea = Entry.IsCrimea,
                         dateofClose = Entry.DateOfClose,
                         Enabled = Entry.DateOfClose > DateTime.Now ? true : false,
                         SemesterName = (Entry.SemesterId != 1) ? Semester.Name : "",
                         SecondTypeName = "",
                         StudyLevelGroupId = Entry.StudyLevelGroupId,
                         StudyLevelGroupName = (bisEng ? ((String.IsNullOrEmpty(Entry.StudyLevelGroupNameEng)) ? Entry.StudyLevelGroupNameRus : Entry.StudyLevelGroupNameEng) : Entry.StudyLevelGroupNameRus) +
                                    (sectype == null ? "" : (bisEng ? sectype.NameEng : sectype.Name))
                     }).ToList();

                int iVuzAddType = context.PersonEducationDocument.Where(x => x.PersonId == PersonId).Select(x => x.VuzAdditionalTypeId ?? 1).ToList().DefaultIfEmpty(1).Max();

                MotivateMailModel mdl = new MotivateMailModel()
                {
                    CommitId = gComm.ToString(),
                    OldCommitId = (gComm.Equals(gCommitId))?"": gCommitId.ToString(),
                    Apps = apps.OrderBy(x=>x.Priority).ToList(),
                    UILanguage = Util.GetUILang(PersonId),
                    VersionId = VersionId.ToString("N"),
                    StudyLevelGroupId = apps.Select(x => x.StudyLevelGroupId).First(),
                    VuzAdditionalType = iVuzAddType
                };
                return View(mdl);
            }
        }
        public ActionResult PriorityChangerApplication(string AppId, string V)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gAppId;
            if (!Guid.TryParse(AppId, out gAppId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            Guid gVersionId;
            if (!Guid.TryParse(V, out gVersionId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            bool isEng = Util.GetCurrentThreadLanguageIsEng();

            return View(AbiturientClass.GetPriorityChangerApplication(gAppId, gVersionId, PersonId, isEng));
        }
        public ActionResult PriorityChangerApp(PriorityChangerApplicationModel model)
        {
            return View("PriorityChangerApplication", model);
        }

        [HttpPost]
        public ActionResult ChangePriority(MotivateMailModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommId;
            if (!Guid.TryParse(model.CommitId, out gCommId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet); 
             
            Guid OldCommitId = Guid.Empty;
            if (!String.IsNullOrEmpty(model.OldCommitId))
            {
                if (!Guid.TryParse(model.OldCommitId, out OldCommitId))
                    return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);
                else
                    using (OnlinePriemEntities context = new OnlinePriemEntities())
                    {
                        var Ids = context.Application.Where(x => x.PersonId == PersonId && x.CommitId == OldCommitId && !x.IsDeleted).Select(x => x.Id).ToList();
                        foreach (var AppId in Ids)
                        {
                            var App = context.Application.Where(x => x.Id == AppId).FirstOrDefault();
                            if (App == null)
                                continue;
                            App.IsCommited = false;
                        }
                        context.SaveChanges();
                        Util.CopyApplicationFiles(OldCommitId, gCommId, PersonId);
                        Util.CommitApplication(gCommId, PersonId, context);
                    }
            }

            //создаём новую версию изменений
            SortedList<string, object> slParams = new SortedList<string, object>();
            slParams.Add("CommitId", gCommId); 
            slParams.Add("VersionDate", DateTime.Now);
            string val = Util.AbitDB.InsertRecordReturnValue("ApplicationCommitVersion", slParams);
            int iCommitVersionId = 0;
            int.TryParse(val, out iCommitVersionId);

            int prior = 0;
            string[] allKeys = Request.Form.AllKeys;
            foreach (string key in allKeys)
            {
                Guid appId;
                if (!Guid.TryParse(key, out appId))
                    continue;

                SortedList<string, object> dic = new SortedList<string, object>(); 
                dic.AddItem("@Id", appId);

                bool bIsAG = (int)Util.AbitDB.GetValue("Select COUNT(*) FROM AG_Application WHERE Id=@Id", dic) > 0;
                if (!bIsAG)
                {
                    string query = "Select DateOfClose, Priority from Application inner join Entry on Entry.Id = EntryId where Application.Id = @Id";
                    DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
                    DataRow r = tbl.Rows[0];
                    int priority = r.Field<int>("Priority");
                    DateTime? dateofClose = r.Field<DateTime?>("DateOfClose");
                    if (dateofClose != null)
                        if (dateofClose < DateTime.Now)
                            prior = priority - 1;

                    query = "UPDATE [Application] SET IsViewed=0, Priority=@Priority WHERE Id=@Id AND PersonId=@PersonId AND CommitId=@CommitId;" +
                        " INSERT INTO [ApplicationCommitVersonDetails] (ApplicationCommitVersionId, ApplicationId, Priority) VALUES (@ApplicationCommitVersionId, @Id, @Priority)";
                    dic.AddItem("@Priority", ++prior);
                    dic.AddItem("@PersonId", PersonId);
                    dic.AddItem("@CommitId", gCommId);
                    dic.AddItem("@ApplicationCommitVersionId", iCommitVersionId);

                    try
                    {
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }
                    catch { }
                }
                else
                {
                    string query = "UPDATE [AG_Application] SET Priority=@Priority WHERE Id=@Id AND PersonId=@PersonId AND CommitId=@CommitId" +
                        " INSERT INTO [ApplicationCommitVersonDetails] (ApplicationCommitVersionId, ApplicationId, Priority) VALUES (@ApplicationCommitVersionId, @Id, @Priority)";
                    try
                    {
                        dic.AddItem("@Priority", ++prior);
                        dic.AddItem("@PersonId", PersonId);
                        dic.AddItem("@CommitId", gCommId);
                        dic.AddItem("@ApplicationCommitVersionId", iCommitVersionId);
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }
                    catch { }
                }
            }
            return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", model.CommitId } });
        }
        [HttpPost]
        public ActionResult PriorityChangeApplication(PriorityChangerApplicationModel model)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommId = model.CommitId;

            int prior = 0;
            string[] allKeys = Request.Form.AllKeys;
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                if (context.ApplicationVersion.Where(x => x.Id == model.ApplicationVersionId).Count() == 0)
                    context.ApplicationVersion.Add(new ApplicationVersion() { Id = model.ApplicationVersionId, ApplicationId = model.ApplicationId, VersionDate = DateTime.Now });

                var s = context.ApplicationDetails.Where(x => x.ApplicationId == model.ApplicationId).Select(x => new { x.Id, x.InnerEntryInEntryId }).ToList();
                Guid EntryId = context.Application.Where(x => x.Id == model.ApplicationId).Select(x => x.EntryId).First();
                var defaults = context.InnerEntryInEntry.Where(x => x.EntryId == EntryId)
                    .Select(x => new 
                    { 
                        InnerEntryInEntryId = x.Id, 
                        x.ObrazProgramId, 
                        ObrazProgramName = x.SP_ObrazProgram.Name, 
                        x.ProfileId, 
                        ProfileName = x.SP_Profile.Name 
                    }).ToList().OrderBy(x => x.ObrazProgramName).ThenBy(x => x.ProfileName);

                foreach (string key in allKeys)
                {
                    Guid InnerEntryInEntryId;
                    if (!Guid.TryParse(key, out InnerEntryInEntryId))
                        continue;

                    prior++;

                    var versDetails = s.Where(x => x.InnerEntryInEntryId == InnerEntryInEntryId).ToList();
                    if (versDetails.Count == 0) //ещё ничего не создано
                    {
                        context.ApplicationDetails.Add(new ApplicationDetails()
                        {
                            Id = Guid.NewGuid(),
                            ApplicationId = model.ApplicationId,
                            InnerEntryInEntryId = InnerEntryInEntryId,
                            InnerEntryInEntryPriority = prior,
                        });
                        //вставляем в логи
                        if (context.ApplicationVersionDetails
                            .Where(x => x.ApplicationVersionId == model.ApplicationVersionId && x.InnerEntryInEntryId == InnerEntryInEntryId && x.InnerEntryInEntryPriority == prior).Count() == 0)
                        {
                            context.ApplicationVersionDetails.Add(new ApplicationVersionDetails()
                            {
                                ApplicationVersionId = model.ApplicationVersionId,
                                InnerEntryInEntryId = InnerEntryInEntryId,
                                InnerEntryInEntryPriority = prior,
                            });
                        }
                    }
                    else //уже что-то есть - нужно лишь обновить и дополнить, если требуется
                    {
                        var avd = context.ApplicationDetails
                            .Where(x => x.InnerEntryInEntryId == InnerEntryInEntryId && x.ApplicationId == model.ApplicationId)
                            .FirstOrDefault();
                        if (avd == null)
                        {
                            context.ApplicationDetails.Add(new ApplicationDetails()
                            {
                                Id = Guid.NewGuid(),
                                ApplicationId = model.ApplicationId,
                                InnerEntryInEntryId = InnerEntryInEntryId,
                                InnerEntryInEntryPriority = prior
                            });
                            //вставляем в логи
                            context.ApplicationVersionDetails.Add(new ApplicationVersionDetails()
                            {
                                ApplicationVersionId = model.ApplicationVersionId,
                                InnerEntryInEntryId = InnerEntryInEntryId,
                                InnerEntryInEntryPriority = prior
                            });
                        }
                        else // если есть - обновить только приоритет
                        {
                            avd.InnerEntryInEntryPriority = prior;
                        }
                    }

                    context.SaveChanges();
                }
            }

            var mdl = AbiturientClass.GetPriorityChangerApplication(model.ApplicationId, model.ApplicationVersionId, PersonId, Util.GetCurrentThreadLanguageIsEng());
            mdl.MessageText = "Данные успешно сохранены";

            return View("PriorityChangerApplication", mdl);
        }
        #endregion
        #region ExamsBlock

        public ActionResult ApplicationExams(string ComId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommitId;
            if (!Guid.TryParse(ComId, out gCommitId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            Guid gComm = gCommitId;

            bool bisEng = Util.GetCurrentThreadLanguageIsEng();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                bool isPrinted = (bool)Util.AbitDB.GetValue("SELECT IsPrinted FROM ApplicationCommit WHERE Id=@Id ", new SortedList<string, object>() { { "@Id", gCommitId } });
                if (isPrinted)
                {
                    int NotEnabledApplication = (int)Util.AbitDB.GetValue(@"select count (Application.Id) from Application
                                        inner join Entry on Entry.Id = EntryId
                                        where CommitId = @Id
                                        and Entry.DateOfClose < GETDATE()", new SortedList<string, object>() { { "@Id", gCommitId } });
                    if (NotEnabledApplication == 0)
                    {
                        return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", gCommitId } });
                    }
                    else
                    {
                        gComm = Guid.NewGuid();
                        Util.CopyApplicationsInAnotherCommit(gCommitId, gComm, PersonId);
                    }
                }

                var apps = GetAppsWithExams(PersonId, gComm, bisEng);

                var ExamsList = apps.SelectMany(x => x.Exams).ToList();
                if (ExamsList.Count == 0 || ExamsList.Where(x=>x.ExamInBlockList.Count>1).Count()==0)
                {
                    foreach (var app in apps)
                        foreach (var ex in app.Exams)
                        {
                            context.ApplicationSelectedExam.Add(new ApplicationSelectedExam()
                            {
                                ApplicationId = app.Id,
                                ExamInEntryBlockUnitId = (ex.SelectedExamInBlockId == Guid.Empty) ? ex.FirstUnitId : ex.SelectedExamInBlockId,
                            });
                        }
                    context.SaveChanges();
                    return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", gComm.ToString() } });
                }

                Mag_ApplicationExams mdl = new Mag_ApplicationExams()
                {
                    CommitId = gComm.ToString(),
                    Applications = apps,
                };
                return View(mdl);
            }
        }
        public List<SimpleApplicationWithExams> GetAppsWithExams(Guid PersonId, Guid gComm, bool bisEng)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var apps =
                    (from App in context.Application
                     join Entry in context.Entry on App.EntryId equals Entry.Id
                     join Semester in context.Semester on Entry.SemesterId equals Semester.Id

                     join apstype in context.ApplicationSecondType on App.SecondTypeId equals apstype.Id into _sectype
                     from sectype in _sectype.DefaultIfEmpty()

                     where App.PersonId == PersonId && App.CommitId == gComm && App.Enabled == true
                     select new SimpleApplicationWithExams()
                     {
                         Id = App.Id,
                         Priority = App.Priority,
                         StudyForm = (bisEng ? ((String.IsNullOrEmpty(Entry.StudyFormNameEng)) ? Entry.StudyFormName : Entry.StudyFormNameEng) : Entry.StudyFormName),
                         StudyBasis = (bisEng ? ((String.IsNullOrEmpty(Entry.StudyBasisNameEng)) ? Entry.StudyBasisName : Entry.StudyBasisNameEng) : Entry.StudyBasisName),
                         Profession = Entry.LicenseProgramCode + " " + (bisEng ? ((String.IsNullOrEmpty(Entry.LicenseProgramNameEng)) ? Entry.LicenseProgramName : Entry.LicenseProgramNameEng) : Entry.LicenseProgramName),
                         ObrazProgram = Entry.ObrazProgramCrypt + " " + (bisEng ? ((String.IsNullOrEmpty(Entry.ObrazProgramNameEng)) ? Entry.ObrazProgramName : Entry.ObrazProgramNameEng) : Entry.ObrazProgramName),
                         Specialization = (bisEng ? ((String.IsNullOrEmpty(Entry.ProfileNameEng)) ? Entry.ProfileName : Entry.ProfileNameEng) : Entry.ProfileName),
                         HasManualExams = false,
                         HasSeparateObrazPrograms = context.InnerEntryInEntry.Where(x => x.EntryId == App.EntryId).Count() > 0,
                         InnerEntryInEntryId = context.InnerEntryInEntry.Where(x => x.EntryId == App.EntryId).Count() == 1 ? context.InnerEntryInEntry.Where(x => x.EntryId == App.EntryId).Select(x => x.Id).FirstOrDefault() : Guid.Empty,
                         EntryId = App.EntryId,
                         IsGosLine = App.IsGosLine,
                         dateofClose = Entry.DateOfClose,
                         Enabled = Entry.DateOfClose > DateTime.Now ? true : false,
                         SemesterName = (Entry.SemesterId != 1) ? Semester.Name : "",
                         SecondTypeName = "",
                         StudyLevelGroupId = Entry.StudyLevelGroupId,
                         StudyLevelGroupName = (bisEng && !String.IsNullOrEmpty(Entry.StudyLevelGroupNameEng) ? Entry.StudyLevelGroupNameEng : Entry.StudyLevelGroupNameRus) +
                                   (sectype == null ? "" :( bisEng ? sectype.NameEng : sectype.Name))
                     }).ToList();
                foreach (var app in apps)
                {
                    app.Exams = Util.GetExamList(app.Id);
                    app.HasManualExams = app.Exams.Where(x=>x.isVisible).Count() > 0;
                }
                return apps;
            }
        }
        [HttpPost]
        public ActionResult ApplicationExamsSave(Mag_ApplicationExams mdl)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            Guid gCommitId;
            if (!Guid.TryParse(mdl.CommitId.ToString(), out gCommitId))
                return Json(Resources.ServerMessages.IncorrectGUID, JsonRequestBehavior.AllowGet);

            bool bisEng = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var apps = (from App in context.Application
                            where App.PersonId == PersonId && App.CommitId == gCommitId && App.Enabled == true
                            select App.Id).ToList();

                foreach (var app in mdl.Applications)
                {
                    Guid gAppId = app.Id;
                    if (!apps.Contains(gAppId))
                    { continue; }

                    var units = (from App in context.Application
                                 join block in context.ExamInEntryBlock on App.EntryId equals block.EntryId
                                 join unit in context.ExamInEntryBlockUnit on block.Id equals unit.ExamInEntryBlockId
                                 where App.Id == gAppId
                                 select new { blockId = block.Id, unitId = unit.Id }).ToList();
                    if (app.Exams !=null)
                        foreach (var Exam in app.Exams)
                        {
                            Guid gUnitId = Exam.SelectedExamInBlockId;
                            Guid gBlockId = Exam.Id;

                            if (units.Where(x => x.blockId == gBlockId && x.unitId == gUnitId).Count() == 0)
                            { continue; }

                            string query = @"
                            Delete from dbo.ApplicationSelectedExam 
where 
ApplicationId = @AppId 
and ExamInEntryBlockUnitId IN 
    (select Id from ExamInEntryBlockUnit where ExamInEntryBlockId IN (
        select Id from ExamInEntryBlock 
        where Id = @BlockId
        or ParentExamInEntryBlockId = (Select ParentExamInEntryBlockId from ExamInEntryBlock where Id = @BlockId))
    )
and ExamInEntryBlockUnitId <> @UnitId";
                            Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@AppId", gAppId }, { "@BlockId", gBlockId }, { "@UnitId", gUnitId } });

                            query = @"
if NOT EXISTS (Select * from ApplicationSelectedExam where
ApplicationId=@AppId and ExamInEntryBlockUnitId=@UnitId)
begin
insert into dbo.ApplicationSelectedExam (ApplicationId, ExamInEntryBlockUnitId) VALUES (@AppId, @UnitId)
end";
                            Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@AppId", gAppId }, { "@UnitId", gUnitId } });
                        }
                }
            }
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var cnt = (from App in context.Application
                            join block in context.ExamInEntryBlock on App.EntryId equals block.EntryId
                            join unit in context.ExamInEntryBlockUnit on block.Id equals unit.ExamInEntryBlockId
                            where App.PersonId == PersonId && App.CommitId == gCommitId && App.Enabled == true && (block.ParentExamInEntryBlockId == null)

                            select new
                            {
                                appId = App.Id,
                                blockId = block.Id,
                                unitcoun = unit.Id,
                            }).ToList();
                int Cnt1 = (from c in cnt
                            group c by c.blockId into ex
                            where ex.Count() >1
                            select ex).Count();
                int Cnt2 = (from App in context.Application
                            join units in context.ApplicationSelectedExam on App.Id equals units.ApplicationId
                            where App.PersonId == PersonId && App.CommitId == gCommitId && App.Enabled == true
                            select new
                            {
                                units.ApplicationId,
                                units.ExamInEntryBlockUnitId,
                            }).Distinct().Count();

                mdl.Applications = GetAppsWithExams(PersonId, gCommitId, bisEng);
                if (Cnt1 > Cnt2)
                {
                    mdl.ErrorMsg = new List<string>();
                    mdl.ErrorMsg.Add("Не все экзамены выбраны");
                    return View("ApplicationExams", mdl);
                }
                else
                {
                    mdl.ErrorMsg = new List<string>();
                    return RedirectToAction("PriorityChanger", new RouteValueDictionary() { { "ComId", mdl.CommitId.ToString() } });
                }
            }
        }
        
        #endregion
        #endregion

        #region Files
        public ActionResult AddFiles()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            string query = "SELECT Id, FileName, FileSize, Comment FROM PersonFile WHERE PersonId=@PersonId AND IsDeleted=0 order by LoadDate desc";
            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });

            List<AppendedFile> lst =
                (from DataRow rw in tbl.Rows
                 select new AppendedFile() { Id = rw.Field<Guid>("Id"), FileName = rw.Field<string>("FileName"), FileSize = rw.Field<int>("FileSize"), Comment = rw.Field<string>("Comment") })
                .ToList();

            AppendFilesModel model = new AppendFilesModel() { Files = lst, FileTypes = Util.GetPersonFileTypeList() };
            return View(model);
        }

        public ActionResult AddSharedFiles()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return RedirectToAction("Index");

                Util.SetThreadCultureByCookies(Request.Cookies);
                AppendFilesModel model = new AppendFilesModel();
                model.Files = Util.GetFileList(PersonId);
                model.FileTypes = Util.GetPersonFileTypeList();
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult GetFileList()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");
            List<AppendedFile> lstFiles = Util.GetFileList(PersonId);

            return Json(new { IsOk = lstFiles.Count() > 0 ? true : false, Data = lstFiles });
        }

        [HttpPost]
        public ActionResult AddSharedFile()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json(Resources.ServerMessages.EmptyFileError);

            string fileName = Request.Files["File"].FileName;

            int lastSlashPos = 0;
            lastSlashPos = fileName.LastIndexOfAny(new char[] { '\\', '/' });
            if (lastSlashPos > 0)
                fileName = fileName.Substring(lastSlashPos + 1);
            int PersonFileTypeId = Convert.ToInt32(Request.Form["FileTypeId"]);

            string fileComment = Request.Form["Comment"];
           

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
                if (!String.IsNullOrEmpty(fileComment))
                    fileComment += " ";
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
                string query = " INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);" +
                    "\n INSERT INTO PersonFile (Id, PersonId, FileName, FileSize, FileExtention, LoadDate, Comment, MimeType, PersonFileTypeId, FileHash) " +
                    "\n VALUES (@Id, @PersonId, @FileName,  @FileSize, @FileExtention, @LoadDate, @Comment, @MimeType, @PersonFileTypeId, @FileHash);";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", Guid.NewGuid());
                dic.Add("@PersonId", PersonId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", fileComment);
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));
                dic.Add("@PersonFileTypeId", PersonFileTypeId);

                string sFileHash = Util.SHA1Byte(fileData);
                dic.Add("@FileHash", sFileHash);

                Util.AbitDB.ExecuteQuery(query, dic);

                Util.AbitDB.ExecuteQuery(@"update dbo.Application set IsViewed=0 where PersonId=@PersonId", dic);
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }
            if (Request.Form["Stage"] != null)
            {
                string stage = Convert.ToString(Request.Form["Stage"]);
                return RedirectToAction("Index", "Abiturient", new RouteValueDictionary() { { "step", stage } });
            }
            return RedirectToAction("AddSharedFiles");
        }

        [HttpPost]
        public ActionResult AddFile()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("Ошибка авторизации");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json("Файл не приложен или пуст");

            string fileName = Request.Files["File"].FileName;
            int lastSlashPos = 0;
            lastSlashPos = fileName.LastIndexOfAny(new char[] { '\\', '/' });
            if (lastSlashPos > 0)
                fileName = fileName.Substring(lastSlashPos);

            string fileComment = Request.Form["Comment"];

            int PersonFileTypeId = int.Parse(Request.Form["FileTypeId"]);

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
                if (!String.IsNullOrEmpty(fileComment))
                    fileComment += " ";
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
                Guid FileId = Guid.NewGuid();
                string query = "INSERT INTO PersonFile (Id, PersonId, FileName, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, PersonFileTypeId, FileHash) " +
                    " VALUES (@Id, @PersonId, @FileName, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, @PersonFileTypeId, @FileHash);"
                    + "\n INSERT INTO FileStorage(Id, FileData) VALUES (@Id, @FileData);";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", FileId);
                dic.Add("@PersonId", PersonId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@IsReadOnly", false);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", fileComment);
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));
                dic.Add("@PersonFileTypeId", PersonFileTypeId);

                string sFileHash = Util.SHA1Byte(fileData);
                dic.Add("@FileHash", sFileHash);

                Util.AbitDB.ExecuteQuery(query, dic);
                Util.AbitDB.ExecuteQuery(@"update dbo.Application set IsViewed=0 where PersonId=@PersonId", dic);

                query = "INSERT INTO FileStorage (Id, FileData) VALUES (@Id, @FileData)";
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }
            return RedirectToAction("AddFiles");
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

            DataTable tbl = Util.AbitDB.GetDataTable("SELECT FileName, MimeType, FileExtention FROM PersonFile WHERE PersonId=@PersonId AND Id=@Id", slParams);

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

        public ActionResult FilesList(string id)
        {
            Guid PersonId;
            Guid ApplicationId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Content(Resources.ServerMessages.AuthorizationRequired);
            if (!Guid.TryParse(id, out ApplicationId))
                return Content(Resources.ServerMessages.IncorrectGUID);
            string fontspath = Server.MapPath("~/Templates/times.ttf");
            return File(PDFUtils.GetFilesList(PersonId, ApplicationId, fontspath), "application/pdf", "FilesList.pdf");
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

            Guid fileId;
            if (!Guid.TryParse(id, out fileId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID };
                return Json(res);
            }
            string attr = Util.AbitDB.GetStringValue("SELECT IsReadOnly FROM PersonFile WHERE PersonId=@PersonId AND Id=@Id", new SortedList<string, object>() { { "@PersonId", PersonId }, { "@Id", fileId } });
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
                Util.AbitDB.ExecuteQuery("UPDATE PersonFile SET IsDeleted = 1 WHERE PersonId=@PersonId AND Id=@Id", new SortedList<string, object>() { { "@PersonId", PersonId }, { "@Id", fileId } });
            }
            catch
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.ErrorWhileDeleting };
                return Json(res);
            }

            var result = new { IsOk = true, ErrorMessage = "" };
            return Json(result);
        }

        public JsonResult DeleteSharedFile(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired };
                return Json(res);
            }

            Guid fileId;
            if (!Guid.TryParse(id, out fileId))
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID };
                return Json(res);
            }
            string attr = Util.AbitDB.GetStringValue("SELECT ISNULL([IsReadOnly], 'False') FROM PersonFile WHERE PersonId=@PersonId AND Id=@Id", new SortedList<string, object>() { { "@PersonId", PersonId }, { "@Id", fileId } });
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
                Util.AbitDB.ExecuteQuery("UPDATE PersonFile SET IsDeleted = 1 WHERE PersonId=@PersonId AND Id=@Id", new SortedList<string, object>() { { "@PersonId", PersonId }, { "@Id", fileId } });
            }
            catch
            {
                var res = new { IsOk = false, ErrorMessage = Resources.ServerMessages.ErrorWhileDeleting };
                return Json(res);
            }

            var result = new { IsOk = true, ErrorMessage = "" };
            return Json(result);
        }
        #endregion

        public ActionResult MotivatePost()
        {
            string appId = Request.Form["AppId"];
            string mailId = Request.Form["MailId"];
            string mailText = Request.Form["MailText"];

            Guid Id;
            Guid.TryParse(mailId, out Id);
            Guid ApplicationId;
            Guid.TryParse(appId, out ApplicationId);

            string query = "";
            SortedList<string, object> dic = new SortedList<string, object>();
            if (Id == Guid.Empty && ApplicationId == Guid.Empty)
                return RedirectToAction("Main");
            else if (Id == Guid.Empty)
            {
                query = "INSERT INTO MotivateMail (Id, ApplicationId, MailText) VALUES (@Id, @ApplicationId, @MailText)";
                dic.Add("@Id", Guid.NewGuid());
                dic.AddItem("@ApplicationId", ApplicationId);
                dic.AddItem("@MailText", mailText);
            }
            else
            {
                query = "UPDATE MotivateMail SET MailText=@MailText WHERE Id=@Id";
                dic.Add("@Id", Guid.NewGuid());
                dic.AddItem("@MailText", mailText);
            }

            Util.AbitDB.ExecuteQuery(query, dic);

            if (ApplicationId != Guid.Empty)
                return RedirectToAction("Index", "Application", new RouteValueDictionary() { { "id", appId } });
            else
                return RedirectToAction("Main");
        }

        #region RectorScholarship
        public ActionResult NewApplicationRectorScholarship()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");
            else
            {
                NewApplicationRectorScholarshipModel mdl = new NewApplicationRectorScholarshipModel();
                mdl.Files = GetRectorScholarshipFileList(PersonId);
                return View(mdl);
            }
        }

        private List<AppendedFile> GetRectorScholarshipFileList(Guid PersonId)
        {
            string query = "SELECT Id, FileName, FileSize, Comment, IsApproved FROM RectorScholarshipApplicationFile WHERE RectorScholarshipApplicationId=@AppId";
            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@AppId", PersonId } });
            var lFiles =
                (from DataRow row in tbl.Rows
                 select new AppendedFile()
                 {
                     Id = row.Field<Guid>("Id"),
                     FileName = row.Field<string>("FileName"),
                     FileSize = row.Field<int>("FileSize"),
                     Comment = row.Field<string>("Comment"),
                     IsShared = false,
                     IsApproved = row.Field<bool?>("IsApproved").HasValue ?
                        row.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected : ApprovalStatus.NotSet
                 }).ToList();

            query = "SELECT Id, FileName, FileSize, Comment, IsApproved FROM PersonFile WHERE PersonId=@PersonId";
            tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
            var lSharedFiles =
                (from DataRow row in tbl.Rows
                 select new AppendedFile()
                 {
                     Id = row.Field<Guid>("Id"),
                     FileName = row.Field<string>("FileName"),
                     FileSize = row.Field<int>("FileSize"),
                     Comment = row.Field<string>("Comment"),
                     IsShared = true,
                     IsApproved = row.Field<bool?>("IsApproved").HasValue ?
                        row.Field<bool>("IsApproved") ? ApprovalStatus.Approved : ApprovalStatus.Rejected : ApprovalStatus.NotSet
                 }).ToList();

            return lFiles.Union(lSharedFiles).OrderBy(x => x.IsShared).ToList();
        }

        [HttpPost]
        public ActionResult NewApplicationRectorScholarshipAddFile()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn", "Account");

            if (Request.Files["File"] == null || Request.Files["File"].ContentLength == 0 || string.IsNullOrEmpty(Request.Files["File"].FileName))
                return Json(Resources.ServerMessages.EmptyFileError);

            string fileName = Request.Files["File"].FileName;
            string fileComment = Request.Form["Comment"];
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

            try
            {
                string query = "INSERT INTO RectorScholarshipApplicationFile (Id, RectorScholarshipApplicationId, FileName, FileData, FileSize, FileExtention, IsReadOnly, LoadDate, Comment, MimeType, [FileTypeId]) " +
                    " VALUES (@Id, @ApplicationId, @FileName, @FileData, @FileSize, @FileExtention, @IsReadOnly, @LoadDate, @Comment, @MimeType, 1)";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Id", Guid.NewGuid());
                dic.Add("@ApplicationId", PersonId);
                dic.Add("@FileName", fileName);
                dic.Add("@FileData", fileData);
                dic.Add("@FileSize", fileSize);
                dic.Add("@FileExtention", fileext);
                dic.Add("@IsReadOnly", false);
                dic.Add("@LoadDate", DateTime.Now);
                dic.Add("@Comment", fileComment);
                dic.Add("@MimeType", Util.GetMimeFromExtention(fileext));
                Util.AbitDB.ExecuteQuery(query, dic);
            }
            catch
            {
                return Json("Ошибка при записи файла");
            }

            return RedirectToAction("NewApplicationRectorScholarship");
        }

        #endregion

        #region EqualWithRussia
        public ActionResult CheckEqualWithRussia2(EqualWithRussiaModel model)
        {
            return View("CheckEqualWithRussia", model);
        }
        [HttpPost]
        public ActionResult SetEqualWithRussia(EqualWithRussiaModel model)
        {

            string email = model.Email;
            string remixPwd = Util.MD5Str(model.Password);

            model.Errors = "";

            string query = "SELECT Id FROM [User] WHERE Password=@Password AND Email=@Email";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@Password", remixPwd);
            dic.Add("@Email", email);
            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);

            if (tbl.Rows.Count == 0)
            {
                model.Errors = "Неверно введён пароль";
                return CheckEqualWithRussia2(model);
            }
            if (tbl.Rows.Count == 1)
            {
                query = "UPDATE Person SET AbiturientTypeId=1 WHERE Id=@Id";
                Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Id", tbl.Rows[0].Field<Guid?>("Id") } });
                return View("SetEqualWithRussia_Success");
            }

            return CheckEqualWithRussia2(model);
        }

        public ActionResult EqualWithRussiaSendEmail()
        {
            string query = @"
SELECT [User].Email
  FROM [OnlinePriem2012].[dbo].[Person]
  INNER JOIN [OnlinePriem2012].[dbo].[User] ON [User].Id = Person.Id
  INNER JOIN [OnlinePriem2012].[dbo].Country ON Country.Id = Person.NationalityId
  WHERE Country.PriemDictionaryId IN (5,7,8,9,10,11,12,13,14,15) AND Person.AbiturientTypeId = 2";
            DataTable tbl = Util.AbitDB.GetDataTable(query, null);
            SortedList<string, object> dic = new SortedList<string, object>();
            foreach (DataRow rw in tbl.Rows)
            {
                string email = rw.Field<string>("Email");
                string body = string.Format(Util.GetMailBody(Server.MapPath("~/Templates/EmailBodyEqualWithRussia.eml")),
                        Util.ServerAddress + Url.Action("CheckEqualWithRussia", "Abiturient", new RouteValueDictionary() { { "email", email } }));
                try
                {
                    MailMessage msg = new MailMessage();
                    msg.To.Add(email);
                    msg.Body = body;
                    msg.Subject = "Приёмная комиссия СПбГУ - участие в равном конкурсе с гражданами РФ";
                    SmtpClient client = new SmtpClient();
                    client.Send(msg);
                    query = "INSERT INTO User_SentEmails([From], [Email], [Text]) VALUES (@From, @Email, @Text)";
                    dic.Clear();
                    dic.Add("@From", "no-reply@spb.edu");
                    dic.Add("@Email", email);
                    dic.Add("@Text", msg.Body);
                    Util.AbitDB.ExecuteQuery(query, dic);
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception exc)
                {
                    try
                    {
                        query = "INSERT INTO User_SentEmails([From], [Email], [Text], [FailStatus]) VALUES (@From, @Email, @Text, @FailStatus)";
                        dic.Clear();
                        dic.Add("@From", "no-reply@spb.edu");
                        dic.Add("@Email", email);
                        dic.Add("@Text", body);
                        dic.Add("@FailStatus", exc.Message);
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }
                    catch { }//вдруг база сломалась, тогда всё, не залогировать никак
                }
            }
            return View("Main");
        }

        public ActionResult CheckEqualWithRussia(string email)
        {
            EqualWithRussiaModel model = new EqualWithRussiaModel();
            model.Email = email;
            return View(model);
        }
        #endregion

        #region Ajax

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetFacs(string studyform, string studybasis, string entry)
        {
            int iStudyFormId;
            int iStudyBasisId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;
            int iEntryId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            string query = string.Format("SELECT DISTINCT FacultyId, FacultyName FROM {0} WHERE StudyFormId=@StudyFormId AND StudyBasisId=@StudyBasisId " +
                "AND IsSecond=@IsSecond ORDER BY FacultyId", iEntryId == 2 ? "extStudyPlan" : "extStudyPlan1K");
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@IsSecond", iEntryId == 3 ? true : false);

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var facs =
                from DataRow rw in tbl.Rows
                select new { Id = rw.Field<int>("FacultyId"), Name = rw.Field<string>("FacultyName") };
            return Json(facs);
        }

        public ActionResult GetLicenseProgramList(string slId)
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyLevelId;
            if (!int.TryParse(slId, out iStudyLevelId))
                iStudyLevelId = 1;

            string query = "SELECT DISTINCT LicenseProgramId, LicenseProgramCode, LicenseProgramName FROM Entry WHERE StudyLevelId=@StudyLevelId ORDER BY LicenseProgramCode, LicenseProgramName";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyLevelId", iStudyLevelId);

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var profs =
                (from DataRow rw in tbl.Rows
                 select new
                 {
                     Id = rw.Field<int>("LicenseProgramId"),
                     Name = "(" + rw.Field<string>("LicenseProgramCode") + ") " + rw.Field<string>("LicenseProgramName")
                 }).OrderBy(x => x.Name);
            return Json(profs);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetProfsAll(string studyform, string studybasis, string entry, string isSecond = "0", string isParallel = "0",
            string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            int iEntryId = 1;
            int iSemesterId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            if (iEntryId == 8 || iEntryId == 10)
            {
                iEntryId = 3;
            }
            if (iEntryId == 16)
            {
                iEntryId = 1;
            }
            if (iEntryId == 18)
            {
                iEntryId = 1;
            }
            if (iEntryId == 17)
            {
                iEntryId = 2;
            }
            if (iEntryId == 15)
            {
                iEntryId = 4;
            }
            if (iEntryId == 1001)
            {
                iEntryId = 6;
            }
            if (iEntryId == 1002)
            {
                iEntryId = 6;
            }
            if (iEntryId == 1003)
            {
                iEntryId = 7;
            }
            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1;

            string query = @"SELECT DISTINCT LP.Id, LP.Code, LP.Name, LP.NameEng FROM SP_LicenseProgram LP 
INNER JOIN SP_StudyLevel SL ON LP.StudyLevelId = SL.Id 
INNER JOIN SP_StudyPlanHelp HLP ON HLP.LicenseProgramId = LP.Id
WHERE StudyLevelGroupId=@StudyLevelGroupId AND HLP.CampaignYear=@CampaignYear AND HLP.StudyFormId=@StudyFormId AND HLP.SemesterId=@SemesterId";

            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyLevelGroupId", iEntryId);//2 == mag, 1 == 1kurs, 3 - SPO, 4 - аспирант
            dic.Add("@CampaignYear", Util.iPriemYear);
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@SemesterId", iSemesterId);
            bool isEng = Util.GetCurrentThreadLanguageIsEng();

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var profs =
                (from DataRow rw in tbl.Rows
                 select new
                 {
                     Id = rw.Field<int>("Id"),
                     Name = (String.IsNullOrEmpty(rw.Field<string>("Code")) ? "" : "(" + rw.Field<string>("Code") + ") ") +
                        (isEng ? (string.IsNullOrEmpty(rw.Field<string>("NameEng")) ? rw.Field<string>("Name") : rw.Field<string>("NameEng")) : rw.Field<string>("Name"))
                 }).OrderBy(x => x.Name);

            if (profs.Count() == 0)
            {
                return Json(new { NoFree = true });
            }

            return Json(profs);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetProfs(string studyform, string studybasis, string entry, string isSecond = "0", string isParallel = "0", 
            string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            int iEntryId = 1;
            int iSemesterId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            int iStudyLevelId = 0;
            
            if (iEntryId == 8 || iEntryId == 10)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 3;
            }
            if (iEntryId == 16)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 1;
            }
            if (iEntryId == 18)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 1;
            }
            if (iEntryId == 17)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 2;
            }
            if (iEntryId == 15)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 4;
            }
            if (iEntryId == 1001)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 6;
            }
            if (iEntryId == 1002)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 6;
            }
            if (iEntryId == 1003)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 7;
            }
            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1;

            bool bIsSecond = isSecond == "1" ? true : false;
            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;
            int GosLine = Util.IsGosLine(PersonId);

            string query = "SELECT DISTINCT LicenseProgramId, LicenseProgramCode, LicenseProgramName, LicenseProgramNameEng FROM Entry " +
                "WHERE StudyFormId=@StudyFormId AND StudyBasisId=@StudyBasisId AND StudyLevelGroupId=@StudyLevelGroupId AND IsSecond=@IsSecond AND IsParallel=@IsParallel " +
                "AND IsReduced=@IsReduced AND [CampaignYear]=@Year AND SemesterId=@SemesterId" +
                (GosLine == 0 ? " AND IsForeign = 0 " : "");

            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@StudyLevelGroupId", iEntryId);//2 == mag, 1 == 1kurs, 3 - SPO, 4 - аспирант
            if (iStudyLevelId != 0)
            {
                query += " AND StudyLevelId=@StudyLevelId";
                dic.Add("@StudyLevelId", iStudyLevelId);//Id=8 - 9kl, Id=10 - 11 kl
            }
            dic.Add("@IsSecond", bIsSecond);
            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@Year", Util.iPriemYear);
            dic.Add("@SemesterId", iSemesterId);

            bool isEng = Util.GetCurrentThreadLanguageIsEng();

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var profs =
                (from DataRow rw in tbl.Rows
                 select new
                 {
                     Id = rw.Field<int>("LicenseProgramId"),
                     Name = (String.IsNullOrEmpty(rw.Field<string>("LicenseProgramCode")) ? "" : "(" + rw.Field<string>("LicenseProgramCode") + ") ") + 
                        (isEng ?
                          (string.IsNullOrEmpty(rw.Field<string>("LicenseProgramNameEng")) ? rw.Field<string>("LicenseProgramName") : rw.Field<string>("LicenseProgramNameEng")) 
                          : rw.Field<string>("LicenseProgramName"))
                 }).OrderBy(x => x.Name);

            if (profs.Count() == 0)
            {
                return Json(new { NoFree = true });
            }

            return Json(profs);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetObrazPrograms(string prof, string studyform, string studybasis, string entry, string isParallel = "0", string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;

            int iEntryId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            int iStudyLevelId = 0;
            if (iEntryId == 8 || iEntryId == 10)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 3;
            }
            if (iEntryId == 1001 || iEntryId == 1002)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 6;
            }
            if (iEntryId == 1003 )
            {
                iStudyLevelId = iEntryId;
                iEntryId = 7;
            }
            int iSemesterId;
            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1;
            int iProfessionId = 1;
            if (!int.TryParse(prof, out iProfessionId))
                iProfessionId = 1;

            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;

            int GosLine = Util.IsGosLine(PersonId);

            string query = "SELECT DISTINCT ObrazProgramId, ObrazProgramName, ObrazProgramNameEng FROM Entry " +
                "WHERE StudyFormId=@StudyFormId AND StudyBasisId=@StudyBasisId AND LicenseProgramId=@LicenseProgramId " +
                "AND StudyLevelGroupId=@StudyLevelGroupId AND IsParallel=@IsParallel AND IsReduced=@IsReduced " +
                "AND CampaignYear=@Year AND SemesterId=@SemesterId" +
                (GosLine == 0 ? " AND IsForeign = 0 " : "");
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@LicenseProgramId", iProfessionId);
            dic.Add("@StudyLevelGroupId", iEntryId);
            if (iStudyLevelId != 0)
            {
                query += " AND StudyLevelId=@StudyLevelId";
                dic.Add("@StudyLevelId", iStudyLevelId);//Id=8 - 9kl, Id=10 - 11 kl
            }
            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@Year", Util.iPriemYear);
            dic.Add("@SemesterId", iSemesterId);

            bool isEng = Util.GetCurrentThreadLanguageIsEng();

            query += " ORDER BY ObrazProgramName, ObrazProgramNameEng";

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var OPs = from DataRow rw in tbl.Rows
                      select new
                      {
                          Id = rw.Field<int>("ObrazProgramId"),
                          Name = isEng ?
                            (string.IsNullOrEmpty(rw.Field<string>("ObrazProgramNameEng")) ? rw.Field<string>("ObrazProgramName") : rw.Field<string>("ObrazProgramNameEng"))
                            : rw.Field<string>("ObrazProgramName")
                      };

            return Json(new { NoFree = OPs.Count() > 0 ? false : true, List = OPs });
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetSpecializations(string prof, string obrazprogram, string studyform, string studybasis, string entry, string CommitId, string isParallel = "0", string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;
            int iEntryId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            int iStudyLevelId = 0;
            if (iEntryId == 8 || iEntryId == 10)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 3;
            }
            if (iEntryId == 1001 || iEntryId == 1002)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 6;
            }
            if (iEntryId == 1003)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 7;
            }

            int iProfessionId = 1;
            if (!int.TryParse(prof, out iProfessionId))
                iProfessionId = 1;
            int iObrazProgramId = 1;
            if (!int.TryParse(obrazprogram, out iObrazProgramId))
                iObrazProgramId = 1;
            int iSemesterId;
            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1; 

            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;
            //bool bIsGosLine = isgosline == "1" ? true : false;

            int GosLine = Util.IsGosLine(PersonId);

            string query = "SELECT DISTINCT ProfileId, (case when (ProfileName is null) then 'нет' else ProfileName end) as 'ProfileName' FROM Entry WHERE StudyFormId=@StudyFormId " +
                "AND StudyBasisId=@StudyBasisId AND LicenseProgramId=@LicenseProgramId AND ObrazProgramId=@ObrazProgramId AND StudyLevelGroupId=@StudyLevelGroupId " +
                //"AND Entry.Id NOT IN (SELECT EntryId FROM [Application] WHERE PersonId=@PersonId AND IsCommited='True' AND EntryId IS NOT NULL and CommitId=@CommitId and IsDeleted=0 and IsGosLine<>@IsGosLine) " +
                "AND IsParallel=@IsParallel AND IsReduced=@IsReduced "+ 
                "AND CampaignYear=@Year AND SemesterId=@SemesterId " + 
                (GosLine == 0 ? " AND IsForeign = 0 " : "");

            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@PersonId", PersonId);
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@LicenseProgramId", iProfessionId);
            dic.Add("@ObrazProgramId", iObrazProgramId);
            dic.Add("@StudyLevelGroupId", iEntryId);
           // dic.Add("@IsGosLine", bIsGosLine);
            if (iStudyLevelId != 0)
            {
                query += " AND StudyLevelId=@StudyLevelId";
                dic.Add("@StudyLevelId", iStudyLevelId);//Id=8 - 9kl, Id=10 - 11 kl
            }
            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@Year", Util.iPriemYear);
            dic.Add("@SemesterId", iSemesterId); 
            dic.Add("@CommitId", Guid.Parse(CommitId));

            DataTable tblSpecs = Util.AbitDB.GetDataTable(query, dic);
            var Specs =
                from DataRow rw in tblSpecs.Rows
                select new { SpecId = rw.Field<int?>("ProfileId"), SpecName = rw.Field<string>("ProfileName") };

            var ret = new
            {
                NoFree = Specs.Count() == 0 ? true : false,
                List = Specs.Select(x => new { Id = x.SpecId, Name = x.SpecName }).ToList()
            };

            
            int Crimea = Util.IsCrimea(PersonId);
            return Json(new { ret, GosLine, Crimea });
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetAbitCertsAndExams()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json("", JsonRequestBehavior.AllowGet);

            string query = "SELECT DISTINCT Number FROM EgeCertificate WHERE PersonId=@PersonId";
            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
            List<string> certs = (from DataRow rw in tbl.Rows
                                  select rw.Field<string>("Number")).ToList();

            query = "SELECT EgeExam.Id, EgeExam.Name FROM EgeCertificate INNER JOIN EgeMark ON EgeMark.EgeCertificateId=EgeCertificate.Id " +
                " INNER JOIN EgeExam ON EgeExam.Id=EgeMark.EgeExamId WHERE EgeCertificate.PersonId=@PersonId";
            tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });

            List<KeyValuePair<int, string>> exams =
                Util.EgeExamsAll.Except(
                (from DataRow rw in tbl.Rows
                 select new { Id = rw.Field<int>("Id"), Name = rw.Field<string>("Name") }).
                 ToDictionary(x => x.Id, y => y.Name)).ToList();

            var result = new { Certs = certs, Exams = exams };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult AddMark(string examName, string examValue, string IsInUniversity, string IsSecondWave)
        {

            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка авторизации" };
                return Json(result);
            }

            int iExamId = 0;
            if (!int.TryParse(examName, out iExamId))
                iExamId = 0;

            int iExamValue = 0;
            if (!int.TryParse(examValue, out iExamValue))
                iExamValue = 0;

           
            bool bIsInUniversity = (IsInUniversity == "true");  
           /* if (bool.TryParse(IsInUniversity, out bIsInUniversity))
                bIsInUniversity = false;*/

            bool bIsSecondWave = (IsSecondWave == "true");
            /*if (bool.TryParse(IsSecondWave, out bIsSecondWave))
                bIsSecondWave = false;*/
             
            SortedList<string, object> dic = new SortedList<string, object>();
            Guid EgeCertificateId = Guid.Empty;
            
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
            
                //номер должен быть уникальным
               /*if (certs.Count() > 1)
                    return Json(new { IsOk = false, ErrorMessage = "Данный сертификат в базе данных принадлежит другому лицу" });//Это косяк, двух не может быть!!!
                if (certs.Count() == 1)
                {
                    if (certs[0].PersonId != PersonId)
                        return Json(new { IsOk = false, ErrorMessage = "Данный сертификат в базе данных принадлежит другому лицу" });
                    else
                        EgeCertificateId = certs[0].Id;
                }*/

                //query = "SELECT EgeMark.Value FROM EgeMark INNER JOIN EgeCertificate ON EgeCertificate.Id=EgeMark.EgeCertificateId " +
                //    " WHERE EgeCertificate.PersonId=@PersonId AND EgeMark.EgeExamId=@ExamId";
                //dic.Clear();
                //dic.Add("@PersonId", PersonId);
                //dic.Add("@ExamId", iExamId);
                //string MarkVal = Util.AbitDB.GetStringValue(query, dic);

                string MarkVal = context.EgeMark.Where(x => x.EgeCertificate.PersonId == PersonId && x.EgeExamId == iExamId)
                    .Select(x => new { x.IsInUniversity, x.IsSecondWave, x.Value })
                    .ToList()
                    .Select(x => x.IsSecondWave ? "Сдаю во второй волне" : (x.IsInUniversity ? "Сдаю в СПбГУ" : x.Value.ToString()))
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(MarkVal) && MarkVal != "0")
                    return Json(new { IsOk = false, ErrorMessage = "Оценка по данному предмету уже введена" });

                try
                {
                    if (EgeCertificateId == Guid.Empty)
                    {
                        EgeCertificateId = Guid.NewGuid();
                        context.EgeCertificate.Add(new EgeCertificate()
                        {
                            Id = EgeCertificateId,
                            Is2014 = false,
                            Number = "нет свидетельства",
                            PersonId = PersonId
                        });
                        context.SaveChanges();
                    }

                    Guid MarkId = Guid.NewGuid();
                    context.EgeMark.Add(new EgeMark()
                    {
                        Id = MarkId,
                        EgeCertificateId = EgeCertificateId,
                        EgeExamId = iExamId,
                        Value = iExamValue,
                        IsInUniversity = bIsInUniversity,
                        IsSecondWave = bIsSecondWave
                    });
                    context.SaveChanges();
                    string exName = Util.EgeExamsAll.ContainsKey(iExamId) ? Util.EgeExamsAll[iExamId] : "";
                    string exValue = "";
                    if (bIsSecondWave)
                        exValue = "Сдаю во второй волне";
                    else if (bIsInUniversity)
                        exValue = "Сдаю в СПбГУ";
                    else
                        exValue = iExamValue.ToString();
                    
                    var res = new
                    {
                        IsOk = true,
                        Data = new
                        {
                            Id = MarkId.ToString(),
                            ExamName = exName,
                            ExamMark = exValue//iExamValue.ToString()
                        },
                        ErrorMessage = ""
                    };
                    return Json(res);
                }

                catch
                {
                    return Json(new { IsOk = false, ErrorMessage = "Ошибка при сохранении оценки." });
                }
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult DeleteEgeMark()
        {
            string mId = Request.Params["mId"];
            Guid id;
            if (!Util.CheckAuthCookies(Request.Cookies, out id))
            {
                var result = new { IsOk = false, ErrorMsg = "Ошибка авторизации" };
                return Json(result);
            }

            Guid markId;
            if (!Guid.TryParse(mId, out markId))
            {
                var result = new { IsOk = false, ErrorMsg = "Некорректный идентификатор" };
                return Json(result);
            }

            try
            {
                Util.AbitDB.ExecuteQuery("DELETE FROM EgeMark WHERE Id=@Id", new SortedList<string, object>() { { "@Id", markId } });
                var res = new { IsOk = true, ErrorMsg = "" };
                return Json(res);
            }
            catch
            {
                var result = new { IsOk = false, ErrorMsg = "Ошибка при обновлении" };
                return Json(result);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult UpdateScienceWorks(string ScWorkInfo, string ScWorkType, string ScWorkYear)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var result = new { IsOk = false, ErrorMsg = "Ошибка авторизации" };
                return Json(result);
            }

            int iScWorkType = 1;
            if (!(int.TryParse(ScWorkType, out iScWorkType)))
                iScWorkType = 1;

            Guid wrkId = Guid.NewGuid();

            string query = "INSERT INTO PersonScienceWork (Id, PersonId, WorkTypeId, WorkInfo, WorkYear) VALUES (@Id, @PersonId, @WorkTypeId, @WorkInfo, @WorkYear)";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@Id", wrkId);
            dic.Add("@PersonId", PersonId);
            dic.Add("@WorkTypeId", iScWorkType);
            dic.Add("@WorkInfo", ScWorkInfo);
            dic.Add("@WorkYear", ScWorkYear);
            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            try
            {
                Util.AbitDB.ExecuteQuery(query, dic);
                string scType = !bIsEng ? Util.ScienceWorkTypeAll[iScWorkType] : Util.ScienceWorkTypeEngAll[iScWorkType];
                string scInfo = HttpUtility.HtmlEncode(ScWorkInfo);
                string scYear = HttpUtility.HtmlEncode(ScWorkYear);
                var res = new { IsOk = true, Data = new { Id = wrkId.ToString("N"), Type = scType, Info = scInfo, Year = scYear }, ErrorMsg = "" };
                return Json(res);
            }
            catch
            {
                var result = new { IsOk = false, ErrorMsg = "Ошибка при сохранении данных" };
                return Json(result);
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult DeleteScienceWorks(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка авторизации" };
                return Json(result);
            }

            Guid wrkId = Guid.Empty;
            if (!Guid.TryParse(id, out wrkId))
            {
                var result = new { IsOk = false, ErrorMessage = "Некорректный идентификатор" };
                return Json(result);
            }
            try
            {
                Util.AbitDB.ExecuteQuery("DELETE FROM PersonScienceWork WHERE Id=@Id", new SortedList<string, object>() { { "@Id", wrkId } });
                DataTable tbl = Util.AbitDB.GetDataTable("SELECT count(Id) as cnt FROM PersonScienceWork WHERE PersonId=@Id", new SortedList<string, object>() { { "@Id", PersonId } });
                var res = new { IsOk = true, Count = tbl.Rows[0].Field<int>("cnt"), ErrorMessage = "" }; 
                return Json(res);
            }
            catch
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка при обновлении данных" };
                return Json(result);
            }
            //using (AbitDB db = new AbitDB())
            //{
            //    PersonScienceWork psw = 
            //        db.PersonScienceWork.Where(x => x.Id == wrkId && x.PersonId == PersonId).DefaultIfEmpty(null).First();

            //    if (psw == null)
            //    {
            //        var result = new { IsOk = false, ErrorMessage = "Запись не найдена" };
            //        return Json(result);
            //    }

            //    try
            //    {
            //        db.PersonScienceWork.DeleteObject(psw);
            //        db.SaveChanges();
            //    }
            //    catch
            //    {
            //        var result = new { IsOk = false, ErrorMessage = "Ошибка при обновлении данных" };
            //        return Json(result);
            //    }

            //    var res = new { IsOk = true, ErrorMessage = "" };
            //    return Json(res);
            //}
        }

        public JsonResult LoadVuzNames(string schoolType)
        {
            int iSchoolType;
            int.TryParse(schoolType, out iSchoolType);
            string query = @"SELECT SchoolName, count(SchoolName) as cnt 
FROM EducationDocument 
WHERE SchoolName IS NOT NULL AND SchoolTypeId=@SchTypeId 
group by SchoolName
Order by cnt desc";
            DataTable tbl = Util.StudDB.GetDataTable(query, new SortedList<string, object>() { { "@SchTypeId", iSchoolType } });
            List<string> vals =
                (from DataRow rw in tbl.Rows
                 select rw.Field<string>("SchoolName")).ToList();
            return Json(new { IsOk = true, Values = vals });
        }

        public ActionResult AddWorkPlace(string WorkStag, string WorkPlace, string WorkProf, string WorkSpec)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка авторизации" };
                return Json(result);
            }

            Guid workId = Guid.NewGuid();
            string query = "INSERT INTO PersonWork(Id, PersonId, Stage, WorkPlace, WorkProfession, WorkSpecifications) " +
                " VALUES (@Id, @PersonId, @Stage, @WorkPlace, @WorkProfession, @WorkSpecifications)";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@Id", workId);
            dic.Add("@PersonId", PersonId);
            dic.Add("@Stage", WorkStag);
            dic.Add("@WorkPlace", WorkPlace);
            dic.Add("@WorkProfession", WorkProf);
            dic.Add("@WorkSpecifications", WorkSpec);

            try
            {
                Util.AbitDB.ExecuteQuery(query, dic);
                var res = new
                {
                    IsOk = true,
                    Data = new { Id = workId.ToString("N"), Place = WorkPlace, Stag = WorkStag, Level = WorkProf, Duties = WorkSpec },
                    ErrorMessage = ""
                };
                return Json(res);
            }
            catch
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка при сохранении данных" };
                return Json(result);
            }
        }

        public ActionResult DeleteWorkPlace(string wrkId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка авторизации" };
                return Json(result);
            }

            Guid workId = new Guid();
            if (!Guid.TryParse(wrkId, out workId))
            {
                var result = new { IsOk = false, ErrorMessage = "Некорректный идентификатор" };
                return Json(result);
            }

            try
            {
                Util.AbitDB.ExecuteQuery("DELETE FROM PersonWork WHERE Id=@Id", new SortedList<string, object>() { { "@Id", workId } });
                DataTable tbl = Util.AbitDB.GetDataTable("SELECT count(Id) as cnt FROM PersonWork WHERE PersonId=@Id", new SortedList<string, object>() { { "@Id", PersonId } });
                var res = new { IsOk = true, Count = tbl.Rows[0].Field<int>("cnt"), ErrorMessage = "" };  
                return Json(res);
            }
            catch
            {
                var result = new { IsOk = false, ErrorMessage = "Ошибка при обновлении" };
                return Json(result);
            }

            //try
            //{
            //    PersonWork pw = Util.ABDB.PersonWork.Where(x => x.Id == workId && x.PersonId == PersonId).DefaultIfEmpty(null).First();
            //    if (pw == null)
            //    {
            //        var result = new { IsOk = false, ErrorMessage = "Запись не найдена" };
            //        return Json(result);
            //    }
            //    Util.ABDB.PersonWork.DeleteObject(pw);
            //    Util.ABDB.SaveChanges();
            //    var res = new { IsOk = true, ErrorMessage = "" };
            //    return Json(res);
            //}
            //catch
            //{
            //    var result = new { IsOk = false, ErrorMessage = "Ошибка при обновлении" };
            //    return Json(result);
            //}
        }

        public ActionResult DeleteMsg(string id)
        {
            if (id == "0")//system messages
                return Json(new { IsOk = true });

            Guid MessageId;
            if (!Guid.TryParse(id, out MessageId))
                return Json(new { IsOk = false, ErrorMessage = "" });

            string query = "UPDATE PersonalMessage SET IsRead=@IsRead WHERE Id=@Id";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@IsRead", true);
            dic.Add("@Id", MessageId);

            try
            {
                Util.AbitDB.ExecuteQuery(query, dic);
            }
            catch (Exception e)
            {
                return Json(new { IsOk = false, ErrorMessage = e.Message });//
            }

            return Json(new { IsOk = true });
        }

        #region KLADR
        public JsonResult GetCityNames(string regionId)
        {
            try
            {
                string sRegionKladrCode = Util.GetRegionKladrCodeByRegionId(regionId);
                var towns = Util.GetCityListByRegion(sRegionKladrCode);

                return Json(new { IsOk = true, List = towns });
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = "Ошибка при выполнении запроса. Попробуйте обновить страницу" });
            }
        }
        public JsonResult GetStreetNames(string regionId, string cityName)
        {
            try
            {
                string sRegionKladrCode = Util.GetRegionKladrCodeByRegionId(regionId);
                var streets = Util.GetStreetListByRegion(sRegionKladrCode, cityName);

                return Json(new { IsOk = true, List = streets });
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = "Ошибка при выполнении запроса. Попробуйте обновить страницу" });
            }
        }
        public JsonResult GetHouseNames(string regionId, string cityName, string streetName)
        {
            try
            {
                string sRegionKladrCode = Util.GetRegionKladrCodeByRegionId(regionId);
                var streets = Util.GetHouseListByStreet(sRegionKladrCode, cityName, streetName);

                return Json(new { IsOk = true, List = streets.Distinct().ToList() });
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = "Ошибка при выполнении запроса. Попробуйте обновить страницу" });
            }
        }
        public JsonResult GetPostIndexByAddres(string regionId, string cityName, string streetName, string houseName)
        {
            try
            {
                string sRegionKladrCode = Util.GetRegionKladrCodeByRegionId(regionId);
                var index = Util.GetPostIndexByAddress(sRegionKladrCode, cityName, streetName, houseName);

                return Json(new { IsOk = true, Index = index });
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = "Ошибка при выполнении запроса. Попробуйте обновить страницу" });
            }
        }
        #endregion

        #region Certificates_AJAX
        public JsonResult GetTypeCertificate(string TypeId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });
            int iTypeId;
            if (!int.TryParse(TypeId, out iTypeId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var Cert = context.LanguageCertificatesType.Where(x => x.Id == iTypeId).Select(x => new { x.BoolType, x.ValueType }).FirstOrDefault();
                if (Cert == null)
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });
                else  return Json(new { IsOk = true, IsBool = Cert.BoolType });
            }

        }
        public JsonResult AddCertificates(string TypeId, string Number, string BoolType, string Value)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            bool bIsEng = Util.GetCurrentThreadLanguageIsEng();

            int iTypeId;
            if (!int.TryParse(TypeId, out iTypeId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            bool IsBoolType;
            if (BoolType == "1") IsBoolType = true;
            else if (BoolType == "0") IsBoolType = false;
            else return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            double iValue;
            if (!double.TryParse(Value.Replace('.', ','), out iValue) && !IsBoolType)
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var Cert = new PersonLanguageCertificates()
                {
                    PersonId = PersonId,
                    LanguageCertificateTypeId = iTypeId,
                    Number = Number,
                    ResultBool = IsBoolType ? (bool?)true : null,
                    ResultValue = !IsBoolType ? (double?)iValue : null,
                };
                context.PersonLanguageCertificates.Add(Cert);
                context.SaveChanges();

                var Ol = context.PersonLanguageCertificates.Where(x => x.Id == Cert.Id).Select(x => new
                {
                    Id = x.Id,
                    Name = !bIsEng ? x.LanguageCertificatesType.Name : x.LanguageCertificatesType.NameEng,
                    IsBool = x.LanguageCertificatesType.BoolType,
                    Number = x.Number,
                    Value = x.ResultValue,
                }).FirstOrDefault();

                return Json(new
                {
                    Id = Ol.Id,
                    IsOk = true,
                    Name = Ol.Name,
                    IsBool = Ol.IsBool,
                    Number = Ol.Number,
                    Value = Ol.Value,
                });
            }
        }
        public JsonResult DeleteCertificate(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            int Certid;
            if (!int.TryParse(id, out Certid))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            string query = "DELETE FROM PersonLanguageCertificates WHERE Id=@Id";
            Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Id", Certid } });
            DataTable tbl = Util.AbitDB.GetDataTable("SELECT count(Id) as cnt FROM PersonLanguageCertificates WHERE PersonId=@Id", new SortedList<string, object>() { { "@Id", PersonId } });
            return Json(new { IsOk = true, Count = tbl.Rows[0].Field<int>("cnt") });
        }
        #endregion
        #region Olympiads_AJAX
        public JsonResult GetOlympTypeList(string OlympYear)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            int iOlympYear;
            if (!int.TryParse(OlympYear, out iOlympYear))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var OlData = context.OlympBook.Where(x => x.OlympYear == iOlympYear).Select(x => new { Id = x.OlympTypeId, Name = x.OlympType.Name }).Distinct().ToList();

                return Json(new
                {
                    IsOk = true,
                    List = OlData
                });
            }
        }
        public JsonResult GetOlympNameList(string OlympTypeId, string OlympYear)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            int iOlympTypeId;
            if (!int.TryParse(OlympTypeId, out iOlympTypeId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });
            int iOlympYear;
            if (!int.TryParse(OlympYear, out iOlympYear))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var OlData = context.OlympBook.Where(x => x.OlympTypeId == iOlympTypeId && x.OlympYear == iOlympYear).Select(x => new { Id = x.OlympNameId, Name = x.OlympName.Name }).Distinct().ToList();

                return Json(new
                {
                    IsOk = true,
                    List = OlData
                });
            }
        }
        public JsonResult GetOlympSubjectList(string OlympTypeId, string OlympNameId, string OlympYear)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            int iOlympTypeId;
            if (!int.TryParse(OlympTypeId, out iOlympTypeId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iOlympNameId;
            if (!int.TryParse(OlympNameId, out iOlympNameId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iOlympYear;
            if (!int.TryParse(OlympYear, out iOlympYear))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var OlData = context.OlympBook.Where(x => x.OlympTypeId == iOlympTypeId && x.OlympNameId == iOlympNameId && x.OlympYear == iOlympYear)
                    .Select(x => new { Id = x.OlympSubjectId, Name = x.OlympSubject.Name }).Distinct().ToList();

                return Json(new
                {
                    IsOk = true,
                    List = OlData
                });
            }
        }

        public JsonResult AddOlympiad(string OlympYear, string OlympTypeId, string OlympNameId, string OlympSubjectId, string OlympValueId, string Series, string Number, string Date)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            int iOlympYear;
            if (!int.TryParse(OlympYear, out iOlympYear))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iOlympTypeId;
            if (!int.TryParse(OlympTypeId, out iOlympTypeId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iOlympNameId;
            if (!int.TryParse(OlympNameId, out iOlympNameId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iOlympSubjectId;
            if (!int.TryParse(OlympSubjectId, out iOlympSubjectId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iOlympValueId;
            if (!int.TryParse(OlympValueId, out iOlympValueId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            DateTime _date;
            DateTime? dtDate;
            if (!DateTime.TryParse(Date, out _date))
                dtDate = null;
            else
                dtDate = _date;


            Guid Id = Guid.NewGuid();
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                context.Olympiads.Add(new Olympiads()
                {
                    Id = Id,
                    OlympYear = iOlympYear,
                    OlympNameId = iOlympNameId,
                    OlympSubjectId = iOlympSubjectId,
                    OlympTypeId = iOlympTypeId,
                    OlympValueId = iOlympValueId,
                    DocumentSeries = Series,
                    DocumentNumber = Number,
                    DocumentDate = dtDate,
                    PersonId = PersonId
                });
                context.SaveChanges();

                var Ol = context.Olympiads.Where(x => x.Id == Id).Select(x => new
                {
                    OlympName = x.OlympName.Name,
                    OlympSubject = x.OlympSubject.Name,
                    OlympType = x.OlympType.Name,
                    OlympValue = x.OlympValue.Name,
                    OlympYear = x.OlympYear
                }).FirstOrDefault();

                return Json(new
                {
                    IsOk = true,
                    Id = Id.ToString("N"),
                    Type = Ol.OlympType,
                    Name = Ol.OlympName,
                    Subject = Ol.OlympSubject,
                    Value = Ol.OlympValue,
                    Year = Ol.OlympYear,
                    Doc = Series + " " + Number + " от " + (dtDate.HasValue ? dtDate.Value.ToShortDateString() : "-")
                });
            }
        }
        public JsonResult DeleteOlympiad(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid OlympId;
            if (!Guid.TryParse(id, out OlympId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            string query = "DELETE FROM Olympiads WHERE Id=@Id"; 
            Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Id", OlympId } });
            DataTable tbl = Util.AbitDB.GetDataTable("SELECT count(Id) as cnt FROM Olympiads WHERE PersonId=@Id", new SortedList<string, object>() { { "@Id", PersonId } });
            return Json(new { IsOk = true, Count = tbl.Rows[0].Field<int>("cnt")});
        }
        #endregion

        #region AG_Applications_OLD
        /*[OutputCache(NoStore = true, Duration = 0)]
        public JsonResult GetProfs_AG(string classid, string profileId, string CommitId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false });

            int iEntryClassId;
            int.TryParse(classid, out iEntryClassId);

            int iProgramId;
            int.TryParse(profileId, out iProgramId);

            string query = "SELECT DISTINCT ProfileId, ProfileName FROM AG_qEntry WHERE EntryClassId=@ClassId AND ProgramId=@ProgramId AND " +
                " Id NOT IN (SELECT EntryId FROM [AG_Application] WHERE PersonId=@PersonId AND Enabled='False')";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@PersonId", PersonId);
            dic.Add("@ClassId", iEntryClassId);
            dic.Add("@ProgramId", iProgramId);
            
            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var res = (from DataRow rw in tbl.Rows
                       select new
                       {
                           Id = rw.Field<int>("ProfileId"),
                           Name = rw.Field<string>("ProfileName")
                       });
            return Json(new { IsOk = true, Vals = res });
        }
        */
        /* [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult GetSpecializations_AG(string classid, string programid, string profileid, string CommitId)
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iEntryClassId = 0;
            int iProgramId = 0;
            int iProfileId = 0;

            if (!int.TryParse(classid, out iEntryClassId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });
            if (!int.TryParse(programid, out iProgramId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });
            if (!int.TryParse(profileid, out iProfileId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {

                int cnt = context.AG_Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId).Count();
                if (cnt >= 2)
                    return Json(new { IsOk = false, ErrorMessage = "У абитуриента уже имеется 2 активных заявления" });

                //cnt = context.AG_Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId && x.AG_Entry.ProgramId != iProgramId).Count();
                //if (cnt > 0)
                //    return Json(new { IsOk = false, ErrorMessage = "У абитуриента уже подано заявление на другой профиль" });

                cnt = context.AG_Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId && x.AG_Entry.ProgramId == iProgramId).Count();
                if (cnt > 0)
                {
                    var lstCheckUsed = (from Ent in context.AG_Entry
                                        where Ent.ProgramId == iProgramId && Ent.EntryClassId == iEntryClassId
                                        select new { Ent.ProgramId, Ent.ProfileId, Ent.HasManualExams }).Except
                                (
                                from App in context.AG_Application
                                where App.AG_Entry.ProgramId == iProgramId && App.AG_Entry.EntryClassId == iEntryClassId
                                && App.PersonId == PersonId && App.Enabled == true && App.CommitId == gCommId
                                select new { App.AG_Entry.ProgramId, App.AG_Entry.ProfileId, App.AG_Entry.HasManualExams }
                                ).ToList();
                    if (lstCheckUsed.Count == 0)
                        return Json(new { IsOk = false, ErrorMessage = "Заявление уже подавалось" });
                }

                var Profs = context.AG_Entry.Where(x => x.EntryClassId == iEntryClassId && x.ProgramId == iProgramId && x.ProfileId != null)
                    .Select(x => new { Id = x.ProfileId, Name = x.AG_Profile.Name }).ToList();
                if (Profs.Count > 1)
                    return Json(new { IsOk = true, Data = Profs });
                else
                {
                    bool HasProfileExams = context.AG_Entry.Where(x => x.EntryClassId == iEntryClassId && x.ProgramId == iProgramId && x.ProfileId == iProfileId).Select(x => x.HasManualExams).FirstOrDefault();

                    var Exams = (from Ent in context.AG_Entry
                                    join ManualExamsInEntry in context.AG_ManualExamInAG_Entry on Ent.Id equals ManualExamsInEntry.EntryId
                                    join ManualExam in context.AG_ManualExam on ManualExamsInEntry.ExamId equals ManualExam.Id
                                    where Ent.EntryClassId == iEntryClassId && Ent.ProgramId == iProgramId
                                    select new { Value = ManualExam.Id, Name = ManualExam.Name }).ToList();

                    return Json(new { IsOk = true, HasProfileExams = HasProfileExams, Exams = Exams });
                }
            }
        }*/
      /*  [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult CheckSpecializations_AG(string classid, string programid, string specid, string CommitId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int iEntryClassId = 0;
            int iProgramId = 0;
            int iProfileId = 0;

            int.TryParse(classid, out iEntryClassId);
            int.TryParse(programid, out iProgramId);
            int.TryParse(specid, out iProfileId);

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                int cnt = context.AG_Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId).Count();
                if (cnt >= 2)
                    return Json(new { IsOk = false, ErrorMessage = "У абитуриента уже имеется 2 активных заявления" });

                cnt = context.AG_Application
                    .Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId && x.AG_Entry.EntryClassId == iEntryClassId
                        && x.AG_Entry.ProgramId == iProgramId && x.AG_Entry.ProfileId == iProfileId).Count();
                if (cnt > 0)
                    return Json(new { IsOk = false, ErrorMessage = "Заявление уже подавалось" });
                else
                {
                    bool HasProfileExams = context.AG_Entry.Where(x => x.EntryClassId == iEntryClassId && x.ProgramId == iProgramId && x.ProfileId == iProfileId).Select(x => x.HasManualExams).FirstOrDefault();

                    var Exams = (from Ent in context.AG_Entry
                                 join ManualExamsInEntry in context.AG_ManualExamInAG_Entry on Ent.Id equals ManualExamsInEntry.EntryId
                                 join ManualExam in context.AG_ManualExam on ManualExamsInEntry.ExamId equals ManualExam.Id
                                 where Ent.EntryClassId == iEntryClassId && Ent.ProgramId == iProgramId
                                 select new { Value = ManualExam.Id, Name = ManualExam.Name }).ToList().OrderBy(x => x.Name).ToList();

                    return Json(new { IsOk = true, HasProfileExams = HasProfileExams, Exams = Exams });
                }
            }
        }
      */
 /*       [HttpPost]
        public JsonResult CheckApplication_AG(string profession, string Entryclass, string profileid, string manualExam, string NeedHostel, string CommitId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                //20.06.2015 17:00
                if (DateTime.Now >= new DateTime(2015, 6, 20, 17, 0, 0))
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.AG_PriemClosed });

                bool needHostel = string.IsNullOrEmpty(NeedHostel) ? false : true;

                int iEntryClassId = Util.ParseSafe(Entryclass);
                int iProfession = Util.ParseSafe(profession);
                int iProfileId = Util.ParseSafe(profileid);
                int iManualExamId = Util.ParseSafe(manualExam);

                //------------------Проверка на дублирование заявлений---------------------------------------------------------------------
                var EntryList =
                    (from Ent in context.AG_Entry
                     join EntInEntGroup in context.AG_EntryInEntryGroup on Ent.Id equals EntInEntGroup.EntryId
                     where Ent.ProgramId == iProfession && Ent.EntryClassId == iEntryClassId && (iProfileId != 0 ? Ent.ProfileId == iProfileId : true)
                     select new
                     {
                         EntryId = Ent.Id,
                         EntInEntGroup.EntryGroupId,
                         Ent.DateOfStartEntry,
                         Ent.DateOfStopEntry
                     }).ToList();
                if (EntryList.Count > 1)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_2MoreEntry + " (" + EntryList.Count.ToString() + ")" });
                if (EntryList.Count == 0)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NoEntry });

                Guid EntryId = EntryList.First().EntryId;
                DateTime? timeOfStart = EntryList.First().DateOfStartEntry;
                DateTime? timeOfStop = EntryList.First().DateOfStopEntry;

                //проверка на группы
                var EntryGroupList =
                    (from Entr in context.AG_Entry
                     join EntrInEntryGroup in context.AG_EntryInEntryGroup on Entr.Id equals EntrInEntryGroup.EntryId
                     join Abit in context.AG_Application on Entr.Id equals Abit.EntryId
                     where Abit.PersonId == PersonId && Abit.Enabled == true && Abit.CommitId == gCommId
                     select EntrInEntryGroup.EntryGroupId);

                var AllNeededEntries =
                    (from Entr in context.AG_Entry
                     join EntrInEntryGroup in context.AG_EntryInEntryGroup on Entr.Id equals EntrInEntryGroup.EntryId
                     where EntryGroupList.Contains(EntrInEntryGroup.EntryGroupId)
                     select Entr.Id).ToList();

                var FreeEntries = AllNeededEntries.Except(
                    context.AG_Application.Where(x => x.PersonId == PersonId && x.CommitId == gCommId).Select(x => x.EntryId).ToList()).ToList();

                if (FreeEntries.Count == 0)
                    return Json(new { IsOk = true, FreeEntries = false });
                else
                {
                    if (timeOfStart.HasValue && timeOfStart > DateTime.Now)
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NotOpenedEntry });

                    if (timeOfStop.HasValue && timeOfStop < DateTime.Now)
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_ClosedEntry });

                    var eIds =
                        (from App in context.AG_Application
                         where App.PersonId == PersonId && App.Enabled == true && App.CommitId == gCommId
                         select App.EntryId).ToList();

                    if (eIds.Contains(EntryId))
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_HasApplicationOnEntry });
                }

                return Json(new { IsOk = true, FreeEntries = true });
            }
        }
       */
 /*       [HttpPost]
        public JsonResult AddApplication_AG(string Entryclass, string profession, string profileid, string manualExam, string NeedHostel, string CommitId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                //20.06.2015 17:00
                if (DateTime.Now >= new DateTime(2015, 6, 20, 17, 0, 0))
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.AG_PriemClosed });

                bool needHostel = string.Equals(NeedHostel, "false") ? false : true;

                int iEntryClassId = Util.ParseSafe(Entryclass);
                int iProfession = Util.ParseSafe(profession);
                int iProfileId = Util.ParseSafe(profileid);
                int iManualExamId = Util.ParseSafe(manualExam);

                //------------------Проверка на дублирование заявлений---------------------------------------------------------------------
                var EntryList =
                    (from Ent in context.AG_Entry
                     join EntInEntGroup in context.AG_EntryInEntryGroup on Ent.Id equals EntInEntGroup.EntryId
                     where Ent.ProgramId == iProfession && Ent.EntryClassId == iEntryClassId && (iProfileId != 0 ? Ent.ProfileId == iProfileId : true)
                     select new
                     {
                         EntryId = Ent.Id,
                         EntInEntGroup.EntryGroupId,
                         Ent.DateOfStartEntry,
                         Ent.DateOfStopEntry,
                         ProgramName = Ent.AG_Program.Name,
                         ProfileName = Ent.AG_Profile.Name,
                     }).ToList();

                if (EntryList.Count > 1)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_2MoreEntry + " (" + EntryList.Count + ")" });
                if (EntryList.Count == 0)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NoEntry });

                Guid EntryId = EntryList.First().EntryId;
                DateTime? timeOfStart = EntryList.First().DateOfStartEntry;
                DateTime? timeOfStop = EntryList.First().DateOfStopEntry;
                string Profession = EntryList.First().ProgramName;
                string Specialization = EntryList.First().ProfileName;

                if (timeOfStart.HasValue && timeOfStart > DateTime.Now)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NotOpenedEntry });

                if (timeOfStop.HasValue && timeOfStop < DateTime.Now)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_ClosedEntry });

                //проверка на группы
                var EntryGroupList =
                    (from Entr in context.AG_Entry
                     join EntrInEntryGroup in context.AG_EntryInEntryGroup on Entr.Id equals EntrInEntryGroup.EntryId
                     join Abit in context.AG_Application on Entr.Id equals Abit.EntryId
                     where Abit.PersonId == PersonId && Abit.Enabled == true && Abit.CommitId == gCommId
                     select EntrInEntryGroup.EntryGroupId);

                bool isNoEntries = EntryGroupList.Count() == 0;
                var AllNeededEntries =
                    (from Entr in context.AG_Entry
                     join EntrInEntryGroup in context.AG_EntryInEntryGroup on Entr.Id equals EntrInEntryGroup.EntryId
                     where EntryGroupList.Contains(EntrInEntryGroup.EntryGroupId) || isNoEntries
                     select Entr.Id).ToList();

                var FreeEntries = AllNeededEntries.Except(
                    context.AG_Application.Where(x => x.PersonId == PersonId && x.CommitId == gCommId).Select(x => x.EntryId).ToList()).ToList();

                if (FreeEntries.Count == 0)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NoFreeEntries });
                else
                {
                    if (timeOfStart.HasValue && timeOfStart > DateTime.Now)
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NotOpenedEntry });

                    if (timeOfStop.HasValue && timeOfStop < DateTime.Now)
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_ClosedEntry });

                    var eIds =
                        (from App in context.AG_Application
                         where App.PersonId == PersonId && App.Enabled == true && App.CommitId == gCommId
                         select App.EntryId).ToList();

                    if (eIds.Contains(EntryId))
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_HasApplicationOnEntry });
                }

                int? PriorMax = context.AG_Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId).Select(x => x.Priority).DefaultIfEmpty(0).Max();
                // если в коммите уже есть закоммиченные заявления, то добавляемое тоже считаем закоммиченным
                bool isCommited = context.AG_Application.Where(x => x.PersonId == PersonId && x.IsCommited == true && x.CommitId == gCommId).Count() > 0;
                Guid appId = Guid.NewGuid();
                context.AG_Application.AddObject(new AG_Application()
                {
                    Id = appId,
                    PersonId = PersonId,
                    EntryId = EntryId,
                    HostelEduc = needHostel,
                    Priority = PriorMax.HasValue ? PriorMax.Value + 1 : 1,
                    Enabled = true,
                    DateOfStart = DateTime.Now,
                    ManualExamId = iManualExamId == 0 ? null : (int?)iManualExamId,
                    CommitId = gCommId,
                    IsCommited = isCommited
                });
                context.SaveChanges();

                string ManualExamName = context.AG_ManualExam.Where(x => x.Id == iManualExamId).Select(x => x.Name).FirstOrDefault();

                return Json(new { IsOk = true, Profession = Profession, Specialization = Specialization, ManualExam = ManualExamName, Id = appId.ToString("N") });
            }
        }
   */
    /*    [HttpPost]
        public JsonResult DeleteApplication_AG(string id, string CommitId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                Guid ApplicationId;
                if (!Guid.TryParse(id, out ApplicationId))
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                var App = context.AG_Application.Where(x => x.Id == ApplicationId).FirstOrDefault();
                if (App == null)
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                //if (App.IsCommited)
                //    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_FailDeleteApp_IsCommited });
                try
                {
                    context.AG_Application.DeleteObject(App);
                    context.SaveChanges();
                }
                catch
                {
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_DeleteApp_Fail });
                }

                return Json(new { IsOk = true });
            }
        }
      */
        #endregion

        #region Mag_Applications
        [HttpPost]
        public JsonResult AddApplication_Mag(string priority, string studyform, string studybasis, 
            string entry, string isSecond, string isReduced, string isParallel, string profession, string obrazprogram, 
            string specialization, string NeedHostel, string IsForeign, string IsCrimea, string CommitId, string semesterId="1", 
            string secondtype="1", string reason="")
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                /*if (DateTime.Now >= new DateTime(2014, 6, 23, 0, 0, 0))
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.AG_PriemClosed });*/

                bool needHostel = string.Equals(NeedHostel, "false") ? false : true;

                int iStudyFormId = Util.ParseSafe(studyform);
                int iStudyBasisId = Util.ParseSafe(studybasis);
                int EntryTypeId = Util.ParseSafe(entry);
                if (EntryTypeId == 8 || EntryTypeId == 10)
                    EntryTypeId = 3;
                if (EntryTypeId == 1001 || EntryTypeId == 1002)
                    EntryTypeId = 6; 
                if (EntryTypeId == 1003)
                    EntryTypeId = 7;
                int iPriority = Util.ParseSafe(priority);
                int iProfession = Util.ParseSafe(profession);
                int iObrazProgram = Util.ParseSafe(obrazprogram);

                int iSemesterId;
                if (!int.TryParse(semesterId, out iSemesterId))
                    iSemesterId = 1;

                int iSecondType;
                if (!int.TryParse(secondtype, out iSecondType))
                    iSecondType = 1;

                bool bIsParallel = Util.ParseSafe(isParallel) == 1;
                bool bIsReduced = Util.ParseSafe(isReduced) == 1;
                bool bIsSecond = Util.ParseSafe(isSecond) == 1;
                bool bIsForeign = Util.ParseSafe(IsForeign) == 1;
                bool bIsCrimea = Util.ParseSafe(IsCrimea) == 1;
                
                int iSpecialization = 0;
                if ((specialization != null) && (specialization != "") && (specialization != "null"))
                    iSpecialization = int.Parse(specialization);

                bool bisEng = Util.GetCurrentThreadLanguageIsEng();

                //------------------Проверка на дублирование заявлений---------------------------------------------------------------------
                var EntryList =
                     (from Ent in context.Entry
                      join SPStudyLevel in context.SP_StudyLevel on Ent.StudyLevelId equals SPStudyLevel.Id
                      join Semester in context.Semester on Ent.SemesterId equals Semester.Id
                      where Ent.StudyFormId == iStudyFormId &&
                            Ent.StudyBasisId == iStudyBasisId &&
                            Ent.LicenseProgramId == iProfession &&
                            Ent.ObrazProgramId == iObrazProgram &&
                            Ent.CampaignYear == Util.iPriemYear &&
                            SPStudyLevel.StudyLevelGroupId == EntryTypeId &&
                            Ent.IsParallel == bIsParallel &&
                            Ent.IsReduced == bIsReduced &&
                            Ent.IsSecond == bIsSecond &&
                           (iSpecialization == 0 ? true : Ent.ProfileId == iSpecialization) &&
                            Ent.SemesterId == iSemesterId &&
                            Ent.IsForeign == bIsForeign &&
                            Ent.IsCrimea == bIsCrimea
                      select new
                      {
                          EntryId = Ent.Id,
                          Ent.DateOfStart,
                          Ent.DateOfClose, 
                          Ent.FacultyId,
                          Ent.FacultyName,
                          SemestrName = Semester.Name,
                          StudyFormName = bisEng ? (String.IsNullOrEmpty(Ent.StudyFormNameEng) ? Ent.StudyFormName : Ent.StudyFormNameEng) : Ent.StudyFormName,
                          StudyBasisName = Ent.StudyBasisName,
                          Profession = bisEng ? (String.IsNullOrEmpty(Ent.LicenseProgramNameEng) ? Ent.LicenseProgramName : Ent.LicenseProgramNameEng) : Ent.LicenseProgramName,
                          ObrazProgram = bisEng ? (String.IsNullOrEmpty(Ent.ObrazProgramNameEng) ? Ent.ObrazProgramName : Ent.ObrazProgramNameEng) : Ent.ObrazProgramName,
                          Specialization = bisEng ? (String.IsNullOrEmpty(Ent.ProfileNameEng) ? Ent.ProfileName : Ent.ProfileNameEng) : Ent.ProfileName,
                      }).ToList();
                 
                if (EntryList.Count > 1)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_2MoreEntry + " (" + EntryList.Count + ")" });
                if (EntryList.Count == 0)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NoEntry });

                Guid EntryId = EntryList.First().EntryId;

                if (context.Application.Where(x => x.PersonId == PersonId && x.CommitId == gCommId && x.EntryId == EntryId && x.IsDeleted == false
                    && x.C_Entry.IsForeign == bIsForeign && x.C_Entry.IsCrimea == bIsCrimea).Count() > 0)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.ErrorHasApplication }); 

                DateTime? timeOfStart; 
                DateTime? timeOfStop;

                string StudyFormName = EntryList.First().StudyFormName;
                string StudyBasisName = EntryList.First().StudyBasisName;
                string Profession = EntryList.First().Profession;
                string ObrazProgram = EntryList.First().ObrazProgram;
                string Specialization = EntryList.First().Specialization;
                string faculty = EntryList.First().FacultyName;
                string SemesterName = EntryList.First().SemestrName;
                  
                int res = Util.GetRess(PersonId);

                timeOfStart = EntryList.First().DateOfStart;
                timeOfStop = EntryList.First().DateOfClose;
                  
                if (timeOfStart.HasValue && timeOfStart > DateTime.Now)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NotOpenedEntry });

                if (timeOfStop.HasValue && timeOfStop < DateTime.Now)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_ClosedEntry });

                //проверка на 3 направления
                if (EntryTypeId == 1 && !bIsForeign)
                {
                    var cnt = context.Abiturient.Where(x => x.CommitId == gCommId && x.LicenseProgramId != iProfession && !x.IsDeleted).Select(x => x.LicenseProgramId).Distinct().Count();
                    if (cnt > 2)
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_3MorePrograms });
                }

                int PriorMax = context.Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId).Select(x => (int?)x.Priority).DefaultIfEmpty(0).Max() ?? 1;
                if (PriorMax >= iPriority)
                {
                    int count = context.Application.Where(x => x.PersonId == PersonId && x.Enabled == true && x.CommitId == gCommId && x.Priority == iPriority).Select(x => x.Id).Count();
                    if (count == 0)
                    {
                    }
                    else
                    {
                        iPriority = PriorMax + 1;
                    }
                }
                else
                    iPriority = PriorMax + 1;
                
                // если в коммите уже есть закоммиченные заявления, то добавляемое тоже считаем закоммиченным
                bool isCommited = context.Application.Where(x => x.PersonId == PersonId && x.IsCommited == true && x.CommitId == gCommId).Count() > 0;
                Guid appId = Guid.NewGuid();
                context.Application.Add(new Application()
                {
                    Id = appId,
                    PersonId = PersonId,
                    EntryId = EntryId,
                    HostelEduc = needHostel,
                    Priority = iPriority,
                    Enabled = true,
                    EntryType = EntryTypeId,
                    DateOfStart = DateTime.Now,
                    CommitId = gCommId,
                    IsCommited = isCommited,
                    SecondTypeId = iSecondType
                });
                context.SaveChanges();

                var Applications = context.Abiturient.Where(x => x.PersonId == PersonId && x.CommitId == gCommId && x.IsCommited == isCommited)
                    .Select(x => new { x.StudyLevelGroupNameRus, x.StudyLevelGroupNameEng, x.SecondTypeId}).FirstOrDefault();
                string LevelGroupName = bisEng ? Applications.StudyLevelGroupNameEng : Applications.StudyLevelGroupNameRus +
                                        ((Applications.SecondTypeId == 3) ? (bisEng ? " (recovery)" : " (восстановление)") :
                                        ((Applications.SecondTypeId == 2) ? (bisEng ? " (transfer)" : " (перевод)") :
                                        ((Applications.SecondTypeId == 4) ? (bisEng ? " (changing form of education)" : " (смена формы обучения)") :
                                        ((Applications.SecondTypeId == 5) ? (bisEng ? " (changing basis of education)" : " (смена основы обучения)") :
                                        ((Applications.SecondTypeId == 6) ? (bisEng ? " (changing educational program)" : " (смена образовательной программы)") :
                                        "")))));

                return Json(new { IsOk = true, StudyLevelGroupName = LevelGroupName, StudyFormName = StudyFormName, StudyBasisName = StudyBasisName, Profession = Profession, Specialization = Specialization, ObrazProgram = ObrazProgram, Id = appId.ToString("N"), Faculty = faculty, isgosline = IsForeign, isCrimea = IsCrimea, semesterId = SemesterName, Reason = reason });
            }
        }
        [HttpPost]
        public JsonResult DeleteApplication_Mag(string id, string CommitId)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                Guid ApplicationId;
                if (!Guid.TryParse(id, out ApplicationId))
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                var App = context.Application.Where(x => x.Id == ApplicationId).FirstOrDefault();
                if (App == null)
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                //if (App.IsCommited)
                //    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_FailDeleteApp_IsCommited });
                try
                {
                    App.IsDeleted = true;
                    context.SaveChanges();
                }
                catch
                {
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_DeleteApp_Fail });
                }

                return Json(new { IsOk = true });
            }
        }
        [HttpPost]
        public JsonResult CheckApplication_Mag(string studyform, string studybasis, string entry, string isSecond, 
            string isReduced, string isParallel, string profession, string obrazprogram, string specialization, string NeedHostel, string IsForeign, string IsCrimea, string CommitId, string semesterId = "1")
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            Guid gCommId;
            if (!Guid.TryParse(CommitId, out gCommId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)//а что это могло бы значить???
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                //if (DateTime.Now >= new DateTime(2014, 6, 23, 0, 0, 0))
                //    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.AG_PriemClosed });

                bool needHostel = string.IsNullOrEmpty(NeedHostel) ? false : true;

                int iStudyFormId = Util.ParseSafe(studyform);
                int iStudyBasisId = Util.ParseSafe(studybasis);
                int EntryTypeId = Util.ParseSafe(entry);
                if (EntryTypeId == 8 || EntryTypeId == 10)
                {
                    EntryTypeId = 3;
                }
                if (EntryTypeId == 1001 || EntryTypeId == 1002)
                    EntryTypeId = 6;
                if (EntryTypeId == 1003)
                    EntryTypeId = 7;
                int iProfession = Util.ParseSafe(profession);
                int iObrazProgram = Util.ParseSafe(obrazprogram);
                int iParallel = Util.ParseSafe(isParallel);
                int iReduced = Util.ParseSafe(isReduced);
                int iSecond = Util.ParseSafe(isSecond);
                int iSemesterId;
                if (!int.TryParse(semesterId, out iSemesterId))
                    iSemesterId = 1;

                bool bIsParallel = iParallel == 1;
                bool bIsReduced = iReduced == 1;
                bool bIsSecond = iSecond == 1;
                bool bIsForeign = Util.ParseSafe(IsForeign) == 1;
                bool bIsCrimea = Util.ParseSafe(IsCrimea) == 1;

                int gSpecialization = 0;
                if ((specialization != null) && (specialization != "") && (specialization != "null"))
                    gSpecialization = int.Parse(specialization);

                //------------------Проверка на дублирование заявлений---------------------------------------------------------------------
                var EntryList =
                      (from Ent in context.Entry
                       join SPStudyLevel in context.SP_StudyLevel on Ent.StudyLevelId equals SPStudyLevel.Id
                       where Ent.StudyFormId == iStudyFormId &&
                             Ent.StudyBasisId == iStudyBasisId &&
                             Ent.LicenseProgramId == iProfession &&
                             Ent.ObrazProgramId == iObrazProgram &&
                             Ent.CampaignYear == Util.iPriemYear &&
                             SPStudyLevel.StudyLevelGroupId == EntryTypeId &&
                             Ent.IsParallel == bIsParallel &&
                             Ent.IsReduced == bIsReduced &&
                             Ent.IsSecond == bIsSecond &&
                            (gSpecialization == 0 ? true : Ent.ProfileId == gSpecialization) &&
                             Ent.SemesterId == iSemesterId 
                             //&&
                             //Ent.IsCrimea == bIsCrimea 
                             //&& 
                             //Ent.IsForeign == bIsForeign
                       select new
                       {
                           EntryId = Ent.Id,
                           Ent.DateOfStart,
                           Ent.DateOfClose,
                           StudyFormName = Ent.StudyFormName,
                           StudyBasisName = Ent.StudyBasisName,
                           Profession = Ent.LicenseProgramName,
                           ObrazProgram = Ent.ObrazProgramName,
                           Specialization = Ent.ProfileName,
                           IsForeign = Ent.IsForeign,
                           IsCrimea = Ent.IsCrimea
                       }).ToList();

                if (EntryList.Count > 1)
                {
                    //если при разбиении на группы везде по 1 конкурсу, то не считается
                    var GroupedView = EntryList.Select(x => new { x.StudyFormName, x.StudyBasisName, x.Profession, x.ObrazProgram, x.Specialization, x.IsCrimea, x.IsForeign })
                        .Select(x => new
                        {
                            //x.StudyFormName,
                            //x.StudyBasisName,
                            //x.Profession,
                            //x.ObrazProgram,
                            //x.Specialization,
                            //x.IsCrimea,
                            //x.IsForeign,
                            //число конкурсов по разбиению
                            CNT = EntryList.Where(z => z.StudyFormName == x.StudyFormName && x.StudyBasisName == z.StudyBasisName 
                                && x.Profession == z.Profession && x.ObrazProgram == z.ObrazProgram && x.Specialization == z.Specialization && x.IsCrimea == z.IsCrimea && x.IsForeign == z.IsForeign).Count()
                        }).ToList();

                    if (GroupedView.Where(x => x.CNT > 1).Count() > 0)
                        return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_2MoreEntry + " (" + EntryList.Count.ToString() + ")" });
                }
                if (EntryList.Count == 0)
                    return Json(new { IsOk = false, ErrorMessage = Resources.NewApplication.NewApp_NoEntry });

                Guid EntryId = EntryList.First().EntryId;
                DateTime? timeOfStart = EntryList.First().DateOfStart;
                DateTime? timeOfStop = EntryList.First().DateOfClose; 

                return Json(new { IsOk = true, FreeEntries = true });
            }
        }
        #endregion

        public JsonResult GetStudyLevels_AG()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (PersonInfo == null)
                    return Json(new { IsOk = false });

                if (PersonInfo.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                List<int> StudyLevelGroupIdList = new List<int>() { 6, 7 };
                
                var lst = Util.GetEnableLevelList(new List<int>() { 6, 7 }, PersonInfo);
                if (lst.Count >0)
                    return Json(new { IsOk = true, List = lst });
                else
                    return Json(new { IsOk = false, ErrorMessage = "Несоответствие образования"});
            }
        }
        public JsonResult GetStudyLevels_SPO()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var Person = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                if (Person == null)
                    return Json(new { IsOk = false });

                if (Person.PersonEducationDocument == null)
                    return Json(new { IsOk = false, ErrorMessage = "Нет сведений об образовании!" });

                PersonEducationDocument PersonEducationDocument = Person.PersonEducationDocument.OrderByDescending(x=>x.SchoolTypeId).FirstOrDefault();
                if (PersonEducationDocument.SchoolTypeId == 1)
                {
                    if (PersonEducationDocument.SchoolExitClassId != 0)
                    {
                        //школьники 11 класса могут поступать всюду
                        //школьники 9-10 классов могут поступать только на 9 класс
                        //остальные школьники в пролёте
                        if (PersonEducationDocument.SchoolExitClass.IntValue < 9)
                        {
                            return Json(new { IsOk = false, ErrorMessage = "Для " + PersonEducationDocument.SchoolExitClass.IntValue + " класса доступен только приём в АГ" });
                        }
                        else if (PersonEducationDocument.SchoolExitClass.IntValue < 11)
                        {
                            var lst = context.SP_StudyLevel.Where(x => x.Id == 10).Select(x => new { x.Id, x.Name }).ToList();
                            return Json(new { IsOk = true, List = lst });
                        }
                        else
                        {
                            var lst = context.SP_StudyLevel.Where(x => x.StudyLevelGroupId == 3).Select(x => new { x.Id, x.Name }).ToList();
                            return Json(new { IsOk = true, List = lst });
                        }
                    }
                    else
                        return Json(new { IsOk = false, ErrorMessage = "Нет данных об оконченном классе!" });
                }
                else
                {
                    var lst = context.SP_StudyLevel.Where(x => x.StudyLevelGroupId == 3).Select(x => new { x.Id, x.Name }).ToList();
                    return Json(new { IsOk = true, List = lst });
                }

            }
        }
        #endregion

        public ActionResult HeartBeat()
        {
            try
            {
                string query = "SELECT Id FROM [dbo].[_HeartBeat] WHERE Id=@Id";
                Util.AbitDB.GetValue(query, new SortedList<string, object>() { { "@Id", Guid.NewGuid() } });
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message + (ex.InnerException == null ? "" : "\nINNER EXCEPTION: " + ex.InnerException), JsonRequestBehavior.AllowGet);
            }
        }

        #region OtherPassport AJAX
        public ActionResult AddOtherPassport(string PassportType, string PassportSeries, string PassportNumber, string PassportSurname)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            bool UILang = Util.GetCurrentThreadLanguageIsEng();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                int iPassportTypeId = 0;
                if (!int.TryParse(PassportType, out iPassportTypeId))
                    return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

                string PassportTypeName = context.PassportType.Where(x => x.Id == iPassportTypeId)
                    .Select(x => UILang ? x.NameEng : x.Name).FirstOrDefault();

                if (string.IsNullOrEmpty(PassportSurname))
                    PassportSurname = context.Person.Where(x => x.Id == PersonId).Select(x => x.Surname).FirstOrDefault();

                PersonOtherPassport ins = new PersonOtherPassport();
                ins.PassportTypeId = iPassportTypeId;
                ins.PersonId = PersonId;
                ins.Surname = PassportSurname;
                ins.PassportSeries = PassportSeries;
                ins.PassportNumber = PassportNumber;

                context.PersonOtherPassport.Add(ins);
                context.SaveChanges();

                return Json(new
                {
                    IsOk = true,
                    PassportType = PassportTypeName,
                    PassportSurname = PassportSurname,
                    PassportNumber = PassportNumber,
                    PassportSeries = PassportSeries,
                    Id = ins.Id
                });
            }
        }
        public ActionResult GetOtherPassportList()
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            bool isEng = Util.GetCurrentThreadLanguageIsEng(); 


            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var lst = context.PersonOtherPassport.Where(x => x.PersonId == PersonId)
                    .Select(x => new
                    {
                        x.Id,
                        PassportType = isEng ? x.PassportType.NameEng : x.PassportType.Name,
                        x.PassportSeries,
                        x.PassportNumber,
                        PassportSurname = x.Surname,
                    }).ToList();

                return Json(new
                {
                    IsOk = true,
                    List = lst,
                });
            }
        }
        public ActionResult DeleteOtherPassport(string id)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });

            string UILang = Util.GetUILang(PersonId);

            int iId = 0;
            if (!int.TryParse(id, out iId))
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.IncorrectGUID });

            int cnt = 0;

            try
            {
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var ent = context.PersonOtherPassport.Where(x => x.Id == iId && x.PersonId == PersonId).FirstOrDefault();
                    if (ent != null)
                    {
                        context.PersonOtherPassport.Remove(ent);
                        context.SaveChanges();
                        cnt = context.PersonOtherPassport.Where(x => x.PersonId == PersonId).Count();
                    }
                    else
                    {
                        return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.AuthorizationRequired });
                    }
                }
            }
            catch
            {
                return Json(new { IsOk = false, ErrorMessage = Resources.ServerMessages.ErrorWhileDeleting });
            }

            return Json(new { IsOk = true, Count = cnt });
        }
        #endregion

        #region UFMS
        public ActionResult RusLangExam_ufms()
        {
            return View("RusLangExam_ufms");
        }
        public ActionResult ufms(string HiddenId)
        {
            return View("RusLangExam_ufms");
        }
        #endregion

        #region TechInfo
        public ActionResult CacheCount()
        {
            return Json(Util.CacheSID_User.Count.ToString(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult GetErrorPage()
        {
            return View("Error");
        }
    }
}