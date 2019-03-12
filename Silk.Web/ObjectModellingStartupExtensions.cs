using Microsoft.Extensions.Options;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using Silk.Web.ObjectMapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
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

			services.AddSingleton(MappingStoreFactory);
			services.AddScoped<IObjectMapper, WebObjectMapper>();
			return services;
		}

		public static IServiceCollection AddObjectMapperRule(this IServiceCollection services,
			IIntersectionRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> rule)
		{
			services.Configure<MappingOptions>(
				options => options.IntersectionRules.Add(rule)
				);
			return services;
		}

		public static IServiceCollection AddObjectMapperFieldSource(this IServiceCollection services,
			IIntersectCandidateSource<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> source)
		{
			services.Configure<MappingOptions>(
				options => options.IntersectCandidateSources.Add(source)
				);
			return services;
		}

		private static MappingStore MappingStoreFactory(IServiceProvider serviceProvider)
		{
			var typeConverters = serviceProvider.GetService<IEnumerable<ITypeConverter>>()?.ToArray();
			var mappingOptions = serviceProvider.GetRequiredService<IOptions<MappingOptions>>().Value;

			var analyzer = new DefaultIntersectionAnalyzer<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>(
				mappingOptions.IntersectCandidateSources, mappingOptions.IntersectionRules
				);
			if (typeConverters != null && typeConverters.Length > 0)
				analyzer.AddTypeConverters(typeConverters);

			return new MappingStore(analyzer);
		}
	}
}
