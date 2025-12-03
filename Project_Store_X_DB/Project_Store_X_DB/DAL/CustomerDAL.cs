using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class CustomerDAL : DBConnection
    {
        // 1.Get Customers
        public DataTable GetCustomers()
        {
            string query = "SELECT CustomerID, CustomerName, CustomerPhone, CustomerAddress FROM Customers";
            return ExecuteQuery(query);
        }
        // 2. Search Customers by Name or Phone
        public DataTable SearchCustomers(string keyword)
        {
            string query = @"SELECT CustomerID, CustomerName, CustomerPhone, CustomerAddress 
                             FROM Customers 
                             WHERE CustomerName LIKE @Keyword OR CustomerPhone LIKE @Keyword";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            return ExecuteQuery(query, parameters);
        }

        // 3. Add Customer
        public bool InsertCustomer(string name, string phone, string address)
        {
            string query = "INSERT INTO Customers (CustomerName, CustomerPhone," +
                           " CustomerAddress) VALUES (@Name, @Phone, @Address)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", name),
                new SqlParameter("@Phone", phone),
                new SqlParameter("@Address", address ?? (object)DBNull.Value)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 4. update Customer
        public bool UpdateCustomer(int id, string name, string phone, string address)
        {
            string query = "UPDATE Customers SET CustomerName = @Name, CustomerPhone = @Phone," +
                           " CustomerAddress = @Address WHERE CustomerID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Name", name),
                new SqlParameter("@Phone", phone),
                new SqlParameter("@Address", address ?? (object)DBNull.Value)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 5. Delete Customer
        public bool DeleteCustomer(int id)
        {
            string query = "DELETE FROM Customers WHERE CustomerID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // Get Customer Order History
        public DataTable GetCustomerOrderHistory(int customerID)
        {
            string sql = @"
        SELECT 
            o.OrderID,
            o.OrderDate,
            p.ProductName,
            od.Quantity,
            CASE WHEN od.Quantity = 0 THEN 0 ELSE CAST(od.TotalAmount
            / od.Quantity AS decimal(18,2)) END AS UnitPrice,
            od.TotalAmount
        FROM Orders o
        INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
        LEFT JOIN Products p ON od.ProductID = p.ProductID
        WHERE o.CustomerID = @CustomerID
        ORDER BY o.OrderDate DESC, o.OrderID DESC, p.ProductName";
            SqlParameter[] p = new SqlParameter[]
            {
        new SqlParameter("@CustomerID", customerID)
            };
            return ExecuteQuery(sql, p);
        }
    }
}