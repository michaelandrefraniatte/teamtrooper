using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkFileShare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string fileName = "";
            string path = "";
            OpenFileDialog fDialog = new OpenFileDialog();
            fDialog.Title = "Attach customer proposal document";
            fDialog.Filter = "Doc Files|*.doc|Docx File|*.docx|PDF doc|*.pdf";
            fDialog.InitialDirectory = @"C:\";
            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    WebClient client = new WebClient();
                    NetworkCredential nc = new NetworkCredential("mic", "seck");
                    client.Credentials = nc;
                    byte[] arrReturn = client.UploadFile(@"\\LAPTOP-BDVJOOQG\lsp\doc.pdf", fDialog.FileName);
                    MessageBox.Show(arrReturn.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
