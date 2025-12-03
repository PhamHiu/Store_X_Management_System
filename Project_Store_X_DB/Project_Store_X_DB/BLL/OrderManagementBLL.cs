using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class OrderManagementBLL
    {
        private OrderManagementDAL _dal = new OrderManagementDAL();
        public DataTable GetOrderList() => _dal.GetAllOrders();
        public DataTable SearchOrder(string keyword) => _dal.SearchOrders(keyword);
        // get detail products of an order
        public DataTable GetOrderProducts(int orderID)
        {
            if (orderID <= 0) return null;
            return _dal.GetOrderDetails(orderID);
        }
        // Logic cancel order
        public string CancelOrder(int orderID)
        {
            if (orderID <= 0) return "Invalid Order ID.";
            try
            {            
                if (_dal.DeleteOrder(orderID))
                    return "Success";
                return "Failed to cancel order.";
            }
            catch (Exception ex)
            {
                return "System Error: " + ex.Message;
            }
        }
    }
}