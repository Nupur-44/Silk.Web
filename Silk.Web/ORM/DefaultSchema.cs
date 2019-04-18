using System;
using System.Collections.Generic;
using System.Reflection;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Web.ORM
{
	public class DefaultSchema : Schema
	{
		private readonly Dictionary<Type, object> _primaryKeyReferenceFactories
			= new Dictionary<Type, object>();

		public DefaultSchema(IEnumerable<EntityModel> entityModels,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters) :
			base(entityModels, methodCallConverters)
		{
		}

		public PrimaryKeyEntityReferenceFactory<T> GetOrCreatePrimaryKeyReferenceFactory<T>()
			where T : class
		{
			var type = typeof(T);
			if (_primaryKeyReferenceFactories.TryGetValue(type, out var factory))
				return factory as PrimaryKeyEntityReferenceFactory<T>;

			lock (_primaryKeyReferenceFactories)
			{
				if (_primaryKeyReferenceFactories.TryGetValue(type, out factory))
					return factory as PrimaryKeyEntityReferenceFactory<T>;

				factory = CreatePrimaryKeyReferenceFactory<T>();
				_primaryKeyReferenceFactories.Add(type, factory);
				return factory as PrimaryKeyEntityReferenceFactory<T>;
			}
		}
	}
}
