using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SkypeMagic
{
    [Serializable]
    public class Persist
    {
        public List<Message> Log;
        public List<Message> Links;
        public List<Event> Events;

        public Persist()
        {
            Log = new List<Message>();
            Links = new List<Message>();
            Events = new List<Event>();
        }
    }
}
