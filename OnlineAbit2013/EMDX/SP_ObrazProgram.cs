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
    
    public partial class SP_ObrazProgram
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SP_ObrazProgram()
        {
            this.C_Entry = new HashSet<C_Entry>();
            this.PersonCurrentEducation = new HashSet<PersonCurrentEducation>();
            this.InnerEntryInEntry = new HashSet<InnerEntryInEntry>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEng { get; set; }
        public string Number { get; set; }
        public int LicenseProgramId { get; set; }
        public int FacultyId { get; set; }
        public int ProgramModeId { get; set; }
        public bool IsExpress { get; set; }
        public bool IsOpen { get; set; }
        public string Holder { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Entry> C_Entry { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonCurrentEducation> PersonCurrentEducation { get; set; }
        public virtual SP_LicenseProgram SP_LicenseProgram { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InnerEntryInEntry> InnerEntryInEntry { get; set; }
    }
}
