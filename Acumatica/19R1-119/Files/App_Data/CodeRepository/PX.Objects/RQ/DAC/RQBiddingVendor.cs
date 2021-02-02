using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.PO;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.RQ
{
    [Serializable]
	[PXCacheName(Messages.RQBiddingVendor)]
	public partial class RQBiddingVendor : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<RQBiddingVendor>.By<lineID>
		{
			public static RQBiddingVendor Find(PXGraph graph, int? lineNbr) => FindBy(graph, lineNbr);
		}
		public static class FK
		{
			public class Requisition : RQRequisition.PK.ForeignKeyOf<RQBiddingVendor>.By<reqNbr> { }
			public class PORemitContact : PO.PORemitContact.PK.ForeignKeyOf<RQBiddingVendor>.By<remitContactID> { }
			public class PORemitAddress : PO.PORemitAddress.PK.ForeignKeyOf<RQBiddingVendor>.By<remitAddressID> { }
			public class FOBPoint : CS.FOBPoint.PK.ForeignKeyOf<RQBiddingVendor>.By<fOBPoint> { }
		}
		#endregion

		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }
		protected int? _LineID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? LineID
		{
			get
			{
				return this._LineID;
			}
			set
			{
				this._LineID = value;
			}
		}
		#endregion
		#region ReqNbr
		public abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		protected String _ReqNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXDBDefault(typeof(RQRequisition.reqNbr))]
		[PXParent(typeof(FK.Requisition))]
		public virtual String ReqNbr
		{
			get
			{
				return this._ReqNbr;
			}
			set
			{
				this._ReqNbr = value;
			}
		}
		#endregion		
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDefault]
		[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		/*[PXDefault(typeof(Search2<Vendor.defLocationID,
			LeftJoin<RQBiddingVendor,
			On<RQBiddingVendor.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>,
			And<RQBiddingVendor.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
			And<RQBiddingVendor.vendorLocationID, Equal<Vendor.defLocationID>>>>>,
			Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>,
			And<RQBiddingVendor.reqNbr, IsNull>>>))]*/
		[PXDefaultValidate(
			typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>),
			typeof(Search<RQBiddingVendor.reqNbr,
			Where<RQBiddingVendor.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>,			
			And<RQBiddingVendor.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
			And<RQBiddingVendor.vendorLocationID, Equal<Required<RQBiddingVendor.vendorLocationID>>>>>>))]
		[PXFormula(typeof(Default<RQBiddingVendor.vendorID>))]
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
		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;
		[PXDBInt()]
		[PORemitAddress(typeof(Select2<BAccount2,
			InnerJoin<Location, On<Location.bAccountID, Equal<BAccount2.bAccountID>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<Location.bAccountID>, And<Address.addressID, Equal<Location.defAddressID>>>,
			LeftJoin<PORemitAddress, On<PORemitAddress.bAccountID, Equal<Address.bAccountID>,
						And<PORemitAddress.bAccountAddressID, Equal<Address.addressID>,
				And<PORemitAddress.revisionID, Equal<Address.revisionID>, And<PORemitAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
			Where<Location.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>, 
			  And<Location.locationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>), Required = false)]
		[PXUIField()]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		protected Int32? _RemitContactID;
		[PXDBInt()]
		[PORemitContactAttribute(typeof(Select2<Location,
				InnerJoin<Contact, On<Contact.bAccountID, Equal<Location.bAccountID>, And<Contact.contactID, Equal<Location.defContactID>>>,
				LeftJoin<PORemitContact, On<PORemitContact.bAccountID, Equal<Contact.bAccountID>,
				And<PORemitContact.bAccountContactID, Equal<Contact.contactID>,
						And<PORemitContact.revisionID, Equal<Contact.revisionID>,
				And<PORemitContact.isDefaultContact, Equal<boolTrue>>>>>>>,
				Where<Location.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>, 
					And<Location.locationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>), Required = false)]
		public virtual Int32? RemitContactID
		{
			get
			{
				return this._RemitContactID;
			}
			set
			{
				this._RemitContactID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXString(5)]
		[PXDefault(typeof(
			Coalesce<
			Search<Vendor.curyID, Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>,
			Search<PX.Objects.GL.Company.baseCuryID>>),
			PersistingCheck = PXPersistingCheck.Nothing)]		
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		[CurrencyInfo(ModuleCode = GL.BatchModule.PO)]
		[PXUIField(DisplayName = "Currency")]
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
		#region EntryDate
		public abstract class entryDate : PX.Data.BQL.BqlDateTime.Field<entryDate> { }
		protected DateTime? _EntryDate;
		[PXDefault()]
		[PXDBDate()]
		[PXUIField(DisplayName = "Entry Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? EntryDate
		{
			get
			{
				return this._EntryDate;
			}
			set
			{
				this._EntryDate = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;

		[PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region PromisedDate
		public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
		protected DateTime? _PromisedDate;

		[PXDBDate()]
		[PXUIField(DisplayName = "Promised Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? PromisedDate
		{
			get
			{
				return this._PromisedDate;
			}
			set
			{
				this._PromisedDate = value;
			}
		}
		#endregion
		#region FOBPoint
		public abstract class fOBPoint : PX.Data.BQL.BqlString.Field<fOBPoint> { }
		protected String _FOBPoint;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(Search<FOBPoint.fOBPointID>), DescriptionField = typeof(FOBPoint.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.vFOBPointID,
								 Where<Location.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXSelector(typeof(Search<Carrier.carrierID>), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.vCarrierID,
							Where<Location.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>,
										And<Location.locationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region TotalQuoteQty
		public abstract class totalQuoteQty : PX.Data.BQL.BqlDecimal.Field<totalQuoteQty> { }
		protected Decimal? _TotalQuoteQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Bid Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? TotalQuoteQty
		{
			get
			{
				return this._TotalQuoteQty;
			}
			set
			{
				this._TotalQuoteQty = value;
			}
		}
		#endregion
		#region CuryTotalQuoteExtCost
		public abstract class curyTotalQuoteExtCost : PX.Data.BQL.BqlDecimal.Field<curyTotalQuoteExtCost> { }
		protected Decimal? _CuryTotalQuoteExtCost;
		[PXDBCurrency(typeof(RQBiddingVendor.curyInfoID), typeof(RQBiddingVendor.totalQuoteExtCost))]
		[PXUIField(DisplayName = "Total Extended Cost", Enabled = false)]				
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTotalQuoteExtCost
		{
			get
			{
				return this._CuryTotalQuoteExtCost;
			}
			set
			{
				this._CuryTotalQuoteExtCost = value;
			}
		}
		#endregion
		#region TotalQuoteExtCost
		public abstract class totalQuoteExtCost : PX.Data.BQL.BqlDecimal.Field<totalQuoteExtCost> { }
		protected Decimal? _TotalQuoteExtCost;

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalQuoteExtCost
		{
			get
			{
				return this._TotalQuoteExtCost;
			}
			set
			{
				this._TotalQuoteExtCost = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlBool.Field<status> { }
		protected bool? _Status = false;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Sent")]
		public virtual bool? Status
		{
			get
			{
				return _Status;
			}
			set
			{
				_Status = value;
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

}
