using System;
using System.Linq;
using Silk.Data;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Web.ORM
{
	public class PrimaryKeyEntityReferenceRule :
		IIntersectionRule<EntityModel, EntityField, TypeModel, PropertyInfoField>
	{
		public Schema Schema { get; set; }
		public ITypeInstanceFactory TypeInstanceFactory { get; set; }

		public bool IsValidIntersection(
			IntersectCandidate<EntityModel, EntityField, TypeModel, PropertyInfoField> intersectCandidate,
			out IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField> intersectedFields
			)
		{
			if (intersectCandidate.RightField.FieldDataType.IsGenericType &&
				intersectCandidate.RightField.FieldDataType.GetGenericTypeDefinition() == typeof(IEntityReference<>) &&
				intersectCandidate.RightField.FieldDataType.GetGenericArguments()[0] == intersectCandidate.LeftField.FieldDataType)
			{
				var referenceKeys = intersectCandidate.LeftField.SubFields
					.Where(q => q.IsEntityLocalField)
					.ToArray();

				if (referenceKeys.Length == 1)
				{
					intersectedFields = BuildIntersectedFields(
						intersectCandidate.LeftField,
						intersectCandidate.LeftPath.Child(referenceKeys[0]),
						intersectCandidate.RightPath
						);
					return true;
				}
			}

			intersectedFields = null;
			return false;
		}

		private IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField> BuildIntersectedFields(
			EntityField referenceField,	IFieldPath<EntityModel, EntityField> referenceKey,
			IFieldPath<TypeModel, PropertyInfoField> modelField
			)
		{
			//  to obey generic type constraints we need to use reflection
			//  so there's no use in using the ill-suited generic dispatch pattern here
			var factory = Activator.CreateInstance(
				typeof(Factory<,>).MakeGenericType(referenceField.FieldDataType, referenceKey.FinalField.FieldDataType),
				TypeInstanceFactory, Schema
				) as Factory;
			return factory.BuildIntersectedFields(referenceKey, modelField);
		}

		private abstract class Factory
		{
			public abstract IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField> BuildIntersectedFields(
				IFieldPath<EntityModel, EntityField> referenceKey,
				IFieldPath<TypeModel, PropertyInfoField> modelField
				);
		}

		private class Factory<TEntity, TPrimaryKey> : Factory
			where TEntity : class
		{
			private readonly Schema _schema;
			private readonly ITypeInstanceFactory _typeInstanceFactory;

			public Factory(ITypeInstanceFactory typeInstanceFactory, Schema schema)
			{
				_typeInstanceFactory = typeInstanceFactory;
				_schema = schema;
			}

			public override IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField> BuildIntersectedFields(
				IFieldPath<EntityModel, EntityField> referenceKey,
				IFieldPath<TypeModel, PropertyInfoField> modelField
				)
			{
				return new IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField, TPrimaryKey, IEntityReference<TEntity>>(
					referenceKey.FinalField, modelField.FinalField,
					referenceKey, modelField, typeof(PrimaryKeyEntityReferenceRule),
					ConverterBuilder
					);
			}

			private TryConvertDelegate<TPrimaryKey, IEntityReference<TEntity>> ConverterBuilder()
			{
				var referenceFactory = _schema.CreatePrimaryKeyReferenceFactory<TEntity>();
				if (referenceFactory == null)
					throw new InvalidOperationException("Reference type not supported.");

				var converter = new Converter<TEntity, TPrimaryKey>(_typeInstanceFactory, referenceFactory);
				return converter.TryConvert;
			}
		}

		private class Converter<TEntity, TPrimaryKey>
			where TEntity : class
		{
			private readonly ITypeInstanceFactory _typeInstanceFactory;
			private readonly PrimaryKeyEntityReferenceFactory<TEntity> _referenceFactory;

			public Converter(
				ITypeInstanceFactory typeInstanceFactory,
				PrimaryKeyEntityReferenceFactory<TEntity> referenceFactory
				)
			{
				_typeInstanceFactory = typeInstanceFactory;
				_referenceFactory = referenceFactory;
			}

			public bool TryConvert(TPrimaryKey primaryKey, out IEntityReference<TEntity> entityReference)
			{
				var primaryKeyStr = primaryKey.ToString();
				entityReference = _referenceFactory.Create(_typeInstanceFactory, primaryKeyStr);
				return true;
			}
		}
	}
}
