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
    
    public partial class ApplicationCommit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ApplicationCommit()
        {
            this.PersonChangeStudyFormReason = new HashSet<PersonChangeStudyFormReason>();
        }
    
        public System.Guid Id { get; set; }
        public int IntNumber { get; set; }
        public bool IsPrinted { get; set; }
        public bool IsImported { get; set; }
        public Nullable<System.DateTime> DateReviewDocs { get; set; }
        public bool IsVisible { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonChangeStudyFormReason> PersonChangeStudyFormReason { get; set; }
    }
}