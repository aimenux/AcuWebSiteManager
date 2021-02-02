using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CT;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.RecurringItem)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMRecurringItem : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBDefault(typeof(PMTask.projectID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		[PXDBDefault(typeof(PMTask.taskID))]
		[PXParent(typeof(Select<PMTask, Where<PMTask.taskID, Equal<Current<taskID>>>>))]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[NonStockItem(IsKey = true)]
		[PXDefault]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>))]
		[PMUnit(typeof(inventoryID))]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
		}
		[Branch(useDefaulting: false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXLocalizableDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>),
			typeof(Customer.localeName), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region AccountSource
		public abstract class accountSource : PX.Data.BQL.BqlString.Field<accountSource> { }
		[PXDBString(1, IsFixed = true)]
		[PMAccountSource.RecurentList()]
		[PXDefault(PMAccountSource.Customer)]
		[PXUIField(DisplayName = "Account Source")]
		public virtual String AccountSource
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(DisplayName = "Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubMask
		public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { }

		[PMRecurentBillSubAccountMask]
		public virtual String SubMask
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(typeof(accountID), DisplayName = "Subaccount", DescriptionField = typeof(Sub.description))]
		public virtual Int32? SubID
		{
			get;
			set;
		}
		#endregion
		#region ResetUsage
		public abstract class resetUsage : PX.Data.BQL.BqlString.Field<resetUsage> { }
		
		[PXDefault(ResetUsageOption.Never)]
		[PXUIField(DisplayName = "Reset Usage")]
		[PXDBString(1, IsFixed = true)]
		[ResetUsageOption.ListForProject()]
		public virtual string ResetUsage
		{
			get;
			set;
		}
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlDecimal.Field<included> { }
		
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Included")]
		public virtual Decimal? Included
		{
			get;
			set;
		}
		#endregion
		#region Used
		public abstract class used : PX.Data.BQL.BqlDecimal.Field<used> { }
		
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used", Enabled = false)]
		public virtual Decimal? Used
		{
			get;
			set;
		}
		#endregion
		#region UsedTotal
		public abstract class usedTotal : PX.Data.BQL.BqlDecimal.Field<usedTotal> { }
		
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used Total", Enabled = false)]
		public virtual Decimal? UsedTotal
		{
			get;
			set;
		}
		#endregion
		#region LastBilledDate
		public abstract class lastBilledDate : PX.Data.BQL.BqlDateTime.Field<lastBilledDate> { }
		
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Billed Date", Enabled = false)]
		public virtual DateTime? LastBilledDate
		{
			get;
			set;
		}
		#endregion
		#region LastBilledQty
		public abstract class lastBilledQty : PX.Data.BQL.BqlDecimal.Field<lastBilledQty> { }
		
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Billed Qty.", Enabled = false)]
		public virtual Decimal? LastBilledQty
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

	[PXBreakInheritance]
	[PMRecurringItemAccum]	
	[PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMRecurringItemAccum : PMRecurringItem
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		[PXDBInt(IsKey = true)]
		public override Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		public override Int32? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region Used
		public new abstract class used : PX.Data.BQL.BqlDecimal.Field<used> { }

		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used", Enabled = false)]
		public override Decimal? Used
		{
			get;
			set;
		}
		#endregion
		#region UsedTotal
		public new abstract class usedTotal : PX.Data.BQL.BqlDecimal.Field<usedTotal> { }

		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used Total", Enabled = false)]
		public override Decimal? UsedTotal
		{
			get;
			set;
		}
		#endregion
	}
}
