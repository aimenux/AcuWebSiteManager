namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;
	using PX.Objects.GL;

	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(INSetupMaint))]
    [PXCacheName(Messages.INSetupMaint)]
    public partial class INSetup : PX.Data.IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class IssuesReasonCode : ReasonCode.PK.ForeignKeyOf<INSetup>.By<issuesReasonCode> { }
			public class ReceiptReasonCode : ReasonCode.PK.ForeignKeyOf<INSetup>.By<receiptReasonCode> { }
			public class AdjustmentReasonCode : ReasonCode.PK.ForeignKeyOf<INSetup>.By<adjustmentReasonCode> { }
			public class PIReasonCode : ReasonCode.PK.ForeignKeyOf<INSetup>.By<pIReasonCode> { }
		}
		#endregion
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.BQL.BqlString.Field<batchNumberingID> { }
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Batch Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String BatchNumberingID
		{
			get
			{
				return _BatchNumberingID;
			}
			set
			{
				_BatchNumberingID = value;
			}
		}
		#endregion
		#region IssueNumberingID
		public abstract class issueNumberingID : PX.Data.BQL.BqlString.Field<issueNumberingID> { }
		protected String _IssueNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("INISSUE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Issue Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String IssueNumberingID
		{
			get
			{
				return this._IssueNumberingID;
			}
			set
			{
				this._IssueNumberingID = value;
			}
		}
		#endregion
		#region ReceiptNumberingID
		public abstract class receiptNumberingID : PX.Data.BQL.BqlString.Field<receiptNumberingID> { }
		protected String _ReceiptNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("INRECEIPT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Receipt/Transfer Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String ReceiptNumberingID
		{
			get
			{
				return this._ReceiptNumberingID;
			}
			set
			{
				this._ReceiptNumberingID = value;
			}
		}
		#endregion		
		#region AdjustmentNumberingID
		public abstract class adjustmentNumberingID : PX.Data.BQL.BqlString.Field<adjustmentNumberingID> { }
		protected String _AdjustmentNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("INADJUST")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Adjustment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String AdjustmentNumberingID
		{
			get
			{
				return this._AdjustmentNumberingID;
			}
			set
			{
				this._AdjustmentNumberingID = value;
			}
		}
		#endregion
		#region ReplenishmentNumberingID
		public abstract class replenishmentNumberingID : PX.Data.BQL.BqlString.Field<replenishmentNumberingID> { }
		protected String _ReplenishmentNumberingID;
		[PXDBString(10, IsUnicode=true)]
		[PXDefault("INREPL")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Replenishment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String ReplenishmentNumberingID
		{
			get
			{
				return this._ReplenishmentNumberingID;
			}
			set
			{
				this._ReplenishmentNumberingID = value;
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
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		protected Boolean? _HoldEntry;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Documents on Entry")]
		public virtual Boolean? HoldEntry
		{
			get
			{
				return this._HoldEntry;
			}
			set
			{
				this._HoldEntry = value;
			}
		}
		#endregion
		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.BQL.BqlBool.Field<requireControlTotal> { }
		protected Boolean? _RequireControlTotal;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Validate Document Totals on Entry")]
		public virtual Boolean? RequireControlTotal
		{
			get
			{
				return this._RequireControlTotal;
			}
			set
			{
				this._RequireControlTotal = value;
			}
		}
		#endregion
		#region UseInventorySubItem
		public abstract class useInventorySubItem : PX.Data.BQL.BqlBool.Field<useInventorySubItem> { }
		protected Boolean? _UseInventorySubItem;
		[PXBool()]
		[PXUIField(Visible = false)]
		public virtual Boolean? UseInventorySubItem
		{
			get
			{
				return PXAccess.FeatureInstalled<FeaturesSet.subItem>();
			}			
		}
		#endregion
		#region AutoAddLineBarcode
		public abstract class autoAddLineBarcode : PX.Data.BQL.BqlBool.Field<autoAddLineBarcode> { }
		protected bool? _AutoAddLineBarcode;
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Automatically Add Receipt Line for Barcode")]
		public virtual bool? AutoAddLineBarcode
		{
			get
			{
				return this._AutoAddLineBarcode;
			}
			set
			{
				this._AutoAddLineBarcode = value;
			}
		}
		#endregion
		#region AddByOneBarcode
		public abstract class addByOneBarcode : PX.Data.BQL.BqlBool.Field<addByOneBarcode> { }
		protected bool? _AddByOneBarcode;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Add One Unit per Barcode")]
		public virtual bool? AddByOneBarcode
		{
			get
			{
				return this._AddByOneBarcode;
			}
			set
			{
				this._AddByOneBarcode = value;
			}
		}
		#endregion
		#region ARClearingAcctID
		public abstract class aRClearingAcctID : PX.Data.BQL.BqlInt.Field<aRClearingAcctID> { }
		protected Int32? _ARClearingAcctID;
		[Account(DisplayName = "AR Clearing Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSetup.aRClearingAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? ARClearingAcctID
		{
			get
			{
				return this._ARClearingAcctID;
			}
			set
			{
				this._ARClearingAcctID = value;
			}
		}
		#endregion
		#region ARClearingSubID
		public abstract class aRClearingSubID : PX.Data.BQL.BqlInt.Field<aRClearingSubID> { }
		protected Int32? _ARClearingSubID;
		[SubAccount(typeof(INSetup.aRClearingAcctID), DisplayName = "AR Clearing Sub.",
					 DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSetup.aRClearingSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ARClearingSubID
		{
			get
			{
				return this._ARClearingSubID;
			}
			set
			{
				this._ARClearingSubID = value;
			}
		}
		#endregion
		#region INTransitAcctID
		public abstract class iNTransitAcctID : PX.Data.BQL.BqlInt.Field<iNTransitAcctID> { }
		protected Int32? _INTransitAcctID;
		[PXDefault()]
		[Account(DisplayName = "In-Transit Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSetup.iNTransitAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? INTransitAcctID
		{
			get
			{
				return this._INTransitAcctID;
			}
			set
			{
				this._INTransitAcctID = value;
			}
		}
		#endregion
		#region INTransitSubID
		public abstract class iNTransitSubID : PX.Data.BQL.BqlInt.Field<iNTransitSubID> { }
		protected Int32? _INTransitSubID;
		[SubAccount(typeof(INSetup.iNTransitAcctID), DisplayName = "In-Transit Sub.",
					 DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		[PXForeignReference(typeof(Field<INSetup.iNTransitSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? INTransitSubID
		{
			get
			{
				return this._INTransitSubID;
			}
			set
			{
				this._INTransitSubID = value;
			}
		}
		#endregion
		#region INProgressAcctID
		public abstract class iNProgressAcctID : PX.Data.BQL.BqlInt.Field<iNProgressAcctID> { }
		protected Int32? _INProgressAcctID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.kitAssemblies>>))]
		[Account(DisplayName = "Work In-Progress Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSetup.iNProgressAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? INProgressAcctID
		{
			get
			{
				return this._INProgressAcctID;
			}
			set
			{
				this._INProgressAcctID = value;
			}
		}
		#endregion
		#region INProgressSubID
		public abstract class iNProgressSubID : PX.Data.BQL.BqlInt.Field<iNProgressSubID> { }
		protected Int32? _INProgressSubID;
		[SubAccount(typeof(INSetup.iNTransitAcctID), DisplayName = "Work In-Progress Sub.",
					 DescriptionField = typeof(Sub.description))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.kitAssemblies>>))]
		[PXForeignReference(typeof(Field<INSetup.iNProgressSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? INProgressSubID
		{
			get
			{
				return this._INProgressSubID;
			}
			set
			{
				this._INProgressSubID = value;
			}
		}
		#endregion
		#region IssuesReasonCode
		public abstract class issuesReasonCode : PX.Data.BQL.BqlString.Field<issuesReasonCode> { }
		protected String _IssuesReasonCode;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.issue>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Issue/Return Reason Code")]
		[PXForeignReference(typeof(FK.IssuesReasonCode))]
		public virtual String IssuesReasonCode
		{
			get
			{
				return this._IssuesReasonCode;
			}
			set
			{
				this._IssuesReasonCode = value;
			}
		}
		#endregion
		#region ReceiptReasonCode
		public abstract class receiptReasonCode : PX.Data.BQL.BqlString.Field<receiptReasonCode> { }
		protected String _ReceiptReasonCode;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.receipt>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Receipt Reason Code")]
		[PXForeignReference(typeof(FK.ReceiptReasonCode))]
		public virtual String ReceiptReasonCode
		{
			get
			{
				return this._ReceiptReasonCode;
			}
			set
			{
				this._ReceiptReasonCode = value;
			}
		}
		#endregion
		#region AdjustmentReasonCode
		public abstract class adjustmentReasonCode : PX.Data.BQL.BqlString.Field<adjustmentReasonCode> { }
		protected String _AdjustmentReasonCode;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.adjustment>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Adjustment Reason Code")]
		[PXForeignReference(typeof(FK.AdjustmentReasonCode))]
		public virtual String AdjustmentReasonCode
		{
			get
			{
				return this._AdjustmentReasonCode;
			}
			set
			{
				this._AdjustmentReasonCode = value;
			}
		}
		#endregion
		#region DfltStkItemClassID
		public abstract class dfltStkItemClassID : PX.Data.BQL.BqlInt.Field<dfltStkItemClassID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Default Stock Item Class", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), 
			new [] { typeof(INItemClass.itemClassCD), typeof(INItemClass.descr), typeof(INItemClass.stkItem) },
			DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		[PXRestrictor(typeof(Where<INItemClass.stkItem, Equal<boolTrue>>), Messages.EnteredItemClassIsNotStock)]
		public virtual int? DfltStkItemClassID { get; set; }
		#endregion
		#region DfltNonStkItemClassID
		public abstract class dfltNonStkItemClassID : PX.Data.BQL.BqlInt.Field<dfltNonStkItemClassID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Default Non-Stock Item Class", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD),
			new[] { typeof(INItemClass.itemClassCD), typeof(INItemClass.descr), typeof(INItemClass.stkItem) },
			DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		[PXRestrictor(typeof(Where<INItemClass.stkItem, Equal<boolFalse>>), Messages.EnteredItemClassIsNotNonStock)]
		public virtual int? DfltNonStkItemClassID { get; set; }
		#endregion
		#region DfltPostClassID
		public abstract class dfltPostClassID : PX.Data.BQL.BqlString.Field<dfltPostClassID> { }
		protected String _DfltPostClassID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String DfltPostClassID
		{
			get
			{
				return this._DfltPostClassID;
			}
			set
			{
				this._DfltPostClassID = value;
			}
		}
		#endregion
		#region DfltLotSerClassID
		public abstract class dfltLotSerClassID : PX.Data.BQL.BqlString.Field<dfltLotSerClassID> { }
		protected String _DfltLotSerClassID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String DfltLotSerClassID
		{
			get
			{
				return this._DfltLotSerClassID;
			}
			set
			{
				this._DfltLotSerClassID = value;
			}
		}
		#endregion
		#region UpdateGL
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		[PXDBBool()]
		[PXUIField(DisplayName="Update GL")]
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
		#region PerRetainTran
		public abstract class perRetainTran : PX.Data.BQL.BqlShort.Field<perRetainTran> { }
		protected Int16? _PerRetainTran;
		[PXDBShort()]
		[PXUIField(DisplayName = "Keep Transactions for")]
		[PXDefault((short)99)]
		public virtual Int16? PerRetainTran
		{
			get
			{
				return this._PerRetainTran;
			}
			set
			{
				this._PerRetainTran = value;
			}
		}
		#endregion
		#region PerRetainHist
		public abstract class perRetainHist : PX.Data.BQL.BqlShort.Field<perRetainHist> { }
		protected Int16? _PerRetainHist;
		[PXDBShort()]
		[PXUIField(DisplayName = "Periods to Retain History")]
		[PXDefault((short)0)]
		public virtual Int16? PerRetainHist
		{
			get
			{
				return this._PerRetainHist;
			}
			set
			{
				this._PerRetainHist = value;
			}
		}
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
		protected Boolean? _NegQty;
		[PXDBBool()]
		[PXUIField(DisplayName = "Allow Negative Quantity")]
		[PXDefault(false)]
		public virtual Boolean? NegQty
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
		#region PINumberingID
		public abstract class pINumberingID : PX.Data.BQL.BqlString.Field<pINumberingID> { }
		protected String _PINumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("PIID")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "PI Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String PINumberingID
		{
			get
			{
				return this._PINumberingID;
			}
			set
			{
				this._PINumberingID = value;
			}
		}
		#endregion
		#region PIUseTags
		public abstract class pIUseTags : PX.Data.BQL.BqlBool.Field<pIUseTags> { }
		protected Boolean? _PIUseTags;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Tags")]
		public virtual Boolean? PIUseTags
		{
			get
			{
				return this._PIUseTags;
			}
			set
			{
				this._PIUseTags = value;
			}
		}
		#endregion
		#region PILastTagNumber
		public abstract class pILastTagNumber : PX.Data.BQL.BqlInt.Field<pILastTagNumber> { }
		protected Int32? _PILastTagNumber;
		[PXDBInt(MinValue = 0)]
		[PXUIField(DisplayName = "Last Tag Number")]
		[PXDefault((int)0)]
		public virtual Int32? PILastTagNumber
		{
			get
			{
				return this._PILastTagNumber;
			}
			set
			{
				this._PILastTagNumber = value;
			}
		}
		#endregion
		#region PIReasonCode
		public abstract class pIReasonCode : PX.Data.BQL.BqlString.Field<pIReasonCode> { }
		protected String _PIReasonCode;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.adjustment>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Phys.Inventory Reason Code")]
		[PXForeignReference(typeof(FK.PIReasonCode))]
		public virtual String PIReasonCode
		{
			get
			{
				return this._PIReasonCode;
			}
			set
			{
				this._PIReasonCode = value;
			}
		}
		#endregion
		#region KitAssemblyNumberingID
		public abstract class kitAssemblyNumberingID : PX.Data.BQL.BqlString.Field<kitAssemblyNumberingID> { }
		protected String _KitAssemblyNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("INKITASSY")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Kit Assembly Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String KitAssemblyNumberingID
		{
			get
			{
				return this._KitAssemblyNumberingID;
			}
			set
			{
				this._KitAssemblyNumberingID = value;
			}
		}
		#endregion
		#region TurnoverPeriodsPerYear
		public abstract class turnoverPeriodsPerYear : PX.Data.BQL.BqlShort.Field<turnoverPeriodsPerYear> { }
		protected Int16? _TurnoverPeriodsPerYear;
        [PXDBShort(MinValue = 1, MaxValue = 12)]
        [PXDefault((short)12)]
		[PXUIField(DisplayName = "Turnover Periods per Year")]

		public virtual Int16? TurnoverPeriodsPerYear
        {
            get
            {
				return this._TurnoverPeriodsPerYear;
            }
            set
            {
				this._TurnoverPeriodsPerYear = value;
            }
        }
		#endregion
		#region ServiceItemNumberingID
		public abstract class serviceItemNumberingID : PX.Data.BQL.BqlString.Field<serviceItemNumberingID> { } 
		//TODO: need set PersistingCheck and set visible
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault("EQPNBR", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Equipment Numbering Sequence", Visible = false)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual String ServiceItemNumberingID { get; set; }
		#endregion
		#region ModelAttribute
		public abstract class modelAttribute : PX.Data.BQL.BqlString.Field<modelAttribute> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Model Attribute")]
		[PXSelector(typeof(CSAttribute.attributeID))]
		public virtual String ModelAttribute { get; set; }
		#endregion
		#region ManufactureAttribute
		public abstract class manufactureAttribute : PX.Data.BQL.BqlString.Field<manufactureAttribute> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Manufacture Attribute")]
		[PXSelector(typeof(CSAttribute.attributeID))]
		public virtual String ManufactureAttribute { get; set; }
		#endregion
        #region ReplanBackOrders
        public abstract class replanBackOrders : PX.Data.BQL.BqlBool.Field<replanBackOrders> { }
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Replan Back Orders")]
        public bool? ReplanBackOrders
        {
            get;
            set;
        }
        #endregion
        #region TransitSiteID
        public abstract class transitSiteID : PX.Data.BQL.BqlInt.Field<transitSiteID> { }
        protected Int32? _TransitSiteID;
        [PXDBInt]
        [PXDefault]
        [PXDimensionSelector("INSITE", typeof(INSite.siteID), typeof(INSite.siteCD), DirtyRead = true)]
        [PXUIField(DisplayName = "Site used for keep transit items", Required = true)]
        public virtual Int32? TransitSiteID
        {
            get
            {
                return this._TransitSiteID;
            }
            set
            {
                this._TransitSiteID = value;
            }
        }
        #endregion
		#region TransitBranchID
		public abstract class transitBranchID : PX.Data.BQL.BqlInt.Field<transitBranchID> { }
		protected Int32? _TransitBranchID;
		[Branch()]
		[PXDefault(typeof(AccessInfo.branchID))]
		[PXUIField(DisplayName = "In-Transit Branch", Required = true)]
		public virtual Int32? TransitBranchID
		{
			get
			{
				return this._TransitBranchID;
			}
			set
			{
				this._TransitBranchID = value;
			}
		}
		#endregion
		#region AutoReleasePIAdjustment
		public abstract class autoReleasePIAdjustment : Data.BQL.BqlBool.Field<autoReleasePIAdjustment>
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release PI Adjustment Automatically")]
		public bool? AutoReleasePIAdjustment
		{
			get;
			set;
		}
		#endregion
		#region DefaultSiteID
		public abstract class defaultSiteID : PX.Data.BQL.BqlInt.Field<defaultSiteID>
		{
		}
		[IN.Site(DisplayName = "Default Warehouse", DescriptionField = typeof(INSite.descr))]
		public virtual Int32? DefaultSiteID
		{
			get;
			set;
		}
		#endregion
	}
}
