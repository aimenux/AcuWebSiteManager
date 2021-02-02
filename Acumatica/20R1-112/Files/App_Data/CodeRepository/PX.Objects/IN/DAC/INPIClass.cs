using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	#region IN PI Class
	[Serializable]
	[PXCacheName(Messages.INPIClass)]
	public partial class INPIClass : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPIClass>.By<pIClassID>
		{
			public static INPIClass Find(PXGraph graph, string pIClassID) => FindBy(graph, pIClassID);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INPIClass>.By<siteID> { }
			public class PICycle : INPICycle.PK.ForeignKeyOf<INPIClass>.By<cycleID> { }
			public class ABCCode : INABCCode.PK.ForeignKeyOf<INPIClass>.By<aBCCodeID> { }
			public class MovementClass : INMovementClass.PK.ForeignKeyOf<INPIClass>.By<movementClassID> { }
		}
		#endregion
		#region PIClassID
		public abstract class pIClassID : PX.Data.BQL.BqlString.Field<pIClassID> { }
		protected String _PIClassID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INPIClass.pIClassID>))]
		[PXDefault()]
		public virtual String PIClassID
		{
			get
			{
				return this._PIClassID;
			}
			set
			{
				this._PIClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDefault]
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
		protected String _Method;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Generation Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PIMethod.List]
		[PXDefault(PIMethod.FullPhysicalInventory)]
		public virtual String Method
		{
			get
			{
				return this._Method;
			}
			set
			{
				this._Method = value;
			}
		}
		#endregion

		#region CycleID
		public abstract class cycleID : PX.Data.BQL.BqlString.Field<cycleID> { }
		protected String _CycleID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INPICycle.cycleID), DescriptionField = typeof(INPICycle.descr))]
		[PXUIField(DisplayName = "Cycle ID")]
		public virtual String CycleID
		{
			get
			{
				return this._CycleID;
			}
			set
			{
				this._CycleID = value;
			}
		}
		#endregion
		#region ABCCodeID
		public abstract class aBCCodeID : PX.Data.BQL.BqlString.Field<aBCCodeID> { }
		protected String _ABCCodeID;
		[PXDBString(1, IsFixed = true)]
		[PXSelector(typeof(INABCCode.aBCCodeID), DescriptionField = typeof(INABCCode.descr))]
		[PXUIField(DisplayName = "ABC Code")]
		public virtual String ABCCodeID
		{
			get
			{
				return this._ABCCodeID;
			}
			set
			{
				this._ABCCodeID = value;
			}
		}
		#endregion
		#region MovementClassID
		public abstract class movementClassID : PX.Data.BQL.BqlString.Field<movementClassID> { }
		protected String _MovementClassID;
		[PXDBString(1, IsFixed = true)]
		[PXSelector(typeof(INMovementClass.movementClassID), DescriptionField = typeof(INMovementClass.descr))]
		[PXUIField(DisplayName = "Movement Class ID")]
		public virtual String MovementClassID
		{
			get
			{
				return this._MovementClassID;
			}
			set
			{
				this._MovementClassID = value;
			}
		}
		#endregion

		#region SelectedMethod
		public abstract class selectedMethod : PX.Data.BQL.BqlString.Field<selectedMethod> { }
		protected String _SelectedMethod;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Selection Method", Visibility = PXUIVisibility.Visible)]
		[PIInventoryMethod.List]
		[PXDefault(PIInventoryMethod.ItemsHavingNegativeBookQty)]
		public virtual String SelectedMethod
		{
			get
			{
				return this._SelectedMethod;
			}
			set
			{
				this._SelectedMethod = value;
			}
		}
		#endregion

		#region ByFrequency
		public abstract class byFrequency : PX.Data.BQL.BqlBool.Field<byFrequency> { }
		protected Boolean? _ByFrequency;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ByFrequency
		{
			get
			{
				return this._ByFrequency;
			}
			set
			{
				this._ByFrequency = value;
			}
		}
		#endregion
		#region ByABCFrequency
		public abstract class byABCFrequency : PX.Data.BQL.BqlBool.Field<byABCFrequency> { }
		protected Boolean? _ByABCFrequency;
		[PXBool()]
		[PXUIField(DisplayName = "By Frequency")]
		public virtual Boolean? ByABCFrequency
		{
			get
			{
				return this._ByFrequency;
			}
			set
			{
				if (this.Method == PIMethod.ByABCClass)
					this._ByFrequency = value;
			}
		}
		#endregion
		#region ByMovementClassFrequency
		public abstract class byMovementClassFrequency : PX.Data.BQL.BqlBool.Field<byMovementClassFrequency> { }
		protected Boolean? _ByMovementClassFrequency;
		[PXBool()]
		[PXUIField(DisplayName = "By Frequency")]
		public virtual Boolean? ByMovementClassFrequency
		{
			get
			{
				return this._ByFrequency;
			}
			set
			{
				if (this.Method == PIMethod.ByMovementClass)
					this._ByFrequency = value;
			}
		}
		#endregion
		#region ByCycleFrequency
		public abstract class byCycleFrequency : PX.Data.BQL.BqlBool.Field<byCycleFrequency> { }
		protected Boolean? _ByCycleFrequency;
		[PXBool()]
		[PXUIField(DisplayName = "By Frequency")]
		public virtual Boolean? ByCycleFrequency
		{
			get
			{
				return this._ByFrequency;
			}
			set
			{
				if (this.Method == PIMethod.ByCycle)
					this._ByFrequency = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site]
		[PXForeignReference(typeof(FK.Site))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region IncludeZeroItems
		public abstract class includeZeroItems : Data.BQL.BqlBool.Field<includeZeroItems>
		{
		}

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Items with Zero Book Quantity in PI")]
		public virtual Boolean? IncludeZeroItems
		{
			get;
			set;
		}
		#endregion
		#region HideBookQty
		public abstract class hideBookQty : Data.BQL.BqlBool.Field<hideBookQty>
		{
		}

		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hide Book Qty. on PI Count")]
		public virtual Boolean? HideBookQty
		{
			get;
			set;
		}
		#endregion

		#region NAO1
		public abstract class nAO1 : PX.Data.BQL.BqlString.Field<nAO1> { }
		protected String _NAO1;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "1", Visibility = PXUIVisibility.Visible)]
		[PINumberAssignmentOrder.List]
		[PXDefault(PINumberAssignmentOrder.ByLocationID)]
		public virtual String NAO1
		{
			get
			{
				return this._NAO1;
			}
			set
			{
				this._NAO1 = value;
			}
		}
		#endregion
		#region NAO2
		public abstract class nAO2 : PX.Data.BQL.BqlString.Field<nAO2> { }
		protected String _NAO2;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "2", Visibility = PXUIVisibility.Visible)]
		[PINumberAssignmentOrder.List]
		[PXDefault(PINumberAssignmentOrder.ByInventoryID)]
		public virtual String NAO2
		{
			get
			{
				return this._NAO2;
			}
			set
			{
				this._NAO2 = value;
			}
		}
		#endregion
		#region NAO3
		public abstract class nAO3 : PX.Data.BQL.BqlString.Field<nAO3> { }
		protected String _NAO3;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "3", Visibility = PXUIVisibility.Visible)]
		[PINumberAssignmentOrder.List]
		[PXDefault(PINumberAssignmentOrder.BySubItem)]
		public virtual String NAO3
		{
			get
			{
				return this._NAO3;
			}
			set
			{
				this._NAO3 = value;
			}
		}
		#endregion
		#region NAO4
		public abstract class nAO4 : PX.Data.BQL.BqlString.Field<nAO4> { }
		protected String _NAO4;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "4", Visibility = PXUIVisibility.Visible)]
		[PINumberAssignmentOrder.List]
		[PXDefault(PINumberAssignmentOrder.ByLotSerial)]
		public virtual String NAO4
		{
			get
			{
				return this._NAO4;
			}
			set
			{
				this._NAO4 = value;
			}
		}
		#endregion

		#region BlankLines
		public abstract class blankLines : PX.Data.BQL.BqlShort.Field<blankLines> { }
		protected Int16? _BlankLines;
		[PXDBShort(MinValue = 0, MaxValue = 10000)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Blank Lines To Append")]
		public virtual Int16? BlankLines
		{
			get
			{
				return this._BlankLines;
			}
			set
			{
				this._BlankLines = value;
			}
		}
		#endregion

		#region RandomItemsLimit
		public abstract class randomItemsLimit : PX.Data.BQL.BqlShort.Field<randomItemsLimit> { }
		protected Int16? _RandomItemsLimit;
		[PXDBShort(MinValue = 0, MaxValue = 10000)]
		[PXUIField(DisplayName = "Number of Items (up to)")]
		public virtual Int16? RandomItemsLimit
		{
			get
			{
				return this._RandomItemsLimit;
			}
			set
			{
				this._RandomItemsLimit = value;
			}
		}
		#endregion
		#region LastCountPeriod
		public abstract class lastCountPeriod : PX.Data.BQL.BqlShort.Field<lastCountPeriod> { }
		protected Int16? _LastCountPeriod;
		[PXDBShort(MinValue = 0, MaxValue = 10000)]
		[PXUIField(DisplayName = "Last Count Before (days)", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int16? LastCountPeriod
		{
			get
			{
				return this._LastCountPeriod;
			}
			set
			{
				this._LastCountPeriod = value;
			}
		}
		#endregion
		#region UnlockSiteOnCountingFinish
		public abstract class unlockSiteOnCountingFinish : PX.Data.BQL.BqlBool.Field<unlockSiteOnCountingFinish> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Unfreeze Stock When Counting Is Finished")]
		public virtual Boolean? UnlockSiteOnCountingFinish
		{
			get;
			set;
		}
		#endregion
		

		#region System Columns
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	#endregion

    [Serializable]
	[PXCacheName(Messages.INPIClassItem)]
	public partial class INPIClassItem : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPIClassItem>.By<pIClassID, inventoryID>
		{
			public static INPIClassItem Find(PXGraph graph, string pIClassID, int? inventoryID) => FindBy(graph, pIClassID, inventoryID);
		}
		public static class FK
		{
			public class PIClass : INPIClass.PK.ForeignKeyOf<INPIClassItem>.By<pIClassID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INPIClassItem>.By<inventoryID> { }
		}
		#endregion
		#region PIClassID
		public abstract class pIClassID : PX.Data.BQL.BqlString.Field<pIClassID> { }
		protected String _PIClassID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INPIClass.pIClassID))]
		[PXUIField(DisplayName = "Type", Visible = false)]
		[PXParent(typeof(FK.PIClass))]
		public virtual String PIClassID
		{
			get
			{
				return this._PIClassID;
			}
			set
			{
				this._PIClassID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
        [PXParent(typeof(FK.InventoryItem))]
        [StockItem(DisplayName = "Inventory ID", IsKey = true)]
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
	}

    [Serializable]
	[PXCacheName(Messages.INPIClassLocation)]
	public partial class INPIClassLocation : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPIClassLocation>.By<pIClassID, locationID>
		{
			public static INPIClassLocation Find(PXGraph graph, string pIClassID, int? locationID)
				=> FindBy(graph, pIClassID, locationID);
		}
		public static class FK
		{
			public class PIClass : INPIClass.PK.ForeignKeyOf<INPIClassLocation>.By<pIClassID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INPIClassLocation>.By<locationID> { }
		}
		#endregion
		#region PIClassID
		public abstract class pIClassID : PX.Data.BQL.BqlString.Field<pIClassID> { }
		protected String _PIClassID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INPIClass.pIClassID))]
		[PXUIField(DisplayName = "Code", Visible = false)]
		[PXParent(typeof(FK.PIClass))]
		public virtual String PIClassID
		{
			get
			{
				return this._PIClassID;
			}
			set
			{
				this._PIClassID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INPIClass.siteID), IsKey = true)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
	}

	[Serializable]
	public partial class INPIClassItemClass : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPIClassItemClass>.By<pIClassID, itemClassID>
		{
			public static INPIClassItemClass Find(PXGraph graph, string pIClassID, int? itemClassID) 
				=> FindBy(graph, pIClassID, itemClassID);
		}
		public static class FK
		{
			public class PIClass : INPIClass.PK.ForeignKeyOf<INPIClassItemClass>.By<pIClassID> { }
			public class ItemClass : INItemClass.PK.ForeignKeyOf<INPIClassItemClass>.By<itemClassID> { }
		}
		#endregion
		#region PIClassID
		public abstract class pIClassID : PX.Data.BQL.BqlString.Field<pIClassID> { }
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INPIClass.pIClassID))]
		[PXUIField(DisplayName = "Type", Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(FK.PIClass))]
		public virtual string PIClassID { get; set; }
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Item Class ID")]
		[PXParent(typeof(FK.ItemClass))]
		[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		public virtual int? ItemClassID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
