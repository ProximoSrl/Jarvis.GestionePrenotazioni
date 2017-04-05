using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using GestionePrenotazioni.Domain.Support;
using Jarvis.Framework.Bus.Rebus.Integration.Support;
using Jarvis.Framework.Shared.Helpers;
using Rebus;

namespace GestionePrenotazioni.Host.Support
{
    /// <summary>
    /// A Windsor installer used to register everything is needed
    /// for rebus/bus to work.
    /// </summary>
    public class BusInstaller : IWindsorInstaller
    {
        /// <summary>
        /// Construct instance of Bus installer
        /// </summary>
        /// <param name="configuration"></param>
        public BusInstaller(GestionePrenotazioniConfiguration configuration)
        {
            _configuration = configuration;
        }

        private GestionePrenotazioniConfiguration _configuration;
        private ILogger _logger;

        /// <summary>
        /// Standard install configuration
        /// </summary>
        /// <param name="container"></param>
        /// <param name="store"></param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            _logger = container.Resolve<ILoggerFactory>().Create("BusInstaller");


            container.Register(
                Component
                    .For<JarvisRebusConfiguration>()
                    .Instance(_configuration.RebusConfiguration),

                Classes
                    .FromAssemblyInThisApplication()
                    .BasedOn(typeof(IHandleMessages<>))
                    .WithServiceAllInterfaces(),
                Component
                    .For<BusBootstrapper>()
                    .DependsOn(Dependency.OnValue<IWindsorContainer>(container))
                    .DependsOn(Dependency.OnValue("connectionString", _configuration.RebusConnectionString))
                    .DependsOn(Dependency.OnValue("prefix", "GestionePrenotazioni"))
                    .WithStartablePriorityHigh(),
                 Component
                    .For<BusStarter>()
            );
        }

    }
}
