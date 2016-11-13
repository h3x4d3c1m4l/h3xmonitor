using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using h3xmonitor.Monitors;
using h3xmonitor.Settings;
using h3xmonitor.Status;
using Newtonsoft.Json;

namespace h3xmonitor
{
    /// <summary>
    /// Main class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Settings path.
        /// </summary>
        private const string Settings = "h3xmonitor_settings.json";

        /// <summary>
        /// Result path.
        /// </summary>
        private const string Result = "h3xmonitor_result.json";

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Console.Out.WriteLine("h3xmonitor");
            Console.Out.WriteLine("by h3x4d3c1m4l, released under GPL v3 licence\r\n");

            // read settings
            Settings.Settings settings = null;
            try
            {
                // try reading the settings file
                var json = File.ReadAllText(Settings);
                settings = JsonConvert.DeserializeObject<Settings.Settings>(json);
            } catch (Exception)
            {
                // failed, possible syntax error or missing file
                Console.Out.WriteLine("\r\nCan't read settings, a sample settingfile will be written to " + Settings);
                Console.Out.WriteLine("Press any key to continue, or close the window if you do not want this!");
                Console.In.Read();

                // generate sample settings file
                var json = JsonConvert.SerializeObject(new Settings.Settings
                {
                    Servers = new List<Server>
                    {
                        new Server
                        {
                            HostnameOrIP = "server.hostname",
                            FriendlyName = "Servername",
                            DiskControllers = DiskControllers.StandardATA,
                            OS = ServerOS.Windows,
                            Username = "username",
                            Password = "passwords"
                        }
                    }
                }, Formatting.Indented);
                File.WriteAllText(Settings, json);
                Environment.Exit(0);
            }

            // do the monitoring
            List<ServerStatus> statusList = new List<ServerStatus>();
            //foreach (var s in settings.Servers.Where(x => !x.Skip))
            Parallel.ForEach(settings.Servers.Where(x => !x.Skip), s =>
            {
                Console.Out.WriteLine("Server: " + s.FriendlyName);
                ServerStatus status = null;
                try
                {
                    switch (s.OS)
                    {
                        case ServerOS.ESXi:
                            status = new ESXiMonitor(s).GetStatus().Result;
                            break;
                        case ServerOS.Linux:
                            status = new LinuxMonitor(s).GetStatus().Result;
                            break;
                        case ServerOS.Windows:
                            status = new WindowsMonitor(s).GetStatus().Result;
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
                    statusList.Add(status);

                    Console.Out.WriteLine("Server: " + s.FriendlyName + " (done)");
                }
            });

            // create object, serialize to json, write to file
            var resultaat = new Result
            {
                Servers = statusList,
                Date = DateTime.Now
            };
            File.WriteAllText(Result, JsonConvert.SerializeObject(resultaat, Formatting.Indented));
        }
    }
}
