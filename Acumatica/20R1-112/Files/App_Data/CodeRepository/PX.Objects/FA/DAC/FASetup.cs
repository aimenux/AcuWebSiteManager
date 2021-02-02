using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;


namespace PX.Objects.FA
{
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(SetupMaint))]
    [PXCacheName(Messages.FASetup)]
    public partial class FASetup : PX.Data.IBqlTable
	{
		#region RegisterNumberingID
		public abstract class registerNumberingID : PX.Data.BQL.BqlString.Field<registerNumberingID> { }
		protected String _RegisterNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("FAREGISTER")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Transaction Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String RegisterNumberingID
		{
			get
			{
				return this._RegisterNumberingID;
			}
			set
			{
				this._RegisterNumberingID = value;
			}
		}
		#endregion
		#region AssetNumberingID
		public abstract class assetNumberingID : PX.Data.BQL.BqlString.Field<assetNumberingID> { }
		protected String _AssetNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("FASSET")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Asset Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String AssetNumberingID
		{
			get
			{
				return this._AssetNumberingID;
			}
			set
			{
				this._AssetNumberingID = value;
			}
		}
		#endregion
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.BQL.BqlString.Field<batchNumberingID> { }
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXUIField(DisplayName = "Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region TagNumberingID
		public abstract class tagNumberingID : PX.Data.BQL.BqlString.Field<tagNumberingID> { }
		protected String _TagNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tag Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual String TagNumberingID
		{
			get
			{
				return _TagNumberingID;
			}
			set
			{
				_TagNumberingID = value;
			}
		}
		#endregion
		#region CopyTagFromAssetID
		public abstract class copyTagFromAssetID : PX.Data.BQL.BqlBool.Field<copyTagFromAssetID> { }
		protected Boolean? _CopyTagFromAssetID;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Tag Number from Asset ID")]
		public virtual Boolean? CopyTagFromAssetID
		{
			get
			{
				return _CopyTagFromAssetID;
			}
			set
			{
				_CopyTagFromAssetID = value;
			}
		}
		#endregion
		#region FAAccrualAcctID
		public abstract class fAAccrualAcctID : PX.Data.BQL.BqlInt.Field<fAAccrualAcctID> { }
		protected Int32? _FAAccrualAcctID;
		[PXDefault]
		[Account(DisplayName = "FA Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? FAAccrualAcctID
		{
			get
			{
				return this._FAAccrualAcctID;
			}
			set
			{
				this._FAAccrualAcctID = value;
			}
		}
		#endregion
		#region FAAccrualSubID
		public abstract class fAAccrualSubID : PX.Data.BQL.BqlInt.Field<fAAccrualSubID> { }
		protected Int32? _FAAccrualSubID;
		[PXDefault]
		[SubAccount(typeof(FASetup.fAAccrualAcctID), Visibility = PXUIVisibility.Visible, DisplayName = "FA Accrual Sub.", DescriptionField = typeof(Sub.description))]
		public virtual Int32? FAAccrualSubID
		{
			get
			{
				return this._FAAccrualSubID;
			}
			set
			{
				this._FAAccrualSubID = value;
			}
		}
		#endregion
		#region ProceedsAcctID
		public abstract class proceedsAcctID : PX.Data.BQL.BqlInt.Field<proceedsAcctID> { }
		protected Int32? _ProceedsAcctID;
		[Account(null,
			DisplayName = "Proceeds Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		public virtual Int32? ProceedsAcctID
		{
			get
			{
				return this._ProceedsAcctID;
			}
			set
			{
				this._ProceedsAcctID = value;
			}
		}
		#endregion
		#region ProceedsSubID
		public abstract class proceedsSubID : PX.Data.BQL.BqlInt.Field<proceedsSubID> { }
		protected Int32? _ProceedsSubID;
		[SubAccount(typeof(FASetup.proceedsAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Proceeds Subaccount")]
		public virtual Int32? ProceedsSubID
		{
			get
			{
				return this._ProceedsSubID;
			}
			set
			{
				this._ProceedsSubID = value;
			}
		}
		#endregion

		#region AutoPost
		public abstract class autoPost : PX.Data.BQL.BqlBool.Field<autoPost> { }
		protected Boolean? _AutoPost;
		[PXDBBool()]
		[PXUIField(DisplayName = "Automatically Post on Release")]
		[PXDefault(true)]
		public virtual Boolean? AutoPost
		{
			get
			{
				return this._AutoPost;
			}
			set
			{
				this._AutoPost = value;
			}
		}
		#endregion
		#region AutoReleaseAsset
		public abstract class autoReleaseAsset : PX.Data.BQL.BqlBool.Field<autoReleaseAsset> { }
		protected Boolean? _AutoReleaseAsset;
		[PXDBBool()]
		[PXUIField(DisplayName = "Automatically Release Acquisition Transactions")]
		[PXDefault(false)]
		public virtual Boolean? AutoReleaseAsset
		{
			get
			{
				return this._AutoReleaseAsset;
			}
			set
			{
				this._AutoReleaseAsset = value;
			}
		}
		#endregion
		#region AutoReleaseDepr
		public abstract class autoReleaseDepr : PX.Data.BQL.BqlBool.Field<autoReleaseDepr> { }
		protected Boolean? _AutoReleaseDepr;
		[PXDBBool()]
		[PXUIField(DisplayName = "Automatically Release Depreciation Transactions")]
		[PXDefault(false)]
		public virtual Boolean? AutoReleaseDepr
		{
			get
			{
				return this._AutoReleaseDepr;
			}
			set
			{
				this._AutoReleaseDepr = value;
			}
		}
		#endregion
		#region AutoReleaseDisp
		public abstract class autoReleaseDisp : PX.Data.BQL.BqlBool.Field<autoReleaseDisp> { }
		protected Boolean? _AutoReleaseDisp;
		[PXDBBool()]
		[PXUIField(DisplayName = "Automatically Release Disposal Transactions")]
		[PXDefault(false)]
		public virtual Boolean? AutoReleaseDisp
		{
			get
			{
				return this._AutoReleaseDisp;
			}
			set
			{
				this._AutoReleaseDisp = value;
			}
		}
		#endregion
		#region AutoReleaseTransfer
		public abstract class autoReleaseTransfer : PX.Data.BQL.BqlBool.Field<autoReleaseTransfer> { }
		protected bool? _AutoReleaseTransfer;
		[PXDBBool]
		[PXUIField(DisplayName = "Automatically Release Transfer Transactions")]
		[PXDefault(false)]
		public virtual bool? AutoReleaseTransfer
		{
			get
			{
				return _AutoReleaseTransfer;
			}
			set
			{
				_AutoReleaseTransfer = value;
			}
		}
		#endregion
        #region AutoReleaseReversal
        public abstract class autoReleaseReversal : PX.Data.BQL.BqlBool.Field<autoReleaseReversal> { }
        protected bool? _AutoReleaseReversal;
        [PXDBBool]
        [PXUIField(DisplayName = "Automatically Release Reversal Transactions")]
        [PXDefault(false)]
        public virtual bool? AutoReleaseReversal
        {
            get
            {
                return _AutoReleaseReversal;
            }
            set
            {
                _AutoReleaseReversal = value;
            }
        }
        #endregion
        #region AutoReleaseSplit
        public abstract class autoReleaseSplit : PX.Data.BQL.BqlBool.Field<autoReleaseSplit> { }
        protected bool? _AutoReleaseSplit;
        [PXDBBool]
        [PXUIField(DisplayName = "Automatically Release Split Transactions")]
        [PXDefault(false)]
        public virtual bool? AutoReleaseSplit
        {
            get
            {
                return _AutoReleaseSplit;
            }
            set
            {
                _AutoReleaseSplit = value;
            }
        }
        #endregion
        #region UpdateGL
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		[PXDBBool()]
		[PXUIField(DisplayName = "Update GL")]
		[PXDefault(false)]
		public virtual Boolean? UpdateGL
		{
			get
			{
				return this._UpdateGL;
			}
			set
			{
				this._UpdateGL = value;
			}
		}
		#endregion
		#region DeprHistoryView
		public abstract class deprHistoryView : PX.Data.BQL.BqlString.Field<deprHistoryView>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new [] { SideBySide, BookSheet },
					new [] { Messages.SideBySide, Messages.BookSheet }) {}
			}

			public const string SideBySide = "S";
			public const string BookSheet = "B";

			public class sideBySide : PX.Data.BQL.BqlString.Constant<sideBySide>
			{
				public sideBySide() : base(SideBySide) {}
			}
			public class bookSheet : PX.Data.BQL.BqlString.Constant<bookSheet>
			{
				public bookSheet() : base(BookSheet) {}
			}
		}
		protected String _DeprHistoryView;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Depreciation History View")]
		[deprHistoryView.List]
		[PXDefault(deprHistoryView.BookSheet, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DeprHistoryView
		{
			get
			{
				return _DeprHistoryView;
			}
			set
			{
				_DeprHistoryView = value;
			}
		}
		#endregion
		#region ShowSideBySide
		public abstract class showSideBySide : PX.Data.BQL.BqlBool.Field<showSideBySide> { }
		[PXBool]
		[PXUIField(Visibility = PXUIVisibility.Visible)]
		public virtual bool? ShowSideBySide
		{
			get
			{
				return DeprHistoryView == FASetup.deprHistoryView.SideBySide;
			}
			set
			{
			}
		}
		#endregion
		#region ShowBookSheet
		public abstract class showBookSheet : PX.Data.BQL.BqlBool.Field<showBookSheet> { }
		[PXBool]
		[PXUIField(Visibility = PXUIVisibility.Visible)]
		public virtual bool? ShowBookSheet
		{
			get
			{
				return DeprHistoryView == FASetup.deprHistoryView.BookSheet;
			}
			set
			{
			}
		}
		#endregion
        #region SummPost
        public abstract class summPost : PX.Data.BQL.BqlBool.Field<summPost> { }
        protected Boolean? _SummPost;
        [PXDBBool()]
        [PXUIField(DisplayName = "Post Summary on Updating GL")]
        [PXDefault(false)]
        public virtual Boolean? SummPost
        {
            get
            {
                return this._SummPost;
            }
            set
            {
                this._SummPost = value;
            }
        }
        #endregion
        #region SummPostDepreciation
        public abstract class summPostDepreciation : PX.Data.BQL.BqlBool.Field<summPostDepreciation> { }
        protected Boolean? _SummPostDepreciation;
        [PXDBBool()]
		[PXUIField(DisplayName = "Post Depreciation Summary on Updating GL")]
		[PXDefault]
		[PXFormula(typeof(FASetup.summPost))]
		[PXUIEnabled(typeof(Where<FASetup.summPost, NotEqual<True>>))]
		public virtual Boolean? SummPostDepreciation
		{
			get
			{
				return this._SummPostDepreciation;
			}
			set
			{
				this._SummPostDepreciation = value;
			}
		}
        #endregion
        #region DepreciateInDisposalPeriod
        public abstract class depreciateInDisposalPeriod : PX.Data.BQL.BqlBool.Field<depreciateInDisposalPeriod> { }
        protected bool? _DepreciateInDisposalPeriod;
        [PXDBBool]
        [PXUIField(DisplayName = "Depreciate in Disposal Period")]
        [PXDefault(true)]
        public virtual bool? DepreciateInDisposalPeriod
        {
            get
            {
                return _DepreciateInDisposalPeriod;
            }
            set
            {
                _DepreciateInDisposalPeriod = value;
            }
        }
        #endregion
        #region AccurateDepreciation
        public abstract class accurateDepreciation : PX.Data.BQL.BqlBool.Field<accurateDepreciation> { }
        protected bool? _AccurateDepreciation;
        [PXDBBool]
        [PXUIField(DisplayName = "Show Accurate Depreciation")]
        [PXDefault(false)]
        public virtual bool? AccurateDepreciation
        {
            get
            {
                return _AccurateDepreciation;
            }
            set
            {
                _AccurateDepreciation = value;
            }
        }
        #endregion
		#region ReconcileBeforeDisposal
		public abstract class reconcileBeforeDisposal : PX.Data.BQL.BqlBool.Field<reconcileBeforeDisposal> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Require Full Reconciliation before Disposal")]
		[PXDefault(true)]
		public virtual bool? ReconcileBeforeDisposal { get; set; }
		#endregion
		#region AllowEditPredefinedDeprMethod
		public abstract class allowEditPredefinedDeprMethod : PX.Data.BQL.BqlBool.Field<allowEditPredefinedDeprMethod> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Allow to Modify Predefined Depreciation Methods")]
		[PXDefault(false)]
		public virtual bool? AllowEditPredefinedDeprMethod { get; set; }
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
