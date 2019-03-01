using Silk.Data;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Web.ORM
{
	public sealed class PrimaryKeyEntityReference<TEntity> : IEntityReference<TEntity>
		where TEntity : class
	{
		private readonly TEntity _referenceEntity;

		private PrimaryKeyEntityReference(TEntity entity)
		{
			_referenceEntity = entity;
		}

		public TEntity AsEntity()
			=> _referenceEntity;

		public static PrimaryKeyEntityReference<TEntity> Create<TId>(
			TypeModelHelper<TEntity, TId, TId> typeModelHelper,
			ITypeInstanceFactory typeInstanceFactory,
			TId primaryKey
			)
		{
			var instance = typeInstanceFactory.CreateInstance<TEntity>();
			typeModelHelper.WriteValueToInstance(instance, primaryKey);
			return new PrimaryKeyEntityReference<TEntity>(instance);
		}
	}
}
