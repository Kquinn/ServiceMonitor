using System.ServiceProcess;

namespace ServiceMonitor
{    
    public class Service
    {
        public enum Frequency { Never,Hourly,Daily,Weekly,Monthly } 
        public string Name { get; set; }
        public ServiceController serviceController { get; set; }
        public string Process { get; set; }
        public int MemoryLimitMB { get; set; } = 0;
        public string AutoRestartTime { get; set; } = "0";
        public Frequency AutoRestartFrequency { get; set; } = Frequency.Never;
        public bool ClearMSMQ { get; set; }
    }
    
}
