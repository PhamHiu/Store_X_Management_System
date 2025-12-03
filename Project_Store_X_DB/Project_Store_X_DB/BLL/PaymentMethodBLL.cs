using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class PaymentMethodBLL
    {
        private PaymentMethodDAL _dal = new PaymentMethodDAL();
        public DataTable GetPaymentMethodList() => _dal.GetPaymentMethods();
        public DataTable SearchPaymentMethod(string keyword) => _dal.SearchPaymentMethods(keyword);
        // Logic add
        public string AddPaymentMethod(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Method Name is required.";
            try
            {
                if (_dal.InsertPaymentMethod(name))
                    return "Success";
                return "Failed to add payment method.";
            }
            catch (Exception ex)
            {
                // throw Exception duplicate 
                if (ex.Message.Contains("UNIQUE")) return "Method Name already exists.";
                return "Error: " + ex.Message;
            }
        }
        // Logic update
        public string EditPaymentMethod(int id, string name)
        {
            if (id <= 0) return "Invalid ID.";
            if (string.IsNullOrWhiteSpace(name)) return "Method Name is required.";

            try
            {
                if (_dal.UpdatePaymentMethod(id, name))
                    return "Success";
                return "Failed to update payment method.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE")) return "Method Name already exists.";
                return "Error: " + ex.Message;
            }
        }
        // Logic delete
        public string RemovePaymentMethod(int id)
        {
            try
            {
                if (_dal.DeletePaymentMethod(id))
                    return "Success";
                return "Failed to delete.";
            }
            catch (Exception ex)
            {
                // check can't delete if being used in orders
                if (ex.Message.Contains("FK") || ex.Message.Contains("REFERENCE"))
                {
                    return "Cannot delete: This payment method is being used in orders.";
                }
                return "Error: " + ex.Message;
            }
        }
    }
}