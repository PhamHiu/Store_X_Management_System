using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    internal class DashbroadDAL : DBConnection
    {
        public int GetNewCustomersCount(DateTime start, DateTime end)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Customers c
                WHERE EXISTS (
                    SELECT 1 FROM Orders o
                    WHERE o.CustomerID = c.CustomerID AND o.OrderDate BETWEEN @start AND @end
                )
                AND NOT EXISTS (
                    SELECT 1 FROM Orders o2
                    WHERE o2.CustomerID = c.CustomerID AND o2.OrderDate < @start
                )";
            var p = new SqlParameter[]
            {
                new SqlParameter("@start", start),
                new SqlParameter("@end", end)
            };
            var dt = ExecuteQuery(sql, p);
            if (dt.Rows.Count == 0) return 0;
            return Convert.ToInt32(dt.Rows[0][0]);
        }
        public int GetOrdersCount(DateTime start, DateTime end)
        {
            string sql = @"SELECT COUNT(*) FROM Orders WHERE OrderDate BETWEEN @start AND @end";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            var dt = ExecuteQuery(sql, p);
            return (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }
        public int GetProductsSoldCount(DateTime start, DateTime end)
        {
            string sql = @"
                SELECT ISNULL(SUM(od.Quantity),0)
                FROM OrderDetails od
                INNER JOIN Orders o ON od.OrderID = o.OrderID
                WHERE o.OrderDate BETWEEN @start AND @end";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            var dt = ExecuteQuery(sql, p);
            return (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }
        public decimal GetTotalRevenue(DateTime start, DateTime end)
        {
            string sql = @"
                SELECT ISNULL(SUM(od.TotalAmount),0)
                FROM OrderDetails od
                INNER JOIN Orders o ON od.OrderID = o.OrderID
                WHERE o.OrderDate BETWEEN @start AND @end";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            var dt = ExecuteQuery(sql, p);
            return (dt.Rows.Count > 0) ? Convert.ToDecimal(dt.Rows[0][0]) : 0m;
        }
        public DataTable GetRevenueGrouped(DateTime start, DateTime end, string periodGroup)
        {
            string groupExpr;
            switch (periodGroup?.ToLower())
            {
                case "month":
                    groupExpr = "CONVERT(char(7), o.OrderDate, 126)"; // yyyy-MM
                    break;
                case "quarter":
                    groupExpr = "CONCAT(DATEPART(year,o.OrderDate), '-Q', DATEPART(quarter,o.OrderDate))";
                    break;
                case "year":
                    groupExpr = "CONVERT(char(4), DATEPART(year, o.OrderDate))";
                    break;
                default:
                    groupExpr = "CONVERT(char(10), o.OrderDate, 126)"; // yyyy-MM-dd
                    break;
            }
            string sql = $@"
                SELECT {groupExpr} AS PeriodLabel, ISNULL(SUM(od.TotalAmount),0) AS Revenue
                FROM Orders o
                INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
                WHERE o.OrderDate BETWEEN @start AND @end
                GROUP BY {groupExpr}
                ORDER BY {groupExpr}";

            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetRevenueByCategory(DateTime start, DateTime end)
        {
            string sql = @"
                SELECT ISNULL(c.CategoryName,'Uncategorized') AS CategoryName,
                       ISNULL(SUM(od.TotalAmount),0) AS Revenue
                FROM Orders o
                INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
                INNER JOIN Products p ON od.ProductID = p.ProductID
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                WHERE o.OrderDate BETWEEN @start AND @end
                GROUP BY ISNULL(c.CategoryName,'Uncategorized')
                ORDER BY Revenue DESC";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetTop5BestSelling(DateTime start, DateTime end)
        {
            string sql = @"
                SELECT TOP 5 p.ProductName, ISNULL(SUM(od.Quantity),0) AS QuantitySold
                FROM Orders o
                INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
                INNER JOIN Products p ON od.ProductID = p.ProductID
                WHERE o.OrderDate BETWEEN @start AND @end
                GROUP BY p.ProductName
                ORDER BY QuantitySold DESC";

            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetCustomerSpending(DateTime start, DateTime end)
        {
            string sql = @"
                SELECT c.CustomerID, c.CustomerName, ISNULL(SUM(od.TotalAmount),0) AS TotalSpent
                FROM Orders o
                INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
                INNER JOIN Customers c ON o.CustomerID = c.CustomerID
                WHERE o.OrderDate BETWEEN @start AND @end
                GROUP BY c.CustomerID, c.CustomerName
                ORDER BY TotalSpent DESC";

            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetEmployeeProductivityByMonth(DateTime start, DateTime end)
        {
            string sql = @"
        SELECT 
            o.EmployeeID,
            ISNULL(e.EmployeeName,'(Unknown)') AS EmployeeName,
            COUNT(DISTINCT o.OrderID) AS OrdersCount,
            ISNULL(SUM(od.TotalAmount),0) AS TotalValue
        FROM Orders o
        INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
        LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
        WHERE o.OrderDate BETWEEN @start AND @end
        GROUP BY o.EmployeeID, e.EmployeeName
        ORDER BY TotalValue DESC";
            var p = new SqlParameter[]
            {
        new SqlParameter("@start", start),
        new SqlParameter("@end", end)
            };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetRevenueByPeriodDetailed(DateTime start, DateTime end, string periodGroup)
        {
            string groupExpr;
            switch (periodGroup?.ToLower())
            {
                case "month":
                    groupExpr = "CONVERT(char(7), o.OrderDate, 126)"; // yyyy-MM
                    break;
                case "quarter":
                    groupExpr = "CONCAT(DATEPART(year,o.OrderDate), '-Q', DATEPART(quarter,o.OrderDate))";
                    break;
                case "year":
                    groupExpr = "CONVERT(char(4), DATEPART(year, o.OrderDate))";
                    break;
                default:
                    groupExpr = "CONVERT(char(10), o.OrderDate, 126)"; // yyyy-MM-dd
                    break;
            }
            string sql = $@"
        SELECT {groupExpr} AS PeriodLabel,
               COUNT(DISTINCT o.OrderID) AS OrdersCount,
               ISNULL(SUM(od.TotalAmount),0) AS TotalRevenue
        FROM Orders o
        INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
        WHERE o.OrderDate BETWEEN @start AND @end
        GROUP BY {groupExpr}
        ORDER BY {groupExpr}";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }

        public DataTable GetPaymentMethodUsage(DateTime start, DateTime end, string periodGroup)
        {
            string groupExpr;
            switch (periodGroup?.ToLower())
            {
                case "month":
                    groupExpr = "CONVERT(char(7), o.OrderDate, 126)";
                    break;
                case "quarter":
                    groupExpr = "CONCAT(DATEPART(year,o.OrderDate), '-Q', DATEPART(quarter,o.OrderDate))";
                    break;
                case "year":
                    groupExpr = "CONVERT(char(4), DATEPART(year, o.OrderDate))";
                    break;
                default:
                    groupExpr = "CONVERT(char(10), o.OrderDate, 126)";
                    break;
            }

            string sql = $@"
        SELECT {groupExpr} AS PeriodLabel,
               pm.MethodName,
               COUNT(*) AS MethodCount
        FROM Orders o
        INNER JOIN PaymentMethods pm ON o.MethodID = pm.MethodID
        WHERE o.OrderDate BETWEEN @start AND @end
        GROUP BY {groupExpr}, pm.MethodName
        ORDER BY {groupExpr}, MethodCount DESC";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetBestSellingProducts(DateTime start, DateTime end, int topN = 100)
        {
            string sql = @"
        SELECT TOP (@topN)
            p.ProductID,
            p.ProductName,
            ISNULL(c.CategoryName,'') AS CategoryName,
            ISNULL(SUM(od.Quantity),0) AS QuantitySold,
            ISNULL(SUM(od.TotalAmount),0) AS TotalRevenue
        FROM OrderDetails od
        INNER JOIN Orders o ON od.OrderID = o.OrderID
        INNER JOIN Products p ON od.ProductID = p.ProductID
        LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
        WHERE o.OrderDate BETWEEN @start AND @end
        GROUP BY p.ProductID, p.ProductName, c.CategoryName
        ORDER BY QuantitySold DESC, TotalRevenue DESC";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end), new SqlParameter("@topN", topN) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetCurrentInventory()
        {
            string sql = @"
        SELECT 
            p.ProductID,
            p.ProductName,
            ISNULL(c.CategoryName,'') AS CategoryName,
            ISNULL(s.SupplierName,'') AS SupplierName,
            p.InventoryQuantity,
            p.Price,
            (p.InventoryQuantity * p.Price) AS InventoryValue
        FROM Products p
        LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
        LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
        ORDER BY p.ProductName";
            return ExecuteQuery(sql);
        }
        public DataTable GetLowStockAlert(int threshold = 10)
        {
            string sql = @"
        SELECT 
            p.ProductID,
            p.ProductName,
            ISNULL(s.SupplierName,'') AS SupplierName,
            ISNULL(s.SupplierPhone,'') AS SupplierPhone,
            p.InventoryQuantity
        FROM Products p
        LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
        WHERE p.InventoryQuantity < @threshold
        ORDER BY p.InventoryQuantity ASC";
            var p = new SqlParameter[] { new SqlParameter("@threshold", threshold) };
            return ExecuteQuery(sql, p);
        }

        public DataTable GetEmployeeSales(DateTime start, DateTime end)
        {
            string sql = @"
        SELECT 
            e.EmployeeID,
            e.EmployeeName,
            e.Position,
            COUNT(DISTINCT o.OrderID) AS OrdersCount,
            ISNULL(SUM(od.TotalAmount),0) AS TotalSales
        FROM Orders o
        INNER JOIN Employees e ON o.EmployeeID = e.EmployeeID
        INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
        WHERE o.OrderDate BETWEEN @start AND @end
        GROUP BY e.EmployeeID, e.EmployeeName, e.Position
        ORDER BY TotalSales DESC";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end) };
            return ExecuteQuery(sql, p);
        }
        public DataTable GetTopCustomers(DateTime start, DateTime end, int topN = 100)
        {
            string sql = @"
        SELECT TOP (@topN)
            c.CustomerID,
            c.CustomerName,
            c.CustomerPhone,
            COUNT(DISTINCT o.OrderID) AS OrdersCount,
            ISNULL(SUM(od.TotalAmount),0) AS TotalSpent
        FROM Orders o
        INNER JOIN Customers c ON o.CustomerID = c.CustomerID
        INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
        WHERE o.OrderDate BETWEEN @start AND @end
        GROUP BY c.CustomerID, c.CustomerName, c.CustomerPhone
        ORDER BY TotalSpent DESC";
            var p = new SqlParameter[] { new SqlParameter("@start", start), new SqlParameter("@end", end), new SqlParameter("@topN", topN) };
            return ExecuteQuery(sql, p);
        }
    }
}