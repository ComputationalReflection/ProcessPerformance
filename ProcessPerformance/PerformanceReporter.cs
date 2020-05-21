using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessPerformance
{

    //////////////////////////////////////////////////////////////////////////////
    // -------------------------------------------------------------------------//
    // Project ProcessPerformance                                               //
    // Computational Reflection Research Group, University of Oviedo            //
    // -------------------------------------------------------------------------//
    // File: PerformanceReporter.cs                                             //
    // Author: garciaRmiguel@uniovi.es                                          //
    // Description:                                                             //
    //    Implements the performance reporter that collects information about   //
    //    the CPU, memory and network usage of one or more processes.           //
    // -------------------------------------------------------------------------//
    //////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Implements the performance reporter.
    /// </summary>    
    public sealed class PerformanceReporter : IDisposable
    {
        private DateTime _etwStartTime;
        private TraceEventSession _etwSession;
        private Func<HashSet<int>> _processList;
        private NetworkInterface _network;
        private readonly Counters _counters = new Counters();

        private class Counters
        {
            //Process Network            
            public long processDownloadSpeed;
            public long processTotalBytesReceived;
            public long processUploadSpeed;
            public long processTotalBytesSent;

            //Process CPU
            public DateTime cpuLastTime;
            public TimeSpan cpuLastTotalProcessorTime;
            public DateTime cpuCurrentTime;
            public TimeSpan cpuCurrentTotalProcessorTime;
            public double cpuProcessorTime;

            //Process Memory
            public long physicalMemory;

            //System Network
            public DateTime networkLastTime;
            public DateTime networkCurrentTime;
            public long networkLastBytesSend;
            public long networkLastBytesReceived;
            public long networkTotalBytesSend;
            public long networkTotalBytesReceived;
            public long networkUploadSpeed;
            public long networkDownloadSpeed;
        }

        public PerformanceReporter(String[] processNames, string networkIP = null)
        {
            if (processNames.Length == 0)
            {
                _processList = () => { return Process.GetProcesses().Select(p => p.Id).ToHashSet(); };
            }
            else
            {
                _processList = () =>
                {
                    return processNames.Aggregate(new List<int>(), (partial, processName) =>
                    {
                        partial.AddRange(Process.GetProcessesByName(processName).Select(p => p.Id)); return partial;
                    }).ToHashSet();
                };
            }

            if(! String.IsNullOrEmpty(networkIP) && NetworkInterface.GetIsNetworkAvailable())
            {                
                _network = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.GetIPProperties().UnicastAddresses.Any( a => a.Address.ToString().Equals(networkIP)));
            }

            Task.Run(() => StartEtwSession());
        }

        
        private void StartEtwSession()
        {
            try
            {
                ResetCounters();

                using (_etwSession = new TraceEventSession("KernelTcpIpEventsSession"))
                {
                    _etwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All);
                    _etwSession.Source.Kernel.TcpIpRecv += data =>
                    {
                        if (_processList().Contains(data.ProcessID))
                        {
                            Interlocked.Add(ref _counters.processDownloadSpeed, data.size * 8);
                            Interlocked.Add(ref _counters.processTotalBytesReceived, data.size);
                        }
                    };

                    _etwSession.Source.Kernel.TcpIpSend += data =>
                    {
                        if (_processList().Contains(data.ProcessID))
                        {
                            Interlocked.Add(ref _counters.processUploadSpeed, data.size * 8);
                            Interlocked.Add(ref _counters.processTotalBytesSent, data.size);
                        }
                    };
                    _etwSession.Source.Process();
                }
            }
            catch
            {
                ResetCounters();
            }
        }

        public void GetMemoryData()
        {
            long physicalMemory = 0;
            foreach (var processId in _processList())
            {
                try
                {
                    var p = Process.GetProcessById(processId);
                    if (p != null)
                        physicalMemory += p.PeakWorkingSet64;
                }
                catch { }
            }

            _counters.physicalMemory = physicalMemory / 1024 / 1024;
        }

        public void GetNetworkData()
        {
            if (_network == null)
                return;

            var statistics = _network.GetIPv4Statistics();

            if (_counters.networkLastTime == null || _counters.networkLastTime == new DateTime())
            {
                _counters.networkLastTime = DateTime.Now;
                _counters.networkLastBytesReceived = statistics.BytesReceived;
                _counters.networkLastBytesSend = statistics.BytesSent;
                _counters.networkUploadSpeed = 0L;
                _counters.networkDownloadSpeed = 0L;
                _counters.networkTotalBytesReceived = 0L;
                _counters.networkTotalBytesSend = 0L;
            }
            else
            {
                _counters.networkCurrentTime = DateTime.Now;
                var timeDifferenceInSeconds = (_counters.networkCurrentTime - _counters.networkLastTime).TotalSeconds;
                long networkCurrentBytesReceived = statistics.BytesReceived;
                long networkCurrentBytesSent = statistics.BytesSent;
                _counters.networkDownloadSpeed = Convert.ToInt64((networkCurrentBytesReceived - _counters.networkLastBytesReceived) * 8 / 1000 / timeDifferenceInSeconds);
                _counters.networkUploadSpeed = Convert.ToInt64((networkCurrentBytesSent - _counters.networkLastBytesSend) * 8 / 1000 / timeDifferenceInSeconds);
                _counters.networkTotalBytesReceived += (networkCurrentBytesReceived - _counters.networkLastBytesReceived);
                _counters.networkTotalBytesSend += (networkCurrentBytesSent - _counters.networkLastBytesSend);
                _counters.networkLastTime = _counters.networkCurrentTime;
                _counters.networkLastBytesReceived = networkCurrentBytesReceived;
                _counters.networkLastBytesSend = networkCurrentBytesSent;                
            }
        }

        public void GetCPUData()
        {
            TimeSpan totalProcessorTime = new TimeSpan();
            foreach (var processId in _processList())
            {
                try
                {
                    var p = Process.GetProcessById(processId);
                    if (p != null)
                        totalProcessorTime += p.TotalProcessorTime;
                }
                catch { }
            }

            if (_counters.cpuLastTime == null || _counters.cpuLastTime == new DateTime())
            {
                _counters.cpuLastTime = DateTime.Now;
                _counters.cpuLastTotalProcessorTime = totalProcessorTime;
                _counters.cpuProcessorTime = 0;
            }
            else
            {
                _counters.cpuCurrentTime = DateTime.Now;
                _counters.cpuCurrentTotalProcessorTime = totalProcessorTime;


                double CPUUsage = (_counters.cpuCurrentTotalProcessorTime.TotalMilliseconds - _counters.cpuLastTotalProcessorTime.TotalMilliseconds)
                    / _counters.cpuCurrentTime.Subtract(_counters.cpuLastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);

                _counters.cpuLastTime = _counters.cpuCurrentTime;
                _counters.cpuLastTotalProcessorTime = _counters.cpuCurrentTotalProcessorTime;

                _counters.cpuProcessorTime = CPUUsage < 0 ? 0 : CPUUsage * 100;
            }
        }


        public ReportData GetPerformanceData()
        {
            var timeDifferenceInSeconds = (DateTime.Now - _etwStartTime).TotalSeconds;

            ReportData performanceData;

            Parallel.Invoke(
                () => GetMemoryData(),
                () => GetCPUData(),
                () => GetNetworkData()
            );

            lock (_counters)
            {
                performanceData = new ReportData
                {
                    Threads = _processList().Count,
                    ProcessDownloadSpeed = Convert.ToInt64((_counters.processDownloadSpeed / 1000) / timeDifferenceInSeconds),
                    ProcessUploadSpeed = Convert.ToInt64((_counters.processUploadSpeed / 1000) / timeDifferenceInSeconds),
                    ProcessReceivedData = _counters.processTotalBytesReceived / 1024,
                    ProcessSentData = _counters.processTotalBytesSent / 1024,
                    ProcessMemoryUsage = _counters.physicalMemory,
                    ProcessCPUUsage = _counters.cpuProcessorTime,
                    NetworkDownloadSpeed = _counters.networkDownloadSpeed,
                    NetworkUploadSpeed = _counters.networkUploadSpeed,
                    NetworkReceivedData = _counters.networkTotalBytesReceived / 1024,
                    NetworkSentData = _counters.networkTotalBytesSend / 1024
                };
            }

            ResetCounters();

            return performanceData;
        }

        private void ResetCounters()
        {
            lock (_counters)
            {
                _counters.processUploadSpeed = 0;
                _counters.processDownloadSpeed = 0;
                _counters.networkUploadSpeed = 0;
                _counters.networkDownloadSpeed = 0;
                _counters.physicalMemory = 0;
                _counters.cpuProcessorTime = 0;
            }
            _etwStartTime = DateTime.Now;
        }

        public void Dispose()
        {
            _etwSession?.Dispose();
        }
    }
}
