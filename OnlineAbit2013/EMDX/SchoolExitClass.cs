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
