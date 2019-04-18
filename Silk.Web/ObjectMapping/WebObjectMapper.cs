using System.Collections.Generic;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;

namespace Silk.Web.ObjectMapping
{
	public class WebObjectMapper : IObjectMapper
	{
		private readonly MappingStore _mappingStore;
		private readonly ITypeInstanceFactory _typeInstanceFactory;
		private readonly IReaderWriterFactory<TypeModel, PropertyInfoField> _readerWriterFactory;

		public WebObjectMapper(
			MappingStore mappingStore,
			ITypeInstanceFactory typeInstanceFactory,
			IReaderWriterFactory<TypeModel, PropertyInfoField> readerWriterFactory
			)
		{
			_mappingStore = mappingStore;
			_typeInstanceFactory = typeInstanceFactory;
			_readerWriterFactory = readerWriterFactory;
		}

		private IMapping<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField> GetMapping<TFrom, TTo>()
			where TTo : class
			=> _mappingStore.GetMapping<TFrom, TTo>();

		public void Inject<TFrom, TTo>(TFrom from, TTo to)
			where TTo : class
		{
			var reader = _readerWriterFactory.CreateGraphReader<TFrom>(from);
			var writer = _readerWriterFactory.CreateGraphWriter<TTo>(to);
			var mapping = GetMapping<TFrom, TTo>();
			mapping.Map(reader, writer);
		}

		public void InjectAll<TFrom, TTo>(IEnumerable<TFrom> from, IEnumerable<TTo> to)
			where TTo : class
		{
			var mapping = GetMapping<TFrom, TTo>();

			using (var fromEnumerator = from.GetEnumerator())
			using (var toEnumerator = to.GetEnumerator())
			{
				while (fromEnumerator.MoveNext() && toEnumerator.MoveNext())
				{
					var reader = _readerWriterFactory.CreateGraphReader<TFrom>(fromEnumerator.Current);
					var writer = _readerWriterFactory.CreateGraphWriter<TTo>(toEnumerator.Current);
					mapping.Map(reader, writer);
				}
			}
		}

		public TTo Map<TFrom, TTo>(TFrom from)
			where TTo : class
		{
			var graph = _typeInstanceFactory.CreateInstance<TTo>();
			Inject(from, graph);
			return graph;
		}

		public IEnumerable<TTo> MapAll<TFrom, TTo>(IEnumerable<TFrom> from)
			where TTo : class
		{
			foreach (var obj in from)
				yield return Map<TFrom, TTo>(obj);
		}
	}
}
