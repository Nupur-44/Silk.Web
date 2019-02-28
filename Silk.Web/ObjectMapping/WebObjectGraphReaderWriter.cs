using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Web.ObjectMapping
{
	public abstract class WebObjectGraphReaderWriterBase
	{
		public abstract void CommitEnumerable<T>(IFieldPath<TypeModel, PropertyInfoField> fieldPath,
			IEnumerable<T> enumerable);
	}

	public abstract class WebObjectGraphReaderWriterBase<TGraph> : WebObjectGraphReaderWriterBase,
		IGraphReader<TypeModel, PropertyInfoField>,
		IGraphWriter<TypeModel, PropertyInfoField>
	{
		private static readonly ObjectGraphPropertyAccessor<TGraph> _propertyAccessor =
			ObjectGraphPropertyAccessor.GetFor<TGraph>();

		private readonly IFieldPath<TypeModel, PropertyInfoField> _fieldPath;
		public TGraph Graph { get; private set; }
		private readonly ITypeInstanceFactory _typeInstanceFactory;

		public WebObjectGraphReaderWriterBase(
			TGraph graph,
			ITypeInstanceFactory typeInstanceFactory,
			IFieldPath<TypeModel, PropertyInfoField> fieldPath
			)
		{
			Graph = graph;
			_typeInstanceFactory = typeInstanceFactory;
			_fieldPath = fieldPath;
		}

		public bool CheckContainer(IFieldPath<TypeModel, PropertyInfoField> fieldPath)
		{
			var checker = _propertyAccessor.GetPropertyChecker(fieldPath, skipLastField: false,
				pathOffset: _fieldPath?.Fields.Count ?? 0);
			return checker(Graph);
		}

		public bool CheckPath(IFieldPath<TypeModel, PropertyInfoField> fieldPath)
		{
			var checker = _propertyAccessor.GetPropertyChecker(fieldPath, skipLastField: true,
				pathOffset: _fieldPath?.Fields.Count ?? 0);
			return checker(Graph);
		}

		public void CreateContainer(IFieldPath<TypeModel, PropertyInfoField> fieldPath)
		{
			var containerCreator = GetContainerCreator(fieldPath, _fieldPath?.Fields.Count ?? 0);
			Graph = containerCreator(Graph, _typeInstanceFactory);
		}

		public IGraphWriterStream<TypeModel, PropertyInfoField> CreateEnumerableStream<T>(IFieldPath<TypeModel, PropertyInfoField> fieldPath)
		{
			return new WebOpenGraphEnumerableStream<T>(this, fieldPath, _typeInstanceFactory);
		}

		public IGraphReaderEnumerator<TypeModel, PropertyInfoField> GetEnumerator<T>(IFieldPath<TypeModel, PropertyInfoField> fieldPath)
		{
			var reader = _propertyAccessor.GetEnumerableReader<T>(fieldPath, _fieldPath?.Fields.Count ?? 0);
			var enumerable = reader(Graph);
			return new WebObjectGraphReaderEnumerator<T>(enumerable.GetEnumerator(), fieldPath, _typeInstanceFactory);
		}

		public T Read<T>(IFieldPath<TypeModel, PropertyInfoField> fieldPath)
		{
			var reader = _propertyAccessor.GetPropertyReader<T>(fieldPath, _fieldPath?.Fields.Count ?? 0);
			return reader(Graph);
		}

		public void Write<T>(IFieldPath<TypeModel, PropertyInfoField> fieldPath, T value)
		{
			var writer = _propertyAccessor.GetPropertyWriter<T>(fieldPath, _fieldPath?.Fields.Count ?? 0);
			Graph = writer(Graph, value);
		}

		public override void CommitEnumerable<T>(IFieldPath<TypeModel, PropertyInfoField> fieldPath,
			IEnumerable<T> enumerable)
		{
			var writer = _propertyAccessor.GetEnumerableWriter<T>(fieldPath, _fieldPath?.Fields.Count ?? 0);
			writer(Graph, enumerable);
		}

		private static readonly MethodInfo _createInstanceMethod = typeof(ITypeInstanceFactory)
			.GetMethod(nameof(ITypeInstanceFactory.CreateInstance));
		private static readonly Dictionary<string, Delegate> _containerCreators
			= new Dictionary<string, Delegate>();

		private static Func<TGraph, ITypeInstanceFactory, TGraph> GetContainerCreator(IFieldPath<PropertyInfoField> fieldPath, int pathOffset = 0)
		{
			var flattenedPath = string.Join(".", fieldPath.Fields.Select(field => field.FieldName));

			if (_containerCreators.TryGetValue(flattenedPath, out var @delegate))
				return @delegate as Func<TGraph, ITypeInstanceFactory, TGraph>;

			lock (_containerCreators)
			{
				if (_containerCreators.TryGetValue(flattenedPath, out @delegate))
					return @delegate as Func<TGraph, ITypeInstanceFactory, TGraph>;

				@delegate = CreateContainerCreator(fieldPath, pathOffset);
				_containerCreators.Add(flattenedPath, @delegate);
				return @delegate as Func<TGraph, ITypeInstanceFactory, TGraph>;
			}
		}

		private static Func<TGraph, ITypeInstanceFactory, TGraph> CreateContainerCreator(IFieldPath<PropertyInfoField> fieldPath, int pathOffset)
		{
			var graph = Expression.Parameter(typeof(TGraph), "graph");
			var factory = Expression.Parameter(typeof(ITypeInstanceFactory), "factory");

			Expression property = graph;

			foreach (var field in fieldPath.Fields.Skip(pathOffset))
				property = Expression.Property(property, field.FieldName);

			var lambda = Expression.Lambda<Func<TGraph, ITypeInstanceFactory, TGraph>>(
				Expression.Block(
					Expression.Assign(property, Expression.Call(factory, _createInstanceMethod.MakeGenericMethod(
						fieldPath.FinalField.RemoveEnumerableType()
						))),
					graph
					),
				graph, factory
				);
			return lambda.Compile();
		}
	}

	public class WebObjectGraphReaderWriter<TGraph> :
		WebObjectGraphReaderWriterBase<TGraph>
		where TGraph : class
	{
		public WebObjectGraphReaderWriter(TGraph graph, ITypeInstanceFactory typeInstanceFactory) :
			base(graph, typeInstanceFactory, null)
		{
		}
	}
}
