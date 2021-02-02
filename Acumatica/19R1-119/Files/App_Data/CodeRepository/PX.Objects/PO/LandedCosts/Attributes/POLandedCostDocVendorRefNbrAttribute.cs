using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.PO.LandedCosts.Attributes
{
	public class POLandedCostDocVendorRefNbrAttribute : BaseVendorRefNbrAttribute
	{
		#region Ctor
		public POLandedCostDocVendorRefNbrAttribute() : base(typeof(POLandedCostDoc.vendorID))
		{
		}
		#endregion

		#region Implementation
		protected override bool IsIgnored(PXCache sender, object row)
		{
			POLandedCostDoc r = (POLandedCostDoc)row;
			return r.Released == true || r.CreateBill != true || base.IsIgnored(sender, row) || (String.IsNullOrEmpty(r.VendorRefNbr) && r.Hold == true);
		}

		protected override EntityKey GetEntityKey(PXCache sender, object row)
		{
			var ek = new EntityKey();
			ek._DetailID = DETAIL_DUMMY;
			ek._MasterID = GetMasterNoteId(typeof(POLandedCostDoc), typeof(POLandedCostDoc.noteID), row);

			return ek;
		}

		public override Guid? GetSiblingID(PXCache sender, object row)
		{
			return (Guid?)sender.GetValue<POLandedCostDoc.noteID>(row);
		}
		#endregion
	}
}
