using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.SQLite3;

namespace Silk.Web.Tests
{
	[TestClass]
	public class ORMEntityTests
	{
		[TestMethod]
		public void Can_Store_And_Retrieve_Entities()
		{
			var serviceProvider = new ServiceCollection()
				.AddScoped<IDataProvider>(sP => new SQLite3DataProvider("Data Source=:memory:;Mode=Memory"))
				.AddORM()
				.AddORMEntity<TestEntity>()
				.BuildServiceProvider();
			using (var scope = serviceProvider.CreateScope())
			{
				var table = scope.ServiceProvider.GetRequiredService<IEntityTable<TestEntity>>();
				var store = scope.ServiceProvider.GetRequiredService<ISqlEntityStore<TestEntity>>();

				table.CreateTable().Execute();

				var obj = new TestEntity { Data = "Hello World" };
				store.Insert(obj).Execute();

				Assert.AreNotEqual(0, obj.Id);

				store.Select(query => query.AndWhere(q => q.Id == obj.Id), out var fetchResult).Execute();

				Assert.AreEqual(1, fetchResult.Result.Count);
				Assert.AreEqual(obj.Data, fetchResult.Result[0].Data);
			}
		}

		[TestMethod]
		public void ORM_Services_Create_Instances_With_Services()
		{
			var serviceProvider = new ServiceCollection()
				.AddScoped<IDataProvider>(sP => new SQLite3DataProvider("Data Source=:memory:;Mode=Memory"))
				.AddScoped<TestService>()
				.AddORM()
				.AddORMEntity<TestEntity>()
				.BuildServiceProvider();
			using (var scope = serviceProvider.CreateScope())
			{
				var table = scope.ServiceProvider.GetRequiredService<IEntityTable<TestEntity>>();
				var store = scope.ServiceProvider.GetRequiredService<ISqlEntityStore<TestEntity>>();

				table.CreateTable().Execute();

				var obj = new TestEntity { Data = "Hello World" };
				store.Insert(obj).Execute();

				store.Select<TestView>(query => query.AndWhere(q => q.Id == obj.Id), out var fetchResult).Execute();

				Assert.AreEqual(1, fetchResult.Result.Count);
				Assert.AreEqual(obj.Data, fetchResult.Result[0].Data);
				Assert.IsNotNull(fetchResult.Result[0].Service);
			}
		}

		private class TestEntity
		{
			public int Id { get; private set; }
			public string Data { get; set; }
		}

		private class TestView
		{
			public TestView(TestService testService)
			{
				Service = testService;
			}

			public TestService Service { get; }
			public string Data { get; set; }
		}

		private class TestService
		{
		}
	}
}
