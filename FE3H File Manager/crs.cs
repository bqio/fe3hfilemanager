using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FE3H_File_Manager
{
    public partial class crs : Form
    {
        public crs()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Set all values.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!textBox1.Text.Contains("$"))
            {
                MessageBox.Show("Invalid template. You must use $.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Convert.ToInt32(textBox2.Text) >= Convert.ToInt32(textBox3.Text))
            {
                MessageBox.Show("The start of the range must be less than the end of the range.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            (Owner as Form1).CreateCustomRangeScript(textBox1.Text, Convert.ToInt32(textBox2.Text), Convert.ToInt32(textBox3.Text));
            Dispose();
        }
    }
}
