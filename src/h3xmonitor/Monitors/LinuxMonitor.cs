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
        public async Task<ServerStatus> GetStatus()
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
                using (var getIssue = client.CreateCommand("cat /etc/issue"))
                using (var getVersie = client.CreateCommand("uname -r"))
                {
                    hostname = getHostname.Execute().Trim();
                    var issue = getIssue.Execute();

                    if (issue.Length > 8)
                        osVersie = issue.Substring(0, issue.Length - 8) + " (" + getVersie.Execute().Trim() + ")";
                    else
                        osVersie = "Unknown Linux" + " (" + getVersie.Execute().Trim() + ")";
                }

                // disk space check
                var filesystems = new List<FilesystemStatus>();
                using (var getFilesystems = client.CreateCommand("df --block-size 1"))
                {
                    var filesystemInfo = getFilesystems.Execute();
                    foreach (var r in filesystemInfo.Split('\n').Where(x => x.StartsWith("/dev/") && !x.StartsWith("/dev/shm "))) // alleen /dev/-regels doen er toe
                    {
                        var filesystemSplit = r.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        long freeBytes = long.Parse(filesystemSplit[3]);
                        long usedBytes = long.Parse(filesystemSplit[2]);
                        filesystems.Add(new FilesystemStatus
                        {
                            Name = filesystemSplit[5],
                            TotalBytes = freeBytes + usedBytes,
                            FreeBytes = freeBytes
                        });
                    }
                }

                return new ServerStatus
                {
                    Hostname = hostname,
                    Harddisks = null,
                    OSVersion = osVersie.Trim(),
                    Filesystems = filesystems
                };
            }
        }
    }
}
