using Project_Store_X_DB.BLL;
using System;
using System.Data;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Project_Store_X_DB
{
    public partial class UC_PaymentMethod : UserControl
    {
        private PaymentMethodBLL _bll = new PaymentMethodBLL();
        public UC_PaymentMethod()
        {
            InitializeComponent();
            this.Load += UC_PaymentMethod_Load;
        }
        private void UC_PaymentMethod_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData(string keyword = "")
        {
            DataTable dt = string.IsNullOrEmpty(keyword) ? _bll.GetPaymentMethodList() : _bll.SearchPaymentMethod(keyword);
            dgvPaymentMethod.DataSource = dt;
            FormatGrid();
        }
        private void FormatGrid()
        {
            if (dgvPaymentMethod.Columns["MethodID"] != null)
                dgvPaymentMethod.Columns["MethodID"].HeaderText = "ID";

            if (dgvPaymentMethod.Columns["MethodName"] != null)
                dgvPaymentMethod.Columns["MethodName"].HeaderText = "Payment Method Name";

            dgvPaymentMethod.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        // button Create
        private void btnCreate_Click(object sender, EventArgs e)
        {
            string msg = _bll.AddPaymentMethod(txtMethodName.Text);

            if (msg == "Success")
            {
                MessageBox.Show("Added successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearInputs();
            }
            else MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        // button Update
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtMethodName.Tag == null)
            {
                MessageBox.Show("Please select a method first.", "Notice");
                return;
            }
            int id = Convert.ToInt32(txtMethodName.Tag);
            string msg = _bll.EditPaymentMethod(id, txtMethodName.Text);
            if (msg == "Success")
            {
                MessageBox.Show("Updated successfully!", "Info");
                LoadData();
                ClearInputs();
            }
            else MessageBox.Show(msg, "Error");
        }
        // button Delete
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtMethodName.Tag == null)
            {
                MessageBox.Show("Please select a method to delete.", "Notice");
                return;
            }

            if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(txtMethodName.Tag);
                string msg = _bll.RemovePaymentMethod(id);

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
            txtSearchPaymentMethod.Text = "";
        }
        // search Text Changed
        private void txtSearchPaymentMethod_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearchPaymentMethod.Text.Trim());
        }

        // Click Grid
        private void DgvPaymentMethod_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // prevent error when click empty row
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvPaymentMethod.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            if (row.Cells["MethodID"].Value == null || row.Cells["MethodID"].Value == DBNull.Value) return;
            try
            {
                txtMethodName.Tag = row.Cells["MethodID"].Value;
                txtMethodName.Text = row.Cells["MethodName"].Value.ToString();

                if (txtMethodID != null) txtMethodID.Text = row.Cells["MethodID"].Value.ToString();
            }
            catch { }
        }
        private void ClearInputs()
        {
            txtMethodName.Text = "";
            if (txtMethodID != null) txtMethodID.Text = "";
            txtMethodName.Tag = null;
        }
    }
}