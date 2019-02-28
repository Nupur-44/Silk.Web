using Microsoft.Extensions.DependencyInjection;
using Silk.Data.Modelling;
using Silk.Web.ObjectMapping;
using System.Linq;

namespace Silk.Web
{
	public static class ObjectModellingStartupExtensions
	{
		/// <summary>
		/// Adds the IObjectMapper service.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddObjectMapper(this IServiceCollection services)
		{
			if (!services.Any(q => q.ServiceType == typeof(ITypeInstanceFactory)))
				services.AddScoped<ITypeInstanceFactory, ServiceProviderTypeFactory>();

			if (!services.Any(q => q.ServiceType == typeof(IReaderWriterFactory<TypeModel, PropertyInfoField>)))
				services.AddScoped<IReaderWriterFactory<TypeModel, PropertyInfoField>, WebReaderWriterFactory>();

			services.AddSingleton(sP => new MappingStore());
			services.AddScoped<IObjectMapper, WebObjectMapper>();
			return services;
		}
	}
}
