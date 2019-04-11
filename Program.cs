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
        protected override void OnStart(string[] args)
        {
            runApplication();
        }

        private static void runApplication(bool console = false)
        {


            List<Timer> timerList = new List<Timer>(); //We store these in a list so GC does not eat them

            ConfigurationModel configuration = new ConfigurationModel {
                    ServiceList = AppConfiguration.GetServiceListConfiguration(),
                    SMTPConfig = AppConfiguration.GetSMTPConfiguration() };

            foreach (Service currentService in configuration.ServiceList)
            {
                timerList.Add(MonitorTimer.CreateTimer(currentService, configuration.SMTPConfig));
                Task callTask = Task.Run(() => MonitorService.Monitor(currentService, configuration.SMTPConfig));
            }

            if (console)
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
                            runApplication(true);
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
                runApplication(true);
            }
            else
            {
                Run(new Program());
            }
        }
    }
}
