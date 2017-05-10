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

    public class SupportDialog
    {
        public string DialogId { get; set; }
        public string Theme { get; set; }
        public bool IsNew { get; set; }
        public bool IsMine { get; set; }
        public List<DialogMessage> Messages { get; set; }
        public string NewMessage { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }
        public List<OperatorProfilePhoto> Photolst;
    }

    public class SupportOperator
    {
        public string Name { get; set; }
        public List<ProfilePhotoDictionary> Photolst;

    }
    public class OperatorProfilePhoto
    {
        public Guid UserId { get; set; }
        public string bPhoto { get; set; }
        public string imgPhoto { get; set; }
    }
    public class ProfilePhotoDictionary
    {
        public string Id { get; set; }
        public string Img { get; set; }
        public bool Selected { get; set; }
    }
    
}