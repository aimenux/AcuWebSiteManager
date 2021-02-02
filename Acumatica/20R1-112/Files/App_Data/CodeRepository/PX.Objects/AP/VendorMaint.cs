using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.SM;
using Branch = PX.SM.Branch;
using PX.Objects.IN;

namespace PX.Objects.AP
{
	#region Specialized DAC Classes
	[PXSubstitute(GraphType = typeof(VendorMaint))]
    [PXCacheName(Messages.Vendor)]
	[System.SerializableAttribute()]
	public partial class VendorR : Vendor
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[VendorRaw(typeof(Where<Vendor.type, Equal<BAccountType.vendorType>,
							 Or<Vendor.type, Equal<BAccountType.combinedType>>>), DescriptionField = typeof(Vendor.acctName), IsKey = true, DisplayName = "Vendor ID")]
		[PXDefault()]
		[PXPersonalDataWarning]
		public override String AcctCD
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
	}
	#endregion

	public class VendorMaint : BusinessAccountGraphBase<VendorR, VendorR, Where<BAccount.type, Equal<BAccountType.vendorType>,
							Or<BAccount.type, Equal<BAccountType.combinedType>>>>
	{
		protected LocationValidator LocationValidator;

		#region Repo methods

		public static Vendor FindByID(PXGraph graph, int? bAccountID)
		{
			return PXSelect<Vendor,
									Where2<Where<Vendor.type, Equal<BAccountType.vendorType>,
										Or<Vendor.type, Equal<BAccountType.combinedType>>>,
									And<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>>
									.Select(graph, bAccountID);
		}

		public static Vendor GetByID(PXGraph graph, int? bAccountID)
		{
			var vendor = FindByID(graph, bAccountID);

			if (vendor == null)
			{
				throw new PXException(Common.Messages.EntityWithIDDoesNotExist, EntityHelper.GetFriendlyEntityName(typeof(Vendor)),
					bAccountID);
			}

			return vendor;
		}

		#endregion

		#region InternalTypes
		[Serializable]
		[PXCacheName(Messages.VendorBalanceSummary)]
		public partial class VendorBalanceSummary : IBqlTable
		{
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[PXDBInt()]
			[PXDefault()]
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
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
			protected Decimal? _Balance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Balance", Visible = true, Enabled = false)]
			public virtual Decimal? Balance
			{
				get
				{
					return this._Balance;
				}
				set
				{
					this._Balance = value;
				}
			}
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }
			protected Decimal? _DepositsBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayment Balance", Enabled = false)]
			public virtual Decimal? DepositsBalance
			{
				get
				{
					return this._DepositsBalance;
				}
				set
				{
					this._DepositsBalance = value;
				}
			}
			#endregion
			#region RetainageBalance
			public abstract class retainageBalance : PX.Data.BQL.BqlDecimal.Field<retainageBalance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Retained Balance", Visibility = PXUIVisibility.Visible, Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual decimal? RetainageBalance
			{
				get;
				set;
			}
			#endregion

			public virtual void Init()
			{
				if (!this.Balance.HasValue) this.Balance = Decimal.Zero;
				if (!this.DepositsBalance.HasValue) this.DepositsBalance = Decimal.Zero;
				if (!this.RetainageBalance.HasValue) this.RetainageBalance = Decimal.Zero;
			}

		}

		[Serializable]
		[PXProjection(typeof(Select<Vendor, Where<Vendor.payToVendorID, Equal<CurrentValue<Vendor.bAccountID>>>>))]
		public class SuppliedByVendor : Vendor { }
		#endregion

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<APNotificationSource.vendor>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
		[PXUIField(DisplayName = "Mailing ID")]
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		#region NBranchID
		[GL.Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
			Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
		{
			
		}
		#endregion
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
		Where<SiteMap.url, Like<Common.urlReports>,
			And<Where<SiteMap.screenID, Like<PXModule.ap_>,
					Or<SiteMap.screenID, Like<PXModule.cl_>,
					Or<SiteMap.screenID, Like<PXModule.po_>, 
					Or<SiteMap.screenID, Like<PXModule.sc_>,
					Or<SiteMap.screenID, Like<PXModule.rq_>>>>>>>>,
		OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
		Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
		DescriptionField = typeof(SiteMap.title))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
		{
		}
		
		#endregion

		#region NotificationRecipient		
		[PXDBInt]
		[PXDBLiteDefault(typeof(NotificationSource.sourceID))]
		protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10)]
		[PXDefault]
		[VendorContactType.List]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,			
			And<NotificationRecipient.refNoteID, Equal<Current<Vendor.noteID>>>>))]
		protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationRecipient.contactType), DirtyRead = true)]
		protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationRecipient_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXString()]
		[PXUIField(DisplayName = "Email", Enabled = false)]
		protected virtual void NotificationRecipient_Email_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region Public Selects

		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentVendor;
		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Address.addressID, Equal<Current<Location.vRemitAddressID>>>>> RemitAddress;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Contact.contactID, Equal<Current<Location.vRemitContactID>>>>> RemitContact;
		public PXSelectJoin<VendorPaymentMethodDetail,
							InnerJoin<PaymentMethod, On<VendorPaymentMethodDetail.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,	
							InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
							    And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
                                    And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>>,
							Where<VendorPaymentMethodDetail.bAccountID, Equal<Optional<LocationExtAddress.bAccountID>>,
									And<VendorPaymentMethodDetail.locationID, Equal<Optional<LocationExtAddress.locationID>>,
									And<VendorPaymentMethodDetail.paymentMethodID, Equal<Optional<LocationExtAddress.vPaymentMethodID>>>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> PaymentDetails;
		public PXSelect<PaymentMethodDetail, 
                    Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<LocationExtAddress.vPaymentMethodID>>,
                        And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>> PaymentTypeDetails;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Optional<Vendor.vendorClassID>>>> VendorClass;
		public PXSetup<APSetup> APSetup;
		[PXCopyPasteHiddenView]
		public PXSelect<VendorBalanceSummary> VendorBalance;

		public CRNotificationSourceList<Vendor, Vendor.vendorClassID, APNotificationSource.vendor> NotificationSources;

		public CRNotificationRecipientList<Vendor, Vendor.vendorClassID> NotificationRecipients;

		public PXSelect<POVendorInventory, Where<POVendorInventory.vendorID, Equal<Current<Vendor.bAccountID>>>> VendorItems;

		[Obsolete("The view is obsolete and will be eliminated in Acumatica 8.0")]
		[PXCopyPasteHiddenView]
		public PXSelect<TaxPeriod> taxperiods;
		
		[CRReference(typeof(Vendor.bAccountID), Persistent = true)]
        public CRActivityList<VendorR>
			Activities;

		public PXSelectJoin<TaxTranReport,
				InnerJoin<Tax, On<Tax.taxID, Equal<TaxTranReport.taxID>>,
				InnerJoin<TaxReportLine, On<TaxReportLine.vendorID, Equal<Tax.taxVendorID>>,
				InnerJoin<TaxBucketLine,
					On<TaxBucketLine.vendorID, Equal<TaxReportLine.vendorID>,
				 And<TaxBucketLine.lineNbr, Equal<TaxReportLine.lineNbr>,
				 And<TaxBucketLine.bucketID, Equal<TaxTranReport.taxBucketID>>>>>>>,
				Where<Tax.taxVendorID, Equal<Required<TaxPeriodFilter.vendorID>>,
					And<TaxTranReport.released, Equal<True>,
					And<TaxTranReport.voided, Equal<False>,
					And<TaxTranReport.taxPeriodID, IsNull,
					And<TaxTranReport.taxType, NotEqual<TaxType.pendingPurchase>,
					And<TaxTranReport.taxType, NotEqual<TaxType.pendingSales>,
					And<TaxTranReport.origRefNbr, Equal<Empty>>>>>>>>,
				OrderBy<Asc<TaxTranReport.tranDate>>> OldestNotReportedTaxTran;

		[PXViewName(CR.Messages.Answers)]
		public CRAttributeList<Vendor> Answers;
		
		[PXCopyPasteHiddenView]
		public PXSelect<SuppliedByVendor> SuppliedByVendors;
		
		public VendorMaint()
		{
			Activities.GetNewEmailAddress =
				() =>
				{
					var contact = (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Current<Vendor.defContactID>>>>.
						Select(this);

					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
						: String.Empty;
				};

			APSetup setup = APSetup.Current;
			Views.Caches.Remove(typeof(Vendor));

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);

			action.AddMenuAction(ChangeID);

			PXUIFieldAttribute.SetVisible<Vendor.localeName>(BAccount.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);

			LocationValidator = new LocationValidator();

			// right baccount type for redirect from empty Pay-to Vendor field
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });
		}

		[PXHidden]
		public PXSelect<INItemXRef> xrefs;

		#endregion

		#region Buttons

		public PXAction<VendorR> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (CurrentVendor.Current != null)
			{
				APAccessDetail graph = CreateInstance<APAccessDetail>();
				graph.Vendor.Current = graph.Vendor.Search<Vendor.acctCD>(CurrentVendor.Current.AcctCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<VendorR> viewCustomer;
		[PXUIField(DisplayName = Messages.ViewCustomer, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCustomer(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null && (bacct.Type == BAccountType.CustomerType || bacct.Type == BAccountType.CombinedType))
			{
				Save.Press();
				AR.CustomerMaint editingBO = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<AR.Customer.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, "Edit Customer");
			}
			return adapter.Get();
		}

		public PXAction<VendorR> viewBusnessAccount;
		[PXUIField(DisplayName = Messages.ViewBusnessAccount, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBusnessAccount(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null)
			{
				Save.Press();
				CR.BusinessAccountMaint editingBO = PXGraph.CreateInstance<PX.Objects.CR.BusinessAccountMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<BAccount.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, "Edit Business Account");
			}
			return adapter.Get();
		}
		
		public PXAction<VendorR> extendToCustomer;
		[PXUIField(DisplayName = Messages.ExtendToCustomer, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ExtendToCustomer(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			Vendor vendor = this.CurrentVendor.SelectSingle();
			if (bacct != null && (bacct.Type == BAccountType.VendorType || bacct.Type == BAccountType.CombinedType))
			{
				Save.Press();
				AR.CustomerMaint editingBO = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
				AR.Customer customer = (AR.Customer)editingBO.BAccount.Cache.Extend<BAccount>(bacct);
				editingBO.BAccount.Current = customer;
				customer.NoteID = bacct.NoteID;
				customer.Type = BAccountType.CombinedType;
				customer.LocaleName = vendor?.LocaleName;
				LocationExtAddress defLocation = editingBO.DefLocation.Select();
				editingBO.DefLocation.Cache.RaiseRowSelected(defLocation);
				string locationType = LocTypeList.CombinedLoc;
				if (defLocation.CTaxZoneID == null)
					editingBO.DefLocation.Cache.SetDefaultExt<Location.cTaxZoneID>(defLocation);
				editingBO.InitCustomerLocation(defLocation, locationType, false);
				defLocation = editingBO.DefLocation.Update(defLocation);
			    foreach (LocationExtAddress iLoc in editingBO.Locations.Select())
				{
					if (iLoc.LocationID != defLocation.LocationID)
					{
						editingBO.InitCustomerLocation(iLoc, locationType, false);
			            editingBO.Locations.Update(iLoc);
					}
				}
				throw new PXRedirectRequiredException(editingBO, "Edit Customer");
			}
			return adapter.Get();
		}


		public PXAction<VendorR> viewBalanceDetails;
		[PXUIField(DisplayName = "View Balance Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewBalanceDetails(PXAdapter adapter)
		{
			Vendor vendor = this.BAccount.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APVendorBalanceEnq graph = PXGraph.CreateInstance<APVendorBalanceEnq>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, "ViewBalanceDetails");
			}
			return adapter.Get();
		}

		public PXAction<VendorR> viewRemitOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ViewRemitOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.RemitAddress.Current);
			return adapter.Get();
		}



		public PXAction<VendorR> newBillAdjustment;
		[PXUIField(DisplayName = AP.Messages.APInvoiceEntry, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewBillAdjustment(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APInvoiceEntry invEntry = PXGraph.CreateInstance<APInvoiceEntry>();
				invEntry.Clear();

				APInvoice newDoc = invEntry.Document.Insert(new APInvoice());
				invEntry.Document.Cache.SetValueExt<APInvoice.vendorID>(newDoc, vendor.BAccountID);

				throw new PXRedirectRequiredException(invEntry, AP.Messages.APInvoiceEntry);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> newManualCheck;
		[PXUIField(DisplayName = AP.Messages.APPaymentEntry, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewManualCheck(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APPaymentEntry payEntry = PXGraph.CreateInstance<APPaymentEntry>();
				payEntry.Clear();
				APPayment newDoc = payEntry.Document.Insert(new APPayment());
				newDoc.VendorID = vendor.BAccountID;
				payEntry.Document.Cache.RaiseFieldUpdated<APPayment.vendorID>(newDoc, null);
				throw new PXRedirectRequiredException(payEntry, AP.Messages.APPaymentEntry);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> vendorDetails;
		[PXUIField(DisplayName = AP.Messages.APDocumentEnq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable VendorDetails(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				graph.Filter.Select(); //Select() is called to trigger the filter delegate, in which the totals are calculated.
				throw new PXRedirectRequiredException(graph, AP.Messages.APDocumentEnq);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> approveBillsForPayments;
		[PXUIField(DisplayName = AP.Messages.APApproveBills, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ApproveBillsForPayments(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APApproveBills graph = PXGraph.CreateInstance<APApproveBills>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, AP.Messages.APApproveBills);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> payBills;
		[PXUIField(DisplayName = AP.Messages.APPayBills, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable PayBills(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APPayBills graph = PXGraph.CreateInstance<APPayBills>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, AP.Messages.APPayBills);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> vendorPrice;
		[PXUIField(DisplayName = "Vendor Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable VendorPrice(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APVendorPriceMaint graph = PXGraph.CreateInstance<APVendorPriceMaint>();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, "Vendor Prices");
			}
			return adapter.Get();
		}

        //+ MMK 2011/10/03
        public PXAction<VendorR> balanceByVendor;
        [PXUIField(DisplayName = AP.Messages.BalanceByVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable BalanceByVendor(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP632500", AP.Messages.BalanceByVendor); //?????

            }
            return adapter.Get();
        }

        public PXAction<VendorR> vendorHistory;
        [PXUIField(DisplayName = AP.Messages.VendorHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable VendorHistory(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP652000", AP.Messages.VendorHistory); //?????
            }
            return adapter.Get();
        }

        //AP Aged Past Due
        public PXAction<VendorR> aPAgedPastDue;
        [PXUIField(DisplayName = AP.Messages.APAgedPastDue, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable APAgedPastDue(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP631000", AP.Messages.APAgedPastDue); //?????
            }
            return adapter.Get();
        }

        public PXAction<VendorR> aPAgedOutstanding;
        [PXUIField(DisplayName = AP.Messages.APAgedOutstanding, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable APAgedOutstanding(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP631500", AP.Messages.APAgedOutstanding); //?????
            }
            return adapter.Get();
        }

        public PXAction<VendorR> aPDocumentRegister;
        [PXUIField(DisplayName = AP.Messages.APDocumentRegister, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable APDocumentRegister(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["OrgBAccountID"] = null;
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP621500", AP.Messages.APDocumentRegister); //?????
            }
            return adapter.Get();
        }

        public PXAction<VendorR> repVendorDetails;
        [PXUIField(DisplayName = AP.Messages.RepVendorDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable RepVendorDetails(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP655500", AP.Messages.RepVendorDetails); //?????
            }
            return adapter.Get();
        }

		protected override LocationMaint CreateLocationGraph()
		{
			return PXGraph.CreateInstance<VendorLocationMaint>();
		}

		protected override SelectedLocation CreateSelectedLocation()
		{
			return new SelectedVendorLocation();
		}


		#region Buttons
		public new PXAction<Vendor> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public override IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null)
			{
				Address address = this.DefAddress.Current;
				if (address != null && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true);
				}

				Address remitAddress = this.RemitAddress.Current;
				if (remitAddress != null && remitAddress.IsValidated == false && (remitAddress.AddressID!= address.AddressID))
				{
					PXAddressValidator.Validate<Address>(this, remitAddress, true);
				}
				LocationExtAddress locAddress = this.DefLocation.Select();
				//Needs to compare defAddress - AddressID  would be null
				if (locAddress != null && locAddress.IsValidated == false && locAddress.DefAddressID != address.AddressID) 
				{
					PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true);
				}

			}
			return adapter.Get();
		}
		#endregion
        #region MyButtons (MMK)
        public PXAction<VendorR> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<VendorR> inquiry;
        [PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder)]
        protected virtual IEnumerable Inquiry(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<VendorR> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

		public PXChangeID<VendorR, VendorR.acctCD> ChangeID;
		#endregion

		#region Select Delegates

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable remitContact()
		{
			Contact cnt = null;
			Vendor vendor = this.BAccount.Current;
			LocationExtAddress defLocation = DefLocation.Select();
			if (defLocation != null && defLocation.VRemitContactID != null)
			{
				cnt = FindContact(defLocation.VRemitContactID);
				if (cnt != null)
				{
					if (defLocation.VRemitContactID == vendor.DefContactID)
					{
						cnt = PXCache<Contact>.CreateCopy(cnt);
						PXUIFieldAttribute.SetEnabled(this.RemitContact.Cache, cnt, false);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.RemitContact.Cache, cnt, true);
					}
				}
			}
			return new Contact[] { cnt };
		}

		protected Address formRemitAddress;

        [Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable remitAddress()
		{
			Address addr = null;
			Vendor vendor = this.BAccount.Current;
			LocationExtAddress defLocation = DefLocation.Select();
			if (defLocation != null && defLocation.VRemitAddressID != null)
			{
				addr = FindAddress(defLocation.VRemitAddressID);
				if (addr != null)
				{
					if (defLocation.VRemitAddressID == vendor.DefAddressID)
					{
						addr = PXCache<Address>.CreateCopy(addr);
						PXUIFieldAttribute.SetEnabled(this.RemitAddress.Cache, addr, false);
						formRemitAddress = addr;
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.RemitAddress.Cache, addr, true);
					}
				}
			}
			return new Address[] { addr };
		}


		protected virtual IEnumerable vendorBalance()
		{

			Vendor vendor = (Vendor)this.BAccountAccessor.Current;
			List<VendorBalanceSummary> list = new List<VendorBalanceSummary>(1);
			bool isInserted = (this.BAccountAccessor.Cache.GetStatus(vendor) == PXEntryStatus.Inserted);
			if (!isInserted)
			{
				PXSelectBase<APVendorBalanceEnq.APLatestHistory> sel = new PXSelectJoinGroupBy<APVendorBalanceEnq.APLatestHistory, 
					LeftJoin<CuryAPHistory, On<APVendorBalanceEnq.APLatestHistory.branchID, Equal<CuryAPHistory.branchID>,
						And<APVendorBalanceEnq.APLatestHistory.accountID, Equal<CuryAPHistory.accountID>,
						And<APVendorBalanceEnq.APLatestHistory.vendorID, Equal<CuryAPHistory.vendorID>,
						And<APVendorBalanceEnq.APLatestHistory.subID, Equal<CuryAPHistory.subID>,
						And<APVendorBalanceEnq.APLatestHistory.curyID, Equal<CuryAPHistory.curyID>,
						And<APVendorBalanceEnq.APLatestHistory.lastActivityPeriod, Equal<CuryAPHistory.finPeriodID>>>>>>>>,
					Where<APVendorBalanceEnq.APLatestHistory.vendorID, Equal<Current<Vendor.bAccountID>>>,
					Aggregate<
						Sum<CuryAPHistory.finBegBalance,
						Sum<CuryAPHistory.curyFinBegBalance,
						Sum<CuryAPHistory.finYtdBalance,
						Sum<CuryAPHistory.curyFinYtdBalance,
						Sum<CuryAPHistory.tranBegBalance,
						Sum<CuryAPHistory.curyTranBegBalance,
						Sum<CuryAPHistory.tranYtdBalance,
						Sum<CuryAPHistory.curyTranYtdBalance,

						Sum<CuryAPHistory.finPtdPayments,
						Sum<CuryAPHistory.finPtdPurchases,
						Sum<CuryAPHistory.finPtdDiscTaken,
						Sum<CuryAPHistory.finPtdWhTax,
						Sum<CuryAPHistory.finPtdCrAdjustments,
						Sum<CuryAPHistory.finPtdDrAdjustments,
						Sum<CuryAPHistory.finPtdRGOL,
						Sum<CuryAPHistory.finPtdDeposits,
						Sum<CuryAPHistory.finYtdDeposits,
						
						Sum<CuryAPHistory.finPtdRetainageWithheld,
						Sum<CuryAPHistory.finYtdRetainageWithheld,
						Sum<CuryAPHistory.finPtdRetainageReleased,
						Sum<CuryAPHistory.finYtdRetainageReleased,

						Sum<CuryAPHistory.tranPtdPayments,
						Sum<CuryAPHistory.tranPtdPurchases,
						Sum<CuryAPHistory.tranPtdDiscTaken,
						Sum<CuryAPHistory.tranPtdWhTax,
						Sum<CuryAPHistory.tranPtdCrAdjustments,
						Sum<CuryAPHistory.tranPtdDrAdjustments,
						Sum<CuryAPHistory.tranPtdRGOL,
						Sum<CuryAPHistory.tranPtdDeposits,
						Sum<CuryAPHistory.tranYtdDeposits,
						
						Sum<CuryAPHistory.tranPtdRetainageWithheld,
						Sum<CuryAPHistory.tranYtdRetainageWithheld,
						Sum<CuryAPHistory.tranPtdRetainageReleased,
						Sum<CuryAPHistory.tranYtdRetainageReleased,

						Sum<CuryAPHistory.curyFinPtdPayments,
						Sum<CuryAPHistory.curyFinPtdPurchases,
						Sum<CuryAPHistory.curyFinPtdDiscTaken,
						Sum<CuryAPHistory.curyFinPtdWhTax,
						Sum<CuryAPHistory.curyFinPtdCrAdjustments,
						Sum<CuryAPHistory.curyFinPtdDrAdjustments,
						Sum<CuryAPHistory.curyFinPtdDeposits,
						Sum<CuryAPHistory.curyFinYtdDeposits,
						
						Sum<CuryAPHistory.curyFinPtdRetainageWithheld,
						Sum<CuryAPHistory.curyFinYtdRetainageWithheld,
						Sum<CuryAPHistory.curyFinPtdRetainageReleased,
						Sum<CuryAPHistory.curyFinYtdRetainageReleased,

						Sum<CuryAPHistory.curyTranPtdPayments,
						Sum<CuryAPHistory.curyTranPtdPurchases,
						Sum<CuryAPHistory.curyTranPtdDiscTaken,
						Sum<CuryAPHistory.curyTranPtdWhTax,
						Sum<CuryAPHistory.curyTranPtdCrAdjustments,
						Sum<CuryAPHistory.curyTranPtdDrAdjustments,
						Sum<CuryAPHistory.curyTranPtdDeposits,
						Sum<CuryAPHistory.curyTranYtdDeposits,
						
						Sum<CuryAPHistory.curyTranPtdRetainageWithheld,
						Sum<CuryAPHistory.curyTranYtdRetainageWithheld,
						Sum<CuryAPHistory.curyTranPtdRetainageReleased,
						Sum<CuryAPHistory.curyTranYtdRetainageReleased
						>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(this);

				VendorBalanceSummary res = new VendorBalanceSummary();
				foreach (PXResult<APVendorBalanceEnq.APLatestHistory, CuryAPHistory> it in sel.Select())
				{
					CuryAPHistory iHst = it;
					Aggregate(res, iHst);
				}
				list.Add(res);
			}

			return list;
		}

		protected virtual void Address_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null &&
				addr.AddressID == null)
			{
				e.Cancel = true;
			}
		}

		protected override void Address_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
            base.Address_RowUpdated(cache, e);

			Address addr = e.Row as Address;
			if (formRemitAddress != null &&
				addr != null)
			{
				int? id = formRemitAddress.AddressID;
				Guid? noteID = formRemitAddress.NoteID;
				PXCache<Address>.RestoreCopy(formRemitAddress, addr);
				formRemitAddress.AddressID = id;
				formRemitAddress.NoteID = noteID;
			}
		}
		#endregion

		#region Overrides
		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);
			//if (!map.ContainsKey(typeof(Address)))
			//map.Add(typeof(Address), typeof(Address));
            Caches.AddCacheMappingsWithInheritance(this,typeof(VendorR)); 
		    Caches.AddCacheMappingsWithInheritance(this,typeof(Location)); 
			//var vendorCache = Caches[typeof(VendorR)];
			//var locationCache = Caches[typeof(Location)];
		}

		public override void Persist()
		{
			if (CurrentVendor.Current != null
				&& CurrentVendor.Cache.GetStatus(CurrentVendor.Current) == PXEntryStatus.Updated)
			{
				bool errorsExist = false;

				foreach (LocationExtAddress location in Locations.Select())
				{
					PXEntryStatus locationStatus = Locations.Cache.GetStatus(location);

					if (locationStatus != PXEntryStatus.Updated && locationStatus != PXEntryStatus.Inserted)
					{
						errorsExist |= !ValidateLocation(Locations.Cache, location);
					}
				}

				if (errorsExist)
				{
					throw new PXException(Common.Messages.RecordCanNotBeSaved);
				}
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				bool persisted = false;
				try
				{
                    persisted = (base.Persist(typeof(Vendor), PXDBOperation.Update) > 0);
				}
				catch
				{
					Caches[typeof(Vendor)].Persisted(true);
					throw;
				}
				base.Persist();
				if (persisted)
				{
					base.SelectTimeStamp();
				}
				ts.Complete();
			}
		}
		#endregion

		#region Vendor events

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
			Where<LocationExtAddress.bAccountID,
			Equal<Current<VendorR.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void VendorR_DefLocationID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void Vendor_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			PXRowInserting inserting = delegate(PXCache sender, PXRowInsertingEventArgs args)
			{
				Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
				if (branch != null)
				{
					object cd = branch.BranchCD;
					this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(args.Row, ref cd);
					((LocationExtAddress)args.Row).LocationCD = (string)cd;
				}
			};

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.AddHandler<LocationExtAddress>(inserting);
			}

			// Executing Base Business Account Event
			base.OnBAccountRowInserted(cache, e);

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.RemoveHandler<LocationExtAddress>(inserting);
			}
		}

		protected virtual void Vendor_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Vendor row = (Vendor)e.Row;

			if (row == null)
				return;

			PXEntryStatus status = cache.GetStatus(row);

			bool isExistingRecord = !(status == PXEntryStatus.Inserted && row.AcctCD == null);

			viewCustomer.SetEnabled((row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType) && isExistingRecord);
			viewBusnessAccount.SetEnabled(isExistingRecord);
			newBillAdjustment.SetEnabled(isExistingRecord);
			newManualCheck.SetEnabled(isExistingRecord);
			extendToCustomer.SetEnabled(!(row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType) && isExistingRecord);
			viewRestrictionGroups.SetEnabled(isExistingRecord);
			ChangeID.SetEnabled(isExistingRecord);

			vendorDetails.SetEnabled(isExistingRecord);
			approveBillsForPayments.SetEnabled(isExistingRecord);
			payBills.SetEnabled(isExistingRecord);
			vendorPrice.SetEnabled(isExistingRecord);

			balanceByVendor.SetEnabled(isExistingRecord);
			vendorHistory.SetEnabled(isExistingRecord);
			aPAgedPastDue.SetEnabled(isExistingRecord);
			aPAgedOutstanding.SetEnabled(isExistingRecord);
			aPDocumentRegister.SetEnabled(isExistingRecord);
			repVendorDetails.SetEnabled(isExistingRecord);
			
			bool mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<Vendor.curyID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Vendor.curyRateTypeID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Vendor.allowOverrideCury>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<Vendor.allowOverrideRate>(cache, null, mcFeatureInstalled);

			bool isVendor1099 = row.Vendor1099 == true;
			PXUIFieldAttribute.SetVisible<VendorBalanceSummary.depositsBalance>(this.VendorBalance.Cache, null, isExistingRecord);
			PXUIFieldAttribute.SetVisible<VendorBalanceSummary.balance>(this.VendorBalance.Cache, null, isExistingRecord);
			PXUIFieldAttribute.SetVisible<VendorBalanceSummary.retainageBalance>(VendorBalance.Cache, null, isExistingRecord);

			PXUIFieldAttribute.SetEnabled<Vendor.taxReportFinPeriod>(cache, null, row.TaxPeriodType != PX.Objects.TX.VendorTaxPeriodType.FiscalPeriod);
			PXUIFieldAttribute.SetEnabled<Vendor.taxReportPrecision>(cache, null, row.TaxUseVendorCurPrecision != true);
			PXUIFieldAttribute.SetEnabled<Vendor.box1099>(cache, null, isVendor1099);
			PXUIFieldAttribute.SetEnabled<Vendor.foreignEntity>(cache, null, isVendor1099);

			bool retainageApply = row.RetainageApply == true;
			PXUIFieldAttribute.SetEnabled<Vendor.retainagePct>(cache, row, retainageApply);
			PXUIFieldAttribute.SetEnabled<Vendor.paymentsByLinesAllowed>(cache, row, row.TaxAgency != true);

			if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>())
			{
				bool isTaxInvoiceNumberingIDRequired = row.SVATOutputTaxEntryRefNbr == VendorSVATTaxEntryRefNbr.TaxInvoiceNbr;
				PXUIFieldAttribute.SetVisible<Vendor.sVATTaxInvoiceNumberingID>(cache, null, isTaxInvoiceNumberingIDRequired);
				PXUIFieldAttribute.SetRequired<Vendor.sVATTaxInvoiceNumberingID>(cache, isTaxInvoiceNumberingIDRequired);
				PXDefaultAttribute.SetPersistingCheck<Vendor.sVATTaxInvoiceNumberingID>(cache, null, isTaxInvoiceNumberingIDRequired
					? PXPersistingCheck.NullOrBlank
					: PXPersistingCheck.Nothing);
			}

			Delete.SetEnabled(CanDelete(row));

			bool isPayToVendor = PXSelect<Vendor,
				Where<Vendor.payToVendorID, Equal<Current<Vendor.bAccountID>>,
					And<Vendor.bAccountID, NotEqual<Current<Vendor.bAccountID>>>>,				
				OrderBy<Asc<Vendor.bAccountID>>>
				.SelectSingleBound(this, new object[] {row})
				.RowCast<Vendor>()
				.Any();

			PXUIFieldAttribute.SetEnabled<Vendor.payToVendorID>(
				cache, 
				row,
				!isPayToVendor
				&& !isVendor1099
				&& row.TaxAgency != true 
				&& row.IsLaborUnion != true);

			SuppliedByVendors.Cache.AllowSelect = isPayToVendor;
			SuppliedByVendors.Cache.AllowInsert =
			SuppliedByVendors.Cache.AllowUpdate =
			SuppliedByVendors.Cache.AllowDelete = false;
		}

		protected virtual void Vendor_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Vendor row = e.Row as Vendor;
			if (row != null)
			{
				PX.Objects.TX.Tax tax = PXSelect<PX.Objects.TX.Tax, Where<PX.Objects.TX.Tax.taxVendorID, Equal<Current<Vendor.bAccountID>>>>.Select(this);
				if (tax != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.TaxVendorDeleteErr);
				}
				if (row.Type == BAccountType.CombinedType)
				{
					// We shouldn't delete BAccount entity when it is in use by Customer entity
					PXTableAttribute tableAttr = cache.Interceptor as PXTableAttribute;
					tableAttr.BypassOnDelete(typeof(BAccount));
                    PXNoteAttribute.ForceRetain<VendorR.noteID>(cache);
                    PXParentAttribute.SetLeaveChildren<Location.bAccountID>(this.Caches[typeof(Location)], null, true);
				}
			}
		}

		protected virtual void Vendor_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			Vendor row = e.Row as Vendor;
			if (row != null && row.Type == BAccountType.CombinedType)
				ChangeBAccountType(row, BAccountType.CustomerType);
		}

		protected virtual void Vendor_AcctName_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			base.OnBAccountAcctNameFieldUpdated(cache, e);
		}

		protected virtual void Vendor_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				if (cmpany.Current == null || string.IsNullOrEmpty(cmpany.Current.BaseCuryID))
				{
					throw new PXException();
				}
				e.NewValue = cmpany.Current.BaseCuryID;
				e.Cancel = true;
			}
		}

		protected virtual void Vendor_TaxPeriodType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Vendor row = e.Row as Vendor;
			if (row != null && row.TaxPeriodType == PX.Objects.TX.VendorTaxPeriodType.FiscalPeriod)
			{
				row.TaxReportFinPeriod = true;
			}
		}
		
		protected virtual void Vendor_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Vendor row = e.Row as Vendor;

			if (row.TaxUseVendorCurPrecision != true)
			{
				GL.Company company = new PXSetup<GL.Company>(this).Current;
				CurrencyList cury = PXSelect<CurrencyList, Where<CurrencyList.curyID, Equal<Required<CurrencyList.curyID>>>>.Select(this, company.BaseCuryID);

				if (row.TaxReportPrecision == cury.DecimalPlaces)
				{
					cache.SetValueExt<Vendor.taxUseVendorCurPrecision>(row, true);
					cache.RaiseExceptionHandling<Vendor.taxUseVendorCurPrecision>(row, true, new PXSetPropertyException(Messages.UseCurrencyPrecisionWasSet, PXErrorLevel.Warning));
				}
			}

			if (row.TaxAgency == true && row.RetainageApply == true)
			{
				cache.RaiseExceptionHandling<Vendor.retainageApply>(row, true, new PXSetPropertyException(Messages.UseApplyRetainageForTaxAgency, PXErrorLevel.Error));
			}

			LocationExtAddress loc = DefLocation.SelectSingle();
			if (loc == null)
				return;
			PXSelectBase<CashAccount> select = new PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<Location.vCashAccountID>>>>(this);
			if (!String.IsNullOrEmpty(row.CuryID) && row.AllowOverrideCury != true)
			{
				CashAccount cashacct = select.Select(loc.VCashAccountID);
				if (cashacct != null)
				{
					if (row.CuryID != cashacct.CuryID)
					{
						PXUIFieldAttribute.SetWarning<Location.vCashAccountID>(DefLocation.Cache, loc, Messages.VendorCuryDifferentDefPayCury);
					}
				}
			}

			ValidateAPAndReclassificationAccountsAndSubs(cache, row);

			if (row != null && (row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType))
			{
				AR.Customer customer =
					SelectFrom<AR.Customer>
					.Where<AR.Customer.acctCD.IsEqual<@P.AsString>>
					.View
					.Select(this, row.AcctCD);
				AR.CustomerMaint.VerifyParentBAccountID<Vendor.parentBAccountID>(this, cache, customer, row);
			}
		}

		private void ValidateAPAndReclassificationAccountsAndSubs(PXCache sender, Vendor vendor)
		{
			int? APAccount = DefLocation.SelectSingle()?.VAPAccountID;
			if (APAccount == null) return;

			string errorMsg = null;

			bool subsEnbaled = PXAccess.FeatureInstalled<FeaturesSet.subAccount>();
			bool accountIdentical = vendor.PrebookAcctID == APAccount;

			if (accountIdentical && !subsEnbaled)
			{
				errorMsg = Messages.APAndReclassAccountBoxesShouldNotHaveTheSameAccounts;
			}
			else if (accountIdentical && subsEnbaled && vendor.PrebookSubID == DefLocation.Current.VAPSubID)
			{
				errorMsg = Messages.APAndReclassAccountSubaccountBoxesShouldNotHaveTheSameAccountSubaccountPairs;
			}

			if (errorMsg != null)
			{
				var errorEx = new PXSetPropertyException(errorMsg, PXErrorLevel.Error);

				var acctIDState = (PXFieldState)sender.GetStateExt<Vendor.prebookAcctID>(vendor);
				sender.RaiseExceptionHandling<Vendor.prebookAcctID>(vendor, acctIDState.Value, errorEx);

				var subIDState = (PXFieldState)sender.GetStateExt<Vendor.prebookSubID>(vendor);
				sender.RaiseExceptionHandling<Vendor.prebookSubID>(vendor, subIDState.Value, errorEx);
			}
		}

		private void CheckPayToVendorRelations(Vendor vendor, bool? isUnsuitableType)
		{
			if (vendor == null) return;

			if (isUnsuitableType == true
				&& (vendor.PayToVendorID != null 
					|| PXSelect<Vendor, Where<Vendor.payToVendorID, Equal<Current<Vendor.bAccountID>>>, OrderBy<Asc<Vendor.bAccountID>>>.SelectSingleBound(this, new object[] { vendor }).Any()))
			{
				throw new PXSetPropertyException(Messages.VendorInPayRelation, vendor.AcctCD);
			}
		}

		protected virtual void Vendor_Vendor1099_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CheckPayToVendorRelations(e.Row as Vendor, e.NewValue as bool?);
		}

		protected virtual void Vendor_IsLaborUnion_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CheckPayToVendorRelations(e.Row as Vendor, e.NewValue as bool?);
		}

		protected virtual void Vendor_TaxAgency_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CheckPayToVendorRelations(e.Row as Vendor, e.NewValue as bool?);
		}

		protected virtual void Vendor_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void Vendor_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				e.Cancel = true;
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == "PaymentDetails")
			{ 
				DefLocation.Current = DefLocation.Select();
			}

			// This very ugly crutch exists for one purpose - to force the LocationExtAddress Cache's
			// Current value to contain vPaymentMethodID when it is needed by Cash Account selector in
			// Payment Settings. We have to do this because the graph's two views Locations and DefLocation 
			// share the same cache, and Locations sometimes overwrites its Current with a null vPaymentMethodId.
			// This will exist until complete redesign of VendorMaint.
			// -
			if (viewName == "_LocationExtAddressVCashAccountID_PX.Objects.CA.CashAccount+cashAccountID_" &&
					(DefLocation.Current.VPaymentMethodID == null))
			{
				DefLocation.Current = DefLocation.Select();
			}

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		protected virtual void Vendor_VendorClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Vendor row = (Vendor)e.Row;
			VendorClass vc = (VendorClass)PXSelectorAttribute.Select<Vendor.vendorClassID>(cache, row, e.NewValue);
			this.doCopyClassSettings = false;
			if (vc != null)
			{
				doCopyClassSettings = true;
				if (cache.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (BAccount.Ask(Messages.Warning, Messages.VendorClassChangeWarning, MessageButtons.YesNo, false) == WebDialogResult.No)
					{
						doCopyClassSettings = false;
						BAccount.ClearDialog();
					}
				}
			}
		}

		protected virtual void Vendor_VendorClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DefLocation.Current = DefLocation.Select();

			VendorClass.RaiseFieldUpdated(cache, e.Row);

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
				if (branch != null && DefLocation.Current != null && DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
				{
					object cd = branch.BranchCD;
					this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(DefLocation.Current, ref cd);
					DefLocation.Current.LocationCD = (string)cd;
					DefLocation.Cache.Normalize();
				}
			}

			DefAddress.Current = DefAddress.Select();
			if (DefAddress.Current != null && DefAddress.Current.AddressID != null)
            {
                InitDefAddress(DefAddress.Current);
                DefAddress.Cache.MarkUpdated(DefAddress.Current);
            }

			Vendor row = (Vendor)e.Row;
			if (VendorClass.Current != null && doCopyClassSettings)
			{
				VendorClass.RaiseFieldUpdated(cache, e.Row);

				CopyAccounts(cache, row);

				foreach (LocationExtAddress location in Locations.Select())
				{
					InitVendorLocation(location, location.LocType, true);
				}
			}
		}

		protected virtual void Vendor_TaxAgency_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Vendor vendor = e.Row as Vendor;
			if (vendor == null) return;

			if (vendor.TaxAgency == true && e.OldValue as bool? != true)
			{
				vendor.PaymentsByLinesAllowed = false;
			}
		}

		#endregion

		#region LocationExtAddress Events

		[PXDBString(10, IsUnicode = true, BqlField = typeof(CR.Standalone.Location.vTaxZoneID))]
		[PXUIField(DisplayName = "Tax Zone", Required = false)]
		[PXDefault(typeof(Search<VendorClass.taxZoneID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TX.TaxZone.taxZoneID>), DescriptionField = typeof(TX.TaxZone.descr), CacheGlobal = true)]
		public virtual void LocationExtAddress_VTaxZoneID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(AccountAttribute), nameof(AccountAttribute.ControlAccountForModule), null)]
		public virtual void LocationExtAddress_CARAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(AccountAttribute), nameof(AccountAttribute.ControlAccountForModule), null)]
		public virtual void LocationExtAddress_CRetainageAcctID_CacheAttached(PXCache sender)
		{
		}


        public virtual void LocationExtAddress_VTaxZoneID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            LocationExtAddress row = (LocationExtAddress)e.Row;

            if (row == null)
                return;

			// Synchronize the customer tax zone id in default location record
			// with what we're setting to vendor tax zone id. WHY: we don't
			// want to see empty tax zone id in case we extend this vendor to Combined type
			// (vendor & customer) - in both Business Account and Customers screens,
			// the tax zone id UI field is by default pulled from Location.cTaxZoneID.
			// -
            if (BAccount.Current.Type == BAccountType.VendorType && (row.CTaxZoneID == null || (string)e.OldValue == row.CTaxZoneID))
                this.DefLocation.Cache.SetValue<Location.cTaxZoneID>(row, row.VTaxZoneID);
        }

		protected override void LocationExtAddress_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			LocationExtAddress record = (LocationExtAddress)e.Row;

			record.IsRemitAddressSameAsMain = (record.VRemitAddressID == record.VDefAddressID);
			record.IsRemitContactSameAsMain = (record.VRemitContactID == record.VDefContactID);

			FillPaymentDetails(record);

			base.LocationExtAddress_RowInserted(sender, e);
		}
	

		protected override void LocationExtAddress_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				LocationExtAddress row = (LocationExtAddress)e.Row;

				bool expenseAcctFieldsShouldBeRequired = BAccountAccessor.Current?.TaxAgency == true;

				PXUIFieldAttribute.SetRequired<LocationExtAddress.vExpenseAcctID>(sender, expenseAcctFieldsShouldBeRequired);
				PXUIFieldAttribute.SetRequired<LocationExtAddress.vExpenseSubID>(sender, expenseAcctFieldsShouldBeRequired);

				row.IsRemitAddressSameAsMain = (row.VRemitAddressID == row.VDefAddressID);
				row.IsRemitContactSameAsMain = (row.VRemitContactID == row.VDefContactID);	
				FillPaymentDetails(row);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.vCashAccountID>(sender, e.Row, String.IsNullOrEmpty(row.VPaymentMethodID)==false);

                if (this.VendorClass.Current != null)
                {
                    bool isRequired = (this.VendorClass.Current.RequireTaxZone ?? false) && row.IsDefault == true;
                    PXDefaultAttribute.SetPersistingCheck<LocationExtAddress.vTaxZoneID>(this.DefLocation.Cache, null, PXPersistingCheck.Nothing);
                    PXDefaultAttribute.SetPersistingCheck<LocationExtAddress.vTaxZoneID>(this.DefLocation.Cache, e.Row, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetRequired<LocationExtAddress.vTaxZoneID>(this.DefLocation.Cache, isRequired);
                }
			}

			base.LocationExtAddress_RowSelected(sender, e);
		}

		protected virtual void LocationExtAddress_VDefAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defAddressID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null);
		}

		protected virtual void LocationExtAddress_VDefContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defContactID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null);
		}

		protected virtual void LocationExtAddress_VRemitAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defAddressID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null);
		}

		protected virtual void LocationExtAddress_VRemitContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defContactID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null); 
		}

		protected virtual void LocationExtAddress_IsRemitContactSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress owner = (LocationExtAddress)e.Row;
			if (owner != null)
			{
				if (owner.IsRemitContactSameAsMain == true)
				{
					if (owner.VRemitContactID != owner.VDefContactID)
					{
						Contact contact = this.FindContact(owner.VRemitContactID);
						if (contact != null && contact.ContactID == owner.VRemitContactID)
						{
							this.RemitContact.Delete(contact);
						}
						owner.VRemitContactID = owner.VDefContactID;
						//if (cache.Locate(owner) != null)
						//  cache.Update(owner);
					}
				}

				if (owner.IsRemitContactSameAsMain == false)
				{
					if (owner.VRemitContactID != null)
					{
						if (owner.VRemitContactID == owner.VDefContactID)
						{
							Contact defContact = this.FindContact(owner.VDefContactID);
							Contact cont = PXCache<Contact>.CreateCopy(defContact);
							cont.ContactID = null;
							cont.BAccountID = owner.BAccountID;
						    cont.NoteID = null;
							cont.ContactType = ContactTypesAttribute.BAccountProperty;
							cont = (Contact)this.RemitContact.Cache.Insert(cont);
							owner.VRemitContactID = cont.ContactID;
							//if (cache.Locate(owner) != null)
							//  cache.Update(owner);
						}
					}
				}
			}
		}

		protected virtual void LocationExtAddress_IsRemitAddressSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress owner = (LocationExtAddress)e.Row;
			if (owner != null)
			{
				if (owner.IsRemitAddressSameAsMain == true)
				{
					if (owner.VRemitAddressID != owner.VDefAddressID)
					{
						Address extAddr = this.FindAddress(owner.VRemitAddressID);
						if (extAddr != null && extAddr.AddressID == owner.VRemitAddressID)
						{
							this.RemitAddress.Delete(extAddr);
						}
						owner.VRemitAddressID = owner.VDefAddressID;
						//if (cache.Locate(owner) != null)
						//  cache.Update(owner);
					}
				}
				else if (owner.VRemitAddressID != null)
				{
					if (owner.VRemitAddressID == owner.VDefAddressID)
					{
						Address defAddress = this.FindAddress(owner.VDefAddressID);
						Address addr = PXCache<Address>.CreateCopy(defAddress);
						addr.AddressID = null;
						addr.BAccountID = owner.BAccountID;
						addr = this.RemitAddress.Insert(addr);
						owner.VRemitAddressID = addr.AddressID;
						//if (cache.Locate(owner) != null)
						//  cache.Update(owner);
						formRemitAddress = addr;
					}
				}
			}
		}

		protected virtual void LocationExtAddress_CBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}
		protected virtual void LocationExtAddress_VPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			string oldValue = (string)e.OldValue;
			if (!String.IsNullOrEmpty(oldValue))
			{
				this.ClearPaymentDetails(row, oldValue, true);
			}
			cache.SetDefaultExt<LocationExtAddress.vCashAccountID>(e.Row);
			this.FillPaymentDetails(row);
			this.PaymentDetails.View.RequestRefresh();
		}
		protected virtual void LocationExtAddress_VPaymentMethodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			if (row == null)
			{
				e.Cancel = true;
			}
			else
			{
				if (this.VendorClass.Current != null && String.IsNullOrEmpty(this.VendorClass.Current.PaymentMethodID) == false)
				{
					e.NewValue = VendorClass.Current.PaymentMethodID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void LocationExtAddress_VCashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			if (row != null)
			{
				if (this.VendorClass.Current != null && this.VendorClass.Current.CashAcctID.HasValue 
					&& row.VPaymentMethodID == this.VendorClass.Current.PaymentMethodID)
				{
					e.NewValue = this.VendorClass.Current.CashAcctID;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_VPaymentByType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.paymentByType>(VendorClass.Current);
				e.Cancel = true;
			}
		}
		protected virtual void LocationExtAddress_VTaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.taxZoneID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VAPAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.aPAcctID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VAPSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.aPSubID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VExpenseAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			if (this.BAccountAccessor.Current != null)
			{
				BAccount acct = this.BAccountAccessor.Current;
				if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
				{
					VendorClass vClass = VendorClass.Current;
					if (vClass != null && vClass.ExpenseAcctID != null)
					{
						e.NewValue = vClass.ExpenseAcctID;
						e.Cancel = true;
					}
				}
			}
		}
		protected virtual void LocationExtAddress_VExpenseSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			if (this.BAccountAccessor.Current != null)
			{
				BAccount acct = this.BAccountAccessor.Current;
				if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
				{
					VendorClass vClass = VendorClass.Current;
					if (vClass != null && vClass.ExpenseSubID != null)
					{
						e.NewValue = vClass.ExpenseSubID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void LocationExtAddress_VRetainageAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress location = e.Row as LocationExtAddress;
			if (location == null) return;

			BAccount acct = BAccountAccessor.Current;
			if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
			{
				VendorClass vClass = VendorClass.Current;
				if (vClass != null && vClass.RetainageAcctID != null)
				{
					e.NewValue = vClass.RetainageAcctID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void LocationExtAddress_VRetainageSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress location = e.Row as LocationExtAddress;
			if (location == null) return;

			BAccount acct = BAccountAccessor.Current;
			if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
			{
				VendorClass vClass = VendorClass.Current;
				if (vClass != null && vClass.RetainageSubID != null)
				{
					e.NewValue = vClass.RetainageSubID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_VDiscountAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (VendorClass.Current != null)
            {
                e.NewValue = VendorClass.Cache.GetValue<VendorClass.discountAcctID>(VendorClass.Current);
                e.Cancel = true;
            }
        }

        protected virtual void LocationExtAddress_VDiscountSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (VendorClass.Current != null)
            {
                e.NewValue = VendorClass.Cache.GetValue<VendorClass.discountSubID>(VendorClass.Current);
                e.Cancel = true;
            }
        }

		protected virtual void LocationExtAddress_VFreightAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.freightAcctID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VFreightSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.freightSubID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VShipTermsID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.shipTermsID>(VendorClass.Current);
				e.Cancel = true;
			}
		}	

		protected virtual void LocationExtAddress_VRcptQtyAction_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.rcptQtyAction>(VendorClass.Current) ?? POReceiptQtyAction.AcceptButWarn;
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VPrintOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.printPO>(VendorClass.Current) ?? false;
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VEmailOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.emailPO>(VendorClass.Current) ?? false;
				e.Cancel = true;
			}
		}

		protected virtual void Location_VPaymentTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.paymentMethodID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected override void LocationExtAddress_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			base.LocationExtAddress_RowPersisting(cache, e);

			if (e.Cancel)
				return;

			LocationExtAddress location = (LocationExtAddress)e.Row;

			if (location == null)
				return;

			bool validationResult = ValidateLocation(cache, location);

			if (!validationResult)
			{
					Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(cache, location);

					if (errors.Any())
					{
						throw new PXException(Common.Messages.RecordCanNotBeSaved);
					}
		    }
		}

		protected override void LocationExtAddress_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			base.LocationExtAddress_RowPersisted(cache, e);
			LocationExtAddress row = e.Row as LocationExtAddress;
			if(row != null && e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Completed)
			{
				row.VDefAddressID = row.DefAddressID;
				row.VDefContactID = row.DefContactID;
			}
		}

	   #endregion

		#region NotificationRecipient Events
		protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
			if (contact == null)
			{
				switch (row.ContactType)
				{
					case VendorContactType.Primary:
						contact = DefContact.SelectWindowed(0, 1);
						break;
					case VendorContactType.Remittance:
						contact = RemitContact.SelectWindowed(0, 1);
						break;
					case VendorContactType.Shipping:
						contact = DefLocationContact.SelectWindowed(0, 1);
						break;
				}
			}
			if (contact != null)
				row.Email = contact.EMail;
		}
		#endregion

		#region Other Events

		protected virtual void VendorPaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if(e.Row!= null)
			{
				VendorPaymentMethodDetail row = (VendorPaymentMethodDetail)e.Row;
				PaymentMethodDetail iTempl = this.FindTemplate(row);
				bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
				PXDefaultAttribute.SetPersistingCheck<VendorPaymentMethodDetail.detailValue>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}		
		#endregion

		#region Auxillary Functions

		protected virtual void FillPaymentDetails(LocationExtAddress account)
		{
			if (account != null)
			{
				if (!string.IsNullOrEmpty(account.VPaymentMethodID))
				{
					PaymentMethod paymentTypeDef = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, account.VPaymentMethodID);
					if (paymentTypeDef != null)
                    {
                        List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
                        foreach (PaymentMethodDetail it in this.PaymentTypeDetails.Select(account.VPaymentMethodID))
                        {
                            VendorPaymentMethodDetail detail = null;
                            foreach (VendorPaymentMethodDetail iPDet in this.PaymentDetails.Select(account.BAccountID, account.LocationID, account.VPaymentMethodID))
                            {
                                if (iPDet.DetailID == it.DetailID)
                                {
                                    detail = iPDet;
                                    break;
                                }
                            }
                            if (detail == null)
                            {
                                toAdd.Add(it);
                            }
                        }
                        using (ReadOnlyScope rs = new ReadOnlyScope(this.PaymentDetails.Cache))
                        {
                            foreach (PaymentMethodDetail it in toAdd)
                            {
                                VendorPaymentMethodDetail detail = new VendorPaymentMethodDetail();
                                detail.BAccountID = account.BAccountID;
                                detail.LocationID = account.LocationID;
                                detail.PaymentMethodID = account.VPaymentMethodID;
                                detail.DetailID = it.DetailID;
                                detail = this.PaymentDetails.Insert(detail);
                            }
                            if (toAdd.Count > 0)
                            {
                                this.PaymentDetails.View.RequestRefresh();
                            }
                        }
                    }
				}
			}

		}

		protected virtual void ClearPaymentDetails(LocationExtAddress account, string paymentTypeID, bool clearNewOnly)
		{
			foreach (VendorPaymentMethodDetail it in this.PaymentDetails.Select(account.BAccountID, account.LocationID, paymentTypeID))
			{
				bool doDelete = true;
				if (clearNewOnly)
				{
					PXEntryStatus status = this.PaymentDetails.Cache.GetStatus(it);
					doDelete = (status == PXEntryStatus.Inserted);
				}
				if (doDelete)
					this.PaymentDetails.Delete(it);
			}
		}

		protected override void SetAsDefault(ContactExtAddress row)
		{
			LocationExtAddress location = DefLocation.Select();
			if (location != null)
			{
				if (location.VRemitContactID != null && (location.VRemitContactID == location.VDefContactID))
				{
					int? defContactID = (row != null) ? row.ContactID : null;
					location.VRemitContactID = defContactID;

					this.DefLocation.Cache.MarkUpdated(location);
				}
			}
			base.SetAsDefault(row);
		}

		protected override void InitDefAddress(Address aAddress)
		{
			base.InitDefAddress(aAddress);
			if (CurrentVendor.Cache.GetStatus(CurrentVendor.Current) == PXEntryStatus.Inserted || doCopyClassSettings)
			{
				aAddress.CountryID = VendorClass.Current?.CountryID ?? aAddress.CountryID;
			}
		}

		public virtual void CopyAccounts(PXCache sender, Vendor row)
		{
			DefLocation.Current = DefLocation.Select();

			sender.SetDefaultExt<Vendor.discTakenAcctID>(row);
			sender.SetDefaultExt<Vendor.discTakenSubID>(row);
			sender.SetDefaultExt<Vendor.prepaymentAcctID>(row);
			sender.SetDefaultExt<Vendor.prepaymentSubID>(row);
			sender.SetDefaultExt<Vendor.prebookAcctID>(row);
			sender.SetDefaultExt<Vendor.prebookSubID>(row);
			sender.SetDefaultExt<Vendor.pOAccrualAcctID>(row);
			sender.SetDefaultExt<Vendor.pOAccrualSubID>(row);
			sender.SetDefaultExt<Vendor.curyID>(row);
			sender.SetDefaultExt<Vendor.curyRateTypeID>(row);
			sender.SetDefaultExt<Vendor.priceListCuryID>(row);
			
			if (this.DefLocation.Current != null)
			{
				this.DefLocation.Cache.MarkUpdated(this.DefLocation.Current);
			}

			sender.SetDefaultExt<Vendor.allowOverrideCury>(row);
			sender.SetDefaultExt<Vendor.allowOverrideRate>(row);
			sender.SetDefaultExt<Vendor.termsID>(row);

			sender.SetDefaultExt<Vendor.localeName>(row);
			sender.SetDefaultExt<Vendor.groupMask>(row);

			sender.SetDefaultExt<Vendor.retainageApply>(row);
			sender.SetDefaultExt<Vendor.paymentsByLinesAllowed>(row);
		}

		protected virtual void Aggregate(VendorBalanceSummary aRes, CuryAPHistory aSrc)
		{
			aRes.Init();
			aRes.VendorID = aSrc.VendorID;
			aRes.Balance += aSrc.FinYtdBalance ?? Decimal.Zero;
			aRes.DepositsBalance += aSrc.FinYtdDeposits ?? Decimal.Zero;
			aRes.RetainageBalance += aSrc.FinYtdRetainageWithheld - aSrc.FinYtdRetainageReleased;
		}

		private static List<Type> initLocationFields = new List<Type>
		{
			typeof(Location.vCarrierID),
			typeof(Location.vFOBPointID),
			typeof(Location.vLeadTime),
			typeof(Location.vShipTermsID),
			typeof(Location.vBranchID),
			typeof(Location.vTaxZoneID),
			typeof(Location.vExpenseAcctID),
			typeof(Location.vExpenseSubID),
			typeof(Location.vDiscountAcctID),
			typeof(Location.vDiscountSubID),
			typeof(Location.vFreightAcctID),
			typeof(Location.vFreightSubID),
			typeof(Location.vRcptQtyAction),
			typeof(Location.vRcptQtyMin),
			typeof(Location.vRcptQtyMax),
			typeof(Location.vAPAccountID),
			typeof(Location.vAPSubID),
			typeof(Location.vCashAccountID),
			typeof(Location.vPaymentMethodID),
			typeof(Location.vPaymentByType),
			typeof(Location.vShipTermsID),
			typeof(Location.vRcptQtyAction),
			typeof(Location.vPrintOrder),
			typeof(Location.vEmailOrder),
			typeof(Location.vRemitAddressID),
			typeof(Location.vRemitContactID),
			typeof(Location.vTaxCalcMode),
			typeof(Location.vRetainageAcctID),
			typeof(Location.vRetainageSubID),
		};

        public virtual void InitVendorLocation(IBqlTable location, string aLocationType, bool onlySetDefault)
        {
            if (location == null) return;

            PXCache cache = this.Caches[location.GetType()];
            foreach (var field in initLocationFields)
            {
                if (onlySetDefault || cache.GetValue(location, field.Name) == null)
                    cache.SetDefaultExt(location, field.Name);
            }
            cache.SetValue<Location.locType>(location, aLocationType);
        }

		protected virtual PaymentMethodDetail FindTemplate(VendorPaymentMethodDetail aDet)
		{			
			 PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
			        	                And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
                                        And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                            Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}

		protected virtual bool ValidateDetail(VendorPaymentMethodDetail aRow)
		{
			PaymentMethodDetail iTempl = this.FindTemplate(aRow);
			if (iTempl != null && (iTempl.IsRequired ?? false) && String.IsNullOrEmpty(aRow.DetailValue))
			{
				this.PaymentDetails.Cache.RaiseExceptionHandling<VendorPaymentMethodDetail.detailValue>(aRow, aRow.DetailValue, new PXSetPropertyException(CA.Messages.ERR_RequiredValueNotEnterd));
				return false;
			}
			return true;
		}

		private bool CanDelete(Vendor row)
		{
			if (row != null)
			{
				PX.Objects.TX.Tax tax = PXSelect<PX.Objects.TX.Tax, Where<PX.Objects.TX.Tax.taxVendorID, Equal<Current<Vendor.bAccountID>>>>.Select(this);
				if (tax != null)
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region Private members
		private bool doCopyClassSettings;
		#endregion

		#region Domain Functions

		protected virtual bool ValidateLocation(PXCache cache, LocationExtAddress location)
		{
			bool res = true;
			VendorR acct = this.BAccountAccessor.Current;
			if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
			{
				if (VendorClass.Current != null && VendorClass.Current.RequireTaxZone == true && location.IsDefault == true && location.VTaxZoneID == null)
				{
					res &= ValidationHelper.SetErrorEmptyIfNull<Location.vTaxZoneID>(cache, location, location.VTaxZoneID);
				}

				res &= LocationValidator.ValidateVendorLocation(cache, acct, location);

				PXSelectBase<CashAccount> select = new PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<Location.vCashAccountID>>>>(this);
				if (!String.IsNullOrEmpty(acct.CuryID) && acct.AllowOverrideCury != true)
				{
					CashAccount cashacct = select.Select(location.VCashAccountID);
					if (cashacct != null)
					{
						if (acct.CuryID != cashacct.CuryID)
						{
							PXUIFieldAttribute.SetWarning<Location.vCashAccountID>(cache, location, Messages.VendorCuryDifferentDefPayCury);
						}
					}
				}

				foreach (VendorPaymentMethodDetail it in this.PaymentDetails.Select())
				{
					if (!ValidateDetail(it))
					{
						res = false;
					}
				}
			}

			return res;
		}

		#endregion

		#region Soap-related handlers
		protected bool addressAdded;
		protected override void Address_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall)
			{
				if (!addressAdded)
				{
					base.Address_RowInserted(cache, e);
					addressAdded = true;
				}
				else if (DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
				{
					DefLocation.Current.VRemitAddressID = DetailInserted<Address>(cache, (Address)e.Row, null);
					DefLocation.Current.IsRemitAddressSameAsMain = false;
				}
			}
		}
		protected bool contactAdded;
		protected override void Contact_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall)
			{
				if (!contactAdded)
				{
					base.Contact_RowInserted(cache, e);
					contactAdded = true;
				}
				else if (DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
				{
					DefLocation.Current.VRemitContactID = DetailInserted<Contact>(cache, (Contact)e.Row, null);
					DefLocation.Current.IsRemitContactSameAsMain = false;
				}
			}
		}
		#endregion
	}
}

