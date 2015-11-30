using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using BDClassLib;

namespace OnlineAbit2013.Controllers
{
    public static partial class Util
    {
        public static void InitDB()
        {
            _abitDB = new SQLClass();
            _abitDB.OpenDatabase(ConstClass.AbitDB);
            _studDB = new SQLClass();
            _studDB.OpenDatabase(ConstClass.StudDB);
        }
    }
}