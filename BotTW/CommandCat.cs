using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandCat : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        public CommandCat(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                msgAgent.AddMessage("eto kot'");
            }
            else
            {
                msgAgent.AddMessage("Недостаточно прав для выполнения этой команды, " + /*chatID.ToString() +*/ ". Необходим уровень " + requiredAccessLevel);
            }
        }
    }
}
