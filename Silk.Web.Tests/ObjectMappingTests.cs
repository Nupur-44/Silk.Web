using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;

namespace Silk.Web.Tests
{
	[TestClass]
	public class ObjectMappingTests
	{
		[TestMethod]
		public void Object_Mapper_Uses_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection()
				.AddObjectMapper()
				.AddScoped<ServiceType>()
				.BuildServiceProvider();
			using (var scope = serviceProvider.CreateScope())
			{
				var mapper = scope.ServiceProvider.GetRequiredService<IObjectMapper>();
				var source = new SourceType
				{
					Data = 1,
					Sub = new SubSourceType { Data = 2 }
				};
				var target = mapper.Map<SourceType, TargetType>(source);

				Assert.IsNotNull(target.Service);
				Assert.IsNotNull(target.Sub.Service);
				Assert.ReferenceEquals(target.Service, target.Sub.Service);
			}
		}

		private class SourceType
		{
			public int Data { get; set; }
			public SubSourceType Sub { get; set; }
		}

		private class SubSourceType
		{
			public int Data { get; set; }
		}

		private class TargetType
		{
			public TargetType(ServiceType service)
			{
				Service = service;
			}

			public ServiceType Service { get; }
			public int Data { get; set; }
			public SubTargetType Sub { get; set; }
		}

		private class SubTargetType
		{
			public SubTargetType(ServiceType service)
			{
				Service = service;
			}

			public ServiceType Service { get; }
			public int Data { get; set; }
		}

		private class ServiceType
		{
		}
	}
}
