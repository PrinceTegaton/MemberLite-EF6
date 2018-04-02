using MemberLite_EF6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MemberLite_EF6.Controllers
{
    public class UserControlController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public void SignOut()
        {
            //if (User.Identity.Name.StartsWith("USR-")) Users.SignOut(User.Identity.Name);
            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect(FormsAuthentication.LoginUrl);
        }

        [HttpPost]
        public ContentResult CheckEmailAvailability(string Email)
        {
            return Content(Users.EmailIsAvailabile(Email).ToString().ToLower());
        }

        

        [HttpPost]
        public ActionResult SignIn(SignInVM User)
        {
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
                    //AppUtility.SetCookie("UserFName", u.FirstName);
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

        public ActionResult VerifyEmail(string e, string c)
        {
            string email = Server.HtmlDecode(e);
            string code = Server.HtmlDecode(c);

            if (!Users.EmailIsConfirmed(email))
            {
                TempData["emailconfirmed"] = Users.VerifyEmail(email, code);
                return View("EmailConfirm");
            }

            return new HttpNotFoundResult();
        }
    }
}