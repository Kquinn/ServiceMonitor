using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Xml;

namespace ServiceMonitor
{
    public static class AppConfiguration
    {
        public static List<Service> GetServiceListConfiguration()
        {
            List<Service> output = new List<Service>();
            XmlNode serviceMonitorConfig = GetConfigXML();


            foreach (XmlNode service in serviceMonitorConfig["Services"].SelectNodes("Service"))
            {
                try
                {
                    if (service.Attributes["Name"] != null)
                    {
                        Service currentService = new Service { Name = service.Attributes["Name"].Value };

                        currentService.serviceController = new ServiceController(currentService.Name);
                        try
                        {
                            currentService.serviceController.Status.ToString();//This will throw an exception if the service does not exist
                        }
                        catch (InvalidOperationException ex)
                        {
                            if (ex.InnerException.Message == "The specified service does not exist as an installed service")
                            {
                                Messages.LogMessage($"The specified service {currentService.Name} does not exist as an installed service", EventLogEntryType.Warning, EventID.ServiceDoesNotExist);
                                break;
                            }
                            else
                            {
                                throw ex;
                            }
                        }

                        Messages.LogMessage($"Found service {currentService.Name}, retrieving configuration");

                        if (service["MemoryLimitMB"] != null)
                        {
                            if (int.TryParse(service["MemoryLimitMB"].FirstChild.Value, out int memoryLimitMB))
                            {
                                currentService.MemoryLimitMB = memoryLimitMB;
                            }
                        }
                        if (service["AutoRestartTime"] != null)
                        {
                            currentService.AutoRestartTime = service["AutoRestartTime"].FirstChild.Value;
                        }
                        if (service["AutoRestartFrequency"] != null)
                        {
                            switch (service["AutoRestartFrequency"].FirstChild.Value)
                            {
                                case "Hourly":
                                    currentService.AutoRestartFrequency = Service.Frequency.Hourly;
                                    break;
                                case "Daily":
                                    currentService.AutoRestartFrequency = Service.Frequency.Daily;
                                    break;
                                case "Weekly":
                                    currentService.AutoRestartFrequency = Service.Frequency.Weekly;
                                    break;
                                case "Monthly":
                                    currentService.AutoRestartFrequency = Service.Frequency.Monthly;
                                    break;
                                default:
                                    currentService.AutoRestartFrequency = Service.Frequency.Never;
                                    break;
                            }
                        }
                        if (service["ClearMSMQ"] != null)
                        {
                            currentService.ClearMSMQ = bool.TryParse(service["ClearMSMQ"].FirstChild.Value, out bool result);
                        }

                        currentService.Process = GetServicePath(currentService.Name);
                        if (currentService.Process == null)
                        {
                            throw new Exception($"Unable to locate path to executable for service {currentService.Name}");
                        }
                        output.Add(currentService);
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogMessage(ex.Message, EventLogEntryType.Error, EventID.Generic);
                }
            }
            return output;
        }

        private static XmlNode GetConfigXML()
        {
            try
            {
                XmlDocument config = new XmlDocument();
                string applicationName = Process.GetCurrentProcess().ProcessName;
                Messages.LogMessage($"Loading configuration file.");
                config.Load($"{applicationName}.config");
                XmlNode serviceMonitorConfig = config["ServiceMonitor"];
                return serviceMonitorConfig;
            }
            catch (Exception ex)
            {
                Messages.LogMessage(ex.Message, EventLogEntryType.Error, EventID.FailedToLoadConfiguration);
                throw ex;
            }
        }


        private static string GetServicePath(string serviceName)
        {
            WqlObjectQuery wqlObjectQuery = new WqlObjectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName));
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                string[] exeSeparator = { ".exe" };
                return managementObject.GetPropertyValue("PathName").ToString().Split('\\').Last().Split(exeSeparator, StringSplitOptions.RemoveEmptyEntries).First();
            }

            return null;
        }

        public static SMTPConfig GetSMTPConfiguration()
        {
            XmlNode serviceMonitorConfig = GetConfigXML();
            SMTPConfig output = new SMTPConfig();
            foreach (XmlNode smtpConfig in serviceMonitorConfig.SelectNodes("SMTP"))
            {
                try
                {
                    if (smtpConfig["Server"] != null)
                    {
                        output.Server = smtpConfig["Server"].FirstChild.Value;
                    }
                    if (smtpConfig["ToEmail"] != null)
                    {
                        output.ToEmail = smtpConfig["ToEmail"].FirstChild.Value;
                    }
                    if (smtpConfig["FromEmail"] != null)
                    {
                        output.FromEmail = smtpConfig["FromEmail"].FirstChild.Value;
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogMessage(ex.Message, EventLogEntryType.Error, EventID.SMTPConfigurationFailed);
                }
            }
            return output;
        }
        
    }
}