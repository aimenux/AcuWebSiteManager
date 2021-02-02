using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.IN;
using System.Collections;
using PX.Objects.GL;
using PX.Web.UI;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.Objects.Common;
using System.Collections.Generic;
using System.Linq;

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

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Invoice Ref. Nbr.", Visibility = PXUIVisibility.Visible)]
		protected virtual void PMTran_ARRefNbr_CacheAttached(PXCache sender)
		{
		}

		#endregion

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMCostCode_IsProjectOverride_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDecimal]
		[PXUIField(DisplayName = "Total Quantity", Enabled = false)]
		protected virtual void PMRegister_QtyTotal_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDecimal]
		[PXUIField(DisplayName = "Total Billable Quantity", Enabled = false)]
		protected virtual void PMRegister_BillableQtyTotal_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDecimal]
		[PXUIField(DisplayName = "Total Amount", Enabled = false)]
		protected virtual void PMRegister_AmtTotal_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public PXFilter<TranFilter> Filter;
		public PXCancel<TranFilter> Cancel;

	    public PXSelect<ARTran> Dummy;
		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<TranFilter.projectID.FromCurrent>> Project;

		[PXFilterable]
		public PXSelectJoin<PMTran,
				LeftJoin<Account, On<Account.accountID, Equal<PMTran.offsetAccountID>>,
				LeftJoin<PMRegister, On<PMTran.tranType, Equal<PMRegister.module>, And<PMTran.refNbr, Equal<PMRegister.refNbr>>>>>,
				Where<PMTran.projectID, Equal<Current<TranFilter.projectID>>>> Transactions;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMRegister> dummy;//Needed for CacheAttached to work on a Joined PMRegister

		public virtual IEnumerable transactions()
		{
			TranFilter filter = this.Filter.Current;
			if (filter != null && (
				filter.ProjectID != null ||
				filter.AccountID != null ||
				filter.AccountGroupID != null ||
				filter.ARRefNbr != null ||
				filter.TranID != null))
			{
				var parameters = new List<object>();
				PXSelectBase<PMTran> select = new PXSelectJoin<PMTran,
					LeftJoin<Account, On<Account.accountID, Equal<PMTran.offsetAccountID>>,
					LeftJoin<PMRegister, On<PMTran.tranType, Equal<PMRegister.module>, And<PMTran.refNbr, Equal<PMRegister.refNbr>>>>>,
					Where<True, Equal<True>>>(this);

				if (filter.ARRefNbr != null)
				{
					//nbrs of batches originated from invoice
					var refNbrs = PXSelectJoin<ARInvoice,
						InnerJoin<PMRegister, On<PMRegister.origNoteID, Equal<ARInvoice.noteID>>>,
						Where<ARRegister.docType, Equal<Current<TranFilter.aRDocType>>,
								And<ARRegister.refNbr, Equal<Current<TranFilter.aRRefNbr>>>>>.Select(this)
								.Select(r => ((PMRegister)(PXResult<ARInvoice, PMRegister>)r).RefNbr).ToArray();

					if (refNbrs.Any())
					{
						parameters.Add(refNbrs);
						select.WhereAnd<Where2
							<Where<PMTran.aRTranType, Equal<Current<TranFilter.aRDocType>>, And<PMTran.aRRefNbr, Equal<Current<TranFilter.aRRefNbr>>>>,
							Or
							<Where<PMTran.refNbr, In<Required<PMTran.refNbr>>>>>>();
					}
					else
					{
						select.WhereAnd<Where<PMTran.aRTranType, Equal<Current<TranFilter.aRDocType>>, And<PMTran.aRRefNbr, Equal<Current<TranFilter.aRRefNbr>>>>>();
					}
				}

				if (filter.TranID != null)
				{
					select.WhereAnd<Where<PMTran.tranID, Equal<Current<TranFilter.tranID>>>>();
				}

				if (filter.ProjectID != null)
				{
					select.WhereAnd<Where<PMTran.projectID, Equal<Current<TranFilter.projectID>>>>();
				}

				if (filter.AccountGroupID != null)
				{
					select.WhereAnd<Where<PMTran.accountGroupID, Equal<Current<TranFilter.accountGroupID>>, Or<Account.accountGroupID, Equal<Current<TranFilter.accountGroupID>>>>>();
				}

				if (filter.AccountID != null)
				{
					select.WhereAnd<Where<PMTran.accountID, Equal<Current<TranFilter.accountID>>, Or<Account.accountID, Equal<Current<TranFilter.accountID>>>>>();
				}

				if (filter.ProjectTaskID != null)
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
				return TimeCardMaint.QSelect(this, select.View.BqlSelect, parameters.ToArray());
			}
			return Enumerable.Empty<PMTran>();
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

		public PXAction<TranFilter> viewOrigDocument;
		[PXButton]
		[PXUIField]
		public virtual void ViewOrigDocument()
		{
			PMRegister doc = PXSelect<PMRegister, Where<PMRegister.refNbr, Equal<Current<PMTran.refNbr>>>>.Select(this);
			var helper = new EntityHelper(this);
			if(doc.OrigNoteID.HasValue)
				helper.NavigateToRow(doc.OrigNoteID.Value, PXRedirectHelper.WindowMode.NewWindow);
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

        public PXAction<TranFilter> viewCustomer;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewCustomer(PXAdapter adapter)
        {
            BAccount account = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);

            if (account != null)
            {
                if (account.Type == BAccountType.CustomerType || account.Type == BAccountType.CombinedType)
                {
                    CustomerMaint graph = CreateInstance<CustomerMaint>();
                    graph.BAccount.Current = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else if (account.Type == BAccountType.VendorType)
                {
                    VendorMaint graph = CreateInstance<VendorMaint>();
                    graph.BAccount.Current = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else if (account.Type == BAccountType.EmployeeType || account.Type == BAccountType.EmpCombinedType)
                {
                    EmployeeMaint graph = CreateInstance<EmployeeMaint>();
                    graph.Employee.Current = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            return adapter.Get();
        }

		public PXAction<TranFilter> viewProforma;
		[PXUIField(DisplayName = "", Enabled = false)]
		[PXButton]
		public virtual IEnumerable ViewProforma(PXAdapter adapter)
		{
			ProformaEntry target = PXGraph.CreateInstance<ProformaEntry>();
			target.Clear();
			target.Document.Current = PXSelect<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMTran.proformaRefNbr>>>>.Select(this);
			throw new PXRedirectRequiredException(target, "Proforma") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<TranFilter> viewInvoice;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
			target.Clear();
			target.Document.Current = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<PMTran.aRTranType>>, And<ARInvoice.refNbr, Equal<Current<PMTran.aRRefNbr>>>>>.Select(this);
			throw new PXRedirectRequiredException(target, "Invoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryID> e)
		{
			if (Project.Current != null)
				e.ReturnValue = Project.Current.CuryID;
		}

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class TranFilter : IBqlTable
		{
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
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
			#region AccountGroupID
			public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
			[AccountGroup]
			public virtual int? AccountGroupID
			{
				get;
				set;
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
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			/// <summary>
			/// Account
			/// </summary>
			[Account(null, typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>))]
			public virtual int? AccountID
			{
				get;
				set;
			}
			#endregion
			#region ARDocType
			public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
			/// <summary>
			/// AR Document Type
			/// </summary>
			[PXDBString(ARRegister.docType.Length, IsFixed = true)]
			[ARDocTypeList]
			[PXUIField(DisplayName = "AR Doc. Type", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string ARDocType
			{
				get;
				set;
			}
			#endregion
			#region ARRefNbr
			public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }
			/// <summary>
			/// AR Document Reference Number
			/// </summary>
			[PXDBString(15, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "AR Doc. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Current<aRDocType>>>>))]
			public virtual string ARRefNbr
			{
				get;
				set;
			}
			#endregion
			#region TranID
			public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
			/// <summary>
			/// Project transaction ID
			/// </summary>
			[PXDBLong]
			[PXUIField(DisplayName = "Tran. ID")]
			public virtual long? TranID
			{
				get;
				set;
			}
			#endregion
		}

		/// <summary>
		/// List of type of AR Documents
		/// </summary>
		public class ARDocTypeListAttribute : LabelListAttribute
		{
			private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
			{
				{ ARDocType.Invoice, AR.Messages.Invoice },
				{ ARDocType.CreditMemo, AR.Messages.CreditMemo },
				{ ARDocType.DebitMemo, AR.Messages.DebitMemo }
			};

			public ARDocTypeListAttribute() : base(_valueLabelPairs)
			{ }
		}
	}
}
