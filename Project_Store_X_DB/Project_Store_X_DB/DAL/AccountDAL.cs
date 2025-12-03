using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class AccountDAL : DBConnection
    {
        public DataTable GetAccountByUsername(string username)
        {
            string spName = "usp_GetAccountByUsername";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };
            return ExecuteQuery(spName, parameters, true);
        }
    }
}