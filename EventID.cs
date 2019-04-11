namespace ServiceMonitor
{
    public enum EventID
    {
            FailedToLoadConfiguration = 1000,
            ServiceDoesNotExist = 1001,
            MemoryThresholdExceeded = 1002,
            ServiceInstallationFailed = 1003,
            AcquiredProcessID = 1004,
            TimerRestart = 1005,
            SMTPConfigurationFailed = 1006,
            Generic = 9999
    }
    
}
