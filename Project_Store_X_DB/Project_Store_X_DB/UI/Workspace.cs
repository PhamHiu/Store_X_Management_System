using Project_Store_X_DB.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using DocumentFormat.OpenXml.Drawing;// khai báo dùng kéo form borderstyle none

namespace Project_Store_X_DB
{
    public partial class Workspace : Form
    {

        // Borderstyle = none nhưng vẫn kéo form được
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2; 

        // 🔹 Khai báo các UserControl một lần (để không bị reload lại mỗi khi click)
        UC_CreateOrder ucCreateOrder = new UC_CreateOrder();
        UC_Customers ucCustomers = new UC_Customers();
        UC_Products ucProducts = new UC_Products();
        UC_Categories ucCategories = new UC_Categories();
        UC_Suppliers ucSuppliers = new UC_Suppliers();
        UC_OrderManagement ucOrderManagement = new UC_OrderManagement();
        UC_PaymentMethod ucPaymentMethod = new UC_PaymentMethod();
        UC_Employees ucEmployees = new UC_Employees();
        UC_Dashboard ucDashboard = new UC_Dashboard();
        public Workspace()
        {
            InitializeComponent();
            // Use DPI autoscaling
            this.AutoScaleMode = AutoScaleMode.Dpi;
            // Optionally ensure Designer baseline (if you designed at 96 DPI)
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);

            // Wire drag events, role logic, etc...
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            btnCreateOrder.Visible = false;
            btnCustomers.Visible = false;
            btnProducts.Visible = false;
            btnCategories.Visible = false;
            btnSuppliers.Visible = false;
            btnOrderManagement.Visible = false;
            btnPaymentMethod.Visible = false;
            btnEmployees.Visible = false;
            btnDashboard.Visible = false;
            string checkRole = UserSession.CurrentEmployee.RoleName;

            if (checkRole.Trim() == "Admin")
            {
                // Call buttons
                btnCreateOrder.Visible = true;
                btnCustomers.Visible = true;
                btnProducts.Visible = true;
                btnCategories.Visible = true;
                btnSuppliers.Visible = true;
                btnOrderManagement.Visible = true;
                btnPaymentMethod.Visible = true;
                btnEmployees.Visible = true;
                btnDashboard.Visible = true;
            }
            else if (checkRole.Trim() == "Sale Staff")
            {
                // Call buttons
                btnCreateOrder.Visible = true;
                btnCustomers.Visible = true;
                btnProducts.Visible = true;
                btnOrderManagement.Visible = true;
                btnDashboard.Visible = true;
                try
                {
                    ucOrderManagement?.SetCancelOrderEnabled(false); // requires method in UC_OrderManagement
                }
                catch { }
            }
            else if (checkRole.Trim() == "Warehouse Staff")
            {
                // Call buttons
                btnProducts.Visible = true;
                
            }
            else;
            {
                

            }
        }
        // 🔹 Hàm dùng để load UserControl vào panelMain
        private void LoadUserControl(UserControl uc)
        {
            panelMain.Controls.Clear();   // Xóa control cũ
            uc.Dock = DockStyle.Fill;     // Giãn đầy panel
            panelMain.Controls.Add(uc);   // Thêm UserControl vào
        }


        // 🔹 Các sự kiện click cho từng button trong menu
        private void btnPOS_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucCreateOrder);

        }
        //private void btnCreateOrders_Click(object sender, EventArgs e)
        //{
        //    LoadUserControl(ucCreateOrder);
        //}

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucCustomers);
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucProducts);
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucCategories);
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucSuppliers);
        }

        private void btnOrderManagement_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucOrderManagement);
        }

        private void btnPaymentMethod_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucPaymentMethod);
        }

        private void btnEmployee_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucEmployees);

        }
        //private void btnEmployees_Click(object sender, EventArgs e)
        //{
        //    LoadUserControl(ucEmployees);
        //}
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            LoadUserControl(ucDashboard);

        }
        private void btnX_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to Exit?",
                "Logout Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Logout Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                UserSession.Clear();
                this.Hide(); // Ẩn form hiện tại
                Login loginForm = new Login(); // Tạo lại form đăng nhập
                loginForm.Show();
                this.Close(); // Đóng form Workspace
            }

        }

        private void Workspace_MouseDown(object sender, MouseEventArgs e)
        {
            //phải dùng sự kiện mousedow khi kéo ở mode (FormBorderstyle = none )
            //sự kiện này áp dụng cả cho form và control trên form đẻ quy định vùng kéo 
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }







        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
