using System;
using System.Data;
using System.Windows.Forms;
using Project_Store_X_DB.BLL;

namespace Project_Store_X_DB
{
    public partial class UC_Employees : UserControl
    {
        private EmployeeBLL _bll = new EmployeeBLL();        
        private int _selectedAccountID = 0; // save selected AccountID for update
        public UC_Employees()
        {
            InitializeComponent();
            this.Load += UC_Employees_Load;
        }
        private void UC_Employees_Load(object sender, EventArgs e)
        {
            LoadRoles();
            LoadData();
        }
        private void LoadRoles()
        {
            DataTable dt = _bll.GetRoleList();
            cbRole.DataSource = dt;
            cbRole.DisplayMember = "RoleName";
            cbRole.ValueMember = "RoleID";
            cbRole.SelectedIndex = -1;
        }
        private void LoadData()
        {
            DataTable dt = _bll.GetEmployeeList();
            dgvEmployees.DataSource = dt;
            FormatGrid();
        }
        // button add
        private void btnCreate_Click(object sender, EventArgs e)
        {
            int roleID = (cbRole.SelectedValue != null) ? Convert.ToInt32(cbRole.SelectedValue) : 0;
            // Bill will hash password 
            string msg = _bll.AddEmployee(
                txtEmployeeName.Text,
                txtPosition.Text,
                roleID,
                txtUsername.Text,
                txtPasswordHash.Text
            );
            if (msg == "Success")
            {
                MessageBox.Show("Employee created successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // button update
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtEmployeeID.Tag == null)
            {
                MessageBox.Show("Please select an employee to update.", "Notice");
                return;
            }

            int empID = Convert.ToInt32(txtEmployeeID.Tag);
            int roleID = (cbRole.SelectedValue != null) ? Convert.ToInt32(cbRole.SelectedValue) : 0;

            // if txtPasswordHash is empty,that mean not change password
            string msg = _bll.EditEmployee(
                empID,
                _selectedAccountID,
                txtEmployeeName.Text,
                txtPosition.Text,
                roleID,
                txtPasswordHash.Text
            );
            if (msg == "Success")
            {
                MessageBox.Show("Updated successfully!", "Info");
                LoadData();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // button delete
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtEmployeeID.Tag == null)
            {
                MessageBox.Show("Please select an employee to delete.", "Notice");
                return;
            }
            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(txtEmployeeID.Tag);
                string msg = _bll.RemoveEmployee(id);
                if (msg == "Success")
                {
                    MessageBox.Show("Deleted successfully.", "Info");
                    LoadData();
                    ClearInputs();
                }
                else
                {
                    // throw error delete admin (ID 1)
                    MessageBox.Show(msg, "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // button refresh
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
        }
        private void DgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // check cell click error
            DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            try
            {
                // 3. preven null data
                if (row.Cells["EmployeeID"].Value == null || row.Cells["EmployeeID"].Value == DBNull.Value) return;
                int empID = Convert.ToInt32(row.Cells["EmployeeID"].Value);
                txtEmployeeID.Tag = empID;
                // check null for ID when convert
                if (row.Cells["AccountID"].Value != null && row.Cells["AccountID"].Value != DBNull.Value)
                {
                    _selectedAccountID = Convert.ToInt32(row.Cells["AccountID"].Value);
                }
                // Map data
                txtEmployeeID.Text = row.Cells["EmployeeID"].Value?.ToString();
                txtEmployeeName.Text = row.Cells["EmployeeName"].Value?.ToString();
                txtPosition.Text = row.Cells["Position"].Value?.ToString();
                txtUsername.Text = row.Cells["Username"].Value?.ToString();
                if (row.Cells["RoleID"].Value != null)
                {
                    cbRole.SelectedValue = row.Cells["RoleID"].Value;
                }
                txtPasswordHash.Text = "";

                // Logic protect Admin ID 1
                if (empID == 1)
                {
                    btnDelete.Enabled = false;
                    cbRole.Enabled = false;
                    btnDelete.BackColor = System.Drawing.Color.Gray;
                }
                else
                {
                    btnDelete.Enabled = true;
                    cbRole.Enabled = true;
                    btnDelete.BackColor = System.Drawing.Color.Firebrick;
                }
            }
            catch (Exception)
            {

                return;
            }
        }
        private void ClearInputs()
        {
            txtEmployeeID.Text = "";
            txtEmployeeName.Text = "";
            txtPosition.Text = "";
            txtUsername.Text = "";
            txtPasswordHash.Text = "";
            cbRole.SelectedIndex = -1;
            txtEmployeeID.Tag = null;
            _selectedAccountID = 0;
            btnDelete.Enabled = true;
            cbRole.Enabled = true;
            btnDelete.BackColor = System.Drawing.Color.Firebrick;
        }
        private void FormatGrid()
        {
            if (dgvEmployees.Columns["EmployeeID"] != null) dgvEmployees.Columns["EmployeeID"].HeaderText = "ID";
            if (dgvEmployees.Columns["EmployeeName"] != null) dgvEmployees.Columns["EmployeeName"].HeaderText = "Full Name";
            if (dgvEmployees.Columns["RoleName"] != null) dgvEmployees.Columns["RoleName"].HeaderText = "Role";
            if (dgvEmployees.Columns["RoleID"] != null) dgvEmployees.Columns["RoleID"].Visible = false;
            if (dgvEmployees.Columns["AccountID"] != null) dgvEmployees.Columns["AccountID"].Visible = false;
            dgvEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

    }
}