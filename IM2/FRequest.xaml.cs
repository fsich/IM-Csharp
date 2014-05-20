using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IM2
{
    /// <summary>
    /// Interaction logic for FRequest.xaml
    /// </summary>
    public partial class FRequest : Window
    {
        public FRequest()
        {
            InitializeComponent();
        }

        public FRequest(string request) : this()
        {
            GetLabel().Content += request;
        }

        public Label GetLabel()
        {
            return FriendRequestLabel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string a = FriendRequestLabel.ContentStringFormat;
            App.LocalClient.AcceptFriend(a.Split(':')[1]);
            Close();
            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
