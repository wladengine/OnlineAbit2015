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
    
    public partial class Person
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Person()
        {
            this.Application = new HashSet<Application>();
            this.EgeCertificate = new HashSet<EgeCertificate>();
            this.Olympiads = new HashSet<Olympiads>();
            this.PersonEducationDocument = new HashSet<PersonEducationDocument>();
            this.PersonOtherPassport = new HashSet<PersonOtherPassport>();
            this.PersonScienceWork = new HashSet<PersonScienceWork>();
            this.PersonWork = new HashSet<PersonWork>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid UserId { get; set; }
        public int Barcode { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public bool Sex { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public Nullable<int> CountryOfBirth { get; set; }
        public Nullable<int> PassportTypeId { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public Nullable<System.DateTime> PassportDate { get; set; }
        public string PassportAuthor { get; set; }
        public string PassportCode { get; set; }
        public Nullable<System.DateTime> PassportValid { get; set; }
        public Nullable<int> NationalityId { get; set; }
        public int RegistrationStage { get; set; }
        public int AbiturientTypeId { get; set; }
        public bool IsLocked { get; set; }
        public bool IsImported { get; set; }
        public Nullable<System.DateTime> DateReviewDocs { get; set; }
        public Nullable<bool> IsDisabled { get; set; }
        public bool IsCreatedByComission { get; set; }
        public string SNILS { get; set; }
        public bool HasRussianNationality { get; set; }
        public string SurnameEng { get; set; }
        public string NameEng { get; set; }
        public string SecondNameEng { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Application> Application { get; set; }
        public virtual Country Nationality { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EgeCertificate> EgeCertificate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Olympiads> Olympiads { get; set; }
        public virtual PassportType PassportType { get; set; }
        public virtual User User { get; set; }
        public virtual PersonAddInfo PersonAddInfo { get; set; }
        public virtual PersonChangeStudyFormReason PersonChangeStudyFormReason { get; set; }
        public virtual PersonContacts PersonContacts { get; set; }
        public virtual PersonCurrentEducation PersonCurrentEducation { get; set; }
        public virtual PersonDisorderInfo PersonDisorderInfo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonEducationDocument> PersonEducationDocument { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonOtherPassport> PersonOtherPassport { get; set; }
        public virtual PersonSchoolInfo PersonSchoolInfo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonScienceWork> PersonScienceWork { get; set; }
        public virtual PersonSportQualification PersonSportQualification { get; set; }
        public virtual PersonVisaInfo PersonVisaInfo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonWork> PersonWork { get; set; }
    }
}
