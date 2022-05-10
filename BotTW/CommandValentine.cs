using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;

namespace BotTW
{
    class CommandValentine : Command
    {
        private readonly Role requiredAccessLevel = Role.Any;

        Random rng;
        Random rngValentine;

        public CommandValentine(string _name) : base(_name) { }

        public void Action(TwitchClient botClient, Message msgAgent, string senderName, string channelName, Role role, string recipient, List<TwitchLib.Api.Core.Models.Undocumented.Chatters.ChatterFormatted> chatters)
        {
            if (role.HasFlag(requiredAccessLevel))
            {
                if (chatters != null && chatters.Count != 0)
                {
                    string answer;

                    rngValentine = new Random();
                    int rngRecipient = rngValentine.Next(0, chatters.Count);

                    answer = senderName + " дарит " + chatters[rngRecipient].Username + " валентинку со словами: ";
                    List<string> punchlines = new List<string>
                {
                    "С тобой у нас любовь и ласка, как у хлебушка с колбаской (´꒳`)♡",
                    "Будь ты картинкой, я бы тебя сохранил (❤️ω❤️)",
                    "Люблю твой сладкий рулет OwO",
                    "Ты случайно не гугл, тогда почему я нашел в тебе всё, что мне нужно? UwU",
                    "Я буду мурчать только для тебя (´ ε ` )♡",
                    "Ты, конечно, не ящик пивчанского, но когда я вижу тебя моё сердце бьется чаще (´,,•ω•,,)♡",
                    "Ты лучше сына маминой подруги ♡ (⇀ 3 ↼)",
                    "Твои родители случайно не пекари? Тогда откуда у них такая булочка (´｡• ᵕ •｡`) ♡",
                    "Твоя мама случайно не медуза Горгона? От твоего взгляда мой член каменеет (≧◡≦) ♡",
                    "Твои родители случайно не писатели? Тогда откуда у них такая сказка (´• ω •`) ♡",
                    "Ты случайно не баунти? Просто каждая минута с тобой райское наслаждение  ♡(｡-ω -)",
                    "Представляешь, срок годности булочки с корицей всего 3 дня.Заюш, как ты еще держишься? (｡・//ε//・｡)",
                    "Твои родители случайно не вегетарианцы?Тогда откуда у них такой персик? ♡ (⇀ 3 ↼)",
                    "Знаешь почему ты так сильно устаешь к концу дня? Потому что ты весь день был замечательным котеночком, а это тяжелый труд ♡( ◡‿◡ )",
                    "Самый милый котик сейчас читает это сообщение (*♡∀♡)",
                    "Мир прекрасен, потому что в нем есть ты ( ˘⌣˘)♡(˘⌣˘ )",
                    "Ты почувствовал(а) любовь с первого взгляда, или мне взглянуть ещё раз? ♡ ～('▽^人)",
                    "Ради тебя за пивом хоть в  5 утра (￣ε￣＠)",
                    "Я конечно не колода карт, но меня ты тоже можешь разложить на столе ♡( ◡‿◡ )",
                    "Ты конечно не коктейль с трубочкой, но я бы тебя засосал (*♡∀♡)",
                    "Я конечно не мороженое, но тоже таю в твоих объятиях (￣ε￣＠)",
                    "Сори, но из сладенького сегодня только я ♡(｡-ω -)"
                };


                    rng = new Random();
                    int punchlineNumber = rng.Next(0, punchlines.Count);





                    msgAgent.AddMessage(answer + punchlines[punchlineNumber]);
                }
            }
            else
            {
                msgAgent.AddMessage("Недостаточно прав для выполнения этой команды, " + /*chatID.ToString() +*/ ". Необходим уровень " + requiredAccessLevel);
            }
        }
    }
}
