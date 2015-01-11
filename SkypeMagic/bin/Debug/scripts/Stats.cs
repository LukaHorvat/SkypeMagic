using System;
using System.Linq;
using SkypeMagic;
using System.Collections.Generic;

class Stats : Script
{
    public void Count()
    {
        var stats = Persist.Log.Aggregate(new Dictionary<string, int>(), (dict, msg) =>
        {
            if (dict.ContainsKey(msg.Sender)) dict[msg.Sender]++;
            else dict[msg.Sender] = 1;
            return dict;
        });
        foreach (var entry in stats)
        {
            Skype.SendMessageToConv(entry.Key + ": " + entry.Value + " poruka");
        }
    }

    public void Hours()
    {
        var count = new int[24];
        foreach (var msg in Persist.Log)
        {
            count[msg.TimeStamp.Hour]++;
        }
        var max = count.Max();
        for (int i = 0; i < 24; ++i)
        {
            var padding = i < 10 ? "0" : "";
            Skype.SendMessageToConv(padding + i + new string('#', (int)((float)count[i] / max * 30)) + " " + count[i]);
        }
    }
}