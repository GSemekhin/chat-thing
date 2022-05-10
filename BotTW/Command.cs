using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class Command
    {


        public Command(string _name)
        {
            Name = _name;
        }

        public string Name { get; set; }

        public virtual void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient) { }
    }
}
