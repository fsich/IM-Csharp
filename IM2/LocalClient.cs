using System;
using System.Collections.Concurrent;
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
        public const byte FriendRemove = 13;

        public const int MaxUsernameLength = 32;
        public const int MaxPasswordLength = 32;

        private bool isConnected, mode = false;
        private BinaryReader reader;
        private BinaryWriter writer;
        private NetworkStream stream;
        private Thread thread;
        private TcpClient tcpClient;
        private Window currentMainWin;
        public string Name { get; set; }
        public string Pass { get; set; }

        public ConcurrentDictionary<ChatWin,Int32> ActiveWins = new ConcurrentDictionary<ChatWin,Int32>();  //thread-save collection
        public Label ErrorLabel { get; set; }
   
        private MainWindow loginWin;

        private const string TOOLONG_PW = "Password is too long.";
        private const string TOOLONG_UN = "Password is too long.";
        private const string USERNAME_EXISTS = "Username exists.";
        private const string BAD_P_U = "Wrong username or password.";
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
                thread = new Thread(new ThreadStart(ClientStart));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

            }
        }

        private void PrintError(string msg)
        {
            //přístup k ui vláknu
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ErrorLabel.Content = msg;
            })); 
        }

        private void ClientStart()
        {
            tcpClient = new TcpClient("localhost",3000);
            isConnected = true;
            stream = tcpClient.GetStream();
            
            reader = new BinaryReader(stream, Encoding.UTF8);
            writer = new BinaryWriter(stream, Encoding.UTF8);
            byte response;
            while (isConnected)
            {
                byte packet;
                try {
                    packet = reader.ReadByte();
                } catch(EndOfStreamException e) //pokud se klient odpojil
                {
                    thread.Abort();
                    isConnected = false;
                    return;
                }
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
                                //vytvoí instanci nového okna z ui vlákna
                                currentMainWin.Dispatcher.Invoke(new Action(() =>
                                {
                                    MainScreen win = new MainScreen();
                                    currentMainWin = win;
                                    ErrorLabel = win.GetErrorLabel();
                                    win.Show();
                                    win.UpdateLayout();
                                    loginWin.Close();
                                    SyncFriendList();
                                }));
                                    
                                break;
                            case TooLongPassword:
                                PrintError(TOOLONG_PW);
                                break; 
                            case TooLongUsername:
                                PrintError(TOOLONG_UN);
                                break;
                            case Exists:
                                PrintError(USERNAME_EXISTS);
                                break;
                            case InvalidPassOrName:
                                PrintError(BAD_P_U);
                                break;
                        }
                        break;
                    case Send:
                        string from = reader.ReadString();
                        string msg = reader.ReadString();
                        RcvMessage(from, msg);
                        break;
                    case FriendRequest:
                        string request = reader.ReadString();
                        //přístup k ui vláknu
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                        {
 
                            FRequest fRequest = new FRequest(request);
                            fRequest.Show();
                        }));

                        break;
                    case FriendRequestAccepted:
                        SyncFriendList();
                        break;
                }
            }     
        }
    //synchronizuje lokalni friendlist s databazí
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
    //prida pritele do fl
    public void AcceptFriend(string from)
    {
        writer.Write(FriendRequestAccepted);
        writer.Write(from);
        writer.Flush();
        SyncFriendList();
    }
    //posle friend req
    public void SendFriendRequest(string to){
        writer.Write(FriendRequest);
        writer.Write(to);
        writer.Flush();
    }
    //odesle zrávu
    public void SendMsg(string to, string msg){
        writer.Write(Send);
        writer.Write(to);
        writer.Write(msg);
        writer.Flush();
    }
    //zobrazi zpravu do patřicneho okna
    public void RcvMessage(string from, string msg)
    {
        //nutné požít dispatcher, protože objekt je v jiném vlákně
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
        {
            foreach (var c in ActiveWins)
            {
                if (c.Key.GetTitleLabel().Content.ToString().ToLower().Equals(from.ToLower()))
                {
                    PrintMessage(c.Key, msg,from);
                    return;
                }
            }
            ChatWin win = new ChatWin(from);
            win.Show();
            win.GetTitleLabel().Content = from;
            ActiveWins.TryAdd(win,0);
            PrintMessage(win, msg,from);
        }
            ));

    }
    //zobrazi zpravu a zformatuje text
    public void PrintMessage(ChatWin w, string msg,string from)
    {
      DateTime dt = DateTime.Now;
      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
           w.GetMessageBox()
                .AppendText(Environment.NewLine+String.Format("{0:d/M/yyyy HH:mm:ss}", dt) + " " + from + ": " + msg)));


    }


    public void Disconnect()
    {
         writer.Write(Disconnect_);
         writer.Flush();
    }
    public void RemoveFriend(string name)
    {
        writer.Write(FriendRemove);
        writer.Write(name);
        writer.Flush();
    }
    }
}
