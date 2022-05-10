using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;

namespace BotTW
{
    class Message
    {
        private readonly Role requiredAccessLevel = Role.Owner;

        Logs log;
        TwitchClient client;
        string channel;

        List<OneMessage> messages;

        public int MsgInterval { get; set; }

        public bool Silence { get; set; }

        #region Constructor
        public Message(Logs _log, TwitchClient _client, string _channel)
        {
            log = _log;
            client = _client;
            channel = _channel;
            MsgInterval = 1100;
            Silence = false;

            messages = new List<OneMessage>();

            Thread.Sleep(2000);
            Task.Run(() => Sending());
        }
        #endregion Constructor

        #region ChangeSendingMode
        public void ChangeSendingMode(Role senderRole, string senderName, bool requestedMode)
        {
            //часть уведомлений не будет отправлено, так как есть проверка на режим тишины непосредственно при отправке
            if (senderRole.HasFlag(requiredAccessLevel))
            {
                if (Silence == false && requestedMode == true)
                {
                    Silence = true;
                    AddMessage("Включен режим тишины " + senderName);
                    log.WriteLog(DateTime.UtcNow.ToString() + "Включен режим тишины " + senderName);
                }
                else if (Silence == true && requestedMode == false)
                {
                    Silence = false;
                    AddMessage("Отключен режим тишины " + senderName);
                    log.WriteLog(DateTime.UtcNow.ToString() + "Отключен режим тишины " + senderName);
                }
                else
                {
                    if (Silence == false)
                    {
                        AddMessage("Уже отключен режим тишины " + senderName);
                    }
                    else
                    {
                        AddMessage("Уже включен режим тишины " + senderName);
                    }
                    log.WriteLog(DateTime.UtcNow.ToString() + "Попытка смены режима отправки сообщения, запрошен действующий режим " + senderName + " " + requestedMode);
                }
            }
            else
            {
                log.WriteLog(DateTime.UtcNow.ToString() + "Попытка смены режима отправки сообщения, недостаточно прав" + senderName);
            }
        }
        #endregion ChangeSendingMode

        #region ChangeSendingInterval
        public void ChangeSendingInterval(Role senderRole, string senderName, int requestedInterval)
        {
            if (senderRole.HasFlag(requiredAccessLevel))
            {
                if (requestedInterval > 0 && requestedInterval < 10000)
                {
                    MsgInterval = requestedInterval;
                    AddMessage("Интервал отправки сообщений: " + requestedInterval + " " + senderName);
                    log.WriteLog(DateTime.UtcNow.ToString() + "Смена интервала отправки сообщений " + senderName + " " + requestedInterval);
                }
                else
                {
                    AddMessage("Предложенный интервал в недопустимом диапазоне, действует " + MsgInterval + " " + senderName);

                    log.WriteLog(DateTime.UtcNow.ToString() + "Попытка смены интервала отправки сообщений, недопустимый интервал  " + senderName + " " + requestedInterval);
                }
            }
            else
            {
                log.WriteLog(DateTime.UtcNow.ToString() + "Попытка смены интервала отправки сообщений, недостаточно прав " + senderName);
            }
        }
        #endregion ChangeSendingInterval

        #region AddMessage
        public void AddMessage(string msgText)
        {
            messages.Add(new OneMessage(string.Empty, msgText));
        }
        public void AddMessage(string replyTo, string msgText)
        {
            messages.Add(new OneMessage(replyTo, msgText));
        }
        #endregion AddMessage

        #region Sending
        public void Sending()
        {
            while (true)
            {
                if (messages.Count != 0)
                {

                    if (!Silence)
                    {
                        if (messages[0].messageText.Length < 500)
                        {
                            if (messages[0].replyTo == string.Empty)
                            {
                                client.SendMessage(channel, messages[0].messageText);
                            }
                            else
                            {
                                client.SendReply(channel, messages[0].replyTo, messages[0].messageText);
                            }
                        }
                        else
                        {
                            while (messages[0].messageText.Length / 500 > 0)
                            {
                                client.SendMessage(channel, messages[0].messageText.Substring(0, 499));
                                messages[0].messageText = messages[0].messageText.Remove(0, 499);
                                Thread.Sleep(MsgInterval);
                            }
                            client.SendMessage(channel, messages[0].messageText);
                        }
                    }
                    messages.RemoveAt(0);
                    Thread.Sleep(MsgInterval);
                }
                Thread.Sleep(50);
            }
        }
        #endregion Sending
    }
}
