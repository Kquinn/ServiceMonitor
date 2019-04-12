using System;
using System.Diagnostics;

namespace ServiceMonitor
{
    public static class Messages
    {
        public static void LogMessage(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
        }

        public static void LogMessage(string message, EventLogEntryType logEntryType, EventID eventID)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
            else
            {
                EventLog.WriteEntry("ServiceMonitor", message, logEntryType, (int)eventID);
            }
        }
    }
}
