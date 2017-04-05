using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using GestionePrenotazioni.Domain;
using GestionePrenotazioni.Domain.Support;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.ProjectionEngine;
using Jarvis.Framework.Kernel.ProjectionEngine.Client;
using Jarvis.Framework.Kernel.ProjectionEngine.RecycleBin;
using Jarvis.Framework.Shared.Messages;
using Jarvis.Framework.Shared.ReadModel;
using MongoDB.Driver;
using NEventStore.Persistence;
using System;
using System.Collections.Generic;

namespace GestionePrenotazioni.Host.Support
{
    public class ProjectionsInstaller<TNotifier> : IWindsorInstaller where TNotifier : INotifyToSubscribers
    {
        readonly GestionePrenotazioniConfiguration _config;

        public ProjectionsInstaller(
            GestionePrenotazioniConfiguration config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var mainConnectionUrl = new MongoUrl(_config.MainDatabaseConnectionString);
            var mainDb = new MongoClient(mainConnectionUrl)
                .GetDatabase(mainConnectionUrl.DatabaseName);

            // add rm prefix to collections
            var config = new ProjectionEngineConfig
            {
                EventStoreConnectionString = _config.MainDatabaseConnectionString,
                Slots = new[] { "*" },
                PollingMsInterval = 1000,
                ForcedGcSecondsInterval = 0,
                DelayedStartInMilliseconds = 2000,
                EngineVersion = "v3",
                BucketInfo = new List<BucketInfo>()
                {
                    new BucketInfo()
                    {
                        BufferSize = 10000,
                        Slots = new [] { "*" }
                    }
                },
            };

            var readModelDb = mainDb;

            container.Register(
                Component
                    .For(typeof(IReader<,>), typeof(IMongoDbReader<,>))
                    .ImplementedBy(typeof(MongoReaderForProjections<,>))
                    .DependsOn(Dependency.OnValue<IMongoDatabase>(readModelDb))
            );

            container.Register(
               Component
                   .For<IHousekeeper>()
                   .ImplementedBy<NullHouseKeeper>(),
               Component
                   .For<INotifyToSubscribers>()
                   .ImplementedBy<TNotifier>(),
               Component
                       .For<ICommitEnhancer>()
                       .ImplementedBy<CommitEnhancer>(),
               Component
                   .For<INotifyCommitHandled>()
                   .ImplementedBy<NullNotifyCommitHandled>(),
               Classes
                   .FromAssemblyContaining<GestionePrenotazioniDefinitionClass>()
                   .BasedOn<IProjection>()
                   .WithServiceAllInterfaces()
                   .LifestyleSingleton(),
               Component
                   .For<IInitializeReadModelDb>()
                   .ImplementedBy<InitializeReadModelDb>(),
               Component
                   .For<IConcurrentCheckpointTracker>()
                   .ImplementedBy<ConcurrentCheckpointTracker>()
                   .DependsOn(Dependency.OnValue<IMongoDatabase>(readModelDb)),
               Component
                   .For(new[]
                   {
                        typeof (ICollectionWrapper<,>),
                        typeof (IReadOnlyCollectionWrapper<,>)
                   })
                   .ImplementedBy(typeof(CollectionWrapper<,>))
                   .DependsOn(Dependency.OnValue<IMongoDatabase>(readModelDb)),
               Component
                   .For<IRebuildContext>()
                   .ImplementedBy<RebuildContext>()
                   .DependsOn(Dependency.OnValue<bool>(RebuildSettings.NitroMode)),
               Component
                   .For<IMongoStorageFactory>()
                   .ImplementedBy<MongoStorageFactory>()
                   .DependsOn(Dependency.OnValue<IMongoDatabase>(readModelDb)),
               Component
                   .For<IRecycleBin>()
                   .ImplementedBy<RecycleBin>()
                   .DependsOn(Dependency.OnValue<IMongoDatabase>(readModelDb))
               );

            //if (config.EngineVersion == "v3")

            container.Register(
                Component.For<ProjectionEngineConfig>()
                    .Instance(config),
               Component
                   .For<ICommitPollingClient>()
                   .ImplementedBy<CommitPollingClient2>()
                   .DependsOn(Dependency.OnValue("id", "Main-Poller"))
                   .LifeStyle.Transient,
                Component
                    .For<Func<IPersistStreams, ICommitPollingClient>>()
                    .Instance(ps => container.Resolve<ICommitPollingClient>(new { persistStreams = ps })),
              Component
                    .For<ProjectionEngine, ITriggerProjectionsUpdate>()
                    .ImplementedBy<ProjectionEngine>()
                    .LifestyleSingleton()
                    .StartUsingMethod(x => x.Start)
                    .StopUsingMethod(x => x.Stop)
                    );

        }
    }
}
