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
using PX.Objects.CR.Extensions;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GraphExtensions.ExtendBAccount;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.SM;
using Branch = PX.SM.Branch;
using PX.Objects.IN;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CR.Extensions.Relational;
using PX.Objects.GDPR;
using PX.Data.ReferentialIntegrity.Attributes;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AP
{
	#region Specialized DAC Classes
	[PXSubstitute(GraphType = typeof(VendorMaint))]
    [PXCacheName(Messages.Vendor)]
	[System.SerializableAttribute()]
	public partial class VendorR : Vendor
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		public new abstract class primaryContactID : PX.Data.BQL.BqlInt.Field<primaryContactID> { }

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

	public class VendorMaint : PXGraph<VendorMaint, VendorR>
	{
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
		[PXDBDefault(typeof(NotificationSource.sourceID))]
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

		[PXViewName(CR.Messages.BAccount)]
		public PXSelect<
				VendorR,
			Where2<
				Match<Current<AccessInfo.userName>>,
				And<Where<BAccount.type, Equal<BAccountType.vendorType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>
			BAccount;

		public virtual PXSelectBase<VendorR> BAccountAccessor => BAccount;

		[PXHidden]
		public PXSelect<CRLocation>
			BaseLocations;

		[PXHidden]
		public PXSelect<Address>
			AddressDummy;

		[PXHidden]
		public PXSelect<Contact>
			ContactDummy;

		public PXSelect<BAccountItself, Where<BAccount.bAccountID, Equal<Optional<BAccount.bAccountID>>>> CurrentBAccountItself;

		public PXSetup<GL.Company> cmpany;

		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentVendor;
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

			// right baccount type for redirect from empty Pay-to Vendor field
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });

			PXUIFieldAttribute.SetDisplayName<VendorR.acctName>(BAccount.Cache, "Account Name");
		}

		[PXHidden]
		public PXSelect<INItemXRef> xrefs;

		#endregion

		#region Standard Buttons

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable cancel(PXAdapter a)
		{
			foreach (VendorR e in (new PXCancel<VendorR>(this, "Cancel")).Press(a))
			{
				BAccount acct = e as BAccount;
				if (acct != null)
				{
					if (BAccountAccessor.Cache.GetStatus(e) == PXEntryStatus.Inserted)
					{
						BAccount e1 = PXSelectReadonly<BAccountItself, Where<BAccountItself.acctCD, Equal<Required<BAccountItself.acctCD>>,
							And<BAccountItself.bAccountID, NotEqual<Required<BAccountItself.bAccountID>>>>>.Select(this, acct.AcctCD, acct.BAccountID);
						if (e1 != null && (e1.BAccountID != acct.BAccountID))
						{
							BAccountAccessor.Cache.RaiseExceptionHandling<BAccount.acctCD>(e, null, new PXSetPropertyException(EP.Messages.BAccountExists));
						}
					}
				}
				yield return e;
			}
		}

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


		public PXAction<VendorR> viewBusnessAccount;
		[PXUIField(DisplayName = Messages.ViewBusnessAccount, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
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

		#endregion

		#region Overrides
		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);
			//if (!map.ContainsKey(typeof(Address)))
			//map.Add(typeof(Address), typeof(Address));
            Caches.AddCacheMappingsWithInheritance(this,typeof(VendorR)); 
		    Caches.AddCacheMappingsWithInheritance(this,typeof(CRLocation)); 
			//var vendorCache = Caches[typeof(VendorR)];
			//var locationCache = Caches[typeof(CRLocation)];
		}

		public override void Persist()
		{
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

		protected virtual void Vendor_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Vendor row = (Vendor)e.Row;

			if (row == null)
				return;

			PXEntryStatus status = cache.GetStatus(row);

			bool isExistingRecord = !(status == PXEntryStatus.Inserted && row.AcctCD == null);

			viewBusnessAccount.SetEnabled(isExistingRecord);
			newBillAdjustment.SetEnabled(isExistingRecord);
			newManualCheck.SetEnabled(isExistingRecord);
			viewRestrictionGroups.SetEnabled(isExistingRecord);
			ChangeID.SetEnabled(isExistingRecord && row.IsBranch != true);

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
			
			PXUIFieldAttribute.SetEnabled<Vendor.vendor1099>(cache, row, row.IsBranch != true);
			PXUIFieldAttribute.SetEnabled<Vendor.taxAgency>(cache, row, row.IsBranch != true);

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

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXExcludeRowsFromReferentialIntegrityCheck(
			ForeignTableExcludingConditions = typeof(ExcludeWhen<BAccount2>
				.Joined<On<BAccount2.bAccountID.IsEqual<BAccount.parentBAccountID>>>
				.Satisfies<Where<BAccount2.isBranch.IsEqual<True>>>))]
		protected virtual void _(Events.CacheAttached<BAccount.parentBAccountID> e) { }

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
				if (row.Type == BAccountType.CombinedType || row.IsBranch == true)
				{
					// We shouldn't delete BAccount entity when it is in use by Customer entity
					PXTableAttribute tableAttr = cache.Interceptor as PXTableAttribute;
					tableAttr.BypassOnDelete(typeof(BAccount));
                    PXNoteAttribute.ForceRetain<VendorR.noteID>(cache);
				}
			}
		}

		protected virtual void Vendor_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			if (!(e.Row is Vendor row)) return;

			string newBAccountType = null;
			if (row.Type == BAccountType.CombinedType)
			{
				newBAccountType = BAccountType.CustomerType;
			}
			else if (row.Type == BAccountType.VendorType && row.IsBranch == true)
			{
				newBAccountType = BAccountType.BranchType;
			}

			if (!string.IsNullOrEmpty(newBAccountType))
			{
				ChangeBAccountType(row, newBAccountType);
			}
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
			var defLocationExt = this.GetExtension<DefLocationExt>();

			int? APAccount = defLocationExt.DefLocation.SelectSingle()?.VAPAccountID;
			if (APAccount == null) return;

			string errorMsg = null;

			bool subsEnbaled = PXAccess.FeatureInstalled<FeaturesSet.subAccount>();
			bool accountIdentical = vendor.PrebookAcctID == APAccount;

			if (accountIdentical && !subsEnbaled)
			{
				errorMsg = Messages.APAndReclassAccountBoxesShouldNotHaveTheSameAccounts;
			}
			else if (accountIdentical && subsEnbaled && vendor.PrebookSubID == defLocationExt.DefLocation.Current.VAPSubID)
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
				var defLocationExt = this.GetExtension<DefLocationExt>();
				defLocationExt.DefLocation.Current = defLocationExt.DefLocation.Select();
			}

			// This very ugly crutch exists for one purpose - to force the Location Cache's
			// Current value to contain vPaymentMethodID when it is needed by Cash Account selector in
			// Payment Settings. We have to do this because the graph's two views Locations and DefLocation 
			// share the same cache, and Locations sometimes overwrites its Current with a null vPaymentMethodId.
			// This will exist until complete redesign of VendorMaint.
			// -
			if (viewName == "_LocationVCashAccountID_PX.Objects.CA.CashAccount+cashAccountID_"
				&& (this.Caches[typeof(CRLocation)].Current as CRLocation)?.VPaymentMethodID == null)
			{
				var defLocationExt = this.GetExtension<DefLocationExt>();
				defLocationExt.DefLocation.Current = defLocationExt.DefLocation.Select();
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
			var defLocationExt = this.GetExtension<DefLocationExt>();
			defLocationExt.DefLocation.Current = defLocationExt.DefLocation.Select();

			VendorClass.RaiseFieldUpdated(cache, e.Row);

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
				if (branch != null
					&& defLocationExt.DefLocation.Current != null
					&& defLocationExt.DefLocation.Cache.GetStatus(defLocationExt.DefLocation.Current) == PXEntryStatus.Inserted)
				{
					object cd = branch.BranchCD;
					defLocationExt.DefLocation.Cache.RaiseFieldUpdating<CRLocation.locationCD>(defLocationExt.DefLocation.Current, ref cd);
					defLocationExt.DefLocation.Current.LocationCD = (string)cd;
					defLocationExt.DefLocation.Cache.Normalize();
				}
			}

			var defContactAddress = this.GetExtension<DefContactAddressExt>();
			defContactAddress.DefAddress.Current = defContactAddress.DefAddress.Select();

			if (defContactAddress.DefAddress.Current != null && defContactAddress.DefAddress.Current.AddressID != null)
			{
				defContactAddress.InitDefAddress(defContactAddress.DefAddress.Current);
				defContactAddress.DefAddress.Cache.MarkUpdated(defContactAddress.DefAddress.Current);
			}

			Vendor row = (Vendor)e.Row;
			if (VendorClass.Current != null && doCopyClassSettings)
			{
				VendorClass.RaiseFieldUpdated(cache, e.Row);

				CopyAccounts(cache, row);

				var locationDetails = this.GetExtension<LocationDetailsExt>();
				foreach (CRLocation location in locationDetails.Locations.Select())
				{
					defLocationExt.InitLocation(location, location.LocType, true);
					locationDetails.Locations.Cache.MarkUpdated(location);
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
						var defContactAddress = this.GetExtension<DefContactAddressExt>();
						contact = defContactAddress.DefContact.SelectWindowed(0, 1);
						break;
					case VendorContactType.Remittance:
						var defLocationExt = this.GetExtension<DefLocationExt>();
						contact = defLocationExt.RemitContact.SelectWindowed(0, 1);
						break;
					case VendorContactType.Shipping:
						defLocationExt = this.GetExtension<DefLocationExt>();
						contact = defLocationExt.DefLocationContact.SelectWindowed(0, 1);
						break;
				}
			}
			if (contact != null)
				row.Email = contact.EMail;
		}
		#endregion

		#region Auxillary Functions

		public virtual void CopyAccounts(PXCache sender, Vendor row)
		{
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

		protected void ChangeBAccountType(BAccount descendantEntity, string type)
		{
			BAccountItself baccount = CurrentBAccountItself.SelectSingle(descendantEntity.BAccountID);
			if (baccount != null)
			{
				baccount.Type = type;
				CurrentBAccountItself.Update(baccount);
			}
		}

		#endregion

		#region Private members
		private bool doCopyClassSettings;
		#endregion

		#region Extensions

		#region Details

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class PaymentDetailsExt : PXGraphExtension<VendorMaint>
		{
			#region Views

			public PXSelectJoin<
					VendorPaymentMethodDetail,
				InnerJoin<PaymentMethod,
					On<VendorPaymentMethodDetail.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
				InnerJoin<PaymentMethodDetail,
					On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
					And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
					And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
						Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>>,
				Where<
					VendorPaymentMethodDetail.bAccountID, Equal<Optional<CRLocation.bAccountID>>,
					And<VendorPaymentMethodDetail.locationID, Equal<Optional<CRLocation.locationID>>,
					And<VendorPaymentMethodDetail.paymentMethodID, Equal<Optional<CRLocation.vPaymentMethodID>>>>>,
				OrderBy<
					Asc<PaymentMethodDetail.orderIndex>>>
				PaymentDetails;

			public PXSelect<
					PaymentMethodDetail,
				Where<
					PaymentMethodDetail.paymentMethodID, Equal<Optional<CRLocation.vPaymentMethodID>>,
					And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
						Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>
				PaymentTypeDetails;

			#endregion

			#region Events

			[PXOverride]
			public virtual IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows, ExecuteSelectDelegate ExecuteSelect)
			{
				if (viewName == nameof(PaymentDetails))
				{
					FillPaymentDetails(Base.GetExtension<DefLocationExt>().DefLocation.SelectSingle());
				}

				return ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}

			#region CacheAttached

			[PXDBDefault(typeof(CRLocation.bAccountID))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<VendorPaymentMethodDetail.bAccountID> e) { }

			[PXDBDefault(typeof(CRLocation.locationID))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<VendorPaymentMethodDetail.locationID> e) { }

			#endregion

			#region Field-level

			protected virtual void _(Events.FieldUpdated<CRLocation, CRLocation.vPaymentMethodID> e)
			{
				CRLocation row = e.Row;

				string oldValue = (string)e.OldValue;
				if (!String.IsNullOrEmpty(oldValue))
				{
					ClearPaymentDetails(row, oldValue, true);
				}

				e.Cache.SetDefaultExt<CRLocation.vCashAccountID>(e.Row);

				FillPaymentDetails(row);

				PaymentDetails.View.RequestRefresh();
			}

			#endregion

			#region Row-level

			protected virtual void _(Events.RowInserted<CRLocation> e)
			{
				FillPaymentDetails(e.Row);
			}

			protected virtual void _(Events.RowSelected<VendorPaymentMethodDetail> e)
			{
				if (e.Row != null)
				{
					VendorPaymentMethodDetail row = (VendorPaymentMethodDetail)e.Row;
					PaymentMethodDetail iTempl = this.FindTemplate(row);
					bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
					PXDefaultAttribute.SetPersistingCheck<VendorPaymentMethodDetail.detailValue>(e.Cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				}
			}

			protected virtual void _(Events.RowPersisting<CRLocation> e)
			{
				if (e.Cancel)
					return;

				CRLocation location = e.Row;

				if (location == null)
					return;

				bool validationResult = ValidateLocationPaymentMethod(e.Cache, location);

				if (!validationResult)
				{
					Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(e.Cache, location);

					if (errors.Any())
					{
						throw new PXException(Common.Messages.RecordCanNotBeSaved);
					}
				}
			}

			#endregion

			#endregion

			#region Methods

			public virtual PaymentMethodDetail FindTemplate(VendorPaymentMethodDetail aDet)
			{
				PaymentMethodDetail res = PXSelect<
						PaymentMethodDetail,
					Where<
						PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
						And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
						And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
							Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>
					.Select(Base, aDet.PaymentMethodID, aDet.DetailID);

				return res;
			}

			public virtual bool ValidateLocationPaymentMethod(PXCache cache, CRLocation location)
			{
				bool res = true;
				VendorR acct = Base.BAccountAccessor.Current;
				if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
				{
					foreach (VendorPaymentMethodDetail it in this.PaymentDetails.Select())
					{
						if (!ValidatePaymentDetail(it))
						{
							res = false;
						}
					}
				}

				return res;
			}

			public virtual bool ValidatePaymentDetail(VendorPaymentMethodDetail aRow)
			{
				PaymentMethodDetail iTempl = this.FindTemplate(aRow);
				if (iTempl != null && (iTempl.IsRequired ?? false) && String.IsNullOrEmpty(aRow.DetailValue))
				{
					this.PaymentDetails.Cache.RaiseExceptionHandling<VendorPaymentMethodDetail.detailValue>(aRow, aRow.DetailValue, new PXSetPropertyException(CA.Messages.ERR_RequiredValueNotEnterd));
					return false;
				}
				return true;
			}

			public virtual void FillPaymentDetails(CRLocation location)
			{
				if (location != null)
				{
					if (!string.IsNullOrEmpty(location.VPaymentMethodID))
					{
						PaymentMethod paymentTypeDef = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(Base, location.VPaymentMethodID);
						if (paymentTypeDef != null)
						{
							List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
							foreach (PaymentMethodDetail it in PaymentTypeDetails.Select(location.VPaymentMethodID))
							{
								VendorPaymentMethodDetail detail = null;
								foreach (VendorPaymentMethodDetail iPDet in PaymentDetails.Select(location.BAccountID, location.LocationID, location.VPaymentMethodID))
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
							using (ReadOnlyScope rs = new ReadOnlyScope(PaymentDetails.Cache))
							{
								foreach (PaymentMethodDetail it in toAdd)
								{
									VendorPaymentMethodDetail detail = new VendorPaymentMethodDetail();
									detail.BAccountID = location.BAccountID;
									detail.LocationID = location.LocationID;
									detail.PaymentMethodID = location.VPaymentMethodID;
									detail.DetailID = it.DetailID;
									detail = PaymentDetails.Insert(detail);
								}
								if (toAdd.Count > 0)
								{
									PaymentDetails.View.RequestRefresh();
								}
							}
						}
					}
				}
			}

			public virtual void ClearPaymentDetails(CRLocation location, string paymentTypeID, bool clearNewOnly)
			{
				foreach (VendorPaymentMethodDetail it in PaymentDetails.Select(location.BAccountID, location.LocationID, paymentTypeID))
				{
					bool doDelete = true;
					if (clearNewOnly)
					{
						PXEntryStatus status = PaymentDetails.Cache.GetStatus(it);
						doDelete = (status == PXEntryStatus.Inserted);
					}
					if (doDelete)
						PaymentDetails.Delete(it);
				}
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefContactAddressExt : DefContactAddressExt<VendorMaint, VendorR, VendorR.acctName>
			.WithCombinedTypeValidation
		{
			#region Events

			protected virtual void _(Events.RowInserting<Address> e)
			{
				Address addr = e.Row as Address;
				if (addr == null)
					return;

				if (addr.AddressID == null)
				{
					e.Cancel = true;
				}
				else
				{
					InitDefAddress(addr);
				}
			}

			#endregion

			#region Methods

			public virtual void InitDefAddress(Address aAddress)
			{
				if (Base.CurrentVendor.Cache.GetStatus(Base.CurrentVendor.Current) == PXEntryStatus.Inserted || Base.doCopyClassSettings)
				{
					aAddress.CountryID = Base.VendorClass.Current?.CountryID ?? aAddress.CountryID;
				}
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefLocationExt : DefLocationExt<VendorMaint, DefContactAddressExt, LocationDetailsExt, VendorR, VendorR.bAccountID, VendorR.defLocationID>
			.WithUIExtension
			.WithCombinedTypeValidation
		{
			#region State

			public override List<Type> InitLocationFields => new List<Type>
			{
				typeof(CRLocation.vCarrierID),
				typeof(CRLocation.vFOBPointID),
				typeof(CRLocation.vLeadTime),
				typeof(CRLocation.vShipTermsID),
				typeof(CRLocation.vBranchID),
				typeof(CRLocation.vTaxZoneID),
				typeof(CRLocation.vExpenseAcctID),
				typeof(CRLocation.vExpenseSubID),
				typeof(CRLocation.vDiscountAcctID),
				typeof(CRLocation.vDiscountSubID),
				typeof(CRLocation.vFreightAcctID),
				typeof(CRLocation.vFreightSubID),
				typeof(CRLocation.vRcptQtyAction),
				typeof(CRLocation.vRcptQtyMin),
				typeof(CRLocation.vRcptQtyMax),
				typeof(CRLocation.vAPAccountID),
				typeof(CRLocation.vAPSubID),
				typeof(CRLocation.vCashAccountID),
				typeof(CRLocation.vPaymentMethodID),
				typeof(CRLocation.vPaymentByType),
				typeof(CRLocation.vShipTermsID),
				typeof(CRLocation.vRcptQtyAction),
				typeof(CRLocation.vPrintOrder),
				typeof(CRLocation.vEmailOrder),
				typeof(CRLocation.vRemitAddressID),
				typeof(CRLocation.vRemitContactID),
				typeof(CRLocation.vTaxCalcMode),
				typeof(CRLocation.vRetainageAcctID),
				typeof(CRLocation.vRetainageSubID),
				//typeof(CRLocation.vDefAddressID),
				//typeof(CRLocation.vDefContactID),
			};

			#endregion

			#region Views

			[PXViewName(PO.Messages.PORemitContact)]
			public PXSelect<
					Contact,
				Where<
					Contact.bAccountID, Equal<Current<CRLocation.bAccountID>>,
					And<Contact.contactID, Equal<Current<CRLocation.vRemitContactID>>>>>
				RemitContact;

			[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
			public virtual IEnumerable remitContact()
			{
				return SelectEntityByKey<Contact, Contact.contactID, CRLocation.vRemitContactID, CRLocation.overrideRemitContact>();
			}

			[PXViewName(PO.Messages.PORemitAddress)]
			public PXSelect<
					Address,
				Where<
					Address.bAccountID, Equal<Current<CRLocation.bAccountID>>,
					And<Address.addressID, Equal<Current<CRLocation.vRemitAddressID>>>>>
				RemitAddress;

			[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
			public virtual IEnumerable remitAddress()
			{
				return SelectEntityByKey<Address, Address.addressID, CRLocation.vRemitAddressID, CRLocation.overrideRemitAddress>();
			}

			#endregion

			#region Actions

			public PXAction<VendorR> viewRemitOnMap;
			[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXButton()]
			public virtual IEnumerable ViewRemitOnMap(PXAdapter adapter)
			{
				BAccountUtility.ViewOnMap(this.RemitAddress.SelectSingle());
				return adapter.Get();
			}

			[PXOverride]
			public override bool DoValidateAddresses(ValidateAddressesDelegate baseDel)
			{
				base.DoValidateAddresses(baseDel);

				Address remitAddress = this.RemitAddress.SelectSingle();
				if (remitAddress != null && remitAddress.IsValidated == false)
				{
					if (PXAddressValidator.Validate<Address>(Base, remitAddress, true))
					{
						return true;
					}
				}

				return false;
			}

			#endregion

			#region Events

			#region CacheAttached

			[PXDBInt()]
			[PXDBChildIdentity(typeof(CRLocation.locationID))]
			[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
			[PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<VendorR.bAccountID>>>>),
				DescriptionField = typeof(Location.locationCD),
				DirtyRead = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected override void _(Events.CacheAttached<VendorR.defLocationID> e) { }

			[PXDBChildIdentity(typeof(Address.addressID))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vRemitAddressID> e) { }

			[PXDBChildIdentity(typeof(Contact.contactID))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vRemitContactID> e) { }

			[PXDBString(10, IsUnicode = true, BqlField = typeof(CR.Standalone.Location.vTaxZoneID))]
			[PXUIField(DisplayName = "Tax Zone", Required = false)]
			[PXDefault(typeof(Search<VendorClass.taxZoneID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXSelector(typeof(Search<TX.TaxZone.taxZoneID>), DescriptionField = typeof(TX.TaxZone.descr), CacheGlobal = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected override void _(Events.CacheAttached<CRLocation.vTaxZoneID> e) { }

			[Account(null, typeof(Search<Account.accountID,
				Where2<
					Match<Current<AccessInfo.userName>>,
					And<Account.active, Equal<True>,
					And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AR Account", Required = true)] // remove ControlAccountForModule
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected override void _(Events.CacheAttached<CRLocation.cARAccountID> e) { }

			[Account(DisplayName = "Retainage Receivable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)] // remove ControlAccountForModule
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected override void _(Events.CacheAttached<CRLocation.cRetainageAcctID> e) { }

			[CashAccount(typeof(Search2<
					CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount,
					On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
				Where2<Match<Current<AccessInfo.userName>>,
					And<CashAccount.clearingAccount, Equal<False>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<CRLocation.vPaymentMethodID>>,
					And<PaymentMethodAccount.useForAP, Equal<True>>>>>>),
				Visibility = PXUIVisibility.Visible)]   // remove vBranchID from attribute
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected override void _(Events.CacheAttached<CRLocation.vCashAccountID> e) { }

			[PXFormula(typeof(Switch<Case<Where<CRLocation.vRemitAddressID, Equal<CurrentValue<AP.VendorR.defAddressID>>>, False>, True>))]
			[PXMergeAttributes(Method = MergeMethod.Append)] // for optimized export
			protected virtual void _(Events.CacheAttached<CRLocation.overrideRemitAddress> e) { }

			[PXFormula(typeof(Switch<Case<Where<CRLocation.vRemitContactID, Equal<CurrentValue<AP.VendorR.defContactID>>>, False>, True>))]
			[PXMergeAttributes(Method = MergeMethod.Append)] // for optimized export
			protected virtual void _(Events.CacheAttached<CRLocation.overrideRemitContact> e) { }

			#endregion

			#region Field-level

			protected virtual void _(Events.FieldUpdated<CRLocation, CRLocation.vTaxZoneID> e)
			{
				CRLocation row = e.Row;

				if (row == null)
					return;

				// Synchronize the customer tax zone id in default location record
				// with what we're setting to vendor tax zone id. WHY: we don't
				// want to see empty tax zone id in case we extend this vendor to Combined type
				// (vendor & customer) - in both Business Account and Customers screens,
				// the tax zone id UI field is by default pulled from Location.cTaxZoneID.
				// -
				if (Base.BAccount.Current.Type == BAccountType.VendorType && (row.CTaxZoneID == null || (string)e.OldValue == row.CTaxZoneID))
					this.DefLocation.Cache.SetValue<CRLocation.cTaxZoneID>(row, row.VTaxZoneID);
			}

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vPaymentMethodID> e)
			{
				if (e.Row == null)
				{
					e.Cancel = true;
				}
				else
				{
					if (Base.VendorClass.Current != null && String.IsNullOrEmpty(Base.VendorClass.Current.PaymentMethodID) == false)
					{
						e.NewValue = Base.VendorClass.Current.PaymentMethodID;
						e.Cancel = true;
					}
				}
			}

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vCashAccountID> e)
			{
				if (e.Row != null)
				{
					if (Base.VendorClass.Current != null && Base.VendorClass.Current.CashAcctID.HasValue
														&& e.Row.VPaymentMethodID == Base.VendorClass.Current.PaymentMethodID)
					{
						e.NewValue = Base.VendorClass.Current.CashAcctID;
						e.Cancel = true;
					}
					else
					{
						e.NewValue = null;
						e.Cancel = true;
					}
				}
			}

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vTaxZoneID> e)
			{
				BAccount acct = Base.BAccountAccessor.Current;
				if (e.Row == null || acct == null) return;

				if (acct.IsBranch == true)
				{
					e.NewValue = e.Row.VTaxZoneID;
					e.Cancel = true;
				}
				else
				{
					DefaultFrom<VendorClass.taxZoneID>(e.Args, Base.VendorClass.Cache);
				}
			}

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vExpenseSubID> e)
			{
				BAccount acct = Base.BAccountAccessor.Current;
				if (e.Row == null || acct == null) return;

				if (acct.IsBranch == true)
				{
					e.NewValue = e.Row.CMPExpenseSubID;
					e.Cancel = true;
				}
				else
				{
					DefaultFrom<VendorClass.expenseSubID>(e.Args, Base.VendorClass.Cache, false);
				}
			}

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vDiscountSubID> e)
			{
				BAccount acct = Base.BAccountAccessor.Current;
				if (e.Row == null || acct == null) return;

				if (acct.IsBranch == true)
				{
					e.NewValue = e.Row.CMPDiscountSubID;
					e.Cancel = true;
				}
				else
				{
					DefaultFrom<VendorClass.discountSubID>(e.Args, Base.VendorClass.Cache);
				}
			}

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.cBranchID> e)
			{
				e.NewValue = null;
				e.Cancel = true;
			}

			//protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vDefAddressID> e) =>
			//	DefaultFrom<Vendor.defAddressID>(e.Args, Base.BAccount.Cache);

			//protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vDefContactID> e) =>
			//	DefaultFrom<Vendor.defContactID>(e.Args, Base.BAccount.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vRemitAddressID> e) =>
				DefaultFrom<Vendor.defAddressID>(e.Args, Base.BAccount.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vRemitContactID> e) =>
				DefaultFrom<Vendor.defContactID>(e.Args, Base.BAccount.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vExpenseAcctID> e) =>
				DefaultFrom<VendorClass.expenseAcctID>(e.Args, Base.VendorClass.Cache, false);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vRetainageAcctID> e) =>
				DefaultFrom<VendorClass.retainageAcctID>(e.Args, Base.VendorClass.Cache, false);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vRetainageSubID> e) =>
				DefaultFrom<VendorClass.retainageSubID>(e.Args, Base.VendorClass.Cache, false);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vPaymentByType> e) =>
				DefaultFrom<VendorClass.paymentByType>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vAPAccountID> e) =>
				DefaultFrom<VendorClass.aPAcctID>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vAPSubID> e) =>
				DefaultFrom<VendorClass.aPSubID>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vDiscountAcctID> e) =>
				DefaultFrom<VendorClass.discountAcctID>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vFreightAcctID> e) =>
				DefaultFrom<VendorClass.freightAcctID>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vFreightSubID> e) =>
				DefaultFrom<VendorClass.freightSubID>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vShipTermsID> e) =>
				DefaultFrom<VendorClass.shipTermsID>(e.Args, Base.VendorClass.Cache);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vRcptQtyAction> e) =>
				DefaultFrom<VendorClass.rcptQtyAction>(e.Args, Base.VendorClass.Cache, true, POReceiptQtyAction.AcceptButWarn);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vPrintOrder> e) =>
				DefaultFrom<VendorClass.printPO>(e.Args, Base.VendorClass.Cache, false);

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vEmailOrder> e) =>
				DefaultFrom<VendorClass.emailPO>(e.Args, Base.VendorClass.Cache, false);

			#endregion

			#region Row-level

			protected override void _(Events.RowInserted<VendorR> e, PXRowInserted del)
			{
				var row = (VendorR)e.Row;
				if (row != null)
				{
					PXRowInserting inserting = delegate (PXCache sender, PXRowInsertingEventArgs args)
					{
						Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(Base);
						if (branch != null)
						{
							object cd = branch.BranchCD;
							this.DefLocation.Cache.RaiseFieldUpdating<CRLocation.locationCD>(args.Row, ref cd);
							((CRLocation)args.Row).LocationCD = (string)cd;
						}
					};

					if (Base.VendorClass.Current != null && Base.VendorClass.Current.DefaultLocationCDFromBranch == true)
					{
						Base.RowInserting.AddHandler<CRLocation>(inserting);
					}

					InsertLocation(e.Cache, row);

					if (Base.VendorClass.Current != null && Base.VendorClass.Current.DefaultLocationCDFromBranch == true)
					{
						Base.RowInserting.RemoveHandler<CRLocation>(inserting);
					}
				}

				del?.Invoke(e.Cache, e.Args);
			}

			protected override void _(Events.RowSelected<CRLocation> e)
			{
				base._(e);

				if (e.Row != null)
				{
					CRLocation row = e.Row;

					bool expenseAcctFieldsShouldBeRequired = Base.BAccountAccessor.Current?.TaxAgency == true;

					PXUIFieldAttribute.SetRequired<CRLocation.vExpenseAcctID>(e.Cache, expenseAcctFieldsShouldBeRequired);
					PXUIFieldAttribute.SetRequired<CRLocation.vExpenseSubID>(e.Cache, expenseAcctFieldsShouldBeRequired);

					PXUIFieldAttribute.SetEnabled<CRLocation.vCashAccountID>(e.Cache, e.Row, String.IsNullOrEmpty(row.VPaymentMethodID) == false);

					if (Base.VendorClass.Current != null)
					{
						bool isRequired = (Base.VendorClass.Current.RequireTaxZone ?? false) && row.IsDefault == true;
						PXDefaultAttribute.SetPersistingCheck<CRLocation.vTaxZoneID>(this.DefLocation.Cache, null, PXPersistingCheck.Nothing);
						PXDefaultAttribute.SetPersistingCheck<CRLocation.vTaxZoneID>(this.DefLocation.Cache, e.Row, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
						PXUIFieldAttribute.SetRequired<CRLocation.vTaxZoneID>(this.DefLocation.Cache, isRequired);
					}
				}
			}

			protected virtual void _(Events.RowPersisting<CRLocation> e)
			{
				if (e.Cancel)
					return;

				CRLocation location = e.Row;

				if (location == null)
					return;

				bool validationResult = ValidateLocation(e.Cache, location);

				if (!validationResult)
				{
					Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(e.Cache, location);

					if (errors.Any())
					{
						throw new PXException(Common.Messages.RecordCanNotBeSaved);
					}
				}
			}

			protected virtual void _(Events.RowPersisting<VendorR> e)
			{
				VendorR row = e.Row;
				if (row == null)
					return;

				CRLocation loc = DefLocation.SelectSingle();
				if (loc == null)
					return;

				CheckCury(row, loc);
			}

			#endregion

			#endregion

			#region Methods

			public override bool ValidateLocation(PXCache cache, CRLocation location)
			{
				bool res = true;
				VendorR acct = Base.BAccountAccessor.Current;
				if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
				{
					if (Base.VendorClass.Current != null && Base.VendorClass.Current.RequireTaxZone == true && location.IsDefault == true && location.VTaxZoneID == null)
					{
						res &= ValidationHelper.SetErrorEmptyIfNull<CRLocation.vTaxZoneID>(cache, location, location.VTaxZoneID);
					}

					res &= locationValidator.ValidateVendorLocation(cache, acct, location);

					CheckCury(acct, location);
				}

				return res;
			}

			public virtual void CheckCury(VendorR vendor, CRLocation location)
			{
				PXSelectBase<CashAccount> select = new PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CRLocation.vCashAccountID>>>>(Base);

				if (!String.IsNullOrEmpty(vendor.CuryID) && vendor.AllowOverrideCury != true)
				{
					CashAccount cashacct = select.Select(location.VCashAccountID);

					if (cashacct != null)
					{
						if (vendor.CuryID != cashacct.CuryID)
						{
							PXUIFieldAttribute.SetWarning<CRLocation.vCashAccountID>(DefLocation.Cache, location, Messages.VendorCuryDifferentDefPayCury);
						}
					}
				}
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ContactDetailsExt : BusinessAccountContactDetailsExt<VendorMaint, CreateContactFromVendorGraphExt, VendorR, VendorR.bAccountID> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationDetailsExt : LocationDetailsExt<VendorMaint, DefContactAddressExt, VendorR, VendorR.bAccountID> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class PrimaryContactGraphExt : CRPrimaryContactGraphExt<
			VendorMaint, ContactDetailsExt,
			VendorR, VendorR.bAccountID, VendorR.primaryContactID>
		{
			protected override PXView ContactsView => Base1.Contacts.View;

			#region Events

			[PXVerifySelector(typeof(
				SelectFrom<Contact>
				.Where<
					Contact.bAccountID.IsEqual<VendorR.bAccountID.FromCurrent>
					.And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>
					.And<Contact.isActive.IsEqual<True>>
				>
				.SearchFor<Contact.contactID>),
				fieldList: new[]
				{
					typeof(Contact.displayName),
					typeof(Contact.salutation),
					typeof(Contact.phone1),
					typeof(Contact.eMail)
				},
				VerifyField = false,
				DescriptionField = typeof(Contact.displayName)
			)]
			[PXUIField(DisplayName = "Name")]
			[PXMergeAttributes(Method = MergeMethod.Merge)]
			protected virtual void _(Events.CacheAttached<VendorR.primaryContactID> e) { }

			#endregion
		}

		#endregion

		#region Address Lookup Extension

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<VendorMaint, VendorR, Address>
		{
			protected override string AddressView => nameof(DefContactAddressExt.DefAddress);
			protected override string ViewOnMap => nameof(DefContactAddressExt.ViewMainOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorMaintRemitAddressLookupExtension : CR.Extensions.AddressLookupExtension<VendorMaint, VendorR, Address>
		{
			protected override string AddressView => nameof(DefLocationExt.RemitAddress);
			protected override string ViewOnMap => nameof(DefLocationExt.viewRemitOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorMaintDefLocationAddressLookupExtension : CR.Extensions.AddressLookupExtension<VendorMaint, VendorR, Address>
		{
			protected override string AddressView => nameof(DefLocationExt.DefLocationAddress);
			protected override string ViewOnMap => nameof(DefLocationExt.ViewDefLocationAddressOnMap);
		}

		#endregion

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ExtendToCustomer : ExtendToCustomerGraph<VendorMaint, VendorR>
		{
			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(VendorR));
			}
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorRemitSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<VendorMaint, VendorRemitSharedContactOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.vRemitContactID),
					IsOverrideRelated = typeof(CRLocation.overrideRemitContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(VendorR))
				{
					RelatedID = typeof(VendorR.bAccountID),
					ChildID = typeof(VendorR.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorRemitSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<VendorMaint, VendorRemitSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.vRemitAddressID),
					IsOverrideRelated = typeof(CRLocation.overrideRemitAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(VendorR))
				{
					RelatedID = typeof(VendorR.bAccountID),
					ChildID = typeof(VendorR.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorDefSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<VendorMaint, VendorDefSharedContactOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defContactID),
					IsOverrideRelated = typeof(CRLocation.overrideContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(VendorR))
				{
					RelatedID = typeof(VendorR.bAccountID),
					ChildID = typeof(VendorR.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class VendorDefSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<VendorMaint, VendorDefSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defAddressID),
					IsOverrideRelated = typeof(CRLocation.overrideAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(VendorR))
				{
					RelatedID = typeof(VendorR.bAccountID),
					ChildID = typeof(VendorR.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateContactFromVendorGraphExt : CRCreateContactActionBase<VendorMaint, VendorR>
		{
			#region Initialization

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<DocumentAddress>(Base.GetExtension<DefContactAddressExt>().DefAddress);
				Contacts = new PXSelectExtension<DocumentContact>(Base.GetExtension<DefContactAddressExt>().DefContact);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<ContactFilter> e)
			{
				PXUIFieldAttribute.SetReadOnly<ContactFilter.fullName>(e.Cache, e.Row, true);
			}

			public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.fullName> e)
			{
				e.NewValue = Contacts.SelectSingle()?.FullName;
			}

			#endregion

			#region Overrides

			protected override void FillRelations<TNoteField>(CRRelationsList<TNoteField> relations, Contact target)
			{
			}

			protected override IConsentable MapConsentable(DocumentContact source, IConsentable target)
			{
				return target;
			}

			#endregion
		}

		#endregion
	}
}

