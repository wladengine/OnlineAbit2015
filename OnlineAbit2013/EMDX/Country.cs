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
    
    public partial class Country
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Country()
        {
            this.Person = new HashSet<Person>();
            this.PersonContacts = new HashSet<PersonContacts>();
            this.PersonEducationDocument = new HashSet<PersonEducationDocument>();
            this.PersonVisaInfo = new HashSet<PersonVisaInfo>();
        }
    
        public int Id { get; set; }
        public string NameEng { get; set; }
        public string Name { get; set; }
        public int RegionId { get; set; }
        public Nullable<int> LevelOfUsing { get; set; }
        public Nullable<int> PriemDictionaryId { get; set; }
        public bool IsCountry { get; set; }
        public bool IsNationality { get; set; }
        public bool IsSNG { get; set; }
        public bool IsRussia { get; set; }
    
        public virtual Region Region { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Person> Person { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonContacts> PersonContacts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonEducationDocument> PersonEducationDocument { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonVisaInfo> PersonVisaInfo { get; set; }
    }
}