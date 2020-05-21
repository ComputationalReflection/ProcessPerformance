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
    // File: Parameters.cs                                                      //
    // Author: garciaRmiguel@uniovi.es                                          //
    // Description:                                                             //
    //    Encapsulates all the parameter data.                                  //
    // -------------------------------------------------------------------------//
    //////////////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Encapsulates all the parameter data.
    /// </summary>    
    public struct Parameters
    {
        /// <summary>
        /// Input process names (default is empty, all running processes)
        /// </summary>
        public string[] ProcessNames; 

        /// <summary>
        /// Network IP (default is null)
        /// </summary>
        public string NetworkIP;

        /// <summary>
        /// Report Interval Time in milisecons (default is 1000)
        /// </summary>
        public int IntervalTime;

        /// <summary>
        /// CSV option (default is false)
        /// </summary>
        public bool CSV;
    }
}
