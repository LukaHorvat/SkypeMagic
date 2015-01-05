using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeMagic
{
    class GHCi
    {
        static Process ghci;
        static bool listening = false;
        public static void SendCode(string code, Action<string> resp)
        {
            if (ghci == null || ghci.HasExited)
            {
                var info = new ProcessStartInfo("ghci")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };
                ghci = Process.Start(info);
                ghci.BeginOutputReadLine();
                ghci.BeginErrorReadLine();
                ghci.OutputDataReceived += delegate (object t, DataReceivedEventArgs args)
                {
                    resp(args.Data);
                };
                ghci.ErrorDataReceived += delegate (object t, DataReceivedEventArgs args)
                {
                    resp(args.Data);
                };
            }
            ghci.StandardInput.WriteLine(code);
        }
    }
}
