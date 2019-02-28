using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Silk.Web
{
	public static class StartupExtensions
	{
		/// <summary>
		/// Adds Silk services.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddSilk(this IServiceCollection services)
		{
			services.AddObjectMapper();
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
