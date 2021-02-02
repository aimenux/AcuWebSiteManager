using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.SM;
using urlReports = PX.Objects.Common.urlReports;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	public sealed class ConstructionReportSelectorAttribute : PXSelectorAttribute
	{
		public ConstructionReportSelectorAttribute()
			: base(typeof(SelectFrom<SiteMap>
					.Where<SiteMap.url.IsLike<urlReports>
						.And<SiteMap.screenID.IsLike<PXModule.ap_>
							.Or<SiteMap.screenID.IsLike<PXModule.po_>>
							.Or<SiteMap.screenID.IsLike<PXModule.sc_>>
							.Or<SiteMap.screenID.IsLike<PXModule.rq_>>
							.Or<SiteMap.screenID.IsLike<PXModule.cl_>>>>
					.AggregateTo<GroupBy<SiteMap.screenID>>
					.SearchFor<SiteMap.screenID>),
				typeof(SiteMap.screenID), typeof(SiteMap.title))
		{
			Headers = new[]
			{
				CA.Messages.ReportID,
				CA.Messages.ReportName
			};
			DescriptionField = typeof(SiteMap.title);
		}
	}
}