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
    
    public partial class OlympBook
    {
        public System.Guid Id { get; set; }
        public int OlympLevelId { get; set; }
        public int OlympNameId { get; set; }
        public int OlympSubjectId { get; set; }
        public int OlympTypeId { get; set; }
        public int OlympYear { get; set; }
    
        public virtual OlympLevel OlympLevel { get; set; }
        public virtual OlympName OlympName { get; set; }
        public virtual OlympSubject OlympSubject { get; set; }
        public virtual OlympType OlympType { get; set; }
    }
}
