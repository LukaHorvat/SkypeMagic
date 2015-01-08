using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace SkypeMagic
{
    class Bot
    {
        Persist persist;
        Skype skype;
        Dictionary<string, Action<string[]>> scripts;
        List<Action<Message>> rawScripts;

        public Bot(Persist persist)
        {
            var db = File.Exists(@"C:\Users\Luka\AppData\Roaming\Skype\luka.horvat2\main.db") ? @"C:\Users\Luka\AppData\Roaming\Skype\luka.horvat2\main.db" : @"C:\Users\Administrator\AppData\Roaming\Skype\gacar.bot\main.db";
            var conv = Console.ReadLine();
            skype = new Skype(db, conv);
            this.persist = persist;

            scripts = new Dictionary<string, Action<string[]>>();
            rawScripts = new List<Action<Message>>();
            var watcher = new FileSystemWatcher("scripts");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            watcher.EnableRaisingEvents = true;
            watcher.Created += Recompile;
            watcher.Changed += Recompile;
            watcher.Deleted += Recompile;
            watcher.Renamed += Recompile;
            Recompile(null, null);

            skype.OnMessage += delegate (Message msg)
            {
                persist.Log.Add(msg);
                foreach (var scr in rawScripts) scr(msg);
                if (msg.Sender == "gacar.bot") return;
                var match = Regex.Match(msg.Text, @"!(\w*)(.*)");
                if (!match.Success) return;
                if (scripts.ContainsKey(match.Groups[1].Value)) scripts[match.Groups[1].Value](match.Groups[2].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            };
        }

        public void Recompile(object t, FileSystemEventArgs args)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var parameters = new CompilerParameters(new[] { "System.dll", "System.Core.dll", "System.Data.dll", "System.Net.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Reflection.Metadata.dll", "SkypeMagic.exe" });
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;

            var res = provider.CompileAssemblyFromFile(parameters, Directory.GetFiles("scripts"));
            foreach (CompilerError err in res.Errors) Console.WriteLine(err);
            if (res.Errors.Count > 0) return;
            var types = res.CompiledAssembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(Script)));
            var methods = types.SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
            scripts.Clear();
            rawScripts.Clear();
            foreach (var info in methods)
            {
                if (info.Name == "Raw") rawScripts.Add(msg => info.Invoke(MakeNewObj(info.DeclaringType), new[] { msg }));
                else scripts.Add(info.Name.ToLower(), MakeScript(info));
            }
        }

        private Action<string[]> MakeScript(MethodInfo info)
        {
            var types = info.GetParameters().Select(x => x.ParameterType);
            return (string[] strs) =>
            {
                var numArgs = types.Count();
                if (numArgs > 0 && types.Last() == typeof(string[]))
                {
                    strs = strs.Take(numArgs - 1).Concat(new[] { string.Join(" ", strs.Skip(numArgs - 1)) }).ToArray();
                }
                if (strs.Count() != types.Count())
                {
                    skype.SendMessageToConv("Krivi broj argumenata");
                    return;
                }
                var args = types.Zip<Type, string, object>(strs, (type, str) =>
                {
                    if (type == typeof(string)) return str;
                    if (type == typeof(int)) return int.Parse(str);
                    if (type == typeof(bool)) return bool.Parse(str);
                    if (type == typeof(long)) return long.Parse(str);
                    if (type == typeof(string[])) return str.Split(' ');
                    skype.SendMessageToConv("Krivi tip argumenta. Očekivan " + type + " za argument " + str);
                    return null;
                });
                var script = MakeNewObj(info.DeclaringType);
                info.Invoke(script, args.ToArray());
            };
        }

        private Script MakeNewObj(Type type)
        {
            Script script = (Script)Activator.CreateInstance(type);
            script.Fill(persist, skype);
            return script;
        }
    }
}
