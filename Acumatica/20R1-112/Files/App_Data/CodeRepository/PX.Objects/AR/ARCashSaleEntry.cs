using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.Reclassification.UI;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;

using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.Common.Bql;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Objects.Extensions.PaymentTransaction;

namespace PX.Objects.AR
{
	public class ARCashSaleEntry : ARDataEntryGraph<ARCashSaleEntry, ARCashSale>, IGraphWithInitialization
	{
	    #region Extensions

	    public class ARCashSaleEntryDocumentExtension : PaidInvoiceGraphExtension<ARCashSaleEntry>
	    {
	        public override void Initialize()
	        {
	            base.Initialize();

	            Documents = new PXSelectExtension<PaidInvoice>(Base.Document);
	            Lines = new PXSelectExtension<DocumentLine>(Base.Transactions);
	        }

			public override void SuppressApproval()
			{
				Base.Approval.SuppressApproval = true;
			}

			protected override PaidInvoiceMapping GetDocumentMapping()
	        {
	            return new PaidInvoiceMapping(typeof(ARCashSale))
	            {
                    HeaderTranPeriodID = typeof(ARCashSale.adjTranPeriodID),
	                HeaderDocDate = typeof(ARCashSale.adjDate)
                };
	        }

	        protected override DocumentLineMapping GetDocumentLineMapping()
	        {
	            return new DocumentLineMapping(typeof(ARTran));
	        }
	    }

	    #endregion

        #region Selects
		public PXSelect<InventoryItem> dummy_nonstockitem_for_redirect_newitem;
		public PXSelect<AP.Vendor> dummy_vendor_taxAgency_for_avalara;

		public ToggleCurrency<ARCashSale> CurrencyView;
        [PXCopyPasteHiddenFields(typeof(ARCashSale.extRefNbr), typeof(ARCashSale.clearDate),typeof(ARCashSale.cleared))]
		public PXSelectJoin<ARCashSale,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARCashSale.customerID>>>,
			Where<ARCashSale.docType, Equal<Optional<ARCashSale.docType>>,
			And2<Where<ARCashSale.origModule, NotEqual<BatchModule.moduleSO>, Or<ARCashSale.released, Equal<boolTrue>>>,
			And<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>>>>> Document;
		public PXSelect<ARCashSale, Where<ARCashSale.docType, Equal<Current<ARCashSale.docType>>, And<ARCashSale.refNbr, Equal<Current<ARCashSale.refNbr>>>>> CurrentDocument;

		public PXSelectJoin<CCProcessingCenter, LeftJoin<CustomerPaymentMethod,
				On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<ARCashSale.pMInstanceID>>>> ProcessingCenter;
	
		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARCashSale.docType>>, And<ARTran.refNbr, Equal<Current<ARCashSale.refNbr>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Transactions;
		public PXSelect<ARTax, Where<ARTax.tranType, Equal<Current<ARCashSale.docType>>, And<ARTax.refNbr, Equal<Current<ARCashSale.refNbr>>>>, OrderBy<Asc<ARTax.tranType, Asc<ARTax.refNbr, Asc<ARTax.taxID>>>>> Tax_Rows;
        [PXCopyPasteHiddenView]
        public PXSelectJoin<ARTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>, Where<ARTaxTran.module, Equal<BatchModule.moduleAR>, And<ARTaxTran.tranType, Equal<Current<ARCashSale.docType>>, And<ARTaxTran.refNbr, Equal<Current<ARCashSale.refNbr>>>>>> Taxes;
		public PXSelect<ARAddress, Where<ARAddress.addressID, Equal<Current<ARCashSale.billAddressID>>>> Billing_Address;
		public PXSelect<ARContact, Where<ARContact.contactID, Equal<Current<ARCashSale.billContactID>>>> Billing_Contact;

		[PXViewName(Messages.ARAddress)]
		public PXSelect<ARShippingAddress, Where<ARShippingAddress.addressID, Equal<Current<ARCashSale.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.ARContact)]
		public PXSelect<ARShippingContact, Where<ARShippingContact.contactID, Equal<Current<ARCashSale.shipContactID>>>> Shipping_Contact;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARCashSale.curyInfoID>>>> currencyinfo;
		[PXReadOnlyView]
		public PXSelect<CATran> dummy_CATran;


		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<ARCashSale.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>> customerclass;
		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<ARCashSale.cashAccountID>>>> cashaccount;
		public PXSetup<OrganizationFinPeriod, Where<OrganizationFinPeriod.finPeriodID, Equal<Current<ARCashSale.adjFinPeriodID>>,
													And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<ARCashSale.branchID>>>>> finperiod;
		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<ARCashSale.paymentMethodID>>>> paymentmethod;

		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<ARCashSale.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>, And<Location.locationID, Equal<Optional<ARCashSale.customerLocationID>>>>> location;

		public PXSetup<GLSetup> glsetup;
		public PXSetup<ARSetup> arsetup;

		public PXSelect<ARBalances> arbalances;

		public PXSelect<CustSalesPeople, Where<CustSalesPeople.bAccountID, Equal<Current<ARCashSale.customerID>>,
												And<CustSalesPeople.locationID, Equal<Current<ARCashSale.customerLocationID>>>>> salesPerSettings;
		public PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Current<ARCashSale.docType>>,
												And<ARSalesPerTran.refNbr, Equal<Current<ARCashSale.refNbr>>,
												And<ARSalesPerTran.adjdDocType, Equal<ARDocType.undefined>,
												And2<Where<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byInvoice>, Or<Current<ARCashSale.released>, Equal<boolFalse>>>,
												Or<ARSalesPerTran.adjdDocType, Equal<Current<ARCashSale.docType>>,
												And<ARSalesPerTran.adjdRefNbr, Equal<Current<ARCashSale.refNbr>>,
												And<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byPayment>>>>>>>>> salesPerTrans;

		public ARPaymentChargeSelect<ARCashSale, ARCashSale.paymentMethodID, ARCashSale.cashAccountID, ARCashSale.docDate, ARCashSale.tranPeriodID, ARCashSale.pMInstanceID,
			Where<ARPaymentChargeTran.docType, Equal<Current<ARCashSale.docType>>,
				And<ARPaymentChargeTran.refNbr, Equal<Current<ARCashSale.refNbr>>>>> PaymentCharges;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>))]
		[CRDefaultMailTo(typeof(Select<ARContact, Where<ARContact.contactID, Equal<Current<ARCashSale.billContactID>>>>))]
		public CRActivityList<ARCashSale>
			Activity;

		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;
		public PXSelect<ARSetupApproval,
			Where<Current<ARCashSale.docType>, Equal<ARDocType.cashReturn>,
				And<ARSetupApproval.docType, Equal<ARDocType.cashReturn>>>> SetupApproval;

		public PXSelect<ExternalTransaction,
			Where<ExternalTransaction.refNbr, Equal<Current<ARCashSale.refNbr>>,
				And<ExternalTransaction.docType, Equal<Current<ARCashSale.docType>>,
			Or<Where<ExternalTransaction.refNbr, Equal<Current<ARCashSale.origRefNbr>>,
				And<ExternalTransaction.docType, Equal<Current<ARCashSale.origDocType>>>>>>>,
			OrderBy<Desc<ExternalTransaction.transactionID>>> ExternalTran;

		public PXSelectOrderBy<CCProcTran, OrderBy<Desc<CCProcTran.tranNbr>>> ccProcTran;
		public IEnumerable CcProcTran()
		{
			var externalTrans = ExternalTran.Select();
			var query = new PXSelect<CCProcTran, 
				Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>>(this);
			foreach (ExternalTransaction extTran in externalTrans)
			{
				foreach (CCProcTran procTran in query.Select(extTran.TransactionID))
				{
					yield return procTran;
				}
			}
		}

		#endregion
		[PXViewName(EP.Messages.Approval)]
		public EPApprovalAutomationWithoutHoldDefaulting<ARCashSale, ARCashSale.approved, ARCashSale.rejected, ARCashSale.hold, ARSetupApproval> Approval;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public ARCashSaleEntry()
			: base()
		{
			{
				ARSetup record = arsetup.Select();
			}

			{
				GLSetup record = glsetup.Select();
			}
            RowUpdated.AddHandler<ARCashSale>(ParentRowUpdated);
			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

            FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });

			var arAddressCache = Caches[typeof(ARAddress)];
			var arContactCache = Caches[typeof(ARContact)];
			var arShippingAddressCache = Caches[typeof(ARShippingAddress)];
			var arShippingContactCache = Caches[typeof(ARShippingContact)];
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<ARCashSale>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(ARTran), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<ARTran.tranType>(PXDbType.Char, 3, ((ARCashSaleEntry)graph).Document.Current?.DocType),
							new PXDataFieldValue<ARTran.refNbr>(((ARCashSaleEntry)graph).Document.Current?.RefNbr)
						};
					}));
			}
		}

		public override Dictionary<string, string> PrepareReportParams(string reportID, ARCashSale doc)
		{
			if (reportID == "AR641000")
			{
				var parameters = new Dictionary<string, string>();
				parameters["ARInvoice.DocType"] = doc.DocType;
				parameters["ARInvoice.RefNbr"] = doc.RefNbr;
				return parameters;
			}

			return base.PrepareReportParams(reportID, doc);
		}

		#region Cache Attached
        #region InventoryItem
        #region COGSSubID
        [PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
        [SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
        public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

		#region ARSalesPerTran
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		protected virtual void ARSalesPerTran_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARCashSale.refNbr))]
		[PXParent(typeof(Select<ARCashSale, Where<ARCashSale.docType, Equal<Current<ARSalesPerTran.docType>>,
						 And<ARCashSale.refNbr, Equal<Current<ARSalesPerTran.refNbr>>>>>))]
		protected virtual void ARSalesPerTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

        [PXDBInt()]
        [PXDBDefault(typeof(ARCashSale.branchID))]
        protected virtual void ARSalesPerTran_BranchID_CacheAttached(PXCache sender)
        {
        }
		[SalesPerson(DirtyRead = true, Enabled = false, IsKey = true, DescriptionField = typeof(Contact.displayName))]
		protected virtual void ARSalesPerTran_SalespersonID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<Search<CustSalesPeople.commisionPct, Where<CustSalesPeople.bAccountID, Equal<Current<ARCashSale.customerID>>,
				And<CustSalesPeople.locationID, Equal<Current<ARCashSale.customerLocationID>>,
				And<CustSalesPeople.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>>,
			Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		protected virtual void ARSalesPerTran_CommnPct_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commissionable Amount", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<ARCashSale.curyCommnblAmt>))]
		protected virtual void ARSalesPerTran_CuryCommnblAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Mult<ARSalesPerTran.curyCommnblAmt, Div<ARSalesPerTran.commnPct, decimal100>>), typeof(SumCalc<ARCashSale.curyCommnAmt>))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		protected virtual void ARSalesPerTran_CuryCommnAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region ARPaymentChargeTran
		#region LineNbr
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXLineNbr(typeof(ARCashSale.chargeCntr), DecrementOnDelete = false)]
        public virtual void ARPaymentChargeTran_LineNbr_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#region CashAccountID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBDefault(typeof(ARCashSale.cashAccountID))]
        public virtual void ARPaymentChargeTran_CashAccountID_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#region EntryTypeID

		/// <summary>
		/// <see cref="ARPaymentChargeTran.EntryTypeID"/> cache attached event.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXSelector(typeof(Search2<CAEntryType.entryTypeId,
                            InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
                            Where<CashAccountETDetail.accountID, Equal<Current<ARCashSale.cashAccountID>>,
                            And<CAEntryType.drCr, Equal<CADrCr.cACredit>>>>))]
		public virtual void ARPaymentChargeTran_EntryTypeID_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region TranDate
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBDefault(typeof(ARCashSale.adjDate))]
        public virtual void ARPaymentChargeTran_TranDate_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region FinPeriodID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault()]
		[FinPeriodID(
			branchSourceType: typeof(ARPaymentChargeTran.cashAccountID),
			branchSourceFormulaType: typeof(Selector<ARPaymentChargeTran.cashAccountID, CashAccount.branchID>),
			masterFinPeriodIDType: typeof(ARPaymentChargeTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(ARCashSale.adjTranPeriodID))]
        public virtual void ARPaymentChargeTran_FinPeriodID_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region CuryTranAmt
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundFormula(typeof(Switch<Case<Where<ARPaymentChargeTran.consolidate, Equal<True>>, ARPaymentChargeTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARCashSale.curyConsolidateChargeTotal>))]
		public virtual void ARPaymentChargeTran_CuryTranAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		[PXDBInt()]
		[PXDefault(typeof(ARCashSale.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void ARTran_ProjectID_CacheAttached(PXCache sender)
		{ }

        [FinPeriodID(
            branchSourceType: typeof(ARTran.branchID),
            masterFinPeriodIDType: typeof(ARTran.tranPeriodID),
            headerMasterFinPeriodIDType: typeof(ARCashSale.adjTranPeriodID))]
        protected virtual void ARTran_FinPeriodID_CacheAttached(PXCache sender)
        {
        }


		[PXDefault(typeof(Coalesce<Search<PMAccountTask.taskID, Where<PMAccountTask.projectID, Equal<Current<ARTran.projectID>>, And<PMAccountTask.accountID, Equal<Current<ARTran.accountID>>>>>,
					Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<ARTran.projectID>>, And<PMTask.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(ARTran.projectID), BatchModule.CA, DisplayName = "Project Task")]
		protected virtual void ARTran_TaskID_CacheAttached(PXCache sender)
		{ }

		[PXDBDefault(typeof(ARRegister.branchID))]
		[Branch(Enabled = false)]
		protected virtual void ARTaxTran_BranchID_CacheAttached(PXCache sender)
		{
		}

	    [FinPeriodID(branchSourceType: typeof(ARTaxTran.branchID),
	        headerMasterFinPeriodIDType: typeof(ARCashSale.adjTranPeriodID))]
	    [PXDefault]
	    protected virtual void ARTaxTran_FinPeriodID_CacheAttached(PXCache sender)
	    {
	    }

        [PXDefault]
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeARCashSale>>>),
		DescriptionField = typeof(EPAssignmentMap.name))]
		[PXUIField(DisplayName = "Approval Map")]
		protected virtual void EPApproval_AssignmentMapID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate]
		[PXDefault(typeof(ARCashSale.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(ARCashSale.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(ARCashSale.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(ARCashSale.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(ARCashSale.curyOrigDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(ARCashSale.origDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		protected virtual void EPApproval_SourceItemType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = new ARDocType.ListAttribute()
					.ValueLabelDic[Document.Current.DocType];

				e.Cancel = true;
			}
		}
		protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = EPApprovalHelper.BuildEPApprovalDetailsString(sender, Document.Current);
			}
		}
		#endregion

		#region Other Buttons
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARCashSale ardoc in adapter.Get<ARCashSale>())
			{
				if (ardoc.Hold == false)
				{
					cache.MarkUpdated(ardoc);
					list.Add(ardoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();

			PXLongOperation.StartOperation(this, delegate() { ARDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

		[PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable VoidCheck(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Released == true && Document.Current.Voided == false && Document.Current.DocType == ARDocType.CashSale)
			{
				ARCashSale doc = PXCache<ARCashSale>.CreateCopy(Document.Current);
				FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<ARCashSale.finPeriodID, ARCashSale.branchID>(Document.Cache, doc, finperiod, typeof(OrganizationFinPeriod.aRClosed));
                try
				{
					_IsVoidCheckInProgress = true;
					this.VoidCheckProc(doc);
				}
				finally
				{
					_IsVoidCheckInProgress = false;
				}

				Document.Cache.RaiseExceptionHandling<ARCashSale.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

				List<ARCashSale> rs = new List<ARCashSale>();
				rs.Add(Document.Current);
                return rs;
			}
			else if (Document.Current != null && Document.Current.Released == false && Document.Current.Voided == false && Document.Current.DocType == ARDocType.CashSale)
			{
				if (ExternalTranHelper.HasTransactions(ExternalTran))
				{
					ARCashSale doc = Document.Current;
					doc.Voided = true;
					doc.OpenDoc = false;
					doc.PendingProcessing = false;
					doc = this.Document.Update(doc);
					this.Save.Press();
				}
			}
			return adapter.Get();
		}


		public PXAction<ARCashSale> ViewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Document.Current.OrigDocType, Document.Current.OrigRefNbr, Document.Current.OrigModule);
			return adapter.Get();
		}

		public PXAction<ARCashSale> viewExternalTransaction;
		[PXUIField(DisplayName = Messages.ViewExternalTransaction, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewExternalTransaction(PXAdapter adapter)
		{
			if (this.ccProcTran.Current != null)
			{
				CCProcTran row = this.ccProcTran.Current;
				ExternalTransactionMaint graph = PXGraph.CreateInstance<ExternalTransactionMaint>();
				graph.CurrentTransaction.Current = graph.CurrentTransaction.Search<ExternalTransaction.transactionID>(row.TransactionID);

				if (graph.CurrentTransaction.Current != null)
				{
					throw new PXRedirectRequiredException(graph, newWindow: true, AR.Messages.ViewExternalTransaction) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}


		public PXAction<ARCashSale> reclassifyBatch;
		[PXUIField(DisplayName = AP.Messages.ReclassifyGLBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReclassifyBatch(PXAdapter adapter)
		{
			var document = Document.Current;

			if (document != null)
			{
				ReclassifyTransactionsProcess.TryOpenForReclassificationOfDocument(Document.View, BatchModule.AR, document.BatchNbr, document.DocType,
					document.RefNbr);
			}

			return adapter.Get();
		}

		public PXAction<ARCashSale> customerDocuments;
		[PXUIField(DisplayName = "Customer Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable CustomerDocuments(PXAdapter adapter)
		{
			if (customer.Current != null)
			{
				ARDocumentEnq graph = PXGraph.CreateInstance<ARDocumentEnq>();
				graph.Filter.Current.CustomerID = customer.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Customer Details");
			}
			return adapter.Get();
		}

		public PXAction<ARCashSale> sendARInvoiceMemo;
		[PXUIField(DisplayName = "Send AR Invoice/Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SendARInvoiceMemo(PXAdapter adapter)
		{
			ARCashSale invoice = Document.Current;
			if (Document.Current != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["DocType"] = invoice.DocType;
				mailParams["RefNbr"] = invoice.RefNbr;
				if (!ReportNotificationGenerator.Send("AR641000", mailParams).Any())
				{
					throw new PXException(ErrorMessages.MailSendFailed);
				}
				Clear();
				Document.Current = Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
			}
			return adapter.Get();
		}


		public PXAction<ARCashSale> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (ARCashSale current in adapter.Get<ARCashSale>())
			{
				//ARCashSale current = this.CurrentDocument.Current;
				if (current != null)
				{
					ARAddress address = this.Billing_Address.Current;
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						PXAddressValidator.Validate<ARAddress>(this, address, true);
					}
				}
				yield return current;
			}
			//return adapter.Get();
		}

		public PXAction<ARCashSale> viewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			ARTran currentLine = Transactions.Current;

			if (currentLine != null &&
				Transactions.Cache.GetStatus(currentLine) == PXEntryStatus.Notchanged)
			{
				Save.Press();
				ARInvoiceEntry.ViewScheduleForLine(this, Document.Current, Transactions.Current);
			}

			return adapter.Get();
		}
		#endregion

		#region ARCashSale Events

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Original Document", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual void ARCashSale_OrigRefNbr_CacheAttached(PXCache sender)
		{
		}

		protected virtual void ARCashSale_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ARDocType.CashSale;
			e.Cancel = true;
		}

		protected virtual void ARCashSale_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARCashSale doc = e.Row as ARCashSale;
			if (PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
			{
				if (this.Approval.GetAssignedMaps(doc, sender).Any())
				{
					sender.SetValue<ARCashSale.hold>(doc, true);
				}
			}
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void ARCashSale_CustomerID_CacheAttached(PXCache sender)
        {
        }


		protected virtual void ARCashSale_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			customer.RaiseFieldUpdated(sender, e.Row);

			{
				sender.SetDefaultExt<ARCashSale.customerLocationID>(e.Row);
				sender.SetDefaultExt<ARCashSale.printInvoice>(e.Row);

                sender.SetDefaultExt<ARPayment.paymentMethodID>(e.Row);
				if (((ARCashSale)e.Row).DocType != ARDocType.CreditMemo)
				{
					sender.SetDefaultExt<ARCashSale.termsID>(e.Row);
				}
				else
				{
					sender.SetValueExt<ARCashSale.termsID>(e.Row, null);
				}
			}

			ARAddressAttribute.DefaultRecord<ARCashSale.billAddressID>(sender, e.Row);
			ARContactAttribute.DefaultRecord<ARCashSale.billContactID>(sender, e.Row);
		}

		protected virtual void ARCashSale_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (ARTaxTran taxTran in Taxes.Select())
			{
				Taxes.Cache.MarkUpdated(taxTran);
			}
		}

		protected virtual void ARCashSale_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

				sender.SetDefaultExt<ARCashSale.aRAccountID>(e.Row);
				sender.SetDefaultExt<ARCashSale.aRSubID>(e.Row);
			sender.SetDefaultExt<ARCashSale.branchID>(e.Row);
			sender.SetDefaultExt<ARCashSale.taxZoneID>(e.Row);
			sender.SetDefaultExt<ARCashSale.taxCalcMode>(e.Row);
			sender.SetDefaultExt<ARCashSale.salesPersonID>(e.Row);
			sender.SetDefaultExt<ARCashSale.workgroupID>(e.Row);
			sender.SetDefaultExt<ARCashSale.ownerID>(e.Row);
			sender.SetDefaultExt<ARCashSale.avalaraCustomerUsageType>(e.Row);
			if (PM.ProjectAttribute.IsPMVisible( BatchModule.AR))
			{
				sender.SetDefaultExt<ARCashSale.projectID>(e.Row);
			}

			ARShippingAddressAttribute.DefaultRecord<ARCashSale.shipAddressID>(sender, e.Row);
			ARShippingContactAttribute.DefaultRecord<ARCashSale.shipContactID>(sender, e.Row);
		}

		protected virtual void ARCashSale_ExtRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            ARCashSale row = (ARCashSale)e.Row;
			if (e.Row != null && ((ARCashSale)e.Row).DocType == ARDocType.VoidPayment)
			{
				//avoid webdialog in PaymentRef attribute
				e.Cancel = true;
			}
			else
			{
				if (row!= null && string.IsNullOrEmpty((string)e.NewValue) == false && String.IsNullOrEmpty(row.PaymentMethodID))
				{
                    PaymentMethod pm = this.paymentmethod.Current;
                    ARCashSale dup = null;
                    if (pm != null && pm.IsAccountNumberRequired == true)
                    {
                        dup = PXSelectReadonly<ARCashSale, Where<ARCashSale.customerID, Equal<Current<ARCashSale.customerID>>, And<ARCashSale.pMInstanceID, Equal<Current<ARCashSale.pMInstanceID>>, And<ARCashSale.extRefNbr, Equal<Required<ARCashSale.extRefNbr>>, And<ARCashSale.voided, Equal<False>, And<Where<ARCashSale.docType, NotEqual<Current<ARCashSale.docType>>, Or<ARCashSale.refNbr, NotEqual<Current<ARCashSale.refNbr>>>>>>>>>>.Select(this, e.NewValue);
                    }
                    else
                    {
                        dup = PXSelectReadonly<ARCashSale, Where<ARCashSale.customerID, Equal<Current<ARCashSale.customerID>>, And<ARCashSale.paymentMethodID, Equal<Current<ARCashSale.paymentMethodID>>, And<ARCashSale.extRefNbr, Equal<Required<ARCashSale.extRefNbr>>, And<ARCashSale.voided, Equal<False>, And<Where<ARCashSale.docType, NotEqual<Current<ARCashSale.docType>>, Or<ARCashSale.refNbr, NotEqual<Current<ARCashSale.refNbr>>>>>>>>>>.Select(this, e.NewValue);
                    }
					if (dup != null)
					{
                        sender.RaiseExceptionHandling<ARCashSale.extRefNbr>(e.Row, e.NewValue, new PXSetPropertyException(Messages.DuplicateCustomerPayment, PXErrorLevel.Warning, dup.ExtRefNbr, dup.DocDate, dup.DocType, dup.RefNbr));
					}
				}
			}
		}



		private object GetAcctSub<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			object NewValue = cache.GetValueExt<Field>(data);
			if (NewValue is PXFieldState)
			{
				return ((PXFieldState)NewValue).Value;
			}
			else
			{
				return NewValue;
			}
		}

		protected virtual void ARCashSale_ARAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<Location.aRAccountID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARCashSale_ARSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<Location.aRSubID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARCashSale_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<ARCashSale.pMInstanceID>(e.Row);
            sender.SetDefaultExt<ARCashSale.cashAccountID>(e.Row);
        }

		protected virtual void ARCashSale_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetValueExt<ARPayment.refTranExtNbr>(e.Row, null);
			sender.SetDefaultExt<ARCashSale.cashAccountID>(e.Row);
        }

		protected virtual void ARCashSale_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;
			if (cashaccount.Current == null || cashaccount.Current.CashAccountID != payment.CashAccountID)
			{
				cashaccount.Current = (CashAccount)PXSelectorAttribute.Select<ARCashSale.cashAccountID>(sender, e.Row);
			}

			if (_IsVoidCheckInProgress == false && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<ARCashSale.curyInfoID>(sender, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<ARCashSale.adjDate>(e.Row, payment.DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					payment.CuryID = info.CuryID;
				}
			}

			//sender.SetDefaultExt<ARCashSale.branchID>(e.Row);
			payment.Cleared = false;
			payment.ClearDate = null;
			sender.SetDefaultExt<ARCashSale.depositAsBatch>(e.Row);
			sender.SetDefaultExt<ARCashSale.depositAfter>(e.Row);


			if ((cashaccount.Current != null) && (cashaccount.Current.Reconcile == false))
			{
				payment.Cleared = true;
				payment.ClearDate = payment.DocDate;
			}
		}
		protected virtual void ARCashSale_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;

			if (payment.Cleared == true)
			{
				if (payment.ClearDate == null)
				{
					payment.ClearDate = payment.DocDate;
				}
			}
			else
			{
				payment.ClearDate = null;
			}
		}
		protected virtual void ARCashSale_AdjDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;

			if (payment.Released == false && payment.DocType != ARDocType.CashReturn)
			{
				CurrencyInfoAttribute.SetEffectiveDate<ARCashSale.adjDate>(sender, e);
				sender.SetDefaultExt<ARCashSale.depositAfter>(e.Row);
			}
		}


		protected virtual void ARCashSale_DepositAfter_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARCashSale row = (ARCashSale)e.Row;
			if ((row.DocType == ARDocType.Payment || row.DocType == ARDocType.CashSale || row.DocType == ARDocType.Refund || row.DocType == ARDocType.CashReturn)
				&& row.DepositAsBatch == true)
			{
				e.NewValue = row.AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_DepositAsBatch_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale row = (ARCashSale)e.Row;

			if ((row.DocType == ARDocType.Payment || row.DocType == ARDocType.CashSale || row.DocType == ARDocType.Refund) || row.DocType == ARDocType.CashReturn)
			{
				sender.SetDefaultExt<ARPayment.depositAfter>(e.Row);
			}
		}

		protected virtual void ARCashSale_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARCashSale doc = (ARCashSale)e.Row;

			if (doc.CashAccountID == null)
			{
				if (sender.RaiseExceptionHandling<ARCashSale.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARCashSale.cashAccountID).Name)))
				{
					throw new PXRowPersistingException(typeof(ARCashSale.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARCashSale.cashAccountID).Name);
				}
			}

            if (String.IsNullOrEmpty(doc.PaymentMethodID))
            {
                if (sender.RaiseExceptionHandling<ARCashSale.paymentMethodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARCashSale.paymentMethodID).Name)))
                {
                    throw new PXRowPersistingException(typeof(ARCashSale.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARCashSale.paymentMethodID).Name);
                }
            }

			ValidateTaxConfiguration(sender, doc);

            PaymentMethod currentPaymentMethod = this.paymentmethod.Current;

			PXDefaultAttribute.SetPersistingCheck<ARPayment.pMInstanceID>(sender, doc, (currentPaymentMethod != null
				&& currentPaymentMethod.IsAccountNumberRequired == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			Terms terms = (Terms)PXSelectorAttribute.Select<ARCashSale.termsID>(Document.Cache, doc);

			if (terms == null)
			{
				sender.SetValue<ARCashSale.termsID>(doc, null);
				return;
			}

			if (terms.InstallmentType == CS.TermsInstallmentType.Multiple)
			{
				sender.RaiseExceptionHandling<ARCashSale.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.Cash_Sale_Cannot_Have_Multiply_Installments, typeof(ARCashSale.termsID).Name));
			}

			PXDefaultAttribute.SetPersistingCheck<ARCashSale.extRefNbr>(sender, doc, ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) || arsetup.Current.RequireExtRef == false) ?
				PXPersistingCheck.Nothing : PXPersistingCheck.Null);

			PaymentRefAttribute.SetUpdateCashManager<ARCashSale.extRefNbr>(sender, e.Row, ((ARCashSale)e.Row).DocType != ARDocType.VoidPayment);
		}

		private void ValidateTaxConfiguration(PXCache cache, ARCashSale cashSale)
		{
			bool reduceOnEarlyPayments = false;
			bool reduceTaxableAmount = false;
			foreach (PXResult<ARTax, Tax> result in PXSelectJoin<ARTax,
				InnerJoin<Tax, On<Tax.taxID, Equal<ARTax.taxID>>>,
				Where<ARTax.tranType, Equal<Current<ARCashSale.docType>>,
				And<ARTax.refNbr, Equal<Current<ARCashSale.refNbr>>>>>.Select(this))
			{
				Tax tax = (Tax)result;
				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
				{
					reduceOnEarlyPayments = true;
				}
				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount)
				{
					reduceTaxableAmount = true;
				}
				if (reduceOnEarlyPayments && reduceTaxableAmount)
				{
					cache.RaiseExceptionHandling<ARCashSale.taxZoneID>(cashSale, cashSale.TaxZoneID, new PXSetPropertyException(TX.Messages.InvalidTaxConfiguration));
				}
			}
		}

		protected bool InternalCall = false;
		/// <summary>
		/// Determines whether the approval is required for the document.
		/// </summary>
		/// <param name="doc">The document for which the check should be performed.</param>
		/// <param name="cache">The cache.</param>
		/// <returns>Returns <c>true</c> if approval is required; otherwise, returns <c>false</c>.</returns>
	    private bool IsApprovalRequired(ARCashSale doc, PXCache cache)
		{
			var isApprovalInstalled = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();
			var areMapsAssigned = Approval.GetAssignedMaps(doc, cache).Any();
			return doc.DocType == ARDocType.CashReturn && isApprovalInstalled && areMapsAssigned;
		}

		protected virtual void ARCashSale_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARCashSale doc = e.Row as ARCashSale;

			if (doc == null || InternalCall)
			{
				return;
			}

			bool dontApprove = !IsApprovalRequired(doc, cache);
			if (doc.DontApprove != dontApprove)
			{
				cache.SetValueExt<ARCashSale.dontApprove>(doc, dontApprove);
			}

			// We need this for correct tabs repainting
			// in migration mode.
			// 
			PaymentCharges.Cache.AllowSelect = true;

			bool isDepositAfterEditable = doc.DocType == ARDocType.Payment ||
										  doc.DocType == ARDocType.CashSale ||
										  doc.DocType == ARDocType.CashReturn;

			PXUIFieldAttribute.SetVisible<ARCashSale.depositAfter>(cache, doc, isDepositAfterEditable && doc.DepositAsBatch == true);

			bool clearEnabled = doc.Hold == true && cashaccount.Current?.Reconcile == true;


			bool isDocumentReleasedOrVoided = doc.Released == true || doc.Voided == true;
			Shipping_Address.Cache.AllowUpdate = 
			Shipping_Contact.Cache.AllowUpdate = !isDocumentReleasedOrVoided;

			if (isDocumentReleasedOrVoided)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;
				release.SetEnabled(false);
				voidCheck.SetEnabled(doc.Voided == false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARCashSale.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARCashSale.curyID>(cache, doc, false);

				//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
				PXDBCurrencyAttribute.SetBaseCalc<ARCashSale.curyDocBal>(cache, null, true);
				PXDBCurrencyAttribute.SetBaseCalc<ARCashSale.curyDiscBal>(cache, null, true);

				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = true;
				Transactions.Cache.AllowUpdate = true;
				Transactions.Cache.AllowInsert = doc.CustomerID != null && doc.CustomerLocationID != null;
				release.SetEnabled(doc.Hold == false);
				voidCheck.SetEnabled(false);
			}

			PXUIFieldAttribute.SetEnabled<ARCashSale.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<ARCashSale.refNbr>(cache, doc);
			PXUIFieldAttribute.SetEnabled<ARCashSale.batchNbr>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyLineTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyTaxTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyDocBal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyCommnblAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyCommnAmt>(cache, doc, false);
            PXUIFieldAttribute.SetEnabled<ARCashSale.curyVatExemptTotal>(cache, doc, false);
            PXUIFieldAttribute.SetEnabled<ARCashSale.curyVatTaxableTotal>(cache, doc, false);

			PXUIFieldAttribute.SetEnabled<ARCashSale.cleared>(cache, doc, clearEnabled);
			PXUIFieldAttribute.SetEnabled<ARCashSale.clearDate>(cache, doc, clearEnabled && doc.Cleared == true);

			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.deposited>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositType>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositNbr>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositDate>(cache, null, false);

			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAfter>(cache, doc, isDepositAfterEditable && doc.DepositAsBatch == true);
			PXUIFieldAttribute.SetRequired<ARCashSale.depositAfter>(cache, isDepositAfterEditable && doc.DepositAsBatch == true);

			PXPersistingCheck depositAfterPersistCheck = (isDepositAfterEditable && doc.DepositAsBatch == true) ? PXPersistingCheck.NullOrBlank
																											    : PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<ARCashSale.depositAfter>(cache, doc, depositAfterPersistCheck);

			if (doc.CustomerID != null && Transactions.Any())
				{
					PXUIFieldAttribute.SetEnabled<ARCashSale.customerID>(cache, doc, false);
				}

			PXUIFieldAttribute.SetEnabled<ARCashSale.cCPaymentStateDescr>(cache, null, false);

			bool isDeposited = string.IsNullOrEmpty(doc.DepositNbr) == false && string.IsNullOrEmpty(doc.DepositType) == false;
			CashAccount cashAccount = this.cashaccount.Current;
			bool isClearingAccount = cashAccount != null && cashAccount.CashAccountID == doc.CashAccountID && cashAccount.ClearingAccount == true;
			bool enableDepositEdit = !isDeposited && cashAccount != null && (isClearingAccount || doc.DepositAsBatch != isClearingAccount);

			if (enableDepositEdit)
			{
				cache.AllowUpdate = true;
				PXSetPropertyException exception = doc.DepositAsBatch != isClearingAccount ? new PXSetPropertyException(Messages.DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag, PXErrorLevel.Warning)
																						   : null;
				cache.RaiseExceptionHandling<ARCashSale.depositAsBatch>(doc, doc.DepositAsBatch, exception);
			}

			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAsBatch>(cache, doc, enableDepositEdit);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAfter>(cache, doc, !isDeposited && isClearingAccount && doc.DepositAsBatch == true);

			ARAddress address = this.Billing_Address.Select();
			bool enableAddressValidation = doc.Released == false &&
										   address != null && address.IsDefaultAddress == false && address.IsValidated == false;

			this.validateAddresses.SetEnabled(enableAddressValidation);

			bool allowPaymentChargesEdit = doc.Released != true && (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn);
			this.PaymentCharges.Cache.AllowInsert = allowPaymentChargesEdit;
			this.PaymentCharges.Cache.AllowUpdate = allowPaymentChargesEdit;
			this.PaymentCharges.Cache.AllowDelete = allowPaymentChargesEdit;

			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert;
			Taxes.Cache.AllowUpdate = Transactions.Cache.AllowUpdate;
			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete;

			#region Migration Mode Settings

			bool isMigratedDocument = doc.IsMigratedRecord == true;
			bool isUnreleasedMigratedDocument = isMigratedDocument && doc.Released != true;

			if (isUnreleasedMigratedDocument)
			{
				PaymentCharges.Cache.AllowSelect = false;
			}

			bool disableCaches = arsetup.Current?.MigrationMode == true
				? !isMigratedDocument
				: isUnreleasedMigratedDocument;
			if (disableCaches)
			{
				bool primaryCacheAllowInsert = Document.Cache.AllowInsert;
				bool primaryCacheAllowDelete = Document.Cache.AllowDelete;
				this.DisableCaches();
				Document.Cache.AllowInsert = primaryCacheAllowInsert;
				Document.Cache.AllowDelete = primaryCacheAllowDelete;
			}

			#endregion
			if (IsApprovalRequired(doc, cache))
			{
				if (doc.Status == ARDocStatus.PendingApproval || doc.Status == ARDocStatus.Rejected)
				{
					release.SetEnabled(false);
				}

				if (doc.DocType == ARDocType.CashReturn)
				{
					if ((doc.Status == ARDocStatus.PendingApproval || doc.Status == ARDocStatus.Rejected ||
						doc.Status == ARDocStatus.Closed || doc.Status == ARDocStatus.Balanced) && doc.DontApprove == false)
					{
						PXUIFieldAttribute.SetEnabled(cache, doc, false);
					}
					if (doc.Status == ARDocStatus.PendingApproval || doc.Status == ARDocStatus.Rejected ||
						doc.Status == ARDocStatus.Balanced)
					{
						PXUIFieldAttribute.SetEnabled<ARPayment.hold>(cache, doc, true);
					}
				}

				if (doc.Status == ARDocStatus.PendingApproval 
					|| doc.Status == ARDocStatus.Rejected 
					|| (doc.Status == ARDocStatus.Balanced && doc.DontApprove == false)
					|| doc.Status == ARDocStatus.Closed)
				{
					Transactions.Cache.AllowInsert = false;
					Taxes.Cache.AllowInsert = false;
					Approval.Cache.AllowInsert = false;
					salesPerTrans.Cache.AllowInsert = false;
					PaymentCharges.Cache.AllowInsert = false;

					Transactions.Cache.AllowUpdate = false;
					Taxes.Cache.AllowUpdate = false;
					Approval.Cache.AllowUpdate = false;
					salesPerTrans.Cache.AllowUpdate = false;
					PaymentCharges.Cache.AllowUpdate = false;

					Transactions.Cache.AllowDelete = false;
					Taxes.Cache.AllowDelete = false;
					Approval.Cache.AllowDelete = false;
					salesPerTrans.Cache.AllowDelete = false;
					PaymentCharges.Cache.AllowDelete = false;
				}
			}

		    PXUIFieldAttribute.SetEnabled<ARCashSale.docType>(cache, doc, true);
		    PXUIFieldAttribute.SetEnabled<ARCashSale.refNbr>(cache, doc, true);
		}

        protected virtual void ARCashSale_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
			ARCashSale doc = e.Row as ARCashSale;
			if (doc != null)
			{
				using (new PXConnectionScope())
				{
					PXFormulaAttribute.CalcAggregate<ARPaymentChargeTran.curyTranAmt>(PaymentCharges.Cache, e.Row, true);
				}
			}
        }

		protected virtual void ARCashSale_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARCashSale)e.Row).Released == false )
			{
				e.NewValue = ((ARCashSale)e.Row).AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARCashSale)e.Row).Released == false)
			{
				e.NewValue = ((ARCashSale)e.Row).AdjFinPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_TranPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARCashSale)e.Row).Released == false)
			{
				e.NewValue = ((ARCashSale)e.Row).AdjTranPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale row = e.Row as ARCashSale;
			if (row != null)
			{
				foreach (ARTran tran in Transactions.Select())
				{
					Transactions.Cache.SetDefaultExt<ARTran.projectID>(tran);
				}

			}
		}

		protected virtual void ARCashSale_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.Row, -(((ARRegister)e.Row).OrigDocAmt));
		}

		protected virtual void ARCashSale_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;
			if (payment.Released == false)
			{
					payment.DocDate = payment.AdjDate;

				payment.FinPeriodID = payment.AdjFinPeriodID;
				payment.TranPeriodID = payment.AdjTranPeriodID;

				sender.RaiseExceptionHandling<ARCashSale.finPeriodID>(e.Row, payment.FinPeriodID, null);
			}

			ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.Row, (((ARRegister)e.Row).OrigDocAmt));
		}

		protected virtual void ARCashSale_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARCashSale doc = e.Row as ARCashSale;
			if (doc.Released != true && !this.IsCopyPasteContext)
			{
					doc.DocDate = doc.AdjDate;

				doc.FinPeriodID  = doc.AdjFinPeriodID;
				doc.TranPeriodID = doc.AdjTranPeriodID;

				sender.RaiseExceptionHandling<ARCashSale.finPeriodID>(doc, doc.FinPeriodID, null);

				if (sender.ObjectsEqual<ARCashSale.curyDocBal, ARCashSale.curyOrigDiscAmt>(e.Row, e.OldRow) == false && doc.CuryDocBal - doc.CuryOrigDiscAmt != doc.CuryOrigDocAmt)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDiscAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARCashSale.curyOrigDocAmt>(doc, doc.CuryDocBal - doc.CuryOrigDiscAmt);
					else
						sender.SetValueExt<ARCashSale.curyOrigDocAmt>(doc, 0m);
				}
				else if (sender.ObjectsEqual<ARCashSale.curyOrigDocAmt>(e.Row, e.OldRow) == false)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDocAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARCashSale.curyOrigDiscAmt>(doc, doc.CuryDocBal - doc.CuryOrigDocAmt);
					else
						sender.SetValueExt<ARCashSale.curyOrigDiscAmt>(doc, 0m);
				}
				if (doc.Hold != true)
				{
					if (doc.CuryDocBal < doc.CuryOrigDocAmt)
					{
						sender.RaiseExceptionHandling<ARCashSale.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (doc.CuryOrigDocAmt < 0)
					{
						sender.RaiseExceptionHandling<ARCashSale.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}
					else
					{
						sender.RaiseExceptionHandling<ARCashSale.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, null);
					}
				}

				PaymentCharges.UpdateChangesFromPayment(sender, e);
			}

			if (e.OldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.OldRow, -(((ARRegister)e.OldRow).OrigDocAmt));
			}
			ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.Row, (((ARRegister)e.Row).OrigDocAmt));
		}

        protected virtual void ParentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
			if (!sender.ObjectsEqual<ARCashSale.branchID>(e.Row, e.OldRow))
        {
            foreach (ARSalesPerTran tran in salesPerTrans.Select())
            {
                this.salesPerTrans.Cache.MarkUpdated(tran);
            }
        }
        }
		#endregion

		#region CurrencyInfo Events
		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (customer.Current != null && customer.Current.AllowOverrideRate == false)
				{
					curyenabled = false;
				}

				ARCashSale doc = PXSelect<ARCashSale, Where<ARCashSale.curyInfoID, Equal<Current<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph);
				if (doc != null && doc.DocType == ARDocType.CashReturn)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryID))
				{
					e.NewValue = cashaccount.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryRateTypeID))
				{
					e.NewValue = cashaccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((ARCashSale)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}
		#endregion

		#region CATran Events
		protected virtual void CATran_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_TranPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_CuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region ARTran events
		protected virtual void ARTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran != null && tran.InventoryID == null && location.Current != null)
			{
				e.NewValue = location.Current.CSalesAcctID;
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran != null && tran.AccountID != null && location.Current != null)
			{
				InventoryItem item = (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, tran.InventoryID);
				EPEmployee employee = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, PXAccess.GetUserID());
				CRLocation companyloc =
					PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<GL.Branch.branchID, Equal<Required<ARTran.branchID>>>>.Select(this, tran.BranchID);
				SalesPerson salesperson = (SalesPerson)PXSelect<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<ARTran.salesPersonID>>>>.SelectSingleBound(this, new object[] { e.Row });

				int? customer_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cSalesSubID>(location.Current);
				int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.salesSubID>(item);
				int? employee_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.salesSubID>(employee);
				int? company_SubID = (int?)Caches[typeof(CRLocation)].GetValue<CRLocation.cMPSalesSubID>(companyloc);
				int? salesperson_SubID = (int?)Caches[typeof(SalesPerson)].GetValue<SalesPerson.salesSubID>(salesperson);

				object value = SubAccountMaskAttribute.MakeSub<ARSetup.salesSubMask>(this, arsetup.Current.SalesSubMask,
					new object[] { customer_SubID, item_SubID, employee_SubID, company_SubID, salesperson_SubID },
					new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(EPEmployee.salesSubID), typeof(Location.cMPSalesSubID), typeof(SalesPerson.salesSubID) });

				sender.RaiseFieldUpdating<ARTran.subID>(e.Row, ref value);

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category")]
        [ARCashSaleTax(typeof(ARCashSale), typeof(ARTax), typeof(ARTaxTran), parentBranchIDField: typeof(ARCashSale.branchID),
			   //Per Unit Tax settings
			   Inventory = typeof(ARTran.inventoryID), UOM = typeof(ARTran.uOM), LineQty = typeof(ARTran.qty))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
        protected virtual void ARTran_TaxCategoryID_CacheAttached(PXCache sender)
        {
        }

		[PXBool]
		[DR.DRTerms.Dates(typeof(ARTran.dRTermStartDate), typeof(ARTran.dRTermEndDate), typeof(ARTran.inventoryID), typeof(ARTran.deferredCode))]
		protected virtual void ARTran_RequiresTerms_CacheAttached(PXCache sender) { }

		protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (TaxAttribute.GetTaxCalc<ARTran.taxCategoryID>(sender, e.Row) == TaxCalc.Calc && taxzone.Current != null && !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID) && ((ARTran)e.Row).InventoryID == null)
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
			}
		}

		protected virtual void ARTran_UnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((ARTran)e.Row).InventoryID == null)
			{
				e.NewValue = 0m;
			}
		}


		protected virtual void ARTran_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row == null)
                return;

            ARCashSale doc = this.Document.Current;
			if (doc != null && row.InventoryID != null && row.UOM != null && doc.CustomerID != null && row.Qty != null && row.ManualPrice != true)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;
				Location c = location.Select();
				if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
					customerPriceClass = c.CPriceClassID;

                e.NewValue = ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, doc.CustomerID, ((ARTran)e.Row).InventoryID, ((ARTran)e.Row).SiteID, currencyinfo.Select(), ((ARTran)e.Row).UOM, row.Qty, doc.DocDate.Value, row.CuryUnitPrice) ?? 0m;

				ARSalesPriceMaint.CheckNewUnitPrice<ARTran, ARTran.curyUnitPrice>(sender, row, e.NewValue);
			}
			else
			{
				e.NewValue = sender.GetValue<ARTran.curyUnitPrice>(e.Row);
				e.Cancel = e.NewValue != null;
				return;
			}
		}

		protected virtual void ARTran_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				if (row.ManualPrice != true && row.IsFree != true && !sender.Graph.IsCopyPasteContext)
				{
					sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
				}
			}
		}

		protected virtual void ARTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.unitPrice>(e.Row);
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
			sender.SetValue<ARTran.unitPrice>(e.Row, null);
		}

		protected virtual void ARTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
                sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
			}
		}


		protected virtual void ARTran_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SOShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARTran_SalesPersonID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.subID>(e.Row);
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>), PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		protected virtual void ARTran_InventoryID_CacheAttached(PXCache sender)
        {
        }


		protected virtual void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.accountID>(e.Row);
			sender.SetDefaultExt<ARTran.subID>(e.Row);
			sender.SetDefaultExt<ARTran.taxCategoryID>(e.Row);
			sender.SetDefaultExt<ARTran.deferredCode>(e.Row);
			sender.SetDefaultExt<ARTran.uOM>(e.Row);

			sender.SetDefaultExt<ARTran.unitPrice>(e.Row);
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);

			ARTran tran = e.Row as ARTran;
			IN.InventoryItem item = PXSelectorAttribute.Select<IN.InventoryItem.inventoryID>(sender, tran) as IN.InventoryItem;
			if (item != null && tran != null)
			{
				tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, "Descr", customer.Current?.LocaleName);
			}
		}

		protected virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;
			if (row != null)
			{
				TaxAttribute.Calculate<ARTran.taxCategoryID, ARCashSaleTaxAttribute>(sender, e);
			}

			if (row.ManualDisc != true)
			{
				var discountCode = (ARDiscount)PXSelectorAttribute.Select<SOLine.discountID>(sender, row);
				row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
			}

			if ((e.ExternalCall || sender.Graph.IsImport)
					&& sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.uOM>(e.Row, e.OldRow)
					&& sender.ObjectsEqual<ARTran.qty>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow)
					&& sender.ObjectsEqual<ARTran.siteID>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.manualPrice>(e.Row, e.OldRow)
					&& (!sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow))
					&& row.ManualPrice == oldRow.ManualPrice)
				row.ManualPrice = true;

			if (row.ManualPrice != true)
			{
				row.CuryUnitPriceDR = row.CuryUnitPrice;
			}
		}

		protected virtual void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			TaxAttribute.Calculate<ARTran.taxCategoryID, ARCashSaleTaxAttribute>(sender, e);
		}

		protected virtual void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
		}

		protected virtual void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARTran documentLine = e.Row as ARTran;
			if (documentLine == null) return;

			viewSchedule.SetEnabled(sender.GetStatus(e.Row) != PXEntryStatus.Inserted);

			#region Migration Mode Settings

			ARCashSale doc = Document.Current;

			if (doc != null &&
				doc.IsMigratedRecord == true &&
				doc.Released != true)
			{
				PXUIFieldAttribute.SetEnabled<ARTran.defScheduleID>(sender, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.deferredCode>(sender, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.dRTermStartDate>(sender, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.dRTermEndDate>(sender, null, false);
			}

			#endregion
		}

		protected virtual void ARTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row == null) return;

			DR.ScheduleHelper.DeleteAssociatedScheduleIfDeferralCodeChanged(this, e.Row as ARTran);
		}

		protected virtual void ARTran_DrCr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = ARInvoiceType.DrCr(Document.Current.DocType);
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_DRTermStartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var line = e.Row as ARTran;

			if (line != null && line.RequiresTerms == true)
			{
				e.NewValue = Document.Current.DocDate;
			}
		}
		#endregion

		#region ARTaxTran Events
		protected virtual void ARTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void ARTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is ARTaxTran arTaxTran))
				return;

			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}

		protected virtual void ARTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				((ARTaxTran)e.Row).TaxZoneID = Document.Current.TaxZoneID;
			}
		}
		#endregion

		#region ARSalesPerTran events

		protected virtual void ARSalesPerTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ARSalesPerTran row = (ARSalesPerTran)e.Row;
			foreach (ARSalesPerTran iSpt in this.salesPerTrans.Select())
			{
				if (iSpt.SalespersonID == row.SalespersonID)
				{
					PXEntryStatus status = this.salesPerTrans.Cache.GetStatus(iSpt);
					if (!(status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted))
					{
						sender.RaiseExceptionHandling<ARSalesPerTran.salespersonID>(e.Row, null, new PXException(Messages.ERR_DuplicatedSalesPersonAdded));
						e.Cancel = true;
						break;
					}
				}
			}
		}
		#endregion

        #region Voiding
        private bool _IsVoidCheckInProgress = false;

		protected virtual void ARCashSale_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_AdjFinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		public virtual void VoidCheckProc(ARCashSale doc)
		{
			this.Clear(PXClearOption.PreserveTimeStamp);

            TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.NoCalc);

			foreach (PXResult<ARCashSale, CurrencyInfo> res in PXSelectJoin<ARCashSale, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARCashSale.curyInfoID>>>, Where<ARCashSale.docType, Equal<Required<ARCashSale.docType>>, And<ARCashSale.refNbr, Equal<Required<ARCashSale.refNbr>>>>>.Select(this, (object)doc.DocType, doc.RefNbr))
			{
				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				ARCashSale newdocument = new ARCashSale();
				newdocument.DocType = ARDocType.CashReturn;
				newdocument.RefNbr = null;
				newdocument.CuryInfoID = info.CuryInfoID;
				newdocument.OrigDocType = ((ARCashSale)res).DocType;
				newdocument.OrigRefNbr = ((ARCashSale)res).RefNbr;
                newdocument.OrigModule = GL.BatchModule.AR;

                newdocument = Document.Insert(newdocument);

				if (newdocument.RefNbr == null)
				{
					//manual numbering, check for occasional duplicate
					ARCashSale duplicate = PXSelect<ARCashSale>.Search<ARCashSale.docType, ARCashSale.refNbr>(this, newdocument.DocType, newdocument.OrigRefNbr);
					if (duplicate != null)
					{
						throw new PXException(ErrorMessages.RecordExists);
					}

					newdocument.RefNbr = newdocument.OrigRefNbr;
					this.Document.Cache.Normalize();
					newdocument = this.Document.Update(newdocument);
				}

				newdocument = PXCache<ARCashSale>.CreateCopy((ARCashSale)res);
                newdocument.OrigModule = GL.BatchModule.AR;

				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);
				if (state.IsCaptured)
				{
					newdocument.RefTranExtNbr = state.ExternalTransaction.TranNumber;
				}
				newdocument.CuryInfoID = info.CuryInfoID;
				newdocument.DocType = Document.Current.DocType;
				newdocument.RefNbr = Document.Current.RefNbr;
				newdocument.OrigDocType = Document.Current.OrigDocType;
				newdocument.OrigRefNbr = Document.Current.OrigRefNbr;
				newdocument.CATranID = null;
				newdocument.NoteID = null;
				newdocument.RefNoteID = null;
				newdocument.IsTaxPosted = false;
				newdocument.IsTaxValid = false;

				//must set for _RowSelected
				newdocument.OpenDoc = true;
				newdocument.Released = false;
				Document.Cache.SetDefaultExt<ARCashSale.isMigratedRecord>(newdocument);
				newdocument.Hold = (arsetup.Current.HoldEntry ?? false) || IsApprovalRequired(newdocument, Document.Cache);			
				newdocument.Printed = false;
				newdocument.Emailed = false;
				newdocument.LineCntr = 0;
				newdocument.AdjCntr = 0;
				newdocument.BatchNbr = null;
				newdocument.AdjDate = doc.DocDate;
			    FinPeriodIDAttribute.SetPeriodsByMaster<ARCashSale.adjFinPeriodID>(Document.Cache, newdocument, doc.AdjTranPeriodID);
				newdocument.CuryDocBal = newdocument.CuryOrigDocAmt + newdocument.CuryOrigDiscAmt;
				newdocument.CuryChargeAmt = 0;
				newdocument.CuryConsolidateChargeTotal = 0;
				newdocument.ClosedDate = null;
				newdocument.ClosedFinPeriodID = null;
				newdocument.ClosedTranPeriodID = null;

				newdocument.Cleared = false;
				newdocument.ClearDate = null;

				newdocument.Deposited = false;
				newdocument.DepositDate = null;
				newdocument.DepositType = null;
				newdocument.DepositNbr = null;

                newdocument.CuryVatTaxableTotal = 0m;
                newdocument.CuryVatExemptTotal = 0m;
				newdocument = this.Document.Update(newdocument);

				if (info != null)
				{
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARCashSale.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}
			}

			this.FieldDefaulting.AddHandler<ARTran.salesPersonID>((sender, e) => { e.NewValue = null; e.Cancel = true; });

			foreach (ARTran srcTran in PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Required<ARTran.tranType>>, And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<Where<ARTran.lineType, IsNull, Or<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				ARTran tran = PXCache<ARTran>.CreateCopy(srcTran);
				tran.TranType = null;
				tran.RefNbr = null;
				tran.DrCr = null;
				tran.Released = null;
                tran.CuryInfoID = null;
				tran.NoteID = null;

				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARTran.salesPersonID>(this.Transactions.Cache, tran);
				if (sp == null || sp.IsActive == false)
					tran.SalesPersonID = null;

				tran = Transactions.Insert(tran);
				PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, srcTran, Transactions.Cache, tran);
			}

			this.RowInserting.AddHandler<ARSalesPerTran>((sender, e) => { e.Cancel = true; });

			foreach (ARSalesPerTran salespertran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>, And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				ARSalesPerTran newtran = PXCache<ARSalesPerTran>.CreateCopy(salespertran);

				newtran.DocType = Document.Current.DocType;
				newtran.RefNbr = Document.Current.RefNbr;
				newtran.Released = false;
				newtran.CuryInfoID = null;
				newtran.CuryCommnblAmt *= -1m;
				newtran.CuryCommnAmt *= -1m;

				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARSalesPerTran.salespersonID>(this.salesPerTrans.Cache, newtran);
				if (!(sp == null || sp.IsActive == false))
				{
					this.salesPerTrans.Update(newtran);
				}
			}

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			if (!IsExternalTax(doc.TaxZoneID))
			{
				foreach (ARTaxTran tax in PXSelect<ARTaxTran, Where<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>, And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
				{
					ARTaxTran new_artax = new ARTaxTran();
					new_artax.TaxID = tax.TaxID;

					new_artax = this.Taxes.Insert(new_artax);

					if (new_artax != null)
					{
						new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
						new_artax.TaxRate = tax.TaxRate;
						new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_artax.CuryTaxAmt = tax.CuryTaxAmt;
						new_artax.CuryTaxDiscountAmt = tax.CuryTaxDiscountAmt;
						new_artax.CuryTaxableDiscountAmt = tax.CuryTaxableDiscountAmt;
						new_artax = this.Taxes.Update(new_artax);
					}
				}
			}
			ARCashSale document = Document.Current;
			document.CuryOrigDiscAmt = doc.CuryOrigDiscAmt;
			Document.Update(document);

			PaymentCharges.ReverseCharges(doc, Document.Current);
        }
		#endregion

		#region External Tax Provider

		public virtual bool IsExternalTax(string taxZoneID)
			{
					return false;
					}

		public virtual ARCashSale CalculateExternalTax(ARCashSale invoice)
					{
			return invoice;
		}

		#endregion

		public class PaymentTransaction : PaymentTransactionGraph<ARCashSaleEntry, ARCashSale>
		{
			public PXSelect<ExternalTransaction> externalTran;

			protected override PaymentTransactionDetailMapping GetPaymentTransactionMapping()
			{
				return new PaymentTransactionDetailMapping(typeof(CCProcTran));
			}

			protected override ExternalTransactionDetailMapping GetExternalTransactionMapping()
			{
				return new ExternalTransactionDetailMapping(typeof(ExternalTransaction));
			}

			protected override PaymentMapping GetPaymentMapping()
			{
				return new PaymentMapping(typeof(ARCashSale));
			}

			protected override void MapViews(ARCashSaleEntry graph)
			{
				this.PaymentTransaction = new PXSelectExtension<PaymentTransactionDetail>(Base.ccProcTran);
				this.ExternalTransaction = new PXSelectExtension<ExternalTransactionDetail>(Base.ExternalTran);
			}

			protected override void BeforeVoidPayment(ARCashSale doc)
			{
				base.BeforeVoidPayment(doc);
				ReleaseAfterVoid = doc.VoidAppl == true && doc.Released == false && this.ARSetup.Current.IntegratedCCProcessing == true;
			}

			protected override void BeforeCapturePayment(ARCashSale doc)
			{
				base.BeforeCapturePayment(doc);
				ReleaseAfterCapture = doc.Released == false && ARSetup.Current.IntegratedCCProcessing == true;
			}

			protected override void BeforeCreditPayment(ARCashSale doc)
			{
				base.BeforeCreditPayment(doc);
				ReleaseAfterCredit = doc.Released == false && ARSetup.Current.IntegratedCCProcessing == true;
			}

			protected override IEnumerable<AfterTranProcDelegate> GetAfterAuthorizeActions()
			{
				yield return ChangeDocProcessingStatus;
			}

			protected override IEnumerable<AfterTranProcDelegate> GetAfterCaptureActions()
			{
				yield return (IBqlTable table, CCTranType type, bool success)
					=> ReleaseAfterCapture = ((ARCashSale)table).Released == false && ARSetup.Current.IntegratedCCProcessing == true;
				yield return ChangeDocProcessingStatus;
				yield return UpdateCashSale;
			}

			protected override IEnumerable<AfterTranProcDelegate> GetAfterVoidActions()
			{
				yield return ChangeDocProcessingStatus;
				yield return UpdateCashSale;
			}

			protected override IEnumerable<AfterTranProcDelegate> GetAfterCreditActions()
			{
				yield return ChangeDocProcessingStatus;
				yield return UpdateCashSale;
			}

			protected override void RowSelected(Events.RowSelected<ARCashSale> e)
			{
				base.RowSelected(e);
				Base.ccProcTran.AllowUpdate = false;
				Base.ccProcTran.AllowDelete = false;
				Base.ccProcTran.AllowInsert = false;
				ARCashSale doc = e.Row;
				if (doc == null)
					return;
				TranHeldwarnMsg = AR.Messages.CCProcessingARPaymentTranHeldWarning;
				PXCache cache = e.Cache;
				bool docTypePayment = IsDocTypePayment(doc);
				doc.IsCCPayment = false;
				bool enableCCProcess = EnableCCProcess(doc);

				bool isPMInstanceRequired = false;
				if (!string.IsNullOrEmpty(doc.PaymentMethodID))
				{
					isPMInstanceRequired = Base.paymentmethod.Current?.IsAccountNumberRequired ?? false;
				}

				if (doc.IsMigratedRecord != true &&
					Base.paymentmethod.Current != null &&
					Base.paymentmethod.Current.PaymentType == CA.PaymentMethodType.CreditCard)
				{
					doc.IsCCPayment = true;
				}
		
				ExternalTransactionState tranState = GetActiveTransactionState();
				bool canAuthorize = doc.Hold == false && docTypePayment && !(tranState.IsPreAuthorized || tranState.IsCaptured);
				bool canCapture = doc.Hold == false && docTypePayment && !tranState.IsCaptured;
				bool canVoid = doc.Hold == false && (doc.DocType == ARDocType.CashReturn && (tranState.IsCaptured || tranState.IsPreAuthorized)) ||
							   (tranState.IsPreAuthorized && docTypePayment);
				bool canCredit = doc.Hold == false && doc.DocType == ARDocType.CashReturn && !tranState.IsRefunded;

				this.authorizeCCPayment.SetEnabled(enableCCProcess && canAuthorize);
				this.captureCCPayment.SetEnabled(enableCCProcess && canCapture);
				this.voidCCPayment.SetEnabled(enableCCProcess && canVoid);
				this.creditCCPayment.SetEnabled(enableCCProcess && canCredit);
				doc.CCPaymentStateDescr = GetPaymentStateDescr(tranState);

				bool canValidate = false;
				if (enableCCProcess)
				{
					canValidate = CanValidate(doc);
				}
				this.validateCCPayment.SetEnabled(canValidate);

				this.recordCCPayment.SetEnabled(false);
				this.recordCCPayment.SetVisible(false);
				this.captureOnlyCCPayment.SetEnabled(false);
				this.captureOnlyCCPayment.SetVisible(false);

				PXUIFieldAttribute.SetRequired<ARCashSale.extRefNbr>(cache, enableCCProcess || ARSetup.Current.RequireExtRef == true);
				PXUIFieldAttribute.SetVisible<ARCashSale.cCPaymentStateDescr>(cache, doc, enableCCProcess && doc.CCPaymentStateDescr != null);
				PXUIFieldAttribute.SetVisible<ARCashSale.refTranExtNbr>(cache, doc, ((doc.DocType == ARDocType.CashReturn) && enableCCProcess));
				PXUIFieldAttribute.SetRequired<ARPayment.pMInstanceID>(cache, isPMInstanceRequired);
				PXDefaultAttribute.SetPersistingCheck<ARPayment.pMInstanceID>(cache, doc, isPMInstanceRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				if (doc.Released == true || doc.Voided == true)
				{
					cache.AllowUpdate = enableCCProcess; 
				}
				else if (enableCCProcess && (tranState.IsPreAuthorized || tranState.IsCaptured))
				{
					PXUIFieldAttribute.SetEnabled(cache, doc, false);
					if (doc.Status != ARDocStatus.PendingApproval)
					{
						PXUIFieldAttribute.SetEnabled<ARCashSale.adjDate>(cache, doc, true);
						PXUIFieldAttribute.SetEnabled<ARCashSale.adjFinPeriodID>(cache, doc, true);
					}
					PXUIFieldAttribute.SetEnabled<ARCashSale.hold>(cache, doc, true);
					//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
					PXDBCurrencyAttribute.SetBaseCalc<ARCashSale.curyDocBal>(cache, null, true);
					PXDBCurrencyAttribute.SetBaseCalc<ARCashSale.curyDiscBal>(cache, null, true);

					cache.AllowDelete = false;
					cache.AllowUpdate = true;
					Base.Transactions.Cache.AllowDelete = true;
					Base.Transactions.Cache.AllowUpdate = true;
					Base.Transactions.Cache.AllowInsert = doc.CustomerID != null && doc.CustomerLocationID != null;
					Base.release.SetEnabled(doc.Hold == false);
					Base.voidCheck.SetEnabled(false);
				}
				else
				{

					PXUIFieldAttribute.SetEnabled<ARCashSale.refTranExtNbr>(cache, doc, enableCCProcess && ((doc.DocType == ARDocType.CashReturn) && !tranState.IsRefunded));
					PXUIFieldAttribute.SetEnabled<ARPayment.pMInstanceID>(cache, doc, isPMInstanceRequired);
					cache.AllowDelete = !ExternalTranHelper.HasTransactions(Base.ExternalTran);
				}

				bool enableVoidCheck = doc.Released == true && docTypePayment && doc.Voided == false;
				bool isCCStateClear = !(tranState.IsCaptured || tranState.IsPreAuthorized);

				if (doc.Released == false && !enableVoidCheck && docTypePayment && doc.Voided == false)
				{
					if (ExternalTranHelper.HasTransactions(Base.ExternalTran) && (isCCStateClear || (tranState.IsPreAuthorized && tranState.ProcessingStatus == ProcessingStatus.VoidFail)))
						enableVoidCheck = true;
				}

				Base.voidCheck.SetEnabled(enableVoidCheck);

				#region CCProcessing integrated with doc
				if (enableCCProcess && (ARSetup.Current.IntegratedCCProcessing ?? false))
				{
					if (doc.Released == false)
					{
						bool releaseActionEnabled = doc.Hold == false &&
													doc.OpenDoc == true &&
												   (doc.DocType == ARDocType.CashReturn ? tranState.IsRefunded : tranState.IsCaptured);

						Base.release.SetEnabled(releaseActionEnabled);
					}
				}
				#endregion

				PXUIFieldAttribute.SetEnabled<ARCashSale.docType>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARCashSale.refNbr>(cache, doc, true);
			}

			protected virtual void FieldUpdated(Events.FieldUpdated<ARCashSale.paymentMethodID> e)
			{
				PXCache cache = e.Cache;
				ARCashSale cashSale = e.Row as ARCashSale;
				if (cashSale == null) return;
				SetPendingProcessingIfNeeded(cache, cashSale);
			}

			public static bool IsDocTypeSuitableForCC(ARCashSale doc)
			{	
				bool isDocTypeSuitableForCC = (doc.DocType == ARDocType.CashSale) || (doc.DocType == ARDocType.CashReturn);
				return isDocTypeSuitableForCC;
			}

			public static bool IsDocTypePayment(ARCashSale doc)
			{
				bool docTypePayment = doc.DocType == ARDocType.CashSale;
				return docTypePayment;
			}

			public bool EnableCCProcess(ARCashSale doc)
			{
				bool enableCCProcess = false;

				if (doc.IsMigratedRecord != true &&
					Base.paymentmethod.Current != null &&
					Base.paymentmethod.Current.PaymentType == CA.PaymentMethodType.CreditCard)
				{
					enableCCProcess = IsDocTypeSuitableForCC(doc);
				}
				enableCCProcess = enableCCProcess && !doc.Voided.Value;

				return enableCCProcess;
			}

			public bool CanValidate(ARCashSale doc)
			{
				bool enableCCProcess = EnableCCProcess(doc);

				if (!enableCCProcess)
					return false;

				ExternalTransactionState tranState = GetActiveTransactionState();
				bool canValidate = doc.Hold == false && IsDocTypePayment(doc) && tranState.IsActive;

				bool getTranSupported = false;
				if (canValidate)
				{
					CCProcessingCenter procCenter = Base.ProcessingCenter.SelectSingle();
					getTranSupported = CCProcessingFeatureHelper.IsFeatureSupported(procCenter, CCProcessingFeature.TransactionGetter);
				}

				bool result = canValidate && getTranSupported;
				return result;
			}

			private string GetPaymentStateDescr(ExternalTransactionState state)
			{
				return GetLastTransactionDescription();
			}

			protected void SetPendingProcessingIfNeeded(PXCache sender, ARCashSale document)
			{
				PaymentMethod pm = new PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>(Base)
					.SelectSingle(document.PaymentMethodID);
				bool pendingProc = false;
				if (pm != null && pm.PaymentType == PaymentMethodType.CreditCard && pm.IsAccountNumberRequired == true
					&& document.Released == false)
				{
					pendingProc = true;
				}
				sender.SetValue<ARRegister.pendingProcessing>(document, pendingProc);
			}

			private static void ChangeDocProcessingStatus(IBqlTable aTable, CCTranType tranType, bool success)
			{
				ARCashSale cashSale = (ARCashSale)aTable;
				if (success)
				{
					var cashSaleGraph = GetGraphByDocTypeRefNbr(cashSale.DocType, cashSale.RefNbr);
					var extTran = cashSaleGraph.ExternalTran.SelectSingle();
					bool pendingProcessing = true;
					if (extTran != null)
					{
						ExternalTransactionState state = ExternalTranHelper.GetTransactionState(cashSaleGraph, extTran);
						if (state.IsCaptured && !state.IsOpenForReview)
						{
							pendingProcessing = false;
						}
						if (cashSale.DocType == ARDocType.CashReturn && (state.IsVoided || state.IsRefunded)
							&& !state.IsOpenForReview)
						{
							pendingProcessing = false;
						}
						ChangeDocProcessingFlags(state, cashSale, tranType);
						UpdateOriginDocProcessingStatus(cashSaleGraph, tranType);
					}
					cashSale.PendingProcessing = pendingProcessing;
					cashSaleGraph.Document.Update(cashSale);
					cashSaleGraph.Save.Press();
					cashSaleGraph.Document.Cache.RestoreCopy(cashSale, cashSaleGraph.Document.Current);
				}
			}

			private static void UpdateOriginDocProcessingStatus(ARCashSaleEntry graph, CCTranType tranType)
			{
				IExternalTransaction tran = graph.ExternalTran.SelectSingle();
				ARCashSale cashSale = graph.Document.Current;
				ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(graph, tran);
				if (tranType == CCTranType.VoidOrCredit 
					&& tran.DocType == cashSale.OrigDocType && tran.RefNbr == cashSale.OrigRefNbr
					&& tran.DocType == ARDocType.CashSale)
				{
					var oCashSaleGraph = GetGraphByDocTypeRefNbr(tran.DocType, tran.RefNbr);
					var oExtTran = oCashSaleGraph.ExternalTran.SelectSingle();
					ARCashSale oCashSale = oCashSaleGraph.Document.Current;
					if (oExtTran.TransactionID == tran.TransactionID)
					{
						ChangeDocProcessingFlags(tranState, oCashSale, tranType);
						graph.Caches[typeof(ARCashSale)].Update(oCashSale);
					}
				}
			}

			public static void UpdateCashSale(IBqlTable aTable, CCTranType tranType, bool success)
			{
				if (!success)
					return;
				ARCashSale toProc = (ARCashSale)aTable;
				ARCashSaleEntry cashSaleGraph = GetGraphByDocTypeRefNbr(toProc.DocType, toProc.RefNbr);
				IExternalTransaction currTran = cashSaleGraph.ExternalTran.SelectSingle();
				if (currTran != null)
				{
					toProc.DocDate = currTran.LastActivityDate.Value.Date;
					toProc.AdjDate = currTran.LastActivityDate.Value.Date;

					ExternalTransactionState tranState = ExternalTranHelper.GetTransactionState(cashSaleGraph, currTran);
					if (tranState.IsActive)
					{
						toProc.ExtRefNbr = currTran.TranNumber;
					}
					else if (toProc.DocType != ARDocType.CashReturn && (tranState.IsVoided || tranState.IsDeclined))
					{
						toProc.ExtRefNbr = null;
					}

					cashSaleGraph.Document.Update(toProc);
					cashSaleGraph.Save.Press();
					cashSaleGraph.Document.Cache.RestoreCopy(aTable, cashSaleGraph.Document.Current);
				}
			}

			private static ARCashSaleEntry GetGraphByDocTypeRefNbr(string docType, string refNbr)
			{
				var cashSaleGraph = CreateInstance<ARCashSaleEntry>();
				cashSaleGraph.Document.Current = PXSelect<ARCashSale, Where<ARCashSale.docType, Equal<Required<ARCashSale.docType>>,
					And<ARCashSale.refNbr, Equal<Required<ARCashSale.refNbr>>>>>
						.SelectWindowed(cashSaleGraph, 0, 1, docType, refNbr);
				return cashSaleGraph;
			}

			private static void ChangeDocProcessingFlags(ExternalTransactionState tranState, ARCashSale doc, CCTranType tranType)
			{
				if (tranState.HasErrors) return;
				doc.IsCCAuthorized = doc.IsCCCaptured = doc.IsCCRefunded = false;
				if (!tranState.IsDeclined && !tranState.IsOpenForReview && !ExternalTranHelper.IsExpired(tranState.ExternalTransaction))
				{
					switch (tranType)
					{
						case CCTranType.AuthorizeAndCapture: doc.IsCCCaptured = true; break;
						case CCTranType.CaptureOnly: doc.IsCCCaptured = true; break;
						case CCTranType.PriorAuthorizedCapture: doc.IsCCCaptured = true; break;
						case CCTranType.AuthorizeOnly: doc.IsCCAuthorized = true; break;
						case CCTranType.Credit: doc.IsCCRefunded = true; break;
					}
					if (tranType == CCTranType.VoidOrCredit && tranState.IsRefunded)
					{
						doc.IsCCRefunded = true;
					}
				}
			}
		}
	}

	public class ARDataEntryGraph<TGraph, TPrimary> : PXGraph<TGraph, TPrimary>, PX.Objects.GL.IVoucherEntry
		where TGraph : PXGraph
		where TPrimary : ARRegister, new()
	{
		public PXAction DeleteButton
		{
			get
			{
				return this.Delete;
			}
		}
		public PXAction<TPrimary> release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TPrimary> voidCheck;
		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable VoidCheck(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TPrimary> viewBatch;
		[PXUIField(DisplayName = "Review Batch", Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			foreach (TPrimary ardoc in adapter.Get<TPrimary>())
			{
				if (!String.IsNullOrEmpty(ardoc.BatchNbr))
				{
					JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
					graph.BatchModule.Current = PXSelect<Batch,
						Where<Batch.module, Equal<BatchModule.moduleAR>,
						And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
						.Select(this, ardoc.BatchNbr);
					throw new PXRedirectRequiredException(graph, "Current batch record");
				}
			}
			return adapter.Get();
		}

		public PXAction<TPrimary> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXString()]
			string ActionName)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					List<object> items = new List<object>();
					foreach (object record in adapter.Get())
					{
						items.Add(record);
					}

					Save.Press();

					List<object> result = new List<object>();
					PXAdapter newAdapter = new PXAdapter(new PXView.Dummy(this, adapter.View.BqlSelect, items));
					newAdapter.MassProcess = adapter.MassProcess;
					foreach (object data in action.Press(newAdapter))
					{
						result.Add(data);
					}
					return result;
				}
			}
			return adapter.Get();
		}

		public PXAction<TPrimary> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.InquiriesFolder)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					foreach (object data in action.Press(adapter)) ;
				}
			}
			return adapter.Get();
		}

		public virtual Dictionary<string, string> PrepareReportParams(string reportID, TPrimary doc)
		{
			var parameters = new Dictionary<string, string>();

			string[] reportsWithParams = new string[] { ARReports.AREditDetailedReportID, ARReports.ARRegisterDetailedReportID };
			Reports.Controls.Report report = PX.Data.Reports.PXReportTools.LoadReport(reportID, null);
			
			if (report == null)
				throw new PXException(ErrorMessages.ElementDoesntExistOrNoRights, reportID);
			
			if (reportsWithParams.Contains(reportID))
			{
				Dictionary<string, string> generalParams = new Dictionary<string, string>
				{
					["DocType"] = doc.DocType,
					["RefNbr"] = doc.RefNbr,
					["OrgBAccountID"] = PXAccess.GetBranchCD(doc.BranchID)
				};

				foreach (Reports.ReportParameter param in report.Parameters)
				{
					string paramValue = null;
					bool isGeneralParam = generalParams.TryGetValue(param.Name, out paramValue);

					if (!isGeneralParam && param.Nullable)
					{
						parameters[param.Name] = null;
					}
					else if (isGeneralParam)
					{
						parameters[param.Name] = paramValue;
					}
				}
			}
			else
			{
				string tableName = doc.GetType().Name;
				var generalFilters = new Dictionary<string, string>();
				generalFilters[tableName + ".DocType"] = doc.DocType;
				generalFilters[tableName + ".RefNbr"] = doc.RefNbr;

				foreach (Reports.FilterExp filter in report.Filters)
				{
					string filterValue = null;
					bool isGeneralFilter = generalFilters.TryGetValue(filter.DataField, out filterValue);

					if (isGeneralFilter)
					{
						parameters[filter.DataField] = filterValue;
					}
				}
			}
			return parameters;
		}

		public PXAction<TPrimary> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID
			)
		{
		    PXReportRequiredException ex = null;
			Dictionary<PX.SM.PrintSettings, PXReportRequiredException > reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				this.Caches[typeof(TPrimary)].MarkUpdated(doc);
				Dictionary <string, string> parameters = PrepareReportParams(reportID, doc);
				string customerReportID = GetCustomerReportID(reportID, doc);

				ex = PXReportRequiredException.CombineReport(ex, customerReportID, parameters);

				reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters, adapter, new NotificationUtility(this).SearchPrinter, ARNotificationSource.Customer, reportID, customerReportID, doc.BranchID);
			}

			this.Save.Press();
			if (ex != null)
			{
				PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint);

				throw ex;
			}

			return adapter.Get();
		}

		public virtual string GetCustomerReportID(string reportID, TPrimary doc)
		{
			return reportID;
		}

		public ARDataEntryGraph()
			: base()
		{
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
		}
	}
}
