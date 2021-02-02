using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.CR.Extensions.CRCreateActions.BC
{
	public class CreateContactFromCustomerGraphExt : PXGraphExtension<CustomerMaint>
	{
		public PXAction<Customer> newContact;
		[PXUIField(DisplayName = Messages.AddContact, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewContact(PXAdapter adapter)
		{
			var ext = Base.GetExtension<CustomerMaint.CreateContactFromCustomerGraphExt>();

			if (ext == null)
				return adapter.Get();

			return ext.CreateContact.Press(adapter);
		}
	}

	public class CreateContactFromVendorGraphExt : PXGraphExtension<VendorMaint>
	{
		public PXAction<Vendor> newContact;
		[PXUIField(DisplayName = Messages.AddContact, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewContact(PXAdapter adapter)
		{
			var ext = Base.GetExtension<VendorMaint.CreateContactFromVendorGraphExt>();

			if (ext == null)
				return adapter.Get();

			return ext.CreateContact.Press(adapter);
		}
	}
}
