using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;

namespace PX.Objects.CT
{
	[TableAndChartDashboardType]
	public class ExpiringContractsEng : PXGraph<ExpiringContractsEng>
	{
		public PXCancel<ExpiringContractFilter> Cancel;
		public PXAction<ExpiringContractFilter> viewContract;


		public PXFilter<ExpiringContractFilter> Filter;
		[PXFilterable]
		public PXSelectJoin<Contract, InnerJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>
			, InnerJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>,
			And<Contract.type, Equal<Contract.type.expiring>>>> Contracts;

		public ExpiringContractsEng()
		{
			Contracts.Cache.AllowDelete = false;
			Contracts.Cache.AllowUpdate = false;
			Contracts.Cache.AllowInsert = false;
		}

		public virtual IEnumerable contracts()
		{
			ExpiringContractFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<Contract> select = new PXSelectJoin<Contract
					, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>
					, InnerJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>,
					Where<Contract.baseType, Equal<CTPRType.contract>,
					And<Contract.expireDate, LessEqual<Current<ExpiringContractFilter.endDate>>,
					And<Contract.expireDate, GreaterEqual<Current<ExpiringContractFilter.beginDate>>, 
					And<Contract.status, NotEqual<Contract.status.canceled>>>>>>(this);
				
				if (filter.ShowAutoRenewable != true)
				{
					select.WhereAnd<Where<Contract.autoRenew, Equal<False>, Or<Contract.autoRenew, IsNull>>>();
				}
				
				if (!string.IsNullOrEmpty(filter.CustomerClassID))
				{
					select.WhereAnd<Where<Customer.customerClassID, Equal<Current<ExpiringContractFilter.customerClassID>>>>();
				}

				if (filter.TemplateID != null)
				{
					select.WhereAnd<Where<Contract.templateID, Equal<Current<ExpiringContractFilter.templateID>>>>();
				}

				/*
				 * Expiring Contracts has a hierarchical structure and we need to show only the latest expiring node hidding
				 * all of its original contracts
				*/
				foreach ( PXResult<Contract> result in select.Select())
				{
					bool skipItem = false;
					if (((Contract)result).Type == Contract.type.Expiring)
					{
						Contract child = PXSelect<Contract, Where<Contract.originalContractID, Equal<Required<Contract.originalContractID>>>>.Select(this, ((Contract)result).ContractID);
						skipItem = child != null;
					}

					if (!skipItem)
						yield return result;
				}
			}
			else
				yield break;

		}

		#region Action Delagates

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			if (Contracts.Current != null)
			{
				ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
				target.Clear();
				target.Contracts.Current = Contracts.Current;
				throw new PXRedirectRequiredException(target, true, "ViewContract") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		#endregion

		public virtual void ExpiringContractFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
		}

		#region Local Types
		[Serializable]
		public partial class ExpiringContractFilter : IBqlTable
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
			#region TemplateID
			public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
			protected Int32? _TemplateID;
			[ContractTemplate]
			public virtual Int32? TemplateID
			{
				get
				{
					return this._TemplateID;
				}
				set
				{
					this._TemplateID = value;
				}
			}
			#endregion
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
			protected DateTime? _BeginDate;
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? BeginDate
			{
				get
				{
					return _BeginDate;
				}
				set
				{
					_BeginDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date")]
			public virtual DateTime? EndDate { get; set; }
			#endregion
			#region ExpireXDays
			public abstract class expireXDays : PX.Data.BQL.BqlInt.Field<expireXDays> { }

			protected Int32? _ExpireXDays;

			[PXDBInt()]
			[PXDefault(30)]
			[PXUIField(DisplayName = "Duration")]
			public virtual Int32? ExpireXDays
			{
				get
				{
					return this._ExpireXDays;
				}
				set
				{
					this._ExpireXDays = value;
				}
			}
			#endregion
			#region ShowAutoRenewable
			public abstract class showAutoRenewable : PX.Data.BQL.BqlBool.Field<showAutoRenewable> { }
			protected bool? _ShowAutoRenewable;
			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Show Contracts Available for Mass Renewal")]
			public virtual bool? ShowAutoRenewable
			{
				get
				{
					return _ShowAutoRenewable;
				}
				set
				{
					_ShowAutoRenewable = value;
				}
			}
			#endregion
		}
				
		#endregion
	}

	
}
