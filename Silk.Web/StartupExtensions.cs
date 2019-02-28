using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Silk.Data.Modelling;
using Silk.Web.ObjectMapping;
using System.Linq;

namespace Silk.Web
{
	public static class StartupExtensions
	{
		internal static IServiceCollection AddDefaultMappingDependencyServices(this IServiceCollection services)
		{
			if (!services.Any(q => q.ServiceType == typeof(ITypeInstanceFactory)))
				services.AddScoped<ITypeInstanceFactory, ServiceProviderTypeFactory>();

			if (!services.Any(q => q.ServiceType == typeof(IReaderWriterFactory<TypeModel, PropertyInfoField>)))
				services.AddScoped<IReaderWriterFactory<TypeModel, PropertyInfoField>, WebReaderWriterFactory>();

			return services;
		}

		/// <summary>
		/// Adds Silk services.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddSilk(this IServiceCollection services)
		{
			services.AddObjectMapper();
			services.AddORM();
			services.AddMvc();

			return services;
		}

		/// <summary>
		/// Adds Silk middleware to the HTTP pipeline. Includes MVC and RazorPages.
		/// </summary>
		/// <param name="applicationBuilder"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseSilk(this IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseMvcWithDefaultRoute();
			return applicationBuilder;
		}
	}
}
