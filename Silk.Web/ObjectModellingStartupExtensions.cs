using Microsoft.Extensions.DependencyInjection;
using Silk.Data.Modelling;
using Silk.Web.ObjectMapping;

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
			services.AddDefaultMappingDependencyServices();

			services.AddSingleton(sP => new MappingStore());
			services.AddScoped<IObjectMapper, WebObjectMapper>();
			return services;
		}
	}
}
