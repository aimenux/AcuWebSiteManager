using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AP.DAC
{
	public class APReportParameters : IBqlTable
	{
		#region NotInactiveVendorID
		public abstract class notInactiveVendorID : PX.Data.BQL.BqlInt.Field<notInactiveVendorID> { }

		[Vendor]
		[PXRestrictor(typeof(Where<Vendor.status, NotEqual<BAccount.status.inactive>>),
			Messages.VendorIsInStatus,
			typeof(Vendor.status))]
		public virtual int? NotInactiveVendorID { get; set; }

		#endregion
	}
}
