using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            var processName = "chrome";
            var npr = PerformanceReporter.Create(processName);
            while (true)
            {
                Task.Delay(1000).Wait();
                var result = npr.GetPerformanceData();
                var threads = Process.GetProcessesByName(processName).Count();
                Console.WriteLine($"{processName} ({threads} ths) = CPU: { result.ProcessCPUUsage.ToString("0.00")} % | Memory: {result.ProcessMemoryUsage.ToString("N0")} MB | " +
                    $"Process: Sent {result.ProcessSentData.ToString("N0")} KB ({result.ProcessUploadSpeed.ToString("N0")} kbps) - Received {result.ProcessReceivedData.ToString("N0")} KB ({result.ProcessDownloadSpeed.ToString("N0")} kbps) | " +
                    $"Network: Upload {result.NetworkUploadSpeed.ToString("N0")} kbps - Download {result.NetworkDownloadSpeed.ToString("N0")} kbps");                
            }
        }
    }
}
