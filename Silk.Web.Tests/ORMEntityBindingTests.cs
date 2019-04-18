using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.SQLite3;
using Silk.Web.ORM;
using System;
using System.Collections.Generic;

namespace Silk.Web.Tests
{
	[TestClass]
	public class ORMEntityBindingTests
	{
		[TestMethod]
		public void Binding_Single_PrimaryKey_Model_Returns_EntityReference()
		{
			var serviceProvider = new ServiceCollection()
				.AddORM()
				.AddORMEntity<Entity>()
				.BuildServiceProvider();

			var bindingProvider = new ORMEntityModelBindingProvider();

			using (var scope = serviceProvider.CreateScope())
			{
				var primaryKey = Guid.NewGuid();
				var providerContext = new MockModelBindingProviderContext(
					scope.ServiceProvider, typeof(Entity)
					);
				var binder = bindingProvider.GetBinder(providerContext);
				var bindingContext = new MockModelBindingContext(primaryKey.ToString(), scope.ServiceProvider);
				binder.BindModelAsync(bindingContext);

				var result = bindingContext.Result.Model as Data.SQL.ORM.PrimaryKeyEntityReference<Entity>;
				Assert.IsNotNull(result);
				Assert.AreEqual(primaryKey, result.AsEntity().Id);
			}
		}

		private class Entity
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}

		private class MockModelBindingContext : ModelBindingContext
		{
			public override ActionContext ActionContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override string BinderModelName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override BindingSource BindingSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override string FieldName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override bool IsTopLevelObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override object Model { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override ModelMetadata ModelMetadata { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override string ModelName { get; set; } = "Test";
			public override ModelStateDictionary ModelState { get; set; }
			public override Func<ModelMetadata, bool> PropertyFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override ValidationStateDictionary ValidationState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override IValueProvider ValueProvider { get; set; }
			public override ModelBindingResult Result { get; set; }

			public override HttpContext HttpContext { get; }

			public MockModelBindingContext(string value, IServiceProvider serviceProvider)
			{
				ValueProvider = new SingleValueProvider(value);
				HttpContext = new DefaultHttpContext { RequestServices = serviceProvider };
			}

			public override NestedScope EnterNestedScope(ModelMetadata modelMetadata, string fieldName, string modelName, object model)
			{
				throw new NotImplementedException();
			}

			public override NestedScope EnterNestedScope()
			{
				throw new NotImplementedException();
			}

			protected override void ExitNestedScope()
			{
				throw new NotImplementedException();
			}
		}

		private class SingleValueProvider : IValueProvider
		{
			private readonly string _value;

			public SingleValueProvider(string value)
			{
				_value = value;
			}

			public bool ContainsPrefix(string prefix)
			{
				throw new NotImplementedException();
			}

			public ValueProviderResult GetValue(string key)
			{
				return new ValueProviderResult(new Microsoft.Extensions.Primitives.StringValues(_value));
			}
		}

		private class MockModelBindingProviderContext : ModelBinderProviderContext
		{
			public override BindingInfo BindingInfo => throw new NotImplementedException();

			public override ModelMetadata Metadata { get; }

			public override IModelMetadataProvider MetadataProvider => throw new NotImplementedException();

			public override IServiceProvider Services { get; }

			public MockModelBindingProviderContext(IServiceProvider services, Type modelType)
			{
				Services = services;
				Metadata = new MockModelMetadata(modelType);
			}

			public override IModelBinder CreateBinder(ModelMetadata metadata)
			{
				throw new NotImplementedException();
			}
		}

		private class MockModelMetadata : ModelMetadata
		{
			public MockModelMetadata(Type modelType) : base(ModelMetadataIdentity.ForType(modelType))
			{
			}

			public override IReadOnlyDictionary<object, object> AdditionalValues => throw new NotImplementedException();

			public override ModelPropertyCollection Properties => throw new NotImplementedException();

			public override string BinderModelName => throw new NotImplementedException();

			public override Type BinderType => throw new NotImplementedException();

			public override BindingSource BindingSource => throw new NotImplementedException();

			public override bool ConvertEmptyStringToNull => throw new NotImplementedException();

			public override string DataTypeName => throw new NotImplementedException();

			public override string Description => throw new NotImplementedException();

			public override string DisplayFormatString => throw new NotImplementedException();

			public override string DisplayName => throw new NotImplementedException();

			public override string EditFormatString => throw new NotImplementedException();

			public override ModelMetadata ElementMetadata => throw new NotImplementedException();

			public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues => throw new NotImplementedException();

			public override IReadOnlyDictionary<string, string> EnumNamesAndValues => throw new NotImplementedException();

			public override bool HasNonDefaultEditFormat => throw new NotImplementedException();

			public override bool HtmlEncode => throw new NotImplementedException();

			public override bool HideSurroundingHtml => throw new NotImplementedException();

			public override bool IsBindingAllowed => throw new NotImplementedException();

			public override bool IsBindingRequired => throw new NotImplementedException();

			public override bool IsEnum => throw new NotImplementedException();

			public override bool IsFlagsEnum => throw new NotImplementedException();

			public override bool IsReadOnly => throw new NotImplementedException();

			public override bool IsRequired => throw new NotImplementedException();

			public override ModelBindingMessageProvider ModelBindingMessageProvider => throw new NotImplementedException();

			public override int Order => throw new NotImplementedException();

			public override string Placeholder => throw new NotImplementedException();

			public override string NullDisplayText => throw new NotImplementedException();

			public override IPropertyFilterProvider PropertyFilterProvider => throw new NotImplementedException();

			public override bool ShowForDisplay => throw new NotImplementedException();

			public override bool ShowForEdit => throw new NotImplementedException();

			public override string SimpleDisplayProperty => throw new NotImplementedException();

			public override string TemplateHint => throw new NotImplementedException();

			public override bool ValidateChildren => throw new NotImplementedException();

			public override IReadOnlyList<object> ValidatorMetadata => throw new NotImplementedException();

			public override Func<object, object> PropertyGetter => throw new NotImplementedException();

			public override Action<object, object> PropertySetter => throw new NotImplementedException();
		}
	}
}
