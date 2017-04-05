using Castle.Core.Logging;
using Castle.Windsor;
using Metrics;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json.Serialization;
using Owin;
using Owin.Metrics;
using Swashbuckle.Application;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using GestionePrenotazioni.Host.Support.Web;
using GestionePrenotazioni.Domain.Support;

namespace GestionePrenotazioni.Host.Support
{
    class GestionePrenotazioniApplication
    {
        private static GestionePrenotazioniConfiguration _config;
        private static IWindsorContainer _container;

        public static void SetConfig(GestionePrenotazioniConfiguration config)
        {
            _config = config;
        }

        public static void SetContainer(IWindsorContainer container)
        {
            _container = container;
        }

        public void Configuration(IAppBuilder application)
        {
            if (_config == null)
                throw new ApplicationException("Configuration is null, you forget to initialize GestionePrenotazioniApplication.SetConfig");

            ConfigureApi(application);
            ConfigureAdmin(application);

            Metric
                .Config
                .WithOwin(middleware => application.Use(middleware),
                           config => config
                    .WithRequestMetricsConfig(c => c.WithAllOwinMetrics())
                    .WithMetricsEndpoint(endpointConfig => endpointConfig
                        .MetricsEndpoint("metrics/metrics")
                        .MetricsTextEndpoint("metrics/text")
                        .MetricsHealthEndpoint("metrics/health")
                        .MetricsJsonEndpoint("metrics/json")
                        .MetricsPingEndpoint("metrics/ping")
                    ));
        }

        void ConfigureAdmin(IAppBuilder application)
        {
            var appFolder = FindAppRoot();

            var fileSystem = new PhysicalFileSystem(appFolder);

            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem,
                EnableDefaultFiles = true
            };

            application.UseFileServer(options);
        }

        static string FindAppRoot()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory
                .ToLowerInvariant()
                .Split(System.IO.Path.DirectorySeparatorChar)
                .ToList();

            while (true)
            {
                var last = root.Last();
                if (last == String.Empty || last == "debug" || last == "release" || last == "bin")
                {
                    root.RemoveAt(root.Count - 1);
                    continue;
                }

                break;
            }

            root.Add("app");

            var appFolder = String.Join("" + System.IO.Path.DirectorySeparatorChar, root);
            return appFolder;
        }

        private static void ConfigureJson(HttpConfiguration config)
        {
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.None;

            // Necessario per evitare che la UI di Swagger "impazzisca"
            jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }

        private static void ConfigureLogging(HttpConfiguration config)
        {
            var loggerFactory = _container.Resolve<IExtendedLoggerFactory>();

            config.Filters.Add(new LogFilterAttribute(loggerFactory.Create("LogFilter")));

            config.Services.Add(
                typeof(IExceptionLogger),
                new Log4NetExceptionLogger(loggerFactory)
            );
        }

        private static void ConfigureSwagger(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.OperationFilter<ExamplesOperationFilter>();
                    c.SingleApiVersion("v1", "GestionePrenotazioni Api");
                })
                .EnableSwaggerUi(c =>
                {
                });
        }

        private static void ConfigureWebApi(HttpConfiguration config) {
            // Abilita la gestione delle chiamate cors
            config.MapHttpAttributeRoutes();

            config.EnableCors(new EnableCorsAttribute("*", "*", "*", "Content-Disposition"));
        }

        static void ConfigureApi(IAppBuilder application)
        {
            var config = new HttpConfiguration
            {
                DependencyResolver = new WindsorResolver(_container),
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            ConfigureJson(config);

            ConfigureLogging(config);

            ConfigureSwagger(config);

            ConfigureWebApi(config);

            application.UseWebApi(config);

        }
    }
}