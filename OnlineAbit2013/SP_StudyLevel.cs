//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineAbit2013
{
    using System;
    using System.Collections.Generic;
    
    public partial class SP_StudyLevel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SP_StudyLevel()
        {
            this.C_Entry = new HashSet<C_Entry>();
            this.PersonCurrentEducation = new HashSet<PersonCurrentEducation>();
            this.SP_LicenseProgram = new HashSet<SP_LicenseProgram>();
        }
    
        public int Id { get; set; }
        public int StudyLevelGroupId { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public string NameEng { get; set; }
        public string ClassName { get; set; }
        public Nullable<int> Duration { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Entry> C_Entry { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonCurrentEducation> PersonCurrentEducation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_LicenseProgram> SP_LicenseProgram { get; set; }
    }
}
