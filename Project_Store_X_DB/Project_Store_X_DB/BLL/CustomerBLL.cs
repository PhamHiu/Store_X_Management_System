using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class CustomerBLL
    {
        private CustomerDAL _dal = new CustomerDAL();
        public DataTable GetCustomerList()
        {
            return _dal.GetCustomers();
        }
        public DataTable SearchCustomer(string keyword)
        {
            return _dal.SearchCustomers(keyword);
        }
        public string AddCustomer(string name, string phone, string address)
        {
            // Validate data
            if (string.IsNullOrWhiteSpace(name)) return "Customer Name is required.";
            if (string.IsNullOrWhiteSpace(phone)) return "Phone Number is required.";
            try
            {
                if (_dal.InsertCustomer(name, phone, address))
                    return "Success";
                return "Failed to add customer.";
            }
            catch (Exception ex)
            {
                // check for duplicate phone numbers
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("CustomerPhone"))
                {
                    return "Phone number already exists. Please check again.";
                }
                return "System Error: " + ex.Message;
            }
        }
        public string EditCustomer(int id, string name, string phone, string address)
        {
            if (id <= 0) return "Invalid Customer ID.";
            if (string.IsNullOrWhiteSpace(name)) return "Customer Name is required.";
            if (string.IsNullOrWhiteSpace(phone)) return "Phone Number is required.";

            try
            {
                if (_dal.UpdateCustomer(id, name, phone, address))
                    return "Success";
                return "Failed to update customer.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("CustomerPhone"))
                {
                    return "Phone number already exists.";
                }
                return "System Error: " + ex.Message;
            }
        }
        public string RemoveCustomer(int id)
        {
            try
            {
                if (_dal.DeleteCustomer(id))
                    return "Success";
                return "Failed to delete customer.";
            }
            catch (Exception ex)
            {
                // heck whether the customer has placed an order"
                if (ex.Message.Contains("FK") || ex.Message.Contains("REFERENCE"))
                {
                    return "Cannot delete: This customer has existing orders history.";
                }
                return "System Error: " + ex.Message;
            }
        }
        public DataTable GetCustomerOrderHistory(int customerID)
        {
            if (customerID <= 0) return new DataTable();
            return _dal.GetCustomerOrderHistory(customerID);
        }
    }
}