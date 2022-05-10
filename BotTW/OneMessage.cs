using System;
using System.Collections.Generic;
using System.Text;

namespace BotTW
{
    class OneMessage
    {
        public string replyTo;
        public string messageText;
        public OneMessage(string _replyTo, string _messageText)
        {
            replyTo = _replyTo;
            messageText = _messageText;
        }
    }
}
