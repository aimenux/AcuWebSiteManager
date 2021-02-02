using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class PMSiteAddressAttribute : AddressAttribute
	{
		public PMSiteAddressAttribute(Type SelectType)
			: base(typeof(PMSiteAddress.addressID), typeof(PMSiteAddress.isDefaultBillAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PMSiteAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<PMSiteAddress, PMSiteAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMProject project = e.Row as PMProject;

			PXCache addressCache = sender.Graph.Caches[_RecordType];

			PMSiteAddress siteAddress = (PMSiteAddress)addressCache.Insert(new PMSiteAddress { RevisionID = 0, OverrideAddress = true });
			project.SiteAddressID = siteAddress.AddressID;

			addressCache.IsDirty = false;
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<PMSiteAddress, PMSiteAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<PMSiteAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}
	}
}
