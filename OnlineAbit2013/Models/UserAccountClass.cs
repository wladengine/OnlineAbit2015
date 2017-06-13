using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAbit2013
{
    public class UserAccountClass
    {
        public Guid Id { get; set; }
        public string SID { get; set; }
        public bool IsApproved { get; set; }
        public string Ticket { get; set; }
        public bool? IsForeign { get; set; }
        public bool IsDormsAccount { get; set; }
    }
}