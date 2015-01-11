using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Collections.Generic;

namespace SkypeMagic
{
    public class Skype
    {
        string dbPath;
        string convName;
        SQLiteAsyncConnection conn;
        long lastReadTs = 0;

        public Action<Message> OnMessage;
        public Action<Leave> OnLeave;

        public bool SpamThrottle = true;
        private List<Tuple<DateTime, string>> spam;

        public Skype(string dbPath, string convName)
        {
            this.dbPath = dbPath;
            this.convName = convName;

            conn = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.NoMutex);
            lastReadTs = conn.ExecuteScalarAsync<long>("SELECT timestamp FROM Messages ORDER BY id DESC LIMIT 1").Result;

            var watcher = new FileSystemWatcher(Path.GetDirectoryName(dbPath), Path.GetFileName(dbPath));
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += delegate (object t, FileSystemEventArgs args)
            {
                CheckMessages();
            };

            var timer = new Timer(10 * 60 * 1000);
            timer.Elapsed += delegate
            {
                WithTextBox(textBox => //To stop Skype from doing idle things
                {
                    WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.SPACE, null);
                    WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.BACK, null);
                });
            };
            timer.Start();

            spam = new List<Tuple<DateTime, string>>();
        }

        private async void CheckMessages()
        {
            try
            {
                await Task.Delay(100); //File is locked when this event fires so we wait for a bit first
                var res = await conn.QueryAsync<Message>("SELECT timestamp as SkypeTime, author as Sender, body_xml as Text FROM Messages WHERE timestamp > ? ORDER BY id ASC", lastReadTs);
                var leaves = await conn.QueryAsync<Leave>("SELECT leavereason as Reason, identities as Name FROM Messages WHERE timestamp > ? ORDER BY id ASC", lastReadTs);
                res = res.Where(m => m.Text != null).ToList();
                leaves = leaves.Where(l => l.Reason == 6).ToList();
                foreach (var msg in res)
                {
                    msg.Sender = WebUtility.HtmlDecode(msg.Sender);
                    msg.Text = WebUtility.HtmlDecode(msg.Text);
                }

                try
                {
                    if (OnMessage != null)
                    {
                        foreach (var msg in res) OnMessage(msg);
                    }
                    if (OnLeave != null)
                    {
                        foreach (var lv in leaves) OnLeave(lv);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    while (e != null)
                    {
                        Console.WriteLine(e.Message);
                        e = e.InnerException;
                    }
                }
                if (res.Count > 0) lastReadTs = res[res.Count - 1].SkypeTime;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendMessageToConv(string msg)
        {
            for (var i = 0; i < spam.Count; ++i)
            {
                if (DateTime.Now - spam[i].Item1 > TimeSpan.FromSeconds(10))
                {
                    spam.RemoveAt(i);
                    --i;
                }
            }
            if (SpamThrottle && spam.Any(t => t.Item2 == msg)) return;
            WithTextBox(textBox =>
            {
                WinAPI.SendMessageW(textBox, WinAPI.WM_SETTEXT, 0, msg);
                WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.RETURN, null);
            });
            spam.Add(Tuple.Create(DateTime.Now, msg));
        }

        private void WithTextBox(Action<IntPtr> action)
        {
            try
            {
                var skypeHandle = Process.GetProcessesByName("Skype")[0].MainWindowHandle;
                IntPtr conv;
                conv = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TConversationForm", convName);
                if (conv == IntPtr.Zero) conv = WinAPI.FindWindowEx(skypeHandle, IntPtr.Zero, "TConversationForm", convName);
                var ctrl = WinAPI.FindWindowEx(conv, IntPtr.Zero, "TChatEntryControl", "");
                var textBox = WinAPI.FindWindowEx(ctrl, IntPtr.Zero, "TChatRichEdit", "");
                action(textBox);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Skype is not running");
            }
        }
    }
}
