using System;
using System.Net.Mail;

namespace ServiceMonitor
{
    public class Notification
    {
        public static void SendEmail(string serviceName, double memorySize, int memoryThreshold, bool restartSuccess)
        {
            SmtpClient client = new SmtpClient("smtp4apps.pylusd.org");
            string from = "ServiceMonitor@pylusd.org";
            string to = "kquinn@pylusd.org";
            string subject = $"Service {serviceName} -";

            if (restartSuccess)
            {
                subject += "Memory Threshold Exceeded";
            }
            else
            {
                subject += "WARNING - Service failed to restart";
            }

            string message = $"Service:{serviceName}\nMemory Threshold:{memoryThreshold}MB\nCurrent Memory Usage:{memorySize}MB";

            string messageFooter =
                "\n\n\n" +
                "-------------------------------------" +
                $"\nSent from: {Environment.MachineName}" +
                $"\nDate Time:{DateTime.Now}" +
                "\n-------------------------------------";
            if (restartSuccess)
            {
                message += $"\nService was stopped, files cleared, and service restarted correctly.";
            }
            else
            {
                message += $"\n WARNING: Service was stopped and failed to restart correctly!";
            }
            message += restartSuccess;
            message += messageFooter;
            client.Send(from, to, subject, message);
        }
    }
}
