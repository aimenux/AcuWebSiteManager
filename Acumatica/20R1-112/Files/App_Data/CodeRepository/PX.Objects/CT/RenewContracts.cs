using System;
using PX.Data;
using System.Collections;
using PX.Objects.AR;
using PX.Objects.GL;

namespace PX.Objects.CT
{	
	[TableAndChartDashboardType]
	public class RenewContracts : PXGraph<RenewContracts>
	{
		public PXCancel<RenewalContractFilter> Cancel;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<RenewalContractFilter> EditDetail;
		public PXFilter<RenewalContractFilter> Filter;
		public PXFilteredProcessing<
			ContractsList, RenewalContractFilter> 
			Items;

		public RenewContracts()
		{
			Items.SetSelected<ContractsList.selected>();
		}

		protected virtual IEnumerable items()
		{
			RenewalContractFilter filter = Filter.Current;
			BqlCommand select = new Select<ContractsList,
				Where<Add<ContractsList.expireDate, IsNull<Minus<ContractsList.autoRenewDays>, Zero>>, LessEqual<Current<RenewalContractFilter.renewalDate>>>,
				OrderBy<Asc<ContractsList.contractCD>>>();


			if (!string.IsNullOrEmpty(filter.CustomerClassID))
			{
				select = 
					select.WhereAnd<
						Where<ContractsList.customerClassID, Equal<Current<RenewalContractFilter.customerClassID>>>>();
			}

			if (filter.TemplateID != null)
			{
				select =
					select.WhereAnd<
						Where<ContractsList.templateID, Equal<Current<RenewalContractFilter.templateID>>>>();
			}			

			return this.QuickSelect(select);			
		}

		#region Actions

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable editDetail(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
				target.Clear();
				target.Contracts.Current = PXSelect<
					CT.Contract, 
					Where<CT.Contract.contractID, Equal<Current<ContractsList.contractID>>>>
					.Select(this);
				throw new PXRedirectRequiredException(target, true, "ViewContract"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}

		#endregion
			
		#region EventHandlers
		protected virtual void RenewalContractFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void RenewalContractFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			RenewalContractFilter filter = Filter.Current;
			Items.SetProcessDelegate<ContractMaint>(delegate(ContractMaint graph, ContractsList item)
			{
				RenewContract(graph, item, filter);
			});
		}

		[Obsolete()]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void ContractsList_StartDate_CacheAttached(PXCache sender) {}

		protected virtual void ContractsList_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ContractsList contract = e.Row as ContractsList;
			if (contract == null) return;

			if (contract.NextDate < contract.ExpireDate)
			{
				PXUIFieldAttribute.SetEnabled<ContractsList.selected>(sender, contract, false);
				sender.RaiseExceptionHandling<ContractsList.selected>(contract, null, new PXSetPropertyException(Messages.BillContractBeforeRenewal, PXErrorLevel.RowWarning));
			}
		}

		#endregion

		public static void RenewContract(ContractMaint docgraph, ContractsList item, RenewalContractFilter filter)
		{
			docgraph.Contracts.Current = PXSelect<
				Contract, 
				Where<Contract.contractID, Equal<Required<Contract.contractID>>>>
				.Select(docgraph, item.ContractID);
			docgraph.Billing.Current = docgraph.Billing.Select();
			docgraph.RenewContract(filter.RenewalDate.Value);
		}

		#region Local Types

		[Serializable]
		public partial class RenewalContractFilter : IBqlTable
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
			#region RenewalDate
			public abstract class renewalDate : PX.Data.BQL.BqlDateTime.Field<renewalDate> { }
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Renewal Date")]
			public virtual DateTime? RenewalDate { get; set; }
			#endregion
		}
				
	
		[Serializable]
		[PXProjection(typeof(Select2<
			Contract,
			InnerJoin<ContractBillingSchedule, 
				On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>,
			InnerJoin<Customer, 
				On<Customer.bAccountID, Equal<Contract.customerID>>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>,
				And<Contract.type, NotEqual<Contract.type.unlimited>,
				And<Contract.autoRenew, Equal<True>,
				And2<Not<Exists<
					Select<ChildContract,
					Where<ChildContract.originalContractID, Equal<Contract.contractID>,
						And<Where<Contract.type, Equal<Contract.type.renewable>,
								Or<Contract.type, Equal<Contract.type.expiring>>>>>
						>>
					>,
				And<Where<Contract.status, Equal<Contract.status.active>,
					Or<Contract.status, Equal<Contract.status.expired>>>>>>>>>))]
		public partial class ContractsList : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
			public bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion

			#region ContractID
			public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
			protected Int32? _ContractID;
			[Contract(BqlTable = typeof(Contract))]
			public virtual Int32? ContractID
			{
				get
				{
					return this._ContractID;
				}
				set
				{
					this._ContractID = value;
				}
			}
			#endregion
			#region ContractCD
			public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
			protected String _ContractCD;
			[PXDimension(ContractAttribute.DimensionName)]
			[PXDBString(IsUnicode = true, IsKey = true, InputMask = "", BqlTable = typeof(Contract), IsFixed = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible)]			
			public virtual String ContractCD
			{
				get
				{
					return this._ContractCD;
				}
				set
				{
					this._ContractCD = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXDBInt(BqlTable = typeof(Contract))]
			[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(Customer.bAccountID), SubstituteKey = typeof(Customer.acctCD))]
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

			#region CustomerName
			public abstract class customerName : PX.Data.BQL.BqlString.Field<customerName> { }
			protected string _CustomerName;
			[PXDBString(60, IsUnicode = true, BqlField = typeof(Customer.acctName))]
			[PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.Visible)]
			public virtual string CustomerName
			{
				get
				{
					return this._CustomerName;
				}
				set
				{
					this._CustomerName = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			/// <summary>
			/// Identifier of the <see cref="CustomerClass">customer class</see> 
			/// to which the customer belongs.
			/// </summary>
			[PXDBString(10, IsUnicode = true, BqlTable = typeof(Customer))]
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
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXDBString(60, IsUnicode = true, BqlTable = typeof(Contract))]
			[PXDefault()]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion
			#region AutoRenewDays
			public abstract class autoRenewDays : PX.Data.BQL.BqlInt.Field<autoRenewDays> { }
			protected Int32? _AutoRenewDays;
			[PXDBInt(MinValue = 0, MaxValue = 365, BqlTable = typeof(Contract))]
			[PXUIField(DisplayName = "Renewal Point")]
			public virtual Int32? AutoRenewDays
			{
				get
				{
					return this._AutoRenewDays;
				}
				set
				{
					this._AutoRenewDays = value;
				}
			}
			#endregion
			#region LastDate
			public abstract class lastDate : PX.Data.BQL.BqlDateTime.Field<lastDate> { }
			protected DateTime? _LastDate;
			[PXDBDate(BqlTable = typeof(ContractBillingSchedule))]
			[PXUIField(DisplayName = "Last Billing Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? LastDate
			{
				get
				{
					return this._LastDate;
				}
				set
				{
					this._LastDate = value;
				}
			}
			#endregion

			#region NextDate
			public abstract class nextDate : PX.Data.BQL.BqlDateTime.Field<nextDate> { }
			protected DateTime? _NextDate;
			[PXDBDate(BqlTable = typeof(ContractBillingSchedule))]
			[PXUIField(DisplayName = "Next Billing Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? NextDate
			{
				get
				{
					return this._NextDate;
				}
				set
				{
					this._NextDate = value;
				}
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			protected DateTime? _StartDate;
			[PXDBDate(BqlTable = typeof(Contract))]			
			[PXUIField(DisplayName = "Setup Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
			#region ExpireDate
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			protected DateTime? _ExpireDate;
			[PXDBDate(BqlTable = typeof(Contract))]
			[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? ExpireDate
			{
				get
				{
					return this._ExpireDate;
				}
				set
				{
					this._ExpireDate = value;
				}
			}
			#endregion

			#region Type
			public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
			protected String _Type;
			[PXDBString(1, IsFixed = true, BqlTable = typeof(Contract))]
			[PXUIField(DisplayName = "Contract Type", Visibility = PXUIVisibility.Visible)]
			[Contract.type.List]
			[PXDefault(Contract.type.Renewable)]
			public virtual String Type
			{
				get
				{
					return this._Type;
				}
				set
				{
					this._Type = value;
				}
			}
			#endregion

			#region TemplateID
			public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
			protected Int32? _TemplateID;
			[ContractTemplate(BqlTable = typeof(Contract))]
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

			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			protected String _Status;
			[PXDBString(1, IsFixed = true , BqlTable = typeof(Contract))]
			[Contract.status.List]
			[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.Visible)]
			public virtual String Status
			{
				get
				{
					return this._Status;
				}
				set
				{
					this._Status = value;
				}
			}
			#endregion

			

		}

		[Serializable]
		[PXHidden]
		public class ChildContract : Contract
		{
			#region ContractID
			public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
			#endregion
			#region ContractCD
			public new abstract class contractCD : IBqlField { }
			#endregion
			#region OriginalContractID
			public new abstract class originalContractID : PX.Data.BQL.BqlInt.Field<originalContractID> { }
			#endregion
			#region MasterContractID
			public new abstract class masterContractID : PX.Data.BQL.BqlInt.Field<masterContractID> { }
			#endregion		
		}
		#endregion
	}
}
