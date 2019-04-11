using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace ServiceMonitor
{
    public class ServiceControl
    {
        public static void StopService(ServiceController service)
        {
            var timeout = new TimeSpan(0, 0, 30);
            Messages.LogMessage($"Stopping {service.ServiceName} service. Current status is {service.Status}");
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    throw new Exception($"Timed out waiting for {service.ServiceName} to stop.");
                }
            }
            Messages.LogMessage($"{service.ServiceName} service successfully stopped");
        }
        public static void StartService(ServiceController service)
        {
            var timeout = new TimeSpan(0, 0, 30);
            Messages.LogMessage($"Starting {service.ServiceName} service. Current status is {service.Status}");
            if (service.Status != ServiceControllerStatus.Running)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                if (service.Status != ServiceControllerStatus.Running)
                {
                    throw new Exception($"Timed out waiting for {service.ServiceName} to start.");
                }
            }
            Messages.LogMessage($"{service.ServiceName} service successfully started");
        }

        public static bool RestartService(ServiceController serviceController, bool memoryThresholdReached, bool clearMSMQ = false)
        {

            List<ServiceController> dependentServicesList = serviceController.DependentServices.ToList();
            foreach (ServiceController currentDependentService in dependentServicesList)
            {
                StopService(currentDependentService);
            }

            StopService(serviceController);
            if (clearMSMQ)
            {
                int filecount = 0;
                ServiceController msmqServiceController = new ServiceController("MSMQ");
                StopService(msmqServiceController);
                try
                {

                    Messages.LogMessage($"Cleaning MSMQ files...");
                    string mqFileDirectory = "C:\\Windows\\SysNative\\msmq\\storage\\";
                    string mqBackupDirectory = "C:\\msmq_backupMQ";

                    try
                    {
                        if (!Directory.Exists(mqBackupDirectory))
                        {
                            Directory.CreateDirectory(mqBackupDirectory);
                        }

                        string[] files = Directory.GetFiles(mqFileDirectory, "*.mq");

                        foreach (string file in files)
                        {
                            var filename = file.Split('\\').Last();
                            File.Move(file, mqBackupDirectory + '\\' + filename);
                            filecount++;
                        }
                    }
                    catch (Exception ex1)
                    {
                        Messages.LogMessage(ex1.Message, EventLogEntryType.Error, EventID.Generic);
                    }

                }
                catch (Exception ex)
                {

                    Messages.LogMessage(ex.Message, EventLogEntryType.Error, EventID.Generic);
                    return false;
                    //log exception and continue to ensure services start back up.
                }
                Thread.Sleep(500);
                StartService(msmqServiceController);
            }

            StartService(serviceController);
            return true;


        }
    }
}
