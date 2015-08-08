using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ReportIPTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string connectionName = ConfigurationManager.AppSettings["ConnectionName"];
            using (Process rasphoneDisconnect = new Process())
            {
                rasphoneDisconnect.StartInfo = new ProcessStartInfo("rasphone", $"-h {connectionName}");
                rasphoneDisconnect.Start();
                rasphoneDisconnect.WaitForExit();
            }

            using (Process rasphoneReconnect = new Process())
            {
                rasphoneReconnect.StartInfo = new ProcessStartInfo("rasphone", $"-d {connectionName}");
                rasphoneReconnect.Start();
                rasphoneReconnect.WaitForExit();
            }

            string ipconfigContent;
            using (Process ipconfigAll = new Process())
            {
                ipconfigAll.StartInfo = new ProcessStartInfo("ipconfig", "/all");
                ipconfigAll.StartInfo.UseShellExecute = false;
                ipconfigAll.StartInfo.RedirectStandardOutput = true;
                ipconfigAll.Start();

                using (StreamReader sr = ipconfigAll.StandardOutput)
                {
                    ipconfigContent = sr.ReadToEnd();
                }
            }

            using (var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpHost"]))
            {
                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"]);
                    msg.To.Add(ConfigurationManager.AppSettings["MailTo"]);
                    msg.Subject = DateTime.Now.ToShortDateString() + " IP";
                    msg.Body = ipconfigContent;
                    client.Send(msg);
                }
            }
        }
    }
}