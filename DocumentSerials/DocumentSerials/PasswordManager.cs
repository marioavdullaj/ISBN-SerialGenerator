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
using System.Linq;
using Microsoft.Win32;

namespace DocumentSerials
{
    public partial class PasswordManager : Form
    {
        private SerialCode sc;
        private Stopwatch stopWatch;
        private ServerDatabase db;

        private Dictionary<string, List<Tuple<string, int>>> Passwords { get; set; }

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
            Passwords = new Dictionary<string, List<Tuple<string, int>>>() { };

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
                dt.Columns.Add("Duration");
            }
            else
                dt = (DataTable)dataGridView1.DataSource;

            dataGridView1.UseWaitCursor = true;

            if (!Passwords.ContainsKey(doc))
                Passwords.Add(doc, new List<Tuple<string, int>>());

            for (int actual_rows = dt.Rows.Count, i = actual_rows + 1; i <= actual_rows + n; i++)
            {
                psw = sc.Generate(doc, duration, i);
                Passwords[doc].Add(new Tuple<string, int>(psw, duration));
                // Update gridview
                DataRow dr = dt.NewRow();
                dr[0] = i.ToString();
                dr[1] = doc;
                dr[2] = psw;
                dr[3] = duration.ToString();
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

        private void exportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FilterIndex = 2;
                dialog.RestoreDirectory = true;
                dialog.FileName = "SerialCodes_" + textBox2.Text +
                    "-" + DateTime.Today.Day + "-" + DateTime.Today.Month + "-" + DateTime.Today.Year +
                    ".csv";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Can use dialog.FileName
                    using (Stream stream = dialog.OpenFile())
                    {   /*
                        DataTable dt = (DataTable)dataGridView1.DataSource;
                        foreach (DataRow row in dt.Rows)
                        {
                            byte[] output = Encoding.UTF8.GetBytes(row[2].ToString() + "\n");
                            stream.Write(output, 0, output.Length);
                        }*/
                        DataTable dt = (DataTable)dataGridView1.DataSource;
                        if (dt != null)
                        {
                            StringBuilder sb = new StringBuilder();

                            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                                              Select(column => column.ColumnName);
                            sb.AppendLine(string.Join(",", columnNames));

                            foreach (DataRow row in dt.Rows)
                            {
                                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                                sb.AppendLine(string.Join(",", fields));
                            }
                            var b = Encoding.UTF8.GetBytes(sb.ToString());
                            stream.Write(b, 0, b.Length);

                        }
                        else
                            MessageBox.Show("No codes can be exported");

                    }
                    MessageBox.Show("Export completed!");
                }
            }
        }

        private void clearPasswords()
        {
            DataTable dt = (DataTable)dataGridView1.DataSource;
            if (dt != null)
                dt.Clear();
            dataGridView1.DataSource = dt;
            Passwords.Clear();
        }

        private void clearAllButton_Click(object sender, EventArgs e)
        {
            clearPasswords();
            progressBar1.Value = 0;
        }

        private void exportDbButton_Click(object sender, EventArgs e)
        {
            bool res = db.Insert(Passwords);
            if (res)
            {
                MessageBox.Show("Serial codes inserted correctly");
                Passwords.Clear();
            }
            else
                MessageBox.Show("Error during the insertion of the serial codes");
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            string ISBN = textBox2.Text;
            int duration = comboBox1.SelectedIndex + 1;
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

            generatePasswords(ISBN, duration, numberOfPasswords);

        }
    }
}
    
       

       

        

    



