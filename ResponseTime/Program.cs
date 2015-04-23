using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Timers;

namespace ResponseTime
{
    class Program
    {
        protected static bool Lock { get; set; }
        
        static void Main()
        {
            Lock = false;

            var interval = int.Parse(ConfigurationManager.AppSettings["Interval"]);
            var timer = new Timer(interval);
            timer.Elapsed += Run;
            timer.Enabled = true;

            Console.ReadKey();
        }

        static void Run(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Lock)
            {
                return;
            }

            Lock = true;
            Console.WriteLine("BEGIN: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                var urls = ConfigurationManager.AppSettings["Urls"].Split(',');
                foreach (var url in urls)
                {
                    var currentUrl = ConfigurationManager.AppSettings["Url" + url];
                    var begin = DateTime.Now;
                    GetResponse(currentUrl);
                    var end = DateTime.Now;
                    var time = (end - begin).TotalMilliseconds;
                    var output = begin.ToString("yyyy-MM-dd HH:mm:ss") + " " + time + " " + currentUrl;
                    AddToLog(output);
                    Console.WriteLine(output);
                }
                AddToLog("");
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR: " + exception.Message);
                Lock = false;
                return;
            }

            Console.WriteLine("END: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Lock = false;
        }

        static void GetResponse(string url)
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadString(url);
            }
        }

        static void AddToLog(string line)
        {
            File.AppendAllText(ConfigurationManager.AppSettings["LogPath"], line + "\r\n");
        }
    }
}
