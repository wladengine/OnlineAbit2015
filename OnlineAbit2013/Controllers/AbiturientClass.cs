using OnlineAbit2013.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAbit2013.Controllers
{
    public static class AbiturientClass
    {
        public static PriorityChangerApplicationModel GetPriorityChangerApplication(Guid gAppId, Guid gVersionId, Guid PersonId, bool isEng)
        {
            using (OnlinePriemEntities context = new OnlinePriemEntities())
            {
                var PersonInfo = context.Person.Where(x => x.Id == PersonId).FirstOrDefault();
                var appl =
                    (from App in context.Application
                     join Entry in context.Entry on App.EntryId equals Entry.Id
                     join OPIE in context.InnerEntryInEntry on App.EntryId equals OPIE.EntryId

                     where App.PersonId == PersonId && App.IsCommited == true && App.Enabled == true && App.Id == gAppId
                     select new
                     {
                         Id = App.Id,
                         CommitId = App.CommitId,
                         CommitName = isEng ? Entry.StudyLevelGroupNameEng : Entry.StudyLevelGroupNameRus,
                         App.EntryId,
                     }).FirstOrDefault();

                var appPriors = (from AppDetails in context.ApplicationDetails
                                 where AppDetails.ApplicationId == gAppId
                                 select new
                                 {
                                     AppDetails.InnerEntryInEntryId,
                                     AppDetails.InnerEntryInEntryPriority,
                                 }).Distinct().ToList();

                var InnerEnts =
                    (from InnerEnInEntry in context.InnerEntryInEntry
                     where InnerEnInEntry.EntryId == appl.EntryId
                     select new StandartObrazProgramInEntryRow()
                     {
                         Id = InnerEnInEntry.Id,
                         Name = isEng ? InnerEnInEntry.SP_ObrazProgram.NameEng : InnerEnInEntry.SP_ObrazProgram.Name,
                         Priority = InnerEnInEntry.DefaultPriorityValue,
                         DefaultPriority = InnerEnInEntry.DefaultPriorityValue
                     }).ToList();

                var InnerEntryBase = context.InnerEntryInEntry.Where(x => x.EntryId == appl.EntryId)
                    .Select(x => new { x.Id, ObrazProgram = x.SP_ObrazProgram.Name, Profile = x.SP_Profile.Name }).ToList();

                int ind = 0;
                foreach (var InEnt in InnerEnts)
                {
                    if (appPriors.Where(x => x.InnerEntryInEntryId == InEnt.Id).Count() > 0)
                        InnerEnts[ind].Priority = appPriors.Where(x => x.InnerEntryInEntryId == InEnt.Id).First().InnerEntryInEntryPriority;
                    ind++;
                }

                var RetVal = new List<KeyValuePair<Guid, InnerEntryInEntrySmallEntity>>();
                foreach (var zz in appPriors)
                {
                    RetVal.Add(new KeyValuePair<Guid, InnerEntryInEntrySmallEntity>(
                        zz.InnerEntryInEntryId,
                        new InnerEntryInEntrySmallEntity()
                        {
                            ObrazProgramName = InnerEntryBase.Where(x => x.Id == zz.InnerEntryInEntryId).Select(x => x.ObrazProgram).First(),
                            ProfileName = InnerEntryBase.Where(x => x.Id == zz.InnerEntryInEntryId).Select(x => x.Profile).First(),
                            Priority = zz.InnerEntryInEntryPriority
                        })
                    );
                }

                foreach (var xxx in InnerEntryBase)
                {
                    if (RetVal.Where(x => x.Key == xxx.Id).Count() == 0)
                    {
                        RetVal.Add(new KeyValuePair<Guid, InnerEntryInEntrySmallEntity>(
                            xxx.Id,
                            new InnerEntryInEntrySmallEntity()
                            {
                                ObrazProgramName = xxx.ObrazProgram,
                                ProfileName = xxx.Profile,
                                Priority = 1000
                            })
                        );
                    }
                }

                return new PriorityChangerApplicationModel()
                {
                    ApplicationId = gAppId,
                    CommitId = appl.CommitId,
                    CommitName = appl.CommitName,
                    lstInnerEntries = RetVal.OrderBy(x => x.Value.Priority).ThenBy(x => x.Value.ObrazProgramName).ThenBy(x => x.Value.ProfileName).Distinct().ToList(),
                    ApplicationVersionId = gVersionId
                };
            }
        }
    }
}