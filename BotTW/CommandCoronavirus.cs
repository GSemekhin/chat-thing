using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandCoronavirus : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        Random rng;

        public CommandCoronavirus(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                string answer;

                answer = "Шанс заражения COVID-19 у " + recipient + " ";

                rng = new Random();
                int chance = rng.Next(0, 101);

                answer = answer + chance.ToString() + "% ";

                if (chance == 0)
                {
                    answer += "EZY";
                }
                else if (chance == 100)
                {
                    answer += "pepoRope";
                }
                else
                {
                    answer += "coronaS coronaS coronaS";
                }

                msgAgent.AddMessage(answer);
            }
            else
            {
                msgAgent.AddMessage("Недостаточно прав для выполнения этой команды, " + /*chatID.ToString() +*/ ". Необходим уровень " + requiredAccessLevel);
            }
        }
    }
}

