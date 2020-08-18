using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentSerials
{
    public partial class PasswordManager : Form
    {
        private SerialCode sc;
        public PasswordManager()
        {
            Init();
        }// so finally, when you modify, you need to do this
        // hereherehererer 00.20
        //do it , try
        //ok
        // no, first save, then commit, then commit changes, then sync
        // you didn't tell me that, i always save with CTRL+S so you didnt see it :D
        private void Init()
        {
            InitializeComponent();
            sc = new SerialCode();
            
            // initialize combobox
            for(int i = 1; i <= 36; i++)
            {
                comboBox1.Items.Add(i + " Months");
            }
            comboBox1.SelectedIndex = 0;
        }


        private void generatePasswords(string doc, int duration, int n)
        {
            //List<string> passwords = new List<string> { };

            progressBar1.Value = 0;
            progressBar1.Maximum = n;
            string psw;

            DataTable dt;
            if (dataGridView1.DataSource == null)
            {
                dt = new DataTable();
                dt.Columns.Add("#");
                dt.Columns.Add("Document");
                dt.Columns.Add("Password");
            }
            else
                dt = (DataTable)dataGridView1.DataSource;

            dataGridView1.UseWaitCursor = true;

            for (int actual_rows = dt.Rows.Count, i = actual_rows + 1; i <= actual_rows + n; i++)
            {
                psw = sc.Generate(doc, duration);
                DataRow dr = dt.NewRow();
                dr[0] = i.ToString();
                dr[1] = doc;
                dr[2] = psw;
                dt.Rows.Add(dr);
                progressBar1.Value = i - actual_rows;
            }
            dataGridView1.DataSource = dt;
            dataGridView1.UseWaitCursor = false;

            return;

        }
        public void button1_Click(object sender, EventArgs e)
        {

            string documentNumber = textBox2.Text;
            int duration = comboBox1.SelectedIndex + 1;
            if (String.IsNullOrEmpty(documentNumber))
            {
                MessageBox.Show("You must specify the ISBN of the book to generate the codes");
                return;
            }

            int numberOfPasswords = 0;
            // validation of the fields
            try
            {

                numberOfPasswords = Convert.ToInt32(textBox1.Text);
                if (numberOfPasswords <= 0)
                {
                    MessageBox.Show("You must generate at least one password");
                    return;
                }
            }
            catch(Exception _)
            {
                MessageBox.Show("The number of passwords format is not valid, please insert a number");
            }

            generatePasswords(documentNumber, duration, numberOfPasswords);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FilterIndex = 2;
                dialog.RestoreDirectory = true;
                dialog.FileName = "SerialCodes_" + textBox2.Text + 
                    "-" + DateTime.Today.Day + "-" + DateTime.Today.Month + "-" + DateTime.Today.Year + 
                    ".txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Can use dialog.FileName
                    using (Stream stream = dialog.OpenFile())
                    {
                        DataTable dt = (DataTable)dataGridView1.DataSource;
                        foreach(DataRow row in dt.Rows) {
                            byte[] output = Encoding.UTF8.GetBytes(row[2].ToString() + "\n");
                            stream.Write(output, 0, output.Length);
                        }
                    }
                    MessageBox.Show("Export completed!");
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dataGridView1.DataSource;
            if(dt != null)
                dt.Clear();
            dataGridView1.DataSource = dt;
        }
    }
}
