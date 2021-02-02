using System;
using SW.Data;

namespace SW.Objects.CS
{
	public class PreferencesMaint : SWGraph<PreferencesMaint>
	{
		public SWSave<Preferences> Save;
		public SWCancel<Preferences> Cancel;

		public SWSelect<Preferences> Prefs;
	}
}