using Project_Store_X_DB.BLL;
using Project_Store_X_DB.DTO;
using System;
using System.Data;
using System.Text; // Dùng cho StringBuilder
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Project_Store_X_DB
{
    public partial class UC_OrderManagement : UserControl
    {
        private OrderManagementBLL _bll = new OrderManagementBLL();
        public UC_OrderManagement()
        {
            InitializeComponent();
            this.Load += UC_OrderManagement_Load;
        }
        private void UC_OrderManagement_Load(object sender, EventArgs e)
        {
            string checkRole = UserSession.CurrentEmployee.RoleName;
            if (checkRole.Trim() == "Admin")
                btnCancelOrder.Visible = true;
            else btnCancelOrder .Visible = false;
                LoadData();
        }      
        private void FormatGrid()
        {
            if (dgvOrdersM.Columns["OrderID"] != null) dgvOrdersM.Columns["OrderID"].HeaderText = "Order ID";
            if (dgvOrdersM.Columns["OrderDate"] != null) dgvOrdersM.Columns["OrderDate"].HeaderText = "Date";
            if (dgvOrdersM.Columns["CustomerName"] != null) dgvOrdersM.Columns["CustomerName"].HeaderText = "Customer";
            if (dgvOrdersM.Columns["Salesperson"] != null) dgvOrdersM.Columns["Salesperson"].HeaderText = "Salesperson";
            if (dgvOrdersM.Columns["PaymentMethod"] != null) dgvOrdersM.Columns["PaymentMethod"].HeaderText = "Payment";
            if (dgvOrdersM.Columns["GrandTotal"] != null)
            {
                dgvOrdersM.Columns["GrandTotal"].HeaderText = "Total Amount";
                dgvOrdersM.Columns["GrandTotal"].DefaultCellStyle.Format = "N2"; 
            }
            dgvOrdersM.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        // Fill data into TextBox
        private void dgvOrdersM_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvOrdersM.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            try
            {
                txtOrderID.Tag = row.Cells["OrderID"].Value;
                txtOrderID.Text = row.Cells["OrderID"].Value?.ToString();
                txtOrderDate.Text = Convert.ToDateTime(row.Cells["OrderDate"].Value).ToString("dd/MM/yyyy HH:mm");
                txtCustomerName.Text = row.Cells["CustomerName"].Value?.ToString();
                txtSalesperson.Text = row.Cells["Salesperson"].Value?.ToString(); 
                txtTotalAmount.Text = Convert.ToDecimal(row.Cells["GrandTotal"].Value).ToString("N2");
            }
            catch { }
        }
        // button View Details
        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (txtOrderID.Tag == null)
            {
                MessageBox.Show("Please select an order to view details.", "Notice");
                return;
            }
            int orderID = Convert.ToInt32(txtOrderID.Tag);
            DataTable dtDetails = _bll.GetOrderProducts(orderID);            // get details from BLL
            if (dtDetails != null && dtDetails.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Order Details: #{orderID}");
                sb.AppendLine("------------------------------------------------");
                foreach (DataRow row in dtDetails.Rows)
                {
                    string pName = row["ProductName"].ToString();
                    string qty = row["Quantity"].ToString();
                    string price = Convert.ToDecimal(row["UnitPrice"]).ToString("N2");
                    string total = Convert.ToDecimal(row["TotalAmount"]).ToString("N2");
                    sb.AppendLine($"- {pName} | x{qty} | ${price} => ${total}");
                }
                sb.AppendLine("------------------------------------------------");
                MessageBox.Show(sb.ToString(), "Order Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No products found in this order.", "Info");
            }
        }
        // button Cancel Order
        private void btnCancelOrder_Click(object sender, EventArgs e)
        {        
            if (txtOrderID.Tag == null)
            {
                MessageBox.Show("Please select an order to cancel.", "Notice");
                return;
            }
            if (MessageBox.Show("Are you sure you want to cancel (delete) this order? This action cannot be undone.", 
                                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int orderID = Convert.ToInt32(txtOrderID.Tag);
                string msg = _bll.CancelOrder(orderID);
                if (msg == "Success")
                {
                    MessageBox.Show("Order cancelled successfully.", "Info");
                    LoadData();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // button Print Invoice
        private void btnPrintInvoice_Click(object sender, EventArgs e)
        {
            if (txtOrderID.Tag == null)
            {
                MessageBox.Show("Please select an order to print.", "Notice");
                return;
            }
            MessageBox.Show("Printing invoice functionality is coming soon!", "Info");
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
            txtSearchOrderM.Text = "";
        }
        private void txtSearchOrderM_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearchOrderM.Text.Trim());
        }
        private void LoadData(string keyword = "")
        {
            DataTable dt = string.IsNullOrEmpty(keyword) ? _bll.GetOrderList() : _bll.SearchOrder(keyword);
            dgvOrdersM.DataSource = dt;
            FormatGrid();
        }
        private void ClearInputs()
        {
            txtOrderID.Text = "";
            txtOrderDate.Text = "";
            txtCustomerName.Text = "";
            txtSalesperson.Text = "";
            txtTotalAmount.Text = "";
            txtOrderID.Tag = null;
        }
        // off button Cancel Order if no Admin role
        public void SetCancelOrderEnabled(bool enabled)
        {   
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetCancelOrderEnabled(enabled)));
                return;
            }
            btnCancelOrder.Enabled = enabled;
            btnCancelOrder.Visible = enabled;
        }
    }
}