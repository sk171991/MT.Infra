using Dapper;
using MT.Infra.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MT.Infra.BusinessLayer
{
   public  class Dashboardmanage
    {

        DapperRepository DAO = null;
        public Dashboardmanage()
        {
            DAO = new DapperRepository();
        }

        public string GetRole(string MID)
        {
            string storedProc = "sp_getRole";
            DynamicParameters param = new DynamicParameters();
            param.Add("@MID", MID);
            string role = (string)DAO.ExecuteScalar(System.Data.CommandType.StoredProcedure, sql: storedProc, parameters: param);
            return role;
        }
        public class StatusValue
        {
            public int Total { get; set; }
            public string Name { get; set; }
        }

        public IEnumerable<StatusValue> Dash(string MID)
        {
            string storedProc = "sp_ServiceRequestDashboard";
            DynamicParameters param = new DynamicParameters();
            param.Add("@mid", MID);
            var listitems = DAO.GetItems<StatusValue>(System.Data.CommandType.StoredProcedure, storedProc, param);
            return listitems;
        }

        public IEnumerable<object> TaskApproverforApprover()
        {
            string storedProc = "SP_Approvertask";
            DynamicParameters param = new DynamicParameters();
            var listitems = DAO.GetItems<object>(System.Data.CommandType.StoredProcedure, storedProc, param);
            return listitems;
        }

        public IEnumerable<object> TaskApproverforAdmin()
        {
            string storedProc = "SP_Admintask";
            DynamicParameters param = new DynamicParameters();
            var listitems = DAO.GetItems<object>(System.Data.CommandType.StoredProcedure, storedProc, param);
            return listitems;
        }
    }
}
