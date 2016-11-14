using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading.Tasks;
using h3xmonitor.Settings;
using h3xmonitor.Status;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System.Linq;

namespace h3xmonitor.Monitors
{
    /// <summary>
    /// Monitoring for Windows machines.
    /// </summary>
    public class WindowsMonitor : IMonitor
    {
        /// <summary>
        /// Server to monitor.
        /// </summary>
        private readonly Server _server;

        /// <summary>
        /// Create a Windows monitor.
        /// </summary>
        /// <param name="pServer">Server that will be monitored</param>
        public WindowsMonitor(Server pServer)
        {
            _server = pServer;
        }

        /// <inheritdoc/>
        public async Task<ServerStatus> GetStatus()
        {
            const string Namespace = @"root\cimv2";
            const string OSQuery = "SELECT Caption, CSName FROM Win32_OperatingSystem";
            const string DiskspaceQuery = "SELECT VolumeName, Name, Size, FreeSpace " +
                                          "FROM Win32_LogicalDisk " +
                                          "WHERE DriveType = 3 AND FileSystem != 'FTPUSE' " +
                                          "AND FreeSpace IS NOT NULL AND Size IS NOT NULL";

            // create Credentials
            var securepassword = new SecureString();
            foreach (char c in _server.Password)
            {
                securepassword.AppendChar(c);
            }
            CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default, "", _server.Username,
                securepassword);

            // create SessionOptions using Credentials
            var sessionOptions = new WSManSessionOptions();
            sessionOptions.AddDestinationCredentials(Credentials);

            using (var mySession = CimSession.Create(_server.HostnameOrIP, sessionOptions))
            {
                // computernaam en OS-versie
                var osQuery = mySession.QueryInstances(Namespace, "WQL", OSQuery);
                var osVersie = (string)osQuery.First().CimInstanceProperties["Caption"].Value;
                var hostname = (string)osQuery.First().CimInstanceProperties["CSName"].Value;

                // partitie ruimtes
                var filesystems = new List<FilesystemStatus>();
                var filesystemsQuery = mySession.QueryInstances(Namespace, "WQL", DiskspaceQuery);
                foreach (var f in filesystemsQuery)
                {
                    var fcip = f.CimInstanceProperties;
                    var totalBytes = fcip["Size"].Value;
                    var freeBytes = fcip["FreeSpace"].Value;
                    var name = (string) fcip["Name"].Value;
                    var volumeName = fcip["VolumeName"]?.Value;
                    if (!string.IsNullOrWhiteSpace(volumeName as string))
                        name += " - " + volumeName;

                    filesystems.Add(new FilesystemStatus
                    {
                        Name = name,
                        TotalBytes = (long)(ulong) totalBytes,
                        FreeBytes = (long)(ulong) freeBytes
                    });
                }

                // harddisk status
                List<HarddiskStatus> disks = null;
                try
                {
                    disks = GetDiskStatus(_server.HostnameOrIP, _server.Username, securepassword);
                }
                catch (Exception ex)
                {
                    // TODO: log this
                }

                var result = new ServerStatus
                {
                    Hostname = hostname,
                    OSVersion = osVersie,
                    Filesystems = filesystems,
                    Harddisks = disks
                };
                return result;
            }
        }

        /// <summary>
        /// Get disk status.
        /// </summary>
        /// <param name="pHostname"></param>
        /// <param name="pUsername"></param>
        /// <param name="pPassword"></param>
        /// <returns></returns>
        private List<HarddiskStatus> GetDiskStatus(string pHostname, string pUsername, SecureString pPassword)
        {
            const string smartctl = @"C:\Program Files\smartmontools\bin\smartctl.exe";
            const string shell = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

            var credential = new PSCredential(pUsername, pPassword);
            var connectionInfo = new WSManConnectionInfo(new Uri("http://" + pHostname + ":5985/wsman"), shell, credential);

            Runspace remote = RunspaceFactory.CreateRunspace(connectionInfo);
            remote.Open();

            var harddisks = new List<HarddiskStatus>();
            using (var ps = PowerShell.Create())
            {
                ps.Runspace = remote;
                ps.AddCommand(smartctl);
                ps.AddArgument("--scan");
                var detectDisksOutput = ps.Invoke();
                ps.Commands.Clear();

                foreach (var d in detectDisksOutput.Where(x => !((string) x.BaseObject).Contains("SCSI device")))
                {
                    // get disk info
                    var diskRow = (string) d.BaseObject;
                    var disk = diskRow.Split(' ')[0];
                    ps.AddCommand(smartctl);
                    ps.AddArgument("-a");
                    ps.AddArgument(disk);
                    var diskOutput = ps.Invoke();
                    ps.Commands.Clear();

                    // check if device supports SMART
                    if (SmartmontoolsTools.DiskIgnoreStrings.Any(x => diskOutput.Any(y => ((string)y.BaseObject).Contains(x))))
                        continue; // skip, it's probably an optical drive

                    var diskName = (string) diskOutput.First(x => ((string) x.BaseObject).StartsWith("Device Model:")).BaseObject;
                    var diskNameTrimmed = diskName.Substring(13).Trim();

                    harddisks.Add(new HarddiskStatus
                    {
                        Model = diskNameTrimmed,
                        SMARTData = diskOutput.Aggregate("", (current, o) => current + o.BaseObject + "\r\n"),
                        IsHealthy = true
                    });
                }
            }

            return harddisks;
        }
    }
}
