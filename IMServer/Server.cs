using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace IMServer
{
    internal class Server
    {
        private MySqlConnection con;
        private TcpListener tcpListener;
        private Thread listenThread;
        private Dictionary<string, ImClient> ConnectedClients = new Dictionary<string, ImClient>(StringComparer.InvariantCultureIgnoreCase);
        public static Logger Logger= new Logger(); 
        public Server()
        {
            CreateDBConnection();
            tcpListener = new TcpListener(IPAddress.Any, 3000);
            tcpListener.Start();
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            Console.ReadLine();
        }

        //pripoji se do db a vytvoří pottřebné tabulky
        private void CreateDBConnection()
        {
            MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();
            conn_string.Server = "localhost";
            conn_string.UserID = "root";
            conn_string.Password = "heslo";
            conn_string.Database = "test";
            try
            {
                con = new MySqlConnection(conn_string.ToString());
                con.Open();
                String querry =
                    "CREATE TABLE IF NOT EXISTS `clients` (" +
                    "id int NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                    "name VARCHAR(32) NOT NULL UNIQUE," +
                    "pass VARCHAR(32) NOT NULL" +
                    ");";
                MySqlCommand cmd = new MySqlCommand(querry, con);
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                querry = "CREATE TABLE IF NOT EXISTS `friendlist` (" +
                         "id int NOT NULL AUTO_INCREMENT," +
                         "name VARCHAR(32) NOT NULL," +
                         "user_id int NOT NULL,"+
                         "PRIMARY KEY (id, name)" +
                         ");";

                cmd = new MySqlCommand(querry, con);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                Console.WriteLine("Pripojeno k databazi.");
                //con.Close();
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
            }
        }

        //zkontroluje zda uzivatel jiz v databazi je
        public bool UserExists(string username)
        {
            try {
                string querry = "SELECT count(*) FROM `clients` WHERE name=@name";
                MySqlCommand cmd = new MySqlCommand(querry, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", username);
                int totalCount = Convert.ToInt32(cmd.ExecuteScalar());
                if (totalCount == 0)
                {
                    return false;
                }
                return true;
                
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
                return false;
            }
        }

        //zkontroluje zda je shoda ve jméně a hesle
        public bool PasswordAndUserMatches(string username, string pass)
        {
            try {
                string querry = "SELECT count(*) FROM `clients` WHERE name=@name AND pass=@pass";
                MySqlCommand cmd = new MySqlCommand(querry, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@pass", pass);
                int totalCount = Convert.ToInt32(cmd.ExecuteScalar());
                if (totalCount == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
                return false;
            }
        } 

        //vytvorí nového uživatele v databázi
        public void CreateNewUser(string username, string pass)
        {
            try {
                string querry = "INSERT INTO `clients` (name,pass) VALUES (@name, @pass)";
                MySqlCommand cmd = new MySqlCommand(querry,con);
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
            }
        }    

        //vrátí řetězec který obsahuje jména přátel z databáze 
        public string GetFriendList(string user){
            try
            {
                String qry =
                    "SELECT friendlist.name from clients,friendlist WHERE clients.id = friendlist.user_id AND clients.id = @id";
                MySqlCommand cmd = new MySqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", GetId(user));
                MySqlDataReader rdr = cmd.ExecuteReader();
                string l = "";
                while (rdr.Read())
                {
                    l+= rdr.GetString("name");
                }
                rdr.Close();
                return l;
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
                return "";
            }
        }   

        
        public void AddFriend(string user, string f)
        {
            try {
                string querry = "INSERT INTO friendlist (name,user_id) VALUES (@name,@userid)";
                MySqlCommand cmd = new MySqlCommand(querry, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", GetId(user));
                cmd.Parameters.AddWithValue("@name", f);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
            }
        } 

        //
        public bool CanMessage(string from, string to)
        {
            string a = GetFriendList(from);
            string b = GetFriendList(to);
            return a.Contains(to) && b.Contains(from);
        } 

        public int GetId(string name) {
            try
            { 
                String qry = "SELECT id FROM `clients` WHERE name = @name ";
                MySqlCommand cmd = new MySqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", name);
                MySqlDataReader rdr = cmd.ExecuteReader();
                int id = -1;
                if (rdr.Read())
                    id = rdr.GetInt32(0);
                rdr.Close();
                return id;
            }
            catch (MySqlException e)
            {
                Logger.Log(Logger.Level.MySQLWarning, e.Message);
                return -1;
            }
        } 

        public bool IsClientOnline(string username)
        {
            return ConnectedClients.ContainsKey(username);
        } 

        public void AddOnlineClient(string username, ImClient client){
            if (!IsClientOnline(username))
                ConnectedClients.Add(username,client);
        }

        public void RemoveOnlineUser(string username)
        {
            ConnectedClients.Remove(username);
        }

        private void ListenForClients()
        {
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                ImClient c = new ImClient(client, this);
                c.Start();
            }
        }

        public MySqlConnection MySqlConnection
        {
            get { return con; }
        }

        public TcpListener TcpListener
        {
            get { return tcpListener; }

        }
        public Thread ListenThread
        {
            get { return listenThread; }
        }

        public ImClient GetClient(string to)
        {
            ImClient c;
            ConnectedClients.TryGetValue(to, out c);
            return c;
        }
    }
}