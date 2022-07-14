using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeServer
{
    public class GameLobby
    {
        public GameLobby(Client p1, Client p2, int ID)
        {
            pX = p1;
            pO = p2;
            Xturn = true;
            gameID = ID;
            field = new int[10];
            for(int i = 0; i < 10; i++)
            {
                field[i] = 0;
            }
            gameFinished = false;
        }
        public bool gameFinished;
        public int gameID;
        public int[] field; //0 - свободна 1 - крестики 2 - нолики
        public Client pX;
        public Client pO;
        public bool Xturn; //true если ход крестиков, false если ход ноликов
    }
}
