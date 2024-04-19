using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
namespace NetworkFileShareTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(img as System.Drawing.Image);
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.CopyFromScreen(0, 0, 0, 0, img.Size);
            Save(img);
            img.Dispose();
            graphics.Dispose();
        }
        public static void Save(Image bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);    // "\\\\DESKTOP-NT6GT0U\\test\\capture.jpg"
                using (FileStream fs = new FileStream("\\\\LAPTOP-BDVJOOQG\\test\\capture.jpg", FileMode.Create, System.IO.FileAccess.Write))
                {
                    ms.Position = 0;
                    ms.CopyTo(fs);
                }
                bmp.Dispose();
            }
        }
    }
}