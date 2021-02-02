using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.CR
{
	public class CRCampaignClassMaint : PXGraph<CRCampaignClassMaint, CRCampaignType>
	{
		[PXViewName(Messages.CampaignClass)]
		public PXSelect<CRCampaignType>
			CampaignClass;

		[PXViewName(Messages.Attributes)]
		public CSAttributeGroupList<CRCampaignType, CRCampaign> Mapping;

		#region Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Campaign Class", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void CRCampaignType_TypeID_CacheAttached(PXCache sender)
		{
		}
		#endregion
	}
}