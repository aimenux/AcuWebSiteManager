using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class VendorDataProvider
	{
		public static Vendor GetVendor(PXGraph graph, int? vendorId)
		{
			return SelectFrom<Vendor>
				.Where<Vendor.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, vendorId);
		}
	}
}