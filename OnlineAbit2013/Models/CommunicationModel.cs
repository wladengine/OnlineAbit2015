using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace OnlineAbit2013.Models
{
    public class GlobalCommunicationModelApplicantList
    {
        public List<GlobalCommunicationApplicantShort> ApplicantList;
        public string SortOrder;
        
    }
    public class GlobalCommunicationApplicantShort
    {
        public int Number; //1

        public string FIO; //2
        //public string Surname;
        //public string Name;//3
        //public string SecondName ;//4

        public bool isComplete ;//3
        
        public string PortfolioAssessmentRu; //4
        public string PortfolioAssessmentDe;//5
        public string PortfolioAssessmentCommon ;//6

        public bool Interview ;//7
        public string InterviewAssessmentRu;//8
        public string InterviewAssessmentDe ;//9
        public string InterviewAssessmentCommon;//10

        public string OverallResults;//11
        public string Status; //12
    }
    public class GlobalCommunicationApplicant
    {
        public string Number;

        public bool Sex;
        public string Surname;
        public string Name;
        public string SecondName;

        public string Photo;

        public string Nationality;
        public string DateOfBirth;
        public string PlaceOfBirth;
        public string CountryOfBirth;
        public string Email;
        public string PosstalAddress;

        public string PassportValid;
        public string VisaApplicationPlace;

        public bool HasFee;
        public bool HasNoFee;

        public string CertificateDetails;

        public bool IsComplete;
        public string Status;

        public string RuPortfolioPts;
        public string DePortfolioPts;
        public string CommonPortfolioPts;

        public bool Interview;

        public string RuInterviewPts;
        public string DeInterviewPts;
        public string CommonInterviewPts;

        public string OverallPts; 

        public List<CommunicationFile> lstFiles;

        public int StatusId;
        public List<SelectListItem> StatusList;

        public bool isRussian;
        public bool isGermany;

        public List<CommunicateCertificateInfo> Certificates;
    }
    public class CommunicationFile
    {
        public Guid Id;
        public string FileName;
        public bool IsPersonFile;
        public string Comment;
        public CommunicationFileType type;
    }

    public enum CommunicationFileType
    {
        PassportScan,
        Diploma,
        EnglishCertificates,
        MotivationLetter,
        CV,
        Portfolio,
        Photo,
        etc
    }

    public class CommunicationStat
    {
        public Dictionary<string, string> columns;
    }

    public class CommunicateCertificateInfo
    {
        public string TypeName;
        public bool BoolType;
        public string Result;
    }
}