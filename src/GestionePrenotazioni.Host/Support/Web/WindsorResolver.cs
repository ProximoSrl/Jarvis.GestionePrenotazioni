using Castle.MicroKernel;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace GestionePrenotazioni.Host.Support.Web
{
    public class WindsorResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly IWindsorContainer _container;


        public WindsorResolver(IWindsorContainer container)
        {
            _container = container;
        }



        public IDependencyScope BeginScope()
        {
            return new WindsorDependencyScope(_container);
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public object GetService(Type serviceType)
        {
            if (!_container.Kernel.HasComponent(serviceType))
                return null;

            return _container.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (!_container.Kernel.HasComponent(serviceType))
                return new object[0];

            return _container.ResolveAll(serviceType).Cast<object>();
        }
    }
}