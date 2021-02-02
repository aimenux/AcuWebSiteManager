using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.FA;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(EquipmentMaint))]
	[PXCacheName(Messages.Equipment)]
	public partial class EPEquipment : PX.Data.IBqlTable
	{
		#region EquipmentID
		public abstract class equipmentID : PX.Data.BQL.BqlInt.Field<equipmentID> { }
		protected int? _EquipmentID;
		[PXDBIdentity()]
		public virtual int? EquipmentID
		{
			get
			{
				return this._EquipmentID;
			}
			set
			{
				this._EquipmentID = value;
			}
		}
		#endregion
		#region EquipmentCD
		public abstract class equipmentCD : PX.Data.BQL.BqlString.Field<equipmentCD> { }
		protected String _EquipmentCD;
		[PXUIField(DisplayName = "Equipment ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(15, IsUnicode = true, IsKey=true)]
		[PXDefault()]
		public virtual String EquipmentCD
		{
			get
			{
				return this._EquipmentCD;
			}
			set
			{
				this._EquipmentCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[EPEquipmentStatus.List]
		[PXDBString(1, IsFixed = true)]
		[PXDefault(EPEquipmentStatus.Active)]
		[PXUIField(DisplayName = "Status")]
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
		#region FixedAssetID
		public abstract class fixedAssetID : PX.Data.BQL.BqlInt.Field<fixedAssetID> { }
		protected Int32? _FixedAssetID;
		[PXDBInt]
		[PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.recordType, NotEqual<FARecordType.classType>>>), 
			SubstituteKey = typeof(FixedAsset.assetCD),
			DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Fixed Asset")]
		public virtual Int32? FixedAssetID
		{
			get
			{
				return this._FixedAssetID;
			}
			set
			{
				this._FixedAssetID = value;
			}
		}
		#endregion
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar")]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion
		#region RunRateItemID
		public abstract class runRateItemID : PX.Data.BQL.BqlInt.Field<runRateItemID> { }
		protected Int32? _RunRateItemID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Run-Rate Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
		[PXForeignReference(typeof(Field<runRateItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? RunRateItemID
		{
			get
			{
				return this._RunRateItemID;
			}
			set
			{
				this._RunRateItemID = value;
			}
		}
		#endregion
		#region SetupRateItemID
		public abstract class setupRateItemID : PX.Data.BQL.BqlInt.Field<setupRateItemID> { }
		protected Int32? _SetupRateItemID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Setup-Rate Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
		[PXForeignReference(typeof(Field<runRateItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? SetupRateItemID
		{
			get
			{
                return this._SetupRateItemID;
			}
			set
			{
                this._SetupRateItemID = value;
			}
		}
		#endregion
		#region SuspendRateItemID
		public abstract class suspendRateItemID : PX.Data.BQL.BqlInt.Field<suspendRateItemID> { }
		protected Int32? _SuspendRateItemID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Suspend-Rate Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
		[PXForeignReference(typeof(Field<runRateItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? SuspendRateItemID
		{
			get
			{
				return this._SuspendRateItemID;
			}
			set
			{
				this._SuspendRateItemID = value;
			}
		}
		#endregion
		#region RunRate
		public abstract class runRate : PX.Data.BQL.BqlDecimal.Field<runRate> { }
		protected decimal? _RunRate;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Selector<EPEquipment.runRateItemID, InventoryItem.stdCost>))]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Run Rate")]
		public virtual decimal? RunRate
		{
			get
			{
				return this._RunRate;
			}
			set
			{
				this._RunRate = value;
			}
		}
		#endregion
		#region SetupRate
		public abstract class setupRate : PX.Data.BQL.BqlDecimal.Field<setupRate> { }
		protected decimal? _SetupRate;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Selector<EPEquipment.setupRateItemID, InventoryItem.stdCost>))]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Setup Rate")]
		public virtual decimal? SetupRate
		{
			get
			{
				return this._SetupRate;
			}
			set
			{
				this._SetupRate = value;
			}
		}
		#endregion
		#region SuspendRate
		public abstract class suspendRate : PX.Data.BQL.BqlDecimal.Field<suspendRate> { }
		protected decimal? _SuspendRate;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Selector<EPEquipment.suspendRateItemID, InventoryItem.stdCost>))]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Suspend Rate")]
		public virtual decimal? SuspendRate
		{
			get
			{
				return this._SuspendRate;
			}
			set
			{
				this._SuspendRate = value;
			}
		}
		#endregion
		#region DefAccountGroupID
		public abstract class defAccountGroupID : PX.Data.BQL.BqlInt.Field<defAccountGroupID> { }
		protected Int32? _DefAccountGroupID;
		[AccountGroup(DisplayName = "Default Account Group")]
		public virtual Int32? DefAccountGroupID
		{
			get
			{
				return this._DefAccountGroupID;
			}
			set
			{
				this._DefAccountGroupID = value;
			}
		}
		#endregion
		#region DefaultAccountID
		public abstract class defaultAccountID : PX.Data.BQL.BqlInt.Field<defaultAccountID> { }
		protected Int32? _DefaultAccountID;
		[Account(DisplayName = "Default Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultAccountID
		{
			get
			{
				return this._DefaultAccountID;
			}
			set
			{
				this._DefaultAccountID = value;
			}
		}
		#endregion
		#region DefaultSubID
		public abstract class defaultSubID : PX.Data.BQL.BqlInt.Field<defaultSubID> { }
		protected Int32? _DefaultSubID;
		[SubAccount(DisplayName = "Default Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultSubID
		{
			get
			{
				return this._DefaultSubID;
			}
			set
			{
				this._DefaultSubID = value;
			}
		}
		#endregion

		#region Attributes
		[CRAttributesField(typeof(EPEquipment.classID))]
		public virtual string[] Attributes { get; set; }
	
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		[PXString(20)]
		public string ClassID
		{
			get { return PX.Objects.PM.GroupTypes.Equipment; }
		}


		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.OS, "{0}", new Type[] { typeof(EPEquipment.equipmentCD) },
		   new Type[] { typeof(EPEquipment.description) },
		   Line1Format = "{0}", Line1Fields = new Type[] { typeof(EPEquipment.status) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(EPEquipment.description) }
		)]
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

	
	public static class EPEquipmentStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Active, InActive },
				new string[] { Messages.Active, Messages.Inactive }) { ; }
		}
		public const string Active = "A";
        public const string InActive = "I";

		public class EquipmentStatusActive : PX.Data.BQL.BqlString.Constant<EquipmentStatusActive>
		{
			public EquipmentStatusActive() : base(EPEquipmentStatus.Active) { ;}
		}
	}

}
