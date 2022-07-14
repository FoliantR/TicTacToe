#pragma warning disable CS8618,CS8625
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
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace TicTacToeClient
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        #region Private Members

        private bool ForcedExit = false; //Флаг для экстренного завершения приложения
        private string username = "USER"; //Имя пользователя (передается из главного окна)
        private Player PreviousSelectedPlayer = null;
        private bool InviteAccepted = false;
        private bool role; //1 - крестик, 0 - ноль
        private bool turn; //1- ход клиента, 0 - ход опонента
        private int[] field = new int[10];
        private String Opponent = "sampleNAME";
        private int InGame = 0; //0 - главное меню, 1 - в игре, 2 - завершение игры
        ArrayList Buttons = new ArrayList(); //Лист для изменения кнопок

        private Socket clientSocket; // Сокет клиента
        private EndPoint epServer; // EndPoint Сервера
        private byte[] dataStream = new byte[100000];  //Массив байт для получения и отправки данных

        //Инициализация типов делегатов
        private delegate void DelegateString(string str);
        private delegate void DelegateListPlayer(List<Player> players);
        private delegate void DelegateBool(bool flag);
        private delegate void DelegateInt(int Num);
        private delegate void DelegateIntArray(int[] IntArray);

        //Инициализация объектов делегатов
        private DelegateString displayMessageDelegate; // displayMessageDelegate - делегат для вывода сообщений
        private DelegateString exitDelegate; //ExitDelegate - Делегат для закрытия окна
        private DelegateString InviteWindowDelegate; // InviteWindowDelegate - делегат для открытия окна приглашения
        private DelegateString UpdateGameLineDelegate; //Делегат для обновления строки с информацией об игре
        private DelegateInt EndGameInfoDelegate; //Делегат для отображения строки о завершении игры
        private DelegateString LeaveButtonDelegate; //Делегат для переключения текста на кнопке выхода
        private DelegateListPlayer displayPlayerDelegate; // displayPlayerDelegate - делегат для вывода списка пользователей
        private DelegateBool UIcontrolDelegate; // UIcontrolDelegate - делегат для переключения интерфейса
        private DelegateBool FieldClearDelegate; //Делегат для очистки поля
        private DelegateString FieldControlDelegate; //FieldControlDelegate - делегат для отображения поля
        private DelegateIntArray ColorCellsDelegate; //Делегат для перекраски кнопок
        #endregion

        #region Public Members

        public List<Player> playerList;
        string IPstring;
        string PortString;
        void SetUser(string new_username)
        {
            username = new_username;
        }

        #endregion

        #region Window Constructor

        public GameWindow(string UserName, string IP, string Port)
        {
            InitializeComponent();
            //Добавление кнопок в ArrayList
            Buttons.Add(B1);
            Buttons.Add(B2);
            Buttons.Add(B3);
            Buttons.Add(B4);
            Buttons.Add(B5);
            Buttons.Add(B6);
            Buttons.Add(B7);
            Buttons.Add(B8);
            Buttons.Add(B9);
            SetUser(UserName);
            IPstring = IP;            
            PortString = Port;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.displayMessageDelegate = new DelegateString(this.DisplayMessage); //Создание делегата для метода вывода сообщений
            this.exitDelegate = new DelegateString(this.ExitHandler); //Создание делегата для закрытия окна при ошибке
            this.displayPlayerDelegate = new DelegateListPlayer(this.DisplayPlayerList); //Создание делегата для вывода списка пользователей
            this.InviteWindowDelegate = new DelegateString(this.DisplayInviteWindow); //Создание делегата для открытия окна приглашения
            this.UIcontrolDelegate = new DelegateBool(this.UIcontrol); //Создание делегата для управления интерфейсом
            this.UpdateGameLineDelegate = new DelegateString(this.UpdateGameLine); //создание делегата для обновления строки с информацией об игре
            this.FieldControlDelegate = new DelegateString(this.FieldControl); //Создание делегата для управления полем
            this.LeaveButtonDelegate = new DelegateString(this.LeaveButtonSwitch); // Создание деелагата для переключения кнопки выхода
            this.EndGameInfoDelegate = new DelegateInt(this.EndGameInfo); //Создание делегата для отображения строки о завершении игры
            this.FieldClearDelegate = new DelegateBool(this.FieldClear); // Создание делегата для очистки поля
            this.ColorCellsDelegate = new DelegateIntArray(this.ColorCells); //Создание делегата для перекраски кнопок
            try
            {
                //Создание пакета для отправки серверу
                Packet sendData = new Packet();
                sendData.UserName = this.username;
                sendData.DataType = DataIdentifier.LoginRequest; //Сначала клиент отправляет запрос на присоединение к игре
                //Инициализация сокета
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //Инициализация IP-адреса сервера из аргумента окна
                IPAddress serverIP = IPAddress.Parse(IP.Trim());
                int serverPort = Convert.ToInt32(Port);
                //Инициализация IPEndPoint сервера
                IPEndPoint server = new IPEndPoint(serverIP, serverPort);
                //Инициализация EndPoint сервера
                epServer = (EndPoint)server;
                //Преобразование пакета в байты
                byte[] data = sendData.GetDataStream();
                //Отправка данных на сервер
                clientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                //Инициализация потока байт для получения данных от сервера
                this.dataStream = new byte[100000];
                //Начинаем прослушивать на предмет данных от сервера
                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (Exception ex)
            {
                string ExitReason = "ConnectionError";
                MessageBox.Show("Connection Error: " + ex.Message, "GameClient");
                this.Dispatcher.Invoke(this.exitDelegate, new object[] { ExitReason });
            }
        }

        #endregion

        #region Send And Receive
        private void SendData(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                string ExitReason = "ConnectionError";
                MessageBox.Show("Send Data Error: " + ex.Message, "GameClient");
                this.Dispatcher.Invoke(this.exitDelegate, new object[] { ExitReason });
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                // Получить все данные
                this.clientSocket.EndReceive(ar);
                // Создаем пакет для хранения полученных данных
                Packet receivedData = new Packet(this.dataStream);
                //В зависимости от типа данных обрабатываем пакет
                switch (receivedData.DataType)
                {
                    case DataIdentifier.LoginConfirmation:
                        if (receivedData.LoginStatus)
                        {
                            //Получено разрешение на вход от сервера
                            Packet ClientRequest = new Packet();
                            ClientRequest.DataType = DataIdentifier.ClientListRequest;
                            byte[] data = ClientRequest.GetDataStream();
                            clientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                        }
                        else
                        {
                            //Сервер не дал разрешение на вход
                            string ExitReason = "LoginDenied";
                            this.Dispatcher.Invoke(this.exitDelegate, new object[] { ExitReason });
                        }
                        break;
                    case DataIdentifier.ClientList:
                        this.Dispatcher.Invoke(this.displayPlayerDelegate, new object[] { new List<Player>(receivedData.players_list) });
                        break;
                    case DataIdentifier.Message:
                        if (!String.IsNullOrEmpty(receivedData.Message)) this.Dispatcher.Invoke(this.displayMessageDelegate, new object[] { receivedData.Message });
                        break;
                    case DataIdentifier.Invite:
                        if (receivedData.Invite == username)
                        {
                            if (!InviteAccepted)
                            {
                                string InviteString = "Вы получили приглашение в игру от игрока " + receivedData.UserName;
                                this.Dispatcher.Invoke(this.displayMessageDelegate, new object[] { InviteString });
                                this.Dispatcher.Invoke(this.InviteWindowDelegate, new object[] { receivedData.UserName });
                                if (InviteAccept.InviteAns)
                                {
                                    InviteString = "Вы приняли приглашение игрока " + receivedData.UserName;
                                    InviteAccepted = true;
                                    //Создание пакета для отправки ответа на приглашение
                                    Packet sendData = receivedData;
                                    sendData.DataType = DataIdentifier.InviteAnswer;
                                    sendData.InviteAnswer = true;
                                    // Преобразование пакета в поток байт
                                    byte[] byteData = sendData.GetDataStream();
                                    // Отправка пакета
                                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                                }
                                else
                                {
                                    InviteString = "Вы отклонили приглашение игрока " + receivedData.UserName;
                                    InviteAccepted = false;
                                    //Создание пакета для отправки ответа на приглашение
                                    Packet sendData = receivedData;
                                    sendData.DataType = DataIdentifier.InviteAnswer;
                                    sendData.InviteAnswer = false;
                                    // Преобразование пакета в поток байт
                                    byte[] byteData = sendData.GetDataStream();
                                    // Отправка пакета
                                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                                }
                                this.Dispatcher.Invoke(this.displayMessageDelegate, new object[] { InviteString });
                            }
                            else
                            {
                                string InviteString = "Вы отклонили приглашение игрока " + receivedData.UserName;
                                //Создание пакета для отправки ответа на приглашение
                                Packet sendData = receivedData;
                                sendData.DataType = DataIdentifier.InviteAnswer;
                                sendData.InviteAnswer = false;
                                // Преобразование пакета в поток байт
                                byte[] byteData = sendData.GetDataStream();
                                // Отправка пакета
                                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                            }
                            
                        }
                        break;
                    case DataIdentifier.GameStart:
                        for (int i = 1; i < 10; i++)
                        {
                            field[i] = 0;
                        }
                        InGame = 1;
                        bool flag = true;
                        this.Dispatcher.Invoke(this.UIcontrolDelegate, new object[] { flag });
                        String GameStr = receivedData.UserName + " (Крестики) VS. " + receivedData.Invite + " (Нолики)";
                        this.Dispatcher.Invoke(this.UpdateGameLineDelegate, new object[] { GameStr });
                        if (receivedData.UserName == username)
                        {
                            role = true;
                            turn = true;
                            Opponent = receivedData.Invite;
                        }
                        else
                        {
                            role = false;
                            turn = false;
                            Opponent = receivedData.UserName;
                        }
                        bool enableCells = true;
                        this.Dispatcher.Invoke(this.FieldClearDelegate, new object[] { enableCells });
                        this.Dispatcher.Invoke(this.FieldControlDelegate, new object[] { Opponent });
                        break;
                    case DataIdentifier.Field:
                        if (receivedData.Field[0] == 1 && role || receivedData.Field[0] == 2 && !role) turn = true;
                        else turn = false;
                        for(int i = 1; i < 10; i++)
                        {
                            field[i] = receivedData.Field[i];
                        }
                        this.Dispatcher.Invoke(this.FieldControlDelegate, new object[] { Opponent });
                        break;
                    case DataIdentifier.GameEnd:
                        InGame = 2;
                        string leaveStr = "Выйти в главное меню";
                        this.Dispatcher.Invoke(this.LeaveButtonDelegate, new object[] { leaveStr }); //Обновление кнопки
                        this.Dispatcher.Invoke(this.EndGameInfoDelegate, new object[] { receivedData.Result }); //Обновления информации о конце игры
                        bool enableCellsEnd = false;
                        this.Dispatcher.Invoke(this.FieldClearDelegate, new object[] { enableCellsEnd }); //Отключение кнопок
                        if (receivedData.Result > 0) this.Dispatcher.Invoke(this.ColorCellsDelegate, new object[] { receivedData.WinCells }); //Закраска победных клеток
                        break;
                    default:
                        break;
                }
                //Очищаем поток байт
                this.dataStream = new byte[100000];
                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (ObjectDisposedException ex)
            {
                string ExitReason = "ConnectionError";
                this.Dispatcher.Invoke(this.exitDelegate, new object[] { ExitReason });
                MessageBox.Show("ObjectDisposedException: " + ex.Message, "GameClient");
            }
            catch (Exception ex)
            {
                string ExitReason = "ConnectionError";
                this.Dispatcher.Invoke(this.exitDelegate, new object[] { ExitReason });
                MessageBox.Show("Receive Data: " + ex.Message, "GameClient");
            }
        }
        #endregion

        #region Delegate Methods

        //Очистика поля в начале игры
        private void FieldClear(bool enableCells)
        {
            if (enableCells) TurnInfo.Foreground = Brushes.Black;
            foreach (Button B in Buttons)
            {
                if (enableCells)
                {
                    B.IsEnabled = true;
                    if (role) B.Style = Application.Current.FindResource("XCellEmpty") as Style;
                    else B.Style = Application.Current.FindResource("OCellEmpty") as Style;
                }
                else B.IsEnabled = false;
            }
        }

        //Отображение поля
        private void FieldControl(String opponent)
        {
            //Обновление информации о ходе
            if (!turn)
            {
                TurnInfo.Text = "Ход игрока " + opponent;
                FieldGrid.IsEnabled = false;
            }
            else
            {
                TurnInfo.Text = "Ваш ход";
                FieldGrid.IsEnabled = true;
            }
            //Обработка кнопок
            int counter = 0;
            foreach(Button B in Buttons)
            {
                counter++;
                if (field[counter] == 1)
                {
                    B.Style = Application.Current.FindResource("XCell") as Style;
                    B.IsEnabled = false;
                }
                else if(field[counter] == 2)
                {
                    B.Style = Application.Current.FindResource("OCell") as Style;
                    B.IsEnabled = false;
                }
            }
        }

        //Обновление строки с информаицей об игре
        private void UpdateGameLine(String str)
        {
            GameInfoBlock.Text = str;
        }

        //Переключение текста на кнопке выхода
        private void LeaveButtonSwitch(String str)
        {
            surrender.Content = str;
        }

        //Переключение интерфейса
        private void UIcontrol(bool flag)
        {
            //true = игровое лобби
            //false = главное меню с чатом
            if (flag)
            {
                GamePanel.Visibility = Visibility.Visible;
                ChatPanel.Visibility = Visibility.Hidden;
                InviteButton.Visibility = Visibility.Hidden;
                surrender.Content = "Сдаться и выйти из игры";
            }
            else
            {
                GamePanel.Visibility = Visibility.Hidden;
                ChatPanel.Visibility = Visibility.Visible;
                InviteButton.Visibility = Visibility.Visible;
            }
        }

        //Вывод сообщения
        private void DisplayMessage(string message)
        {
            ChatBox.Text += message + Environment.NewLine;
        }

        private void EndGameInfo(int result)
        {
            string StatusStr = "";
            switch (result)
            {
                case 0:
                    StatusStr = "Ничья. Победила дружба!";
                    break;
                case 1:
                    if (role) StatusStr = "Поздравляем! Вы победили!";
                    else StatusStr = "Победа игрока " + Opponent + " (крестики)";
                    break;
                case 2:
                    if (!role) StatusStr = "Поздравляем! Вы победили!";
                    else StatusStr = "Победа игрока " + Opponent + " (нолики)";
                    break;
                case -1:
                    if (!role) StatusStr = "Игрок " + Opponent + " Вышел из игры. Вам засчитана победа";
                    break;
                case -2:
                    if (role) StatusStr = "Игрок " + Opponent + " Вышел из игры. Вам засчитана победа";
                    break;
            }
            if (InGame == 2)
            {
                if(role && result == 1 || !role && result == 2) TurnInfo.Foreground = Brushes.Green;
                else if(result==0) TurnInfo.Foreground = Brushes.Orange;
                else TurnInfo.Foreground = Brushes.Red;
            }
            TurnInfo.Text = StatusStr;
        }
        
        //Вывод списка пользователей
        private void DisplayPlayerList(List<Player> players)
        {
            playerList = players;
            PlayerListBox.Items.Clear();
            for (int i = 0; i < players.Count; i++)
            {
                string userLine = players[i].Name;
                if (players[i].Name == username) userLine += " (Вы)";
                else if (players[i].InGame) userLine += " - в игре";
                else userLine += " - свободен";
                PlayerListBox.Items.Add(userLine);
            }
        }

        //Перекраска кнопок после победы одного из игроков
        private void ColorCells(int[] Cells)
        {
            int counter;
            for(int i = 0; i < 3; i++)
            {
                counter = 0;
                foreach(Button B in Buttons)
                {
                    counter++;
                    if (counter == Cells[i])
                    {
                        if (field[Cells[i]] == 1) B.Style = Application.Current.FindResource("XCellWin") as Style;
                        else B.Style = Application.Current.FindResource("OCellWin") as Style;
                    }
                }
            }
        }

        //Закрытие приложения из-за различных причин
        private void ExitHandler(string exit_reason)
        {
            switch (exit_reason)
            {
                case "LoginDenied":
                    ConnectionStatus.ConnectionBroken = true;
                    ForcedExit = true;
                    MessageBox.Show("ERROR: Введенное имя уже занято или недопустимо");
                    break;
                case "ConnectionError":
                    ConnectionStatus.ConnectionBroken = true;
                    ForcedExit = true;
                    break;
                default:
                    break;
            }
            this.Close();
        }

        //Открытия окна приглашения
        private void DisplayInviteWindow(string inviterStr)
        {
            InviteWindow newInvite = new InviteWindow(inviterStr);
            newInvite.ShowDialog();
        }

        #endregion

        #region Events

        private void SendTurn(int buttonIndex)
        {
            Packet SendPacket = new Packet();
            SendPacket.DataType = DataIdentifier.Turn;
            SendPacket.Turn = buttonIndex;
            byte[] byteData = SendPacket.GetDataStream();
            clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
        }

        private void InviteButton_Click(object sender, RoutedEventArgs e)
        {
            if(playerList[PlayerListBox.SelectedIndex].InGame==false && playerList[PlayerListBox.SelectedIndex].Name != username)
            {
                //Отправка приглашения другому игроку
                Packet sendData = new Packet();
                sendData.UserName = this.username;
                sendData.Invite = this.playerList[PlayerListBox.SelectedIndex].Name;
                sendData.DataType = DataIdentifier.Invite;
                //Преобразование пакета в поток байт
                byte[] byteData = sendData.GetDataStream();
                // Отправка пакета
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                ChatBox.Text += "Вы отправили приглашение в игру пользователю " + sendData.Invite + Environment.NewLine;
            }
        }

        private void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(UserMessageBox.Text))
            {
                try
                {
                    //Создание пакета для отправки сообщения
                    Packet sendData = new Packet();
                    sendData.UserName = this.username;
                    sendData.Message = UserMessageBox.Text.Trim();
                    sendData.DataType = DataIdentifier.Message;
                    // Преобразование пакета в поток байт
                    byte[] byteData = sendData.GetDataStream();
                    // Отправка пакета
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);
                    //Очистка поля для ввода сообщений
                    UserMessageBox.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Send Error: " + ex.Message, "GameClient");
                }
            }
            
        }

        //Функция выхода при закрытия окна
        private void Logoff(object sender, EventArgs e)
        {
            try
            {
                if (this.clientSocket != null && ForcedExit == false)
                {
                    //Сдача партии, если еще идет игра, либо выход из законченной игры
                    if (InGame != 0) surrender_Click(sender, new RoutedEventArgs());
                    //Создание пакета Logout
                    Packet sendData = new Packet();
                    sendData.DataType = DataIdentifier.LogOut;
                    sendData.UserName = this.username;
                    sendData.Message = null;
                    //Преобразование пакета в массив байт
                    byte[] byteData = sendData.GetDataStream();
                    //Отправка пакета с сообщением о выходе на сервер
                    this.clientSocket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Logoff Error: " + ex.Message, "GameClient");
            }
        }

        //Включение/отключение кнопки приглашения (чтобы нельзя было пригласить себя или игроков в игре)
        private void PlayerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlayerListBox.SelectedIndex == -1)
            {
                if(PreviousSelectedPlayer == null)
                {
                    InviteButton.IsEnabled = false;
                    InviteButton.ToolTip = "Выберите пользователя для приглашения";
                }
                else
                {
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        if (PreviousSelectedPlayer.Name == playerList[i].Name)
                        {
                            PlayerListBox.SelectedIndex = i;
                        }
                    }
                    if (PlayerListBox.SelectedIndex == -1)
                    {
                        InviteButton.IsEnabled = false;
                        InviteButton.ToolTip = "Выберите пользователя для приглашения";
                    }
                }
            }
            else 
            {
                PreviousSelectedPlayer = playerList[PlayerListBox.SelectedIndex]; //Запоминаем предыдущего выбранного игрока
                if (playerList[PlayerListBox.SelectedIndex].InGame)
                {
                    InviteButton.IsEnabled = false;
                    InviteButton.ToolTip = "Выбранный пользователь уже находится в игре";
                }
                else if (playerList[PlayerListBox.SelectedIndex].Name == username)
                {
                    InviteButton.IsEnabled = false;
                    InviteButton.ToolTip = "Вы не можете пригласить в игру сами себя";
                }
                else
                {
                    InviteButton.IsEnabled = true;
                    InviteButton.ToolTip = null;
                }
            }
            
        }

        private void surrender_Click(object sender, RoutedEventArgs e)
        {
            switch (InGame)
            {
                case 1:
                    SendTurn(0); //Отправка сообщения со сдачей партии
                    InGame = 0;
                    UIcontrol(false);
                    InviteAccepted = false;
                    break;
                case 2:
                    Packet Rejoin = new Packet();
                    Rejoin.DataType = DataIdentifier.Rejoin;
                    byte[] byteData = Rejoin.GetDataStream();
                    this.clientSocket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer);
                    InGame = 0;
                    UIcontrol(false);
                    InviteAccepted = false;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Button Events 
        private void B1_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(1);
        }

        private void B2_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(2);
        }

        private void B3_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(3);
        }

        private void B4_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(4);
        }

        private void B5_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(5);
        }

        private void B6_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(6);
        }

        private void B7_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(7);
        }

        private void B8_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(8);
        }

        private void B9_Click(object sender, RoutedEventArgs e)
        {
            if (turn && InGame == 1) SendTurn(9);
        }

        #endregion

        
    }
}

