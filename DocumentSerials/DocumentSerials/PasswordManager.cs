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
using System.Drawing;

namespace DocumentSerials
{
    public partial class PasswordManager : Form
    {
        private SerialCode sc;
        private Stopwatch stopWatch;
        private ServerDatabase db;

        private Dictionary<string, List<Tuple<string, int>>> Passwords { get; set; }
        private List<Tuple<int, string>> books;
        private Dictionary<string, int> ActualRow { get; set; }
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
            ActualRow = new Dictionary<string, int>() { };
            books = new List<Tuple<int, string>>();
        }

        private void InitUI()
        {
            // initialize combobox
            for (int i = 1; i <= 36; i++)
            {
                comboBox1.Items.Add(i + " Months");
            }
            comboBox1.SelectedIndex = 0;

            // initialize book combobox
            books = db.GetBooks();
            foreach (var book in books)
            {
                bookComboBox.Items.Add(book.Item2);
            }
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            string ISBN = bookComboBox.Text;
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

            int numOfBlocks = 0;
            try
            {

                numOfBlocks = Convert.ToInt32(numBlockTextBox.Text);
                if (numOfBlocks <= 0)
                {
                    MessageBox.Show("At least one block");
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The number of blocks format is not valid, please insert a number");
            }

            int codeSize = 0;
            try
            {

                codeSize = Convert.ToInt32(codeSizeTextBox.Text);
                if (codeSize <= 8)
                {
                    MessageBox.Show("Code too short, it must be at least long 8 characters");
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The code size format is not valid, please insert a number");
            }

            if(codeSize % numOfBlocks != 0)
            {
                MessageBox.Show(codeSize + " characters cannot be divided into " + numOfBlocks + " blocks");
                return;
            }
            sc.NumBlocks = numOfBlocks;
            sc.Size = codeSize;

            generatePasswords(ISBN, duration, numberOfPasswords);
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
            if (!ActualRow.ContainsKey(doc))
                ActualRow.Add(doc, 0);

            int db_row = db.Count(doc);
            int actual_row = (ActualRow[doc] > db_row) ? ActualRow[doc] : db_row;
            int i;
            for (i = actual_row + 1; i <= actual_row + n; i++)
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
                progressBar1.Value = i - actual_row;
            }
            ActualRow[doc] = i-1;
            dataGridView1.DataSource = dt;
            dataGridView1.UseWaitCursor = false;

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            txtTimer.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

            // NOW WE INSERT INTO THE DB
            DialogResult dialogResult = MessageBox.Show("Insert into the database?", "DB Conneciton", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {

                bool res = db.Insert(Passwords);
                if (res)
                {
                    MessageBox.Show("Serial codes inserted correctly");
                    // Clear the passwords
                    Passwords.Clear();
                    string isbn = bookComboBox.Text;
                    // And update the total number of codes generated for the book
                    countTextBox.Text = db.Count(isbn).ToString() + " codes generated";
                }
                else
                    MessageBox.Show("Error during the insertion of the serial codes");
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }

            return;
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FilterIndex = 2;
                dialog.RestoreDirectory = true;
                dialog.FileName = "SerialCodes_" + bookComboBox.Text +
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

        private void bookComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string isbn = bookComboBox.Text;
            countTextBox.Text = db.Count(isbn).ToString() + " codes generated";
        }

        private void PasswordManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.CloseConnection();
        }

        private async Task OpenConnection()
        {
            // Opening the connection
            label10.Text = "Connecting....";
            label10.ForeColor = Color.Yellow;
            await Task.Delay(10);
            bool res = db.OpenConnection();
            if (res)
            {
                label10.Text = "Connected";
                label10.ForeColor = Color.Green;
                InitUI();
            }
            else
            {
                label10.Text = "Error";
                label10.ForeColor = Color.DarkRed;
            }
        }

        private async void label11_Click(object sender, EventArgs e)
        {
            await OpenConnection();
        }
    }
}
    
       

       

        

    



