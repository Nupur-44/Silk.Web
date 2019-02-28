using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Web.ObjectMapping
{
	public class WebOpenGraphEnumerableStream<TData> : IGraphWriterStream<TypeModel, PropertyInfoField>
	{
		private readonly List<StreamReaderWriter> _writers
			= new List<StreamReaderWriter>();
		private readonly WebObjectGraphReaderWriterBase _objectReaderWriter;
		private readonly IFieldPath<TypeModel, PropertyInfoField> _fieldPath;
		private readonly ITypeInstanceFactory _typeInstanceFactory;

		public WebOpenGraphEnumerableStream(WebObjectGraphReaderWriterBase objectReaderWriter,
			IFieldPath<TypeModel, PropertyInfoField> fieldPath,
			ITypeInstanceFactory typeInstanceFactory)
		{
			_objectReaderWriter = objectReaderWriter;
			_fieldPath = fieldPath;
			_typeInstanceFactory = typeInstanceFactory;
		}

		public IGraphWriter<TypeModel, PropertyInfoField> CreateNew()
		{
			var writer = new StreamReaderWriter(default(TData), _typeInstanceFactory, _fieldPath);
			_writers.Add(writer);
			return writer;
		}

		public void Dispose()
		{
			_objectReaderWriter.CommitEnumerable<TData>(_fieldPath, _writers.Select(q => q.Graph));
		}

		private class StreamReaderWriter : WebObjectGraphReaderWriterBase<TData>
		{
			public StreamReaderWriter(TData graph, ITypeInstanceFactory typeInstanceFactory, IFieldPath<TypeModel, PropertyInfoField> fieldPath) :
				base(graph, typeInstanceFactory, fieldPath)
			{
			}
		}
	}
}
