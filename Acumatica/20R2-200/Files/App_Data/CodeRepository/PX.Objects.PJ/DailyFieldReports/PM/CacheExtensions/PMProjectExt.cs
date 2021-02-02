using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions
{
	public sealed class PMProjectExt : PXCacheExtension<PMProject>
    {
		#region SiteAddressID
		public abstract class siteAddressID : BqlInt.Field<siteAddressID> { }
		[PXDBInt]
		[PMSiteAddress(typeof(Select<PMSiteAddress>))]
		public int? SiteAddressID
		{
			get;
			set;
		}
		#endregion
	}
}