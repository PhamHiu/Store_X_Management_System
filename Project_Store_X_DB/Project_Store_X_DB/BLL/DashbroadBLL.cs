using System;
using System.Data;
using Project_Store_X_DB.DAL;

namespace Project_Store_X_DB.BLL
{
    public class DashboardIndicators
    {
        public int NewCustomers { get; set; }
        public int OrdersCount { get; set; }
        public int ProductsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
    internal class DashbroadBLL
    {
        private DashbroadDAL _dal = new DashbroadDAL();

        public DashboardIndicators GetIndicators(DateTime start, DateTime end)
        {
            return new DashboardIndicators
            {
                NewCustomers = _dal.GetNewCustomersCount(start, end),
                OrdersCount = _dal.GetOrdersCount(start, end),
                ProductsSold = _dal.GetProductsSoldCount(start, end),
                TotalRevenue = _dal.GetTotalRevenue(start, end)
            };
        }
        public DataTable GetRevenueSeries(DateTime start, DateTime end, string periodGroup)
        {
            return _dal.GetRevenueGrouped(start, end, periodGroup);
        }
        public DataTable GetRevenueByCategory(DateTime start, DateTime end)
        {
            return _dal.GetRevenueByCategory(start, end);
        }
        public DataTable GetTop5BestSelling(DateTime start, DateTime end)
        {
            return _dal.GetTop5BestSelling(start, end);
        }
        public DataTable GetCustomerSpending(DateTime start, DateTime end)
        {
            return _dal.GetCustomerSpending(start, end);
        }
        public DataTable GetEmployeeProductivityByMonth(DateTime start, DateTime end)
        {
            return _dal.GetEmployeeProductivityByMonth(start, end);
        }
        // data for reports
        public DataTable GetRevenueByPeriodDetailed(DateTime start, DateTime end, string periodGroup)
        {
            return _dal.GetRevenueByPeriodDetailed(start, end, periodGroup);
        }
        public DataTable GetPaymentMethodUsage(DateTime start, DateTime end, string periodGroup)
        {
            return _dal.GetPaymentMethodUsage(start, end, periodGroup);
        }
        public DataTable GetBestSellingProducts(DateTime start, DateTime end, int topN = 100)
        {
            return _dal.GetBestSellingProducts(start, end, topN);
        }
        public DataTable GetCurrentInventory()
        {
            return _dal.GetCurrentInventory();
        }
        public DataTable GetLowStockAlert(int threshold = 5)
        {
            return _dal.GetLowStockAlert(threshold);
        }
        public DataTable GetEmployeeSales(DateTime start, DateTime end)
        {
            return _dal.GetEmployeeSales(start, end);
        }
        public DataTable GetTopCustomers(DateTime start, DateTime end, int topN = 100)
        {
            return _dal.GetTopCustomers(start, end, topN);
        }


    }

}