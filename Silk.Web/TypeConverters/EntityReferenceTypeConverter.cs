using Silk.Data;
using Silk.Data.Modelling;

namespace Silk.Web.TypeConverters
{
	public class EntityReferenceTypeConverter<T> : TypeConverter<IEntityReference<T>, T>
		where T : class
	{
		public override bool TryConvert(IEntityReference<T> from, out T to)
		{
			if (from == null)
			{
				to = default;
				return false;
			}
			to = from.AsEntity();
			return true;
		}
	}
}
