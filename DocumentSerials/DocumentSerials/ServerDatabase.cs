using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace DocumentSerials
{
    class ServerDatabase
    {
        public MySqlConnection Connector { get; set; }
        private string ConnectionString { get; set; }
        private MD5 md5;

        /* DB PARAMETERS HERE */// 
        private string server = ConfigurationManager.AppSettings["server"];
        private string db_name = ConfigurationManager.AppSettings["db_name"];
        private int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
        private string user = ConfigurationManager.AppSettings["user"];
        private string password = ConfigurationManager.AppSettings["password"];

        public ServerDatabase()
        {
            ConnectionString = "Server=" + server + "; Port=" + port.ToString() +
                                "; Database=" + db_name + "; Uid=" + user + "; Pwd=" + password + "; pooling = true;";
            Connector = new MySqlConnection(ConnectionString);
            md5 = MD5.Create();
        }

        public bool OpenConnection()
        {
            if (Connector.State.Equals(System.Data.ConnectionState.Open))
                return true;

            try
            {
                Connector.Open();
                return Connector.State.Equals(System.Data.ConnectionState.Open);
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                Connector.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        #region CRUD OPERATIONS
        public List<Book> GetBooks()
        {
            List<Book> books = new List<Book>();
            string query = "SELECT * FROM book";
            if(OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, Connector);
                MySqlDataReader data = cmd.ExecuteReader();

                while(data.Read())
                {
                    books.Add(new Book(Convert.ToInt32(data["id"]), data["title"].ToString(), data["description"].ToString()));
                }
                data.Close();
            }

            return books;
        }

        public int GetBookId(string title)
        {
            string query = "SELECT id FROM book WHERE title = '" + title + "'";
            int id = -1;

            //Open Connection
            if (OpenConnection())
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, Connector);

                //ExecuteScalar will return one value
                id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return id;
        }

        public int Count()
        {
            string query = "SELECT Count(*) FROM activation_codes";
            int Count = -1;

            //Open Connection
            if (OpenConnection())
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, Connector);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");
            }
            return Count;
        }

        public int Count(string title)
        {
            int bookid = GetBookId(title);
            string query = "SELECT Count(*) FROM activation_codes WHERE bookid = " + bookid;
            int Count = -1;

            //Open Connection
            if (OpenConnection())
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, Connector);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");
            }
            return Count;
        }

        public bool Insert(Dictionary<string, List<Tuple<string, int>>> passwords)
        {
            int result = -1;
            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.Append("INSERT INTO activation_codes (actcode, country, bookid, creation_date, creation_code) VALUES ");

            foreach (string title in passwords.Keys)
            {
                foreach(var item in passwords[title])
                {
                    string psw = item.Item1;
                    int duration = Convert.ToInt32(item.Item2);
                    // for now we put 1, we gotta create in the UI the country select as well
                    int country = 0;
                    DateTime now = DateTime.Now;
                    string datenow = now.ToString("yyyy-MM-dd");
                    // for now, we're going to fix in a while
                    int bookid = GetBookId(title);
                    // MD5 creation_code from datetime now
                    byte[] creation_code = md5.ComputeHash(Encoding.ASCII.GetBytes(datenow));
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < creation_code.Length; i++) sb.Append(creation_code[i].ToString("X2"));
                    // append the values to be inserted
                    queryBuilder.Append("('" + psw + "', " + country + ", "+ bookid+", '"+ datenow + "','"+sb.ToString()+"'),");
                }
            }
            queryBuilder = queryBuilder.Remove(queryBuilder.Length - 1, 1);

            if (OpenConnection())
            {
                try
                {
                    string query = queryBuilder.ToString();
                    MySqlCommand cmd = new MySqlCommand(query, Connector);

                    result = cmd.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return result > 0;
        }

        public bool InsertBooks(List<Book> books)
        {
            int result = -1;
            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.Append("INSERT INTO book VALUES ");

            foreach (Book book in books)
            {
                queryBuilder.Append("(" + book.Id + ", '" + book.Title.Replace("'", "''") + "', '" + book.Description + "'),");
            }
            queryBuilder = queryBuilder.Remove(queryBuilder.Length - 1, 1);

            if (OpenConnection())
            {
                try
                {
                    string query = queryBuilder.ToString();
                    MySqlCommand cmd = new MySqlCommand(query, Connector);

                    result = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return result > 0;
        }

        #endregion
    }
}
