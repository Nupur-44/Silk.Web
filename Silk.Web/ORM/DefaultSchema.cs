using System.Collections.Generic;
using System.Reflection;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Web.ORM
{
	public class DefaultSchema : Schema
	{
		public DefaultSchema(IEnumerable<EntityModel> entityModels,
			Dictionary<MethodInfo, IMethodCallConverter> methodCallConverters) :
			base(entityModels, methodCallConverters)
		{
		}
	}
}
