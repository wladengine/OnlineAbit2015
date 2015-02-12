using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OnlineAbit2013.Models;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using System.Net.Mail;

namespace OnlineAbit2013.Controllers
{
    public class AbiturientController : Controller
    {
        public ActionResult OpenPersonalAccount()
        {
            Request.Cookies.SetThreadCultureByCookies();
            return View("PersonStartPage");
        }
    }
}
