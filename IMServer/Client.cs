using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace IMServer
{
    internal class ImClient
    {
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

        private Server instance;
        private BinaryReader reader;
        private BinaryWriter writer;
        private NetworkStream stream;
        private bool Connected = false;
        private TcpClient Client;

        public ImClient(TcpClient client, Server instance)
        {
            this.Client = client;
            this.instance = instance;
        }
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(StartClient));
            t.Start();
        }

        private void StartClient()
        {
            Console.WriteLine("Client se pokousi pripojit na server.");
            stream = Client.GetStream();

            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            writer.Write(ClientConnect); // prvni packet co se posle, zazada o jmeno a heslo
            writer.Flush(); //vycisti buffer
            while(!Connected){
                byte response = reader.ReadByte(); //ceka na odpoveď
                if (response == ClientConnect) //client se pokousi pripojit
                {
                    //[string username][string pass][boolean register]
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
                        Console.WriteLine("Novy uzivatel byl registrovan.");
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
                    Console.WriteLine("Uzivatel " + Name + " pripojen.");
                }
               
                while (Connected)
                {
                    byte packet = reader.ReadByte();
                    switch (packet)
                    {
                            //[from][to][msg]
                        case Send:
                            string from = Name;
                            string to = reader.ReadString();
                            string msg = reader.ReadString();
                            if (!instance.IsClientOnline(to))
                                break;
                            if (!instance.CanMessage(from, to))
                                break;
                            ImClient client = instance.GetClient(to);
                            client.BinaryWriter.Write(Send);
                            client.BinaryWriter.Write(from);
                            client.BinaryWriter.Write(msg);
                            Console.WriteLine(from + " > " + to + " > " + msg);
                            client.BinaryWriter.Flush();
                            break;
                        case Disconnect:
                            Connected = false;
                            stream.Close();
                            writer.Close();
                            reader.Close();
                            instance.RemoveOnlineUser(Name);
                            Console.WriteLine("Uzivatel "+ Name+" se odpojil.");
                            break;
                        case UpdateFriendList:
                            writer.Write(instance.GetFriendList(Name));
                            break;
                        case FriendRequest:
                            string user = reader.ReadString();
                            if (!instance.IsClientOnline(user))
                                return;
                            ImClient c = instance.GetClient(user);
                            instance.AddFriend(Name, reader.ReadString());
                            c.BinaryWriter.Write(FriendRequest);
                            c.BinaryWriter.Write(Name);
                            c.BinaryWriter.Flush();
                            break;
                        case FriendRequestAccepted:
                            string s = reader.ReadString();
                            instance.AddFriend(s,Name);
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
