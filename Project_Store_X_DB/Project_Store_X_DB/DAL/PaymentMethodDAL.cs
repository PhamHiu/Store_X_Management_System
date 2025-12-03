using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class PaymentMethodDAL : DBConnection
    {
        // 1. get all payment methods
        public DataTable GetPaymentMethods()
        {
            string query = "SELECT MethodID, MethodName FROM PaymentMethods";
            return ExecuteQuery(query);
        }
        // 2. search payment methods
        public DataTable SearchPaymentMethods(string keyword)
        {
            string query = "SELECT MethodID, MethodName FROM PaymentMethods WHERE MethodName LIKE @Keyword";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            return ExecuteQuery(query, parameters);
        }
        // 3. add new payment method
        public bool InsertPaymentMethod(string name)
        {
            string query = "INSERT INTO PaymentMethods (MethodName) VALUES (@Name)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", name)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 4. upđate payment method
        public bool UpdatePaymentMethod(int id, string name)
        {
            string query = "UPDATE PaymentMethods SET MethodName = @Name WHERE MethodID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Name", name)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 5. delete payment method
        public bool DeletePaymentMethod(int id)
        {
            string query = "DELETE FROM PaymentMethods WHERE MethodID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id)
            };
            return ExecuteNonQuery(query, parameters);
        }
    }
}