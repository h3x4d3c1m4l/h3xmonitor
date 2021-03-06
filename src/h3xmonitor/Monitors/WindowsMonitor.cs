﻿/*
    This file is part of h3xmonitor.

    h3xmonitor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    h3xmonitor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
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
        public async Task<ServerStatus> GetStatusAsync()
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
                var osVersie = ((string)osQuery.First().CimInstanceProperties["Caption"].Value).Trim();
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

                    // Size and FreeSpace is a long on some platforms, on others it might be a string
                    filesystems.Add(new FilesystemStatus
                    {
                        Name = name,
                        TotalBytes = totalBytes is string ? ulong.Parse((string) totalBytes) : (ulong) totalBytes,
                        FreeBytes = freeBytes is string ? ulong.Parse((string)freeBytes) : (ulong)freeBytes,
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
