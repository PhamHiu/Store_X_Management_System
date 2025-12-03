using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class OrderManagementDAL : DBConnection
    {
        // 1. get all orders
        public DataTable GetAllOrders()
        {
            string query = @"
                SELECT 
                    o.OrderID,
                    o.OrderDate,
                    c.CustomerName,
                    e.EmployeeName AS Salesperson,
                    pm.MethodName AS PaymentMethod,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM OrderDetails WHERE OrderID = o.OrderID) AS GrandTotal
                FROM Orders o
                JOIN Customers c ON o.CustomerID = c.CustomerID
                JOIN Employees e ON o.EmployeeID = e.EmployeeID
                JOIN PaymentMethods pm ON o.MethodID = pm.MethodID
                ORDER BY o.OrderDate DESC"; // Sắp xếp đơn mới nhất lên đầu
            return ExecuteQuery(query);
        }
        // 2.Search bill
        public DataTable SearchOrders(string keyword)
        {
            string query = @"
                SELECT 
                    o.OrderID,
                    o.OrderDate,
                    c.CustomerName,
                    e.EmployeeName AS Salesperson,
                    pm.MethodName AS PaymentMethod,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM OrderDetails WHERE OrderID = o.OrderID) AS GrandTotal
                FROM Orders o
                JOIN Customers c ON o.CustomerID = c.CustomerID
                JOIN Employees e ON o.EmployeeID = e.EmployeeID
                JOIN PaymentMethods pm ON o.MethodID = pm.MethodID
                WHERE c.CustomerName LIKE @Keyword OR CAST(o.OrderID AS NVARCHAR) LIKE @Keyword
                ORDER BY o.OrderDate DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            return ExecuteQuery(query, parameters);
        }
        // 3. View order details
        public DataTable GetOrderDetails(int orderID)
        {
            string query = @"
                SELECT 
                    p.ProductName,
                    od.Quantity,
                    p.Price AS UnitPrice,
                    od.TotalAmount
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @OrderID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderID", orderID)
            };
            return ExecuteQuery(query, parameters);
        }
        // 4. cancel order(must be dalete in orderdetail first)
        public bool DeleteOrder(int orderID)
        {
            string query = @"
                BEGIN TRANSACTION;
                BEGIN TRY
                    DELETE FROM OrderDetails WHERE OrderID = @OrderID;
                    DELETE FROM Orders WHERE OrderID = @OrderID;
                    COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                    ROLLBACK TRANSACTION;
                    THROW;
                END CATCH";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OrderID", orderID)
            };
            return ExecuteNonQuery(query, parameters);
        }
    }
}