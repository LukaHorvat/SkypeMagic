using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeMagic
{
    class Message
    {
        public string Sender;
        public string Text;

        public Message(string sender, string text)
        {
            Sender = sender;
            Text = text;
        }
    }
}
