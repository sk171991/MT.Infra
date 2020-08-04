using Dapper;
using MT.Infra.Common;
using System;
using System.Collections.Generic;

namespace MT.Infra.BusinessLayer
{
   public  class AssetsManagement
    {
        DapperRepository dao = null;
        public AssetsManagement()
        {

            dao = new DapperRepository();

        }
        //USERINSERT
        public int AssetsInsert(string SystemName, string SystemIP, string SerialNumber, string MachineCode, string MachineType, DateTime RegistrationDate, DateTime ExpirationDate)
        {

            DynamicParameters con = new DynamicParameters();
            con.Add("@SystemName", SystemName);
            con.Add("@SystemIP", SystemIP);
            con.Add("@SerialNumber", SerialNumber);
            con.Add("@MachineCode", MachineCode);
            con.Add("@MachineType", MachineType);
            con.Add("@RegistrationDate", RegistrationDate);
            con.Add("@ExpirationDate", ExpirationDate);
            string sqlQuery = "sp_Assetsinsert";

            int rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: con);

            return rowsEffected;
        }


        public IEnumerable<ViewAssets> GetAssets()
        {

            string storedProc = "sp_AssetMapping";

            return dao.GetItems<ViewAssets>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public IEnumerable<ViewAssetsModel> ConfigureAssets()
        {

            string storedProc = "sp_ViewAssets";

            return dao.GetItems<ViewAssetsModel>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public int UpdateAssets(ViewAssetsModel getA)
        {
            DynamicParameters con = new DynamicParameters();
            con.Add("@ID", getA.ID);
            con.Add("@SystemName", getA.SystemName);
            con.Add("@SystemIP", getA.SystemIP);
            con.Add("@SerialNumber", getA.SerialNumber);
            con.Add("@MachineCode", getA.MachineCode);
            con.Add("@MachineType", getA.MachineType);
            con.Add("@RegistrationDate", getA.RegistrationDate);
            con.Add("@ExpirationDate", getA.ExpirationDate);
            string sqlQuery = "sp_updateAssets";

            int rowsEffected = dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: con);

            return rowsEffected;
        }

        public int DeleteAssests(int ID)
        {

            DynamicParameters con = new DynamicParameters();
            string sqlQuery = "sp_deleteAssets";
            con.Add("@ID", ID);

            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: con);

        }

        public int SoftwareDelete(int ID)
        {

            DynamicParameters param = new DynamicParameters();
            string sqlQuery = "sp_SoftwareDelete";
            param.Add("@ID", ID);

            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

        }

        public IEnumerable<Softwares> SoftwareDetails()
        {
            string sqlQuery = "sp_SoftwaresDetails";
           
            return dao.GetItems<Softwares>(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery);

        }

        public int AddSoftware(int ID , string Name , string Description)
        {
            string sqlQuery = "sp_addSoftwares";
            DynamicParameters param = new DynamicParameters();
            param.Add("@ID", ID);
            param.Add("@Name", Name);
            param.Add("@Description", Description);
            return dao.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: param);

        }
        public class Asset
        {
            public string SystemName { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public IEnumerable<string> Status { get; set; }
            public IEnumerable<string> MachineType { get; set; }

        }

        public class Softwares
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime CreatedTimeStamp { get; set; }
        }
        public class ViewAssets
        {
            public int ID { get; set; }
            public string SystemName { get; set; }
            public string SystemIP { get; set; }
            public string SerialNumber { get; set; }
            public string MachineCode { get; set; }
            public string MachineType { get; set; }
            public DateTime RegistrationDate { get; set; }
            public DateTime ExpirationDate { get; set; }
            public string ServiceRequestID { get; set; }
            public DateTime? AssignedDate { get; set; }
        }
        public IEnumerable<ViewAssets> SearchAssets(Asset assetdetails)
        {
            string assetStatus = string.Empty;
            string machineType = string.Empty;
            DynamicParameters param = new DynamicParameters();
            foreach (var status in assetdetails.Status)
            {
                if (status == "")
                {
                    assetStatus = null;
                }
                else
                {
                    assetStatus = status;
                }
            }
            foreach (var type in assetdetails.MachineType)
            {
                if (type == "")
                {
                    machineType = null;
                }
                else
                {
                    machineType = type;
                }
            }
            param.Add("@SystemName", assetdetails.SystemName);
            param.Add("@RegistrationDate", assetdetails.RegistrationDate);
            param.Add("@ExpirationDate", assetdetails.ExpirationDate);
            param.Add("@MachineType", machineType);
            param.Add("@Status", assetStatus);

            string storedProc = "Sp_SearchAssets";

            return dao.GetItems<ViewAssets>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }
        public class AssetDashboard
        {
            public int Total { get; set; }
            public string MachineType { get; set; }
        }

        public IEnumerable<ViewAssets> AssetDashboardSearch(string MachineType, string Status)
        {

            DynamicParameters param = new DynamicParameters();

            param.Add("@MachineType", MachineType);
            param.Add("@Status", Status);

            string storedProc = "Sp_AssetDashboardSearch";

            return dao.GetItems<ViewAssets>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public IEnumerable<AssetDashboard> getAvailableAssets()
        {
            string storedProc = "sp_AssetAvailableDash";

            return dao.GetItems<AssetDashboard>(System.Data.CommandType.StoredProcedure, storedProc);
        }

        public IEnumerable<AssetDashboard> getUnAvailableAssets()
        {
            string storedProc = "sp_AssetUnAvailableDash";

            return dao.GetItems<AssetDashboard>(System.Data.CommandType.StoredProcedure, storedProc);
        }

        public string SRCreatorMail(int SRID)
        {
            string query = "select EmailId from Users where id in ( Select user_id from ServiceRequest where id =" + SRID + ")";
            return (string)dao.ExecuteScalar(System.Data.CommandType.Text, query);
        }

        public int AssetUnassign(int ID)
        {

            string updtQuery = "Update AssetSRMapping set MetaActive = 0 where Asset_ID = " + ID + "and MetaActive = 1";

            return dao.Execute(System.Data.CommandType.Text, sql: updtQuery);
        }

        
        public IEnumerable<Softwares> ViewSoftwares(int AssetID)
        {

            string storedProc = "sp_AssetSoftwareMapping";
            DynamicParameters param = new DynamicParameters();
            param.Add("@AssetId", AssetID);
            return dao.GetItems<Softwares>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public IEnumerable<Softwares> AddSoftwares(int AssetID)
        {

            string storedProc = "sp_ViewSoftwares";
            DynamicParameters param = new DynamicParameters();
            param.Add("@AssetId", AssetID);
            return dao.GetItems<Softwares>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public int TagSoftware(int AssetID , int SoftwareID)
        {
            string storedProc = "sp_TagAssetSoftware";
            DynamicParameters param = new DynamicParameters();
            param.Add("@AssetId", AssetID);
            param.Add("@SoftwareID", SoftwareID);
            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }


        public int UnTagSoftware(int AssetID, int SoftwareID)
        {
            string storedProc = "sp_UnTagAssetSoftware";
            DynamicParameters param = new DynamicParameters();
            param.Add("@AssetId", AssetID);
            param.Add("@SoftwareID", SoftwareID);
            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public void AssetAssign(int SRID, int AssetID)
        {

            string storedProc = "sp_AssetSRMapping";

            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);
            param.Add("@AssetID", AssetID);

            dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
        }

        public IEnumerable<AssetExpiryParameters> GetAsset(int SRID)
        {
           string storedProc = "sp_AssetAssigned";

            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);

            return dao.GetItems<AssetExpiryParameters>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }

        public IEnumerable<AssetReports> GetAssetMIDList(int SRID)
        {
            string storedProc = "sp_GetAssetMIDList";

            DynamicParameters param = new DynamicParameters();
            param.Add("@SRID", SRID);

            return dao.GetItems<AssetReports>(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);

        }

        public class AssetExpiryParameters
        {
           public string SystemName { get; set; }
           public string SystemIP { get; set; }
           public string MachineType { get; set; }
           public DateTime ExpirationDate { get; set; }
           
        }
        public IList<AssetExpiryParameters> AssetExpiry()
        {
            string storedProc = "sp_AssetExpiryDetails";
            return dao.GetList<AssetExpiryParameters>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public int SetAssetInActive()
        {
            string storedProc = "sp_SetAssetInActive";
            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

        public int FreeAssetsAfterSRExpiry()
        {
            string storedProc = "sp_FreeAssetsAfterSRExpiry";
            return dao.Execute(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }
        public class AssetReports
        {
            public string MID { get; set; }
            public string EmployeeName { get; set; }
            public string ServiceRequestID { get; set; }
            public string SRDescription { get; set; }
            public string SRCreatedBy { get; set; }
            public DateTime SRCreatedTimeStamp { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime? TillDate { get; set; }
            public string AssetName { get; set; }
            public string IPAddress { get; set; }
            public string Category { get; set; }
            public DateTime? ExpirationDate { get; set; }

        }
        public IEnumerable<AssetReports> AssetReport()
        {
            string storedProc = "sp_AssetReport";
            return dao.GetItems<AssetReports>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }
        public IEnumerable<AssetReports> AssetReportSearch(AssetReports reports)
        {
            string ServiceRequestID = string.Empty;
           
            if (reports.ServiceRequestID != null)
            {
                if (reports.ServiceRequestID.Contains("SR000000"))
                {
                    ServiceRequestID = reports.ServiceRequestID;
                    ServiceRequestID = ServiceRequestID.Substring(ServiceRequestID.LastIndexOf("0") + 1);
                    
                }
                else
                {
                    ServiceRequestID = reports.ServiceRequestID;
                }
            }
            else
            {
                ServiceRequestID = null;
            }
            string storedProc = "sp_AssetReportSearch";
            DynamicParameters param = new DynamicParameters();
            param.Add("@ServiceRequestID", ServiceRequestID);
            param.Add("@AssetName", reports.AssetName);
            param.Add("@SRTillDate", reports.TillDate);
            param.Add("@AssetExpiryDate", reports.ExpirationDate);
            param.Add("@Category", reports.Category);

            return dao.GetItems<AssetReports>(System.Data.CommandType.StoredProcedure, sql: storedProc , parameters:param);
        }
    }
}
