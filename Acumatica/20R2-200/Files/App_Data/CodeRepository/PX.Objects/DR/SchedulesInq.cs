using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.IN;


namespace PX.Objects.DR
{
	[TableAndChartDashboardType]
	[Serializable]
	public class SchedulesInq : PXGraph<SchedulesInq>
	{
		#region Selects and Actions
		public PXCancel<SchedulesFilter> Cancel;
		public PXFilter<SchedulesFilter> Filter;

		public PXSelectReadonly<SchedulesInqResult> Records;
		public PXSetup<DRSetup> Setup;

		protected virtual int[] FilteringBranchIDs
		{
			get
			{
				int[] branchIDs = null;
				if (Filter.Current.BranchID != null)
				{
					branchIDs = new int[] { (int)Filter.Current.BranchID };
				}
				else if (Filter.Current.OrganizationID != null)
				{
					branchIDs = PXAccess.GetChildBranchIDs(Filter.Current.OrganizationID);
				}
				return branchIDs;
			}
		}

		protected virtual IEnumerable filter()
		{
			PXCache cache = this.Caches[typeof(SchedulesFilter)];
			if (cache != null)
			{
				SchedulesFilter filter = cache.Current as SchedulesFilter;
				if (filter != null)
				{
					int startRow = 0;
					int totalRows = 0;

					filter.TotalDeferred = 0;
					filter.TotalScheduled = 0;

					BqlCommand cmd = ComposeBQLCommandForRecords(filter);
					if(cmd == null)
						yield return cache.Current;

					cmd = cmd.AggregateNew<Aggregate<
								Sum<SchedulesInqResult.signTotalAmt, 
								Sum<SchedulesInqResult.signDefAmt>>>>();

					PXView view = new PXView(this, true, cmd);

					List<object> record = view.Select(new[] { filter },
									null,
									null,
									null,
									null,
									Records.View.GetExternalFilters(),
									ref startRow,
									0,
									ref totalRows);

					if (record.Count > 0)
					{
						PXResult<SchedulesInqResult, DRSchedule, DRDeferredCode, InventoryItem> rec = 
							(PXResult<SchedulesInqResult, DRSchedule, DRDeferredCode, InventoryItem>)record[0];
						filter.TotalScheduled = ((SchedulesInqResult)rec).SignTotalAmt;
						filter.TotalDeferred = ((SchedulesInqResult)rec).SignDefAmt;
					}
				}
			}
			yield return cache.Current;
			cache.IsDirty = false;
		}

		public virtual IEnumerable records()
		{
			var ret = new List<SchedulesInqResult>();

			int startRow = PXView.StartRow;
			int totalRows = 0;

			SchedulesFilter filter = this.Filter.Current;

			BqlCommand cmd = ComposeBQLCommandForRecords(filter);
			if (cmd == null)
				return ret;

			PXView view = new PXView(this, true, cmd);

			foreach (PXResult<SchedulesInqResult, DRSchedule, DRDeferredCode, InventoryItem> record in
					view.Select(
						PXView.Currents,
						null,
						PXView.Searches,
						PXView.SortColumns,
						PXView.Descendings,
						PXView.Filters,
						ref startRow,
						PXView.MaximumRows,
						ref totalRows))
			{
				SchedulesInqResult schedulesInqResult = (SchedulesInqResult)record;
				InventoryItem inventoryItem = (InventoryItem)record;

				schedulesInqResult.ComponentCD = inventoryItem.InventoryCD;
				schedulesInqResult.DocumentType = DRScheduleDocumentType.BuildDocumentType(schedulesInqResult.Module, schedulesInqResult.DocType);

				ret.Add(schedulesInqResult);
			}

			PXView.StartRow = 0;

			return ret;
		}

		public virtual BqlCommand ComposeBQLCommandForRecords(SchedulesFilter filter)
		{
			if (filter == null)
				return null;

			PXSelectBase<SchedulesInqResult> select = new PXSelectJoin<SchedulesInqResult,
				InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<SchedulesInqResult.scheduleID>>,
				InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<SchedulesInqResult.defCode>>,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SchedulesInqResult.componentID>>>>>,
				Where<DRDeferredCode.accountType, Equal<Current<SchedulesFilter.accountType>>>>(this);

			if (!string.IsNullOrEmpty(filter.DeferredCode))
			{
				select.WhereAnd<Where<SchedulesInqResult.defCode, Equal<Current<SchedulesFilter.deferredCode>>>>();
			}
						
			if (filter.OrgBAccountID != null)
			{
				select.WhereAnd<Where<SchedulesInqResult.branchID, Inside<Current<SchedulesFilter.orgBAccountID>>>>(); //MatchWithOrg
			}
			
			if (filter.AccountID != null)
			{
				select.WhereAnd<Where<SchedulesInqResult.defAcctID, Equal<Current<SchedulesFilter.accountID>>>>();
			}

			if (filter.SubID != null)
			{
				select.WhereAnd<Where<SchedulesInqResult.defSubID, Equal<Current<SchedulesFilter.subID>>>>();
			}

			if (filter.BAccountID != null)
			{
				select.WhereAnd<Where<SchedulesInqResult.bAccountID, Equal<Current<SchedulesFilter.bAccountID>>>>();
			}

			if (filter.ComponentID != null)
			{
				select.WhereAnd<Where<SchedulesInqResult.componentID, Equal<Current<SchedulesFilter.componentID>>>>();
			}

			return select.View.BqlSelect;
		}

		/// <summary>
		/// Explicitly instantiate the business account cache to 
		/// rename the <see cref="BAccountR.acctName"/> column in the 
		/// <see cref="BAccountR_AcctName_CacheAttached(PXCache)"/> handler.
		/// </summary>
		public PXSelect<BAccountR> DummyBusinessAccount;

		public PXAction<SchedulesFilter> viewDocument;
		[PXUIField(DisplayName = "")]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Records.Current != null)
			{
				DRRedirectHelper.NavigateToOriginalDocument(this, Records.Current);
			}
			return adapter.Get();
		}
		#endregion

		public SchedulesInq()
		{
			DRSetup setup = Setup.Current;
		}

		#region Cache Attached
		[PXCustomizeBaseAttribute(
			typeof(PXUIFieldAttribute), 
			nameof(PXUIFieldAttribute.DisplayName), 
			Messages.BusinessAccountName)]
		protected virtual void BAccountR_AcctName_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(BranchAttribute), nameof(BranchAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void SchedulesInqResult_BranchID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void SchedulesInqResult_DefCode_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void SchedulesInqResult_DefAcctID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void SchedulesInqResult_DefSubID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void SchedulesInqResult_AccountID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDefaultAttribute), nameof(PXDefaultAttribute.PersistingCheck), PXPersistingCheck.Nothing)]
		protected virtual void SchedulesInqResult_SubID_CacheAttached(PXCache sender) { }
		#endregion

		protected virtual void SchedulesFilter_AccountType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SchedulesFilter row = e.Row as SchedulesFilter;
			if (row != null)
			{				
				row.BAccountID = null;
				row.DeferredCode = null;
				row.AccountID = null; 
				row.SubID = null;
			}
		}


		#region Local Types
		[Serializable]
		public partial class SchedulesFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[Organization(
				onlyActive: false,
				Required = false)]
			public virtual int? OrganizationID { get; set; }
			#endregion

			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[BranchOfOrganization(
				typeof(SchedulesFilter.organizationID),
				onlyActive: false,
				Required = false,
				PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			public int? OrgBAccountID { get; set; }
			#endregion
			#region AccountType
			public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }
			protected string _AccountType;
			[PXDBString(1)]
			[PXDefault(DeferredAccountType.Income)]
			[LabelList(typeof(DeferredAccountType))]
			[PXUIField(DisplayName = "Code Type", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string AccountType
			{
				get
				{
					return this._AccountType;
				}
				set
				{
					this._AccountType = value;

					switch (value)
					{
						case DeferredAccountType.Expense:
							_BAccountType = CR.BAccountType.VendorType;
							break;
						default:
							_BAccountType = CR.BAccountType.CustomerType;
							break;
					}
				}
			}
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[Account(DisplayName = "Deferral Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
			public virtual Int32? AccountID
			{
				get
				{
					return this._AccountID;
				}
				set
				{
					this._AccountID = value;
				}
			}
			#endregion
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			protected Int32? _SubID;
			[SubAccount(typeof(SchedulesFilter.accountID), DisplayName = "Deferral Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			public virtual Int32? SubID
			{
				get
				{
					return this._SubID;
				}
				set
				{
					this._SubID = value;
				}
			}
			#endregion
			#region DeferredCode
			public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
			protected String _DeferredCode;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Deferral Code")]
            [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<SchedulesFilter.accountType>>>>),
                        typeof(DRDeferredCode.deferredCodeID),
                        typeof(DRDeferredCode.description),
                        typeof(DRDeferredCode.accountType),
                        typeof(DRDeferredCode.accountID),
                        typeof(DRDeferredCode.subID),
                        typeof(DRDeferredCode.method),
                        typeof(DRDeferredCode.active)
                        )]
            public virtual String DeferredCode
			{
				get
				{
					return this._DeferredCode;
				}
				set
				{
					this._DeferredCode = value;
				}
			}
			#endregion

			#region BAccountType
			public abstract class bAccountType : PX.Data.BQL.BqlString.Field<bAccountType> { }
			protected String _BAccountType;
			[PXDefault(CR.BAccountType.CustomerType)]
			[PXString(2, IsFixed = true)]
			[PXStringList(new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType },
					new string[] { CR.Messages.VendorType, CR.Messages.CustomerType })]
			public virtual String BAccountType
			{
				get
				{
					return this._BAccountType;
				}
				set
				{
					this._BAccountType = value;
				}
			}
			#endregion
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt]
			[PXUIField(DisplayName = Messages.BusinessAccount)]
			[PXSelector(
				typeof(Search<
					BAccountR.bAccountID, 
					Where<BAccountR.type, Equal<Current<SchedulesFilter.bAccountType>>>>), 
				new Type[] 
				{
					typeof(BAccountR.acctCD),
					typeof(BAccountR.acctName),
					typeof(BAccountR.type)
				}, 
				SubstituteKey = typeof(BAccountR.acctCD))]
			public virtual int? BAccountID
			{
				get;
				set;
			}
			#endregion
			#region ComponentID
			public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
			protected Int32? _ComponentID;
						
			[AnyInventory(DisplayName="Component")]
			public virtual Int32? ComponentID
			{
				get
				{
					return this._ComponentID;
				}
				set
				{
					this._ComponentID = value;
				}
			}

			#endregion
			#region TotalScheduled
			public abstract class totalScheduled : PX.Data.BQL.BqlDecimal.Field<totalScheduled> { }
			[PXDecimal(2)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Scheduled", Enabled = false)]
			public virtual decimal? TotalScheduled { get; set; }
			#endregion
			#region TotalDeferred
			public abstract class totalDeferred : PX.Data.BQL.BqlDecimal.Field<totalDeferred> { }
			[PXDecimal(2)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Deferred", Enabled = false)]
			public virtual decimal? TotalDeferred { get; set; }
			#endregion
		}

		[Serializable]
		public partial class SchedulesInqResult : DRScheduleDetail
		{
			#region ComponentID
			public new abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = Messages.ComponentID, Visibility = PXUIVisibility.Visible)]
			public override Int32? ComponentID
			{
				get
				{
					return this._ComponentID;
				}
				set
				{
					this._ComponentID = value;
				}
			}

			#endregion
			#region ComponentCD
			public abstract class componentCD : PX.Data.BQL.BqlString.Field<componentCD> { }
			protected string _ComponentCD;

			[PXString]
			[PXUIField(DisplayName = Messages.ComponentID, Visibility = PXUIVisibility.Visible)]
			public virtual string ComponentCD
			{
				get
				{
					return this._ComponentCD;
				}
				set
				{
					this._ComponentCD = value;
				}
			}

			#endregion
			#region SignTotalAmt
			public abstract class signTotalAmt : PX.Data.BQL.BqlDecimal.Field<signTotalAmt> { }
			[PXBaseCury]
			[PXFormula(typeof(Switch<
								Case<Where<Current<SchedulesFilter.accountType>, Equal<AccountType.income>,
										And<SchedulesInqResult.docType, Equal<ARDocType.creditMemo>,
										Or<Current<SchedulesFilter.accountType>, Equal<AccountType.expense>,
										And<SchedulesInqResult.docType, Equal<APDocType.debitAdj>>>>>,
							Minus<SchedulesInqResult.totalAmt>>,
							SchedulesInqResult.totalAmt>
			))]
			[PXUIField(DisplayName = "Total Amount")]
			public virtual decimal? SignTotalAmt {get; set;}
			#endregion
			#region SignDefAmt
			public abstract class signDefAmt : PX.Data.BQL.BqlDecimal.Field<signDefAmt> { }
			[PXBaseCury]
			[PXFormula(typeof(Switch<
								Case<Where<Current<SchedulesFilter.accountType>, Equal<AccountType.income>,
										And<SchedulesInqResult.docType, Equal<ARDocType.creditMemo>,
										Or<Current<SchedulesFilter.accountType>, Equal<AccountType.expense>,
										And<SchedulesInqResult.docType, Equal<APDocType.debitAdj>>>>>,
							Minus<SchedulesInqResult.defAmt>>,
							SchedulesInqResult.defAmt>
			))]
			[PXUIField(DisplayName = "Deferred Amount", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual decimal? SignDefAmt	{get; set;}
			#endregion
		}

		#endregion
	}
}
