using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;

namespace MemberLite_EF6.Models
{
    public partial class Users
    {
        public static string ReturnMessage;


        public enum SignInTypes
        {
            All = 0,
            Direct = 1,
            Facebook = 2,
            Twitter = 3,
            Google = 4
        }

        public SignInTypes UserSignInType
        {
            get { return (SignInTypes)this.SignInType; }
            set
            {
                this.SignInType = (int)this.UserSignInType;
            }
        }

        public enum StatusType
        {
            All = 0,
            Active = 1,
            Locked = 2,
            Banned = 3
        }
        public StatusType UserStatus
        {
            get { return (StatusType)this.Status; }
            set
            {
                this.Status = (int)this.UserStatus;
            }
        }

        public string Fullname { get { return string.Join(" ", this.OtherNames, this.FirstName); } }


        public static bool EmailIsAvailabile(string Email)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                var email = db.Users.Select(a => new { a.Email })
                                 .Where(a => a.Email == Email)
                                 .FirstOrDefault();
                if (email == null)
                {
                    ReturnMessage = "This email is available";
                    return true;
                }

                ReturnMessage = "This email is unavailable";
                return false;
            }
        }

        public static bool NameIsValid(string Name)
        {
            string _file = Path.Combine(AppUtility.AppDataPath, "StaticContents/NameRestriction.txt");
            string names = File.ReadAllText(_file).ToLower()
                            .Replace(Environment.NewLine, "")
                            .Replace(", ", ",").Replace(",  ", ",").Replace(",   ", ",")
                            .Replace(" ,", ",").Replace("  ,", ",").Replace("   ,", ",");

            string[] restricted = AppUtility.StripHtml(names).Split(',');
            if (restricted.Contains(Name.Trim().ToLower()))
            {
                return false;
            }

            return true;
        }

        public bool Create()
        {
            //First line of defence
            if (this.Password == "" || this.Password.Length < 5)
            {
                ReturnMessage = "Password format is incorrect";
                return false;
            }

            if (!NameIsValid(this.FirstName))
            {
                ReturnMessage = "Your name is not valid";
                return false;
            }

            if (!NameIsValid(this.OtherNames))
            {
                ReturnMessage = "Your name is not valid";
                return false;
            }
            //=============================================

            try
            {

                using (var db = new MemberLiteEntities().Init)
                {
                    //Validate email
                    var uEmail = db.Users.Select(a => new { a.Email })
                                     .Where(a => a.Email == this.Email)
                                     .FirstOrDefault();
                    if (uEmail != null)
                    {
                        ReturnMessage = "Sorry! This email is already in use";
                        return false;
                    }

                    //Generate UserID
                    //USR-{MONTH YEAR JOINED}-{RANDOM}
                    string _userID = "";
                    createUserID:
                    _userID = string.Join("-", "USR", DateTime.Now.ToString("MM") + DateTime.Now.ToString("yy"), new Random().Next(10, 9000000));

                    //Check if generated id exist in DB
                    var uID = db.Users.Select(a => new { a.UserID })
                                        .Where(a => a.UserID == _userID)
                                        .FirstOrDefault();

                    //You can generate a simple GUID
                    //Using string _id = Guid.NewGuid().ToString();

                    if (uID != null)
                    {
                        goto createUserID;
                    }
                    else
                    {
                        this.UserID = _userID;
                    }

                    //Encrypt passkey
                    string userIDHash = Crypto.SHA256Hash(this.UserID);
                    string pwd = Crypto.SHA256Hash(this.Password.ToUpper());
                    string finalPwd = Crypto.SHA256Hash(userIDHash + pwd);

                    this.VerificationCode = AppUtility.GenerateAlphaNumeric(15);
                    this.Password = finalPwd;
                    this.Status = (int)StatusType.Active;
                    this.EmailConfirmed = false;
                    this.DateStamp = DateTime.Now;

                    db.Users.Add(this);
                    db.SaveChanges();

                    ReturnMessage = "Account created ok";
                    return true;
                }
            }
            catch (DbEntityValidationException ex)
            {
                CustomErrorLogger.Log(DBHelper.HandleEFException(ex));

                //Users should not see your exception message
                ReturnMessage = "An error occurred while processing your details. Please try again!";
                return false;
            }
            catch (Exception ex)
            {
                CustomErrorLogger.Log(ex.InnerException.Message);
                ReturnMessage = "An error occurred while processing your details. Please try again!";
                return false;
            }
        }


        public bool Authenticate(string Email, string Passkey)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                var u = db.Users.Select(a => new
                {
                    a.UserID,
                    a.FirstName,
                    a.Email,
                    a.Password,
                    a.Status
                })
                .Where(a => a.Email == Email)
                .FirstOrDefault();
                if (u == null)
                {
                    ReturnMessage = "Invalid login or password! Check and try again";
                    return false;
                }

                string userIDHash = Crypto.SHA256Hash(u.UserID);
                string pwdHash = Crypto.SHA256Hash(Passkey.ToUpper());
                string finalHash = Crypto.SHA256Hash(userIDHash + pwdHash);

                if (finalHash == u.Password)
                {
                    //Check account status
                    var status = (StatusType)u.Status;
                    if (status == StatusType.Locked)
                    {
                        if (LockoutReleaseDate.HasValue)
                        {
                            //perform lock action
                        }

                        ReturnMessage = "Your account is locked!";
                        return false;
                    }
                    else if (status == StatusType.Banned)
                    {
                        ReturnMessage = "You have been banned!";
                        return false;
                    }

                    this.UserID = u.UserID;

                    //Log login history
                    db.LoginHistory.Add(new LoginHistory
                    {
                        UserID = u.UserID,
                        IP = AppUtility.GetUserIPAddress(),
                        DeviceType = AppUtility.GetDeviceType(),
                        DateStamp = DateTime.Now,
                        UserAgent = HttpContext.Current.Request.Browser.Browser
                    });
                    db.SaveChanges();

                    ReturnMessage = "Login ok!";
                    return true;
                }
                else
                {
                    ReturnMessage = "Invalid login or password! Check and try again.";
                    return false;
                }
            }
        }

        public static bool ChangePassword(string UserID, string OldPassword, string NewPassword)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                var u = db.Users.Find(UserID);

                string oldPwdHash = u.Password;
                string userIDHash = Crypto.SHA256Hash(UserID);
                string passwordHash = Crypto.SHA256Hash(OldPassword.ToUpper());

                if (Crypto.SHA256Hash(userIDHash + passwordHash) == oldPwdHash)
                {
                    string _password = Crypto.SHA256Hash(NewPassword.ToUpper());
                    passwordHash = Crypto.SHA256Hash(userIDHash + _password);

                    db.Users.Find(UserID).Password = passwordHash;
                    db.SaveChanges();

                    ReturnMessage = "New Password has been set successfully";
                    return true;
                }
                else
                {
                    ReturnMessage = "Old password provided is incorrect";
                    return false;
                }
            }
        }


        public bool UpdateProfileBasic()
        {
            if (!NameIsValid(this.FirstName))
            {
                ReturnMessage = "Your name is not valid";
                return false;
            }

            if (!NameIsValid(this.OtherNames))
            {
                ReturnMessage = "Your name is not valid";
                return false;
            }

            if (!AppUtility.ValidateEmail(this.Email))
            {
                ReturnMessage = "Your email address format is not valid";
                return false;
            }


            using (var db = new MemberLiteEntities().Init)
            {
                //Validate email
                var uEmail = db.Users.Select(a => new { a.UserID, a.Email })
                                 .Where(a => a.Email == this.Email && a.UserID != this.UserID)
                                 .FirstOrDefault();
                if (uEmail != null)
                {
                    ReturnMessage = "Sorry! This email is already in use";
                    return false;
                }

                //Validate phone number
                var uPhone = db.Users.Select(a => new { a.UserID, a.Phone })
                             .Where(a => a.Phone == this.Phone && a.UserID != this.UserID)
                             .FirstOrDefault();

                if (uPhone != null)
                {
                    if (!string.IsNullOrEmpty(uPhone.Phone.ToString()))
                    {
                        ReturnMessage = "Sorry! This phone number is already in use";
                        return false;
                    }
                }

                var u = db.Users.Where(a => a.UserID == this.UserID).FirstOrDefault();

                u.FirstName = this.FirstName;
                u.OtherNames = this.OtherNames;
                u.Gender = this.Gender;
                u.DOB = this.DOB;
                u.Email = this.Email;
                u.Phone = this.Phone;
                u.LastUpdated = DateTime.Now;

                //Detect email address change and send verificaton
                //SendVerificationEmail(this.UserID);

                db.SaveChanges();

                ReturnMessage = "Your profile has been updated";
                return true;
            }
        }

        public static bool SendVerificationEmail(string UserID)
        {
            try
            {
                using (var db = new MemberLiteEntities().Init)
                {
                    var u = db.Users.Select(a => new { a.UserID, a.FirstName, a.OtherNames, a.Email, a.VerificationCode })
                                  .Where(a => a.UserID == UserID)
                                  .FirstOrDefault();

                    if (u == null)
                    {
                        ReturnMessage = "Invalid user";
                        return false;
                    }

                    string link = new Uri(string.Format(AppConfig.Url + "access/verifyemail?e={0}&c={1}", u.Email, u.VerificationCode)).AbsoluteUri;
                    string body = File.ReadAllText(AppUtility.AppDataPath + "/MailTemplates/EmailVerification.htm");

                    body = body.Replace("{site_name}", AppConfig.Name);
                    body = body.Replace("{site_url}", AppConfig.Url);
                    body = body.Replace("{name}", u.OtherNames + " " + u.FirstName);
                    body = body.Replace("{verify_link}", link);
                    body = body.Replace("{email}", u.Email);
                    body = body.Replace("{support_mail}", WebMailer.Support);

                    if (WebMailer.Send(WebMailer.Alert, u.Email, AppConfig.Name + " - Email Verification", body, true))
                    {
                        ReturnMessage = "Verification message has been sent, please goto your inbox and confirm it now.";
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                CustomErrorLogger.Log(ex.Message);
            }

            ReturnMessage = "Unable to send verification mail";
            return false;
        }

        public static bool VerifyEmail(string Email, string Code)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Code))
            {
                ReturnMessage = "Verification session has expired.";
                return false;
            }

            using (var db = new MemberLiteEntities().Init)
            {
                var u = db.Users.Where(a => a.Email == Email && a.VerificationCode == Code).FirstOrDefault();

                if (u != null)
                {
                    u.EmailConfirmed = true;
                    db.SaveChanges();
                    ReturnMessage = "Email verification successful";
                    return true;
                }
            }
            ReturnMessage = "Verification failed! Please try again or resend a new link.";
            return false;
        }

        public static bool ResetPassword(string Login)
        {
            string userID = "", email = "", newPwd, fname = "";

            using (var db = new MemberLiteEntities().Init)
            {
                long __phone = 0, val = 0;
                if (Int64.TryParse(Login, out val))
                {
                    __phone = Convert.ToInt64(Login);
                }

                if (Login.Contains("@"))
                {
                    if (AppUtility.ValidateEmail(Login))
                    {
                        var u = db.Users.Select(a => new { a.UserID, a.Email, a.FirstName, a.OtherNames })
                                            .Where(a => a.Email == Login).FirstOrDefault();
                        if (u != null)
                        {
                            userID = u.UserID;
                            fname = u.OtherNames + " " + u.FirstName;
                            email = Login;
                        }
                        goto notfound;
                    }
                    else
                    {
                        ReturnMessage = "Email address format is incorrect!";
                        return false;
                    }
                }
                else if (__phone != 0)
                {
                    var u = db.Users.Select(a => new { a.UserID, a.Email, a.Phone, a.FirstName, a.OtherNames })
                                        .Where(a => a.Phone == __phone).FirstOrDefault();
                    if (u != null)
                    {
                        userID = u.UserID;
                        fname = u.FirstName;
                        email = Login;
                    }
                    goto notfound;
                }
                else
                {
                    ReturnMessage = "Provide your login email or phone number!";
                    return false;
                }

                notfound:
                if (userID == "")
                {
                    ReturnMessage = "User not found! Please try again.";
                    return false;
                }

                newPwd = AppUtility.GenerateAlphaNumeric(10);

                string userIDHash = Crypto.SHA256Hash(userID);
                string pwd = Crypto.SHA256Hash(newPwd.ToUpper());
                string finalPwd = Crypto.SHA256Hash(userIDHash + pwd);

                db.Users.Find(userID).Password = finalPwd;
                db.SaveChanges();
                ReturnMessage = "Password reset ok but could not send email. Pls try again!";
            }

            string msg = File.ReadAllText(AppUtility.AppDataPath + "MailTemplates/PasswordReset.htm");
            msg = msg.Replace("{site_name}", AppConfig.Name);
            msg = msg.Replace("{fullname}", fname);
            msg = msg.Replace("{new_pwd}", newPwd);
            msg = msg.Replace("{site_url}", AppConfig.Url);
            msg = msg.Replace("{support_mail}", WebMailer.Support);

            if (WebMailer.Send(WebMailer.Alert, email, "Password Reset", msg, true))
            {
                ReturnMessage = "Password reset complete! Check your email for a new password.";
            }
            return true;
        }

        public static string ForceResetPassword(string UserID)
        {
            //force a password reset for admin
            //this is rarely used
            string newPwd;

            if (!string.IsNullOrEmpty(UserID))
            {
                newPwd = AppUtility.GenerateAlphaNumeric(10);
                string userIDHash = Crypto.SHA256Hash(UserID);
                string pwdHash = Crypto.SHA256Hash(newPwd.ToUpper());
                pwdHash = Crypto.SHA256Hash(userIDHash + pwdHash);

                using (var db = new MemberLiteEntities().Init)
                {
                    db.Users.Find(UserID).Password = pwdHash;
                    db.SaveChanges();
                }
                ReturnMessage = "Password reset ok";
                return newPwd;
            }

            return string.Empty;

        }

        public static bool UpdateStatus(string UserID, int Status)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                var u = db.Users.Where(a => a.UserID == UserID).FirstOrDefault();
                if (u != null)
                {
                    u.Status = Status;
                    if (Status == (int)Users.StatusType.Locked)
                        u.LockoutReleaseDate = DateTime.Now;
                    db.SaveChanges();

                    ReturnMessage = "User status updated ok";
                    return true;
                }
            }
            ReturnMessage = "Unable to update user status";
            return false;
        }

        public static bool HasAvatar(string UserID)
        {
            string avatar = HttpContext.Current.Server.MapPath(Path.Combine(AppConfig.AvatarDirectory, UserID + ".jpg"));
            if (File.Exists(avatar))
            {
                return true;
            }

            return false;
        }

        public static StatusType GetStatus(string UserID)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                var u = db.Users.Select(a => new { a.UserID, a.Status })
                                                .Where(a => a.UserID == UserID)
                                                .FirstOrDefault();
                if (u != null)
                {
                    return (StatusType)u.Status;
                }
            }

            return StatusType.All;
        }

        public static bool IsActive(string UserID)
        {
            if (GetStatus(UserID) == StatusType.Active)
            {
                return true;
            }
            return false;
        }

        public static string GetAvatar(string UserID)
        {
            if (!string.IsNullOrEmpty(UserID))
            {
                string avatar = HttpContext.Current.Server.MapPath(Path.Combine(AppConfig.AvatarDirectory, UserID + ".jpg"));
                if (File.Exists(avatar)) { return VirtualPathUtility.ToAbsolute(Path.Combine(AppConfig.AvatarDirectory, UserID + ".jpg")); }
            }

            return VirtualPathUtility.ToAbsolute(AppConfig.AvatarDirectory + @"\default.png");
        }

        public class Name
        {
            public string FirstName { get; set; }
            public string OtherNames { get; set; }
            public string Fullname { get; set; }
            public string LastName { get; set; }

            public Name(string UserID)
            {
                using (var db = new MemberLiteEntities().Init)
                {
                    var u = db.Users.Select(a => new { a.UserID, a.FirstName, a.OtherNames })
                                        .Where(a => a.UserID == UserID)
                                        .FirstOrDefault();
                    if (u != null)
                    {
                        this.FirstName = u.FirstName;
                        this.OtherNames = u.OtherNames;
                        this.Fullname = string.Join(" ", u.FirstName, u.OtherNames);

                        //pick last name if othernames contains more than one
                        this.LastName = this.OtherNames.Split(' ').Last();
                    }
                }
            }

        }

        public static string GetUserIDByEmail(string Email)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                var u = db.Users.Select(a => new { a.UserID, a.Email })
                            .Where(a => a.Email == Email)
                            .FirstOrDefault();
                if (u != null)
                {
                    return u.UserID;
                }
            }
            return string.Empty;
        }

        public static bool EmailIsConfirmed(string Email, bool IsUserID = false)
        {
            using (var db = new MemberLiteEntities().Init)
            {
                dynamic u;
                if (!IsUserID)
                {
                    u = db.Users.Select(a => new { a.Email, a.EmailConfirmed })
                                    .Where(a => a.Email == Email)
                                    .FirstOrDefault();
                }
                else
                {
                    u = db.Users.Select(a => new { a.UserID, a.Email, a.EmailConfirmed })
                                    .Where(a => a.UserID == Email)
                                    .FirstOrDefault();
                }

                if (u != null)
                {
                    return u.EmailConfirmed;
                }

                return false;
            }
        }

    }
}