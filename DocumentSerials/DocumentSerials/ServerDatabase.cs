using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DocumentSerials
{
    class ServerDatabase
    {
        public MySqlConnection Connector { get; set; }
        private string ConnectionString { get; set; }

        /* DB PARAMETERS HERE */
        private string server = "localhost";
        private string db_name = "activation_codes";
        private int port = 3306;
        private string user = "root";
        private string password = "prosecco";

        public ServerDatabase()
        {
            ConnectionString = "Server=" + server + "; Port=" + port.ToString() +
                                "; Database=" + db_name + "; Uid=" + user + "; Pwd=" + password + "; pooling = true; " +
                                "SslMode=REQUIRED;";
            Connector = new MySqlConnection(ConnectionString);
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

        public int Count(string ISBN)
        {
            string query = "SELECT Count(*) FROM activation_code WHERE ISBN = " + ISBN;
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, Connector);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        public bool Insert(string ISBN, List<string> passwords)
        {
            int result = -1;
            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.Append("INSERT INTO activation_code (ISBN, password) VALUES ");

            for(var i = 0; i < passwords.Count; i++)
            {
                string psw = passwords[i];
                string separator = (i < passwords.Count - 1) ? "," : "";
                queryBuilder.Append("('" + ISBN + "', '" + psw + "')"+separator);
            }

            if (this.OpenConnection())
            {
                try
                {
                    string query = queryBuilder.ToString();
                    MySqlCommand cmd = new MySqlCommand(query, Connector);

                    result = cmd.ExecuteNonQuery();
                    this.CloseConnection();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return (result == passwords.Count);
        }

        #endregion
    }
}
