using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class Party : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        DB database;
        Message msgAgent;
        List<string> BadPhrase;
        public Party(string _name, DB _database, Message _msgAgent) : base(_name)
        {
            database = _database;
            msgAgent = _msgAgent;
            BadPhrase = database.GetPartyPhrase();
        }

        public void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient, string messageText)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                if (BadPhrase.Any(messageText.Contains))
                {
                    msgAgent.AddMessage("@" + senderName + ", мы играем на третьей сложности ");
                }
            }
            else
            {
                msgAgent.AddMessage(String.Empty/*"Недостаточно прав для выполнения этой команды, " + chatID.ToString() + ". Необходим уровень " + requiredAccessLevel*/);
            }
        }

        public List<string> Add(string badPhrase)
        {
            if (database.AddPartyPhrase(badPhrase, true))
            {
                msgAgent.AddMessage("В списке уже была фраза \"" + badPhrase + "\"");

            }
            else
            {
                msgAgent.AddMessage("В список добавлена фраза \"" + badPhrase + "\"");
            }
            BadPhrase = database.GetPartyPhrase();
            return BadPhrase;
        }

        public List<string> Delete(string badPhrase)
        {

            if (database.DeactivatePartyPhrase(badPhrase))
            {
                msgAgent.AddMessage("Удалена фраза \"" + badPhrase + "\"");
                BadPhrase = database.GetPartyPhrase();
            }
            else
            {
                msgAgent.AddMessage("В списке не было фразы \"" + badPhrase + "\"");
            }
            return BadPhrase;
        }

        public List<string> Update()
        {
            BadPhrase = database.GetPartyPhrase();
            msgAgent.AddMessage("Обновлён список плохих фраз");
            return BadPhrase;
        }
    }



}
