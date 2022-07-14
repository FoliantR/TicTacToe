#pragma warning disable CS8618,CS8625,CS8605
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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
namespace TicTacToeServer
{
    public class Client
    {
        public Client()
        {
            GameId = 0;
        }
        public Client(EndPoint Epoint,string name_, int GameID_)
        {
            endPoint = Epoint;
            Name = name_;
            GameId = GameID_;
        }
        public EndPoint endPoint;
        public string Name;
        public int GameId;
    }
}
