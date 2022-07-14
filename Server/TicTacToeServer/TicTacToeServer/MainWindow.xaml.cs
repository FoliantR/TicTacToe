#pragma warning disable CS8618,CS8625,CS8605
using System;
using System.Collections;
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
using System.IO;

namespace TicTacToeServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Constructor
        public MainWindow()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        #endregion

        #region Private Members
        
        private ArrayList clientList; // Лист клиентов
        private ArrayList gameList; //Лист лобби игры
        private Socket serverSocket; // Сокет сервера

        private byte[] dataStream = new byte[100000]; //Массив байт для получения и отправки данных

        private delegate void UpdateLogsDelegate(string LogLine); // Делегат для логов сервера
        private UpdateLogsDelegate NewLogs = null; //Объект делегата для логов сервера

        private bool IsRunning = false; //флаг указывающий на работу сервера

        private int gameCount = 0;

        #endregion

        #region Events

        //Нажатие кнопки запуск/остановка сервера
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            //если сервер запущен, останавливаем
            if (IsRunning)
            {
                IsRunning = false;
                StatusBlock.Text = "Статус Сервера: OFFLINE";
                StatusBlock.Foreground = new SolidColorBrush(Colors.Red);
                ToggleButton.Content = "Запустить Сервер";
                LogsBox.Text += "Закрытие сокета и остановка сервера" + Environment.NewLine;
                Server_Stop();
            }
            //если сервер не работает, запускаем
            else
            {
                IsRunning = true;
                StatusBlock.Text = "Статус Сервера: ONLINE";
                StatusBlock.Foreground = new SolidColorBrush(Colors.Green);
                ToggleButton.Content = "Остановить Сервер";
                LogsBox.Text += "Запуск сервера" + Environment.NewLine;
                Server_Load();
            }
        }

        //Нажатие кнопки выхода
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Проверка на остановку сервера при выходе из приложения
        public void OnClose(object sender, EventArgs e)
        {
            if (IsRunning) Server_Stop(); //Если Сервер работаем, сначала останавливаем его
        }

        #endregion

        #region Server Load and Stop

        //Запуск сервера
        private void Server_Load()
        {
            try
            {
                this.clientList = new ArrayList();  //Инициализация массива клиентов
                this.gameList = new ArrayList(); //Инициализация массива игр
                this.NewLogs = new UpdateLogsDelegate(this.UpdateLogs);  // Инициализация делегата для логов сервера
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //Инициализация сокета
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000); //Инициализирую IPEndPoint для сервера, прослушивание на порте 30000
                serverSocket.Bind(server); // Биндим сокет к IPEndPoint сервера
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0); //Инициализация IPEndPoint для клиентов
                EndPoint epSender = (EndPoint)clients; // Инициализация EndPoint для клиентов
                //Начинаем прослушивать на предмет получения данных от клиентов
                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);
            }
            catch (Exception ex)
            {
                StatusBlock.Text = "Error";
                MessageBox.Show("Load Error: " + ex.Message, "UDP Server");
            }
        }

        //Остановка сервера
        private void Server_Stop()
        {
            //Закрытие сокета
            if (serverSocket != null) serverSocket.Close();
        }

        #endregion

        #region Send

        //Отправка пакета клиентам
        void Send(Packet SendPacket, ArrayList SendTo, string LogLine)
        {
            byte[] data = SendPacket.GetDataStream();
            foreach (Client client in SendTo)
            {
                // Отправление пакета клиенту
                serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
            }
            //Логи для сервера 
            this.Dispatcher.Invoke(this.NewLogs, new object[] { LogLine });
        }

        //Завершение отправки данных клиенту
        public void SendData(IAsyncResult asyncResult)
        {
            try
            {
                //Завершение отправки
                serverSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sending Data Error: " + ex.Message, "UDP Server");
            }
        }
        #endregion

        #region Receive
        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                Packet ReceivedPacket = new Packet(this.dataStream); //Инициализация пакета для хранения полученных данных
                Packet AnswerPacket = new Packet(); //Инициализация пакета для отправки ответа
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);  //Инициализация IPEndPoint для клиентов
                EndPoint epSender = (EndPoint)clients; //Инициализация EndPoint клиентов
                serverSocket.EndReceiveFrom(asyncResult, ref epSender); //Получаем все данные
                //Заполняем пакет для отправки ответа
                AnswerPacket.DataType = ReceivedPacket.DataType;
                AnswerPacket.UserName = ReceivedPacket.UserName;
                String logLine;
                switch (ReceivedPacket.DataType)
                {
                    case DataIdentifier.LoginRequest:
                        Client client = new Client();
                        client.endPoint = epSender;
                        client.Name = ReceivedPacket.UserName;
                        //Проверка на занятость имени
                        bool NameFree = true;
                        bool NameCorrect = true;
                        ArrayList NewClient = new ArrayList();
                        NewClient.Add(client);
                        AnswerPacket.DataType = DataIdentifier.LoginConfirmation;
                        foreach (Client row in clientList)
                        {
                            if (row.Name == client.Name) NameFree = false;
                        }
                        if (client.Name == null || client.Name == "") NameCorrect = false;
                        if (NameFree == true && NameCorrect == true)
                        {
                            //Имя прошло проверку, отправка разрешения на вход в игру
                            AnswerPacket.LoginStatus = true;
                            Send(AnswerPacket, NewClient, string.Format("Разрешение на вход узлу {0} под именем {1}", epSender.ToString(), ReceivedPacket.UserName));
                            //Добавление нового клиента в список
                            this.clientList.Add(client);
                            //Отправка сообщения всем клиентам о входе нового игрока
                            AnswerPacket.DataType = DataIdentifier.Message;
                            AnswerPacket.UserName = client.Name;
                            AnswerPacket.Message = string.Format("{0} зашел на сервер", ReceivedPacket.UserName);
                            Send(AnswerPacket, clientList, AnswerPacket.Message);
                        }
                        else
                        {
                            //Имя недопустимо, вход пользователю не разрешается
                            if (NameFree) logLine = String.Format("Попытка узла {0} зайти под недопустимым именем", epSender.ToString());
                            else logLine = String.Format("Попытка узла {0} зайти под занятым именем {1}", epSender.ToString(), client.Name);
                            AnswerPacket.LoginStatus = false;
                            Send(AnswerPacket, NewClient, logLine);
                        }
                        break;

                    case DataIdentifier.LogOut:
                        //Удаление клиента из списка
                        foreach (Client c in this.clientList)
                        {
                            if (c.endPoint.Equals(epSender))
                            {
                                this.clientList.Remove(c);
                                break;
                            }
                        }
                        //Отправка всем клиентам сообщения о выходе игрока из игры
                        AnswerPacket.DataType = DataIdentifier.Message;
                        AnswerPacket.Message = string.Format("{0} вышел с сервера", ReceivedPacket.UserName);
                        Send(AnswerPacket, clientList, AnswerPacket.Message);
                        //Отправка всем клиентам списка пользователей
                        SendClientList();
                        break;
                    case DataIdentifier.ClientListRequest:
                        SendClientList();
                        break;
                    case DataIdentifier.Message:
                        //отправка сообщения всем клиентам
                        AnswerPacket.Message = string.Format("{0}: {1}", ReceivedPacket.UserName, ReceivedPacket.Message);
                        Send(AnswerPacket, clientList, AnswerPacket.Message);
                        break;
                    case DataIdentifier.Invite:
                        //Проверка корректности приглашения и его отправка
                        Client invite_sender = new Client();
                        Client invite_recipent = new Client();
                        bool sender_free = false;
                        bool recipent_free = false;
                        foreach(Client row in clientList)
                        {
                            if (row.Name == ReceivedPacket.UserName && row.endPoint.Equals(epSender))
                            {
                                invite_sender = row;
                                if(row.GameId==0) sender_free = true;
                            }
                            if (row.Name == ReceivedPacket.Invite)
                            {
                                invite_recipent = row;
                                if(row.GameId==0) recipent_free = true;
                            }
                        }
                        if (sender_free)
                        {
                            if (recipent_free)
                            {
                                AnswerPacket = ReceivedPacket;
                                string inviteLogLine = "Отправка приглашения в игру от пользователя " + AnswerPacket.UserName + " пользователю " + AnswerPacket.Invite;
                                ArrayList RecipentList = new ArrayList();
                                RecipentList.Add(invite_recipent);
                                Send(AnswerPacket, RecipentList, inviteLogLine);
                            }
                            else
                            {
                                AnswerPacket.DataType = DataIdentifier.Message;
                                AnswerPacket.UserName = ReceivedPacket.UserName;
                                AnswerPacket.Message = "Ошибка отправки приглашения. Пользователь " + ReceivedPacket.Invite + " уже находится в игре или вышел с сервера";
                                ArrayList RecipentList = new ArrayList();
                                RecipentList.Add(invite_sender);
                                string inviteLogLine = "Ошибка отправки приглашения от пользователя " + ReceivedPacket.UserName + " пользователю " + ReceivedPacket.Invite + ". Получатель вышел с сервера или уже находится в игре";
                                Send(AnswerPacket, RecipentList, inviteLogLine);
                            }
                        }
                        break;
                    case DataIdentifier.InviteAnswer:
                        bool P1_free = false;
                        bool P2_free = false;
                        bool P1_online = false;
                        bool P2_online = false;
                        Client P1 = new Client();
                        Client P2 = new Client();
                        foreach (Client row in clientList)
                        {
                            if (row.Name == ReceivedPacket.UserName)
                            {
                                P1 = row;
                                P1_online = true;
                                if (row.GameId == 0) P1_free = true;
                            }
                            if (row.Name == ReceivedPacket.Invite && row.endPoint.Equals(epSender))
                            {
                                P2 = row;
                                P2_online = true;
                                if (row.GameId == 0) P2_free = true;
                            }
                        }
                        if (ReceivedPacket.InviteAnswer)
                        {
                            //Приглашение принято. Проверка, что первый игрок все еще свободен и начало игры
                            if (P2_free)
                            {
                                if (P1_free)
                                {
                                    AnswerPacket.DataType = DataIdentifier.Message;
                                    AnswerPacket.UserName = ReceivedPacket.Invite;
                                    AnswerPacket.Message = "Пользователь " + ReceivedPacket.Invite + " принял ваше приглашение в игру!";
                                    ArrayList RecipentList = new ArrayList();
                                    RecipentList.Add(P1);
                                    string inviteLogLine = "Пользователь " + ReceivedPacket.Invite + " принял приглашение в игру от пользователя " + ReceivedPacket.UserName;
                                    Send(AnswerPacket, RecipentList, inviteLogLine);
                                    //Создать новое игровое лобби
                                    CreateNewGame(P1, P2);
                                    SendClientList();
                                }
                                else
                                {
                                    AnswerPacket.DataType = DataIdentifier.Message;
                                    AnswerPacket.UserName = ReceivedPacket.UserName;
                                    AnswerPacket.Message = "Невозможно принять приглашение. Пользователь " + ReceivedPacket.UserName + " уже находится в игре или вышел с сервера";
                                    ArrayList RecipentList = new ArrayList();
                                    RecipentList.Add(P2);
                                    string inviteLogLine = "Ошибка принятия приглашения от пользователя " + ReceivedPacket.UserName + " пользователю " + ReceivedPacket.Invite + ". Отправитель вышел с сервера или уже находится в игре";
                                    Send(AnswerPacket, RecipentList, inviteLogLine);
                                }
                            }
                        }
                        else
                        {
                            //Приглашение отклонено. Отправка первому игроку отклонения приглашения
                            if(P1_online && P2_online)
                            {
                                AnswerPacket.DataType = DataIdentifier.Message;
                                AnswerPacket.UserName = ReceivedPacket.Invite;
                                AnswerPacket.Message = "Пользователь " + ReceivedPacket.Invite + " отклонил ваше приглашение в игру";
                                ArrayList RecipentList = new ArrayList();
                                RecipentList.Add(P1);
                                string inviteLogLine = "Пользователь " + ReceivedPacket.Invite + " отклонил приглашение в игру от пользователя " + ReceivedPacket.UserName;
                                Send(AnswerPacket, RecipentList, inviteLogLine);
                            }
                        }
                        break;
                    case DataIdentifier.Turn:
                        //Обработка хода 
                        bool TurnCorrect = false;
                        Client Sender = new Client();
                        foreach(Client row in clientList)
                        {
                            if (row.endPoint.Equals(epSender)) Sender = row;
                        }
                        foreach(GameLobby game in gameList)
                        {
                            if (game.gameID == Sender.GameId && !game.gameFinished)
                            {
                                if (ReceivedPacket.Turn == 0)
                                {
                                    game.gameFinished = true;
                                    //Окончание игры по причине выхода игрока
                                    AnswerPacket.DataType = DataIdentifier.GameEnd;
                                    if (Sender == game.pX)
                                    {
                                        AnswerPacket.Result = -1;
                                        ArrayList lastPlayer = new ArrayList();
                                        lastPlayer.Add(game.pO);
                                        string logLine_pX_left = "Окончание игры GameId = " + game.gameID + " по причине выхода игрока " + game.pX.Name;
                                        Send(AnswerPacket, lastPlayer, logLine_pX_left);
                                    }
                                    else
                                    {
                                        AnswerPacket.Result = -2;
                                        ArrayList lastPlayer = new ArrayList();
                                        lastPlayer.Add(game.pX);
                                        string logLine_pO_left = "Окончание игры GameId = " + game.gameID + " по причине выхода игрока " + game.pO.Name;
                                        Send(AnswerPacket, lastPlayer, logLine_pO_left);
                                    }
                                    foreach (Client row in clientList)
                                    {
                                        if (row.endPoint.Equals(Sender.endPoint)) row.GameId = 0;
                                    }
                                    SendClientList();
                                }
                                else
                                {
                                    //Ход - это не выход из игры
                                    if (Sender == game.pX && game.Xturn && game.field[ReceivedPacket.Turn] == 0)
                                    {
                                        game.field[ReceivedPacket.Turn] = 1;
                                        TurnCorrect = true;
                                    }
                                    else if (Sender == game.pO && !game.Xturn && game.field[ReceivedPacket.Turn] == 0)
                                    {
                                        game.field[ReceivedPacket.Turn] = 2;
                                        TurnCorrect = true;
                                    }
                                    if (TurnCorrect)
                                    {
                                        //Отправка игрового поля
                                        game.Xturn = !game.Xturn;
                                        if (game.Xturn) game.field[0] = 1;
                                        else game.field[0] = 2;
                                        AnswerPacket.DataType = DataIdentifier.Field;
                                        AnswerPacket.Field = game.field;
                                        ArrayList playersInLobby = new ArrayList();
                                        playersInLobby.Add(game.pX);
                                        playersInLobby.Add(game.pO);
                                        string LoglineFieldUpdate = "Отправка игрового поля в игре GameID = " + game.gameID + " Текущее поле:";
                                        for (int i = 0; i < 10; i++)
                                        {
                                            LoglineFieldUpdate += " " + AnswerPacket.Field[i];
                                        }
                                        Send(AnswerPacket, playersInLobby, LoglineFieldUpdate);
                                        //Проверка на окончание партии по причине победы одной из сторон
                                        bool wonX = false;
                                        bool wonO = false;
                                        //Проверка на победу X (8 комбинаций)
                                        int[][] WinCombinations = { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, new int[] { 1, 4, 7 }, new int[] { 2, 5, 8 }, new int[] { 3, 6, 9 }, new int[] { 1, 5, 9 }, new int[] { 3, 5, 7 } };
                                        for(int i = 0; i < 8; i++)
                                        {
                                            if(game.field[WinCombinations[i][0]] == 1 && game.field[WinCombinations[i][1]] == 1 && game.field[WinCombinations[i][2]]==1)
                                            {
                                                //Победили крестики
                                                wonX = true;
                                                AnswerPacket.WinCells[0] = WinCombinations[i][0];
                                                AnswerPacket.WinCells[1] = WinCombinations[i][1];
                                                AnswerPacket.WinCells[2] = WinCombinations[i][2];
                                            }
                                            else if(game.field[WinCombinations[i][0]] == 2 && game.field[WinCombinations[i][1]] == 2 && game.field[WinCombinations[i][2]] == 2)
                                            {
                                                //Победили нолики
                                                wonO = true;
                                                AnswerPacket.WinCells[0] = WinCombinations[i][0];
                                                AnswerPacket.WinCells[1] = WinCombinations[i][1];
                                                AnswerPacket.WinCells[2] = WinCombinations[i][2];
                                            }
                                        }
                                        if (wonX)
                                        {
                                            //Победа крестиков
                                            game.gameFinished = true;
                                            //Окончание игры по причине победы крестиков
                                            AnswerPacket.DataType = DataIdentifier.GameEnd;
                                            AnswerPacket.Result = 1;
                                            ArrayList players = new ArrayList();
                                            players.Add(game.pX);
                                            players.Add(game.pO);
                                            string logLine_Xwin = "Окончание игры GameId = " + game.gameID + " по причине победы игрока " + game.pX.Name + " (Крестики)";
                                            Send(AnswerPacket, players, logLine_Xwin);
                                        }
                                        else if (wonO)
                                        {
                                            //Победа ноликов
                                            game.gameFinished = true;
                                            //Окончание игры по причине победы ноликов
                                            AnswerPacket.DataType = DataIdentifier.GameEnd;
                                            AnswerPacket.Result = 2;
                                            ArrayList players = new ArrayList();
                                            players.Add(game.pX);
                                            players.Add(game.pO);
                                            string logLine_Owin = "Окончание игры GameId = " + game.gameID + " по причине победы игрока " + game.pO.Name + " (Нолики)";
                                            Send(AnswerPacket, players, logLine_Owin);
                                        }
                                        else
                                        {
                                            //Проверка на окончание партии по причине ничьи
                                            bool draw = true;
                                            for (int i = 1; i < 10; i++)
                                            {
                                                if (game.field[i] == 0) draw = false;
                                            }
                                            if (draw)
                                            {
                                                game.gameFinished = true;
                                                //Окончание игры по причине ничьей
                                                AnswerPacket.DataType = DataIdentifier.GameEnd;
                                                AnswerPacket.Result = 0;
                                                ArrayList players = new ArrayList();
                                                players.Add(game.pX);
                                                players.Add(game.pO);
                                                string logLine_draw = "Окончание игры GameId = " + game.gameID + " по причине ничьей";
                                                Send(AnswerPacket, players, logLine_draw);
                                            }
                                        } 
                                    }
                                }
                                
                            }
                        }
                        break;
                    case DataIdentifier.Rejoin:
                        String RejoinLog = "";
                        foreach(Client row in clientList)
                        {
                            if (row.endPoint.Equals(epSender))
                            {
                                foreach(GameLobby game in gameList)
                                {
                                    if (game.gameID == row.GameId && game.gameFinished == true)
                                    {
                                        row.GameId = 0;
                                        RejoinLog = "Возвращение игрока " + row.Name + " в главное меню";
                                    }
                                }
                            }
                        }
                        this.Dispatcher.Invoke(this.NewLogs, new object[] { RejoinLog });
                        SendClientList();
                        break;
                    default:
                        break;
                }
                //Заново прослушиваем на входящее соединение
                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(this.ReceiveData), epSender);
            }
            catch (ObjectDisposedException)
            {
                //EndReceiveFrom() всегда выдает исключение ObjectDisposedException при закрытии сокета,
                //т. к. вызов Close() для сокета также выполняет Dispose(), поэтому необходимо обрабатывать исключение при закрытии сокета
            }
            catch (Exception ex)
            {
                MessageBox.Show("Receiving Data Error: " + ex.Message, "GameServer");
            }
        }

        #endregion

        #region Methods

        //Отправка всем клиентам списка клиентов
        public void SendClientList()
        {
            Packet AnswerPacket = new Packet();
            string logLine;
            List<Player> playerList = new List<Player>();
            foreach (Client row in clientList)
            {
                playerList.Add(new Player(row));
            }
            AnswerPacket.DataType = DataIdentifier.ClientList;
            AnswerPacket.players_list = new List<Player>(playerList);
            logLine = "Отправка списка пользователей всем клиентам";
            Send(AnswerPacket, clientList, logLine);
        }

        public void CreateNewGame(Client P1, Client P2)
        {
            gameCount++;
            Random rand = new Random();
            if(rand.Next(2)>0)
            {
                Client swap;
                swap = P1;
                P1 = P2;
                P2 = swap;
            }
            GameLobby NewGameLobby = new GameLobby(P1, P2, gameCount);
            gameList.Add(NewGameLobby);
            string newGameLog = "Создание игры с пользователями " + P1.Name + " и " + P2.Name;
            this.Dispatcher.Invoke(this.NewLogs, new object[] { newGameLog });
            foreach (Client row in clientList)
            {
                if (row.Name == P1.Name) row.GameId = NewGameLobby.gameID;
                if (row.Name == P2.Name) row.GameId = NewGameLobby.gameID;
            }
            Packet SendData = new Packet();
            SendData.DataType = DataIdentifier.GameStart;
            SendData.UserName = P1.Name;
            SendData.Invite = P2.Name;
            ArrayList RecipentList = new ArrayList();
            RecipentList.Add(P1);
            RecipentList.Add(P2);
            String logline = "Начало игры GameID = " + NewGameLobby.gameID;
            Send(SendData, RecipentList, logline);
        }

        #endregion

        #region Logs Update

        //Метод для добавления строки логов
        void UpdateLogs(string LogLine)
        {
            LogsBox.Text += LogLine + Environment.NewLine;
        }
         
        #endregion
    }
}
