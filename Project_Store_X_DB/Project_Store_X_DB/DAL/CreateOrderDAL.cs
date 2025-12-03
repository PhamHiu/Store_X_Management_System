using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Project_Store_X_DB.DAL
{
    public class CreateOrderDAL : DBConnection
    {
        // 1. get Customers
        public DataTable GetCustomers()
        {
            return ExecuteQuery("SELECT CustomerID, CustomerName, CustomerPhone, CustomerAddress FROM Customers");
        }
        // 1b. get Customer Names for autocomplete
        public DataTable GetCustomerNames()
        {
            return ExecuteQuery("SELECT CustomerID, CustomerName, CustomerPhone, CustomerAddress FROM Customers");
        }
        // 1c. add new Customer, return new CustomerID
        public int AddCustomer(string name, string phone, string address)
        {
            int newCustomerID = -1;
            try
            {
                OpenConnection();

                string query = @"INSERT INTO Customers (CustomerName, CustomerPhone, CustomerAddress)
                                 VALUES (@Name, @Phone, @Address);
                                 SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Phone", phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Address", address ?? string.Empty);

                    object result = cmd.ExecuteScalar();
                    newCustomerID = Convert.ToInt32(result);
                }
            }
            catch (Exception)
            {
                throw; // Let BLL/UI handle messaging
            }
            finally
            {
                CloseConnection();
            }
            return newCustomerID;
        }
        // 2. get Payment Methods
        public DataTable GetPaymentMethods()
        {
            return ExecuteQuery("SELECT MethodID, MethodName FROM PaymentMethods");
        }
        // 3. get Products to flow panel
        public DataTable GetProductsAvailable()
        {
            // only get products with InventoryQuantity > 0
            return ExecuteQuery("SELECT ProductID, ProductName, Price, InventoryQuantity, ImageURL FROM Products WHERE InventoryQuantity > 0");
        }
        // 4. crate Order với Transaction
        // return new OrderID if success, else -1
        public int CreateOrder(int customerID, int employeeID, int methodID, DataTable cartDetails)
        {
            int newOrderID = -1;
            OpenConnection();
            SqlTransaction transaction = _conn.BeginTransaction();
            try
            {
                // Step 1: Insert into Orders
                string queryOrder = @"INSERT INTO Orders (CustomerID, EmployeeID, MethodID, OrderDate) 
                                      VALUES (@CusID, @EmpID, @MetID, GETDATE()); 
                                      SELECT SCOPE_IDENTITY();"; 
                using (SqlCommand cmd = new SqlCommand(queryOrder, _conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@CusID", customerID);
                    cmd.Parameters.AddWithValue("@EmpID", employeeID);
                    cmd.Parameters.AddWithValue("@MetID", methodID);
                    // return new OrderID
                    object result = cmd.ExecuteScalar();
                    newOrderID = Convert.ToInt32(result);
                }
                // Step 2: Insert into OrderDetails
                foreach (DataRow row in cartDetails.Rows)
                {
                    string queryDetail = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalAmount) 
                                           VALUES (@OrderID, @ProdID, @Qty, @Total)";
                    using (SqlCommand cmd = new SqlCommand(queryDetail, _conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", newOrderID);
                        cmd.Parameters.AddWithValue("@ProdID", row["ProductID"]);
                        cmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                        cmd.Parameters.AddWithValue("@Total", row["Total"]);
                        cmd.ExecuteNonQuery();
                    }
                }
       
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // if any error, rollback
                throw new Exception("Erorr Transaction: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return newOrderID;
        }
    }
}