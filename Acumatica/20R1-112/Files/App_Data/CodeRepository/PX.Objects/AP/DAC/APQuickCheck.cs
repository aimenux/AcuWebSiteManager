using System;
using PX.Data;
using PX.Data.EP;

using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.Common.Interfaces;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.Common.Attributes;
using PX.Objects.GL.Descriptor;

namespace PX.Objects.AP.Standalone
{
	[PXProjection(typeof(Select2<APRegister,
		InnerJoin<APInvoice, On<APInvoice.docType, Equal<APRegister.docType>,
			And<APInvoice.refNbr, Equal<APRegister.refNbr>>>,
		InnerJoin<APPayment, On<APPayment.docType, Equal<APRegister.docType>,
			And<APPayment.refNbr, Equal<APRegister.refNbr>>>>>,
		Where<APRegister.docType, Equal<APDocType.quickCheck>,
			Or<APRegister.docType, Equal<APDocType.voidQuickCheck>>>>), Persistent = true)]
	[PXSubstitute(GraphType = typeof(APQuickCheckEntry))]
	[PXPrimaryGraph(typeof(APQuickCheckEntry))]
	[Serializable]
	[PXEMailSource] 
	public class APQuickCheck : APRegister, IPrintCheckControlable, IAssign, IApprovable, IApprovalDescription
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(APRegister.docType))]
		[PXDefault(APDocType.QuickCheck)]
		[APQuickCheckType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFieldDescription]
		[PXDependsOnFields(typeof(APQuickCheck.aPInvoiceDocType), typeof(APQuickCheck.aPPaymentDocType))]
		public override String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(APRegister.refNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[APQuickCheckType.RefNbr(typeof(Search2<APQuickCheck.refNbr,
			InnerJoinSingleTable<Vendor, On<APQuickCheck.vendorID, Equal<Vendor.bAccountID>>>,
			Where<APQuickCheck.docType, Equal<Current<APQuickCheck.docType>>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>, OrderBy<Desc<APQuickCheck.refNbr>>>), Filterable = true)]
		[APQuickCheckType.Numbering()]
		[PXFieldDescription]
		[PXDependsOnFields(typeof(APQuickCheck.aPInvoiceRefNbr), typeof(APQuickCheck.aPPaymentRefNbr))]
		public override String RefNbr
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
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[VendorActive(
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName),
			CacheGlobal = true,
			Filterable = true,
			BqlField = typeof(APRegister.vendorID))]
		[PXDefault]
		public override int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region VendorLocationID
		public new abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		[LocationID(
			typeof(Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
				And<Location.isActive, Equal<True>,
				And<MatchWithBranch<Location.vBranchID>>>>),
			DescriptionField = typeof(Location.descr),
			Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(APRegister.vendorLocationID))]
		[PXDefault(typeof(Coalesce<
			Search2<Vendor.defLocationID,
			InnerJoin<CRLocation,
				On<CRLocation.locationID, Equal<Vendor.defLocationID>,
				And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
			Where<Vendor.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
				And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
		public override Int32? VendorLocationID
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

		#region SuppliedByVendorID
		public abstract class suppliedByVendorID : PX.Data.BQL.BqlInt.Field<suppliedByVendorID> { }

		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor that supplied the goods. 
		/// Always equals to VendorID in APQuickCheck.
		/// </value>
		[PXDBInt(BqlField = typeof(APInvoice.suppliedByVendorID))]
		[PXFormula(typeof(APQuickCheck.vendorID))]
		public virtual int? SuppliedByVendorID { get; set; }
		#endregion

		#region SuppliedByVendorLocationID
		public abstract class suppliedByVendorLocationID : PX.Data.BQL.BqlInt.Field<suppliedByVendorLocationID> { }

		/// <summary>
		/// Identifier of the <see cref="Location">Location</see> of the <see cref="Vendor">Supplied-by Vendor</see>, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Location.LocationID"/> field. Defaults to AP bill's <see cref="APInvoice.VendorLocationID">vendor location</see>.
		/// Always equals to VendorLocationID in APQuickCheck.
		/// </value>
		[PXDBInt(BqlField = typeof(APInvoice.suppliedByVendorLocationID))]
		[PXFormula(typeof(APQuickCheck.vendorLocationID))]
		public virtual int? SuppliedByVendorLocationID { get; set; }
		#endregion

		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;
		[PXDBInt(BqlField = typeof(APPayment.remitAddressID))]
		[APAddress(typeof(Select2<Location,
			InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
			InnerJoin<Address, On<Address.addressID, Equal<Location.remitAddressID>, And<Where<Address.bAccountID, Equal<Location.bAccountID>, Or<Address.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
			LeftJoin<APAddress, On<APAddress.vendorID, Equal<Address.bAccountID>, And<APAddress.vendorAddressID, Equal<Address.addressID>, And<APAddress.revisionID, Equal<Address.revisionID>, And<APAddress.isDefaultAddress, Equal<True>>>>>>>>,
			Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>, And<Location.locationID, Equal<Current<APQuickCheck.vendorLocationID>>>>>))]
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

		[PXSelector(typeof(APContact.contactID), ValidateValue = false)]    //Attribute for showing contact email field on Automatic Notifications screen in the list of availible emails for
																			//Quick Checks screen. Relies on the work of platform, which uses PXSelector to compose email list
		[PXUIField(DisplayName = "Remittance Contact", Visible = false)]    //Attribute for displaying user friendly contact email field on Automatic Notifications screen in the list of availible emails.
		[PXDBInt(BqlField = typeof(APPayment.remitContactID))]
		[APContact(typeof(Select2<Location,
			InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
			InnerJoin<Contact, On<Contact.contactID, Equal<Location.remitContactID>, And<Where<Contact.bAccountID, Equal<Location.bAccountID>, Or<Contact.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
			LeftJoin<APContact, On<APContact.vendorID, Equal<Contact.bAccountID>, And<APContact.vendorContactID, Equal<Contact.contactID>, And<APContact.revisionID, Equal<Contact.revisionID>, And<APContact.isDefaultContact, Equal<True>>>>>>>>,
			Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>, And<Location.locationID, Equal<Current<APQuickCheck.vendorLocationID>>>>>))]
		public virtual int? RemitContactID
		{
			get;
			set;
		}
		#endregion
		#region APAccountID
		public new abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
		[PXDefault(typeof(Search<Location.aPAccountID, 
			Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
				And<Location.locationID, Equal<Optional<APQuickCheck.vendorLocationID>>>>>))]
		[Account(typeof(APQuickCheck.branchID), typeof(Search<Account.accountID,
					Where2<Match<Current<AccessInfo.userName>>,
						 And<Account.active, Equal<True>,
						 And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						  Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AP Account", BqlField = typeof(APRegister.aPAccountID),
			ControlAccountForModule = ControlAccountModule.AP)]
		public override Int32? APAccountID
		{
			get
			{
				return this._APAccountID;
			}
			set
			{
				this._APAccountID = value;
			}
		}
		#endregion
		#region APSubID
		public new abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
		[PXDefault(typeof(Search<Location.aPSubID,
			Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
				And<Location.locationID, Equal<Optional<APQuickCheck.vendorLocationID>>>>>))]
		[SubAccount(typeof(APQuickCheck.aPAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Visible, BqlField = typeof(APRegister.aPSubID))]
		public override Int32? APSubID
		{
			get
			{
				return this._APSubID;
			}
			set
			{
				this._APSubID = value;
			}
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }

		[PXDBString(10, IsUnicode = true, BqlField = typeof(APInvoice.termsID))]
		[PXDefault(typeof(Search<Vendor.termsID, Where<Vendor.bAccountID, Equal<Current<APQuickCheck.vendorID>>>>))]
		[APTermsSelector]
		[Terms(typeof(APQuickCheck.docDate), null, null, typeof(APQuickCheck.curyDocBal), typeof(APQuickCheck.curyOrigDiscAmt))]
		[PXUIField(DisplayName = "Terms")]
		public virtual string TermsID
		{
			get;
			set;
		}
		#endregion
		#region LineCntr
		public new abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		[PXDBInt(BqlField = typeof(APRegister.lineCntr))]
		[PXDefault(0)]
		public override Int32? LineCntr
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
		#region AdjCntr
		public new abstract class adjCntr : PX.Data.BQL.BqlInt.Field<adjCntr> { }

		[PXDBInt(BqlField = typeof(APRegister.adjCntr))]
		[PXDefault(0)]
		public override int? AdjCntr
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong(BqlField = typeof(APRegister.curyInfoID))]
		[CurrencyInfo(ModuleCode = BatchModule.AP)]
		public override Int64? CuryInfoID
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
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.origDocAmt), BqlField = typeof(APRegister.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CuryOrigDocAmt
		{
			get
			{
				return this._CuryOrigDocAmt;
			}
			set
			{
				this._CuryOrigDocAmt = value;
			}
		}
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.origDocAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? OrigDocAmt
		{
			get
			{
				return this._OrigDocAmt;
			}
			set
			{
				this._OrigDocAmt = value;
			}
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.taxAmt), BqlField = typeof(APInvoice.curyTaxAmt))]
		[PXUIField(DisplayName = "Tax Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxAmt { get; set; }
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		[PXDBDecimal(4, BqlField = typeof(APInvoice.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxAmt { get; set; }
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.docBal), BaseCalc = false, BqlField = typeof(APRegister.curyDocBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override Decimal? CuryDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		#endregion
		#region DocBal
		public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.docBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		#endregion
		#region CuryOrigDiscAmt
		public new abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.origDiscAmt), BqlField = typeof(APRegister.curyOrigDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CuryOrigDiscAmt
		{
			get
			{
				return this._CuryOrigDiscAmt;
			}
			set
			{
				this._CuryOrigDiscAmt = value;
			}
		}
		#endregion
		#region OrigDiscAmt
		public new abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.origDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? OrigDiscAmt
		{
			get
			{
				return this._OrigDiscAmt;
			}
			set
			{
				this._OrigDiscAmt = value;
			}
		}
		#endregion
		#region CuryDiscTaken
		public new abstract class curyDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscTaken> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.discTaken), BqlField = typeof(APRegister.curyDiscTaken))]
		public override Decimal? CuryDiscTaken
		{
			get
			{
				return this._CuryDiscTaken;
			}
			set
			{
				this._CuryDiscTaken = value;
			}
		}
		#endregion
		#region DiscTaken
		public new abstract class discTaken : PX.Data.BQL.BqlDecimal.Field<discTaken> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.discTaken))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DiscTaken
		{
			get
			{
				return this._DiscTaken;
			}
			set
			{
				this._DiscTaken = value;
			}
		}
		#endregion
		#region CuryDiscBal
		public new abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.discBal), BaseCalc = false, BqlField = typeof(APRegister.curyDiscBal))]
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override Decimal? CuryDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		#endregion
		#region DiscBal
		public new abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.discBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		#endregion
		#region CuryOrigWhTaxAmt
		public new abstract class curyOrigWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigWhTaxAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.origWhTaxAmt), BqlField = typeof(APRegister.curyOrigWhTaxAmt))]
		[PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override Decimal? CuryOrigWhTaxAmt
		{
			get
			{
				return this._CuryOrigWhTaxAmt;
			}
			set
			{
				this._CuryOrigWhTaxAmt = value;
			}
		}
		#endregion
		#region OrigWhTaxAmt
		public new abstract class origWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<origWhTaxAmt> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.origWhTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? OrigWhTaxAmt
		{
			get
			{
				return this._OrigWhTaxAmt;
			}
			set
			{
				this._OrigWhTaxAmt = value;
			}
		}
		#endregion
		#region CuryWhTaxBal
		public new abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.whTaxBal), BaseCalc = false, BqlField = typeof(APRegister.curyWhTaxBal))]
		public override Decimal? CuryWhTaxBal
		{
			get
			{
				return this._CuryWhTaxBal;
			}
			set
			{
				this._CuryWhTaxBal = value;
			}
		}
		#endregion
		#region WhTaxBal
		public new abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.whTaxBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? WhTaxBal
		{
			get
			{
				return this._WhTaxBal;
			}
			set
			{
				this._WhTaxBal = value;
			}
		}
		#endregion
		#region CuryTaxWheld
		public new abstract class curyTaxWheld : PX.Data.BQL.BqlDecimal.Field<curyTaxWheld> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.taxWheld), BqlField = typeof(APRegister.curyTaxWheld))]
		public override Decimal? CuryTaxWheld
		{
			get
			{
				return this._CuryTaxWheld;
			}
			set
			{
				this._CuryTaxWheld = value;
			}
		}
		#endregion
		#region TaxWheld
		public new abstract class taxWheld : PX.Data.BQL.BqlDecimal.Field<taxWheld> { }
		[PXDBBaseCury(BqlField = typeof(APRegister.taxWheld))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? TaxWheld
		{
			get
			{
				return this._TaxWheld;
			}
			set
			{
				this._TaxWheld = value;
			}
		}
		#endregion
		#region DocDesc
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(APRegister.docDesc))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String DocDesc
		{
			get
			{
				return this._DocDesc;
			}
			set
			{
				this._DocDesc = value;
			}
		}
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID(BqlField = typeof(APRegister.createdByID))]
		public override Guid? CreatedByID
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
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID(BqlField = typeof(APRegister.createdByScreenID))]
		public override String CreatedByScreenID
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
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime(BqlField = typeof(APRegister.createdDateTime))]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public override DateTime? CreatedDateTime
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
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID(BqlField = typeof(APRegister.lastModifiedByID))]
		public override Guid? LastModifiedByID
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
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID(BqlField = typeof(APRegister.lastModifiedByScreenID))]
		public override String LastModifiedByScreenID
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
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime(BqlField = typeof(APRegister.lastModifiedDateTime))]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public override DateTime? LastModifiedDateTime
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
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(BqlField = typeof(APRegister.Tstamp))]
		public override Byte[] tstamp
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
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

		[PXDBString(15, IsUnicode = true, BqlField = typeof(APRegister.batchNbr))]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[BatchNbr(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAP>>>),
			IsMigratedRecordField = typeof(APQuickCheck.isMigratedRecord))]
		public override string BatchNbr
		{
			get;
			set;
		}
		#endregion
		#region PrebookBatchNbr
		public new abstract class prebookBatchNbr : PX.Data.BQL.BqlString.Field<prebookBatchNbr> { }
		[PXDBString(15, IsUnicode = true, BqlField = typeof(APRegister.prebookBatchNbr))]
		[PXUIField(DisplayName = "Pre-releasing Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Batch.batchNbr))]
		public override String PrebookBatchNbr
		{
			get
			{
				return this._PrebookBatchNbr;
			}
			set
			{
				this._PrebookBatchNbr = value;
			}
		}
		#endregion
		#region VoidBatchNbr
		public new abstract class voidBatchNbr : PX.Data.BQL.BqlString.Field<voidBatchNbr> { }
		[PXDBString(15, IsUnicode = true, BqlField = typeof(APRegister.voidBatchNbr))]
		[PXUIField(DisplayName = "Void Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Batch.batchNbr))]
		public override String VoidBatchNbr
		{
			get
			{
				return this._VoidBatchNbr;
			}
			set
			{
				this._VoidBatchNbr = value;
			}
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDefault(APDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[APDocStatus.List]
		[PXDBString(1, IsFixed = true, BqlField = typeof(APRegister.status))]
		[AP.APPayment.SetStatusCheck]
		[PXDependsOnFields(
			typeof(APQuickCheck.voided),
			typeof(APQuickCheck.hold),
			typeof(APQuickCheck.scheduled),
			typeof(APQuickCheck.released),
			typeof(APQuickCheck.printed),
			typeof(APQuickCheck.prebooked),
			typeof(APQuickCheck.openDoc),
			typeof(APQuickCheck.printCheck),
			typeof(APQuickCheck.approved),
			typeof(APQuickCheck.dontApprove),
			typeof(APQuickCheck.rejected),
			typeof(APQuickCheck.docType))]
		public override string Status
		{
			get { return _Status; }
			set { _Status = value; }
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool(BqlField = typeof(APRegister.released))]
		[PXDefault(false)]
		public override Boolean? Released
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
		#region OpenDoc
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		[PXDBBool(BqlField = typeof(APRegister.openDoc))]
		[PXDefault(true)]
		public override Boolean? OpenDoc
		{
			get
			{
				return this._OpenDoc;
			}
			set
			{
				this._OpenDoc = value;
			}
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		[PXDBBool(BqlField = typeof(APRegister.scheduled))]
		[PXDefault(false)]
		public override Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
			}
		}
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool(BqlField = typeof(APRegister.voided))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Void", Visible = false)]
		public override Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region Prebooked
		public new abstract class prebooked : PX.Data.BQL.BqlBool.Field<prebooked> { }
		[PXDBBool(BqlField = typeof(APRegister.prebooked))]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.Prebooked, Visible = false)]
		public override Boolean? Prebooked
		{
			get
			{
				return this._Prebooked;
			}
			set
			{
				this._Prebooked = value;
			}
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.AP, Messages.SearchableTitleDocument, new Type[] { typeof(APQuickCheck.docType), typeof(APQuickCheck.refNbr), typeof(APQuickCheck.vendorID), typeof(Vendor.acctName) },
			new Type[] { typeof(APQuickCheck.invoiceNbr), typeof(APQuickCheck.docDesc) },
			 NumberFields = new Type[] { typeof(APQuickCheck.refNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(APQuickCheck.docDate), typeof(APQuickCheck.status), typeof(APQuickCheck.invoiceNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(APQuickCheck.docDesc) },
			MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APQuickCheck.vendorID>>>),
			SelectForFastIndexing = typeof(Select2<APQuickCheck, InnerJoin<Vendor, On<APQuickCheck.vendorID, Equal<Vendor.bAccountID>>>>)
		)]
		[PXNote(BqlField = typeof(APRegister.noteID), DescriptionField = typeof(APQuickCheck.refNbr))]
		public override Guid? NoteID
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
		#region ClosedDate
		public new abstract class closedDate : PX.Data.BQL.BqlDateTime.Field<closedDate> { }
		[PXDBDate(BqlField = typeof(APRegister.closedDate))]
		[PXUIField(DisplayName = "Closed Date", Visibility = PXUIVisibility.Invisible)]
		public override DateTime? ClosedDate { get; set; }
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		[FinPeriodID(
		    branchSourceType: typeof(APQuickCheck.branchID),
		    masterFinPeriodIDType: typeof(APQuickCheck.closedTranPeriodID),
		    BqlField = typeof(APRegister.closedFinPeriodID))]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public override String ClosedFinPeriodID
		{
			get
			{
				return this._ClosedFinPeriodID;
			}
			set
			{
				this._ClosedFinPeriodID = value;
			}
		}
		#endregion
		#region ClosedTranPeriodID
		public new abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }
		[PeriodID(BqlField = typeof(APRegister.closedTranPeriodID))]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public override String ClosedTranPeriodID
		{
			get
			{
				return this._ClosedTranPeriodID;
			}
			set
			{
				this._ClosedTranPeriodID = value;
			}
		}
		#endregion
		#region RGOLAmt
		public new abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }
		[PXDBDecimal(4, BqlField = typeof(APRegister.rGOLAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		[PXDBString(15, IsUnicode = true, BqlField = typeof(APRegister.scheduleID))]
		public override string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region ImpRefNbr
		public new abstract class impRefNbr : PX.Data.BQL.BqlString.Field<impRefNbr> { }
		[PXDBString(15, IsUnicode = true, BqlField = typeof(APRegister.impRefNbr))]
		public override String ImpRefNbr
		{
			get
			{
				return this._ImpRefNbr;
			}
			set
			{
				this._ImpRefNbr = value;
			}
		}
		#endregion
		//APInvoice
		#region APInvoiceDocType
		public abstract class aPInvoiceDocType : PX.Data.BQL.BqlString.Field<aPInvoiceDocType> { }
		[PXDBString(3, IsFixed = true, BqlField = typeof(APInvoice.docType))]
		[PXDefault()]
		[PXRestriction()]
		public virtual String APInvoiceDocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region APInvoiceRefNbr
		public abstract class aPInvoiceRefNbr : PX.Data.BQL.BqlString.Field<aPInvoiceRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(APInvoice.refNbr))]
		[PXDefault()]
		[PXRestriction()]
		public virtual String APInvoiceRefNbr
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
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true, BqlField = typeof(APInvoice.invoiceNbr))]
		[PXUIField(DisplayName = "Vendor Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;
		[PXDBDate(BqlField = typeof(APInvoice.invoiceDate))]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Vendor Ref. Date", Visibility = PXUIVisibility.Invisible)]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(APInvoice.taxZoneID))]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXDefault(typeof(Search<Location.vTaxZoneID, Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>, And<Location.locationID, Equal<Current<APQuickCheck.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.taxTotal), BqlField = typeof(APInvoice.curyTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;
		[PXDBDecimal(4, BqlField = typeof(APInvoice.taxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.lineTotal), BqlField = typeof(APInvoice.curyLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBDecimal(4, BqlField = typeof(APInvoice.lineTotal))]
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

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
		protected Decimal? _CuryVatExemptTotal;
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.vatExemptTotal), BqlField = typeof(APInvoice.curyVatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal
		{
			get
			{
				return this._CuryVatExemptTotal;
			}
			set
			{
				this._CuryVatExemptTotal = value;
			}
		}
		#endregion

		#region VatExemptTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
		protected Decimal? _VatExemptTotal;
		[PXDBDecimal(4, BqlField = typeof(APInvoice.vatExemptTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal
		{
			get
			{
				return this._VatExemptTotal;
			}
			set
			{
				this._VatExemptTotal = value;
			}
		}
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
		protected Decimal? _CuryVatTaxableTotal;
		[PXDBCurrency(typeof(APQuickCheck.curyInfoID), typeof(APQuickCheck.vatTaxableTotal), BqlField = typeof(APInvoice.curyVatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal
		{
			get
			{
				return this._CuryVatTaxableTotal;
			}
			set
			{
				this._CuryVatTaxableTotal = value;
			}
		}
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
		protected Decimal? _VatTaxableTotal;
		[PXDBDecimal(4, BqlField = typeof(APInvoice.vatTaxableTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal
		{
			get
			{
				return this._VatTaxableTotal;
			}
			set
			{
				this._VatTaxableTotal = value;
			}
		}
		#endregion

		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		protected Boolean? _SeparateCheck;
		[PXDBBool(BqlField = typeof(APInvoice.separateCheck))]
		[PXDefault(true)]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region PaySel
		public abstract class paySel : PX.Data.BQL.BqlBool.Field<paySel> { }
		protected bool? _PaySel = false;
		[PXDBBool(BqlField = typeof(APInvoice.paySel))]
		[PXDefault(false)]
		public bool? PaySel
		{
			get
			{
				return _PaySel;
			}
			set
			{
				_PaySel = value;
			}
		}
		#endregion
		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.BQL.BqlInt.Field<prebookAcctID> { }
		protected Int32? _PrebookAcctID;

		[PXDefault(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APQuickCheck.vendorID>>>>), SourceField = typeof(Vendor.prebookAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), BqlField = typeof(APInvoice.prebookAcctID), AvoidControlAccounts = true)]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.BQL.BqlInt.Field<prebookSubID> { }
		protected Int32? _PrebookSubID;

		[PXDefault(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APQuickCheck.vendorID>>>>), SourceField = typeof(Vendor.prebookSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(APQuickCheck.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), BqlField = typeof(APInvoice.prebookSubID))]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
			}
		}
		#endregion
		//APPayment
		#region APPaymentDocType
		public abstract class aPPaymentDocType : PX.Data.BQL.BqlString.Field<aPPaymentDocType> { }
		[PXDBString(3, IsFixed = true, BqlField = typeof(APPayment.docType))]
		[PXRestriction()]
		[PXDefault()]
		public virtual String APPaymentDocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region APPaymentRefNbr
		public abstract class aPPaymentRefNbr : PX.Data.BQL.BqlString.Field<aPPaymentRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(APPayment.refNbr))]
		[PXRestriction()]
		[PXDefault()]
		public virtual String APPaymentRefNbr
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
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(APPayment.paymentMethodID))]
		[PXDefault(typeof(Search<Location.paymentMethodID, Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>, And<Location.locationID, Equal<Current<APQuickCheck.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>))]
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;

		[PXDefault(typeof(Coalesce<Search2<Location.cashAccountID,
										InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
											And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
										Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
										And<Location.locationID, Equal<Current<APQuickCheck.vendorLocationID>>,
										And<Location.vPaymentMethodID, Equal<Current<APQuickCheck.paymentMethodID>>>>>>,
								   Search2<PaymentMethodAccount.cashAccountID, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<APQuickCheck.paymentMethodID>>,
											And<CashAccount.branchID, Equal<Current<APQuickCheck.branchID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
											And<PaymentMethodAccount.aPIsDefault, Equal<True>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(APQuickCheck.branchID), typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<APQuickCheck.paymentMethodID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>),
								Visibility = PXUIVisibility.Visible, BqlField = typeof(APPayment.cashAccountID))]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(IsDetail = false, BqlField = typeof(APRegister.branchID))]
		public override Int32? BranchID
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
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(APRegister.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public override String CuryID
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
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(APPayment.extRefNbr))]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[PaymentRef(typeof(APQuickCheck.cashAccountID), typeof(APQuickCheck.paymentMethodID), typeof(APQuickCheck.stubCntr), Table = typeof(APPayment))]
		public virtual string ExtRefNbr { get; set; }
		#endregion
		#region AdjDate
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate(BqlField = typeof(APPayment.adjDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
		#region AdjFinPeriodID
		public abstract class adjFinPeriodID : PX.Data.BQL.BqlString.Field<adjFinPeriodID> { }
		protected String _AdjFinPeriodID;
		[APOpenPeriod(
			typeof(APQuickCheck.adjDate),
            masterFinPeriodIDType: typeof(APQuickCheck.adjTranPeriodID),
			selectionModeWithRestrictions: FinPeriodSelectorAttribute.SelectionModesWithRestrictions.All,
			sourceSpecificationTypes:
			new[]
			{
				typeof(CalendarOrganizationIDProvider.SourceSpecification<APQuickCheck.branchID, True>),
				typeof(CalendarOrganizationIDProvider.SourceSpecification<
					APQuickCheck.cashAccountID,
					Selector<APQuickCheck.cashAccountID, CashAccount.branchID>,
					False>),
			},
            IsHeader = true,
			BqlField = typeof(APPayment.adjFinPeriodID))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AdjFinPeriodID
		{
			get
			{
				return this._AdjFinPeriodID;
			}
			set
			{
				this._AdjFinPeriodID = value;
			}
		}
		#endregion
		#region AdjTranPeriodID
		public abstract class adjTranPeriodID : PX.Data.BQL.BqlString.Field<adjTranPeriodID> { }
		protected String _AdjTranPeriodID;

        [PeriodID(BqlField = typeof(APPayment.adjTranPeriodID))]
		public virtual String AdjTranPeriodID
		{
			get
			{
				return this._AdjTranPeriodID;
			}
			set
			{
				this._AdjTranPeriodID = value;
			}
		}
		#endregion
		#region StubCntr
		public abstract class stubCntr : PX.Data.BQL.BqlInt.Field<stubCntr> { }
		protected Int32? _StubCntr;
		[PXDBInt(BqlField = typeof(APPayment.stubCntr))]
		[PXDefault(0)]
		public virtual Int32? StubCntr
		{
			get
			{
				return this._StubCntr;
			}
			set
			{
				this._StubCntr = value;
			}
		}
		#endregion
		#region BillCntr
		public abstract class billCntr : PX.Data.BQL.BqlInt.Field<billCntr> { }
		protected Int32? _BillCntr;
		[PXDBInt(BqlField = typeof(APPayment.billCntr))]
		[PXDefault(0)]
		public virtual Int32? BillCntr
		{
			get
			{
				return this._BillCntr;
			}
			set
			{
				this._BillCntr = value;
			}
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		protected Boolean? _Cleared;
		[PXDBBool(BqlField = typeof(APPayment.cleared))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual Boolean? Cleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
				this._Cleared = value;
			}
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		protected DateTime? _ClearDate;
		[PXDBDate(BqlField = typeof(APPayment.clearDate))]
		[PXUIField(DisplayName = "Clear Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }

		[PXDBLong(BqlField = typeof(APPayment.cATranID))]
		[APQuickCheckCashTranID()]
		public virtual long? CATranID
		{
			get;
			set;
		}
		#endregion

		#region CuryOrigTaxDiscAmt
		public abstract class curyOrigTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]

		[PXUIField(DisplayName = "Discounted Tax Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDBCurrency(typeof(curyInfoID), typeof(origTaxDiscAmt), BqlField = typeof(APPayment.curyOrigTaxDiscAmt))]
		public decimal? CuryOrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigTaxDiscAmt
		public abstract class origTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<origTaxDiscAmt> { }
		[PXDBBaseCury(BqlField = typeof(APPayment.origTaxDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? OrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion

		#region ChargeCntr
		public abstract class chargeCntr : PX.Data.BQL.BqlInt.Field<chargeCntr> { }
		protected Int32? _ChargeCntr;
		[PXDBInt(BqlField = typeof(APPayment.chargeCntr))]
		[PXDefault(0)]
		public virtual Int32? ChargeCntr
		{
			get
			{
				return this._ChargeCntr;
			}
			set
			{
				this._ChargeCntr = value;
			}
		}
		#endregion
		//APRegister
		#region DocDate
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate(BqlField = typeof(APRegister.docDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public override DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		[PeriodID(BqlField = typeof(APRegister.tranPeriodID))]
		public override String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[APOpenPeriod(
			typeof(APQuickCheck.docDate),
            masterFinPeriodIDType: typeof(APQuickCheck.tranPeriodID),
			selectionModeWithRestrictions: FinPeriodSelectorAttribute.SelectionModesWithRestrictions.All,
			sourceSpecificationTypes:
			new[]
			{
				typeof(CalendarOrganizationIDProvider.SourceSpecification<APQuickCheck.branchID, True>),
				typeof(CalendarOrganizationIDProvider.SourceSpecification<
					APQuickCheck.cashAccountID,
					Selector<APQuickCheck.cashAccountID, CashAccount.branchID>,
					False>),
			},
			BqlField = typeof(APRegister.finPeriodID))]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region VendorID_Vendor_acctName
		public new abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
		#endregion
		#region VoidAppl
		public abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl> { }
		[PXBool()]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return APPaymentType.VoidAppl(this._DocType);
			}
			set
			{
				if ((bool)value)
				{
					this._DocType = APPaymentType.GetVoidingAPDocType(DocType) ?? APDocType.VoidCheck;
				}
			}
		}
		#endregion
		#region PrintCheck
		public abstract class printCheck : PX.Data.BQL.BqlBool.Field<printCheck> { }

		[PXDBBool(BqlField = typeof(APPayment.printCheck))]
		[FormulaDefault(typeof(
			IsNull<
				IIf<Where<
					isMigratedRecord, Equal<True>>,
					False,
					Selector<paymentMethodID, PaymentMethod.printOrExport>>,
				False>))]
		[PXDefault]
		[PXUIField(DisplayName = "Print Check")]
		public virtual bool? PrintCheck { get; set; }
		#endregion

		#region IsPrintingProcess
		public abstract class isPrintingProcess : PX.Data.BQL.BqlBool.Field<isPrintingProcess> { }

		/// <summary>
		/// Indicates that this check under printing processing to prevent update <see cref="CashAccountCheck"/> table by <see cref="AP.PaymentRefAttribute"/> />
		/// </summary>
		[PXBool]
		public virtual bool? IsPrintingProcess { get; set; }
		#endregion
		#region IsReleaseProcess
		public abstract class isReleaseProcess : PX.Data.BQL.BqlBool.Field<isReleaseProcess> { }

		/// <summary>
		/// Indicates that this check under release processing to prevent the question about the saving of the last check number by <see cref="AP.PaymentRefAttribute"/> />
		/// </summary>
		[PXBool]
		public virtual bool? IsReleaseCheckProcess { get; set; }
		#endregion

		#region Printed
		public new abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		[PXDBBool(BqlField = typeof(APRegister.printed))]
		[PXDefault]
		[PXFormula(typeof(IIf<Where<APQuickCheck.printCheck, NotEqual<True>, And<Selector<APQuickCheck.paymentMethodID, PaymentMethod.printOrExport>, Equal<True>,
			Or<Selector<APQuickCheck.paymentMethodID, PaymentMethod.printOrExport>, NotEqual<True>>>>, True, False>))]
		public override bool? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
			}
		}
		#endregion
		#region DepositAsBatch
		public abstract class depositAsBatch : PX.Data.BQL.BqlBool.Field<depositAsBatch> { }
		protected Boolean? _DepositAsBatch;
		[PXDBBool(BqlField = typeof(APPayment.depositAsBatch))]
		[PXUIField(DisplayName = "Batch Deposit", Enabled = false)]
		[PXDefault(false)]
		public virtual Boolean? DepositAsBatch
		{
			get
			{
				return this._DepositAsBatch;
			}
			set
			{
				this._DepositAsBatch = value;
			}
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter> { }
		protected DateTime? _DepositAfter;
		[PXDBDate(BqlField = typeof(APPayment.depositAfter))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deposit After", Enabled = false, Visible = false)]
		public virtual DateTime? DepositAfter
		{
			get
			{
				return this._DepositAfter;
			}
			set
			{
				this._DepositAfter = value;
			}
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited> { }
		protected Boolean? _Deposited;
		[PXDBBool(BqlField = typeof(APPayment.deposited))]
		[PXUIField(DisplayName = "Deposited", Enabled = false)]
		[PXDefault(false)]
		public virtual Boolean? Deposited
		{
			get
			{
				return this._Deposited;
			}
			set
			{
				this._Deposited = value;
			}
		}
		#endregion
		#region DepositDate
		public abstract class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate> { }
		protected DateTime? _DepositDate;
		[PXDBDate(BqlField = typeof(APPayment.depositDate))]
		[PXUIField(DisplayName = "Batch Deposit Date", Enabled = false)]
		public virtual DateTime? DepositDate
		{
			get
			{
				return this._DepositDate;
			}
			set
			{
				this._DepositDate = value;
			}
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }
		protected String _DepositType;
		[PXUIField(Enabled = false)]
		[PXDBString(3, IsFixed = true, BqlField = typeof(APPayment.depositType))]
		public virtual String DepositType
		{
			get
			{
				return this._DepositType;
			}
			set
			{
				this._DepositType = value;
			}
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr> { }
		protected String _DepositNbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(APPayment.depositNbr))]
		[PXUIField(DisplayName = "Batch Deposit Nbr.", Enabled = false)]
		public virtual String DepositNbr
		{
			get
			{
				return this._DepositNbr;
			}
			set
			{
				this._DepositNbr = value;
			}
		}
		#endregion

		#region TaxCalcMode
		public new abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		#endregion
		#region HasWithHoldTax
		public abstract class hasWithHoldTax : PX.Data.BQL.BqlBool.Field<hasWithHoldTax> { }
		protected bool? _HasWithHoldTax;
		[PXBool]
		[RestrictWithholdingTaxCalcMode(typeof(APQuickCheck.taxZoneID), typeof(APQuickCheck.taxCalcMode))]
		public virtual bool? HasWithHoldTax
		{
			get { return this._HasWithHoldTax; }
			set { this._HasWithHoldTax = value; }
		}
		#endregion
		#region HasUseTax
		public abstract class hasUseTax : PX.Data.BQL.BqlBool.Field<hasUseTax> { }
		protected bool? _HasUseTax;
		[PXBool]
		[RestrictUseTaxCalcMode(typeof(APQuickCheck.taxZoneID), typeof(APQuickCheck.taxCalcMode))]
		public virtual bool? HasUseTax
		{
			get { return this._HasUseTax; }
			set { this._HasUseTax = value; }
		}
		#endregion
	}

	public class APQuickCheckType : APDocType
	{
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APQuickCheck.refNbr),
				typeof(APQuickCheck.docDate),
				typeof(APQuickCheck.finPeriodID),
				typeof(APQuickCheck.vendorID),
				typeof(APQuickCheck.vendorID_Vendor_acctName),
				typeof(APQuickCheck.vendorLocationID),
				typeof(APQuickCheck.curyID),
				typeof(APQuickCheck.curyOrigDocAmt),
				typeof(APQuickCheck.curyDocBal),
				typeof(APQuickCheck.status),
				typeof(APQuickCheck.cashAccountID),
				typeof(APQuickCheck.paymentMethodID),
				typeof(APQuickCheck.extRefNbr))
			{
			}
		}

		/// <summary>
		/// Specialized for APQuickCheck version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the AP Payment. <br/>
		/// References APQuickCheck.docType and APQuickCheck.docDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in AP Setup and APQuickCheck types:<br/>
		/// namely - APSetup.checkNumberingID for QuickCheck and null for VoidQuickCheck<br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(APQuickCheck.docType), typeof(APQuickCheck.docDate),
					new string[] { QuickCheck, VoidQuickCheck },
					new Type[] { typeof(APSetup.checkNumberingID), null }) { ; }
		}

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { QuickCheck, VoidQuickCheck },
				new string[] { Messages.QuickCheck, Messages.VoidQuickCheck }) { ; }
		}

		[Obsolete("Obsoilete. Will be removed in Acumatica ERP 2019R1")]
		public static bool VoidAppl(string DocType)
		{
			return (DocType == VoidCheck);
		}
	}
}
