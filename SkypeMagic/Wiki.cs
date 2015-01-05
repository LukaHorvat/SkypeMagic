using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkypeMagic
{
    class Wiki
    {
        public static string SearchWiki(string topic)
        {
            var res = new WebClient().DownloadString("http://en.wikipedia.org/w/api.php?format=json&action=opensearch&search=" + Uri.EscapeDataString(topic) + "&limit=1");
            var match = Regex.Match(res, "\"(http:.*)\"");
            if (match.Success) return match.Groups[1].Value;
            return "Nope";
        }
    }
}
