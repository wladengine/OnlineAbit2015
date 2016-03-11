//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Abiturient
    {
        public System.Guid Id { get; set; }
        public System.Guid PersonId { get; set; }
        public int Priority { get; set; }
        public int Barcode { get; set; }
        public bool Enabled { get; set; }
        public int EntryType { get; set; }
        public bool HostelEduc { get; set; }
        public Nullable<System.DateTime> DateOfDisable { get; set; }
        public System.DateTime DateOfStart { get; set; }
        public System.Guid EntryId { get; set; }
        public Nullable<System.Guid> StudyPlanId { get; set; }
        public string StudyPlanNumber { get; set; }
        public Nullable<int> FacultyId { get; set; }
        public string FacultyName { get; set; }
        public int LicenseProgramId { get; set; }
        public string LicenseProgramCode { get; set; }
        public string LicenseProgramName { get; set; }
        public int ObrazProgramId { get; set; }
        public string ObrazProgramCrypt { get; set; }
        public string ObrazProgramName { get; set; }
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public int StudyBasisId { get; set; }
        public string StudyBasisName { get; set; }
        public int StudyFormId { get; set; }
        public string StudyFormName { get; set; }
        public int StudyLevelId { get; set; }
        public string StudyLevelName { get; set; }
        public bool IsApprovedByComission { get; set; }
        public Nullable<int> SecondTypeId { get; set; }
        public bool IsSecond { get; set; }
        public bool IsReduced { get; set; }
        public Nullable<System.DateTime> DateReviewDocs { get; set; }
        public int SemesterId { get; set; }
        public bool IsParallel { get; set; }
        public Nullable<System.DateTime> DateOfClose { get; set; }
        public int CampaignYear { get; set; }
        public string ObrazProgramNameEng { get; set; }
        public string LicenseProgramNameEng { get; set; }
        public string ProfileNameEng { get; set; }
        public string StudyBasisNameEng { get; set; }
        public string StudyFormNameEng { get; set; }
        public System.Guid CommitId { get; set; }
        public bool IsCommited { get; set; }
        public string StudyLevelGroupNameEng { get; set; }
        public string StudyLevelGroupNameRus { get; set; }
        public int StudyLevelGroupId { get; set; }
        public bool IsGosLine { get; set; }
        public int ApplicationCommitNumber { get; set; }
        public Nullable<bool> HasInnerPriorities { get; set; }
        public Nullable<bool> HasManualExams { get; set; }
        public bool IsImported { get; set; }
        public Nullable<int> CompetitionId { get; set; }
        public string ApproverName { get; set; }
        public Nullable<System.DateTime> DocInsertDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsCommonRussianCompetition { get; set; }
        public bool IsPrinted { get; set; }
        public bool IsForeign { get; set; }
        public bool IsCrimea { get; set; }
        public int PersonBarcode { get; set; }
        public bool IsUpdatedByComission { get; set; }
    }
}