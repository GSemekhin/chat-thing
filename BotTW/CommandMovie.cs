using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandMovie : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        private string movieName;
        public string MovieName
        {
            get
            {
                return movieName;
            }
            private set
            {
                movieName = value;
            }
        }
        public CommandMovie(string _name) : base(_name) { }

        public void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName,
            Role role, string recipient, string messageText, DB _bd)
        {
            List<string> moviesCommands = new List<string> {"!movie", "!film", "!cartoon", "!series", "!anime",
                "!ф", "!фильм", "!мульт", "!кино", "!сериал", "!мультик", "!аниме" };
            MovieName = messageText.Remove(0, messageText.IndexOf("!фильм ") + 7);
            if (MovieName == "-")
            {
                MovieName = "сейчас мы ничего не смотрим";
                foreach (string synonym in moviesCommands)
                {
                    _bd.AddSimpleAnswer(synonym, MovieName, true);
                }
                msgAgent.AddMessage(recipient + ", сейчас мы ничего не смотрим");
            }
            else
            {
                string commandMovieDB = "сейчас мы смотрим: " + MovieName;
                foreach (string synonym in moviesCommands)
                {
                    _bd.AddSimpleAnswer(synonym, commandMovieDB, true);
                }
                msgAgent.AddMessage(recipient + ", теперь название фильма: " + MovieName);
            }
        }
    }
}
