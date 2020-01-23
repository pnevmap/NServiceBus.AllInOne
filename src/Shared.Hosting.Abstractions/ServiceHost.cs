using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Shared.Hosting.Abstractions.Logging;
using Topshelf;
using Topshelf.HostConfigurators;

namespace Shared.Hosting.Abstractions
{
    public static class ServiceHost
    {
        private static readonly string[] TopShelfMangeVerbs =
        {
            "help", "install", "stop", "uninstall"
        };

        public static int Run(string[] args, string name, IService service, ServiceHostingControl hostingControl = null)
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            var selfLogger = false;

            if (Log.Logger == Logger.None)
            {
                Log.Logger = BuildConfiguration(args).CreateNewLogger(name);
                selfLogger = true;
            }

            Log.Logger.Information($"Using current directory: {Directory.GetCurrentDirectory()}");

            var result = -1;
            
            try
            {
                result = CanRunToshelf()
                    ? RunTopshelf(args, name, service, hostingControl)
                    : RunPlainConsole(name, service, hostingControl);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failure running service {name}", name);
            }

            if(selfLogger)
                Log.CloseAndFlush();

            return result;
        }

        private static int RunPlainConsole(string name, IService service, ServiceHostingControl hostingControl)
        {
            Log.Information("Using plain console to run service");
            
            try
            {
                Console.Title = name;
               
                service.Start();
            }
            catch (Exception e)
            {
                Log.ForContext(typeof(ServiceHost)).Fatal(e, "Failed to run service");
                return -1;
            }
            
            var stop = new AutoResetEvent(false);

            if(hostingControl != null)
                hostingControl.HostControl = new ConsoleHostControl(stop);
            
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                stop.Set();
            };

            stop.WaitOne();

            try
            {
                service.Stop();
            }
            catch (Exception e)
            {
                Log.ForContext(typeof(ServiceHost)).Error(e, "Failed to properly stop service");
                return -2;
            }
            
            return 0;
        }
        
        private static int RunTopshelf(IReadOnlyList<string> args, string name, IService service, ServiceHostingControl hostingControl)
        {
            Log.Information("Using Topshelf to run service");
            
            var rc = HostFactory.Run(x =>
            {
                if (hostingControl?.CustomConfiguration == null)
                    ValidateCommandLine(x, args);
                else
                    hostingControl.CustomConfiguration.Invoke(x);

                x.Service<IService>(sc =>
                {
                    sc.ConstructUsing(() =>
                    {
                        // just to repeat as if it started as a service then different entry point is used
                        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

                        if (Log.Logger == Logger.None)
                            Log.Logger = BuildConfiguration(new string[0]).CreateNewLogger(name);

                        var log = Log.ForContext(typeof(ServiceHost));
                        
                        log.Information($"Using current directory to run Topshelf: {Directory.GetCurrentDirectory()}");

                        try
                        {
                            return service;
                        }
                        catch (Exception e)
                        {
                            log.Fatal(e, "Failed to create service");
                            throw;
                        }
                    });

                    sc.WhenStarted((s, c) =>
                    {
                        if (hostingControl != null)
                            hostingControl.HostControl = c;

                        try
                        {
                            s.Start();
                        }
                        catch (Exception e)
                        {
                            Log.ForContext(typeof(ServiceHost)).Fatal(e, "Failed to start service");
                            throw;
                        }

                        return true;
                    });

                    sc.WhenStopped(tc => tc.Stop());
                });

                x.UseSerilog();
                x.SetDisplayName(name);
                x.SetServiceName(name);
                
                x.EnableServiceRecovery(r =>
                {
                    r.SetResetPeriod(1);
                    r.RestartService(0);
                    r.TakeNoAction();
                });
            });

            return (int) Convert.ChangeType(rc, rc.GetTypeCode());
        }

        private static void ValidateCommandLine(HostConfigurator configurator, IReadOnlyList<string> args)
        {
            if(!args.Any())
                return;

            if (TopShelfMangeVerbs.Contains(args[0], StringComparer.OrdinalIgnoreCase))
                return;

            if (string.Equals("run", args[0], StringComparison.OrdinalIgnoreCase))
            {
                configurator.ApplyCommandLine("");
                return;
            }

            if (string.Equals("start", args[0], StringComparison.OrdinalIgnoreCase))
            {
                if (args.Count == 1)
                    return;

                if (string.Equals(args[1], "-instance", StringComparison.OrdinalIgnoreCase) ||
                   args[1].StartsWith("-instance:", StringComparison.OrdinalIgnoreCase))
                    return;

                configurator.ApplyCommandLine("start");
            }

            configurator.ApplyCommandLine("");
        }

        private static bool CanRunToshelf()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    Log.Logger.Verbose($"Can run in Topshelf, platform {Environment.OSVersion.Platform}");
                    return true;
                default:
                    Log.Logger.Verbose($"Cannot run in Topshelf, platform {Environment.OSVersion.Platform}");
                    return false;
            }
        }
        
        private class ConsoleHostControl : HostControl
        {
            private readonly AutoResetEvent _stop;

            public ConsoleHostControl(AutoResetEvent stop)
            {
                _stop = stop;
            }

            public void RequestAdditionalTime(TimeSpan timeRemaining)
            {
            }

            public void Stop()
            {
                _stop.Set();
            }

            public void Stop(TopshelfExitCode exitCode)
            {
                _stop.Set();
            }
        }

        private static IConfiguration BuildConfiguration(string[] args, Action<IConfigurationBuilder> configBuilder = null)
        {
            var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true, false);

            if (!string.IsNullOrWhiteSpace(environmentName))
                builder.AddJsonFile($"appsettings.{environmentName}.json", true, false);

            builder.AddEnvironmentVariables();

            if (args != null && args.Any())
                builder.AddCommandLine(args);

            configBuilder?.Invoke(builder);

            return builder.Build();
        }
    }
}