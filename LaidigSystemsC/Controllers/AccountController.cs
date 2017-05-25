using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaidigSystemsC.Models;
using System.Web.Security;

namespace LaidigSystemsC.Controllers
{

  
    public class AccountController : Controller
    {

     private   OurDbContext db = new OurDbContext();


        // GET: Account
        public ActionResult Index()
        {
            if (User.Identity.Name != null)
            {
                using (OurDbContext db = new OurDbContext())
                {
                    return View(db.useraccounts.ToList());
                }
            }
            return RedirectToAction("Index", "Siemens");
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(UserAccount account)
        {
            if(ModelState.IsValid)
            {
                OurDbContext db = new OurDbContext();                
                db.useraccounts.Add(account);
                db.SaveChanges();                
                ModelState.Clear();
                ViewBag.Message = account.FirstName + " " + account.LastName + " Successfully Registered !";               
            }

            Session["UserId"] = account.UserId.ToString();
            Session["UserName"] = account.UserName.ToString();
            Session["userRole"] = account.userTypes.ToString();
            string userrole = Session["userRole"].ToString();
            Mails mail = new Mails();

            ModelState.Clear();
            string ActivationUrl = Url.Action("Login", "Account", new { username = account.FirstName, email = account.Email }, "http");
            mail.SendActivationLinkUsingEmail(account, ActivationUrl, "username");
            //return RedirectToAction("Login", "Account");
            return View();
        }


        //Login

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult HomePage()
        {
            return View();
        }



        [HttpPost]
        public ActionResult Login(UserAccount user)
        {
            using (OurDbContext db = new OurDbContext())
            {
                try
                {              

                var usr = db.useraccounts.SingleOrDefault(u => u.UserName == user.UserName && u.password == user.password && u.userStatus.ToString() == "Active");
                if(usr!=null)
                {
                    Session["UserId"] = usr.UserId.ToString();
                    Session["UserName"] = usr.FirstName.ToString();
                    Session["userRole"] = usr.userTypes.ToString();
                    string userrole = Session["userRole"].ToString();

                    return RedirectToAction("Index", "Siemens");
                }
                else
                {
                    ModelState.AddModelError("", " UserName or Password is Wrong !");
                }

                }
                catch (Exception ex)
                {
                    return PartialView("~/Views/Shared/Error.cshtml", ex);
                }
            }
            return View();
        }

        public ActionResult LoggedIn()
        {
            
            if (Session["UserId"]!=null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
            
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(UserAccount account)
        {
           
                if (account.Email.ToString() != "")
                {
                    
                    using (OurDbContext db = new OurDbContext())
                    {
                        try
                        {

                            var usr = db.useraccounts.SingleOrDefault(u => u.Email == account.Email );
                            if (usr != null)
                            {


                                Mails mail = new Mails();

                                ModelState.Clear();
                                string ActivationUrl = Url.Action("ChangePassword", "Account", new { email = account.Email }, "http");
                                mail.SendForgotPasswordUsingEmail(account, ActivationUrl);

                                ViewBag.ShowMessage = "Password chage link has sent to your mailid! Please click to chage password your account";
                                //return View();

                            }
                           

                        }
                        catch (Exception ex)
                        {
                            return PartialView("~/Views/Shared/Error.cshtml", ex);
                        }
                    }
                return View();

            }
                else
                {
                    ViewBag.ShowMessage = "Your EmailID is not Found in My Database Acount";
                    return View();
                }
           
           
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(UserAccount account)
        {
            UserAccount user = new UserAccount();
            using (var db = new OurDbContext())
            {
                user = db.useraccounts.Where(s => s.Email == account.Email).FirstOrDefault<UserAccount>();
            }
            if (user != null)
            {
                
                user.password = account.password;
                user.ConfirmPassword = account.password;
               

            }
            using (var db = new OurDbContext())
            {
                //3. Mark entity as modified
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                //4. call SaveChanges
                db.SaveChanges();
                //string message = "User records updated for " + user.FirstName + " " + user.LastName + "user Type : " + user.userTypes;
                //Mails mail = new Mails();
                //mail.SendActivationLinkUsingEmailUserUpdate(Session["UserName"].ToString(), message);
                ViewBag.message = "Successfully Changed";
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login","Account");
        }


        public ActionResult Uploadtypes()
        {
            return View();
        }



        public ActionResult CheckExistingEmail(string email)
        {
                if (db.useraccounts.Any(x => x.Email == email))
                {
                    return Json(string.Format("{0} already exits", email), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            
        }

        public ActionResult CheckExistingUserName(string username)
        {
            if (db.useraccounts.Any(x => x.UserName == username))
            {
                return Json(string.Format("{0} already exits", username), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }


    }
}