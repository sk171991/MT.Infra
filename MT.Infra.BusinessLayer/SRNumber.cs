using Dapper;
using LinqToExcel;
using MT.Infra.Common;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Web.Mvc;
using System.Linq;

namespace MT.Infra.BusinessLayer
{
    public class SRNumber
    {
        readonly DapperRepository dao = null;
        public SRNumber()
        {
            dao = new DapperRepository();
        }

        public int generateSRNumber(string SRDescription, int AssetUsageType_ID, DateTime FromDate,
                                    DateTime TillDate, int User_ID, string UserLocation, string ContactNumber)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRDescription", SRDescription);
            param.Add("@AssetUsageType_ID", AssetUsageType_ID);
            param.Add("@FromDate", FromDate);
            param.Add("@TillDate", TillDate);
            param.Add("@User_ID", User_ID);
            param.Add("@UserLocation", UserLocation);
            param.Add("@ContactNumber", ContactNumber);
            param.Add("@CreatedTimeStamp", DateTime.Now);
            param.Add("@LastModifiedTimeStamp", DateTime.Now);
            param.Add("@MetaActive", 1);
            param.Add("@SRID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            string sqlQuery = "sp_ServiceRequest";

           dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
            int SRID = param.Get<int>("SRID");
            return SRID;
        }

        public int UploadAttachment(string fileName, string filePath)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@fileName", fileName);
            param.Add("@filePath", filePath);
           
           string sqlQuery = "sp_UploadAttachment";

           int fileUpload = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

            return fileUpload;
        }

        public int AttachmentSRMapping(int SRID, string AttachmentName)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            param.Add("@AttachmentName", AttachmentName);

            string sqlQuery = "sp_AttachmentSRMapping";

            int retVal = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

            return retVal;
        }

        public class EmployeeData
        {
            public int ID { get; set; }

            public string MID { get; set; }

            public string Name { get; set; }

        }

        public class GetAttachment
        {
            public string fileName { get; set; }

            public string filePath { get; set; }
        }

        public IEnumerable<GetAttachment> GetAttachments(int SRID)
        {
           DynamicParameters param = new DynamicParameters();
           param.Add("@SRID", SRID);
           
           string sqlQuery = "sp_GetAttachment";

           return dao.GetItems<GetAttachment>(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
           
        }

        public string ImportExcelData(int SRID)
        {

            string filePath = "";
            string fileName = "";
            
            foreach (var items in GetAttachments(SRID))
            {
                fileName = items.fileName;
                filePath = items.filePath;
            }

            string pathToExcelFile = filePath;

            string sheetName = "Sheet1";

            string data = "";
            var excelFile = new ExcelQueryFactory(pathToExcelFile);
            try
            {
                var empDetails = from a in excelFile.Worksheet<EmployeeData>(sheetName) select a;
                
                foreach (var a in empDetails)
                {
                    if (a.MID != null && a.Name != null)
                    {
                        int result = InsertExcelData(a.MID, a.Name, SRID);
                        if (result <= 0)
                        {
                            data = "Error !!";
                            continue;
                        }
                        else
                        {
                            data = "Success";
                        }
                    }
                    else
                    {
                        data = "Wrong Column Names";
                    }
                }
                
            }
            catch(Exception ex)
            {

            }
            return data;
        }
        
        public int InsertExcelData(string MID , string Name , int SRID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);
            param.Add("@Name", Name);
            param.Add("@SRID", SRID);
            string sqlQuery = "sp_InsertExcelData";

            int rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
            return rowsEffected;
        }

        public int CheckSRIDforMID(int SRID)
        {

            DynamicParameters param = new DynamicParameters();

            param.Add("@SRID", SRID);
            param.Add("@return", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.ReturnValue);
            string sqlQuery = "sp_CheckSRIDforMID";

            dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
            int retVal = param.Get<int>("return");
            return retVal;

        }

        public IEnumerable<EmployeeData> GetRequestedMID(int SRID)
        {
            string sqlQuery = "sp_GetRequestedMID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            
            return dao.GetItems<EmployeeData>(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
           
        }

        public string CheckAssetMIDMapping(int MID,int AssetID)
        {
            string sqlQuery = "sp_CheckAssetMIDMapping";
            DynamicParameters param = new DynamicParameters();
            param.Add("@AssetID", AssetID);
            param.Add("@MID", MID);
            param.Add("@return", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 5215585);

            dao.ExecuteScalar(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
            string exists = param.Get<string>("return");
            return exists;
            
        }

        public int TagAssetMID(int ID,int AssetID)
        {
            DynamicParameters param = new DynamicParameters();

            param.Add("@ID", ID);
            param.Add("@AssetID", AssetID);
            string sqlQuery = "sp_TagAssetMID";

            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

        }

        public int UnTagAssetMID(int MID, int AssetID)
        {
            DynamicParameters param = new DynamicParameters();

            param.Add("@MID", MID);
            param.Add("@AssetID", AssetID);
            string sqlQuery = "sp_UnTagAssetMID";

            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

        }

        public IEnumerable<EmployeeData> GetTagAssetMID(int AssetID)
        {
            DynamicParameters param = new DynamicParameters();

            param.Add("@AssetID", AssetID);
            string sqlQuery = "sp_GetTagAssetMID";

            return dao.GetItems<EmployeeData>(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

        }

        public string generateServiceRequestStatusID(int ServiceRequest_ID, int Role_ID, int User_ID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ServiceRequest_ID", ServiceRequest_ID);
            param.Add("@Status_ID", 1);
            param.Add("@Role_ID", Role_ID);
            param.Add("@User_ID", User_ID);
            param.Add("@Comments", " ");
            param.Add("@CreatedTimeStamp", DateTime.Now);
            param.Add("@LastModifiedTimeStamp", DateTime.Now);
            param.Add("@MetaActive", 1);
            string sqlQuery = "sp_SRStatus";

            string rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param).ToString();
            return rowsEffected;
        }

        public string getUserId(string mid)
        {
            string sqlQuery = "select ID from Users where EmployeeID= @mid";
            DynamicParameters param = new DynamicParameters();
            param.Add("@mid", mid);
            string uid = dao.ExecuteScalar(commandType: System.Data.CommandType.Text, sql: sqlQuery, parameters: param).ToString();
            return uid;
        }


        public string getUsermail(string mid)
        {
            string sqlQuery = "select Role_ID from Users where EmployeeID= @mid";
            DynamicParameters param = new DynamicParameters();
            param.Add("@mid", mid);
            string rName = dao.ExecuteScalar(commandType: System.Data.CommandType.Text, sql: sqlQuery, parameters: param).ToString();
            return rName;
        }

        class SelectItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public SelectList getPurpose()
        {
            List<SelectItem> li = new List<SelectItem>();
            string storedProc = "sp_Purpose";

            var result = dao.GetItems<SelectItem>(System.Data.CommandType.StoredProcedure, sql: storedProc);
            var list = new SelectList(result, "ID", "Name");

            return list;
        }

        public string generateServiceRequestNumber()
        {
            string sqlQuery = "Select top 1 (ID) as SRID from ServiceRequest order by CreatedTimeStamp desc";
            DynamicParameters param = new DynamicParameters();
            string srid = dao.ExecuteScalar(commandType: System.Data.CommandType.Text, sql: sqlQuery, parameters: param).ToString();
            return srid;
        }

        public string generateRoleID(int userID)
        {
            string sqlQuery = "select Role_ID from users where ID= @userID";
            DynamicParameters param = new DynamicParameters();
            param.Add("@userID", userID);
            string roleid = dao.ExecuteScalar(commandType: System.Data.CommandType.Text, sql: sqlQuery, parameters: param).ToString();
            return roleid;
        }

        public class SoftwareList
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public IEnumerable<SoftwareList> GetSoftwares()
        {
            string storedProc = "sp_getSoftwares";

            return dao.GetItems<SoftwareList>(System.Data.CommandType.StoredProcedure, sql: storedProc);

        }

        class SelectStatus
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public SelectList getStatus()
        {
            List<SelectStatus> li = new List<SelectStatus>();
            string storedProc = "sp_Status";

            var result = dao.GetItems<SelectStatus>(System.Data.CommandType.StoredProcedure, sql: storedProc);
            var list = new SelectList(result, "ID", "Name");

            return list;
        }

        public class showForGetExistingSR
        {
            public string SR_ID { get; set; }
            public string SRDescription { get; set; }
            public string Status { get; set; }
            public string UserRole { get; set; }
            public string UserName { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime TillDate { get; set; }
            public DateTime LastModifiedTimeStamp { get; set; }
        }

        public IEnumerable<showForGetExistingSR> getExistingSR(string MID)
        {
            string storedProc = "sp_ExistingSR";
            DynamicParameters param = new DynamicParameters();
            param.Add("@mid", MID);
            return dao.GetItems<showForGetExistingSR>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public IEnumerable<showForGetExistingSR> ReferExistingSR(string MID)
        {
            string storedProc = "sp_ReferExistingSR";
            DynamicParameters param = new DynamicParameters();
            param.Add("@mid", MID);
            return dao.GetItems<showForGetExistingSR>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public string getRole(string MID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);
            string storedProc = "sp_getRole";

            string role = dao.ExecuteScalar(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param).ToString();
            return role;
        }

        public class showForSearchExistingSR
        {
            public string SR_ID { get; set; }
            public string SRDescription { get; set; }
            public string Status { get; set; }
            public string UserName { get; set; }
            public string UserRole { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime TillDate { get; set; }
            public DateTime LastModifiedTimeStamp { get; set; }
            public string CreatedBy { get; set; }

        }

        public IEnumerable<showForSearchExistingSR> findExistingSR(int? SRID, DateTime? Fromdate, DateTime? Tilldate, int? Status, string Role, string MID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            param.Add("@Fromdate", Fromdate);
            param.Add("@Tilldate", Tilldate);
            param.Add("@Status", Status);
            param.Add("@Role", Role);
            param.Add("@Mid", MID);
            string storedProc = "sp_SearchSR";

            return dao.GetItems<showForSearchExistingSR>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }

        public class showForSRDetails
        {
            public string Name { get; set; }
            public string ServiceRequest_ID { get; set; }
            public string statusName { get; set; }
            public string AssetUsageType { get; set; }
            public string UserLocation { get; set; }
            public string ContactNumber { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime TillDate { get; set; }
            public string SRDescription { get; set; }
            public string AssignedTo { get; set; }
            public string Role { get; set; }
            public DateTime LastModifiedTimeStamp { get; set; }
            public int User_ID { get; set; }
            public int Role_ID { get; set; } 
        }

        public IEnumerable<showForSRDetails> getSRDetails(int SRID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@srid", SRID);
            string storedProc = "sp_SRDetailsFromExistingSR";

            return dao.GetItems<showForSRDetails>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }

        public class showForTicketHistory
        {
            public DateTime LastModifiedTimeStamp { get; set; }
            public string StatusName { get; set; }
            public string UserName { get; set; }
            public string UserRole { get; set; }
            public string Comments { get; set; }
        }

        public IEnumerable<showForTicketHistory> getTicketHistory(int SRID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            string storedProc = "sp_ticketHistory";

            return dao.GetItems<showForTicketHistory>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }

        public string generateStatusID(int SRID)
        {
            string sqlQuery = "select top 1 status_id from ServiceRequestStatus where ServiceRequest_ID = @SRID order by LastModifiedTimeStamp desc";
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            string statusid = dao.ExecuteScalar(commandType: System.Data.CommandType.Text, sql: sqlQuery, parameters: param).ToString();
            return statusid;
        }

        public string generateServiceRequestDetails(int ServiceRequest_ID, int Role_ID, int User_ID, string comments, int statusID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@ServiceRequest_ID", ServiceRequest_ID);
            param.Add("@Status_ID", statusID);
            param.Add("@Role_ID", Role_ID);
            param.Add("@User_ID", User_ID);
            param.Add("@Comments", comments);
            param.Add("@CreatedTimeStamp", DateTime.Now);
            param.Add("@LastModifiedTimeStamp", DateTime.Now);
            param.Add("@MetaActive", 1);
            string sqlQuery = "sp_ServiceRequestStatus";

            string rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param).ToString();
            return rowsEffected;
        }

        public int CancelSR(string CurrentSRID, int Currentuserid, int Currentroleid,string comments, DateTime CurrentLastModifiedTimeStamp)
        {
            int CurrentSRid = Convert.ToInt32(CurrentSRID);
            DynamicParameters param = new DynamicParameters();
            param.Add("@srid", CurrentSRid);
            param.Add("@roleid", Currentroleid);
            param.Add("@userid", Currentuserid);
            param.Add("@Comments", comments);
            param.Add("@lastmodifiedtimestamp", CurrentLastModifiedTimeStamp);
            string storedProc = "sp_CloseSR";

            int rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
            return rowsEffected;
        }

        public class showForGetApproverExistingSR
        {
            public string SR_ID { get; set; }
            public string SRDescription { get; set; }
            public string Status { get; set; }
            public string UserRole { get; set; }
            public string UserName { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime TillDate { get; set; }
            public DateTime LastModifiedTimeStamp { get; set; }
            public string CreatedBy { get; set; }

        }

        public IEnumerable<showForGetApproverExistingSR> getApproverExistingSR()
        {
            string storedProc = "sp_ApproverExistingSR";

            return dao.GetItems<showForGetApproverExistingSR>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public string ApproveSR(int CurrentSRID, int Currentuserid, int Currentroleid, string comment)
        {
            int CurrentSRid = Convert.ToInt32(CurrentSRID);
            DynamicParameters param = new DynamicParameters();
            param.Add("@srid", CurrentSRid);
            param.Add("@comment", comment);
            string storedProc = "sp_approveSR";

            string rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param).ToString();
            return rowsEffected;
        }

        public string RejectSR(int CurrentSRID, int Currentuserid, int Currentroleid, string comment)
        {
            int CurrentSRid = Convert.ToInt32(CurrentSRID);
            DynamicParameters param = new DynamicParameters();
            param.Add("@srid", CurrentSRid);
            param.Add("@comment", comment);
            string storedProc = "sp_rejectSR";

            string rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param).ToString();
            return rowsEffected;
        }


        public IEnumerable<showForGetApproverExistingSR> getSRDashboardSearch(string MID, string status, string Role)
        {

            string storedProc = "sp_SRDashboard";
            DynamicParameters param = new DynamicParameters();
            param.Add("@mid", MID);
            param.Add("@Role", Role);
            param.Add("@Status", status);
            return dao.GetItems<showForGetApproverExistingSR>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }
        public int UI_InsertAssignedStatus(int SRID,string mid,string Comments)
        {
            string storedProc = "sp_UIInsertAssignedStatusinSRStatus";

            DynamicParameters paramObj = new DynamicParameters();
            paramObj.Add("@srid", SRID);
            paramObj.Add("@mid", mid);
            paramObj.Add("@Comments", Comments);
            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: paramObj);
        }

        public int ApproveTokenUpdateAction(int CurrentSRID, string MID)
        {
            string sqlQuery = "sp_ApproveTokenUpdate";

            DynamicParameters param = new DynamicParameters();
            param.Add("@srid", CurrentSRID);
            param.Add("@mid", MID);
            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
        }

        public int RejectTokenUpdateAction(int CurrentSRID, string mid)
        {
            string sqlQuery = "sp_RejectTokenUpdate";

            DynamicParameters param = new DynamicParameters();
            param.Add("@srid", CurrentSRID);
            param.Add("@mid", mid);
            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);
        }

        public int InsertNextSRStatus(int SRID, int UserId, int RoleId, string Comments, string Status)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            param.Add("@UserId", UserId);
            param.Add("@RoleId", RoleId);
            param.Add("@Comments", Comments);
            param.Add("@Status", Status);
            param.Add("@Retval", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            string storedProc = "sp_InsertNextSRStatus";

            dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: storedProc, parameters:param);
            int rowsEffected = param.Get<int>("Retval");
            return rowsEffected;

        }

        public string SRCreatorMail(int SRID)
        {
            string query = "select EmailId from Users where id in ( Select user_id from ServiceRequest where id =" + SRID + ")";
            return (string)dao.ExecuteScalar(System.Data.CommandType.Text, query);
        }

    }
}
