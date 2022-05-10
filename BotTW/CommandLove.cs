using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandLove : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        Random rng;

        public CommandLove(string _name) : base(_name) { }

        public void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient, string messageText)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                string answer;



                if (messageText.Length - 1 > messageText.IndexOf("!люблю ") + 7)
                {
                    string loveObject = messageText.Remove(0, messageText.IndexOf("!люблю ") + 7);
                    rng = new Random();
                    int percentLove = rng.Next(0, 101);
                    if (percentLove == 35)
                    {
                        answer = senderName + " любит аниме AYAYA";
                    }
                    else if (percentLove == 74)
                    {
                        answer = senderName + " любит чатик GivePLZ";
                    }
                    else if (percentLove == 26)
                    {
                        answer = senderName + " любит аниме DansGame";
                    }
                    else if (percentLove == 16)
                    {
                        answer = senderName + " любит сельдерей boouchEclown";
                    }
                    else if (percentLove == 28)
                    {
                        answer = senderName + " любит хотдоги TriHard";
                    }
                    else if (percentLove == 29)
                    {
                        answer = senderName + " любит оливьешечку с колбаской и майонезом boouchEgasm";
                    }
                    else if (percentLove < 5)
                    {
                        answer = senderName + " ненавидит " + loveObject + " BibleThump";
                    }
                    else if (percentLove > 95)
                    {
                        answer = senderName + " обожает " + loveObject + " bleedPurple";
                    }
                    else
                    {
                        answer = senderName + " любит " + loveObject + " на " + percentLove.ToString() + "% <3";
                    }

                }
                else
                {
                    answer = senderName + ", напиши, что ты любишь <3";
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

