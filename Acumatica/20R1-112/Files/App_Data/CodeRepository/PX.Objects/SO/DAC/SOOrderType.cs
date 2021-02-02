using PX.Common;
using PX.Objects.Common;

namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Objects.AR;
	using PX.Objects.IN;
	using PX.Objects.CS;
	using PX.Objects.GL;
	using PX.SM;
	using PX.Data.ReferentialIntegrity.Attributes;

	/*
1. Non-inventory orders cannot require location and cannot create shipments
2. Non-ar orders cannot have inventory doc type IN,DM,CM
*/

	[System.SerializableAttribute]
	[PXPrimaryGraph(typeof(SOOrderTypeMaint))]
	[PXCacheName(Messages.OrderType, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class SOOrderType : IBqlTable, PXNoteAttribute.IPXCopySettings
	{
		#region Keys
		public class PK: PrimaryKeyOf<SOOrderType>.By<orderType>
		{
			public static SOOrderType Find(PXGraph graph, string orderType) => FindBy(graph, orderType);
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask=">aa")]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search2<SOOrderType.orderType, InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>,
			Where<SOOrderType.requireShipping, Equal<boolFalse>, Or<FeatureInstalled<FeaturesSet.inventory>>>>))]
		[PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), null)]
		[PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), null)]
		public virtual String OrderType
		{
			get { return this._OrderType; }
			set { this._OrderType = value; }
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get { return this._Active; }
			set { this._Active = value; }
		}
		#endregion
		#region DaysToKeep
		public abstract class daysToKeep : PX.Data.BQL.BqlShort.Field<daysToKeep> { }
		protected Int16? _DaysToKeep;
		[PXDBShort]
		[PXDefault((short)30)]
		[PXUIField(DisplayName = "Days To Keep")]
		public virtual Int16? DaysToKeep
		{
			get { return this._DaysToKeep; }
			set { this._DaysToKeep = value; }
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get { return this._Descr; }
			set { this._Descr = value; }
		}
		#endregion		
		#region Template
		public abstract class template : PX.Data.BQL.BqlString.Field<template> { }
		protected String _Template;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Order Template", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search<SOOrderTypeT.orderType, Where2<Where<SOOrderTypeT.requireAllocation, NotEqual<True>
				, Or2<FeatureInstalled<FeaturesSet.warehouseLocation>
					, Or2<FeatureInstalled<FeaturesSet.lotSerialTracking>
						, Or2<FeatureInstalled<FeaturesSet.subItem>
							, Or2<FeatureInstalled<FeaturesSet.replenishment>
								, Or<FeatureInstalled<FeaturesSet.sOToPOLink>>
							>
						>
					>
				>>, And<Where<SOOrderTypeT.requireShipping, Equal<boolFalse>, Or<FeatureInstalled<FeaturesSet.inventory>>>>
			>
		>
			), DirtyRead = true, DescriptionField = typeof(SOOrderTypeT.descr))]
		public virtual String Template
		{
			get { return this._Template; }
			set { this._Template = value; }
		}
		#endregion
		#region IsSystem
		public abstract class isSystem : PX.Data.BQL.BqlBool.Field<isSystem> { }
		protected Boolean? _IsSystem;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is System Template", Enabled = false)]
		public virtual Boolean? IsSystem
		{
			get
			{
				return this._IsSystem;
			}
			set
			{
				this._IsSystem = value;
			}
		}
		#endregion
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Automation Behavior", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[SOBehavior.List]
		public virtual String Behavior
		{
			get { return this._Behavior; }
			set { this._Behavior = value; }
		}
		#endregion
		#region DefaultOperation
		public abstract class defaultOperation : PX.Data.BQL.BqlString.Field<defaultOperation>
		{
			public const int Length = 1;
		}
		protected String _DefaultOperation;
		[PXDBString(defaultOperation.Length, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Default Operation")]
		[PXDefault(typeof(Search<SOOrderType.defaultOperation,
			Where<SOOrderType.orderType, Equal<Current<SOOrderType.behavior>>>>))]
		[SOOperation.List]
		public virtual String DefaultOperation
		{
			get { return this._DefaultOperation; }
			set { this._DefaultOperation = value; }
		}
		#endregion		
		#region INDocType
		public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }
		protected String _INDocType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[INTranType.SOList]
		[PXUIField(DisplayName = "Inventory Transaction Type")]
		public virtual String INDocType
		{
			get { return this._INDocType; }
			set { this._INDocType = value; }
		}
		#endregion
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		protected String _ARDocType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[ARDocType.SOList]
		[PXUIField(DisplayName = "AR Document Type")]
		public virtual String ARDocType
		{
			get { return this._ARDocType; }
			set { this._ARDocType = value; }
		}
		#endregion
		#region OrderPlanType
		public abstract class orderPlanType : PX.Data.BQL.BqlString.Field<orderPlanType> { }
		protected String _OrderPlanType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Order Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), DescriptionField = typeof(INPlanType.descr))]
		public virtual String OrderPlanType
		{
			get { return this._OrderPlanType; }
			set { this._OrderPlanType = value; }
		}
		#endregion
		#region ShipmentPlanType
		public abstract class shipmentPlanType : PX.Data.BQL.BqlString.Field<shipmentPlanType> { }
		protected String _ShipmentPlanType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Shipment Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), DescriptionField=typeof(INPlanType.descr))]
		public virtual String ShipmentPlanType
		{
			get { return this._ShipmentPlanType; }
			set { this._ShipmentPlanType = value; }
		}
		#endregion
		#region OrderNumberingID
		public abstract class orderNumberingID : PX.Data.BQL.BqlString.Field<orderNumberingID> { }
		protected String _OrderNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXSelector(typeof(Search<Numbering.numberingID>))]
		[PXUIField(DisplayName = "Order Numbering Sequence")]
		public virtual String OrderNumberingID
		{
			get { return this._OrderNumberingID; }
			set { this._OrderNumberingID = value; }
		}
		#endregion
		#region InvoiceNumberingID
		public abstract class invoiceNumberingID : PX.Data.BQL.BqlString.Field<invoiceNumberingID> { }
		protected String _InvoiceNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Search<Numbering.numberingID>))]
		[PXUIField(DisplayName = "Invoice Numbering Sequence")]
		public virtual String InvoiceNumberingID
		{
			get { return this._InvoiceNumberingID; }
			set { this._InvoiceNumberingID = value; }
		}
		#endregion
		#region UserInvoiceNumbering
		public abstract class userInvoiceNumbering : PX.Data.BQL.BqlBool.Field<userInvoiceNumbering> { }
		protected Boolean? _UserInvoiceNumbering;
		[PXBool]
		[PXFormula(typeof(Selector<SOOrderType.invoiceNumberingID, Numbering.userNumbering>))]
		[PXUIField(DisplayName = "Manual Invoice Numbering")]
		public virtual Boolean? UserInvoiceNumbering
		{
			get { return this._UserInvoiceNumbering; }
			set { this._UserInvoiceNumbering = value; }
		}
		#endregion
		#region MarkInvoicePrinted
		public abstract class markInvoicePrinted : PX.Data.BQL.BqlBool.Field<markInvoicePrinted> { }
		protected Boolean? _MarkInvoicePrinted;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Mark as Printed")]
		public virtual Boolean? MarkInvoicePrinted
		{
			get { return this._MarkInvoicePrinted; }
			set { this._MarkInvoicePrinted = value; }
		}
		#endregion
		#region MarkInvoiceEmailed
		public abstract class markInvoiceEmailed : PX.Data.BQL.BqlBool.Field<markInvoiceEmailed> { }
		protected Boolean? _MarkInvoiceEmailed;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Mark as Emailed")]
		public virtual Boolean? MarkInvoiceEmailed
		{
			get { return this._MarkInvoiceEmailed; }
			set { this._MarkInvoiceEmailed = value; }
		}
		#endregion
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		protected Boolean? _HoldEntry;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold Orders on Entry")]
		public virtual Boolean? HoldEntry
		{
			get { return this._HoldEntry; }
			set { this._HoldEntry = value; }
		}
		#endregion
		#region InvoiceHoldEntry
		public abstract class invoiceHoldEntry : PX.Data.BQL.BqlBool.Field<invoiceHoldEntry> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold Invoices on Entry")]
		public virtual Boolean? InvoiceHoldEntry { get; set; }
		#endregion
		#region CreditHoldEntry
		public abstract class creditHoldEntry : PX.Data.BQL.BqlBool.Field<creditHoldEntry> { }
		protected Boolean? _CreditHoldEntry;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold Document on Failed Credit Check")]
		public virtual Boolean? CreditHoldEntry
		{
			get { return this._CreditHoldEntry; }
			set { this._CreditHoldEntry = value; }
		}
		#endregion
		#region UseCuryRateFromSO
		public abstract class useCuryRateFromSO : Data.BQL.BqlBool.Field<useCuryRateFromSO> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Currency Rate from Sales Order", FieldClass = nameof(FeaturesSet.Multicurrency))]
		public virtual bool? UseCuryRateFromSO
		{
			get;
			set;
		}
		#endregion

		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected Boolean? _RequireAllocation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Stock Allocation")]
		public virtual Boolean? RequireAllocation
		{
			get { return this._RequireAllocation; }
			set { this._RequireAllocation = value; }
		}
		#endregion		
		#region RequireLocation
		public abstract class requireLocation : PX.Data.BQL.BqlBool.Field<requireLocation> { }
		protected Boolean? _RequireLocation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Location", Enabled = false)]
		public virtual Boolean? RequireLocation
		{
			get { return this._RequireLocation; }
			set { this._RequireLocation = value; }
		}
		#endregion
		#region RequireLotSerial
		public abstract class requireLotSerial : PX.Data.BQL.BqlBool.Field<requireLotSerial> { }
		protected Boolean? _RequireLotSerial;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Lot/Serial Entry")]
		public virtual Boolean? RequireLotSerial
		{
			get { return this._RequireLotSerial; }
			set { this._RequireLotSerial = value; }
		}
		#endregion
		#region AllowQuickProcess
		public abstract class allowQuickProcess : PX.Data.BQL.BqlBool.Field<allowQuickProcess> { }
		protected Boolean? _AllowQuickProcess;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Quick Process")]
		[PXUIVisible(typeof(Where<behavior, In3<SOOrderTypeConstants.salesOrder, SOOrderTypeConstants.invoiceOrder, SOOrderTypeConstants.creditMemo>>))]
		public virtual Boolean? AllowQuickProcess
		{
			get { return this._AllowQuickProcess; }
			set { this._AllowQuickProcess = value; }
		}
		#endregion

		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.BQL.BqlBool.Field<requireControlTotal> { }
		protected Boolean? _RequireControlTotal;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Control Total")]
		public virtual Boolean? RequireControlTotal
		{
			get { return this._RequireControlTotal; }
			set { this._RequireControlTotal = value; }
		}
		#endregion
		#region RequireShipping
		public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
		protected Boolean? _RequireShipping;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Shipments")]
		public virtual Boolean? RequireShipping
		{
			get
			{
				return SOBehavior.GetRequireShipmentValue(this.Behavior, this._RequireShipping);
			}
			set
			{
				this._RequireShipping = SOBehavior.GetRequireShipmentValue(this.Behavior, value);
			}
		}
		#endregion
		#region SupportsApproval
		public abstract class supportsApproval : PX.Data.BQL.BqlBool.Field<supportsApproval> { }
		protected bool? _SupportsApproval;
		[PXBool]        
		[PXUIField(DisplayName = "Supports Approval", Enabled = false)]
		public virtual bool? SupportsApproval
		{
			get { return _SupportsApproval; }
			set { _SupportsApproval = value; }
		}
		#endregion
		#region CopyLotSerialFromShipment
		public abstract class copyLotSerialFromShipment : PX.Data.BQL.BqlBool.Field<copyLotSerialFromShipment> { }
		protected Boolean? _CopyLotSerialFromShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Lot/Serial numbers from Shipment back to Sales Order")]
		public virtual Boolean? CopyLotSerialFromShipment
		{
			get { return this._CopyLotSerialFromShipment; }
			set { this._CopyLotSerialFromShipment = value; }
		}
		#endregion

		#region BillSeparately
		public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately> { }
		protected Boolean? _BillSeparately;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill Separately")]
		public virtual Boolean? BillSeparately
		{
			get { return this._BillSeparately; }
			set { this._BillSeparately = value; }
		}
		#endregion
		#region ShipSeparately
		public abstract class shipSeparately : PX.Data.BQL.BqlBool.Field<shipSeparately> { }
		protected Boolean? _ShipSeparately;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ship Separately")]
		public virtual Boolean? ShipSeparately
		{
			get { return this._ShipSeparately; }
			set { this._ShipSeparately = value; }
		}
		#endregion
		#region SalesAcctDefault
		public abstract class salesAcctDefault : PX.Data.BQL.BqlString.Field<salesAcctDefault> { }
		protected String _SalesAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Sales Account from")]
		[SOSalesAcctSubDefault.AcctList]
		[PXDefault(SOSalesAcctSubDefault.MaskItem)]
		public virtual String SalesAcctDefault
		{
			get { return this._SalesAcctDefault; }
			set { this._SalesAcctDefault = value; }
		}
		#endregion
		#region MiscAcctDefault
		public abstract class miscAcctDefault : PX.Data.BQL.BqlString.Field<miscAcctDefault> { }
		protected String _MiscAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Misc. Account from")]
		[SOMiscAcctSubDefault.AcctList]
		[PXDefault(SOMiscAcctSubDefault.MaskItem)]
		public virtual String MiscAcctDefault
		{
			get { return this._MiscAcctDefault; }
			set { this._MiscAcctDefault = value; }
		}
		#endregion
		#region FreightAcctDefault
		public abstract class freightAcctDefault : PX.Data.BQL.BqlString.Field<freightAcctDefault> { }
		protected String _FreightAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Freight Account from")]
		[SOFreightAcctSubDefault.AcctList]
		[PXDefault(SOFreightAcctSubDefault.MaskShipVia)]
		public virtual String FreightAcctDefault
		{
			get { return this._FreightAcctDefault; }
			set { this._FreightAcctDefault = value; }
		}
		#endregion
		#region DiscAcctDefault
		public abstract class discAcctDefault : PX.Data.BQL.BqlString.Field<discAcctDefault> { }
		protected String _DiscAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Discount Account from")]
		[SODiscAcctSubDefault.AcctList]
		[PXDefault(SODiscAcctSubDefault.MaskLocation)]
		public virtual String DiscAcctDefault
		{
			get { return this._DiscAcctDefault; }
			set { this._DiscAcctDefault = value; }
		}
		#endregion
		#region COGSAcctDefault
		public abstract class cOGSAcctDefault : PX.Data.BQL.BqlString.Field<cOGSAcctDefault> { }
		protected String _COGSAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use COGS Account from", Visible = false, Enabled = false)]
		[SOCOGSAcctSubDefault.AcctList]
		//[PXDefault(SOCOGSAcctSubDefault.MaskItem)]
		public virtual String COGSAcctDefault
		{
			get { return this._COGSAcctDefault; }
			set { this._COGSAcctDefault = value; }
		}
		#endregion
		#region SalesSubMask
		public abstract class salesSubMask : PX.Data.BQL.BqlString.Field<salesSubMask> { }
		protected String _SalesSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SOSalesSubAccountMask(DisplayName = "Combine Sales Sub. From")]
		public virtual String SalesSubMask
		{
			get { return this._SalesSubMask; }
			set { this._SalesSubMask = value; }
		}
		#endregion
		#region MiscSubMask
		public abstract class miscSubMask : PX.Data.BQL.BqlString.Field<miscSubMask> { }
		protected String _MiscSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SOMiscSubAccountMask(DisplayName = "Combine Misc. Sub. from")]
		public virtual String MiscSubMask
		{
			get { return this._MiscSubMask; }
			set { this._MiscSubMask = value; }
		}
		#endregion
		#region FreightSubMask
		public abstract class freightSubMask : PX.Data.BQL.BqlString.Field<freightSubMask> { }
		protected String _FreightSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SOFreightSubAccountMask(DisplayName = "Combine Freight Sub. from")]
		public virtual String FreightSubMask
		{
			get { return this._FreightSubMask; }
			set { this._FreightSubMask = value; }
		}
		#endregion
		#region DiscSubMask
		public abstract class discSubMask : PX.Data.BQL.BqlString.Field<discSubMask> { }
		protected String _DiscSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<FeatureInstalled<FeaturesSet.customerDiscounts>>>))]
		[SODiscSubAccountMask(DisplayName = "Combine Discount Sub. from")]
		public virtual String DiscSubMask
		{
			get { return this._DiscSubMask; }
			set { this._DiscSubMask = value; }
		}
		#endregion
		#region FreightAcctID
		public abstract class freightAcctID : PX.Data.BQL.BqlInt.Field<freightAcctID> { }
		protected Int32? _FreightAcctID;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<SOOrderType.freightAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? FreightAcctID
		{
			get { return this._FreightAcctID; }
			set { this._FreightAcctID = value; }
		}
		#endregion
		#region FreightSubID
		public abstract class freightSubID : PX.Data.BQL.BqlInt.Field<freightSubID> { }
		protected Int32? _FreightSubID;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SubAccount(typeof(SOOrderType.freightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<SOOrderType.freightSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? FreightSubID
		{
			get { return this._FreightSubID; }
			set { this._FreightSubID = value; }
		}
		#endregion
		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion
		#region RecalculateDiscOnPartialShipment
		public abstract class recalculateDiscOnPartialShipment : PX.Data.BQL.BqlBool.Field<recalculateDiscOnPartialShipment> { }
		protected bool? _RecalculateDiscOnPartialShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Discount On Partial Shipment")]
		public virtual bool? RecalculateDiscOnPartialShipment
		{
			get { return _RecalculateDiscOnPartialShipment; }
			set { _RecalculateDiscOnPartialShipment = value; }
		}
		#endregion
		#region ShipFullIfNegQtyAllowed
		public abstract class shipFullIfNegQtyAllowed : PX.Data.BQL.BqlBool.Field<shipFullIfNegQtyAllowed>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ship in Full if Negative Quantity Is Allowed")]
		public virtual bool? ShipFullIfNegQtyAllowed
		{
			get;
			set;
		}
		#endregion
		#region CalculateFreight
		public abstract class calculateFreight : PX.Data.BQL.BqlBool.Field<calculateFreight> { }
		protected bool? _CalculateFreight;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Calculate Freight")]
		public virtual bool? CalculateFreight
		{
			get { return _CalculateFreight; }
			set { _CalculateFreight = value; }
		}
		#endregion
		

		#region COGSSubMask
		public abstract class cOGSSubMask : PX.Data.BQL.BqlString.Field<cOGSSubMask> { }
		protected String _COGSSubMask;
		//[PXDefault()]
		[SOCOGSSubAccountMask(DisplayName = "Combine COGS Sub. From", Visible = false, Enabled = false)]
		public virtual String COGSSubMask
		{
			get { return this._COGSSubMask; }
			set { this._COGSSubMask = value; }
		}
		#endregion
		#region DiscountAcctID
		public abstract class discountAcctID : PX.Data.BQL.BqlInt.Field<discountAcctID> { }
		protected Int32? _DiscountAcctID;
		[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<FeatureInstalled<FeaturesSet.customerDiscounts>>>))]
		[PXForeignReference(typeof(Field<SOOrderType.discountAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? DiscountAcctID
		{
			get { return this._DiscountAcctID; }
			set { this._DiscountAcctID = value; }
		}
		#endregion
		#region DiscountSubID
		public abstract class discountSubID : PX.Data.BQL.BqlInt.Field<discountSubID> { }
		protected Int32? _DiscountSubID;
		[SubAccount(typeof(SOOrderType.discountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And2<FeatureInstalled<FeaturesSet.customerDiscounts>, And<FeatureInstalled<FeaturesSet.subAccount>>>>))]
		[PXForeignReference(typeof(Field<SOOrderType.discountSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? DiscountSubID
		{
			get { return this._DiscountSubID; }
			set { this._DiscountSubID = value; }
		}
		#endregion
		#region OrderPriority
		public abstract class orderPriority : PX.Data.BQL.BqlShort.Field<orderPriority> { }
		protected Int16? _OrderPriority;
		[PXDBShort]
		public virtual Int16? OrderPriority
		{
			get { return this._OrderPriority; }
			set { this._OrderPriority = value; }
		}
		#endregion
		#region CopyNotes
		public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
		protected bool? _CopyNotes;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Notes")]
		public virtual bool? CopyNotes
		{
			get { return _CopyNotes; }
			set { _CopyNotes = value; }
		}
		#endregion
		#region CopyFiles
		public abstract class copyFiles : PX.Data.BQL.BqlBool.Field<copyFiles> { }
		protected bool? _CopyFiles;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Attachments")]
		public virtual bool? CopyFiles
		{
			get { return _CopyFiles; }
			set { _CopyFiles = value; }
		}
		#endregion
		#region CopyLineNotesToShipment
		public abstract class copyLineNotesToShipment : PX.Data.BQL.BqlBool.Field<copyLineNotesToShipment> { }
		protected bool? _CopyLineNotesToShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes To Shipment")]
		public virtual bool? CopyLineNotesToShipment
		{
			get { return _CopyLineNotesToShipment; }
			set { _CopyLineNotesToShipment = value; }
		}
		#endregion
		#region CopyLineFilesToShipment
		public abstract class copyLineFilesToShipment : PX.Data.BQL.BqlBool.Field<copyLineFilesToShipment> { }
		protected bool? _CopyLineFilesToShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Attachments To Shipment")]
		public virtual bool? CopyLineFilesToShipment
		{
			get { return _CopyLineFilesToShipment; }
			set { _CopyLineFilesToShipment = value; }
		}
		#endregion
		#region CopyLineNotesToInvoice
		public abstract class copyLineNotesToInvoice : PX.Data.BQL.BqlBool.Field<copyLineNotesToInvoice> { }
		protected bool? _CopyLineNotesToInvoice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes To Invoice")]
		public virtual bool? CopyLineNotesToInvoice
		{
			get { return _CopyLineNotesToInvoice; }
			set { _CopyLineNotesToInvoice = value; }
		}
		#endregion
		#region CopyLineNotesToInvoiceOnlyNS
		public abstract class copyLineNotesToInvoiceOnlyNS : PX.Data.BQL.BqlBool.Field<copyLineNotesToInvoiceOnlyNS> { }
		protected bool? _CopyLineNotesToInvoiceOnlyNS;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Only Non-Stock")]
		public virtual bool? CopyLineNotesToInvoiceOnlyNS
		{
			get { return _CopyLineNotesToInvoiceOnlyNS; }
			set { _CopyLineNotesToInvoiceOnlyNS = value; }
		}
		#endregion
		#region CopyLineFilesToInvoice
		public abstract class copyLineFilesToInvoice : PX.Data.BQL.BqlBool.Field<copyLineFilesToInvoice> { }
		protected bool? _CopyLineFilesToInvoice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Attachments To Invoice")]
		public virtual bool? CopyLineFilesToInvoice
		{
			get { return _CopyLineFilesToInvoice; }
			set { _CopyLineFilesToInvoice = value; }
		}
		#endregion
		#region CopyLineFilesToInvoiceOnlyNS
		public abstract class copyLineFilesToInvoiceOnlyNS : PX.Data.BQL.BqlBool.Field<copyLineFilesToInvoiceOnlyNS> { }
		protected bool? _CopyLineFilesToInvoiceOnlyNS;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Only Non-Stock")]
		public virtual bool? CopyLineFilesToInvoiceOnlyNS
		{
			get { return _CopyLineFilesToInvoiceOnlyNS; }
			set { _CopyLineFilesToInvoiceOnlyNS = value; }
		}
		#endregion
		#region CustomerOrderIsRequired
		public abstract class customerOrderIsRequired : PX.Data.BQL.BqlBool.Field<customerOrderIsRequired> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Customer Order Nbr.")]
		public virtual bool? CustomerOrderIsRequired
		{
			get;
			set;
		}
		#endregion
		#region CustomerOrderValidation
		public abstract class customerOrderValidation : PX.Data.BQL.BqlString.Field<customerOrderValidation> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Customer Order Nbr. Validation")]
		[CustomerOrderValidationType.List]
		[PXDefault(CustomerOrderValidationType.None)]
		public virtual String CustomerOrderValidation
		{
			get;
			set;
		}
		#endregion

		#region PostLineDiscSeparately
		public abstract class postLineDiscSeparately : PX.Data.BQL.BqlBool.Field<postLineDiscSeparately> { }
		protected bool? _PostLineDiscSeparately;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Line Discounts Separately")]
		public virtual bool? PostLineDiscSeparately
		{
			get { return _PostLineDiscSeparately; }
			set { _PostLineDiscSeparately = value; }
		}
		#endregion
		#region UseDiscountSubFromSalesSub
		public abstract class useDiscountSubFromSalesSub : PX.Data.BQL.BqlBool.Field<useDiscountSubFromSalesSub> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Discount Sub. from Sales Sub.", FieldClass = SubAccountAttribute.DimensionName)]
		public virtual bool? UseDiscountSubFromSalesSub { get; set; }
		#endregion
		#region CommitmentTracking
		public abstract class commitmentTracking : PX.Data.BQL.BqlBool.Field<commitmentTracking> { }
		protected Boolean? _CommitmentTracking;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Commitment Tracking")]
		public virtual Boolean? CommitmentTracking
		{
			get { return this._CommitmentTracking; }
			set { this._CommitmentTracking = value; }
		}
		#endregion
		#region AutoWriteOff
		public abstract class autoWriteOff : PX.Data.BQL.BqlBool.Field<autoWriteOff> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Auto Write-Off")]
		public virtual bool? AutoWriteOff { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(SOOrderType.orderType),
			Selector = typeof(SOOrderType.orderType), 
			FieldList = new [] { typeof(SOOrderType.orderType), typeof(SOOrderType.descr) })]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get { return this._CreatedByID; }
			set { this._CreatedByID = value; }
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get { return this._CreatedByScreenID; }
			set { this._CreatedByScreenID = value; }
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get { return this._CreatedDateTime; }
			set { this._CreatedDateTime = value; }
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get { return this._LastModifiedByID; }
			set { this._LastModifiedByID = value; }
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get { return this._LastModifiedByScreenID; }
			set { this._LastModifiedByScreenID = value; }
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get { return this._LastModifiedDateTime; }
			set { this._LastModifiedDateTime = value; }
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get { return this._tstamp; }
			set { this._tstamp = value; }
		}
		#endregion

		public bool CanHaveApplications => ARDocType.IsIn(AR.ARDocType.Invoice, AR.ARDocType.DebitMemo);
	}

	[PXProjection(typeof(Select<SOOrderType, 
		Where<SOOrderType.orderType, Equal<SOOrderType.template>,
			And<SOOrderType.orderType, IsNotNull>>>))]
	[Serializable]
	[PXHidden]
	public partial class SOOrderTypeT : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOOrderTypeT>.By<orderType>
		{
			public static SOOrderTypeT Find(PXGraph graph, string orderType) => FindBy(graph, orderType);
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">aa", BqlField = typeof(SOOrderType.orderType))]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		public virtual String OrderType
		{
			get { return this._OrderType; }
			set { this._OrderType = value; }
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true,  BqlField = typeof(SOOrderType.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get { return this._Descr; }
			set { this._Descr = value; }
		}
		#endregion
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;
		[PXDBString(2, IsFixed = true, InputMask = ">aa", BqlField = typeof(SOOrderType.behavior))]
		[PXUIField(DisplayName = "Automation Behavior", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[SOBehavior.List]
		public virtual String Behavior
		{
			get { return this._Behavior; }
			set { this._Behavior = value; }
		}
		#endregion
		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected Boolean? _RequireAllocation;
		[PXDBBool(BqlField = typeof(SOOrderType.requireAllocation))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Stock Allocation")]
		public virtual Boolean? RequireAllocation
		{
			get { return this._RequireAllocation; }
			set { this._RequireAllocation = value; }
		}
		#endregion
		#region RequireShipping
		public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
		protected Boolean? _RequireShipping;
		[PXDBBool(BqlField = typeof(SOOrderType.requireShipping))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Shipments")]
		public virtual Boolean? RequireShipping
		{
			get { return this._RequireShipping; }
			set { this._RequireShipping = value; }
		}
		#endregion
	}

	public class SOAutomation
	{
		public const string Behavior = "Behavior";
		public const string GraphType = "PX.Objects.SO.SOOrderEntry";

		public class behavior : PX.Data.BQL.BqlString.Constant<behavior>
		{
			public behavior() : base(Behavior) { }
		}
	}

	public class SOBehavior
	{
		public const string SO = SOOrderTypeConstants.SalesOrder;
		public const string IN = SOOrderTypeConstants.Invoice;
		public const string QT = SOOrderTypeConstants.QuoteOrder;
		public const string RM = SOOrderTypeConstants.RMAOrder;
		public const string CM = SOOrderTypeConstants.CreditMemo;

		public class sO : PX.Data.BQL.BqlString.Constant<sO>
		{
			public sO() : base(SO) { }
		}

		public class qT : PX.Data.BQL.BqlString.Constant<qT>
		{
			public qT() : base(QT) { }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
			{				
					Pair(SO, Messages.SOName),
					Pair(IN, Messages.INName),
					Pair(QT, Messages.QTName),
					Pair(CM, Messages.CMName),
					Pair(RM, Messages.RMName),
			}
				) {}
		}

		public static bool? GetRequireShipmentValue(string behavior, bool? value)
		{
			if (behavior.IsIn(SOBehavior.SO, SOBehavior.RM))
				return true;
			if (behavior.IsIn(SOBehavior.IN, SOBehavior.QT, SOBehavior.CM))
				return false;
			return value;
		}

		public static bool IsPredefinedBehavior(string behavior)
		{
			return behavior.IsIn(SOBehavior.IN, SOBehavior.QT, SOBehavior.CM, SOBehavior.SO, SOBehavior.RM);
		}

		public static string DefaultOperation(string behavior, string ardoctype)
		{
			switch (behavior)
			{
				case SO:
				case IN:
					switch (ardoctype)
					{
						case ARDocType.Invoice:
						case ARDocType.DebitMemo:
						case ARDocType.CashSale:
						case ARDocType.NoUpdate:
							return SOOperation.Issue;
						case ARDocType.CreditMemo:
						case ARDocType.CashReturn:
							return SOOperation.Receipt;
						default:
							return null;
					}
				case QT:
					return SOOperation.Issue;
				case CM:
				case RM:
					return SOOperation.Receipt;
			}
			return null;
		}
	}

	public class AllocationAllowed : WhereBase<FeatureInstalled<FeaturesSet.warehouseLocation>,
			Or2<FeatureInstalled<FeaturesSet.lotSerialTracking>,
				Or2<FeatureInstalled<FeaturesSet.subItem>,
					Or2<FeatureInstalled<FeaturesSet.replenishment>,
						Or<FeatureInstalled<FeaturesSet.sOToPOLink>>>>>>
	{
		public override bool UseParenthesis() { return true; }
	}

	public static class CustomerOrderValidationType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(None, Messages.CustOrdeNoValidation),
					Pair(Warn, Messages.CustOrderWarnOnDuplicates),
					Pair(Error, Messages.CustOrderErrorOnDuplicates),
				}) {}
		}

		public const string None = "N";
		public const string Warn = "W";
		public const string Error = "E";
	}

}
