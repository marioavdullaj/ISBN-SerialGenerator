using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DocumentSerials
{
    class ServerDatabase
    {
        public MySqlConnection Connector { get; set; }
        private string ConnectionString { get; set; }

        /* DB PARAMETERS HERE */// 
        private string server = "localhost";
        private string db_name = "activation_codes";
        private int port = 3306;
        private string user = "root";
        private string password = "prosecco";

        public ServerDatabase()
        {
            ConnectionString = "Server=" + server + "; Port=" + port.ToString() +
                                "; Database=" + db_name + "; Uid=" + user + "; Pwd=" + password + "; pooling = true;";
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
    }
}
