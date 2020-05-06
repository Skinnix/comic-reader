using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skinnix.ComicReader.Services
{
    public interface IStartStopCollector : IStartService, IStopService
    {
        IEnumerable<IStartService> GetStartServices();
        IEnumerable<IStopService> GetStopServices();
    }

    internal class StartStopCollector : IStartStopCollector
    {
        private readonly IServiceCollection services;
        private readonly IServiceProvider provider;

        public ServiceState State { get; private set; }

        internal StartStopCollector(IServiceCollection services, IServiceProvider provider)
        {
            this.services = services;
            this.provider = provider;
        }

        private IEnumerable<T> GetServices<T>()
        {
            foreach(ServiceDescriptor service in services)
            {
                if (service.Lifetime == ServiceLifetime.Singleton)
                {
                    if (service.ImplementationInstance is T s)
                        yield return s;
                    else if (typeof(T).IsAssignableFrom(service.ServiceType) || typeof(T).IsAssignableFrom(service.ImplementationType))
                        yield return (T)provider.GetService(service.ServiceType);
                }
            }
        }

        public IEnumerable<IStartService> GetStartServices() => GetServices<IStartService>();
        public IEnumerable<IStopService> GetStopServices() => GetServices<IStopService>();

        public async Task Start()
        {
            if (State != ServiceState.Idle)
                return;
            State = ServiceState.Starting;

            var tasks = new LinkedList<Task>();
            foreach (var service in GetStartServices())
                tasks.AddLast(service.Start());

            await Task.WhenAll(tasks);
            State = ServiceState.Running;
        }

        public async Task Stop()
        {
            if (State != ServiceState.Running)
                return;
            State = ServiceState.Stopping;

            var tasks = new LinkedList<Task>();
            foreach (var service in GetStopServices())
                tasks.AddLast(service.Stop());

            await Task.WhenAll(tasks);
            State = ServiceState.Idle;
        }
    }

    public static class StartStopCollectorExtensions
    {
        public static IServiceCollection AddStartStop(this IServiceCollection services)
            => services.AddSingleton<IStartStopCollector>(provider => new StartStopCollector(services, provider));

		public static Task Start(this IServiceProvider provider)
			=> provider.GetService<IStartStopCollector>().Start();
	}
}
