using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using static PX.Objects.PM.BillingProcess;

namespace PX.Objects.PM
{
	public class BillingProcessVisibilityRestriction : PXGraphExtension<BillingProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ProjectsUnbilled.WhereAnd<
				Where<PMProject.defaultBranchID, IsNotNull,
					Or<Where<PMProject.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
			Base.ProjectsUbilledCutOffDateExcluded.WhereAnd<
				Where<PMProject.defaultBranchID, IsNotNull,
					Or<Where<PMProject.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
			Base.ProjectsProgressive.WhereAnd<
				Where<PMProject.defaultBranchID, IsNotNull,
					Or<Where<PMProject.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
			Base.ProjectsRecurring.WhereAnd<
				Where<PMProject.defaultBranchID, IsNotNull,
					Or<Where<PMProject.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
		}

		public void _(Events.RowSelected<BillingFilter> e)
		{
			BillingFilter filter = Base.Filter.Current;

			Base.Items.SetProcessDelegate<PMBillEngine>(
					delegate (PMBillEngine engine, ProjectsList item)
					{
						PMProject project = PMProject.PK.Find(Base, item.ProjectID);
						if (project.DefaultBranchID != null)
						{
							Customer customer = null;
							if (project.CustomerID != null)
								customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>,
									And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>.Select(Base, project.CustomerID);

							if (customer == null)
							{
								string customerName = GetCustomerName(project.CustomerID);
								throw new PXException(AR.Messages.BranchRestrictedByCustomer, customerName, PXAccess.GetBranchCD());
							}
						}

						engine.Clear();
						if (engine.Bill(item.ProjectID, filter.InvoiceDate, filter.InvFinPeriodID).IsEmpty)
						{
							throw new PXSetPropertyException(Warnings.NothingToBill, PXErrorLevel.RowWarning);
						}
					});
		}

		private string GetCustomerName(int? customerId)
		{
			if (!customerId.HasValue)
			{
				return null;
			}
			Customer customer = PXSelect<Customer,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, customerId);
			return customer?.AcctName;
		}
	}
}
