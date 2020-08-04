using System;
using System.Web;
using System.Web.Mvc;
using MT.Infra.Tool.Models;
using MT.Infra.BusinessLayer;
using MT.Infra.Common;
using System.Net.Http;
using System.DirectoryServices.AccountManagement;
using System.Web.Security;
using System.DirectoryServices.Protocols;
using System.Net;

namespace MT.Infra.Tool.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public Employee loginData { get; set; } 
     
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost] 
        public ActionResult Index(Employee loginData)
        {
            if (ModelState.IsValid)
            {
                string empId = loginData.MID;
                string pwd = loginData.Password;


                if (!string.IsNullOrEmpty(empId) && !string.IsNullOrEmpty(pwd))
                {
                    Log.CreateLog("Successful Insert", logLevel: Level.Info);
                    return RedirectToAction("Welcome");
                }

            }
            ModelState.AddModelError("", "Invalid login attempt");
            return View(loginData);
        }

        public ActionResult Welcome()
        {
            ViewBag.Message = "Mail Sent";
            return View("View");
        }

      
        public ActionResult Approved(string val)
        {
            
            ViewBag.Message = val;
            return View("View");
        }

        [HttpPost]
        public JsonResult ResetPwd(string Email)
        {
           // string emailId = register.Email;
            LoginUser loginUser = new LoginUser();
            bool retVal = loginUser.ResetPassword(Email);
            if(retVal == true)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            
        }

        public string DecodeString(string RequestEncodeString)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(RequestEncodeString);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string decodedString = new String(decoded_char);
            return decodedString;
        }

        // GET: Reset
        [HttpGet]
        public ActionResult Reset(string id,string dt)
        {
            LoginUser loginUser = new LoginUser();
            string emailId = DecodeString(id);
            string resetTime = DecodeString(dt);
            bool validPasswordReq =  loginUser.CheckResetTime(resetTime);
            if (validPasswordReq)
            {
                ViewBag.Email = emailId;
                return View();
                
            }
            else
            {
                ViewBag.Action = "This link is Invalid as the time limit to reset the password has exceeded . Please try again !!!";
                return View("View");
            }

        }

        [HttpPost]
        public JsonResult Reset(Register register)
        {
            LoginUser loginUser = new LoginUser();

            if (register.Password != null && register.ConfirmPassword != null)
            {
                string emailId = register.Email;
                var password = System.Web.Helpers.Crypto.HashPassword(register.Password);
                var verified = System.Web.Helpers.Crypto.VerifyHashedPassword(password, register.ConfirmPassword);
                if (verified)
                {
                    try
                    {
                        int pwdStatus = loginUser.ChangePassword(emailId, password);
                        if (pwdStatus == 1)
                        {
                            ViewBag.Email = emailId;
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            ViewBag.Email = emailId;
                            return Json(false, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.CreateLog(ex);
                    }
                }
                else
                {
                    return Json(new
                    {
                        error = "Password and Confirm Password do not match"
                    });
                }
            }
            return Json(new
            {
                error = "Please fill the Missing Mandatory fields"
            });
        }
    

        public ActionResult Action(string status)
        {

            if (status == "Approved" || status == "Rejected")
            {
                ViewBag.Action = "Thanks for your Action, SR has been " + status;
            }
            else if (status.Contains("already"))
            {
                ViewBag.Action = "SR has been " + status;
            }
            else if (status.Contains("Invalid"))
            {
                ViewBag.Action = "SR approve/reject link is " + status;
            }
            else
            {
                ViewBag.Action = status;
            }
            return View("View");
        }

        // GET: Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Register register)
        {
            if (ModelState.IsValid)
            {
                string UserExist = UserType(register.MID);
                if (UserExist == null)
                {
                    LoginUser loginUser = new LoginUser();
                    string EmployeeID = register.MID;
                    string Name = register.FullName;
                    string UserLocation = register.City;
                    string EmailId = register.Email;
                    //var password = System.Web.Helpers.Crypto.HashPassword(register.Password);
                    string password = loginUser.EncodeString(register.Password);
                   

                    try
                    {
                        int retVal = loginUser.Register(Name, EmployeeID, password, UserLocation, EmailId);

                        if (retVal == 1)
                        {
                           
                          ViewBag.Register = "Your registration is Successful. You will soon receive email confirmation to login to the application";
                            
                        }
                        else
                        {
                            ViewBag.Register = "Email ID already exists . Please give correct Email ID for registration";
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.CreateLog(ex);
                    }
                }
                else
                {
                    ViewBag.Register = "Registration cannot be completed as MID already exists";
                }
            }
            return View();
        }

        // GET: login
        public ActionResult Loginform()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Loginform(Employee loginData)
        {
            
            string strName = loginData.MID;
            string strPwd = loginData.Password;
            LoginUser lgnUsr = new LoginUser();
            try
            {
                if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strPwd))
                {
                    string usrType = UserType(strName);
                    if (usrType != null)
                    {
                        string getPwd = lgnUsr.GetPassword(strName);
                        //var verified = System.Web.Helpers.Crypto.VerifyHashedPassword(getPwd, strPwd);
                        string decryptPwD = DecodeString(getPwd);
                        if(decryptPwD == strPwd)
                        { 
                            foreach (var items in lgnUsr.getUser(strName))
                            {
                                TempData["userMailID"] = items.EmailId;
                                Session["username"] = items.Name;
                                Session["id"] = strName;
                            }

                            switch (usrType)
                            {
                                case "Approver":
                                    return RedirectToAction("ApproverDashboard", "Approver");

                                case "Admin":
                                    return RedirectToAction("AdminDashboard", "Admin");

                                default:
                                    return RedirectToAction("UserDashboard", "User");

                            }
                        }

                        else
                        {
                            ViewBag.Message = "Please enter correct MID or Password";
                        }

                    }
                    else
                    {
                        ViewBag.Message = "Access Denied : Please register yourself to access the application";
                    }
                }
                else
                {
                    ViewBag.Message = "Please enter MID and Password";
                }
            }
            catch (Exception ex)
            {
                Log.CreateLog(ex);
            }
            return View("Loginform");
        }

        [NonAction]
        public bool ValidateUser(string uName, string pwd)
        {
            using (var domainContext = new PrincipalContext(ContextType.Domain,"MINDTREE"))
            {
                using (var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, uName))
                {
                    bool isValid = domainContext.ValidateCredentials(uName, pwd);
                    string st = foundUser.DisplayName;
                    string userMailID = foundUser.EmailAddress;
                    TempData["userMailID"] = userMailID;

                    Session["username"] = st;
                    Session["id"] = uName;
                    return isValid;
                }
            }
        }

        public static bool fnValidateUser(string uName, string pwd)
        {
            bool validation;
            try
            {
              
                LdapConnection lcon = new LdapConnection(Environment.UserDomainName);
                NetworkCredential nc = new NetworkCredential(uName, pwd);
                lcon.Credential = nc;
                lcon.AuthType = AuthType.Anonymous;
                // user has authenticated at this point,
                // as the credentials were used to login to the dc.
                lcon.Bind();
                validation = true;
            }
            catch (LdapException)
            {
                validation = false;
            }
            return validation;
        }

        [NonAction]
        public bool InsertUser(string uName, string pwd)
        {
            int retVal = 1;
            using (var domainContext = new PrincipalContext(ContextType.Domain, "MINDTREE"))
            {
                using (var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, uName))
                {
                    UserManagement um = new UserManagement();
                    if (retVal == um.UserInsert(foundUser.DisplayName, uName,null, null, foundUser.EmailAddress, "1"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        [NonAction]
        public string UserType(string mID)
        {
            LoginUser lgnUsr = new LoginUser();
            return lgnUsr.getRole(mID);
        }


        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Loginform", "Login");
        }
    }
}