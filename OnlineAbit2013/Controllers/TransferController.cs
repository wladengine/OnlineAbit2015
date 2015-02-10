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

namespace OnlineAbit2013.Controllers
{
    public class TransferController : Controller
    {
        #region Ajax

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetProfs(string studyform, string studybasis, string entry, string isSecond = "0", string isParallel = "0", string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            int iEntryId = 1;
            int iSemesterId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            int iStudyLevelId = 0;
            if (iEntryId == 8 || iEntryId == 10)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 3;
            }

            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1;

            bool bIsSecond = isSecond == "1" ? true : false;
            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;

            string query = "SELECT DISTINCT LicenseProgramId, LicenseProgramCode, LicenseProgramName, LicenseProgramNameEng FROM Entry " +
                "WHERE StudyFormId=@StudyFormId AND StudyBasisId=@StudyBasisId AND StudyLevelGroupId=@StudyLevelGroupId AND IsSecond=@IsSecond AND IsParallel=@IsParallel " +
                "AND IsReduced=@IsReduced AND [CampaignYear]=@Year AND SemesterId=@SemesterId";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@StudyLevelGroupId", iEntryId);//2 == mag, 1 == 1kurs, 3 - SPO
            if (iStudyLevelId != 0)
            {
                query += " AND StudyLevelId=@StudyLevelId";
                dic.Add("@StudyLevelId", iStudyLevelId);//Id=8 - 9kl, Id=10 - 11 kl
            }
            dic.Add("@IsSecond", bIsSecond);
            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@Year", Util.iPriemYear);
            dic.Add("@SemesterId", iSemesterId);

            bool isEng = Util.GetCurrentThreadLanguageIsEng();

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var profs =
                (from DataRow rw in tbl.Rows
                 select new
                 {
                     Id = rw.Field<int>("LicenseProgramId"),
                     Name = "(" + rw.Field<string>("LicenseProgramCode") + ") " +
                        (isEng ?
                          (string.IsNullOrEmpty(rw.Field<string>("LicenseProgramNameEng")) ? rw.Field<string>("LicenseProgramName") : rw.Field<string>("LicenseProgramNameEng"))
                          : rw.Field<string>("LicenseProgramName"))
                 }).OrderBy(x => x.Name);

            if (profs.Count() == 0)
            {
                return Json(new { NoFree = true });
            }

            return Json(profs);
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetObrazPrograms(string prof, string studyform, string studybasis, string entry, string isParallel = "0", string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;

            int iEntryId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            int iStudyLevelId = 0;
            if (iEntryId == 8 || iEntryId == 10)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 3;
            }
            if (iEntryId == 16)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 1;
            }
            if (iEntryId == 18)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 1;
            }
            if (iEntryId == 17)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 2;
            }
            if (iEntryId == 15)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 4;
            }
            int iSemesterId;
            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1;
            int iProfessionId = 1;
            if (!int.TryParse(prof, out iProfessionId))
                iProfessionId = 1;

            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;

            string query = "SELECT DISTINCT ObrazProgramId, ObrazProgramName, ObrazProgramNameEng FROM Entry " +
                "WHERE StudyFormId=@StudyFormId AND StudyBasisId=@StudyBasisId AND LicenseProgramId=@LicenseProgramId " +
                "AND StudyLevelGroupId=@StudyLevelGroupId AND IsParallel=@IsParallel AND IsReduced=@IsReduced " +
                " AND CampaignYear=@Year AND SemesterId=@SemesterId ";
            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@LicenseProgramId", iProfessionId);

            dic.Add("@StudyLevelGroupId", iEntryId);
            if (iStudyLevelId != 0)
            {
                query += " AND StudyLevelId=@StudyLevelId";
                dic.Add("@StudyLevelId", iStudyLevelId);//Id=8 - 9kl, Id=10 - 11 kl
            }

            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@Year", Util.iPriemYear);
            dic.Add("@SemesterId", iSemesterId);

            bool isEng = Util.GetCurrentThreadLanguageIsEng();

            DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
            var OPs = from DataRow rw in tbl.Rows
                      select new
                      {
                          Id = rw.Field<int>("ObrazProgramId"),
                          Name = isEng ?
                            (string.IsNullOrEmpty(rw.Field<string>("ObrazProgramNameEng")) ? rw.Field<string>("ObrazProgramName") : rw.Field<string>("ObrazProgramNameEng"))
                            : rw.Field<string>("ObrazProgramName")
                      };

            return Json(new { NoFree = OPs.Count() > 0 ? false : true, List = OPs });
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetSpecializations(string prof, string obrazprogram, string studyform, string studybasis, string entry, string isParallel = "0", string isReduced = "0", string semesterId = "1")
        {
            Guid PersonId;
            Util.CheckAuthCookies(Request.Cookies, out PersonId);

            int iStudyFormId;
            int iStudyBasisId;
            if (!int.TryParse(studyform, out iStudyFormId))
                iStudyFormId = 1;
            if (!int.TryParse(studybasis, out iStudyBasisId))
                iStudyBasisId = 1;

            int iEntryId = 1;
            if (!int.TryParse(entry, out iEntryId))
                iEntryId = 1;

            int iStudyLevelId = 0;
            if (iEntryId == 8 || iEntryId == 10)
            {
                iStudyLevelId = iEntryId;
                iEntryId = 3;
            }

            int iProfessionId = 1;
            if (!int.TryParse(prof, out iProfessionId))
                iProfessionId = 1;
            int iObrazProgramId = 1;
            if (!int.TryParse(obrazprogram, out iObrazProgramId))
                iObrazProgramId = 1;
            int iSemesterId;
            if (!int.TryParse(semesterId, out iSemesterId))
                iSemesterId = 1;

            bool bIsReduced = isReduced == "1" ? true : false;
            bool bIsParallel = isParallel == "1" ? true : false;

            string query = "SELECT DISTINCT ProfileId, ProfileName FROM Entry INNER JOIN SP_StudyLevel ON SP_StudyLevel.Id = Entry.StudyLevelId WHERE StudyFormId=@StudyFormId " +
                "AND StudyBasisId=@StudyBasisId AND LicenseProgramId=@LicenseProgramId AND ObrazProgramId=@ObrazProgramId AND Entry.StudyLevelGroupId=@StudyLevelGroupId " +
               // "AND Entry.Id NOT IN (SELECT EntryId FROM [Application] WHERE PersonId=@PersonId AND Enabled='True' AND EntryId IS NOT NULL) " +
                "AND IsParallel=@IsParallel AND IsReduced=@IsReduced AND CampaignYear=@Year AND SemesterId=@SemesterId";

            SortedList<string, object> dic = new SortedList<string, object>();
            dic.Add("@PersonId", PersonId);
            dic.Add("@StudyFormId", iStudyFormId);
            dic.Add("@StudyBasisId", iStudyBasisId);
            dic.Add("@LicenseProgramId", iProfessionId);
            dic.Add("@ObrazProgramId", iObrazProgramId);
            dic.Add("@StudyLevelGroupId", iEntryId);
            if (iStudyLevelId != 0)
            {
                query += " AND StudyLevelId=@StudyLevelId";
                dic.Add("@StudyLevelId", iStudyLevelId);//Id=8 - 9kl, Id=10 - 11 kl
            }
            dic.Add("@IsParallel", bIsParallel);
            dic.Add("@IsReduced", bIsReduced);
            dic.Add("@Year", Util.iPriemYear);
            dic.Add("@SemesterId", iSemesterId);

            DataTable tblSpecs = Util.AbitDB.GetDataTable(query, dic);
            var Specs =
                from DataRow rw in tblSpecs.Rows
                select new { SpecId = rw.Field<Guid?>("ProfileId"), SpecName = rw.Field<string>("ProfileName") };

            var ret = new
            {
                NoFree = Specs.Count() == 0 ? true : false,
                List = Specs.Select(x => new { Id = x.SpecId, Name = x.SpecName }).ToList()
            };

            int GosLine = Util.IsGosLine(PersonId);

            return Json(new { ret, GosLine });
        }

        #endregion
    }
}
