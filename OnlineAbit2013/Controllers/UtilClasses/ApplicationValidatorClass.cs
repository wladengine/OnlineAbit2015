using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnlineAbit2013.EMDX;

namespace OnlineAbit2013.Controllers
{
    public static class ApplicationValidatorClass
    {
        public static List<string> GetApplicationCommitValidation(Guid CommitId)
        {
            List<string> lstValidationErrors = new List<string>();

            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var Commit = context.Application.Where(x => x.CommitId == CommitId)
                    .Select(x => new { ApplicationId = x.Id, x.PersonId, x.C_Entry.SP_StudyLevel.StudyLevelGroupId })
                    .ToList();

                Guid PersonId = Commit.Select(x => x.PersonId).DefaultIfEmpty(Guid.Empty).First();
            }

            return lstValidationErrors;
        }
    }
}