using Jarvis.Framework.Bus.Rebus.Integration.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.Support
{
    public class GestionePrenotazioniConfiguration
    {
        public bool DisableRepositoryLockOnAggregateId { get; protected set; }
        public bool EnableSingleAggregateRepositoryCache { get; protected set; }
        public bool EnableSnapshotCache { get; protected set; }
        public bool HasMetersEnabled { get; protected set; }
        public string LogsConnectionString { get; protected set; }

        public string RebusConnectionString { get; protected set; }

        public string MainDatabaseConnectionString { get; protected set; }

        public bool NitroMode { get; protected set; }

        public Boolean RebuildEnabled { get; protected set; }
        public String ServerAddress { get; protected set; }

        public string TopShelfDescription { get; protected set; }
        public string TopShelfServiceName { get; protected set; }
        public string TopShelfDisplayName { get; protected set; }

        public string RootUrlAddress { get; protected set; }

        public JarvisRebusConfiguration RebusConfiguration { get; protected set; }   
        public string InputQueue { get; protected set; }
        public string ErrorQueue { get; protected set; }
    }
}