using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceMonitor
{

    public class Program : ServiceBase
    {
        private static List<Service> serviceList = new List<Service>();

        protected override void OnStart(string[] args)
        {
            runApplication();
        }

        static void consoleApp(string[] args)
        {

            runApplication(true);

        }

        private static void runApplication(bool console = false)
        {
            List<Timer> timerList = new List<Timer>(); //We store these in a list so GC does not eat them
            List<Service> serviceList = AppConfiguration.GetConfiguration();
            foreach (Service currentService in serviceList)
            {
                timerList.Add(MonitorTimer.CreateTimer(currentService));
                Task callTask = Task.Run(() => MonitorService.Monitor(currentService));
            }
            if(console)
            {
                while (true)
                {
                    Console.WriteLine("Running!");
                    Console.ReadLine();
                }
            }
        }
        
        static void Main(string[] args)
        {
            if (Environment.UserInteractive && args.Length > 0)
            {
                try
                {
                    switch (args[0])
                    {
                        case "-install":
                            {
                                Messages.LogMessage("Installing Service...");
                                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                        case "-uninstall":
                            {
                                Messages.LogMessage("Uninstalling Service...");
                                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                        default:
                            consoleApp(args);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogMessage(ex.Message, EventLogEntryType.Information, EventID.ServiceInstallationFailed);
                }

            }
            else if (Environment.UserInteractive && args.Length == 0)
            {
                consoleApp(args);
            }
            else
            {
                Run(new Program());
            }
        }
    }
}
