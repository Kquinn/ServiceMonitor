using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceMonitor
{
    class Program
    {



        static void Main(string[] args)
        {
            string processName = "***REMOVED***ReportingService";
            string userInput = "";
            Task callTask = Task.Run(() => MonitorService(processName,1,1500));
            while (userInput != "exit" && userInput != "quit")
            {
                userInput = Console.ReadLine();
            }
        }





        static async Task MonitorProcess(string processName,double delay)
        {
            await Task.Delay(10).ConfigureAwait(false);
            Console.WriteLine($"Begin monitoring process {processName}...");
            var processArray = Process.GetProcessesByName(processName);
            while(true)
            {
                processArray = Process.GetProcessesByName(processName);
                if (processArray == null || processArray.Length==0)
                {
                    Console.WriteLine($"Unable to acquire a process ID for {processName} trying again in 10 seconds.");
                }
                else if (processArray.Length > 1)
                {
                    Console.WriteLine($"Acquired more than one process ID for {processName} trying again in 10 seconds.");
                    Thread.Sleep(10 * 1000);
                }
                else
                {
                    var process = processArray[0];
                    double memsize = 0; // memsize in Megabyte
                    double previousmemsize = 0;
                    PerformanceCounter PC = new PerformanceCounter();
                    PC.CategoryName = "Process";
                    PC.CounterName = "Working Set - Private";
                    PC.InstanceName = process.ProcessName;
                    while (!process.HasExited)
                    {
                        
                        memsize = Math.Round(PC.NextValue() / 1024.0 / 1024.0, 2);
                        if(previousmemsize != memsize || previousmemsize == 0)
                        {
                            Console.WriteLine($"Process {process.ProcessName} with ID {process.Id} is consuming {memsize}MB of memory.");
                            Thread.Sleep((int)(delay * 1000));
                            previousmemsize = memsize;
                        }
                        
                    }
                    Console.WriteLine($"Process {process.ProcessName} has exited, monitoring interrupted.");
                    PC.Close();
                    PC.Dispose();
                }
                Thread.Sleep(10 * 1000);
            }

        }





        static async Task MonitorService(string serviceName, double delay, int memoryThreshold)
        {
            await Task.Delay(10).ConfigureAwait(false);
            Console.WriteLine($"Begin monitoring service {serviceName}...");
            ServiceController serviceController;
            try
            {
                serviceController = new ServiceController(serviceName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
            

            var processArray = Process.GetProcessesByName(serviceName);
            while (true)
            {
                processArray = Process.GetProcessesByName(serviceName);
                if (processArray == null || processArray.Length == 0)
                {
                    Console.WriteLine($"Unable to acquire a process ID for {serviceName} trying again in 10 seconds.");
                }
                else if (processArray.Length > 1)
                {
                    Console.WriteLine($"Acquired more than one process ID for {serviceName} trying again in 10 seconds.");
                }
                else
                {
                    var process = processArray[0];
                    Console.WriteLine($"Acquired process {serviceName} with ID {process.Id}. Creating performance couter...");
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
                            Console.WriteLine($"Process {process.ProcessName} with ID {process.Id} is consuming {memsize}MB of memory.");
                            Thread.Sleep((int)(delay * 1000));
                            previousmemsize = memsize;
                        }
                        if (memsize > memoryThreshold)
                        {
                            Console.WriteLine($"Service exceeded memory threshhold, exiting monitoring loop and restarting service.");
                            SendNotification(serviceController.ServiceName, memsize, memoryThreshold);
                            RestartService(serviceController);
                            return;//TODO: DEBUG switch back to break after debugging
                            //break;
                        }
                    }
                    Console.WriteLine($"Process {process.ProcessName} has exited, monitoring interrupted.");
                    PC.Close();
                    PC.Dispose();
                }
                Thread.Sleep(10 * 1000);
            }

        }

        private static void SendNotification(string serviceName, double memorySize, int memoryThreshold)
        {
            SmtpClient client = new SmtpClient("smtp4apps.pylusd.org");
            string from = "ServiceMonitor@pylusd.org";
            string to = "noc@pylusd.org";
            string subject = $"Service {serviceName} - Memory Threshold Exceeded";
            string message = $"Service:{serviceName}\nMemory Threshold:{memoryThreshold}MB\nCurrent Memory Usage:{memorySize}MB";
            string messageFooter = 
                "\n\n\n" +
                "-------------------------------------" +
                $"\nSent from: {Environment.MachineName}" +
                $"\nDate Time:{DateTime.Now}"+
                "\n-------------------------------------";
            message += messageFooter;
            client.Send(from, to, subject, message);
        }


        private static void RestartService(ServiceController serviceController)
        {

            if(serviceController.ServiceName == "***REMOVED***NetService")
            {
                ServiceController msmqServiceController = new ServiceController("MSMQ");
                //stop ***REMOVED***
                serviceController.Stop();
                Console.WriteLine($"Stopping {serviceController.ServiceName} service...");
                while(serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    //stop MSMQ
                    msmqServiceController.Stop();
                    Console.WriteLine($"Stopping {msmqServiceController.ServiceName} service...");
                    while (msmqServiceController.Status != ServiceControllerStatus.Stopped)
                    {
                        //TODO: cleanup MQ files
                        Console.WriteLine($"Cleaning MSMQ files...");
                        Thread.Sleep(500);
                        //start MSMQ
                        while (msmqServiceController.Status != ServiceControllerStatus.Running)
                        {
                            Console.WriteLine($"Starting {msmqServiceController.ServiceName} service...");
                            msmqServiceController.Start();
                        }
                    }
                    //start ***REMOVED***
                    while (serviceController.Status != ServiceControllerStatus.Running)
                    {
                        Console.WriteLine($"Starting {serviceController.ServiceName} service...");
                        serviceController.Start();
                    }
                        
                }
                
                
                
                
            }

        }

    }
}
