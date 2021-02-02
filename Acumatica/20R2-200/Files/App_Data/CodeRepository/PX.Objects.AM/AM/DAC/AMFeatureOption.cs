﻿using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AM.GraphExtensions;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Feature Option")]
	public class AMFeatureOption : IBqlTable, IConfigOption
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMFeatureOption>.By<featureID, lineNbr>
        {
            public static AMFeatureOption Find(PXGraph graph, string featureID, int lineNbr)
                => FindBy(graph, featureID, lineNbr);
        }

        public static class FK
        {
            public class Feature : AMFeature.PK.ForeignKeyOf<AMFeatureOption>.By<featureID> { }
        }
        #endregion

        #region FeatureID
        public abstract class featureID : PX.Data.BQL.BqlString.Field<featureID> { }

		protected string _FeatureID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Feature ID", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMFeature.featureID))]
        [PXParent(typeof(Select<AMFeature, Where<AMFeature.featureID, Equal<Current<AMFeatureOption.featureID>>>>))]
        public virtual string FeatureID
		{
			get
			{
				return this._FeatureID;
			}
			set
			{
				this._FeatureID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		protected int? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMFeature.lineCntrOption))]
        public virtual int? LineNbr
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
		#region Label
		public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

		protected string _Label;
		[PXDBString(30, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Label")]
        [PXCheckUnique(typeof(AMFeatureOption.featureID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected int? _InventoryID;
        [Inventory]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual int? InventoryID
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		protected int? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMFeatureOption.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMFeatureOption.inventoryID))]
        public virtual int? SubItemID   
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
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
		#region FixedInclude
		public abstract class fixedInclude : PX.Data.BQL.BqlBool.Field<fixedInclude> { }

		protected bool? _FixedInclude;
		[PXDBBool]
		[PXDefault(false)]
        [ConfigOptionFixedInclude(typeof(AMFeatureOption.qtyRequired))]
		[PXUIField(DisplayName = "Fixed Include")]
		public virtual bool? FixedInclude
		{
			get
			{
				return this._FixedInclude;
			}
			set
			{
				this._FixedInclude = value;
			}
		}
		#endregion
		#region QtyEnabled
		public abstract class qtyEnabled : PX.Data.BQL.BqlBool.Field<qtyEnabled> { }

		protected bool? _QtyEnabled;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Qty Enabled")]
		public virtual bool? QtyEnabled
		{
			get
			{
				return this._QtyEnabled;
			}
			set
			{
				this._QtyEnabled = value;
			}
		}
		#endregion
		#region QtyRequired
		public abstract class qtyRequired : PX.Data.BQL.BqlString.Field<qtyRequired> { }

		protected string _QtyRequired;
		[FormulaString]
		[PXUIField(DisplayName = "Qty Required")]
		public virtual string QtyRequired
		{
			get
			{
				return this._QtyRequired;
			}
			set
			{
				this._QtyRequired = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		protected string _UOM;
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
		public virtual string UOM
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
		#region MinQty
		public abstract class minQty : PX.Data.BQL.BqlString.Field<minQty> { }

		protected string _MinQty;
        [FormulaString]
        [PXUIField(DisplayName = "Min Qty")]
		public virtual string MinQty
		{
			get
			{
				return this._MinQty;
			}
			set
			{
				this._MinQty = value;
			}
		}
		#endregion
		#region MaxQty
		public abstract class maxQty : PX.Data.BQL.BqlString.Field<maxQty> { }

		protected string _MaxQty;
        [FormulaString]
        [PXUIField(DisplayName = "Max Qty")]
		public virtual string MaxQty
		{
			get
			{
				return this._MaxQty;
			}
			set
			{
				this._MaxQty = value;
			}
		}
		#endregion
		#region LotQty
		public abstract class lotQty : PX.Data.BQL.BqlString.Field<lotQty> { }

		protected string _LotQty;
        [FormulaString]
        [PXUIField(DisplayName = "Lot Qty")]
		public virtual string LotQty
		{
			get
			{
				return this._LotQty;
			}
			set
			{
				this._LotQty = value;
			}
		}
		#endregion
		#region ScrapFactor
		public abstract class scrapFactor : PX.Data.BQL.BqlDecimal.Field<scrapFactor> { }

		protected string _ScrapFactor;
        [FormulaString]
        [PXUIField(DisplayName = "Scrap Factor")]
		public virtual string ScrapFactor
		{
			get
			{
				return this._ScrapFactor;
			}
			set
			{
				this._ScrapFactor = value;
			}
		}
        #endregion
        #region MaterialType
        public abstract class materialType : PX.Data.BQL.BqlInt.Field<materialType> { }

        protected int? _MaterialType;
        [PXDBInt]
        [PXDefault(AMMaterialType.Regular)]
        [PXUIField(DisplayName = "Material Type", Visible = false)]
        [AMMaterialType.ConfigList]
        public virtual int? MaterialType
        {
            get
            {
                return this._MaterialType;
            }
            set
            {
                this._MaterialType = value;
            }
        }
        #endregion
        #region PhantomRouting
        public abstract class phantomRouting : PX.Data.BQL.BqlInt.Field<phantomRouting> { }

		protected int? _PhantomRouting;
		[PXDBInt]
		[PXDefault(PhantomRoutingOptions.Exclude)]
		[PXUIField(DisplayName = "Phantom Routing", Visible = false)]
        [PhantomRoutingOptions.List]
		public virtual int? PhantomRouting
		{
			get
			{
				return this._PhantomRouting;
			}
			set
			{
				this._PhantomRouting = value;
			}
		}
		#endregion
		#region PriceFactor
		public abstract class priceFactor : PX.Data.BQL.BqlString.Field<priceFactor> { }

		protected string _PriceFactor;
        [FormulaString]
        [PXDefault("1", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Price Factor")]
		public virtual string PriceFactor
		{
			get
			{
				return this._PriceFactor;
			}
			set
			{
				this._PriceFactor = value;
			}
		}
		#endregion
		#region ResultsCopy
		public abstract class resultsCopy : PX.Data.BQL.BqlBool.Field<resultsCopy> { }

		protected bool? _ResultsCopy;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Results Copy")]
		public virtual bool? ResultsCopy
		{
			get
			{
				return this._ResultsCopy;
			}
			set
			{
				this._ResultsCopy = value;
			}
		}
        #endregion
        #region BFlush
        public abstract class bFlush : PX.Data.BQL.BqlBool.Field<bFlush> { }

        protected Boolean? _BFlush;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Backflush")]
        public virtual Boolean? BFlush
        {
            get
            {
                return this._BFlush;
            }
            set
            {
                this._BFlush = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
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
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
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
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
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
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp]
		public virtual byte[] tstamp
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
	    #region QtyRoundUp
	    public abstract class qtyRoundUp : PX.Data.BQL.BqlBool.Field<qtyRoundUp> { }

	    [PXDBBool]
	    [PXDefault(false, typeof(Search<InventoryItemExt.aMQtyRoundUp, Where<InventoryItem.inventoryID,
	        Equal<Current<AMFeatureOption.inventoryID>>>>))]
	    [PXUIField(DisplayName = "Qty Round Up", Visible = false)]
	    public bool? QtyRoundUp { get; set; }
	    #endregion
	    #region BatchSize
	    public abstract class batchSize : PX.Data.BQL.BqlDecimal.Field<batchSize> { }

	    protected decimal? _BatchSize;
	    [BatchSize]
	    [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual decimal? BatchSize
	    {
	        get
	        {
	            return this._BatchSize;
	        }
	        set
	        {
	            this._BatchSize = value;
	        }
	    }
        #endregion
        #region SubcontractSource
        public abstract class subcontractSource : PX.Data.BQL.BqlInt.Field<subcontractSource> { }

        protected int? _SubcontractSource;
        [PXDBInt]
        [PXDefault(AMSubcontractSource.None)]
        [PXUIField(DisplayName = "Subcontract Source")]
        [AMSubcontractSource.List]
        public virtual int? SubcontractSource
        {
            get
            {
                return this._SubcontractSource;
            }
            set
            {
                this._SubcontractSource = value;
            }
        }
        #endregion
        #region PrintResults
        /// <summary>
        /// Flag used for reporting
        /// </summary>
        public abstract class printResults : PX.Data.BQL.BqlBool.Field<printResults> { }

        protected bool? _PrintResults;
        /// <summary>
        /// Flag used for reporting
        /// </summary>
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Print Results")]
        public virtual bool? PrintResults
        {
            get
            {
                return this._PrintResults;
            }
            set
            {
                this._PrintResults = value;
            }
        }
        #endregion
    }
}