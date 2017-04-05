using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Jarvis.Framework.Kernel.Commands;
using Jarvis.Framework.Shared.Commands;
using Jarvis.Framework.Shared.ReadModel;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.Support
{
    public class CoreInstaller : IWindsorInstaller
    {
        private GestionePrenotazioniConfiguration _config;

        public CoreInstaller(GestionePrenotazioniConfiguration config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var logUrl = new MongoUrl(_config.LogsConnectionString);
            var logDb = new MongoClient(logUrl).GetDatabase(logUrl.DatabaseName);


            container.Register(
                 Component
                    .For<IMessagesTracker>()
                    .ImplementedBy<MongoDbMessagesTracker>()
                    .DependsOn(Dependency.OnValue<IMongoDatabase>(logDb)),
                Component
                    .For<ICommandBus, IInProcessCommandBus>()
                    .ImplementedBy<GestionePrenotazioniInProcessCommandBus>(),
                Classes
                    .FromThisAssembly()
                    .BasedOn(typeof(ICommandHandler<>))
                    .WithServiceFirstInterface()
                    .LifestyleTransient()
                );
        }
    }
}