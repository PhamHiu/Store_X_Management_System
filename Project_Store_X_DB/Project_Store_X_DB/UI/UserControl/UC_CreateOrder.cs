using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Project_Store_X_DB.BLL;
using Project_Store_X_DB.DTO; // Get UserSession

namespace Project_Store_X_DB
{
    public partial class UC_CreateOrder : UserControl
    {
        private CreateOrderBLL _bll = new CreateOrderBLL();
        private DataTable _dtCart; // table temp for cart items
        private DataTable _dtProductsCache; // cached product list for searching
        public UC_CreateOrder()
        {
            InitializeComponent();
            this.Load += UC_CreateOrder_Load;
            // Make ComboBox editable and enable autocomplete in constructor setup
            cbCustomer.DropDownStyle = ComboBoxStyle.DropDown;
            cbCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbCustomer.AutoCompleteSource = AutoCompleteSource.CustomSource;        
            InitCartTable();
        }
        private void InitCartTable()
        {
            _dtCart = new DataTable();
            _dtCart.Columns.Add("ProductID", typeof(int));
            _dtCart.Columns.Add("ProductName", typeof(string));
            _dtCart.Columns.Add("Price", typeof(decimal));
            _dtCart.Columns.Add("Quantity", typeof(int));
            _dtCart.Columns.Add("Total", typeof(decimal)); // Price * Qty

            dgvOrders.DataSource = _dtCart; // show in DataGridView
            FormatGrid();
        }
        private void FormatGrid()
        {
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (dgvOrders.Columns["ProductID"] != null) dgvOrders.Columns["ProductID"].Visible = false;
            dgvOrders.Columns["Total"].DefaultCellStyle.Format = "N2";
            dgvOrders.Columns["Price"].DefaultCellStyle.Format = "N2";
            // 1. prevent edit all cells
            foreach (DataGridViewColumn col in dgvOrders.Columns)
            {
                col.ReadOnly = true;
            }
            // 2. only unlock columns Quantity
            if (dgvOrders.Columns["Quantity"] != null)
            {
                dgvOrders.Columns["Quantity"].ReadOnly = false;
                dgvOrders.Columns["Quantity"].DefaultCellStyle.BackColor = Color.LightYellow; // change bg color to indicate editable
            }
        }
        private void UC_CreateOrder_Load(object sender, EventArgs e)
        {
            // Ensure textboxes are empty on load
            txtPhone.Text = string.Empty;
            txtAdress.Text = string.Empty;
            cbCustomer.Text = string.Empty;
            LoadHeaderData();
            LoadProductsToFlowLayout();
            txtDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtSalesperson.Text = UserSession.CurrentEmployee?.EmployeeName; // display salesperson name                       
        }
        private void LoadHeaderData()
        {
            //// Load Customers into ComboBox as DataSource (kept for selection)
            cbCustomer.DataSource = _bll.GetCustomerList();
            cbCustomer.DisplayMember = "CustomerName";
            cbCustomer.ValueMember = "CustomerID";
            cbCustomer.SelectedIndex = -1;
            // Build AutoCompleteCustomSource from customer names (new)
            var ac = new AutoCompleteStringCollection();
            DataTable dtNames = _bll.GetCustomerNames();
            foreach (DataRow r in dtNames.Rows)
            {
                string name = r["CustomerName"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    ac.Add(name);
                }
            }
            cbCustomer.AutoCompleteCustomSource = ac;
            // Load Payment Methods
            comboBox1.DataSource = _bll.GetPaymentList();
            comboBox1.DisplayMember = "MethodName";
            comboBox1.ValueMember = "MethodID";
            comboBox1.SelectedIndex = 0; // set default method index 0
        }
        // do load products into flow layout panel
        private void LoadProductsToFlowLayout()
        {
            flpProducts.Controls.Clear();
            DataTable dtProd = _bll.GetProductList();
            foreach (DataRow row in dtProd.Rows)
            {
                UC_ProductCard card = new UC_ProductCard();
                card.SetData(
                    Convert.ToInt32(row["ProductID"]),
                    row["ProductName"].ToString(),
                    Convert.ToDecimal(row["Price"]),
                    Convert.ToInt32(row["InventoryQuantity"]),
                    row["ImageURL"].ToString()
                );             
                card.Width = 130; // size card
                card.Height = 180; // size card
                card.Margin = new Padding(10);
                // Attach click event to select product
                card.OnSelect += (s, ev) => AddProductToCart(card);
                flpProducts.Controls.Add(card);
            }
        }//--------------------------------------------------------------
        // Logic add product to cart
        private void AddProductToCart(UC_ProductCard product)
        {
            foreach (DataRow row in _dtCart.Rows)
            {
                if (Convert.ToInt32(row["ProductID"]) == product.ProductID)
                {
                    int currentQty = Convert.ToInt32(row["Quantity"]);
                    if (currentQty + 1 > product.Stock)
                    {
                        MessageBox.Show("Not enough stock!", "Warning");
                        return;
                    }
                    row["Quantity"] = currentQty + 1;
                    row["Total"] = (currentQty + 1) * product.Price;
                    CalculateTotals();
                    return;
                }
            }
            // if hasn't product then add new row
            if (product.Stock > 0)
            {
                _dtCart.Rows.Add(product.ProductID, "Product " + product.ProductID, product.Price, 1, product.Price);
                CalculateTotals();
            }
            else
            {
                MessageBox.Show("Out of stock!", "Warning");
            }
        }
        // Calculate subtotal and total
        private void CalculateTotals()
        {
            decimal subtotal = 0;
            foreach (DataRow row in _dtCart.Rows)
            {
                subtotal += Convert.ToDecimal(row["Total"]);
            }

            txtSubtotal.Text = subtotal.ToString("N2");
            txtTotal.Text = subtotal.ToString("N2");

            decimal discount = 0;
            decimal.TryParse(txtDiscount.Text, out discount);

            decimal total = subtotal - discount;            
        }
        private void CbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCustomer.SelectedIndex == -1)
            {
                // If user typed a name but didn't select item, try to find exact match in data source
                string typed = cbCustomer.Text?.Trim();
                if (string.IsNullOrEmpty(typed))
                {
                    txtPhone.Text = string.Empty;
                    txtAdress.Text = string.Empty;
                    return;
                }
                DataTable dt = (DataTable)cbCustomer.DataSource;
                if (dt != null)
                {
                    DataRow[] found = dt.Select($"CustomerName = '{typed.Replace("'", "''")}'");
                    if (found.Length > 0)
                    {
                        txtPhone.Text = found[0]["CustomerPhone"].ToString();
                        txtAdress.Text = found[0]["CustomerAddress"].ToString();
                        cbCustomer.SelectedValue = found[0]["CustomerID"];
                        return;
                    }
                }
                return;
            }
            // Selected from list -> fill details
            DataRowView drv = (DataRowView)cbCustomer.SelectedItem;
            txtPhone.Text = drv["CustomerPhone"].ToString();
            txtAdress.Text = drv["CustomerAddress"].ToString();
        }
        //button checkout click
        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Session expired. Please login again.");
                return;
            }
            int customerID = (cbCustomer.SelectedValue != null) ? Convert.ToInt32(cbCustomer.SelectedValue) : 0;
            int methodID = (comboBox1.SelectedValue != null) ? Convert.ToInt32(comboBox1.SelectedValue) : 0;
            int employeeID = UserSession.CurrentEmployee.EmployeeID;
            // If customer not selected but user typed a name, create new customer first
            if (customerID <= 0)
            {
                string typedName = cbCustomer.Text?.Trim();
                if (string.IsNullOrEmpty(typedName))
                {
                    MessageBox.Show("Please select or type a Customer name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    customerID = _bll.AddCustomer(typedName, txtPhone.Text?.Trim(), txtAdress.Text?.Trim());
                    // Refresh customer list and autocomplete so the new customer becomes selectable
                    LoadHeaderData();
                    // Set the combo box to the new customer
                    cbCustomer.SelectedValue = customerID;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            string result = _bll.CheckoutOrder(customerID, employeeID, methodID, _dtCart);
            if (result == "Success")
            {
                MessageBox.Show("Order placed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cbCustomer.SelectedIndex = -1;
                cbCustomer.Text = string.Empty;
                txtPhone.Text = string.Empty;
                txtAdress.Text = string.Empty;
                _dtCart.Rows.Clear();
                CalculateTotals();
                LoadProductsToFlowLayout();                
            }
            else
            {
                MessageBox.Show(result, "Transaction Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // button cancel click
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear cart?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                cbCustomer.SelectedIndex = -1;
                cbCustomer.Text = string.Empty;
                txtPhone.Text = string.Empty;
                txtAdress.Text = string.Empty;
                _dtCart.Rows.Clear();
                CalculateTotals();
            }
        }
        private void DgvOrders_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // only handle Quantity column changes
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "Quantity")
            {
                DataRow row = _dtCart.Rows[e.RowIndex];
                int newQty = 0;
                decimal price = Convert.ToDecimal(row["Price"]);
                var cellValue = dgvOrders.Rows[e.RowIndex].Cells["Quantity"].Value;
                if (int.TryParse(cellValue?.ToString(), out newQty) && newQty > 0)
                {
                    // Update to DataTable (to synchronize)
                    row["Quantity"] = newQty;
                    row["Total"] = newQty * price;
                }
                else
                {                   
                    MessageBox.Show("Số lượng phải lớn hơn 0", "Cảnh báo"); // If enter <=0  -> Reset  1 or errorq
                    row["Quantity"] = 1;
                    row["Total"] = price;
                    dgvOrders.Refresh();
                }
                CalculateTotals();
            }
        }        // 2. event open when cell is edited (for ComboBox or Checkbox cells)
        private void DgvOrders_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvOrders.IsCurrentCellDirty)
            {
                dgvOrders.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        // 3. cathch data error event (for invalid input)
        private void DgvOrders_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (dgvOrders.Columns[e.ColumnIndex].Name == "Quantity")
            {
                MessageBox.Show("Please enter integers only!", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = false; // not cancel edit to allow correction
            }
        }
        // TextChanged handler for the search textbox
        private void TxtSearchCreatOrder_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string keyword = txtSearchCreatOrder.Text?.Trim();
                if (string.IsNullOrEmpty(keyword))
                {
                    // show all
                    RenderProducts(_dtProductsCache);
                    return;
                }
                if (_dtProductsCache == null)
                {
                    _dtProductsCache = _bll.GetProductList();// fallback: reload and search server-side if needed
                    if (_dtProductsCache == null) return;
                }
                string esc = keyword.Replace("'", "''"); // escape single quotes for DataView RowFilter
                // If keyword is numeric try to include ProductID exact match
                int id;
                string rowFilter;
                if (int.TryParse(keyword, out id))
                {
                    rowFilter = $"Convert(ProductID, 'System.String') LIKE '%{esc}%' OR ProductName LIKE '%{esc}%'";
                }
                else
                {
                    rowFilter = $"ProductName LIKE '%{esc}%'";
                }
                var dv = new DataView(_dtProductsCache);
                dv.RowFilter = rowFilter;
                RenderProducts(dv.ToTable());  // Render only matching rows
            }
            catch
            {
                // ignore filter errors to avoid breaking UX; optionally log
            }
        }
            private void RenderProducts(DataTable dtProd)
        {
            flpProducts.Controls.Clear();
            if (dtProd == null || dtProd.Rows.Count == 0) return;

            foreach (DataRow row in dtProd.Rows)
            {
                UC_ProductCard card = new UC_ProductCard();
                card.SetData(
                    Convert.ToInt32(row["ProductID"]),
                    row["ProductName"].ToString(),
                    Convert.ToDecimal(row["Price"]),
                    Convert.ToInt32(row["InventoryQuantity"]),
                    row["ImageURL"].ToString()
                );
                card.Width = 130;
                card.Height = 180;
                card.Margin = new Padding(10);
                card.OnSelect += (s, ev) => AddProductToCart(card);
                flpProducts.Controls.Add(card);
            }
        }
    }
}