/*
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
using System.Threading.Tasks;
using h3xmonitor.Settings;
using h3xmonitor.Status;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Linq;

namespace h3xmonitor.Monitors
{
    /// <summary>
    /// VMware ESXi server monitor.
    /// </summary>
    public class ESXiMonitor : IMonitor
    {  
        /// <summary>
        /// Server to monitor.
        /// </summary>
        private readonly Server _server;

        /// <summary>
        /// Create a ESXi monitor.
        /// </summary>
        /// <param name="pServer">Server that will be monitored</param>
        public ESXiMonitor(Server pServer)
        {
            _server = pServer;
        }

        /// <summary>
        /// Key event handler for keyboard interactive SSH login.
        /// </summary>
        /// <param name="pSender"></param>
        /// <param name="pE"></param>
        private void HandleKeyEvent(object pSender, AuthenticationPromptEventArgs pE)
        {
            foreach (AuthenticationPrompt prompt in pE.Prompts)
            {
                if (prompt.Request.IndexOf("Password:", StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    prompt.Response = _server.Password;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<ServerStatus> GetStatusAsync()
        {
            // create SSH connection
            using (var kauth = new KeyboardInteractiveAuthenticationMethod(_server.Username))
            using (var client = new SshClient(new ConnectionInfo(_server.HostnameOrIP, _server.Username, kauth)))
            {
                kauth.AuthenticationPrompt += HandleKeyEvent;
                client.Connect();

                // hostname, OS version
                string hostname, osVersie;
                using (var getHostname = client.CreateCommand("hostname"))
                using (var getVersie = client.CreateCommand("uname -r"))
                {
                    hostname = getHostname.Execute().Trim();
                    osVersie = getVersie.Execute().Trim();
                }

                // disk space check
                var filesystems = new List<FilesystemStatus>();
                using (var getFilesystems = client.CreateCommand("df"))
                {
                    var filesystemInfo = getFilesystems.Execute();
                    foreach (var r in filesystemInfo.Split('\n').Where(x => x.StartsWith("VMFS"))) // only check if the disk has VMFS filesystem
                    {
                        var filesystemSplit = r.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        filesystems.Add(new FilesystemStatus
                        {
                            Name = filesystemSplit[5],
                            TotalBytes = ulong.Parse(filesystemSplit[1]),
                            FreeBytes = ulong.Parse(filesystemSplit[3])
                        });
                    }
                }

                // SMART check
                var disks = new List<HarddiskStatus>();
                if (_server.DiskControllers.HasFlag(DiskControllers.StandardATA))
                    disks.AddRange(GetDiskInfoUsingEsxcli(client));
                if (_server.DiskControllers.HasFlag(DiskControllers.LSIMegaRAID))
                    disks.AddRange(GetDiskInfoUsingStorcli(client));

                return new ServerStatus
                {
                    Hostname = hostname,
                    Harddisks = disks,
                    OSVersion = osVersie.Trim(),
                    Filesystems = filesystems
                };
            }
        }

        /// <summary>
        /// Gets disk status using LSI storcli.
        /// </summary>
        /// <param name="pClient">SSH client</param>
        /// <returns>List of disks</returns>
        private static List<HarddiskStatus> GetDiskInfoUsingStorcli(SshClient pClient)
        {
            var disks = new List<HarddiskStatus>();
            using (var getAll = pClient.CreateCommand("./opt/lsi/storcli/storcli /c0 /eall /sall show all J"))
            using (var getSMART = pClient.CreateCommand("./opt/lsi/storcli/storcli /c0 /eall /sall show smart J"))
            {
                var resultObj = JsonConvert.DeserializeObject<JObject>(getAll.Execute());
                foreach (var c in resultObj["Controllers"])
                {
                    // iterate through controllers
                    var responseData = c["Response Data"];
                    foreach (var d in responseData.Where(x => ((JProperty)x).Name.Contains("Detailed Information")))
                    {
                        // iterate through disks
                        var driveAddr = ((JProperty) d).Name.Split(' ')[1]; // /c0/e252/s1
                        var drive = d.First();
                        var driveModel = drive[$"Drive {driveAddr} Device attributes"]["Model Number"].Value<string>();
                        var driveAlertFlagged = drive[$"Drive {driveAddr} State"]["S.M.A.R.T alert flagged by drive"].Value<string>();
                        disks.Add(new HarddiskStatus
                        {
                            Address = driveAddr,
                            Model = driveModel,
                            IsHealthy = driveAlertFlagged.Equals("No"),
                            SMARTData = null
                        });
                    }
                }
            }

            return disks;
        }

        /// <summary>
        /// Gets disk status using esxcli.
        /// </summary>
        /// <param name="pClient">SSH client</param>
        /// <returns>List of disks</returns>
        private static List<HarddiskStatus> GetDiskInfoUsingEsxcli(SshClient pClient)
        {
            const string DiskNamePrefix = "   Devfs Path: /vmfs/devices/disks/";

            // search for disk names
            var diskNames = new List<string>();
            using (var getDisks = pClient.CreateCommand("esxcli storage core device list"))
            {
                var getDisksResult = getDisks.Execute();
                var getDisksSplit = getDisksResult.Split('\n');
                foreach (var l in getDisksSplit)
                {
                    if (l.StartsWith(DiskNamePrefix))
                        diskNames.Add(l.Replace(DiskNamePrefix, ""));
                }
            }

            // per disk status ophalen
            var disks = new List<HarddiskStatus>();
            foreach (var d in diskNames)
            {
                // iterate through disks
                using (var getDiskInfo = pClient.CreateCommand("esxcli storage core device smart get -d=" + d))
                {
                    var diskStatus = new HarddiskStatus();
                    var getDiskInfoResult = getDiskInfo.Execute();
                    diskStatus.Address = d;
                    diskStatus.SMARTData = getDiskInfoResult;
                    diskStatus.Model = d;

                    // look for health status
                    foreach (var r in getDiskInfoResult.Split('\n'))
                    {
                        if (!r.StartsWith("Health Status") || !r.Contains("OK")) continue;
                        diskStatus.IsHealthy = true;
                        break;
                    }
                    disks.Add(diskStatus);
                }
            }
            return disks;
        }
    }
}
