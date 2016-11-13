# h3xmonitor
h3xmonitor is a very basic server monitoring application.

## Features

On supported OS'es (see next section), h3xmonitor supports monitoring:
* Disk space
* SMART status

It also reports if an exception occured while connecting to a server or while retrieving information from the server.

In the near future port monitoring will be added too. Suggestions for other features are welcome!

## Monitoring support

Supports monitoring the following OS'es:
* VMware ESXi
  * 5.1.0 and 6.0.0 tested
  * also supports LSI MegaRAID controllers, if storcli is installed
* Windows
  * only tested on Windows 10 Pro
  * needs WinRM to be enabled and smartmontools to be installed to it's default directory
* Linux
  * tested on: Ubuntu, Synology NAS
  * needs smartmontools to be installed
  
## How to use

Use h3xmonitor by creating a config file (example present in misc directory). Putting it in the same directory as it's executable and running the executable. h3xmonitor wil try to connect to the server, retrieve information and writes the result to `result.json`.

## Development tools

h3xmonitor is developed using Visual Studio 2015 with ReSharper.

You will also need .NET Core tools, available here: https://www.microsoft.com/net/core#windows

## Code style

I use the default ReSharper 2016.2.2 code style rules, with one exception:

Method parameters are CamelCased with a 'p' prefix. For example `pServerIP` or `pTimeout` are valid parameter names.

## Libraries used (and their licences)

* Json.NET
  * https://github.com/JamesNK/Newtonsoft.Json
  * Licenced under MIT
* SSH.NET
  * https://github.com/sshnet/SSH.NET
  * Licenced under MIT
* Several CoreFX and Powershell libraries
  * https://github.com/dotnet/corefx
  * https://github.com/PowerShell/PowerShell
  * Some parts licenced under MIT, other parts licenced under Apache 2
