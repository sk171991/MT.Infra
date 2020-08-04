using Dapper;
using MT.Infra.BusinessLayer.Models;
using MT.Infra.Common;
using System;
using System.Collections.Generic;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MT.Infra.BusinessLayer
{
    public class Token
    {
        readonly DapperRepository dao = null;

        public Token()
        {
            dao = new DapperRepository();
        }

        public string generateGUID()
        {
            Guid obj = Guid.NewGuid();
            return obj.ToString();
        }

        public class InsertToken
        {
            public string guid { get; set; }
            public int ServiceRequest_ID { get; set; }
            public int Status_ID { get; set; }
            public int User_ID { get; set; }
            public DateTime CreatedTimeStamp { get; set; }
            public DateTime LastModifiedTimeStamp { get; set; }
            public int MetaActive { get; set; }
        }

        public int insertToken(int SRID)
        {
            string storedProc = "sp_insertToken";
            string guid = generateGUID();

            DynamicParameters param = new DynamicParameters();
            param.Add("@guid", guid);
            param.Add("@ServiceRequest_ID", SRID);
            param.Add("@Status_ID", 1);
            param.Add("@CreatedTimeStamp", DateTime.Now);
            param.Add("@LastModifiedTimeStamp", DateTime.Now);
            param.Add("@MetaActive", 1);

            int retVal = dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

            sendMailToUser(SRID);
            sendMailToApprover(SRID,guid,1);
            return retVal;
        }

        public string nextInsertToken(int SRID)
        {
            string storedProc = "sp_insertToken";
            string guid = generateGUID();

            DynamicParameters param = new DynamicParameters();
            param.Add("@guid", guid);
            param.Add("@ServiceRequest_ID", SRID);
            param.Add("@Status_ID", 1);
            param.Add("@CreatedTimeStamp", DateTime.Now);
            param.Add("@LastModifiedTimeStamp", DateTime.Now);
            param.Add("@MetaActive", 1);

            if (dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param) == 1)
            {
                return guid;
            }
            else
            {
                return "failure";
            }
        }

        public void sendMailToApprover2(int srid, string guid)
        {
            int SRID = srid;
            string Name = string.Empty;
            string srDescription = string.Empty;
            string ApproverEmailID = string.Empty;
            DateTime? CreatedTimeStamp = null;
            int? Role_ID = null;
            int? Delegate_ID = null;
            int? MetaActive = null;

            try
            {
                foreach (var item in userSRDetailsForMail(srid))
                {
                    srDescription = item.SRDescription;
                    CreatedTimeStamp = item.CreatedTimeStamp;
                    Name = item.Name;
                }

                foreach (var items in approverList())
                {
                    ApproverEmailID = items.ApproverEmailID;
                    Role_ID = items.Role_ID;
                    Delegate_ID = items.Delegate_ID;
                    MetaActive = items.MetaActive;
                }

                Outlook.Application oApp = new Outlook.Application();

                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                oMsg.Subject = "New Service Ticket ID SR000000" + SRID + "  has been submitted";

                oMsg.To = ApproverEmailID;

                string ApproveLink = "http://localhost:56438/api/Mail/MailResponse?Action=Approved&guid=" + guid;
                string rejectlink = "http://localhost:56438/api/Mail/MailResponse?Action=Rejected&guid=" + guid;

                // Set HTMLBody.    
                String sHtml = "Approver 1 couldn't take action for below SR <br>" +
                "SR Number SR000000" + SRID + "<br>" +
                "Created By : " + Name + "<br>" +
                "SR Description : " + srDescription + "<br>" +
                "Submitted On : " + DateTime.Now + "<br><br>" +

                "Please take action against below SR000000" + SRID +
                    "<br><a href=" + ApproveLink + ">" + " Approve</a> " +
                    " or <a href=" + rejectlink + "> Reject </a></br>";

                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }



        public IEnumerable<MailParametersForUsers> userSRDetailsForMail(int SRID)
        {
            string storedProc = "sp_userSRDetails";
            DynamicParameters parameters = new DynamicParameters();
            return dao.GetItems<MailParametersForUsers>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: parameters);
        }

        public void sendMailToUser(int SRID)
        {
            
            string SRDescription = string.Empty;
            string EmailID = string.Empty;
            try
            {
                foreach (var items in userSRDetailsForMail(SRID))
                {
                    
                    SRDescription = items.SRDescription;
                    EmailID = items.EmailId;
                }

                // Create the Outlook application.
                Outlook.Application oApp = new Outlook.Application();

                // Create a new mail item.
                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                // Set the subject.

                oMsg.Subject = "New Service Ticket ID SR000000" + SRID + "  has been submitted";

                oMsg.To = EmailID;

                // Set HTMLBody.

                string sHtml = "A New Ticket has been Submitted for below SR <br>" +
                "SR Number : "+ SRID + "<br>" +
                "SR Description : " + SRDescription + "<br>" +
                "Submitted On : " + DateTime.Now + "<br><br>";

                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }

        }

        public IEnumerable<ApproverDetails> approverList()
        {
            string storedProc = "sp_approverList";

            return dao.GetItems<ApproverDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public void sendMailToApprover(int SrID , string GUID,int count)
        {
            string SRID = string.Empty;
            string Name = string.Empty;
            string srDescription = string.Empty;
            string ApproverEmailID = string.Empty;
            DateTime? CreatedTimeStamp = null;
            int? Role_ID = null;
            int? Delegate_ID = null;
            int? MetaActive = null;

            try
            {
                foreach(var item in userSRDetailsForMail(SrID))
                {
                    SRID = item.SRID;
                    srDescription = item.SRDescription;
                    CreatedTimeStamp = item.CreatedTimeStamp;
                    Name = item.Name;
                }

                foreach (var items in approverList())
                {
                    ApproverEmailID = items.ApproverEmailID;
                    Role_ID = items.Role_ID;
                    Delegate_ID = items.Delegate_ID;
                    MetaActive = items.MetaActive;
                    break;
                }

                Outlook.Application oApp = new Outlook.Application();

                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                oMsg.Subject = "New Service Ticket ID " + SRID + "  has been submitted";

                oMsg.To = ApproverEmailID;

                string ApproveLink = "http://localhost:56438/api/Mail/MailResponse?Action=Approved&guid=" + GUID;
                string rejectlink = "http://localhost:56438/api/Mail/MailResponse?Action=Rejected&guid=" + GUID;

                // Set HTMLBody.    
                String sHtml = "A New Ticket has been Submitted for below SR <br>" +
                "SR Number : " + SRID + "<br>" +
                "Created By : " + Name + "<br>" +
                "SR Description : " + srDescription + "<br>" +
                "Submitted On : " + DateTime.Now + "<br><br>" +

                "Please take action against below " + SRID +
                    "<br><a href=" + ApproveLink + ">" + " Approve</a> " +
                    " or <a href=" + rejectlink + "> Reject </a></br>";

                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }



        public int InsertAction(string GUID, string Action)
        {
            string sqlQuery = "sp_insertAction";

            DynamicParameters param = new DynamicParameters();
            param.Add("@Action", Action);
            param.Add("@GUID", GUID);

            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
        }

        public int InsertAssignedStatus(string guid)
        {
            string storedProc = "sp_InsertAssignedStatusinSRStatus";

            DynamicParameters paramObj = new DynamicParameters();
            paramObj.Add("@guid", guid);

            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: paramObj);
        }

        public void ApproverAction(string Action, string guid)
        {
            IEnumerable<MailParametersForUsers> mailParamForUser = ParamForUsers(Action, guid);
            IEnumerable<MailParametersForAdmin> mailParamForAdmin = ParamForAdmin(Action, guid);

            if (Action == "Approved")
            {
                sendMailToUser_AfterApproverAction(mailParamForUser, Action);
                sendMailToAdmin_AfterApproverAction(mailParamForUser, mailParamForAdmin);
            }
            else
            {
                sendMailToUser_AfterApproverAction(mailParamForUser, Action);
            }
        }



        public IEnumerable<MailParametersForUsers> ParamForUsers(string Action, string guid)
        {
            string storedProc = "sp_userMailDetails";

            DynamicParameters paramObj = new DynamicParameters();
            paramObj.Add("@guid", guid);

            return dao.GetItems<MailParametersForUsers>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: paramObj);
        }

        public IEnumerable<MailParametersForAdmin> ParamForAdmin(string Action, string guid)
        {
            string storedProc = "sp_adminMailDetails";

            DynamicParameters paramObj = new DynamicParameters();
            paramObj.Add("@guid", guid);

            return dao.GetItems<MailParametersForAdmin>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: paramObj);
        }

        public void sendMailToUser_AfterApproverAction(IEnumerable<MailParametersForUsers> mailparamforUser, string Action)
        {

            string SRID = string.Empty;
            string mailId = string.Empty;
            string subject = string.Empty;
            try
            {
                foreach (var items in mailparamforUser)
                {
                    SRID = items.SRID;
                    mailId = items.EmailId;
                    subject = items.SRDescription;
                }
                Outlook.Application oApp = new Outlook.Application();

                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                oMsg.Subject = "Approver Remarks for  Ticket ID : " + SRID;

                oMsg.To = mailId;

                string sHtml;

                sHtml = "This is to inform you that your SR raised " + SRID + " " +
                                  "has been reviewed by Approver" + " " +
                                  "and has been " + Action;
                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }

        public IEnumerable<AdminDetails> adminList()
        {
            string storedProc = "sp_adminList";

            return dao.GetItems<AdminDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc);

        }

        public void sendMailToAdmin_AfterApproverAction(IEnumerable<MailParametersForUsers> mailparamforUser, IEnumerable<MailParametersForAdmin> mailParamForAdmin)
        {
            string Name = string.Empty;
            string SRID = string.Empty;
            string subject = string.Empty;
            DateTime? createdOn = null;
            string adminMailId = string.Empty;
            int? Role_ID = null;
            int? Delegate_ID = null;
            int? MetaActive = null;

            try
            {
                foreach (var items in mailparamforUser)
                {
                    Name = items.Name;
                    SRID = items.SRID;
                    subject = items.SRDescription;
                    createdOn = items.CreatedTimeStamp;
                }

                foreach (var items in adminList())
                {
                    adminMailId = items.AdminEmailID;
                    Role_ID = items.Role_ID;
                    Delegate_ID = items.Delegate_ID;
                    MetaActive = items.MetaActive;
                    break;
                }

                // Create the Outlook application.
                Outlook.Application oApp = new Outlook.Application();

                // Create a new mail item.
                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                // Set the subject.
                oMsg.Subject = "Take Action for " + SRID;

                // send mail to
                oMsg.To = adminMailId;


                // Set HTMLBody.    
                String sHtml = "A New Ticket has been Submitted for below SR <br>" +
                "SR Number : "  + SRID + "<br>" +
                "Created By : " + Name + "<br>" +
                "SR Description : " + subject + "<br>" +
                "SR Created Date  : " + createdOn + "<br>" + "<br>" +
                "Please take action against " + SRID + " which is assigned to you" ;

                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null; 

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }




        #region UI MAIL

        public IEnumerable<MailParametersForUsers> UI_userSRDetailsForMail(string SRID)
        {
            string storedProc = "sp_UIuserSRDetails";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);

            return dao.GetItems<MailParametersForUsers>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }


        public void UI_sendMailToUser_AfterApproved(string SRID)
        {
            string mailId = string.Empty;
            string subject = string.Empty;
            try
            {
                foreach (var items in UI_userSRDetailsForMail(SRID))
                {
                    SRID = items.SRID;
                    mailId = items.EmailId;
                    subject = items.SRDescription;
                }
                Outlook.Application oApp = new Outlook.Application();

                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                oMsg.Subject = "Approver Remarks for  Ticket ID : " + SRID;

                oMsg.To = mailId;

                string sHtml;

                sHtml = "This is to inform you that your SR raised " + SRID + " " +
                                  "has been reviewed by Approver" + " " +
                                  "and has been Approved" ;
                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }

        public void UI_sendMailToUser_AfterReject(string SRID)
        {

           
            string mailId = string.Empty;
            string subject = string.Empty;
            try
            {
                foreach (var items in UI_userSRDetailsForMail(SRID))
                {
                    SRID = items.SRID;
                    mailId = items.EmailId;
                    subject = items.SRDescription;
                }
                Outlook.Application oApp = new Outlook.Application();

                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                oMsg.Subject = "Approver Remarks for  Ticket ID : " + SRID;

                oMsg.To = mailId;

                string sHtml;

                sHtml = "This is to inform you that your SR raised " + SRID + " " +
                                  "has been reviewed by Approver" + " " +
                                  "and has been Rejected";
                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }

        public void UI_sendMailToAdmin_AfterApproved(string SRID)
        {
            string Name = string.Empty;
            string subject = string.Empty;
            DateTime? createdOn = null;
            string adminMailId = string.Empty;
            int? Role_ID = null;
            int? Delegate_ID = null;
            int? MetaActive = null;

            try
            {
                foreach (var items in UI_userSRDetailsForMail(SRID))
                {
                    Name = items.Name;
                    SRID = items.SRID;
                    subject = items.SRDescription;
                    createdOn = items.CreatedTimeStamp;
                }

                foreach (var items in adminList())
                {
                    adminMailId = items.AdminEmailID;
                    Role_ID = items.Role_ID;
                    Delegate_ID = items.Delegate_ID;
                    MetaActive = items.MetaActive;
                    break;
                }

                // Create the Outlook application.
                Outlook.Application oApp = new Outlook.Application();

                // Create a new mail item.
                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                // Set the subject.
                oMsg.Subject = "Take Action for " + SRID;

                // send mail to
                oMsg.To = adminMailId;


                // Set HTMLBody.    
                String sHtml = "A New Ticket has been Submitted for below SR <br>" +
                "SR Number : " + SRID + "<br>" +
                "Created By : " + Name + "<br>" +
                "SR Description : " + subject + "<br>" +
                "SR Created Date : " + createdOn + "<br>" + "<br>" +
                "Please take action against " + SRID + " which is assigned to you";

                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }


        #endregion

        
        public int generateToken(string GUID, string ServiceRequest_ID, DateTime MailSentTimeStamp, int MetaActive)
        {
            string sqlQuery = "Insert Into Token (GUID,ServiceRequest_ID,MailSentTimeStamp,MetaActive) Values('" + GUID + "','" + ServiceRequest_ID + "','" + MailSentTimeStamp + "'," + MetaActive + ")";

            return dao.Execute(commandType: System.Data.CommandType.Text, sql: sqlQuery);
        }

        public IList<InsertToken> getActionTime(string GUID)
        {
            string sqlQuery = "select CreatedTimeStamp from Token where GUID = '" + GUID + "'";

            return dao.GetList<InsertToken>(commandType: System.Data.CommandType.Text, sql: sqlQuery);
        }

        public string getTokenStatus(string GUID, int MetaActive)
        {
            string sqlQuery = "sp_getTokenStatus";

            DynamicParameters param = new DynamicParameters();
            param.Add("@GUID", GUID);
            param.Add("@MetaActive", MetaActive);

            return dao.ExecuteScalar(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param).ToString();
        }

        public IEnumerable<TokenDetails> getTokenDetails()
        {
            string storedProc = "sp_getTokenDetails";

            return dao.GetItems<TokenDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public class SRExpiryParameters
        {
            public string SRID { get; set; }
            public string CreatedBy { get; set; }
            public string CreatorMailID { get; set; }
            public string SRDescription { get; set; }
            public string UsageType { get; set; }
            public DateTime SRCreatedDate { get; set; }
            public DateTime SRTillDate { get; set; }
            public int DiffInDate { get; set; }
        }

        public IList<SRExpiryParameters> SRExpiryList()
        {
            string storedProc = "sp_SrExpiryDetails";

            return dao.GetList<SRExpiryParameters>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public void srExpiryRemainderMailToUser()
        {
           
            if (SRExpiryList().Count != 0)
            {
                try
                {
                    foreach (var item in SRExpiryList())
                    {

                        // Create the Outlook application.
                        Outlook.Application oApp = new Outlook.Application();

                        // Create a new mail item.
                        Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                        // Set the subject.

                        oMsg.Subject = "Service Request Expiry Reminder for " + item.SRID;

                        oMsg.To = item.CreatorMailID;

                        // Set HTMLBody.

                        string sHtml = "The SR Number :" + item.SRID + " raised on " + item.SRCreatedDate +
                        " with below details will expire on " + item.SRTillDate + "<br>" +
                        "SR Description:" + item.SRDescription + "<br>" +
                        "Usage Type : " + item.UsageType + "<br>" + "<br>" +
                        "If you wish to extend your ticket, kindly raise a new service request and this ticket will get auto closed on " + item.SRTillDate + "<br>" + "<br>" +
                        "Note: This is automated email and no responses would be monitored";

                        oMsg.HTMLBody = sHtml;

                        oMsg.Send();

                        oMsg = null;

                        oApp = null;

                    }
                }
                catch (Exception e)
                {
                    Log.CreateLog(e);
                }
            }
            
        }

        public void sendMailToUser_WhenNOApproverAction(int SRID, string Approver2, string UserMailId)
        {
            try
            {
                Outlook.Application oApp = new Outlook.Application();

                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                oMsg.Subject = "Remarks for Ticket ID SR000000" + SRID;

                oMsg.To = UserMailId;

                string sHtml;

                sHtml = "We already tried to take approvals 2 times for this SRID SR000000" + SRID + "." + "<br>" +
                        "However as there is no response from the approvers, the SRID SR000000" + SRID + "will be auto closed." + "<br>" +
                        "We recommend you to raise new SR" + "<br>" +
                        //"Please get in touch with Approver " + Approver2 + "<br>" +
                        " <br>" +
                        "Note: This is automated email and no responses would be monitored";

                oMsg.HTMLBody = sHtml;

                oMsg.Send();

                oMsg = null;

                oApp = null;

            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
        }

        public IEnumerable<TokenDetails> getApprover1TokenDetails()
        {
            string storedProc = "sp_getApprover1TokenDetails";

            return dao.GetItems<TokenDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public IEnumerable<TokenDetails> getApprover2TokenDetails()
        {
            string storedProc = "sp_getApprover2TokenDetails";

            return dao.GetItems<TokenDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public void SetTokenInActive(int SRID , string Guid)
        {
            string storedProc = "sp_SetTokenInActive";
            DynamicParameters param = new DynamicParameters();

            param.Add("@SRID", SRID);
            param.Add("@Guid", Guid);

            dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc , parameters: param);
        }
    }
}
