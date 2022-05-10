using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandAge : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        Random rng;

        public CommandAge(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                string answer;

                answer = "стримеру ";

                rng = new Random();
                int age = rng.Next(13, 63);
                

                answer = answer + age.ToString() + Declension.GetDeclension(age, " год", " года", " лет");

                msgAgent.AddMessage(recipient + ", " + answer);
            }
            else
            {
                msgAgent.AddMessage("Недостаточно прав для выполнения этой команды, " + /*chatID.ToString() +*/ ". Необходим уровень " + requiredAccessLevel);
            }
        }
    }
}
