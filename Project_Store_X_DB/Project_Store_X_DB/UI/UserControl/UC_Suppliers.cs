using System;
using System.Data;
using System.Windows.Forms;
using Project_Store_X_DB.BLL;

namespace Project_Store_X_DB
{
    public partial class UC_Suppliers : UserControl
    {
        private SupplierBLL _bll = new SupplierBLL();      
        public UC_Suppliers()
        {
            InitializeComponent();
            this.Load += UC_Suppliers_Load;
        }
        private void UC_Suppliers_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData(string keyword = "")
        {
            DataTable dt = string.IsNullOrEmpty(keyword) ? _bll.GetSupplierList() : _bll.SearchSupplier(keyword);
            dgvSuppliers.DataSource = dt;
            FormatGrid();
        }
        private void FormatGrid()
        {
            if (dgvSuppliers.Columns["SupplierID"] != null) dgvSuppliers.Columns["SupplierID"].HeaderText = "ID";
            if (dgvSuppliers.Columns["SupplierName"] != null) dgvSuppliers.Columns["SupplierName"].HeaderText = "Company Name";
            if (dgvSuppliers.Columns["SupplierEmail"] != null) dgvSuppliers.Columns["SupplierEmail"].HeaderText = "Email";
            if (dgvSuppliers.Columns["SupplierPhone"] != null) dgvSuppliers.Columns["SupplierPhone"].HeaderText = "Phone";
            if (dgvSuppliers.Columns["SupplierAddress"] != null) dgvSuppliers.Columns["SupplierAddress"].HeaderText = "Address";
            dgvSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        // button Create Click event
        private void btnCreate_Click(object sender, EventArgs e)
        {
            string msg = _bll.AddSupplier(
                txtSupplierName.Text,
                txtSupplierEmail.Text,
                txtPhoneNumber.Text, 
                txtSupplierAddress.Text
            );
            if (msg == "Success")
            {
                MessageBox.Show("Supplier added successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearInputs();
            }
            else MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        // button Update Click event
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtSupplierName.Tag == null)
            {
                MessageBox.Show("Please select a supplier first.", "Notice");
                return;
            }
            int id = Convert.ToInt32(txtSupplierName.Tag);
            string msg = _bll.EditSupplier(
                id,
                txtSupplierName.Text,
                txtSupplierEmail.Text,
                txtPhoneNumber.Text,
                txtSupplierAddress.Text
            );
            if (msg == "Success")
            {
                MessageBox.Show("Updated successfully!", "Info");
                LoadData();
                ClearInputs();
            }
            else MessageBox.Show(msg, "Error");
        }
        // button Delete Click event
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtSupplierName.Tag == null)
            {
                MessageBox.Show("Please select a supplier first.", "Notice");
                return;
            }
            if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(txtSupplierName.Tag);
                string msg = _bll.RemoveSupplier(id);
                if (msg == "Success")
                {
                    MessageBox.Show("Deleted successfully.", "Info");
                    LoadData();
                    ClearInputs();
                }
                else MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
            txtSearchSuppliers.Text = "";
        }
        //search suppliers on text changed
        private void txtSearchSuppliers_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearchSuppliers.Text.Trim());
        }
        // fill data to dgv on cell click
        private void DgvSuppliers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvSuppliers.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            if (row.Cells["SupplierID"].Value == null || row.Cells["SupplierID"].Value == DBNull.Value) return;
            try
            {
                txtSupplierName.Tag = row.Cells["SupplierID"].Value;
                // Map data to textboxes
                txtSupplierName.Text = row.Cells["SupplierName"].Value.ToString();
                txtSupplierEmail.Text = row.Cells["SupplierEmail"].Value.ToString();
                txtPhoneNumber.Text = row.Cells["SupplierPhone"].Value.ToString();
                txtSupplierAddress.Text = row.Cells["SupplierAddress"].Value.ToString();
                if (txtSupplierID != null) txtSupplierID.Text = row.Cells["SupplierID"].Value.ToString();
            }
            catch
            {
                // pass exception
            }
        }
        private void ClearInputs()
        {
            txtSupplierName.Text = "";
            txtSupplierEmail.Text = "";
            txtPhoneNumber.Text = "";
            txtSupplierAddress.Text = "";
            if (txtSupplierID != null) txtSupplierID.Text = "";
            txtSupplierName.Tag = null;
        }
    }
}