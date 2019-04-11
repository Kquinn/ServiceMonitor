# Future Development
This is a list of features that will eventually be built in to ServiceMonitor
    * Refine ServiceMonitor process
    * Implement ProcessMonitor in configuration file
    * Inlude SMTP in configuration file

# Purpose
The goal of ServiceMonitor is to automatically monitor, report, and restart on a service with a memory leak. In an ideal setting this software would be unnecessary, but we often operate in less than ideal conditions. Using this software means that the service(s) you are monitoring (Target Service) may be restarted and some processing information may be lost as a result.
 
# Configuration
ServiceMonitor is configured using an XML file named ServiceMonitor.config in the same directory as the application. 

The example service below is called “Monitor This Service” in Windows, will restart the service daily at 4:30am, will restart the service the process uses more than 5000MB of memory, and since the ClearMSMQ flag is set, will clear out the Messaging Queue.

## Configuration Settings

### Service.Name
Function: This setting identifies the name of the service to be monitored (Target Service), this is listed as “Service Name” in the Services mmc.
Valid Values: The name of any currently installed services. Note: An invalid name will log to the event log, but will not prevent this software from running. Services are not plug-and-play, ServiceMonitor will need to be restarted if the configuration changes or a new service is added.

### MemoryLimitMB
Function: Sets the maximum allowable memory a service can use before ServiceMonitor restarts the process. This limit also triggers ClearMSMQ. Setting value to zero disables this feature.
Valid Values: Any 32 bit integer

### AutoRestartTime
Function: Sets the time of day to restart using a 24 hour clock. Target Service will be restarted at the set time based on the date the service is started and the restart frequency.
Valid Values: Any valid time of day 00:00 – 23:59

### AutoRestartFrequency
Function: Indicates how often a service is to be restarted. Set to Never to disable this feature.
Valid Values: Never, Hourly, Daily, Weekly, Monthly

### ClearMSMQ
Function: When TRUE and targeted service uses more memory than the threshold allows, this process stops the MSMQ, clears out queue (.mq) files by moving them to C:\msmq_backupMQ\*, then restarts MSMQ and the targeted service.
Valid Values: TRUE, FALSE
WARNING: This will result in messages not making it to their destination and any service which relies on MSMQ being shut down during the process.

 
# Service Installation, Uninstallation, and Console Mode
## Installation
To install ServiceMonitor as a Service, set up the configuration file as directed in this document, copy both the .exe and .config to the C:\windows\system32\ folder and use the following command:
    C:\windows\system32\ServiceMonitor.exe - install	
## Uninstallation
Issue the folliwng command:
    C:\windows\system32\ServiceMonitor.exe - uninstall
## Console Mode
ServiceMonitor may be run in the console by specifying no arguments at runtime. This can be used for initial setup, troubleshooting, and debugging.

 
# Logging
ServiceMonitor logs to the console when run in the console environment and logs to the Event Log when run as a service. This is intended to allow for testing and debugging from the console without adding extraneous information to the Event Log.

# Event IDs
1000 - FailedToLoadConfiguration
1001 - ServiceDoesNotExist
1002 - MemoryThresholdExceeded
1003 - ServiceInstallationFailed
1004 - AcquiredProcessID
1005 - TimerRestart
9999 - Generic
