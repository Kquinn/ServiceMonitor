using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceMonitor
{
    public class MonitorProcess
    {
        public static async Task Monitor(string processName, double delay)
        {
            await Task.Delay(10).ConfigureAwait(false);
            Console.WriteLine($"Begin monitoring process {processName}...");
            var processArray = Process.GetProcessesByName(processName);
            while (true)
            {
                processArray = Process.GetProcessesByName(processName);
                if (processArray == null || processArray.Length == 0)
                {
                    Console.WriteLine($"Unable to acquire a process ID for {processName} trying again in 5 seconds.");
                }
                else if (processArray.Length > 1)
                {
                    Console.WriteLine($"Acquired more than one process ID for {processName} trying again in 5 seconds.");
                    Thread.Sleep(10 * 1000);
                }
                else
                {
                    var process = processArray[0];
                    double memsize = 0;
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

                    }
                    Console.WriteLine($"Process {process.ProcessName} has exited, monitoring interrupted.");
                    PC.Close();
                    PC.Dispose();
                }
                Thread.Sleep(5 * 1000);
            }

        }
    }
}
