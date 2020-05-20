using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessPerformance
{
    public class PerformanceData
    {
        public int Threads { get; set; }
        public long ProcessDownloadSpeed { get; set; }
        public long ProcessReceivedData { get; set; }
        public long ProcessUploadSpeed { get; set; }
        public long ProcessSentData { get; set; }
        public long ProcessMemoryUsage { get; set; }
        public double ProcessCPUUsage { get; set; }        
        public long NetworkDownloadSpeed { get; set; }
        public long NetworkReceivedData { get; set; }
        public long NetworkUploadSpeed { get; set; }
        public long NetworkSentData { get; set; }
    }
}
