using Project_Store_X_DB.BLL;
using Project_Store_X_DB.DTO;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ClosedXML.Excel;

namespace Project_Store_X_DB

{
    public partial class UC_Dashboard : UserControl
    {
        private DashbroadBLL _bll = new DashbroadBLL();
        private string _currentPeriod = "day";
        private DateTime _startDate, _endDate;
        public UC_Dashboard()
        {
            InitializeComponent();
            // wire buttons (ensure names exist in Designer)
            btnDay.Click += (s, e) => { _currentPeriod = "day"; RefreshAll(); };
            btnMonth.Click += (s, e) => { _currentPeriod = "month"; RefreshAll(); };
            btnQuarter.Click += (s, e) => { _currentPeriod = "quarter"; RefreshAll(); };
            btnYear.Click += (s, e) => { _currentPeriod = "year"; RefreshAll(); };
            btnRefresh.Click += (s, e) => RefreshAll();
            this.Load += UC_Dashboard_Load;
            SetupCharts();
        }
        private void UC_Dashboard_Load(object sender, EventArgs e)
        {
            string checkRole = UserSession.CurrentEmployee.RoleName;

            if (checkRole.Trim() == "Admin")
            {
                btnChangeReport.Visible = true;
            }
            else
            {
                btnChangeReport.Visible = false;
            }
            SetRangeForPeriod(_currentPeriod);
            RefreshAll();
        }
        private void SetRangeForPeriod(string period)
        {
            DateTime now = DateTime.Now.Date;
            switch (period)
            {
                case "month":
                    _startDate = new DateTime(now.Year, now.Month, 1);
                    _endDate = _startDate.AddMonths(1).AddSeconds(-1);
                    break;
                case "quarter":
                    int q = (now.Month - 1) / 3 + 1;
                    _startDate = new DateTime(now.Year, (q - 1) * 3 + 1, 1);
                    _endDate = _startDate.AddMonths(3).AddSeconds(-1);
                    break;
                case "year":
                    _startDate = new DateTime(now.Year, 1, 1);
                    _endDate = _startDate.AddYears(1).AddSeconds(-1);
                    break;
                default:
                    _startDate = now;
                    _endDate = now.AddDays(1).AddSeconds(-1);
                    break;
            }
        }
        private void RefreshAll()
        {
            SetRangeForPeriod(_currentPeriod);
            // indicators
            var ind = _bll.GetIndicators(_startDate, _endDate);
            lblIndicator1.Text = ind.NewCustomers.ToString();
            lblIndicator2.Text = ind.OrdersCount.ToString();
            lblIndicator3.Text = ind.ProductsSold.ToString();
            lblIndicator4.Text = ind.TotalRevenue.ToString("N2");           
            var dtRev = _bll.GetRevenueSeries(_startDate, _endDate, _currentPeriod);// revenue series
            BindRevenueChart(dtRev);           
            var dtCat = _bll.GetRevenueByCategory(_startDate, _endDate);// revenue by category
            BindRevenueByCategoryChart(dtCat);           
            var dtTop = _bll.GetTop5BestSelling(_startDate, _endDate);// top5
            BindTop5Chart(dtTop);         
            var dtCust = _bll.GetCustomerSpending(_startDate, _endDate);// customer spending datagrid
            dgvStatistic.DataSource = dtCust;
            FormatCustomerGrid();
        }
        private void SetupCharts()// chartRevenue 
        {
            chartRevenue.Series.Clear();
            var s = new Series("Revenue") { ChartType = SeriesChartType.Column, XValueType = ChartValueType.String };
            chartRevenue.Series.Add(s);
            chartRevenue.ChartAreas[0].AxisX.Interval = 1;
            // chartRevenuebyCategory: configure doughnut, legend and label style
            chartRevenuebyCategory.Series.Clear();
            // Ensure a legend exists and is configured
            chartRevenuebyCategory.Legends.Clear();
            var legend = new Legend("CategoriesLegend")
            {
                Docking = Docking.Right,
                LegendStyle = LegendStyle.Table,
                TableStyle = LegendTableStyle.Auto
            };
            chartRevenuebyCategory.Legends.Add(legend);
            var s2 = new Series("ByCategory")
            {
                ChartType = SeriesChartType.Doughnut,
                XValueType = ChartValueType.String,
                IsValueShownAsLabel = false // show percent via Label token
            };
            s2["PieLabelStyle"] = "Intside";
            s2["PieDrawingStyle"] = "Concave";
            s2.Legend = legend.Name;
            chartRevenuebyCategory.Series.Add(s2);
            // top5 (horizontal bar) - keep as before
            chartTop5BestSelling.Series.Clear();
            var s3 = new Series("Top5") { ChartType = SeriesChartType.Bar, XValueType = ChartValueType.String };
            chartTop5BestSelling.Series.Add(s3);
            chartTop5BestSelling.ChartAreas[0].AxisX.LabelStyle.Angle = 0;
        }
        private void BindRevenueChart(DataTable dt)
        {
            var series = chartRevenue.Series["Revenue"];
            series.Points.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var label = r["PeriodLabel"].ToString();
                var value = Convert.ToDecimal(r["Revenue"]);
                series.Points.AddXY(label, value);
            }
        }
        private void BindRevenueByCategoryChart(DataTable dt)
        {
            var series = chartRevenuebyCategory.Series["ByCategory"];
            series.Points.Clear();

            if (dt == null || dt.Rows.Count == 0)
            {
                chartRevenuebyCategory.Titles.Clear();
                return;
            }
            // Expecting column "CategoryName" and "Revenue" from DAL. Be defensive.
            string nameCol = dt.Columns.Contains("CategoryName") ? "CategoryName"
                           : dt.Columns.Contains("ProductName") ? "ProductName"
                           : (dt.Columns.Count > 0 ? dt.Columns[0].ColumnName : null);
            foreach (DataRow r in dt.Rows)
            {
                string labelText = "(Unknown)";
                if (!string.IsNullOrEmpty(nameCol) && !r.IsNull(nameCol))
                    labelText = r[nameCol].ToString();
                double revenue = 0.0;
                if (dt.Columns.Contains("Revenue") && !r.IsNull("Revenue"))
                    double.TryParse(r["Revenue"].ToString(), out revenue);
                else
                {
                    // fallback: find first numeric column
                    foreach (DataColumn c in dt.Columns)
                    {
                        if ((c.DataType == typeof(decimal) || c.DataType == typeof(double) || c.DataType == typeof(int))
                            && !r.IsNull(c))
                        {
                            double.TryParse(r[c].ToString(), out revenue);
                            break;
                        }
                    }
                }
                int idx = series.Points.AddXY(labelText, revenue);
                var pt = series.Points[idx];
                // Legend item text = category name
                pt.LegendText = labelText;
                // Show percentage as data label (Chart control computes percent automatically)
                pt.Label = "#PERCENT{P0}"; // P0 => no decimals; use P1 for one decimal
                pt.ToolTip = $"{labelText}: {revenue:N2} ({pt.Label})";
            }
            // Optional aesthetic tweaks
            chartRevenuebyCategory.ChartAreas[0].Area3DStyle.Enable3D = false;
            chartRevenuebyCategory.Invalidate();
        }
        private void BindTop5Chart(DataTable dt)
        {
            var series = chartTop5BestSelling.Series["Top5"];
            series.Points.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var name = r["ProductName"].ToString();
                var qty = Convert.ToInt32(r["QuantitySold"]);
                int idx = series.Points.AddXY(name, qty);
                series.Points[idx].Label = qty.ToString();
            }
        }
        private void FormatCustomerGrid()
        {
            dgvStatistic.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (dgvStatistic.Columns["CustomerID"] != null)
                dgvStatistic.Columns["CustomerID"].Visible = false;
            if (dgvStatistic.Columns["TotalSpent"] != null)
                dgvStatistic.Columns["TotalSpent"].DefaultCellStyle.Format = "N2";
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Refresh failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblIndicator4_Click(object sender, EventArgs e)
        {

        }
        // Add this handler method inside the class
        private void BtnChangeReport_Click(object sender, EventArgs e)
        {
            try
            {
                // Use current calendar month
                DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime end = start.AddMonths(1).AddSeconds(-1);

                var dt = _bll.GetEmployeeProductivityByMonth(start, end);
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No productivity data for the current month.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                // Bind and format
                dgvStatistic.DataSource = dt;
                dgvStatistic.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (dgvStatistic.Columns["EmployeeID"] != null) dgvStatistic.Columns["EmployeeID"].HeaderText = "Employee ID";
                if (dgvStatistic.Columns["EmployeeName"] != null) dgvStatistic.Columns["EmployeeName"].HeaderText = "Employee Name";

                if (dgvStatistic.Columns["OrdersCount"] != null)
                {
                    dgvStatistic.Columns["OrdersCount"].HeaderText = "Number of Orders";
                    dgvStatistic.Columns["OrdersCount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (dgvStatistic.Columns["TotalValue"] != null)
                {
                    dgvStatistic.Columns["TotalValue"].HeaderText = "Total Value";
                    dgvStatistic.Columns["TotalValue"].DefaultCellStyle.Format = "N2";
                    dgvStatistic.Columns["TotalValue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                // Ensure sorting by TotalValue desc
                if (dgvStatistic.DataSource is DataTable)
                {
                    var dv = new DataView((DataTable)dgvStatistic.DataSource) { Sort = "TotalValue DESC" };
                    dgvStatistic.DataSource = dv;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load monthly employee productivity: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }       
            private void BtnExportReport_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"Dashboard_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                })
                {
                    if (sfd.ShowDialog() != DialogResult.OK) return;
                    // Gather data
                    var indicators = _bll.GetIndicators(_startDate, _endDate);
                    DataTable dtRevenue = _bll.GetRevenueByPeriodDetailed(_startDate, _endDate, _currentPeriod);
                    DataTable dtPaymentUsage = _bll.GetPaymentMethodUsage(_startDate, _endDate, _currentPeriod);
                    DataTable dtBestSellers = _bll.GetBestSellingProducts(_startDate, _endDate, 200);
                    DataTable dtRevenueByCategory = _bll.GetRevenueByCategory(_startDate, _endDate);
                    DataTable dtInventory = _bll.GetCurrentInventory();
                    DataTable dtLowStock = _bll.GetLowStockAlert(10);
                    DataTable dtEmployeeSales = _bll.GetEmployeeSales(_startDate, _endDate);
                    DataTable dtTopCustomers = _bll.GetTopCustomers(_startDate, _endDate, 200);
                    // Add percent column to category table (Revenue share)
                    if (dtRevenueByCategory != null && dtRevenueByCategory.Rows.Count > 0)
                    {
                        decimal total = 0;
                        foreach (DataRow r in dtRevenueByCategory.Rows)
                        {
                            total += Convert.ToDecimal(r["Revenue"]);
                        }
                        if (!dtRevenueByCategory.Columns.Contains("RevenueShare"))
                            dtRevenueByCategory.Columns.Add("RevenueShare", typeof(string));
                        foreach (DataRow r in dtRevenueByCategory.Rows)
                        {
                            decimal rev = Convert.ToDecimal(r["Revenue"]);
                            r["RevenueShare"] = total > 0 ? (rev / total).ToString("P1") : "0%";
                        }
                    }
                    // Build workbook
                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        // Revenue overview + indicators
                        var wsInd = wb.Worksheets.Add("Revenue_Overview");
                        wsInd.Cell(1, 1).Value = "Metric"; wsInd.Cell(1, 2).Value = "Value";
                        wsInd.Cell(2, 1).Value = "Report generated"; wsInd.Cell(2, 2).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        wsInd.Cell(3, 1).Value = "Period"; wsInd.Cell(3, 2).Value = $"{_currentPeriod} | {_startDate:yyyy-MM-dd} to {_endDate:yyyy-MM-dd}";
                        wsInd.Cell(4, 1).Value = "New Customers"; wsInd.Cell(4, 2).Value = indicators.NewCustomers;
                        wsInd.Cell(5, 1).Value = "Orders Count"; wsInd.Cell(5, 2).Value = indicators.OrdersCount;
                        wsInd.Cell(6, 1).Value = "Products Sold"; wsInd.Cell(6, 2).Value = indicators.ProductsSold;
                        wsInd.Cell(7, 1).Value = "Total Revenue"; wsInd.Cell(7, 2).Value = indicators.TotalRevenue;
                        wsInd.Columns().AdjustToContents();
                        // Revenue by period
                        if (dtRevenue != null && dtRevenue.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Revenue_ByPeriod");
                            ws.Cell(1, 1).InsertTable(dtRevenue, "RevenueByPeriod", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Payment method usage
                        if (dtPaymentUsage != null && dtPaymentUsage.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Payment_Method_Usage");
                            ws.Cell(1, 1).InsertTable(dtPaymentUsage, "PaymentUsage", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Best sellers
                        if (dtBestSellers != null && dtBestSellers.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Best_Sellers");
                            ws.Cell(1, 1).InsertTable(dtBestSellers, "BestSellers", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Revenue by category (with share)
                        if (dtRevenueByCategory != null && dtRevenueByCategory.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Revenue_ByCategory");
                            ws.Cell(1, 1).InsertTable(dtRevenueByCategory, "RevenueByCategory", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Inventory current
                        if (dtInventory != null && dtInventory.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Inventory_Current");
                            ws.Cell(1, 1).InsertTable(dtInventory, "InventoryCurrent", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Low stock alert
                        if (dtLowStock != null && dtLowStock.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Low_Stock_Alert");
                            ws.Cell(1, 1).InsertTable(dtLowStock, "LowStock", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Employee sales
                        if (dtEmployeeSales != null && dtEmployeeSales.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Employee_Sales");
                            ws.Cell(1, 1).InsertTable(dtEmployeeSales, "EmployeeSales", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Top customers
                        if (dtTopCustomers != null && dtTopCustomers.Rows.Count > 0)
                        {
                            var ws = wb.Worksheets.Add("Top_Customers");
                            ws.Cell(1, 1).InsertTable(dtTopCustomers, "TopCustomers", true);
                            ws.Columns().AdjustToContents();
                        }
                        // Save workbook
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Export to Excel finished.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

    }
}