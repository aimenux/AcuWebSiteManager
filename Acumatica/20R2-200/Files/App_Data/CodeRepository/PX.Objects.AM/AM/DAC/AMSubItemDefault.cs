using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{	
	[Serializable]
    [PXCacheName("Subitem Default")]
    public class AMSubItemDefault : IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected int? _SiteID;
        [Site(IsKey = true, Enabled = false)]
		[PXDefault]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual int? SiteID
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		protected int? _SubItemID;
        [SubItem(typeof(AMSubItemDefault.inventoryID), IsKey = true, Enabled = false)]
		[PXDefault]
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
        #region IsItemDefault
        public abstract class isItemDefault : PX.Data.BQL.BqlBool.Field<isItemDefault> { }

		protected bool? _IsItemDefault;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public virtual bool? IsItemDefault
		{
			get
			{
                return this._IsItemDefault;
			}
			set
			{
                this._IsItemDefault = value;
			}
		}
        #endregion
        #region BOMID
        /// <summary>
        /// Default BOM ID
        /// </summary>
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		protected string _BOMID;
        /// <summary>
        /// Default BOM ID
        /// </summary>
        [BomID(DisplayName = "Default BOM ID")]
        [BOMIDSelector(typeof(Search2<AMBomItemActive.bOMID,
            LeftJoin<InventoryItem, On<AMBomItemActive.inventoryID, Equal<InventoryItem.inventoryID>>>,
            Where<AMBomItemActive.inventoryID, Equal<Current<AMSubItemDefault.inventoryID>>,
                And<AMBomItemActive.siteID, Equal<Current<AMSubItemDefault.siteID>>,
                And<Where<AMBomItemActive.subItemID, Equal<Current<AMSubItemDefault.subItemID>>,
                    Or<AMBomItemActive.subItemID, IsNull>>>>>>))]
        public virtual string BOMID
		{
			get
			{
				return this._BOMID;
			}
			set
			{
				this._BOMID = value;
			}
		}
        #endregion
        #region PlanningBOMID
        /// <summary>
        /// Planning BOM ID
        /// </summary>
        public abstract class planningBOMID : PX.Data.BQL.BqlString.Field<planningBOMID> { }

        protected string _PlanningBOMID;
        /// <summary>
        /// Planning BOM ID
        /// </summary>
        [BomID(DisplayName = "Planning BOM ID")]
        [BOMIDSelector(typeof(Search2<AMBomItemActive.bOMID,
            LeftJoin<InventoryItem, On<AMBomItemActive.inventoryID, Equal<InventoryItem.inventoryID>>>,
            Where<AMBomItemActive.inventoryID, Equal<Current<AMSubItemDefault.inventoryID>>,
                And<AMBomItemActive.siteID, Equal<Current<AMSubItemDefault.siteID>>,
                    And<Where<AMBomItemActive.subItemID, Equal<Current<AMSubItemDefault.subItemID>>,
                        Or<AMBomItemActive.subItemID, IsNull>>>>>>))]
        public virtual string PlanningBOMID
        {
            get
            {
                return this._PlanningBOMID;
            }
            set
            {
                this._PlanningBOMID = value;
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp()]
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
	}
}