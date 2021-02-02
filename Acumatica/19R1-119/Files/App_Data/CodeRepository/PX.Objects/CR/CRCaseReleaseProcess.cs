using System;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Data;
using PX.Objects.CT;
using PX.SM;

namespace PX.Objects.CR
{
	#region CaseFilter

	[Serializable]
	[PXHidden]
	public partial class CaseFilter : IBqlTable
	{
		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Case Class")]
		[PXSelector(typeof(CRCaseClass.caseClassID),
			DescriptionField = typeof(CRCaseClass.description),
			CacheGlobal = true)]
		public virtual String CaseClassID { get; set; }
		#endregion

		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(CustomerClass.customerClassID),
			DescriptionField = typeof(CustomerClass.descr),
			CacheGlobal = true)]
		[PXUIField(DisplayName = "Business Account Class")]
		public virtual String CustomerClassID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[CustomerAndProspect(DisplayName = "Business Account")]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		[Contract(typeof(Where<Contract.customerID, Equal<Current<CaseFilter.customerID>>,
			Or<Current<CaseFilter.customerID>, IsNull>>), DisplayName = "Contract")]
		public virtual Int32? ContractID { get; set; }
		#endregion
	}

	#endregion

	[TableAndChartDashboardType]
	public class CRCaseReleaseProcess : PXGraph<CRCaseReleaseProcess>
	{
		public PXFilter<CaseFilter> 
			Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(CaseFilter))]
		public PXFilteredProcessingJoin<CRCase, CaseFilter,
				InnerJoin<Customer, On<CRCase.customerID, Equal<Customer.bAccountID>>,
				LeftJoin<Contract, On<Contract.contractID, Equal<CRCase.contractID>>,
				LeftJoin<CRCaseClass, On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>>>>,
				Where<CRCase.isBillable, Equal<True>,
					And<CRCase.majorStatus, Equal<CRCaseMajorStatusesAttribute.closed>,
					And2<Where<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perCase>, Or<CRCaseClass.perItemBilling, IsNull>>,
					And2<Where<CRCase.released, NotEqual<True>, Or<CRCase.released, IsNull>>,
					And2<Where<Current<CaseFilter.caseClassID>, IsNull, 
						Or<CRCase.caseClassID, Equal<Current<CaseFilter.caseClassID>>>>,
					And2<Where<Current<CaseFilter.customerClassID>, IsNull, 
						Or<Customer.customerClassID, Equal<Current<CaseFilter.customerClassID>>>>,
					And2<Where<Current<CaseFilter.customerID>, IsNull, 
						Or<Customer.bAccountID, Equal<Current<CaseFilter.customerID>>>>,
					And<Where<Current<CaseFilter.contractID>, IsNull, 
						Or<Contract.contractID, Equal<Current<CaseFilter.contractID>>>>>>>>>>>>> 
			Items;

		[PXHidden]
		public PXSelect<BAccount> BaseAccounts;

		[PXHidden]
		public PXSelect<CRCase> BaseCases;

		public CRCaseReleaseProcess()
		{
			Items.SetSelected<CRCase.selected>();

			PXProcessingStep[] targets = PXAutomation.GetProcessingSteps(this);
			if (targets.Length > 0)
			{
				Items.SetProcessTarget(targets[0].GraphName,
					targets.Length > 1 ? null : targets[0].Name,
					targets[0].Actions[0].Name,
					targets[0].Actions[0].Menus[0],
					null, null);
			}
			else
			{
				throw new PXException(SO.Messages.MissingMassProcessWorkFlow);
			}

			PXUIFieldAttribute.SetVisible(Items.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CRCase.caseCD>(Items.Cache, null);
			PXUIFieldAttribute.SetVisible<CRCase.subject>(Items.Cache, null);
			PXUIFieldAttribute.SetVisible<CRCase.contractID>(Items.Cache, null);
			PXUIFieldAttribute.SetVisible<CRCase.timeBillable>(Items.Cache, null);
			PXUIFieldAttribute.SetVisible<CRCase.overtimeBillable>(Items.Cache, null);
			
			var BAccountCache = Caches[typeof(Customer)];
			PXUIFieldAttribute.SetVisible(BAccountCache, null, false);
			PXUIFieldAttribute.SetDisplayName<Customer.acctName>(BAccountCache, Messages.BAccountName);
			PXUIFieldAttribute.SetDisplayName<Customer.classID>(BAccountCache, Messages.BAccountClass);
			
			Actions.Move("Process", "Cancel");
		}

		#region Actions

		public PXCancel<CaseFilter>
			Cancel;

		#endregion

		#region Case Event Handlers

		[CRCaseBillableTime]
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Billable Time", Enabled = false)]
		public virtual void CRCase_TimeBillable_CacheAttached(PXCache sender)
		{

		}

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Billable Overtime")]
        public virtual void CRCase_OvertimeBillable_CacheAttached(PXCache sender)
        {

        }

		#endregion
	}
}
