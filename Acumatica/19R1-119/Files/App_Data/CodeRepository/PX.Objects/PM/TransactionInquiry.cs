using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using System.Collections;
using PX.Objects.GL;
using PX.Web.UI;
using PX.Objects.EP;
using PX.Objects.CM;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
    [Serializable]
	public class TransactionInquiry : PXGraph<TransactionInquiry>
	{
		#region DAC Attributes Override

		#region PMTran

		[AccountGroup(DisplayName="Debit Account Group")]
		protected virtual void PMTran_AccountGroupID_CacheAttached(PXCache sender)
		{
		}

		[PXDBBaseCury]
		[PXUIField(DisplayName = "Amount (Reversed)")]
		protected virtual void PMTran_TranCuryAmount_CacheAttached(PXCache sender)
		{
		}

		#endregion

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMCostCode_IsProjectOverride_CacheAttached(PXCache sender)
		{
		}

		#region ARTran

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Invoice Ref. Nbr.", Visibility = PXUIVisibility.Visible)]
		protected virtual void ARTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#endregion

		public PXFilter<TranFilter> Filter;
		public PXCancel<TranFilter> Cancel;

	    public PXSelect<ARTran> Dummy;
		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;
		public PXSetup<PMProject, Where<PMProject.contractID, Equal<Current<TranFilter.projectID>>>> Project;

		[PXFilterable]
		public PXSelectJoin<PMTran,
				LeftJoin<Account, On<Account.accountID, Equal<PMTran.offsetAccountID>>,
				LeftJoin<PMRegister, On<PMTran.tranType, Equal<PMRegister.module>, And<PMTran.refNbr, Equal<PMRegister.refNbr>>>,
				LeftJoin<ARTran, On<ARTran.tranType, Equal<PMTran.aRTranType>, And<ARTran.refNbr, Equal<PMTran.aRRefNbr>, And<ARTran.lineNbr, Equal<PMTran.refLineNbr>>>>>>>,
				Where<PMTran.projectID, Equal<Current<TranFilter.projectID>>>> Transactions;


		public virtual IEnumerable transactions()
		{
            PXSelectBase<PMTran> select = new PXSelectJoin<PMTran,
                LeftJoin<Account, On<Account.accountID, Equal<PMTran.offsetAccountID>>,
                LeftJoin<PMRegister, On<PMTran.tranType, Equal<PMRegister.module>, And<PMTran.refNbr, Equal<PMRegister.refNbr>>>,
				LeftJoin<ARTran, On<ARTran.tranType, Equal<PMTran.aRTranType>, And<ARTran.refNbr, Equal<PMTran.aRRefNbr>, And<ARTran.lineNbr, Equal<PMTran.refLineNbr>>>>>>>,
                Where<PMTran.projectID, Equal<Current<TranFilter.projectID>>>>(this);

			TranFilter filter = this.Filter.Current;
			if (filter != null)
			{
                if(filter.AccountGroupID != null)
                {
                    select.WhereAnd<Where<PMTran.accountGroupID, Equal<Current<TranFilter.accountGroupID>>, Or<Account.accountGroupID, Equal<Current<TranFilter.accountGroupID>>>>>();
                }
                if(filter.ProjectTaskID != null)
                {
                    select.WhereAnd<Where<PMTran.taskID, Equal<Current<TranFilter.projectTaskID>>>>();
                }
				if (filter.CostCode != null)
				{
					select.WhereAnd<Where<PMTran.costCodeID, Equal<Current<TranFilter.costCode>>>>();
				}
				if (filter.InventoryID != null)
				{
					select.WhereAnd<Where<PMTran.inventoryID, Equal<Current<TranFilter.inventoryID>>>>();
				}
				if (filter.ResourceID != null)
				{
					select.WhereAnd<Where<PMTran.resourceID, Equal<Current<TranFilter.resourceID>>>>();
				}
				if (filter.OnlyAllocation == true)
				{
					select.WhereAnd<Where<PMRegister.isAllocation, Equal<True>>>();
				}
				if (filter.IncludeUnreleased.GetValueOrDefault() == false)
				{
					select.WhereAnd<Where<PMRegister.released, Equal<True>>>();
				}
				if (filter.DateFrom != null && filter.DateTo != null)
				{
					if (filter.DateFrom == filter.DateTo)
					{
						select.WhereAnd<Where<PMTran.date, Equal<Current<TranFilter.dateFrom>>>>();
					}
					else
					{
						select.WhereAnd<Where<PMTran.date, Between<Current<TranFilter.dateFrom>, Current<TranFilter.dateTo>>>>();
					}
				}
				else if (filter.DateFrom != null)
				{
					select.WhereAnd<Where<PMTran.date, GreaterEqual<Current<TranFilter.dateFrom>>>>();
				}
				else if (filter.DateTo != null)
				{
					select.WhereAnd<Where<PMTran.date, LessEqual<Current<TranFilter.dateTo>>>>();
				}
			}

			return TimeCardMaint.QSelect(this, select.View.BqlSelect);
		}

        public TransactionInquiry()
        {
            Transactions.Cache.AllowInsert = false;
            Transactions.Cache.AllowUpdate = false;
            Transactions.Cache.AllowDelete = false;

            PXUIFieldAttribute.SetDisplayName<Account.accountGroupID>(Caches[typeof(Account)], Messages.CreditAccountGroup);
        }

        public PXAction<TranFilter> viewDocument;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
        [PXButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            RegisterEntry graph = CreateInstance<RegisterEntry>();
            graph.Document.Current = graph.Document.Search<PMRegister.refNbr>(Transactions.Current.RefNbr, Transactions.Current.TranType);
            throw new PXRedirectRequiredException(graph, "PMTransactiosn"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
        }

        public PXAction<TranFilter> viewInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
        [PXButton]
        public virtual IEnumerable ViewInventory(PXAdapter adapter)
        {
            InventoryItem inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMTran.inventoryID>>>>.SelectSingleBound(this, new object[] { Transactions.Current });
            if(inv != null && inv.StkItem == true)
            {
                InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
                graph.Item.Current = inv;
                throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if(inv != null)
            {
                NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
                graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(inv.InventoryID);
                throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }


		public PXAction<TranFilter> viewInvoice;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			ARTran arTran = PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<PMTran.aRTranType>>, 
				And<ARTran.refNbr, Equal<Current<PMTran.aRRefNbr>>, 
				And<ARTran.lineNbr, Equal<Current<PMTran.refLineNbr>>>>>>.SelectSingleBound(this, new object[] { Transactions.Current });
			if (arTran != null)
			{
				ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
				target.Clear();
				target.Document.Current = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARTran.tranType>>, And<ARInvoice.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(this, arTran.TranType, arTran.RefNbr);
				throw new PXRedirectRequiredException(target, "Invoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			
			return adapter.Get();
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryID> e)
		{
			if (Project.Current != null)
				e.ReturnValue = Project.Current.CuryID;
		}

		protected virtual void _(Events.RowSelected<PMTran> e)
		{
			if (e.Row != null)
			{
				PMAccountGroup accGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMTran.accountGroupID>>>>.Select(this, e.Row.AccountGroupID);
					
				if (accGroup != null && accGroup.Type == AccountType.Income || accGroup.Type == AccountType.Liability)
				{
					e.Row.TranCuryAmountNormal = e.Row.TranCuryAmount * -1;
				}
				else
				{
					e.Row.TranCuryAmountNormal = e.Row.TranCuryAmount;
				}
			}
		}

		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class TranFilter : IBqlTable
		{
			#region AccountGroupID
			public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
			protected Int32? _AccountGroupID;
			[AccountGroupAttribute()]
			public virtual Int32? AccountGroupID
			{
				get
				{
					return this._AccountGroupID;
				}
				set
				{
					this._AccountGroupID = value;
				}
			}
			#endregion
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
			[PXDefault]
			[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, Equal<False>>>), WarnIfCompleted = false)]
			public virtual Int32? ProjectID
			{
				get
				{
					return this._ProjectID;
				}
				set
				{
					this._ProjectID = value;
				}
			}
			#endregion
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(TranFilter.projectID))]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region CostCode
			public abstract class costCode : PX.Data.BQL.BqlInt.Field<costCode> { }
			[CostCode(Filterable = false, SkipVerification = true)]
			public virtual Int32? CostCode
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[PXDBInt]
			[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
			[PMInventorySelector(Filterable = true)]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region ResourceID
			public abstract class resourceID : PX.Data.BQL.BqlInt.Field<resourceID> { }
			protected Int32? _ResourceID;
			[PXEPEmployeeSelector]
			[PXDBInt()]
			[PXUIField(DisplayName = "Employee")]
			public virtual Int32? ResourceID
			{
				get
				{
					return this._ResourceID;
				}
				set
				{
					this._ResourceID = value;
				}
			}
			#endregion
			#region DateFrom
			public abstract class dateFrom : PX.Data.BQL.BqlDateTime.Field<dateFrom> { }
			protected DateTime? _DateFrom;
			[PXDBDate()]
			[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? DateFrom
			{
				get
				{
					return this._DateFrom;
				}
				set
				{
					this._DateFrom = value;
				}
			}
			#endregion
			#region DateTo
			public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }
			protected DateTime? _DateTo;
			[PXDBDate()]
			[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? DateTo
			{
				get
				{
					return this._DateTo;
				}
				set
				{
					this._DateTo = value;
				}
			}
			#endregion
			#region OnlyAllocation
			public abstract class onlyAllocation : PX.Data.BQL.BqlBool.Field<onlyAllocation> { }
			protected Boolean? _OnlyAllocation;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show only Allocation Transactions")]
			public virtual Boolean? OnlyAllocation
			{
				get
				{
					return this._OnlyAllocation;
				}
				set
				{
					_OnlyAllocation = value;
				}
			}
			#endregion
			#region IncludeUnreleased
			public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }
			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Include Unreleased Transactions")]
			public virtual Boolean? IncludeUnreleased
			{
				get;
				set;
			}
			#endregion
		}
	}

	
}
