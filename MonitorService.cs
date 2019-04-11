using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceMonitor
{
    public class MonitorService
    {
        public static async Task Monitor(Service targetService,SMTPConfig smtpConfiguration)
        {
            await Task.Delay(10).ConfigureAwait(false);
            Messages.LogMessage($"Begin monitoring service for {targetService.Name}");


            var processArray = Process.GetProcessesByName(targetService.Process);
            while (true)
            {
                targetService.serviceController.Refresh();
                try
                {
                    Console.WriteLine($"Status:{targetService.serviceController.Status}");
                    if (targetService.serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        ServiceControl.StartService(targetService.serviceController);
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogMessage(ex.Message, EventLogEntryType.Error, EventID.Generic);
                    return;
                }

                processArray = Process.GetProcessesByName(targetService.Process);
                if (processArray == null || processArray.Length == 0)
                {
                    Messages.LogMessage($"Unable to acquire a process ID for {targetService.Name} trying again in 10 seconds.");
                }
                else if (processArray.Length > 1)
                {
                    Messages.LogMessage($"Acquired more than one process ID for {targetService.Name} trying again in 10 seconds.");
                }
                else
                {
                    var process = processArray[0];
                    Messages.LogMessage($"Acquired process {targetService.Name} with ID {process.Id}.", EventLogEntryType.Information, EventID.AcquiredProcessID);
                    Messages.LogMessage($"Creating performance couter...");
                    double memsize = 0; // memsize in Megabyte
                    double previousmemsize = 0;
                    PerformanceCounter PC = new PerformanceCounter();
                    PC.CategoryName = "Process";
                    PC.CounterName = "Working Set - Private";
                    PC.InstanceName = process.ProcessName;
                    while (!process.HasExited)
                    {

                        memsize = Math.Round(PC.NextValue() / 1024.0 / 1024.0, 2);
                        if (previousmemsize != memsize || previousmemsize == 0)
                        {
                            Messages.LogMessage($"Process {process.ProcessName} with ID {process.Id} is consuming {memsize}MB");
                            Thread.Sleep(5000);
                            previousmemsize = memsize;
                        }
                        if (memsize > targetService.MemoryLimitMB)
                        {
                            Messages.LogMessage($"Service exceeded memory threshhold, exiting monitoring loop and restarting service.", EventLogEntryType.Error, EventID.MemoryThresholdExceeded);

                            bool serviceRestartedSuccessfully = ServiceControl.RestartService(targetService.serviceController, true, targetService.ClearMSMQ);
                            Notification.SendEmail(targetService, smtpConfiguration, memsize, serviceRestartedSuccessfully);
                            break;
                        }
                    }
                    Messages.LogMessage($"Process {process.ProcessName} has exited, monitoring interrupted.");
                    PC.Close();
                    PC.Dispose();
                }
                Thread.Sleep(10000);
            }

        }
    }
}
