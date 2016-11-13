using System;

namespace h3xmonitor.Settings
{
    /// <summary>
    /// Types of disk controllers.
    /// </summary>
    [Flags]
    public enum DiskControllers
    {
        /// <summary>
        /// Standard ATA compatible controller.
        /// </summary>
        StandardATA = 1,

        /// <summary>
        /// LSI MegaRAID controller.
        /// </summary>
        LSIMegaRAID = 2
    }
}
