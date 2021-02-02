using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.SM;

namespace PX.Objects.AR
{
    [Serializable]
	public partial class ARIntegrityCheckFilter : PX.Data.IBqlTable
	{
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		protected String _CustomerClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer Class")]
		public virtual String CustomerClassID
		{
			get
			{
				return this._CustomerClassID;
			}
			set
			{
				this._CustomerClassID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodNonLockedSelector]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}

	[TableAndChartDashboardType]
	public class ARIntegrityCheck : PXGraph<ARIntegrityCheck>
	{
		public PXCancel<ARIntegrityCheckFilter> Cancel;
		public PXFilter<ARIntegrityCheckFilter> Filter;
		[PXFilterable]
        [PX.SM.PXViewDetailsButton(typeof(Customer.acctCD), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXFilteredProcessing<Customer, ARIntegrityCheckFilter,
			Where<Match<Current<AccessInfo.userName>>>> ARCustomerList;
		
		public PXSelect<Customer, 
			Where<Customer.customerClassID, Equal<Current<ARIntegrityCheckFilter.customerClassID>>,
			And<Match<Current<AccessInfo.userName>>>>> Customer_ClassID;
		public PXSelect<Customer,
			Where<Match<Current<AccessInfo.userName>>>> Customers;


		protected virtual IEnumerable arcustomerlist()
		{

			if (Filter.Current != null && Filter.Current.CustomerClassID != null)
			{
				using (new PXFieldScope(Customer_ClassID.View,
					typeof(Customer.bAccountID),
					typeof(Customer.acctCD),
					typeof(Customer.customerClassID)))
					return Customer_ClassID.SelectDelegateResult();
			}
			else
			{
				using (new PXFieldScope(Customers.View,
					typeof(Customer.bAccountID),
					typeof(Customer.acctCD),
					typeof(Customer.customerClassID)))
					return Customers.SelectDelegateResult();
			}

		}

		public ARIntegrityCheck()
		{
			ARSetup setup = ARSetup.Current;
		}

		protected virtual void ARIntegrityCheckFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool errorsOnForm = PXUIFieldAttribute.GetErrors(sender, null, PXErrorLevel.Error, PXErrorLevel.RowError).Count > 0;
			ARCustomerList.SetProcessEnabled(!errorsOnForm);
			ARCustomerList.SetProcessAllEnabled(!errorsOnForm);
			ARCustomerList.SuppressMerge = true;
			ARCustomerList.SuppressUpdate = true;

			ARIntegrityCheckFilter filter = Filter.Current;
			ARCustomerList.SetProcessDelegate<ARReleaseProcess>(
				delegate(ARReleaseProcess re, Customer cust)
				{
					re.Clear(PXClearOption.PreserveTimeStamp);
					re.IntegrityCheckProc(cust, filter.FinPeriodID);
				}
			);

			//For perfomance recomended select not more than maxCustomerCount customers, 
			//becouse the operation is performed for a long time.
			const int maxCustomerCount = 5;
			ARCustomerList.SetParametersDelegate(delegate(List<Customer> list)
			{
				bool processing = true;
				if (PX.Common.PXContext.GetSlot<AUSchedule>() == null && list.Count > maxCustomerCount)
				{
					WebDialogResult wdr = ARCustomerList.Ask(Messages.ContinueValidatingBalancesForMultipleCustomers, MessageButtons.OKCancel);
					processing = wdr == WebDialogResult.OK;
				}
				return processing;
			});
		}

		public PXSetup<ARSetup> ARSetup;
	}
}
