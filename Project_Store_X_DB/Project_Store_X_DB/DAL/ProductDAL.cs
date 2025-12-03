using System;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class ProductDAL : DBConnection
    {
        // 1.get product list
        public DataTable GetProducts()
        {
            string query = @"
                SELECT 
                    p.ProductID, 
                    p.ProductName, 
                    p.ProductDesc, 
                    p.ImageURL, 
                    p.Price, 
                    p.InventoryQuantity, 
                    p.CategoryID, 
                    c.CategoryName, 
                    p.SupplierID, 
                    s.SupplierName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID";
            return ExecuteQuery(query);
        }
        // 2. get category ComboBox
        public DataTable GetCategoriesForCombo()
        {
            return ExecuteQuery("SELECT CategoryID, CategoryName FROM Categories");
        }
        // 3. get Supplier for ComboBox
        public DataTable GetSuppliersForCombo()
        {
            return ExecuteQuery("SELECT SupplierID, SupplierName FROM Suppliers");
        }
        // 4. add product
        public bool InsertProduct(string name, string desc, string imageURL, 
            decimal price, int quantity, int categoryID, int supplierID)
        {
            string query = @"
                INSERT INTO Products (ProductName, ProductDesc, ImageURL, Price, 
                                        InventoryQuantity, CategoryID, SupplierID) 
                VALUES (@Name, @Desc, @Image, @Price, @Qty, @CatID, @SupID)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", desc ?? (object)DBNull.Value),
                new SqlParameter("@Image", imageURL ?? (object)DBNull.Value),
                new SqlParameter("@Price", price),
                new SqlParameter("@Qty", quantity),
                new SqlParameter("@CatID", categoryID),
                new SqlParameter("@SupID", supplierID)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 5. update product
        public bool UpdateProduct(int id, string name, string desc, string imageURL, 
            decimal price, int quantity, int categoryID, int supplierID)
        {
            string query = @"
                UPDATE Products 
                SET ProductName = @Name, 
                    ProductDesc = @Desc, 
                    ImageURL = @Image, 
                    Price = @Price, 
                    InventoryQuantity = @Qty, 
                    CategoryID = @CatID, 
                    SupplierID = @SupID 
                WHERE ProductID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", desc ?? (object)DBNull.Value),
                new SqlParameter("@Image", imageURL ?? (object)DBNull.Value),
                new SqlParameter("@Price", price),
                new SqlParameter("@Qty", quantity),
                new SqlParameter("@CatID", categoryID),
                new SqlParameter("@SupID", supplierID)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 6. delete product
        public bool DeleteProduct(int id)
        {
            string query = "DELETE FROM Products WHERE ProductID = @ID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ID", id)
            };
            return ExecuteNonQuery(query, parameters);
        }
        // 7. search products by name
        public DataTable SearchProducts(string keyword)
        {
            string query = @"
                SELECT p.*, c.CategoryName, s.SupplierName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.ProductName LIKE @Keyword";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            return ExecuteQuery(query, parameters);
        }
        // 8. Product Inventory Report
        public DataTable GetProductInventoryReport(DateTime snapshot)
        {
            
            string sql = @"
        SELECT 
            p.ProductID,
            p.ProductName,
            ISNULL(c.CategoryName,'') AS CategoryName,
            ISNULL(sup.SupplierName,'') AS SupplierName,
            p.InventoryQuantity AS EndingInventory,
            ISNULL(p.InventoryQuantity + sold.SoldSince, p.InventoryQuantity) AS BeginningInventory
        FROM Products p
        LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
        LEFT JOIN Suppliers sup ON p.SupplierID = sup.SupplierID
        LEFT JOIN (
            SELECT od.ProductID, ISNULL(SUM(od.Quantity),0) AS SoldSince
            FROM OrderDetails od
            INNER JOIN Orders o ON od.OrderID = o.OrderID
            WHERE o.OrderDate >= @snapshot
            GROUP BY od.ProductID
        ) sold ON p.ProductID = sold.ProductID
        ORDER BY p.ProductName;
            ";
            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@snapshot", snapshot)
            };
            return ExecuteQuery(sql, parameters);
        }
    }
}