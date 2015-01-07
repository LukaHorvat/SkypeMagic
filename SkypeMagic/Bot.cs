using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace SkypeMagic
{
    class Bot
    {
        Skype skype;

        public Bot(Persist persist)
        {
            var db = File.Exists(@"C:\Users\Luka\AppData\Roaming\Skype\luka.horvat2\main.db") ? @"C:\Users\Luka\AppData\Roaming\Skype\luka.horvat2\main.db" : @"C:\Users\Administrator\AppData\Roaming\Skype\gacar.bot\main.db";
            var conv = Console.ReadLine();
            skype = new Skype(db, conv);
            skype.OnMessage += delegate (Message msg)
            {
                persist.Log.Add(msg);
                if (msg.Sender == "gacar.bot") return;
                if (msg.Text == "!gacar introduce") skype.SendMessageToConv("Bok. Ja sam jebeni bot");
                if (msg.Text.StartsWith("!wiki ")) skype.SendMessageToConv(Wiki.SearchWiki(msg.Text.Substring("!wiki ".Length)));
                if (msg.Text.StartsWith("!youtube ")) skype.SendMessageToConv(YouTube.SearchYouTube(msg.Text.Substring("!youtube".Length)));
                if (msg.Text.StartsWith("!votekick "))
                {
                    var name = msg.Text.Substring("!votekick ".Length);
                    skype.SendMessageToConv("Otvorena minuta za glasanje za '/kick " + name + "'. Potrebno " + votesToKick + " glasova. Napisi '!yes' da glasaš.");
                    Fork(60, Tuple.Create(name, new SortedSet<string>(), false), VoteKick, _ => skype.SendMessageToConv("Isteklo vrijeme za glasanje"));
                }
                if (msg.Text.StartsWith("!ghci ")) GHCi.SendCode(msg.Text.Substring("!ghci ".Length), str => skype.SendMessageToConv(" " + str));
                if (msg.Text.Contains("http")) persist.Links.Add(msg);
                if (msg.Text.StartsWith("!links"))
                {
                    skype.SendMessageToConv("Zadnjih 10 poslanih linkova:");
                    foreach (var link in persist.Links.Reverse<Message>().Take(10).Reverse()) SendMessageLog(link);
                }
                if (msg.Text.StartsWith("!phone "))
                {
                    var person = msg.Text.Substring("!phone ".Length);
                    if (person == "svi") skype.SendMessageToConv(File.ReadAllText("phones.txt"));
                    var numbers = File.ReadAllLines("phones.txt");
                    foreach (var line in numbers)
                    {
                        var nameNum = line.Split(' ');
                        if (nameNum[0].Split('|').Contains(person)) skype.SendMessageToConv("Broj od " + person + " je " + nameNum[1]);
                    }
                }
            };
        }

        int votesToKick = 4;
        public Tuple<string, SortedSet<string>, bool> VoteKick(Message msg, Tuple<string, SortedSet<string>, bool> votes)
        {
            if (votes.Item3) return votes;
            if (msg.Text == "!yes")
            {
                if (votes.Item2.Contains(msg.Sender)) return votes;
                votes.Item2.Add(msg.Sender);
                skype.SendMessageToConv(votes.Item2.Count + "/" + votesToKick + " za '/kick " + votes.Item1 + "'");
                if (votes.Item2.Count >= votesToKick)
                {
                    skype.SendMessageToConv("/kick " + votes.Item1);
                    skype.SendMessageToConv("/add " + votes.Item1);
                    return Tuple.Create(votes.Item1, votes.Item2, true);
                }
            }
            return votes;
        }

        public void Fork<T>(int seconds, T init, Func<Message, T, T> processor, Action<T> end = null)
        {
            Action<Message> del = delegate (Message msg)
            {
                init = processor(msg, init);
            };
            skype.OnMessage += del;
            var timer = new Timer(seconds * 1000);
            timer.Elapsed += delegate
            {
                skype.OnMessage -= del;
                if (end != null) end(init);
                timer.Stop();
            };
            timer.Start();
        }

        public void SendMessageLog(Message msg)
        {
            var time = msg.TimeStamp;
            skype.SendMessageToConv(string.Format("[{0}.{1} {2}:{3} {4}] {5}", 
                time.Day, time.Month, 
                time.Hour, time.Minute, 
                msg.Sender, Regex.Replace(msg.Text, "<.*?>", "")));
        }
    }
}
