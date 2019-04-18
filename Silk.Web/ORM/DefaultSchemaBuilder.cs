using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Web.ORM
{
	public class DefaultSchemaBuilder : SchemaBuilder
	{
		private readonly ITypeInstanceFactory _typeInstanceFactory;

		public DefaultSchemaBuilder(ITypeInstanceFactory typeInstanceFactory)
		{
			_typeInstanceFactory = typeInstanceFactory;
		}

		public override Schema Build()
		{
			var primaryKeyReferenceRule = new PrimaryKeyEntityReferenceRule
			{
				TypeInstanceFactory = _typeInstanceFactory
			};

			EntityToTypeRules.Insert(
                0, primaryKeyReferenceRule
                );

            var schema = new DefaultSchema(
				BuildEntityModels(),
				MethodCallConverters
				);

            primaryKeyReferenceRule.Schema = schema;

            return schema;
		}
	}
}
