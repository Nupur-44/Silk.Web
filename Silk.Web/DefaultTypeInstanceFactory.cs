using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Silk.Web
{
	public class DefaultTypeInstanceFactory : ITypeInstanceFactory
	{
		public T CreateInstance<T>()
		{
			var factory = GetFactory<T>();
			return factory();
		}

		private static readonly Dictionary<Type, Delegate> _factories
			= new Dictionary<Type, Delegate>();

		public static DefaultTypeInstanceFactory Instance { get; }
			= new DefaultTypeInstanceFactory();

		private static ConstructorInfo GetParameterlessConstructor(Type type)
		{
			return type
				.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.FirstOrDefault(ctor => ctor.GetParameters().Length == 0);
		}

		private static Func<T> GetFactory<T>()
		{
			var type = typeof(T);
			if (_factories.TryGetValue(type, out var factory))
				return factory as Func<T>;

			lock (_factories)
			{
				if (_factories.TryGetValue(type, out factory))
					return factory as Func<T>;

				factory = CreateFactory<T>();
				_factories.Add(type, factory);
				return factory as Func<T>;
			}
		}

		private static Func<T> CreateFactory<T>()
		{
			var ctor = GetParameterlessConstructor(typeof(T));
			if (ctor == null)
				throw new InvalidOperationException($"{typeof(T).FullName} doesn't have a parameterless constructor.");

			var lambda = Expression.Lambda<Func<T>>(
				Expression.New(ctor)
				);
			return lambda.Compile();
		}
	}
}
