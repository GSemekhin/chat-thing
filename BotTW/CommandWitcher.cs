using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandWitcher : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        public CommandWitcher(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                DateTime lastWitcherStream = new DateTime(2019, 5, 6, 17, 10, 0);
                TimeSpan delta = DateTime.UtcNow.Subtract(lastWitcherStream);
                string answer = "С последнего стрима по Ведьмаку прошло " +
                    delta.Days.ToString() + " " + Declension.GetDeclension(delta.Days, "день", "дня", "дней") + " " +
                    delta.Hours.ToString() + " " + Declension.GetDeclension(delta.Hours, "час", "часа", "часов") + " " +
                    delta.Minutes.ToString() + " " + Declension.GetDeclension(delta.Minutes, "минута", "минуты", "минут") + " " +
                    delta.Seconds.ToString() + " " + Declension.GetDeclension(delta.Seconds, "секунда", "секунды", "секунд") + " FeelsBadMan";
                answer = answer + " Зато есть запись прохождения дополнения https://www.twitch.tv/collections/V3q_SNuoDBYo2Q FeelsGoodMan";
                Console.WriteLine();
                msgAgent.AddMessage(answer);
            }
            else
            {
                msgAgent.AddMessage(String.Empty/*"Недостаточно прав для выполнения этой команды, " + chatID.ToString() + ". Необходим уровень " + requiredAccessLevel*/);
            }
        }
    }
}
