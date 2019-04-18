using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silk.Data;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Web.ORM
{
	public class ORMEntityModelBindingProvider : IModelBinderProvider
	{
		private bool _isReady;

		private readonly Dictionary<Type, IModelBinder> _typeBinders = new Dictionary<Type, IModelBinder>();

		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (!_isReady)
				Initialize(context.Services);

			if (_typeBinders.TryGetValue(context.Metadata.ModelType, out var binder))
			{
				return binder;
			}

			return null;
		}

		private void Initialize(IServiceProvider services)
		{
			_isReady = true;
			var ormOptions = services.GetRequiredService<IOptions<ORMOptions>>().Value;
			var schema = services.GetRequiredService<DefaultSchema>();
			var builder = new BinderBuilder(schema);
			foreach (var entityConfig in ormOptions.ORMEntityConfigurations)
			{
				entityConfig.Dispatch(builder);
				if (builder.Binder != null)
					_typeBinders.Add(builder.EntityReferenceType, builder.Binder);
			}
		}

		private class BinderBuilder : IORMEntityExecutor
		{
			private readonly Schema _schema;

			public IModelBinder Binder { get; private set; }
			public Type EntityReferenceType { get; private set; }

			public BinderBuilder(Schema schema)
			{
				_schema = schema;
			}

			public void Execute<T>() where T : class
			{
				Binder = null;
				EntityReferenceType = typeof(IEntityReference<T>);

				var entityModel = _schema.GetEntityModel<T>();
				var primaryKeyFields = entityModel.Fields.Where(q => q.IsPrimaryKey).ToArray();

				if (primaryKeyFields.Length == 1)
				{
					Binder = new PrimaryKeyEntityBinder<T>(
						_schema.CreatePrimaryKeyReferenceFactory<T>()
						);
				}
			}
		}

		private class PrimaryKeyEntityBinder<TEntity> : IModelBinder
			where TEntity : class
		{
			private readonly PrimaryKeyEntityReferenceFactory<TEntity> _entityReferenceFactory;

			public PrimaryKeyEntityBinder(
				PrimaryKeyEntityReferenceFactory<TEntity> entityReferenceFactory
				)
			{
				_entityReferenceFactory = entityReferenceFactory;
			}

			public Task BindModelAsync(ModelBindingContext bindingContext)
			{
				if (bindingContext == null)
				{
					throw new ArgumentNullException(nameof(bindingContext));
				}

				var modelName = bindingContext.ModelName;

				// Try to fetch the value of the argument by name
				var valueProviderResult =
					bindingContext.ValueProvider.GetValue(modelName);

				if (valueProviderResult == ValueProviderResult.None)
				{
					return Task.CompletedTask;
				}

				bindingContext.ModelState?.SetModelValue(modelName,
					valueProviderResult);

				var value = valueProviderResult.FirstValue;

				// Check if the argument value is null or empty
				if (string.IsNullOrEmpty(value))
				{
					return Task.CompletedTask;
				}

				var entityReference = _entityReferenceFactory.Create(
					bindingContext.HttpContext.RequestServices.GetRequiredService<ITypeInstanceFactory>(),
					value
					);

				if (entityReference == null)
				{
					// Non-integer arguments result in model state errors
					bindingContext.ModelState.TryAddModelError(
											modelName,
											$"Couldn't convert '{value}' entity reference.");
					return Task.CompletedTask;
				}

				bindingContext.Result = ModelBindingResult.Success(entityReference);
				return Task.CompletedTask;
			}
		}
	}
}
