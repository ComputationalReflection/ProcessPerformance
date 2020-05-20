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
    public sealed class PerformanceReporter : IDisposable
    {
        private DateTime _etwStartTime;
        private TraceEventSession _etwSession;
        private Func<HashSet<int>> _processList;

        private readonly Counters _counters = new Counters();

        private class Counters
        {
            //Process network            
            public long received;
            public long totalReceived;
            public long sent;
            public long totalSent;

            //Process CPU
            public DateTime cpuLastTime;
            public TimeSpan cpuLastTotalProcessorTime;
            public DateTime cpuCurrentTime;
            public TimeSpan cpuCurrentTotalProcessorTime;
            public double processorTime;

            //Process Memory
            public long physicalMemory;

            //Network
            public DateTime networkLastTime;
            public DateTime networkCurrentTime;
            public long networkLastBytesSend;
            public long networkLastBytesReceived;
            public long networkUploadSpeed;
            public long networkDownloadSpeed;
        }

        private PerformanceReporter(String[] processNames)
        {
            _processList = () =>
            {
                return processNames.Aggregate(new List<int>(), (partial, processName) =>
                {
                    partial.AddRange(Process.GetProcessesByName(processName).Select(p => p.Id)); return partial;
                }).ToHashSet();
            };
                

        }

        public static PerformanceReporter Create(String[] processNames)
        {
            var performancePresenter = new PerformanceReporter(processNames);
            performancePresenter.Initialise();
            return performancePresenter;
        }

        private void Initialise()
        {
            Task.Run(() => StartEtwSession());
        }

        private void StartEtwSession()
        {
            try
            {
                ResetCounters();

                using (_etwSession = new TraceEventSession("MyKernelAndClrEventsSession"))
                {
                    _etwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All);
                    _etwSession.Source.Kernel.TcpIpRecv += data =>
                    {
                        if (_processList().Contains(data.ProcessID))
                        {
                            Interlocked.Add(ref _counters.received, data.size * 8);
                            Interlocked.Add(ref _counters.totalReceived, data.size);
                        }
                    };

                    _etwSession.Source.Kernel.TcpIpSend += data =>
                    {
                        if (_processList().Contains(data.ProcessID))
                        {
                            Interlocked.Add(ref _counters.sent, data.size * 8);
                            Interlocked.Add(ref _counters.totalSent, data.size);
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
            if (!NetworkInterface.GetIsNetworkAvailable())
                return;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var statistics = NetworkInterface.GetAllNetworkInterfaces()[0].GetIPv4Statistics();

            if (_counters.networkLastTime == null || _counters.networkLastTime == new DateTime())
            {
                _counters.networkLastTime = DateTime.Now;
                _counters.networkLastBytesReceived = statistics.BytesReceived;
                _counters.networkLastBytesSend = statistics.BytesSent;
                _counters.networkUploadSpeed = 0L;
                _counters.networkDownloadSpeed = 0L;
            }
            else
            {
                _counters.networkCurrentTime = DateTime.Now;
                long networkCurrentBytesReceived = statistics.BytesReceived;
                long networkCurrentBytesSent = statistics.BytesSent;
                _counters.networkDownloadSpeed = (networkCurrentBytesReceived - _counters.networkLastBytesReceived) * 8 / 1000;
                _counters.networkUploadSpeed = (networkCurrentBytesSent - _counters.networkLastBytesSend) * 8 / 1000;
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
                _counters.processorTime = 0;
            }
            else
            {
                _counters.cpuCurrentTime = DateTime.Now;
                _counters.cpuCurrentTotalProcessorTime = totalProcessorTime;


                double CPUUsage = (_counters.cpuCurrentTotalProcessorTime.TotalMilliseconds - _counters.cpuLastTotalProcessorTime.TotalMilliseconds)
                    / _counters.cpuCurrentTime.Subtract(_counters.cpuLastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);

                _counters.cpuLastTime = _counters.cpuCurrentTime;
                _counters.cpuLastTotalProcessorTime = _counters.cpuCurrentTotalProcessorTime;

                _counters.processorTime = CPUUsage < 0 ? 0 : CPUUsage * 100;
            }
        }


        public PerformanceData GetPerformanceData()
        {
            var timeDifferenceInSeconds = (DateTime.Now - _etwStartTime).TotalSeconds;

            PerformanceData performanceData;

            Parallel.Invoke(
                () => GetMemoryData(),
                () => GetCPUData(),
                () => GetNetworkData()
            );

            lock (_counters)
            {
                performanceData = new PerformanceData
                {
                    Threads = _processList().Count,
                    ProcessDownloadSpeed = Convert.ToInt64((_counters.received / 1000) / timeDifferenceInSeconds),
                    ProcessUploadSpeed = Convert.ToInt64((_counters.sent / 1000) / timeDifferenceInSeconds),
                    ProcessReceivedData = _counters.totalReceived / 1024,
                    ProcessSentData = _counters.totalSent / 1024,
                    ProcessMemoryUsage = _counters.physicalMemory,
                    ProcessCPUUsage = _counters.processorTime,
                    NetworkDownloadSpeed = _counters.networkDownloadSpeed,
                    NetworkUploadSpeed = _counters.networkUploadSpeed
                };
            }

            ResetCounters();

            return performanceData;
        }

        private void ResetCounters()
        {
            lock (_counters)
            {
                _counters.sent = 0;
                _counters.received = 0;
                _counters.networkUploadSpeed = 0;
                _counters.networkDownloadSpeed = 0;
                _counters.physicalMemory = 0;
                _counters.processorTime = 0;
            }
            _etwStartTime = DateTime.Now;
        }

        public void Dispose()
        {
            _etwSession?.Dispose();
        }
    }
}
