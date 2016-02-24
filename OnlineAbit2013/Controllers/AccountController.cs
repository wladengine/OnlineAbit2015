﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using OnlineAbit2013.Models;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Data;
using Recaptcha;

namespace OnlineAbit2013.Controllers
{
    public class AccountController : Controller
    {

        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            if (Request.Url.AbsoluteUri.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == -1 && Util.bUseRedirection &&
                Request.Url.AbsoluteUri.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) == -1)
                return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));

            Guid UserId;

            if (Util.CheckAuthCookies(Request.Cookies, out UserId))
            {
                DataTable tbl = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                new SortedList<string, object>() { { "@PersonId", UserId }, { "@GroupId", Util.GlobalCommunicationGroupId } });
                if (tbl.Rows.Count > 0)
                    return RedirectToAction("Index", "Communication");

                return RedirectToAction("Main", "Abiturient");
            }

            Util.SetThreadCultureByCookies(Request.Cookies);
            if (Request.Cookies["sid"] == null || string.IsNullOrEmpty(Request.Cookies["sid"].Value))
            {
                FormsAuthentication.SignOut();
                if (Response.Cookies["sid"] == null)
                {
                    Response.Cookies.Add(new HttpCookie("sid") { Expires = DateTime.Now.AddYears(-20), HttpOnly = true, Value = "", Path = "/" });
                }
                else
                {
                    Response.Cookies["sid"].Value = "";
                    Response.Cookies["sid"].HttpOnly = true;
                    Response.Cookies["sid"].Path = "/";
                    Response.Cookies["sid"].Expires = DateTime.Now.AddYears(-20);
                }
            }

            return View();
        }
        public ActionResult LogOnFor()
        {
            if (Request.Cookies["sid"] == null || string.IsNullOrEmpty(Request.Cookies["sid"].Value))
            {
                FormsAuthentication.SignOut();
                if (Response.Cookies["sid"] == null)
                {
                    Response.Cookies.Add(new HttpCookie("sid") { Expires = DateTime.Now.AddYears(-20), HttpOnly = true, Value = "", Path = "/" });
                }
                else
                {
                    Response.Cookies["sid"].Value = "";
                    Response.Cookies["sid"].HttpOnly = true;
                    Response.Cookies["sid"].Path = "/";
                    Response.Cookies["sid"].Expires = DateTime.Now.AddYears(-20);
                }
            }

            return View("LogOnForeign");
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            if (ModelState.IsValid)
            {
                DateTime usrTime = DateTime.Now;
                try
                {
                    usrTime = Convert.ToDateTime(Request.Form["time"], System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                }
                catch 
                {
                    //при плохо отправленном или не распарсенном времени добавлять 24 часа
                    usrTime = DateTime.Now.AddHours(24);
                }
                string email = model.Email;
                string remixPwd = Util.MD5Str(model.Password);

                string query = "SELECT Id, SID, IsApproved, Ticket, IsForeign, IsDormsAccount FROM [User] LEFT JOIN AuthTicket ON AuthTicket.UserId=[User].Id WHERE Password=@Password AND Email=@Email";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Password", remixPwd);
                dic.Add("@Email", email);
                DataTable tbl = Util.AbitDB.GetDataTable(query, dic);

                var Usr = (from DataRow rw in tbl.Rows
                           select new
                           {
                               Id = rw.Field<Guid>("Id"),
                               SID = rw.Field<string>("SID"),
                               IsApproved = rw.Field<bool>("IsApproved"),
                               Ticket = rw.Field<string>("Ticket"),
                               IsForeign = rw.Field<bool?>("IsForeign"),
                               IsDormsAccount = rw.Field<bool?>("IsDormsAccount")
                           }).FirstOrDefault();
                if (Usr != null)
                {
                    if (!string.IsNullOrEmpty(Usr.Ticket))
                    {
                        dic.Clear();
                        dic.Add("@Ticket", Util.MD5Str(remixPwd + DateTime.Now.ToString()));
                        dic.Add("@UserId", Usr.Id);
                        query = "UPDATE AuthTicket SET Ticket=@Ticket WHERE UserId=@UserId";
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }
                    else
                    {
                        dic.Clear();
                        dic.Add("@Ticket", Util.MD5Str(remixPwd + DateTime.Now.ToString()));
                        dic.Add("@UserId", Usr.Id);
                        query = "INSERT INTO AuthTicket (Ticket, UserId) VALUES (@Ticket, @UserId)";
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }

                    string sid = Usr.SID;
                    if (!Usr.IsApproved)
                    {
                        ModelState.AddModelError("", Resources.LogOn.ValidationSummaryNotApproved);
                        ModelState.AddModelError("Email", Resources.LogOn.NotApprovedError);
                        return View();
                    }

                    Response.Cookies.SetAuthCookies(Usr.Id, usrTime, model.RememberMe);
                    FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
                    bool dorms = Usr.IsDormsAccount.HasValue ? Usr.IsDormsAccount.Value : false;
                    if (!dorms)
                    {
                        DataTable tbl_comm = Util.AbitDB.GetDataTable("SELECT * FROM GroupUsers WHERE PersonId=@PersonId and GroupId=@GroupId",
                        new SortedList<string, object>() { { "@PersonId", Usr.Id }, { "@GroupId", Util.GlobalCommunicationGroupId } });
                        if (tbl.Rows.Count > 0)
                            return RedirectToAction("Index", "Communication");

                        return RedirectToAction("Main", "Abiturient");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Dorms");
                    }
                }
                else
                {
                    ModelState.AddModelError("", Resources.LogOn.ValidationSummaryWrongUsernamePassword);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        public ActionResult LogOnFor(LogOnForeignModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                DateTime usrTime = Convert.ToDateTime(Request.Form["time"], System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                string userName = model.UserName;
                string remixPwd = Util.MD5Str(model.Password);

                string query = "SELECT Id, SID, IsApproved, Ticket, IsForeign FROM [User] LEFT JOIN AuthTicket ON AuthTicket.UserId=[User].Id WHERE Password=@Password AND UserName=@UserName";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.Add("@Password", remixPwd);
                dic.Add("@UserName", userName);
                DataTable tbl = Util.AbitDB.GetDataTable(query, dic);

                var Usr = (from DataRow rw in tbl.Rows
                           select new
                           {
                               Id = rw.Field<Guid>("Id"),
                               SID = rw.Field<string>("SID"),
                               IsApproved = rw.Field<bool>("IsApproved"),
                               Ticket = rw.Field<string>("Ticket"),
                               IsForeign = rw.Field<bool?>("IsForeign")
                           }).FirstOrDefault();
                if (Usr != null)
                {
                    if (!string.IsNullOrEmpty(Usr.Ticket))
                    {
                        dic.Clear();
                        dic.Add("@Ticket", Util.MD5Str(remixPwd + DateTime.Now.ToString()));
                        dic.Add("@UserId", Usr.Id);
                        query = "UPDATE AuthTicket SET Ticket=@Ticket WHERE UserId=@UserId";
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }
                    else
                    {
                        dic.Clear();
                        dic.Add("@Ticket", Util.MD5Str(remixPwd + DateTime.Now.ToString()));
                        dic.Add("@UserId", Usr.Id);
                        query = "INSERT INTO AuthTicket (UserId, Ticket) VALUES (@Ticket, @UserId)";
                        Util.AbitDB.ExecuteQuery(query, dic);
                    }

                    string sid = Usr.SID;
                    if (!Usr.IsApproved)
                    {
                        ModelState.AddModelError("", "This login is not approved yet. Please, check your e-mail, specified at registration");
                        ModelState.AddModelError("UserName", "This login is not approved yet.");
                        return View();
                    }

                    Response.Cookies.SetAuthCookies(Usr.Id, usrTime, model.RememberMe);
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Usr.IsForeign.HasValue && Usr.IsForeign.Value == true)
                        return RedirectToAction("Index", "ForeignAbiturient");
                    else
                        return RedirectToAction("Index", "Abiturient");
                }
                else
                {
                    ModelState.AddModelError("", "Wrong username or password");
                }
            }

            // If we got this far, something failed, redisplay form
            return View("LogOnForeign", model);
        }

        //
        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            string sid;
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return RedirectToAction("LogOn");
            else
                sid = Request.Cookies["sid"].Value;

            FormsAuthentication.SignOut();
            if (Response.Cookies["sid"] != null)
            {
                Response.Cookies["sid"].Value = "";
                Response.Cookies["sid"].Path = "/";
                Response.Cookies["sid"].Expires = DateTime.Now.AddYears(-12);
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("sid") { Value = "", Path = "/", Expires = DateTime.Now.AddYears(-12) });
            }

            if (Response.Cookies["t"] != null)
            {
                Response.Cookies["t"].Value = "";
                Response.Cookies["t"].Path = "/";
                Response.Cookies["t"].Expires = DateTime.Now.AddYears(-12);
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("t") { Value = "", Path = "/", Expires = DateTime.Now.AddYears(-12) });
            }

            //Изменяем ключ безопасности на какой-нибудь рандомный
            string newTicket = Guid.NewGuid().ToString("N");
            Util.AbitDB.ExecuteQuery("UPDATE AuthTicket SET Ticket=@Ticket WHERE UserId=@UserId",
                new SortedList<string, object>() { { "@Ticket", newTicket }, { "@UserId", PersonId } });
            //var ticket = Util.ABDB.AuthTicket.Where(x => x.UserId == PersonId).FirstOrDefault();
            //if (ticket != null)
            //{
            //    ticket.Ticket = Guid.NewGuid().ToString("N");
            //    Util.ABDB.SaveChanges();
            //}
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [OutputCache(Duration = 0, NoStore = false)]
        public ActionResult Register()
        {
            if (Request.Url.AbsoluteUri.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == -1 && Util.bUseRedirection &&
                Request.Url.AbsoluteUri.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) == -1)
                return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));

            Util.SetThreadCultureByCookies(Request.Cookies);
            return View();
        }
        public ActionResult RegisterFor()
        {
            return View("RegisterForeign");
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            string errMsg = "";

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "");
                return View(model);
            }

            bool res = Util.CheckCaptcha(Request, out errMsg);
            if (!res)
            {
                ModelState.AddModelError("", errMsg);
                return View(model);
            }

            string password = model.Password ?? "";
            string email = model.Email ?? "";

            List<string> errlist;
            if (!Util.CheckRegistrationInfo(password, email, out errlist))
            {
                foreach (string er in errlist)
                    ModelState.AddModelError("", er);
                return View(model);
            }
            Guid UserId;
            try
            {
                UserId = Util.CreateNewUser(model.Password, model.Email);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(model);
            }
            //string ticket = Util.ABDB.User.Where(x => x.Id == id).Select(x => x.EmailTicket).FirstOrDefault();
            string query = "SELECT EmailTicket FROM [User] WHERE Id=@Id";
            string ticket = Util.AbitDB.GetStringValue(query, new SortedList<string, object>() { { "@Id", UserId } });
            if (string.IsNullOrEmpty(ticket))
            {
                ModelState.AddModelError("", "Ошибка при сохранении пользователя. Попробуйте зарегистрироваться ещё раз");
                return View(model);
            }
            EmailConfirmationModel mdl = new EmailConfirmationModel()
            {
                RegStatus = EmailConfirmationStatus.FirstEmailSent,
                Email = email,
                Link = Util.ServerAddress + Url.Action("EmailConfirmation", "Account", new RouteValueDictionary() { { "email", email }, { "t", ticket } })
            };

            try
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(email);
                msg.Body =
                    string.Format
                    (
                        Util.GetMailBody
                        (
                            Server.MapPath
                            (
                            string.Format("~/Templates/EmailBodyConfirm{0}.eml",
                            System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ? "" : "Foreign")
                            )
                        ), mdl.Link, email, model.Password
                    );
                msg.Subject = "Регистрация на сайте СПбГУ";
                SmtpClient client = new SmtpClient();
                Util.SendMail(client, msg);
            }
            catch (Exception e)
            {
                Util.LogError(e.Message + ";" + (e.InnerException != null ? e.InnerException.Message : ""));
            }

            return View("EmailConfirmation", mdl);
        }
        [HttpPost]
        public ActionResult RegisterFor(RegisterModel model)
        {
            string errMsg = "";

            bool res = Util.CheckCaptcha(Request, out errMsg);
            if (!res)
            {
                ModelState.AddModelError("", errMsg);
                return View(model);
            }

            string password = model.Password;
            string email = model.Email;
            List<string> errlist;
            if (!Util.CheckRegistrationInfo(password, email, out errlist))
            {
                foreach (string er in errlist)
                    ModelState.AddModelError("", er);
                return View(model);
            }
            Guid UserId;
            try
            {
                UserId = Util.CreateNewUser(model.Password, model.Email);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(model);
            }
            //string ticket = Util.ABDB.User.Where(x => x.Id == id).Select(x => x.EmailTicket).FirstOrDefault();
            string query = "SELECT EmailTicket FROM [User] WHERE Id=@Id";
            string ticket = Util.AbitDB.GetStringValue(query, new SortedList<string, object>() { { "@Id", UserId } });
            if (string.IsNullOrEmpty(ticket))
            {
                ModelState.AddModelError("", "Error while saving an account. Please, try again");
                return View(model);
            }
            EmailConfirmationModel mdl = new EmailConfirmationModel()
            {
                RegStatus = EmailConfirmationStatus.FirstEmailSent,
                Email = email,
                Link = Util.ServerAddress + Url.Action("EmailConfirmation", "Account", new RouteValueDictionary() { { "email", email }, { "t", ticket } })
            };

            try
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(email);
                msg.Body = string.Format(Util.GetMailBody(Server.MapPath("~/Templates/EmailBodyConfirmForeign.eml")), mdl.Link, email, model.Password);
                msg.Subject = "Registration on Admission Office Site of SPbSU";
                SmtpClient client = new SmtpClient();
                Util.SendMail(client, msg);
            }
            catch { }

            return View("EmailConfirmation", mdl);
        }

        

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            string errPwdMismath = System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ?
                "Введённые новый пароль и его подтверждение не совпадают" : "The new password and confirmation do not match";

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", errPwdMismath);
            }

            string authError = System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ?
                "Ошибка при авторизации пользователя. Попробуйте <a href=\"../../Account/LogOn\">войти в систему заново</a> со старым паролем" :
                "Error in user authentication. Try to <a href=\"../../Account/LogOn\">logon</a> in system with old password";

            Guid UserId;
            if (!Util.CheckAuthCookies(Request.Cookies, out UserId))//если вдруг ВНЕЗАПНО просрочились куки или зашёл правильный пользователь
                return View("Error", new AccountErrorModel() { ErrorHtmlString = authError });

            string query = "SELECT Password, Email FROM [User] WHERE Id=@Id";
            DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Id", UserId } });
            if (tbl.Rows.Count == 0)
                return View("Error", new AccountErrorModel()
                {
                    ErrorHtmlString = authError
                });
            var User =
                (from DataRow rw in tbl.Rows
                 select new { Password = rw.Field<string>("Password"), Email = rw.Field<string>("Email") }).FirstOrDefault();
            //var User = Util.ABDB.User.Where(x => x.Id == UserId).FirstOrDefault();
            string remixPwdOld = User.Password;
            string oldPwdError = System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ?
                "Введён неправильный предыдущий пароль" : "Wrong old password";
            if (Util.MD5Str(model.OldPassword) != remixPwdOld)
            {
                ModelState.AddModelError("", oldPwdError);
                return View();
            }

            string remixPwd = Util.MD5Str(model.NewPassword);
            try//пробуем сохранить новый пароль в базе
            {
                query = "UPDATE [User] SET Password=@Password WHERE Id=@Id";
                Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Password", remixPwd }, { "@Id", UserId } });
            }
            catch//не получилось сохранить
            {
                string PwdSaveError = System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ?
                "Ошибка при сохранении пароля. Попробуйте <a href=\"../../Account/LogOn\">войти в систему заново</a> со старым паролем" :
                "Failed to save your password. Try to <a href=\"../../Account/LogOn\">logon</a> again with the old password";
                ModelState.AddModelError("", PwdSaveError);
                return View();
            }

            try//пробуем отправить письмо
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(User.Email);
                msg.Body = string.Format
                (
                    Util.GetMailBody(
                        Server.MapPath
                        (
                            string.Format("~/Templates/EmailBodyChangePassword{0}.eml",
                            System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ? "" : "Foreign")
                        )
                    ),
                model.NewPassword);
                msg.Subject =
                    System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ?
                    "Изменение пароля на сайте приёмной комиссии СПбГУ" : "SPbSU Admission Committee site - Changing the password";
                SmtpClient client = new SmtpClient();
                Util.SendMail(client, msg);
            }
            catch//что-то сломалось при отправке письма - нет уведомления на ящик, след-но откат до старого пароля
            {
                try//пробуем откатиться
                {
                    //User.Password = remixPwdOld;
                    //Util.ABDB.SaveChanges();
                    query = "UPDATE [User] SET Password=@Password WHERE Id=@Id";
                    Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Password", remixPwd }, { "@Id", UserId } });
                    ModelState.AddModelError("", System.Threading.Thread.CurrentThread.CurrentUICulture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ?
                        "Не удалось отправить письмо на указанный при регистрации e-mail. Возвращён старый пароль" :
                        "Failed to send an email to address, specified during the registration . Old password saved");
                    return View();
                }
                catch { }//не удалось откатиться - ну, пусть хоть сохранится новый. Если забыл, то пусть пользуется напоминалкой пароля
            }
            return View("ChangePasswordSuccess");
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            return View();
        }

        public ActionResult EmailConfirmation(string email, string t)
        {
            Util.SetThreadCultureByCookies(Request.Cookies);
            string query = "SELECT Id, IsApproved FROM [User] WHERE Email=@Email AND EmailTicket=@EmailTicket";
            DataTable tbl = Util.AbitDB.GetDataTable(query,
                new SortedList<string, object>() { { "@Email", email }, { "@EmailTicket", t } });
            var usr = (from DataRow rw in tbl.Rows
                       select new { Id = rw.Field<Guid>("Id"), IsApproved = rw.Field<bool?>("IsApproved") }).FirstOrDefault();
            if (usr != null)
            {
                //User user = usr.First();
                //user.IsApproved = true;
                //Util.ABDB.SaveChanges();
                query = "UPDATE [User] SET IsApproved=@IsApproved WHERE Id=@Id";
                Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@IsApproved", true }, { "@Id", usr.Id } });
                return View(new EmailConfirmationModel()
                {
                    RegStatus = EmailConfirmationStatus.Confirmed
                });
            }
            else
            {
                return View(new EmailConfirmationModel()
                {
                    RegStatus = EmailConfirmationStatus.WrongTicket
                });
            }
        }

        public ActionResult Captcha()
        {
            return View();
        }

        public ActionResult PostCaptcha()
        {
            string msg = "";
            bool res = Util.CheckCaptcha(Request, out msg);
            if (!res)
                ModelState.AddModelError("", msg);
            else
                ModelState.AddModelError("", "OK");

            return View("Captcha");
        }

        public JsonResult ShangeLanguage(string lang)
        {
            Guid PersonId;
            if (!Util.CheckAuthCookies(Request.Cookies, out PersonId))
                return Json(new { IsOk = false });

            if (lang.IndexOf("ru", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (Util.SetUILang(PersonId, "ru"))
                    return Json(new { IsOk = true });
                else
                    return Json(new { IsOk = false });
            }
            if (lang.IndexOf("en", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (Util.SetUILang(PersonId, "ru"))
                    return Json(new { IsOk = true });
                else
                    return Json(new { IsOk = false });
            }

            //if we going so far, something fails
            return Json(new { IsOk = false });
        }

        public ActionResult ForgotPassword()
        {
            Request.Cookies.SetThreadCultureByCookies();
            return View();
        }

        public JsonResult PasswordRestore(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { NoEmail = true });
            }
            else
            {
                string query = "SELECT [User].Id, Person.Surname AS P_SURNAME, Person.BirthDate AS P_BIRTH " +
                    " FROM [User] LEFT JOIN Person ON Person.Id=[User].Id " +
                    " WHERE [User].Email=@Email";
                DataTable tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@Email", email } });
                if (tbl.Rows.Count == 0)
                    return Json(new { NoEmail = true });
                else
                {
                    string Surname = tbl.Rows[0].Field<string>("P_SURNAME");
                    DateTime? BirthDate = tbl.Rows[0].Field<DateTime?>("P_BIRTH");

                    bool needToApprove = true;
                    if (string.IsNullOrEmpty(Surname) && !BirthDate.HasValue)
                        needToApprove = false;

                    if (!needToApprove)
                        return Json(new { IsOk = true });

                    return Json(new { NeedInfo = true });
                }
            }
        }

        public ActionResult RestoreByData(string email, string surname, string birthdate, string empty = "1782")
        {
            if (empty == "1782")
            {
                string query = "SELECT [User].Id FROM [User] WHERE [User].Email=@Email";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.AddItem("@Email", email);
                DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
                string newPass = System.IO.Path.GetRandomFileName();
                string remixPwd = Util.MD5Str(newPass);

                if (SendEmail(email, newPass))
                {
                    query = "UPDATE [User] SET Password=@Password WHERE Email=@Email";
                    Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Password", remixPwd }, { "@Email", email } });
                    return Json(new { IsOk = true, Email = true });
                }
                else
                {
                    return Json(new { IsOk = false, Email = false });
                }
            }
            else
            {
                DateTime BirthDate;
                if (!DateTime.TryParse(birthdate, System.Globalization.CultureInfo.GetCultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out BirthDate))
                    return Json(new { IsOk = false });
                string query = "SELECT [User].Id FROM [User] INNER JOIN Person ON Person.Id=[User].Id WHERE [User].Email=@Email AND Surname=@Surname AND BirthDate=@BirthDate";
                SortedList<string, object> dic = new SortedList<string, object>();
                dic.AddItem("@Email", email);
                dic.AddItem("@Surname", surname);
                dic.AddItem("@BirthDate", BirthDate);
                DataTable tbl = Util.AbitDB.GetDataTable(query, dic);
                
                string newPass = System.IO.Path.GetRandomFileName();
                string remixPwd = Util.MD5Str(newPass);

                if (SendEmail(email, newPass))
                {
                    query = "UPDATE [User] SET Password=@Password WHERE Email=@Email";
                    Util.AbitDB.ExecuteQuery(query, new SortedList<string, object>() { { "@Password", remixPwd }, { "@Email", email } });
                    return Json(new { IsOk = true, Email = true });
                }
                else
                {
                    return Json(new { IsOk = false, Email = false });
                }
            }
        }

        private bool SendEmail(string email, string password)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(email);
                msg.Body =
                    string.Format
                    (
                        Util.GetMailBody
                        (
                            Server.MapPath
                            (
                                string.Format("~/Templates/EmailBodyRestore{0}.eml",
                                System.Threading.Thread.CurrentThread.CurrentCulture.Name == System.Globalization.CultureInfo.GetCultureInfo("ru-RU").Name ? "" : "Foreign")
                            )
                        ), password
                    );
                msg.Subject = System.Threading.Thread.CurrentThread.CurrentCulture.Name == System.Globalization.CultureInfo.GetCultureInfo("ru-RU").Name ?
                    "Личный кабинет поступающего в СПбГУ - смена пароля" : "Applicant Personal Account - password change";
                SmtpClient client = new SmtpClient();
                Util.SendMail(client, msg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ActionResult Data()
        {
            string query = "SELECT Id, Name FROM Region WHERE RegionNumber IS NOT NULL";
            DataTable tbl = Util.AbitDB.GetDataTable(query, null);
            DataRegModel model = new DataRegModel();
            model.listRegion = (from DataRow rw in tbl.Rows
                                select new SelectListItem()
                                {
                                    Value = rw.Field<int>("Id").ToString(),
                                    Text = rw.Field<string>("Name")
                                }).ToList();
            return View(model);
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
