using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.TM;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;
using PX.Objects.IN;
using PX.Objects.AP;

namespace PX.Objects.PM
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.ChangeRequest)]
	[Serializable]
	[PXEMailSource]
	public class PMChangeRequestLine : PX.Data.IBqlTable, IQuantify
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(PMChangeRequest.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(PMChangeRequest.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		[PXParent(typeof(Select<PMChangeRequest, Where<PMChangeRequest.refNbr, Equal<Current<PMChangeRequestLine.refNbr>>>>))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMChangeRequest.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBDefault(typeof(PMChangeRequest.projectID))]
		[PXDBInt]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region CostTaskID
		public abstract class costTaskID : PX.Data.BQL.BqlInt.Field<costTaskID> { }
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[ProjectTask(typeof(projectID),	AlwaysEnabled = true)]
		[PXForeignReference(typeof(Field<costTaskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? CostTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostAccountGroupID
		public abstract class costAccountGroupID : PX.Data.BQL.BqlInt.Field<costAccountGroupID> { }
		[AccountGroup(typeof(Where<PMAccountGroup.isExpense, Equal<True>>))]
		[PXForeignReference(typeof(Field<costAccountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public virtual Int32? CostAccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(null, typeof(costTaskID), PX.Objects.GL.AccountType.Expense, typeof(costAccountGroupID))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID")]
		[PMInventorySelector]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[SubItem(typeof(inventoryID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region RevenueTaskID
		public abstract class revenueTaskID : PX.Data.BQL.BqlInt.Field<revenueTaskID> { }
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ProjectTask(typeof(projectID), AlwaysEnabled = true, AllowNull = true, DisplayName = "Revenue Task")]
		[PXForeignReference(typeof(Field<revenueTaskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? RevenueTaskID
		{
			get;
			set;
		}
		#endregion
		#region RevenueAccountGroupID
		public abstract class revenueAccountGroupID : PX.Data.BQL.BqlInt.Field<revenueAccountGroupID> { }
		[AccountGroup(typeof(Where<PMAccountGroup.type, Equal<PX.Objects.GL.AccountType.income>>), DisplayName = "Revenue Account Group")]
		[PXForeignReference(typeof(Field<revenueAccountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public virtual Int32? RevenueAccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region RevenueCodeID
		public abstract class revenueCodeID : PX.Data.BQL.BqlInt.Field<revenueCodeID> { }
		[PXDefault(typeof(costCodeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[CostCode(null, typeof(revenueTaskID), PX.Objects.GL.AccountType.Income, typeof(revenueAccountGroupID), DisplayName = "Revenue Code", AllowNullValue = true)]
		public virtual Int32? RevenueCodeID
		{
			get;
			set;
		}
		#endregion
		#region RevenueInventoryID
		public abstract class revenueInventoryID : PX.Data.BQL.BqlInt.Field<revenueInventoryID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Revenue Inventory ID")]
		[PMInventorySelector]
		public virtual Int32? RevenueInventoryID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXFieldDescription]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor()]
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
		#region IsCommitment
		public abstract class isCommitment : PX.Data.BQL.BqlBool.Field<isCommitment> { }
		[PXDBBool]
		[PXDefault(false, typeof(Search<PMAccountGroup.createsCommitment, Where<PMAccountGroup.groupID, Equal<Current<PMChangeRequestLine.costAccountGroupID>>>>))]
		[PXUIField(DisplayName = "Create Commitment")]
		public bool? IsCommitment
		{
			get;
			set;
		}
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<PMChangeRequestLine.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion

		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
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
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
		#region ExtCost
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXFormula(typeof(Mult<qty, unitCost>),
			typeof(SumCalc<PMChangeRequest.costTotal>))]
		public virtual Decimal? ExtCost
		{
			get;
			set;
		}
		#endregion
		#region PriceMarkupPct
		public abstract class priceMarkupPct : PX.Data.BQL.BqlDecimal.Field<priceMarkupPct> { }

		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.markupPct, Where<InventoryItem.inventoryID, Equal<Current<PMChangeRequestLine.inventoryID>>>>))]
		[PXUIField(DisplayName = "Price Markup (%)")]
		public virtual Decimal? PriceMarkupPct
		{
			get;
			set;
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		[PXFormula(typeof(Mult<Div<Add<decimal100, priceMarkupPct>, decimal100>, unitCost>))]
		public virtual Decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion
		#region ExtPrice
		public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Price")]
		[PXFormula(typeof(Mult<qty, unitPrice>))]
		public virtual Decimal? ExtPrice
		{
			get;
			set;
		}
		#endregion
		#region LineMarkupPct
		public abstract class lineMarkupPct : PX.Data.BQL.BqlDecimal.Field<priceMarkupPct> { }

		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<PMAccountGroup.defaultLineMarkupPct, Where<PMAccountGroup.groupID, Equal<Current<PMChangeRequestLine.costAccountGroupID>>>>))]
		[PXUIField(DisplayName = "Line Markup (%)")]
		public virtual Decimal? LineMarkupPct
		{
			get;
			set;
		}
		#endregion
		#region LineAmount
		public abstract class lineAmount : PX.Data.BQL.BqlDecimal.Field<lineAmount> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Amount")]
		[PXFormula(typeof(Div<Mult<extPrice, Add<decimal100, lineMarkupPct>>, decimal100>),
			typeof(SumCalc<PMChangeRequest.lineTotal>))]
		public virtual Decimal? LineAmount
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
}
