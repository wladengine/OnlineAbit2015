﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineAbit2013.EMDX
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class OnlinePriemEntities : DbContext
    {
        public OnlinePriemEntities()
            : base("name=OnlinePriemEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C_Entry> C_Entry { get; set; }
        public virtual DbSet<Application> Application { get; set; }
        public virtual DbSet<ApplicationCommit> ApplicationCommit { get; set; }
        public virtual DbSet<ApplicationCommitVersion> ApplicationCommitVersion { get; set; }
        public virtual DbSet<ApplicationCommitVersonDetails> ApplicationCommitVersonDetails { get; set; }
        public virtual DbSet<ApplicationDetails> ApplicationDetails { get; set; }
        public virtual DbSet<ApplicationFile> ApplicationFile { get; set; }
        public virtual DbSet<ApplicationSelectedExam> ApplicationSelectedExam { get; set; }
        public virtual DbSet<ApplicationVersion> ApplicationVersion { get; set; }
        public virtual DbSet<ApplicationVersionDetails> ApplicationVersionDetails { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<EgeCertificate> EgeCertificate { get; set; }
        public virtual DbSet<EgeExam> EgeExam { get; set; }
        public virtual DbSet<EgeMark> EgeMark { get; set; }
        public virtual DbSet<Exam> Exam { get; set; }
        public virtual DbSet<ExamInEntryBlock> ExamInEntryBlock { get; set; }
        public virtual DbSet<ExamInEntryBlockUnit> ExamInEntryBlockUnit { get; set; }
        public virtual DbSet<ExamName> ExamName { get; set; }
        public virtual DbSet<ExamTimetable> ExamTimetable { get; set; }
        public virtual DbSet<FileType> FileType { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<GroupUsers> GroupUsers { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<OlympBook> OlympBook { get; set; }
        public virtual DbSet<Olympiads> Olympiads { get; set; }
        public virtual DbSet<OlympLevel> OlympLevel { get; set; }
        public virtual DbSet<OlympName> OlympName { get; set; }
        public virtual DbSet<OlympSubject> OlympSubject { get; set; }
        public virtual DbSet<OlympType> OlympType { get; set; }
        public virtual DbSet<OlympValue> OlympValue { get; set; }
        public virtual DbSet<PassportType> PassportType { get; set; }
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<PersonAddInfo> PersonAddInfo { get; set; }
        public virtual DbSet<PersonChangeStudyFormReason> PersonChangeStudyFormReason { get; set; }
        public virtual DbSet<PersonContacts> PersonContacts { get; set; }
        public virtual DbSet<PersonCurrentEducation> PersonCurrentEducation { get; set; }
        public virtual DbSet<PersonDisorderInfo> PersonDisorderInfo { get; set; }
        public virtual DbSet<PersonEducationDocument> PersonEducationDocument { get; set; }
        public virtual DbSet<PersonFileType> PersonFileType { get; set; }
        public virtual DbSet<PersonHighEducationInfo> PersonHighEducationInfo { get; set; }
        public virtual DbSet<PersonOtherPassport> PersonOtherPassport { get; set; }
        public virtual DbSet<PersonSchoolInfo> PersonSchoolInfo { get; set; }
        public virtual DbSet<PersonScienceWork> PersonScienceWork { get; set; }
        public virtual DbSet<PersonSportQualification> PersonSportQualification { get; set; }
        public virtual DbSet<PersonVisaInfo> PersonVisaInfo { get; set; }
        public virtual DbSet<PersonWork> PersonWork { get; set; }
        public virtual DbSet<PortfolioFilesMark> PortfolioFilesMark { get; set; }
        public virtual DbSet<PortfolioStatus> PortfolioStatus { get; set; }
        public virtual DbSet<Qualification> Qualification { get; set; }
        public virtual DbSet<Region> Region { get; set; }
        public virtual DbSet<ReturnDocumentType> ReturnDocumentType { get; set; }
        public virtual DbSet<SchoolExitClassToStudyLevel> SchoolExitClassToStudyLevel { get; set; }
        public virtual DbSet<SchoolTypeAll> SchoolTypeAll { get; set; }
        public virtual DbSet<Semester> Semester { get; set; }
        public virtual DbSet<SP_Faculty> SP_Faculty { get; set; }
        public virtual DbSet<SP_LicenseProgram> SP_LicenseProgram { get; set; }
        public virtual DbSet<SP_ObrazProgram> SP_ObrazProgram { get; set; }
        public virtual DbSet<SP_Profile> SP_Profile { get; set; }
        public virtual DbSet<SP_StudyLevel> SP_StudyLevel { get; set; }
        public virtual DbSet<SP_StudyPlanHelp> SP_StudyPlanHelp { get; set; }
        public virtual DbSet<SportQualification> SportQualification { get; set; }
        public virtual DbSet<StudyBasis> StudyBasis { get; set; }
        public virtual DbSet<StudyForm> StudyForm { get; set; }
        public virtual DbSet<StudyLevel> StudyLevel { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<VuzAdditionalType> VuzAdditionalType { get; set; }
        public virtual DbSet<ApplicationSecondType> ApplicationSecondType { get; set; }
        public virtual DbSet<Abiturient> Abiturient { get; set; }
        public virtual DbSet<Entry> Entry { get; set; }
        public virtual DbSet<extPerson> extPerson { get; set; }
        public virtual DbSet<qAbiturient> qAbiturient { get; set; }
        public virtual DbSet<qObrazProgram> qObrazProgram { get; set; }
        public virtual DbSet<SchoolType> SchoolType { get; set; }
        public virtual DbSet<Comission> Comission { get; set; }
        public virtual DbSet<InnerEntryInEntry> InnerEntryInEntry { get; set; }
        public virtual DbSet<extApplicationDetails> extApplicationDetails { get; set; }
        public virtual DbSet<PersonFile> PersonFile { get; set; }
        public virtual DbSet<extDefaultEntryDetails> extDefaultEntryDetails { get; set; }
        public virtual DbSet<SchoolExitClass> SchoolExitClass { get; set; }
        public virtual DbSet<ScienceWorkType> ScienceWorkType { get; set; }
        public virtual DbSet<LanguageCertificatesType> LanguageCertificatesType { get; set; }
        public virtual DbSet<PersonLanguageCertificates> PersonLanguageCertificates { get; set; }
        public virtual DbSet<ApplicationAddedToProtocol> ApplicationAddedToProtocol { get; set; }
        public virtual DbSet<ExamTimeTableOneDayRestriction> ExamTimeTableOneDayRestriction { get; set; }
    
        public virtual int PersonEducationDocument_delete(Nullable<System.Guid> personId, Nullable<int> id)
        {
            var personIdParameter = personId.HasValue ?
                new ObjectParameter("PersonId", personId) :
                new ObjectParameter("PersonId", typeof(System.Guid));
    
            var idParameter = id.HasValue ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PersonEducationDocument_delete", personIdParameter, idParameter);
        }
    
        public virtual int PersonEducationDocument_insert(Nullable<System.Guid> personId, Nullable<int> schoolTypeId, Nullable<int> countryEducId, Nullable<int> regionEducId, Nullable<int> vuzAdditionalTypeId, string schoolCity, string schoolName, string schoolNum, string schoolExitYear, Nullable<int> schoolExitClassId, string series, string number, Nullable<bool> isEqual, string equalDocumentNumber, Nullable<double> avgMark, Nullable<bool> isExcellent, ObjectParameter id)
        {
            var personIdParameter = personId.HasValue ?
                new ObjectParameter("PersonId", personId) :
                new ObjectParameter("PersonId", typeof(System.Guid));
    
            var schoolTypeIdParameter = schoolTypeId.HasValue ?
                new ObjectParameter("SchoolTypeId", schoolTypeId) :
                new ObjectParameter("SchoolTypeId", typeof(int));
    
            var countryEducIdParameter = countryEducId.HasValue ?
                new ObjectParameter("CountryEducId", countryEducId) :
                new ObjectParameter("CountryEducId", typeof(int));
    
            var regionEducIdParameter = regionEducId.HasValue ?
                new ObjectParameter("RegionEducId", regionEducId) :
                new ObjectParameter("RegionEducId", typeof(int));
    
            var vuzAdditionalTypeIdParameter = vuzAdditionalTypeId.HasValue ?
                new ObjectParameter("VuzAdditionalTypeId", vuzAdditionalTypeId) :
                new ObjectParameter("VuzAdditionalTypeId", typeof(int));
    
            var schoolCityParameter = schoolCity != null ?
                new ObjectParameter("SchoolCity", schoolCity) :
                new ObjectParameter("SchoolCity", typeof(string));
    
            var schoolNameParameter = schoolName != null ?
                new ObjectParameter("SchoolName", schoolName) :
                new ObjectParameter("SchoolName", typeof(string));
    
            var schoolNumParameter = schoolNum != null ?
                new ObjectParameter("SchoolNum", schoolNum) :
                new ObjectParameter("SchoolNum", typeof(string));
    
            var schoolExitYearParameter = schoolExitYear != null ?
                new ObjectParameter("SchoolExitYear", schoolExitYear) :
                new ObjectParameter("SchoolExitYear", typeof(string));
    
            var schoolExitClassIdParameter = schoolExitClassId.HasValue ?
                new ObjectParameter("SchoolExitClassId", schoolExitClassId) :
                new ObjectParameter("SchoolExitClassId", typeof(int));
    
            var seriesParameter = series != null ?
                new ObjectParameter("Series", series) :
                new ObjectParameter("Series", typeof(string));
    
            var numberParameter = number != null ?
                new ObjectParameter("Number", number) :
                new ObjectParameter("Number", typeof(string));
    
            var isEqualParameter = isEqual.HasValue ?
                new ObjectParameter("IsEqual", isEqual) :
                new ObjectParameter("IsEqual", typeof(bool));
    
            var equalDocumentNumberParameter = equalDocumentNumber != null ?
                new ObjectParameter("EqualDocumentNumber", equalDocumentNumber) :
                new ObjectParameter("EqualDocumentNumber", typeof(string));
    
            var avgMarkParameter = avgMark.HasValue ?
                new ObjectParameter("AvgMark", avgMark) :
                new ObjectParameter("AvgMark", typeof(double));
    
            var isExcellentParameter = isExcellent.HasValue ?
                new ObjectParameter("IsExcellent", isExcellent) :
                new ObjectParameter("IsExcellent", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PersonEducationDocument_insert", personIdParameter, schoolTypeIdParameter, countryEducIdParameter, regionEducIdParameter, vuzAdditionalTypeIdParameter, schoolCityParameter, schoolNameParameter, schoolNumParameter, schoolExitYearParameter, schoolExitClassIdParameter, seriesParameter, numberParameter, isEqualParameter, equalDocumentNumberParameter, avgMarkParameter, isExcellentParameter, id);
        }
    
        public virtual int PersonEducationDocument_update(Nullable<System.Guid> personId, Nullable<int> schoolTypeId, Nullable<int> countryEducId, Nullable<int> regionEducId, Nullable<int> vuzAdditionalTypeId, string schoolCity, string schoolName, string schoolNum, string schoolExitYear, Nullable<int> schoolExitClassId, string series, string number, Nullable<bool> isEqual, string equalDocumentNumber, Nullable<double> avgMark, Nullable<bool> isExcellent, Nullable<int> id)
        {
            var personIdParameter = personId.HasValue ?
                new ObjectParameter("PersonId", personId) :
                new ObjectParameter("PersonId", typeof(System.Guid));
    
            var schoolTypeIdParameter = schoolTypeId.HasValue ?
                new ObjectParameter("SchoolTypeId", schoolTypeId) :
                new ObjectParameter("SchoolTypeId", typeof(int));
    
            var countryEducIdParameter = countryEducId.HasValue ?
                new ObjectParameter("CountryEducId", countryEducId) :
                new ObjectParameter("CountryEducId", typeof(int));
    
            var regionEducIdParameter = regionEducId.HasValue ?
                new ObjectParameter("RegionEducId", regionEducId) :
                new ObjectParameter("RegionEducId", typeof(int));
    
            var vuzAdditionalTypeIdParameter = vuzAdditionalTypeId.HasValue ?
                new ObjectParameter("VuzAdditionalTypeId", vuzAdditionalTypeId) :
                new ObjectParameter("VuzAdditionalTypeId", typeof(int));
    
            var schoolCityParameter = schoolCity != null ?
                new ObjectParameter("SchoolCity", schoolCity) :
                new ObjectParameter("SchoolCity", typeof(string));
    
            var schoolNameParameter = schoolName != null ?
                new ObjectParameter("SchoolName", schoolName) :
                new ObjectParameter("SchoolName", typeof(string));
    
            var schoolNumParameter = schoolNum != null ?
                new ObjectParameter("SchoolNum", schoolNum) :
                new ObjectParameter("SchoolNum", typeof(string));
    
            var schoolExitYearParameter = schoolExitYear != null ?
                new ObjectParameter("SchoolExitYear", schoolExitYear) :
                new ObjectParameter("SchoolExitYear", typeof(string));
    
            var schoolExitClassIdParameter = schoolExitClassId.HasValue ?
                new ObjectParameter("SchoolExitClassId", schoolExitClassId) :
                new ObjectParameter("SchoolExitClassId", typeof(int));
    
            var seriesParameter = series != null ?
                new ObjectParameter("Series", series) :
                new ObjectParameter("Series", typeof(string));
    
            var numberParameter = number != null ?
                new ObjectParameter("Number", number) :
                new ObjectParameter("Number", typeof(string));
    
            var isEqualParameter = isEqual.HasValue ?
                new ObjectParameter("IsEqual", isEqual) :
                new ObjectParameter("IsEqual", typeof(bool));
    
            var equalDocumentNumberParameter = equalDocumentNumber != null ?
                new ObjectParameter("EqualDocumentNumber", equalDocumentNumber) :
                new ObjectParameter("EqualDocumentNumber", typeof(string));
    
            var avgMarkParameter = avgMark.HasValue ?
                new ObjectParameter("AvgMark", avgMark) :
                new ObjectParameter("AvgMark", typeof(double));
    
            var isExcellentParameter = isExcellent.HasValue ?
                new ObjectParameter("IsExcellent", isExcellent) :
                new ObjectParameter("IsExcellent", typeof(bool));
    
            var idParameter = id.HasValue ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PersonEducationDocument_update", personIdParameter, schoolTypeIdParameter, countryEducIdParameter, regionEducIdParameter, vuzAdditionalTypeIdParameter, schoolCityParameter, schoolNameParameter, schoolNumParameter, schoolExitYearParameter, schoolExitClassIdParameter, seriesParameter, numberParameter, isEqualParameter, equalDocumentNumberParameter, avgMarkParameter, isExcellentParameter, idParameter);
        }
    }
}
