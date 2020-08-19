using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet.Messages;
using System.Collections.Generic;

namespace DocumentSerials
{
    public partial class PasswordManager : Form
    {
        private SerialCode sc;
        private Stopwatch stopWatch;
        private ServerDatabase db;

        private string ISBN { get; set; }
        private int Duration { get; set; }
        private List<string> Passwords { get; set; }

        public PasswordManager()
        {
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            // Initialize private components
            sc = new SerialCode();
            stopWatch = new Stopwatch();
            db = new ServerDatabase();
            Passwords = new List<string>() { };

            // initialize combobox
            for (int i = 1; i <= 36; i++)
            {
                comboBox1.Items.Add(i + " Months");
            }
            comboBox1.SelectedIndex = 0;
        }


        private void generatePasswords(string doc, int duration, int n)
        {

            stopWatch.Start();
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
                Passwords.Add(psw);

                // Update gridview
                DataRow dr = dt.NewRow();
                dr[0] = i.ToString();
                dr[1] = doc;
                dr[2] = psw;
                dt.Rows.Add(dr);
                // update progress bar
                progressBar1.Value = i - actual_rows;
            }

            dataGridView1.DataSource = dt;
            dataGridView1.UseWaitCursor = false;

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            txtTimer.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
            
            return;
        }

        public void button1_Click(object sender, EventArgs e)
        {
            ISBN = textBox2.Text;
            Duration = comboBox1.SelectedIndex + 1;
            if (String.IsNullOrEmpty(ISBN))
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
            catch (Exception)
            {
                MessageBox.Show("The number of passwords format is not valid, please insert a number");
            }

            generatePasswords(ISBN, Duration, numberOfPasswords);
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
                        foreach (DataRow row in dt.Rows)
                        {
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
            if (dt != null)
                dt.Clear();
            dataGridView1.DataSource = dt;
            Passwords.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(ISBN.Equals(String.Empty) || Passwords.Count <= 0)
            {
                MessageBox.Show("Insert ISBN and generate serials first");
                return;
            }
             
            bool res = db.Insert(ISBN, Passwords);
            if (res)
            {
                MessageBox.Show("Serial codes inserted correctly");
                Passwords.Clear();
            }
            else
                MessageBox.Show("Error during the insertion of the serial codes");
        }
    }
}
    
       

       

        

    



