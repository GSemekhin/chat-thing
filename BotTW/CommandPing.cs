using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandPing : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        public CommandPing(string _name) : base(_name) { }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            string answer = "pong";
            if (senderName.ToLower() == "i_am_d0br0")
            {
                answer += ", слава богу";
            }
            msgAgent.AddMessage(answer);
        }
    }
}
