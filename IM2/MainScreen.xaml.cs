using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IM2
{
    /// <summary>
    /// Interaction logic for MainScreen.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        public MainScreen()
        {
            InitializeComponent();
        }

        public ListBox GetFriendlist()
        {
            return Friendlist;
        }
        public void setErrorContent(string con)
        {
            Thread thread = new System.Threading.Thread(new ThreadStart(
                delegate()
                {
                    GetErrorLabel().Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.SystemIdle,
                      TimeSpan.FromSeconds(1),
                      new Action(
                        delegate()
                        {
                            GetErrorLabel().Content=con;
                        }
                    ));
                }
            ));
            thread.Start();
        }



    private void Win_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public Label GetErrorLabel()
        {
            return _ErrorLabel;
        }

        private void Friendlist_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UIElement elem = (UIElement)Friendlist.InputHitTest(e.GetPosition(Friendlist));
            while ( elem != Friendlist )
            {
                if ( elem is ListBoxItem )
                {
                    string selectedItem = ((ListBoxItem)elem).Name;
                    ChatWin w = new ChatWin();
                    w.GetTitleLabel().Content = selectedItem;
                    w.Show();
                    
                    App.LocalClient.ActiveWins.Add(w);
                    return;
                }
                elem = (UIElement)VisualTreeHelper.GetParent( elem );
            }
        }

        private void FriendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            string a = FriendRequestBox.Text;
            if (a.Equals(""))
                GetErrorLabel().Content = "You have to specify name";
            App.LocalClient.SendFriendRequest(a);
        }
    }
}
