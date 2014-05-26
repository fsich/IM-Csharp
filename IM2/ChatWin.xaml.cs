using System;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IM2
{
    /// <summary>
    /// Interaction logic for ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {
        private readonly string with;
        public ChatWin(string with)
        {
            this.with = with;
            InitializeComponent();
            NameLabel.Content += with;
        }

        public Label GetTitleLabel()
        {
            return NameLabel;
        }

        public RichTextBox GetMessageBox()
        {
            return MessageBox;
        }

        public TextBox GetSendBox()
        {
            return SendBox;
        }

        private void onChatWinClose(object sender, RoutedEventArgs e)
        {
            Int32 i;
            App.LocalClient.ActiveWins.TryRemove(this,out i);
            Close();
            e.Handled = true;
            
        }

        private void SendEvent(object sender, RoutedEventArgs e)
        {
            App.LocalClient.SendMsg(with,SendBox.Text);
            App.LocalClient.PrintMessage(this, SendBox.Text, App.LocalClient.Name);
            SendBox.Text = "";
            
        }

        private void SendBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            App.LocalClient.SendMsg(with, SendBox.Text);
            App.LocalClient.PrintMessage(this, SendBox.Text, App.LocalClient.Name); //odesle zpravu sam sobe
            SendBox.Text = "";
        }

        
    }
}
