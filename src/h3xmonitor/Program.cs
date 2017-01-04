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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using h3xmonitor.Logging;
using h3xmonitor.Monitors;
using h3xmonitor.Settings;
using h3xmonitor.Status;
using Microsoft.PowerShell.Commands;
using Newtonsoft.Json;

namespace h3xmonitor
{
    /// <summary>
    /// Main class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // parse arguments
            Parser.Default.ParseArguments<Options>(args).WithParsed(pOptions =>
            {
                // succesfully parsed, print header
                var verInf = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                Console.Error.WriteLine($"{verInf.ProductName} {verInf.ProductVersion}\r\n{verInf.LegalCopyright}\r\n");

                //get JSON stream
                Log.Write(LoglineLevel.Debug, "Reading settings from " + (pOptions.InputFile ?? "stdin"));
                StreamReader confStrReader = null;
                var confReader = pOptions.InputFile == null ? new JsonTextReader(Console.In) : new JsonTextReader(confStrReader = File.OpenText(pOptions.InputFile));

                // deserialize from stream
                var serializer = new JsonSerializer();
                var config = serializer.Deserialize<Settings.Settings>(confReader);
                confStrReader?.Dispose();

                // do monitoring
                Log.Write(LoglineLevel.Debug, "Starting monitoring");
                var serverStatusses = GetServerStatusses(config.Servers);
                var result = new Result {Date = DateTime.Now, Servers = serverStatusses};

                // return result
                Log.Write(LoglineLevel.Debug, "Outputting result");
                StreamWriter resultStrWriter = null;
                FileStream resultFileStream = null;
                var resultWriter = pOptions.OutputFile == null ? new JsonTextWriter(Console.Out) : new JsonTextWriter(resultStrWriter = new StreamWriter(resultFileStream = File.OpenWrite(pOptions.OutputFile)));
                serializer.Serialize(resultWriter, result);
                resultStrWriter?.Dispose();
                resultFileStream?.Dispose();
            }).WithNotParsed(pOptions =>
            {
                // error during parsing
                Environment.Exit(1);
            });

            // close application
            Log.StopAndAwaitStopping();
            Environment.Exit(0);
        }

        /// <summary>
        /// Check host services.
        /// </summary>
        /// <param name="pHost"></param>
        /// <param name="pServices"></param>
        /// <returns></returns>
        private static List<ServiceStatus> GetServiceStatusses(string pHost, List<ServiceTest> pServices)
        {
            var statusses = new List<ServiceStatus>();
            foreach (var s in pServices)
            {                
                var status = new ServiceStatus { Port = s.Port, Reference = s.Reference };
                statusses.Add(status);

                // now really check
                using (var tcpClient = new TcpClient())
                try
                {
                    var connect = tcpClient.ConnectAsync(pHost, (int) s.Port);
                    connect.Wait();
                    status.IsOpen = true; // succeeded
                }
                catch (Exception)
                {
                    // failed, but no problem
                }
            }

            return statusses;
        }

        private static List<ServerStatus> GetServerStatusses(IEnumerable<Server> configServers)
        {
            List<ServerStatus> statusList = new List<ServerStatus>();
            //foreach (var s in settings.Servers.Where(x => !x.Skip))
            Parallel.ForEach(configServers.Where(x => !x.Skip), s =>
            {
                Log.Write(LoglineLevel.Info, "Server: " + s.FriendlyName);
                ServerStatus status = null;
                try
                {
                    // platform specific monitoring features
                    switch (s.OS)
                    {
                        case ServerOS.ESXi:
                            status = new ESXiMonitor(s).GetStatusAsync().Result;
                            break;
                        case ServerOS.Linux:
                            status = new LinuxMonitor(s).GetStatusAsync().Result;
                            break;
                        case ServerOS.Windows:
                            status = new WindowsMonitor(s).GetStatusAsync().Result;
                            break;
                        default:
                            throw new Exception("Unsupported server type: " + s.OS);
                    }
                }
                catch (Exception ex)
                {
                    status = new ServerStatus
                    {
                        MonitoringFailed = true,
                        MonitoringError = ex.ToString()
                    };
                }
                finally
                {
                    // ping test
                    try
                    {
                        var pingSender = new Ping();
                        var pingResult = pingSender.SendPingAsync(s.HostnameOrIP, 1000).Result;
                        if (pingResult.Status == IPStatus.Success)
                            status.Ping = pingResult.RoundtripTime;
                    }
                    catch (Exception)
                    {
                        // failed
                        status.Ping = null;
                    }

                    // services test
                    List<ServiceStatus> services = null;
                    if (s.TCPServices != null)
                        services = GetServiceStatusses(s.HostnameOrIP, s.TCPServices);

                    // try to retrieve the server IP
                    try
                    {
                        var dnsResult = Dns.GetHostAddressesAsync(s.HostnameOrIP).Result;
                        status.IP =
                            dnsResult.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?.ToString() ??
                            dnsResult.First()?.ToString();
                    }
                    catch (Exception)
                    {
                        // doesn't matter
                    }

                    status.FriendlyName = s.FriendlyName;
                    status.Reference = s.Reference;
                    status.OS = s.OS;
                    status.Services = services;
                    statusList.Add(status);

                    Log.Write(LoglineLevel.Debug, "Server: " + s.FriendlyName + " (done)");
                }
            });

            return statusList;
        }
    }
}
