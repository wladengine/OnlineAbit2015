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
    
    public partial class ExamTimetable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ExamTimetable()
        {
            this.ApplicationSelectedExam = new HashSet<ApplicationSelectedExam>();
        }
    
        public int Id { get; set; }
        public System.Guid ExamInEntryBlockUnitId { get; set; }
        public System.DateTime ExamDate { get; set; }
        public string Address { get; set; }
        public System.DateTime DateOfClose { get; set; }
        public Nullable<int> BaseExamTimeTableId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApplicationSelectedExam> ApplicationSelectedExam { get; set; }
        public virtual ExamInEntryBlockUnit ExamInEntryBlockUnit { get; set; }
    }
}
