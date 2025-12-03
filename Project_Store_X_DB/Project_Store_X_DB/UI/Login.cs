using Project_Store_X_DB.BLL;
using Project_Store_X_DB.DTO;
using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Runtime.InteropServices;// khai báo dùng kéo form borderstyle none

namespace Project_Store_X_DB
{
    public partial class Login : Form
    {
        // Borderstyle = none nhưng vẫn kéo form được
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;


        // Khai báo BLL
        private AccountBLL _accountBLL = new AccountBLL();

        public Login()
        {
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim(); 
            EmployeeDTO user = _accountBLL.CheckLogin(username, password);
            if (user != null)
            {
                MessageBox.Show($"Loggin successful!", "Notification", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                UserSession.CurrentEmployee = user;
                this.Hide();
                Workspace workspaceForm = new Workspace();
                workspaceForm.Show();
            }
            else
            {            
                MessageBox.Show("Incorrect username or password.", "Login Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void chkShowPassword_CheckedChanged_1(object sender, EventArgs e)
        {
            if (chkShowPassword.Checked)
            {
                txtPassword.UseSystemPasswordChar = false;
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
            }
        }
        private void btnExit_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
             "Are you sure want to exit?",
             "Confirmation",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Question
         );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void Login_MouseDown(object sender, MouseEventArgs e)
        
        {
            //phải dùng sự kiện mousedow khi kéo ở mode (FormBorderstyle = none )
            //sự kiện này áp dụng cả cho form và control trên form đẻ quy định vùng kéo 
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

    }
}
