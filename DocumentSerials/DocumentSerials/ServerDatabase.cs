using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using DocumentSerials.Models;
using System.CodeDom;

namespace DocumentSerials
{
    class ServerDatabase
    {
        public MySqlConnection Connector { get; set; }
        private string ConnectionString { get; set; }
        private MD5 md5;

        private string server = Settings.ReadSetting("server");
        private string db_name = Settings.ReadSetting("database");
        private int port = Convert.ToInt32(Settings.ReadSetting("port"));
        private string user = Settings.ReadSetting("user");
        private string password = Settings.ReadSetting("password");
        private string appuser = Settings.ReadSetting("appuser");

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

        public List<Duration> GetDurations()
        {
            List<Duration> durations = new List<Duration>();
            string query = "SELECT * FROM duration";
            if (OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, Connector);
                MySqlDataReader data = cmd.ExecuteReader();

                while (data.Read())
                {
                    durations.Add(new Duration(Convert.ToInt32(data["id"]), data["description"].ToString()));
                }
                data.Close();
            }

            return durations;
        }

        public List<Country> GetCountries()
        {
            List<Country> countries = new List<Country>();
            string query = "SELECT * FROM country";
            if (OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, Connector);
                MySqlDataReader data = cmd.ExecuteReader();

                while (data.Read())
                {
                    countries.Add(new Country(Convert.ToInt32(data["iso"]), data["code"].ToString(), data["name"].ToString()));
                }
                data.Close();
            }

            return countries;
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

        public int GetCountryIso(string name)
        {
            string query = "SELECT iso FROM country WHERE name = '" + name + "'";
            int iso = -1;

            //Open Connection
            if (OpenConnection())
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, Connector);

                //ExecuteScalar will return one value
                iso = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return iso;
        }

        public int GetDurationId(string description)
        {
            string query = "SELECT id FROM duration WHERE description = '" + description + "'";
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

        public String InsertCode(List<Code> codes)
        {
            int result = -1;
            String creation_code = Guid.NewGuid().ToString();
            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.Append("INSERT INTO activation_codes (actcode, country, bookid, creation_date, creation_code) VALUES ");

            foreach (Code code in codes)
            {
                string psw = code.Actcode;
                int durationId = GetDurationId(code.Duration);
                int countryIso = GetCountryIso(code.Country);
                string datenow = DateTime.Now.ToString("yyyy-MM-dd");
                int bookid = GetBookId(code.Book);
                // append the values to be inserted
                queryBuilder.Append("('" + psw + "', " + countryIso + ", "+ bookid+", '"+ datenow + "','"+creation_code+"'),");
            }
            queryBuilder = queryBuilder.Remove(queryBuilder.Length - 1, 1);

            if (OpenConnection())
            {
                try
                {
                    string query = queryBuilder.ToString();
                    MySqlCommand cmd = new MySqlCommand(query, Connector);

                    result = cmd.ExecuteNonQuery();
                    if (result <= 0)
                        creation_code = String.Empty;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return creation_code;
        }

        public bool InsertJob(String book, String country, int count, String creation_code)
        {
            int result = -1;
            try
            {
                String query = "INSERT INTO jobs (bookid, count, countryid, creation_code, creation_date, user) VALUES ";
                string datenow = DateTime.Now.ToString("yyyy-MM-dd");
                int bookid = GetBookId(book);
                int countryid = GetCountryIso(country);

                query += "(" + bookid.ToString() + ", " + count.ToString() + ", " + countryid.ToString() + ", '" + creation_code.ToString() + "', '" + datenow.ToString() + "', '" + appuser + "');";

                if (OpenConnection())
                {
                    try
                    {
                        MySqlCommand cmd = new MySqlCommand(query, Connector);

                        result = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return result > 0;
        }
        #region XML_UPLOAD
        public bool InsertXML(List<Object> list, Type type)
        {
            int result = -1;
            StringBuilder queryBuilder = new StringBuilder();

            if (type == typeof(Country))
            {

                queryBuilder.Append("INSERT INTO country VALUES ");

                foreach (Country country in list)
                {
                    queryBuilder.Append("(" + country.Id + ", '" + country.Iso + "', '" + country.Name.Replace("'", "''") + "'),");
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
            }
            if (type == typeof(Duration))
            {
                queryBuilder.Append("INSERT INTO duration VALUES ");

                foreach (Duration duration in list)
                {
                    queryBuilder.Append("(" + duration.Id + ", '" + duration.Description + "'),");
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
            }
            if (type == typeof(Book))
            {
                queryBuilder.Append("INSERT INTO book VALUES ");

                foreach (Book book in list)
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
            }
            return result > 0;
        }
        #endregion
#endregion
    }
}
