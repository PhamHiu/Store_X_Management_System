using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class CreateOrderBLL
    {
        private CreateOrderDAL _dal = new CreateOrderDAL();
        public DataTable GetCustomerList() => _dal.GetCustomers();
        public DataTable GetPaymentList() => _dal.GetPaymentMethods();
        public DataTable GetProductList() => _dal.GetProductsAvailable();
        // New: expose customer names / info for autocomplete and UI
        public DataTable GetCustomerNames() => _dal.GetCustomerNames();

        // New: create customer and return new CustomerID
        public int AddCustomer(string name, string phone, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name is required.");

            return _dal.AddCustomer(name.Trim(), phone?.Trim(), address?.Trim());
        }
        // logic checkout order
        public string CheckoutOrder(int customerID, int employeeID, int methodID, DataTable cart)
        {
            // 1. check input
            if (cart == null || cart.Rows.Count == 0)
            {
                return "Cart is empty!";
            }
            if (customerID <= 0) return "Please select a Customer.";
            if (methodID <= 0) return "Please select a Payment Method.";
            try
            {
                // 2. Call DAL to create order
                int orderID = _dal.CreateOrder(customerID, employeeID, methodID, cart);

                if (orderID > 0)
                    return "Success"; 
                else
                    return "Failed to create order.";
            }
            catch (Exception ex)
            {
                // throw error if not have product in stock
                return "Error: " + ex.Message;
            }
        }
    }
}