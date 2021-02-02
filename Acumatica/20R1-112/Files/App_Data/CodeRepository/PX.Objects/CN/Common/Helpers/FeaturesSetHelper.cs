using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CN.Common.Helpers
{
	public static class FeaturesSetHelper
	{
		private const string FeatureRequiredError = "Feature '{0}' should be enabled";
		private const string ProcoreIntegrationFeatureName = "Procore Integration";
		private const string ConstructionFeatureName = "Construction";
		private const string ProjectManagementFeatureName = "Construction Project Management";

		public static void CheckProcoreIntegrationFeature()
		{
			CheckFeature<FeaturesSet.procoreIntegration>(ProcoreIntegrationFeatureName);
		}

		public static void CheckConstructionFeature()
		{
			CheckFeature<FeaturesSet.construction>(ConstructionFeatureName);
		}

		public static void CheckProjectManagementFeature()
		{
			CheckFeature<FeaturesSet.constructionProjectManagement>(ProjectManagementFeatureName);
		}

		private static void CheckFeature<TFeature>(string featureName)
			where TFeature : IBqlField
		{
			if (!PXAccess.FeatureInstalled<TFeature>())
			{
				throw new Exception(string.Format(FeatureRequiredError, featureName));
			}
		}
	}
}