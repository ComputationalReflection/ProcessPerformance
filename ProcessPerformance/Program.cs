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
            var processNames = new string[] { };
            var npr = PerformanceReporter.Create(processNames);
            while (true)
            {
                Task.Delay(500).Wait();
                var result = npr.GetPerformanceData();
                
                Console.WriteLine($"{String.Join('+',processNames)} ({result.Threads} ths) = CPU: { result.ProcessCPUUsage.ToString("0.00")} % | Memory: {result.ProcessMemoryUsage.ToString("N0")} MB | " +
                    $"Process: Sent {result.ProcessSentData.ToString("N0")} KB ({result.ProcessUploadSpeed.ToString("N0")} kbps) - Received {result.ProcessReceivedData.ToString("N0")} KB ({result.ProcessDownloadSpeed.ToString("N0")} kbps) | " +
                    $"Network: Sent {result.NetworkSentData.ToString("N0")} KB ({result.NetworkUploadSpeed.ToString("N0")} kbps) - Received {result.NetworkReceivedData.ToString("N0")} KB ({result.NetworkDownloadSpeed.ToString("N0")} kbps)");                
            }
        }
    }
}
