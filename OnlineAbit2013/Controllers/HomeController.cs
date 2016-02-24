using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;


namespace OnlineAbit2013.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string lang)
        {
            if (Request.Url.AbsoluteUri.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == -1 && Util.bUseRedirection &&
                Request.Url.AbsoluteUri.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) == -1)
                return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));

            Guid g;
            if (!Util.CheckAuthCookies(Request.Cookies, out g))
                return RedirectToAction("LogOn", "Account");
            else
            {
                DataTable tbl = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", g }, { "@GroupId", Util.GlobalCommunicationGroupId } });
                if (tbl.Rows.Count > 0)
                    return RedirectToAction("Index", "Communication");

                return RedirectToAction("Main", "Abiturient");
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult AboutRussianCitizens()
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            return View();
        }
    }
}
