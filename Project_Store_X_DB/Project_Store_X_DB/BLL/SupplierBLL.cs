using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class SupplierBLL
    {
        private SupplierDAL _dal = new SupplierDAL();
        public DataTable GetSupplierList() => _dal.GetSuppliers();
        public DataTable SearchSupplier(string keyword) => _dal.SearchSuppliers(keyword);
        // Logic add
        public string AddSupplier(string name, string email, string phone, string address)
        {
            // check input
            if (string.IsNullOrWhiteSpace(name)) return "Supplier Name is required.";
            if (string.IsNullOrWhiteSpace(phone)) return "Phone Number is required.";
            if (string.IsNullOrWhiteSpace(email)) return "Email is required.";
            try
            {
                if (_dal.InsertSupplier(name, email, phone, address))
                    return "Success";
                return "Failed to add supplier.";
            }
            catch (Exception ex)
            {
                // check duplicate email or phone
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("SupplierEmail"))
                    return "Email already exists.";
                if (ex.Message.Contains("SupplierPhone"))
                    return "Phone number already exists.";

                return "Error: " + ex.Message;
            }
        }
        // Logic update
        public string EditSupplier(int id, string name, string email, string phone, string address)
        {
            if (id <= 0) return "Invalid Supplier ID.";
            if (string.IsNullOrWhiteSpace(name)) return "Supplier Name is required.";
            try
            {
                if (_dal.UpdateSupplier(id, name, email, phone, address))
                    return "Success";
                return "Failed to update supplier.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE")) return "Email or Phone already exists.";
                return "Error: " + ex.Message;
            }
        }
        // Logic delete
        public string RemoveSupplier(int id)
        {
            try
            {
                if (_dal.DeleteSupplier(id))
                    return "Success";
                return "Failed to delete.";
            }
            catch (Exception ex)
            {
                // constrain foreign key
                if (ex.Message.Contains("FK") || ex.Message.Contains("REFERENCE"))
                {
                    return "Cannot delete: This supplier is linked to existing products.";
                }
                return "Error: " + ex.Message;
            }
        }
    }
}