using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace BotTW
{
    class API
    {

        const string API_PATH_LINUX = @"/var/shit/API.txt";
        const string API_PATH_WINDOWS = @"C:\Users\Gregory\Desktop\API.txt";

        private List<TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted> chatters;
        private List<TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted> chattersWithoutBots;

        static TwitchAPI api;

        Logs log;
        Message msg;
        DB bd;

        List<string> bots;

        private TwitchPubSub client;

        string apiTargetChannelName;

        string apiClientID;
        string apiToken;

        public delegate void MethodContainerStreamUp();
        public event MethodContainerStreamUp onStreamUpEvent;

        #region API constructor
        public API(Logs _log, Message _msg, DB _bd, List<string> _bots)
        {
            log = _log;
            msg = _msg;
            bd = _bd;
            bots = _bots;

            apiClientID = string.Empty;
            apiToken = string.Empty;
            apiTargetChannelName = string.Empty;

            StreamReader apiReader = null;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    apiReader = new StreamReader(API_PATH_WINDOWS);
                }
                else
                {
                    apiReader = new StreamReader(API_PATH_LINUX);
                }
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Не открылся поток на чтение информации из файла в АПИ");
            }

            try
            {
                apiClientID = apiReader.ReadLine();
                apiToken = apiReader.ReadLine();
                apiTargetChannelName = apiReader.ReadLine();
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Не прочиталась информация об АПИ из файла");
            }

            Login(apiClientID, apiToken);

            //Список зрителей
            System.Timers.Timer chattersTimer = new System.Timers.Timer();
            chattersTimer.Interval = 60000;
            chattersTimer.Elapsed += new System.Timers.ElapsedEventHandler(chattersTimer_Elapsed);
            chattersTimer.AutoReset = true;
            chattersTimer.Enabled = true;
            //Список зрителей
        }
        #endregion API constructor

        #region API login
        public void Login(string apiClientID, string apiToken)
        {
            try
            {
                api = new TwitchAPI();
                api.Settings.ClientId = apiClientID;
                api.Settings.AccessToken = apiToken;

                client = new TwitchPubSub();
                client.OnPubSubServiceConnected += onPubSubServiceConnected;
                client.OnRewardRedeemed += onRewardRedeemed;
                client.OnPubSubServiceClosed += OnPubSubServiceClosed;
                client.OnStreamUp += onStreamUp;

                client.OnPrediction += onPrediction;
                client.Connect();

                log.WriteLog("[Ошибка][Критичное] Попытка подключиться к АПИ");
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное]Не удалось подключиться к АПИ");
            }
        }
        #endregion API login

        #region chatters
        public List<TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted> GetChatters()
        {
            return this.chatters;
        }
        public List<TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted> GetChattersWithoutBots()
        {
            return this.chattersWithoutBots;
        }

        private async void chattersTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                chatters = await api.Undocumented.GetChattersAsync(apiTargetChannelName);
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное]Отвалилось получение списка зрителей");
            }
            //Добавление владельца канала
            chatters.Add(new TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted(apiTargetChannelName, TwitchLib.Api.Core.Enums.UserType.Broadcaster));
            bots = bd.GetBots();
            var selectedChatters = chatters.Where(i => !bots.Contains(i.Username));

            chattersWithoutBots = selectedChatters.ToList();
        }
        #endregion chatters

        private void onPrediction(object sender, OnPredictionArgs e)
        {
            if (e.Type == TwitchLib.PubSub.Enums.PredictionType.EventCreated)
            {
                msg.AddMessage("Ставочки на " + e.Title + " принимаются " + e.PredictionTime + " секунд CorgiDerp");
            }
            else if (e.Status == TwitchLib.PubSub.Enums.PredictionStatus.Locked)
            {
                msg.AddMessage("Ставочки закрыты boouchEshy");
            }
        }

        private void onStreamUp(object sender, OnStreamUpArgs e)
        {
            onStreamUpEvent?.Invoke();
        }
        private void onRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            if (e.RewardTitle.Equals("Очищение кармы") && (e.Status.Equals("UNFULFILLED")))
            {
                string answer;
                Random rng = new Random();
                int answerNo = rng.Next(0, 7);

                switch (answerNo)
                {
                    case 1:
                        answer = "Властью, данной мне стримером, я отпускаю тебе все грехи в автоматическом режиме, " + e.DisplayName + " MrDestructoid";
                        break;
                    case 2:
                        answer = e.DisplayName + ", да очистит тебя свет, отпускаю все твои грехи, дабы наконец-то обрел ты искупление, кое так долго искал CoolStoryBob";
                        break;
                    case 3:
                        answer = e.DisplayName + ", проходи в исповедальню - отпущу грехи твои KappaPride";
                        break;
                    case 4:
                        answer = e.DisplayName + ", отпускаю грехи твои - иди и шали дальше TehePelo";
                        break;
                    default:
                        answer = e.DisplayName + ", уплати ещё " + e.RewardCost + " звонких монет во славу бога нашего boouchEclown";
                        break;
                }
                msg.AddMessage(answer);
            }
            else
            if (e.RewardTitle.Equals("Хенд мейд куки") && (e.Status.Equals("UNFULFILLED")))
            {
                string answer = e.DisplayName + " становится фулл печеней boouchEComfort boouchEComfort boouchEComfort";

                msg.AddMessage(answer);
                answer = "boouchEsubHype boouchEsubHype boouchEsubHype";
                msg.AddMessage(answer);
            }
        }
        private void onPubSubServiceConnected(object sender, EventArgs e)
        {
            client.ListenToPredictions("131598181");
            client.ListenToRewards("131598181");
            client.ListenToVideoPlayback("131598181");
            client.SendTopics("o78rl66c2h4k3wo2hz19400h4ben3a");
            log.WriteLog("[Успех][Критичное] Подключились к АПИ");
        }

        private void OnPubSubServiceClosed(object sender, EventArgs e)
        {
            try
            {
                log.WriteLog("[Ошибка][Критичное] Отвалились награды, попытка переподключиться");
                Login(apiClientID, apiToken);
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Отвалились награды, не получилось переподключиться");
            }
        }
    }
}
