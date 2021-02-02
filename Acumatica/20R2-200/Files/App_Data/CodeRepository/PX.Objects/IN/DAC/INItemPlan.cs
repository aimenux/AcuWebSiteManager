using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using NonZeroPlansByInventory = PX.Data.Select<PX.Objects.IN.INItemPlan,
				PX.Data.Where<PX.Objects.IN.INItemPlan.inventoryID, PX.Data.Equal<PX.Data.Current<PX.Objects.IN.InventoryItem.inventoryID>>,
					PX.Data.And<PX.Objects.IN.INItemPlan.planQty, PX.Data.NotEqual<PX.Objects.CS.decimal0>>>>;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INItemPlan)]
	public partial class INItemPlan : PX.Data.IBqlTable, IQtyPlanned
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemPlan>.By<planID>
		{
			public static INItemPlan Find(PXGraph graph, long? planID) => FindBy(graph, planID);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INItemPlan>.By<siteID> { }
			public class PlanType : INPlanType.PK.ForeignKeyOf<INItemPlan>.By<planType> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemPlan>.By<inventoryID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INItemPlan>.By<locationID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INItemPlan>.By<subItemID> { }
			public class LotSerialStatus : INLotSerialStatus.PK.ForeignKeyOf<INItemPlan>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<INItemPlan>.By<vendorID> { }
			public class SourceSite : INSite.PK.ForeignKeyOf<INItemPlan>.By<sourceSiteID> { }
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<INItemPlan>.By<bAccountID> { }
			public class OrigItemPlan : INItemPlanOrig.PK.ForeignKeyOf<INItemPlan>.By<origPlanID> { }
			public class SupplyItemPlan : INItemPlanSupply.PK.ForeignKeyOf<INItemPlan>.By<supplyPlanID> { }
			public class DemandItemPlan : INItemPlanDemand.PK.ForeignKeyOf<INItemPlan>.By<demandPlanID> { }
			public class OrigPlanType : INPlanType.PK.ForeignKeyOf<INItemPlan>.By<origPlanType> { }
		}
        #endregion
        #region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule :
				InventoryItem.baseUnit.PreventEditIfExists<NonZeroPlansByInventory>
			{ }

			public class InventoryLotSerClassIDRule :
				PreventEditOf<InventoryItem.lotSerClassID>.On<InventoryItemMaint>.IfExists<
					Select<INItemPlan,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And<planType, NotIn3<INPlanConstants.plan60, INPlanConstants.plan68, INPlanConstants.plan69, INPlanConstants.plan70, INPlanConstants.plan73>,
						And<planQty, NotEqual<decimal0>>>>>>
			{
				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object firstPreventingEntity, string fieldName, string currentTableName, string foreignTableName)
					=> PXMessages.Localize(Messages.ItemLotSerClassVerifying);
			}

			public class InventoryDecimalBaseUnitRule: PreventEditOf<InventoryItem.decimalBaseUnit>.On<InventoryItemMaintBase>.IfExists<NonZeroPlansByInventory>
			{
				protected override void OnPreventEdit(GetEditPreventingReasonArgs args)
				{
					if ((bool?)args.NewValue == true)
						args.Cancel = true;
				}

				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs args,
					object firstPreventingEntity, string fieldName, string currentTableName, string foreignTableName)
				{
					var planType = INPlanType.PK.Find(args.Graph, ((INItemPlan)firstPreventingEntity).PlanType);
					return PXMessages.LocalizeFormat(IN.Messages.DecimalBaseUnitCouldNotUnchecked, ((InventoryItem)args.Row).BaseUnit, planType.Descr);
				}
			}
		}
		protected Int32? _InventoryID;
		[AnyInventory]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site()]
		[PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
		[PXDefault()]
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
		#region PlanDate
		public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }
		protected DateTime? _PlanDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Planned On")]
		public virtual DateTime? PlanDate
		{
			get
			{
				return this._PlanDate;
			}
			set
			{
				this._PlanDate = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLongIdentity(IsKey = true)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region FixedSource
		public abstract class fixedSource : PX.Data.BQL.BqlString.Field<fixedSource> { }
		protected String _FixedSource;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Fixed Source")]
		[PXDefault(INReplenishmentSource.Purchased, PersistingCheck = PXPersistingCheck.Nothing)]
		[INReplenishmentSource.INPlanList]
		public virtual String FixedSource
		{
			get
			{
				return this._FixedSource;
			}
			set
			{
				this._FixedSource = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected bool? _Active = false;
		[PXExistance()]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName="Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true, DescriptionField = typeof(INPlanType.descr))]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region ExcludePlanLevel
		public abstract class excludePlanLevel : PX.Data.BQL.BqlInt.Field<excludePlanLevel> { }
		[PXDBInt]
		public int? ExcludePlanLevel { get; set; }
		#endregion
        #region OrigPlanID
        public abstract class origPlanID : PX.Data.BQL.BqlLong.Field<origPlanID> { }
        protected Int64? _OrigPlanID;
        [PXDBLong()]
        public virtual Int64? OrigPlanID
        {
            get
            {
                return this._OrigPlanID;
            }
            set
            {
                this._OrigPlanID = value;
            }
        }
        #endregion
		#region OrigPlanType
		public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
		protected String _OrigPlanType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Orig. Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
		public virtual String OrigPlanType
		{
			get
			{
				return this._OrigPlanType;
			}
			set
			{
				this._OrigPlanType = value;
			}
		}
		#endregion
        #region OrigNoteID
        public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
        protected Guid? _OrigNoteID;
        [PXDBGuid()]
        public virtual Guid? OrigNoteID
        {
            get
            {
                return this._OrigNoteID;
            }
            set
            {
                this._OrigNoteID = value;
            }
        }
        #endregion
        #region OrigPlanLevel
        public abstract class origPlanLevel : PX.Data.BQL.BqlInt.Field<origPlanLevel> { }
        [PXDBInt()]
        public int? OrigPlanLevel
        {
            get;
            set;
        }
        #endregion
		#region IgnoreOrigPlan
		public abstract class ignoreOrigPlan : PX.Data.BQL.BqlBool.Field<ignoreOrigPlan> { }
		/// <summary>
		/// The field is used for breaking inheritance between plans.
		/// It may be needed, e.g., when Lot/Serial Number of the base plan differs from the derivative one.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public bool? IgnoreOrigPlan { get; set; }
		#endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem()]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INItemPlan.siteID), ValidComboRequired = false)]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[LotSerialNbr]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt()]
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
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		protected Int32? _VendorLocationID;
		[PXDBInt()]
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion
		#region SourceSiteID
		public abstract class sourceSiteID : PX.Data.BQL.BqlInt.Field<sourceSiteID> { }
		protected Int32? _SourceSiteID;
		[Site]		
		[PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
		public virtual Int32? SourceSiteID
		{
			get
			{
				return this._SourceSiteID;
			}
			set
			{
				this._SourceSiteID = value;
			}
		}
		#endregion
		#region SupplyPlanID
		public abstract class supplyPlanID : PX.Data.BQL.BqlLong.Field<supplyPlanID> { }
		protected Int64? _SupplyPlanID;
		[PXDBLong()]
		[PXSelector(typeof(Search<INItemPlan.planID>), DirtyRead = true)]
		public virtual Int64? SupplyPlanID
		{
			get
			{
				return this._SupplyPlanID;
			}
			set
			{
				this._SupplyPlanID = value;
			}
		}
		#endregion
		#region DemandPlanID
		public abstract class demandPlanID : PX.Data.BQL.BqlLong.Field<demandPlanID> { }
		protected Int64? _DemandPlanID;
		[PXDBLong()]
		[PXSelector(typeof(Search<INItemPlan.planID>))]
		public virtual Int64? DemandPlanID
		{
			get
			{
				return this._DemandPlanID;
			}
			set
			{
				this._DemandPlanID = value;
			}
		}
		#endregion
		#region PlanQty
		public abstract class planQty : PX.Data.BQL.BqlDecimal.Field<planQty> { }
		protected Decimal? _PlanQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName="Planned Qty.")]
		public virtual Decimal? PlanQty
		{
			get
			{
				return this._PlanQty;
			}
			set
			{
				this._PlanQty = value;
			}
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXDBGuid()]
		[PXDefault()]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "On Hold")]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Reverse
		public abstract class reverse : PX.Data.BQL.BqlBool.Field<reverse> { }
		protected Boolean? _Reverse;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Reverse")]
		public virtual Boolean? Reverse
		{
			get
			{
				return this._Reverse;
			}
			set
			{
				this._Reverse = value;
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
		#region Methods
		public static implicit operator SiteStatus(INItemPlan item)
		{
			SiteStatus ret = new SiteStatus();
			ret.InventoryID = item.InventoryID;
			ret.SubItemID = item.SubItemID;
			ret.SiteID = item.SiteID;

			return ret;
		}

		public static implicit operator LocationStatus(INItemPlan item)
		{
			LocationStatus ret = new LocationStatus();
			ret.InventoryID = item.InventoryID;
			ret.SubItemID = item.SubItemID;
			ret.SiteID = item.SiteID;
			ret.LocationID = item.LocationID;

			return ret;
		}

		public static implicit operator LotSerialStatus(INItemPlan item)
		{
			LotSerialStatus ret = new LotSerialStatus();
			ret.InventoryID = item.InventoryID;
			ret.SubItemID = item.SubItemID;
			ret.SiteID = item.SiteID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;

			return ret;
		}

        public static implicit operator ItemLotSerial(INItemPlan item)
        {
            ItemLotSerial ret = new ItemLotSerial();
            ret.InventoryID = item.InventoryID;
            ret.LotSerialNbr = item.LotSerialNbr;

			return ret;
        }

        public static implicit operator SiteLotSerial(INItemPlan item)
        {
            SiteLotSerial ret = new SiteLotSerial();
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.LotSerialNbr = item.LotSerialNbr;

            return ret;
        }
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt()]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region IsTemporary
		public abstract class isTemporary : PX.Data.BQL.BqlBool.Field<isTemporary> { }
		/// <summary>
		/// The flag indicates if the record is not for persistence
		/// </summary>
		[PXBool]
		public virtual bool? IsTemporary { get; set; }
		#endregion
		#region RefEntityType
		public abstract class refEntityType : PX.Data.BQL.BqlString.Field<refEntityType> { }
		[PXDBString(255, IsUnicode = false)]
		[PXDefault]
		public string RefEntityType
		{
			get;
			set;
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
	}

	/// <summary>
	/// An alias for Supply Inventory Plan (<see cref="INItemPlan"/>) which can be used for building complex BQL queries.
	/// </summary>
	[PXBreakInheritance]
	[PXHidden]
	public class INItemPlanSupply : INItemPlan
	{
		#region Keys
		public new class PK : PrimaryKeyOf<INItemPlanSupply>.By<planID>
		{
			public static INItemPlanSupply Find(PXGraph graph, long? planID) => FindBy(graph, planID);
		}
		#endregion
		#region PlanID
		public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID>
		{
		}
		#endregion
	}

	/// <summary>
	/// An alias for Demand Inventory Plan (<see cref="INItemPlan"/>) which can be used for building complex BQL queries.
	/// </summary>
	[PXBreakInheritance]
	[PXHidden]
	public class INItemPlanDemand : INItemPlan
	{
		#region Keys
		public new class PK : PrimaryKeyOf<INItemPlanDemand>.By<planID>
		{
			public static INItemPlanDemand Find(PXGraph graph, long? planID) => FindBy(graph, planID);
		}
		#endregion
		#region PlanID
		public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID>
		{
		}
		#endregion
	}

	/// <summary>
	/// An alias for Original Inventory Plan (<see cref="INItemPlan"/>) which can be used for building complex BQL queries.
	/// </summary>
	[PXBreakInheritance]
	[PXHidden]
	public class INItemPlanOrig : INItemPlan
	{
		#region Keys
		public new class PK : PrimaryKeyOf<INItemPlanOrig>.By<planID>
		{
			public static INItemPlanOrig Find(PXGraph graph, long? planID) => FindBy(graph, planID);
		}
		#endregion
		#region PlanID
		public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID>
		{
		}
		#endregion
	}
}
