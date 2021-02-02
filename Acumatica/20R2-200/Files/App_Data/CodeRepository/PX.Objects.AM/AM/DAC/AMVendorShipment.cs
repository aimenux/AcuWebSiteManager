using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.CR;
using System;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.EP;
using PX.Objects.CM;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.VendorShipment)]
    [System.Diagnostics.DebuggerDisplay("ShipmentNbr = {ShipmentNbr}, Status = {Status}")]
    public class AMVendorShipment : IBqlTable, INotable
    {
        public class PK : PrimaryKeyOf<AMVendorShipment>.By<shipmentNbr>
        {
            public static AMVendorShipment Find(PXGraph graph, string shipmentNbr) => FindBy(graph, shipmentNbr);
        }

        public static class FK
        {
            public class Vendor : BAccount.PK.ForeignKeyOf<AMVendorShipment>.By<vendorID> { }
            public class ShipAddress : AMVendorShipmentAddress.PK.ForeignKeyOf<AMVendorShipment>.By<shipAddressID> { }
            public class ShipContact : AMVendorShipmentContact.PK.ForeignKeyOf<AMVendorShipment>.By<shipContactID> { }
            public class Site : INSite.PK.ForeignKeyOf<AMVendorShipment>.By<siteID> { }
            public class ShipTerms : PX.Objects.CS.ShipTerms.PK.ForeignKeyOf<AMVendorShipment>.By<shipTermsID> { }
            public class ShipZone : PX.Objects.CS.ShippingZone.PK.ForeignKeyOf<AMVendorShipment>.By<shipZoneID> { }
            public class Carrier : PX.Objects.CS.Carrier.PK.ForeignKeyOf<AMVendorShipment>.By<shipVia> { }
        }

        #region ShipmentNbr
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        protected string _ShipmentNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Shipment ID", Visibility = PXUIVisibility.SelectorVisible)]
        [AutoNumber(typeof(AMPSetup.vendorShipmentNumberingID), typeof(AMVendorShipment.shipmentDate))]
        [PXSelector(typeof(Search<AMVendorShipment.shipmentNbr>))]
        [PX.Data.EP.PXFieldDescription]
        public virtual string ShipmentNbr
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
        #region ShipmentType
        public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
        protected String _ShipmentType;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(AMShipType.Shipment)]
        [AMShipType.List()]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        protected String _Status;
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [VendorShipmentStatus.List]
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
        [PXDBBool]
        [PXDefault(true)]
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
        #region ShipmentDate
        public abstract class shipmentDate : PX.Data.BQL.BqlDateTime.Field<shipmentDate> { }
        protected DateTime? _ShipmentDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Shipment Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ShipmentDate
        {
            get
            {
                return this._ShipmentDate;
            }
            set
            {
                this._ShipmentDate = value;
            }
        }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        [PX.Objects.GL.Branch(typeof(Coalesce<
            Search<Location.vBranchID, Where<Location.bAccountID, Equal<Current<AMVendorShipment.vendorID>>, And<Location.locationID, Equal<Current<AMVendorShipment.vendorLocationID>>>>>,
            Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
        [PXDefault]
        [PXForeignReference(typeof(FK.Vendor))]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region VendorLocationID
        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        protected Int32? _VendorLocationID;
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<AMVendorShipment.vendorID>>,
            And<Location.isActive, Equal<True>,
            And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<AMVendorShipment.vendorID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.vBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<AMVendorShipment.vendorID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
        [PXFormula(typeof(Default<AMVendorShipment.vendorID>))]
        [PXForeignReference(typeof(Field<vendorLocationID>.IsRelatedTo<Location.locationID>))]
        public virtual Int32? VendorLocationID
        {
            get
            {
                return this._VendorLocationID;
            }
            set
            {
                this._VendorLocationID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;

        [Site(DescriptionField = typeof(INSite.descr))]
        [PXDefault((object)null, typeof(Coalesce<Search<Location.vSiteID, Where<Current<AMVendorShipment.shipDestType>, Equal<POShippingDestination.site>,
                                                    And<Location.bAccountID, Equal<Current<AMVendorShipment.vendorID>>,
                                                    And<Location.locationID, Equal<Current<AMVendorShipment.vendorLocationID>>>>>>,
                                        Search<INSite.siteID, Where<Current<AMVendorShipment.shipDestType>, Equal<POShippingDestination.site>,
                                                    And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>>>))]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
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
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
        protected int? _WorkgroupID;
        [PXDBInt]
        [PX.TM.PXCompanyTreeSelector]
        [PXFormula(typeof(Selector<AMVendorShipment.vendorID, Selector<Vendor.workgroupID, PX.TM.EPCompanyTree.description>>))]
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
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        protected Int32? _EmployeeID;
        [PXDBInt()]
        [PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSubordinateSelector]
        [PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Int32? EmployeeID
        {
            get
            {
                return this._EmployeeID;
            }
            set
            {
                this._EmployeeID = value;
            }
        }
        #endregion
        #region ShipAddressID
        public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
        protected Int32? _ShipAddressID;
        [PXDBInt()]
        [AMVendorShipmentAddress(typeof(Select2<Address,
                    InnerJoin<CRLocation, On<Address.bAccountID, Equal<CRLocation.bAccountID>,
                        And<Address.addressID, Equal<CRLocation.defAddressID>,
                        And<Current<AMVendorShipment.shipDestType>, NotEqual<POShippingDestination.site>,
                        And<CRLocation.bAccountID, Equal<Current<AMVendorShipment.vendorID>>,
                        And<CRLocation.locationID, Equal<Current<AMVendorShipment.vendorLocationID>>>>>>>,
                    LeftJoin<AMVendorShipmentAddress, On<AMVendorShipmentAddress.bAccountID, Equal<Address.bAccountID>,
                        And<AMVendorShipmentAddress.bAccountAddressID, Equal<Address.addressID>,
                        And<AMVendorShipmentAddress.revisionID, Equal<Address.revisionID>,
                        And<AMVendorShipmentAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
                    Where<True, Equal<True>>>))]
        [PXUIField()]
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
        [AMVendorShipmentContact(typeof(Select2<Contact,
                    InnerJoin<CRLocation, On<Contact.bAccountID, Equal<CRLocation.bAccountID>,
                        And<Contact.contactID, Equal<CRLocation.defContactID>,
                        And<Current<AMVendorShipment.shipDestType>, NotEqual<POShippingDestination.site>,
                        And<CRLocation.bAccountID, Equal<Current<AMVendorShipment.vendorID>>,
                        And<CRLocation.locationID, Equal<Current<AMVendorShipment.vendorLocationID>>>>>>>,
                    LeftJoin<AMVendorShipmentContact, On<AMVendorShipmentContact.bAccountID, Equal<Contact.bAccountID>,
                        And<AMVendorShipmentContact.bAccountContactID, Equal<Contact.contactID>,
                        And<AMVendorShipmentContact.revisionID, Equal<Contact.revisionID>,
                        And<AMVendorShipmentContact.isDefaultContact, Equal<boolTrue>>>>>>>,
                    Where<True, Equal<True>>>))]
        [PXUIField()]
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
        #region ShipDestType
        public abstract class shipDestType : PX.Data.BQL.BqlString.Field<shipDestType> { }
        protected String _ShipDestType;
        [PXDBString(1, IsFixed = true)]
        [POShippingDestination.List()]
        [PXDefault(POShippingDestination.Vendor)]
        //[PXFormula(typeof(Switch<            
        //    Case<Where<Selector<AMVendorShipment.vendorLocationID, Location.vSiteID>, IsNotNull>, POShippingDestination.site>,
        //    POShippingDestination.company>))]
        [PXUIField(DisplayName = "Shipping Destination Type")]
        public virtual String ShipDestType
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
        #region ControlQty
        public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }
        protected Decimal? _ControlQty;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Quantity")]
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
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released", Visible = false, Enabled = false)]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByte.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp]
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
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
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
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
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
        #region BatNbr
        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }
        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Batch Nbr", Visibility = PXUIVisibility.Invisible)]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
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
        #region ShipTermsID
        public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID>
        {
            public class PreventEditIfShipmentExists : PreventEditOf<ShipTerms.freightAmountSource>.On<ShipTermsMaint>
                .IfExists<Select<AMVendorShipment, Where<AMVendorShipment.shipTermsID, Equal<Current<ShipTerms.shipTermsID>>>>>
            {
                protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object sh, string fld, string tbl, string foreignTbl)
                {
                    return PXMessages.LocalizeFormat(PX.Objects.SO.Messages.ShipTermsUsedInShipment, ((AMVendorShipment)sh).ShipmentNbr);
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
        #region Residential
        public abstract class residential : PX.Data.BQL.BqlBool.Field<residential> { }
        protected Boolean? _Residential;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Residential Delivery")]
        public virtual Boolean? Residential
        {
            get
            {
                return this._Residential;
            }
            set
            {
                this._Residential = value;
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
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        protected String _CuryID;
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Freight Currency", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<PX.Objects.GL.Company.baseCuryID>))]
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
        #region CuryFreightCost
        public abstract class curyFreightCost : PX.Data.BQL.BqlDecimal.Field<curyFreightCost> { }
        protected Decimal? _CuryFreightCost;
        [PXDBCurrency(typeof(AMVendorShipment.curyInfoID), typeof(AMVendorShipment.freightCost))]
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
        #region FreightAmountSource
        public abstract class freightAmountSource : PX.Data.BQL.BqlString.Field<freightAmountSource> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FreightAmountSource]
        [PXUIField(DisplayName = "Invoice Freight Price Based On", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMVendorShipment.overrideFreightAmount, Equal<True>>, FreightAmountSourceAttribute.shipmentBased,
            Case<Where<AMVendorShipment.orderCntr, Equal<int0>, And<AMVendorShipment.shipTermsID, IsNull>>, Null>>,
            IsNull<Selector<AMVendorShipment.shipTermsID, ShipTerms.freightAmountSource>, Current<AMVendorShipment.freightAmountSource>>>))]
        public virtual string FreightAmountSource
        {
            get;
            set;
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
        #region CuryFreightAmt
        public abstract class curyFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyFreightAmt> { }
        protected Decimal? _CuryFreightAmt;
        [PXDBCurrency(typeof(AMVendorShipment.curyInfoID), typeof(AMVendorShipment.freightAmt))]
        [PXUIField(DisplayName = "Freight Price")]
        [PXUIVerify(typeof(Where<AMVendorShipment.curyFreightAmt, GreaterEqual<decimal0>>), PXErrorLevel.Error, PX.Objects.CS.Messages.Entry_GE, typeof(decimal0))]
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
    }
}