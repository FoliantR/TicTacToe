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

namespace TicTacToeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ip_box.Text = "127.0.0.1";
            this.port_box.Text = "30000";
            port_box.IsEnabled = false;
        }
        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionStatus.ConnectionBroken = false;
            string user = login_box.Text;
            string IP;
            if (String.IsNullOrEmpty(ip_box.Text)) IP = "127.0.0.1";
            else IP = ip_box.Text;
            string Port;
            if (String.IsNullOrEmpty(port_box.Text)) Port = "30000";
            else Port = port_box.Text;
            GameWindow gameWindow = new GameWindow(user,IP,Port);
            this.Hide();
            if (!ConnectionStatus.ConnectionBroken) gameWindow.ShowDialog();
            if (ConnectionStatus.ConnectionBroken) this.Show();
            else this.Close();
        }
        private void exit_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
