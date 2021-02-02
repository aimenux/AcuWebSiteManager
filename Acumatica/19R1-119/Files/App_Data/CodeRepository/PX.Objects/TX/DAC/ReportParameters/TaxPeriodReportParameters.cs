using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.DAC.ReportParameters;
using PX.Objects.GL.Attributes;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX.DAC.ReportParameters
{
	public class TaxPeriodReportParameters: IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false)]
		public int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[TaxPeriodFilterBranch(typeof(TaxPeriodReportParameters.organizationID), hideBranchField: false)]
		public int? BranchID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(typeof(Search<Vendor.bAccountID, Where<Vendor.taxAgency, Equal<True>>>), DisplayName = "Tax Agency")]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		protected string _TaxPeriodID;
		[GL.FinPeriodID]
		[PXSelector(typeof(Search<TaxPeriod.taxPeriodID,
				Where<TaxPeriod.organizationID, Equal<Optional2<TaxPeriodForReportShowing.organizationID>>,
					And<TaxPeriod.vendorID, Equal<Optional2<TaxPeriodForReportShowing.vendorID>>,
						And<TaxPeriod.status, NotEqual<TaxPeriodStatus.open>>>>>),
			typeof(TaxPeriod.taxPeriodID), typeof(TaxPeriod.startDateUI), typeof(TaxPeriod.endDateUI), typeof(TaxPeriod.status))]
		[PXUIField(DisplayName = "Tax Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
	}
}
