using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessPerformance
{
    //////////////////////////////////////////////////////////////////////////////
    // -------------------------------------------------------------------------//
    // Project ProcessPerformance                                               //
    // Computational Reflection Research Group, University of Oviedo            //
    // -------------------------------------------------------------------------//
    // File: ReportData.cs                                                      //
    // Author: garciaRmiguel@uniovi.es                                          //
    // Description:                                                             //
    //    Encapsulates all the information collected in the report.             //
    // -------------------------------------------------------------------------//
    //////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Encapsulates all the information collected in the report.
    /// </summary>
    public class ReportData
    {
        /// <summary>
        /// Number of threads
        /// </summary>
        public int Threads { get; set; }

        /// <summary>
        /// Process Download Speed in kbps
        /// </summary>
        public long ProcessDownloadSpeed { get; set; }

        /// <summary>
        /// Total Kbytes received by the process 
        /// </summary>
        public long ProcessReceivedData { get; set; }

        /// <summary>
        /// Process Upload Speed in kbps
        /// </summary>
        public long ProcessUploadSpeed { get; set; }

        /// <summary>
        /// Total Kbytes sent by the process
        /// </summary>
        public long ProcessSentData { get; set; }

        /// <summary>
        /// Physical Memory in mbytes used by the process
        /// </summary>
        public long ProcessMemoryUsage { get; set; }

        /// <summary>
        /// % of CPU used by the process
        /// </summary>
        public double ProcessCPUUsage { get; set; }

        /// <summary>
        /// Network Download Speed in kbps
        /// </summary>
        public long NetworkDownloadSpeed { get; set; }

        /// <summary>
        /// Total Kbytes received by the network
        /// </summary>
        public long NetworkReceivedData { get; set; }

        /// <summary>
        /// Network Upload Speed in kbps
        /// </summary>
        public long NetworkUploadSpeed { get; set; }

        /// <summary>
        /// Total Kbytes sent by the network
        /// </summary>
        public long NetworkSentData { get; set; }
    }
}
