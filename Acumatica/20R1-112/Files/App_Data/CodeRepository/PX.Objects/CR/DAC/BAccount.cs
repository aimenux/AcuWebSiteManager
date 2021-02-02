using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.EP;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.SM;
using PX.TM;
using PX.Objects.GL;

namespace PX.Objects.CR
{
	/// <exclude/>
	[System.SerializableAttribute()]
	[CRCacheIndependentPrimaryGraphList(new Type[]{
        typeof(CR.BusinessAccountMaint),
		typeof(EP.EmployeeMaint),
		typeof(AP.VendorMaint),
		typeof(AP.VendorMaint),
		typeof(AR.CustomerMaint),
		typeof(AR.CustomerMaint),
		typeof(AP.VendorMaint),
		typeof(AR.CustomerMaint),
		typeof(CR.BusinessAccountMaint)},				
		new Type[]{
            typeof(Select<CR.BAccount, Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
                    And<Current<BAccount.viewInCrm>, Equal<True>>>>),
			typeof(Select<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			typeof(Select<AP.VendorR, Where<AP.VendorR.bAccountID, Equal<Current<BAccount.bAccountID>>>>), 
			typeof(Select<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Current<BAccountR.bAccountID>>>>), 
			typeof(Select<AR.Customer, Where<AR.Customer.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			typeof(Select<AR.Customer, Where<AR.Customer.bAccountID, Equal<Current<BAccountR.bAccountID>>>>),
			typeof(Where<CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.vendorType>>>), 
			typeof(Where<CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.customerType>>>), 
			typeof(Select<CR.BAccount, 
				Where2<Where<
					CR.BAccount.type, Equal<BAccountType.prospectType>,
					Or<CR.BAccount.type, Equal<BAccountType.customerType>,
					Or<CR.BAccount.type, Equal<BAccountType.vendorType>,
					Or<CR.BAccount.type, Equal<BAccountType.combinedType>>>>>,
						And<Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
						Or<Current<BAccount.bAccountID>, Less<Zero>>>>>>)
		})]
	[PXCacheName(Messages.BusinessAccount, PXDacType.Catalogue, CacheGlobal = true)]
	[CREmailContactsView(typeof(Select2<Contact,
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>, 
		Where<Contact.bAccountID, Equal<Optional<BAccount.bAccountID>>,
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	[DebuggerDisplay("{GetType().Name,nq}: BAccountID = {BAccountID,nq}, AcctCD = {AcctCD}, AcctName = {AcctName}")]
	public partial class BAccount : IBqlTable, IAssign, IPXSelectable, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BAccount>.By<bAccountID>
		{
			public static BAccount Find(PXGraph graph, int bAccountID) => FindBy(graph, bAccountID);
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Account ID")]
		[PXReferentialIntegrityCheck]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region AcctDC
		public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		protected String _AcctCD;
		[PXDimensionSelector("BIZACCT", typeof(Search<BAccount.acctCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask="")]
		[PXDefault()]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXPersonalDataWarning]
		public virtual String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		protected String _AcctName;
		[PXDBString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String AcctName
		{
			get
			{
				return this._AcctName;
			}
			set
			{
				this._AcctName = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID), DescriptionField = typeof(CRCustomerClass.description), CacheGlobal = true)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual String ClassID { get; set; }
		#endregion

		#region LegalName
		public abstract class legalName : PX.Data.BQL.BqlString.Field<legalName> { }
		[PXDBString(255, IsUnicode = true)]
		[PXDefault]
		[PXFormula(typeof(acctName))]
		[PXUIField(DisplayName = "Legal Name")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String LegalName
		{
			get;
			set;
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[BAccountType.List()]
		public virtual String Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
			  #endregion
		#region IsCustomerOrCombined
		public abstract class isCustomerOrCombined : PX.Data.BQL.BqlBool.Field<isCustomerOrCombined> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCalced(typeof(Switch<Case<Where<BAccount.type, Equal<BAccountType.customerType>, Or<BAccount.type, Equal<BAccountType.combinedType>>>, True>, False>), typeof(bool))]
		public virtual bool? IsCustomerOrCombined { get; set; }
		#endregion
		#region AcctReferenceNbr
		public abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }
		protected String _AcctReferenceNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Ref.#", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		public virtual String AcctReferenceNbr
		{
			get
			{
				return this._AcctReferenceNbr;
			}
			set
			{
				this._AcctReferenceNbr = value;
			}
		}
		#endregion
		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		protected Int32? _ParentBAccountID;
		[PXDBInt]
		[PXUIField(DisplayName = "Parent Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector("BIZACCT",
			typeof(Search2<BAccountR.bAccountID,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>>>,
				Where<Current<BAccount.bAccountID>, NotEqual<BAccountR.bAccountID>,
				And2<Where<BAccountR.type, Equal<BAccountType.customerType>,
					Or<BAccountR.type, Equal<BAccountType.prospectType>,
					Or<BAccountR.type, Equal<BAccountType.combinedType>,
					Or<BAccountR.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>>),
			typeof(BAccountR.acctCD),
			typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type), typeof(BAccountR.classID),
			typeof(BAccountR.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail), 
			DescriptionField = typeof(BAccountR.acctName))]
		[PXForeignReference(typeof(Field<parentBAccountID>.IsRelatedTo<bAccountID>))]
		[PXMassMergableField]
		public virtual Int32? ParentBAccountID
		{
			get
			{
				return this._ParentBAccountID;
			}
			set
			{
				this._ParentBAccountID = value;
			}
		}
		#endregion
		#region ConsolidateToParent
		public abstract class consolidateToParent : PX.Data.BQL.BqlBool.Field<consolidateToParent> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Balance")]
		public virtual bool? ConsolidateToParent { get; set; }
		#endregion
		#region ConsolidatingBAccountID
		public abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }

		[PXDBInt]
		[PXFormula(typeof(Switch<
			Case<Where<BAccount.parentBAccountID, IsNotNull, And<BAccount.consolidateToParent, Equal<True>>>, BAccount.parentBAccountID>,
			BAccount.bAccountID>))]
		public virtual Int32? ConsolidatingBAccountID { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Active, Hold, HoldPayments, Inactive, OneTime, CreditHold },
					new string[] { Messages.Active, Messages.Hold, Messages.HoldPayments, Messages.Inactive, Messages.OneTime, Messages.CreditHold }) { }
			}

			public const string Active = "A";
			public const string Hold = "H";
			public const string HoldPayments = "P";
			public const string Inactive = "I";
			public const string OneTime = "T";
			public const string CreditHold = "C";

			public class active : PX.Data.BQL.BqlString.Constant<active>
			{
				public active() : base(Active) { ;}
			}
			public class hold : PX.Data.BQL.BqlString.Constant<hold>
			{
				public hold() : base(Hold) { ;}
			}
			public class holdPayments : PX.Data.BQL.BqlString.Constant<holdPayments>
			{
				public holdPayments() : base(HoldPayments) { ;}
			}
			public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
			{
				public inactive() : base(Inactive) { ;}
			}
			public class oneTime : PX.Data.BQL.BqlString.Constant<oneTime>
			{
				public oneTime() : base(OneTime) { ;}
			}
			public class creditHold : PX.Data.BQL.BqlString.Constant<creditHold>
			{
				public creditHold() : base(CreditHold) { ;}
			}
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Required = true)]
		[status.List()]
		[PXDefault(status.Active)]
		[PXMassUpdatableField]
		[PXMassMergableField]
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
		#region CampaignSourceID
		public abstract class campaignSourceID : PX.Data.BQL.BqlString.Field<campaignSourceID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Campaign")]
		[PXSelector(typeof(Search3<CRCampaign.campaignID, OrderBy<Desc<CRCampaign.campaignID>>>),
			DescriptionField = typeof(CRCampaign.campaignName), Filterable = true)]
		public virtual String CampaignSourceID { get; set; }
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		protected Int32? _DefAddressID;
		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXUIField(DisplayName = "Default Address", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(
			typeof(Search<Address.addressID>),
			DescriptionField = typeof(Address.displayName),
			DirtyRead = true)]
        public virtual Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		protected Int32? _DefContactID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>), DirtyRead = true)]
		public virtual Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region DefLocationID
		public abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		protected Int32? _DefLocationID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Location.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<Location.locationID,
			Where<Location.bAccountID,
			Equal<Current<BAccount.bAccountID>>>>),
			DescriptionField = typeof(Location.locationCD), 
			DirtyRead = true)]
		public virtual Int32? DefLocationID
		{
			get
			{
				return this._DefLocationID;
			}
			set
			{
				this._DefLocationID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		[Obsolete("The field is obsolete and will be removed in Acumatica 8.0.")]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>),DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
		[PXMassMergableField]
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
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		protected String _TaxRegistrationID;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String TaxRegistrationID
		{
			get
			{
				return this._TaxRegistrationID;
			}
			set
			{
				this._TaxRegistrationID = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		[PXMassUpdatableField]
		[PXMassMergableField]
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

		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(BAccount.workgroupID))]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassUpdatableField]
		[PXMassMergableField]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.CR, "{0} {1}", new Type[] { typeof(BAccount.type), typeof(BAccount.acctName) },
			new Type[] { typeof(BAccount.acctCD), typeof(BAccount.defContactID), typeof(Contact.displayName), typeof(Contact.eMail), 
                        typeof(Contact.phone1), typeof(Contact.phone2), typeof(Contact.phone3), typeof(Contact.webSite) },
			NumberFields = new Type[] { typeof(BAccount.acctCD) },
			  Line1Format = "{0}{1}{3}{4}{5}", Line1Fields = new Type[] {  typeof(BAccount.type), typeof(BAccount.acctCD), typeof(BAccount.defContactID), typeof(Contact.displayName), typeof(Contact.phone1), typeof(Contact.eMail) },
			  Line2Format = "{1}{2}{3}", Line2Fields = new Type[] { typeof(BAccount.defAddressID), typeof(Address.displayName), typeof(Address.city), typeof(Address.state) },
			  WhereConstraint = typeof(Where<BAccount.type, Equal<BAccountType.prospectType>>),
			  SelectForFastIndexing = typeof(Select2<BAccount, InnerJoin<Contact, On<Contact.contactID, Equal<BAccount.defContactID>>>, Where<BAccount.type, Equal<BAccountType.prospectType>>>)
		  )]
		[PXUniqueNote(
			DescriptionField = typeof(BAccount.acctCD),
			Selector = typeof(Search<BAccount.acctCD, Where<BAccount.type, Equal<BAccountType.prospectType>>>),
			ActivitiesCountByParent = true,
			ShowInReferenceSelector = true,
            PopupTextEnabled = true)]
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
		#region Attributes

		[CRAttributesField(typeof (BAccount.classID), new[] {typeof (Customer.customerClassID), typeof (Vendor.vendorClassID)})]
		public virtual string[] Attributes { get; set; }

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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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

		#region CasesCount

		public abstract class casesCount : PX.Data.BQL.BqlInt.Field<casesCount> { }

		[PXInt]
		public virtual Int32? CasesCount { get; set; }

		#endregion

		#region Count

		public abstract class count : PX.Data.BQL.BqlInt.Field<count> { }

		[PXInt]
		[PXUIField(DisplayName = "Count")]
		public virtual Int32? Count { get; set; }

		#endregion

		#region LastActivity

		public abstract class lastActivity : PX.Data.BQL.BqlDateTime.Field<lastActivity> { }

		[PXDate]
		[PXUIField(DisplayName = "Last Activity")]
		public DateTime? LastActivity { get; set; }

		#endregion

		#region PreviewHtml
		public abstract class previewHtml : PX.Data.BQL.BqlString.Field<previewHtml> { }
		[PXString]
		//[PXUIField(Visible = false)]
		public virtual String PreviewHtml { get; set; }
		#endregion

        #region ViewInCrm
        public abstract class viewInCrm : PX.Data.BQL.BqlBool.Field<viewInCrm> { }
        protected bool? _ViewInCrm;
        [PXBool]
        [PXUIField(DisplayName = "View In CRM")]
        public virtual bool? ViewInCrm
        {
            get
            {
                return this._ViewInCrm;
            }
            set
            {
                this._ViewInCrm = value;
            }
        }
		#endregion
	}

	#region BAccount2

	[Serializable]
	public sealed class BAccount2 : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		public new abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }

		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }

		public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
	}

	#endregion

	#region BAccountParent

	[Serializable]
	[PXCacheName(Messages.ParentBusinessAccount)]        
    [CRCacheIndependentPrimaryGraphList(new Type[]{
    typeof(CR.BusinessAccountMaint),    
		},
        new Type[]{      
			typeof(Select<BAccountCRM, 
				Where<BAccountCRM.bAccountID, Equal<Current<BAccountParent.bAccountID>>, 
					Or<Current<BAccountParent.bAccountID>, Less<Zero>>>>),            
		})]    
	public sealed class BAccountParent : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		public new abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }

		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }

		public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }

		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[PXDimensionSelector("BIZACCT", typeof(BAccount.acctCD), typeof(BAccount.acctCD))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Parent Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override string AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region AcctName
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Parent Business Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXMassMergableField]
		public override string AcctName
		{
			get
			{
				return this._AcctName;
			}
			set
			{
				this._AcctName = value;
			}
		}
		#endregion
	}

	#endregion
}
