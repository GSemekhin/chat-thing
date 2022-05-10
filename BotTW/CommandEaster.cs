using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandEaster : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;
        Random rng;

        public CommandEaster(string _name) : base(_name) { }

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
                    answer = senderName + " делает творожную пасху для " + chatters[teaRecipient].Username + " FeelsKinkyMan";
                }
                else if (chatters[teaRecipient].Username.ToLower().Equals("boouche"))
                {
                    answer = senderName + " делится куличиком с " + chatters[teaRecipient].Username + " boouchEgasm";
                }
                else
                {
                    answer = senderName + " и " + chatters[teaRecipient].Username + " стучатся яичками KappaPride";
                }
                //конец генерации

                msgAgent.AddMessage(answer);
            }
        }
    }
}
