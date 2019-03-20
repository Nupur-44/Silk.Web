using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Silk.Web.ORM
{
	public class ORMOptions
	{
		public List<ORMEntityConfiguration> ORMEntityConfigurations { get; }
			= new List<ORMEntityConfiguration>();

		public Dictionary<MethodInfo, IMethodCallConverter> MethodCallConverters { get; }
			= new Dictionary<MethodInfo, IMethodCallConverter>();

		public void AddEntity<T>(Action<EntityDefinition<T>> configure = null)
			where T : class
		{
			ORMEntityConfigurations.Add(new ORMEntityConfiguration<T>(configure));
		}
	}
}
