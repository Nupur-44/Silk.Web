using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Silk.Data.Modelling;
using Silk.Web;
using Silk.Web.ObjectMapping;
using Silk.Web.ORM;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
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

		public static IServiceCollection AddTypeConverter<TFrom, TTo>(this IServiceCollection services, ITypeConverter<TFrom, TTo> typeConverter)
		{
			services.AddSingleton<ITypeConverter>(typeConverter);
			return services;
		}

		public static IServiceCollection AddTypeConverter<TConverter>(this IServiceCollection services)
			where TConverter : class, ITypeConverter
		{
			services.AddSingleton<ITypeConverter, TConverter>();
			return services;
		}

		/// <summary>
		/// Adds Silk services.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddSilk(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
			services.AddScoped(UrlHelperFactory);
			services.AddObjectMapper();
			services.AddORM();
			services.AddMvc(options =>
			{
				options.ModelBinderProviders.Insert(0, new ORMEntityModelBindingProvider());
			}).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			return services;
		}

		/// <summary>
		/// Adds Silk middleware to the HTTP pipeline. Includes MVC and RazorPages.
		/// </summary>
		/// <param name="applicationBuilder"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseSilk(this IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseStaticFiles();
			applicationBuilder.UseMvcWithDefaultRoute();
			return applicationBuilder;
		}

		private static IUrlHelper UrlHelperFactory(IServiceProvider serviceProvider)
		{
			var factory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
			var context = serviceProvider.GetRequiredService<IActionContextAccessor>().ActionContext;
			return factory.GetUrlHelper(context);
		}
	}
}
