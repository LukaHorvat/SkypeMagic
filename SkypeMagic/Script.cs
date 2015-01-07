using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SkypeMagic
{
    public class Script
    {
        protected Persist Persist;
        protected Skype Skype;

        public void Fill(Persist persist, Skype skype)
        {
            Persist = persist;
            Skype = skype;
        }
        public void Fork<T>(int seconds, T init, Func<Message, T, T> processor, Action<T> end = null)
        {
            Action<Message> del = delegate (Message msg)
            {
                init = processor(msg, init);
            };
            Skype.OnMessage += del;
            var timer = new Timer(seconds * 1000);
            timer.Elapsed += delegate
            {
                Skype.OnMessage -= del;
                if (end != null) end(init);
                timer.Stop();
            };
            timer.Start();
        }

        public string Join(string[] args)
        {
            return string.Join(" ", args);
        }
    }
}
