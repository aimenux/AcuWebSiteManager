using System;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
namespace PX.Objects.IN
{
    [Serializable]
    [PXPrimaryGraph(typeof(INTransferEntry))]
    [PXCacheName(Messages.InTransitLine)]
    public partial class INTransitLine : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<INTransitLine>.By<costSiteID>
        {
            public static INTransitLine Find(PXGraph graph, int? costSiteID) => FindBy(graph, costSiteID);
        }
		public static class FK
		{
			public class CostSite : INCostSite.PK.ForeignKeyOf<INTransitLine>.By<costSiteID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<INTransitLine>.By<toSiteID> { }
			public class Site : INSite.PK.ForeignKeyOf<INTransitLine>.By<siteID> { }
			public class ToLocation : INLocation.PK.ForeignKeyOf<INTransitLine>.By<toLocationID> { }
		}
		#endregion

		#region CostSiteID
        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
        protected Int32? _CostSiteID;
        [PXDBForeignIdentity(typeof(INCostSite))]
        public virtual Int32? CostSiteID
        {
            get
            {
                return this._CostSiteID;
            }
            set
            {
                this._CostSiteID = value;
            }
        }
        #endregion

        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
        protected String _TransferNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXFieldDescription]
        public virtual String TransferNbr
        {
            get
            {
                return this._TransferNbr;
            }
            set
            {
                this._TransferNbr = value;
            }
        }
        #endregion

        #region TransferLineNbr
        public abstract class transferLineNbr : PX.Data.BQL.BqlInt.Field<transferLineNbr> { }
        protected Int32? _TransferLineNbr;

		[PXDBInt(IsKey = true)]
        [PXFieldDescription]
        public virtual Int32? TransferLineNbr
        {
            get
            {
                return this._TransferLineNbr;
            }
            set
            {
                this._TransferLineNbr = value;
            }
        }
        #endregion

        #region SOOrderType
        public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
        protected String _SOOrderType;
        [PXDBString(2, IsFixed = true)]
        public virtual String SOOrderType
        {
            get
            {
                return this._SOOrderType;
            }
            set
            {
                this._SOOrderType = value;
            }
        }
        #endregion
        #region SOOrderNbr
        public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
        protected String _SOOrderNbr;
        [PXDBString(15, InputMask = "", IsUnicode = true)]
        public virtual String SOOrderNbr
        {
            get
            {
                return this._SOOrderNbr;
            }
            set
            {
                this._SOOrderNbr = value;
            }
        }
        #endregion

        #region SOOrderLineNbr
        public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
        protected Int32? _SOOrderLineNbr;

        [PXDBInt]
        public virtual Int32? SOOrderLineNbr
        {
            get
            {
                return this._SOOrderLineNbr;
            }
            set
            {
                this._SOOrderLineNbr = value;
            }
        }
        #endregion

        #region SOShipmentType
        public abstract class sOShipmentType : PX.Data.BQL.BqlString.Field<sOShipmentType> { }
        protected String _SOShipmentType;
        [PXDBString(1, IsFixed = true)]
        public virtual String SOShipmentType
        {
            get
            {
                return this._SOShipmentType;
            }
            set
            {
                this._SOShipmentType = value;
            }
        }
        #endregion
        #region SOShipmentNbr
        public abstract class sOShipmentNbr : PX.Data.BQL.BqlString.Field<sOShipmentNbr> { }
        protected String _SOShipmentNbr;
        [PXDBString(15, InputMask = "", IsUnicode = true)]
        public virtual String SOShipmentNbr
        {
            get
            {
                return this._SOShipmentNbr;
            }
            set
            {
                this._SOShipmentNbr = value;
            }
        }
        #endregion

        #region SOShipmentLineNbr
        public abstract class sOShipmentLineNbr : PX.Data.BQL.BqlInt.Field<sOShipmentLineNbr> { }
        protected Int32? _SOShipmentLineNbr;

        [PXDBInt]
        public virtual Int32? SOShipmentLineNbr
        {
            get
            {
                return this._SOShipmentLineNbr;
            }
            set
            {
                this._SOShipmentLineNbr = value;
            }
        }
        #endregion

        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [IN.Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr))]
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
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
        protected Int32? _ToSiteID;
        [IN.ToSite(DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr))]
        public virtual Int32? ToSiteID
        {
            get
            {
                return this._ToSiteID;
            }
            set
            {
                this._ToSiteID = value;
            }
        }
        #endregion

        #region OrigModule
        public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>
		{
            public const string PI = "PI";

            public class List : PXStringListAttribute
            {
				public List() : base(
					new[]
                {
						Pair(GL.BatchModule.SO, GL.Messages.ModuleSO),
						Pair(GL.BatchModule.PO, GL.Messages.ModulePO),
						Pair(GL.BatchModule.IN, GL.Messages.ModuleIN),
						Pair(PI, Messages.ModulePI),
						Pair(GL.BatchModule.AP, GL.Messages.ModuleAP),
					}) {}
            }
        }
        protected String _OrigModule;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(GL.BatchModule.IN)]
        [origModule.List]
        public virtual String OrigModule
        {
            get
            {
                return this._OrigModule;
            }
            set
            {
                this._OrigModule = value;
            }
        }
        #endregion

        #region ToLocationID
        public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
        protected Int32? _ToLocationID;
        [IN.Location(DisplayName = "To Location ID")]
        public virtual Int32? ToLocationID
        {
            get
            {
                return this._ToLocationID;
            }
            set
            {
                this._ToLocationID = value;
            }
        }
		#endregion
		#region IsLotSerial
		/// <summary>
		/// Denormalization of <see cref="INTranSplit.LotSerialNbr"/>
		/// </summary>
	    [PXDBBool]
	    [PXDefault(false)]
		public virtual Boolean? IsLotSerial { get; set; }
	    public abstract class isLotSerial : PX.Data.BQL.BqlBool.Field<isLotSerial> { }
	    #endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(DescriptionField = typeof(INTransitLine.transferNbr),
            Selector = typeof(INTransitLine.transferNbr))]
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
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        protected Guid? _RefNoteID;
        [PXDBGuid()]
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
		#region IsFixedInTransit
		/// <summary>
		/// Denormalization of <see cref="INTranSplit.IsFixedInTransit"/>
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? IsFixedInTransit { get; set; }
	    public abstract class isFixedInTransit : PX.Data.BQL.BqlBool.Field<isFixedInTransit> { }
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

    }
}
