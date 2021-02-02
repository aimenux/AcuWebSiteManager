using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PX.Api;
using PX.Common;
using PX.Objects.CR;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.Objects.IN.Attributes;
using System.Web.Compilation;
using PX.Objects.Common.Attributes;

namespace PX.Objects.IN
{
	using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;

	#region QtyAllocType

	public static class QtyAllocType // = buckets
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new[] {Pair("undefined", "undefined")}) {}

			public override bool IsLocalizable { get { return false; } }

			public override void CacheAttached(PXCache sender)
			{
				PXCache cache = sender.Graph.Caches[typeof(INPlanType)];

				List<string> values = new List<string>();
				List<string> labels = new List<string>();

				foreach (string fieldName in cache.Fields)
				{
					object val = cache.GetStateExt(null, fieldName);

					if (val is PXIntState)
					{
						values.Add(char.ToLower(fieldName[0]) + fieldName.Substring(1));
						labels.Add(((PXIntState)val).DisplayName);
					}
				}

				this._AllowedValues = values.ToArray();
				this._AllowedLabels = labels.ToArray();
				this._NeutralAllowedLabels = _AllowedLabels;

				base.CacheAttached(sender);
			}
		}
	}
	#endregion

	#region QtyAllocDocType
	public static class QtyAllocDocType
	{
	}
	#endregion

	#region Filter

	[Serializable]
	public partial class InventoryAllocDetEnqFilter : PX.Data.IBqlTable, IQtyAllocated
	{

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDefault()]
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[SubItemRawExt(typeof(InventoryAllocDetEnqFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion
		#region SubItemCDWildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		//        [Site(Visibility = PXUIVisibility.Visible)]
		[Site(DescriptionField = typeof(INSite.descr), DisplayName = "Warehouse")]
		//        [PXDefault()]
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

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(InventoryAllocDetEnqFilter.siteID), KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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
		#region LotSerialNbrWildcard
		public abstract class lotSerialNbrWildcard : PX.Data.BQL.BqlString.Field<lotSerialNbrWildcard> { };
		[PXDBString(100, IsUnicode = true)]
		public virtual String LotSerialNbrWildcard
		{
			get
			{
				var sql = PXDatabase.Provider.SqlDialect;
				return sql.WildcardAnything + this._LotSerialNbr + sql.WildcardAnything;
			}
		}
		#endregion


		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "On Hand", Enabled = false)]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion

		#region QtyTotalAddition
		public abstract class qtyTotalAddition : PX.Data.BQL.BqlDecimal.Field<qtyTotalAddition> { }
		protected Decimal? _QtyTotalAddition;
		[InventoryAllocationField(IsAddition = true, IsTotal = true)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Addition", Enabled = false)]
		public virtual Decimal? QtyTotalAddition
		{
			get
			{
				return this._QtyTotalAddition;
			}
			set
			{
				this._QtyTotalAddition = value;
			}
		}
		#endregion

		#region QtyPOPrepared
		public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		protected Decimal? _QtyPOPrepared;
		[InventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InclQtyPOPrepared), SortOrder = 10)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase Prepared", Enabled = false)]
		public virtual Decimal? QtyPOPrepared
		{
			get
			{
				return this._QtyPOPrepared;
			}
			set
			{
				this._QtyPOPrepared = value;
			}
		}
		#endregion
		#region InclQtyPOPrepared
		public abstract class inclQtyPOPrepared : PX.Data.BQL.BqlBool.Field<inclQtyPOPrepared> { }
		protected bool? _InclQtyPOPrepared;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyPOPrepared
		{
			get
			{
				return this._InclQtyPOPrepared;
			}
			set
			{
				this._InclQtyPOPrepared = value;
			}
		}
		#endregion


		#region QtyPOOrders
		public abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		protected Decimal? _QtyPOOrders;
		[InventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InclQtyPOOrders), SortOrder = 20)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase Orders", Enabled = false)]
		public virtual Decimal? QtyPOOrders
		{
			get
			{
				return this._QtyPOOrders;
			}
			set
			{
				this._QtyPOOrders = value;
			}
		}
		#endregion
		#region InclQtyPOOrders
		public abstract class inclQtyPOOrders : PX.Data.BQL.BqlBool.Field<inclQtyPOOrders> { }
		protected bool? _InclQtyPOOrders;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyPOOrders
		{
			get
			{
				return this._InclQtyPOOrders;
			}
			set
			{
				this._InclQtyPOOrders = value;
			}
		}
		#endregion

		#region QtyPOReceipts
		public abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		protected Decimal? _QtyPOReceipts;
		[InventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InclQtyPOReceipts), SortOrder = 30)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "PO Receipts", Enabled = false)]
		public virtual Decimal? QtyPOReceipts
		{
			get
			{
				return this._QtyPOReceipts;
			}
			set
			{
				this._QtyPOReceipts = value;
			}
		}
		#endregion
		#region InclQtyPOReceipts
		public abstract class inclQtyPOReceipts : PX.Data.BQL.BqlBool.Field<inclQtyPOReceipts> { }
		protected bool? _InclQtyPOReceipts;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyPOReceipts
		{
			get
			{
				return this._InclQtyPOReceipts;
			}
			set
			{
				this._InclQtyPOReceipts = value;
			}
		}
		#endregion

		#region QtyINReceipts
		public abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		protected Decimal? _QtyINReceipts;
		[InventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InclQtyINReceipts), SortOrder = 40)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "IN Receipts [*]", Enabled = false)]
		public virtual Decimal? QtyINReceipts
		{
			get
			{
				return this._QtyINReceipts;
			}
			set
			{
				this._QtyINReceipts = value;
			}
		}
		#endregion
		#region InclQtyINReceipts
		public abstract class inclQtyINReceipts : PX.Data.BQL.BqlBool.Field<inclQtyINReceipts> { }
		protected bool? _InclQtyINReceipts;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyINReceipts
		{
			get
			{
				return this._InclQtyINReceipts;
			}
			set
			{
				this._InclQtyINReceipts = value;
			}
		}
		#endregion

		#region QtyINIssue
		public abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		protected Decimal? _QtyINIssues;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtyINIssues), SortOrder = 190)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "IN Issues [**]", Enabled = false)]
		public virtual Decimal? QtyINIssues
		{
			get
			{
				return this._QtyINIssues;
			}
			set
			{
				this._QtyINIssues = value;
			}
		}
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.BQL.BqlBool.Field<inclQtyINIssues> { }
		protected bool? _InclQtyINIssues;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyINIssues
		{
			get
			{
				return this._InclQtyINIssues;
			}
			set
			{
				this._InclQtyINIssues = value;
			}
		}
		#endregion

		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		protected Decimal? _QtyInTransit;
		[InventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InclQtyInTransit), SortOrder = 50)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In-Transit [**]", Enabled = false)]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
		#region InclQtyInTransit
		public abstract class inclQtyInTransit : PX.Data.BQL.BqlBool.Field<inclQtyInTransit> { }
		protected bool? _InclQtyInTransit;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyInTransit
		{
			get
			{
				return this._InclQtyInTransit;
			}
			set
			{
				this._InclQtyInTransit = value;
			}
		}
		#endregion

        #region QtyInTransitToSO
        public abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
        protected Decimal? _QtyInTransitToSO;
		[InventoryAllocationField(IsAddition = false, SortOrder = 60)]
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "In-Transit to SO", Enabled = false)]
        public virtual Decimal? QtyInTransitToSO
        {
            get
            {
                return this._QtyInTransitToSO;
            }
            set
            {
                this._QtyInTransitToSO = value;
            }
        }
        #endregion

		#region QtyTotalDeduction
		public abstract class qtyTotalDeduction : PX.Data.BQL.BqlDecimal.Field<qtyTotalDeduction> { }
		protected Decimal? _QtyTotalDeduction;
		[InventoryAllocationField(IsAddition = false, IsTotal = true)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Deduction", Enabled = false)]
		public virtual Decimal? QtyTotalDeduction
		{
			get
			{
				return this._QtyTotalDeduction;
			}
			set
			{
				this._QtyTotalDeduction = value;
			}
		}
		#endregion

		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		protected Decimal? _QtyNotAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "On Loc. Not Available", Enabled = false)]
		public virtual Decimal? QtyNotAvail
		{
			get
			{
				return this._QtyNotAvail;
			}
			set
			{
				this._QtyNotAvail = value;
			}
		}
		#endregion
		#region QtyOnHandNotAvail
		public abstract class qtyOnHandNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyOnHandNotAvail>
		{
		}
		/// <summary>
		/// Quantity on Hand on Not Available Locations
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyOnHandNotAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyExpired
		public abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
		protected Decimal? _QtyExpired;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Expired [*]", Enabled = false)]
		public virtual Decimal? QtyExpired
		{
			get
			{
				return this._QtyExpired;
			}
			set
			{
				this._QtyExpired = value;
			}
		}
        #endregion

        #region QtyFSSrvOrdPrepared
        public abstract class qtyFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
        protected Decimal? _QtyFSSrvOrdPrepared;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtyFSSrvOrdPrepared), SortOrder = 200)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS Prepared [**]", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdPrepared
        {
            get
            {
                return this._QtyFSSrvOrdPrepared;
            }
            set
            {
                this._QtyFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdPrepared
        public abstract class inclQtyFSSrvOrdPrepared : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
        protected bool? _InclQtyFSSrvOrdPrepared;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? InclQtyFSSrvOrdPrepared
        {
            get
            {
                return this._InclQtyFSSrvOrdPrepared;
            }
            set
            {
                this._InclQtyFSSrvOrdPrepared = value;
            }
        }
        #endregion

        #region QtySOPrepared
        public abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		protected Decimal? _QtySOPrepared;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtySOPrepared), SortOrder = 140)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Prepared", Enabled = false)]
		public virtual Decimal? QtySOPrepared
		{
			get
			{
				return this._QtySOPrepared;
			}
			set
			{
				this._QtySOPrepared = value;
			}
		}
		#endregion
		#region InclQtySOPrepared
		public abstract class inclQtySOPrepared : PX.Data.BQL.BqlBool.Field<inclQtySOPrepared> { }
		protected bool? _InclQtySOPrepared;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtySOPrepared
		{
			get
			{
				return this._InclQtySOPrepared;
			}
			set
			{
				this._InclQtySOPrepared = value;
			}
		}
        #endregion

        #region QtyFSSrvOrdBooked
        public abstract class qtyFSSrvOrdBooked : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
        protected Decimal? _QtyFSSrvOrdBooked;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtyFSSrvOrdBooked), SortOrder = 210)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS Booked [**]", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdBooked
        {
            get
            {
                return this._QtyFSSrvOrdBooked;
            }
            set
            {
                this._QtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdBooked
        public abstract class inclQtyFSSrvOrdBooked : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdBooked> { }
        protected bool? _InclQtyFSSrvOrdBooked;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? InclQtyFSSrvOrdBooked
        {
            get
            {
                return this._InclQtyFSSrvOrdBooked;
            }
            set
            {
                this._InclQtyFSSrvOrdBooked = value;
            }
        }
        #endregion

        #region QtySOBooked
        public abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		protected Decimal? _QtySOBooked;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtySOBooked), SortOrder = 150)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Booked [**]", Enabled = false)]
		public virtual Decimal? QtySOBooked
		{
			get
			{
				return this._QtySOBooked;
			}
			set
			{
				this._QtySOBooked = value;
			}
		}
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.BQL.BqlBool.Field<inclQtySOBooked> { }
		protected bool? _InclQtySOBooked;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtySOBooked
		{
			get
			{
				return this._InclQtySOBooked;
			}
			set
			{
				this._InclQtySOBooked = value;
			}
		}
        #endregion

        #region QtyFSSrvOrdAllocated
        public abstract class qtyFSSrvOrdAllocated : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
        protected Decimal? _QtyFSSrvOrdAllocated;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtyFSSrvOrdAllocated), SortOrder = 220)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS Allocated [**]", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdAllocated
        {
            get
            {
                return this._QtyFSSrvOrdAllocated;
            }
            set
            {
                this._QtyFSSrvOrdAllocated = value;
            }
        }
        #endregion
        #region InclQtyFSSrvOrdAllocated
        public abstract class inclQtyFSSrvOrdAllocated : PX.Data.BQL.BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
        protected bool? _InclQtyFSSrvOrdAllocated;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? InclQtyFSSrvOrdAllocated
        {
            get
            {
                return this._InclQtyFSSrvOrdAllocated;
            }
            set
            {
                this._InclQtyFSSrvOrdAllocated = value;
            }
        }
        #endregion

        #region QtySOShipping
        public abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		protected Decimal? _QtySOShipping;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtySOShipping), SortOrder = 160)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Allocated [**]", Enabled = false)]
		public virtual Decimal? QtySOShipping
		{
			get
			{
				return this._QtySOShipping;
			}
			set
			{
				this._QtySOShipping = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.BQL.BqlBool.Field<inclQtySOShipping> { }
		protected bool? _InclQtySOShipping;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region QtySOShippingReverse
		public abstract class qtySOShippingReverse : PX.Data.BQL.BqlDecimal.Field<qtySOShippingReverse> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOShippingReverse { get; set; }
		#endregion

		#region QtySOShipped
		public abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		protected Decimal? _QtySOShipped;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtySOShipped), SortOrder = 170)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Shipped [**]", Enabled = false)]
		public virtual Decimal? QtySOShipped
		{
			get
			{
				return this._QtySOShipped;
			}
			set
			{
				this._QtySOShipped = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.BQL.BqlBool.Field<inclQtySOShipped> { }
		protected bool? _InclQtySOShipped;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region QtySOShippedReverse
		public abstract class qtySOShippedReverse : PX.Data.BQL.BqlDecimal.Field<qtySOShippedReverse> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOShippedReverse { get; set; }
		#endregion

		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		protected Decimal? _QtyINAssemblySupply;
		[InventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InclQtyINAssemblySupply), SortOrder = 70)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Kit Assembly Supply", Enabled = false)]
		public virtual Decimal? QtyINAssemblySupply
		{
			get
			{
				return this._QtyINAssemblySupply;
			}
			set
			{
				this._QtyINAssemblySupply = value;
			}
		}
		#endregion
		#region InclQtyINAssemblySupply
		public abstract class inclQtyINAssemblySupply : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblySupply> { }
		protected bool? _InclQtyINAssemblySupply;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyINAssemblySupply
		{
			get
			{
				return this._InclQtyINAssemblySupply;
			}
			set
			{
				this._InclQtyINAssemblySupply = value;
			}
		}
		#endregion

		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		protected Decimal? _QtyINAssemblyDemand;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtyINAssemblyDemand), SortOrder = 230)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Kit Assembly Demand", Enabled = false)]
		public virtual Decimal? QtyINAssemblyDemand
		{
			get
			{
				return this._QtyINAssemblyDemand;
			}
			set
			{
				this._QtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.BQL.BqlBool.Field<inclQtyINAssemblyDemand> { }
		protected bool? _InclQtyINAssemblyDemand;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyINAssemblyDemand
		{
			get
			{
				return this._InclQtyINAssemblyDemand;
			}
			set
			{
				this._InclQtyINAssemblyDemand = value;
			}
		}
        #endregion
        
        #region QtyInTransitToProduction
        public abstract class qtyInTransitToProduction : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
        protected Decimal? _QtyInTransitToProduction;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "In Transit to Production")]
        public virtual Decimal? QtyInTransitToProduction
        {
            get
            {
                return this._QtyInTransitToProduction;
            }
            set
            {
                this._QtyInTransitToProduction = value;
            }
        }
        #endregion
       
        #region QtyProductionSupplyPrepared
        public abstract class qtyProductionSupplyPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
        protected Decimal? _QtyProductionSupplyPrepared;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Supply Prepared")]
        public virtual Decimal? QtyProductionSupplyPrepared
        {
            get
            {
                return this._QtyProductionSupplyPrepared;
            }
            set
            {
                this._QtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionSupplyPrepared
        public abstract class inclQtyProductionSupplyPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupplyPrepared> { }
        protected bool? _InclQtyProductionSupplyPrepared;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false)]
        public virtual bool? InclQtyProductionSupplyPrepared
        {
            get
            {
                return this._InclQtyProductionSupplyPrepared;
            }
            set
            {
                this._InclQtyProductionSupplyPrepared = value;
            }
        }
        #endregion

        #region QtyProductionSupply
        public abstract class qtyProductionSupply : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
        protected Decimal? _QtyProductionSupply;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Supply")]
        public virtual Decimal? QtyProductionSupply
        {
            get
            {
                return this._QtyProductionSupply;
            }
            set
            {
                this._QtyProductionSupply = value;
            }
        }
        #endregion
        #region InclQtyProductionSupply
        public abstract class inclQtyProductionSupply : PX.Data.BQL.BqlBool.Field<inclQtyProductionSupply> { }
        protected bool? _InclQtyProductionSupply;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false)]
        public virtual bool? InclQtyProductionSupply
        {
            get
            {
                return this._InclQtyProductionSupply;
            }
            set
            {
                this._InclQtyProductionSupply = value;
            }
        }
        #endregion

        #region QtyPOFixedProductionPrepared
        public abstract class qtyPOFixedProductionPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
        protected Decimal? _QtyPOFixedProductionPrepared;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Purchase for Prod. Prepared")]
        public virtual Decimal? QtyPOFixedProductionPrepared
        {
            get
            {
                return this._QtyPOFixedProductionPrepared;
            }
            set
            {
                this._QtyPOFixedProductionPrepared = value;
            }
        }
        #endregion

        #region QtyPOFixedProductionOrders
        public abstract class qtyPOFixedProductionOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
        protected Decimal? _QtyPOFixedProductionOrders;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Purchase for Production")]
        public virtual Decimal? QtyPOFixedProductionOrders
        {
            get
            {
                return this._QtyPOFixedProductionOrders;
            }
            set
            {
                this._QtyPOFixedProductionOrders = value;
            }
        }
        #endregion
        
        #region QtyProductionDemandPrepared
        public abstract class qtyProductionDemandPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
        protected Decimal? _QtyProductionDemandPrepared;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Demand Prepared")]
        public virtual Decimal? QtyProductionDemandPrepared
        {
            get
            {
                return this._QtyProductionDemandPrepared;
            }
            set
            {
                this._QtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region InclQtyProductionDemandPrepared
        public abstract class inclQtyProductionDemandPrepared : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemandPrepared> { }
        protected bool? _InclQtyProductionDemandPrepared;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false)]
        public virtual bool? InclQtyProductionDemandPrepared
        {
            get
            {
                return this._InclQtyProductionDemandPrepared;
            }
            set
            {
                this._InclQtyProductionDemandPrepared = value;
            }
        }
        #endregion
        
        #region QtyProductionDemand
        public abstract class qtyProductionDemand : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
        protected Decimal? _QtyProductionDemand;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Demand")]
        public virtual Decimal? QtyProductionDemand
        {
            get
            {
                return this._QtyProductionDemand;
            }
            set
            {
                this._QtyProductionDemand = value;
            }
        }
        #endregion
        #region InclQtyProductionDemand
        public abstract class inclQtyProductionDemand : PX.Data.BQL.BqlBool.Field<inclQtyProductionDemand> { }
        protected bool? _InclQtyProductionDemand;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false)]
        public virtual bool? InclQtyProductionDemand
        {
            get
            {
                return this._InclQtyProductionDemand;
            }
            set
            {
                this._InclQtyProductionDemand = value;
            }
        }
        #endregion
        
        #region QtyProductionAllocated
        public abstract class qtyProductionAllocated : PX.Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
        protected Decimal? _QtyProductionAllocated;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production Allocated")]
        public virtual Decimal? QtyProductionAllocated
        {
            get
            {
                return this._QtyProductionAllocated;
            }
            set
            {
                this._QtyProductionAllocated = value;
            }
        }
        #endregion
        #region InclQtyProductionAllocated
        public abstract class inclQtyProductionAllocated : PX.Data.BQL.BqlBool.Field<inclQtyProductionAllocated> { }
        protected bool? _InclQtyProductionAllocated;
        [PXDBBool()]
        [PXUIField(DisplayName = " ", Enabled = false)]
        public virtual bool? InclQtyProductionAllocated
        {
            get
            {
                return this._InclQtyProductionAllocated;
            }
            set
            {
                this._InclQtyProductionAllocated = value;
            }
        }
        #endregion

        #region QtySOFixedProduction
        public abstract class qtySOFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
        protected Decimal? _QtySOFixedProduction;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "SO to Production")]
        public virtual Decimal? QtySOFixedProduction
        {
            get
            {
                return this._QtySOFixedProduction;
            }
            set
            {
                this._QtySOFixedProduction = value;
            }
        }
        #endregion
        #region QtyProdFixedPurchase
        // M9
        public abstract class qtyProdFixedPurchase : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
        protected Decimal? _QtyProdFixedPurchase;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production to Purchase.  
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production to Purchase")]
        public virtual Decimal? QtyProdFixedPurchase
        {
            get
            {
                return this._QtyProdFixedPurchase;
            }
            set
            {
                this._QtyProdFixedPurchase = value;
            }
        }
        #endregion
        #region QtyProdFixedProduction
        // MA
        public abstract class qtyProdFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
        protected Decimal? _QtyProdFixedProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production to Production
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production to Production")]
        public virtual Decimal? QtyProdFixedProduction
        {
            get
            {
                return this._QtyProdFixedProduction;
            }
            set
            {
                this._QtyProdFixedProduction = value;
            }
        }
        #endregion
        #region QtyProdFixedProdOrdersPrepared
        // MB
        public abstract class qtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
        protected Decimal? _QtyProdFixedProdOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for Prod. Prepared
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production for Prod. Prepared")]
        public virtual Decimal? QtyProdFixedProdOrdersPrepared
        {
            get
            {
                return this._QtyProdFixedProdOrdersPrepared;
            }
            set
            {
                this._QtyProdFixedProdOrdersPrepared = value;
            }
        }
        #endregion
        #region QtyProdFixedProdOrders
        // MC
        public abstract class qtyProdFixedProdOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
        protected Decimal? _QtyProdFixedProdOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for Production
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production for Production")]
        public virtual Decimal? QtyProdFixedProdOrders
        {
            get
            {
                return this._QtyProdFixedProdOrders;
            }
            set
            {
                this._QtyProdFixedProdOrders = value;
            }
        }
        #endregion
        #region QtyProdFixedSalesOrdersPrepared
        // MD
        public abstract class qtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
        protected Decimal? _QtyProdFixedSalesOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for SO Prepared
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production for SO Prepared")]
        public virtual Decimal? QtyProdFixedSalesOrdersPrepared
        {
            get
            {
                return this._QtyProdFixedSalesOrdersPrepared;
            }
            set
            {
                this._QtyProdFixedSalesOrdersPrepared = value;
            }
        }
        #endregion
        #region QtyProdFixedSalesOrders
        // ME
        public abstract class qtyProdFixedSalesOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
        protected Decimal? _QtyProdFixedSalesOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for SO Prepared
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Production for SO")]
        public virtual Decimal? QtyProdFixedSalesOrders
        {
            get
            {
                return this._QtyProdFixedSalesOrders;
            }
            set
            {
                this._QtyProdFixedSalesOrders = value;
            }
        }
        #endregion

        #region QtyINReplaned
        public abstract class qtyINReplaned : PX.Data.BQL.BqlDecimal.Field<qtyINReplaned> { }
		protected Decimal? _QtyINReplaned;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In Replaned", Enabled = false)]
		public virtual Decimal? QtyINReplaned
		{
			get
			{
				return this._QtyINReplaned;
			}
			set
			{
				this._QtyINReplaned = value;
			}
		}
		#endregion
		#region InclQtyINReplaned
		public abstract class inclQtyINReplaned : PX.Data.BQL.BqlBool.Field<inclQtyINReplaned> { }
		protected bool? _InclQtyINReplaned;
		[PXDBBool()]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual bool? InclQtyINReplaned
		{
			get
			{
				return this._InclQtyINReplaned;
			}
			set
			{
				this._InclQtyINReplaned = value;
			}
		}
        #endregion

        #region InclQtyPOFixedReceipt
        public abstract class inclQtyPOFixedReceipt : PX.Data.BQL.BqlBool.Field<inclQtyPOFixedReceipt> { }
        protected Boolean? _InclQtyPOFixedReceipt;
        [PXBool()]
        [PXDefault(typeof(False), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOFixedReceipt
        {
            get
            {
                return this._InclQtyPOFixedReceipt;
            }
            set
            {
                this._InclQtyPOFixedReceipt = value;
            }
        }
        #endregion

        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.BQL.BqlBool.Field<inclQtySOReverse> { }
		protected Boolean? _InclQtySOReverse;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual Boolean? InclQtySOReverse
		{
			get
			{
				return this._InclQtySOReverse;
			}
			set
			{
				this._InclQtySOReverse = value;
			}
		}
		#endregion
		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		protected Decimal? _QtySOBackOrdered;
		[InventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InclQtySOBackOrdered), SortOrder = 180)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Back Ordered [**]", Enabled = false)]
		public virtual Decimal? QtySOBackOrdered
		{
			get
			{
				return this._QtySOBackOrdered;
			}
			set
			{
				this._QtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlBool.Field<inclQtySOBackOrdered> { }
		protected Boolean? _InclQtySOBackOrdered;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = " ", Enabled = false)]
		public virtual Boolean? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available", Enabled = false)]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		protected Decimal? _QtyHardAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available for Shipping", Enabled = false)]
		public virtual Decimal? QtyHardAvail
		{
			get
			{
				return this._QtyHardAvail;
			}
			set
			{
				this._QtyHardAvail = value;
			}
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }
		protected decimal? _QtyActual;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available for Issue", Enabled = false)]
		public virtual decimal? QtyActual
		{
			get { return _QtyActual; }
			set { _QtyActual = value; }
		}
        #endregion

        #region QtyFixedFSSrvOrd
        public abstract class qtyFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
        protected decimal? _QtyFixedFSSrvOrd;
		[InventoryAllocationField(IsAddition = true, SortOrder = 260)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "FS to Purchase", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual decimal? QtyFixedFSSrvOrd
        {
            get
            {
                return this._QtyFixedFSSrvOrd;
            }
            set
            {
                this._QtyFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrd
        public abstract class qtyPOFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
        protected decimal? _QtyPOFixedFSSrvOrd;
		[InventoryAllocationField(IsAddition = true, SortOrder = 290)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Purchase for FS", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual decimal? QtyPOFixedFSSrvOrd
        {
            get
            {
                return this._QtyPOFixedFSSrvOrd;
            }
            set
            {
                this._QtyPOFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrdPrepared
        public abstract class qtyPOFixedFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
        protected decimal? _QtyPOFixedFSSrvOrdPrepared;
		[InventoryAllocationField(IsAddition = true, SortOrder = 280)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Purchase for FS Prepared", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual decimal? QtyPOFixedFSSrvOrdPrepared
        {
            get
            {
                return this._QtyPOFixedFSSrvOrdPrepared;
            }
            set
            {
                this._QtyPOFixedFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrdReceipts
        public abstract class qtyPOFixedFSSrvOrdReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
        protected decimal? _QtyPOFixedFSSrvOrdReceipts;
		[InventoryAllocationField(IsAddition = true, SortOrder = 300)]
		[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Receipts for FS", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
        public virtual decimal? QtyPOFixedFSSrvOrdReceipts
        {
            get
            {
                return this._QtyPOFixedFSSrvOrdReceipts;
            }
            set
            {
                this._QtyPOFixedFSSrvOrdReceipts = value;
            }
        }
        #endregion

        #region QtySOFixed
        public abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		protected decimal? _QtySOFixed;
		[InventoryAllocationField(IsAddition = false, SortOrder = 240)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO to Purchase", Enabled = false)]
		public virtual decimal? QtySOFixed
		{
			get
			{
				return this._QtySOFixed;
			}
			set
			{
				this._QtySOFixed = value;
			}
		}
		#endregion
		#region QtyPOFixedOrders
		public abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		protected decimal? _QtyPOFixedOrders;
		[InventoryAllocationField(IsAddition = true, SortOrder = 90)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for SO", Enabled = false)]
		public virtual decimal? QtyPOFixedOrders
		{
			get
			{
				return this._QtyPOFixedOrders;
			}
			set
			{
				this._QtyPOFixedOrders = value;
			}
		}
		#endregion
		#region QtyPOFixedPrepared
		public abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		protected decimal? _QtyPOFixedPrepared;
		[InventoryAllocationField(IsAddition = true, SortOrder = 80)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for SO Prepared", Enabled = false)]
		public virtual decimal? QtyPOFixedPrepared
		{
			get
			{
				return this._QtyPOFixedPrepared;
			}
			set
			{
				this._QtyPOFixedPrepared = value;
			}
		}
		#endregion
		#region QtyPOFixedReceipts
		public abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		protected decimal? _QtyPOFixedReceipts;
		[InventoryAllocationField(IsAddition = true, SortOrder = 100)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Receipts for SO", Enabled = false)]
		public virtual decimal? QtyPOFixedReceipts
		{
			get
			{
				return this._QtyPOFixedReceipts;
			}
			set
			{
				this._QtyPOFixedReceipts = value;
			}
		}
		#endregion
		#region QtySODropShip
		public abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		protected decimal? _QtySODropShip;
		[InventoryAllocationField(IsAddition = false, SortOrder = 250)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO to Drop-Ship", Enabled = false)]
		public virtual decimal? QtySODropShip
		{
			get
			{
				return this._QtySODropShip;
			}
			set
			{
				this._QtySODropShip = value;
			}
		}
		#endregion
		#region QtyPODropShipOrders
		public abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		protected decimal? _QtyPODropShipOrders;
		[InventoryAllocationField(IsAddition = true, SortOrder = 120)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship for SO", Enabled = false)]
		public virtual decimal? QtyPODropShipOrders
		{
			get
			{
				return this._QtyPODropShipOrders;
			}
			set
			{
				this._QtyPODropShipOrders = value;
			}
		}
		#endregion
		#region QtyPODropShipPrepared
		public abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		protected decimal? _QtyPODropShipPrepared;
		[InventoryAllocationField(IsAddition = true, SortOrder = 110)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship for SO Prepared", Enabled = false)]
		public virtual decimal? QtyPODropShipPrepared
		{
			get
			{
				return this._QtyPODropShipPrepared;
			}
			set
			{
				this._QtyPODropShipPrepared = value;
			}
		}
		#endregion
		#region QtyPODropShipReceipts
		public abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		protected decimal? _QtyPODropShipReceipts;
		[InventoryAllocationField(IsAddition = true, SortOrder = 130)]
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship for SO Receipts", Enabled = false)]
		public virtual decimal? QtyPODropShipReceipts
		{
			get
			{
				return this._QtyPODropShipReceipts;
			}
			set
			{
				this._QtyPODropShipReceipts = value;
			}
		}
		#endregion

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		protected String _BaseUnit;
		[PXDefault("")]
		[INUnit(DisplayName = "Base Unit", Enabled = false)]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}

        #endregion
        #region NegQty
        public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
		protected bool? _NegQty;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region InclQtyAvail
		public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
		protected Boolean? _InclQtyAvail;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? InclQtyAvail
		{
			get
			{
				return this._InclQtyAvail;
			}
			set
			{
				this._InclQtyAvail = value;
			}
		}
		#endregion


		#region Label
		public abstract class label : PX.Data.BQL.BqlString.Field<label> { }
		protected String _Label;
		[PXString]
		[PXUIField]
		[PXDefault(Messages.ExceptLocationNotAvailable, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Label
		{
			get
			{
				return this._Label;
			}
			set
			{
				this._Label = value;
			}
		}
		#endregion
		#region Label2
		public abstract class label2 : PX.Data.BQL.BqlString.Field<label2> { }
		protected String _Label2;
		[PXString]
		[PXUIField]
		[PXDefault(Messages.ExceptExpiredNotAvailable, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Label2
		{
			get
			{
				return this._Label2;
			}
			set
			{
				this._Label2 = value;
			}
		}
		#endregion
	}

	#endregion

	#region //Additional DAC
	/*
	// INTran alias (to ensure there are no released positive part of transfer)
	public class INTranAlias : INTran
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region OrigTranType
		public new abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType> { }
		#endregion
		#region OrigRefNbr
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		#endregion
		#region OrigLineNbr
		public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		#endregion
		#region InvtMult
		public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
	};
*/
	public class INTransfer : INRegister
    {
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion
        #region NoteID
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
    #endregion

    #region Resultset

    [Serializable]
	public partial class InventoryAllocDetEnqResult : PX.Data.IBqlTable, IQtyPlanned
	{
		public InventoryAllocDetEnqResult() { }

		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;
		//[BatchModule.List()] // other list ???
		[PXString(2)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region QADocType
		// QA means QtyAllocDocType , to not confuse with original DocType which is module-specific ...
		public abstract class qADocType : PX.Data.BQL.BqlString.Field<qADocType> { }
		protected String _QADocType;
		[PXString(100, IsUnicode = true)] // ???
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList]
		public virtual String QADocType
		{
			get
			{
				return this._QADocType;
			}
			set
			{
				this._QADocType = value;
			}
		}
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;

		[PXUIField(Visibility = PXUIVisibility.Invisible)]
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

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXString()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RefNbr { get; set; }
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Subitem")]
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
		//            [PXDBInt(IsKey = true)] //???
		[Location(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
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

		#region LocNotAvailable
		public abstract class locNotAvailable : PX.Data.BQL.BqlBool.Field<locNotAvailable> { }
		protected Boolean? _LocNotAvailable;
		[PXBool]
		[PXUIField(DisplayName = "Loc. Not Available", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? LocNotAvailable
		{
			get
			{
				return this._LocNotAvailable;
			}
			set
			{
				this._LocNotAvailable = value;
			}
		}
		#endregion

		#region SiteId
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		//[PXDBInt(IsKey = true)] //???
		[Site(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Warehouse")]
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


		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Number", Visibility = PXUIVisibility.SelectorVisible)]
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

		#region Expired
		public abstract class expired : PX.Data.BQL.BqlBool.Field<expired> { }
		protected Boolean? _Expired;
		[PXBool]
		[PXUIField(DisplayName = "Expired", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? Expired
		{
			get
			{
				return this._Expired;
			}
			set
			{
				this._Expired = value;
			}
		}
		#endregion

		#region AllocationType
		public abstract class allocationType : PX.Data.BQL.BqlString.Field<allocationType> { }
		protected String _AllocationType;
		[PXString(50)]
		[PXUIField(DisplayName = "Allocation Type", Visibility = PXUIVisibility.SelectorVisible)]
		[QtyAllocType.List()]
		public virtual String AllocationType
		{
			get
			{
				return this._AllocationType;
			}
			set
			{
				this._AllocationType = value;
			}
		}
		#endregion
		#region PlanDate
		public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }
		protected DateTime? _PlanDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Allocation Date")]
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
		#region PlanQty
		public abstract class planQty : PX.Data.BQL.BqlDecimal.Field<planQty> { }
		protected Decimal? _PlanQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Reverse
		public abstract class reverse : PX.Data.BQL.BqlBool.Field<reverse> { }
		protected Boolean? _Reverse;
		[PXDBBool()]
		[PXDefault()]
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
		#region //BaseUnit
		/*
			public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
			protected String _BaseUnit;
			[INUnit(DisplayName = "Base Unit")]
			public virtual String BaseUnit
			{
				get
				{
					return this._BaseUnit;
				}
				set
				{
					this._BaseUnit = value;
				}
			}
*/
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong()]
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
		#region ExcludePlanLevel
		public abstract class excludePlanLevel : PX.Data.BQL.BqlInt.Field<excludePlanLevel> { }
		[PXDBInt]
		public int? ExcludePlanLevel { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXInt()]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
		[PXUIField(DisplayName = "Account ID")]
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
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		protected String _AcctName;
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Name")]
		public virtual String AcctName
		{
			get
			{
				return this._AcctName;
			}
			set
			{
				this._AcctName = value;
			}
		}
		#endregion
	
		#region GridLineNbr
		// to be grid key
		public abstract class gridLineNbr : PX.Data.BQL.BqlInt.Field<gridLineNbr> { }
		protected Int32? _GridLineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual Int32? GridLineNbr
		{
			get
			{
				return this._GridLineNbr;
			}
			set
			{
				this._GridLineNbr = value;
			}
		}
		#endregion
	}

	#endregion
		

	[PX.Objects.GL.TableAndChartDashboardType]
	public class InventoryAllocDetEnq : PXGraph<InventoryAllocDetEnq>
	{
		#region Aliases
		[PXHidden]
		public class INItemPlan : IN.INItemPlan
		{
			[PXDBInt(IsKey = true), PXDefault]
			public override int? InventoryID { get; set; }
		}

		[PXHidden]
		public partial class INLocation : IBqlTable
		{
			#region LocationID
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			[PXDBForeignIdentity(typeof(INCostSite))]
			public virtual Int32? LocationID { get; set; }
			#endregion
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			[Site(IsKey = true)]
			public virtual Int32? SiteID { get; set; }
			#endregion
			#region LocationCD
			public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
			[LocationRaw(IsKey = true)]
			public virtual String LocationCD { get; set; }
			#endregion
			#region InclQtyAvail
			public abstract class inclQtyAvail : PX.Data.BQL.BqlBool.Field<inclQtyAvail> { }
			[PXDBBool]
			public virtual Boolean? InclQtyAvail { get; set; }
			#endregion
		}

		[PXHidden]
		public partial class BAccount : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBIdentity]
			public virtual Int32? BAccountID { get; set; }
			#endregion
			#region AcctCD
			public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
			[PXDBString(30, IsUnicode = true, IsKey = true)]
			public virtual String AcctCD { get; set; }
			#endregion
			#region AcctName
			public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
			[PXDBString(60, IsUnicode = true)]
			public virtual String AcctName { get; set; }
			#endregion
		}

		[PXHidden]
		public partial class INSite : IBqlTable
		{
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			[PXDBForeignIdentity(typeof(INCostSite))]
			public virtual Int32? SiteID { get; set; }
			#endregion
			#region SiteCD
			public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
			[SiteRaw(true, IsKey = true)]
			public virtual String SiteCD { get; set; }
			#endregion
		}

		[PXHidden]
		public class INRegister : IBqlTable
		{
			#region DocType
			[PXDBString(1, IsKey = true, IsFixed = true)]
			[INDocType.List]
			public virtual String DocType { get; set; }
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			public virtual String RefNbr { get; set; }
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region NoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			#endregion
		}

		[PXHidden]
		public class SOOrder : IBqlTable
		{
			#region OrderType
			[PXDBString(2, IsKey = true, IsFixed = true)]
			public virtual String OrderType { get; set; }
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			#endregion
			#region OrderNbr
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String OrderNbr { get; set; }
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			#endregion
			#region NoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			#endregion
		}

		[PXHidden]
		public class SOOrderShipment : IBqlTable
		{
			#region OrderType
			[PXDBString(2, IsKey = true, IsFixed = true)]
			public virtual String OrderType { get; set; }
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			#endregion
			#region OrderNbr
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String OrderNbr { get; set; }
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			#endregion
			#region ShipmentType
			[PXDBString(1, IsKey = true, IsFixed = true)]
			public virtual String ShipmentType { get; set; }
			public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
			#endregion
			#region ShipmentNbr
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String ShipmentNbr { get; set; }
			public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
			#endregion
			#region InvtDocType
			public abstract class invtDocType : PX.Data.BQL.BqlString.Field<invtDocType> { }
			[PXDBString(1, IsFixed = true)]
			public virtual string InvtDocType { get; set; }
			#endregion
			#region InvtRefNbr
			public abstract class invtRefNbr : PX.Data.BQL.BqlString.Field<invtRefNbr> { }
			[PXDBString(15, IsUnicode = true, InputMask = "")]
			public virtual string InvtRefNbr { get; set; }
			#endregion
			#region OrderNoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? OrderNoteID
			{
				get;
				set;
			}
			public abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID> { }
			#endregion
			#region ShipmentNoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? ShipmentNoteID { get; set; }
			public abstract class shipmentNoteID : PX.Data.BQL.BqlGuid.Field<shipmentNoteID> { }
			#endregion
			#region InvtNoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? InvtNoteID { get; set; }
			public abstract class invtNoteID : PX.Data.BQL.BqlGuid.Field<invtNoteID> { }
			#endregion
		}

		[PXHidden]
		public class SOShipment : IBqlTable
		{
			#region ShipmentNbr
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String ShipmentNbr { get; set; }
			public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
			#endregion
			#region ShipmentType
			[PXDBString(1, IsFixed = true)]
			[SOShipmentType.List]
			public virtual String ShipmentType { get; set; }
			public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
			#endregion
			#region NoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			#endregion
		}

		[PXHidden]
		public class ARRegister : IBqlTable
		{
			#region DocType
			[PXDBString(AR.ARRegister.docType.Length, IsKey = true, IsFixed = true)]
			[ARDocType.List()]
			public virtual string DocType { get; set; }
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			public virtual string RefNbr { get; set; }
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region NoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			#endregion
		}

		[PXHidden]
		public class POOrder : IBqlTable
		{
			#region OrderType
			[PXDBString(2, IsKey = true, IsFixed = true)]
			[POOrderType.List]
			public virtual String OrderType { get; set; }
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
	#endregion
			#region OrderNbr
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String OrderNbr { get; set; }
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
	#endregion
			#region NoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			#endregion
		}

		[PXHidden]
		public class POOrderReceipt : IBqlTable
		{
			#region ReceiptNbr
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			public virtual String ReceiptNbr { get; set; }
			public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
			#endregion
			#region POType
			[PXDBString(2, IsKey = true, IsFixed = true)]
			public virtual String POType { get; set; }
			public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
			#endregion
			#region PONbr
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			public virtual String PONbr { get; set; }
			public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
			#endregion
			#region OrderNoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? OrderNoteID { get; set; }
			public abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID> { }
			#endregion
			#region ReceiptNoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? ReceiptNoteID { get; set; }
			public abstract class receiptNoteID : PX.Data.BQL.BqlGuid.Field<receiptNoteID> { }
			#endregion
		}

		[PXHidden]
		public class POReceipt : IBqlTable
		{
			#region ReceiptType
			[PXDBString(2, IsFixed = true, IsKey = true)]
			[POReceiptType.List]
			public virtual String ReceiptType { get; set; }
			public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
			#endregion
			#region ReceiptNbr
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String ReceiptNbr { get; set; }
			public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
			#endregion
			#region NoteID
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			#endregion
		}

		[PXHidden]
		public partial class INTransitLine : IBqlTable
		{
			#region TransferNbr
			public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			public virtual String TransferNbr { get; set; }
			#endregion
			#region TransferLineNbr
			public abstract class transferLineNbr : PX.Data.BQL.BqlInt.Field<transferLineNbr> { }
			[PXDBInt(IsKey = true)]
			public virtual Int32? TransferLineNbr { get; set; }
			#endregion
			#region NoteID
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			[PXDBGuid(IsImmutable = true)]
			public virtual Guid? NoteID { get; set; }
			#endregion
			#region RefNoteID
			public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			[PXDBGuid()]
			public virtual Guid? RefNoteID { get; set; }
			#endregion
		}
		#endregion

		public class ItemPlanWithExtraInfo
		{
			public INItemPlan ItemPlan { get; }
			public INPlanType PlanType { get; }
			public INLocation Location { get; }
			public INLotSerialStatus LotSerialStatus { get; }
			public BAccount BAccount { get; }
			public IEnumerable<SOOrderShipment> ShipmentItemPlanToSOOrderItemPlanLinks { get; }
			public IEnumerable<POOrderReceipt> ReceiptItemPlanToPOOrderItemPlanLinks { get; }
			public object RefEntity { get; set; }

			public ItemPlanWithExtraInfo(
				PXResult
					<INItemPlan, INPlanType, INLocation, INLotSerialStatus, 
					BAccount, INSubItem, INSite, SOOrderShipment, ARRegister,
					POOrderReceipt, INRegister, SOOrder, POOrder, POReceipt, INTransitLine> actualRecord,
				IEnumerable<SOOrderShipment> soLinks,
				IEnumerable<POOrderReceipt> poLinks)
			{
				ItemPlan = actualRecord;
				PlanType = actualRecord;
				Location = actualRecord;
				LotSerialStatus = actualRecord;
				BAccount = actualRecord;
				ShipmentItemPlanToSOOrderItemPlanLinks = soLinks;
				ReceiptItemPlanToPOOrderItemPlanLinks = poLinks;

				INRegister reg = actualRecord;
				SOOrder so = actualRecord;
				ARRegister ar = actualRecord;
				POOrder po = actualRecord;
				POReceipt por = actualRecord;
                INTransitLine tl = actualRecord;
				SOShipment sos = null;

				SOOrderShipment shipment = soLinks.FirstOrDefault(r => r.ShipmentNoteID != null);
				if (shipment != null)
				{
					sos = new SOShipment()
					{
						ShipmentNbr = shipment.ShipmentNbr,
						ShipmentType = shipment.ShipmentType,
						NoteID = shipment.ShipmentNoteID
					};
				}

                RefEntity = reg.RefNbr != null
					? reg
					: so.OrderNbr != null
						? so
						: po.OrderNbr != null
							? po
							: (sos != null && sos.ShipmentNbr != null)
								? sos
								: (ar.RefNbr != null)
									? ar
									: por.ReceiptNbr != null
										? por
										: tl.TransferNbr != null
											? tl
											: (object)null;
			}
		}

		public PXFilter<CR.BAccount> Dummy_bAccount;
		public PXAction<InventoryAllocDetEnqFilter> RefreshTotal;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh, Tooltip = Messages.ttipRefresh, SpecialType = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "")]
		protected virtual IEnumerable refreshTotal(PXAdapter a)
		{
			Filter.Cache.Current = (InventoryAllocDetEnqFilter)Filter.Select();
			ResultRecords.View.RequestRefresh();
			return a.Get();
		}

		
		public PXCancel<InventoryAllocDetEnqFilter> Cancel;
		public PXAction<InventoryAllocDetEnqFilter> viewDocument;
		public PXFilter<InventoryAllocDetEnqFilter> Filter;
		[PXFilterable]
		public PXSelectOrderBy<InventoryAllocDetEnqResult,
				OrderBy<Asc<InventoryAllocDetEnqResult.module,
						Asc<InventoryAllocDetEnqResult.qADocType,
						Asc<InventoryAllocDetEnqResult.refNbr>>>>>
			ResultRecords;

		[PXFilterable]
		public PXSelectOrderBy<InventoryQtyByPlanType,
				OrderBy<Asc<InventoryQtyByPlanType.sortOrder>>> Addition;
		[PXFilterable]
		public PXSelectOrderBy<InventoryQtyByPlanType,
				OrderBy<Asc<InventoryQtyByPlanType.sortOrder>>> Deduction;

		protected Lazy<IEnumerable<InventoryAllocDetEnqResult>> cachedResultRecords;

		private readonly bool _warehouseLocationFeature;
		private readonly bool _lotSerialFeature;
		private readonly EntityHelper _entityHelper;

		private Dictionary<string, string> _displayNameByEntityType;

		public InventoryAllocDetEnq()
		{
			_displayNameByEntityType = new Dictionary<string, string>();

			ResultRecords.Cache.AllowInsert = false;
			ResultRecords.Cache.AllowDelete = false;
			ResultRecords.Cache.AllowUpdate = false;

			cachedResultRecords = new Lazy<IEnumerable<InventoryAllocDetEnqResult>>(CalculateResultRecords);

			_warehouseLocationFeature = PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>();
			_lotSerialFeature = PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>();
			_entityHelper = new EntityHelper(this);

			if (!_warehouseLocationFeature && !_lotSerialFeature)
			{
				PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyInTransit>(Filter.Cache, Messages.InTransit);
				PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOBooked>(Filter.Cache, Messages.SOBooked);
				PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOShipping>(Filter.Cache, Messages.SOAllocated);
				PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOShipped>(Filter.Cache, Messages.SOShipped);
				PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyINIssues>(Filter.Cache, Messages.INIssues);
				PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyINReceipts>(Filter.Cache, Messages.INReceipts);
			}
			else
			{
				if (_warehouseLocationFeature && _lotSerialFeature)
				{
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyInTransit>(Filter.Cache, Messages.InTransit2S);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOBooked>(Filter.Cache, Messages.SOBooked2S);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOShipping>(Filter.Cache, Messages.SOAllocated2S);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOShipped>(Filter.Cache, Messages.SOShipped2S);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyINIssues>(Filter.Cache, Messages.INIssues2S);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyINReceipts>(Filter.Cache, Messages.INReceiptsS);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyExpired>(Filter.Cache, Messages.ExpiredS);
				}
				else
				{
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyInTransit>(Filter.Cache, Messages.InTransitS);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOBooked>(Filter.Cache, Messages.SOBookedS);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOShipping>(Filter.Cache, Messages.SOAllocatedS);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtySOShipped>(Filter.Cache, Messages.SOShippedS);
					PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyINIssues>(Filter.Cache, Messages.INIssuesS);
					if (!_warehouseLocationFeature && _lotSerialFeature)
					{
						PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyINReceipts>(Filter.Cache, Messages.INReceipts);
						PXUIFieldAttribute.SetDisplayName<InventoryAllocDetEnqFilter.qtyExpired>(Filter.Cache, Messages.Expired);
					}
				}
			}
		}

		protected virtual void InventoryAllocDetEnqFilter_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void InventoryAllocDetEnqFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			InventoryAllocDetEnqFilter row = (InventoryAllocDetEnqFilter)e.Row;
			if (!_warehouseLocationFeature && !_lotSerialFeature)
			{
				PXUIFieldAttribute.SetVisible<InventoryAllocDetEnqFilter.label2>(sender, row, false);
				PXUIFieldAttribute.SetVisible<InventoryAllocDetEnqFilter.label>(sender, row, false);
			}
			else
			{
				PXUIFieldAttribute.SetVisible<InventoryAllocDetEnqFilter.label2>(sender, row, true);
				PXUIFieldAttribute.SetVisible<InventoryAllocDetEnqFilter.label>(sender, row, true);
				row.Label2 = PXMessages.LocalizeNoPrefix(Messages.ExceptExpired2S);
				if (!_warehouseLocationFeature)
				{
					PXUIFieldAttribute.SetVisible<InventoryAllocDetEnqFilter.label>(sender, row, false);
					row.Label2 = PXMessages.LocalizeNoPrefix(Messages.ExceptExpiredS);
				}
				if (!_lotSerialFeature)
				{
					PXUIFieldAttribute.SetVisible<InventoryAllocDetEnqFilter.label2>(sender, row, false);
				}
			}
			if (string.Equals(row.Label, Messages.ExceptLocationNotAvailable, StringComparison.Ordinal))
			{
				row.Label = PXMessages.LocalizeNoPrefix(Messages.ExceptLocationNotAvailable);
			}
		}

		protected virtual void InventoryAllocDetEnqResult_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual IEnumerable filter()
		{
			PXCache cache = this.Caches[typeof(InventoryAllocDetEnqFilter)];
			if (cache != null)
			{
				InventoryAllocDetEnqFilter filter = cache.Current as InventoryAllocDetEnqFilter;
				if (filter != null)
				{
					filter.QtyOnHand = 0m;

					filter.QtyTotalAddition = 0m;

					filter.QtyPOPrepared = 0m;
					filter.QtyPOOrders = 0m;
					filter.QtyPOReceipts = 0m;
					filter.QtyINReceipts = 0m;
					filter.QtyInTransit = 0m;
                    filter.QtyInTransitToSO = 0m;
					filter.QtyINAssemblySupply = 0m;
                    filter.QtyInTransitToProduction = 0m;
                    filter.QtyProductionSupplyPrepared = 0m;
                    filter.QtyProductionSupply = 0m;
                    filter.QtyPOFixedProductionPrepared = 0m;
                    filter.QtyPOFixedProductionOrders = 0m;
				    filter.QtyProdFixedProdOrdersPrepared = 0m;
				    filter.QtyProdFixedProdOrders = 0m;
				    filter.QtyProdFixedSalesOrdersPrepared = 0m;
				    filter.QtyProdFixedSalesOrders = 0m;

                    filter.QtyTotalDeduction = 0m;

					filter.QtyHardAvail = 0m;
					filter.QtyActual = 0m;
					filter.QtyNotAvail = 0m;
					filter.QtyOnHandNotAvail = 0m;
					filter.QtyExpired = 0m;

                    filter.QtyFSSrvOrdPrepared = 0m;
                    filter.QtyFSSrvOrdBooked = 0m;
                    filter.QtyFSSrvOrdAllocated = 0m;

                    filter.QtySOPrepared = 0m;
					filter.QtySOBooked = 0m;
					filter.QtySOShipping = 0m;
					filter.QtySOShippingReverse = 0m;
					filter.QtySOShipped = 0m;
					filter.QtySOShippedReverse = 0m;
					filter.QtyINIssues = 0m;
					filter.QtyINAssemblyDemand = 0m;
                    filter.QtyProductionDemandPrepared = 0m;
                    filter.QtyProductionDemand = 0m;
                    filter.QtyProductionAllocated = 0m;
                    filter.QtySOFixedProduction = 0m;
				    filter.QtyProdFixedPurchase = 0m;
				    filter.QtyProdFixedProduction = 0m;
                    filter.QtySOBackOrdered = 0m;

                    filter.QtyFixedFSSrvOrd = 0m;
                    filter.QtyPOFixedFSSrvOrd = 0m;
                    filter.QtyPOFixedFSSrvOrdPrepared = 0m;
                    filter.QtyPOFixedFSSrvOrdReceipts = 0m;

                    filter.QtySOFixed = 0m;
					filter.QtyPOFixedOrders = 0m;
					filter.QtyPOFixedPrepared = 0m;
					filter.QtyPOFixedReceipts = 0m;
					filter.QtySODropShip = 0m;
					filter.QtyPODropShipOrders = 0m;
					filter.QtyPODropShipPrepared = 0m;
					filter.QtyPODropShipReceipts = 0m;

					filter.QtyAvail = 0m;


					// InventoryId is required field 
					if (filter.InventoryID != null)
					{

						// 'included' checkboxes
						InventoryItem inventoryItemRec = InventoryItem.PK.Find(this, filter.InventoryID);
						INAvailabilityScheme availSchemeRec = PXSelectReadonly2<INAvailabilityScheme,
							InnerJoin<INItemClass, On<INItemClass.FK.AvailabilityScheme>>,
							Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>
							.Select(this, inventoryItemRec.ItemClassID);
						filter.InclQtyPOPrepared = availSchemeRec.InclQtyPOPrepared;
						filter.InclQtyPOOrders = availSchemeRec.InclQtyPOOrders;
						filter.InclQtyPOReceipts = availSchemeRec.InclQtyPOReceipts;
						filter.InclQtyINReceipts = availSchemeRec.InclQtyINReceipts;
						filter.InclQtyInTransit = availSchemeRec.InclQtyInTransit;

                        filter.InclQtyFSSrvOrdPrepared = availSchemeRec.InclQtyFSSrvOrdPrepared;
                        filter.InclQtyFSSrvOrdBooked = availSchemeRec.InclQtyFSSrvOrdBooked;
                        filter.InclQtyFSSrvOrdAllocated = availSchemeRec.InclQtyFSSrvOrdAllocated;

                        filter.InclQtySOPrepared = availSchemeRec.InclQtySOPrepared;
						filter.InclQtySOBooked = availSchemeRec.InclQtySOBooked;
						filter.InclQtySOShipping = availSchemeRec.InclQtySOShipping;
						filter.InclQtySOShipped = availSchemeRec.InclQtySOShipped;
						filter.InclQtyINIssues = availSchemeRec.InclQtyINIssues;
						filter.InclQtyINAssemblyDemand = availSchemeRec.InclQtyINAssemblyDemand;
						filter.InclQtyINAssemblySupply = availSchemeRec.InclQtyINAssemblySupply;
                        filter.InclQtyProductionDemandPrepared = availSchemeRec.InclQtyProductionDemandPrepared;
                        filter.InclQtyProductionDemand = availSchemeRec.InclQtyProductionDemand;
                        filter.InclQtyProductionAllocated = availSchemeRec.InclQtyProductionAllocated;
                        filter.InclQtyProductionSupplyPrepared = availSchemeRec.InclQtyProductionSupplyPrepared;
                        filter.InclQtyProductionSupply = availSchemeRec.InclQtyProductionSupply;
                        filter.InclQtySOBackOrdered = availSchemeRec.InclQtySOBackOrdered;
						filter.InclQtySOReverse = availSchemeRec.InclQtySOReverse;
						filter.BaseUnit = inventoryItemRec.BaseUnit;


						// QtyOnHand , QtyExpired , QtyLocNotAvail calculation :
						// simplified (without cost) version of code from IN401000
						PXSelectBase<INLocationStatus> calcStatusCmd = 
							new PXSelectReadonly2<INLocationStatus,
							//InnerJoin<INSiteStatus,
							//	On<INSiteStatus.inventoryID, Equal<INLocationStatus.inventoryID>,
							//	And<INSiteStatus.subItemID, Equal<INLocationStatus.subItemID>,
							//	And<INSiteStatus.siteID, Equal<INLocationStatus.siteID>>>>>,
										InnerJoin<InventoryItem,
											On<INLocationStatus.FK.InventoryItem>,
										InnerJoin<INLocation,
											On<INLocationStatus.FK.Location>,
										InnerJoin<INSubItem,
											On<INLocationStatus.FK.SubItem>,
										LeftJoin<INLotSerClass,
											On<InventoryItem.FK.LotSerClass>,
										LeftJoin<INLotSerialStatus,
											On<INLotSerialStatus.inventoryID, Equal<INLocationStatus.inventoryID>,
												And<INLotSerClass.lotSerAssign, Equal<INLotSerAssign.whenReceived>,
												And<INLotSerialStatus.subItemID, Equal<INLocationStatus.subItemID>,
												And<INLotSerialStatus.siteID, Equal<INLocationStatus.siteID>,
												And<INLotSerialStatus.locationID, Equal<INLocationStatus.locationID>,
												And<INLotSerClass.lotSerClassID, IsNotNull,
												And<INLotSerClass.lotSerTrack, NotEqual<INLotSerTrack.notNumbered>>>>>>>>,
										InnerJoin<INSite,
											On2<INLocationStatus.FK.Site,
												And<Match<IN.INSite, Current<AccessInfo.userName>>>>>>>>>>,
							Where<INLocationStatus.inventoryID, Equal<Current<InventoryAllocDetEnqFilter.inventoryID>>>,
							OrderBy<Asc<InventoryItem.inventoryCD, 
									Asc<INLocationStatus.siteID, 
									Asc<INSubItem.subItemCD, 
									Asc<INLocationStatus.locationID, 
									Asc<INLotSerialStatus.lotSerialNbr>>>>>>>(this);

						if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD))
							calcStatusCmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryAllocDetEnqFilter.subItemCDWildcard>>>>();

						if (filter.SiteID != null)
							calcStatusCmd.WhereAnd<Where<INLocationStatus.siteID, Equal<Current<InventoryAllocDetEnqFilter.siteID>>>>();

						if (filter.LocationID != null)
							calcStatusCmd.WhereAnd<Where<INLocationStatus.locationID, Equal<Current<InventoryAllocDetEnqFilter.locationID>>>>();

						if (!string.IsNullOrEmpty(filter.LotSerialNbr))
							calcStatusCmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Like<Current<InventoryAllocDetEnqFilter.lotSerialNbrWildcard>>>>();

						PXResultset<INLocationStatus> calcStatusRecs = calcStatusCmd.Select();

						// only 3 values here : QtyOnHand, QtyOnLocNotAvail, QtyExpired
						foreach (PXResult<INLocationStatus, /*INSiteStatus,*/ InventoryItem, INLocation, INSubItem, INLotSerClass, INLotSerialStatus> it in calcStatusRecs)
						{
							INLocationStatus ls_rec = it;
							//INSiteStatus ss_rec = it;
							InventoryItem ii_rec = it;
							//INSubItem si_rec = it;  
							INLocation l_rec = it;
							INLotSerialStatus lss_rec = it;

							filter.QtyOnHand += (lss_rec.QtyOnHand ?? ls_rec.QtyOnHand);

							if (!(l_rec.InclQtyAvail ?? true))
							{
								filter.QtyNotAvail += lss_rec.QtyAvail ?? ls_rec.QtyAvail;
								filter.QtyOnHandNotAvail += lss_rec.QtyOnHand ?? ls_rec.QtyOnHand;
							}
							else
							{
								if ((lss_rec.ExpireDate != null) && (DateTime.Compare((DateTime)this.Accessinfo.BusinessDate, (DateTime)lss_rec.ExpireDate) > 0))
								{
									filter.QtyExpired += lss_rec.QtyOnHand;
								}
							}
						}

						foreach (InventoryAllocDetEnqResult it in this.ResultRecords.Select()) //???
						{
							Aggregate(filter, it);
						}

						filter.QtyTotalAddition =
							((filter.InclQtyPOPrepared ?? false) ? filter.QtyPOPrepared : 0m)
							+ ((filter.InclQtyPOOrders ?? false) ? filter.QtyPOOrders : 0m)
							+ ((filter.InclQtyPOReceipts ?? false) ? filter.QtyPOReceipts : 0m)
							+ ((filter.InclQtyINReceipts ?? false) ? filter.QtyINReceipts : 0m)
							+ ((filter.InclQtyInTransit ?? false) ? filter.QtyInTransit : 0m)
							+ ((filter.InclQtyINAssemblySupply ?? false) ? filter.QtyINAssemblySupply : 0m)
							+ ((filter.InclQtyProductionSupplyPrepared ?? false) ? filter.QtyProductionSupplyPrepared : 0m)
							+ ((filter.InclQtyProductionSupply ?? false) ? filter.QtyProductionSupply : 0m);
							
						filter.QtyTotalDeduction =
                            filter.QtyExpired
							+ ((filter.InclQtySOPrepared ?? false) ? filter.QtySOPrepared : 0m)

                            + ((filter.InclQtyFSSrvOrdPrepared ?? false) ? filter.QtyFSSrvOrdPrepared : 0m)
                            + ((filter.InclQtyFSSrvOrdBooked ?? false) ? filter.QtyFSSrvOrdBooked : 0m)
                            + ((filter.InclQtyFSSrvOrdAllocated ?? false) ? filter.QtyFSSrvOrdAllocated : 0m)

                            + ((filter.InclQtySOBooked ?? false) ? filter.QtySOBooked : 0m)
							+ ((filter.InclQtySOShipping ?? false) ? filter.QtySOShipping : 0m)
							+ ((filter.InclQtySOShipped ?? false) ? filter.QtySOShipped : 0m)
							+ ((filter.InclQtyINIssues ?? false) ? filter.QtyINIssues : 0m)
							+ ((filter.InclQtyINAssemblyDemand ?? false) ? filter.QtyINAssemblyDemand : 0m)
							+ ((filter.InclQtyProductionDemandPrepared ?? false) ? filter.QtyProductionDemandPrepared : 0m)
							+ ((filter.InclQtyProductionDemand ?? false) ? filter.QtyProductionDemand : 0m)
							+ ((filter.InclQtyProductionAllocated ?? false) ? filter.QtyProductionAllocated : 0m)
							+ ((filter.InclQtySOBackOrdered ?? false) ? filter.QtySOBackOrdered : 0m);

                        filter.QtyAvail = filter.QtyOnHand + filter.QtyTotalAddition - (filter.QtyTotalDeduction + filter.QtyNotAvail);
						filter.QtyHardAvail =
							filter.QtyOnHand - filter.QtyOnHandNotAvail
							- filter.QtyINIssues
							- (filter.QtySOShipping - filter.QtySOShippingReverse)
							- (filter.QtySOShipped - filter.QtySOShippedReverse)
							- filter.QtyProductionAllocated;
						filter.QtyActual =
							filter.QtyOnHand - filter.QtyOnHandNotAvail
							- (filter.QtySOShipped - filter.QtySOShippedReverse);
					}
				}
			}
			yield return cache.Current;
			cache.IsDirty = false;
		}

		protected virtual void InventoryAllocDetEnqFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			cachedResultRecords = new Lazy<IEnumerable<InventoryAllocDetEnqResult>>(CalculateResultRecords);
		}

		protected virtual IEnumerable GetInventoryQtyByPlanType(bool isAddition, bool sort = true)
		{
			var header = (InventoryAllocDetEnqFilter)Filter.Select();

			var headerType = typeof(InventoryAllocDetEnqFilter);
			var headerCache = Caches[headerType];
			var allocationFields = headerCache.GetAttributesOfType<InventoryAllocationFieldAttribute>(header, null)
				.Where(p => p.IsAddition == isAddition)
				.Select(f => new {
					allocationField = f,
					uiField = headerCache.GetAttributesOfType<PXUIFieldAttribute>(header, f.FieldName).FirstOrDefault()
				}).Where(f => f.uiField?.Visible == true); // Feature is enable = visible is true.

			var rows = allocationFields.Select((p) => new InventoryQtyByPlanType()
			{
				PlanType = PXMessages.LocalizeNoPrefix(p.uiField.DisplayName),
				Qty = (decimal?)headerCache.GetValue(header, p.uiField.FieldName),
				Included = !string.IsNullOrEmpty(p.allocationField.InclQtyFieldName) ?
					(bool?)headerCache.GetValue(header, p.allocationField.InclQtyFieldName) : null,
				IsTotal = p.allocationField.IsTotal,
				SortOrder = p.allocationField.SortOrder
			}).ToList();

			if (!sort) return rows;

			IEnumerable<InventoryQtyByPlanType> resultset;

			var totalRow = rows.Where(r => r.IsTotal.GetValueOrDefault()).SingleOrDefault();
			if (totalRow != null)
			{
				rows.Remove(totalRow);
				var sorted = PXView.Sort(rows).RowCast<InventoryQtyByPlanType>().ToList();
				if (!PXView.ReverseOrder)
				{
					sorted.Add(totalRow);
				}
				else
				{
					sorted.Insert(0, totalRow);
				}
				resultset = sorted;
			}
			else
			{
				resultset = PXView.Sort(rows).RowCast<InventoryQtyByPlanType>();
			}

			PXDelegateResult delegateResult = new PXDelegateResult();
			delegateResult.AddRange(resultset);
			delegateResult.IsResultSorted = true;

			return delegateResult;
		}

		protected virtual IEnumerable addition()
		{
			return GetInventoryQtyByPlanType(true);

		}

		protected virtual IEnumerable deduction()
		{
			return GetInventoryQtyByPlanType(false);
		}


		public virtual bool AdjustPrevResult(Type allocationType, List<InventoryAllocDetEnqResult> resultList, INItemPlan itemPlan, Guid? previousNoteID)
		{
			decimal? planQty = itemPlan.Reverse == true
				? -itemPlan.PlanQty
				: itemPlan.PlanQty;

			InventoryAllocDetEnqResult[] previousItemPlansByRefNoteID = 
				resultList
				.Where(
					parent => parent.RefNoteID == previousNoteID
								&& parent.AllocationType == allocationType.Name
								&& parent.Reverse == itemPlan.Reverse
								&& parent.SubItemID == itemPlan.SubItemID
								&& parent.SiteID == itemPlan.SiteID
								&& (parent.LocationID == null || parent.LocationID == itemPlan.LocationID)
								&& (string.IsNullOrEmpty(parent.LotSerialNbr) || parent.LotSerialNbr == itemPlan.LotSerialNbr))
				.ToArray();

			foreach (InventoryAllocDetEnqResult previousItemPlan in previousItemPlansByRefNoteID)
			{
				if (previousItemPlan.PlanQty >= 0 && previousItemPlan.PlanQty > planQty || previousItemPlan.PlanQty < 0 && previousItemPlan.PlanQty < planQty)
			{
					previousItemPlan.PlanQty -= planQty;
					planQty = 0m;
				}
				else
				{
					planQty -= previousItemPlan.PlanQty;
					previousItemPlan.PlanQty = 0m;

					resultList.Remove(previousItemPlan);
				}

				if (planQty == 0m)
					break;
				}

			return previousItemPlansByRefNoteID.Any();
		}

		public virtual void ProcessItemPlanRecAs(Type planTypeInclQtyField, List<InventoryAllocDetEnqResult> resultList, ItemPlanWithExtraInfo itemPlanWithExtraInfo)
		{
			short qtyMult =
				this.Caches[typeof(INPlanType)]
					.GetValue(itemPlanWithExtraInfo.PlanType, planTypeInclQtyField.Name)
					as short? ?? 0;

			if (qtyMult == 0)
				return;

			INItemPlan itemPlan = itemPlanWithExtraInfo.ItemPlan;

			if (planTypeInclQtyField == typeof(INPlanType.inclQtyPOReceipts))
			{
				if ((itemPlan.OrigPlanType ?? INPlanConstants.Plan70) == INPlanConstants.Plan70)
					foreach (var poOrderLink in itemPlanWithExtraInfo.ReceiptItemPlanToPOOrderItemPlanLinks)
						if (AdjustPrevResult(typeof(INPlanType.inclQtyPOOrders), resultList, itemPlan, poOrderLink.OrderNoteID))
							break;
			}
			else if (planTypeInclQtyField == typeof(INPlanType.inclQtyPOFixedReceipts))
			{
				if ((itemPlan.OrigPlanType ?? INPlanConstants.Plan76) == INPlanConstants.Plan76)
					foreach (var poOrderLink in itemPlanWithExtraInfo.ReceiptItemPlanToPOOrderItemPlanLinks)
						if (AdjustPrevResult(typeof(INPlanType.inclQtyPOFixedOrders), resultList, itemPlan, poOrderLink.OrderNoteID))
							break;
			}
            else if (planTypeInclQtyField == typeof(INPlanType.inclQtyPOFixedFSSrvOrdReceipts))
            {
                if ((itemPlan.OrigPlanType ?? INPlanConstants.PlanF7) == INPlanConstants.PlanF7)
                    foreach (var poOrderLink in itemPlanWithExtraInfo.ReceiptItemPlanToPOOrderItemPlanLinks)
                        if (AdjustPrevResult(typeof(INPlanType.inclQtyPOFixedFSSrvOrd), resultList, itemPlan, poOrderLink.OrderNoteID))
                            break;
            }
            else if (planTypeInclQtyField == typeof(INPlanType.inclQtyPODropShipReceipts))
			{
				if ((itemPlan.OrigPlanType ?? INPlanConstants.Plan74) == INPlanConstants.Plan74)
					foreach (var poOrderLink in itemPlanWithExtraInfo.ReceiptItemPlanToPOOrderItemPlanLinks)
						if (AdjustPrevResult(typeof(INPlanType.inclQtyPODropShipOrders), resultList, itemPlan, poOrderLink.OrderNoteID))
							break;
			}
			else if (planTypeInclQtyField == typeof(INPlanType.inclQtySOShipping))
			{
				if (itemPlan.OrigPlanType == INPlanConstants.Plan60
					|| itemPlan.OrigPlanType == INPlanConstants.Plan61
					|| itemPlan.OrigPlanType == INPlanConstants.Plan63)
				{
					foreach (var soOrderLink in itemPlanWithExtraInfo.ShipmentItemPlanToSOOrderItemPlanLinks)
					{
						if (itemPlan.OrigPlanType == INPlanConstants.Plan60)
						{
							if (AdjustPrevResult(typeof(INPlanType.inclQtySOBooked), resultList, itemPlan, soOrderLink.OrderNoteID))
								break;
						}
						else if (itemPlan.OrigPlanType == INPlanConstants.Plan61 || itemPlan.OrigPlanType == INPlanConstants.Plan63)
						{
							if (AdjustPrevResult(planTypeInclQtyField, resultList, itemPlan, soOrderLink.OrderNoteID))
								break;
						}
					}
				}
			}

			// InTransit adjusting
			if (planTypeInclQtyField == typeof(INPlanType.inclQtyINReceipts)
				|| planTypeInclQtyField == typeof(INPlanType.inclQtyPOReceipts)
				|| planTypeInclQtyField == typeof(INPlanType.inclQtyPOFixedReceipts))
			{
				if (itemPlan.OrigPlanType == INPlanConstants.Plan42)
					AdjustPrevResult(typeof(INPlanType.inclQtyInTransit), resultList, itemPlan, itemPlan.OrigNoteID);
				if (itemPlan.OrigPlanType == INPlanConstants.Plan44)
					AdjustPrevResult(typeof(INPlanType.inclQtyInTransitToSO), resultList, itemPlan, itemPlan.OrigNoteID);
			}

			INLocation location = itemPlanWithExtraInfo.Location;
			INLotSerialStatus lotSerialStatus = itemPlanWithExtraInfo.LotSerialStatus;
			BAccount bAccount = itemPlanWithExtraInfo.BAccount;
			object refEntity = itemPlanWithExtraInfo.RefEntity;

			decimal planQty = qtyMult * itemPlan.PlanQty ?? 0m;
			var item = new InventoryAllocDetEnqResult
			{
				Module = INPlanConstants.ToModuleField(itemPlan.PlanType),
				AllocationType = planTypeInclQtyField.Name,
				PlanDate = itemPlan.PlanDate,
				QADocType = itemPlan.RefEntityType,
				RefNoteID = itemPlan.RefNoteID,
				RefNbr = refEntity != null ? _entityHelper.GetEntityRowID(refEntity.GetType(), refEntity, ", ") : null,
				SiteID = itemPlan.SiteID,
				LocationID = itemPlan.LocationID,
				Reverse = itemPlan.Reverse,
				PlanQty = itemPlan.Reverse == true ? -planQty : planQty,
				BAccountID = bAccount.BAccountID,
				AcctName = bAccount.AcctName,
				LocNotAvailable = location.InclQtyAvail == false,
				Expired = lotSerialStatus.ExpireDate != null && lotSerialStatus.ExpireDate < this.Accessinfo.BusinessDate,
				LotSerialNbr = itemPlan.LotSerialNbr,
				SubItemID = itemPlan.SubItemID,
				PlanID = itemPlan.PlanID,
				ExcludePlanLevel = itemPlan.ExcludePlanLevel,
			};

			if (refEntity is INRegister inregister)
			{
				item.QADocType += inregister.DocType;

				if (!_displayNameByEntityType.ContainsKey(item.QADocType))
				{
					string value = new INRegisterCacheNameAttribute(Messages.Register).GetName(new IN.INRegister() { DocType = inregister.DocType });
					_displayNameByEntityType.Add(item.QADocType, value);
				}
			}
			else
			{
				if (!_displayNameByEntityType.ContainsKey(item.QADocType))
				{
					string value = ((PXCacheNameAttribute)PXBuildManager.GetType(item.QADocType, false)
						?.GetCustomAttributes(typeof(PXCacheNameAttribute), false).FirstOrDefault())
						?.GetName() ?? item.QADocType;
					_displayNameByEntityType.Add(item.QADocType, value);
				}
			}

			resultList.Add(item);
		}

		protected virtual IEnumerable resultRecords()
		{
			return cachedResultRecords.Value;
		}

		protected virtual IEnumerable<InventoryAllocDetEnqResult> CalculateResultRecords()
		{
			_displayNameByEntityType.Clear();

			InventoryAllocDetEnqFilter filter = Filter.Current;

			// InventoryID is required
			if (filter.InventoryID == null)
				return Enumerable.Empty<InventoryAllocDetEnqResult>();

			PXSelectBase<INItemPlan> cmd = GetResultRecordsSelect(filter);
			PXResultset<INItemPlan> itemPlansWithExtraInfo = cmd.Select();

			var resultList = new List<InventoryAllocDetEnqResult>();
			foreach (ItemPlanWithExtraInfo ip in UnwrapAndGroup(itemPlansWithExtraInfo))
			{
				Type inclQtyField = INPlanConstants.ToInclQtyField(ip.ItemPlan.PlanType);
				if (inclQtyField != null && inclQtyField != typeof(INPlanType.inclQtyINReplaned))
					ProcessItemPlanRecAs(inclQtyField, resultList, ip);
			}

			// numerate grid lines (key column) to let ViewDocument button work
			int nextLineNbr = 1;
			DateTime minPlanDate = new DateTime(1900, 1, 1);
			foreach (InventoryAllocDetEnqResult it in resultList)
			{
				if (it.PlanDate == minPlanDate)
					it.PlanDate = null;
				it.GridLineNbr = nextLineNbr++;
			}

			PXStringListAttribute.SetList<InventoryAllocDetEnqResult.qADocType>(this.Caches<InventoryAllocDetEnqResult>(), null,
				_displayNameByEntityType.Keys.ToArray(), _displayNameByEntityType.Values.ToArray());

			return resultList;
		}

		public override bool IsDirty
		{
			get { return false; }
		}

		public virtual PXSelectBase<INItemPlan> GetResultRecordsSelect(InventoryAllocDetEnqFilter filter)
		{
			PXSelectBase<INItemPlan> cmd =
				new PXSelectJoin<INItemPlan,
				InnerJoin<INPlanType,
					On<INItemPlan.FK.PlanType>,
				LeftJoin<INLocation,
					On<INItemPlan.FK.Location>,
				LeftJoin<INLotSerialStatus,
					On<INItemPlan.FK.LotSerialStatus>,
				LeftJoin<BAccount,
					On<INItemPlan.FK.BAccount>,
				LeftJoin<INSubItem,
					On<INItemPlan.FK.SubItem>,
				InnerJoin<INSite,
					On<INItemPlan.FK.Site>,
				LeftJoin<SOOrderShipment,
					On<SOOrderShipment.shipmentNoteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<ARRegister,
					On<ARRegister.noteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<POOrderReceipt,
					On<POOrderReceipt.receiptNoteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<INRegister,
					On<INRegister.noteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<SOOrder,
					On<SOOrder.noteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<POOrder,
					On<POOrder.noteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<POReceipt,
					On<POReceipt.noteID, Equal<INItemPlan.refNoteID>>,
				LeftJoin<INTransitLine,
					On<INTransitLine.noteID, Equal<INItemPlan.refNoteID>>
				>>>>>>>>>>>>>>,
			Where<INItemPlan.planQty, NotEqual<decimal0>,
				And<INItemPlan.inventoryID, Equal<Current<InventoryAllocDetEnqFilter.inventoryID>>,
				And<Match<INSite, Current<AccessInfo.userName>>>>>,
			OrderBy<Asc<INSubItem.subItemCD, // sorting must be done with PlanType preceding location
					Asc<INSite.siteCD,
					Asc<INItemPlan.origPlanType,
					Asc<INItemPlan.planType,
					Asc<INLocation.locationCD>>>>>>>(this);

			if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD) && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryAllocDetEnqFilter.subItemCDWildcard>>>>();
			}
			if (filter.SiteID != null)
			{
				cmd.WhereAnd<Where<INItemPlan.siteID, Equal<Current<InventoryAllocDetEnqFilter.siteID>>>>();
			}
			if (filter.LocationID != null && filter.LocationID != -1) // there are cases when filter.LocationID = -1
			{
				cmd.WhereAnd<Where<INItemPlan.locationID, Equal<Current<InventoryAllocDetEnqFilter.locationID>>>>();
			}
			if (!string.IsNullOrEmpty(filter.LotSerialNbr))
			{
				cmd.WhereAnd<Where<INItemPlan.lotSerialNbr, Like<Current<InventoryAllocDetEnqFilter.lotSerialNbrWildcard>>>>();
			}

			return cmd;
		}

		public virtual ItemPlanWithExtraInfo[] UnwrapAndGroup(PXResultset<INItemPlan> records)
		{
			return records.ToArray()
					.Select(r => (PXResult<
								INItemPlan,
								INPlanType,
								INLocation,
								INLotSerialStatus,
								BAccount,
								INSubItem,
								INSite,
								SOOrderShipment,
								ARRegister,
								POOrderReceipt,
								INRegister,
								SOOrder,
								POOrder,
								POReceipt,
								INTransitLine>)r)
					.GroupBy(
						t => (INItemPlan)t,
						t => new
						{
							Record = t,
							SOLink = (SOOrderShipment)t,
							POLink = (POOrderReceipt)t
						})
					.Select(t => new ItemPlanWithExtraInfo(
								t.First().Record,
								t.Select(s => s.SOLink).Where(link => link?.ShipmentNbr != null).ToArray(),
								t.Select(s => s.POLink).Where(link => link?.ReceiptNbr != null).ToArray()))
					.ToArray();
		}

		protected virtual void Aggregate(InventoryAllocDetEnqFilter aDest, InventoryAllocDetEnqResult aSrc)
		{
			if (aSrc.ExcludePlanLevel != null)
			{
				if (aDest.LocationID != null && !string.IsNullOrEmpty(aDest.LotSerialNbr))
				{
					if ((aSrc.ExcludePlanLevel & INPlanLevel.ExcludeLocationLotSerial) == INPlanLevel.ExcludeLocationLotSerial)
						return;
				}
				else if (aDest.LocationID != null)
				{
					if ((aSrc.ExcludePlanLevel & INPlanLevel.ExcludeLocation) == INPlanLevel.ExcludeLocation)
						return;
				}
				else
				{
					if ((aSrc.ExcludePlanLevel & INPlanLevel.ExcludeSite) == INPlanLevel.ExcludeSite)
						return;
				}
			}

			bool isSORelated = aSrc.AllocationType.IsIn(
					nameof(INAvailabilityScheme.inclQtySOPrepared),
					nameof(INAvailabilityScheme.inclQtySOBooked),
					nameof(INAvailabilityScheme.inclQtySOShipping),
					nameof(INAvailabilityScheme.inclQtySOShipped),
					nameof(INAvailabilityScheme.inclQtySOBackOrdered));

			if (aDest.InclQtySOReverse != true && aSrc.Reverse == true && isSORelated) return;

			switch (aSrc.AllocationType)
			{
				case nameof(INAvailabilityScheme.inclQtyINReceipts):
					aDest.QtyINReceipts += (aSrc.LocNotAvailable == false) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtyInTransit):
					aDest.QtyInTransit += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtySOPrepared):
					aDest.QtySOPrepared += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtySOBooked):
					aDest.QtySOBooked += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtySOShipping):
					aDest.QtySOShipping += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					aDest.QtySOShippingReverse += (aSrc.LocNotAvailable == false && aSrc.Expired == false && aSrc.Reverse == true) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtySOShipped):
					aDest.QtySOShipped += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					aDest.QtySOShippedReverse += (aSrc.LocNotAvailable == false && aSrc.Expired == false && aSrc.Reverse == true) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtyINIssues):
					aDest.QtyINIssues += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					break;
				case nameof(INAvailabilityScheme.inclQtySOBackOrdered):
					aDest.QtySOBackOrdered += (aSrc.LocNotAvailable == false && aSrc.Expired == false) ? aSrc.PlanQty : 0m;
					break;									
				case nameof(INAvailabilityScheme.inclQtyPOReceipts):
					aDest.QtyPOReceipts += aSrc.PlanQty;
					break;
				default:
					INPlanType plantype = new INPlanType();
					PXCache cache = Caches[typeof(INPlanType)];
					cache.SetValue(plantype, aSrc.AllocationType, (short)1);

					INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase(this, aDest, aSrc, plantype, null);
					break;
			}
		}

		public PXAction<InventoryAllocDetEnqFilter> viewSummary;

		[PXButton]
		[PXUIField(DisplayName = Messages.InventorySummary)]
		protected virtual IEnumerable ViewSummary(PXAdapter a)
		{
			object subItemState = this.ResultRecords.Current.With(cur => ResultRecords.Cache.GetValueExt<InventorySummaryEnquiryResult.subItemID>(cur));
			string subItemCD = subItemState is PXSegmentedState
				? (string) ((PXSegmentedState) subItemState).Value
				: (string) subItemState;

			InventorySummaryEnq.Redirect(
				Filter.Current.InventoryID,
				subItemCD ?? Filter.Current.SubItemCD,
				ResultRecords.Current?.SiteID ?? Filter.Current.SiteID,
				ResultRecords.Current?.LocationID ?? Filter.Current.LocationID,
				false);
			return a.Get();
		}

		[PXUIField(DisplayName = "")]
		[PXEditDetailButton]
        protected virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            if (this.ResultRecords.Current != null)
            {
                InventoryAllocDetEnqResult res = this.ResultRecords.Current;
                var entityrow = _entityHelper.GetEntityRow(res.RefNoteID);

                var transitLine = entityrow as IN.INTransitLine;
                if (transitLine != null)
                {
                    _entityHelper.NavigateToRow((Guid)transitLine.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
                }
                else
                {
                    INReplenishmentOrder replenishmentOrder = entityrow as INReplenishmentOrder;
                    if (replenishmentOrder != null && res.PlanID != null)
                    {
                        SO.SOOrder order =
                            PXSelectJoin<SO.SOOrder,
                            InnerJoin<INReplenishmentLine,
                                On<SO.SOOrder.orderType, Equal<INReplenishmentLine.sOType>,
                                And<SO.SOOrder.orderNbr, Equal<INReplenishmentLine.sONbr>>>>,
                            Where<INReplenishmentLine.planID, Equal<Required<InventoryAllocDetEnqResult.planID>>>>
                            .Select(this, res.PlanID);
                        PXRedirectHelper.TryRedirect(this, order, PXRedirectHelper.WindowMode.NewWindow);
                    }
                    else
                        _entityHelper.NavigateToRow((Guid)res.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
                }

                throw new PXException(Messages.UnableNavigateDocument);
            }
            return adapter.Get();
        }

        public static void Redirect(int? inventoryID, string subItemCD, string lotSerNum, int? siteID, int? locationID)
		{
			InventoryAllocDetEnq graph = PXGraph.CreateInstance<InventoryAllocDetEnq>();
			graph.Filter.Current.InventoryID = inventoryID;
			graph.Filter.Current.SubItemCD = subItemCD;
			graph.Filter.Current.SiteID = siteID;
			graph.Filter.Current.LocationID = locationID;
			graph.Filter.Current.LotSerialNbr = lotSerNum;
			graph.Filter.Select();

			throw new PXRedirectRequiredException(graph, Messages.InventoryAllocDet);
		}
	}
}
