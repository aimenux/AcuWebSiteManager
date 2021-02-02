using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using System.Diagnostics;
using PX.Objects.CR;
using System.Collections;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.EP;

namespace PX.Objects.CT
{
	[TableAndChartDashboardType]
	public class ContractBilling : PXGraph<ContractBilling>
	{
		public PXCancel<BillingFilter> Cancel;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<BillingFilter> EditDetail;
		public PXFilter<BillingFilter> Filter;
        public PXFilteredProcessingJoin<Contract, BillingFilter, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>,
                LeftJoin<Customer, On<Contract.customerID, Equal<Customer.bAccountID>>>>,
                Where2<Where<ContractBillingSchedule.nextDate, LessEqual<Current<BillingFilter.invoiceDate>>,Or<ContractBillingSchedule.type, Equal<BillingType.BillingOnDemand>>>,
                And<Contract.baseType, Equal<CTPRType.contract>,
                And<Contract.isCancelled, Equal<False>,
                And<Contract.isCompleted, Equal<False>,
                And<Contract.isActive, Equal<True>,
                And2<Where<Current<BillingFilter.templateID>, IsNull, Or<Current<BillingFilter.templateID>, Equal<Contract.templateID>>>,
                And2<Where<Current<BillingFilter.customerClassID>, IsNull, Or<Current<BillingFilter.customerClassID>, Equal<Customer.customerClassID>>>,
                And<Where<Current<BillingFilter.customerID>, IsNull, Or<Current<BillingFilter.customerID>, Equal<Contract.customerID>>>>>>>>>>>> Items;

		public ContractBilling()
		{
            Items.SetSelected<Contract.selected>();
		}

        #region Actions

        [PXUIField(DisplayName = Messages.ViewContract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable editDetail(PXAdapter adapter)
        {
            if (Items.Current != null)
            {
                ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
                target.Clear();
	            target.Contracts.Current = PXSelect<CT.Contract,
													Where<CT.Contract.contractID, Equal<Required<Contract.contractID>>>>
													.Select(target, Items.Current.ContractID);

                throw new PXRedirectRequiredException(target, true, "ViewContract") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        #endregion

		#region EventHandlers
		protected virtual void BillingFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void BillingFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			BillingFilter filter = (BillingFilter)e.Row;
			Items.SetProcessDelegate(
                    delegate(Contract item)
                        {
                            CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
							ContractBillingSchedule schedule = PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Required<Contract.contractID>>>>.Select(engine, item.ContractID);

							if (schedule.Type == BillingType.OnDemand)
								engine.Bill(item.ContractID, filter.InvoiceDate);
							else
								engine.Bill(item.ContractID);
						});
		}

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXUIEnabledAttribute))]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void Contract_ExpireDate_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region Local Types


		[Serializable]
		public partial class BillingFilter : IBqlTable
		{
			#region InvoiceDate
			public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
			protected DateTime? _InvoiceDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Billing Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? InvoiceDate
			{
				get
				{
					return this._InvoiceDate;
				}
				set
				{
					this._InvoiceDate = value;
				}
			}
			#endregion

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

			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer(DescriptionField = typeof(Customer.acctName))]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
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
		}

		#endregion
	}
}
