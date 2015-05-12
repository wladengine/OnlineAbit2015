using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAbit2013.Models
{
    public class InnerEntryInEntrySmallEntity
    {
        public string ObrazProgramName { get; set; }
        public string ProfileName { get; set; }
        public int? Priority { get; set; }
    }

    public class PriorityChangerApplicationModel
    {
        public Guid ApplicationVersionId { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid CommitId { get; set; }
        public string CommitName { get; set; }
        public string ErrorText { get; set; }
        public string MessageText { get; set; }
        public List<KeyValuePair<Guid, InnerEntryInEntrySmallEntity>> lstInnerEntries { get; set; }
    }
}