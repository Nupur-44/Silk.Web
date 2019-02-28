using Silk.Data.SQL.ORM.Schema;

namespace Silk.Web.ORM
{
	public class DefaultSchemaBuilder : SchemaBuilder
	{
		public override Schema Build()
		{
			return new DefaultSchema(
				BuildEntityModels(),
				MethodCallConverters
				);
		}
	}
}
