using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BotTW
{
    class Bot
    {
        //MYSQL
        DB database;

        //АПИ
        API api;

        //Логи
        Logs logs;

        Party party;

        TwitchClient botClient;

        List<string> BadPhrases;

        List<Command> commands;
        List<SimpleAnswer> commandsSimple;
        List<string> bots;

        AccessLevel accessLevel = new AccessLevel();

        //Message msgAgent;
        CommandTea tea;
        CommandEaster easter;
        CommandLove love;
        CommandMovie movie;
        CommandValentine valentine;

        string targetChannel;
        Message msg;

        Perspective perspective;

        #region Bot constructor
        public Bot()
        {
            logs = new Logs();

            //MYSQL
            database = new DB(logs);
            database.onDBConnect += OnBDConnected;


            commandsSimple = database.GetSimpleAnswers();
            bots = database.GetBots();


            Login();
            msg = new Message(logs, botClient, targetChannel);
            //АПИ
            api = new API(logs, msg, database, bots);

            botClient.OnMessageReceived += Client_OnMessageReceived;
            botClient.OnNewSubscriber += Client_OnNewSubscribed;
            botClient.OnReSubscriber += Client_OnReSubscribed;
            botClient.OnGiftedSubscription += Client_OnGiftedSubscription;
            botClient.OnContinuedGiftedSubscription += Client_OnContinuedGiftedSubscription;
            botClient.OnCommunitySubscription += Client_OnCommunitySubscription;
            botClient.OnConnected += Client_OnConnected;
            botClient.OnMessageSent += Client_OnMessageSent;
            botClient.OnDisconnected += Client_OnDisconnected;
            botClient.OnMessageCleared += Client_OnMessageCleared;

            botClient.OnUserJoined += Client_OnUserJoined;
            botClient.OnUserLeft += Client_OnUserLeft;


            party = new Party("пати", database, msg);

            commands = new List<Command>
            {
                new CommandWitcher("!ведьмак"),
                new CommandWitcher("!witcher"),
                new CommandDick("!садовник"),
                new CommandPing("!ping"),
                new CommandIQ("!iq"),
                new CommandIQ("!айку"),
                new CommandAge("!age"),
                new CommandAge("!возраст"),
                new CommandNY("!ny"),
                new CommandNY("!нг"),
            };

            //АПИ
            tea = new CommandTea("!чай");
            valentine = new CommandValentine("валентинка");
            easter = new CommandEaster("!хв");
            //АПИ
            love = new CommandLove("!люблю");

            movie = new CommandMovie("!фильм");

            perspective = new Perspective(msg);
        }
        #endregion Bot constructor

        #region login to twitch
        void Login()
        {
            string botToken = string.Empty;
            string botUsername = string.Empty;
            targetChannel = string.Empty;

            FileStream fstream;
            TextReader reader;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fstream = new FileStream(@"C:\Users\Gregory\Desktop\Twitch.txt", FileMode.Open);
            }
            else
            {
                fstream = new FileStream(@"/var/shit/Twitch.txt", FileMode.Open);
            }

            reader = new StreamReader(fstream);
            if (fstream != null)
            {
                if (reader != null)
                {
                    //Чтение информации для логина на твич
                    botUsername = reader.ReadLine();
                    botToken = reader.ReadLine();
                    targetChannel = reader.ReadLine();
                }
                else
                {
                    logs.WriteLog("[Ошибка][Критичное] Файл инфы о твиче не открылся - reader null");
                }
            }
            else
            {
                logs.WriteLog("[Ошибка][Критичное] Файл инфы о твиче не открылся - fstream null");
            }

            //Подключение к твичу
            try
            {
                ConnectionCredentials credentials = new ConnectionCredentials(botUsername, botToken);
                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 50,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
                WebSocketClient customClient = new WebSocketClient(clientOptions);
                botClient = new TwitchClient(customClient);
                botClient.Initialize(credentials, targetChannel);
                botClient.Connect();

            }
            catch
            {
                logs.WriteLog("[Ошибка][Критичное] Не удалось подключиться к твичу");
            }
        }
        #endregion Login to twitch


        void MessageProcessing(string messageText)
        {

        }


        #region Events

        private void OnBDConnected()
        {
            commandsSimple = database.GetSimpleAnswers();
            bots = database.GetBots();
            BadPhrases = party.Update();
            logs.WriteLog("[Успех][Критичное] Сработало событие Подключились к БД");
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            foreach (JoinedChannel channel in botClient.JoinedChannels)
            {
                botClient.LeaveChannel(channel);
            }
            botClient.Connect();
            botClient.JoinChannel(targetChannel);
            logs.WriteLog("[Ошибка][Критичное] Переподключение к чату");
        }
        private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            string query = string.Format("INSERT INTO PartJoin (senderName, eventTime, streamUp, eventType) VALUES ('{0}', '{1}', '{2}', '{3}')",
                e.Username, DateTime.UtcNow.ToString("yyyyMMddHHmmss"), 0, 1);
            database.Write(query);
        }
        private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            string query = string.Format("INSERT INTO PartJoin (senderName, eventTime, streamUp, eventType) VALUES ('{0}', '{1}', '{2}', '{3}')",
                e.Username, DateTime.UtcNow.ToString("yyyyMMddHHmmss"), 0, 0);
            database.Write(query);
        }


        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            logs.WriteLog("[Успех][Критичное] Подключились к чату канала " + e.AutoJoinChannel);
        }


        private void Client_OnMessageSent(object sender, OnMessageSentArgs e)
        {
            string query = string.Format("INSERT INTO botMessages (senderName, messageTime, messageText) VALUES ('{0}', '{1}', '{2}')",
                 e.SentMessage.DisplayName, DateTime.UtcNow.ToString("yyyyMMddHHmmss"), e.SentMessage.Message);
            database.Write(query);
        }
        #endregion Events

        private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            logs.WriteLog("Получено сообщение " + DateTime.UtcNow);


            await Task.Run(() => database.WriteMessage(e.ChatMessage.Id, e.ChatMessage.Username, e.ChatMessage.UserId, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                e.ChatMessage.Message, e.ChatMessage.IsBroadcaster, e.ChatMessage.IsModerator, e.ChatMessage.IsVip, e.ChatMessage.IsSubscriber,
                e.ChatMessage.IsHighlighted, e.ChatMessage.RawIrcMessage));

            //Проверка, что автор сообщения не табуретка
            if (!e.ChatMessage.UserId.Equals("474756793"))
            {
                string text = e.ChatMessage.Message.ToLower();
                Regex patternCommand = new Regex(@"! *[А-яA-z0-9]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                MatchCollection matchesCommand = patternCommand.Matches(text);
                if (matchesCommand.Count > 0)
                {

                    foreach (Match match in matchesCommand)
                    {


                        switch (match.Value.Replace(" ", ""))
                        {
                            case "!чай":
                                tea.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, Role.Any, recipient: string.Empty, api.GetChattersWithoutBots());
                                return;
                            case "!дискорд":
                                if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    msg.AddMessage("/announce Наш уютный дискорд (18+) boouchEshy discord.gg/jk3stv3");
                                }
                                break;
                            case "!соцсети":
                                if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    msg.AddMessage("/announce Анонсы и мемы: https://discord.gg/jk3stv3 Инсайды: https://t.me/boouche Фото: https://instagram.com/boouche Хайлайты: https://youtube.com/channel/UC0iJtnhd7jyJH0h_MXmtnxw");
                                }
                                break;
                            case "!повтор":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0"))
                                {
                                    msg.AddMessage("/announce " + e.ChatMessage.Message);

                                }
                                return;
                            case "!тишина":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    //убрали из сообщения !тишина
                                    text = e.ChatMessage.Message.Remove(0, 8);
                                    //извлекаем
                                    string sendingMode = text;

                                    if (text.Equals("да"))
                                    {
                                        msg.ChangeSendingMode(Role.Owner, e.ChatMessage.DisplayName, true);
                                    }
                                    else
                                    {
                                        msg.ChangeSendingMode(Role.Owner, e.ChatMessage.DisplayName, false);
                                    }
                                }
                                return;
                            case "!интервал":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 10)
                                    {
                                        //убрали из сообщения !интервал
                                        text = e.ChatMessage.Message.Remove(0, 10);
                                        //извлекаем
                                        int sendingInterval;
                                        if (int.TryParse(text, out sendingInterval))
                                        {
                                            msg.ChangeSendingInterval(Role.Owner, e.ChatMessage.DisplayName, sendingInterval);
                                        }
                                    }
                                }
                                return;
                            case "!добавить":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 11)
                                    {
                                        //убрали из сообщения !добавить
                                        text = e.ChatMessage.Message.Remove(0, 10);
                                        //извлекаем
                                        string commandName = text.Substring(0, text.IndexOf(' ')).ToLower();
                                        string commandText = text.Substring(text.IndexOf(' ') + 1);

                                        if (database.AddSimpleAnswer(commandName, commandText, true))
                                        {
                                            msg.AddMessage("Текст команды " + commandName + " был обновлён");
                                        }
                                        else
                                        {
                                            msg.AddMessage("Добавлена команда " + commandName + " - " + commandText);
                                        }
                                        commandsSimple = database.GetSimpleAnswers();
                                    }
                                    else
                                    {
                                        msg.AddMessage("Использование - !добавить !ИмяДобавляемойКоманды");
                                    }
                                }
                                return;
                            case "!удалить":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 10)
                                    {
                                        //убрали из сообщения !удалить
                                        text = text.Remove(0, 9);
                                        //извлекаем
                                        string commandName = text;
                                        if (database.DeactivateSimpleAnswer(commandName))
                                        {
                                            msg.AddMessage("Команда " + commandName + " отключена");
                                            commandsSimple = database.GetSimpleAnswers();
                                        }
                                        else
                                        {
                                            msg.AddMessage("Нет активной команды " + commandName);
                                        }
                                    }
                                    else
                                    {
                                        msg.AddMessage("Использование - !удалить !ИмяУдаляемойКоманды");
                                    }
                                }
                                return;
                            case "!обновить":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    commandsSimple = database.GetSimpleAnswers();
                                    msg.AddMessage("Список команд обновлён");
                                }
                                return;
                            case "!бот":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 7)
                                    {
                                        //убрали из сообщения !бот
                                        text = e.ChatMessage.Message.Remove(0, 5);

                                        string botName = text;

                                        if (database.AddBot(botName))
                                        {
                                            msg.AddMessage("Бот " + botName + " уже был в списке");
                                        }
                                        else
                                        {
                                            msg.AddMessage("Добавлен бот " + botName);
                                        }
                                        bots = database.GetBots();
                                    }
                                    else
                                    {
                                        msg.AddMessage("Использование - !бот ИмяБота");
                                    }
                                }
                                return;
                            case "!небот":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 8)
                                    {
                                        //убрали из сообщения !небот
                                        text = text.Remove(0, 7);
                                        //извлекаем
                                        string botName = text;
                                        if (database.DeleteBot(botName))
                                        {
                                            msg.AddMessage("Бот " + botName + " удалён");
                                            bots = database.GetBots();
                                        }
                                        else
                                        {
                                            msg.AddMessage("Нет " + botName + " в списке ботов");
                                        }
                                    }
                                    else
                                    {
                                        msg.AddMessage("Использование - !небот ИмяБота");
                                    }
                                }
                                return;
                            case "!обновитьбот":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    bots = database.GetBots();
                                    msg.AddMessage("Список ботов обновлён");
                                }
                                return;
                            case "!всекоманды":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    string answer = "Простые команды:";
                                    foreach (SimpleAnswer cmd in commandsSimple)
                                    {
                                        answer = answer + " " + cmd.Name;
                                    }
                                    answer = answer + " Остальные команды:";
                                    foreach (Command cmd in commands)
                                    {
                                        answer = answer + " " + cmd.Name;
                                    }
                                    answer = answer + " !чай !добавить !обновить !удалить";
                                    msg.AddMessage(answer);
                                }
                                return;
                            case "!sr":
                                if (e.ChatMessage.IsSubscriber != true)
                                {
                                    string answer = "@" + e.ChatMessage.DisplayName + ", чтобы заказать песню, нужно быть сабскрайбером";
                                    msg.AddMessage(e.ChatMessage.Id, answer);
                                }
                                return;
                            case "!патидобавить":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 14)
                                    {
                                        //убрали из сообщения !патидобавить
                                        text = text.Remove(0, 14);
                                        //извлекаем
                                        string commandName = text;

                                        BadPhrases = party.Add(text);

                                    }
                                    else
                                    {
                                        msg.AddMessage("Использование - !патидобавить !ДобавляемаяФраза");
                                    }
                                }
                                return;
                            case "!патиудалить":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > 15)
                                    {
                                        //убрали из сообщения !патиудалить
                                        text = text.Remove(0, 13);
                                        //извлекаем
                                        string commandName = text;

                                        BadPhrases = party.Delete(text);

                                    }
                                    else
                                    {
                                        msg.AddMessage("Использование - !патиудалить !УдаляемаяФраза");
                                    }
                                }
                                return;
                            case "!патиобновить":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    BadPhrases = party.Update();
                                }
                                return;
                            case "!люблю":
                                love.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, Role.Any, recipient: string.Empty, e.ChatMessage.Message);
                                return;
                            case "!фильм":
                                if (e.ChatMessage.Username.ToLower().Equals("i_am_d0br0") || e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                                {
                                    if (text.Length > (text.IndexOf("!фильм ") + 7))
                                    {
                                        movie.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, Role.Any, recipient: e.ChatMessage.DisplayName, e.ChatMessage.Message, database);
                                        commandsSimple = database.GetSimpleAnswers();

                                    }
                                    else
                                    {
                                        msg.AddMessage(e.ChatMessage.DisplayName + ", использование - !фильм Название фильма. Текущее название: " + movie.MovieName);
                                    }
                                    return;
                                }
                                //TODO убрать goto
                                goto penis;
                            default:
                            penis:
                                if (commands != null)
                                {
                                    foreach (Command cmd in commands)
                                    {
                                        if (match.Value.Replace(" ", "") == cmd.Name)
                                        {

                                            Regex patternUsername = new Regex(@"@[A-z0-9_]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                                            MatchCollection matchesUsername = patternUsername.Matches(text);

                                            if (matchesUsername.Count == 0)
                                            {

                                                Role role = Role.None;

                                                role = accessLevel.GetRole(e.ChatMessage.Username);

                                                cmd.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, role, recipient: "@" + e.ChatMessage.DisplayName);

                                                return;
                                            }
                                            else
                                            {
                                                string recipients = string.Empty;
                                                foreach (Match recipient in matchesUsername)
                                                {
                                                    recipients = recipients + " " + recipient.Value;
                                                }

                                                Role role = Role.None;

                                                role = accessLevel.GetRole(e.ChatMessage.Username);

                                                cmd.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, role, recipients);
                                                return;
                                            }
                                        }
                                    }
                                }
                                if (commandsSimple != null)
                                {
                                    foreach (SimpleAnswer cmdS in commandsSimple)
                                    {
                                        if (match.Value.Replace(" ", "") == cmdS.Name)
                                        {

                                            Regex patternUsername = new Regex(@"@[A-z0-9_]+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                                            MatchCollection matchesUsername = patternUsername.Matches(text);

                                            if (matchesUsername.Count == 0)
                                            {

                                                Role role = Role.None;

                                                role = accessLevel.GetRole(e.ChatMessage.Username);

                                                cmdS.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, role, recipient: "@" + e.ChatMessage.DisplayName);

                                                return;
                                            }
                                            else
                                            {
                                                string recipients = string.Empty;
                                                foreach (Match recipient in matchesUsername)
                                                {
                                                    recipients = recipients + " " + recipient.Value;
                                                }

                                                Role role = Role.None;

                                                role = accessLevel.GetRole(e.ChatMessage.Username);

                                                cmdS.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, role, recipients);
                                                return;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                //Пинг удачи
                if ((e.ChatMessage.UserId == "54691903") && (text.Contains("удачи")))
                {
                    msg.AddMessage("удачи");
                }
                //Флоппи кстати
                if ((e.ChatMessage.UserId == "143404803") && (text.Contains("кстати")))
                {
                    msg.AddMessage("AYAYA");
                }
                //Надя
                if ((e.ChatMessage.UserId == "131598181") && (text.Contains("!hug")))
                {
                    Random rng = new Random();
                    int pup = rng.Next(0, 10);
                    if (pup > 8)
                    {
                        msg.AddMessage("Как же хочется обнимашек FeelsStrongMan");
                    }
                }
                if ((e.ChatMessage.UserId == "131598181") && (text.Contains("!love")))
                {
                    Random rng = new Random();
                    int pup = rng.Next(0, 10);
                    if (pup > 8)
                    {
                        msg.AddMessage("Как же хочется дулечку PepeLuv");
                    }
                }
                if ((e.ChatMessage.UserId == "131598181") && (text.Contains("!кусь")))
                {
                    Random rng = new Random();
                    int pup = rng.Next(0, 10);
                    if (pup > 8)
                    {
                        msg.AddMessage("Как же хочется котлеточку OpieOP");
                    }
                }
                if (e.ChatMessage.UserId == "131598181")
                {
                    Random rng = new Random();
                    int pup = rng.Next(0, 20);
                    if (pup > 18)
                    {
                        msg.AddMessage("Пуп Надю AYAYA");
                    }
                }
                if (!e.ChatMessage.IsSubscriber)
                {
                    party.Action(botClient, msg, e.ChatMessage.DisplayName, e.ChatMessage.Channel, Role.Any, "", text);
                }
            }
            database.SetToxicity(e.ChatMessage.Id, perspective.GetToxicityScore(e.ChatMessage.Message).Result);
        }


        private void Client_OnNewSubscribed(object sender, OnNewSubscriberArgs e)
        {
            msg.AddMessage("boouchEsubHype boouchEsubHype boouchEsubHype");
        }

        private void Client_OnReSubscribed(object sender, OnReSubscriberArgs e)
        {
            msg.AddMessage("boouchEsubHype boouchEluv boouchEsubHype");
        }

        private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            //msg.AddMessage("boouchEluv boouchEluv boouchEluv");
        }

        private void Client_OnContinuedGiftedSubscription(object sender, OnContinuedGiftedSubscriptionArgs e)
        {
            msg.AddMessage("Подписочка boouchEsubHype boouchEsubHype boouchEsubHype");
        }

        private void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            msg.AddMessage("Подарочки boouchEsubHype boouchEsubHype boouchEsubHype");
        }

        private void Client_OnMessageCleared(object sender, OnMessageClearedArgs e)
        {
            database.MarkMessageAsDeleted(e.TargetMessageId);
            logs.WriteLog(DateTime.UtcNow.ToString() + " Пометили сообщение как удалённое " + e.TargetMessageId);
        }

        //Проверка, что подключен к чату. Если нет, то переподключение
        public void TwitchLifeSupport()
        {
            if (botClient.IsConnected)
            {
                logs.WriteLog(DateTime.UtcNow.ToString() + " Вызвали поддержание жизни бота, он подключен");
            }
            else
            {
                try
                {
                    botClient.Disconnect();
                }
                catch
                {
                    logs.WriteLog("! " + DateTime.UtcNow.ToString() + " Вызвали поддержание жизни бота, он не подключен, не получилось переподключиться - ошибка");
                }
                if (botClient.IsConnected)
                {
                    logs.WriteLog(DateTime.UtcNow.ToString() + " Вызвали поддержание жизни бота, он переподключился");
                }
                else
                {
                    logs.WriteLog("! " + DateTime.UtcNow.ToString() + " Вызвали поддержание жизни бота, он не переподключился");
                }
            }
            if (botClient.JoinedChannels.Count == 0)
            {
                logs.WriteLog("! " + DateTime.UtcNow.ToString() + " Проверили количество подключенных каналов - ноль");
                botClient.Disconnect();
            }
            else
            {
                logs.WriteLog(DateTime.UtcNow.ToString() + " Проверили количество подключенных каналов - не ноль");
            }
        }

    }
}
