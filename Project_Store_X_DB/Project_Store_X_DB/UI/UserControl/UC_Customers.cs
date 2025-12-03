using Project_Store_X_DB.BLL;
using System;
using System.Data;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Project_Store_X_DB
{
    public partial class UC_Customers : UserControl
    {
        private CustomerBLL _bll = new CustomerBLL();

        public UC_Customers()
        {
            InitializeComponent();         
            this.Load += UC_Customers_Load;             
        }
        private void UC_Customers_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData(string keyword = "")
        {
            DataTable dt;
            if (string.IsNullOrEmpty(keyword))
            {
                dt = _bll.GetCustomerList();
            }
            else
            {
                dt = _bll.SearchCustomer(keyword);
            }
            dgvCustomers.DataSource = dt;
            FormatGrid();
        }
        private void FormatGrid()
        {
            if (dgvCustomers.Columns["CustomerID"] != null)
                dgvCustomers.Columns["CustomerID"].HeaderText = "ID";
            if (dgvCustomers.Columns["CustomerName"] != null)
                dgvCustomers.Columns["CustomerName"].HeaderText = "Full Name";
            if (dgvCustomers.Columns["CustomerPhone"] != null)
                dgvCustomers.Columns["CustomerPhone"].HeaderText = "Phone Number";
            if (dgvCustomers.Columns["CustomerAddress"] != null)
                dgvCustomers.Columns["CustomerAddress"].HeaderText = "Address";
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        // Create button
        private void btnCreate_Click(object sender, EventArgs e)
        {
            string msg = _bll.AddCustomer(txtCustomerName.Text, txtPhoneNumber.Text, txtCustomerAddress.Text);

            if (msg == "Success")
            {
                MessageBox.Show("Customer added successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // Update button
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtCustomerName.Tag == null)
            {
                MessageBox.Show("Please select a customer to update.", "Notice");
                return;
            }
            int id = Convert.ToInt32(txtCustomerName.Tag);
            string msg = _bll.EditCustomer(id, txtCustomerName.Text, txtPhoneNumber.Text, txtCustomerAddress.Text);
            if (msg == "Success")
            {
                MessageBox.Show("Updated successfully!", "Info");
                LoadData();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(msg, "Error");
            }
        }

        // Delete button
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtCustomerName.Tag == null)
            {
                MessageBox.Show("Please select a customer to delete.", "Notice");
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this customer?", "Confirm", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(txtCustomerName.Tag);
                string msg = _bll.RemoveCustomer(id);

                if (msg == "Success")
                {
                    MessageBox.Show("Deleted successfully.", "Info");
                    LoadData();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // button Refresh
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
            txtSearchCustomerM.Text = "";
        }
        // click cell in datagridview
        private void DgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];// Save Id to Tag
                txtCustomerName.Tag = row.Cells["CustomerID"].Value;
                // Map data withTextBox
                txtCustomerName.Text = row.Cells["CustomerName"].Value.ToString();
                txtPhoneNumber.Text = row.Cells["CustomerPhone"].Value.ToString();
                txtCustomerAddress.Text = row.Cells["CustomerAddress"].Value.ToString();              
                if (txtCustomerID != null) txtCustomerID.Text = row.Cells["CustomerID"].Value.ToString();// Display on txtCustomerID 
            }
        }
        // Search box text changed
        private void txtSearchCustomerM_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearchCustomerM.Text.Trim());
        }
        private void ClearInputs()
        {
            txtCustomerName.Text = "";
            txtPhoneNumber.Text = "";
            txtCustomerAddress.Text = "";
            if (txtCustomerID != null) txtCustomerID.Text = "";
            txtCustomerName.Tag = null;
        }
        private void BtnViewHistory_Click(object sender, EventArgs e)
        {
            if (txtCustomerName.Tag == null)
            {
                MessageBox.Show("Please select a customer to view purchase history.", "Notice",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int customerID;
            if (!int.TryParse(txtCustomerName.Tag.ToString(), out customerID) || customerID <= 0)
            {
                MessageBox.Show("Invalid customer selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DataTable dtHistory = _bll.GetCustomerOrderHistory(customerID);
            if (dtHistory == null || dtHistory.Rows.Count == 0)
            {
                MessageBox.Show("No purchase history found for this customer.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Show history in the same grid (dgvCustomers) as requested
            dgvCustomers.DataSource = dtHistory;
            // Optional: adjust headers/formatting for history view
            if (dgvCustomers.Columns["OrderID"] != null) dgvCustomers.Columns["OrderID"].HeaderText = "Order ID";
            if (dgvCustomers.Columns["OrderDate"] != null) { dgvCustomers.Columns["OrderDate"].HeaderText = "Date";
                dgvCustomers.Columns["OrderDate"].DefaultCellStyle.Format = "g"; }
            if (dgvCustomers.Columns["ProductName"] != null) dgvCustomers.Columns["ProductName"].HeaderText = "Product";
            if (dgvCustomers.Columns["Quantity"] != null) dgvCustomers.Columns["Quantity"].HeaderText = "Qty";
            if (dgvCustomers.Columns["UnitPrice"] != null) { dgvCustomers.Columns["UnitPrice"].HeaderText = "Unit Price"; 
                dgvCustomers.Columns["UnitPrice"].DefaultCellStyle.Format = "N2"; }
            if (dgvCustomers.Columns["TotalAmount"] != null) { dgvCustomers.Columns["TotalAmount"].HeaderText = "Line Total"; 
                dgvCustomers.Columns["TotalAmount"].DefaultCellStyle.Format = "N2"; }
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}