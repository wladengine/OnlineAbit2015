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
    
    public partial class ApplicationDetails
    {
        public System.Guid Id { get; set; }
        public System.Guid ApplicationId { get; set; }
        public System.Guid InnerEntryInEntryId { get; set; }
        public int InnerEntryInEntryPriority { get; set; }
    
        public virtual InnerEntryInEntry InnerEntryInEntry { get; set; }
    }
}
