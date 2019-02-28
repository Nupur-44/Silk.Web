using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Web.ORM
{
	public abstract class ORMEntityConfiguration
	{
		public abstract void ApplyTo(SchemaBuilder schemaBuilder);
	}

	public class ORMEntityConfiguration<T> : ORMEntityConfiguration
		where T : class
	{
		private readonly Action<EntityDefinition<T>> _configure;

		public ORMEntityConfiguration(Action<EntityDefinition<T>> configure)
		{
			_configure = configure;
		}

		public override void ApplyTo(SchemaBuilder schemaBuilder)
		{
			schemaBuilder.Define(_configure);
		}
	}
}
