using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.PR
{
	public class PRPayStubInq : PXGraph<PRPayStubInq>
	{
		public PXFilter<PayStubFilter> Filter;
		public SelectFrom<PRPayment>
			.Where<PRPayment.employeeID.IsEqual<PayStubFilter.employeeID.FromCurrent>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>
				.And<PRPayment.printed.IsEqual<True>
				.And<PRPayment.voided.IsEqual<False>>>>>.View PayChecks;

		public PRPayStubInq()
		{
			PayChecks.AllowInsert = false;
			PayChecks.AllowUpdate = false;
			PayChecks.AllowDelete = false;
			viewStubReport.SetVisible(false);
		}

		[PXRemoveBaseAttribute(typeof(PXNoteAttribute))]
		[PXGuid]
		public void _(Events.CacheAttached<PRPayment.noteID> e) { }

		public PXAction<PayStubFilter> viewStubReport;
		[PXUIField]
		[PXButton]
		public IEnumerable ViewStubReport(PXAdapter adapter)
		{
			if (PayChecks.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters["DocType"] = PayChecks.Current.DocType;
				parameters["RefNbr"] = PayChecks.Current.RefNbr;

				throw new PXReportRequiredException(parameters, "PR641000", PXBaseRedirectException.WindowMode.New, Messages.PayCheckReport);
			}

			return adapter.Get();
		}
	}

	[PXCacheName(Messages.PayStubFilter)]
	public class PayStubFilter : IBqlTable
	{
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXInt]
		[PXUIField(DisplayName = "Employee", Enabled = false)]
		[PXSelector(typeof(SearchFor<EPEmployee.bAccountID>), SubstituteKey = typeof(EPEmployee.acctName))]
		[PXUnboundDefault(typeof(SearchFor<EPEmployee.bAccountID>.Where<EPEmployee.userID.IsEqual<AccessInfo.userID.FromCurrent>>))]
		public virtual int? EmployeeID { get; set; }
		#endregion
	}
}
