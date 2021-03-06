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
using System.Threading.Tasks;
using h3xmonitor.Settings;
using h3xmonitor.Status;
using Renci.SshNet;
using System.Linq;

namespace h3xmonitor.Monitors
{
    /// <summary>
    /// GNU/Linux monitor.
    /// </summary>
    public class LinuxMonitor : IMonitor
    {
        /// <summary>
        /// Server to monitor.
        /// </summary>
        private readonly Server _server;

        /// <summary>
        /// Create a Linux monitor.
        /// </summary>
        /// <param name="pServer">Server that will be monitored</param>
        public LinuxMonitor(Server pServer)
        {
            _server = pServer;
        }

        //private void HandleKeyEvent(object pSender, AuthenticationPromptEventArgs pE)
        //{
        //    foreach (AuthenticationPrompt prompt in pE.Prompts)
        //    {
        //        if (prompt.Request.IndexOf("Password:", StringComparison.CurrentCultureIgnoreCase) != -1)
        //        {
        //            prompt.Response = _server.Password;
        //        }
        //    }
        //}

        /// <inheritdoc/>
        public async Task<ServerStatus> GetStatusAsync()
        {
            // create SSH session
            //using (var kauth = new KeyboardInteractiveAuthenticationMethod(_server.Username))
            using (var kauth = new PasswordAuthenticationMethod(_server.Username, _server.Password))
            using (var client = new SshClient(new ConnectionInfo(_server.HostnameOrIP, _server.Username, kauth)))
            {
                //kauth.AuthenticationPrompt += HandleKeyEvent;
                client.Connect();

                // hostname, OS version
                string hostname, osVersie;
                using (var getHostname = client.CreateCommand("hostname"))
                using (var getLsbRelease = client.CreateCommand("lsb_release -d -s"))
                using (var getVersie = client.CreateCommand("uname -r"))
                {
                    hostname = getHostname.Execute().Trim();
                    var lsbRelease = getLsbRelease.Execute();

                    if (lsbRelease.Length > 0)
                        osVersie = lsbRelease.Trim() + " (" + getVersie.Execute().Trim() + ")";
                    else
                        osVersie = "Unknown Linux" + " (" + getVersie.Execute().Trim() + ")";
                }

                // disk space check
                var filesystems = new List<FilesystemStatus>();
                using (var getFilesystems = client.CreateCommand("df --block-size 1")) // as bytes
                {
                    var filesystemInfo = getFilesystems.Execute();
                    foreach (var r in filesystemInfo.Split('\n').Where(x => x.StartsWith("/dev/") && !x.StartsWith("/dev/shm "))) // alleen /dev/-regels doen er toe
                    {
                        var filesystemSplit = r.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        var freeBytes = ulong.Parse(filesystemSplit[3]);
                        var usedBytes = ulong.Parse(filesystemSplit[2]);
                        filesystems.Add(new FilesystemStatus
                        {
                            Name = filesystemSplit[5],
                            TotalBytes = freeBytes + usedBytes,
                            FreeBytes = freeBytes
                        });
                    }
                }

                // smartctl
                List<HarddiskStatus> disks = null;
                try
                {
                    disks = new List<HarddiskStatus>();
                    using (
                            var getFilesystems = client.CreateCommand($"echo {_server.Password} | sudo -S smartctl --scan"))
                        // as bytes
                    {
                        var filesystemsOutput = getFilesystems.Execute().Trim().Split('\n');
                        foreach (var d in filesystemsOutput)
                        {
                            // iterate through detected disks
                            var diskAddress = d.Split(' ')[0];
                            using (
                                var getDiskInfo =
                                    client.CreateCommand($"echo {_server.Password} | sudo -S smartctl -a {diskAddress}")
                            )
                            {
                                // get disk info
                                var diskInfo = getDiskInfo.Execute();
                                if (SmartmontoolsTools.DiskIgnoreStrings.Any(x => diskInfo.Contains(x)))
                                    continue; // no SMART support, so skip

                                // parse disk info
                                var diskInfoSplit = diskInfo.Split('\n');
                                var diskModel =
                                    diskInfoSplit.First(x => x.Contains("Device Model:")).Substring(13).Trim();
                                disks.Add(new HarddiskStatus
                                {
                                    Address = diskAddress,
                                    Model = diskModel,
                                    IsHealthy = true,
                                    SMARTData = diskInfo
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    
                }

                return new ServerStatus
                {
                    Hostname = hostname,
                    Harddisks = disks,
                    OSVersion = osVersie.Trim(),
                    Filesystems = filesystems
                };
            }
        }
    }
}
