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
        private string request;
        public FRequest()
        {
            InitializeComponent();
        }

        public FRequest(string request) : this()
        {
            GetLabel().Content += request;
            this.request = request;
        }

        public Label GetLabel()
        {
            return FriendRequestLabel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            App.LocalClient.AcceptFriend(request);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
