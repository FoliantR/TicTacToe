using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace TicTacToeClient
{
    /// <summary>
    /// Логика взаимодействия для InviteWindow.xaml
    /// </summary>
    public partial class InviteWindow : Window
    {
        public InviteWindow(String inviterName)
        {
            InitializeComponent();
            InviterBlock.Text = "Вы получили приглашение в игру от " + inviterName;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            InviteAccept.InviteAns = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            InviteAccept.InviteAns = false;
            this.Close();
        }
    }
}
