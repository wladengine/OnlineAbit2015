//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineAbit2013
{
    using System;
    using System.Collections.Generic;
    
    public partial class Olympiads
    {
        public System.Guid Id { get; set; }
        public System.Guid PersonId { get; set; }
        public int OlympTypeId { get; set; }
        public int OlympNameId { get; set; }
        public int OlympSubjectId { get; set; }
        public int OlympValueId { get; set; }
        public string DocumentSeries { get; set; }
        public string DocumentNumber { get; set; }
        public Nullable<System.DateTime> DocumentDate { get; set; }
        public int OlympYear { get; set; }
    
        public virtual OlympName OlympName { get; set; }
        public virtual OlympSubject OlympSubject { get; set; }
        public virtual OlympType OlympType { get; set; }
        public virtual OlympValue OlympValue { get; set; }
        public virtual Person Person { get; set; }
    }
}
