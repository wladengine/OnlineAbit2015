using OnlineAbit2013.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OnlineAbit2013.EMDX;
using System.Transactions;

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

        public ActionResult ConvertToPDF()
        {
            ConvertPersonFiles();
            ConvertApplicationFiles();

            return new FileContentResult(System.Text.Encoding.ASCII.GetBytes("OK"), "text/plain");
        }
        private void ConvertPersonFiles()
        {
            List<PersonFile> lstPF = new System.Collections.Generic.List<PersonFile>();
            List<PersonFile> lstOut = new System.Collections.Generic.List<PersonFile>();
            using (OnlinePriemEntities ctx = new OnlinePriemEntities())
            {
                lstPF =
                    (from PF in ctx.PersonFile
                     join App in ctx.Application on PF.PersonId equals App.PersonId
                     join Ent in ctx.Entry on App.EntryId equals Ent.Id
                     where Ent.SemesterId == 1 && Ent.CampaignYear == 2016
                     && Ent.ObrazProgramName == "Глобальная коммуникация и международная журналистика (на английском языке)"
                     && (PF.FileExtention == ".doc" || PF.FileExtention == ".docx")
                     select PF).ToList();
            }

            foreach (var PF in lstPF)
            {
                string fName = "D:\\DOC_TO_PDF\\" + PF.FileName;
                StreamWriter sw = new StreamWriter(fName);
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                bw.Write(PF.FileData);
                bw.Flush();
                bw.Close();

                string outFileName = Convert(new System.IO.FileInfo(fName));

                byte[] data = System.IO.File.ReadAllBytes(outFileName);
                lstOut.Add(new PersonFile()
                {
                    Id = Guid.NewGuid(),
                    Comment = PF.Comment,
                    FileData = data,
                    FileName = PF.FileName.Replace(".docx", ".pdf").Replace(".doc", ".pdf"),
                    FileExtention = ".pdf",
                    FileSize = data.Length,
                    IsDeleted = false,
                    IsReadOnly = true,
                    LoadDate = DateTime.Now,
                    MimeType = Util.GetMimeFromExtention(".pdf"),
                    PersonFileTypeId = PF.PersonFileTypeId,
                    PersonId = PF.PersonId,
                });

                System.IO.File.Delete(fName);
                System.IO.File.Delete(outFileName);
            }

            using (TransactionScope tran = new TransactionScope())
            {
                using (OnlinePriemEntities ctx = new OnlinePriemEntities())
                {
                    foreach (var PF in lstOut)
                    {
                        ctx.PersonFile.Add(PF);
                        ctx.SaveChanges();
                    }
                }

                tran.Complete();
            }
        }
        private void ConvertApplicationFiles()
        {
            List<ApplicationFile> lstPF = new System.Collections.Generic.List<ApplicationFile>();
            List<ApplicationFile> lstOut = new System.Collections.Generic.List<ApplicationFile>();
            using (OnlinePriemEntities ctx = new OnlinePriemEntities())
            {
                lstPF =
                    (from PF in ctx.ApplicationFile
                     join App in ctx.Application on PF.ApplicationId equals App.Id
                     join Ent in ctx.Entry on App.EntryId equals Ent.Id
                     where Ent.SemesterId == 1 && Ent.CampaignYear == 2016
                     && Ent.ObrazProgramName == "Глобальная коммуникация и международная журналистика (на английском языке)"
                     && (PF.FileExtention == ".doc" || PF.FileExtention == ".docx")
                     select PF).ToList().Union(
                     (from PF in ctx.ApplicationFile
                      join App in ctx.Application on PF.CommitId equals App.CommitId
                      join Ent in ctx.Entry on App.EntryId equals Ent.Id
                      where Ent.SemesterId == 1 && Ent.CampaignYear == 2016
                      && Ent.ObrazProgramName == "Глобальная коммуникация и международная журналистика (на английском языке)"
                      && (PF.FileExtention == ".doc" || PF.FileExtention == ".docx")
                      select PF).ToList()).ToList();
            }

            foreach (var PF in lstPF)
            {
                string fName = "D:\\DOC_TO_PDF\\" + PF.FileName;
                StreamWriter sw = new StreamWriter(fName);
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                bw.Write(PF.FileData);
                bw.Flush();
                bw.Close();

                string outFileName = Convert(new System.IO.FileInfo(fName));

                byte[] data = System.IO.File.ReadAllBytes(outFileName);
                lstOut.Add(new ApplicationFile()
                {
                    Id = Guid.NewGuid(),
                    Comment = PF.Comment,
                    FileData = data,
                    FileName = PF.FileName.Replace(".docx", ".pdf").Replace(".doc", ".pdf"),
                    FileExtention = ".pdf",
                    FileSize = data.Length,
                    IsDeleted = false,
                    IsReadOnly = true,
                    LoadDate = DateTime.Now,
                    MimeType = Util.GetMimeFromExtention(".pdf"),
                    ApplicationId = PF.ApplicationId,
                    CommitId = PF.CommitId,
                    FileTypeId = PF.FileTypeId,
                });

                System.IO.File.Delete(fName);
                System.IO.File.Delete(outFileName);
            }

            using (TransactionScope tran = new TransactionScope())
            {
                using (OnlinePriemEntities ctx = new OnlinePriemEntities())
                {
                    foreach (var PF in lstOut)
                    {
                        ctx.ApplicationFile.Add(PF);
                        ctx.SaveChanges();
                    }
                }

                tran.Complete();
            }
        }

        private string Convert(System.IO.FileInfo wordFile)
        {
            // Create a new Microsoft Word application object
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();

            // C# doesn't have optional arguments so we'll need a dummy value
            object oMissing = System.Reflection.Missing.Value;

            word.Visible = false;
            word.ScreenUpdating = false;

            // Cast as Object for word Open method
            Object filename = (Object)wordFile.FullName;

            // Use the dummy value as a placeholder for optional arguments
            Document doc = word.Documents.Open(ref filename, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            doc.Activate();

            object outputFileName = wordFile.FullName.Replace(".docx", ".pdf").Replace(".doc", ".pdf");
            object fileFormat = WdSaveFormat.wdFormatPDF;

            // Save document into PDF Format
            doc.SaveAs(ref outputFileName,
                ref fileFormat, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            // Close the Word document, but leave the Word application open.
            // doc has to be cast to type _Document so that it will find the
            // correct Close method.                
            object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
            ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
            doc = null;

            // word has to be cast to type _Application so that it will find
            // the correct Quit method.
            ((_Application)word).Quit(ref oMissing, ref oMissing, ref oMissing);
            word = null;

            return outputFileName.ToString();
        }
    }
}
