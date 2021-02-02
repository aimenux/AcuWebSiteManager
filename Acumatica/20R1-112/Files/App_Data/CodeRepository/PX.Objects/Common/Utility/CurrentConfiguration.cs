using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.Common
{
	class CurrentConfiguration
	{
		#region APSetup

		/// <summary>
		/// Returns actual values of selected fields from APSetup table.
		/// </summary>
		public static APSetupCache ActualAPSetup
		{
			get
			{
				APSetupCache apSetup =
					PXContext.GetSlot<APSetupCache>() ??
					PXContext.SetSlot(
						PXDatabase.GetSlot<APSetupCache>(
							"ActualAPSetup",
							typeof(APSetup)));
				return apSetup;
			}
		}

		public class APSetupCache : IPrefetchable
		{
			public bool isMigrationModeEnabled { get; private set; }

			public void Prefetch()
			{
				using (PXDataRecord apSetup = PXDatabase.SelectSingle<APSetup>(
					new PXDataField<APSetup.migrationMode>()))
				{
					if (apSetup != null)
						isMigrationModeEnabled = (bool)apSetup.GetBoolean(0);
				}
			}
		}
		#endregion

		#region ARSetup

		/// <summary>
		/// Returns actual values of selected fields from APSetup table.
		/// </summary>
		public static ARSetupCache ActualARSetup
		{
			get
			{
				ARSetupCache apSetup =
					PXContext.GetSlot<ARSetupCache>() ??
					PXContext.SetSlot(
						PXDatabase.GetSlot<ARSetupCache>(
							"ActualARSetup",
							typeof(ARSetup)));
				return apSetup;
			}
		}

		public class ARSetupCache : IPrefetchable
		{
			public bool isMigrationModeEnabled { get; private set; }

			public void Prefetch()
			{
				using (PXDataRecord arSetup = PXDatabase.SelectSingle<ARSetup>(
					new PXDataField<ARSetup.migrationMode>()))
				{
					if (arSetup != null)
						isMigrationModeEnabled = (bool)arSetup.GetBoolean(0);
				}
			}
		}
		#endregion
	}
}
