using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineAbit2013.Models
{
    public class RuslangExamModelPersonList
    {
        public bool Enable { get; set; }
        public string findstring { get; set; }
        public List<RuslangExamModelPerson> PersonList { get; set; }
        public int MaxBlocks { get; set; }
        public AbitType? type { get; set; }
    }

    public class RuslangExamModelPerson
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
