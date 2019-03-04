using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.SQLite3;
using Silk.Web.ORM;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ORMStartupExtensions
	{
		/// <summary>
		/// Registers the Silk ORM services.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddORM(this IServiceCollection services)
		{
			services.AddDefaultMappingDependencyServices();

			if (!services.Any(q => q.ServiceType == typeof(IDataProvider)))
				services.AddSingleton<IDataProvider>(new SQLite3DataProvider(new Uri("database.db", UriKind.Relative), nonBinaryGUIDs: true));

			//  default schema
			services.AddSingleton(BuildDefaultSchema);
			services.AddSingleton<Schema>(sP => sP.GetRequiredService<DefaultSchema>());

			return services;
		}

		/// <summary>
		/// Adds an entity type to the ORM.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="services"></param>
		/// <param name="configureEntity"></param>
		/// <returns></returns>
		public static IServiceCollection AddORMEntity<T>(this IServiceCollection services,
			Action<EntityDefinition<T>> configureEntity = null)
			where T : class
		{
			services.Configure<ORMOptions>(options => options.AddEntity(configureEntity));
			services.AddScoped<IEntityTable<T>>(BuildEntityTable<T>);
			services.AddScoped<ISqlEntityStore<T>>(BuildEntityStore<T>);
			return services;
		}

		public static IApplicationBuilder CreateORMSchema(this IApplicationBuilder application)
		{
			using (var scope = application.ApplicationServices.CreateScope())
			{
				var creator = ActivatorUtilities.CreateInstance<ORMSchemaCreator>(
					scope.ServiceProvider
					);
				creator.CreateSchema();
			}
			return application;
		}

		private static IEntityTable<T> BuildEntityTable<T>(IServiceProvider serviceProvider)
			where T : class
		{
			return new EntityTable<T>(
				serviceProvider.GetRequiredService<DefaultSchema>(),
				serviceProvider.GetRequiredService<IDataProvider>()
				);
		}

		private static ISqlEntityStore<T> BuildEntityStore<T>(IServiceProvider serviceProvider)
			where T : class
		{
			return new SqlEntityStore<T>(
				serviceProvider.GetRequiredService<DefaultSchema>(),
				serviceProvider.GetRequiredService<IDataProvider>(),
				serviceProvider.GetRequiredService<ITypeInstanceFactory>(),
				serviceProvider.GetRequiredService<IReaderWriterFactory<TypeModel, PropertyInfoField>>()
				);
		}

		private static DefaultSchema BuildDefaultSchema(IServiceProvider serviceProvider)
		{
			var ormOptions = serviceProvider.GetRequiredService<IOptions<ORMOptions>>().Value;
			var builder = new DefaultSchemaBuilder();

			foreach (var entityConfig in ormOptions.ORMEntityConfigurations)
			{
				entityConfig.ApplyTo(builder);
			}

			return builder.Build() as DefaultSchema;
		}
	}
}
