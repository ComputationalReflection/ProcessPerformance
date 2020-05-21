using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessPerformance
{
    //////////////////////////////////////////////////////////////////////////////
    // -------------------------------------------------------------------------//
    // Project ProcessPerformance                                               //
    // Computational Reflection Research Group, University of Oviedo            //
    // -------------------------------------------------------------------------//
    // File: Program.cs                                                         //
    // Author: garciaRmiguel@uniovi.es                                          //
    // Description:                                                             //
    //    Parses all the command line arguments and launchs the                 //
    //    Performance Reporter.                                                 //
    // -------------------------------------------------------------------------//
    //////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Parses all the command line arguments and launchs the Performance Reporter
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {            
            var parameters = ParseArguments(args);
            var reporter = new PerformanceReporter(parameters.ProcessNames, parameters.NetworkIP);
            if(parameters.CSV)
                Console.WriteLine($"Process Name(s);Thread(s);CPU (%);Memory (MB);Process Sent (KB);Process Upload Speed (kbps);Process Received (KB);Process Download Speed (kbps)" + (String.IsNullOrEmpty(parameters.NetworkIP)?"":";Network Sent (KB);Network Upload Speed (kbps);Network Received (KB);Network Download Speed (kbps)"));
            while (true)
            {
                Task.Delay(parameters.IntervalTime).Wait();
                var result = reporter.GetPerformanceData();
                if(parameters.CSV)
                    Console.WriteLine($"{String.Join('+', parameters.ProcessNames)};{result.Threads};{ result.ProcessCPUUsage.ToString("0.00")};{result.ProcessMemoryUsage.ToString("N0")};" +
                    $"{result.ProcessSentData.ToString("N0")};{result.ProcessUploadSpeed.ToString("N0")};{result.ProcessReceivedData.ToString("N0")};{result.ProcessDownloadSpeed.ToString("N0")}" +
                    (String.IsNullOrEmpty(parameters.NetworkIP) ? "" : $";{result.NetworkSentData.ToString("N0")};{result.NetworkUploadSpeed.ToString("N0")};{result.NetworkReceivedData.ToString("N0")};{result.NetworkDownloadSpeed.ToString("N0")}"));
                else
                    Console.WriteLine($"{String.Join('+', parameters.ProcessNames)} ({result.Threads} ths) = CPU: { result.ProcessCPUUsage.ToString("0.00")} % | Memory: {result.ProcessMemoryUsage.ToString("N0")} MB | " +
                    $"Process: Sent {result.ProcessSentData.ToString("N0")} KB ({result.ProcessUploadSpeed.ToString("N0")} kbps) - Received {result.ProcessReceivedData.ToString("N0")} KB ({result.ProcessDownloadSpeed.ToString("N0")} kbps)" +
                    (String.IsNullOrEmpty(parameters.NetworkIP) ? "" : $" | Network: Sent {result.NetworkSentData.ToString("N0")} KB ({result.NetworkUploadSpeed.ToString("N0")} kbps) - Received {result.NetworkReceivedData.ToString("N0")} KB ({result.NetworkDownloadSpeed.ToString("N0")} kbps)"));                
            }
        }

        private static Parameters ParseArguments(string[] args)
        {
            var processNames = new List<string>();
            var networkIP = "";
            var intervalTime = 1000;
            var csv = false;
            try
            {
                foreach (var arg in args)
                {
                    if (arg.ToLower().Equals("-help"))
                    {
                        Console.WriteLine(HELP_MESSAGE);
                        System.Environment.Exit(0);
                    }
                    else if (arg.ToLower().StartsWith("-network:"))
                        networkIP = arg.Split(':')[1];
                    else if (arg.ToLower().StartsWith("-csv"))
                        csv = true;
                    else if (arg.ToLower().StartsWith("-interval:"))
                        intervalTime = int.Parse(arg.Split(':')[1]);
                    else
                        processNames.Add(arg);
                }
                return new Parameters() { ProcessNames = processNames.ToArray(), NetworkIP = networkIP, IntervalTime = intervalTime, CSV = csv };
            }
            catch 
            {
                Console.Error.WriteLine("\nSome error in the input parameters.Type -help for help.\n");
                System.Environment.Exit(1);
                return new Parameters();
            }
        }

        public const string HELP_MESSAGE =  "ProcessPerformance 2020 Computational Reflection Research Group\n" +
                                "-help                              Displays this usage message.\n" +
                                "-network:'NETWORK_IP'              Specify the network interface IP (disable by default).\n" +                                
                                "-interval:MILISENCONS              Specify the interval time in milisecons (default is 1000).\n" +
                                "-csv                               Specify output format as CSV (disable by default).\n" +
                                "process_1 ... process_n            A list of process names (if empty, all running processes are used).\n" +
                                "\nCtrl + c                         Is the interrupt signal.\n" +
                                "\n";
    }
}
