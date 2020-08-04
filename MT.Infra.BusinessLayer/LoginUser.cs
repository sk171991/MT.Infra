using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MT.Infra.Common;
using Dapper;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MT.Infra.BusinessLayer
{
    public class LoginUser
    {
        DapperRepository dao = null;
        public LoginUser()
        {

            dao = new DapperRepository();

        }

        public string getRole(string MID)
        {
            string storedProc = "sp_getRole";
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);
            string role = (string)dao.ExecuteScalar(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
            return role;
        }
        public class UserDetails
        {
            public string Name { get; set; }
            public string EmailId { get; set; }
        }
        public IEnumerable<UserDetails> getUser(string MID)
        {
            string storedProc = "sp_GetUserDetails";
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);
            return dao.GetItems<UserDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
            
        }

        public int Register(string Name , string EmployeeID , string Password , string UserLocation , string EmailId)
        {
            string storedProc = "sp_Registration";
            DynamicParameters param = new DynamicParameters();
            param.Add("@Name", Name);
            param.Add("@EmployeeID", EmployeeID);
            param.Add("@Password", Password);
            param.Add("@UserLocation", UserLocation);
            param.Add("@EmailId", EmailId);
            param.Add("@return", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.ReturnValue);

            dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
            int retVal = param.Get<int>("return");
            return retVal;
        }

        public string GetPassword(string MID)
        {
            string storedProc = "sp_getPassword";
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);

            string password = dao.ExecuteScalar(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param).ToString();

            return password;
        }

        public int EmailVerification(string EmailId)
        {
            string storedProc = "sp_EmailVerification";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@EmailId", EmailId);

            return Convert.ToInt32(dao.ExecuteScalar(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: parameters));

        }

        public string EncodeString(string RequestedString)
        {
            byte[] encData_byte = new byte[RequestedString.Length];
            encData_byte = System.Text.Encoding.UTF8.GetBytes(RequestedString);
            string encryptString = Convert.ToBase64String(encData_byte);
            return encryptString;
        }

        public bool ResetPassword(string Email)
        {
            bool status = false;

            int retVal = EmailVerification(Email);

            DateTime now = DateTime.Now;

            if (retVal == 1)
            {
                try
                {
                    string encryptEmail = EncodeString(Email);
                    string datetime = EncodeString(now.ToString());

                    Outlook.Application oApp = new Outlook.Application();

                    Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                    oMsg.Subject = "Password Reset Request";

                    oMsg.To = Email;

                    string ResetLnk = "http://localhost:56438/Login/Reset?id="+ encryptEmail + "&dt=" + datetime;

                    // Set HTMLBody.    
                    string sHtml = "Hi Mindtree Mind, <br><br>" +
                    "You seem to have requested for password reset so please click on the below link to change your Password.<br><br>" +
                    "Reset Link : " + "<a href=" + ResetLnk + ">" + " Click here to Reset your Password </a> " + "<br><br>" +

                    "Please note : This reset password link is only valid for the next 30 minutes";

                    oMsg.HTMLBody = sHtml;

                    oMsg.Send();

                    oMsg = null;

                    oApp = null;

                    status =  true;
                }


                catch (Exception e)
                {
                    Log.CreateLog(e);
                }
            }
            else
            {
                return status;
            }

            return status;
        }

        public bool CheckResetTime(string ResetDateTime)
        {
            bool status = false;

            DateTime now = DateTime.Now;

            DateTime resetTime = Convert.ToDateTime(ResetDateTime);

            TimeSpan ts = (now - resetTime);
            int time = Convert.ToInt32(ts.TotalMinutes);

            if(time <= 30)
            { 
                try
                {
                    status = true;
                }


                catch (Exception e)
                {
                    Log.CreateLog(e);
                }
            }
            else
            {
                return status;
            }

            return status;
        }

        public int ChangePassword(string EmailId, string Password)
        {
            string storedProc = "sp_ChangePassword";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Emailid", EmailId);
            parameters.Add("@Password", Password);
            parameters.Add("@return", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.ReturnValue);
        
            dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: parameters);
            int retVal = parameters.Get<int>("return");
            return retVal;

        }

        public class RegisteredUsers
        {
            public string MID { get; set; }
            public string password { get; set; }
        }

        public IEnumerable<RegisteredUsers> GetRegisteredUsers()
        {
            string storedProc = "sp_GetRegisteredUsers";

            return dao.GetItems<RegisteredUsers>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }
        public int ValidationComplete(string MID,string Validate)
        {
            string storedProc = "sp_ValidateRegisterUsers";
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);
            param.Add("@Validate", Validate);
            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }
    }

    public enum Roles
    {
        Approver,
        Admin
    }
}
