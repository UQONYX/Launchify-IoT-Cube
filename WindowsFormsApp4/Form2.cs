using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            richTextBox1.Text += "Welcome to the Launchify Builder";
            richTextBox1.Text +=  " (Version " + Application.ProductVersion +")";
            richTextBox1.Text += Environment.NewLine + "This program needs Python to run. Make sure to add Python.exe to Path:";  
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                WindowsFormsApp4.Properties.Settings.Default.beginningMessage = true;
            }
            if(!checkBox1.Checked)
            {
                WindowsFormsApp4.Properties.Settings.Default.beginningMessage = false;
            }
        }

        private void Form2_MouseLeave(object sender, EventArgs e)
        {

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show("Go to File > New Directory to choose your file's location");
        }
    }
}
