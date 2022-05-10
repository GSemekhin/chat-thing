using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class SimpleAnswer : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;
        string answer;
        bool isActive;

        public SimpleAnswer(string _name, string _answer, bool _isActive) : base(_name)
        {
            answer = _answer;
            isActive = _isActive;
        }

        public override void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient)
        {
            if (isActive)
            {
                msgAgent.AddMessage(recipient + ", " + answer);
            }
        }
    }
}
