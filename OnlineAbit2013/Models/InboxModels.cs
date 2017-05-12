using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAbit2013.Models
{
    public class InboxDialogList
    {
        public List<ShortDialog> DialogList { get; set; }
        public List<OperatorProfilePhoto> Photolst;
    }
    public class ShortDialog
    {
        public Guid Id { get; set; }
        public string Theme { get; set; }
        public bool isRead { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public bool HasAnswer;
        public DialogMessage  LastMes {get;set;}
    }
    public class NewDialog
    {
        public string Theme { get; set; }
        public string Error;
        public string Text { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }
    }

    public class Dialog
    {
        public string DialogId { get; set; }
        public string Theme { get; set; }
        public PartialModelNewMessage PartialNewMessage { get; set; }
        public PartialModelDialog Partial { get; set; }
        public Dialog()
        {
            Partial = new PartialModelDialog();
            PartialNewMessage = new PartialModelNewMessage();
        } 
    }
    public class PartialModelDialog
    {
        public List<DialogMessage> Messages { get; set; }
        public List<OperatorProfilePhoto> Photolst;

        public PartialModelDialog()
        {
            Messages = new List<DialogMessage>();
            Photolst = new List<OperatorProfilePhoto>();
        }
    }
    public class PartialModelNewMessage
    {
        public string NewMessage { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }
        public PartialModelNewMessage()
        { Files = new List<HttpPostedFileBase>(); }
    }


    public class DialogMessage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool Mine { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime time { get; set; }
        public List<FileInfo> Files { get; set; }
        public bool isRead { get; set; }
        public bool HasFiles { get; set; }
        public string DateToWords(bool isShort)
        {
            string s = "";
            if (isShort)
            {
                if (time.Date == DateTime.Now.Date)
                    s = time.ToShortTimeString();
                else
                    if (time.Date == DateTime.Now.AddDays(-1).Date)
                        s = "вчера";
                    else
                        s = String.Format("{0} {1}{2}",
                            time.Day.ToString(), GetShortMonth(time.Month), (time.Year == DateTime.Now.Year ? "" : String.Format(" {0}", time.Year)));
            }
            else
            {
                if (time.Date == DateTime.Now.Date)
                    s = "сегодня";
                else
                    if (time.Date == DateTime.Now.AddDays(-1).Date)
                        s = "вчера";
                    else
                        s = String.Format("{0} {1}{2}",
                            time.Day.ToString(), GetFullMonth(time.Month), (time.Year == DateTime.Now.Year ? "" : String.Format(" {0}", time.Year)));
            }
            return s;
        }
        public string GetShortMonth(int i)
        {
            switch (i)
            {
                case 1: { return "янв"; }
                case 2: { return "фев"; }
                case 3: { return "мар"; }
                case 4: { return "апр"; }
                case 5: { return "мая"; }
                case 6: { return "июн"; }
                case 7: { return "июл"; }
                case 8: { return "авг"; }
                case 9: { return "сен"; }
                case 10: { return "окт"; }
                case 11: { return "ноя"; }
                case 12: { return "дек"; }
                default : { return ""; }

            }
        }
        public string GetFullMonth(int i)
        {
            switch (i)
            {
                case 1: { return "января"; }
                case 2: { return "февраля"; }
                case 3: { return "марта"; }
                case 4: { return "апреля"; }
                case 5: { return "мая"; }
                case 6: { return "июня"; }
                case 7: { return "июля"; }
                case 8: { return "августа"; }
                case 9: { return "сентября"; }
                case 10: { return "октября"; }
                case 11: { return "ноября"; }
                case 12: { return "декабря"; }
                default: { return ""; }

            }
        }
    }

   
    
}