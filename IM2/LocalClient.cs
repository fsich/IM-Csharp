using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IM2
{
    public class LocalClient
    {
        public const byte ClientConnect = 100;
        public const byte OK = 0;
        public const byte Login = 1;
        public const byte WillRegister = 2;
        public const byte TooLongUsername = 3;
        public const byte TooLongPassword = 4;
        public const byte Exists = 5;
        public const byte Send = 6;
        public const byte Disconnect_ = 7;
        public const byte AddFriend = 8;
        public const byte InvalidPassOrName = 9;
        public const byte UpdateFriendList = 10;
        public const byte FriendRequest = 11;
        public const byte FriendRequestAccepted = 12;
        public const int MaxUsernameLength = 32;
        public const int MaxPasswordLength = 32;
        

        private bool isConnected, mode = false;
        private List<string> friendList = new List<string>();
        private BinaryReader reader;
        private BinaryWriter writer;
        private NetworkStream stream;
        private Thread thread;
        private TcpClient tcpClient;
        private Window currentMainWin;
        public string Name { get; set; }
        public string Pass { get; set; }

        public List<ChatWin> ActiveWins = new List<ChatWin>();  
        public Label ErrorLabel { get; set; }
        private MainWindow loginWin;
        public void LogIn(string username, string pass, bool mode, Label l, MainWindow win)
        {
            ErrorLabel = l;
            this.loginWin = win;
            if (!isConnected)
            {
                isConnected = true;
                Name = username;
                Pass = pass;
                this.mode = mode;
                this.currentMainWin = win;
                ErrorLabel = win.GetErrorLabel();
                thread  = new Thread(new ThreadStart(ClientStart));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private void ChangeAsyncErrorContent(string msg)
        {
            ErrorLabel.Dispatcher.Invoke(new Action(() => { ErrorLabel.Content = msg; })); 
        }

        private void ClientStart()
        {
            tcpClient = new TcpClient("localhost",3000);
            isConnected = true;
            stream = tcpClient.GetStream();
            reader = new BinaryReader(stream, Encoding.UTF8);
            writer = new BinaryWriter(stream, Encoding.UTF8);
            byte response = Exists;
            while (isConnected) {
                byte packet = reader.ReadByte();
                switch (packet)
                {
                    case ClientConnect:
                        writer.Write(ClientConnect);
                        writer.Write(Name);
                        writer.Write(Pass);
                        writer.Write(mode);
                        writer.Flush();
                        response = reader.ReadByte();
                        switch (response)
                        {
                            case OK:

                                currentMainWin.Dispatcher.Invoke(new Action(() =>
                                {
                                    MainScreen win = new MainScreen();
                                    currentMainWin = win;
                                    ErrorLabel = win.GetErrorLabel();
                                    win.Show();
                                    loginWin.Close();
                                    SyncFriendList();
                                }));
                                    
                                break;
                            case TooLongPassword:
                                ChangeAsyncErrorContent("Password is too long.");
                                break; 
                            case TooLongUsername:
                                ChangeAsyncErrorContent("Username is too long.");
                                break;
                            case Exists:
                                ChangeAsyncErrorContent("Username exists.");
                                break;
                            case InvalidPassOrName:
                                ChangeAsyncErrorContent("Wrong username or password.");
                                break;
                        }
                        break;
                    //[from][msg]
                    case Send:
                        string from = reader.ReadString();
                        string msg = reader.ReadString();
                        RcvMessage(from,msg);
                        break;
                    case FriendRequest:
                        string request = reader.ReadString();
                        FRequest fRequest = new FRequest(request);
                        fRequest.Show();
                        break;
                    case FriendRequestAccepted:
                        SyncFriendList();
                        break;
                }
            }     
        }

    public void SyncFriendList()
    {
        if (currentMainWin == null) return;
        if (currentMainWin is MainScreen){
            writer.Write(UpdateFriendList);
            writer.Flush();
            string f = reader.ReadString();
            string[] ff = f.Split();
            for (int i = 0; i <= ff.Length - 1; i++)
                if (!((MainScreen)currentMainWin).GetFriendlist().Items.Contains(ff[i]))
                    ((MainScreen)currentMainWin).GetFriendlist().Items.Add(ff[i]);
        }
    }

    public void AcceptFriend(string from)
    {
        writer.Write(FriendRequestAccepted);
        writer.Write(from);
        writer.Flush();
        SyncFriendList();
    }

    public void SendFriendRequest(string to){
        writer.Write(FriendRequest);
        writer.Write(to);
        writer.Flush();
    }

    public void SendMsg(string to, string msg){
        writer.Write(Send);
        writer.Write(to);
        writer.Write(msg);
        writer.Flush();
    }

    public void RcvMessage(string from, string msg){
        foreach (var c in ActiveWins)
        {
            if (c.Title.ToLower().Equals(from.ToLower()))
            {
                PrintMessage(c,msg);
                return;
            }
        }

        ChatWin win = new ChatWin();
        win.Title = from;
        ActiveWins.Add(win);
        PrintMessage(win,msg);
    }

    private void PrintMessage(ChatWin w, string msg)
    {
        DateTime dt = DateTime.Now;
        w.GetMessageBox().AppendText(String.Format("{0:d/M/yyyy HH:mm:ss}", dt)+" "+w.Title+": "+msg+"\n\r");
    }


    public void Disconnect()
    {
         writer.Write(Disconnect_);
         writer.Flush();
    }
    }
}
