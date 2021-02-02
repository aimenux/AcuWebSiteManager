using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.IN;
	using PX.Objects.AR;
	using PX.Objects.CR;
	using PX.Objects.CM;
	using POReceipt = PX.Objects.PO.POReceipt;
	using CRLocation = PX.Objects.CR.Standalone.Location;
	using PX.Objects.Common.Bql;
	using PX.Objects.Common;
    using PX.Objects.Common.Attributes;
    using PX.SM;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(SOShipmentEntry))]
	[PXCacheName(Messages.SOShipment)]
	[PXEMailSource]
	public partial class SOShipment : PX.Data.IBqlTable, PX.Data.EP.IAssign, IFreightBase 
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOShipment>.By<shipmentNbr>
		{
			public static SOShipment Find(PXGraph graph, string shipmentNbr) => FindBy(graph, shipmentNbr);
		}
		public static class FK
		{
			public class ShipAddress : SOShipmentAddress.PK.ForeignKeyOf<SOShipment>.By<shipAddressID> { }
			public class ShipContact : SOShipmentContact.PK.ForeignKeyOf<SOShipment>.By<shipContactID> { }
			public class Site : INSite.PK.ForeignKeyOf<SOShipment>.By<siteID> { }
			public class DestinationSite : INSite.PK.ForeignKeyOf<SOShipment>.By<destinationSiteID> { }
			public class ShipTerms : CS.ShipTerms.PK.ForeignKeyOf<SOShipment>.By<shipTermsID> { }
			public class ShipZone : CS.ShippingZone.PK.ForeignKeyOf<SOShipment>.By<shipZoneID> { }
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<SOShipment>.By<taxCategoryID> { }
			public class Carrier : CS.Carrier.PK.ForeignKeyOf<SOShipment>.By<shipVia> { }
			public class Worksheet : SOPickingWorksheet.PK.ForeignKeyOf<SOShipment>.By<currentWorksheetNbr> { }
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region ShipmentNbr
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr>
		{
			public const string DisplayName = "Shipment Nbr.";
		}
		protected String _ShipmentNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = shipmentNbr.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(SOSetup.shipmentNumberingID), typeof(SOShipment.shipDate))]
		[PXSelector(typeof(Search2<SOShipment.shipmentNbr,
			InnerJoin<INSite, On<SOShipment.FK.Site>,
			LeftJoinSingleTable<Customer, On<SOShipment.customerID, Equal<Customer.bAccountID>>>>,
			Where2<Match<INSite, Current<AccessInfo.userName>>,
			And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>,
			OrderBy<Desc<SOShipment.shipmentNbr>>>))]
		[PX.Data.EP.PXFieldDescription]
        public virtual String ShipmentNbr
		{
			get
			{
				return this._ShipmentNbr;
			}
			set
			{
				this._ShipmentNbr = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate>
		{
			public const string DisplayName = "Shipment Date";
		}
		protected DateTime? _ShipDate;
		[PXDBDate()]
		[PXUIField(DisplayName = shipDate.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Operation")]		
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region ShipmentType
		public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
		protected String _ShipmentType;
		[PXDBString(1, IsFixed = true)]
		[PXExtraKey]
		[PXDefault(SOShipmentType.Issue)]
		[SOShipmentType.ShortList()]
		[PXUIField(DisplayName = "Type")]
		public virtual String ShipmentType
		{
			get
			{
				return this._ShipmentType;
			}
			set
			{
				this._ShipmentType = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			Visibility = PXUIVisibility.SelectorVisible)]
		[CustomerOrOrganizationRestrictor(typeof(shipmentType))]
		[PXRestrictor(
			typeof(Where<Customer.status, IsNull, 
				Or<Customer.status, Equal<BAccount.status.active>,
				Or<Customer.status, Equal<BAccount.status.oneTime>>>>), 
			AR.Messages.CustomerIsInStatus, 
			typeof(Customer.status))]
		[PXForeignReference(typeof(Field<SOShipment.customerID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<SOShipment.customerID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOShipment.customerID>>,
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.cBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<SOShipment.customerID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<SOShipment.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<SOShipment.customerLocationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region CustomerOrderNbr
		public abstract class customerOrderNbr : PX.Data.BQL.BqlString.Field<customerOrderNbr> { }
		protected String _CustomerOrderNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Order Nbr.", Enabled = false)]
		public virtual String CustomerOrderNbr
		{
			get
			{
				return this._CustomerOrderNbr;
			}
			set
			{
				this._CustomerOrderNbr = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<CRLocation.cSiteID, Where<CRLocation.bAccountID, Equal<Current<SOShipment.customerID>>, And<CRLocation.locationID, Equal<Current<SOShipment.customerLocationID>>>>>))]
		[PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
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
		#region DestinationSiteID
		public abstract class destinationSiteID : PX.Data.BQL.BqlInt.Field<destinationSiteID> { }
		protected Int32? _DestinationSiteID;
		[IN.ToSite(DisplayName = "To Warehouse", DescriptionField = typeof(INSite.descr))]
		[PXForeignReference(typeof(Field<destinationSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? DestinationSiteID
		{
			get
			{
				return this._DestinationSiteID;
			}
			set
			{
				this._DestinationSiteID = value;
			}
		}
		#endregion
		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Shipping Address")]
		[SOShipmentAddress(typeof(Select2<Address,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<Address.bAccountID>,
					 And<Address.addressID, Equal<CRLocation.defAddressID>>>,
			LeftJoin<SOShipmentAddress, On<SOShipmentAddress.customerID, Equal<Address.bAccountID>, 
                And<SOShipmentAddress.customerAddressID, Equal<Address.addressID>, 
                And<SOShipmentAddress.revisionID, Equal<Address.revisionID>, 
                And<SOShipmentAddress.isDefaultAddress, Equal<True>>>>>>>,
			Where<CRLocation.bAccountID, Equal<Current<SOShipment.customerID>>,
					 And<CRLocation.locationID, Equal<Current<SOShipment.customerLocationID>>>>>))]
		public virtual Int32? ShipAddressID
		{
			get
			{
				return this._ShipAddressID;
			}
			set
			{
				this._ShipAddressID = value;
			}
		}
		#endregion
		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }
		protected Int32? _ShipContactID;
		[PXDBInt()]
		[SOShipmentContact(typeof(Select2<Contact, 			
			InnerJoin<CRLocation, 
			      On<CRLocation.bAccountID, Equal<Contact.bAccountID>, 
			     And<Contact.contactID,Equal<CRLocation.defContactID>>>,			
			LeftJoin<SOShipmentContact, On<SOShipmentContact.customerID, Equal<Contact.bAccountID>, 
			     And<SOShipmentContact.customerContactID, Equal<Contact.contactID>, 
					 And<SOShipmentContact.revisionID, Equal<Contact.revisionID>, 
					 And<SOShipmentContact.isDefaultContact, Equal<True>>>>>>>, 
			Where<CRLocation.bAccountID, Equal<Current<SOShipment.customerID>>,
			    And<CRLocation.locationID, Equal<Current<SOShipment.customerLocationID>>>>>))]
		public virtual Int32? ShipContactID
		{
			get
			{
				return this._ShipContactID;
			}
			set
			{
				this._ShipContactID = value;
			}
		}
		#endregion
		#region FOBPoint
		public abstract class fOBPoint : PX.Data.BQL.BqlString.Field<fOBPoint> { }
		protected String _FOBPoint;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(Search<FOBPoint.fOBPointID>), DescriptionField = typeof(FOBPoint.description), CacheGlobal = true)]
		public virtual String FOBPoint
		{
			get
			{
				return this._FOBPoint;
			}
			set
			{
				this._FOBPoint = value;
			}
		}
		#endregion
		#region ShipVia
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected String _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion
		#region WillCall
		public abstract class willCall : Data.BQL.BqlBool.Field<willCall> { }
		[PXBool]
		[PXFormula(typeof(Switch<Case2<Where<Selector<shipVia, Carrier.isCommonCarrier>, NotEqual<True>>, True>, False>))]
		[PXUIField(DisplayName = "Will Call", IsReadOnly = true, Enabled = false)]
		public bool? WillCall
		{
			get;
			set;
		}
		#endregion
		#region UseCustomerAccount
		public abstract class useCustomerAccount : PX.Data.BQL.BqlBool.Field<useCustomerAccount> { }
		protected Boolean? _UseCustomerAccount;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Customer's Account")]
		public virtual Boolean? UseCustomerAccount
		{
			get
			{
				return this._UseCustomerAccount;
			}
			set
			{
				this._UseCustomerAccount = value;
			}
		}
		#endregion
		#region Resedential
		public abstract class resedential : PX.Data.BQL.BqlBool.Field<resedential> { }
		protected Boolean? _Resedential;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? Resedential
		{
			get
			{
				return this._Resedential;
			}
			set
			{
				this._Resedential = value;
			}
		}
		#endregion
		#region SaturdayDelivery
		public abstract class saturdayDelivery : PX.Data.BQL.BqlBool.Field<saturdayDelivery> { }
		protected Boolean? _SaturdayDelivery;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? SaturdayDelivery
		{
			get
			{
				return this._SaturdayDelivery;
			}
			set
			{
				this._SaturdayDelivery = value;
			}
		}
		#endregion
		#region Insurance
		public abstract class insurance : PX.Data.BQL.BqlBool.Field<insurance> { }
		protected Boolean? _Insurance;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? Insurance
		{
			get
			{
				return this._Insurance;
			}
			set
			{
				this._Insurance = value;
			}
		}
		#endregion
		#region GroundCollect
		public abstract class groundCollect : PX.Data.BQL.BqlBool.Field<groundCollect> { }
		protected Boolean? _GroundCollect;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ground Collect")]
		public virtual Boolean? GroundCollect
		{
			get
			{
				return this._GroundCollect;
			}
			set
			{
				this._GroundCollect = value;
			}
		}
		#endregion
		#region LabelsPrinted
		public abstract class labelsPrinted : PX.Data.BQL.BqlBool.Field<labelsPrinted> { }
		protected Boolean? _LabelsPrinted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="Labels Printed")]
		public virtual Boolean? LabelsPrinted
		{
			get
			{
				return this._LabelsPrinted;
			}
			set
			{
				this._LabelsPrinted = value;
			}
		}
		#endregion
		#region PickListPrinted
		public abstract class pickListPrinted : PX.Data.BQL.BqlBool.Field<pickListPrinted> { }
		protected Boolean? _PickListPrinted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Pick List Printed")]
		public virtual Boolean? PickListPrinted
		{
			get
			{
				return this._PickListPrinted;
			}
			set
			{
				this._PickListPrinted = value;
			}
		}
		#endregion
		#region ShippedViaCarrier
		public abstract class shippedViaCarrier : PX.Data.BQL.BqlBool.Field<shippedViaCarrier> { }
		protected Boolean? _ShippedViaCarrier;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? ShippedViaCarrier
		{
			get
			{
				return this._ShippedViaCarrier;
			}
			set
			{
				this._ShippedViaCarrier = value;
			}
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID>
		{
			public class PreventEditIfShipmentExists : PreventEditOf<ShipTerms.freightAmountSource>.On<ShipTermsMaint>
				.IfExists<Select<SOShipment, Where<SOShipment.shipTermsID, Equal<Current<ShipTerms.shipTermsID>>>>>
			{
				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object sh, string fld, string tbl, string foreignTbl)
				{
					return PXMessages.LocalizeFormat(Messages.ShipTermsUsedInShipment, ((SOShipment)sh).ShipmentNbr);
				}
			}
		}
		protected String _ShipTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(ShipTerms.shipTermsID), DescriptionField = typeof(ShipTerms.description), CacheGlobal = true)]
		public virtual String ShipTermsID
		{
			get
			{
				return this._ShipTermsID;
			}
			set
			{
				this._ShipTermsID = value;
			}
		}
		#endregion
		#region ShipZoneID
		public abstract class shipZoneID : PX.Data.BQL.BqlString.Field<shipZoneID> { }
		protected String _ShipZoneID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone ID")]
		[PXSelector(typeof(ShippingZone.zoneID), DescriptionField = typeof(ShippingZone.description), CacheGlobal = true)]
		public virtual String ShipZoneID
		{
			get
			{
				return this._ShipZoneID;
			}
			set
			{
				this._ShipZoneID = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Line Total")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Freight Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<GL.Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryFreightCost
		public abstract class curyFreightCost : PX.Data.BQL.BqlDecimal.Field<curyFreightCost> { }
		protected Decimal? _CuryFreightCost;
		[PXDBCurrency(typeof(SOShipment.curyInfoID), typeof(SOShipment.freightCost))]
		[PXUIField(DisplayName = "Freight Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightCost
		{
			get
			{
				return this._CuryFreightCost;
			}
			set
			{
				this._CuryFreightCost = value;
			}
		}
		#endregion
		#region FreightCost
		public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
		protected Decimal? _FreightCost;
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Freight Cost", Enabled = false)]
		public virtual Decimal? FreightCost
		{
			get
			{
				return this._FreightCost;
			}
			set
			{
				this._FreightCost = value;
			}
		}
		#endregion
		#region OverrideFreightAmount
		public abstract class overrideFreightAmount : PX.Data.BQL.BqlBool.Field<overrideFreightAmount> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Freight Price")]
		public virtual bool? OverrideFreightAmount
		{
			get;
			set;
		}
		#endregion
		#region FreightAmountSource
		public abstract class freightAmountSource : PX.Data.BQL.BqlString.Field<freightAmountSource> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[FreightAmountSource]
		[PXUIField(DisplayName = "Invoice Freight Price Based On", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<SOShipment.overrideFreightAmount, Equal<True>>, FreightAmountSourceAttribute.shipmentBased,
			Case<Where<SOShipment.orderCntr, Equal<int0>, And<SOShipment.shipTermsID, IsNull>>, Null>>,
			IsNull<Selector<SOShipment.shipTermsID, ShipTerms.freightAmountSource>, Current<SOShipment.freightAmountSource>>>))]
		public virtual string FreightAmountSource
		{
			get;
			set;
		}
		#endregion
		#region CuryFreightAmt
		public abstract class curyFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyFreightAmt> { }
		protected Decimal? _CuryFreightAmt;
		[PXDBCurrency(typeof(SOShipment.curyInfoID), typeof(SOShipment.freightAmt))]
		[PXUIField(DisplayName = "Freight Price")]
		[PXUIVerify(typeof(Where<SOShipment.curyFreightAmt, GreaterEqual<decimal0>>), PXErrorLevel.Error, CS.Messages.Entry_GE, typeof(decimal0))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightAmt
		{
			get
			{
				return this._CuryFreightAmt;
			}
			set
			{
				this._CuryFreightAmt = value;
			}
		}
		#endregion
		#region FreightAmt
		public abstract class freightAmt : PX.Data.BQL.BqlDecimal.Field<freightAmt> { }
		protected Decimal? _FreightAmt;
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Freight Price", Enabled = false)]
		public virtual Decimal? FreightAmt
		{
			get
			{
				return this._FreightAmt;
			}
			set
			{
				this._FreightAmt = value;
			}
		}
		#endregion
		#region CuryPremiumFreightAmt
		public abstract class curyPremiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyPremiumFreightAmt> { }
		protected Decimal? _CuryPremiumFreightAmt;
		[PXDBCurrency(typeof(SOShipment.curyInfoID), typeof(SOShipment.premiumFreightAmt))]
		[PXUIField(DisplayName = "Premium Freight Price")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryPremiumFreightAmt
		{
			get
			{
				return this._CuryPremiumFreightAmt;
			}
			set
			{
				this._CuryPremiumFreightAmt = value;
			}
		}
		#endregion
		#region PremiumFreightAmt
		public abstract class premiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<premiumFreightAmt> { }
		protected Decimal? _PremiumFreightAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? PremiumFreightAmt
		{
			get
			{
				return this._PremiumFreightAmt;
			}
			set
			{
				this._PremiumFreightAmt = value;
			}
		}
		#endregion
		#region CuryTotalFreightAmt
		public abstract class curyTotalFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyTotalFreightAmt> { }
		protected Decimal? _CuryTotalFreightAmt;
		[PXDBCurrency(typeof(SOShipment.curyInfoID), typeof(SOShipment.totalFreightAmt))]
		[PXFormula(typeof(Add<SOShipment.curyPremiumFreightAmt, SOShipment.curyFreightAmt>))]
		[PXUIField(DisplayName = "Total Freight Price", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTotalFreightAmt
		{
			get
			{
				return this._CuryTotalFreightAmt;
			}
			set
			{
				this._CuryTotalFreightAmt = value;
			}
		}
		#endregion
		#region TotalFreightAmt
		public abstract class totalFreightAmt : PX.Data.BQL.BqlDecimal.Field<totalFreightAmt> { }
		protected Decimal? _TotalFreightAmt;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Total Freight Price")]
		public virtual Decimal? TotalFreightAmt
		{
			get
			{
				return this._TotalFreightAmt;
			}
			set
			{
				this._TotalFreightAmt = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TX.TaxCategory.taxCategoryID), DescriptionField = typeof(TX.TaxCategory.descr))]
		[PXDefault(typeof(Search<Carrier.taxCategoryID, Where<Carrier.carrierID, Equal<Current<SOShipment.shipVia>>>>), SearchOnDefault = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.SO, "{0}: {1} - {3}", new Type[] { typeof(SOShipment.shipmentType), typeof(SOShipment.shipmentNbr), typeof(SOShipment.customerID), typeof(Customer.acctName) },
		   new Type[] { typeof(SOShipment.shipVia) },
		   NumberFields = new Type[] { typeof(SOShipment.shipmentNbr) },
		   Line1Format = "{0:d}{1}{2}{3}", Line1Fields = new Type[] { typeof(SOShipment.shipDate), typeof(SOShipment.status), typeof(SOShipment.shipmentQty), typeof(SOShipment.shipVia) },
		   Line2Format = "{1}{2}{3}{4}", Line2Fields = new Type[] { typeof(SOShipment.shipAddressID), typeof(SOAddress.addressLine1), typeof(SOAddress.addressLine2), typeof(SOAddress.city), typeof(SOAddress.state) },
		   MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<SOShipment.customerID>>>),
		   SelectForFastIndexing = typeof(Select2<SOShipment, InnerJoin<Customer, On<SOShipment.customerID, Equal<Customer.bAccountID>>>>)
	   )]
		[PXNote(new Type[0], ShowInReferenceSelector = true, Selector = typeof(SOShipment.shipmentNbr))]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(typeof(SOSetup.holdShipments))]
		[PXUIField(DisplayName = "Hold")]
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
		#region Confirmed
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		protected Boolean? _Confirmed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Confirmed")]
		public virtual Boolean? Confirmed
		{
			get
			{
				return this._Confirmed;
			}
			set
			{
				this._Confirmed = value;
			}
		}
		#endregion
		#region ConfirmedToVerify
		public abstract class confirmedToVerify : PX.Data.BQL.BqlBool.Field<confirmedToVerify> { }
		[PXDBRestrictionBool(typeof(confirmed))]
		public virtual Boolean? ConfirmedToVerify
		{
			get;
			set;
		}
		#endregion


		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Picked
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Picked")]
		public virtual Boolean? Picked { get; set; }
		public abstract class picked : PX.Data.BQL.BqlBool.Field<picked> { }
		#endregion
		#region PickedViaWorksheet
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Picked via Worksheet")]
		public virtual Boolean? PickedViaWorksheet { get; set; }
		public abstract class pickedViaWorksheet : PX.Data.BQL.BqlBool.Field<pickedViaWorksheet> { }
		#endregion
		#region PickedQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Picked Qty.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? PickedQty { get; set; }
		public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
		#endregion
		#region PackedQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Packed Qty.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? PackedQty { get; set; }
		public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[SOShipmentStatus.List()]
		[PXDefault()]
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
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region OrderCntr
		public abstract class orderCntr : PX.Data.BQL.BqlInt.Field<orderCntr> { }
		protected Int32? _OrderCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? OrderCntr
		{
			get
			{
				return this._OrderCntr;
			}
			set
			{
				this._OrderCntr = value;
			}
		}
		#endregion
		#region BilledOrderCntr
		public abstract class billedOrderCntr : PX.Data.BQL.BqlInt.Field<billedOrderCntr> { }
		protected Int32? _BilledOrderCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? BilledOrderCntr
		{
			get
			{
				return this._BilledOrderCntr;
			}
			set
			{
				this._BilledOrderCntr = value;
			}
		}
		#endregion
		#region UnbilledOrderCntr
		public abstract class unbilledOrderCntr : PX.Data.BQL.BqlInt.Field<unbilledOrderCntr> { }
		protected Int32? _UnbilledOrderCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? UnbilledOrderCntr
		{
			get
			{
				return this._UnbilledOrderCntr;
			}
			set
			{
				this._UnbilledOrderCntr = value;
			}
		}
		#endregion
		#region ReleasedOrderCntr
		public abstract class releasedOrderCntr : PX.Data.BQL.BqlInt.Field<releasedOrderCntr> { }
		protected Int32? _ReleasedOrderCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ReleasedOrderCntr
		{
			get
			{
				return this._ReleasedOrderCntr;
			}
			set
			{
				this._ReleasedOrderCntr = value;
			}
		}
		#endregion
		#region ControlQty
		public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }
		protected Decimal? _ControlQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Control Quantity")]
		public virtual Decimal? ControlQty
		{
			get
			{
				return this._ControlQty;
			}
			set
			{
				this._ControlQty = value;
			}
		}
		#endregion
		#region ShipmentQty
		public abstract class shipmentQty : PX.Data.BQL.BqlDecimal.Field<shipmentQty> { }
		protected Decimal? _ShipmentQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Quantity", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? ShipmentQty
		{
			get
			{
				return this._ShipmentQty;
			}
			set
			{
				this._ShipmentQty = value;
			}
		}
		#endregion
		#region ShipmentWeight
		public abstract class shipmentWeight : PX.Data.BQL.BqlDecimal.Field<shipmentWeight> { }
		protected Decimal? _ShipmentWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Weight", Enabled = false)]
		public virtual Decimal? ShipmentWeight
		{
			get
			{
				return this._ShipmentWeight;
			}
			set
			{
				this._ShipmentWeight = value;
			}
		}
		public virtual Decimal? OrderWeight
		{
			get
			{
				return this._ShipmentWeight;
			}
			set
			{
				this._ShipmentWeight = value;
			}
		}
		#endregion
		#region ShipmentVolume
		public abstract class shipmentVolume : PX.Data.BQL.BqlDecimal.Field<shipmentVolume> { }
		protected Decimal? _ShipmentVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Volume", Enabled = false)]
		public virtual Decimal? ShipmentVolume
		{
			get
			{
				return this._ShipmentVolume;
			}
			set
			{
				this._ShipmentVolume = value;
			}
		}
		public virtual Decimal? OrderVolume
		{
			get
			{
				return this._ShipmentVolume;
			}
			set
			{
				this._ShipmentVolume = value;
			}
		}
		#endregion
		#region PackageLineCntr
		public abstract class packageLineCntr : PX.Data.BQL.BqlInt.Field<packageLineCntr> { }
		protected Int32? _PackageLineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? PackageLineCntr
		{
			get
			{
				return this._PackageLineCntr;
			}
			set
			{
				this._PackageLineCntr = value;
			}
		}
		#endregion
		#region PackageWeight
		public abstract class packageWeight : PX.Data.BQL.BqlDecimal.Field<packageWeight> { }
		protected Decimal? _PackageWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Package Weight", Enabled = false)]
		public virtual Decimal? PackageWeight
		{
			get
			{
				return this._PackageWeight;
			}
			set
			{
				this._PackageWeight = value;
			}
		}
		#endregion
		#region IsPackageValid
		public abstract class isPackageValid : PX.Data.BQL.BqlBool.Field<isPackageValid> { }
		protected Boolean? _IsPackageValid;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsPackageValid
		{
			get
			{
				return this._IsPackageValid;
			}
			set
			{
				this._IsPackageValid = value;
			}
		}
		#endregion
		#region RecalcPackagesReason
		public abstract class recalcPackagesReason : PX.Data.IBqlField
		{
			public const int None = 0;
			public const int ShipVia = 1;
			public const int ShipLine = 2;
		}
		[PXInt()]
		[PXDefault(recalcPackagesReason.None, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? RecalcPackagesReason
		{
			get;
			set;
		}
		#endregion
		#region PackageCount
		public abstract class packageCount : PX.Data.BQL.BqlInt.Field<packageCount> { }
		protected int? _PackageCount;
		[PXInt]
		[PXUIField(DisplayName = "Packages", Enabled = false)]
		public virtual int? PackageCount
		{
			get
			{
				return this._PackageCount;
			}
			set
			{
				this._PackageCount = value;
			}
		}
		#endregion
		#region IsManualPackage
		public abstract class isManualPackage : PX.Data.BQL.BqlBool.Field<isManualPackage> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Manual Packaging")]
		public virtual bool? IsManualPackage
		{
			get;
			set;
		}
		#endregion
		#region IsPackageContentDeleted
		public abstract class isPackageContentDeleted : PX.Data.BQL.BqlBool.Field<isPackageContentDeleted> { }

		[PXBool]
		public virtual bool? IsPackageContentDeleted
		{
			get;
			set;
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
		#region Hidden
		public abstract class hidden : PX.Data.BQL.BqlBool.Field<hidden> { }
		protected Boolean? _Hidden = false;
		[PXBool()]
		public virtual Boolean? Hidden
		{
			get
			{
				return this._Hidden;
			}
			set
			{
				this._Hidden = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		[PX.TM.PXCompanyTreeSelector]
		[PXFormula(typeof(Selector<SOShipment.customerID, Selector<Customer.workgroupID, TM.EPCompanyTree.description>>))]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
			#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid()]
		[PX.TM.PXOwnerSelector(typeof(SOShipment.workgroupID))]
        [PXFormula(typeof(Selector<SOShipment.customerID, Selector<Customer.ownerID, PX.TM.PXOwnerSelectorAttribute.EPEmployee.acctCD>>))]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
    #region FreeItemQtyTot
    public abstract class freeItemQtyTot : PX.Data.BQL.BqlDecimal.Field<freeItemQtyTot> { }
    protected Decimal? _FreeItemQtyTot;
    [PXDBQuantity]
    [PXUIField(DisplayName = "Free Items Quantity Total")]
    public virtual Decimal? FreeItemQtyTot
    {
        get
        {
            return this._FreeItemQtyTot;
        }
        set
        {
            this._FreeItemQtyTot = value;
        }
    }
    #endregion
		#region SiteID_INSite_descr
		public abstract class siteID_INSite_descr : PX.Data.BQL.BqlString.Field<siteID_INSite_descr> { }
		#endregion
		#region ShipVia_Carrier_description
		public abstract class shipVia_Carrier_description : PX.Data.BQL.BqlString.Field<shipVia_Carrier_description> { }
		#endregion
		#region StatusIsNull
		public abstract class statusIsNull : PX.Data.BQL.BqlString.Field<statusIsNull> { }
		protected string _StatusIsNull;
		[PXString()]
		[PXDBCalced(typeof(Switch<Case<Where<SOShipment.status, IsNull>, SOShipmentStatus.autoGenerated>, SOShipment.status>), typeof(string))]
		[SOShipmentStatus.List()]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual string StatusIsNull
		{
			get
			{
				return this._StatusIsNull;
			}
			set
			{
				this._StatusIsNull = value;
			}
		}
		#endregion
		#region BillSeparately
		public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately>
        {
		}
		/// <summary>
		/// When set to <c>true</c> indicates that the shipment should be invoiced by a separate invoice.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Bill Separately")]
		public virtual bool? BillSeparately
		{
			get;
			set;
		}
		#endregion
		#region ConfirmedByID
		public abstract class confirmedByID : PX.Data.BQL.BqlGuid.Field<confirmedByID> { }
		[DBConditionalModifiedByID(typeof(confirmed), true)]
		[PXSelector(typeof(Search<Users.pKID>), new Type[] { typeof(Users.displayName) }, DescriptionField = typeof(Users.displayName), Headers = new string[] { "Confirmed By" } )]
		[PXUIField(DisplayName = "Confirmed By", Enabled = false, IsReadOnly = true)]
		public virtual Guid? ConfirmedByID
		{
			get;
			set;
		}
		#endregion
		#region ConfirmedDateTime
		public abstract class confirmedDateTime : PX.Data.BQL.BqlDateTime.Field<confirmedDateTime> { }
		[DBConditionalModifiedDateTime(typeof(confirmed), true)]
		[PXUIField(DisplayName = "Confirmed On", Enabled = false, IsReadOnly = true)]
		public virtual DateTime? ConfirmedDateTime
		{
			get;
			set;
		}
		#endregion

		#region CurrentWorksheetNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<SOPickingWorksheet.worksheetNbr>))]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXUIVisible(typeof(currentWorksheetNbr.IsNotNull))]
		[PXForeignReference(typeof(FK.Worksheet), Data.ReferentialIntegrity.ReferenceBehavior.SetNull)]
		public virtual String CurrentWorksheetNbr { get; set; }
		public abstract class currentWorksheetNbr : PX.Data.BQL.BqlString.Field<currentWorksheetNbr> { }
		#endregion

		#region Methods
		public static implicit operator SOShipment(PXResult<POReceipt, SOOrderShipment, SOOrder> item)
		{
			SOShipment ret = new SOShipment();
			ret.ShipmentNbr = ((POReceipt)item).ReceiptNbr;
			ret.ShipmentType = INDocType.DropShip;
			ret.CustomerID = ((SOOrder)item).CustomerID;
			ret.CustomerLocationID = ((SOOrder)item).CustomerLocationID;
			ret.CustomerOrderNbr = ((SOOrder)item).CustomerOrderNbr;
			ret.Confirmed = true;
			ret.Hold = false;
			ret.CreatedByID = ((POReceipt)item).CreatedByID;
			ret.CreatedByScreenID = ((POReceipt)item).CreatedByScreenID;
			ret.CreatedDateTime = ((POReceipt)item).CreatedDateTime;
			ret.LastModifiedByID = ((POReceipt)item).LastModifiedByID;
			ret.LastModifiedByScreenID = ((POReceipt)item).LastModifiedByScreenID;
			ret.LastModifiedDateTime = ((POReceipt)item).LastModifiedDateTime;
			ret.ShipDate = ((POReceipt)item).ReceiptDate;
			ret.ShipmentQty = ((POReceipt)item).OrderQty;
			ret.ShipmentVolume = ((POReceipt)item).ReceiptVolume;
			ret.ShipmentWeight = ((POReceipt)item).ReceiptWeight;
			ret.Status = SOShipmentStatus.Receipted;
			ret.WorkgroupID = ((POReceipt)item).WorkgroupID;
			ret.OwnerID = ((POReceipt)item).OwnerID;
			ret.SiteID = null;
			ret.ShipVia = null;
			ret.FOBPoint = null;

			return ret;
		}
		#endregion
	}

	public class SOShipmentStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Open, Messages.Open),
					Pair(Hold, Messages.Hold),
					Pair(Completed, Messages.Completed),
					Pair(Cancelled, Messages.Cancelled),
					Pair(Confirmed, Messages.Confirmed),
					Pair(Invoiced, Messages.Invoiced),
					Pair(PartiallyInvoiced, Messages.PartiallyInvoiced),
					Pair(Receipted, Messages.Receipted),
					Pair(AutoGenerated, Messages.AutoGenerated),
				}) {}
		}

		public const string Open = "N";
		public const string Hold = "H";
		public const string Completed = "C";
		public const string Cancelled = "L";
		public const string Confirmed = "F";
		public const string Invoiced = "I";
		public const string Receipted = "R";
		public const string AutoGenerated = "A";
		public const string PartiallyInvoiced = "Y";

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { ;}
		}

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { ;}
		}

		public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
		{
			public cancelled() : base(Cancelled) { ;}
		}

		public class confirmed : PX.Data.BQL.BqlString.Constant<confirmed>
		{
			public confirmed() : base(Confirmed) { ;}
		}

		public class invoiced : PX.Data.BQL.BqlString.Constant<invoiced>
		{
			public invoiced() : base(Invoiced) { ;}
		}

		public class partiallyInvoiced : PX.Data.BQL.BqlString.Constant<partiallyInvoiced>
		{
			public partiallyInvoiced() : base(PartiallyInvoiced) { ;}
		}

		public class receipted : PX.Data.BQL.BqlString.Constant<receipted>
		{
			public receipted() : base(Receipted) { ;}
		}

		public class autoGenerated : PX.Data.BQL.BqlString.Constant<autoGenerated>
		{
			public autoGenerated() : base(AutoGenerated) { ;}
		}
	}
}