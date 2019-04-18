using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;

namespace Silk.Web.ObjectMapping
{
	public class WebReaderWriterFactory : IReaderWriterFactory<TypeModel, PropertyInfoField>
	{
		private readonly ITypeInstanceFactory _typeInstanceFactory;

		public WebReaderWriterFactory(ITypeInstanceFactory typeInstanceFactory)
		{
			_typeInstanceFactory = typeInstanceFactory;
		}

		public IGraphReader<TypeModel, PropertyInfoField> CreateGraphReader<T>(T graph) 
			=> new ObjectGraphReader<T>(graph);

		public IGraphWriter<TypeModel, PropertyInfoField> CreateGraphWriter<T>(T graph) where T : class
			=> new WebObjectGraphReaderWriter<T>(graph, _typeInstanceFactory);
	}
}
