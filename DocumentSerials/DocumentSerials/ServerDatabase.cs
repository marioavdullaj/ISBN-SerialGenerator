using System;
using System.Collections.Generic;
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

        public ServerDatabase(string server, string db, int port, string user, string password)
        {
            ConnectionString =  "Server=" + server + "; Port=" + port.ToString() + 
                                "; Database=" + db + "; Uid=" + user  + "; Pwd=" + password + "; pooling = true; " +
                                "SslMode=REQUIRED;";
            Connector = new MySqlConnection(ConnectionString);
        }

        public bool StartConnection()
        {
            if (Connector.State.Equals(System.Data.ConnectionState.Open))
                return true;

            Connector.Open();
            return Connector.State.Equals(System.Data.ConnectionState.Open);
        }

        public void CloseConnection()
        {
            Connector.Close();
        }
    }
}
