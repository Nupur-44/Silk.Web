using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Web.ORM
{
	public abstract class ORMEntityConfiguration
	{
		public abstract Type EntityType { get; }
		public abstract void ApplyTo(SchemaBuilder schemaBuilder);
		public abstract void Dispatch(IORMEntityExecutor executor);
	}

	public class ORMEntityConfiguration<T> : ORMEntityConfiguration
		where T : class
	{
		private readonly Action<EntityDefinition<T>> _configure;

		public ORMEntityConfiguration(Action<EntityDefinition<T>> configure)
		{
			_configure = configure;
		}

		public override Type EntityType => typeof(T);

		public override void ApplyTo(SchemaBuilder schemaBuilder)
		{
			schemaBuilder.Define(_configure);
		}

		public override void Dispatch(IORMEntityExecutor executor)
			=> executor.Execute<T>();
	}

	public interface IORMEntityExecutor
	{
		void Execute<T>() where T : class;
	}
}
