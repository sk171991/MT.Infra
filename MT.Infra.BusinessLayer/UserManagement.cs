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
   public class UserManagement
    {
        DapperRepository DAO = null;
        public UserManagement()
        {

            DAO = new DapperRepository();

        }
        //USERINSERT
        public int UserInsert(String Name, string EmployeeId, string Contact, string UserLocation, string EmailId, string Role)
        {

            DynamicParameters con = new DynamicParameters();
            con.Add("@Name", Name);
            con.Add("@EmployeeID", EmployeeId);
            con.Add("@ContactNumber", Contact);
            con.Add("@UserLocation", UserLocation);
            con.Add("@EmailId", EmailId);
            con.Add("@RoleId", Role);
            string sqlQuery = "sp_userinsert";

            int rowsEffected = DAO.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: con);

            return rowsEffected;
        }

        //GET ROLE
        class SelectItem
        {
            public int ID { get; set; }
            public string UserRole { get; set; }
        }

        public SelectList GetRoles()
        {
            List<SelectItem> li = new List<SelectItem>();
            string storedProc = "sp_Roleid";

            var result = DAO.GetItems<SelectItem>(System.Data.CommandType.StoredProcedure, sql: storedProc);

            var list = new SelectList(result, "ID", "UserRole");

            return list;
        }
  
        public IEnumerable<Viewusermodel> GetUser()
        {

            string storedProc = "sp_ViewUser";

            return DAO.GetItems<Viewusermodel>(System.Data.CommandType.StoredProcedure, sql: storedProc);
          }

        public int UpdateUser(Viewusermodel getu)
        {
            DynamicParameters con = new DynamicParameters();
            con.Add("@ID", getu.ID);
            con.Add("@Name", getu.Name);
            con.Add("@EmployeeID", getu.EmployeeID);
            con.Add("@ContactNumber", getu.ContactNumber);
            con.Add("@UserLocation", getu.UserLocation);
            con.Add("@EmailId", getu.EmailId);
            con.Add("@RoleId", getu.Role_ID);
            string sqlQuery = "sp_updateuser";

            int rowsEffected = DAO.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: con);

            return rowsEffected;
        }

        public int DeleteUser(int ID)
        {
            DynamicParameters con = new DynamicParameters();
            string sqlQuery = "sp_deleteUser";
            con.Add("@ID", ID);

            return DAO.Execute(commandType: System.Data.CommandType.StoredProcedure, sql: sqlQuery, parameters: con);

        }

    }
}
