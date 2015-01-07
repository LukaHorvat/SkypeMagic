using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace SkypeMagic
{
    class Skype
    {
        string dbPath;
        string convName;
        SQLiteAsyncConnection conn;
        long lastReadTs = 0;

        public Action<Message> OnMessage;

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
                WithTextBox(textBox =>
                {
                    WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.SPACE, null);
                    WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.BACK, null);
                });
            };
            timer.Start();
        }

        private async void CheckMessages()
        {
            try
            {
                await Task.Delay(100);
                var res = await conn.QueryAsync<Message>("SELECT timestamp as SkypeTime, author as Sender, body_xml as Text FROM Messages WHERE timestamp > ? ORDER BY id ASC", lastReadTs);

                if (OnMessage != null)
                {
                    foreach (var msg in res) OnMessage(msg);
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
            WithTextBox(textBox =>
            {
                WinAPI.SendMessage(textBox, WinAPI.WM_SETTEXT, 0, msg);
                WinAPI.SendMessage(textBox, WinAPI.WM_KEYDOWN, (int)VirtualKeyShort.RETURN, null);
            });
        }

        private void WithTextBox(Action<IntPtr> action)
        {
            try
            {
                var skypeHandle = Process.GetProcessesByName("Skype")[0].MainWindowHandle;
                var conv = WinAPI.FindWindowEx(skypeHandle, IntPtr.Zero, "TConversationForm", convName);
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
