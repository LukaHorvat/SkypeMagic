using SkypeMagic;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

class Google : Script
{
    public void Search(string[] terms)
    {
        try
        {
            var query = Uri.EscapeUriString(Join(terms));
            var res = new WebClient().DownloadString(string.Format("https://www.googleapis.com/customsearch/v1?cx=010778794215221516758%3Av41qnazottc&q={0}&key=AIzaSyBgSJdE10mzKIBIwZReX5ykLJGyAqmVH30", query));
            var matches = Regex.Match(res, @"""items"".*?""snippet"": ""(.*?)"".*?""formattedUrl"": ""(.*?)""", RegexOptions.Singleline);
            if (matches.Success)
            {
                Skype.SendMessageToConv(matches.Groups[1].Value);
                Skype.SendMessageToConv(matches.Groups[2].Value);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}