using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServiceMonitor
{
    [RunInstaller(true)]
    public class MyWindowsServiceInstaller : Installer
    {
        public const string InternalServiceName = "***REMOVED***ReportingMonitoringService";
        public MyWindowsServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = InternalServiceName;
            serviceInstaller.StartType = ServiceStartMode.Manual;

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = InternalServiceName;
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
