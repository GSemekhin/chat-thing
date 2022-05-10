using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandDick : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        Random rng;

        public CommandDick(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                string answer;

                answer = "твой шланг ";

                rng = new Random();
                int dickSize = rng.Next(1, 20);
                if (dickSize > 15)
                {
                    dickSize = rng.Next(15, 45);

                }
                else if ((dickSize < 10) || (dickSize > 17))
                {
                    dickSize = rng.Next(1, 15);
                }

                answer = answer + dickSize.ToString() + " см";

                if (dickSize > 31)
                {
                    answer += " gachiGASM";
                }
                else if (dickSize < 7)
                {
                    answer += " boouchEclown";
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
