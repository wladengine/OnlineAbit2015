using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAbit2013.Models
{
    public class GlobalCommunicationModelApplicantList
    {
        public List<GlobalCommunicationApplicantShort> ApplicantList;
    }
    public class GlobalCommunicationApplicantShort
    {
        public string Number;

        public string Surname;
        public string Name;
        public string SecondName;

        public bool isComplete;

        public int PortfolioAssessmentRu;
        public int PortfolioAssessmentDe;
        public int PortfolioAssessmentCommon;

        public int InterviewAssessmentRu;
        public int InterviewAssessmentDe;
        public int InterviewAssessmentCommon;

        public int OverallResults;
        public string Status;
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

        public string HasFee;
        public string HasNoFee;

        public string CertificateDetails;

        public string Status;
        public string RuPortfolioPts;
        public string DePortfolioPts;

        public string RuInterview;
        public string DeInterview;

        public string RuOverallPts;
        public string DeOverallPts;

        public string RuInvitation;
        public string DeInvitation;
    }
}