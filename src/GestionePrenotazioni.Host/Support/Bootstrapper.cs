using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;
using GestionePrenotazioni.Domain.Support;
using GestionePrenotazioni.Host.Support.Web;
using Jarvis.Framework.Bus.Rebus.Integration.Support;
using Jarvis.Framework.Kernel.ProjectionEngine.Client;
using Jarvis.Framework.Shared;
using Jarvis.Framework.Shared.Helpers;
using Jarvis.Framework.Shared.Messages;
using Jarvis.NEventStoreEx;
using Metrics;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using GestionePrenotazioni.Host.SignalR.Hubs;
using Rebus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jarvis.Framework.Kernel.ProjectionEngine;
using GestionePrenotazioni.Domain.ReadModel;

namespace GestionePrenotazioni.Host.Support
{
    class Bootstrapper
    {
        private GestionePrenotazioniConfiguration _config;

        IDisposable _webApplication;
        IWindsorContainer _container;
        ILogger _logger;
        JarvisStartableFacility _startableFacility;

        private Boolean isStopped = false;

        private String[] _databaseNames = new[] { "events", "originals", "artifacts", "system", "readmodel" };

        public void Start(GestionePrenotazioniConfiguration config)
        {
            _config = config;

            BuildContainer(config);

            if (_config.EnableSingleAggregateRepositoryCache)
            {
                _logger.InfoFormat("Single Aggregate Repository Cache - ENABLED");
                JarvisFrameworkGlobalConfiguration.EnableSingleAggregateRepositoryCache();
            }
            else
            {
                _logger.InfoFormat("Single Aggregate Repository Cache - DISABLED");
                JarvisFrameworkGlobalConfiguration.DisableSingleAggregateRepositoryCache();
            }
            if (_config.DisableRepositoryLockOnAggregateId)
            {
                _logger.InfoFormat("Repository lock on Aggregate Id - DISABLED");
                NeventStoreExGlobalConfiguration.DisableRepositoryLockOnAggregateId();
            }
            else
            {
                _logger.InfoFormat("Repository lock on Aggregate Id - ENABLED");
                NeventStoreExGlobalConfiguration.EnableRepositoryLockOnAggregateId();
            }

            while (StartupCheck() == false)
            {
                _logger.InfoFormat("Some precondition to start the service are not met. Will retry in 3 seconds!");
                Thread.Sleep(3000);
            }

            if (RebuildSettings.ShouldRebuild && Environment.UserInteractive)
            {
                Console.WriteLine("---> Set Log Level to INFO to speedup rebuild (y/N)?");
                var res = Console.ReadLine().Trim().ToLowerInvariant();
                if (res == "y")
                {
                    SetLogLevelTo("INFO");
                }
            }

            InitializeEverything(config);
        }

        public void Stop()
        {
            if (isStopped) return;

            isStopped = true;
        }


        private bool StartupCheck()
        {
            var result = CheckDatabase();

            if (!result)
                _logger.Warn("One or more mongo instances are not operational.");

            return result;
        }

        private bool CheckDatabase()
        {
            if (!CheckConnection(_config.MainDatabaseConnectionString))
                return false;

            return true;
        }

        private Boolean CheckConnection(String connection)
        {
            var url = new MongoUrl(connection);
            var client = new MongoClient(url);
            Task.Factory.StartNew(() =>
            {
                var allDb = client.ListDatabases();
            }); //forces a database connection
            Int32 spinCount = 0;
            ClusterState clusterState;

            while ((clusterState = client.Cluster.Description.State) != ClusterState.Connected &&
                spinCount++ < 100)
            {
                Thread.Sleep(20);
            }
            return clusterState == MongoDB.Driver.Core.Clusters.ClusterState.Connected;
        }

        private void InitializeEverything(GestionePrenotazioniConfiguration config)
        {
            var installers = new List<IWindsorInstaller>()
            {
                new BusInstaller(config),
                new CoreInstaller(config),
                new EventStoreInstaller(config),
            };

            _logger.Debug("Configured Scheduler");

            if (config.HasMetersEnabled)
            {
                //@@TODO: https://github.com/etishor/Metrics.NET/wiki/ElasticSearch
                var binding = config.ServerAddress;
                _logger.DebugFormat("Meters available on {0}", binding);

                Metric
                    .Config
                    .WithHttpEndpoint(binding)
                    .WithAllCounters();
            }

            GestionePrenotazioniApplication.SetConfig(config);
            GestionePrenotazioniApplication.SetContainer(_container);

            //Configure API Server.
            installers.Add(new ApiInstaller());

            var options = new StartOptions();

            _logger.InfoFormat("Web API Started at address {0}", config.ServerAddress);
            options.Urls.Add(config.ServerAddress);

            _container.Install(installers.ToArray());

            var projectionInstaller = new ProjectionsInstaller<NotifyReadModelChanges>(config);
            _container.Install(projectionInstaller);

            var test = _container.Resolve <ICollectionWrapper<AttrezzaturaReadModel, String>>();

            _startableFacility.StartAllIStartable();
            _container.CheckConfiguration();

            _webApplication = WebApp.Start<GestionePrenotazioniApplication>(options);
            _logger.InfoFormat("Server started");
        }

        void BuildContainer(GestionePrenotazioniConfiguration config)
        {
            _container = new WindsorContainer();
            _container.Kernel.ComponentRegistered += Kernel_ComponentRegistered;
            _container.Register(Component.For<GestionePrenotazioniConfiguration>().Instance(config));
            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel, true));
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel, true));

            _container.AddFacility<LoggingFacility>(f => f.LogUsing(new ExtendedLog4netFactory("log4net.config")));
            _logger = _container.Resolve<ILoggerFactory>().Create(GetType());
#if DEBUG
            UdpAppender.AppendToConfiguration();
#endif
            _startableFacility = new JarvisStartableFacility(_logger);
            _container.AddFacility(_startableFacility);
            _container.AddFacility<JarvisTypedFactoryFacility>();
        }

        private void Kernel_ComponentRegistered(string key, Castle.MicroKernel.IHandler handler)
        {
            if (handler.ComponentModel.Services.Contains(typeof(IBus)))
            {
                Debug.WriteLine("IBus registered");
            }
            if (handler.ComponentModel.Services.Contains(typeof(BusBootstrapper)))
            {
                Debug.WriteLine("BusBootstrapper registered");
            }
        }

        /// <summary>
        /// http://forums.asp.net/t/1969159.aspx?How+to+change+log+level+for+log4net+from+code+behind+during+the+application+up+and+running+
        /// </summary>
        /// <param name="logLevel"></param>
        private void SetLogLevelTo(string logLevel)
        {
            try
            {
                log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();

                //Configure all loggers to be at the debug level.
                foreach (log4net.Repository.ILoggerRepository repository in repositories)
                {
                    repository.Threshold = repository.LevelMap[logLevel];
                    log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                    log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                    foreach (log4net.Core.ILogger logger in loggers)
                    {
                        ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[logLevel];
                    }
                }

                //Configure the root logger.
                log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
                log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
                rootLogger.Level = h.LevelMap[logLevel];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR CHANGING LOG LEVEL {0}", ex.Message);
            }
        }
    }

    public class NotifyReadModelChanges : INotifyToSubscribers
    {
        public NotifyReadModelChanges()
        {
        }

        public void Send(object msg)
        {
            ReadModelUpdatedMessage updateMessage;

            if (!(msg is ReadModelUpdatedMessage)) { 
                // TODO Decidere se è il caso di lanciare un'eccezione, ivece!
                return;
            }

            updateMessage = (ReadModelUpdatedMessage)msg;
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();
            
            context.Clients.Group(updateMessage.ModelName).readModelUpdated(updateMessage);
            context.Clients.Group(updateMessage.ModelName + "@" + updateMessage.Id).readModelUpdated(updateMessage);
        }
    }
}