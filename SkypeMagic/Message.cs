using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeMagic
{
    [Serializable]
    public class Message : ICloneable
    {
        public long SkypeTime { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }

        public Message(long skypeTime, string sender, string text)
            : this()
        {
            SkypeTime = skypeTime;
            Sender = sender;
            Text = text;
        }

        public Message()
        {
            TimeStamp = DateTime.UtcNow.AddHours(1);
        }

        public object Clone()
        {
            return new Message(SkypeTime, Sender, Text) { TimeStamp = TimeStamp }; 
        }
    }
}
