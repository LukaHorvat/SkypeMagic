using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SkypeMagic
{
    class Skype : IDisposable
    {
        string dbPath;
        string convName;
        SQLiteConnection conn;
        long lastReadId = 0;

        public Action<Message> OnMessage;

        public Skype(string dbPath, string convName)
        {
            this.dbPath = dbPath;
            this.convName = convName;

            conn = new SQLiteConnection("Data Source=" + dbPath);
            conn.Open();

            var timer = new Timer(1000);
            timer.Elapsed += delegate { CheckMessages(); };
            timer.Start();
        }

        private void CheckMessages()
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, author, body_xml FROM Messages WHERE id > " + lastReadId + " ORDER BY id";
            var ad = new SQLiteDataAdapter(cmd);
            var set = new DataSet();
            ad.Fill(set);

            if (lastReadId != 0)
            {
                for (int i = 0; i < set.Tables[0].Rows.Count; ++i)
                {
                    var row = set.Tables[0].Rows[i];
                    try
                    {
                        if (OnMessage != null) OnMessage(new Message(WebUtility.HtmlDecode((string)row[1]), WebUtility.HtmlDecode((string)row[2])));
                    }
                    catch (InvalidCastException)
                    {
                        Console.WriteLine("Can't cast to string");
                    }
                }
            }
            if (set.Tables[0].Rows.Count > 0) lastReadId = (long)set.Tables[0].Rows[set.Tables[0].Rows.Count - 1][0];
        }

        public void SendMessageToConv(string msg)
        {
            try
            {
                var skypeHandle = Process.GetProcessesByName("Skype")[0].MainWindowHandle;
                var conv = WinAPI.FindWindowEx(skypeHandle, IntPtr.Zero, "TConversationForm", convName);
                var ctrl = WinAPI.FindWindowEx(conv, IntPtr.Zero, "TChatEntryControl", "");
                var textBox = WinAPI.FindWindowEx(ctrl, IntPtr.Zero, "TChatRichEdit", "");

                WinAPI.SendMessage(textBox, WinAPI.WM_SETTEXT, 0, msg);
                WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.RETURN, null);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Skype is not running");
            }
        }

        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
