using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Data;

using iTextSharp.text;
using iTextSharp.text.pdf;
using OnlineAbit2013.Models;
using System.Drawing;
using OnlineAbit2013.EMDX;

namespace OnlineAbit2013.Controllers
{
    public class ShortApplicationDetails
    {
        public Guid ApplicationId { get; set; }
        public int? CurrVersion { get; set; }
        public DateTime? CurrDate { get; set; }
        public string ObrazProgramName { get; set; }
        public string ProfileName { get; set; }
        public int InnerEntryInEntryPriority { get; set; }
    }
    public class ShortAppcation
    {
        public Guid ApplicationId { get; set; }
        public int Priority { get; set; }
        public string LicenseProgramName { get; set; }
        public string ObrazProgramName { get; set; }
        public string ProfileName { get; set; }

        public bool HasInnerPriorities { get; set; }
        public int InnerPrioritiesNum { get; set; }

        public int StudyFormId { get; set; }
        public int StudyBasisId { get; set; }

        public bool IsCrimea { get; set; }
        public bool IsForeign { get; set; }
    }

    public static class PDFUtils
    {
        /// <summary>
        /// PDF Список файлов
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="fontPath"></param>
        /// <returns></returns>
        public static byte[] GetFilesList(Guid personId, Guid applicationId, string fontPath)
        {
            MemoryStream ms = new MemoryStream();

            string query = "SELECT Surname, Name, SecondName FROM Person WHERE Id=@Id";
            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", personId } });

            if (tbl.Rows.Count == 0)
                return new byte[1] { 0x00 };

            var person =
                (from DataRow rw in tbl.Rows
                 select new
                 {
                     Surname = rw.Field<string>("Surname"),
                     Name = rw.Field<string>("Name"),
                     SecondName = rw.Field<string>("SecondName")
                 }).FirstOrDefault();

            string FIO = person.Surname + " " + person.Name + " " + person.SecondName;
            FIO = FIO.Trim();

            using (Document doc = new Document())
            {
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font12 = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font font16 = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.NORMAL);

                PdfWriter writer = PdfWriter.GetInstance(doc, ms);

                doc.Open();

                Paragraph p = new Paragraph("ПРИЕМНАЯ КОМИССИЯ СПБГУ", font16);
                p.Alignment = Element.ALIGN_CENTER;
                doc.Add(p);

                p = new Paragraph("Опись \n поданных документов", font16);
                p.Alignment = Element.ALIGN_CENTER;
                doc.Add(p);

                p = new Paragraph(FIO + "\n\n", font16);
                p.Alignment = Element.ALIGN_CENTER;
                doc.Add(p);

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 40f, 15f, 15f, 20f });
                //table.SetWidthPercentage(new float[] { 10f, 40f, 15f, 15f, 20f }, doc.PageSize);

                PdfPCell cell = new PdfPCell(new Phrase("№ п/п", font12));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Наименование документа (имя файла)", font12));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Копия / подлинник", font12));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Дата подачи (загрузки)", font12));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Подпись сотрудника ПК, принявшего документ при личной подаче", font12));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                int cnt = 0;
                query = "SELECT FileName, Comment, LoadDate FROM PersonFile WHERE PersonId=@PersonId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", personId } });
                var PersonFile =
                    from DataRow rw in tbl.Rows
                    select new
                    {
                        Comment = rw.Field<string>("Comment"),
                        FileName = rw.Field<string>("FileName"),
                        LoadDate = rw.Field<DateTime?>("LoadDate").HasValue ? rw.Field<DateTime>("LoadDate").ToShortDateString() : "нет"
                    };
                query = "SELECT FileName, Comment, LoadDate FROM ApplicationFile WHERE ApplicationId=@AppId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@AppId", applicationId } });
                var ApplicationFile =
                    from DataRow rw in tbl.Rows
                    select new
                    {
                        Comment = rw.Field<string>("Comment"),
                        FileName = rw.Field<string>("FileName"),
                        LoadDate = rw.Field<DateTime?>("LoadDate").HasValue ? rw.Field<DateTime>("LoadDate").ToShortDateString() : "нет"
                    };

                var AllFiles = ApplicationFile.Union(PersonFile);

                foreach (var file in AllFiles)
                {
                    ++cnt;
                    cell = new PdfPCell(new Phrase(cnt.ToString(), font12));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    table.AddCell(new Phrase(string.Format("{0} ({1})", file.Comment, file.FileName), font12));

                    cell = new PdfPCell(new Phrase("Копия", font12));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(file.LoadDate, font12));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    table.AddCell("");
                }

                for (int j = 0; j < 3; j++)
                {
                    ++cnt;
                    cell = new PdfPCell(new Phrase(cnt.ToString(), font12));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    for (int z = 0; z < 4; z++)
                    {
                        cell = new PdfPCell(new Phrase("", font12));
                        table.AddCell(cell);
                    }
                }
                doc.Add(table);


                p = new Paragraph("Создано: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), font12);
                p.Alignment = Element.ALIGN_RIGHT;
                doc.Add(p);

                doc.Close();
            }

            return ms.ToArray();
        }

        //1курс-магистратура ОСНОВНОЙ (AbitTypeId = 1)
        public static byte[] GetApplicationPDF(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    Entry.IsForeign,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    Entry.IsCrimea
                                }).OrderBy(x => x.Priority).ToList();

                var abitProfileList = (from x in context.Application
                                       join Ad in context.extApplicationDetails on x.Id equals Ad.ApplicationId
                                       where x.CommitId == appId
                                       select new ShortApplicationDetails()
                                       {
                                           ApplicationId = x.Id,
                                           CurrVersion = Ad.CurrVersion,
                                           CurrDate = Ad.CurrDate,
                                           InnerEntryInEntryPriority = Ad.InnerEntryInEntryPriority,
                                           ObrazProgramName = ((Ad.ObrazProgramCrypt + " ") ?? "") + Ad.ObrazProgramName,
                                           ProfileName = Ad.ProfileName
                                       }).ToList();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  AddInfo = x.PersonAddInfo.AddInfo,
                                  Parents = x.PersonAddInfo.Parents,
                                  HasPrivileges = x.PersonAddInfo.HasPrivileges ?? false,
                                  x.PersonAddInfo.ReturnDocumentTypeId,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.HasRussianNationality,
                                  x.PersonAddInfo.StartEnglish,
                                  x.PersonAddInfo.EnglishMark,
                                  Language = x.PersonAddInfo.Language.Name,
                                  x.PersonAddInfo.HasTRKI,
                                  x.PersonAddInfo.TRKICertificateNumber,
                              }).FirstOrDefault();
               
                var personEducationList =
                   (from x in context.PersonEducationDocument
                    join hx in context.PersonHighEducationInfo on x.Id equals hx.EducationDocumentId into gj
                    from heduc in gj.DefaultIfEmpty()

                    join q in context.Qualification on heduc.QualificationId equals q.Id into ggj
                    from qual in ggj.DefaultIfEmpty()

                    where x.PersonId == PersonId
                    select new
                    {
                        x.Id,
                        x.SchoolExitYear,
                        x.SchoolName,
                        x.SchoolNum,
                        x.IsEqual,
                        x.EqualDocumentNumber,
                        ProgramName = (heduc == null ? "" : heduc.ProgramName),
                        CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                        QualificationId = (heduc == null ? -1 : heduc.QualificationId),
                        Qualification = (qual == null ? "" : qual.Name),
                        x.CountryEducId,
                        x.SchoolTypeId,
                        EducationDocumentSeries = x.Series,
                        EducationDocumentNumber = x.Number,
                    }).ToList();

                var personEducation = personEducationList.OrderByDescending(x=>x.SchoolTypeId).OrderByDescending(x => x.QualificationId).First();

                MemoryStream ms = new MemoryStream();
                string dotName;

                if (isMag)//mag
                    dotName = "ApplicationMag_page3.pdf";
                else
                    dotName = "Application_page3.pdf";

                byte[] templateBytes;

                List<byte[]> lstFiles = new List<byte[]>();
                List<byte[]> lstAppendixes = new List<byte[]>();
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                AcroFields acrFlds = pdfStm.AcroFields;

                var Version = context.ApplicationCommitVersion.Where(x => x.CommitId == appId).Select(x => new { x.VersionDate, x.Id }).ToList().LastOrDefault();
                string sVersion = "";
                if (Version != null)
                    sVersion = "Версия №" + Version.Id + " от " + Version.VersionDate.ToString("dd.MM.yyyy HH:mm");
                string FIO = ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim();

                List<ShortAppcation> lstApps = abitList
                    .Select(x => new ShortAppcation()
                    {
                        ApplicationId = x.Id,
                        LicenseProgramName = x.ProfessionCode + " " + x.Profession,
                        ObrazProgramName = x.ObrazProgram,
                        ProfileName = x.Specialization,
                        Priority = x.Priority,
                        StudyBasisId = x.StudyBasisId,
                        StudyFormId = x.StudyFormId,
                        HasInnerPriorities = abitProfileList.Where(y => y.ApplicationId == x.Id).Count() > 0,
                        IsCrimea = x.IsCrimea,
                        IsForeign = x.IsForeign
                    }).ToList();
                int incrmtr = 1;
                for (int u = 0; u < lstApps.Count; u++)
                {
                    if (lstApps[u].HasInnerPriorities) //если есть профили
                    {
                        lstApps[u].InnerPrioritiesNum = incrmtr; //то пишем об этом
                        //и сразу же создаём приложение с описанием - потом приложим

                        List<ShortApplicationDetails> lstAppDetails =
                               abitProfileList.Where(x => x.ApplicationId == lstApps[u].ApplicationId).ToList();

                        if (isMag)
                        {
                            lstAppendixes.Add(GetApplicationPDF_ProfileAppendix_Mag(lstAppDetails, lstApps[u].LicenseProgramName, lstApps[u].ObrazProgramName, FIO, dirPath, incrmtr));
                            incrmtr++;
                        }
                        else 
                        {    
                            lstAppendixes.Add(GetApplicationPDF_ProfileAppendix_1kurs(lstAppDetails, lstApps[u].LicenseProgramName, FIO, dirPath, incrmtr));
                            incrmtr++;
                        }
                    }
                }

                if (isMag)
                    lstAppendixes.Add(GetApplicationPDF_Agreement_Mag(FIO, person.Sex, dirPath, sVersion));
                else
                    lstAppendixes.Add(GetApplicationPDF_Agreement_1k(FIO, person.Sex, dirPath, sVersion));

                List<ShortAppcation> lstAppsFirst = new List<ShortAppcation>();
                for (int u = 0; u < 3; u++)
                {
                    if (lstApps.Count > u)
                        lstAppsFirst.Add(lstApps[u]);
                }

                int multiplyer = isMag ? 2 : 1;
                string code = ((multiplyer * 100000) + abitList.First().CommitIntNumber).ToString();

                //добавляем первый файл
                lstFiles.Add(GetApplicationPDF_FirstPage(lstAppsFirst, lstApps, dirPath, isMag ? "ApplicationMag_page1.pdf" : "Application_page1.pdf", FIO, sVersion, code, isMag));
                acrFlds.SetField("Version", sVersion);

                //остальные - по 4 на новую страницу
                int appcount = 3;
                while (appcount < lstApps.Count)
                {
                    lstAppsFirst = new List<ShortAppcation>();
                    for (int u = 0; u < 4; u++)
                    {
                        if (lstApps.Count > appcount)
                            lstAppsFirst.Add(lstApps[appcount]);
                        else
                            break;
                        appcount++;
                    }

                    lstFiles.Add(GetApplicationPDF_NextPage(lstAppsFirst, lstApps, dirPath, "ApplicationMag_page2.pdf", FIO));
                }

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                if (person.BirthDate.HasValue)
                {
                    acrFlds.SetField("BirthDateYear", person.BirthDate.Value.Year.ToString("D2"));
                    acrFlds.SetField("BirthDateMonth", person.BirthDate.Value.Month.ToString("D2"));
                    acrFlds.SetField("BirthDateDay", person.BirthDate.Value.Day.ToString("D2"));
                }
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);

                //dd.MM.yyyy :12.05.2000
                string[] splitStr = GetSplittedStrings(person.PassportAuthor + " " + person.PassportDate.Value.ToString("dd.MM.yyyy"), 60, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("PassportAuthor" + i, splitStr[i - 1]);
                if (person.HasRussianNationality)
                    acrFlds.SetField("HasRussianNationalityYes", "1");
                else
                    acrFlds.SetField("HasRussianNationalityNo", "1");

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);

                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());
                if (person.StartEnglish)
                    acrFlds.SetField("chbEnglishYes", "1");
                else
                    acrFlds.SetField("chbEnglishNo", "1");

                acrFlds.SetField("Phone", person.Phone);
                acrFlds.SetField("Email", email);
                acrFlds.SetField("Mobiles", person.Mobiles);

                acrFlds.SetField("ExitYear", personEducation.SchoolExitYear.ToString());
                string SchoolNameNum = personEducation.SchoolName;
                if (personEducation.SchoolTypeId == 1)
                {
                    if (!String.IsNullOrEmpty(personEducation.SchoolNum))
                    {
                        if (personEducation.SchoolNum.StartsWith("№"))
                            SchoolNameNum += " (" + personEducation.SchoolNum + ")";
                        else if (personEducation.SchoolNum.StartsWith("#"))
                            SchoolNameNum += " (" + personEducation.SchoolNum + ")";
                        else
                            SchoolNameNum += " (№" + personEducation.SchoolNum + ")";
                    }
                }
                splitStr = GetSplittedStrings(SchoolNameNum ?? "", 50, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("School" + i, splitStr[i - 1]);

                //только у магистров
                acrFlds.SetField("HEProfession", personEducation.ProgramName ?? "");
                acrFlds.SetField("Qualification", personEducation.Qualification ?? "");

                acrFlds.SetField("Original", "0");
                acrFlds.SetField("Copy", "0");
                acrFlds.SetField("CountryEduc", personEducation.CountryEduc ?? "");
                acrFlds.SetField("Language", person.Language ?? "");

                string extraPerson = person.Parents ?? "";
                splitStr = GetSplittedStrings(extraPerson, 70, 70, 3);
                for (int i = 1; i <= 3; i++)
                {
                    acrFlds.SetField("Parents" + i.ToString(), splitStr[i - 1]);
                    acrFlds.SetField("ExtraParents" + i.ToString(), splitStr[i - 1]);
                }

                string Attestat = personEducation.SchoolTypeId == 1 ? ("аттестат серия " + personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? "") :
                        ("диплом серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? ""));
                acrFlds.SetField("Attestat", Attestat);
                acrFlds.SetField("Extra", person.AddInfo ?? "");

                if (personEducation.IsEqual && personEducation.CountryEducId != 193)
                {
                    acrFlds.SetField("IsEqual", "1");
                    acrFlds.SetField("EqualSertificateNumber", personEducation.EqualDocumentNumber);
                }
                else
                {
                    acrFlds.SetField("NoEqual", "1");
                }

                if (person.HasPrivileges)
                    acrFlds.SetField("HasPrivileges", "1");

                acrFlds.SetField("ReturnDocumentType" + person.ReturnDocumentTypeId, "1");
                // имею ли образование этого уровня:
                // если закончил школу, спо, нпо и прочее (кроме ВУЗа) + (если выбран ВУЗ + Квалификация 
                if ((personEducationList.Where(x=>x.SchoolTypeId == 4).Select(x=>x).Count() == 0) || (isMag && personEducationList.Where(x=>x.SchoolTypeId == 4).Select(x=>x.Qualification.ToLower().IndexOf("магистр")).Max() <0 ))// personEducation.SchoolTypeId == 4 && (personEducation.Qualification).ToLower().IndexOf("магист") < 0))
                    acrFlds.SetField("NoEduc", "1");
                else
                {
                    acrFlds.SetField("HasEduc", "1");
                    acrFlds.SetField("HighEducation", personEducation.SchoolName);
                }

                if (!isMag)
                {
                    //EGE
                    query = "SELECT ExamName, MarkValue, Number FROM EgeMarksAll WHERE PersonId=@PersonId";
                    tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
                    var exams =
                        from DataRow rw in tbl.Rows
                        select new
                        {
                            ExamName = rw.Field<string>("ExamName"),
                            MarkValue = rw.Field<int?>("MarkValue"),
                            Number = rw.Field<string>("Number")
                        };
                    int egeCnt = 1;
                    foreach (var ex in exams)
                    {
                        acrFlds.SetField("TableName" + egeCnt, ex.ExamName);
                        acrFlds.SetField("TableValue" + egeCnt, ex.MarkValue.ToString());
                        acrFlds.SetField("TableNumber" + egeCnt, ex.Number);

                        if (egeCnt == 4)
                            break;
                        egeCnt++;
                    }

                    //VSEROS
                    var OlympVseros = context.Olympiads.Where(x => x.PersonId == PersonId && x.OlympType.IsVseross)
                        .Select(x => new { x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                    egeCnt = 1;
                    foreach (var ex in OlympVseros)
                    {
                        acrFlds.SetField("OlympVserosName" + egeCnt, ex.Name);
                        acrFlds.SetField("OlympVserosYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                        acrFlds.SetField("OlympVserosDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                        if (egeCnt == 2)
                            break;
                        egeCnt++;
                    }

                    //OTHEROLYMPS
                    var OlympNoVseros = context.Olympiads.Where(x => x.PersonId == PersonId && !x.OlympType.IsVseross)
                        .Select(x => new { x.OlympName.Name, OlympSubject = x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                    egeCnt = 1;
                    foreach (var ex in OlympNoVseros)
                    {
                        acrFlds.SetField("OlympName" + egeCnt, ex.Name + " (" + ex.OlympSubject + ")");
                        acrFlds.SetField("OlympYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                        acrFlds.SetField("OlympDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                        if (egeCnt == 2)
                            break;
                        egeCnt++;
                    }

                    if (!string.IsNullOrEmpty(personEducation.SchoolName))
                        acrFlds.SetField("chbSchoolFinished", "1");

                }

                query = "SELECT WorkPlace, WorkProfession, Stage FROM PersonWork WHERE PersonId=@PersonId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
                var work =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         WorkPlace = rw.Field<string>("WorkPlace"),
                         WorkProfession = rw.Field<string>("WorkProfession"),
                         Stage = rw.Field<string>("Stage")
                     }).FirstOrDefault();
                if (work != null)
                {
                    acrFlds.SetField("HasStag", "1");
                    acrFlds.SetField("WorkPlace", work.WorkPlace + ", " + work.WorkProfession);
                    acrFlds.SetField("Stag", work.Stage);
                }
                else
                    acrFlds.SetField("NoStag", "1");

                int comInd = 1;
                foreach (var comission in abitList.Select(x => x.ComissionAddress).Distinct().ToList())
                {
                    acrFlds.SetField("Comission" + comInd++, comission);
                }

              
                context.SaveChanges();

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                lstFiles.Add(ms.ToArray());

                return MergePdfFiles(lstFiles.Union(lstAppendixes).ToList());
            }
        }

        public static byte[] GetApplicationPDF_ProfileAppendix_Mag(List<ShortApplicationDetails> lst, string LicenseProgramName, string ObrazProgramName, string FIO, string dirPath, int Num)
        {
            MemoryStream ms = new MemoryStream();
            string dotName = "PriorityProfiles_Mag2014.pdf";

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
            AcroFields acrFlds = pdfStm.AcroFields;
            acrFlds.SetField("Num", Num.ToString());
            acrFlds.SetField("FIO", FIO);

            string[] tmp = GetSplittedStrings(ObrazProgramName, 40, 70, 2);
            acrFlds.SetField("ObrazProgramHead1", tmp[0]);
            acrFlds.SetField("ObrazProgramHead2", tmp[1]);
            acrFlds.SetField("LicenseProgram", LicenseProgramName);
            acrFlds.SetField("ObrazProgram", ObrazProgramName);

            int i = 0;
            foreach (var Prof in lst.OrderBy(x => x.InnerEntryInEntryPriority))
            {
                i++;
                //если других программ нет, то нет смысла показывать её название.
                bool bShowObrazProgram = lst.Where(x => x.ObrazProgramName != Prof.ObrazProgramName).Count() > 0;
                acrFlds.SetField("Profile" + i.ToString(), (bShowObrazProgram ? Prof.ObrazProgramName + " /\n" : "") + Prof.ProfileName);
            }

            //int rwind = 1;
            //foreach (var xxxx in lst.OrderBy(x => x.InnerEntryInEntryPriority))
            //{
            //    string sVal = "";
            //    bool bNeedSeparatorInVal = false;
            //    if (xxxx.ObrazProgramName != ObrazProgramName)
            //    {
            //        sVal += xxxx.ObrazProgramName;
            //        bNeedSeparatorInVal = true;
            //    }
            //    if (xxxx.ProfileName != "нет")
            //    {
            //        sVal += (bNeedSeparatorInVal ? "; Профиль: " : "") + xxxx.ProfileName;
            //    }
            //    acrFlds.SetField("Profile" + rwind, sVal);
            //    rwind++;
            //}

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }
        public static byte[] GetApplicationPDF_OPAppendix_1kurs(List<ShortApplicationDetails> lst, string LicenseProgramName, string FIO, string dirPath, int Num)
        {
            MemoryStream ms = new MemoryStream();
            string dotName = "PriorityOP2014.pdf";

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
            AcroFields acrFlds = pdfStm.AcroFields;
            acrFlds.SetField("Num", Num.ToString());
            acrFlds.SetField("FIO", FIO);

            acrFlds.SetField("LicenseProgram", LicenseProgramName);
            int rwind = 1;
            foreach (var p in lst.Select(x => new { x.ObrazProgramName, ObrazProgramInEntryPriority = x.InnerEntryInEntryPriority }).Distinct().OrderBy(x => x.ObrazProgramInEntryPriority))
            {
                acrFlds.SetField("ObrazProgram" + rwind++, p.ObrazProgramName);
            }
            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }
        public static byte[] GetApplicationPDF_ProfileAppendix_1kurs(List<ShortApplicationDetails> lst, string LicenseProgramName, string FIO, string dirPath, int Num)
        {
            MemoryStream ms = new MemoryStream();
            string dotName = "PriorityProfiles2015.pdf";

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            AcroFields acrFlds = pdfStm.AcroFields;

            acrFlds.SetField("Num", Num.ToString());
            acrFlds.SetField("FIO", FIO);

            acrFlds.SetField("LicenseProgram", LicenseProgramName);
            acrFlds.SetField("ObrazProgram", lst.First().ObrazProgramName);

            int i = 0;
            foreach (var Prof in lst.OrderBy(x => x.InnerEntryInEntryPriority))
            {
                i++;
                //если других программ нет, то нет смысла показывать её название.
                bool bShowObrazProgram = lst.Where(x => x.ObrazProgramName != Prof.ObrazProgramName).Count() > 0;
                acrFlds.SetField("Profile" + i.ToString(), (bShowObrazProgram ? Prof.ObrazProgramName + " /\n" : "") + Prof.ProfileName);
            }

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }

        public static byte[] GetApplicationPDF_Agreement_Mag(string FIO, bool bSex, string dirPath, string sVersion)
        {
            MemoryStream ms = new MemoryStream();
            string dotName = string.Format("ApplicationAgreement_MagSex{0}.pdf", bSex ? "1" : "0");

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            AcroFields acrFlds = pdfStm.AcroFields;

            acrFlds.SetField("FIO", FIO);

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }
        public static byte[] GetApplicationPDF_Agreement_1k(string FIO, bool bSex, string dirPath, string sVersion)
        {
            MemoryStream ms = new MemoryStream();
            string dotName = string.Format("ApplicationAgreement_1kSex{0}.pdf", bSex ? "1" : "0");

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            AcroFields acrFlds = pdfStm.AcroFields;

            acrFlds.SetField("FIO", FIO);

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }

        public static byte[] GetApplicationPDF_FirstPage(List<ShortAppcation> lst, List<ShortAppcation> lstFullSource, string dirPath, string dotName, string FIO, string Version, string code, bool isMag)
        {
            MemoryStream ms = new MemoryStream();

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);

            //добавляем штрихкод
            Barcode128 barcode = new Barcode128();
            barcode.Code = code;
            PdfContentByte cb = pdfStm.GetOverContent(1);
            iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
            if (isMag)
                img.SetAbsolutePosition(420, 720);
            else
                img.SetAbsolutePosition(120, 775);
            cb.AddImage(img);

            AcroFields acrFlds = pdfStm.AcroFields;
            acrFlds.SetField("Version", Version);
            acrFlds.SetField("FIO", FIO);

            int rwind = 1;
            foreach (var p in lst.OrderBy(x => x.Priority))
            {
                acrFlds.SetField("Priority" + rwind, p.Priority.ToString());
                acrFlds.SetField("Profession" + rwind, p.LicenseProgramName);
                acrFlds.SetField("ObrazProgram" + rwind, p.ObrazProgramName);
                acrFlds.SetField("Specialization" + rwind, p.HasInnerPriorities ? "Приложение к заявлению № " + p.InnerPrioritiesNum : p.ProfileName);
                acrFlds.SetField("StudyForm" + p.StudyFormId.ToString() + rwind.ToString(), "1");
                acrFlds.SetField("StudyBasis" + p.StudyBasisId.ToString() + rwind.ToString(), "1");

                string sQuota = "";
                if (p.IsCrimea)
                    sQuota = "в рамках квоты мест для лиц, постоянно проживающих в Крыму";
                else if (p.IsForeign)
                    sQuota = "в рамках квоты мест для обучения иностранных граждан и лиц без гражданства";
                acrFlds.SetField("QuotaType" + rwind.ToString(), sQuota);

                rwind++;
            }

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }
        public static byte[] GetApplicationPDF_NextPage(List<ShortAppcation> lst, List<ShortAppcation> lstFullSource, string dirPath, string dotName, string FIO)
        {
            MemoryStream ms = new MemoryStream();

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
            AcroFields acrFlds = pdfStm.AcroFields;
            int rwind = 1;
            foreach (var p in lst.OrderBy(x => x.Priority))
            {
                if (p.StudyBasisId == 1)
                    acrFlds.SetField("Priority" + rwind, p.Priority.ToString());

                acrFlds.SetField("Profession" + rwind, p.LicenseProgramName);
                acrFlds.SetField("ObrazProgram" + rwind, p.ObrazProgramName);
                acrFlds.SetField("Specialization" + rwind, p.HasInnerPriorities ? "Приложение к заявлению № " + p.InnerPrioritiesNum : p.ProfileName);
                acrFlds.SetField("StudyForm" + p.StudyFormId.ToString() + rwind.ToString(), "1");
                acrFlds.SetField("StudyBasis" + p.StudyBasisId.ToString() + rwind.ToString(), "1");

                string sQuota = "";
                if (p.IsCrimea)
                    sQuota = "в рамках квоты мест для лиц, постоянно проживающих в Крыму";
                else if (p.IsForeign)
                    sQuota = "в рамках квоты мест для обучения иностранных граждан и лиц без гражданства";
                acrFlds.SetField("QuotaType" + rwind.ToString(), sQuota);

                rwind++;
            }

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }

        //1курс-магистратура иностранцы (AbitTypeId = 2)
        //PREFIX 3000000
        public static byte[] GetApplicationPDFForeign(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            try
            {
                using (OnlinePriemEntities context = new OnlinePriemEntities())
                {
                    var abit = (from x in context.Application
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                where x.Id == appId
                                select new
                                {
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    x.HostelEduc
                                }).FirstOrDefault();

                    string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                    DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", abit.PersonId } });
                    string email = tbl.Rows[0].Field<string>("Email");

                    query = "SELECT LanguageNameRus, LevelNameRus FROM extForeignPersonLanguage WHERE PersonId=@PersonId";
                    DataTable tblLangs = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", abit.PersonId } });
                    string Language = string.Join(",", (from DataRow rw in tblLangs.Rows select rw.Field<string>("LanguageNameRus") + " - " + rw.Field<string>("LevelNameRus") + ","));

                    var person = (from x in context.Person
                                  where x.Id == abit.PersonId
                                  select new
                                  {
                                      x.Surname,
                                      x.Name,
                                      x.SecondName,
                                      x.PersonAddInfo.HostelAbit,
                                      x.BirthDate,
                                      BirthPlace = x.BirthPlace ?? "",
                                      Sex = x.Sex,
                                      Nationality = x.Nationality.Name,
                                      Country = x.PersonContacts.Country.Name,
                                      PassportType = x.PassportType.Name,
                                      x.PassportSeries,
                                      x.PassportNumber,
                                      x.PassportAuthor,
                                      x.PassportDate,
                                      Address = x.PersonContacts.ForeignAddressInfo,
                                      x.PersonContacts.Phone,
                                      x.PersonContacts.Mobiles,
                                      Language = Language,
                                      AddInfo = x.PersonAddInfo.AddInfo,
                                      Parents = x.PersonAddInfo.Parents,
                                      //Qualification = x.PersonHighEducationInfo.Qualification != null ? x.PersonHighEducationInfo.Qualification.Name : "",
                                  }).FirstOrDefault();

                    var personEducation = context.PersonEducationDocument.Where(x => x.PersonId == PersonId)
                        .Select(x => new
                        {
                            x.SchoolExitYear,
                            x.SchoolName,
                            x.IsEqual,
                            x.EqualDocumentNumber,
                            CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                            x.SchoolTypeId,
                            EducationDocumentSeries = x.Series,
                            EducationDocumentNumber = x.Number,
                        }).FirstOrDefault();

                    MemoryStream ms = new MemoryStream();
                    string dotName;

                    if (abit.EntryType == 2)//mag
                        dotName = "MagApplicationForeign.pdf";
                    else
                        dotName = "ApplicationForeign.pdf";

                    byte[] templateBytes;
                    using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                    {
                        templateBytes = new byte[fs.Length];
                        fs.Read(templateBytes, 0, templateBytes.Length);
                    }

                    PdfReader pdfRd = new PdfReader(templateBytes);
                    PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                    pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                    AcroFields acrFlds = pdfStm.AcroFields;
                    string code = (3000000 + abit.Barcode).ToString();

                    //добавляем штрихкод
                    Barcode128 barcode = new Barcode128();
                    barcode.Code = code;
                    PdfContentByte cb = pdfStm.GetOverContent(1);
                    iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                    if (abit.EntryType == 2)
                        img.SetAbsolutePosition(420, 720);
                    else
                        img.SetAbsolutePosition(440, 740);
                    cb.AddImage(img);

                    acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                    acrFlds.SetField("Profession", abit.ProfessionCode + " " + abit.Profession);
                    //if (abit.EntryType != 2)
                    acrFlds.SetField("Specialization", abit.Specialization);
                    acrFlds.SetField("Faculty", abit.Faculty);
                    acrFlds.SetField("ObrazProgram", abit.ObrazProgram);
                    acrFlds.SetField("StudyForm" + abit.StudyFormId, "1");
                    acrFlds.SetField("StudyBasis" + abit.StudyBasisId, "1");

                    if (abit.HostelEduc)
                        acrFlds.SetField("HostelEducYes", "1");
                    else
                        acrFlds.SetField("HostelEducNo", "1");

                    if (person.HostelAbit ?? false)
                        acrFlds.SetField("HostelAbitYes", "1");
                    else
                        acrFlds.SetField("HostelAbitNo", "1");

                    if (person.Sex)
                        acrFlds.SetField("Male", "1");
                    else
                        acrFlds.SetField("Female", "1");

                    acrFlds.SetField("BirthDate", person.BirthDate.Value.ToShortDateString());
                    acrFlds.SetField("BirthPlace", person.BirthPlace);
                    acrFlds.SetField("Nationality", person.Nationality);

                    acrFlds.SetField("PassportSeries", person.PassportSeries);
                    acrFlds.SetField("PassportNumber", person.PassportNumber);
                    acrFlds.SetField("PassportDate", person.PassportDate.Value.ToShortDateString());
                    acrFlds.SetField("PassportAuthor", person.PassportAuthor);

                    string[] splitStr = PDFUtils.GetSplittedStrings(person.Address, 30, 70, 2);
                    for (int i = 1; i <= 2; i++)
                        acrFlds.SetField("Address" + i.ToString(), splitStr[i - 1]);

                    string phones = (person.Phone ?? "") + ", e-mail: " + email + ", " + (person.Mobiles ?? "");
                    splitStr = PDFUtils.GetSplittedStrings(phones, 30, 70, 2);
                    for (int i = 1; i <= 2; i++)
                        acrFlds.SetField("Phone" + i.ToString(), splitStr[i - 1]);

                    splitStr = PDFUtils.GetSplittedStrings(person.Parents, 40, 70, 3);
                    for (int i = 1; i <= 3; i++)
                        acrFlds.SetField("Parents" + i.ToString(), splitStr[i - 1]);

                    acrFlds.SetField("ExitYear", personEducation.SchoolExitYear);
                    acrFlds.SetField("School", personEducation.SchoolName ?? "");
                    //acrFlds.SetField("Original", "0");
                    //acrFlds.SetField("Copy", "0");

                    acrFlds.SetField("Attestat", personEducation.EducationDocumentSeries + " " + personEducation.EducationDocumentNumber);

                    acrFlds.SetField("Language", person.Language ?? "");
                    acrFlds.SetField("CountryEduc", personEducation.CountryEduc ?? "");
                    acrFlds.SetField("Extra", person.AddInfo ?? "");

                    if (personEducation.IsEqual)
                    {
                        acrFlds.SetField("HasEqual", "1");
                        acrFlds.SetField("EqualityDocument", personEducation.EqualDocumentNumber);
                    }
                    else
                        acrFlds.SetField("NoEqual", "1");

                    acrFlds.SetField("StudyForm" + abit.StudyFormId, "1");

                    if (abit.EntryType != 2)//no mag application
                    {
                        if (abit.StudyLevelId == 16)
                            acrFlds.SetField("chbBak", "1");
                        else
                            acrFlds.SetField("chbSpec", "1");

                        if (personEducation.SchoolTypeId != 4)
                            acrFlds.SetField("NoHE", "1");
                        else
                        {
                            acrFlds.SetField("HasHE", "1");
                            acrFlds.SetField("HEName", personEducation.SchoolName);
                        }
                    }
                    else
                        acrFlds.SetField("Qualification", "1");

                    pdfStm.FormFlattening = true;
                    pdfStm.Close();
                    pdfRd.Close();

                    return ms.ToArray();
                }
            }
            catch
            {
                return System.Text.ASCIIEncoding.UTF8.GetBytes("Еrror");
            }
        }
        //перевод (AbitTypeId = 3)
        public static byte[] GetApplicationPDFTransfer(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                join Semester in context.Semester on Entry.SemesterId equals Semester.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    SemesterName = Semester.Name,
                                    EducYear = Semester.EducYear
                                }).OrderBy(x => x.Priority).FirstOrDefault();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              join PersonCurrentEducation in context.PersonCurrentEducation on PersonId equals PersonCurrentEducation.PersonId
                              join Semester in context.Semester on PersonCurrentEducation.SemesterId equals Semester.Id
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  //x.PersonHighEducationInfo.ProgramName,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.PersonCurrentEducation.HasAccreditation,
                                  x.PersonCurrentEducation.AccreditationDate,
                                  x.PersonCurrentEducation.AccreditationNumber,
                                  x.PersonCurrentEducation.HasScholarship,
                                  x.PersonAddInfo.Parents,
                                  x.PersonAddInfo.AddInfo,
                                  x.PersonCurrentEducation.StudyLevelId,
                                  Semester.EducYear
                              }).FirstOrDefault();

                var personEducation = context.PersonEducationDocument.Where(x => x.PersonId == PersonId)
                    .Select(x => new
                    {
                        x.SchoolName,
                        x.SchoolExitYear,
                        x.Number,
                        x.Series,
                    }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationTransfer.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;
                string code = (3000000 + abitList.Barcode).ToString();

                //добавляем штрихкод
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);

                img.SetAbsolutePosition(280, 780);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("Profession", abitList.ProfessionCode + " " + abitList.Profession);
                acrFlds.SetField("Specialization", abitList.Specialization);
                acrFlds.SetField("Faculty", abitList.Faculty);
                acrFlds.SetField("ObrazProgram", abitList.ObrazProgram);
                acrFlds.SetField("StudyForm" + abitList.StudyFormId, "1");
                acrFlds.SetField("StudyBasis" + abitList.StudyBasisId, "1");

                acrFlds.SetField("Course", abitList.EducYear.ToString());
                acrFlds.SetField("Semester", abitList.SemesterName);

                switch (abitList.StudyLevelId)
                {
                    case 16: { acrFlds.SetField("chbBak", "1"); break; }
                    case 17: { acrFlds.SetField("chbMag", "1"); break; }
                    case 18: { acrFlds.SetField("chbSpec", "1"); break; }
                }

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");

                acrFlds.SetField("BirthDate", person.BirthDate.Value.ToShortDateString());
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);
                acrFlds.SetField("PassportDate", person.PassportDate.Value.ToShortDateString());
                acrFlds.SetField("PassportAuthor", person.PassportAuthor);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                      string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                      person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                      person.Flat == string.Empty ? "" : "кв. " + person.Flat);
                string[] splitStr, strSplit;
                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                string phones = (person.Phone ?? "") + ", e-mail: " + email + ", " + (person.Mobiles ?? "");

                strSplit = GetSplittedStrings(phones, 30, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("Phone" + i.ToString(), strSplit[i - 1]);

                strSplit = GetSplittedStrings(person.Parents, 50, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("Parents" + i.ToString(), strSplit[i - 1]);

                acrFlds.SetField("CurrentEducationName", personEducation.SchoolName);
                acrFlds.SetField(person.HasAccreditation ? "HasAccred" : "NoAccred", "1");
                string AccredInfo = (person.AccreditationNumber ?? "") +
                    (person.AccreditationDate.HasValue ? " от " + person.AccreditationDate.Value.ToShortDateString() : "");
                acrFlds.SetField("EducationAccreditationNumber", person.HasAccreditation ? AccredInfo : "");
                if (abitList.SemesterName != null)
                    acrFlds.SetField("CurrentCourse", person.EducYear.ToString());

                switch (person.StudyLevelId)
                {
                    case 16: { acrFlds.SetField("CurrentBak", "1"); break; }
                    case 17: { acrFlds.SetField("CurrentMag", "1"); break; }
                    case 18: { acrFlds.SetField("CurrentSpec", "1"); break; }
                }

                acrFlds.SetField("ExitYear", "");//person.SchoolExitYear ?? "");
                acrFlds.SetField("School", "");//person.SchoolName ?? "");
                acrFlds.SetField("EducationDocument", ""); //(person.Series ?? "") + " " + (person.Number ?? ""));
                if (person.HasScholarship)
                    acrFlds.SetField("HasScholarship", "1");
                else
                    acrFlds.SetField("NoScholarship", "1");

                //acrFlds.SetField("Extra", person.AddInfo ?? "");
                //acrFlds.SetField("Copy", "1");

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }
        //перевод иностранцев (AbitTypeId = 4)
        public static byte[] GetApplicationPDFTransferForeign(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                join Semester in context.Semester on Entry.SemesterId equals Semester.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    SemesterName = Semester.Name,
                                    EducYear = Semester.EducYear
                                }).OrderBy(x => x.Priority).FirstOrDefault();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              join PersonCurrentEducation in context.PersonCurrentEducation on PersonId equals PersonCurrentEducation.PersonId
                              join Semester in context.Semester on PersonCurrentEducation.SemesterId equals Semester.Id
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  //x.PersonHighEducationInfo.ProgramName,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.PersonCurrentEducation.HasAccreditation,
                                  x.PersonCurrentEducation.AccreditationDate,
                                  x.PersonCurrentEducation.AccreditationNumber,
                                  x.PersonCurrentEducation.HasScholarship,
                                  x.PersonAddInfo.Parents,
                                  x.PersonAddInfo.AddInfo,
                                  x.PersonCurrentEducation.StudyLevelId,
                                  SemesterName = Semester.Name,
                                  Semester.EducYear,
                              }).FirstOrDefault();

                var personEducation = context.PersonEducationDocument.Where(x => x.PersonId == PersonId)
                    .Select(x => new
                    {
                        x.SchoolName,
                        x.SchoolExitYear,
                        x.Number,
                        x.Series,
                        x.CountryEducId,
                        x.EqualDocumentNumber,
                        x.IsEqual,
                    }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationTransferForeign.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;
                string code = (4000000 + abitList.Barcode).ToString();

                //добавляем штрихкод
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);

                img.SetAbsolutePosition(280, 780);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("Profession", abitList.ProfessionCode + " " + abitList.Profession);
                acrFlds.SetField("Specialization", abitList.Specialization);
                acrFlds.SetField("Faculty", abitList.Faculty);
                acrFlds.SetField("ObrazProgram", abitList.ObrazProgram);
                acrFlds.SetField("StudyForm" + abitList.StudyFormId, "1");
                acrFlds.SetField("StudyBasis" + abitList.StudyBasisId, "1");

                acrFlds.SetField("Course", abitList.EducYear.ToString());
                acrFlds.SetField("Semester", abitList.SemesterName);

                switch (abitList.StudyLevelId)
                {
                    case 16: { acrFlds.SetField("chbBak", "1"); break; }
                    case 17: { acrFlds.SetField("chbMag", "1"); break; }
                    case 18: { acrFlds.SetField("chbSpec", "1"); break; }
                }

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");

                acrFlds.SetField("BirthDate", person.BirthDate.Value.ToShortDateString());
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);
                acrFlds.SetField("PassportDate", person.PassportDate.Value.ToShortDateString());
                acrFlds.SetField("PassportAuthor", person.PassportAuthor);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                      string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                      person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                      person.Flat == string.Empty ? "" : "кв. " + person.Flat);
                string[] splitStr, strSplit;
                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                string phones = (person.Phone ?? "") + ", e-mail: " + email + ", " + (person.Mobiles ?? "");

                strSplit = GetSplittedStrings(phones, 30, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("Phone" + i.ToString(), strSplit[i - 1]);

                strSplit = GetSplittedStrings(phones, 70, 70, 2);
                for (int i = 1; i < 3; i++)
                    acrFlds.SetField("Phone" + i.ToString(), strSplit[i - 1]);

                strSplit = GetSplittedStrings(person.Parents, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Parents" + i.ToString(), strSplit[i - 1]);

                /*strSplit = GetSplittedStrings(person.SocialStatus, 50, 70, 2);*/
                for (int i = 1; i < 3; i++)
                    acrFlds.SetField("SocialStatus" + i.ToString(), "");
                acrFlds.SetField("MaritalStatus", "");

                acrFlds.SetField("CurrentEducationName", personEducation.SchoolName);
                acrFlds.SetField(person.HasAccreditation ? "HasAccred" : "NoAccred", "1");
                string AccredInfo = (person.AccreditationNumber ?? "") +
                    (person.AccreditationDate.HasValue ? person.AccreditationDate.Value.ToShortDateString() : "");
                acrFlds.SetField("EducationAccreditationNumber", person.HasAccreditation ? AccredInfo : "");
                if (abitList.SemesterName != null)
                    acrFlds.SetField("CurrentSemester", person.EducYear.ToString() + " курс, " + person.SemesterName);

                switch (person.StudyLevelId)
                {
                    case 16: { acrFlds.SetField("CurrentBak", "1"); break; }
                    case 17: { acrFlds.SetField("CurrentMag", "1"); break; }
                    case 18: { acrFlds.SetField("CurrentSpec", "1"); break; }
                }

                acrFlds.SetField("ExitYear", "");//person.SchoolExitYear ?? "");
                acrFlds.SetField("School", "");//person.SchoolName ?? "");
                acrFlds.SetField("SchoolName", "");//person.SchoolName ?? "");
                acrFlds.SetField("EducationDocument", "");//(person.Series ?? "") + (person.Number ?? ""));
                acrFlds.SetField("CountryEduc", "");//person.CountryEducId.HasValue ? person.CountryEduc.Name : "");
                /*
                if (person.HasScholarship ?? false)
                    acrFlds.SetField("HasScholarship", "1");
                else
                    acrFlds.SetField("NoScholarship", "1");*/
                acrFlds.SetField("Extra", person.AddInfo ?? "");

                strSplit = GetSplittedStrings(person.Parents, 30, 70, 3);
                for (int i = 1; i < 4; i++)
                    acrFlds.SetField("Parents", strSplit[i - 1]);

                if (personEducation.IsEqual && personEducation.CountryEducId != 193)
                {
                    acrFlds.SetField("HasEqual", "1");
                    acrFlds.SetField("EqualNumber", personEducation.EqualDocumentNumber);
                }
                else
                {
                    acrFlds.SetField("NoEqual", "1");
                }
                //acrFlds.SetField("Copy", "1");

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }
        //восстановление (AbitTypeId = 5)
        public static byte[] GetApplicationPDFRecover(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                join Semester in context.Semester on Entry.SemesterId equals Semester.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    SemesterName = Semester.Name,
                                    EducYear = Semester.EducYear
                                }).OrderBy(x => x.Priority).FirstOrDefault();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              join PersonDisorderInfo in context.PersonDisorderInfo on PersonId equals PersonDisorderInfo.PersonId into Sem2
                              from Sem in Sem2.DefaultIfEmpty()
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  //x.PersonHighEducationInfo.ProgramName,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  //Qualification = x.PersonHighEducationInfo.Qualification != null ? x.PersonHighEducationInfo.Qualification.Name : "",
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.PersonDisorderInfo.YearOfDisorder,
                                  x.PersonDisorderInfo.EducationProgramName
                              }).FirstOrDefault();

                //var personEducation = context.PersonEducationDocument.Where(x => x.PersonId == PersonId).Select(x =>
                //    new
                //    {
                //        x.SchoolExitYear,
                //        x.SchoolName,
                //        x.StartEnglish,
                //        x.EnglishMark,
                //        x.IsEqual,
                //        x.EqualDocumentNumber,
                //        CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                //        x.CountryEducId,
                //        x.SchoolTypeId,
                //        EducationDocumentSeries = x.Series,
                //        EducationDocumentNumber = x.Number,
                //        x.AttestatRegion,
                //        x.AttestatSeries,
                //        x.AttestatNumber,
                //    }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationRecover.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;
                string code = (5000000 + abitList.Barcode).ToString();

                //добавляем штрихкод
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);

                img.SetAbsolutePosition(280, 780);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("Profession", abitList.ProfessionCode + " " + abitList.Profession);
                acrFlds.SetField("Specialization", abitList.Specialization);
                acrFlds.SetField("Faculty", abitList.Faculty);
                acrFlds.SetField("ObrazProgram", abitList.ObrazProgram);
                acrFlds.SetField("StudyForm" + abitList.StudyFormId, "1");
                acrFlds.SetField("StudyBasis" + abitList.StudyBasisId, "1");

                acrFlds.SetField("Course", abitList.EducYear.ToString());
                acrFlds.SetField("Semester", abitList.SemesterName);

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");

                acrFlds.SetField("BirthDate", person.BirthDate.Value.ToShortDateString());
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);
                acrFlds.SetField("PassportDate", person.PassportDate.Value.ToShortDateString());
                acrFlds.SetField("PassportAuthor", person.PassportAuthor);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);
                string[] splitStr, strSplit;
                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                string phones = (person.Phone ?? "") + ", e-mail: " + email + ", " + (person.Mobiles ?? "");

                strSplit = GetSplittedStrings(phones, 30, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("Phone" + i.ToString(), strSplit[i - 1]);

                acrFlds.SetField("DisorderYear", person.YearOfDisorder);

                strSplit = GetSplittedStrings(person.EducationProgramName ?? "", 80, 80, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("DisorderProgram" + i.ToString(), strSplit[i - 1]);

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }
        //перевод на бюджет (AbitTypeId = 6)
        public static byte[] GetApplicationPDFChangeStudyBasis(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                join Semester in context.Semester on Entry.SemesterId equals Semester.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    SemesterName = Semester.Name,
                                    EducYear = Semester.EducYear
                                }).OrderBy(x => x.Priority).FirstOrDefault();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              //join PersonChangeStudyFormReason in context.PersonChangeStudyFormReason on PersonId equals PersonChangeStudyFormReason.PersonId
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  //x.PersonHighEducationInfo.ProgramName,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.PersonChangeStudyFormReason.Reason,
                                  x.PersonCurrentEducation.ProfileName
                              }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationChangeStudyBasis.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;
                string code = (6000000 + abitList.Barcode).ToString();

                //добавляем штрихкод
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                if (abitList.EntryType == 2)
                    img.SetAbsolutePosition(420, 720);
                else
                    img.SetAbsolutePosition(440, 740);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("Profession", abitList.ProfessionCode + " " + abitList.Profession);
                acrFlds.SetField("Specialization", person.ProfileName);
                acrFlds.SetField("Faculty", abitList.Faculty);
                acrFlds.SetField("ObrazProgram", abitList.ObrazProgram);
                acrFlds.SetField("StudyForm" + abitList.StudyFormId, "1");
                acrFlds.SetField("StudyBasis" + abitList.StudyBasisId, "1");
                acrFlds.SetField("Course", abitList.EducYear.ToString());
                acrFlds.SetField("Semester", abitList.SemesterName);
                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");

                string Reason = "";
                if (!String.IsNullOrEmpty(person.Reason))
                    Reason = person.Reason;

                string[] ss = GetSplittedStrings(Reason, 95, 95, 3);
                for (int i = 1; i <= 3; i++)
                {
                    acrFlds.SetField("Reason" + i, ss[i - 1]);
                }

                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("BirthDate", person.BirthDate.Value.ToShortDateString());
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);
                acrFlds.SetField("PassportDate", person.PassportDate.Value.ToShortDateString());
                acrFlds.SetField("PassportAuthor", person.PassportAuthor);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);
                string[] splitStr, strSplit;
                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                string phones = (person.Phone ?? "") + ", e-mail: " + email + ", " + (person.Mobiles ?? "");

                strSplit = GetSplittedStrings(phones, 30, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("Phone" + i.ToString(), strSplit[i - 1]);

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }
        //смена обр. программы (AbitTypeId = 7)
        public static byte[] GetApplicationPDFChangeObrazProgram(Guid appId, string dirPath, bool isMag, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                join Semester in context.Semester on Entry.SemesterId equals Semester.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    SemesterName = Semester.Name,
                                    EducYear = Semester.EducYear
                                }).OrderBy(x => x.Priority).FirstOrDefault();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              join Sem in context.Semester on x.PersonCurrentEducation.SemesterId equals Sem.Id into Sem2
                              from Sem in Sem2.DefaultIfEmpty()
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  //x.PersonHighEducationInfo.ProgramName,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.PersonCurrentEducation.LicenseProgramId,
                                  x.PersonCurrentEducation.SemesterId,
                                  Sem.EducYear
                              }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationChangeObrazProgram.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;
                string code = (7000000 + abitList.Barcode).ToString();

                //добавляем штрихкод
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                if (abitList.EntryType == 2)
                    img.SetAbsolutePosition(420, 720);
                else
                    img.SetAbsolutePosition(440, 740);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("Profession", abitList.ProfessionCode + " " + abitList.Profession);
                acrFlds.SetField("ObrazProgram", abitList.ObrazProgram);
                //if (abit.EntryType != 2)
                acrFlds.SetField("Specialization", abitList.Specialization);
                acrFlds.SetField("Faculty", abitList.Faculty);
                acrFlds.SetField("ObrazProgram", abitList.ObrazProgram);
                acrFlds.SetField("StudyForm" + abitList.StudyFormId, "1");
                acrFlds.SetField("StudyBasis" + abitList.StudyBasisId, "1");

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");

                acrFlds.SetField("BirthDate", person.BirthDate.Value.ToShortDateString());
                acrFlds.SetField("BirthPlace", person.BirthPlace);

                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);
                acrFlds.SetField("PassportDate", person.PassportDate.Value.ToShortDateString());
                acrFlds.SetField("PassportAuthor", person.PassportAuthor);

                acrFlds.SetField("Nationality", person.Nationality);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);
                string[] splitStr, strSplit;
                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                string phones = (person.Phone ?? "") + ", e-mail: " + email + ", " + (person.Mobiles ?? "");

                strSplit = GetSplittedStrings(phones, 70, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("Phone" + i.ToString(), strSplit[i - 1]);

                acrFlds.SetField("CurrentProfession", context.Entry.Where(x => x.LicenseProgramId == person.LicenseProgramId).Select(x => x.LicenseProgramName).FirstOrDefault());
                acrFlds.SetField("CurrentCourse", person.EducYear.HasValue ? person.EducYear.ToString() : "-");

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }

       /* public static byte[] GetApplicationPDF_AG(Guid appId, string dirPath, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                join sLevel in context.SP_StudyLevel on Entry.StudyLevelId equals sLevel.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Profession = Entry.LicenseProgramName,
                                    ObrazProgram = Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyLevelGroupId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address,
                                    sLevel.ClassName,
                                    sLevel.Duration,
                                }).OrderBy(x => x.Priority).ToList();

                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelEduc,
                              }).FirstOrDefault();

                

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationAG_2015.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;
                string code = (800000 + person.Barcode).ToString();

                //добавляем штрихкод
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                img.SetAbsolutePosition(70, 750);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());

                acrFlds.SetField("EntryClass", abitList.First().ClassName.ToString());
                acrFlds.SetField("ObrazProgramYear", abitList.First().Duration.ToString());

                acrFlds.SetField("Date", DateTime.Now.ToShortDateString());

                for (int i = 0; i < abitList.Count; i++)
                {
                    var Exams = Util.GetExamList(abitList[i].Id);
                    string _sI = (i + 1).ToString();
                    acrFlds.SetField("Profession"+_sI, abitList[i].Profession);
                    acrFlds.SetField("ObrazProgram" + _sI, abitList[i].ObrazProgram);
                    
                    if (Exams.Count == 0)
                        acrFlds.SetField("ManualExam" + _sI, "нет");
                    else
                    {
                        string ExamNames = "";
                        foreach (var x in Exams)
                        {
                            ExamNames += (x.ExamInBlockList.Where(ex => ex.Value.ToString() == x.SelectedExamInBlockId.ToString()).Select(ex => ex.Text).FirstOrDefault()) +", ";
                        }
                        if (ExamNames.Length >2 )
                            ExamNames = ExamNames.Substring(0, ExamNames.Length-2);
                        acrFlds.SetField("ManualExam" + _sI, ExamNames);
                    }
                    acrFlds.SetField("chbSchoolLevel1" + _sI, abitList[i].StudyLevelGroupId == 6 ? "1" : "0");
                    acrFlds.SetField("chbSchoolLevel2" + _sI, abitList[i].StudyLevelGroupId == 7 ? "1" : "0");
                }
                if (person.HostelEduc)
                    acrFlds.SetField("HostelAbitYes", "1");
                else
                    acrFlds.SetField("HostelAbitNo", "1");

                //string query = "SELECT PrintName, DocumentNumber FROM AG_AllPriveleges WHERE PersonId=@PersonId";
                //DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", abitList.First().PersonId } });
                //var privileges =
                //    (from DataRow rw in tbl.Rows
                //     select rw.Field<string>("PrintName") + " " + rw.Field<string>("DocumentNumber"));
                //string AllPrivileges = privileges.DefaultIfEmpty().Aggregate((x, next) => x + "; " + next) ?? "";
                //int index = 0, startindex = 0;
                //for (int i = 1; i <= 6; i++)
                //{
                //    if (AllPrivileges.Length > startindex && startindex >= 0)
                //    {
                //        int rowLength = 100;//длина строки, разная у первых строк
                //        if (i > 1) //длина остальных строк одинакова
                //            rowLength = 100;
                //        index = startindex + rowLength;
                //        if (index < AllPrivileges.Length)
                //        {
                //            index = AllPrivileges.IndexOf(" ", index);
                //            string val = index > 0 ?
                //                AllPrivileges.Substring(startindex, index - startindex)
                //                : AllPrivileges.Substring(startindex);
                //            acrFlds.SetField("AddDocs" + i.ToString(), val);
                //        }
                //        else
                //            acrFlds.SetField("AddDocs" + i.ToString(),
                //                AllPrivileges.Substring(startindex));
                //    }
                //    startindex = index;
                //}

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }*/

        public static byte[] GetApplicationPDF_SPO(Guid appId, string dirPath, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    IsGosLine = Entry.IsForeign && Entry.StudyBasisId == 1,
                                    Entry.IsCrimea
                                    //x.IsGosLine
                                }).ToList();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  AddInfo = x.PersonAddInfo.AddInfo,
                                  Parents = x.PersonAddInfo.Parents,
                                  HasPrivileges = x.PersonAddInfo.HasPrivileges ?? false,
                                  x.PersonAddInfo.ReturnDocumentTypeId,
                                  SportQualificationName = x.PersonSportQualification.SportQualification.Name,
                                  x.PersonSportQualification.SportQualificationId,
                                  x.PersonSportQualification.SportQualificationLevel,
                                  x.PersonSportQualification.OtherSportQualification,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.HasRussianNationality,
                                  x.PersonAddInfo.StartEnglish,
                                  x.PersonAddInfo.EnglishMark,
                                  Language = x.PersonAddInfo.Language.Name,
                                  x.PersonAddInfo.HasTRKI,
                                  x.PersonAddInfo.TRKICertificateNumber,
                              }).FirstOrDefault();

                var personEducation = context.PersonEducationDocument.Where(x => x.PersonId == PersonId)
                    .Select(x => new
                    {
                        x.SchoolExitYear,
                        x.SchoolName,
                        x.IsEqual,
                        x.EqualDocumentNumber,
                        CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                        x.CountryEducId,
                        x.SchoolTypeId,
                        EducationDocumentSeries = x.Series,
                        EducationDocumentNumber = x.Number,
                    }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationSPO_page3.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                List<byte[]> lstFiles = new List<byte[]>();

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                if (abitList.Where(x => x.IsGosLine).Count() > 0)
                    acrFlds.SetField("IsGosLine", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                if (person.BirthDate.HasValue)
                {
                    acrFlds.SetField("BirthDateYear", person.BirthDate.Value.Year.ToString("D2"));
                    acrFlds.SetField("BirthDateMonth", person.BirthDate.Value.Month.ToString("D2"));
                    acrFlds.SetField("BirthDateDay", person.BirthDate.Value.Day.ToString("D2"));
                }
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);

                //dd.MM.yyyy :12.05.2000
                string[] splitStr = GetSplittedStrings(person.PassportAuthor + " " + person.PassportDate.Value.ToString("dd.MM.yyyy"), 60, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("PassportAuthor" + i, splitStr[i - 1]);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);

                if (person.HasRussianNationality)
                    acrFlds.SetField("HasRussianNationalityYes", "1");
                else
                    acrFlds.SetField("HasRussianNationalityNo", "1");

                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());
                if (person.StartEnglish)
                    acrFlds.SetField("chbEnglishYes", "1");
                else
                    acrFlds.SetField("chbEnglishNo", "1");

                acrFlds.SetField("Phone", person.Phone);
                acrFlds.SetField("Email", email);
                acrFlds.SetField("Mobiles", person.Mobiles);

                acrFlds.SetField("ExitYear", personEducation.SchoolExitYear.ToString());
                splitStr = GetSplittedStrings(personEducation.SchoolName ?? "", 50, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("School" + i, splitStr[i - 1]);

                //только у магистров
                var HEInfo = context.PersonEducationDocument
                    .Where(x => x.PersonId == PersonId && x.PersonHighEducationInfo != null)
                    .Select(x => new { x.PersonHighEducationInfo.ProgramName, Qualification = x.PersonHighEducationInfo.Qualification.Name }).FirstOrDefault();

                if (HEInfo != null)
                {
                    acrFlds.SetField("HEProfession", HEInfo.ProgramName ?? "");
                    acrFlds.SetField("Qualification", HEInfo.Qualification ?? "");

                    acrFlds.SetField("Original", "0");
                    acrFlds.SetField("Copy", "0");
                    acrFlds.SetField("CountryEduc", personEducation.CountryEduc ?? "");
                    acrFlds.SetField("Language", person.Language ?? "");
                }
                //SportQualification
                if (person.SportQualificationId == 0)
                    acrFlds.SetField("SportQualification", "нет");
                else if (person.SportQualificationId == 44)
                    acrFlds.SetField("SportQualification", person.OtherSportQualification);
                else
                    acrFlds.SetField("SportQualification", person.SportQualificationName + "; " + person.SportQualificationLevel);

                string extraPerson = person.Parents ?? "";
                splitStr = GetSplittedStrings(extraPerson, 70, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Parents" + i.ToString(), splitStr[i - 1]);

                string Attestat = personEducation.SchoolTypeId == 1 ? ("аттестат серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? "")) :
                        ("диплом серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? ""));
                acrFlds.SetField("Attestat", Attestat);
                acrFlds.SetField("Extra", person.AddInfo ?? "");

                if (personEducation.IsEqual && personEducation.CountryEducId != 193)
                {
                    acrFlds.SetField("IsEqual", "1");
                    acrFlds.SetField("EqualSertificateNumber", personEducation.EqualDocumentNumber);
                }
                else
                {
                    acrFlds.SetField("NoEqual", "1");
                }

                if (person.HasPrivileges)
                    acrFlds.SetField("HasPrivileges", "1");

                acrFlds.SetField("ReturnDocumentType" + person.ReturnDocumentTypeId, "1");
                if ((personEducation.SchoolTypeId != 2) && (personEducation.SchoolTypeId != 5))//SSUZ & SPO
                    acrFlds.SetField("NoEduc", "1");
                else
                {
                    acrFlds.SetField("HasEduc", "1");
                    acrFlds.SetField("HighEducation", personEducation.SchoolName);
                }

                //VSEROS
                var OlympVseros = context.Olympiads.Where(x => x.PersonId == PersonId && x.OlympType.IsVseross)
                    .Select(x => new { x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                int egeCnt = 1;
                foreach (var ex in OlympVseros)
                {
                    acrFlds.SetField("OlympVserosName" + egeCnt, ex.Name);
                    acrFlds.SetField("OlympVserosYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympVserosDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                //OTHEROLYMPS
                var OlympNoVseros = context.Olympiads.Where(x => x.PersonId == PersonId && !x.OlympType.IsVseross)
                    .Select(x => new { x.OlympName.Name, OlympSubject = x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                egeCnt = 1;
                foreach (var ex in OlympNoVseros)
                {
                    acrFlds.SetField("OlympName" + egeCnt, ex.Name + " (" + ex.OlympSubject + ")");
                    acrFlds.SetField("OlympYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                query = "SELECT WorkPlace, WorkProfession, Stage FROM PersonWork WHERE PersonId=@PersonId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
                var work =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         WorkPlace = rw.Field<string>("WorkPlace"),
                         WorkProfession = rw.Field<string>("WorkProfession"),
                         Stage = rw.Field<string>("Stage")
                     }).FirstOrDefault();
                if (work != null)
                {
                    acrFlds.SetField("HasStag", "1");
                    acrFlds.SetField("WorkPlace", work.WorkPlace + ", " + work.WorkProfession);
                    acrFlds.SetField("Stag", work.Stage);
                }
                else
                    acrFlds.SetField("NoStag", "1");

                var Version = context.ApplicationCommitVersion.Where(x => x.CommitId == appId).Select(x => new { x.VersionDate, x.Id }).ToList().LastOrDefault();
                string sVersion = "";
                if (Version != null)
                    sVersion = "Версия №" + Version.Id + " от " + Version.VersionDate.ToString("dd.MM.yyyy HH:mm");
                string FIO = ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim();

                var Comm = context.ApplicationCommit.Where(x => x.Id == appId).FirstOrDefault();
                if (Comm != null)
                {
                    int multiplyer = 3;
                    string code = ((multiplyer * 100000) + Comm.IntNumber).ToString();

                    List<ShortAppcation> lstApps = abitList
                    .Select(x => new ShortAppcation()
                    {
                        ApplicationId = x.Id,
                        LicenseProgramName = x.ProfessionCode + " " + x.Profession,
                        ObrazProgramName = x.ObrazProgram,
                        ProfileName = x.Specialization,
                        Priority = x.Priority,
                        StudyBasisId = x.StudyBasisId,
                        StudyFormId = x.StudyFormId,
                        HasInnerPriorities = false,
                        IsCrimea = x.IsCrimea,
                        IsForeign = x.IsGosLine
                    }).ToList();

                    List<ShortAppcation> lstAppsFirst = new List<ShortAppcation>();
                    for (int u = 0; u < 3; u++)
                    {
                        if (lstApps.Count > u)
                            lstAppsFirst.Add(lstApps[u]);
                    }

                    //добавляем первый файл
                    lstFiles.Add(GetApplicationPDF_FirstPage(lstAppsFirst, lstApps, dirPath, "ApplicationSPO_page1.pdf", FIO, sVersion, code, false));
                    //acrFlds.SetField("Version", sVersion);

                    //остальные - по 4 на новую страницу
                    int appcount = 3;
                    while (appcount < lstApps.Count)
                    {
                        lstAppsFirst = new List<ShortAppcation>();
                        for (int u = 0; u < 4; u++)
                        {
                            if (lstApps.Count > appcount)
                                lstAppsFirst.Add(lstApps[appcount]);
                            else
                                break;
                            appcount++;
                        }

                        lstFiles.Add(GetApplicationPDF_NextPage(lstAppsFirst, lstApps, dirPath, "ApplicationSPO_page2.pdf", FIO));
                    }
                }

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                lstFiles.Add(ms.ToArray());

                return MergePdfFiles(lstFiles);
            }
        }

        public static byte[] GetApplicationPDF_Aspirant(Guid appId, string dirPath, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    Entry.IsForeign,
                                    Entry.IsCrimea,
                                }).ToList();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  AddInfo = x.PersonAddInfo.AddInfo,
                                  Parents = x.PersonAddInfo.Parents,
                                  HasPrivileges = x.PersonAddInfo.HasPrivileges ?? false,
                                  x.PersonAddInfo.ReturnDocumentTypeId,
                                  SportQualificationName = x.PersonSportQualification.SportQualification.Name,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.HasRussianNationality,
                                  x.PersonAddInfo.HasTRKI,
                                  x.PersonAddInfo.TRKICertificateNumber,
                                  x.PersonAddInfo.StartEnglish,
                                  x.PersonAddInfo.EnglishMark,
                                  Language = x.PersonAddInfo.Language.Name,
                              }).FirstOrDefault();

                var personEducationList =
                   (from x in context.PersonEducationDocument
                    join hx in context.PersonHighEducationInfo on x.Id equals hx.EducationDocumentId into gj
                    from heduc in gj.DefaultIfEmpty()

                    join q in context.Qualification on heduc.QualificationId equals q.Id into ggj
                    from qual in ggj.DefaultIfEmpty()

                    where x.PersonId == PersonId
                    select new
                    {
                        x.Id,
                        x.SchoolExitYear,
                        x.SchoolName,
                        x.SchoolNum,
                        x.IsEqual,
                        x.EqualDocumentNumber,
                        ProgramName = (heduc == null ? "" : heduc.ProgramName),
                        CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                        QualificationId = (heduc == null ? -1 : heduc.QualificationId),
                        Qualification = (qual== null ? "" : qual.Name),
                        x.CountryEducId,
                        x.SchoolTypeId,
                        EducationDocumentSeries = x.Series,
                        EducationDocumentNumber = x.Number,
                    }).ToList();

                var personEducation = personEducationList.OrderByDescending(x => x.QualificationId).First();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationAsp_page3.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }
                List<byte[]> lstFiles = new List<byte[]>();
                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;

                var Version = context.ApplicationCommitVersion.Where(x => x.CommitId == appId).Select(x => new { x.VersionDate, x.Id }).ToList().LastOrDefault();
                string sVersion = "";
                if (Version != null)
                    sVersion = "Версия №" + Version.Id + " от " + Version.VersionDate.ToString("dd.MM.yyyy HH:mm");
                string FIO = ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim();

                List<ShortAppcation> lstApps = abitList
                    .Select(x => new ShortAppcation()
                    {
                        ApplicationId = x.Id,
                        LicenseProgramName = x.ProfessionCode + " " + x.Profession,
                        ObrazProgramName = x.ObrazProgram,
                        ProfileName = x.Specialization,
                        Priority = x.Priority,
                        StudyBasisId = x.StudyBasisId,
                        StudyFormId = x.StudyFormId,
                        HasInnerPriorities = false,
                        IsCrimea = x.IsCrimea,
                        IsForeign = x.IsForeign
                    }).ToList();

                List<ShortAppcation> lstAppsFirst = new List<ShortAppcation>();
                for (int u = 0; u < 4; u++)
                {
                    if (lstApps.Count > u)
                        lstAppsFirst.Add(lstApps[u]);
                }
                int multiplyer = 3;
                string code = ((multiplyer * 100000) + abitList.First().CommitIntNumber).ToString();

                lstFiles.Add(GetApplicationPDF_FirstPage(lstAppsFirst, lstApps, dirPath, "ApplicationAsp_page1.pdf", FIO, sVersion, code, true));
                acrFlds.SetField("Version", sVersion);

                int appcount = 4;
                while (appcount < lstApps.Count)
                {
                    lstAppsFirst = new List<ShortAppcation>();
                    for (int u = 0; u < 4; u++)
                    {
                        if (lstApps.Count > appcount)
                            lstAppsFirst.Add(lstApps[appcount]);
                        else
                            break;
                        appcount++;
                    }
                    lstFiles.Add(GetApplicationPDF_NextPage(lstAppsFirst, lstApps, dirPath, "ApplicationMag_page2.pdf", FIO));
                }

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                if (abitList.Where(x => x.IsForeign).Count() > 0)
                    acrFlds.SetField("IsGosLine", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                if (person.BirthDate.HasValue)
                {
                    acrFlds.SetField("BirthDateYear", person.BirthDate.Value.Year.ToString("D2"));
                    acrFlds.SetField("BirthDateMonth", person.BirthDate.Value.Month.ToString("D2"));
                    acrFlds.SetField("BirthDateDay", person.BirthDate.Value.Day.ToString("D2"));
                }
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);

                //dd.MM.yyyy :12.05.2000
                string[] splitStr = GetSplittedStrings(person.PassportAuthor + " " + person.PassportDate.Value.ToString("dd.MM.yyyy"), 60, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("PassportAuthor" + i, splitStr[i - 1]);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);

                if (person.HasRussianNationality)
                    acrFlds.SetField("HasRussianNationalityYes", "1");
                else
                    acrFlds.SetField("HasRussianNationalityNo", "1");

                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());
                if (person.StartEnglish)
                    acrFlds.SetField("chbEnglishYes", "1");
                else
                    acrFlds.SetField("chbEnglishNo", "1");

                acrFlds.SetField("Phone", person.Phone);
                acrFlds.SetField("Email", email);
                acrFlds.SetField("Mobiles", person.Mobiles);

                acrFlds.SetField("Language", person.Language);

                acrFlds.SetField("ExitYear", personEducation.SchoolExitYear.ToString());
                splitStr = GetSplittedStrings(personEducation.SchoolName ?? "", 50, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("School" + i, splitStr[i - 1]);

                //только у магистров
                
                acrFlds.SetField("HEProfession", personEducation.ProgramName ?? "");
                acrFlds.SetField("Qualification", personEducation.Qualification ?? "");

                acrFlds.SetField("Original", "0");
                acrFlds.SetField("Copy", "0");
                acrFlds.SetField("CountryEduc", personEducation.CountryEduc ?? "");
                acrFlds.SetField("Language", person.Language ?? "");

                string extraPerson = person.Parents ?? "";
                splitStr = GetSplittedStrings(extraPerson, 70, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Parents" + i.ToString(), splitStr[i - 1]);

                string Attestat = personEducation.SchoolTypeId == 1 ? ("аттестат серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? "")) :
                        ("диплом серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? ""));
                acrFlds.SetField("Attestat", Attestat);

                string AddInfo = person.AddInfo ?? "";

                var certificaties = context.PersonLanguageCertificates
                    .Where(x => x.PersonId == PersonId)
                    .Select(x => new 
                    { 
                        CertificateType = x.LanguageCertificatesType.Name,
                        x.LanguageCertificatesType.BoolType,
                        x.LanguageCertificatesType.ValueType,
                        x.Number,
                        x.ResultBool,
                        x.ResultValue
                    }).ToList();

                if (certificaties.Count > 0)
                    AddInfo += "Языковые сертификаты:\n";
                foreach (var langCert in certificaties)
                {
                    string Mrk = "";
                    if (langCert.BoolType)
                        Mrk = (langCert.ResultBool ?? false) ? "зачёт" : "";
                    else
                        Mrk = langCert.ResultValue.HasValue ? langCert.ResultValue.Value.ToString() : "нет результата";

                    AddInfo += langCert.CertificateType + ": #" + langCert.Number + " (" + Mrk + ")\n";
                }

                acrFlds.SetField("Extra", AddInfo);

                if (personEducation.IsEqual && personEducation.CountryEducId != 193)
                {
                    acrFlds.SetField("IsEqual", "1");
                    acrFlds.SetField("EqualSertificateNumber", personEducation.EqualDocumentNumber);
                }
                else
                {
                    acrFlds.SetField("NoEqual", "1");
                }

                if (person.HasPrivileges)
                    acrFlds.SetField("HasPrivileges", "1");

                acrFlds.SetField("ReturnDocumentType" + person.ReturnDocumentTypeId, "1");
               
                if ((personEducation.SchoolTypeId != 4) || (personEducation.SchoolTypeId == 4 && (personEducation.Qualification).ToLower().IndexOf("аспирант") < 0))
                    acrFlds.SetField("NoEduc", "1");
                else
                {
                    acrFlds.SetField("HasEduc", "1");
                    acrFlds.SetField("HighEducation", personEducation.SchoolName);
                }

                //VSEROS
                var OlympVseros = context.Olympiads.Where(x => x.PersonId == PersonId && x.OlympType.IsVseross)
                    .Select(x => new { x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                int egeCnt = 1;
                foreach (var ex in OlympVseros)
                {
                    acrFlds.SetField("OlympVserosName" + egeCnt, ex.Name);
                    acrFlds.SetField("OlympVserosYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympVserosDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                //OTHEROLYMPS
                var OlympNoVseros = context.Olympiads.Where(x => x.PersonId == PersonId && !x.OlympType.IsVseross)
                    .Select(x => new { x.OlympName.Name, OlympSubject = x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                egeCnt = 1;
                foreach (var ex in OlympNoVseros)
                {
                    acrFlds.SetField("OlympName" + egeCnt, ex.Name + " (" + ex.OlympSubject + ")");
                    acrFlds.SetField("OlympYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                query = "SELECT WorkPlace, WorkProfession, Stage FROM PersonWork WHERE PersonId=@PersonId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
                var work =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         WorkPlace = rw.Field<string>("WorkPlace"),
                         WorkProfession = rw.Field<string>("WorkProfession"),
                         Stage = rw.Field<string>("Stage")
                     }).FirstOrDefault();
                if (work != null)
                {
                    acrFlds.SetField("HasStag", "1");
                    acrFlds.SetField("WorkPlace", work.WorkPlace + ", " + work.WorkProfession);
                    acrFlds.SetField("Stag", work.Stage);
                }
                else
                    acrFlds.SetField("NoStag", "1");
                 

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                lstFiles.Add(ms.ToArray());
                return MergePdfFiles(lstFiles.ToList());
            }
        }

        public static byte[] GetApplicationPDF_Ord(Guid appId, string dirPath, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine
                                }).ToList();

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.PersonAddInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.PersonContacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.PersonContacts.City,
                                  Region = x.PersonContacts.Region.Name,
                                  //x.PersonHighEducationInfo.ProgramName,
                                  x.PersonContacts.Code,
                                  x.PersonContacts.Street,
                                  x.PersonContacts.House,
                                  x.PersonContacts.Korpus,
                                  x.PersonContacts.Flat,
                                  x.PersonContacts.Phone,
                                  x.PersonContacts.Mobiles,
                                  AddInfo = x.PersonAddInfo.AddInfo,
                                  Parents = x.PersonAddInfo.Parents,
                                  //Qualification = x.PersonHighEducationInfo.Qualification != null ? x.PersonHighEducationInfo.Qualification.Name : "",
                                  HasPrivileges = x.PersonAddInfo.HasPrivileges ?? false,
                                  x.PersonAddInfo.ReturnDocumentTypeId,
                                  SportQualificationName = x.PersonSportQualification.SportQualification.Name,
                                  x.PersonAddInfo.HostelEduc,
                                  x.PersonContacts.Country.IsRussia,
                                  x.HasRussianNationality,
                                  x.PersonAddInfo.HasTRKI,
                                  x.PersonAddInfo.TRKICertificateNumber,
                                  x.PersonAddInfo.StartEnglish,
                                  x.PersonAddInfo.EnglishMark,
                                  Language = x.PersonAddInfo.Language.Name,
                              }).FirstOrDefault();

                var personEducationList =
                   (from x in context.PersonEducationDocument
                    join hx in context.PersonHighEducationInfo on x.Id equals hx.EducationDocumentId into gj
                    from heduc in gj.DefaultIfEmpty()

                    join q in context.Qualification on heduc.QualificationId equals q.Id into ggj
                    from qual in ggj.DefaultIfEmpty()

                    where x.PersonId == PersonId
                    select new
                    {
                        x.Id,
                        x.SchoolExitYear,
                        x.SchoolName,
                        x.SchoolNum,
                        x.IsEqual,
                        x.EqualDocumentNumber,
                        ProgramName = (heduc == null ? "" : heduc.ProgramName),
                        CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                        QualificationId = (heduc == null ? -1 : heduc.QualificationId),
                        Qualification = (qual == null ? "" : qual.Name),
                        x.CountryEducId,
                        x.SchoolTypeId,
                        EducationDocumentSeries = x.Series,
                        EducationDocumentNumber = x.Number,
                    }).ToList();
                var personEducation = personEducationList.OrderByDescending(x => x.QualificationId).First();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationOrd_2015.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                if (abitList.Where(x => x.IsGosLine).Count() > 0)
                    acrFlds.SetField("IsGosLine", "1");

                acrFlds.SetField("HostelAbitYes", (person.HostelAbit ?? false) ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", (person.HostelAbit ?? false) ? "0" : "1");

                if (person.BirthDate.HasValue)
                {
                    acrFlds.SetField("BirthDateYear", person.BirthDate.Value.Year.ToString("D2"));
                    acrFlds.SetField("BirthDateMonth", person.BirthDate.Value.Month.ToString("D2"));
                    acrFlds.SetField("BirthDateDay", person.BirthDate.Value.Day.ToString());
                }
                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);

                //dd.MM.yyyy :12.05.2000
                string[] splitStr = GetSplittedStrings(person.PassportAuthor + " " + person.PassportDate.Value.ToString("dd.MM.yyyy"), 60, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("PassportAuthor" + i, splitStr[i - 1]);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);

                if (person.HasRussianNationality)
                    acrFlds.SetField("HasRussianNationalityYes", "1");
                else
                    acrFlds.SetField("HasRussianNationalityNo", "1");

                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());
                if (person.StartEnglish)
                    acrFlds.SetField("chbEnglishYes", "1");
                else
                    acrFlds.SetField("chbEnglishNo", "1");

                acrFlds.SetField("Phone", person.Phone);
                acrFlds.SetField("Email", email);
                acrFlds.SetField("Mobiles", person.Mobiles);

                acrFlds.SetField("Language", person.Language);

                acrFlds.SetField("ExitYear", personEducation.SchoolExitYear.ToString());
                splitStr = GetSplittedStrings(personEducation.SchoolName ?? "", 50, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("School" + i, splitStr[i - 1]);

                //только у магистров
                acrFlds.SetField("HEProfession", personEducation.ProgramName ?? "");
                acrFlds.SetField("Qualification", personEducation.Qualification ?? "");

                acrFlds.SetField("Original", "0");
                acrFlds.SetField("Copy", "0");
                acrFlds.SetField("CountryEduc", personEducation.CountryEduc ?? "");
                acrFlds.SetField("Language", person.Language ?? "");

                string extraPerson = person.Parents ?? "";
                splitStr = GetSplittedStrings(extraPerson, 70, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Parents" + i.ToString(), splitStr[i - 1]);

                string Attestat = personEducation.SchoolTypeId == 1 ? ("аттестат серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? "")) :
                        ("диплом серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? ""));
                acrFlds.SetField("Attestat", Attestat);
                acrFlds.SetField("Extra", person.AddInfo ?? "");

                if (personEducation.IsEqual && personEducation.CountryEducId != 193)
                {
                    acrFlds.SetField("IsEqual", "1");
                    acrFlds.SetField("EqualSertificateNumber", personEducation.EqualDocumentNumber);
                }
                else
                {
                    acrFlds.SetField("NoEqual", "1");
                }

                if (person.HasPrivileges)
                    acrFlds.SetField("HasPrivileges", "1");

                acrFlds.SetField("ReturnDocumentType" + person.ReturnDocumentTypeId, "1");
                if ((personEducation.SchoolTypeId != 2) && (personEducation.SchoolTypeId != 5))//SSUZ & SPO
                    acrFlds.SetField("NoEduc", "1");
                else
                {
                    acrFlds.SetField("HasEduc", "1");
                    acrFlds.SetField("HighEducation", personEducation.SchoolName);
                }

                //VSEROS
                var OlympVseros = context.Olympiads.Where(x => x.PersonId == PersonId && x.OlympType.IsVseross)
                    .Select(x => new { x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                int egeCnt = 1;
                foreach (var ex in OlympVseros)
                {
                    acrFlds.SetField("OlympVserosName" + egeCnt, ex.Name);
                    acrFlds.SetField("OlympVserosYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympVserosDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                //OTHEROLYMPS
                var OlympNoVseros = context.Olympiads.Where(x => x.PersonId == PersonId && !x.OlympType.IsVseross)
                    .Select(x => new { x.OlympName.Name, OlympSubject = x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                egeCnt = 1;
                foreach (var ex in OlympNoVseros)
                {
                    acrFlds.SetField("OlympName" + egeCnt, ex.Name + " (" + ex.OlympSubject + ")");
                    acrFlds.SetField("OlympYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                query = "SELECT WorkPlace, WorkProfession, Stage FROM PersonWork WHERE PersonId=@PersonId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
                var work =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         WorkPlace = rw.Field<string>("WorkPlace"),
                         WorkProfession = rw.Field<string>("WorkProfession"),
                         Stage = rw.Field<string>("Stage")
                     }).FirstOrDefault();
                if (work != null)
                {
                    acrFlds.SetField("HasStag", "1");
                    acrFlds.SetField("WorkPlace", work.WorkPlace + ", " + work.WorkProfession);
                    acrFlds.SetField("Stag", work.Stage);
                }
                else
                    acrFlds.SetField("NoStag", "1");

                var Comm = context.ApplicationCommit.Where(x => x.Id == appId).FirstOrDefault();
                if (Comm != null)
                {
                    int multiplyer = 3;
                    string code = ((multiplyer * 100000) + Comm.IntNumber).ToString();

                    //добавляем штрихкод
                    Barcode128 barcode = new Barcode128();
                    barcode.Code = code;
                    PdfContentByte cb = pdfStm.GetOverContent(1);
                    iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                    img.SetAbsolutePosition(440, 740);
                    cb.AddImage(img);
                }

                int rwInd = 1;
                foreach (var abit in abitList.OrderBy(x => x.Priority))
                {
                    acrFlds.SetField("Profession" + rwInd, abit.ProfessionCode + " " + abit.Profession);
                    acrFlds.SetField("Specialization" + rwInd, abit.Specialization);
                    acrFlds.SetField("ObrazProgram" + rwInd, abit.ObrazProgram);
                    acrFlds.SetField("Priority" + rwInd, abit.Priority.ToString());

                    acrFlds.SetField("StudyForm" + abit.StudyFormId + rwInd, "1");
                    acrFlds.SetField("StudyBasis" + abit.StudyBasisId + rwInd, "1");

                    if (abitList.Where(x => x.Profession == abit.Profession && x.ObrazProgram == abit.ObrazProgram && x.Specialization == abit.Specialization && x.StudyFormId == abit.StudyFormId).Count() > 1)
                        acrFlds.SetField("IsPriority" + rwInd, "1");
                    rwInd++;
                }

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }

        public static byte[] GetApplicationPDF_AG(Guid commitId, string dirPath)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList =
                    (from App in context.Application
                     join Commit in context.ApplicationCommit on App.CommitId equals Commit.Id
                     join Entry in context.Entry on App.EntryId equals Entry.Id
                     join sLevel in context.SP_StudyLevel on Entry.StudyLevelId equals sLevel.Id
                     orderby App.Priority
                     where App.CommitId == commitId && App.IsCommited == true && App.Enabled == true
                     select new
                     {
                         App.Id,
                         App.PersonId,
                         App.Barcode,
                         Profession = Entry.LicenseProgramName,
                         ObrazProgram = Entry.ObrazProgramName,
                         Specialization = Entry.ProfileName,
                         Entry.StudyLevelGroupId,
                         App.Priority,
                         Entry.ComissionId,
                         ComissionAddress = Entry.Address,
                         ClassNum = sLevel.ClassName,
                         CommitIntNumber = Commit.IntNumber,
                         sLevel.Duration,
                     }).OrderBy(x => x.Priority).ToList();

                Guid PersonId = abitList.FirstOrDefault().PersonId;
                var PersonAddInfo = (from p in context.PersonAddInfo
                                     where p.PersonId == PersonId
                                     select p).First();
                var person =
                    context.Person.Where(x => x.Id == PersonId).Select(x => new
                    {
                        x.Surname,
                        x.Name,
                        x.SecondName,
                        x.Sex,
                        x.BirthDate,
                        Language = x.PersonAddInfo.Language.Name,
                        x.PersonAddInfo.HasPrivileges,
                        x.PersonAddInfo.HostelEduc,
                        x.PersonAddInfo.NeedSpecialConditions,
                        x.PersonContacts.City,
                        Region = x.PersonContacts.Region.Name,
                        x.PersonContacts.Code,
                        x.PersonContacts.Street,
                        x.PersonContacts.House,
                        x.PersonContacts.Korpus,
                        x.PersonContacts.Flat,
                        x.PersonContacts.Phone,
                        x.PersonContacts.Mobiles,
                        x.PersonContacts.Country.IsRussia,
                        Country = x.PersonContacts.Country.Name,
                        HasSportQualification = x.PersonSportQualification.SportQualificationId  != 0,
                        SportTypeName = x.PersonSportQualification.SportQualification.Name,
                        SportTypeOtherName = x.PersonSportQualification.OtherSportQualification,
                        SportInfo = x.PersonSportQualification.SportQualificationLevel,
                        x.User.Email,
                    }).FirstOrDefault();

                var Olympiads =
                    (from Ol in context.Olympiads
                     
                     join olb in context.OlympBook on new { Ol.OlympYear, Ol.OlympTypeId, Ol.OlympSubjectId, Ol.OlympNameId } equals new { olb.OlympYear, olb.OlympTypeId, olb.OlympSubjectId, olb.OlympNameId} into _olb
                     from olbook in _olb.DefaultIfEmpty()

                     where Ol.PersonId == PersonId
                     select new
                     {
                        Ol.DocumentSeries,
                        Ol.DocumentNumber,
                        Ol.DocumentDate,
                        Ol.OlympYear,
                        OlympName = Ol.OlympName.Name,
                        OlympSubject = Ol.OlympSubject.Name,
                        OlympValue = Ol.OlympValue.Name,
                        OlympLevel = (olbook == null) ? "" : (olbook.OlympLevelId == 4 ? "" : olbook.OlympLevel.Name),
                     }).ToList();
                var personEduc = (from p in context.PersonEducationDocument
                                  join excl in context.SchoolExitClass on p.SchoolExitClassId equals excl.Id
                                  where p.PersonId == PersonId
                                  select new
                                  {
                                      ExitClass = excl.OrderNumber,
                                      p.SchoolNum,
                                      p.SchoolName,
                                      RegionName = p.Region.Name,
                                      p.SchoolCity, 
                                  }).OrderByDescending(x=>x.ExitClass).First();

                var ScienceInfo = (from p in context.PersonScienceWork
                                   where p.PersonId == PersonId && (p.WorkTypeId == 2 || p.WorkTypeId == 7)
                                   select new
                                   {
                                       TypeName = p.ScienceWorkType.Name,
                                       WorkYear = p.WorkYear,
                                       WorkInfo = p.WorkInfo,
                                   }).ToList();
   
                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationAG_2017.pdf";
                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                var Version = context.ApplicationCommitVersion.Where(x => x.CommitId == commitId).Select(x => new { x.VersionDate, x.Id }).ToList().LastOrDefault();
                string sVersion = "";
                if (Version != null)
                    sVersion = "Версия №" + Version.Id + " от " + Version.VersionDate.ToString("dd.MM.yyyy HH:mm");

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                AcroFields acrFlds = pdfStm.AcroFields;

                acrFlds.SetField("Version", sVersion);

                int multiplyer = abitList.FirstOrDefault().StudyLevelGroupId;
                string code = ((multiplyer * 100000) + abitList.First().CommitIntNumber).ToString();
                Barcode128 barcode = new Barcode128();
                barcode.Code = code;
                PdfContentByte cb = pdfStm.GetOverContent(1);
                iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                // img.SetAbsolutePosition(420, 720);
                img.SetAbsolutePosition(120, 775);
                cb.AddImage(img);

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("ObrazProgramYear", abitList.FirstOrDefault().Duration.ToString());
                acrFlds.SetField("EntryClass", abitList.FirstOrDefault().ClassNum);
                if (person.HostelEduc)
                    acrFlds.SetField("HostelAbitYes", "1");
                else
                    acrFlds.SetField("HostelAbitNo", "1");

                if (person.NeedSpecialConditions)
                    acrFlds.SetField("SpecialConditionsYes", "1");
                else
                    acrFlds.SetField("SpecialConditionsNo", "1");

                int inc = 0;
                bool hasSecondApp = abitList.Count() > 1;
                foreach (var abit in abitList)
                {
                    inc++;
                    string i = inc.ToString();
                    if (hasSecondApp)
                    {
                        acrFlds.SetField("chbIsPriority" + i, "1");
                        hasSecondApp = false;
                    }

                    if (abit.ClassNum.IndexOf("10") < 0)
                        acrFlds.SetField("chbSchoolLevel1" + i, "1");
                    else
                        acrFlds.SetField("chbSchoolLevel2" + i, "1");

                    acrFlds.SetField("RegNum" + i, (800000 + abit.Barcode).ToString());
                    acrFlds.SetField("Profession" + i, abit.Profession);
                    acrFlds.SetField("ObrazProgram" + i, abit.ObrazProgram);

                    var Exams = Util.GetExamList(abit.Id);
                    string ExamNames = "";
                    foreach (var x in Exams)
                    {
                        if (x.ExamInBlockList.Count > 1)
                            ExamNames += (x.ExamInBlockList.Where(ex => ex.Value.ToString() == x.SelectedExamInBlockId.ToString()).Select(ex => ex.Text).FirstOrDefault()) + ", ";
                    }
                    if (ExamNames.Length > 2)
                        ExamNames = ExamNames.Substring(0, ExamNames.Length - 2);
                    if (!string.IsNullOrEmpty(ExamNames))
                        acrFlds.SetField("ManualExam" + i, ExamNames);
                    else
                        acrFlds.SetField("ManualExam" + i, "нет");
                }

                acrFlds.SetField("ClassNum", abitList.First().ClassNum.ToString());

                acrFlds.SetField("Surname", person.Surname);
                acrFlds.SetField("Name", person.Name);
                acrFlds.SetField("SecondName", person.SecondName);
                inc = 0;
                foreach (var abit in abitList)
                {
                    inc++;
                    acrFlds.SetField("Specialization" + inc.ToString(), abit.ObrazProgram);
                    acrFlds.SetField("Profile" + inc.ToString(), abit.Profession);
                    if (inc == 2)
                        break;
                }
                string Year = person.BirthDate.Value.Year.ToString();
                string Month = person.BirthDate.Value.Month.ToString();
                Month = (Month.Length == 1) ? "0" + Month : Month;
                string Day = person.BirthDate.Value.Day.ToString();
                Day = (Day.Length == 1) ? "0" + Day : Day;
                for (int i = 0; i < 4; i++)
                    acrFlds.SetField("Year" + (i + 1).ToString(), Year[i].ToString());
                for (int i = 0; i < 2; i++)
                    acrFlds.SetField("Month" + (i + 1).ToString(), Month[i].ToString());
                for (int i = 0; i < 2; i++)
                    acrFlds.SetField("Day" + (i + 1).ToString(), Day[i].ToString());

                string sRegionName = (person.IsRussia ? (string.IsNullOrEmpty(person.Region) ? "" : person.Region + ",") : person.Country + ", ");
                string sCity = (string.IsNullOrEmpty(person.City) ? "" : person.City.Trim());
                string sAddress = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(person.Street) ? "" : person.Street + ", ",
                    string.IsNullOrEmpty(person.House) ? "" : "д." + person.House + ", ",
                    string.IsNullOrEmpty(person.Korpus) ? "" : "корп." + person.Korpus + ", ",
                    string.IsNullOrEmpty(person.Flat) ? "" : "кв." + person.Flat);

                string[] Address = GetSplittedStringByCell(sAddress);
                string[] City = GetSplittedStringByCell(sCity);

                for (int i = 0; i < City.Length; i++)
                    acrFlds.SetField("City" + (i + 1), City[i].ToString());

                string PostIndex = (person.Code) ?? "";
                for (int i = 0; i < PostIndex.Length && i < 6; i++)
                    acrFlds.SetField("Code" + (i + 1), PostIndex[i].ToString());

                for (int i = 0; i < Address.Length && i < 40; i++)
                    acrFlds.SetField("Address" + (i + 1), Address[i].ToString());

                //string[] splitStr = GetSplittedStrings(Address, 50, 50, 2);
                //for (int i = 1; i <= 2; i++)
                //    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                string[] Email = GetSplittedStringByCell(person.Email);
                for (int i = 0; i < Email.Length; i++)
                    acrFlds.SetField("Email" + (i + 1), Email[i].ToString());
                string[] Phone = GetSplittedStringByCell(person.Phone ?? "");
                for (int i = 0; i < Phone.Length; i++ )
                    acrFlds.SetField("Phone" + (i + 1).ToString(), Phone[i].ToString());
                string Mobile = person.Mobiles ?? "";
                for (int i = 0; i < Mobile.Length; i++)
                    acrFlds.SetField("Mobile" + (i + 1).ToString(), Mobile[i].ToString());

                if (!string.IsNullOrEmpty(PersonAddInfo.Parent_Surname))
                {
                    string[] Parent_Surname = GetSplittedStringByCell(PersonAddInfo.Parent_Surname);
                    for (int i = 0; i < Parent_Surname.Length; i++)
                        acrFlds.SetField("ParentSurname" + (i + 1), Parent_Surname[i].ToString());
                }
                if (!string.IsNullOrEmpty(PersonAddInfo.Parent_Name))
                {
                    string[] Parent_Name = GetSplittedStringByCell(PersonAddInfo.Parent_Name);
                    for (int i = 0; i < Parent_Name.Length; i++)
                        acrFlds.SetField("ParentName" + (i + 1), Parent_Name[i].ToString());
                }
                if (!string.IsNullOrEmpty(PersonAddInfo.Parent_SecondName))
                {
                    string[] Parent_SecondName = GetSplittedStringByCell(PersonAddInfo.Parent_SecondName);
                    for (int i = 0; i < Parent_SecondName.Length; i++)
                        acrFlds.SetField("ParentSecondName" + (i + 1), Parent_SecondName[i].ToString());
                }
                if (!string.IsNullOrEmpty(PersonAddInfo.Parent_Phone))
                {
                    string[] Parent_Phone = GetSplittedStringByCell(PersonAddInfo.Parent_Phone);
                    for (int i = 0; i < Parent_Phone.Length; i++)
                        acrFlds.SetField("ParentPhone" + (i + 1), Parent_Phone[i].ToString());
                }
                if (!string.IsNullOrEmpty(PersonAddInfo.Parent_Email))
                {
                    string[] Parent_Email = GetSplittedStringByCell(PersonAddInfo.Parent_Email);
                    for (int i = 0; i < Parent_Email.Length; i++)
                        acrFlds.SetField("ParentEmail" + (i + 1), Parent_Email[i].ToString());
                }
                if (!string.IsNullOrEmpty(PersonAddInfo.Parent_Work))
                {
                    string[] Parent_Work = GetSplittedStringByCell(PersonAddInfo.Parent_Work);
                    for (int i = 0; i < Parent_Work.Length; i++)
                        acrFlds.SetField("ParentWork" + (i + 1), Parent_Work[i].ToString());
                }

                string SchName = string.Format(personEduc.SchoolName + " " + personEduc.SchoolNum);
                string[] splitStr = GetSplittedStrings(SchName, 50, 50, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("SchoolName" + i, splitStr[i - 1]);

                string SchAddress = string.Format(personEduc.RegionName + " " + personEduc.SchoolCity);
                splitStr = GetSplittedStrings(SchAddress, 50, 50, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("SchoolAddress" + i, splitStr[i - 1]);

                acrFlds.SetField("English1", person.Language);
                acrFlds.SetField("English2", "");

                for (int i = 0; (i < 4) && (i < Olympiads.Count()); i++)
                {
                    acrFlds.SetField("OlympSubject" + (i + 1).ToString(), Olympiads[i].OlympSubject.ToString());
                    acrFlds.SetField("OlympYear" + (i + 1).ToString(), Olympiads[i].OlympYear > 0 ? Olympiads[i].OlympYear.ToString() : "");
                    acrFlds.SetField("OlympName" + (i + 1).ToString(), Olympiads[i].OlympName);
                    acrFlds.SetField("OlympStep" + (i + 1).ToString(), "");
                    acrFlds.SetField("OlympLevel" + (i + 1).ToString(), Olympiads[i].OlympLevel);
                    string DiplomNumber = Olympiads[i].DocumentSeries + " " + Olympiads[i].DocumentNumber + (Olympiads[i].DocumentDate.HasValue ? (" от " + Olympiads[i].DocumentDate.Value.ToShortDateString()) : "");
                    acrFlds.SetField("OlympDiploma" + (i + 1).ToString(), Olympiads[i].OlympValue);
                }

                for (int i = 0; (i < 4) && (i < ScienceInfo.Count); i++)
                {
                    acrFlds.SetField("ConferenceName" + (i + 1).ToString(), ScienceInfo[i].TypeName);
                    acrFlds.SetField("ConferenceYear" + (i + 1).ToString(), ScienceInfo[i].WorkYear);
                    acrFlds.SetField("ConferenceName2" + (i + 1).ToString(), ScienceInfo[i].WorkInfo);
                }
                for (int i = 0; i < 3; i++)
                {
                    acrFlds.SetField("Add" + (i + 1).ToString(), "");
                }
                if (person.HasSportQualification)
                    for (int i = 0; i < 1; i++)
                    {
                        acrFlds.SetField("SportType" + (i + 1).ToString(), !String.IsNullOrEmpty(person.SportTypeName) ? person.SportTypeName : person.SportTypeOtherName);
                        acrFlds.SetField("SportInfo" + (i + 1).ToString(), person.SportInfo);
                    }

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                return ms.ToArray();
            }
        }

        public static string[] GetSplittedStrings(string sourceStr, int firstStrLen, int strLen, int numOfStrings)
        {
            sourceStr = sourceStr ?? "";
            string[] retStr = new string[numOfStrings];
            int index = 0, startindex = 0;
            for (int i = 0; i < numOfStrings; i++)
            {
                if (sourceStr.Length > startindex && startindex >= 0)
                {
                    int rowLength = firstStrLen;//длина первой строки
                    if (i > 1) //длина остальных строк одинакова
                        rowLength = strLen;
                    index = startindex + rowLength;
                    if (index < sourceStr.Length)
                    {
                        index = sourceStr.IndexOf(" ", index);
                        string val = index > 0 ? sourceStr.Substring(startindex, index - startindex) : sourceStr.Substring(startindex);
                        retStr[i] = val;
                    }
                    else
                        retStr[i] = sourceStr.Substring(startindex);
                }
                startindex = index;
            }

            return retStr;
        }
        public static byte[] MergePdfFiles(List<byte[]> lstFilesBinary)
        {
            MemoryStream ms = new MemoryStream();
            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            foreach (byte[] doc in lstFilesBinary)
            {
                if (doc == null)
                    continue;

                using (MemoryStream mstr = new MemoryStream())
                {
                    PdfReader reader = new PdfReader(doc);
                    int n = reader.NumberOfPages;
                    //for (int i = 0; i < n; i++)
                    //{
                    //    PdfDictionary ppd = reader.GetPageN(i + 1);
                    //    PdfNumber rotate = ppd.GetAsNumber(PdfName.ROTATE);
                    //    if (rotate == null)
                    //        ppd.Put(PdfName.ROTATE, new PdfNumber(90));
                    //    else
                    //        ppd.Put(PdfName.ROTATE, new PdfNumber((rotate.IntValue + 90) % 360));
                    //}

                    //PdfStamper stamper = new PdfStamper(reader, mstr);
                    //stamper.Close();

                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage page;

                    for (int i = 0; i < n; i++)
                    {
                        document.NewPage();
                        try
                        {
                            page = writer.GetImportedPage(reader, i + 1);
                            writer.SetPageSize(reader.GetPageSize(i + 1));
                            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                        }
                        catch (Exception ex)
                        {
                            string sFileInfo = "";
                            try
                            { sFileInfo = reader.Info.Select(x => x.Key + ":" + x.Value + "\n").Aggregate((x, tail) => x + tail); }
                            catch { }

                            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, "utf-8", false);
                            iTextSharp.text.Font font = new iTextSharp.text.Font(bfTimes, 10);

                            //Paragraph p = new Paragraph("bad file:" + sFileInfo, font);
                            ColumnText ct = new ColumnText(writer.DirectContent);
                            Phrase myText = new Phrase("bad file:" + sFileInfo, new iTextSharp.text.Font(bfTimes, 9));
                            /*the phrase
                            lower-left-x
                            lower-left-y
                            upper-right-x (llx + width)
                            upper-right-y (lly + height)
                            leading (The amount of blank space between lines of print)
                            alignment.*/
                            float llx = 210f;
                            float lly = 393f;
                            float height = 300;
                            ct.SetSimpleColumn(myText, llx, lly, llx + 200, lly + height, 10, Element.ALIGN_LEFT);
                            ct.Go();

                            //cb.Stroke();
                        }
                    }
                }
            }

            document.Close();
            return ms.ToArray();
        }

        public static bool GetDisableApplicationPDF(Guid appId, string dirPath, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                join Commit in context.ApplicationCommit on x.CommitId equals Commit.Id
                                join Entry in context.Entry on x.EntryId equals Entry.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = Entry.StudyLevelGroupId,
                                    StudyLevelGroupName = Entry.StudyLevelGroupNameRus,
                                    Entry.StudyLevelId,
                                    CommitIntNumber = Commit.IntNumber,
                                    x.Priority,
                                    x.IsGosLine,
                                    Entry.ComissionId,
                                    ComissionAddress = Entry.Address, 
                                    x.SecondTypeId
                                }).OrderBy(x => x.Priority).ToList();
                if (abitList.Count == 0)
                    return true;

                string query = "SELECT Email, IsForeign FROM [User] WHERE Id=@Id";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", PersonId } });
                string email = tbl.Rows[0].Field<string>("Email");
                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName
                              }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "DisableApplication2014_page3.pdf";

                byte[] templateBytes;

                List<byte[]> lstFiles = new List<byte[]>();
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;

                var Version = context.ApplicationCommitVersion.Where(x => x.CommitId == appId).Select(x => new { x.VersionDate, x.Id }).ToList().LastOrDefault();
                string sVersion = "";
                if (Version != null)
                    sVersion = "Отказ от заявления №" + Version.Id + " от " + Version.VersionDate.ToString("dd.MM.yyyy HH:mm");
                string FIO = ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim();

                List<ShortAppcation> lstApps = abitList
                    .Select(x => new ShortAppcation()
                    {
                        Priority = x.Priority,
                        ApplicationId = x.Id,
                        LicenseProgramName = x.ProfessionCode + " " + x.Profession,
                        ObrazProgramName = x.ObrazProgram,
                        ProfileName = x.Specialization,
                        StudyBasisId = x.StudyBasisId,
                        StudyFormId = x.StudyFormId,
                    }).ToList();

                List<ShortAppcation> lstAppsFirst = new List<ShortAppcation>();
                for (int u = 0; u < 4; u++)
                {
                    if (lstApps.Count > u)
                        lstAppsFirst.Add(lstApps[u]);
                }

                int multiplyer = abitList.First().EntryType;
                string code = ((multiplyer * 100000) + abitList.First().CommitIntNumber).ToString();

                //добавляем первый файл
                lstFiles.Add(GetDisableApplicationPDF_FirstPage(lstAppsFirst, lstApps, dirPath, "DisableApplication2014_page1.pdf", code, FIO, sVersion ));
                acrFlds.SetField("Version", sVersion);

                //остальные - по 5 на новую страницу
                int appcount = 4;
                while (appcount < lstApps.Count)
                {
                    lstAppsFirst = new List<ShortAppcation>();
                    for (int u = 0; u < 5; u++)
                    {
                        if (lstApps.Count > appcount)
                            lstAppsFirst.Add(lstApps[appcount]);
                        else
                            break;
                        appcount++;
                    }
                    if (appcount >= lstApps.Count)
                        lstFiles.Add(GetDisableApplicationPDF_NextPage(lstAppsFirst, lstApps, dirPath, "DisableApplication2014_page3.pdf", FIO, sVersion));
                    else
                        lstFiles.Add(GetDisableApplicationPDF_NextPage(lstAppsFirst, lstApps, dirPath, "DisableApplication2014_page2.pdf", "", ""));
                }

                context.SaveChanges();

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                if (lstApps.Count <= 4)
                    lstFiles.Add(ms.ToArray());

                byte[] pdfData = MergePdfFiles(lstFiles.ToList());
                DateTime dateTime = DateTime.Now;

                //add File data in storage
                Guid FileId = Guid.NewGuid();
                query = "INSERT INTO FileStorage (Id, FileData) VALUES (@Id, @FileData)";
                SortedList<string, object> prms = new SortedList<string, object>();
                prms.Add("@Id", FileId);
                prms.Add("@FileData", pdfData);
                Util.AbitDB.ExecuteQuery(query, prms);

                //add info in table
                query = @"INSERT INTO PersonFile (  [Id],[PersonId],
                                                    [FileName],[FileExtention],[FileSize],
                                                    [Comment],[LoadDate],[MimeType],
                                                    [IsReadOnly],[PersonFileTypeId])  
                                            VALUES (@Id, @PersonId,
                                                    @FileName, @FileExtention, @FileSize,
                                                    @Comment, @LoadDate, @MimeType,
                                                    @IsReadOnly, 17)";
                
                int ? SecTypeId = abitList.FirstOrDefault().SecondTypeId;
                string SecondType = (SecTypeId.HasValue ?
                                        ((SecTypeId == 3) ? (" (восстановление)") : 
                                        ((SecTypeId == 2) ? (" (перевод)") :  
                                        ((SecTypeId == 5) ? (" (смена основы обучения)") :
                                        ((SecTypeId == 6) ? (" (смена образовательной программы)") : 
                                        "")))) : "");
                prms.Clear();
                prms.Add("@Id", FileId);
                prms.Add("@PersonId", PersonId);
                prms.Add("@FileName", person.Surname + " " + person.Name.FirstOrDefault() + ". (Отказ от заявления штрих-код " + code + ").pdf");
                prms.Add("@FileExtention", ".pdf");
                prms.Add("@FileSize", pdfData.Length);
                prms.Add("@IsReadOnly", true);
                prms.Add("@LoadDate", dateTime);
                prms.Add("@Comment", "Заявление об отказе от участия в конкурсе (" + (String.IsNullOrEmpty(sVersion)?"":sVersion+", ")+ "штрих-код " + code + ") " + abitList.FirstOrDefault().StudyLevelGroupName + SecondType);
                prms.Add("@MimeType", "[Application]/pdf");
                Util.AbitDB.ExecuteQuery(query, prms);
                bool result = true;
                return result;
            }
        }
        public static byte[] GetDisableApplicationPDF_FirstPage(List<ShortAppcation> lst, List<ShortAppcation> lstFullSource, string dirPath, string dotName, string code, string FIO, string Version)
        {
            MemoryStream ms = new MemoryStream();

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);

            Barcode128 barcode = new Barcode128();
            barcode.Code = code;
            PdfContentByte cb = pdfStm.GetOverContent(1);
            iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
            img.SetAbsolutePosition(440, 740);
            cb.AddImage(img);

            AcroFields acrFlds = pdfStm.AcroFields;
            acrFlds.SetField("Version", Version);
            acrFlds.SetField("FIO", FIO);

            int rwind = 1;
            foreach (var p in lst.OrderBy(x => x.Priority))
            {
                acrFlds.SetField("Profession" + rwind, p.LicenseProgramName);
                acrFlds.SetField("ObrazProgram" + rwind, p.ObrazProgramName);
                acrFlds.SetField("Specialization" + rwind, p.ProfileName);
                acrFlds.SetField("StudyForm" + p.StudyFormId.ToString() + rwind.ToString(), "1");
                acrFlds.SetField("StudyBasis" + p.StudyBasisId.ToString() + rwind.ToString(), "1");
                rwind++;
            }

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }
        public static byte[] GetDisableApplicationPDF_NextPage(List<ShortAppcation> lst, List<ShortAppcation> lstFullSource, string dirPath, string dotName, string FIO, string version)
        {
            MemoryStream ms = new MemoryStream();

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            AcroFields acrFlds = pdfStm.AcroFields;
            int rwind = 1;
            foreach (var p in lst.OrderBy(x => x.Priority))
            {
                acrFlds.SetField("Profession" + rwind, p.LicenseProgramName);
                acrFlds.SetField("ObrazProgram" + rwind, p.ObrazProgramName);
                acrFlds.SetField("Specialization" + rwind, p.ProfileName);
                acrFlds.SetField("StudyForm" + p.StudyFormId.ToString() + rwind.ToString(), "1");
                acrFlds.SetField("StudyBasis" + p.StudyBasisId.ToString() + rwind.ToString(), "1");
                rwind++;
            }
            if (!String.IsNullOrEmpty(version)) acrFlds.SetField("Version", version);
            if (!String.IsNullOrEmpty(FIO)) acrFlds.SetField("FIO", FIO);

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }

        public static byte[] GetCommunicationAppCard(string dirPath, Guid PersonId)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var abitList = (from x in context.Application
                                where x.PersonId == PersonId
                                select new
                                {
                                    HasFee = (x.C_Entry.StudyBasisId == 1),
                                    HasNoFee = (x.C_Entry.StudyBasisId == 2),
                                }).ToList();

                var person = (from x in context.Person
                              join us in context.User on x.Id equals us.Id
                              join pCont in context.PersonContacts on x.Id equals pCont.PersonId

                              join port in context.PortfolioFilesMark on x.Id equals port.PersonId into _prt
                              from prt in _prt.DefaultIfEmpty()

                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.SecondNameEng,
                                  x.NameEng,
                                  x.SurnameEng,
                                  Number = x.Barcode,
                                  us.Email,

                                  Status = (prt != null) ? prt.PortfolioStatus.ShortName : "X",
                                  RuPort = (prt != null) ? (prt.RuPortfolioPts ??0) : 0,
                                  DePort = (prt != null) ? (prt.DePortfolioPts ?? 0) : 0,
                                  RuInt = (prt != null) ? (prt.RuInterviewPts ?? 0) : 0,
                                  DeInt = (prt != null) ? (prt.DeInterviewPts ?? 0) : 0,
                                  Interview = (prt != null) ? (prt.Interview ?? false): false,

                                  pCont.Code,
                                  pCont.Country.IsRussia,
                                  Country = pCont.Country.NameEng,
                                  Region = pCont.Region.Name,
                                  pCont.City,
                                  pCont.Street,
                                  pCont.House,
                                  pCont.Korpus,
                                  pCont.Flat,

                                  x.Sex,
                                  Nationality = x.Nationality.NameEng,
                                  x.BirthPlace,
                                  x.BirthDate,

                                  VisaCountry = x.PersonVisaInfo.Country.NameEng,
                                  VisaPostAddress = x.PersonVisaInfo.PostAddress,
                                  VisaTown = x.PersonVisaInfo.Town,

                                  x.PassportValid, 
                              }).FirstOrDefault();
                var Certs = (from x in context.PersonLanguageCertificates
                             where x.PersonId == PersonId
                             select new
                             {
                                 x.LanguageCertificatesType.NameEng,
                                 x.LanguageCertificatesType.BoolType,
                                 x.Number,
                                 x.ResultBool,
                                 x.ResultValue
                             }).ToList();
                bool HasFee = abitList.Where(x => x.HasFee).Count() > 0;
                bool HasNoFee = abitList.Where(x => x.HasNoFee).Count() > 0; 


                MemoryStream ms = new MemoryStream();
                string dotName = "CommunicationAppCard.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                AcroFields acrFlds = pdfStm.AcroFields;

                //добавляем штрихкод
                PdfContentByte cb = pdfStm.GetOverContent(1);

                byte[] imgBytes = (byte[])Util.AbitDB.GetValue("SELECT TOP 1 FileData FROM PersonFile WHERE PersonId=@PersonId and PersonFileTypeId=14",
                new SortedList<string, object>() { { "@PersonId", PersonId } });
                if (imgBytes != null)
                {
                    try
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);
                        img.ScaleToFit(125, 125);

                        //img.Border = iTextSharp.text.Rectangle.BOX;
                        //img.BorderColor = iTextSharp.text.BaseColor.BLACK;
                        //img.BorderWidth = 5f;

                        img.SetAbsolutePosition(420, 690);
                        cb.AddImage(img);
                    }
                    catch { }
                }
                acrFlds.SetField("Number", person.Number.ToString());

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                acrFlds.SetField("FIOEng", ((person.SurnameEng ?? "") + " " + (person.NameEng ?? "") + " " + (person.SecondNameEng ?? "")).Trim());

                acrFlds.SetField("Sex", person.Sex ? "male" : "female");
                acrFlds.SetField("Status", person.Status);

                acrFlds.SetField("DateOfBirth", person.BirthDate.Value.ToShortDateString());
                acrFlds.SetField("PlaceOfBirth", person.BirthPlace);

                acrFlds.SetField("Nationality", person.Nationality);

                string Address = string.Format("{0} {1}{2}",
                   (person.Code) ?? "",
                   (person.IsRussia ? ((person.Region + ", ") ?? "") : person.Country + ", "),
                   (person.City + ", ") ?? "")
                   +
                   string.Format("{0} {1} {2} {3}",
                   person.Street ?? "",
                   (person.House == string.Empty ? "" : person.House),
                   (person.Korpus == string.Empty ? "" : person.Korpus),
                   (person.Flat == string.Empty ? "" : person.Flat));

                acrFlds.SetField("PostalAddress", Address);
                acrFlds.SetField("Email", person.Email);
                

                acrFlds.SetField("HasFee", HasFee ? "yes" : "no");
                acrFlds.SetField("HasNoFee", HasNoFee ? "yes" : "no");
                acrFlds.SetField("ValidUntil", person.PassportValid.HasValue ? (person.PassportValid.Value == DateTime.MinValue ? "" : person.PassportValid.Value.ToShortDateString()) : "");
                acrFlds.SetField("VisaAppPlace", person.VisaCountry + " "+person.VisaTown + " " + person.VisaPostAddress + " ");
                acrFlds.SetField("PortRu", person.RuPort.ToString());
                acrFlds.SetField("PortDe", person.DePort.ToString());
                acrFlds.SetField("PortCom", ((person.RuPort+person.DePort)/2).ToString());

                acrFlds.SetField("IntRu",person.RuInt.ToString());
                acrFlds.SetField("IntDe", person.DeInt.ToString());
                acrFlds.SetField("IntCom", ((person.RuInt+person.DeInt)/2).ToString());

                acrFlds.SetField("Interview", person.Interview ? "yes":"no");
                acrFlds.SetField("Overall", ((person.RuPort + person.DePort) / 2 + (person.RuInt+person.DeInt)/2).ToString());

                string sCerts = "";
                foreach (var x in Certs)
                {
                    sCerts += x.NameEng;
                    if (!String.IsNullOrEmpty(x.Number))
                        sCerts +=  " (" + x.Number + ")";
                    if (x.BoolType)
                        sCerts += " passed";
                    else sCerts += (x.ResultValue != null ? " - " + x.ResultValue.ToString() : "");
                    sCerts += ";";
                    sCerts += Environment.NewLine;
                }
                acrFlds.SetField("Certificates", sCerts);

                pdfStm.FormFlattening = true;

                pdfStm.Close();
                pdfRd.Close();

                //return ms.ToArray();

                List<byte[]> lstFiles = new List<byte[]>();
                lstFiles.Add(ms.ToArray());
                lstFiles.AddRange(GetAdditionalFilesToCommunicationAppCard(PersonId));
                lstFiles.Add(GetImageFileToCommunicationAppCard(PersonId));
                return MergePdfFiles(lstFiles);
            }
        }

        public static List<byte[]> GetAdditionalFilesToCommunicationAppCard(Guid PersonId)
        {
            List<byte[]> lstRet = new List<byte[]>();
            string query = @"SELECT FileData
FROM extAbitFiles_All
WHERE extAbitFiles_All.PersonId=@PersonId AND IndexInAppCard > 0 
AND extAbitFiles_All.FileName NOT LIKE '%pasport%'
AND FileExtention = '.pdf' 
AND IsDeleted = 0
order by IndexInAppCard";

            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
            foreach (DataRow rw in tbl.Rows)
            {
                byte[] pdf = rw.Field<byte[]>("FileData");
                //PdfReader pr = new PdfReader(pdf);
                //pr.page
                lstRet.Add(rw.Field<byte[]>("FileData"));
            }

            return lstRet;
        }
        public static byte[] GetImageFileToCommunicationAppCard(Guid PersonId)
        {
            List<byte[]> lstRet = new List<byte[]>();
            string query = @"SELECT FileData
FROM extAbitFiles_All
WHERE extAbitFiles_All.PersonId=@PersonId AND IndexInAppCard > 0 
AND extAbitFiles_All.FileName NOT LIKE '%pasport%'
AND FileExtention IN ('.jpg', '.jpeg', '.png', '.bmp')
AND IsDeleted = 0
order by IndexInAppCard";

            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
            if (tbl.Rows.Count == 0)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter pw = PdfWriter.GetInstance(doc, ms);
                doc.Open();
                
                //добавляем штрихкод
                PdfContentByte cb = pw.DirectContent;
                foreach (DataRow rw in tbl.Rows)
                {
                    try
                    {
                        doc.NewPage();
                        byte[] imgBytes = rw.Field<byte[]>(0);
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);

                        float fMaxSizeX = 520f;
                        float fMaxSizeY = 770f;
                        if (img.PlainWidth > fMaxSizeX || img.PlainHeight > fMaxSizeY)
                        {
                            //float fPlainWidth = img.PlainWidth;
                            //if (img.PlainWidth > fMaxSizeX)
                            img.ScaleToFit(fMaxSizeX, fMaxSizeY);
                        }
                        img.SetAbsolutePosition(25, 25);
                        cb.AddImage(img);
                    }
                    catch { }
                }

                doc.Close();
                doc.Dispose();
                return ms.ToArray();
            }
        }

        public static byte[] GetCommunicationAbitList(string dirPath, GlobalCommunicationModelApplicantList model)
        { 
            string dotName = "CommunicationAbitList.pdf";

            List<byte[]> lstFiles = new List<byte[]>();
            
            for (int page = 0; page < (model.ApplicantList.Count() / 20 ) + 1; page++)
            {
                byte[] templateBytes;
                MemoryStream ms = new MemoryStream();
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                AcroFields acrFlds = pdfStm.AcroFields;
                int iS = 0;
                for (int i = page*20; i < (page+1)*20 && i< model.ApplicantList.Count; i++)
                {
                    var person = model.ApplicantList[i];
                    string s = iS.ToString();

                    acrFlds.SetField("Number" + s, person.Number.ToString());
                    acrFlds.SetField("FIO" + s, person.FIO.Trim());
                    acrFlds.SetField("IsComplete" + s, person.isComplete ? "yes" : "no");

                    acrFlds.SetField("PortRu" + s, person.PortfolioAssessmentRu);
                    acrFlds.SetField("PortDe" + s, person.PortfolioAssessmentDe);
                    acrFlds.SetField("PortCommon" + s, person.PortfolioAssessmentCommon);

                    acrFlds.SetField("Interview" + s, person.Interview ? "yes" : "no");

                    acrFlds.SetField("IntRu" + s, person.InterviewAssessmentRu);
                    acrFlds.SetField("IntDe" + s, person.InterviewAssessmentDe);
                    acrFlds.SetField("IntCommon" + s, person.InterviewAssessmentCommon);

                    acrFlds.SetField("Overall" + s, person.OverallResults.ToString());
                    iS++;
                }
                acrFlds.SetField("PageNum", (page+1).ToString());
                acrFlds.SetField("DateTime", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

                pdfStm.FormFlattening = true;

                pdfStm.Close();
                pdfRd.Close();

                lstFiles.Add(ms.ToArray());
            }

            MemoryStream ms2 = new MemoryStream();
            Document document = new Document(PageSize.A4_LANDSCAPE);
            PdfWriter writer = PdfWriter.GetInstance(document, ms2);

            document.Open();

            foreach (byte[] doc in lstFiles)
            {
                PdfReader reader = new PdfReader(doc);
                int n = reader.NumberOfPages;

                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page;

                for (int i = 0; i < n; i++)
                {
                    document.SetPageSize(PageSize.A4_LANDSCAPE);
                    document.NewPage();
                    page = writer.GetImportedPage(reader, i + 1);

                    cb.AddTemplate(page, 0, 1f, -1f, 0,
                                      2 * reader.GetPageSizeWithRotation(i + 1).Width / 3 + 50,
                                    0);
                }
            }

            document.Close();
            return ms2.ToArray();
        }
        public static string[] GetSplittedStringByCell(string sInput)
        {
            if (string.IsNullOrEmpty(sInput))
                return new string[1];

            string[] sTmp = new string[sInput.Length];
            int iOutPos = 0;

            for (int i = 0; i < sInput.Length; i++)
            {
                char currChar = sInput[i];
                char nextChar = (i + 1) == sInput.Length ? ' ' : sInput[i + 1];

                List<char> lstToOneCell = new List<char>() { '.', ',', ';', ':', '(', ')', '!', '-' };
                if (lstToOneCell.Contains(currChar) || lstToOneCell.Contains(nextChar))
                {
                    sTmp[iOutPos] = currChar.ToString() + ' ' + nextChar.ToString();
                    i++;
                }
                else
                    sTmp[iOutPos] = currChar.ToString();
                    
                iOutPos++;
            }

            string[] sOut = new string[iOutPos];
            for (int i = 0; i < iOutPos; i++)
                sOut[i] = sTmp[i];

            return sOut;
        }
    } 
}