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
    
    public partial class InnerEntryInEntry
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InnerEntryInEntry()
        {
            this.ApplicationDetails = new HashSet<ApplicationDetails>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid EntryId { get; set; }
        public int ObrazProgramId { get; set; }
        public int ProfileId { get; set; }
        public int DefaultPriorityValue { get; set; }
    
        public virtual C_Entry C_Entry { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApplicationDetails> ApplicationDetails { get; set; }
        public virtual SP_Profile SP_Profile { get; set; }
        public virtual SP_ObrazProgram SP_ObrazProgram { get; set; }
    }
}
