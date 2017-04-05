using GestionePrenotazioni.Domain.Support;
using GestionePrenotazioni.Host.Support;
using Jarvis.Framework.Kernel.ProjectionEngine.Client;
using Jarvis.Framework.Kernel.Support;
using Jarvis.Framework.Shared.Commands;
using Jarvis.Framework.Shared.IdentitySupport;
using System;
using System.IO;
using System.Linq;
using Topshelf;

namespace GestionePrenotazioni.Host
{
    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public static class Program
    {
        static GestionePrenotazioniConfiguration _config;

        static int Main(string[] args)
        {
            var lastErrorFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_lastError.txt");
            if (File.Exists(lastErrorFileName)) File.Delete(lastErrorFileName);
            try
            {

                LoadConfiguration();

                Int32 executionExitCode;
                if (args.Length > 0 && (args[0] == "install" || args[0] == "uninstall"))
                {
                    executionExitCode = (Int32)StartForInstallOrUninstall();
                }
                else
                {
                    executionExitCode = (Int32)StandardStart();
                }
                return executionExitCode;
            }
            catch (Exception ex)
            {
                File.WriteAllText(lastErrorFileName, ex.ToString());
                throw;
            }

        }

        private static TopshelfExitCode StartForInstallOrUninstall()
        {
            var exitCode = HostFactory.Run(host =>
            {
                host.Service<Object>(service =>
                {
                    service.ConstructUsing(() => new Object());
                    service.WhenStarted(s => Console.WriteLine("Start fake for install"));
                    service.WhenStopped(s => Console.WriteLine("Stop fake for install"));
                });

                host.RunAsLocalService();

                host.SetDescription(_config.TopShelfDescription);
                host.SetDisplayName(_config.TopShelfDisplayName);
                host.SetServiceName(_config.TopShelfServiceName);
            });
            return exitCode;
        }

        private static TopshelfExitCode StandardStart()
        {
            MongoRegistration.RegisterMongoConversions(
                "NEventStore.Persistence.MongoDB"
            );

            MongoFlatMapper.EnableFlatMapping(true);

            CommandsExtensions.EnableDiagnostics = true;

            SetupColors();

            ConfigureRebuild(_config);

            var exitCode = HostFactory.Run(host =>
            {

                host.UseOldLog4Net("log4net.config");

                host.Service<Bootstrapper>(service =>
                {
                    service.ConstructUsing(() => new Bootstrapper());
                    service.WhenStarted(s => s.Start(_config));
                    service.WhenStopped(s => s.Stop());
                });

                host.RunAsLocalService();

                host.SetDescription(_config.TopShelfDescription);
                host.SetDisplayName(_config.TopShelfDisplayName);
                host.SetServiceName(_config.TopShelfServiceName);
                host.SetInstanceName(_config.ServerAddress);
            });

            if (Environment.UserInteractive && exitCode != TopshelfExitCode.Ok)
            {
                Console.Error.WriteLine("Abnormal exit from topshelf: {0}. Press a key to continue", exitCode);
                Console.ReadKey();
            }
            return exitCode;
        }

        static void SetupColors()
        {
            if (!Environment.UserInteractive)
                return;
            Console.Title = _config.TopShelfServiceName + " Service @ " + _config.ServerAddress;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Clear();
        }

        static void LoadConfiguration()
        {
            _config = new AppConfigGestionePrenotazioniConfiguration();
        }

        static void ConfigureRebuild(GestionePrenotazioniConfiguration config)
        {
            if (!Environment.UserInteractive)
                return;

            if (!_config.RebuildEnabled)
                return;

            Banner();

            RebuildSettings.Init(true, config.NitroMode);

            if (RebuildSettings.ShouldRebuild)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("---> Rebuild the readmodel (y/N)?");

                var res = Console.ReadLine().Trim().ToLowerInvariant();
                if (res != "y")
                {
                    RebuildSettings.DisableRebuild();
                }
            }
        }

        private static string FindArgument(string[] args, string prefix)
        {
            var arg = args.FirstOrDefault(a => a.StartsWith(prefix));
            if (String.IsNullOrEmpty(arg)) return String.Empty;
            return arg.Substring(prefix.Length);
        }

        private static void Banner()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("===================================================================");
            Console.WriteLine(_config.TopShelfServiceName);
            Console.WriteLine("===================================================================");
            Console.WriteLine("  install                        -> install service");
            Console.WriteLine("  uninstall                      -> remove service");
            Console.WriteLine("  net start " + _config.TopShelfServiceName + "              -> start service");
            Console.WriteLine("  net stop " + _config.TopShelfServiceName + "               -> stop service");
            Console.WriteLine("===================================================================");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}