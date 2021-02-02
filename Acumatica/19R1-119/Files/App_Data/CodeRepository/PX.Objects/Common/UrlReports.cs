using System;
using PX.Data;

namespace PX.Objects.Common
{
    public class urlReports : PX.Data.BQL.BqlString.Constant<urlReports>
	{
        public urlReports() : base("~/Frames/ReportLauncher%") {; }
    }
}
namespace PX.Objects.CA
{
    [Obsolete("Obsolete. Will be removed in Acumatica ERP 2019R1. Please use PX.Objects.Common.urlReports instead.")]
    public class urlReports : PX.Objects.Common.urlReports {  }
}