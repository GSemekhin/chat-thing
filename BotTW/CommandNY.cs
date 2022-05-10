using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandNY : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;


        public CommandNY(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                DateTime NearestNewYear = new DateTime(DateTime.Now.Year + 1, 1, 1);
                DateTime Today = DateTime.Today;
                TimeSpan DaysBeforeNY = NearestNewYear - Today;

                string answer;

                answer = ", до Нового года " + DaysBeforeNY.Days.ToString() +
                        Declension.GetDeclension(DaysBeforeNY.Days, " день", " дня", " дней") +
                        " 🎄 boouchEComfort";


                msgAgent.AddMessage(recipient + answer);
            }
            else
            {
                msgAgent.AddMessage("Недостаточно прав для выполнения этой команды, " + /*chatID.ToString() +*/ ". Необходим уровень " + requiredAccessLevel);
            }
        }
    }
}
