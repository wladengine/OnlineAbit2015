using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAbit2013.Models
{
    public class SupportDialogList
    {
        public string act { get; set; }
        public List<ShortDialog> MyDialogList { get; set; }
        public List<ShortDialog> NewDialogList { get; set; }
        public List<OperatorProfilePhoto> Photolst;
    }

    public class SupportDialog : Dialog
    {
        public bool IsNew { get; set; }
        public bool IsMine { get; set; }

        public SupportDialog():base()
        {
        } 
    }

    public class SupportOperator
    {
        public string Name { get; set; }
        public List<ProfilePhotoDictionary> Photolst;

    }
    public class OperatorProfilePhoto
    {
        public Guid UserId { get; set; }
        public string Photo { get; set; }
    }
    public class ProfilePhotoDictionary
    {
        public string Id { get; set; }
        public string Img { get; set; }
        public bool Selected { get; set; }
    }
    
}