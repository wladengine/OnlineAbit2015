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
    
    public partial class ApplicationFile
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> ApplicationId { get; set; }
        public Nullable<System.Guid> CommitId { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string FileExtention { get; set; }
        public byte[] FileData { get; set; }
        public string Comment { get; set; }
        public System.DateTime LoadDate { get; set; }
        public bool IsReadOnly { get; set; }
        public string MimeType { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public string FailReason { get; set; }
        public int FileTypeId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
