using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeClient
{
    public enum DataIdentifier
    {
        //Структура пакета в зависимости от хедера
        LoginRequest,
        //DataIdentifier = LoginRequest - Запрос о входе в игре.
        // Поле пакета     -> |dataIdentifier|name length|    name      |  
        // Размер в байтах -> |       4      |     4     | name length  |
        LoginConfirmation,
        //DataIdentifier = LoginConfirmation - Подтверждение/Отказ о входе в игру.
        // Поле пакета     -> |dataIdentifier|LoginStatus|
        // Размер в байтах -> |       4      |     1     | 
        LogOut,
        //DataIdentifier = Logut - сообщение о выходе. В поле name Имя вышедшего игрока. 
        // Поле пакета     -> |dataIdentifier|name length|    name   |
        // Размер в байтах -> |       4      |     4     |name length|
        ClientList,
        //DataIdentifier = ClientList - список всех клиентов
        // Поле пакета     -> |dataIdentifier|playerCount|playerFree[0]...[i]...[playerCount-1]| playerNameLength[0] | ...|playerNameLength[i]| ...|playerNameLength[playerCount-1]|   playerName[0]   | ... |   playerName[i]   | ... |   playerName[playerCount-1]   |
        // Размер в байтах -> |       4      |     4     |                 1                   |         4           | ...|       4           | ...|                 4             |playerNameLength[0]| ... |playerNameLength[i]| ... |playerNameLength[playerCount-1]|
        ClientListRequest,
        //DataIdentifier = ClientListRequest - запрос списка клиентов
        // Поле пакета     -> |dataIdentifier|
        // Размер в байтах -> |       4      | 
        Message,
        //DataIdentifier = Message - сообщение.
        // Поле пакета     -> |dataIdentifier|name length|message length|    name   |    message   |
        // Размер в байтах -> |       4      |     4     |       4      |name length|message length|
        Invite,
        //DataIdentifier = Invite - приглашение в игру
        // Поле пакета     -> |dataIdentifier|name1 length|    name1   |name2 length|    name2   |
        // Размер в байтах -> |       4      |     4      |name1 length|     4      |name2 length|
        InviteAnswer,
        //DataIdentifier = InviteAnswer - ответ на приглашение в игру
        // Поле пакета     -> |dataIdentifier|name1 length|    name1   |name2 length|    name2   | Answer |
        // Размер в байтах -> |       4      |     4      |name1 length|     4      |name2 length|    1   |
        GameStart,
        //DataIdentifier = GameStart - сообщение о создании игры. Первый - крестики, второй - нолики
        // Поле пакета     -> |dataIdentifier|name1 length|    name1   |name2 length|    name2   |
        // Размер в байтах -> |       4      |     4      |name1 length|     4      |name2 length|
        Turn,
        //DataIdentifier = Turn - Ход игрока
        // Поле пакета     -> |dataIdentifier| Turn |
        // Размер в байтах -> |       4      |   4  |
        Field,
        //DataIdentifier = Field - Игровое поле. Turn = 1 - крестик, Turn = 2 - нолик. Cell = 0 - пустая, Cell = 1 - крестик, Cell = 2 - нолик
        // Поле пакета     -> |dataIdentifier| Turn  | Cell1 | Cell2 | Cell3 | Cell4 | Cell5 | Cell6 | Cell7 | Cell8 | Cell9 |
        // Размер в байтах -> |       4      |   4   |   4   |   4   |   4   |   4   |   4   |   4   |   4   |   4   |   4   |
        GameEnd,
        //DataIdentifier = GameEnd - сообщение о заершении игры. GameResult = 0 - ничья, -1 крестики вышли, 1 крестики победили, -2 нолики вышли, 2 нолики победили
        // Поле пакета     -> |dataIdentifier|  GameResult  | WinningCell1 | WinningCell2 | WinningCell3 |
        // Размер в байтах -> |       4      |     4        |       4      |       4      |       4      |
        Rejoin,
        //DataIdentifier = Rejoin - сообщение клиента о выходе из гры после ее завершения
        // Поле пакета     -> |dataIdentifier|
        // Размер в байтах -> |       4      | 
        Null
        //Тип пакета для конструктора по умолчанию
    }

    public class Packet
    {
        #region Private Members

        private DataIdentifier dataIdentifier;
        private string name;
        private string invite_target;
        private bool login_approved;
        private string message;
        private bool invite_answer_flag;
        private int player_turn;
        private int gameResult;
        #endregion

        #region Public Packet Fields

        //Хедер
        public DataIdentifier DataType
        {
            get { return dataIdentifier; }
            set { dataIdentifier = value; }
        }

        //Ход
        public int Turn
        {
            get { return player_turn; }
            set { player_turn = value; }
        }

        //Имя пользователя
        public string UserName
        {
            get { return name; }
            set { name = value; }
        }

        //Имя пользователя, которого приглашают в игру
        public string Invite
        {
            get { return invite_target; }
            set { invite_target = value; }
        }

        //Сообщение
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        //Результат игры
        public int Result
        {
            get { return gameResult; }
            set { gameResult = value; }
        }

        //Статус входа на сервер
        public bool LoginStatus
        {
            get { return login_approved; }
            set { login_approved = value; }
        }

        //Ответ на приглашение
        public bool InviteAnswer
        {
            get { return invite_answer_flag; }
            set { invite_answer_flag = value; }
        }

        //Список игроков
        public List<Player> players_list;

        //Поле игры
        public int[] Field;

        //Победные клетки дял подсветки
        public int[] WinCells;
        #endregion

        #region Methods

        //Конструктор пустого пакета по умолчанию
        public Packet()
        {
            this.players_list = new List<Player>();
            this.dataIdentifier = DataIdentifier.Null;
            this.name = null;
            this.invite_target = null;
            login_approved = true;
            Field = new int[10];
            WinCells = new int[3];
        }

        //Конструктор пакета из массива байт
        public Packet(byte[] dataStream)
        {
            // Читаем хедер из начала потока (4 байта)
            this.dataIdentifier = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);
            //Читаю остальную часть пакета в зависимости от типа данных
            switch (this.dataIdentifier)
            {
                case DataIdentifier.LoginRequest:
                    // Читаем длину имени (4 байта)
                    int nameLength = BitConverter.ToInt32(dataStream, 4);
                    // Читаем поле имени
                    if (nameLength > 0) this.name = Encoding.GetEncoding(1251).GetString(dataStream, 8, nameLength);
                    else this.name = null;
                    break;
                case DataIdentifier.LoginConfirmation:
                    bool buf = BitConverter.ToBoolean(dataStream, 4);
                    if (buf)
                    {
                        login_approved = true;
                    }
                    else login_approved = false;
                    break;
                case DataIdentifier.LogOut:
                    // Читаем длину имени (4 байта)
                    int nameLengthLogout = BitConverter.ToInt32(dataStream, 4);
                    // Читаем поле имени
                    if (nameLengthLogout > 0) this.name = Encoding.GetEncoding(1251).GetString(dataStream, 8, nameLengthLogout);
                    else this.name = null;
                    break;
                case DataIdentifier.Message:
                    // Читаем длину имени (4 байта)
                    int nameL = BitConverter.ToInt32(dataStream, 4);
                    // Читаем длину сообщения (4 байта)
                    int msgLength = BitConverter.ToInt32(dataStream, 8);
                    // Читаем поле имени
                    if (nameL > 0) this.name = Encoding.GetEncoding(1251).GetString(dataStream, 12, nameL);
                    else this.name = null;
                    // Читаем поле сообщения
                    if (msgLength > 0) this.message = Encoding.GetEncoding(1251).GetString(dataStream, 12 + nameL, msgLength);
                    else this.message = null;
                    break;
                case DataIdentifier.ClientList:
                    this.players_list = new List<Player>();
                    //Читаем количество клиентов (4 байт)
                    int playerCount = BitConverter.ToInt32(dataStream, 4);
                    int[] playerNameLength = new int[playerCount];
                    bool[] inGame = new bool[playerCount];
                    int currentPos = 8;
                    for (int i = 0; i < playerCount; i++)
                    {
                        //читаем статус пользователя
                        inGame[i] = BitConverter.ToBoolean(dataStream, currentPos);
                        currentPos++;
                    }
                    for (int i = 0; i < playerCount; i++)
                    {
                        //читаем длину имени пользователя
                        playerNameLength[i] = BitConverter.ToInt32(dataStream, currentPos);
                        currentPos += 4;
                    }
                    for (int i = 0; i < playerCount; i++)
                    {
                        Player bufPlayer = new Player();
                        bufPlayer.InGame = inGame[i];
                        //Читаем имя каждого игрока
                        bufPlayer.Name = Encoding.GetEncoding(1251).GetString(dataStream, currentPos, playerNameLength[i]);
                        players_list.Add(bufPlayer);
                        currentPos += playerNameLength[i];
                    }
                    break;
                case DataIdentifier.Invite:
                    // Читаем длину имени 1 (4 байта)
                    int nameLength1 = BitConverter.ToInt32(dataStream, 4);
                    // Читаем поле имени 1 - Отправитель
                    if (nameLength1 > 0) this.name = Encoding.GetEncoding(1251).GetString(dataStream, 8, nameLength1);
                    else this.name = null;
                    // Читаем длину имени 2 (4 байта)
                    int nameLength2 = BitConverter.ToInt32(dataStream, 8 + nameLength1);
                    // Читаем поле имени 2 - Получатель
                    if (nameLength2 > 0) this.invite_target = Encoding.GetEncoding(1251).GetString(dataStream, 12 + nameLength1, nameLength2);
                    else this.invite_target = null;
                    break;
                case DataIdentifier.InviteAnswer:
                    // Читаем длину имени 1 (4 байта)
                    int nameLengthP1 = BitConverter.ToInt32(dataStream, 4);
                    // Читаем поле имени 1 - Отправитель
                    if (nameLengthP1 > 0) this.name = Encoding.GetEncoding(1251).GetString(dataStream, 8, nameLengthP1);
                    else this.name = null;
                    // Читаем длину имени 2 (4 байта)
                    int nameLengthP2 = BitConverter.ToInt32(dataStream, 8 + nameLengthP1);
                    // Читаем поле имени 2 - Получатель
                    if (nameLengthP2 > 0) this.invite_target = Encoding.GetEncoding(1251).GetString(dataStream, 12 + nameLengthP1, nameLengthP2);
                    else this.invite_target = null;
                    bool inviteAns = BitConverter.ToBoolean(dataStream, 12 + nameLengthP1 + nameLengthP2);
                    this.invite_answer_flag = inviteAns;
                    break;
                case DataIdentifier.GameStart:
                    // Читаем длину имени 1 (4 байта)
                    int nameLengthX = BitConverter.ToInt32(dataStream, 4);
                    // Читаем поле имени 1 - Отправитель
                    if (nameLengthX > 0) this.name = Encoding.GetEncoding(1251).GetString(dataStream, 8, nameLengthX);
                    else this.name = null;
                    // Читаем длину имени 2 (4 байта)
                    int nameLengthO = BitConverter.ToInt32(dataStream, 8 + nameLengthX);
                    // Читаем поле имени 2 - Получатель
                    if (nameLengthO > 0) this.invite_target = Encoding.GetEncoding(1251).GetString(dataStream, 12 + nameLengthX, nameLengthO);
                    else this.invite_target = null;
                    break;
                case DataIdentifier.Turn:
                    //Читаем ход (число 0-9)
                    this.player_turn = BitConverter.ToInt32(dataStream, 4);
                    break;
                case DataIdentifier.Field:
                    this.Field = new int[10];
                    for (int i = 0; i < 10; i++)
                    {
                        this.Field[i] = BitConverter.ToInt32(dataStream, 4 + i * 4);
                    }
                    break;
                case DataIdentifier.GameEnd:
                    //Читаем итог игры
                    this.gameResult = BitConverter.ToInt32(dataStream, 4);
                    //Читаем победные клетки
                    if (this.gameResult > 0)
                    {
                        this.WinCells = new int[3];
                        for (int i = 0; i < 3; i++)
                        {
                            this.WinCells[i] = BitConverter.ToInt32(dataStream, 8 + i * 4);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        // Конвертирует пакет в byte[] для получения/отправки
        public byte[] GetDataStream()
        {
            List<byte> dataStream = new List<byte>();

            //Добавляем хедер
            dataStream.AddRange(BitConverter.GetBytes((int)this.dataIdentifier));
            //Создаем остальную часть пакета в зависимости от хедера
            switch (this.dataIdentifier)
            {
                case DataIdentifier.LoginConfirmation:
                    //Добавляем разрешение на вход
                    dataStream.AddRange(BitConverter.GetBytes(this.login_approved));
                    break;
                case DataIdentifier.LoginRequest:
                    //Добавляем длину имени
                    if (this.name != null) dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя
                    if (this.name != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.name));
                    break;
                case DataIdentifier.LogOut:
                    //Добавляем длину имени
                    if (this.name != null) dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя
                    if (this.name != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.name));
                    break;
                case DataIdentifier.Message:
                    //Добавляем длину имени
                    if (this.name != null) dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем длину сообщения
                    if (this.message != null) dataStream.AddRange(BitConverter.GetBytes(this.message.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя
                    if (this.name != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.name));
                    //Добавляем сообщение
                    if (this.message != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.message));
                    break;
                case DataIdentifier.ClientList:
                    if (this.players_list != null)
                    {
                        //Добавляем количество игроков
                        dataStream.AddRange(BitConverter.GetBytes(players_list.Count));
                        for (int i = 0; i < players_list.Count; i++)
                        {
                            //Добавляем статус игрока i
                            dataStream.AddRange(BitConverter.GetBytes(players_list[i].InGame));
                        }

                        for (int i = 0; i < players_list.Count; i++)
                        {
                            //Добавляем длину имени игрока i
                            dataStream.AddRange(BitConverter.GetBytes(players_list[i].Name.Length));
                        }
                        for (int i = 0; i < players_list.Count; i++)
                        {
                            //Добавляем имя игрока i
                            dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(players_list[i].Name));
                        }
                    }
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    break;
                case DataIdentifier.Invite:
                    //Добавляем длину имени 1
                    if (this.name != null) dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя 1
                    if (this.name != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.name));
                    //Добавляем длину имени 2
                    if (this.invite_target != null) dataStream.AddRange(BitConverter.GetBytes(this.invite_target.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя 2
                    if (this.invite_target != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.invite_target));
                    break;
                case DataIdentifier.InviteAnswer:
                    //Добавляем длину имени 1
                    if (this.name != null) dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя 1
                    if (this.name != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.name));
                    //Добавляем длину имени 2
                    if (this.invite_target != null) dataStream.AddRange(BitConverter.GetBytes(this.invite_target.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя 2
                    if (this.invite_target != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.invite_target));
                    //Добавляем ответ
                    dataStream.AddRange(BitConverter.GetBytes(this.invite_answer_flag));
                    break;
                case DataIdentifier.GameStart:
                    //Добавляем длину имени 1
                    if (this.name != null) dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя 1
                    if (this.name != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.name));
                    //Добавляем длину имени 2
                    if (this.invite_target != null) dataStream.AddRange(BitConverter.GetBytes(this.invite_target.Length));
                    else dataStream.AddRange(BitConverter.GetBytes(0));
                    //Добавляем имя 2
                    if (this.invite_target != null) dataStream.AddRange(Encoding.GetEncoding(1251).GetBytes(this.invite_target));
                    break;
                case DataIdentifier.Turn:
                    //Добавляем ход
                    dataStream.AddRange(BitConverter.GetBytes(this.player_turn));
                    break;
                case DataIdentifier.Field:
                    for (int i = 0; i < 10; i++)
                    {
                        dataStream.AddRange(BitConverter.GetBytes(this.Field[i]));
                    }
                    break;
                case DataIdentifier.GameEnd:
                    //Добавляем итог игры
                    dataStream.AddRange(BitConverter.GetBytes(this.gameResult));
                    //Добавляем победные клетки
                    if (this.gameResult > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            dataStream.AddRange(BitConverter.GetBytes(this.WinCells[i]));
                        }
                    }
                    break;
                default:
                    break;
            }
            return dataStream.ToArray();
        }

        #endregion
    }
}
