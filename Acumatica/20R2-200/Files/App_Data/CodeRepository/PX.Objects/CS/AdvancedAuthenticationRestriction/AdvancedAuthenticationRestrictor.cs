using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.CS
{
	internal class AdvancedAuthenticationRestrictor : IAdvancedAuthenticationRestrictor
	{
		private const string _allowedProviderName = "ExchangeIdentityToken";
		private const string _msProviderName = "MicrosoftAccount";
		private const string _googleProviderName = "Google";

		public bool IsAllowedProviderName(string name)
		{
			name.ThrowOnNull(nameof(name));

			if (_allowedProviderName.Equals(name, StringComparison.Ordinal))
				return true;

			if (_msProviderName.Equals(name, StringComparison.Ordinal) || _googleProviderName.Equals(name, StringComparison.Ordinal))
				return PXAccess.FeatureInstalled<FeaturesSet.googleAndMicrosoftSSO>();

			return  PXAccess.FeatureInstalled<FeaturesSet.activeDirectoryAndOtherExternalSSO>();
		}
	}
}
