using GestionePrenotazioni.Domain.Support;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace GestionePrenotazioni.Host.Support
{
    public class AppConfigGestionePrenotazioniConfiguration
        : GestionePrenotazioniConfiguration
    {
        public AppConfigGestionePrenotazioniConfiguration()
        {
            MainDatabaseConnectionString = ConfigurationManager.ConnectionStrings["mainDb"].ConnectionString;
            LogsConnectionString = ConfigurationManager.ConnectionStrings["logDb"].ConnectionString;
            RebusConnectionString = ConfigurationManager.ConnectionStrings["rebusDb"].ConnectionString;
            ServerAddress = ConfigurationManager.AppSettings["ServerAddress"];

            RebuildEnabled = "true".Equals(ConfigurationManager.AppSettings["RebuildEnabled"], StringComparison.OrdinalIgnoreCase);

            TopShelfDescription = ConfigurationManager.AppSettings["TopShelf.Description"];
            TopShelfServiceName = ConfigurationManager.AppSettings["TopShelf.ServiceName"];
            TopShelfDisplayName = ConfigurationManager.AppSettings["TopShelf.DisplayName"];

            RootUrlAddress = ConfigurationManager.AppSettings["RootUrlAddress"];

            InputQueue = ConfigurationManager.AppSettings["InputQueue"];
            ErrorQueue = ConfigurationManager.AppSettings["ErrorQueue"];
            RebusConfiguration = new Jarvis.Framework.Bus.Rebus.Integration.Support.JarvisRebusConfiguration();
            RebusConfiguration.InputQueue = InputQueue;
            RebusConfiguration.ErrorQueue = ErrorQueue;
            RebusConfiguration.EndpointsMap = new Dictionary<string, string>();
            RebusConfiguration.EndpointsMap.Add("GestionePrenotazioni.Domain", InputQueue);
            RebusConfiguration.MaxRetry = 5;
            RebusConfiguration.NumOfWorkers = 4;                       
        }
    }
}