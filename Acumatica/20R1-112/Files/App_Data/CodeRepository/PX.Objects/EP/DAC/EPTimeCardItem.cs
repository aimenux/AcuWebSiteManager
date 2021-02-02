using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    [Serializable]
	[PXCacheName(Messages.TimeCardItem)]
	public partial class EPTimeCardItem : IBqlTable
	{
		#region TimeCardCD

		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
		[PXDBDefault(typeof(EPTimeCard.timeCardCD))]
		[PXDBString(10, IsKey = true)]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Current<EPTimeCardItem.timeCardCD>>>>))]
		public virtual String TimeCardCD { get; set; }

		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(EPTimeCard))]
		[PXUIField(Visible = false)]
		public virtual Int32? LineNbr { get; set; }
		#endregion
		
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectDefault(BatchModule.TA, ForceProjectExplicitly = true)]
		[EPTimeCardProjectAttribute]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[EPTimecardProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
		public virtual Int32? TaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(null, typeof(taskID), GL.AccountType.Expense)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDefault]
		[NonStockItem]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<EPTimeCardItem.inventoryID>>>>))]
		[INUnit(typeof(EPTimeCardItem.inventoryID))]
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
		#region Mon
		public abstract class mon : PX.Data.BQL.BqlDecimal.Field<mon> { }
		protected Decimal? _Mon;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Mon")]
		public virtual Decimal? Mon
		{
			get
			{
				return this._Mon;
			}
			set
			{
				this._Mon = value;
			}
		}
		#endregion
		#region Tue
		public abstract class tue : PX.Data.BQL.BqlDecimal.Field<tue> { }
		protected Decimal? _Tue;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Tue")]
		public virtual Decimal? Tue
		{
			get
			{
				return this._Tue;
			}
			set
			{
				this._Tue = value;
			}
		}
		#endregion
		#region Wed
		public abstract class wed : PX.Data.BQL.BqlDecimal.Field<wed> { }
		protected Decimal? _Wed;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Wed")]
		public virtual Decimal? Wed
		{
			get
			{
				return this._Wed;
			}
			set
			{
				this._Wed = value;
			}
		}
		#endregion
		#region Thu
		public abstract class thu : PX.Data.BQL.BqlDecimal.Field<thu> { }
		protected Decimal? _Thu;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Thu")]
		public virtual Decimal? Thu
		{
			get
			{
				return this._Thu;
			}
			set
			{
				this._Thu = value;
			}
		}
		#endregion
		#region Fri
		public abstract class fri : PX.Data.BQL.BqlDecimal.Field<fri> { }
		protected Decimal? _Fri;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Fri")]
		public virtual Decimal? Fri
		{
			get
			{
				return this._Fri;
			}
			set
			{
				this._Fri = value;
			}
		}
		#endregion
		#region Sat
		public abstract class sat : PX.Data.BQL.BqlDecimal.Field<sat> { }
		protected Decimal? _Sat;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Sat")]
		public virtual Decimal? Sat
		{
			get
			{
				return this._Sat;
			}
			set
			{
				this._Sat = value;
			}
		}
		#endregion
		#region Sun
		public abstract class sun : PX.Data.BQL.BqlDecimal.Field<sun> { }
		protected Decimal? _Sun;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Sun")]
		public virtual Decimal? Sun
		{
			get
			{
				return this._Sun;
			}
			set
			{
				this._Sun = value;
			}
		}
		#endregion
		#region TotalQty
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }

		[PXQuantity]
		[PXUIField(DisplayName = "Total Qty.", Enabled = false)]
		public virtual decimal? TotalQty
		{
			get
			{
				return Mon.GetValueOrDefault() +
					   Tue.GetValueOrDefault() +
					   Wed.GetValueOrDefault() +
					   Thu.GetValueOrDefault() +
					   Fri.GetValueOrDefault() +
					   Sat.GetValueOrDefault() +
					   Sun.GetValueOrDefault();
			}
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }

		[PXDBInt]
		public virtual Int32? OrigLineNbr { get; set; }
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
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
		[PXDBCreatedByID()]
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedByID()]
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
		[PXDBLastModifiedDateTime()]
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
