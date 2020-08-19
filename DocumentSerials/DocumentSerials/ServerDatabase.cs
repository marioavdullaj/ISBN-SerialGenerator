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
            ConnectionString = "Server=" + server + "; Port=" + port.ToString() + "; Database=" + db + "; Uid=" + user  + "; Pwd=" + password;
            Connector = new MySqlConnection(ConnectionString);
        }
    }
}
