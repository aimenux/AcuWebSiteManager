using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCSOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void SOShipment_LastModifiedDateTime_CacheAttached(PXCache sender) { }
	}
}
