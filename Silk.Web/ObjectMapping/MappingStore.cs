using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using System;
using System.Collections.Generic;

namespace Silk.Web.ObjectMapping
{
	public class MappingStore
	{
		private readonly IIntersectionAnalyzer<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> _intersectionAnalyzer;
		private readonly IMappingFactory<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> _mappingFactory;
		private readonly Dictionary<Type, FromTypeMappings> _fromTypeMappings = new Dictionary<Type, FromTypeMappings>();

		public MappingStore(
			IIntersectionAnalyzer<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> intersectionAnalyzer = null,
			IMappingFactory<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> mappingFactory = null
			)
		{
			_intersectionAnalyzer = intersectionAnalyzer ?? new TypeToTypeIntersectionAnalyzer();
			_mappingFactory = mappingFactory ?? new TypeToTypeMappingFactory();
		}

		public IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> GetMapping<TFrom, TTo>()
			where TTo : class
		{
			var fromType = typeof(TFrom);
			if (_fromTypeMappings.TryGetValue(fromType, out var fromMappings))
				return fromMappings.GetMapping<TTo>();

			lock (_fromTypeMappings)
			{
				if (_fromTypeMappings.TryGetValue(fromType, out fromMappings))
					return fromMappings.GetMapping<TTo>();

				fromMappings = new FromTypeMappings<TFrom>(_intersectionAnalyzer, _mappingFactory);
				_fromTypeMappings.Add(fromType, fromMappings);
			}

			return fromMappings.GetMapping<TTo>();
		}

		private abstract class FromTypeMappings
		{
			public abstract IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> GetMapping<TTo>();
		}

		private class FromTypeMappings<TFrom> : FromTypeMappings
		{
			private readonly TypeModel<TFrom> _fromModel = TypeModel.GetModelOf<TFrom>();
			private readonly Dictionary<Type, IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>> _mappings
				= new Dictionary<Type, IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>>();
			private readonly IIntersectionAnalyzer<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> _intersectionAnalyzer;
			private readonly IMappingFactory<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> _mappingFactory;

			public FromTypeMappings(
				IIntersectionAnalyzer<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> intersectionAnalyzer,
				IMappingFactory<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> mappingFactory
				)
			{
				_intersectionAnalyzer = intersectionAnalyzer;
				_mappingFactory = mappingFactory;
			}

			public override IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> GetMapping<TTo>()
			{
				var type = typeof(TTo);
				if (_mappings.TryGetValue(type, out var mapping))
					return mapping;

				lock (_mappings)
				{
					if (_mappings.TryGetValue(type, out mapping))
						return mapping;

					mapping = CreateMapping<TTo>();
					return mapping;
				}
			}

			private IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> CreateMapping<TTo>()
			{
				var intersection = _intersectionAnalyzer.CreateIntersection(
					_fromModel,
					TypeModel.GetModelOf<TTo>()
					);
				return _mappingFactory.CreateMapping(intersection);
			}
		}
	}
}
