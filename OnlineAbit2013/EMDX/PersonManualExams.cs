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
    
    public partial class PersonManualExams
    {
        public int Id { get; set; }
        public Nullable<System.Guid> PersonId { get; set; }
        public Nullable<int> ExamId { get; set; }
    
        public virtual EgeExam EgeExam { get; set; }
    }
}
