using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OnlineAbit2013.Models;
using OfficeOpenXml;
using System.IO;

namespace OnlineAbit2013
{
    public class XLSUtils
    {
        public static byte[] PrintAllToExcel2007(GlobalCommunicationModelApplicantList model, string sheetName, string fileName)
        {
            byte[] bt;

            System.IO.FileInfo newFile = new System.IO.FileInfo(fileName);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new System.IO.FileInfo(fileName);
            }
            using (ExcelPackage doc = new ExcelPackage(newFile))
            {
                ExcelWorksheet ws = doc.Workbook.Worksheets.Add(sheetName.Substring(0, sheetName.Length < 30 ? sheetName.Length - 1 : 30));
                List<string> ColNames = new List<string>() { "Number", "Applicant", "in Roman Letters","Email", "Is complete", "Portfolio Ru", "Portfolio De", "Portfolio Common",
                "Interview", "Ru Interview", "De Interview", "Common Interview ", "Overall"};
                foreach (string col in ColNames)
                    ws.Cells[1, ColNames.IndexOf(col)+1].Value = col;

                int i = 2;

                foreach (var dc in model.ApplicantList)
                {
                    ws.Cells[i, 1].Value = dc.Number.ToString();
                    ws.Cells[i, 2].Value = dc.FIO;
                    ws.Cells[i, 3].Value = dc.FIOEng ?? "";
                    ws.Cells[i, 4].Value = dc.Email ?? "";

                    ws.Cells[i, 5].Value = dc.isComplete? "yes" : "no";
                    ws.Cells[i, 6].Value = dc.PortfolioAssessmentRu;
                    ws.Cells[i, 7].Value = dc.PortfolioAssessmentDe;
                    ws.Cells[i, 8].Value = dc.PortfolioAssessmentCommon;
                    ws.Cells[i, 9].Value = dc.Interview ? "yes" : "no";
                    ws.Cells[i, 10].Value = dc.InterviewAssessmentRu;
                    ws.Cells[i, 11].Value = dc.PortfolioAssessmentDe;
                    ws.Cells[i, 12].Value = dc.PortfolioAssessmentCommon;
                    ws.Cells[i, 13].Value = dc.OverallResults;
                    i++;
                }
                bt = doc.GetAsByteArray();
            }
            return bt;
        }
    }
}