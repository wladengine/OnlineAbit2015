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
    
    public partial class ApplicationFile
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> ApplicationId { get; set; }
        public Nullable<System.Guid> CommitId { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string FileExtention { get; set; }
        public string Comment { get; set; }
        public System.DateTime LoadDate { get; set; }
        public bool IsReadOnly { get; set; }
        public string MimeType { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public string FailReason { get; set; }
        public int FileTypeId { get; set; }
        public bool IsDeleted { get; set; }
        public string FileHash { get; set; }
    }
}
