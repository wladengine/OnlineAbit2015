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
    
    public partial class ReturnDocumentType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ReturnDocumentType()
        {
            this.PersonAddInfo = new HashSet<PersonAddInfo>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEng { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PersonAddInfo> PersonAddInfo { get; set; }
    }
}