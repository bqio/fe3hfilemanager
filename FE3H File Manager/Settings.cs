using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FE3H_File_Manager
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            textBox1.Text = ConfigurationManager.AppSettings["databaseUrl"];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["databaseUrl"] = textBox1.Text;
            MessageBox.Show("Saved.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = ConfigurationManager.AppSettings["databaseUrlDefault"];
        }
    }
}
