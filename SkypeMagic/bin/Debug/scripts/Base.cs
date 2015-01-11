using SkypeMagic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

public class Base : Script
{
    public void Gacar(string cmd)
    {
        if (cmd == "introduce") Skype.SendMessageToConv("Bok. Ja sam Bot Gacar.");
    }

    public void Test(string[] all)
    {
        Skype.SendMessageToConv(string.Concat(all));
    }

    public void Wiki(string[] topic)
    {
        Skype.SendMessageToConv(SearchWiki(Join(topic)));
    }

    public void YouTube(string[] vid)
    {
        Skype.SendMessageToConv(SearchYouTube(Join(vid)));
    }

    private int votesToKick = 4;
    public void VoteKick(string name)
    {
        Skype.SendMessageToConv("Otvorena minuta za glasanje za '/kick " + name + "'. Potrebno " + votesToKick + " glasova. Napisi '!yes' da glasaš.");
        Fork(60, Tuple.Create(name, new SortedSet<string>(), false), VoteKick, _ => Skype.SendMessageToConv("Isteklo vrijeme za glasanje"));
    }

    private Tuple<string, SortedSet<string>, bool> VoteKick(Message msg, Tuple<string, SortedSet<string>, bool> votes)
    {
        if (votes.Item3) return votes;
        if (msg.Text == "!yes")
        {
            if (votes.Item2.Contains(msg.Sender)) return votes;
            votes.Item2.Add(msg.Sender);
            Skype.SendMessageToConv(votes.Item2.Count + "/" + votesToKick + " za '/kick " + votes.Item1 + "'");
            if (votes.Item2.Count >= votesToKick)
            {
                Skype.SendMessageToConv("/kick " + votes.Item1);
                Skype.SendMessageToConv("/add " + votes.Item1);
                return Tuple.Create(votes.Item1, votes.Item2, true);
            }
        }
        return votes;
    }

    public void Links()
    {
        Skype.SendMessageToConv("Zadnjih 10 poslanih linkova:");
        foreach (var link in Persist.Links.Reverse<Message>().Take(10).Reverse()) SendMessageLog(link);
    }

    public void Phone(string person)
    {
        if (person == "svi") Skype.SendMessageToConv(File.ReadAllText("phones.txt"));
        var numbers = File.ReadAllLines("phones.txt");
        foreach (var line in numbers)
        {
            var nameNum = line.Split(' ');
            if (nameNum[0].Split('|').Contains(person)) Skype.SendMessageToConv("Broj od " + person + " je " + nameNum[1]);
        }
    }

    public void Raw(Message msg)
    {
        if (msg.Text.Contains("http") && !msg.Text.StartsWith("["))
        {
            var link = Regex.Match(msg.Text, "http[^ ]*").Groups[0].Value;
            var backlog = Persist.Links.Reverse<Message>().Take(20);
            if (backlog.Any(m => m.Text.Contains(link)))
            {
                Skype.SendMessageToConv("Taj link je vec postan:");
                SendMessageLog(backlog.First(m => m.Text.Contains(link)));
            }
            else Persist.Links.Add(msg);
        }
        if (msg.Sender == "dito_imamskype" && msg.Text.StartsWith("!ghci")) VoteKick("dito_imamskype");
    }


    private string SearchWiki(string topic)
    {
        var res = new WebClient().DownloadString("http://en.wikipedia.org/w/api.php?format=json&action=opensearch&search=" + Uri.EscapeDataString(topic) + "&limit=1");
        var match = Regex.Match(res, "\"(http:.*)\"");
        if (match.Success) return match.Groups[1].Value;
        return "Nope";
    }

    private string SearchYouTube(string vid)
    {
        var res = new WebClient().DownloadString(string.Format("https://gdata.youtube.com/feeds/api/videos?q={0}&max-results=1&v=2&alt=json", Uri.EscapeUriString(vid)));
        var match = Regex.Match(res, @"(https://www\.youtube\.com/watch\?v=.*?)&");
        if (match.Success) return match.Groups[1].Value;
        return "Nope";
    }
}