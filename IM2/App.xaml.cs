using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IM2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static LocalClient LocalClient;
        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            LocalClient.Disconnect();
            this.Shutdown();
        }

        private void Minimalize(object sender, RoutedEventArgs e)
        {
            this.MainWindow.WindowState = WindowState.Minimized;
        }

        private void UnameTbMouseEnter(object sender, MouseEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            if (tb.Text.Equals("Username"))
                tb.Text = "";
        }

        private void UnameTbMouseLeave(object sender, MouseEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Equals(""))
                tb.Text = "Username";
        }

        private void PassTbMouseEnter(object sender, MouseEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Equals("Password"))
                tb.Text = "";
        }

        private void PassTbMouseLeave(object sender, MouseEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Equals(""))
                tb.Text = "Password";
        }
        
        private void Window_click(object sender, MouseButtonEventArgs e)
        {
            ((Window)sender).DragMove();
        }
    }
}
