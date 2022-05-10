using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandTea : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;
        Random rng;

        public CommandTea(string _name) : base(_name) { }

        public void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient, List<TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted> chatters)
        {

            if (chatters != null && chatters.Count != 0)
            {
                string answer = string.Empty;

                rng = new Random();
                int teaRecipient = rng.Next(0, chatters.Count);
                

                //генерация смайлика в зависимости от получателя
                if (chatters[teaRecipient].Username.ToLower().Equals(senderName.ToLower()))
                {
                    answer = senderName + " заваривает чаёк для " + chatters[teaRecipient].Username + " ForeverAlone";
                }
                else if (chatters[teaRecipient].Username.ToLower().Equals("boouche"))
                {
                    answer = senderName + " заваривает чаёк для " + chatters[teaRecipient].Username + " boouchEgasm";
                }
                else if (chatters[teaRecipient].Username.ToLower().Equals("taburetka_bot") || chatters[teaRecipient].Username.ToLower().Equals("nightbot"))
                {
                    answer = senderName + " заваривает чаёк для " + chatters[teaRecipient].Username + " MrDestructoid";
                }
                else
                {
                    answer = senderName + " заваривает чаёк для " + chatters[teaRecipient].Username + " boouchEtea";
                }
                //конец генерации

                msgAgent.AddMessage(answer);
            }
        }
    }
}
