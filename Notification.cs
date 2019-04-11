using System;
using System.Net.Mail;

namespace ServiceMonitor
{
    public class Notification
    {
        public static void SendEmail(Service service, SMTPConfig smtpConfiguration, double memorySize, bool restartSuccess)
        {
            SmtpClient client = new SmtpClient(smtpConfiguration.Server);
            string from = smtpConfiguration.FromEmail;
            string to = smtpConfiguration.ToEmail;
            string subject = $"Service {service.Name} -";

            if (restartSuccess)
            {
                subject += "Memory Threshold Exceeded";
            }
            else
            {
                subject += "WARNING - Service failed to restart";
            }

            string message = $"Service:{service.Name}\nMemory Threshold:{service.MemoryLimitMB}MB\nCurrent Memory Usage:{memorySize}MB";

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
