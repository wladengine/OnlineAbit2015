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
    
    public partial class ApplicationSelectedExam
    {
        public System.Guid ApplicationId { get; set; }
        public System.Guid ExamInEntryBlockUnitId { get; set; }
        public Nullable<int> ExamTimetableId { get; set; }
        public Nullable<System.DateTime> RegistrationDate { get; set; }
    
        public virtual ExamTimetable ExamTimetable { get; set; }
    }
}
