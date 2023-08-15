using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UdpClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UdpClientViewModel _viewModel;
        public MainWindow()
        {
            _viewModel = new UdpClientViewModel();
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void OnStartSendingButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.InitializeUdpClient();
                _viewModel.StartPacketsSending();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _viewModel.ChannelBandwidth = (int)e.NewValue;
        }
    }
}
