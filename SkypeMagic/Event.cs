using System;
using System.Collections.Generic;

namespace SkypeMagic
{
    [Serializable]
    public class Event
    {
        public DateTime When { get; set; }
        public string Name { get; set; }
        public List<string> Comming { get; set; }

        public Event()
        {
            Comming = new List<string>();
        }

        public Event(DateTime when, string name)
            : this()
        {
            When = when;
            Name = name;
        }
    }
}