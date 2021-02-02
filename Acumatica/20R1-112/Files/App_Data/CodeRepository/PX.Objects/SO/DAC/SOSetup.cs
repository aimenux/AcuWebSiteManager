namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;
	using PX.Objects.EP;
	using PX.Objects.GL;

	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(SOSetupMaint))]
    [PXCacheName(Messages.SOSetup)]
    public partial class SOSetup : PX.Data.IBqlTable
	{
		#region ShipmentNumberingID
		public abstract class shipmentNumberingID : PX.Data.BQL.BqlString.Field<shipmentNumberingID> { }
		protected String _ShipmentNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("SOSHIPMENT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Shipment Numbering Sequence")]
		public virtual String ShipmentNumberingID
		{
			get
			{
				return this._ShipmentNumberingID;
			}
			set
			{
				this._ShipmentNumberingID = value;
			}
		}
		#endregion
		#region PickingWorksheetNumberingID
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("PICKWORKSH", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Picking Worksheet Numbering Sequence")]
		public virtual String PickingWorksheetNumberingID { get; set; }
		public abstract class pickingWorksheetNumberingID : PX.Data.BQL.BqlString.Field<pickingWorksheetNumberingID> { }
		#endregion
		#region HoldShipments
		public abstract class holdShipments : PX.Data.BQL.BqlBool.Field<holdShipments> { }
		protected Boolean? _HoldShipments;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Shipments on Entry")]
		public virtual Boolean? HoldShipments
		{
			get
			{
				return this._HoldShipments;
			}
			set
			{
				this._HoldShipments = value;
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
		#region RequireShipmentTotal
		public abstract class requireShipmentTotal : PX.Data.BQL.BqlBool.Field<requireShipmentTotal> { }
		protected Boolean? _RequireShipmentTotal;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Validate Shipment Total on Confirmation")]
		public virtual Boolean? RequireShipmentTotal
		{
			get
			{
				return this._RequireShipmentTotal;
			}
			set
			{
				this._RequireShipmentTotal = value;
			}
		}
		#endregion
		#region AddAllToShipment
		public abstract class addAllToShipment : PX.Data.BQL.BqlBool.Field<addAllToShipment> { }
		protected Boolean? _AddAllToShipment;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Add Zero Lines for Items Not in Stock")]
		public virtual Boolean? AddAllToShipment
		{
			get
			{
				return this._AddAllToShipment;
			}
			set
			{
				this._AddAllToShipment = value;
			}
		}
		#endregion
		#region CreateZeroShipments
		public abstract class createZeroShipments : PX.Data.BQL.BqlBool.Field<createZeroShipments> { }
		protected Boolean? _CreateZeroShipments;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create Zero Shipments")]
		[PXFormula(typeof(Switch<Case<Where<SOSetup.addAllToShipment, Equal<False>>, False>, SOSetup.createZeroShipments>))]
		public virtual Boolean? CreateZeroShipments
		{
			get
			{
				return this._CreateZeroShipments;
			}
			set
			{
				this._CreateZeroShipments = value;
			}
		}
		#endregion
		#region AutoReleaseIN
		public abstract class autoReleaseIN : PX.Data.BQL.BqlBool.Field<autoReleaseIN> { }
		protected bool? _AutoReleaseIN;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release IN Documents")]
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
		#region DefaultOrderAssignmentMapID
		public abstract class defaultOrderAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultOrderAssignmentMapID> { }
		protected int? _DefaultOrderAssignmentMapID;
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeSalesOrder>>>))]
		[PXUIField(DisplayName = "Default Sales Order Assignment Map")]
		public virtual int? DefaultOrderAssignmentMapID
		{
			get
			{
				return this._DefaultOrderAssignmentMapID;
			}
			set
			{
				this._DefaultOrderAssignmentMapID = value;
			}
		}
		#endregion
		#region DefaultShipmentAssignmentMapID
		public abstract class defaultShipmentAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultShipmentAssignmentMapID> { }
		protected int? _DefaultShipmentAssignmentMapID;
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeSalesOrderShipment>>>))]
		[PXUIField(DisplayName = "Default Sales Order Shipment Assignment Map")]
		public virtual int? DefaultShipmentAssignmentMapID
		{
			get
			{
				return this._DefaultShipmentAssignmentMapID;
			}
			set
			{
				this._DefaultShipmentAssignmentMapID = value;
			}
		}
		#endregion

		#region ProrateDiscounts
		public abstract class prorateDiscounts : PX.Data.BQL.BqlBool.Field<prorateDiscounts> { }
		protected Boolean? _ProrateDiscounts;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Prorate Discounts")]
		public virtual Boolean? ProrateDiscounts
		{
			get
			{
				return this._ProrateDiscounts;
			}
			set
			{
				this._ProrateDiscounts = value;
			}
		}
		#endregion
		#region FreeItemShipping
		public abstract class freeItemShipping : PX.Data.BQL.BqlString.Field<freeItemShipping> { }
		protected String _FreeItemShipping;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FreeItemShipType.Proportional)]
		[FreeItemShipType.List()]
		[PXUIField(DisplayName = "Free Item Shipping", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual String FreeItemShipping
		{
			get
			{
				return this._FreeItemShipping;
			}
			set
			{
				this._FreeItemShipping = value;
			}
		}
		#endregion
		#region FreightOption
		public abstract class freightAllocation : PX.Data.BQL.BqlString.Field<freightAllocation> { }
		protected String _FreightAllocation;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FreightAllocationList.FullAmount)]
		[FreightAllocationList.List()]
		[PXUIField(DisplayName = "Freight Allocation on Partial Shipping", Visibility = PXUIVisibility.Visible)]
		public virtual String FreightAllocation
		{
			get
			{
				return this._FreightAllocation;
			}
			set
			{
				this._FreightAllocation = value;
			}
		}
		#endregion
		#region MinGrossProfitValidation
		public abstract class minGrossProfitValidation : PX.Data.BQL.BqlString.Field<minGrossProfitValidation> { }
		protected String _MinGrossProfitValidation;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(MinGrossProfitValidationType.Warning)]
		[MinGrossProfitValidationType.List()]
        [PXUIField(DisplayName = "Validate Min. Markup", Visibility = PXUIVisibility.Visible)]
		public virtual String MinGrossProfitValidation
		{
			get
			{
				return this._MinGrossProfitValidation;
			}
			set
			{
				this._MinGrossProfitValidation = value;
			}
		}
		#endregion
		#region UsePriceAdjustmentMultiplier
		public abstract class usePriceAdjustmentMultiplier : PX.Data.BQL.BqlBool.Field<usePriceAdjustmentMultiplier> { }
		protected Boolean? _UsePriceAdjustmentMultiplier;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use a Price Adjustment Multiplier")]
		public virtual Boolean? UsePriceAdjustmentMultiplier
		{
			get
			{
				return this._UsePriceAdjustmentMultiplier;
			}
			set
			{
				this._UsePriceAdjustmentMultiplier = value;
			}
		}
		#endregion
		#region IgnoreMinGrossProfitCustomerPrice
		public abstract class ignoreMinGrossProfitCustomerPrice : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Customer")]
		public virtual bool? IgnoreMinGrossProfitCustomerPrice
		{
			get;
			set;
		}
		#endregion
		#region IgnoreMinGrossProfitCustomerPriceClass
		public abstract class ignoreMinGrossProfitCustomerPriceClass : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Customer Price Class")]
		public virtual bool? IgnoreMinGrossProfitCustomerPriceClass
		{
			get;
			set;
		}
		#endregion
		#region IgnoreMinGrossProfitPromotionalPrice
		public abstract class ignoreMinGrossProfitPromotionalPrice : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Promotional Prices")]
		public virtual bool? IgnoreMinGrossProfitPromotionalPrice
		{
			get;
			set;
		}
		#endregion
		#region DefaultOrderType
		public abstract class defaultOrderType : PX.Data.BQL.BqlString.Field<defaultOrderType> { }
		protected String _DefaultOrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Sales Order Type")]
		[PXDefault(SOOrderTypeConstants.SalesOrder, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SOOrderType.orderType>),DescriptionField = typeof(SOOrderType.descr))]
        [PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>),Messages.OrderTypeInactive, typeof(SOOrderType.orderType))]
		public virtual String DefaultOrderType
		{
			get
			{
				return this._DefaultOrderType;
			}
			set
			{
				this._DefaultOrderType = value;
			}
		}
		#endregion
		#region TransferOrderType
		public abstract class transferOrderType : PX.Data.BQL.BqlString.Field<transferOrderType> { }
		protected String _TransferOrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Transfer Order Type")]
		[PXDefault(SOOrderTypeConstants.TransferOrder, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<SOOrderType.orderType, 
			InnerJoin<SOOrderTypeOperation, 
						 On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
						 And<SOOrderTypeOperation.iNDocType, Equal<IN.INTranType.transfer>>>>>), DescriptionField = typeof(SOOrderType.descr))]
		public virtual String TransferOrderType
		{
			get
			{
				return this._TransferOrderType;
			}
			set
			{
				this._TransferOrderType = value;
			}
		}
		#endregion
		#region CreditCheckError
		public abstract class creditCheckError : PX.Data.BQL.BqlBool.Field<creditCheckError> { }
		protected Boolean? _CreditCheckError;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Invoices on Failed Credit Check", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CreditCheckError
		{
			get
			{
				return this._CreditCheckError;
			}
			set
			{
				this._CreditCheckError = value;
			}
		}
		#endregion
		#region UseShipDateForInvoiceDate
		public abstract class useShipDateForInvoiceDate : PX.Data.BQL.BqlBool.Field<useShipDateForInvoiceDate> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Shipment Date for Invoice Date")]
		public virtual bool? UseShipDateForInvoiceDate
		{
			get;
			set;
		}
		#endregion
		#region AdvancedAvailCheck
		public abstract class advancedAvailCheck : PX.Data.BQL.BqlBool.Field<advancedAvailCheck> { }
		protected Boolean? _AdvancedAvailCheck;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Advanced Availability Validation")]
		public virtual Boolean? AdvancedAvailCheck
		{
			get
			{
				return this._AdvancedAvailCheck;
			}
			set
			{
				this._AdvancedAvailCheck = value;
			}
		}
		#endregion
        #region UseShippedNotInvoiced
        public abstract class useShippedNotInvoiced : PX.Data.BQL.BqlBool.Field<useShippedNotInvoiced> { }
        protected Boolean? _UseShippedNotInvoiced;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Shipped-Not-Invoiced Account")]
        public virtual Boolean? UseShippedNotInvoiced
        {
            get
            {
                return this._UseShippedNotInvoiced;
            }
            set
            {
                this._UseShippedNotInvoiced = value;
            }
        }
        #endregion
        #region ShippedNotInvoicedAcctID
        public abstract class shippedNotInvoicedAcctID : PX.Data.BQL.BqlInt.Field<shippedNotInvoicedAcctID> { }
        protected Int32? _ShippedNotInvoicedAcctID;
        [GL.Account(DisplayName = "Shipped-Not-Invoiced Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(GL.Account.description), ControlAccountForModule = GL.ControlAccountModule.SO)]
		[PXForeignReference(typeof(Field<SOSetup.shippedNotInvoicedAcctID>.IsRelatedTo<Account.accountID>))]
        public virtual Int32? ShippedNotInvoicedAcctID
        {
            get
            {
                return this._ShippedNotInvoicedAcctID;
            }
            set
            {
                this._ShippedNotInvoicedAcctID = value;
            }
        }
        #endregion
        #region ShippedNotInvoicedSubID
        public abstract class shippedNotInvoicedSubID : PX.Data.BQL.BqlInt.Field<shippedNotInvoicedSubID> { }
        protected Int32? _ShippedNotInvoicedSubID;
        [GL.SubAccount(typeof(SOSetup.shippedNotInvoicedAcctID), DisplayName = "Shipped-Not-Invoiced Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(GL.Sub.description))]
		[PXForeignReference(typeof(Field<SOSetup.shippedNotInvoicedSubID>.IsRelatedTo<Sub.subID>))]
        public virtual Int32? ShippedNotInvoicedSubID
        {
            get
            {
                return this._ShippedNotInvoicedSubID;
            }
            set
            {
                this._ShippedNotInvoicedSubID = value;
            }
        }
        #endregion

		#region SalesProfitabilityForNSKits
		public abstract class salesProfitabilityForNSKits : PX.Data.BQL.BqlString.Field<salesProfitabilityForNSKits> { }
		protected String _SalesProfitabilityForNSKits;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SalesProfitabilityNSKitMethod.NSKitStandardAndStockComponentsCost)]
		[SalesProfitabilityNSKitMethod.List()]
		[PXUIField(DisplayName = "Cost Calculation Basis for Non-Stock Kits", Visibility = PXUIVisibility.Visible)]
		public virtual String SalesProfitabilityForNSKits
		{
			get
			{
				return this._SalesProfitabilityForNSKits;
			}
			set
			{
				this._SalesProfitabilityForNSKits = value;
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
	}


	public static class MinGrossProfitValidationType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(None, Messages.None),
					Pair(Warning, Messages.Warning),
					Pair(SetToMin, Messages.SetToMin),
				}) {}
		}

		public const string None = "N";
		public const string Warning = "W";
		public const string SetToMin = "S";

	}

	public static class SalesPriceUpdateUnitType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(BaseUnit, Messages.BaseUnit),
					Pair(SalesUnit, Messages.SalesUnit),
				}) {}
		}

		public const string BaseUnit = "B";
		public const string SalesUnit = "S";

	}

	public static class FreeItemShipType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Proportional, Messages.Proportional),
					Pair(OnLastShipment, Messages.OnLastShipment),
				}) {}
		}

		public const string Proportional = "P";
		public const string OnLastShipment = "S";
	}

	public static class FreightAllocationList
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FullAmount, Messages.FullAmount),
					Pair(Prorate, Messages.Prorate),
				}) {}
		}

		public const string FullAmount = "A";
		public const string Prorate = "P";

		public class fullAmount : PX.Data.BQL.BqlString.Constant<fullAmount>
		{
			public fullAmount() : base(FullAmount) {}
		}

		public class prorate : PX.Data.BQL.BqlString.Constant<prorate>
		{
			public prorate() : base(Prorate) {}
		}
	}

	public static class SalesProfitabilityNSKitMethod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { StockComponentsCostOnly, NSKitStandardCostOnly, NSKitStandardAndStockComponentsCost },
				new string[] { Messages.StockComponentsCostOnly, Messages.NSKitStandardCostOnly, Messages.NSKitStandardAndStockComponentsCost })
			{ }
		}
		public const string StockComponentsCostOnly = "S";
		public const string NSKitStandardCostOnly = "K";
		public const string NSKitStandardAndStockComponentsCost = "C";
	}
}