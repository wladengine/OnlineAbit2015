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
    
    public partial class PersonLanguageCertificates
    {
        public int Id { get; set; }
        public System.Guid PersonId { get; set; }
        public string Number { get; set; }
        public Nullable<double> ResultValue { get; set; }
        public Nullable<bool> ResultBool { get; set; }
        public int LanguageCertificateTypeId { get; set; }
    
        public virtual LanguageCertificatesType LanguageCertificatesType { get; set; }
    }
}
