using Castle.Core.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using GestionePrenotazioni.Domain;
using GestionePrenotazioni.Domain.Support;
using Jarvis.Framework.Kernel.Engine;
using Jarvis.Framework.Kernel.Engine.Snapshots;
using Jarvis.Framework.Kernel.ProjectionEngine.Client;
using Jarvis.Framework.Kernel.Support;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.Framework.Shared.IdentitySupport.Serialization;
using Jarvis.Framework.Shared.Logging;
using Jarvis.NEventStoreEx.CommonDomainEx;
using Jarvis.NEventStoreEx.CommonDomainEx.Core;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence.EventStore;
using MongoDB.Bson;
using MongoDB.Driver;
using NEventStore;
using NEventStore.Domain;
using NEventStore.Logging;
using NEventStore.Persistence.MongoDB;
using NEventStore.Persistence.MongoDB.Support;
using System;

namespace GestionePrenotazioni.Host.Support
{
    public class EventStoreInstaller : IWindsorInstaller
    {
        readonly GestionePrenotazioniConfiguration _config;

        public EventStoreInstaller(GestionePrenotazioniConfiguration config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            RegisterGlobalComponents(container);
            RegisterMappings(container);
        }

     
       
        void RegisterGlobalComponents(IWindsorContainer container)
        {
            var mainConnectionUrl = new MongoUrl(_config.MainDatabaseConnectionString);
            var mainDb = new MongoClient(mainConnectionUrl)
                .GetDatabase(mainConnectionUrl.DatabaseName);

            ILogger baseLogger = container.Resolve<ILogger>();
            var neventstoreLog = new NEventStoreLog4NetLogger(baseLogger);
 
            container.Register(
                Component
                    .For<IConstructAggregatesEx>()
                    .ImplementedBy<AggregateFactory>(),
                Component
                    .For<EventStoreFactory>()
                    .DependsOn()
                    .LifestyleSingleton(),
                Component
                    .For<IDetectConflicts>()
                    .ImplementedBy<AlwaysConflict>()
                    .LifestyleTransient(),
                Component
                    .For<ICommitPollingClientFactory>()
                    .AsFactory(),
                Component
                    .For<IRepositoryExFactory>()
                    .AsFactory(),
                Component
                    .For<ICounterService>()
                    .ImplementedBy<CounterService>()
                    .DependsOn(Dependency.OnValue<IMongoDatabase>(mainDb)),
                Component
                    .For<IIdentityManager, IIdentityGenerator, IIdentityConverter, IdentityManager>()
                    .ImplementedBy<IdentityManager>(),
                Component
                    .For<ILog>()
                    .Instance(neventstoreLog)
                );

            var mongoPersistenceOptions = new MongoPersistenceOptions();
            mongoPersistenceOptions.DisableSnapshotSupport = true;
            mongoPersistenceOptions.ConcurrencyStrategy = 
                ConcurrencyExceptionStrategy.FillHole;

            var eventsCollection = mainDb.GetCollection<BsonDocument>("Commits");
            mongoPersistenceOptions.CheckpointGenerator = 
                new InMemoryCheckpointGenerator(eventsCollection);

            container.Register(
                Classes
                    .FromAssemblyContaining<GestionePrenotazioniDefinitionClass>()
                    .BasedOn<IPipelineHook>()
                    .WithServiceAllInterfaces(),
                Component
                    .For<IStoreEvents>()
                    .Named("EventStore")
                    .UsingFactory<EventStoreFactory, IStoreEvents>(f =>
                    {
                        var hooks = container.ResolveAll<IPipelineHook>();

                        return f.BuildEventStore(
                                _config.MainDatabaseConnectionString,
                                hooks,
                                mongoPersistenceOptions: mongoPersistenceOptions
                            );
                    })
                    .LifestyleSingleton(),
                Component
                    .For<IRepositoryEx, RepositoryEx>()
                    .ImplementedBy<RepositoryEx>()
                    .LifestyleTransient(),
                Component
                    .For<Func<IRepositoryEx>>()
                    .Instance(() => container.Resolve<IRepositoryEx>()),
                Component
                    .For<ISnapshotManager>()
                    .DependsOn(Dependency.OnValue("cacheEnabled", _config.EnableSnapshotCache))
                    .ImplementedBy<CachedSnapshotManager>(),
                Component
                    .For<IAggregateCachedRepositoryFactory>()
                    .ImplementedBy<AggregateCachedRepositoryFactory>()
                    .DependsOn(Dependency.OnValue("cacheDisabled", false)),
                Component
                    .For<ISnapshotPersistenceStrategy>()
                    .ImplementedBy<NumberOfCommitsShapshotPersistenceStrategy>()
                    .DependsOn(Dependency.OnValue("commitsThreshold", 100)),
                    Component.For<ISnapshotPersister>()
                        .ImplementedBy<MongoSnapshotPersisterProvider>()
                        .DependsOn(Dependency.OnValue<IMongoDatabase>(mainDb))
                );
        }

     
        static void RegisterMappings(IWindsorContainer container)
        {
            var identityManager = container.Resolve<IdentityManager>();

            EnableFlatIdMapping(identityManager);

            MongoRegistration.RegisterAssembly(typeof(GestionePrenotazioniDefinitionClass).Assembly);
            SnapshotRegistration.AutomapAggregateState(typeof(GestionePrenotazioniDefinitionClass).Assembly);

            identityManager.RegisterIdentitiesFromAssembly(typeof(GestionePrenotazioniDefinitionClass).Assembly);
        }

        static void EnableFlatIdMapping(IdentityManager converter)
        {
            MongoFlatIdSerializerHelper.Initialize(converter);
        }
    }
}
