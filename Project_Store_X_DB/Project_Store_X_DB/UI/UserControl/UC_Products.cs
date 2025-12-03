using Project_Store_X_DB.BLL;
using Project_Store_X_DB.DTO;
using System;
using System.Data;
using System.Drawing; // Dùng cho Image
using System.IO;      // Dùng để check file tồn tại
using System.Text;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Project_Store_X_DB
{
    public partial class UC_Products : UserControl
    {
        private ProductBLL _bll = new ProductBLL();
        private string _currentImageURL = ""; // save current image path
        public UC_Products()
        {
            InitializeComponent();
            this.Load += UC_Products_Load;
            picProduct.Cursor = Cursors.Hand; // change cursor to hand
            picProduct.SizeMode = PictureBoxSizeMode.Zoom; // zoom size picture box    
        }
        private void UC_Products_Load(object sender, EventArgs e)
        {
            string checkRole = UserSession.CurrentEmployee.RoleName;

            if (checkRole.Trim() == "Sale Staff")
            {
                btnCreate.Visible = false;
                btnUpdate.Visible = false;
                btnDelete.Visible = false;
                btnExportReportProduct.Visible = false;
            }
            else
            {
                
            }
            LoadComboBoxes();
            LoadData();
            
        }
        private void LoadComboBoxes()
        {
            // Load Categories
            cbCategoryID.DataSource = _bll.GetCategoryList();
            cbCategoryID.DisplayMember = "CategoryName";
            cbCategoryID.ValueMember = "CategoryID";
            cbCategoryID.SelectedIndex = -1;
            // Load Suppliers
            cbSupplierID.DataSource = _bll.GetSupplierList();
            cbSupplierID.DisplayMember = "SupplierName";
            cbSupplierID.ValueMember = "SupplierID";
            cbSupplierID.SelectedIndex = -1;
        }    
        // button Create
        private void btnCreate_Click(object sender, EventArgs e)
        {
            int catID = (cbCategoryID.SelectedValue != null) ? Convert.ToInt32(cbCategoryID.SelectedValue) : 0;
            int supID = (cbSupplierID.SelectedValue != null) ? Convert.ToInt32(cbSupplierID.SelectedValue) : 0;

            string msg = _bll.AddProduct(
                txtProductName.Text,
                txtProductDes.Text,
                _currentImageURL,
                txtPrice.Text,
                txtInventoryQuantity.Text,
                catID,
                supID
            );
            if (msg == "Success")
            {
                MessageBox.Show("Product added successfully!", "Info");
                LoadData();
                ClearInputs();
            }
            else MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }    
        // button Update
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtProductID.Tag == null)
            {
                MessageBox.Show("Please select a product first.");
                return;
            }

            int id = Convert.ToInt32(txtProductID.Tag);
            int catID = Convert.ToInt32(cbCategoryID.SelectedValue);
            int supID = Convert.ToInt32(cbSupplierID.SelectedValue);

            string msg = _bll.EditProduct(
                id,
                txtProductName.Text,
                txtProductDes.Text,
                _currentImageURL,
                txtPrice.Text,
                txtInventoryQuantity.Text,
                catID,
                supID
            );
            if (msg == "Success")
            {
                MessageBox.Show("Updated successfully!");
                LoadData();
                ClearInputs();
            }
            else MessageBox.Show(msg);
        }
        //button Delete
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtProductID.Tag == null)
            {
                MessageBox.Show("Please select a product first.");
                return;
            }
            if (MessageBox.Show("Confirm delete?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(txtProductID.Tag);
                string msg = _bll.RemoveProduct(id);

                if (msg == "Success")
                {
                    MessageBox.Show("Deleted.");
                    LoadData();
                    ClearInputs();
                }
                else MessageBox.Show(msg);
            }
        }
        //Button Refresh
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
        }
        private void LoadData(string keyword = "")
        {
            DataTable dt = string.IsNullOrEmpty(keyword) ? _bll.GetProductList() : _bll.SearchProduct(keyword);
            dgvProducts.DataSource = dt;
            FormatGrid();
        }
        private void ClearInputs()
        {
            txtProductID.Text = "";
            txtProductName.Text = "";
            txtProductDes.Text = "";
            txtPrice.Text = "";
            txtInventoryQuantity.Text = "";
            cbCategoryID.SelectedIndex = -1;
            cbSupplierID.SelectedIndex = -1;
            picProduct.Image = null;
            _currentImageURL = "";
            txtProductID.Tag = null;
        }
       
        // choose image
        private void PicProduct_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _currentImageURL = ofd.FileName; // save path
                picProduct.Image = Image.FromFile(_currentImageURL); // call image
            }
        }
      
        // Search box text changed
        private void txtSearchProducts_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearchProducts.Text.Trim());
        }

        
        // button Export Report Product Inventory to CSV
        private void BtnExportReportProduct_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime snapshot = DateTime.Today.AddHours(2); // 2:00 AM snapshot
                DataTable dt = _bll.GetProductInventoryReport(snapshot);

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No product data available.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Excel (CSV)|*.csv|All files|*.*",
                    FileName = $"ProductInventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                })
                {
                    if (sfd.ShowDialog() != DialogResult.OK) return;

                    // Build CSV with header
                    var sb = new StringBuilder();
                    sb.AppendLine("ProductID,ProductName,Category,Supplier,BeginningInventory,EndingInventory,Note");

                    foreach (DataRow r in dt.Rows)
                    {
                        string productId = r["ProductID"] != DBNull.Value ? r["ProductID"].ToString() : "0";
                        string productName = r.Table.Columns.Contains("ProductName") && r["ProductName"] != DBNull.Value ? r["ProductName"].ToString() : "";
                        string category = r.Table.Columns.Contains("CategoryName") && r["CategoryName"] != DBNull.Value ? r["CategoryName"].ToString() : "";
                        string supplier = r.Table.Columns.Contains("SupplierName") && r["SupplierName"] != DBNull.Value ? r["SupplierName"].ToString() : "";
                        int beginning = r.Table.Columns.Contains("BeginningInventory") && r["BeginningInventory"] != DBNull.Value ? Convert.ToInt32(r["BeginningInventory"]) : 0;
                        int ending = r.Table.Columns.Contains("EndingInventory") && r["EndingInventory"] != DBNull.Value ? Convert.ToInt32(r["EndingInventory"]) : 0;
                        string note = ending < 5 ? "Low stock" : "Pass";

                        // CSV safe quoting: double quotes inside field, wrap field in quotes if contains comma/quote/newline
                        string CsvSafe(string s)
                        {
                            if (string.IsNullOrEmpty(s)) return "";
                            string escaped = s.Replace("\"", "\"\"");
                            if (escaped.IndexOfAny(new char[] { ',', '"', '\r', '\n' }) >= 0)
                                return $"\"{escaped}\"";
                            return escaped;
                        }

                        sb.AppendLine(string.Join(",",
                            CsvSafe(productId),
                            CsvSafe(productName),
                            CsvSafe(category),
                            CsvSafe(supplier),
                            CsvSafe(beginning.ToString()),
                            CsvSafe(ending.ToString()),
                            CsvSafe(note)
                        ));
                    }

                    // Write with UTF8 BOM so Excel recognizes UTF-8
                    var utf8WithBom = new System.Text.UTF8Encoding(true);
                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), utf8WithBom);

                    MessageBox.Show("Export to CSV successful. You can open the file in Excel.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void FormatGrid()
        {
            // hide columns
            if (dgvProducts.Columns["CategoryID"] != null) dgvProducts.Columns["CategoryID"].Visible = false;
            if (dgvProducts.Columns["SupplierID"] != null) dgvProducts.Columns["SupplierID"].Visible = false;
            if (dgvProducts.Columns["ImageURL"] != null) dgvProducts.Columns["ImageURL"].Visible = false;
            // change header text
            if (dgvProducts.Columns["ProductID"] != null) dgvProducts.Columns["ProductID"].HeaderText = "ID";
            if (dgvProducts.Columns["ProductName"] != null) dgvProducts.Columns["ProductName"].HeaderText = "Name";
            if (dgvProducts.Columns["Price"] != null) dgvProducts.Columns["Price"].HeaderText = "Price";
            if (dgvProducts.Columns["InventoryQuantity"] != null) dgvProducts.Columns["InventoryQuantity"].HeaderText = "Stock";
            if (dgvProducts.Columns["CategoryName"] != null) dgvProducts.Columns["CategoryName"].HeaderText = "Category";
            if (dgvProducts.Columns["SupplierName"] != null) dgvProducts.Columns["SupplierName"].HeaderText = "Supplier";
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1.Preven error click Header/NewRow/Null
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            if (row.Cells["ProductID"].Value == null || row.Cells["ProductID"].Value == DBNull.Value) return;
            try
            {
                // 2. Map data
                txtProductID.Tag = row.Cells["ProductID"].Value;
                txtProductID.Text = row.Cells["ProductID"].Value.ToString();
                txtProductName.Text = row.Cells["ProductName"].Value.ToString();
                txtProductDes.Text = row.Cells["ProductDesc"].Value?.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
                txtInventoryQuantity.Text = row.Cells["InventoryQuantity"].Value.ToString();

                // ComboBox
                if (row.Cells["CategoryID"].Value != DBNull.Value)
                    cbCategoryID.SelectedValue = row.Cells["CategoryID"].Value;

                if (row.Cells["SupplierID"].Value != DBNull.Value)
                    cbSupplierID.SelectedValue = row.Cells["SupplierID"].Value;

                // 3. display image
                string imgPath = row.Cells["ImageURL"].Value?.ToString();
                _currentImageURL = imgPath; // save current image path

                if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                {
                    picProduct.Image = Image.FromFile(imgPath);
                }
                else
                {
                    picProduct.Image = null; // null picture
                }
            }
            catch
            {
                // pass exception
            }
        }
        //----------------------------------------------------------
        // Hàm hỗ trợ: Copy ảnh vào thư mục bin/Debug/Images --> triển khai sau
        //----------------------------------------------------------
        //private string SaveImageToAppFolder(string sourcePath)
        //{
        //    try
        //    {
        //        // 1. Nếu chưa chọn ảnh hoặc đường dẫn rỗng -> Trả về rỗng
        //        if (string.IsNullOrEmpty(sourcePath)) return "";

        //        // 2. Kiểm tra xem chuỗi này có phải là đường dẫn file không
        //        // (Nếu đang là tên file ngắn gọn lấy từ DB thì không cần copy lại)
        //        if (!Path.IsPathRooted(sourcePath)) return sourcePath;

        //        // 3. Tạo thư mục Images nếu chưa có
        //        string projectPath = Application.StartupPath; // Đường dẫn đến bin/Debug
        //        string imageFolder = Path.Combine(projectPath, "Images");

        //        if (!Directory.Exists(imageFolder))
        //        {
        //            Directory.CreateDirectory(imageFolder);
        //        }

        //        // 4. Tạo tên file đích
        //        string fileName = Path.GetFileName(sourcePath); // Lấy "anh.jpg" từ "C:\...\anh.jpg"

        //        // Mẹo: Thêm thời gian vào tên file để tránh trùng tên (vd: anh_20231025.jpg)
        //        // string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName); 
        //        // Nhưng để đơn giản, ta giữ nguyên tên file gốc:
        //        string destPath = Path.Combine(imageFolder, fileName);

        //        // 5. Copy file (Ghi đè nếu đã tồn tại)
        //        File.Copy(sourcePath, destPath, true);

        //        // 6. Trả về tên file để lưu vào DB
        //        return fileName;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Lỗi khi lưu ảnh: " + ex.Message);
        //        return "";
        //    }
        //}
    }
}