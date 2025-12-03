using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Project_Store_X_DB
{
    public partial class UC_ProductCard : UserControl
    {
        // Sự kiện khi click vào card này (để UC_CreateOrder bắt được)
        public event EventHandler OnSelect;

        public int ProductID { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public UC_ProductCard()
        {
            InitializeComponent();
            // Gắn sự kiện click cho toàn bộ thành phần con
            this.Click += (s, e) => OnSelect?.Invoke(this, e);
            // Giả sử bạn có PictureBox (picImg) và Label (lblName, lblPrice) trong Designer
            // Nếu chưa có, bạn cần kéo thả vào hoặc tôi sẽ code dynamic ở UC_CreateOrder cho nhanh.
        }

        // Hàm set dữ liệu hiển thị
        public void SetData(int id, string name, decimal price, int stock, string imageURL)
        {
            this.ProductID = id;
            this.Price = price;
            this.Stock = stock;

            // Tạo label hiển thị thông tin (Code tay để bạn đỡ phải design lại)
            Label lblInfo = new Label();
            lblInfo.Text = $"{name}\n${price}\nStock: {stock}";
            lblInfo.Dock = DockStyle.Bottom;
            lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            lblInfo.Height = 50;
            lblInfo.BackColor = Color.WhiteSmoke;
            lblInfo.Click += (s, e) => OnSelect?.Invoke(this, e); // Truyền click ra ngoài
            this.Controls.Add(lblInfo);

            // Xử lý ảnh
            PictureBox pic = new PictureBox();
            pic.Dock = DockStyle.Fill;
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Click += (s, e) => OnSelect?.Invoke(this, e); // Truyền click ra ngoài

            if (!string.IsNullOrEmpty(imageURL))
            {
                string path = Path.Combine(Application.StartupPath, "Images", imageURL);
                if (File.Exists(path)) pic.Image = Image.FromFile(path);
            }
            this.Controls.Add(pic);
        }
    }
}