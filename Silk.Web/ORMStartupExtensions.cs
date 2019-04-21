using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Silk.Data.Modelling;
using Silk.Data.SQL.MSSQL;
using Silk.Data.SQL.MySQL;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Postgresql;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.SQLite3;
using Silk.Web;
using Silk.Web.ORM;
using Silk.Web.TypeConverters;
using System;
using System.Collections.Generic;
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

		public static IServiceCollection ConfigureDataProvider(this IServiceCollection services, IConfigurationSection config)
		{
			switch (config["provider"].ToLowerInvariant())
			{
				case "sqlite3":
				case "sqlite":
					return services.ConfigureDataProvider(new SQLite3DataProvider(
						new Uri(config["file"], UriKind.Relative),
						nonBinaryGUIDs: true
						));
				case "pgsql":
				case "postgres":
				case "postgresql":
					return services.ConfigureDataProvider(new PostgresqlDataProvider(
						config["host"], config["database"], config["user"], config["pass"]
						));
				case "mysql":
				case "mariadb":
					return services.ConfigureDataProvider(new MySQLDataProvider(
						config["host"], config["database"], config["user"], config["pass"]
						));
				case "mssql":
				case "sqlserver":
					return services.ConfigureDataProvider(new MSSqlDataProvider(
						config["host"], config["database"], config["user"], config["pass"]
						));
			}
			throw new Exception("Unrecognized data configuration.");
		}

		public static IServiceCollection ConfigureDataProvider(this IServiceCollection services, IDataProvider dataProvider)
		{
			var existingService = services.FirstOrDefault(q => q.ServiceType == typeof(IDataProvider));
			while (existingService != null)
			{
				services.Remove(existingService);
				existingService = services.FirstOrDefault(q => q.ServiceType == typeof(IDataProvider));
			}
			services.AddSingleton<IDataProvider>(dataProvider);
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
			services.AddSingleton<ITypeConverter, EntityReferenceTypeConverter<T>>();
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
			var typeConverters = serviceProvider.GetService<IEnumerable<ITypeConverter>>()?.ToArray();
			var ormOptions = serviceProvider.GetRequiredService<IOptions<ORMOptions>>().Value;
			var builder = new DefaultSchemaBuilder(DefaultTypeInstanceFactory.Instance);

			if (typeConverters != null && typeConverters.Length > 0)
				builder.AddTypeConverters(typeConverters);

			foreach (var kvp in ormOptions.MethodCallConverters)
				builder.AddMethodConverter(kvp.Key, kvp.Value);

			foreach (var entityConfig in ormOptions.ORMEntityConfigurations)
			{
				entityConfig.ApplyTo(builder);
			}

			return builder.Build() as DefaultSchema;
		}
	}
}
