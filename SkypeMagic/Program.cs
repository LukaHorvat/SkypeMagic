using System;
using System.IO;
using System.IO.Compression;
using System.Timers;
using System.Xml.Serialization;

namespace SkypeMagic
{
    class Program
    {
        static Persist stats;
        static XmlSerializer serializer;

        static void Main(string[] args)
        {
            serializer = new XmlSerializer(typeof(Persist));
            if (File.Exists("data/persist.zip"))
            {
                using (var file = ZipFile.OpenRead("data/persist.zip"))
                using (var reader = new StreamReader(file.GetEntry("persist.xml").Open()))
                {
                    stats = (Persist)serializer.Deserialize(reader);
                }
            }
            else
            {
                stats = new Persist();
                using (var file = ZipFile.Open("data/persist.zip", ZipArchiveMode.Create))
                {
                    file.CreateEntry("persist.xml");
                }
                Backup();
            }
            var bot = new Bot(stats);
            var timer = new Timer(60 * 1000);
            timer.Elapsed += delegate
            {
                Backup();
            };
            timer.Start();

            Console.WriteLine("Bot running. Press any key to backup data and close.");
            Console.ReadLine();
            Backup();
        }

        static void Backup()
        {
            using (var file = ZipFile.Open("data/persist.zip", ZipArchiveMode.Update))
            using (var writer = new StreamWriter(file.GetEntry("persist.xml").Open()))
            {
                serializer.Serialize(writer, stats);
            }
        }
    }
}
