using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using System.Collections.Generic;

namespace Silk.Web.ObjectMapping
{
	public class WebObjectGraphReaderEnumerator<TData> : IGraphReaderEnumerator<TypeModel, PropertyInfoField>
	{
		private readonly IEnumerator<TData> _enumerator;
		private readonly IFieldPath<TypeModel, PropertyInfoField> _fieldPath;
		private readonly ITypeInstanceFactory _typeInstanceFactory;

		public IGraphReader<TypeModel, PropertyInfoField> Current { get; private set; }

		public WebObjectGraphReaderEnumerator(IEnumerator<TData> enumerator, IFieldPath<TypeModel, PropertyInfoField> fieldPath,
			ITypeInstanceFactory typeInstanceFactory)
		{
			_enumerator = enumerator;
			_fieldPath = fieldPath;
			_typeInstanceFactory = typeInstanceFactory;
		}

		public void Dispose()
		{
			_enumerator.Dispose();
		}

		public bool MoveNext()
		{
			var ok = _enumerator.MoveNext();
			if (!ok)
				return false;

			Current = new EnumeratorReaderWriter(_enumerator.Current, _typeInstanceFactory, _fieldPath);
			return true;
		}

		private class EnumeratorReaderWriter : WebObjectGraphReaderWriterBase<TData>
		{
			public EnumeratorReaderWriter(TData graph, ITypeInstanceFactory typeInstanceFactory, IFieldPath<TypeModel, PropertyInfoField> fieldPath) :
				base(graph, typeInstanceFactory, fieldPath)
			{
			}
		}
	}
}
