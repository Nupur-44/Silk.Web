using Microsoft.Extensions.DependencyInjection;
using Silk.Data.Modelling;
using System;

namespace Silk.Web
{
	public class ServiceProviderTypeFactory : ITypeInstanceFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public ServiceProviderTypeFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public T CreateInstance<T>()
			=> ActivatorUtilities.CreateInstance<T>(_serviceProvider);
	}
}
