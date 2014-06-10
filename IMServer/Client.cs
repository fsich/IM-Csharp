using System;
using System.CodeDom;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace IMServer
{
    internal class ImClient
    {
        //druhy paketů
        public const byte ClientConnect = 100;
        public const byte OK = 0;
        public const byte Login = 1;
        public const byte WillRegister = 2;
        public const byte TooLongUsername = 3;
        public const byte TooLongPassword = 4;
        public const byte Exists = 5;
        public const byte Send = 6;
        public const byte Disconnect = 7;
        public const byte AddFriend = 8;
        public const byte InvalidPassOrName = 9;
        public const byte UpdateFriendList = 10;
        public const byte FriendRequest = 11;
        public const byte FriendRequestAccepted = 12;
        public const int MaxUsernameLength = 32;
        public const int MaxPasswordLength = 32;
        public const byte FriendRemove = 13;

        public const string IS_OFFLINE = "User is offline.";
        public const string CANT_MESSAGE = "You can't send message to this user.";
        public const string CLIENT_REGISTERED = "Client {0} was registered.";
        public const string CLIENT_ACCESS = "Client is trying to connect to the server.";
        public const string CLIENT_CONNECTED = "Client {0} connected.";
        public const string CLIENT_DISCONNECTED = "Client {0} disconnected.";
        public const string UPDATING_FRIENDLIST = "Updating friendlist for client: {0}.";
        public const string FRIEND_REQUEST = "Client {0} sent friendrequest to {1}.";
        public const string ACCEPTED_FRIEND_REQUEST = "Client {0} accepted friendrequest from {1}.";
        public const string MESSAGE_SENT = " {0} > {1} : {2}";
        public const string FRIEND_REMOVED = "{0} removed {1} from friendlist.";

        private Server instance;
        private BinaryReader reader;
        private BinaryWriter writer;
        private NetworkStream stream;
        private bool Connected;
        private TcpClient Client;

        public ImClient(TcpClient client, Server instance)
        {
            this.Client = client;
            this.instance = instance;
        }
        //vytvoří se nové vlákno, které bude číst a odesílat data po streamu
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(StartClient));
            t.Start();
        }

        private void StartClient()
        {
            Server.Logger.Log(Logger.Level.ClientCommunication,CLIENT_ACCESS);
            stream = Client.GetStream();

            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            writer.Write(ClientConnect); // prvni packet co se posle, zazada o jmeno a heslo
            writer.Flush(); //vycisti buffer
            while(!Connected)
            {
                byte response;
                try
                {
                    response = reader.ReadByte(); //ceka na odpoveď
                }
                catch (ObjectDisposedException e)
                {
                    Connected = false;
                    return;
                }
                if (response == ClientConnect) //client se pokousi pripojit
                {
                    string username = reader.ReadString();
                    if (username.Length > MaxUsernameLength)
                    {
                        writer.Write(TooLongUsername);
                        writer.Flush();
                        return;
                    }
                    string password = reader.ReadString();
                    if (password.Length > MaxPasswordLength)
                    {
                        writer.Write(TooLongPassword);
                        writer.Flush();
                        return;
                    }
                    bool register = reader.ReadBoolean();
                    if (register)
                    {
                        if (instance.UserExists(username))
                        {
                            writer.Write(Exists);
                            writer.Flush();
                            return;
                        }
                        instance.CreateNewUser(username, password);
                        Server.Logger.Log(Logger.Level.ClientCommunication,CLIENT_REGISTERED.Replace("{0}",username));
                        writer.Write(OK);
                        writer.Flush();
                    }
                    else
                    {
                        if (!instance.PasswordAndUserMatches(username, password))
                        {
                            writer.Write(InvalidPassOrName);
                            writer.Flush();
                            continue;
                        }
                            writer.Write(OK);
                            writer.Flush();
                    }
                    Connected = true;
                    Name = username;
                    ID = instance.GetId(Name);
                    instance.AddOnlineClient(Name, this);
                    Server.Logger.Log(Logger.Level.ClientCommunication,CLIENT_CONNECTED.Replace("{0}",Name));
                }
               
                while (Connected)
                {
                    byte packet = reader.ReadByte();
                    switch (packet)
                    {
                        case Send:
                            string from = Name;
                            string to = reader.ReadString();
                            string msg = reader.ReadString();
                            if (!instance.IsClientOnline(to))
                            {
                                writer.Write(Send);
                                writer.Write(from);
                                writer.Write(IS_OFFLINE); //posle zprávu o tom, že uživatel je offline
                                break;
                            }
                            if (!instance.CanMessage(from, to))
                            {
                                writer.Write(Send);
                                writer.Write(from);
                                writer.Write(CANT_MESSAGE); //posle zprávu o tom, že uživatel není ve friendlistu druhého
                                break;
                            }
                                
                            ImClient client = instance.GetClient(to);
                            client.writer.Write(Send);
                            client.writer.Write(from);
                            client.writer.Write(msg);
                            Server.Logger.Log(Logger.Level.ClientCommunication, MESSAGE_SENT.Replace("{0}",from).Replace("{1}", to).Replace("{2}",msg));
                            client.BinaryWriter.Flush();
                            break;
                        case Disconnect:
                            Connected = false;
                            stream.Close();
                            writer.Close();
                            reader.Close();
                            instance.RemoveOnlineUser(Name);
                            Server.Logger.Log(Logger.Level.ClientCommunication, CLIENT_DISCONNECTED.Replace("{0}",Name));
                            break;
                        case UpdateFriendList:
                            writer.Write(instance.GetFriendList(Name));
                            Server.Logger.Log(Logger.Level.ClientCommunication, UPDATING_FRIENDLIST.Replace("{0}", Name));
                            break;
                        case FriendRequest:
                            string user = reader.ReadString();
                            if (!instance.IsClientOnline(user))
                                return;
                            ImClient c = instance.GetClient(user);
                            Server.Logger.Log(Logger.Level.ClientCommunication, FRIEND_REQUEST.Replace("{0}", Name).Replace("{1}", user));
                            instance.AddFriend(Name, user);
                            c.writer.Write(FriendRequest);
                            c.writer.Write(Name);
                            c.writer.Flush();
                            break;
                        case FriendRequestAccepted:
                            string s = reader.ReadString();
                            Server.Logger.Log(Logger.Level.ClientCommunication, ACCEPTED_FRIEND_REQUEST.Replace("{0}", Name).Replace("{1}", s));
                            instance.AddFriend(s,Name);
                            break;
                        case FriendRemove:
                            string removed = reader.ReadString();
                            instance.RemoveFriend(removed, Name);
                            Server.Logger.Log(Logger.Level.ClientCommunication, FRIEND_REMOVED.Replace("{0}", Name).Replace("{1}", removed));
                            break;
                    }
                }
            }


        }


        public String Name { get; set; }
        public int ID { get; set; }
        public BinaryWriter BinaryWriter { get { return writer; } }
        public BinaryReader BinaryReader { get { return reader; } }

    }
}
