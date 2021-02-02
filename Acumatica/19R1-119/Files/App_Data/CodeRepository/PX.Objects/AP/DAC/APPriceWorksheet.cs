namespace PX.Objects.AP
{
    using System;
    using PX.Data;
    using PX.Objects.IN;
    using PX.Objects.CM;
    using PX.Objects.TX;
    using PX.Objects.CS;

    [System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(APPriceWorksheetMaint))]
	[PXCacheName(Messages.APPriceWorksheet)]
	public partial class APPriceWorksheet : PX.Data.IBqlTable
    {
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
            #region Numbering
            public class NumberingAttribute : AutoNumberAttribute
            {
                public NumberingAttribute()
                    : base(typeof(APSetup.priceWSNumberingID), typeof(APPriceWorksheet.effectiveDate)) { ; }
            }
            #endregion
        }
        protected String _RefNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<APPriceWorksheet.refNbr>), new Type[] { typeof(APPriceWorksheet.refNbr), typeof(APPriceWorksheet.status), typeof(APPriceWorksheet.descr), typeof(APPriceWorksheet.effectiveDate) })]
        [refNbr.Numbering()]
        //[PXFieldDescription]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        protected String _Status;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(AR.SPWorksheetStatus.Hold)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [AR.SPWorksheetStatus.List()]
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
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool()]
        [PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true)]
        //[PXNoUpdate]
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
        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(150, IsUnicode = true)]
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
        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        protected DateTime? _EffectiveDate;
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDate()]
        [PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion
        #region ExpirationDate
        public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
        protected DateTime? _ExpirationDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? ExpirationDate
        {
            get
            {
                return this._ExpirationDate;
            }
            set
            {
                this._ExpirationDate = value;
            }
        }
        #endregion
        #region IsPromotional
        public abstract class isPromotional : PX.Data.BQL.BqlBool.Field<isPromotional> { }
        protected bool? _IsPromotional;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Promotional")]
        public virtual bool? IsPromotional
        {
            get
            {
                return _IsPromotional;
            }
            set
            {
                _IsPromotional = value;
            }
        }
        #endregion
        #region OverwriteOverlapping
        public abstract class overwriteOverlapping : PX.Data.BQL.BqlBool.Field<overwriteOverlapping> { }
        protected bool? _OverwriteOverlapping;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Overwrite Overlapping Prices")]
        public virtual bool? OverwriteOverlapping
        {
            get
            {
                return _OverwriteOverlapping;
            }
            set
            {
                _OverwriteOverlapping = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(new Type[0])]
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
}