using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using System;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.ProformaLine)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMProformaLine : IBqlTable, ISortOrder, IQuantify, IProjectFilter
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
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

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}
		protected String _RefNbr;
		[PXDBString(refNbr.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<PMProforma.refNbr>), Filterable = true)]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(PMProforma.refNbr))]
		[PXParent(typeof(Select<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMProformaLine.refNbr>>>>))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXUIField(DisplayName = "Line Number", Visible = false)]
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(PMProforma.lineCntr))]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXUIField(DisplayName ="Sort Order", Visible = false)]
		[PXDBInt]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;
		[PXDBString(1)]
		public virtual string Type
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXFieldDescription]
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
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt()]
		[PXDefault(typeof(PMProforma.projectID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(PMProformaLine.projectID), BatchModule.AR, DisplayName = "Project Task", AllowCompleted = true, Enabled = false)]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXUIField(DisplayName = "Inventory ID", Enabled = false)]
		[PXDBInt()]
		[PMInventorySelector]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(typeof(accountID), typeof(taskID), GL.AccountType.Income, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[PXDBInt]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
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
		#region ResourceID
		public abstract class resourceID : PX.Data.BQL.BqlInt.Field<resourceID> { }
		protected Int32? _ResourceID;
		[PXEPEmployeeSelector]
		[PXDBInt()]
		[PXUIField(DisplayName = "Employee", Enabled = false)]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
				
		[Vendor(Enabled = false)]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		protected DateTime? _Date;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual DateTime? Date
		{
			get
			{
				return this._Date;
			}
			set
			{
				this._Date = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(typeof(PMProformaLine.branchID), typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>), 			
			DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(PMProformaLine.accountID), typeof(PMProformaLine.branchID), true, DisplayName = "Sales Subaccount", Visibility = PXUIVisibility.Visible)]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PMTax(typeof(PMProforma), typeof(PMTax), typeof(PMTaxTran))]
		[PMRetainedTax(typeof(PMProforma), typeof(PMTax), typeof(PMTaxTran))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<PMProformaLine.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(PMProformaLine.inventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(PMProforma.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(curyInfoID), typeof(unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public virtual Decimal? CuryUnitPrice
		{
			get;set;
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price in Base Currency")]
		public virtual Decimal? UnitPrice
		{
			get; set;
		}
		#endregion
		#region CompletedPct
		public abstract class completedPct : PX.Data.BQL.BqlDecimal.Field<completedPct> 
		{
			public const int Precision = 2;
		}
		[PXDecimal(completedPct.Precision, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Completed (%)")]
		public virtual decimal? CompletedPct
		{
			get;
			set;
		}
		#endregion
		#region CurrentInvoicedPct
		public abstract class currentInvoicedPct : PX.Data.BQL.BqlDecimal.Field<currentInvoicedPct> { }
		[PXDecimal(completedPct.Precision, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Current Invoiced (%)")]
		public virtual decimal? CurrentInvoicedPct
		{
			get;
			set;
		}
		#endregion
		#region BillableQty
		public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }
		protected Decimal? _BillableQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Quantity", Enabled = false)]
		public virtual Decimal? BillableQty
		{
			get
			{
				return this._BillableQty;
			}
			set
			{
				this._BillableQty = value;
			}
		}
		#endregion
		#region CuryBillableAmount
		public abstract class curyBillableAmount : PX.Data.BQL.BqlDecimal.Field<curyBillableAmount> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(billableAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Amount", Enabled = false)]
		public virtual Decimal? CuryBillableAmount
		{
			get;
			set;
		}
		#endregion
		#region BillableAmount
		public abstract class billableAmount : PX.Data.BQL.BqlDecimal.Field<billableAmount> { }
		protected Decimal? _BillableAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Amount in Base Currency", Enabled = false)]
		public virtual Decimal? BillableAmount
		{
			get
			{
				return this._BillableAmount;
			}
			set
			{
				this._BillableAmount = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity to Invoice")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }
		
		[PXDBCurrency(typeof(curyInfoID), typeof(amount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaidAmount
		public abstract class curyPrepaidAmount : PX.Data.BQL.BqlDecimal.Field<curyPrepaidAmount> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(prepaidAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Applied")]
		public virtual Decimal? CuryPrepaidAmount
		{
			get;
			set;
		}
		#endregion
		#region PrepaidAmount
		public abstract class prepaidAmount : PX.Data.BQL.BqlDecimal.Field<prepaidAmount> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Applied in Base Currency")]
		public virtual Decimal? PrepaidAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryMaterialStoredAmount
		public abstract class curyMaterialStoredAmount : PX.Data.BQL.BqlDecimal.Field<curyMaterialStoredAmount> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(materialStoredAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Stored Material", FieldClass=nameof(FeaturesSet.Construction))]
		public virtual Decimal? CuryMaterialStoredAmount
		{
			get;
			set;
		}
		#endregion
		#region MaterialStoredAmount
		public abstract class materialStoredAmount : PX.Data.BQL.BqlDecimal.Field<materialStoredAmount> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Stored Material in Base Currency", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual Decimal? MaterialStoredAmount
		{
			get;
			set;
		}
		#endregion		
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount to Invoice")]
		public virtual Decimal? CuryLineTotal
		{
			get;
			set;
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount To Invoice in Base Currency")]
		public virtual Decimal? LineTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage (%)", FieldClass =nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainage
		public abstract class curyRetainage : PX.Data.BQL.BqlDecimal.Field<curyRetainage> { }
		[PXFormula(typeof(Mult<curyLineTotal, Div<retainagePct, decimal100>>))]
		[PXDBCurrency(typeof(curyInfoID), typeof(retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryRetainage
		{
			get;
			set;
		}
		#endregion
		#region Retainage
		public abstract class retainage : PX.Data.BQL.BqlDecimal.Field<retainage> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retained Amount in Base Currency", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? Retainage
		{
			get;
			set;
		}
		#endregion
		#region CuryAllocatedRetainedAmount
		/// <exclude/>
		public abstract class curyAllocatedRetainedAmount : PX.Data.BQL.BqlDecimal.Field<curyAllocatedRetainedAmount> { }
		/// <summary>
		/// Allocated Retained Amount
		/// </summary>
		[PXDBCurrency(typeof(PMProformaLine.curyInfoID), typeof(PMProformaLine.allocatedRetainedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Allocated Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryAllocatedRetainedAmount
		{
			get;
			set;
		}
		#endregion

		#region AllocatedRetainedAmount
		/// <exclude/>
		public abstract class allocatedRetainedAmount : PX.Data.BQL.BqlDecimal.Field<allocatedRetainedAmount> { }
		/// <summary>
		/// Allocated Retained Amount (in Base Currency)
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? AllocatedRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryPreviouslyInvoiced
		public abstract class curyPreviouslyInvoiced : PX.Data.BQL.BqlDecimal.Field<curyPreviouslyInvoiced>
		{
		}

		/// <summary>
		/// Stored in document currency for Released Progressive lines.
		/// For unreleased lines this value has to be calculated by the ProformaEntry.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(previouslyInvoiced))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Previously Invoiced", Enabled =false)]
		public virtual Decimal? CuryPreviouslyInvoiced
		{
			get;
			set;
		}
		#endregion
		#region PreviouslyInvoiced
		public abstract class previouslyInvoiced : PX.Data.BQL.BqlDecimal.Field<previouslyInvoiced> { }

		/// <summary>
		/// Stored in document currency for Released Progressive lines.
		/// For unreleased lines this value has to be calculated by the ProformaEntry.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Previously Invoiced in Base Currency", Enabled = false)]
		public virtual Decimal? PreviouslyInvoiced
		{
			get;
			set;
		}
		#endregion

		#region Option
		public abstract class option : PX.Data.BQL.BqlString.Field<option>
		{
			public const string BillNow = "N";
			public const string WriteOffRemainder = "C";
			public const string HoldRemainder = "U";
			public const string Writeoff = "X";

			public class holdRemainder : PX.Data.BQL.BqlString.Constant<holdRemainder>
			{
				public holdRemainder() : base(HoldRemainder) {; }
			}

			public class writeoff : PX.Data.BQL.BqlString.Constant<writeoff>
			{
				public writeoff() : base(Writeoff) {; }
			}

			public class bill : PX.Data.BQL.BqlString.Constant<bill>
			{
				public bill() : base(BillNow) {; }
			}

			public class writeOffRemainder : PX.Data.BQL.BqlString.Constant<writeOffRemainder>
			{
				public writeOffRemainder() : base(WriteOffRemainder) {; }
			}
		}
		protected string _Option;
		[PXDefault(option.BillNow, PersistingCheck = PXPersistingCheck.Null)]
		[PXDBString()]
		[PXUIField(DisplayName = "Status")]
		public virtual string Option
		{
			get
			{
				return this._Option;
			}
			set
			{
				this._Option = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXUIField(DisplayName = "Released")]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region IsPrepayment
		public abstract class isPrepayment : PX.Data.BQL.BqlBool.Field<isPrepayment> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsPrepayment
		{
			get;
			set;
		}
		#endregion
		#region DefCode
		public abstract class defCode : PX.Data.BQL.BqlString.Field<defCode> { }
		[PXSelector(typeof(Search<DR.DRDeferredCode.deferredCodeID, Where<DR.DRDeferredCode.accountType, Equal<DR.DeferredAccountType.income>>>), DescriptionField = typeof(DR.DRDeferredCode.description))]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Deferral Code", FieldClass = "DEFFERED")]
		public virtual String DefCode
		{
			get;
			set;
		}
		#endregion


		#region ARInvoiceDocType
		public abstract class aRInvoiceDocType : PX.Data.BQL.BqlString.Field<aRInvoiceDocType> { }

		[PXDBString(3)]
		public virtual String ARInvoiceDocType
		{
			get; set;
		}
		#endregion
		#region ARInvoiceRefNbr
		public abstract class aRInvoiceRefNbr : PX.Data.BQL.BqlString.Field<aRInvoiceRefNbr> { }

		[PXDBString(15, IsUnicode = true)]
		public virtual String ARInvoiceRefNbr
		{
			get; set;
		}
		#endregion
		#region ARInvoiceLineNbr
		public abstract class aRInvoiceLineNbr : PX.Data.BQL.BqlInt.Field<aRInvoiceLineNbr> { }
		[PXDBInt]
		public virtual Int32? ARInvoiceLineNbr
		{
			get;set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMProformaLine.refNbr))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}

	[PXCacheName(Messages.ProformaLine)]
	[PXBreakInheritance]
	public class PMProformaProgressLine : PMProformaLine
	{
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}
		
		[PXDBString(refNbr.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<PMProforma.refNbr>), Filterable = true)]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(PMProforma.refNbr))]
		[PXParent(typeof(Select<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMProformaProgressLine.refNbr>>, And<Current<PMProformaProgressLine.type>, Equal<PMProformaLineType.progressive>>>>))]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1)]
		[PXDefault(PMProformaLineType.Progressive)]
		public override string Type
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
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDefault]
		[PXDBInt]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public override Int32? AccountGroupID
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
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(typeof(PMProformaProgressLine.branchID), typeof(Search<Account.accountID, Where<Account.accountGroupID, Equal<Current<PMProformaProgressLine.accountGroupID>>>>),
			DisplayName = "Sales Account", DescriptionField = typeof(Account.description))]
		public override Int32? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		
		[SubAccount(typeof(PMProformaProgressLine.accountID), typeof(PMProformaProgressLine.branchID), true, DisplayName = "Sales Subaccount", Visibility = PXUIVisibility.Visible)]
		public override Int32? SubID
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
		#region TaxCategoryID
		public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PMTax(typeof(PMProforma), typeof(PMTax), typeof(PMTaxTran))]
		[PMRetainedTax(typeof(PMProforma), typeof(PMTax), typeof(PMTaxTran))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXDefault(typeof(Search<PMBudget.taxCategoryID,
			Where<PMBudget.projectID, Equal<Current<PMProformaProgressLine.projectID>>,
			And<PMBudget.projectTaskID, Equal<Current<PMProformaProgressLine.taskID>>,
			And<PMBudget.accountGroupID, Equal<Current<PMProformaProgressLine.accountGroupID>>,
			And<PMBudget.inventoryID, Equal<Current<PMProformaProgressLine.inventoryID>>,
			And<PMBudget.costCodeID, Equal<Current<PMProformaProgressLine.costCodeID>>>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public override String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region CuryLineTotal
		public new abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		[PXFormula(typeof(Sub<Add<curyAmount, curyMaterialStoredAmount>, curyPrepaidAmount>), typeof(SumCalc<PMProforma.curyProgressiveTotal>))]
		[PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount to Invoice")]
		public override Decimal? CuryLineTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainage
		public new abstract class curyRetainage : PX.Data.BQL.BqlDecimal.Field<curyRetainage> { }
		[PXFormula(typeof(Mult<curyLineTotal, Div<retainagePct, decimal100>>), typeof(SumCalc<PMProforma.curyRetainageDetailTotal>))]
		[PXDBCurrency(typeof(curyInfoID), typeof(retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		public override Decimal? CuryRetainage
		{
			get;
			set;
		}
		#endregion
		public new abstract class isPrepayment : PX.Data.BQL.BqlBool.Field<isPrepayment> { }
	}

	[PXCacheName(Messages.ProformaLine)]
	[PXBreakInheritance]
	public class PMProformaTransactLine : PMProformaLine
	{		
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}

		[PXDBString(refNbr.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<PMProforma.refNbr>), Filterable = true)]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(PMProforma.refNbr))]
		[PXParent(typeof(Select<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMProformaTransactLine.refNbr>>, And<Current<PMProformaTransactLine.type>, Equal<PMProformaLineType.transaction>>>>))]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1)]
		[PXDefault(PMProformaLineType.Transaction)]
		public override string Type
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
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		#region CuryAmount
		public new abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }

		[PXFormula(typeof(Mult<qty, curyUnitPrice>))]
		[PXDBCurrency(typeof(curyInfoID), typeof(amount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public override Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryLineTotal
		public new abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		
		[PXFormula(typeof(Sub<Add<curyAmount, curyMaterialStoredAmount>, curyPrepaidAmount>), typeof(SumCalc<PMProforma.curyTransactionalTotal>))]
		[PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount to Invoice")]
		public override Decimal? CuryLineTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainage
		public new abstract class curyRetainage : PX.Data.BQL.BqlDecimal.Field<curyRetainage> { }
		[PXFormula(typeof(Mult<curyLineTotal, Div<retainagePct, decimal100>>), typeof(SumCalc<PMProforma.curyRetainageDetailTotal>))]
		[PXDBCurrency(typeof(curyInfoID), typeof(retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		public override Decimal? CuryRetainage
		{
			get;
			set;
		}
		#endregion

		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(typeof(PMProformaTransactLine.branchID), typeof(Search2<Account.accountID,
			InnerJoin<PMAccountGroup, On<Account.accountGroupID, Equal<PMAccountGroup.groupID>>>,
			Where2<Where<Current<PMProformaTransactLine.isPrepayment>, Equal<True>, And<Account.accountGroupID, Equal<Current<PMProformaTransactLine.accountGroupID>>>>,
			Or<Where<Current<PMProformaTransactLine.isPrepayment>, Equal<False>, And<Account.accountGroupID, IsNotNull>>>>>),
			DisplayName = "Sales Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXDefault(typeof(Search2<InventoryItem.salesAcctID,
			InnerJoin<Account, On<InventoryItem.salesAcctID, Equal<Account.accountID>>,
			InnerJoin<PMAccountGroup, On<Account.accountGroupID, Equal<PMAccountGroup.groupID>, And<PMAccountGroup.type, Equal<AccountType.income>>>>>,
			Where<InventoryItem.inventoryID, Equal<Current<PMProformaTransactLine.inventoryID>>>>))]
		public override Int32? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[SubAccount(typeof(PMProformaTransactLine.accountID), typeof(PMProformaTransactLine.branchID), true, DisplayName = "Sales Subaccount", Visibility = PXUIVisibility.Visible)]
		public override Int32? SubID
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

		#region TaxCategoryID
		public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PMTax(typeof(PMProforma), typeof(PMTax), typeof(PMTaxTran))]
		[PMRetainedTax(typeof(PMProforma), typeof(PMTax), typeof(PMTaxTran))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public override String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(PMProformaLine.inventoryID))]
		public override String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion

		public new abstract class isPrepayment : PX.Data.BQL.BqlBool.Field<isPrepayment> { }

		public new abstract class option : PX.Data.BQL.BqlString.Field<option> { }


		#region CuryMaxAmount
		public abstract class curyMaxAmount : PX.Data.BQL.BqlDecimal.Field<curyMaxAmount> { }

		[PXCurrency(typeof(curyInfoID), typeof(maxAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Max Limit Amount", Enabled = false, Visible = false)]
		public virtual Decimal? CuryMaxAmount
		{
			get;
			set;
		}
		#endregion
		#region MaxAmount
		public abstract class maxAmount : PX.Data.BQL.BqlDecimal.Field<maxAmount> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Max Limit Amount in Base Currency", Enabled = false, Visible = false)]
		public virtual Decimal? MaxAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryAvailableAmount
		public abstract class curyAvailableAmount : PX.Data.BQL.BqlDecimal.Field<curyAvailableAmount> { }

		[PXCurrency(typeof(curyInfoID), typeof(availableAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Max Available Amount", Enabled = false, Visible = false)]
		public virtual Decimal? CuryAvailableAmount
		{
			get;
			set;
		}
		#endregion
		#region AvailableAmount
		public abstract class availableAmount : PX.Data.BQL.BqlDecimal.Field<availableAmount> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Max Available Amount in Base Currency", Enabled = false, Visible = false)]
		public virtual Decimal? AvailableAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryOverflowAmount
		public abstract class curyOverflowAmount : PX.Data.BQL.BqlDecimal.Field<curyOverflowAmount> { }
				
		[PXCurrency(typeof(curyInfoID), typeof(overflowAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Over-Limit Amount", Enabled = false, Visible = false)]
		public virtual Decimal? CuryOverflowAmount
		{
			get;
			set;
		}
		#endregion
		#region OverflowAmount
		public abstract class overflowAmount : PX.Data.BQL.BqlDecimal.Field<overflowAmount> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Overflow Amount in Base Currency", Enabled = false, Visible = false)]
		public virtual Decimal? OverflowAmount
		{
			get;
			set;
		}
		#endregion
	}

	public static class PMProformaLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Progressive, Transaction },
				new string[] { Messages.Progressive, Messages.Transaction })
			{; }
		}
		public const string Progressive = "P";
		public const string Transaction = "T";
		
		public class progressive : PX.Data.BQL.BqlString.Constant<progressive>
		{
			public progressive() : base(Progressive) {; }
		}
		public class transaction : PX.Data.BQL.BqlString.Constant<transaction>
		{
			public transaction() : base(Transaction) {; }
		}
	}

	/// <summary>
	/// Used in Reports to calculate Previously invoiced amount.
	/// </summary>
	[PXCacheName(Messages.ProformaLine)]
	[Serializable]
	[PXProjection(typeof(Select4<PMProformaLine,
		Where<PMProformaLine.type, Equal<PMProformaLineType.progressive>>, 
		Aggregate<GroupBy<PMProformaLine.projectID,
			GroupBy<PMProformaLine.refNbr,
			GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.accountGroupID,
			GroupBy<PMProformaLine.inventoryID,
			GroupBy<PMProformaLine.costCodeID,
			Sum<PMProformaLine.curyLineTotal,
			Sum<PMProformaLine.lineTotal,
			Sum<PMProformaLine.curyMaterialStoredAmount,
			Sum<PMProformaLine.materialStoredAmount,
			Sum<PMProformaLine.curyRetainage,
			Sum<PMProformaLine.retainage>>>>>>>>>>>>>>), Persistent=false)]
	public class PMProgressLineTotal : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(PMProformaLine.refNbr.Length, IsUnicode = true, IsKey = true, BqlField =typeof(PMProformaLine.refNbr))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMProformaLine.projectID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMProformaLine.taskID))]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(PMProformaLine.inventoryID))]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[PXDBInt(BqlField = typeof(PMProformaLine.costCodeID))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMProformaLine.accountGroupID))]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
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
		#region CuryMaterialStoredAmount
		public abstract class curyMaterialStoredAmount : PX.Data.BQL.BqlDecimal.Field<curyMaterialStoredAmount>
		{
		}
		[PXDBBaseCury(BqlField = typeof(PMProformaLine.curyMaterialStoredAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Stored Material")]
		public virtual Decimal? CuryMaterialStoredAmount
		{
			get; set;
		}
		#endregion
		#region MaterialStoredAmount
		public abstract class materialStoredAmount : PX.Data.BQL.BqlDecimal.Field<materialStoredAmount> { }
		[PXDBBaseCury(BqlField = typeof(PMProformaLine.materialStoredAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Stored Material in Base Currency")]
		public virtual Decimal? MaterialStoredAmount
		{
			get; set;
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal>
		{
		}
		[PXDBBaseCury(BqlField = typeof(PMProformaLine.curyLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total")]
		public virtual Decimal? CuryLineTotal
		{
			get; set;
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		[PXDBBaseCury(BqlField = typeof(PMProformaLine.lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total in Base Currency")]
		public virtual Decimal? LineTotal
		{
			get; set;
		}
		#endregion
		#region CuryRetainage
		public abstract class curyRetainage : PX.Data.BQL.BqlDecimal.Field<curyRetainage>
		{
		}
		[PXDBBaseCury(BqlField = typeof(PMProformaLine.curyRetainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage")]
		public virtual Decimal? CuryRetainage
		{
			get; set;
		}
		#endregion
		#region Retainage
		public abstract class retainage : PX.Data.BQL.BqlDecimal.Field<retainage> { }
		[PXDBBaseCury(BqlField = typeof(PMProformaLine.retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage in Base Currency")]
		public virtual Decimal? Retainage
		{
			get; set;
		}
		#endregion
	}
}
