using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using System.Collections.Generic;

namespace Silk.Web.ObjectMapping
{
	public class MappingOptions
	{
		public List<IIntersectCandidateSource<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>>
			IntersectCandidateSources { get; } = new List<IIntersectCandidateSource<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>>();

		public List<IIntersectionRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>>
			IntersectionRules { get; } = new List<IIntersectionRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>>();

		public MappingOptions()
		{
			IntersectCandidateSources.AddRange(GetDefaultCandidateSources());
			IntersectionRules.AddRange(GetDefaultRules());
		}

		private static IEnumerable<IIntersectionRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>> GetDefaultRules()
		{
			yield return new SameDataTypeRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
			yield return new BothNumericTypesRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
			yield return new ConvertableWithToStringRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
			yield return new ExplicitCastRule<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
			yield return new ConvertableWithTryParse<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
		}

		private static IEnumerable<IIntersectCandidateSource<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>> GetDefaultCandidateSources()
		{
			yield return new ExactPathMatchCandidateSource<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
			yield return new FlattenedNameMatchCandidateSource<TypeModel, PropertyInfoField, TypeModel, PropertyInfoField>();
		}
	}
}
