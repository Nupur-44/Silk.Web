using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silk.Data.SQL.ORM;
using System;

namespace Silk.Web.ORM
{
	public class ORMSchemaCreator
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ORMOptions _ormOptions;

		public ORMSchemaCreator(IServiceProvider serviceProvider, IOptions<ORMOptions> ormOptionsAccessor)
		{
			_serviceProvider = serviceProvider;
			_ormOptions = ormOptionsAccessor.Value;
		}

		public void CreateSchema()
		{
			var executor = new Executor(_serviceProvider);
			foreach (var entityConfig in _ormOptions.ORMEntityConfigurations)
			{
				entityConfig.Dispatch(executor);
			}
		}

		private class Executor : IORMEntityExecutor
		{
			private readonly IServiceProvider _serviceProvider;

			public Executor(IServiceProvider serviceProvider)
			{
				_serviceProvider = serviceProvider;
			}

			public void Execute<T>() where T : class
			{
				var table = _serviceProvider.GetRequiredService<IEntityTable<T>>();

				table.TableExists(out var existsResult).Execute();
				if (!existsResult.Result)
				{
					table.CreateTable().Execute();
				}
			}
		}
	}
}
