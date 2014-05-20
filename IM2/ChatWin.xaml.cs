using System.Windows;
using System.Windows.Controls;

namespace IM2
{
    /// <summary>
    /// Interaction logic for ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {
        public ChatWin()
        {
            InitializeComponent();
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
            App.LocalClient.ActiveWins.Remove(this);
        }

        private void SendEvent(object sender, RoutedEventArgs e)
        {
            App.LocalClient.SendMsg(Title,GetSendBox().Text);
        }
    }
}
