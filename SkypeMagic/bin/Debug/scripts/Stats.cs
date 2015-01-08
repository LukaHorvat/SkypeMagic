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
}