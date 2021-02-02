using PX.Data;
using PX.SM;
using System.Collections;
using System.Linq;

namespace PX.Objects.CS
{
	public class PreferencesSecurityMaintExt : PXGraphExtension<PreferencesSecurityMaint>
	{
		[InjectDependency]
		private IAdvancedAuthenticationRestrictor AdvancedAuthenticationRestrictor { get; set; }

		public IEnumerable identities()
		{
			return Base.identities()
				.OfType<PreferencesIdentityProvider>()
				.Where(p => AdvancedAuthenticationRestrictor.IsAllowedProviderName(p.ProviderName));
		}
	}
}
