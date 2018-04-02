using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MemberLite_EF6.Models;
using System.Web.Security;

namespace MemberLite_EF6.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(SignInVM User)
        {
            //add fullname and userid to vm

            if (ModelState.IsValid)
            {
                Users u = new Users();

                if (u.Authenticate(User.Login, User.Passkey))
                {
                    DateTime exDate = User.RememberMe ? DateTime.Now.AddMonths(6) : DateTime.Now.AddDays(1);
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, u.UserID,
                                                            DateTime.Now,
                                                            exDate,
                                                            true,
                                                            u.Fullname, FormsAuthentication.FormsCookiePath);
                    AppUtility.SetCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket), exDate, FormsAuthentication.RequireSSL);

                    AppUtility.SetCookie("UserFName", u.FirstName);
                    AppUtility.SetCookie("DeviceType", AppUtility.GetDeviceType());

                    if (string.IsNullOrEmpty(User.ReturnUrl))
                    {
                        return Redirect(FormsAuthentication.DefaultUrl);
                    }
                    else
                    {
                        return Redirect("~" + HttpUtility.UrlDecode(User.ReturnUrl));
                    }
                }
                else
                {
                    ModelState.AddModelError("", Users.ReturnMessage);
                    TempData["SignInMsg"] = Users.ReturnMessage;
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Provide login and password");
            TempData["SignInMsg"] = "Provide login and password";
            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        [HttpPost]
        public void SignOut()
        {
            //if (User.Identity.Name.StartsWith("USR-")) Users.SignOut(User.Identity.Name);
            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect(FormsAuthentication.LoginUrl);
        }

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(FormCollection f)
        {
            //Get form values
            var model = new Users
            {
                SignInType = (int)Users.SignInTypes.Direct,
                FirstName = f["FirstName"],
                OtherNames = f["OtherNames"],
                Email = f["Email"],
                Phone = Convert.ToInt64(f["Phone"]),
                Password = f["Password"]
            };

            if (model.Create())
            {
                //login and send verification email
                FormsAuthentication.SetAuthCookie(model.UserID, true);
                Users.SendVerificationEmail(model.UserID);

                return RedirectToAction("Secure");
            }
            else
            {
                ModelState.AddModelError("", Users.ReturnMessage);
                return RedirectToAction("SignUp", "Home");
            }
        }

        [Authorize]
        public ActionResult Secure()
        {
            using (var db = new MemberLiteEntities().Init)
            {
                //fetch user info
                var u = db.Users.Find(User.Identity.Name);
                return View(u);
            }

        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(string OldPassword, string NewPassword1, string NewPassword2)
        {
            if (Users.ChangePassword(User.Identity.Name, OldPassword, NewPassword1))
            {
                //do stuffs here
            }

            ModelState.AddModelError("", Users.ReturnMessage);
            return View();
        }

        [Authorize]
        public ActionResult VerifyEmail(string e, string c)
        {
            string email = Server.HtmlDecode(e);
            string code = Server.HtmlDecode(c);

            if (!Users.EmailIsConfirmed(email))
            {
                if (Users.VerifyEmail(email, code))
                {
                    return Content("Email address has been verified!");
                }
            }
            else
            {
                return Content("Email address has already been verified!");
            }

            return new HttpNotFoundResult();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string Email)
        {
            if (!string.IsNullOrEmpty(Email))
            {
                if (Users.ResetPassword(Email))
                {
                    //do stufs here .. modelerror just to display msg
                    ModelState.AddModelError("", Users.ReturnMessage);
                    return View();
                }
                else
                {
                    ModelState.AddModelError("", Users.ReturnMessage);
                    return View();
                }
            }

            ModelState.AddModelError("", "Provide your account email address first!");
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}