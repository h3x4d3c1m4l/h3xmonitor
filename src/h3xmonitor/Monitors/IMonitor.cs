using System.Threading.Tasks;
using h3xmonitor.Status;

namespace h3xmonitor.Monitors
{
    /// <summary>
    /// Monitor interface, for implementing platform specific monitoring.
    /// </summary>
    interface IMonitor
    {
        /// <summary>
        /// Get the server status.
        /// </summary>
        /// <returns></returns>
        Task<ServerStatus> GetStatus();
    }
}
