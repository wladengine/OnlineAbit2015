//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineAbit2013
{
    using System;
    using System.Collections.Generic;
    
    public partial class SchoolExitClass
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SchoolExitClass()
        {
            this.PersonEducationDocument = new HashSet<PersonEducationDocument>();
            this.PersonSchoolInfo = new HashSet<PersonSchoolInfo>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int IntValue { get; set; }
        public Nullable<int> OrderNumber { get; set; }
        public string NameEng { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonEducationDocument> PersonEducationDocument { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonSchoolInfo> PersonSchoolInfo { get; set; }
    }
}
