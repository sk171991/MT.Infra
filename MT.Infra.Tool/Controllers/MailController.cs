using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MT.Infra.BusinessLayer;
using MT.Infra.Common;
using MT.Infra.Tool.Models;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MT.Infra.Tool.Controllers
{
    public class MailController : ApiController
    {

        #region MailApproveRejectResponse
        [Route("api/Mail/MailResponse")]
        [HttpGet]
        public HttpResponseMessage MailResponse(string Action, string guid)
        {
            string minInDifference = @System.Configuration.ConfigurationManager.AppSettings["minutesDifference"];
            Token tokenObj = new Token();
            IList<Token.InsertToken> items = tokenObj.getActionTime(guid);
            DateTime dt1 = DateTime.Now;
            DateTime dt2 = Convert.ToDateTime(items[0].CreatedTimeStamp);
            TimeSpan ts = (dt1 - dt2);
            int time = Convert.ToInt32(ts.TotalMinutes);

            if (time <= Convert.ToInt32(minInDifference))
            {
                string status = tokenObj.getTokenStatus(guid, 1).Trim();
                if (status != "Approved" && status != "Closed" && status != "Cancel")
                {
                    bool retVal = Convert.ToBoolean(tokenObj.InsertAction(guid, Action));
                    if (retVal)
                    {
                        tokenObj.InsertAssignedStatus(guid);
                        tokenObj.ApproverAction(Action, guid);
                        return Post(Action);
                    }
                }
                else
                {
                    return Post(status + " " + "already");
                }
            }
            else
            {
                return Post("Invalid");
            }
            return Post("Network Error");
        }

        public HttpResponseMessage Post(string Action)
        {
            Service serviceObj = new Service();
            string url = serviceObj.Client.BaseAddress + "Login/Action?status=" + Action;
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(url);
            return response;
        }

        #endregion

        #region SR Expiry
        
        [HttpGet]
        public void SRExpiry()
        {
            Token tokenObj = new Token();
            try
            {
                tokenObj.srExpiryRemainderMailToUser();
            }
            catch(Exception ex)
            {

            }
        }

        #endregion

        #region First Approver Token Expiry
        [Route("api/Mail/FirstApproverTokenExpiry")]
        [HttpGet]
        public void FirstApproverTokenExpiry()
        {
            Token tokenObj = new Token();
            string TokenExpiry = @System.Configuration.ConfigurationManager.AppSettings["TokenExpiry"];

            string srStatus = string.Empty;

            try
            {
                foreach (var item in tokenObj.getApprover1TokenDetails())
                {
                    DateTime dt3 = DateTime.Now;
                    DateTime dt4 = Convert.ToDateTime(item.LastModifiedTimeStamp);
                    TimeSpan ts = (dt3 - dt4);
                    int tokenExpiry = Convert.ToInt32(ts.TotalMinutes);

                    if (tokenExpiry > Convert.ToInt32(TokenExpiry))
                    {
                        srStatus = tokenObj.getTokenStatus(item.guid, 1).Trim();

                        if (srStatus == "Open" && item.MetaActive == 1)
                        {
                            string newGuid = tokenObj.nextInsertToken(item.ServiceRequest_ID);
                            if (newGuid != "failure")
                            {
                                tokenObj.sendMailToApprover2(item.ServiceRequest_ID, newGuid);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        #endregion

        #region Second Approver Token Expiry

        [HttpGet]
        public void SecondApproverTokenExpiry()
        {
            Token tokenObj = new Token();
            string minInDifferenceforTokenExpiry = @System.Configuration.ConfigurationManager.AppSettings["TokenExpiry"];
           
            int SRID;
            string guid = string.Empty;
            string srStatus = string.Empty;
            string Approver2 = string.Empty;
            string UserMailId = string.Empty;
            string SRCreatedBy = string.Empty;
            int? Approver_UserID = null;
            int? User_UserID = null;
            DateTime? LastModifiedTimeStamp = null;
            try
            {
                foreach (var item in tokenObj.getApprover2TokenDetails())
                {
                    SRID = item.ServiceRequest_ID;
                    guid = item.guid;
                    srStatus = item.status;
                    Approver_UserID = item.Approver_UserID;
                    User_UserID = item.User_UserID;
                    SRCreatedBy = item.SRCreatedBy;
                    UserMailId = item.userMailID;
                    Approver2 = item.Approver2;
                    LastModifiedTimeStamp = item.LastModifiedTimeStamp;


                    DateTime dt3 = DateTime.Now;
                    DateTime dt4 = Convert.ToDateTime(LastModifiedTimeStamp);
                    TimeSpan ts = (dt3 - dt4);
                    int tokenExpiry = Convert.ToInt32(ts.TotalMinutes);

                    if (tokenExpiry > Convert.ToInt32(minInDifferenceforTokenExpiry))
                    {
                        srStatus = tokenObj.getTokenStatus(guid, 1).Trim();

                        if (srStatus == "Open")
                        {
                            tokenObj.sendMailToUser_WhenNOApproverAction(SRID, Approver2, UserMailId);
                            tokenObj.SetTokenInActive(SRID, guid);
                        }
                    }
                }
            }
            catch(Exception EX)
            {

            }
        }

        #endregion

        #region AssetExpiryList
        [HttpGet]
        public void AssetExpiryList()
        {
            string ExpiryMailID = System.Configuration.ConfigurationManager.AppSettings["AssetExpiryMailID"];
           
            AssetsManagement asm = new AssetsManagement();

            try
            {
                string textBody = "<table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 500 + "><tr bgcolor='yellow'><td><b>System Name</b></td><td><b>System IP</b></td><td><b>System Type</b></td><td><b>Expiration Date</b></td></tr>";
                if (asm.AssetExpiry().Count != 0)
                {
                    foreach (var item in asm.AssetExpiry())
                    {
                        textBody += "<tr><td>" + item.SystemName.Trim() + "</td><td> " + item.SystemIP.Trim() + "</td> <td> " + item.MachineType + "</td><td> " + item.ExpirationDate + "</td></tr>";

                    }
                    textBody += "</table>";
                    AssetExpiryMail(ExpiryMailID, textBody);
                }
            }
            catch (Exception e)
            {
               
            }
        }


        public void AssetExpiryMail(string expiryMailID, string mailBody)
        {
            
            try
            {
                // Create the Outlook application.
                Outlook.Application oApp = new Outlook.Application();
                // Create a new mail item.
                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                // Set the subject.
                oMsg.Subject = "Asset Expiry Reminder ";

                oMsg.To = expiryMailID;
                
                string sHtml = "The Asset will expire as per below details" + "<br>" + "<br>" + mailBody +
                "<br>"+
                "If you wish to extend Asset Expiration date, kindly update it else asset will be inactive in next 7 days" + "<br>" + "<br>" +
                "Note: This is automated email and no responses would be monitored";

                oMsg.HTMLBody = sHtml;


                oMsg.Send();


                oMsg = null;


                oApp = null;

                
            }
            catch(Exception ex)
            {
                
            }

           
            }

        #endregion


        [HttpGet]
        public async Task<IHttpActionResult> MailSendFromQueue()
        {
            SendMail sendMail = new SendMail();
            try
            {
                IEnumerable<SendMail.MailSendQueue> mailSendQueues = sendMail.GetMailQueues();

                if (mailSendQueues.Count() != 0)
                {
                     return Ok(mailSendQueues);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Log.CreateLog(ex);
            }
            return Ok();
        }

    }
}
