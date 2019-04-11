using System;
using System.Threading;

namespace ServiceMonitor
{
    public class MonitorTimer
    {
        public static Timer CreateTimer(Service currentService)
        {
            TimerCallback timerCallback = new TimerCallback(MonitorTimerCallback);

            
            if(currentService.AutoRestartTime == "0")
            {
                return null; //disable timer
            }
            string[] splitTime = currentService.AutoRestartTime.Split(':');
            int hours = int.Parse(splitTime[0]);
            int minutes = int.Parse(splitTime[1]);

            DateTime triggerTime = DateTime.Today.AddDays(0).AddHours(hours).AddMinutes(minutes);
            TimeSpan timeDue = triggerTime.Subtract(DateTime.Now);

            Messages.LogMessage($"Automatic restart timer scheduled for {triggerTime.ToShortDateString()} at {triggerTime.ToShortTimeString()}");
            Messages.LogMessage($"{timeDue:%h} Hours {timeDue:%m} Minutes {timeDue:%s} Seconds from now.");
            TimeSpan dailyEvent;
            switch (currentService.AutoRestartFrequency)
            {
                case Service.Frequency.Hourly:
                    dailyEvent = new TimeSpan(0, 1, 0, 0);
                    break;
                case Service.Frequency.Daily:
                    dailyEvent = new TimeSpan(1, 0, 0, 0);
                    break;
                case Service.Frequency.Weekly:
                    dailyEvent = new TimeSpan(7, 0, 0, 0);
                    break;
                case Service.Frequency.Monthly:
                    dailyEvent = new TimeSpan(30, 0, 0, 0);
                    break;
                case Service.Frequency.Never:
                    //Intentional fallthrough to default
                default:
                    return null; //No automatic restarts
            }

            return new Timer(timerCallback, currentService, timeDue, dailyEvent);
        }
        static void MonitorTimerCallback(object timedRestartService)
        {
            Service serviceToRestart = (Service)timedRestartService;
            Messages.LogMessage($"Timer fired for {serviceToRestart.Name} at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}",System.Diagnostics.EventLogEntryType.Information,EventID.TimerRestart);
            ServiceControl.RestartService(serviceToRestart.serviceController, false);
        }
    }
}
