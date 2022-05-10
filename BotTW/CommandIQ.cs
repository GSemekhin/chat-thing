using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandIQ : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        Random rng;

        public CommandIQ(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                string answer;

                answer = "у тебя ";

                rng = new Random();
                int pointsIQ = rng.Next(1, 150);
                if (pointsIQ > 120)
                {
                    pointsIQ = rng.Next(1, 150);

                }
                else if ((pointsIQ < 40) || (pointsIQ > 100))
                {
                    pointsIQ = rng.Next(1, 150);
                }

                answer = answer + pointsIQ.ToString() + " iq";

                if (pointsIQ > 130)
                {
                    answer += " EZY";
                }
                else if (pointsIQ > 100)
                {
                    answer += " TehePelo";
                }
                else if (pointsIQ == 69)
                {
                    answer += " Kreygasm";
                }
                else if (pointsIQ < 50)
                {
                    answer += " SMOrc";
                }
                else
                {
                    answer += " :)";
                }

                msgAgent.AddMessage(recipient + ", " + answer);
            }
            else
            {
                msgAgent.AddMessage("Недостаточно прав для выполнения этой команды, " + /*chatID.ToString() +*/ ". Необходим уровень " + requiredAccessLevel);
            }
        }
    }
}
