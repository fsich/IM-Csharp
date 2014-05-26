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
            _ErrorLabel.Dispatcher.Invoke(new Action(
                        delegate()
                        {
                            GetErrorLabel().Content=con;
                        }
                    ));
  
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
                    //string selectedItem = ((ListBoxItem)elem).Name;
                    string selectedItem = Friendlist.SelectedItem.ToString();
                    ChatWin w = new ChatWin(selectedItem);
                    w.GetTitleLabel().Content = selectedItem;
                    w.Show();
                    
                    App.LocalClient.ActiveWins.TryAdd(w,0);
                    return;
                }
                elem = (UIElement)VisualTreeHelper.GetParent( elem );
            }
        }

        private void FriendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            string a = FriendRequestBox.Text;
            if (a.Equals(""))
                return;
            App.LocalClient.SendFriendRequest(a);
            FriendRequestBox.Text = "";
        }

        private void FriendRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            App.LocalClient.RemoveFriend(RemoveFriendBox.Text);
            RemoveFriendBox.Text = "";
        }
    }
}
