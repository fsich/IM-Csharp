using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace IM2
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        public Label GetErrorLabel()
        {
            return _ErrorLabel;
        }
        

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_PasswordTextBox.Text.Equals("Password") || _PasswordTextBox.Text.Equals(""))
            {
                _ErrorLabel.Content = "Invalid Pass";
            }
            if (_UsernameTextBox.Text.Equals("Username") || _UsernameTextBox.Text.Equals(""))
            {
                _ErrorLabel.Content = "Invalid Username";
            }
            LocalClient localClient = App.LocalClient = new LocalClient();
            localClient.LogIn(_UsernameTextBox.Text, _PasswordTextBox.Text,false,_ErrorLabel,this);

        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_PasswordTextBox.Text.Equals("Password") || _PasswordTextBox.Text.Equals(""))
            {
                _ErrorLabel.Content = "Invalid Pass";
            }
            if (_UsernameTextBox.Text.Equals("Username") || _UsernameTextBox.Text.Equals(""))
            {
                _ErrorLabel.Content = "Invalid Username";
            }
            LocalClient localClient = App.LocalClient = new LocalClient();
            localClient.LogIn(_UsernameTextBox.Text, _PasswordTextBox.Text, true, _ErrorLabel, this);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            // your event handler here
            e.Handled = true;
            _ErrorLabel.Content = "Enter pressed";
        }


    }
}
