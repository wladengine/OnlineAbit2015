//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineAbit2013.EMDX
{
    using System;
    using System.Collections.Generic;
    
    public partial class PersonCurrentEducation
    {
        public System.Guid PersonId { get; set; }
        public int EducTypeId { get; set; }
        public int SemesterId { get; set; }
        public int StudyLevelId { get; set; }
        public int CountryId { get; set; }
        public bool HasScholarship { get; set; }
        public bool HasAccreditation { get; set; }
        public string EducName { get; set; }
        public string AccreditationNumber { get; set; }
        public Nullable<System.DateTime> AccreditationDate { get; set; }
        public int LicenseProgramId { get; set; }
        public string ProfileName { get; set; }
        public Nullable<int> RegionId { get; set; }
        public Nullable<int> StudyFormId { get; set; }
        public Nullable<int> StudyBasisId { get; set; }
        public Nullable<int> ObrazProgramId { get; set; }
        public Nullable<System.Guid> ProfileId { get; set; }
    
        public virtual Person Person { get; set; }
        public virtual Region Region { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual SP_LicenseProgram SP_LicenseProgram { get; set; }
        public virtual SP_ObrazProgram SP_ObrazProgram { get; set; }
        public virtual SP_StudyLevel SP_StudyLevel { get; set; }
        public virtual StudyBasis StudyBasis { get; set; }
        public virtual StudyForm StudyForm { get; set; }
    }
}
