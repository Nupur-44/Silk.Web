using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silk.Data;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.GenericDispatch;
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
					BuildPrimaryKeyReferenceBinder(entityModel, primaryKeyFields[0]);
			}

			private void BuildPrimaryKeyReferenceBinder<T>(EntityModel<T> entityModel, EntityField<T> primaryKeyField)
				where T : class
			{
				var builder = new PrimaryKeyBuilder<T>(entityModel);
				primaryKeyField.Dispatch(builder);
				Binder = builder.Binder;
			}
		}

		private class PrimaryKeyBuilder<TEntity> : IFieldGenericExecutor
			where TEntity : class
		{
			private readonly EntityModel<TEntity> _entityModel;

			public IModelBinder Binder { get; private set; }

			public PrimaryKeyBuilder(EntityModel<TEntity> entityModel)
			{
				_entityModel = entityModel;
			}

			void IFieldGenericExecutor.Execute<TField, TData>(IField field)
			{
				var modelTranscriber = _entityModel.GetModelTranscriber<TEntity>();
				var helper = modelTranscriber.SchemaToTypeHelpers.FirstOrDefault(
					q => q.From == field
					) as TypeModelHelper<TEntity, TData, TData>;
				if (helper == null)
					return;

				var intersectionAnalyzer = new DefaultIntersectionAnalyzer<MockModel, MockField, TypeModel, PropertyInfoField>();
				var intersection = intersectionAnalyzer.CreateIntersection(new MockModel(field.FieldName), _entityModel.TypeModel);
				var intersectedFields = intersection.IntersectedFields
					.OfType<IntersectedFields<MockModel, MockField, TypeModel, PropertyInfoField, string, TData>>()
					.FirstOrDefault();
				if (intersectedFields == null)
					return;

				Binder = new PrimaryKeyEntityBinder<TEntity, TData>(helper, intersectedFields.GetConvertDelegate());
			}
		}

		private class MockModel : IModel<MockField>
		{
			private MockField[] _fields;

			public IReadOnlyList<MockField> Fields => _fields;

			IReadOnlyList<IField> IModel.Fields => _fields;

			public MockModel(string fieldName)
			{
				_fields = new[] { new MockField(fieldName) };
			}

			public void Dispatch(IModelGenericExecutor executor)
				=> executor.Execute<MockModel, MockField>(this);

			public IEnumerable<MockField> GetPathFields(IFieldPath<MockField> fieldPath)
			{
				if (fieldPath.FinalField == null)
					return _fields;
				return new MockField[0];
			}
		}

		private class MockField : IField
		{
			public string FieldName { get; }

			public bool CanRead => true;

			public bool CanWrite => true;

			public bool IsEnumerableType => false;

			public Type FieldDataType => typeof(string);

			public Type FieldElementType => null;

			public MockField(string fieldName)
			{
				FieldName = fieldName;
			}

			public void Dispatch(IFieldGenericExecutor executor)
				=> executor.Execute<MockField, string>(this);
		}

		private class PrimaryKeyEntityBinder<TEntity, TId> : IModelBinder
			where TEntity : class
		{
			private readonly TypeModelHelper<TEntity, TId, TId> _typeModelHelper;
			private readonly TryConvertDelegate<string, TId> _tryConvertId;

			public PrimaryKeyEntityBinder(
				TypeModelHelper<TEntity, TId, TId> typeModelHelper,
				TryConvertDelegate<string, TId> tryConvertId
				)
			{
				_typeModelHelper = typeModelHelper;
				_tryConvertId = tryConvertId;
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

				if (!_tryConvertId(value, out var id))
				{
					// Non-integer arguments result in model state errors
					bindingContext.ModelState.TryAddModelError(
											modelName,
											$"Couldn't convert '{value}' entity reference.");
					return Task.CompletedTask;
				}

				var entityReference = PrimaryKeyEntityReference<TEntity>.Create<TId>(
					_typeModelHelper,
					bindingContext.HttpContext.RequestServices.GetRequiredService<ITypeInstanceFactory>(),
					id
					);

				bindingContext.Result = ModelBindingResult.Success(entityReference);
				return Task.CompletedTask;
			}
		}
	}
}
