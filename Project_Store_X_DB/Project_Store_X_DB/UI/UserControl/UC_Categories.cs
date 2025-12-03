using System;
using System.Data;
using System.Windows.Forms;
using Project_Store_X_DB.BLL;

namespace Project_Store_X_DB
{
    public partial class UC_Categories : UserControl
    {
        private CategoryBLL _bll = new CategoryBLL();
        public UC_Categories()
        {
            InitializeComponent();
            this.Load += UC_Categories_Load; 
        }
        private void UC_Categories_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData(string keyword = "")
        {
            DataTable dt;
            if (string.IsNullOrEmpty(keyword))
            {
                dt = _bll.GetAllCategories();
            }
            else
            {
                dt = _bll.FindCategory(keyword);
            }
            dgvCategories.DataSource = dt;
            // set layout header text
            if (dgvCategories.Columns["CategoryID"] != null)
                dgvCategories.Columns["CategoryID"].HeaderText = "ID";
            if (dgvCategories.Columns["CategoryName"] != null)
                dgvCategories.Columns["CategoryName"].HeaderText = "Category Name";
            if (dgvCategories.Columns["CategoryDesc"] != null)
                dgvCategories.Columns["CategoryDesc"].HeaderText = "Description";
            dgvCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        // button Create Click event
        private void btnCreate_Click(object sender, EventArgs e)
        {
            string msg = _bll.AddCategory(txtCategoryName.Text, txtCategoryDesc.Text);

            if (msg == "Success")
            {
                MessageBox.Show("Category added successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();   // Load lại bảng
                ClearInputs(); // Xóa trắng ô nhập
            }
            else
            {
                MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // button Update Click event
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtCategoryName.Tag == null) // check row chosen
            {
                MessageBox.Show("Please select a category from the list first.", "Notice");
                return;
            }
            int id = Convert.ToInt32(txtCategoryName.Tag);
            string msg = _bll.EditCategory(id, txtCategoryName.Text, txtCategoryDesc.Text);
            if (msg == "Success")
            {
                MessageBox.Show("Category updated successfully!", "Info");
                LoadData();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(msg, "Error");
            }
        }
        // button Delete Click event
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtCategoryName.Tag == null)
            {
                MessageBox.Show("Please select a category to delete.", "Notice");
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this category?", "Confirm", 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(txtCategoryName.Tag);
                string msg = _bll.RemoveCategory(id);

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
        // search TextBox TextChanged event
        private void txtSearchCategories_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearchCategories.Text.Trim();
            LoadData(keyword);
        }
        // button Refresh Click event
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
            txtSearchCategories.Text = ""; 
        }
        // get data from datagridview to textbox
        private void DgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) //Check click header
            {
                DataGridViewRow row = dgvCategories.Rows[e.RowIndex];                
                txtCategoryName.Tag = row.Cells["CategoryID"].Value;
                txtCategoryName.Text = row.Cells["CategoryName"].Value.ToString();
                txtCategoryDesc.Text = row.Cells["CategoryDesc"].Value.ToString();
            }
        }     
        private void ClearInputs()
        {
            txtCategoryName.Text = "";
            txtCategoryDesc.Text = "";
            txtCategoryName.Tag = null;
        }
    }
}