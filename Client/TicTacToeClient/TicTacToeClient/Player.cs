using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeClient
{
    public class Player
    {
        public Player()
        {
            Name = "NULL";
            InGame = false;
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
    }
}
