using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace MT.Infra.Common
{
    public  class DapperRepository
    {
        private readonly string connectionString;

        #region Constructor

         public DapperRepository()
        {
            connectionString = ConfigurationManager.ConnectionStrings["MyDatabase"].ConnectionString;
        }

        #endregion

        #region Standard Dapper functionality

        public List<Parent> GetList<Parent, Child>(string query, string splitOn, Func<Parent, Child, Parent> map, object parameters, System.Data.CommandType cmdType, int commandTimeout=30)
        {

            using (var connection = GetOpenConnection())
            {
                return connection.Query<Parent, Child, Parent>(query, map, parameters, null, false, splitOn: splitOn,commandTimeout:commandTimeout,commandType: cmdType).ToList();
            }
        }
        
        public IEnumerable<T>
        GetItems<T>(CommandType commandType, string sql, object parameters = null)
        {
            using (var connection = GetOpenConnection())
            {
                return connection.Query<T>(sql, parameters, commandType: commandType,commandTimeout:0);
            }
        }

        public int Execute(CommandType commandType, string sql, object parameters = null)
        {
            using (var connection = GetOpenConnection())
            {
                return  connection.Execute(sql, parameters, commandType: commandType);
            }
        }

        public object ExecuteScalar(CommandType commandType, string sql, object parameters = null)
        {
            using (var connection = GetOpenConnection())
            {
                return connection.ExecuteScalar(sql, parameters, commandType: commandType);
                
            }
        }

        public SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        #endregion

        public IList<T>
         GetList<T>(CommandType commandType, string sql, object parameters = null)
        {
            using (var connection = GetOpenConnection())
            {
                return connection.Query<T>(sql, parameters, commandType: commandType, commandTimeout: 0).AsList<T>();
            }
        }

    }   
}
