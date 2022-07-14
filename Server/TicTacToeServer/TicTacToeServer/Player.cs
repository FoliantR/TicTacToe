using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeServer
{
    public class Player
    {
        public Player()
        {
            Name = "NULL";
            InGame = true;
        }
        public Player(string name_)
        {
            Name = name_;
            InGame = false;
        }
        public Player(string name_, bool InGame_)
        {
            Name = name_;
            InGame = InGame_;
        }
        public string Name;
        public bool InGame;
        public Player(Client client)
        {
            this.Name = client.Name;
            if (client.GameId == 0) this.InGame = false;
            else this.InGame = true;
        }
    }
}
