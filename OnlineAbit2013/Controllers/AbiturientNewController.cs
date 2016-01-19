using OnlineAbit2013.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineAbit2013.Controllers
{
    public class AbiturientNewController : Controller
    {
        #region UFMS
        public ActionResult RusLangExam_ufms()
        {
            return RedirectToAction("RusLangExam_ufms", "Abiturient");
        }

        public ActionResult ufms(string HiddenId)
        {
            return RedirectToAction("RusLangExam_ufms", "Abiturient");
        }
        #endregion
    }
}
