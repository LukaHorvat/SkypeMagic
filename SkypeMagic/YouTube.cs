using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkypeMagic
{
    class YouTube
    {
        public static string SearchYouTube(string vid)
        {
            var res = new WebClient().DownloadString(String.Format("https://gdata.youtube.com/feeds/api/videos?q={0}&max-results=1&v=2&alt=json", Uri.EscapeUriString(vid)));
            var match = Regex.Match(res, @"(https://www\.youtube\.com/watch\?v=.*?)&");
            if (match.Success) return match.Groups[1].Value;
            return "Nope";
        }
    }
}
