using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCARPaymentEntryExt : PXGraphExtension<ARPaymentEntry>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void ARPayment_LastModifiedDateTime_CacheAttached(PXCache sender) { }

		public PXAction<ARPayment> registerBCAuthTran;
		[PXUIField(DisplayName = "Register BC Auth Tran", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		[PXProcessButton]
		public virtual IEnumerable RegisterBCAuthTran(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}
}
