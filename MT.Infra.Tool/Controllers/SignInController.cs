using MT.Infra.BusinessLayer;
using MT.Infra.Tool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Outlook = Microsoft.Office.Interop.Outlook;
using MT.Infra.Common;

namespace MT.Infra.Tool.Controllers
{
    public class SignInController : ApiController
    {
        
        public async Task<IHttpActionResult> GetUserCredentials()
        {
            LoginUser loginUser = new LoginUser();
            try
            {
                IEnumerable<LoginUser.RegisteredUsers> registeredUsers = loginUser.GetRegisteredUsers();
                if (registeredUsers.Count() != 0)
                {
                    return Ok(registeredUsers);
                }
                else
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                Log.CreateLog(ex);
            }
            return Ok();
        }

        
        [HttpGet]
        public async Task<IHttpActionResult> Post(string MID, string validate)
        {
            LoginUser lgnUsr = new LoginUser();
            string userMailId = null;
            string userName = null;
            try
            {
                if (validate != "False")
                {
                    int retVal = lgnUsr.ValidationComplete(MID, validate);
                    foreach (var items in lgnUsr.getUser(MID))
                    {
                        userMailId = items.EmailId;
                        userName = items.Name;
                    }
                    string subject = "Account Validation : Success";
                    string login = "http://localhost:56438/";
                    // Set HTMLBody.    
                    string sHtml = "Hi Mindtree Mind, <br><br>" +
                    "We have validated your account so please click on below link to SignIn to Infra Tool. <br><br>" +
                    "Sign In Link : " + "<a href=" + login + ">" + " Click here to SignIn </a>";
                    ValidationMail(userMailId, subject, sHtml);
                    return Ok();
                }
                else
                {
                    foreach (var items in lgnUsr.getUser(MID))
                    {
                        userMailId = items.EmailId;
                        userName = items.Name;
                    }
                    string subject = "Account Validation : Failure";
                    string register = "http://localhost:56438/Login/Register";
                    // Set HTMLBody.    
                    string sHtml = "Hi Mindtree Mind, <br><br>" +
                    "We have validated your account and your Mindtree UserID and Credentials did not match so please register again <br><br>" +

                    "Registration Link : " + "<a href=" + register + ">" + " Click here to Register </a>";
                    ValidationMail(userMailId, subject, sHtml);
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                Log.CreateLog(ex);
            }
            return Ok();
        }

        public void ValidationMail(string mailId, string subject, string mailBody)
        {
            Outlook.Application oApp = new Outlook.Application();

            Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

            oMsg.Subject = subject;

            oMsg.To = mailId;
            oMsg.HTMLBody = mailBody;
            oMsg.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
            oMsg.Display(false);
            oMsg.Send();
            oMsg = null;
            oApp = null;
            
        }
    }
}
