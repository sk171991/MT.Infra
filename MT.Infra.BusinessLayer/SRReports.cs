using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MT.Infra.Common;
using Dapper;
using System.Web.Mvc;

namespace MT.Infra.BusinessLayer
{
   public class SRReports
    {
        DapperRepository dao = null;
        public SRReports()
        {

            dao = new DapperRepository();

        }

        public class Reports
        {
            public string Name { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public IList<string> Status { get; set; }


        }

        public class Status
        { 
            public int ID { get; set; }
            public string Name { get; set; }

        }

        public class ReportTable
        {
       
            public string ServiceRequestID { get; set; }
            public string SRDescription { get; set; }
            public string statusName { get; set; }
            public string CreatedBy { get; set; }
            public string UserLocation { get; set; }
            public string ContactNumber { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? TillDate { get; set; }
            public DateTime? CreatedTimeStamp { get; set; }
            public DateTime? LastModifiedTimeStamp { get; set; }
            public string AssetName { get; set; }
            public DateTime? AssetAssignedDate { get; set; }
           
        }

        public IEnumerable<ReportTable> SRReport(Reports rept)
        {
            string selectedStatus = string.Empty;
            DynamicParameters param = new DynamicParameters();
            if (rept.Status == null)
            {
                selectedStatus = null;
            }
            else
            {
                for (int i = 0; i < rept.Status.Count; i++)
                {
                    selectedStatus += rept.Status[i] + ",";
                }
                selectedStatus =  selectedStatus.Trim(',');
            }
            
            param.Add("@Name", rept.Name);
            param.Add("@StartDate", rept.StartDate);
            param.Add("@EndDate", rept.EndDate);
            param.Add("@Status", selectedStatus);
            
            string storedProc = "sp_SRReport";

            return dao.GetItems<ReportTable>(System.Data.CommandType.StoredProcedure, sql: storedProc , parameters:param);
        }

        public IEnumerable<Status> getStatus()
        {
            string storedProc = "sp_Status";
          
            
            return dao.GetItems<Status>(System.Data.CommandType.StoredProcedure, sql: storedProc);

          
        }

    }
}

