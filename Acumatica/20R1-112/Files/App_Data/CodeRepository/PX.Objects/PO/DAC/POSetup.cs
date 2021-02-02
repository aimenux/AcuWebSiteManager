namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;
	using PX.Objects.EP;
	using PX.Objects.GL;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(POSetupMaint))]
	[PXCacheName(Messages.POSetupMaint)]
	public partial class POSetup : PX.Data.IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class PPVReasonCode : ReasonCode.PK.ForeignKeyOf<POSetup>.By<pPVReasonCodeID> { }
			public class RCReturnReasonCode : ReasonCode.PK.ForeignKeyOf<POSetup>.By<rCReturnReasonCodeID> { }
		}
		#endregion
		#region StandardPONumberingID
		public abstract class standardPONumberingID : PX.Data.BQL.BqlString.Field<standardPONumberingID> { }
		protected String _StandardPONumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("POORDER")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Blanket Order Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String StandardPONumberingID
		{
			get
			{
				return this._StandardPONumberingID;
			}
			set
			{
				this._StandardPONumberingID = value;
			}
		}
		#endregion
		#region RegularPONumberingID
		public abstract class regularPONumberingID : PX.Data.BQL.BqlString.Field<regularPONumberingID> { }
		protected String _RegularPONumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("POORDER")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Regular Order Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String RegularPONumberingID
		{
			get
			{
				return this._RegularPONumberingID;
			}
			set
			{
				this._RegularPONumberingID = value;
			}
		}
		#endregion
		#region ReceiptNumberingID
		public abstract class receiptNumberingID : PX.Data.BQL.BqlString.Field<receiptNumberingID> { }
		protected String _ReceiptNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("PORECEIPT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Receipt Numbering Sequence", Visibility = PXUIVisibility.Visible)]
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
		#region LandedCostDocNumberingID
		public abstract class landedCostDocNumberingID : PX.Data.BQL.BqlString.Field<landedCostDocNumberingID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("POLANDCOST")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Landed Cost Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String LandedCostDocNumberingID
		{
			get;
			set;
		}
		#endregion
		#region RequireReceiptControlTotal
		public abstract class requireReceiptControlTotal : PX.Data.BQL.BqlBool.Field<requireReceiptControlTotal> { }
		protected Boolean? _RequireReceiptControlTotal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "For Receipts")]
		public virtual Boolean? RequireReceiptControlTotal
		{
			get
			{
				return this._RequireReceiptControlTotal;
			}
			set
			{
				this._RequireReceiptControlTotal = value;
			}
		}
		#endregion
		#region RequireOrderControlTotal
		public abstract class requireOrderControlTotal : PX.Data.BQL.BqlBool.Field<requireOrderControlTotal> { }
		protected Boolean? _RequireOrderControlTotal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "For Normal and Standard Orders")]
		public virtual Boolean? RequireOrderControlTotal
		{
			get
			{
				return this._RequireOrderControlTotal;
			}
			set
			{
				this._RequireOrderControlTotal = value;
			}
		}
		#endregion
		#region RequireBlanketControlTotal
		public abstract class requireBlanketControlTotal : PX.Data.BQL.BqlBool.Field<requireBlanketControlTotal> { }
		protected Boolean? _RequireBlanketControlTotal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "For Blanket Orders")]
		public virtual Boolean? RequireBlanketControlTotal
		{
			get
			{
				return this._RequireBlanketControlTotal;
			}
			set
			{
				this._RequireBlanketControlTotal = value;
			}
		}
		#endregion
		#region RequireDropShipControlTotal
		public abstract class requireDropShipControlTotal : PX.Data.BQL.BqlBool.Field<requireDropShipControlTotal> { }
		protected Boolean? _RequireDropShipControlTotal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "For Drop-Ship Orders")]
		public virtual Boolean? RequireDropShipControlTotal
		{
			get
			{
				return this._RequireDropShipControlTotal;
			}
			set
			{
				this._RequireDropShipControlTotal = value;
			}
		}
		#endregion
		#region RequireLandedCostsControlTotal
		public abstract class requireLandedCostsControlTotal : PX.Data.BQL.BqlBool.Field<requireLandedCostsControlTotal> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "For Landed Costs")]
		public virtual Boolean? RequireLandedCostsControlTotal
		{
			get;
			set;
		}
		#endregion
		#region HoldReceipts
		public abstract class holdReceipts : PX.Data.BQL.BqlBool.Field<holdReceipts> { }
		protected Boolean? _HoldReceipts;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Receipts on Entry")]
		public virtual Boolean? HoldReceipts
		{
			get
			{
				return this._HoldReceipts;
			}
			set
			{
				this._HoldReceipts = value;
			}
		}
		#endregion
		#region HoldLandedCosts
		public abstract class holdLandedCosts : PX.Data.BQL.BqlBool.Field<holdLandedCosts> { }
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Landed Costs on Entry")]
		public virtual Boolean? HoldLandedCosts
		{
			get;
			set;
		}
		#endregion


		#region AddServicesFromNormalPOtoPR
		public abstract class addServicesFromNormalPOtoPR : PX.Data.BQL.BqlBool.Field<addServicesFromNormalPOtoPR> { }
		protected bool? _AddServicesFromNormalPOtoPR;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Service lines from Normal Purchase Orders via Purchase Receipts")]
		public virtual bool? AddServicesFromNormalPOtoPR
		{
			get
			{
				return this._AddServicesFromNormalPOtoPR;
			}
			set
			{
				this._AddServicesFromNormalPOtoPR = value;
			}
		}
		#endregion
		#region AddServicesFromDSPOtoPR
		public abstract class addServicesFromDSPOtoPR : PX.Data.BQL.BqlBool.Field<addServicesFromDSPOtoPR> { }
		protected bool? _AddServicesFromDSPOtoPR;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Service lines from Drop-Ship Purchase Orders via Purchase Receipts")]
		public virtual bool? AddServicesFromDSPOtoPR
		{
			get
			{
				return this._AddServicesFromDSPOtoPR;
			}
			set
			{
				this._AddServicesFromDSPOtoPR = value;
			}
		}
		#endregion
		
		#region OrderRequestApproval
		public abstract class orderRequestApproval : PX.Data.BQL.BqlBool.Field<orderRequestApproval> { }
		protected bool? _OrderRequestApproval;
		[EPRequireApproval]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Require Approval")]
		public virtual bool? OrderRequestApproval
		{
			get
			{
				return this._OrderRequestApproval;
			}
			set
			{
				this._OrderRequestApproval = value;
			}
		}
		#endregion
		#region DefaultReceiptAssignmentMapID
		public abstract class defaultReceiptAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultReceiptAssignmentMapID> { }
		protected int? _DefaultReceiptAssignmentMapID;
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypePurchaseOrderReceipt>>>))]
		[PXUIField(DisplayName = "Receipt Assignment Map")]
		public virtual int? DefaultReceiptAssignmentMapID
		{
			get
			{
				return this._DefaultReceiptAssignmentMapID;
			}
			set
			{
				this._DefaultReceiptAssignmentMapID = value;
			}
		}
		#endregion
		#region ReceiptRequestApproval
		public abstract class receiptRequestApproval : PX.Data.BQL.BqlBool.Field<receiptRequestApproval> { }
		protected bool? _ReceiptRequestApproval;
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Require Approval")]
		public virtual bool? ReceiptRequestApproval
		{
			get
			{
				return this._ReceiptRequestApproval;
			}
			set
			{
				this._ReceiptRequestApproval = value;
			}
		}
		#endregion
		#region AutoCreateInvoiceOnReceipt
		public abstract class autoCreateInvoiceOnReceipt : PX.Data.BQL.BqlBool.Field<autoCreateInvoiceOnReceipt> { }
		protected Boolean? _AutoCreateInvoiceOnReceipt;
		[PXDBBool()]
		[PXUIField(DisplayName = "Create Bill on Receipt Release", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? AutoCreateInvoiceOnReceipt
		{
			get
			{
				return this._AutoCreateInvoiceOnReceipt;
			}
			set
			{
				this._AutoCreateInvoiceOnReceipt = value;
			}
		}
		#endregion
		#region FreightExpenseAcctID
		public abstract class freightExpenseAcctID : PX.Data.BQL.BqlInt.Field<freightExpenseAcctID> { }
		protected Int32? _FreightExpenseAcctID;
		[Account(DisplayName = "Freight Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<POSetup.freightExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? FreightExpenseAcctID
		{
			get
			{
				return this._FreightExpenseAcctID;
			}
			set
			{
				this._FreightExpenseAcctID = value;
			}
		}
		#endregion
		#region FreightExpenseSubID
		public abstract class freightExpenseSubID : PX.Data.BQL.BqlInt.Field<freightExpenseSubID> { }
		protected Int32? _FreightExpenseSubID;
		[SubAccount(typeof(POSetup.freightExpenseAcctID), DisplayName = "Freight Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<POSetup.freightExpenseSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? FreightExpenseSubID
		{
			get
			{
				return this._FreightExpenseSubID;
			}
			set
			{
				this._FreightExpenseSubID = value;
			}
		}
		#endregion
		#region RCReturnReasonCodeID
		public abstract class rCReturnReasonCodeID : PX.Data.BQL.BqlString.Field<rCReturnReasonCodeID> { }
		protected String _RCReturnReasonCodeID;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, In3<ReasonCodeUsages.issue, ReasonCodeUsages.vendorReturn>>>),
			DescriptionField = typeof(ReasonCode.descr))]
		[PXUIField(DisplayName = "PO Return Reason Code")]
		[PXDefault()]
		[PXForeignReference(typeof(FK.RCReturnReasonCode))]
		public virtual String RCReturnReasonCodeID
		{
			get
			{
				return this._RCReturnReasonCodeID;
			}
			set
			{
				this._RCReturnReasonCodeID = value;
			}
		}
		#endregion
		#region TaxReasonCodeID
		public abstract class taxReasonCodeID : PX.Data.BQL.BqlString.Field<taxReasonCodeID> { }
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.adjustment>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXUIField(DisplayName = "Tax Reason Code", Required = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxReasonCodeID
		{
			get;
			set;
		}
		#endregion
		#region PPVAllocationMode
		public abstract class pPVAllocationMode : PX.Data.BQL.BqlString.Field<pPVAllocationMode> { }
		protected String _PPVAllocationMode;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PPVMode.PPVAccount)]
		[PXUIField(DisplayName = "Allocation Mode", Visibility = PXUIVisibility.Visible)]
		[PPVMode.List]
		public virtual String PPVAllocationMode
		{
			get
			{
				return _PPVAllocationMode;
			}
			set
			{
				this._PPVAllocationMode = value;
			}
		}
		#endregion
		#region PPVReasonCodeID
		public abstract class pPVReasonCodeID : PX.Data.BQL.BqlString.Field<pPVReasonCodeID> { }
		protected String _PPVReasonCodeID;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.adjustment>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXUIField(DisplayName = "Reason Code")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.PPVReasonCode))]
		public virtual String PPVReasonCodeID
		{
			get
			{
				return this._PPVReasonCodeID;
			}
			set
			{
				this._PPVReasonCodeID = value;
			}
		}
		#endregion
		#region AutoReleaseAP
		public abstract class autoReleaseAP : PX.Data.BQL.BqlBool.Field<autoReleaseAP> { }
		protected bool? _AutoReleaseAP;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release AP Documents Automatically")]
		public virtual bool? AutoReleaseAP
		{
			get
			{
				return this._AutoReleaseAP;
			}
			set
			{
				this._AutoReleaseAP = value;
			}
		}
		#endregion
		#region AutoReleaseIN
		public abstract class autoReleaseIN : PX.Data.BQL.BqlBool.Field<autoReleaseIN> { }
		protected bool? _AutoReleaseIN;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release IN Documents Automatically")]
		public virtual bool? AutoReleaseIN
		{
			get
			{
				return this._AutoReleaseIN;
			}
			set
			{
				this._AutoReleaseIN = value;
			}
		}
		#endregion
		#region AutoCreateLCAP
		public abstract class autoCreateLCAP : PX.Data.BQL.BqlBool.Field<autoCreateLCAP> { }
		protected bool? _AutoCreateLCAP;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create Bill on LC Release")]
		public virtual bool? AutoCreateLCAP
		{
			get
			{
				return this._AutoCreateLCAP;
			}
			set
			{
				this._AutoCreateLCAP = value;
			}
		}
		#endregion
		#region AutoReleaseLCIN
		public abstract class autoReleaseLCIN : PX.Data.BQL.BqlBool.Field<autoReleaseLCIN> { }
		protected bool? _AutoReleaseLCIN;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release LC IN Adjustments Automatically")]
		public virtual bool? AutoReleaseLCIN
		{
			get
			{
				return this._AutoReleaseLCIN;
			}
			set
			{
				this._AutoReleaseLCIN = value;
			}
		}
		#endregion
		#region CopyLineDescrSO
		public abstract class copyLineDescrSO : PX.Data.BQL.BqlBool.Field<copyLineDescrSO> { }
		protected bool? _CopyLineDescrSO;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Descriptions from Sales Orders")]
		public virtual bool? CopyLineDescrSO
		{
			get
			{
				return this._CopyLineDescrSO;
			}
			set
			{
				this._CopyLineDescrSO = value;
			}
		}
		#endregion
		#region CopyLineNoteSO
		public abstract class copyLineNoteSO : PX.Data.BQL.BqlBool.Field<copyLineNoteSO> { }
		protected bool? _CopyLineNoteSO;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes from Sales Orders")]
		public virtual bool? CopyLineNoteSO
		{
			get
			{
				return this._CopyLineNoteSO;
			}
			set
			{
				this._CopyLineNoteSO = value;
			}
		}
        #endregion
        #region CopyLineNotesFromServiceOrder
        public abstract class copyLineNotesFromServiceOrder : PX.Data.BQL.BqlBool.Field<copyLineNotesFromServiceOrder> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Line Notes From Service Order", FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? CopyLineNotesFromServiceOrder { get; set; }
        #endregion
        #region CopyLineAttachmentsFromServiceOrder
        public abstract class copyLineAttachmentsFromServiceOrder : PX.Data.BQL.BqlBool.Field<copyLineAttachmentsFromServiceOrder> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Line Attachments From Service Order", FieldClass = "SERVICEMANAGEMENT")]
        public virtual bool? CopyLineAttachmentsFromServiceOrder { get; set; }
        #endregion
        #region ShipDestType
        public abstract class shipDestType : PX.Data.BQL.BqlString.Field<shipDestType> { }
		protected string _ShipDestType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POShipDestType.Location)]
		[POShipDestType.List]
		[PXUIField(DisplayName = "Default Ship Dest. Type")]
		public virtual string ShipDestType
		{
			get
			{
				return this._ShipDestType;
			}
			set
			{
				this._ShipDestType = value;
			}
		}
		#endregion
		#region UpdateSubOnOwnerChange
		public abstract class updateSubOnOwnerChange : PX.Data.BQL.BqlBool.Field<updateSubOnOwnerChange> { }
		protected bool? _UpdateSubOnOwnerChange;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Update Sub. on Order Owner Change", FieldClass = SubAccountAttribute.DimensionName)]
		public virtual bool? UpdateSubOnOwnerChange
		{
			get
			{
				return this._UpdateSubOnOwnerChange;
			}
			set
			{
				this._UpdateSubOnOwnerChange = value;
			}
		}
		#endregion
		#region AutoAddLineReceiptBarcode
		public abstract class autoAddLineReceiptBarcode : PX.Data.BQL.BqlBool.Field<autoAddLineReceiptBarcode> { }
		protected bool? _AutoAddLineReceiptBarcode;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Add Receipt Line for Barcode")]
		public virtual bool? AutoAddLineReceiptBarcode
		{
			get
			{
				return this._AutoAddLineReceiptBarcode;
			}
			set
			{
				this._AutoAddLineReceiptBarcode = value;
			}
		}
		#endregion
		#region ReceiptByOneBarcodeReceiptBarcode
		public abstract class receiptByOneBarcodeReceiptBarcode : PX.Data.BQL.BqlBool.Field<receiptByOneBarcodeReceiptBarcode> { }
		protected bool? _ReceiptByOneBarcodeReceiptBarcode;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Add One Unit per Barcode")]
		public virtual bool? ReceiptByOneBarcodeReceiptBarcode
		{
			get
			{
				return this._ReceiptByOneBarcodeReceiptBarcode;
			}
			set
			{
				this._ReceiptByOneBarcodeReceiptBarcode = value;
			}
		}
		#endregion
		#region ReturnOrigCost
		public abstract class returnOrigCost : PX.Data.BQL.BqlBool.Field<returnOrigCost> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Return with Original Cost")]
		public virtual bool? ReturnOrigCost
		{
			get;
			set;
		}
		#endregion
		#region DefaultReceiptQty
		public abstract class defaultReceiptQty : PX.Data.BQL.BqlString.Field<defaultReceiptQty> { }
		protected string _DefaultReceiptQty;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DefaultReceiptQuantity.OpenQty)]
		[DefaultReceiptQuantity.List]
		[PXUIField(DisplayName = "Default Receipt Quantity")]
		public virtual string DefaultReceiptQty
		{
			get
			{
				return this._DefaultReceiptQty;
			}
			set
			{
				this._DefaultReceiptQty = value;
			}
		}
		#endregion
		#region CopyLineNotesToReceipt
		public abstract class copyLineNotesToReceipt : PX.Data.BQL.BqlBool.Field<copyLineNotesToReceipt> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes to Receipt")]
		public virtual bool? CopyLineNotesToReceipt
		{
			get;
			set;
		}
		#endregion
		#region CopyLineFilesToReceipt
		public abstract class copyLineFilesToReceipt : PX.Data.BQL.BqlBool.Field<copyLineFilesToReceipt> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Attachments to Receipt")]
		public virtual bool? CopyLineFilesToReceipt
		{
			get;
			set;
		}
		#endregion
		#region ChangeCuryRateOnReceipt
		public abstract class changeCuryRateOnReceipt : Data.BQL.BqlBool.Field<changeCuryRateOnReceipt>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Changing Currency Rate on Receipt", FieldClass = nameof(FeaturesSet.Multicurrency))]
		public virtual bool? ChangeCuryRateOnReceipt
		{
			get;
			set;
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
		public virtual bool GetRequireControlTotal(string aOrderType)
		{
			bool result = false;
			switch (aOrderType)
			{
				case POOrderType.Blanket:
				case POOrderType.StandardBlanket:
					result = (bool)this.RequireBlanketControlTotal; break;
				case POOrderType.RegularOrder:
					result = (bool)this.RequireOrderControlTotal; break;
				case POOrderType.DropShip:
					result = (bool)this.RequireDropShipControlTotal; break;
			}
			return result;
		}
	}
	public class POShipDestType
	{
		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
				{
					Pair(Location, Messages.ShipDestCompanyLocation),
					Pair(Site, Messages.ShipDestSite)
				}) {}
		}

		public const string Location = "L";
		public const string Site = "S";
	}

	public class DefaultReceiptQuantity
	{
		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
				{
					Pair(OpenQty, Messages.OpenQuantity),
					Pair(Zero, Messages.ZeroQuantity)
				}) {}
		}

		public const string OpenQty = "O";
		public const string Zero = "Z";
	}

	public class PPVMode
	{
		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
				{
					Pair(Inventory, Messages.PPVInventory),
					Pair(PPVAccount, Messages.PPVAccount)
				}) {}
		}

		public const string Inventory = "I";
		public const string PPVAccount = "A";
	}
}