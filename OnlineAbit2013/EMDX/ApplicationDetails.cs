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
    
    public partial class ApplicationDetails
    {
        public System.Guid Id { get; set; }
        public System.Guid ApplicationId { get; set; }
        public System.Guid InnerEntryInEntryId { get; set; }
        public int InnerEntryInEntryPriority { get; set; }
        public Nullable<bool> ByUser { get; set; }
    
        public virtual InnerEntryInEntry InnerEntryInEntry { get; set; }
    }
}
