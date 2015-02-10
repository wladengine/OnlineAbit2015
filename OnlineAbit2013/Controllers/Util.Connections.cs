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
            //_priem2012DB = new SQLClass();
            //_priem2012DB.OpenDatabase(ConstClass.priem2012DB);

            //_studDB = new SQLClass("Data Source=81.89.183.21;Initial Catalog=EducationUR;Integrated Security=False;User ID=faculty;Password=parolfaculty;MultipleActiveResultSets=True;");
            //_priem2012DB = new SQLClass("Data Source=srveducation.ad.pu.ru;Initial Catalog=Priem2012;Integrated Security=false;User ID=PriemReader; Password=kukushonok");
        }
    }
}