using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class SupplierDAL : DBConnection
    {
        // Get all suppliers
        public DataTable GetSuppliers()
        {
            string query = "SELECT SupplierID, SupplierName, SupplierEmail, SupplierPhone, SupplierAddress FROM Suppliers";
            return ExecuteQuery(query);
        }
        // 2. Search
        public DataTable SearchSuppliers(string keyword)
        {
            string query = @"SELECT * FROM Suppliers 
                             WHERE SupplierName LIKE @Keyword OR SupplierPhone LIKE @Keyword";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            return ExecuteQuery(query, parameters);
        }
        // 3.Add
        public bool InsertSupplier(string name, string email, string phone, string address)
        {
            string query = @"INSERT INTO Suppliers (SupplierName, SupplierEmail, SupplierPhone, SupplierAddress) 
                             VALUES (@Name, @Email, @Phone, @Address)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", name),
                new SqlParameter("@Email", email),
                new SqlParameter("@Phone", phone),
                new SqlParameter("@Address", address ?? (object)DBNull.Value)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 4. update
        public bool UpdateSupplier(int id, string name, string email, string phone, string address)
        {
            string query = @"UPDATE Suppliers 
                             SET SupplierName = @Name, 
                                 SupplierEmail = @Email, 
                                 SupplierPhone = @Phone, 
                                 SupplierAddress = @Address 
                             WHERE SupplierID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Name", name),
                new SqlParameter("@Email", email),
                new SqlParameter("@Phone", phone),
                new SqlParameter("@Address", address ?? (object)DBNull.Value)
            };

            return ExecuteNonQuery(query, parameters);
        }
        // 5.Delete
        public bool DeleteSupplier(int id)
        {
            string query = "DELETE FROM Suppliers WHERE SupplierID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id)
            };

            return ExecuteNonQuery(query, parameters);
        }
    }
}