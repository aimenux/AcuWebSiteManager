using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;

using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.BQLConstants;
using PX.Objects.EP;
using PX.Objects.SO;
using PX.Objects.DR;
using PX.Objects.CA;

using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.GL.Reclassification.UI;
using PX.Objects.AR.BQL;
using PX.Objects.Common.Extensions;
using PX.Objects.PM;
using System.Text;
using PX.Objects.Common.Bql;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL.FinPeriods;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AR
{
	[Serializable]
	public class ARInvoiceEntry : ARDataEntryGraph<ARInvoiceEntry, ARInvoice>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
	    #region Extensions

	    public class ARInvoiceEntryDocumentExtension : InvoiceGraphExtension<ARInvoiceEntry>
	    {
	        protected virtual AdjustMapping GetAdjustMapping()
	        {
	            return new AdjustMapping(typeof(ARAdjust));
            }

            public PXSelectExtension<Adjust> ApplyingAdjustments;

            public override void Initialize()
	        {
	            base.Initialize();

	            Documents = new PXSelectExtension<Invoice>(Base.Document);
	            Lines = new PXSelectExtension<DocumentLine>(Base.Transactions);
				AppliedAdjustments = new PXSelectExtension<Adjust2>(Base.Adjustments);
	            ApplyingAdjustments = new PXSelectExtension<Adjust>(Base.Adjustments_1);

            }

            protected override InvoiceMapping GetDocumentMapping()
	        {
	            return new InvoiceMapping(typeof(ARInvoice))
	            {
	                HeaderTranPeriodID = typeof(ARInvoice.tranPeriodID),
                    HeaderDocDate = typeof(ARInvoice.docDate)
	            };
	        }

	        protected override DocumentLineMapping GetDocumentLineMapping()
	        {
	            return new DocumentLineMapping(typeof(ARTran));
	        }

	        protected override Adjust2Mapping GetAdjust2Mapping()
	        {
	            return new Adjust2Mapping(typeof(ARAdjust2));
	        }

            protected override void _(Events.RowUpdated<Invoice> e)
	        {
	            base._(e);

	            if (ShouldUpdateAdjustmentsOnDocumentUpdated(e))
	            {
	                foreach (Adjust adjust in ApplyingAdjustments.Select())
	                {
                        if (!e.Cache.ObjectsEqual<Invoice.branchID>(e.Row, e.OldRow))
                        {
                            ApplyingAdjustments.Cache.SetDefaultExt<Adjust.adjgBranchID>(adjust);
                        }

                        if (!e.Cache.ObjectsEqual<Invoice.headerTranPeriodID>(e.Row, e.OldRow))
	                    {
	                        FinPeriodIDAttribute.DefaultPeriods<Adjust.adjgFinPeriodID>(ApplyingAdjustments.Cache, adjust);
	                    }

	                    (ApplyingAdjustments.Cache as PXModelExtension<Adjust>)?.UpdateExtensionMapping(adjust);

	                    ApplyingAdjustments.Cache.MarkUpdated(adjust);
	                }
	            }
	        }
        }

	    #endregion

		protected DiscountEngine<ARTran, ARInvoiceDiscountDetail> ARDiscountEngine => DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>();

		#region Internal Definitions + Cache Attached Events
		#region ARInvoice
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(Visibility = PXUIVisibility.Invisible, DisplayName = "Location")]
		protected virtual void ARInvoice_CustomerLocationID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[LastFinchargeDate]
		protected virtual void ARInvoice_LastFinChargeDate_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[LastPaymentDate]
		protected virtual void ARInvoice_LastPaymentDate_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Original Document", Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void ARInvoice_OrigRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true)]
		[PXFormula(typeof(
			IIf<Where<ExternalCall, Equal<True>, Or<PendingValue<ARInvoice.termsID>, IsNull>>,
				IIf<Where<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>,
					Selector<ARInvoice.customerID, Customer.termsID>,
					Null>,
				ARInvoice.termsID>))]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[ARTermsSelector]
		[Terms(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), typeof(ARInvoice.curyOrigDocAmt), typeof(ARInvoice.curyOrigDiscAmt))]
		protected virtual void ARInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}
		#endregion

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
		[PXDBDefault(typeof(ARInvoice.docType))]
		protected virtual void ARSalesPerTran_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARInvoice.refNbr))]
		[PXParent(typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<Current<ARSalesPerTran.docType>>,
						 And<ARInvoice.refNbr, Equal<Current<ARSalesPerTran.refNbr>>>>>))]
		protected virtual void ARSalesPerTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDBDefault(typeof(ARInvoice.branchID), DefaultForInsert = true, DefaultForUpdate = true)]
		protected virtual void ARSalesPerTran_BranchID_CacheAttached(PXCache sender)
		{
		}

		[SalesPerson(DirtyRead = true, Enabled = false, IsKey = true, DescriptionField = typeof(Contact.displayName))]
		protected virtual void ARSalesPerTran_SalespersonID_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		protected virtual void ARSalesPerTran_AdjNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXDefault(ARDocType.Undefined)]
		protected virtual void ARSalesPerTran_AdjdDocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault("")]
		protected virtual void ARSalesPerTran_AdjdRefNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<Search<CustSalesPeople.commisionPct, Where<CustSalesPeople.bAccountID, Equal<Current<ARInvoice.customerID>>,
				And<CustSalesPeople.locationID, Equal<Current<ARInvoice.customerLocationID>>,
				And<CustSalesPeople.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>>,
			Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		protected virtual void ARSalesPerTran_CommnPct_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commissionable Amount", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<ARInvoice.curyCommnblAmt>))]
		protected virtual void ARSalesPerTran_CuryCommnblAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Mult<ARSalesPerTran.curyCommnblAmt, Div<ARSalesPerTran.commnPct, decimal100>>), typeof(SumCalc<ARInvoice.curyCommnAmt>))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		protected virtual void ARSalesPerTran_CuryCommnAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region PMTran
		[PXDBString(3, IsFixed = true)]
		[PXDBDefault(typeof(ARInvoice.docType), PersistingCheck = PXPersistingCheck.Nothing)]
		public void PMTran_ARTranType_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXDBDefault(typeof(ARInvoice.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		public void PMTran_ARRefNbr_CacheAttached(PXCache sender) { }

		[PXDBInt]
		[PXDBDefault(typeof(ARTran.lineNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		public void PMTran_RefLineNbr_CacheAttached(PXCache sender) { }
		#endregion
		[PXDBDefault(typeof(ARRegister.branchID))]
		[Branch(Enabled = false)]
		protected virtual void ARTaxTran_BranchID_CacheAttached(PXCache sender)
		{
		}
		
		#region ARTran
		[IN.LocationAvail(typeof(ARTran.inventoryID), typeof(ARTran.subItemID), typeof(ARTran.siteID), typeof(ARTran.tranType), typeof(ARTran.invtMult), false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void ARTran_LocationID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(ARInvoice.branchID))]
		protected virtual void ARAdjust_AdjgBranchID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Actions
		public ToggleCurrency<ARInvoice> CurrencyView;

		public PXAction<ARInvoice> viewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			ARTran currentLine = Transactions.Current;

			if (currentLine != null &&
				Transactions.Cache.GetStatus(currentLine) == PXEntryStatus.Notchanged)
			{
				Save.Press();
				ViewScheduleForLine(this, Document.Current, currentLine);
			}

			return adapter.Get();
		}

		public static void ViewScheduleForLine(PXGraph graph, ARRegister document, ARTran documentLine)
		{
			PXSelectBase<DRSchedule> correspondingScheduleView = new PXSelect<
				DRSchedule,
					Where<
						DRSchedule.module, Equal<BatchModule.moduleAR>,
				And<DRSchedule.docType, Equal<Current<ARTran.tranType>>,
				And<DRSchedule.refNbr, Equal<Current<ARTran.refNbr>>,
						And<DRSchedule.lineNbr, Equal<Current<ARTran.lineNbr>>>>>>>
				(graph);

			DRSchedule correspondingSchedule = correspondingScheduleView.Select();

			if (correspondingSchedule == null || correspondingSchedule.IsDraft == true)
			{
				PXResult<ARTax, Tax> tax = (PXResult<ARTax, Tax>)PXSelectJoin<
					ARTax,
						LeftJoin<Tax, On<Tax.taxID, Equal<ARTax.taxID>>>,
					Where<
						ARTax.tranType, Equal<Required<ARTax.tranType>>,
						And<ARTax.refNbr, Equal<Required<ARTax.refNbr>>,
						And<ARTax.lineNbr, Equal<Required<ARTax.lineNbr>>>>>>
					.Select(graph, documentLine.TranType, documentLine.RefNbr, documentLine.LineNbr);

				var actualAmount = ARReleaseProcess.GetSalesPostingAmount(graph, document, documentLine, tax, tax,
					amount => PXDBCurrencyAttribute.Round(graph.Caches[typeof(ARTran)], documentLine, amount, CMPrecision.TRANCURY));

				DRDeferredCode deferralCode = PXSelect<
					DRDeferredCode,
					Where<
						DRDeferredCode.deferredCodeID, Equal<Current2<ARTran.deferredCode>>>>
					.Select(graph);

				if (deferralCode != null)
				{
					DRProcess process = PXGraph.CreateInstance<DRProcess>();
					process.CreateSchedule(documentLine, deferralCode, document, actualAmount.Base.Value, isDraft: true);
					process.Actions.PressSave();

					correspondingScheduleView.Cache.Clear();
					correspondingScheduleView.Cache.ClearQueryCache();

					correspondingSchedule = correspondingScheduleView.Select();
				}
			}

			if (correspondingSchedule != null)
			{
				PXRedirectHelper.TryRedirect(
					graph.Caches[typeof(DRSchedule)],
					correspondingSchedule,
					"View Schedule",
					PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		public PXAction<ARInvoice> newCustomer;
		[PXUIField(DisplayName = "New Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewCustomer(PXAdapter adapter)
		{
			CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
			throw new PXRedirectRequiredException(graph, "New Customer");
		}

		public PXAction<ARInvoice> editCustomer;
		[PXUIField(DisplayName = "Edit Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable EditCustomer(PXAdapter adapter)
		{
			if (customer.Current != null)
			{
				CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
				graph.BAccount.Current = customer.Current;
				throw new PXRedirectRequiredException(graph, "Edit Customer");
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> customerDocuments;
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

		public PXAction<ARInvoice> sOInvoice;
		[PXUIField(DisplayName = "SO Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SOInvoice(PXAdapter adapter)
		{
			ARInvoice invoice = Document.Current;
			SOInvoiceEntry graph = CreateInstance<SOInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
			throw new PXRedirectRequiredException(graph, "SO Invoice");
		}

		public PXAction<ARInvoice> viewProforma;
		[PXUIField(DisplayName = PM.Messages.ViewProforma, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewProforma(PXAdapter adapter)
		{
			if( Document.Current != null && Document.Current.ProformaExists == true)
			{
				ProformaEntry target = PXGraph.CreateInstance<ProformaEntry>();
				target.Document.Current = PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Current<ARInvoice.docType>>,
					And<PMProforma.aRInvoiceRefNbr, Equal<Current<ARInvoice.refNbr>>, Or<PMProforma.aRInvoiceRefNbr, Equal<Current<ARInvoice.origRefNbr>>>>>>.Select(this);
				throw new PXRedirectRequiredException(target, true, "ViewInvoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<ARInvoice> sendARInvoiceMemo;
		[PXUIField(DisplayName = "Send AR Invoice/Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SendARInvoiceMemo(
			PXAdapter adapter,
			[PXString]
			string reportID)
		{
			ARInvoice invoice = Document.Current;
			if (reportID == null) reportID = "AR641000";
			if (invoice != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["DocType"] = invoice.DocType;
				mailParams["RefNbr"] = invoice.RefNbr;
				if (!ReportNotificationGenerator.Send(reportID, mailParams).Any())
				{
					throw new PXException(ErrorMessages.MailSendFailed);
				}
				Clear();
				Document.Current = Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
			}
			return adapter.Get();
		}

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
			foreach (ARInvoice ardoc in adapter.Get<ARInvoice>())
			{
				OnBeforeRelease(ardoc);

				if (ardoc.Hold == false && ardoc.Released == false)
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

			PXLongOperation.StartOperation(this, delegate()
			{
                ReleaseProcess(list);
            });

			return list;
		}

		public virtual ARRegister OnBeforeRelease(ARRegister doc)
        {
			return doc;
                }

        public void ReleaseProcess(List<ARRegister> list)
        {
            PXTimeStampScope.SetRecordComesFirst(typeof(ARInvoice), true);

            ARDocumentRelease.ReleaseDoc(list, false, null, (a, b) => { });
        }

		public PXAction<ARInvoice> writeOff;
		[PXUIField(DisplayName = "Write Off", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable WriteOff(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.DocType == ARDocType.Invoice || Document.Current.DocType == ARDocType.DebitMemo || Document.Current.DocType == ARDocType.CreditMemo
				 || Document.Current.DocType == ARDocType.FinCharge || Document.Current.DocType == ARDocType.SmallCreditWO))
			{
				this.Save.Press();

				Customer c = customer.Select(Document.Current.CustomerID);
				if (c != null)
				{
					if (c.SmallBalanceAllow != true)
					{
						throw new PXException(Messages.WriteOffIsDisabled);
					}
					else if (c.SmallBalanceLimit < Document.Current.CuryDocBal)
					{
						decimal limit = c.SmallBalanceLimit ?? 0m;
						int precision = currencyinfo.Current != null && currencyinfo.Current.BasePrecision != null ? (int)currencyinfo.Current.BasePrecision : 2;
						throw new PXException(Messages.WriteOffIsOutOfLimit, limit.ToString("N" + precision));
					}
				}

				ARCreateWriteOff target = PXGraph.CreateInstance<ARCreateWriteOff>();
				if (Document.Current.DocType == ARDocType.CreditMemo)
					target.Filter.Cache.SetValueExt<ARWriteOffFilter.woType>(target.Filter.Current, ARWriteOffType.SmallCreditWO);
				target.Filter.Cache.SetValueExt<ARWriteOffFilter.branchID>(target.Filter.Current, Document.Current.BranchID);
				target.Filter.Cache.SetValueExt<ARWriteOffFilter.customerID>(target.Filter.Current, Document.Current.CustomerID);
				target.Filter.Cache.RaiseFieldUpdated<ARWriteOffFilter.wODate>(target.Filter.Current, target.Filter.Current.WODate);

				foreach (PX.Objects.AR.ARCreateWriteOff.ARRegisterEx doc in target.ARDocumentList.Select())
				{
					if (doc.DocType == Document.Current.DocType && doc.RefNbr == Document.Current.RefNbr)
					{
						doc.Selected = true;
						target.ARDocumentList.Update(doc);
					}
				}

				throw new PXRedirectRequiredException(target, "Create Write-Off");
			}

			return adapter.Get();
		}

		public PXAction<ARInvoice> ViewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Document.Current.OrigDocType, Document.Current.OrigRefNbr, Document.Current.OrigModule);
			return adapter.Get();
					}

		public PXAction<ARInvoice> reverseInvoice;

		[PXUIField(DisplayName = "Reverse Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[PXActionRestriction(typeof(Where<Current<ARInvoice.isRetainageDocument>, Equal<True>, Or<Current<ARInvoice.retainageApply>, Equal<True>>>), Messages.ReverseRetainage)]
		public virtual IEnumerable ReverseInvoice(PXAdapter adapter) => ReverseDocumentAndApplyToReversalIfNeeded(adapter, applyOriginalDocToReversal: false);

		public PXAction<ARInvoice> reverseInvoiceAndApplyToMemo;

		[PXUIField(DisplayName = "Reverse and Apply to Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReverseInvoiceAndApplyToMemo(PXAdapter adapter) => ReverseDocumentAndApplyToReversalIfNeeded(adapter, applyOriginalDocToReversal: true);

		public PXAction<ARInvoice> payInvoice;
		[PXUIField(DisplayName = Messages.EnterPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable PayInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Released == true)
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();

				if (Document.Current.Payable == true && Document.Current.OpenDoc == true)
				{
					if (Document.Current.PendingPPD == true)
					{
						throw new PXSetPropertyException(Messages.PaidPPD);
					}

					ARAdjust2 adj = PXSelect<ARAdjust2, Where<ARAdjust2.adjdDocType, Equal<Current<ARInvoice.docType>>,
						And<ARAdjust2.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>, And<ARAdjust2.released, Equal<False>, And<ARAdjust2.voided, Equal<False>>>>>>.Select(this);
					if (adj != null)
					{
						pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
					}
					else
					{
						pe.CreatePayment(Document.Current);
					}
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
				else if (Document.Current.DocType == ARDocType.CreditMemo)
				{
					pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Document.Current.RefNbr, Document.Current.DocType);
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> createSchedule;
		[PXUIField(DisplayName = "Assign to Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ImageKey = PX.Web.UI.Sprite.Main.Shedule)]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CreateSchedule(PXAdapter adapter)
		{
			if (Document.Current == null) return adapter.Get();

			Save.Press();

			IsSchedulable<ARRegister>.Ensure(this, Document.Current);

			ARScheduleMaint scheduleMaint = PXGraph.CreateInstance<ARScheduleMaint>();

			if ((bool)Document.Current.Scheduled && Document.Current.ScheduleID != null)
			{
				scheduleMaint.Schedule_Header.Current = scheduleMaint.Schedule_Header.Search<Schedule.scheduleID>(Document.Current.ScheduleID);
			}
			else
			{
				scheduleMaint.Schedule_Header.Cache.Insert();
				ARRegister doc = (ARRegister)scheduleMaint.Document_Detail.Cache.CreateInstance();
				PXCache<ARRegister>.RestoreCopy(doc, Document.Current);
				doc = (ARRegister)scheduleMaint.Document_Detail.Cache.Update(doc);
			}

			throw new PXRedirectRequiredException(scheduleMaint, "Create Schedule");
		}

		public PXAction<ARInvoice> reclassifyBatch;
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

		public PXAction<ARInvoice> autoApply;
		[PXUIField(DisplayName = "Auto Apply", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AutoApply(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.DocType == ARDocType.Invoice && Document.Current.Released == false)
			{
				foreach (ARAdjust2 adj in Adjustments_Inv.Select())
				{
					if (adj == null) continue;

					adj.CuryAdjdAmt = 0m;
					Adjustments_Inv.Cache.MarkUpdated(adj);
				}

				decimal? CuryApplAmt = Document.Current.CuryDocBal;

				foreach (ARAdjust2 adj in Adjustments
					.Select()
					.RowCast<ARAdjust2>()
					.Where(adj => adj.CuryDocBal > 0m))
					{
						if (adj.CuryDocBal > CuryApplAmt)
						{
							adj.CuryAdjdAmt = CuryApplAmt;
							Adjustments.Cache.RaiseFieldUpdated<ARAdjust2.curyAdjdAmt>(adj, 0m);
							Adjustments.Cache.MarkUpdated(adj);
							break;
						}
						else
						{
							adj.CuryAdjdAmt = adj.CuryDocBal;
							CuryApplAmt -= adj.CuryDocBal;
							Adjustments.Cache.RaiseFieldUpdated<ARAdjust2.curyAdjdAmt>(adj, 0m);
							Adjustments.Cache.MarkUpdated(adj);
						}
					}
				Adjustments.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> viewPayment;
		[PXUIField(
			DisplayName = "View Payment",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewPayment(PXAdapter adapter)
		{
			if (Document.Current != null && Adjustments.Current != null)
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
				pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Adjustments.Current.AdjgRefNbr, Adjustments.Current.AdjgDocType);

				throw new PXRedirectRequiredException(pe, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> viewInvoice;
		[PXUIField(
			DisplayName = "View Invoice",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && Adjustments_1.Current != null)
			{
				ARInvoiceEntry pe = CreateInstance<ARInvoiceEntry>();
				pe.Document.Current = pe.Document.Search<ARInvoice.refNbr>(Adjustments_1.Current.AdjdRefNbr, Adjustments_1.Current.AdjdDocType);

				throw new PXRedirectRequiredException(pe, true, "Invoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			
			return adapter.Get();
		}

		public PXAction<ARInvoice> viewInvoice2;
		[PXUIField(
			DisplayName = "View Document",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewInvoice2(PXAdapter adapter)
		{
			if (Document.Current != null && Adjustments_2.Current != null)
			{
				switch(Adjustments_2.Current.AdjType)
				{
					case ARAdjust.adjType.Adjusted:
					{
						ARInvoiceEntry pe = CreateInstance<ARInvoiceEntry>();
						pe.Document.Current = pe.Document.Search<ARInvoice.refNbr>(Adjustments_2.Current.AdjdRefNbr, Adjustments_2.Current.AdjdDocType);

						throw new PXRedirectRequiredException(pe, true, "Invoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
					case ARAdjust.adjType.Adjusting:
					{
						ARPaymentEntry pe = CreateInstance<ARPaymentEntry>();
						pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Adjustments_2.Current.AdjgRefNbr, Adjustments_2.Current.AdjgDocType);

						throw new PXRedirectRequiredException(pe, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> viewItem;
		[PXUIField(
			DisplayName = "View Item", 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select, 
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewItem(PXAdapter adapter)
		{
			if (Transactions.Current != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID,
					Equal<Current<ARTran.inventoryID>>>>.SelectSingleBound(this, null);
				if (item != null)
				{
					PXRedirectHelper.TryRedirect(Caches[typeof(InventoryItem)], item, "View Item", PXRedirectHelper.WindowMode.NewWindow);
				}
			}
		
			return adapter.Get();
		}

		public PXAction<ARInvoice> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (ARInvoice current in adapter.Get<ARInvoice>())
			{
				if (current != null)
				{
					ARAddress billAddress = this.Billing_Address.Select();
					if (billAddress != null && billAddress.IsDefaultAddress == false && billAddress.IsValidated == false)
					{
						PXAddressValidator.Validate<ARAddress>(this, billAddress, true);
					}
					ARShippingAddress shipAddress = this.Shipping_Address.Select();
					if (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
					{
						PXAddressValidator.Validate<ARShippingAddress>(this, shipAddress, true);
				}
				}
				yield return current;
			}
		}

		public PXAction<ARInvoice> recalculateDiscountsAction;
		[PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
		{
			if (adapter.MassProcess)
			{
				PXLongOperation.StartOperation(this, delegate ()
				{
					ARDiscountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate, recalcdiscountsfilter.Current, DiscountEngine.DefaultARDiscountCalculationParameters);
					this.Save.Press();
				});
			}
			else if (!adapter.ExternalCall || recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
			{				
				ARDiscountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate, recalcdiscountsfilter.Current, DiscountEngine.DefaultARDiscountCalculationParameters);
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> recalcOk;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable RecalcOk(PXAdapter adapter)
		{
			return adapter.Get();
		}
		
		public static readonly Dictionary<string, string> ARDocTypeDict = new ARDocType.ListAttribute().ValueLabelDic;
		
		public virtual void ClearRetainageSummary(ARInvoice document)
		{
			document.CuryLineRetainageTotal = 0m;
			document.CuryRetainageTotal = 0m;
			document.CuryRetainageUnreleasedAmt = 0m;
			document.CuryRetainedTaxTotal = 0m;
			document.CuryRetainedDiscTotal = 0m;
		}

		/// <summary>
		/// Check if reversing retainage document already exists.
		/// </summary>
		public virtual bool CheckReversingRetainageDocumentAlreadyExists(ARInvoice origDoc, out ARRegister reversingDoc)
		{
			reversingDoc = PXSelect<ARRegister,
				Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.origDocType, Equal<Required<ARRegister.origDocType>>,
					And<ARRegister.origRefNbr, Equal<Required<ARRegister.origRefNbr>>>>>,
				OrderBy<Desc<ARRegister.createdDateTime>>>
				.SelectSingleBound(this, null, ARDocType.CreditMemo, origDoc.DocType, origDoc.RefNbr);

			return
				reversingDoc != null &&
				(reversingDoc.IsOriginalRetainageDocument() == origDoc.IsOriginalRetainageDocument() ||
					reversingDoc.IsChildRetainageDocument() == origDoc.IsChildRetainageDocument());
		}

		public PXAction<ARInvoice> customerRefund;
		[PXUIField(DisplayName = "Customer Refund", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CustomerRefund(PXAdapter adapter)
		{
			if (Document.Current != null && (bool)Document.Current.Released && Document.Current.DocType == ARDocType.CreditMemo && (bool)Document.Current.OpenDoc)
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();

				ARAdjust adj = PXSelect<ARAdjust, Where<ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>,
					And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>, And<ARAdjust.released, Equal<False>>>>>.Select(this);

				if (adj != null)
				{
					pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
				}
				else
				{
					pe.Clear();
					pe.CreatePayment(Document.Current);
				}
				throw new PXRedirectRequiredException(pe, nameof(CustomerRefund)); 
			}
			return adapter.Get();
		}


		#endregion
		#region Override PXGraph.GetStateExt
		private object disableJoined(object val)
		{
			PXFieldState stat = val as PXFieldState;
			if (stat != null)
			{
				stat.Enabled = false;
			}
			return val;
		}
		public override object GetStateExt(string viewName, object data, string fieldName)
		{
			if (viewName == "Adjustments")
			{
				if (data == null)
				{
					int pos = fieldName.IndexOf("__");
					if (pos > 0 && pos < fieldName.Length - 2)
					{
						string s = fieldName.Substring(0, pos);
						PXCache cache = null;
						foreach (Type t in Views[viewName].GetItemTypes())
						{
							if (t.Name == s)
							{
								cache = Caches[t];
							}
						}
						if (cache == null)
						{
							cache = Caches[s];
						}
						if (cache != null)
						{
							return disableJoined(cache.GetStateExt(null, fieldName.Substring(pos + 2)));
						}
						return null;
					}
					else
					{
						return Caches[GetItemType(viewName)].GetStateExt(null, fieldName);
					}
				}
				else
				{
					return base.GetStateExt(viewName, data, fieldName);
				}
			}
			else
			{
				return base.GetStateExt(viewName, data, fieldName);
			}
		}
		#endregion
		#region Selects

		public PXSelect<Standalone.ARRegister> dummy_register;
		public PXSelect<InventoryItem> dummy_nonstockitem_for_redirect_newitem;
		[PXHidden]
		public PXSelect<BAccount> bAccountBasic;
		[PXHidden]
		public PXSelect<BAccountR> bAccountRBasic;
		public PXSelect<AP.Vendor> dummy_vendor_taxAgency_for_avalara;

		[PXViewName(Messages.ARInvoice)]
		[PXCopyPasteHiddenFields(typeof(ARInvoice.invoiceNbr), FieldsToShowInSimpleImport = new[] { typeof(ARInvoice.invoiceNbr) })]
		public PXSelectJoin<ARInvoice,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>,
			Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
			And2<Where<ARInvoice.origModule, Equal<BatchModule.moduleAR>, Or<ARInvoice.origModule, Equal<BatchModule.moduleEP>, Or<ARInvoice.released, Equal<True>>>>,
			And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>> Document;
		public PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Current<ARInvoice.refNbr>>>>> CurrentDocument;

		public PXSelect<RUTROT.RUTROT, Where<True, Equal<False>>> Rutrots;

		[PXViewName(Messages.ARTran)]
		[PXImport(typeof(ARInvoice))]
		public PXOrderedSelect<ARInvoice, ARTran, 
			Where<
				ARTran.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
                And<Where<ARTran.lineType, IsNull, Or<ARTran.lineType, NotEqual<SOLineType.discount>>>>>>,
			OrderBy<
				Asc<ARTran.tranType, 
					Asc<ARTran.refNbr, 
					Asc<ARTran.sortOrder, 
					Asc<ARTran.lineNbr>>>>>>
			Transactions;

		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.lineType, Equal<SOLineType.discount>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Discount_Row;

		[PXCopyPasteHiddenView]
		public PXSelect<ARTax, Where<ARTax.tranType, Equal<Current<ARInvoice.docType>>, And<ARTax.refNbr, Equal<Current<ARInvoice.refNbr>>>>, OrderBy<Asc<ARTax.tranType, Asc<ARTax.refNbr, Asc<ARTax.taxID>>>>> Tax_Rows;
		[PXCopyPasteHiddenView]
        public PXSelectJoin<ARTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>, 
			Where<ARTaxTran.module, Equal<BatchModule.moduleAR>,
				And<ARTaxTran.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTaxTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>>> Taxes;

		/// <summary>
		/// Applications for the current document, except
		/// when it is a credit memo.
		/// </summary>
		[PXCopyPasteHiddenView]
		public PXSelectJoin<ARAdjust2,
			InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust2.adjgDocType>,
				And<ARPayment.refNbr, Equal<ARAdjust2.adjgRefNbr>>>>> Adjustments;

		/// <summary>
		/// Applications for the current document,
		/// when it is an unreleased credit memo.
		/// </summary>
		[PXCopyPasteHiddenView]
		public PXSelectJoin<ARAdjust,
			InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARAdjust.adjgDocType>,
				And<ARInvoice.refNbr, Equal<ARAdjust.adjgRefNbr>>>>> Adjustments_1;

		/// <summary>
		/// Applications for the current document,
		/// when it is a released credit memo.
		/// </summary>
		[PXCopyPasteHiddenView]
		public PXSelectJoin<ARAdjust,
			InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARAdjust.adjgDocType>,
				And<ARInvoice.refNbr, Equal<ARAdjust.adjgRefNbr>>>>> Adjustments_2;

		public PXSelectJoin<CCProcessingCenter, LeftJoin<CustomerPaymentMethod,
			On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>,
			Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<ARInvoice.pMInstanceID>>>> ProcessingCenter;

		public PXSelectJoin<
			ARAdjust2,
				InnerJoinSingleTable<ARPayment,
					On<ARPayment.docType, Equal<ARAdjust2.adjgDocType>,
				And<ARPayment.refNbr, Equal<ARAdjust2.adjgRefNbr>>>,
				InnerJoin<Standalone.ARRegisterAlias,
					On<Standalone.ARRegisterAlias.docType, Equal<ARAdjust2.adjgDocType>,
					And<Standalone.ARRegisterAlias.refNbr, Equal<ARAdjust2.adjgRefNbr>>>,
				InnerJoin<CurrencyInfo,
					On<CurrencyInfo.curyInfoID, Equal<Standalone.ARRegisterAlias.curyInfoID>>>>>,
			Where<
				ARAdjust2.adjdDocType, Equal<Current<ARInvoice.docType>>,
								And<ARAdjust2.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>,
				And<Where<
					Current<ARInvoice.released>, Equal<True>,
					Or<ARAdjust2.released, Equal<Current<ARInvoice.released>>>>>>>>
			Adjustments_Inv;

		public PXSelectJoin<
			ARAdjust,
				InnerJoinSingleTable<ARInvoice,
					On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>,
				And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
				InnerJoin<Standalone.ARRegisterAlias,
					On<Standalone.ARRegisterAlias.docType, Equal<ARAdjust.adjdDocType>,
					And<Standalone.ARRegisterAlias.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
				InnerJoin<CurrencyInfo,
					On<CurrencyInfo.curyInfoID, Equal<Standalone.ARRegisterAlias.curyInfoID>>>>>,
			Where<
				ARAdjust.adjgDocType, Equal<Current<ARInvoice.docType>>,
								And<ARAdjust.adjgRefNbr, Equal<Current<ARInvoice.refNbr>>,
				And<Where<
					Current<ARInvoice.released>, Equal<True>,
					Or<ARAdjust.released, Equal<Current<ARInvoice.released>>>>>>>>
			Adjustments_Crm;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		public PXSelect<
			ARInvoice, 
			Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, 
				And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, 
				And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>> 
			ARInvoice_CustomerID_DocType_RefNbr;

		public PXSelect<PM.PMTran, Where<PM.PMTran.aRTranType, Equal<Required<ARTran.tranType>>, And<PM.PMTran.aRRefNbr, Equal<Required<ARTran.refNbr>>, And<PM.PMTran.refLineNbr, Equal<Required<ARTran.lineNbr>>>>>> RefContractUsageTran;
		public PXSelect<PMBudgetAccum> Budget;
		[PXViewName(Messages.ARAddress)]
		public PXSelect<ARAddress, Where<ARAddress.addressID, Equal<Current<ARInvoice.billAddressID>>>> Billing_Address;
		[PXViewName(Messages.ARContact)]
		public PXSelect<ARContact, Where<ARContact.contactID, Equal<Current<ARInvoice.billContactID>>>> Billing_Contact;

		[PXViewName(Messages.ARAddress)]
		public PXSelect<ARShippingAddress, Where<ARShippingAddress.addressID, Equal<Current<ARInvoice.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.ARContact)]
		public PXSelect<ARShippingContact, Where<ARShippingContact.contactID, Equal<Current<ARInvoice.shipContactID>>>> Shipping_Contact;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>> currencyinfo;

		[PXViewName(Messages.Customer)]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<ARInvoice.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>> customerclass;
		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<ARInvoice.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Location.locationID, Equal<Optional<ARInvoice.customerLocationID>>>>> location;
		public PXSelect<ARBalances> arbalances;
		public PXSetup<OrganizationFinPeriod, Where<OrganizationFinPeriod.finPeriodID, Equal<Current<ARInvoice.finPeriodID>>,
													And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<ARInvoice.branchID>>>>> finperiod;
		public PXSetup<ARSetup> ARSetup;
		public PXSetup<GLSetup> glsetup;
		public PXSetupOptional<SOSetup> soSetup;
		[PXCopyPasteHiddenView]
		public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

		[PXCopyPasteHiddenView()]
		public PXSelectJoinGroupBy<ARDunningLetterDetail,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>,
			LeftJoin<ARDunningLetter, On<ARDunningLetter.dunningLetterID, Equal<ARDunningLetterDetail.dunningLetterID>>>>,
			Where<ARDunningLetterDetail.dunningLetterBAccountID, Equal<Customer.sharedCreditCustomerID>,
				And<ARDunningLetterDetail.refNbr, Equal<Current<ARInvoice.refNbr>>,
				And<ARDunningLetterDetail.docType, Equal<Current<ARInvoice.docType>>,
				And<ARDunningLetter.voided, Equal<False>,
				And<ARDunningLetter.released, Equal<True>,
				And<ARDunningLetterDetail.dunningLetterLevel, Greater<int0>>>>>>>,
			Aggregate<GroupBy<ARDunningLetter.voided,
				GroupBy<ARDunningLetter.released,
				GroupBy<ARDunningLetterDetail.refNbr,
				GroupBy<ARDunningLetterDetail.docType>>>>>> dunningLetterDetail;

		public PXSelect<ARInvoiceDiscountDetail, 
			Where<ARInvoiceDiscountDetail.docType, Equal<Current<ARInvoice.docType>>, 
				And<ARInvoiceDiscountDetail.refNbr, Equal<Current<ARInvoice.refNbr>>>>, 
			OrderBy<Asc<ARInvoiceDiscountDetail.lineNbr>>> ARDiscountDetails;

		public PXSelect<CustSalesPeople, Where<CustSalesPeople.bAccountID, Equal<Current<ARInvoice.customerID>>,
												And<CustSalesPeople.locationID, Equal<Current<ARInvoice.customerLocationID>>>>> salesPerSettings;
		public PXSelectJoin<ARSalesPerTran, LeftJoin<ARSPCommissionPeriod, On<ARSPCommissionPeriod.commnPeriodID, Equal<ARSalesPerTran.commnPaymntPeriod>>>,
												Where<ARSalesPerTran.docType, Equal<Current<ARInvoice.docType>>,
												And<ARSalesPerTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
												And<ARSalesPerTran.adjdDocType, Equal<ARDocType.undefined>,
												And2<Where<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byInvoice>, Or<Current<ARInvoice.released>, Equal<boolFalse>>>,
												Or<ARSalesPerTran.adjdDocType, Equal<Current<ARInvoice.docType>>,
												And<ARSalesPerTran.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>,
												And<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byPayment>>>>>>>>> salesPerTrans;
		public PXSelect<ARFinChargeTran, Where<ARFinChargeTran.tranType, Equal<Current<ARInvoice.docType>>,
												And<ARFinChargeTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>> finChargeTrans;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
		[PMDefaultMailTo(typeof(Select<ARContact, Where<ARContact.contactID, Equal<Current<ARInvoice.billContactID>>>>))]
		public PM.PMActivityList<ARInvoice>
			Activity;

		public PXSelect<RUTROT.RUTROTDistribution,
					Where<True, Equal<False>>> RRDistribution;


		public PXSelect<DRSchedule> dummySchedule_forPXParent;
		public PXSelect<DRScheduleDetail> dummyScheduleDetail_forPXParent;
		public PXSelect<DRScheduleTran> dummyScheduleTran_forPXParent;

		public PXFilter<DuplicateFilter> duplicatefilter;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;
		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}

		public PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>> CurrentBranch;
		public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>> InventoryItem;

		[PXCopyPasteHiddenView]
		public PXSelect<PM.PMBillingRecord> ProjectBillingRecord;
		[PXCopyPasteHiddenView]
		public PXSelect<PM.PMProforma> ProjectProforma;
		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMUnbilledDailySummaryAccum> UnbilledSummary;

		[PXCopyPasteHiddenView()]
		[PXViewName(CR.Messages.CustomerPaymentMethodDetails)]
		public PXSelect<CustomerPaymentMethod,
					Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>,
					And<CustomerPaymentMethod.paymentMethodID, Equal<Current<ARInvoice.paymentMethodID>>>>> CustomerPaymentMethodDetails;
		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;

		[PXHidden]
		public CustomerCreditHelperForInvoices CustomerCreditHelper;
		protected virtual void UpdateARBalances(PXCache cache, object newRow, object oldRow)
			=> CustomerCreditHelper.UpdateARBalances(cache, newRow, oldRow);

		[PXHidden]
		public PXSelect<CRRelation> RelationsLink;

		#region Retainage part
		
		[PXReadOnlyView]
		[PXCopyPasteHiddenView]
		// ARRetainageInvoice class is a ARRegister class alias
		// because only ARRegister part is affecting by the release process
		// and only this way we can get a proper behavior for the QueryCache mechanism.
		//
		public PXSelect<ARRetainageInvoice,
					Where<True, Equal<False>>> RetainageDocuments;

		#endregion

		#endregion

		internal Dictionary<string, HashSet<string>> TaxesByTaxCategory;

        [Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
        public virtual IEnumerable transactions()
		{
			return null;
		}

		public virtual IEnumerable taxes()
		{
			bool hasPPDTaxes = false;
			bool vatReportingInstalled = PXAccess.FeatureInstalled<FeaturesSet.vATReporting>();

			ARTaxTran artaxMax = null;
			decimal? DiscountedTaxableTotal = 0m;
			decimal? DiscountedPriceTotal = 0m;

			foreach (PXResult<ARTaxTran, Tax> res in PXSelectJoin<ARTaxTran,
				LeftJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>,
				Where<ARTaxTran.module, Equal<BatchModule.moduleAR>,
					And<ARTaxTran.tranType, Equal<Current<ARInvoice.docType>>,
					And<ARTaxTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>>>.Select(this))
			{
				if (vatReportingInstalled)
				{
					Tax tax = res;
					ARTaxTran artax = res;
					hasPPDTaxes = tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment || hasPPDTaxes;

					if (hasPPDTaxes &&
						Document.Current != null &&
						Document.Current.CuryOrigDocAmt != null &&
						Document.Current.CuryOrigDocAmt != 0m &&
						Document.Current.CuryOrigDiscAmt != null)
					{
						decimal CashDiscPercent = (decimal) (Document.Current.CuryOrigDiscAmt / Document.Current.CuryOrigDocAmt);
						bool isTaxable = ARPPDCreditMemoProcess.CalculateDiscountedTaxes(Taxes.Cache, artax, CashDiscPercent);
						DiscountedPriceTotal += artax.CuryDiscountedPrice;
						if (isTaxable)
						{
							DiscountedTaxableTotal += artax.CuryDiscountedTaxableAmt;
							if (artaxMax == null || artax.CuryDiscountedTaxableAmt > artaxMax.CuryDiscountedTaxableAmt)
							{
								artaxMax = artax;
							}
						}
					}
				}

				yield return res;
			}

			if (vatReportingInstalled && Document.Current != null)
			{
				Document.Current.HasPPDTaxes = hasPPDTaxes;
				if (hasPPDTaxes)
				{
					decimal? DiscountedDocTotal = DiscountedTaxableTotal + DiscountedPriceTotal;
					Document.Current.CuryDiscountedDocTotal = Document.Current.CuryOrigDocAmt - Document.Current.CuryOrigDiscAmt;

					if (artaxMax != null &&
						Document.Current.CuryVatTaxableTotal + Document.Current.CuryTaxTotal == Document.Current.CuryOrigDocAmt &&
						DiscountedDocTotal != Document.Current.CuryDiscountedDocTotal)
					{
						artaxMax.CuryDiscountedTaxableAmt += Document.Current.CuryDiscountedDocTotal - DiscountedDocTotal;
						DiscountedTaxableTotal = Document.Current.CuryDiscountedDocTotal - DiscountedPriceTotal;
					}

					Document.Current.CuryDiscountedPrice = DiscountedPriceTotal;
					Document.Current.CuryDiscountedTaxableTotal = DiscountedTaxableTotal;
				}
			}
		}

		#region Document Reversal
		public string GetReversingDocType(string docType)
		{
			switch (docType)
			{
				case ARDocType.Invoice:
				case ARDocType.DebitMemo:
					docType = ARDocType.CreditMemo;
					break;
				case ARDocType.CreditMemo:
					docType = ARDocType.DebitMemo;
					break;
			}

			return docType;
		}

		public virtual void ReverseInvoiceProc(ARRegister doc)
		{
			DuplicateFilter filter = PXCache<DuplicateFilter>.CreateCopy(duplicatefilter.Current);
			WebDialogResult dialogRes = duplicatefilter.View.Answer;

			this.Clear(PXClearOption.PreserveTimeStamp);

			foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res in ARInvoice_CurrencyInfo_Terms_Customer.Select(this, doc.DocType, doc.RefNbr))
			{
				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				ARInvoice invoice = PXCache<ARInvoice>.CreateCopy((ARInvoice)res);
				invoice.CuryInfoID = info.CuryInfoID;
				invoice.DocType = GetReversingDocType(invoice.DocType);
				invoice.ProformaExists = false;

				invoice.OrigModule = GL.BatchModule.AR;
				invoice.RefNbr = null;
				invoice.OrigModule = GL.BatchModule.AR;

				//must set for _RowSelected
				invoice.OpenDoc = true;
				invoice.Released = false;
				Document.Cache.SetDefaultExt<ARInvoice.hold>(invoice);
				Document.Cache.SetDefaultExt<ARInvoice.isMigratedRecord>(invoice);
				invoice.Printed = false;
				invoice.Emailed = false;
				invoice.BatchNbr = null;
				invoice.ScheduleID = null;
				invoice.Scheduled = false;
				invoice.NoteID = null;
				invoice.RefNoteID = null;

				invoice.TermsID = null;
				invoice.InstallmentCntr = null;
				invoice.InstallmentNbr = null;
				invoice.DueDate = null;
				invoice.DiscDate = null;
				invoice.CuryOrigDiscAmt = 0m;
				
				FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(CurrentDocument.Cache, invoice, doc.FinPeriodID);

				invoice.CuryDocBal = invoice.CuryOrigDocAmt;
				invoice.OrigDocDate = invoice.DocDate;

				if (doc.IsChildRetainageDocument())
				{
					invoice.OrigDocType = doc.OrigDocType;
					invoice.OrigRefNbr = doc.OrigRefNbr;
				}
				else
				{
					invoice.OrigDocType = doc.DocType;
					invoice.OrigRefNbr = doc.RefNbr;
				}

				invoice.CuryLineTotal = 0m;
				invoice.IsTaxPosted = false;
				invoice.IsTaxValid = false;
				invoice.CuryVatTaxableTotal = 0m;
				invoice.CuryVatExemptTotal = 0m;
				invoice.StatementDate = null;
				invoice.PendingPPD = false;

				if (!string.IsNullOrEmpty(invoice.PaymentMethodID))
				{
					CA.PaymentMethod pm = null;

					if (invoice.CashAccountID.HasValue)
					{
						CA.PaymentMethodAccount pmAccount = null;
						PXResult<CA.PaymentMethod, CA.PaymentMethodAccount> pmResult = (PXResult<CA.PaymentMethod, CA.PaymentMethodAccount>)
																						PXSelectJoin<CA.PaymentMethod,
																							LeftJoin<
																									 CA.PaymentMethodAccount, On<CA.PaymentMethod.paymentMethodID, Equal<CA.PaymentMethodAccount.paymentMethodID>>>,
																							   Where<
																									 CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>,
																									 And<CA.PaymentMethodAccount.cashAccountID, Equal<Required<CA.PaymentMethodAccount.cashAccountID>>>>>.
																						 Select(this, invoice.PaymentMethodID, invoice.CashAccountID);
						pm = pmResult;
						pmAccount = pmResult;

						if (pm == null || pm.UseForAR == false || pm.IsActive == false)
						{
							invoice.PaymentMethodID = null;
							invoice.CashAccountID = null;
						}
						else if (pmAccount == null || pmAccount.CashAccountID == null || pmAccount.UseForAR != true)
						{
							invoice.CashAccountID = null;
						}
					}
					else
					{
						pm = PXSelect<CA.PaymentMethod,
								Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>
                             .Select(this, invoice.PaymentMethodID);

						if (pm == null || pm.UseForAR == false || pm.IsActive == false)
						{
							invoice.PaymentMethodID = null;
							invoice.CashAccountID = null;
							invoice.PMInstanceID = null;
						}
					}

					if (invoice.PMInstanceID.HasValue)
					{
						CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod,
													   Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.
													   Select(this, invoice.PMInstanceID);

						if (string.IsNullOrEmpty(invoice.PaymentMethodID) || cpm == null || cpm.IsActive == false || cpm.PaymentMethodID != invoice.PaymentMethodID)
						{
							invoice.PMInstanceID = null;
						}
					}
				}
				else
				{
					invoice.CashAccountID = null;
					invoice.PMInstanceID = null;
				}

				isReverse = true;
				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARInvoice.salesPersonID>(this.Document.Cache, invoice);

				if (sp == null || sp.IsActive == false)
					invoice.SalesPersonID = null;

				ClearRetainageSummary(invoice);

				invoice = this.Document.Insert(invoice);

				FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(CurrentDocument.Cache, invoice, doc.TranPeriodID);

				isReverse = false;

				if (invoice.RefNbr == null)
				{
					//manual numbering, check for occasional duplicate
					ARInvoice duplicate = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, invoice.DocType, invoice.OrigRefNbr);

					if (duplicate != null)
					{
						PXCache<DuplicateFilter>.RestoreCopy(duplicatefilter.Current, filter);
						duplicatefilter.View.Answer = dialogRes;

						if (duplicatefilter.AskExt() == WebDialogResult.OK)
						{
							duplicatefilter.Cache.Clear();

							if (duplicatefilter.Current.RefNbr == null)
								throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(DuplicateFilter.refNbr).Name);

							duplicate = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, invoice.DocType, duplicatefilter.Current.RefNbr);

							if (duplicate != null)
								throw new PXException(ErrorMessages.RecordExists);

							invoice.RefNbr = duplicatefilter.Current.RefNbr;
						}
					}
					else
						invoice.RefNbr = invoice.OrigRefNbr;

					this.Document.Cache.Normalize();
					invoice = this.Document.Update(invoice);
				}

                ARInvoiceCreated(invoice, doc);

                if (info != null)
				{
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo,
														   Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.
														Select(this, null);

					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}
			}

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			this.FieldDefaulting.AddHandler<ARTran.salesPersonID>((sender, e) =>
			{
				e.NewValue = null;
				e.Cancel = true;
			});

			foreach (ARTran srcTran in PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>, And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				if (srcTran?.LineType == SOLineType.Discount && srcTran.SOOrderLineNbr != null)
					continue;

				ARTran tran = PXCache<ARTran>.CreateCopy(srcTran);
				tran.TranType = null;
				tran.RefNbr = null;
				string origDrCr = tran.DrCr;
				tran.DrCr = null;
				tran.Released = null;
				tran.CuryInfoID = null;
				tran.SOOrderNbr = null;
				tran.SOShipmentNbr = null;
				tran.OrigInvoiceDate = tran.TranDate;
				tran.NoteID = null;
				tran.ManualPrice = true;
				ReverseDRSchedule(doc, tran);

				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARTran.salesPersonID>(this.Transactions.Cache, tran);

				if (sp == null || sp.IsActive == false)
					tran.SalesPersonID = null;

				// Added to prevent ARTran_TaxCategoryID_FieldDefaulting.
				//
				isReverse = true;

				Decimal? curyTranAmt = tran.CuryTranAmt;
				ARTran insertedTran = this.Transactions.Insert(tran);
				PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, srcTran, Transactions.Cache, insertedTran);

				isReverse = false;

				// Added to prevent an incorrect recalculation by
				// DiscountEngine logic. The same way as in AR.
				//
				if (insertedTran != null && insertedTran.CuryTranAmt != curyTranAmt)
				{
					insertedTran.CuryTranAmt = curyTranAmt;
					insertedTran = (ARTran)Transactions.Cache.Update(insertedTran);
				}

				insertedTran.ManualDisc = true;

				if (insertedTran.LineType == SOLineType.Discount)
				{
					insertedTran.DrCr = origDrCr == DrCr.Debit ? DrCr.Credit : DrCr.Debit;
					insertedTran.FreezeManualDisc = true;
					insertedTran.TaxCategoryID = null;
					this.Transactions.Update(insertedTran);
				}
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

            var discountDetailsSet = PXSelect<ARInvoiceDiscountDetail, 
                Where<ARInvoiceDiscountDetail.docType, Equal<Required<ARInvoice.docType>>,
                    And<ARInvoiceDiscountDetail.refNbr, Equal<Required<ARInvoice.refNbr>>>>,
                OrderBy<Asc<ARInvoiceDiscountDetail.docType,
                    Asc<ARInvoiceDiscountDetail.refNbr>>>>
                .Select(this, doc.DocType, doc.RefNbr);

            foreach (ARInvoiceDiscountDetail discountDetail in discountDetailsSet)
			{
				ARInvoiceDiscountDetail newDiscountDetail = PXCache<ARInvoiceDiscountDetail>.CreateCopy(discountDetail);

				newDiscountDetail.DocType = Document.Current.DocType;
				newDiscountDetail.RefNbr = Document.Current.RefNbr;
				newDiscountDetail.IsManual = true;
				ARDiscountEngine.UpdateDiscountDetail(this.ARDiscountDetails.Cache, ARDiscountDetails, newDiscountDetail);
			}

			if (!IsExternalTax(Document.Current.TaxZoneID))
			{
				bool disableTaxCalculation = doc.PendingPPD == true && doc.DocType == ARDocType.CreditMemo;

				foreach (ARTaxTran tax in PXSelect<ARTaxTran, Where<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>,
																And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
				{
					ARTaxTran new_artax = new ARTaxTran { TaxID = tax.TaxID };

					if (disableTaxCalculation)
					{
						TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.NoCalc);
					}

					new_artax = this.Taxes.Insert(new_artax);

					if (new_artax != null)
					{
						new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
						new_artax.TaxRate = tax.TaxRate;
						new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_artax.CuryExemptedAmt = tax.CuryExemptedAmt;
						new_artax.CuryTaxAmt = tax.CuryTaxAmt;
						new_artax.CuryRetainedTaxableAmt = tax.CuryRetainedTaxableAmt;
						new_artax.CuryRetainedTaxAmt = tax.CuryRetainedTaxAmt;

						if (disableTaxCalculation)
						{
							TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualCalc);
						}

						new_artax = this.Taxes.Update(new_artax);
					}
				}
			}
		}

		public virtual void ReverseDRSchedule(ARRegister doc, ARTran tran)
		{
			if (string.IsNullOrEmpty(tran.DeferredCode))
			{
				return;
			}

			DRSchedule schedule = PXSelect<DRSchedule,
				Where<DRSchedule.module, Equal<moduleAR>,
				And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
				And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
										And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>.
									Select(this, doc.DocType, doc.RefNbr, tran.LineNbr);

			if (schedule != null)
			{
				tran.DefScheduleID = schedule.ScheduleID;
			}
		}

		/// <summary>
		/// Reverse current document and apply it to reversal document if needed.
		/// </summary>
		/// <param name="applyOriginalDocToReversal">True to apply document to reversal.</param>
		/// <returns/>
		private IEnumerable ReverseDocumentAndApplyToReversalIfNeeded(PXAdapter adapter, bool applyOriginalDocToReversal)
		{
			ARInvoice origDoc = Document.Current;
			string origDocType = origDoc?.DocType;

			bool docTypeProhibitsReversal = 
				origDocType != ARDocType.Invoice &&
				origDocType != ARDocType.DebitMemo &&
				origDocType != ARDocType.CreditMemo;

			if (origDoc == null || docTypeProhibitsReversal)
			{
				return adapter.Get();
			}

			if (origDoc.InstallmentNbr != null && !string.IsNullOrEmpty(origDoc.MasterRefNbr))
			{
				throw new PXSetPropertyException(Messages.Multiply_Installments_Cannot_be_Reversed, origDoc.MasterRefNbr);
			}

			if (origDoc.IsOriginalRetainageDocument() ||
				origDoc.IsChildRetainageDocument())
			{
				// Verify the case when unreleased retainage
				// document exists.
				// 
				ARRetainageInvoice retainageDoc = RetainageDocuments
					.Select()
					.RowCast<ARRetainageInvoice>()
					.FirstOrDefault(row => row.Released != true);

				if (retainageDoc != null)
				{
					throw new PXException(
						AP.Messages.ReverseRetainageNotReleasedDocument,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[retainageDoc.DocType]),
						retainageDoc.RefNbr,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[origDoc.DocType]));
				}

				// Verify the case when released retainage
				// document exists or payments applied.
				// 
				ARAdjust adj =
					PXSelect<ARAdjust,
					Where<ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>,
						And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARAdjust.voided, Equal<False>>>>>.
					SelectSingleBound(this, null);

				bool hasPaymentsApplied = adj != null;

				if (origDoc.IsOriginalRetainageDocument() &&
					origDoc.CuryRetainageTotal != origDoc.CuryRetainageUnreleasedAmt ||
					hasPaymentsApplied)
				{
					throw new PXException(
						AP.Messages.HasPaymentsOrDebAdjCannotBeReversed,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[origDoc.DocType]),
						origDoc.RefNbr);
				}

				// Verify the case when reversing retainage
				// document exists.
				// 
				ARRegister reversingDoc;
				if (CheckReversingRetainageDocumentAlreadyExists(origDoc, out reversingDoc))
				{
					throw new PXException(
						Messages.ReversingDocumentExists,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[origDoc.DocType]),
						origDoc.RefNbr,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[reversingDoc.DocType]),
						reversingDoc.RefNbr);
				}
			}
			else if (!AskUserApprovalIfReversingDocumentAlreadyExists(origDoc))
			{
				return adapter.Get();
			}

			Save.Press();
			ARInvoice origDocCopy = PXCache<ARInvoice>.CreateCopy(origDoc);
			FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<ARInvoice.finPeriodID, ARInvoice.branchID>(Document.Cache, origDocCopy, finperiod, typeof(OrganizationFinPeriod.aRClosed));

			try
			{
				ReverseInvoiceProc(origDocCopy);
				ARInvoice reversingDoc = Document.Current;

				if (applyOriginalDocToReversal)
				{
					ApplyOriginalDocumentToReversal(origDoc, reversingDoc);
				}

				Document.Cache.RaiseExceptionHandling<ARInvoice.finPeriodID>(reversingDoc, reversingDoc.FinPeriodID, null);
				return new List<ARInvoice> { reversingDoc };
			}
			catch (PXException)
			{
				Clear(PXClearOption.PreserveTimeStamp);
				Document.Current = origDocCopy;
				throw;
			}
		}

		/// <summary>
		/// Ask user for approval for creation of another reversal if reversing document already exists.
		/// </summary>
		/// <param name="origDoc">The original document.</param>
		/// <returns>
		/// True if user approves, false if not.
		/// </returns>
		private bool AskUserApprovalIfReversingDocumentAlreadyExists(ARInvoice origDoc)
		{
			string reversingDocType = GetReversingDocType(origDoc.DocType);
			ARRegister reversingDoc = PXSelect<ARRegister,
				Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.origDocType, Equal<Required<ARRegister.origDocType>>,
					And<ARRegister.origRefNbr, Equal<Required<ARRegister.origRefNbr>>>>>,
				OrderBy<Desc<ARRegister.createdDateTime>>>
				.SelectSingleBound(this, null, reversingDocType, origDoc.DocType, origDoc.RefNbr);

			if (reversingDoc != null)
			{
				string descr;
				ARDocType.ListAttribute list = new ARDocType.ListAttribute();
				list.ValueLabelDic.TryGetValue(reversingDocType, out descr);
				string localizedMsg = PXMessages.LocalizeFormatNoPrefix(Messages.ReversingDocumentExists, descr, reversingDoc.RefNbr);
				return Document.View.Ask(localizedMsg, MessageButtons.YesNo) == WebDialogResult.Yes;
			}

			return true;
		}

		private void ApplyOriginalDocumentToReversal(ARInvoice origDoc, ARInvoice reversingDoc)
		{
			if (!origDoc.HasBalance() || origDoc.Status == ARDocStatus.Closed || reversingDoc == null)
				return;

			switch (reversingDoc.DocType)
			{
				case ARDocType.DebitMemo:
					ApplyOriginalDocAdjustmentToDebitMemo(origDoc, reversingDoc);
					break;

				case ARDocType.CreditMemo:
					ARAdjust applicationToCreditMemo = new ARAdjust
					{
						AdjgDocType = reversingDoc.DocType,
                        AdjgRefNbr = reversingDoc.RefNbr,
						AdjdDocType = origDoc.DocType,
						AdjdRefNbr = origDoc.RefNbr,
						CuryAdjgAmt = origDoc.CuryDocBal,
					};

					Adjustments_1.Insert(applicationToCreditMemo);
					break;
			}
		}

		/// <summary>
		/// Applies the original document adjustment to reversing debit memo. By this moment usually there are already several applications to the debit memo,
		/// so select is used to find an application for a reversing document among them and set its balance.
		/// </summary>
		/// <param name="origDoc">The original document.</param>
		/// <param name="reversingDebitMemo">The reversing debit memo.</param>
		private void ApplyOriginalDocAdjustmentToDebitMemo(ARInvoice origDoc, ARInvoice reversingDebitMemo)
		{
			ARAdjust2 applicationToDebitMemo = PXSelect<ARAdjust2,
												  Where<ARAdjust2.adjdDocType, Equal<Current<ARInvoice.docType>>,
													And<ARAdjust2.adjgDocType, Equal<Required<ARInvoice.docType>>,
													And<ARAdjust2.adjgRefNbr, Equal<Required<ARInvoice.refNbr>>>>>>.
												Select(this, origDoc.DocType, origDoc.RefNbr);

			if (applicationToDebitMemo == null)
			{
				applicationToDebitMemo = new ARAdjust2
				{
					AdjdDocType = reversingDebitMemo.DocType,
					AdjgDocType = origDoc.DocType,
					AdjgRefNbr = origDoc.RefNbr,
					CuryAdjdAmt = origDoc.CuryDocBal
				};

				Adjustments.Insert(applicationToDebitMemo);
			}
			else
			{
				Adjustments.Cache.SetValueExt<ARAdjust2.curyAdjdAmt>(applicationToDebitMemo, origDoc.CuryDocBal);
			}
		}
		#endregion

        public delegate void ARInvoiceCreatedDelegate(ARInvoice invoice, ARRegister doc);
        protected virtual void ARInvoiceCreated(ARInvoice invoice, ARRegister doc)
	    {

	    }

        protected string salesSubMask;

		public virtual string SalesSubMask
		{
			get
			{
				if (salesSubMask == null)
				{
					salesSubMask = ARSetup.Current.SalesSubMask;
				}

				return salesSubMask;
			}
			set
			{
				salesSubMask = value;
			}
		}

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				PMProject project;
				if (ProjectDefaultAttribute.IsProject(this, Document.Current?.ProjectID, out project))
				{
					e.NewValue = project.BillingCuryID;
					e.Cancel = true;
				}
				else if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
				{
					e.NewValue = customer.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				PMProject project;
				if (ProjectDefaultAttribute.IsProject(this, Document.Current?.ProjectID, out project))
				{
					if (!string.IsNullOrEmpty(project.RateTypeID))
					{
						e.NewValue = project.RateTypeID;
						e.Cancel = true;
					}
					else
					{
						CMSetup setup = PXSelect<CMSetup>.Select(this);
						if (setup != null && !string.IsNullOrEmpty(setup.PMRateTypeDflt))
						{
							e.NewValue = setup.PMRateTypeDflt;
							e.Cancel = true;
						}
					}
				}
				else if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryRateTypeID))
				{
					e.NewValue = customer.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((ARInvoice)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (customer.Current != null && !(bool)customer.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}

		protected virtual void _(Events.RowUpdated<CurrencyInfo> e)
		{
			if (e.Row == null) return;

			if (e.Row.CuryRate != null)
			{
			if (Document.Current?.Released != true)
			{
				if (Document.Current?.DocType != ARDocType.CreditMemo)
				{
						Adjustments.Select().ForEach(res =>
						{
							ARAdjust2 adjustment = (ARAdjust2)res;
							CalcBalancesFromInvoiceSide(adjustment, true, false);
							Adjustments.Update(adjustment);

							if (adjustment.CuryWhTaxBal < 0m)
							{
								Adjustments.Cache.RaiseExceptionHandling<ARAdjust2.curyAdjdWOAmt>(adjustment, adjustment.CuryAdjdWOAmt,
									new PXSetPropertyException(Messages.DocumentBalanceNegative));
							}
						});
				}
				else
				{
					Adjustments_1.Select().ForEach(adjustment => CalcBalances(adjustment, true, false));
				}
			}
		}
		}
		#endregion

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<ARInvoice.docDate, ARInvoice.finPeriodID, ARInvoice.curyID>(e.Row, e.OldRow))
			{
				foreach (ARSalesPerTran tran in salesPerTrans.Select())
				{
					this.salesPerTrans.Cache.MarkUpdated(tran);
				}

				foreach (ARFinChargeTran tran in this.finChargeTrans.Select())
				{
					this.finChargeTrans.Cache.MarkUpdated(tran);
				}
				}

			if (!sender.ObjectsEqual<ARInvoice.docDate, ARInvoice.finPeriodID, ARInvoice.curyID, ARInvoice.aRAccountID, ARInvoice.aRSubID, ARInvoice.branchID>(e.Row, e.OldRow))
				{
				foreach (ARAdjust2 tran in Adjustments.Select())
				{
					Adjustments.Cache.MarkUpdated(tran);
				}

				foreach (ARAdjust tran in Adjustments_1.Select())
				{
					Adjustments_1.Cache.MarkUpdated(tran);
				}
			}

			if (!sender.ObjectsEqual<ARInvoice.branchID>(e.Row, e.OldRow))
			{
				foreach (ARSalesPerTran tran in salesPerTrans.Select())
				{
					this.salesPerTrans.Cache.MarkUpdated(tran);
				}
			}
		}

		public bool IsProcessingMode { get; set; }

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public ARInvoiceEntry()
		{
			ARSetup setup = ARSetup.Current;
			
			RowUpdated.AddHandler<ARInvoice>(ParentFieldUpdated);

			RetainageDocuments.Cache.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.retainage>();
			RetainageDocuments.Cache.AllowDelete = false;
			RetainageDocuments.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(RetainageDocuments.Cache, null, false);

			TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });

			if (setup != null && setup.DunningLetterProcessType == DunningProcessType.ProcessByDocument)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.revoked>(Document.Cache, null, true);
			}

            this.Caches.SubscribeCacheCreated(Adjustments.GetItemType(), delegate
            {
			PXUIFieldAttribute.SetVisible<ARAdjust.customerID>(Adjustments.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>());

            });
			PXUIFieldAttribute.SetVisible<ARAdjust.adjdCustomerID>(Adjustments_1.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>());
			PXUIFieldAttribute.SetEnabled<ARAdjust2.curyAdjdPPDAmt>(Adjustments.Cache, null, false);

			OpenPeriodAttribute.SetValidatePeriod<ARRegister.finPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
			TaxesByTaxCategory = new Dictionary<string, HashSet<string>>();

			var arAddressCache = Caches[typeof(ARAddress)];
			var arContactCache = Caches[typeof(ARContact)];
			var arShippingAddressCache = Caches[typeof(ARShippingAddress)];
			var arShippingContactCache = Caches[typeof(ARShippingContact)];
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<ARInvoice>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(ARTran), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<ARTran.tranType>(PXDbType.Char, 3, ((ARInvoiceEntry)graph).Document.Current?.DocType),
							new PXDataFieldValue<ARTran.refNbr>(((ARInvoiceEntry)graph).Document.Current?.RefNbr),
							new PXDataFieldValue<ARTran.lineType>(PXDbType.Char, 2, SOLineType.Freight, PXComp.NEorISNULL),
							new PXDataFieldValue<ARTran.lineType>(PXDbType.Char, 2, SOLineType.Discount, PXComp.NEorISNULL)
						};
					}));
			}
		}

		public override void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers)
		{
			// We need to process fields together that are related to the period for proper validation. For this:
			// 1) set the right order
			// 2) insert after the Customer Location field
			// 3) all fields must belong to the same view.

			int customerLocationIDIndex = script.FindIndex(cmd => cmd.FieldName == nameof(ARInvoice.CustomerLocationID));

			if (customerLocationIDIndex < 0)
				return;

			Api.Models.Command cmdBranch = script.Where(cmd => cmd.FieldName == nameof(ARInvoice.BranchID) && cmd.ObjectName == "CurrentDocument").SingleOrDefault();
			Api.Models.Command cmdDocDate = script.Where(cmd => cmd.FieldName == nameof(ARInvoice.DocDate)).SingleOrDefault();
			Api.Models.Command cmdFinPeriod = script.Where(cmd => cmd.FieldName == nameof(ARInvoice.FinPeriodID)).SingleOrDefault();

			if (cmdBranch == null || cmdDocDate == null || cmdFinPeriod == null)
				return;

			// Period and date fields are in the head of the document.
			// The branch field is located on a tab of the document on another view. Set the same view for processing together.
			cmdBranch.ObjectName = "Document";

			cmdBranch.Commit = false;
			cmdDocDate.Commit = false;
			cmdFinPeriod.Commit = true;

			var commands = 
				new Api.Models.Command[] 
				{
					cmdBranch,
					cmdDocDate,
					cmdFinPeriod
				};

			foreach (Api.Models.Command command in commands)
			{
				script.Remove(command);
			}
			customerLocationIDIndex = script.FindIndex(cmd => cmd.FieldName == nameof(ARInvoice.CustomerLocationID));
			script.InsertRange(customerLocationIDIndex + 1, commands);
		}

		public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);
            map.Add(typeof(CT.Contract), typeof(CT.Contract));
        }

		public override void Persist()
		{
			if (Document.Current != null && !Discount_Row.Any() && Document.Current.CuryDiscTot != 0m)
			{
				AddDiscount(Document.Cache, Document.Current);
			}

			foreach(ARAdjust2 adj in Adjustments.Cache.Inserted
				.Cast<ARAdjust2>()
				.Where(adj => adj.CuryAdjdAmt == 0m))
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.InsertedDeleted);
				}

			foreach(ARAdjust2 adj in Adjustments.Cache.Updated
				.Cast<ARAdjust2>()
				.Where(adj => adj.CuryAdjdAmt == 0m))
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.Deleted);
				}

			foreach (ARInvoice ardoc in Document.Cache.Cached)
			{
				PXEntryStatus status = Document.Cache.GetStatus(ardoc);

				if (status == PXEntryStatus.Deleted && ardoc.PendingPPD == true && ardoc.DocType == ARDocType.CreditMemo)
				{
					PXUpdate<Set<ARAdjust.pPDCrMemoRefNbr, Null>, ARAdjust,
						Where<ARAdjust. pendingPPD, Equal<True>,
							And<ARAdjust.pPDCrMemoRefNbr, Equal<Required<ARAdjust.pPDCrMemoRefNbr>>>>>
						.Update(this, ardoc.RefNbr);
				}

				if ((status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated) && ardoc.DocType == ARDocType.Invoice && ardoc.Released == false && ardoc.ApplyPaymentWhenTaxAvailable != true)
				{
					ARAdjust2 prev_adj = null;

					decimal? CuryApplAmt = 0m;
					decimal? BaseApplAmt = 0m;

					foreach (ARAdjust2 adj in Adjustments_Inv.View
						.SelectMultiBound(new object[] { ardoc })
						.RowCast<ARAdjust2>()
						.WhereNotNull())
					{
						prev_adj = adj;

						// RGOLAmt shouldn't be included into base balance calculation
						// because base amounts have been calculated using Invoice currency rate.
						// 
						BaseApplAmt += adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt;
						CuryApplAmt += BalanceCalculation.GetFullBalanceDelta(adj).CurrencyAdjustedBalanceDelta;

						if (ardoc.CuryDocBal - CuryApplAmt < 0m && CuryApplAmt > 0m)
						{
							Adjustments.Cache.MarkUpdated(adj);
							Adjustments.Cache.RaiseExceptionHandling<ARAdjust2.curyAdjdAmt>(adj, adj.CuryAdjdAmt, 
								new PXSetPropertyException(Messages.Application_Amount_Cannot_Exceed_Document_Amount));
							throw new PXException(Messages.Application_Amount_Cannot_Exceed_Document_Amount);
						}
					}

					if (prev_adj != null)
					{
						decimal? curyDocBal = ardoc.CuryDocBal - CuryApplAmt;
						decimal? baseDocBal = ardoc.DocBal - BaseApplAmt;

						bool isOpenInvoiceWithNegativeBalance = curyDocBal > 0m && baseDocBal < 0m;
						bool isClosedInvoiceWithPositiveBalance = curyDocBal == 0m && baseDocBal != 0m;

						if (isClosedInvoiceWithPositiveBalance || isOpenInvoiceWithNegativeBalance)
						{
							prev_adj.AdjAmt += baseDocBal;
							prev_adj.RGOLAmt -= (prev_adj.ReverseGainLoss != true ? baseDocBal : -baseDocBal);
							Adjustments_Inv.Cache.MarkUpdated(prev_adj);
						}
				}
				}

				if ((status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated) && ardoc.DocType == ARDocType.CreditMemo && ardoc.Released == false)
				{
					decimal? CuryApplAmt = 0m;

					foreach (ARAdjust adj in Adjustments_Crm.View
						.SelectMultiBound(new object[] { ardoc })
						.RowCast<ARAdjust>()
						.WhereNotNull())
					{
						CuryApplAmt += BalanceCalculation.GetFullBalanceDelta(adj).CurrencyAdjustingBalanceDelta;

						if (ardoc.CuryDocBal - CuryApplAmt < 0m && CuryApplAmt > 0m)
						{
							Adjustments_1.Cache.MarkUpdated(adj);
							Adjustments_1.Cache.RaiseExceptionHandling<ARAdjust.curyAdjgAmt>(adj, adj.CuryAdjgAmt, new PXSetPropertyException(Messages.Application_Amount_Cannot_Exceed_Document_Amount));
							throw new PXException(Messages.Application_Amount_Cannot_Exceed_Document_Amount);
						}
					}
				}
			}

			ValidateARDiscountDetails();

			base.Persist();
		}

		public virtual void ValidateARDiscountDetails()
			{
			List<ARInvoiceDiscountDetail> arDiscounts = new List<ARInvoiceDiscountDetail>();

			foreach (ARInvoiceDiscountDetail discount in ARDiscountDetails.Select().ToList())
				{
				arDiscounts.Add(discount);
			}

			var duplicates = arDiscounts.GroupBy(x => new { x.DiscountID, x.DiscountSequenceID, x.Type, x.OrderType, x.OrderNbr }).Where(gr => gr.Count() > 1 && gr.Key.Type != DiscountType.ExternalDocument ).Select(gr => gr.Key);
			if (duplicates.Count() > 0)
				{
				ARDiscountEngine.ValidateDiscountDetails(ARDiscountDetails);
			}
		}

		public PXAction<ARInvoice> notification;
		[PXUIField(DisplayName = "Notifications", Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Notification(PXAdapter adapter,
		[PXString]
		string notificationCD)
		{
			foreach (ARInvoice doc in adapter.Get())
			{
				Document.Current = doc;

				Dictionary<string, string> parameters = new Dictionary<string, string>
				{
					["DocType"] = doc.DocType,
					["RefNbr"] = doc.RefNbr
				};

				using (var ts = new PXTransactionScope())
				{
					if (ProjectDefaultAttribute.IsProject(this, doc.ProjectID) && Activity.IsProjectSourceActive(doc.ProjectID, notificationCD))
					{
						Activity.SendNotification(PMNotificationSource.Project, notificationCD, doc.BranchID, parameters);
					}
					else
					{
						Activity.SendNotification(ARNotificationSource.Customer, notificationCD, doc.BranchID, parameters);
					}
					this.Save.Press();

					ts.Complete();
				}

				yield return doc;
			}
		}
		public override string GetCustomerReportID(string reportID, ARInvoice doc)
		{
			Document.Current = doc;

			if (ProjectDefaultAttribute.IsProject(this, doc.ProjectID) && Activity.ProjectInvoiceReportActive(doc.ProjectID) != null && reportID == ARReports.InvoiceMemoReportID)
			{
				PMProject rec = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.SelectWindowed(this, 0, 1, doc.ProjectID);
				return GetProjectSpecificCustomerReportID(Activity.ProjectInvoiceReportActive(doc.ProjectID), doc, rec);
			}
			else
			{
				return new NotificationUtility(this).SearchReport(ARNotificationSource.Customer, customer.SelectSingle(), reportID, doc.BranchID);
			}
		}

		public virtual string GetProjectSpecificCustomerReportID(string reportID, ARInvoice doc, PMProject project)
		{
			return new NotificationUtility(this).SearchReport(PMNotificationSource.Project, project, reportID, doc.BranchID);
		}

		public virtual IEnumerable adjustments()
		{
			Adjustments.Cache.ClearQueryCache();

			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this);

			foreach (PXResult<ARAdjust2, ARPayment, Standalone.ARRegisterAlias, CurrencyInfo> res in Adjustments_Inv.Select())
			{
				ARPayment payment = res;
				ARAdjust2 adj = res;

				PXCache<ARRegister>.RestoreCopy(payment, (Standalone.ARRegisterAlias)res);

				if (adj == null) continue;

				if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
				{
					CalcBalancesFromInvoiceSide(adj, payment, true, true);
				}

                yield return new PXResult<ARAdjust2, ARPayment>(adj, payment);
			}

			if (Document.Current != null
				&& (Document.Current.DocType == ARDocType.Invoice
					|| Document.Current.DocType == ARDocType.DebitMemo)
				&& Document.Current.Released != true
				&& Document.Current.Scheduled != true
				&& Document.Current.Voided != true)
			{
				using (new ReadOnlyScope(Adjustments.Cache, Document.Cache, arbalances.Cache))
                {
					foreach (PXResult<Standalone.ARRegisterAlias, CurrencyInfo, ARAdjust2, SOAdjust, ARPayment> res in PXSelectReadonly2<
						Standalone.ARRegisterAlias,
							InnerJoin<CurrencyInfo,
								On<CurrencyInfo.curyInfoID, Equal<Standalone.ARRegisterAlias.curyInfoID>>,
							LeftJoin<ARAdjust2,
								On<ARAdjust2.adjgDocType, Equal<Standalone.ARRegisterAlias.docType>,
								And<ARAdjust2.adjgRefNbr, Equal<Standalone.ARRegisterAlias.refNbr>,
								And<ARAdjust2.released, Equal<False>,
								And<ARAdjust2.voided, Equal<False>,
								And<Where<
									ARAdjust2.adjdDocType, NotEqual<Current<ARInvoice.docType>>,
									Or<ARAdjust2.adjdRefNbr, NotEqual<Current<ARInvoice.refNbr>>>>>>>>>,
							LeftJoin<SOAdjust,
								On<SOAdjust.adjgDocType, Equal<Standalone.ARRegisterAlias.docType>,
								And<SOAdjust.adjgRefNbr, Equal<Standalone.ARRegisterAlias.refNbr>,
								And<SOAdjust.adjAmt, Greater<decimal0>>>>,
							InnerJoinSingleTable<ARPayment,
								On<ARPayment.docType, Equal<Standalone.ARRegisterAlias.docType>,
								And<ARPayment.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>>>>>,
						Where2<
							Where<
								Standalone.ARRegisterAlias.customerID, Equal<Current<ARInvoice.customerID>>,
								Or<Standalone.ARRegisterAlias.customerID, Equal<Current<Customer.consolidatingBAccountID>>>>,
							And2<Where<
								Standalone.ARRegisterAlias.docType, Equal<ARDocType.payment>,
								Or<Standalone.ARRegisterAlias.docType, Equal<ARDocType.prepayment>,
								Or<Standalone.ARRegisterAlias.docType, Equal<ARDocType.creditMemo>>>>,
							And<Standalone.ARRegisterAlias.docDate, LessEqual<Current<ARInvoice.docDate>>,
							And<Standalone.ARRegisterAlias.tranPeriodID, LessEqual<Current<ARInvoice.tranPeriodID>>,
							And<Standalone.ARRegisterAlias.released, Equal<boolTrue>,
							And<Standalone.ARRegisterAlias.openDoc, Equal<boolTrue>,
                            And<Standalone.ARRegisterAlias.hold, Equal<False>,
							And<ARAdjust2.adjdRefNbr, IsNull,
							And<SOAdjust.adjgRefNbr, IsNull,
							And<Not<HasUnreleasedVoidPayment<ARPayment.docType, ARPayment.refNbr>>>>>>>>>>>>>
						.Select(this))
                    {
                        ARPayment payment = res;
                        CurrencyInfo pay_info = res;

						PXCache<ARRegister>.RestoreCopy(payment, (Standalone.ARRegisterAlias)res);

						ARAdjust2 adj = new ARAdjust2
						{
							AdjdDocType = Document.Current.DocType,
							AdjdRefNbr = Document.Current.RefNbr,
							AdjgDocType = payment.DocType,
							AdjgRefNbr = payment.RefNbr,
							AdjNbr = payment.AdjCntr,
							CustomerID = payment.CustomerID,
							AdjdCustomerID = Document.Current.CustomerID,
							AdjdBranchID = Document.Current.BranchID,
							AdjgBranchID = payment.BranchID,
							AdjgCuryInfoID = payment.CuryInfoID,
							AdjdOrigCuryInfoID = Document.Current.CuryInfoID,
							//if LE constraint is removed from payment selection this must be reconsidered
							AdjdCuryInfoID = Document.Current.CuryInfoID
						};

						if (Adjustments.Cache.Locate(adj) == null
							|| Adjustments.Cache.GetStatus(adj) == PXEntryStatus.InsertedDeleted)
						{
							adj = Adjustments.Insert(adj);

                            try
                            {
								InitBalancesFromInvoiceSide(CurrencyInfo_CuryInfoID, adj, Document.Current, payment);
                            }
                            catch (Exception ex)
                            {
								Adjustments.Cache.RaiseExceptionHandling<ARAdjust2.curyDocBal>(adj, 0m, ex);
                            }

							if (adj != null)
							{
								yield return new PXResult<ARAdjust2, ARPayment>(adj, payment);
						}
					}
				}
			}
		}
        }

		public virtual IEnumerable adjustments_1()
		{
			foreach (PXResult<ARAdjust, ARInvoice, Standalone.ARRegisterAlias, CurrencyInfo> res in Adjustments_Crm.Select())
			{
				ARAdjust adj = res;
				ARInvoice invoice = res;

				PXCache<ARRegister>.RestoreCopy(invoice, (Standalone.ARRegisterAlias)res);

				if (adj != null)
				{
					if (adj.Released == false)
				{
					if (Adjustments_1.Cache.GetStatus((ARAdjust)res) == PXEntryStatus.Notchanged)
					{
							CalcBalances<ARInvoice>(adj, invoice, true, true);
					}
					}
					adj.AdjType = ARAdjust.adjType.Adjusted;
					this.Caches<ARAdjust>().RaiseFieldUpdated<ARAdjust.adjType>(adj, null);
				}

				yield return new PXResult<ARAdjust, ARInvoice>(adj, invoice);
			}

			Adjustments_1.View.RequestRefresh();
		}

		public virtual IEnumerable adjustments_2()
		{
			foreach (object res in adjustments_1())
			{
				yield return res;
			}

			if (Document.Current != null && Document.Current.Released == true)
			{
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this);

				foreach (PXResult<ARAdjust, Standalone.ARRegisterAlias, ARPayment, CurrencyInfo> res in PXSelectJoin<
					ARAdjust,
						InnerJoin<Standalone.ARRegisterAlias,
							On<Standalone.ARRegisterAlias.docType, Equal<ARAdjust.adjgDocType>,
							And<Standalone.ARRegisterAlias.refNbr, Equal<ARAdjust.adjgRefNbr>>>,
						InnerJoinSingleTable<ARPayment,
							On<ARPayment.docType, Equal<ARAdjust.adjgDocType>,
						And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>,
						InnerJoin<CurrencyInfo,
							On<CurrencyInfo.curyInfoID, Equal<Standalone.ARRegisterAlias.curyInfoID>>>>>,
					Where<
						ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>,
						And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARAdjust.isInitialApplication, NotEqual<True>>>>>
					.Select(this))
				{
					ARAdjust adj = res;
					ARPayment payment = res;
					CurrencyInfo pay_info = res;

					PXCache<ARRegister>.RestoreCopy(payment, (Standalone.ARRegisterAlias)res);

					adj.AdjType = ARAdjust.adjType.Adjusting;
					this.Caches<ARAdjust>().RaiseFieldUpdated<ARAdjust.adjType>(adj, null);

					BalanceCalculation.CalculateApplicationDocumentBalance(
						Adjustments_2.Cache,
						payment,
						adj,
						pay_info,
						inv_info);

                    yield return new PXResult<ARAdjust, ARPayment>(adj, payment);
				}
			}

			Adjustments_2.View.RequestRefresh();
		}

		private class PXLoadInvoiceException : Exception
		{
			public PXLoadInvoiceException() { }

			public PXLoadInvoiceException(SerializationInfo info, StreamingContext context)
				: base(info, context) { }
		}

		public virtual void LoadInvoicesProc()
		{
			try
			{
				if (Document.Current?.CustomerID == null || Document.Current.OpenDoc == false || Document.Current.DocType != ARDocType.Invoice)
				{
					throw new PXLoadInvoiceException();
				}

				Document.Cache.MarkUpdated(Document.Current);
				Document.Cache.IsDirty = true;

				decimal? CuryUnappliedBal = Document.Current.CuryDocBal;

				foreach(ARAdjust2 copy in Adjustments.Select().RowCast<ARAdjust2>().Select(PXCache<ARAdjust2>.CreateCopy))
				{
					if (CuryUnappliedBal > copy.CuryDocBal)
					{
						copy.CuryAdjdAmt = copy.CuryDocBal;
						CuryUnappliedBal -= copy.CuryAdjdAmt;
					}
					else
					{
						copy.CuryAdjdAmt = CuryUnappliedBal;
						CuryUnappliedBal = 0m;
					}

					Adjustments.Cache.Update(copy);

					if (CuryUnappliedBal == 0m)
					{
						throw new PXLoadInvoiceException();
					}
				}
			}
			catch (PXLoadInvoiceException)
			{
			}
		}

		protected virtual void ARTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARTran documentLine = e.Row as ARTran;

			if (documentLine == null) return;
			
			if (documentLine.DeferredCode != null)
			{
				var code = (DRDeferredCode)PXSelectorAttribute.Select<ARTran.deferredCode>(sender, documentLine);
				if (code != null & code.MultiDeliverableArrangement == true)
				{
					var item = InventoryItemGetByID(documentLine.InventoryID);
					var itemCode = item == null ? null : DeferredCodeGetByID(item.DeferredCode);

					if (itemCode == null || itemCode.MultiDeliverableArrangement != true)
					{
						if (sender.RaiseExceptionHandling<ARTran.deferredCode>(documentLine, code.DeferredCodeID,
							new PXSetPropertyException<ARTran.deferredCode>(DR.Messages.MDANotAllowedForItem)))
						{
							throw new PXRowPersistingException(typeof(ARTran.deferredCode).Name, code.DeferredCodeID, DR.Messages.MDANotAllowedForItem);
						}
					}
				}
			}

			ScheduleHelper.DeleteAssociatedScheduleIfDeferralCodeChanged(sender, documentLine);
		}

		#region ARInvoice Events
		protected virtual void ARInvoice_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARInvoice row = e.Row as ARInvoice;
			if (row != null)
			{
				Location customerLocation = location.SelectSingle(row.CustomerLocationID);
				if (customerLocation != null)
				{
					if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
					{
						e.NewValue = customerLocation.CTaxZoneID;
					}
					else
					{
						BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
						if (companyAccount != null)
						{
							Location companyLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
							if (companyLocation != null)
								e.NewValue = companyLocation.VTaxZoneID;
						}
					}
				}
			}
		}

		protected virtual void ARInvoice_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARInvoice.taxZoneID>(e.Row);
			}

		private bool IsTaxZoneDerivedFromCustomer()
		{
			Location customerLocation = location.Select();
			if (customerLocation != null)
			{
				if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
				{
					return true;
				}
			}

			return false;
		}

		protected virtual void ARAddress_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARAddress row = e.Row as ARAddress;
			ARAddress oldRow = e.OldRow as ARAddress;
			if (row != null)
			{
				if (!IsTaxZoneDerivedFromCustomer() && !string.IsNullOrEmpty(row.PostalCode) && oldRow.PostalCode != row.PostalCode)
				{
					string taxZone = TaxBuilderEngine.GetTaxZoneByZip(this, row.PostalCode);
					Document.Cache.SetValueExt<ARInvoice.taxZoneID>(Document.Current, taxZone);
				}
			}
		}

		protected virtual void ARInvoice_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ARDocType.Invoice;
		}

		public object GetAcctSub<Field>(PXCache cache, object data)
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

		protected virtual void ARInvoice_ARAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<CR.Location.aRAccountID>(location.Cache, location.Current);
			}
		}
		
		protected virtual void ARInvoice_ARSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<CR.Location.aRSubID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARInvoice_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<ARInvoice.aRAccountID>(e.Row);
			sender.SetDefaultExt<ARInvoice.aRSubID>(e.Row);
			sender.SetDefaultExt<ARInvoice.branchID>(e.Row);
			sender.SetDefaultExt<ARInvoice.taxZoneID>(e.Row);
			sender.SetDefaultExt<ARInvoice.avalaraCustomerUsageType>(e.Row);
			sender.SetDefaultExt<ARInvoice.salesPersonID>(e.Row);
			sender.SetDefaultExt<ARInvoice.workgroupID>(e.Row);
			sender.SetDefaultExt<ARInvoice.ownerID>(e.Row);

			object projectID = ((ARInvoice)e.Row).ProjectID;
			if (ProjectDefaultAttribute.IsProject(this, ((ARInvoice)e.Row).ProjectID))
			{
				try
				{
					sender.RaiseFieldVerifying<ARInvoice.projectID>(e.Row, ref projectID);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<ARInvoice.projectID>(e.Row, projectID, ex);
				}
			}

			ARShippingAddressAttribute.DefaultRecord<ARInvoice.shipAddressID>(sender, e.Row);
			ARShippingContactAttribute.DefaultRecord<ARInvoice.shipContactID>(sender, e.Row);
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void ARInvoice_CustomerID_CacheAttached(PXCache sender)
        {
        }

		protected virtual void ARInvoice_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice invoice = (ARInvoice)e.Row;
			customer.RaiseFieldUpdated(sender, e.Row);
			if (customer.Current != null)
			{
				invoice.ApplyOverdueCharge = customer.Current.FinChargeApply;

				if (!e.ExternalCall)
				{
					customer.Current.CreditRule = null;
				}
			}

			Adjustments_Inv.Cache.Clear();
			Adjustments_Inv.Cache.ClearQueryCache();

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || sender.GetValuePending<ARInvoice.curyID>(e.Row) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<ARInvoice.curyInfoID>(sender, e.Row);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<ARInvoice.docDate>(e.Row, ((ARInvoice)e.Row).DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						((ARInvoice)e.Row).CuryID = info.CuryID;
					}
				}
			}
			
			sender.SetDefaultExt<ARInvoice.customerLocationID>(e.Row);
			sender.SetDefaultExt<ARInvoice.dontPrint>(e.Row);
			sender.SetDefaultExt<ARInvoice.dontEmail>(e.Row);
			
			try
			{
				ARAddressAttribute.DefaultRecord<ARInvoice.billAddressID>(sender, e.Row);
				ARContactAttribute.DefaultRecord<ARInvoice.billContactID>(sender, e.Row);
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = customer.Current.AcctCD;
				throw;
			}

			sender.SetDefaultExt<ARInvoice.taxZoneID>(e.Row);
			sender.SetDefaultExt<ARInvoice.paymentMethodID>(e.Row);

			// Delete all applications AC-97392
			PXSelect<ARAdjust2,
					Where<ARAdjust2.adjdDocType, Equal<Required<ARInvoice.docType>>,
						And<ARAdjust2.adjdRefNbr, Equal<Required<ARInvoice.refNbr>>>>>
				.Select(this, invoice.DocType, invoice.RefNbr)
				.RowCast<ARAdjust2>()
				.ForEach(application => Adjustments.Cache.Delete(application));

			object projectID = ((ARInvoice)e.Row).ProjectID;
			if (ProjectDefaultAttribute.IsProject(this, ((ARInvoice)e.Row).ProjectID))
			{
				try
				{
					sender.RaiseFieldVerifying<ARInvoice.projectID>(e.Row, ref projectID);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<ARInvoice.projectID>(e.Row, projectID, ex);
				}
			}
		}

		protected virtual void ARInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{

		}

		protected virtual void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARInvoice doc = (ARInvoice)e.Row;

			bool isDiscountableDoc = doc.DocType != ARDocType.CreditMemo && doc.DocType != ARDocType.SmallCreditWO;

			if (isDiscountableDoc && string.IsNullOrEmpty(doc.TermsID))
			{
				if (sender.RaiseExceptionHandling<ARInvoice.termsID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARInvoice.termsID).Name)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.termsID).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARInvoice.termsID).Name);
				}
			}

			if (isDiscountableDoc && doc.DueDate == null)
			{
				if (sender.RaiseExceptionHandling<ARInvoice.dueDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARInvoice.dueDate).Name)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.dueDate).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARInvoice.dueDate).Name);
				}
			}

			if (isDiscountableDoc && doc.DiscDate == null)
			{
				if (sender.RaiseExceptionHandling<ARInvoice.discDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARInvoice.discDate).Name)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.discDate).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARInvoice.discDate).Name);
				}
			}
			
			if (doc.DocType == ARDocType.FinCharge)
			{
				AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(sender, doc.DocType, ARSetup.Current.FinChargeNumberingID);
			}

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) && (doc.DocType == ARDocType.FinCharge))
			{
				if (this.Accessinfo.ScreenID == "AR.30.10.00")
				{
					throw new PXException(PX.Objects.AR.Messages.FinChargeCanNotBeDeleted);
				}
			}

			if (doc.CuryDiscTot > Math.Abs(doc.CuryLineTotal ?? 0m))
			{
				if (sender.RaiseExceptionHandling<ARInvoice.curyDiscTot>(e.Row, doc.CuryDiscTot, new PXSetPropertyException(Messages.DiscountGreaterLineTotal, PXErrorLevel.Error)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.curyDiscTot).Name, null, Messages.DiscountGreaterLineTotal);
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				GLTran gltran = PXSelect<GLTran,
								Where<GLTran.refNbr, Equal<Required<GLTran.refNbr>>,
									And<GLTran.tranType, Equal<Required<GLTran.tranType>>,
									And<GLTran.released, Equal<True>,
									And<GLTran.module, Equal<Required<GLTran.module>>>>>>>.SelectSingleBound(this, null, doc.RefNbr, doc.DocType, BatchModule.AR);
				if (gltran != null)
				{
					throw new PXException(Messages.GLTransExist);
				}

				ARTran artran = PXSelect<ARTran,
								Where<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
									And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
									And<ARTran.released, Equal<True>>>>>.SelectSingleBound(this, null, doc.RefNbr, doc.DocType);
				if (artran != null)
				{
					throw new PXException(Messages.ARTransExist);
				}
			}
		}

		protected virtual void ARInvoice_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<ARInvoice.docDate>(sender, e);
		}
		
		protected virtual void ARInvoice_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Terms terms = (Terms)PXSelectorAttribute.Select<ARInvoice.termsID>(sender, e.Row);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				foreach (ARAdjust2 adj in Adjustments.Select())
				{
					Adjustments.Cache.Delete(adj);
				}
			}
		}

		protected virtual void ARInvoice_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARInvoice.pMInstanceID>(e.Row);
			sender.SetDefaultExt<ARInvoice.cashAccountID>(e.Row);
		}

		protected virtual void ARInvoice_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARInvoice.cashAccountID>(e.Row);
		}

		protected virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARInvoice doc = e.Row as ARInvoice;
			if (doc == null) return;

			// We need this for correct tabs repainting
			// in migration mode.
			// 
			Adjustments.Cache.AllowSelect =
			Adjustments_1.Cache.AllowSelect =
			Adjustments_2.Cache.AllowSelect = true;

			PXUIFieldAttribute.SetVisible<ARInvoice.curyID>(cache, doc, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			bool retainageApply = doc.RetainageApply == true;
			bool isRetainageDocument = doc.IsRetainageDocument == true;
			bool isDocumentReleased = doc.Released == true;
			bool isDocumentInvoice = doc.DocType == ARDocType.Invoice;
			bool isDocumentCreditMemo = doc.DocType == ARDocType.CreditMemo;
			bool isDocumentDebitMemo = doc.DocType == ARDocType.DebitMemo;
			bool isRetainageCreditMemo = doc.DocType == ARDocType.CreditMemo && (doc.IsOriginalRetainageDocument() || doc.IsChildRetainageDocument());
			bool retainTaxes = ARSetup.Current.RetainTaxes ?? false;

			PXUIFieldAttribute.SetRequired<ARInvoice.termsID>(cache, (doc.DocType != ARDocType.CreditMemo));
			PXUIFieldAttribute.SetRequired<ARInvoice.dueDate>(cache, (doc.DocType != ARDocType.CreditMemo));
			PXUIFieldAttribute.SetRequired<ARInvoice.discDate>(cache, (doc.DocType != ARDocType.CreditMemo));
			PXUIFieldAttribute.SetVisible<ARTran.origInvoiceDate>(Transactions.Cache, null, doc.DocType == ARInvoiceType.CreditMemo);
			bool curyenabled = !(customer.Current != null && customer.Current.AllowOverrideCury != true);

			autoApply.SetEnabled(doc.DocType == ARDocType.Invoice && doc.Released == false);
			
			bool shouldDisable = doc.Released == true
								|| doc.Voided == true
								|| doc.DocType == ARDocType.SmallCreditWO
								|| doc.PendingPPD == true
								|| doc.DocType == ARDocType.FinCharge && !IsProcessingMode && cache.GetStatus(doc) == PXEntryStatus.Inserted;

			if (shouldDisable)
			{
				bool isUnreleasedWO = doc.Released != true && doc.DocType == ARDocType.SmallCreditWO;
				bool isUnreleasedPPD = doc.Released != true && doc.PendingPPD == true;

				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.dueDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo && doc.DocType != ARDocType.SmallCreditWO && doc.DocType != ARDocType.FinCharge) && doc.OpenDoc == true && doc.PendingPPD != true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.discDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo && doc.DocType != ARDocType.SmallCreditWO && doc.DocType != ARDocType.FinCharge) && doc.OpenDoc == true && doc.PendingPPD != true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.emailed>(cache, doc, true);
				cache.AllowDelete = isUnreleasedWO || isUnreleasedPPD;
				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

				ARDiscountDetails.Cache.AllowDelete = false;
				ARDiscountDetails.Cache.AllowUpdate = false;
				ARDiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowDelete = false;
				Taxes.Cache.AllowUpdate = false;
				Taxes.Cache.AllowInsert = false;

				release.SetEnabled(isUnreleasedWO || isUnreleasedPPD);
				createSchedule.SetEnabled(false);
				payInvoice.SetEnabled(doc.OpenDoc == true && doc.Payable == true);

				if (isUnreleasedPPD)
				{
					recalculateDiscountsAction.SetEnabled(false);
				}

				bool enablePM = doc.DocType != ARDocType.SmallCreditWO && doc.OpenDoc == true;

				PXUIFieldAttribute.SetEnabled<ARInvoice.paymentMethodID>(cache, doc, enablePM);

				if (enablePM)
				{
					bool hasPaymentMethod = !string.IsNullOrEmpty(doc.PaymentMethodID);
					bool isPMInstanceRequired = false;

					if (hasPaymentMethod)
					{
						CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
						isPMInstanceRequired = pm.IsAccountNumberRequired == true;
					}

					PXUIFieldAttribute.SetEnabled<ARInvoice.pMInstanceID>(cache, doc, enablePM && hasPaymentMethod && isPMInstanceRequired);
					PXUIFieldAttribute.SetEnabled<ARInvoice.cashAccountID>(cache, e.Row, enablePM && hasPaymentMethod);
				}

				Shipping_Address.Cache.AllowUpdate = false;
				Shipping_Contact.Cache.AllowUpdate = false;
			}
			else if (isRetainageCreditMemo && !isDocumentReleased)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.docDesc>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.hold>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.docDate>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.finPeriodID>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.dontPrint>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.dontEmail>(cache, doc, true);
				release.SetEnabled(doc.Hold != true && (doc.Scheduled != true));
				recalculateDiscountsAction.SetEnabled(false);
				ARDiscountDetails.Cache.SetAllEditPermissions(allowEdit: false);
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

				Taxes.Cache.AllowDelete = false;
				Taxes.Cache.AllowUpdate = false;
				Taxes.Cache.AllowInsert = false;
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyDocBal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.batchNbr>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyID>(cache, doc, curyenabled);
				PXUIFieldAttribute.SetEnabled<ARInvoice.hold>(cache, doc, (doc.Scheduled != true));
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyVatExemptTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyVatTaxableTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyGoodsTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyMiscTot>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyDiscTot>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyFreightAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyPremiumFreightAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.multiShipAddress>(cache, doc, false);

				bool hasPaymentMethod = !string.IsNullOrEmpty(doc.PaymentMethodID);
				bool isPMInstanceRequired = false;

				if (doc.DocType == ARDocType.Invoice && hasPaymentMethod)
				{
					CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
					isPMInstanceRequired = pm.IsAccountNumberRequired == true;
				}

				PXUIFieldAttribute.SetEnabled<ARInvoice.paymentMethodID>(cache, e.Row, doc.DocType != ARDocType.SmallCreditWO);
				PXUIFieldAttribute.SetEnabled<ARInvoice.pMInstanceID>(cache, e.Row, doc.DocType != ARDocType.SmallCreditWO && isPMInstanceRequired);
				PXUIFieldAttribute.SetEnabled<ARInvoice.cashAccountID>(cache, e.Row, hasPaymentMethod);

				PXUIFieldAttribute.SetEnabled<ARInvoice.termsID>(cache, doc, (doc.DocType != ARDocType.CreditMemo));
				PXUIFieldAttribute.SetEnabled<ARInvoice.dueDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo));
				PXUIFieldAttribute.SetEnabled<ARInvoice.discDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo));
				
				Terms terms = (Terms)PXSelectorAttribute.Select<ARInvoice.termsID>(cache, doc);
				bool termsMultiple = terms?.InstallmentType == TermsInstallmentType.Multiple;
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDiscAmt>(cache, doc, (doc.DocType != ARDocType.CreditMemo && !termsMultiple));

				//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
				PXDBCurrencyAttribute.SetBaseCalc<ARInvoice.curyDocBal>(cache, doc, true);
				PXDBCurrencyAttribute.SetBaseCalc<ARInvoice.curyDiscBal>(cache, doc, true);

				cache.AllowDelete = true;
				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = (doc.ProformaExists != true || cache.GetStatus(doc) == PXEntryStatus.Inserted) && doc.IsRetainageDocument != true && !isRetainageCreditMemo;
				Transactions.Cache.AllowUpdate = (doc.ProformaExists != true || cache.GetStatus(doc) == PXEntryStatus.Inserted) && doc.IsRetainageDocument != true && !isRetainageCreditMemo;
				Transactions.Cache.AllowInsert =
					doc.IsRetainageDocument != true && 
					!isRetainageCreditMemo &&
					doc.CustomerID != null &&
					doc.CustomerLocationID != null &&
					doc.DocType != ARDocType.FinCharge &&
					(doc.ProjectID != null || !PM.ProjectAttribute.IsPMVisible(BatchModule.AR)) &&
					(doc.ProformaExists != true || cache.GetStatus(doc) == PXEntryStatus.Inserted);

				ARDiscountDetails.Cache.AllowDelete = Transactions.Cache.AllowDelete && !retainageApply;
				ARDiscountDetails.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && !retainageApply;
				ARDiscountDetails.Cache.AllowInsert = Transactions.Cache.AllowInsert && !retainageApply;

				PXUIFieldAttribute.SetEnabled<ARInvoice.curyDiscTot>(cache, doc, !PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>());

				Taxes.Cache.AllowDelete = true;
				Taxes.Cache.AllowUpdate = true;
				Taxes.Cache.AllowInsert = true;

				release.SetEnabled(doc.Hold != true && doc.Scheduled != true);
				createSchedule.SetEnabled(doc.Hold != true && (doc.DocType == ARDocType.Invoice));
				payInvoice.SetEnabled(false);

				PXUIFieldAttribute.SetEnabled<ARInvoice.retainageApply>(cache, doc, !isDocumentDebitMemo && !isDocumentCreditMemo);
				PXUIFieldAttribute.SetEnabled<ARInvoice.projectID>(cache, doc,  !isRetainageDocument);
				PXUIFieldAttribute.SetEnabled<ARInvoice.taxZoneID>(cache, doc, !isRetainageDocument);
				PXUIFieldAttribute.SetEnabled<ARInvoice.branchID>(cache, doc, !isRetainageDocument);

				PXUIFieldAttribute.SetEnabled<ARTran.curyRetainageAmt>(Transactions.Cache, null, retainageApply);
				PXUIFieldAttribute.SetEnabled<ARTran.retainagePct>(Transactions.Cache, null, retainageApply);
			}

			PXUIFieldAttribute.SetEnabled<ARInvoice.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<ARInvoice.refNbr>(cache, doc);

			Adjustments.AllowSelect = doc.DocType != ARDocType.CreditMemo;
			Adjustments.Cache.AllowInsert = false;
			Adjustments.Cache.AllowDelete = false;
			Adjustments.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && !isRetainageCreditMemo;

			Adjustments_1.AllowSelect = doc.DocType == ARDocType.CreditMemo && doc.Released != true;
			Adjustments_1.Cache.AllowInsert = Transactions.Cache.AllowUpdate && doc.Scheduled != true;
			Adjustments_1.Cache.AllowDelete = Transactions.Cache.AllowUpdate && doc.Scheduled != true;
			Adjustments_1.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && doc.Scheduled != true;

			Adjustments_2.AllowSelect = doc.DocType == ARDocType.CreditMemo && doc.Released == true;
			Adjustments_2.Cache.AllowInsert = Transactions.Cache.AllowUpdate && doc.Scheduled != true;
			Adjustments_2.Cache.AllowDelete = Transactions.Cache.AllowUpdate && doc.Scheduled != true;
			Adjustments_2.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && doc.Scheduled != true;

			PXUIFieldAttribute.SetEnabled<ARAdjust2.adjgBranchID>(Adjustments.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARAdjust.adjgBranchID>(Adjustments_1.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARAdjust.adjgBranchID>(Adjustments_2.Cache, null, false);

			if (doc == null || customer.Current == null || isRetainageCreditMemo)
			{
				editCustomer.SetEnabled(false);
			}
			else
			{
				editCustomer.SetEnabled(true);
			}

			customerRefund.SetEnabled(isDocumentCreditMemo && isDocumentReleased && !isRetainageCreditMemo);
			reverseInvoice.SetEnabled(doc?.Released == true);
			SetStateToViewProformaInvoiceAction(doc);

			if (doc.CustomerID != null)
			{
				if (Transactions.Any())
				{
					PXUIFieldAttribute.SetEnabled<ARInvoice.customerID>(cache, doc, false);
				}
			}

			if (ARSetup.Current != null)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.curyOrigDocAmt>(cache, e.Row, (bool)ARSetup.Current.RequireControlTotal || e.Row != null && (bool)((ARInvoice)e.Row).Released);
			}

			PXUIFieldAttribute.SetEnabled<ARInvoice.curyCommnblAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARInvoice.curyCommnAmt>(cache, doc, false);

			if (ARSetup.Current != null)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.commnPct>(cache, e.Row, false);
				if ((bool)doc.Released || (bool)doc.Voided)
				{
					this.salesPerTrans.Cache.AllowInsert = false;
					this.salesPerTrans.Cache.AllowDelete = false;

					PXResult<ARSalesPerTran, ARSPCommissionPeriod> sptRes = (PXResult<ARSalesPerTran, ARSPCommissionPeriod>)this.salesPerTrans.Select();
					bool isCommnPeriodClosed = false;
					if (sptRes != null)
					{
						ARSPCommissionPeriod commnPeriod = (ARSPCommissionPeriod)sptRes;
						if (!String.IsNullOrEmpty(commnPeriod.CommnPeriodID) && commnPeriod.Status == ARSPCommissionPeriodStatus.Closed)//TODO Pank AR IVA - Olga: писать в CommnPeriodID MasterPeriod
						{
							isCommnPeriodClosed = true;
						}
					}
					this.salesPerTrans.Cache.AllowUpdate = !isCommnPeriodClosed;

					PXUIFieldAttribute.SetEnabled<ARInvoice.workgroupID>(cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<ARInvoice.ownerID>(cache, e.Row, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<ARInvoice.workgroupID>(cache, e.Row, true);
					PXUIFieldAttribute.SetEnabled<ARInvoice.ownerID>(cache, e.Row, true);
				}

			}

			PXUIFieldAttribute.SetVisible<ARTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AR));

			ARAddress billAddress = this.Billing_Address.Select();
			ARShippingAddress shipAddress = this.Shipping_Address.Select();
			bool enableAddressValidation = (doc.Released == false)
				&& ((billAddress != null && billAddress.IsDefaultAddress == false && billAddress.IsValidated == false)
				|| (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false));
			this.validateAddresses.SetEnabled(enableAddressValidation);

			CT.ContractBillingTrace cbt = PXSelect<CT.ContractBillingTrace, Where<CT.ContractBillingTrace.contractID, Equal<Required<CT.ContractBillingTrace.contractID>>,
				And<CT.ContractBillingTrace.docType, Equal<Required<CT.ContractBillingTrace.docType>>, And<CT.ContractBillingTrace.refNbr, Equal<Required<CT.ContractBillingTrace.refNbr>>>>>>.SelectWindowed(this, 0, 1, doc.ProjectID, doc.DocType, doc.RefNbr);
			if (cbt != null || doc.ProformaExists == true)
			{
				//this invoice was created as a result of Contract/Project billing. Changing Project/Contract for this Invoice is not allowed.
				PXUIFieldAttribute.SetEnabled<ARInvoice.projectID>(cache, doc, false);
			}
			else
			{
				//Check for project billing without proforma:
				PMBillingRecord billingRecord = PXSelect<PMBillingRecord, Where<PMBillingRecord.aRDocType, Equal<Current<ARInvoice.docType>>, And<PMBillingRecord.aRRefNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(this);
				if (billingRecord != null)
				{
					PXUIFieldAttribute.SetEnabled<ARInvoice.projectID>(cache, doc, false);
				}
			}

			PXUIFieldAttribute.SetEnabled<ARInvoice.taxZoneID>(cache, e.Row, doc.ProformaExists != true && !isRetainageDocument && !isRetainageCreditMemo && doc.Released != true);
			PXUIFieldAttribute.SetEnabled<ARInvoice.avalaraCustomerUsageType>(cache, e.Row, doc.ProformaExists != true && doc.Released != true);
			PXUIFieldAttribute.SetEnabled<ARInvoice.revoked>(cache, e.Row, true);
			bool applyFinChargeVisible = customer.Current != null && customer.Current.FinChargeApply == true && (doc.DocType == ARInvoiceType.Invoice || doc.DocType == ARInvoiceType.DebitMemo || (doc.DocType == ARInvoiceType.FinCharge && ARSetup.Current != null && ARSetup.Current.FinChargeOnCharge == true));
			PXUIFieldAttribute.SetVisible<ARInvoice.applyOverdueCharge>(cache, null, applyFinChargeVisible);
			bool applyFinChargeEnable = doc.Status != ARDocStatus.Closed || doc.LastFinChargeDate == null || doc.LastPaymentDate == null || doc.LastFinChargeDate <= doc.LastPaymentDate;
			PXUIFieldAttribute.SetEnabled<ARInvoice.applyOverdueCharge>(cache, null, applyFinChargeEnable);

			SetDocTypeList(cache, e);

			bool showCashDiscountInfo = false;
			if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() &&
				doc.CuryOrigDiscAmt > 0m &&
				doc.DocType != ARDocType.CreditMemo &&
				doc.DocType != ARDocType.SmallCreditWO)
			{
				Taxes.Select();
				showCashDiscountInfo = doc.HasPPDTaxes == true;
			}

			PXUIFieldAttribute.SetVisible<ARInvoice.curyDiscountedDocTotal>(cache, e.Row, showCashDiscountInfo);
			PXUIFieldAttribute.SetVisible<ARInvoice.curyDiscountedTaxableTotal>(cache, e.Row, showCashDiscountInfo);
			PXUIFieldAttribute.SetVisible<ARInvoice.curyDiscountedPrice>(cache, e.Row, showCashDiscountInfo);

			PXUIVisibility visibility = showCashDiscountInfo ? PXUIVisibility.Visible : PXUIVisibility.Invisible;
			PXUIFieldAttribute.SetVisibility<ARTaxTran.curyDiscountedPrice>(Taxes.Cache, null, visibility);
			PXUIFieldAttribute.SetVisibility<ARTaxTran.curyDiscountedTaxableAmt>(Taxes.Cache, null, visibility);

			#region Retainage

			bool isRetainageApplyInvoice =
				isDocumentInvoice &&
				doc.OrigModule == BatchModule.AR &&
				(!isDocumentReleased &&
				doc.IsRetainageDocument != true ||
				retainageApply);

			bool isRetainageApplyDebitAdjustment = 
				isDocumentCreditMemo &&
				doc.OrigModule == BatchModule.AR &&
				retainageApply;

			PXUIFieldAttribute.SetVisible<ARInvoice.retainageAcctID>(cache, doc, retainageApply);
			PXUIFieldAttribute.SetVisible<ARInvoice.retainageSubID>(cache, doc, retainageApply);
			PXUIFieldAttribute.SetVisible<ARInvoice.retainageApply>(cache, doc, isRetainageApplyInvoice || isRetainageApplyDebitAdjustment);
			PXUIFieldAttribute.SetVisible<ARInvoice.isRetainageDocument>(cache, doc, !isRetainageApplyInvoice && isRetainageDocument);
			PXUIFieldAttribute.SetVisible<ARTran.retainagePct>(Transactions.Cache, null, retainageApply);
			PXUIFieldAttribute.SetVisible<ARTran.curyRetainageAmt>(Transactions.Cache, null, retainageApply);
			PXUIFieldAttribute.SetVisible<ARTaxTran.curyRetainedTaxableAmt>(Taxes.Cache, null, retainageApply && retainTaxes);
			PXUIFieldAttribute.SetVisible<ARTaxTran.curyRetainedTaxAmt>(Taxes.Cache, null, retainageApply && retainTaxes);

			PXUIFieldAttribute.SetRequired<ARInvoice.retainageAcctID>(cache, retainageApply);
			PXUIFieldAttribute.SetRequired<ARInvoice.retainageSubID>(cache, retainageApply);
			
			#endregion

			#region Migration Mode Settings

			bool isMigratedDocument = doc.IsMigratedRecord == true;
			bool isUnreleasedMigratedDocument = isMigratedDocument && doc.Released != true;
			bool isReleasedMigratedDocument = isMigratedDocument && doc.Released == true;
			bool isMigrationMode = ARSetup.Current?.MigrationMode == true;

			// We should show Initial Application for
			// released migrated document with Initial balance.
			// 
			if (doc.DocType != ARDocType.CreditMemo &&
				doc.Released == true &&
				isMigratedDocument &&
				doc.CuryInitDocBal != doc.CuryOrigDocAmt)
			{
				Adjustments.AllowSelect = false;
				Adjustments_2.AllowSelect = true;
				Adjustments_2.Cache.AllowInsert = Adjustments.Cache.AllowInsert;
				Adjustments_2.Cache.AllowDelete = Adjustments.Cache.AllowDelete;
				Adjustments_2.Cache.AllowUpdate = Adjustments.Cache.AllowUpdate;
			}

			PXUIFieldAttribute.SetVisible<ARInvoice.curyDocBal>(cache, doc, !isUnreleasedMigratedDocument);
			PXUIFieldAttribute.SetVisible<ARInvoice.curyInitDocBal>(cache, doc, isUnreleasedMigratedDocument);
			PXUIFieldAttribute.SetVisible<ARInvoice.displayCuryInitDocBal>(cache, doc, isReleasedMigratedDocument);

			if (isMigrationMode)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.retainageApply>(cache, doc, false);
			}

			if (isUnreleasedMigratedDocument)
			{
				Adjustments.Cache.AllowSelect =
				Adjustments_1.Cache.AllowSelect =
				Adjustments_2.Cache.AllowSelect = false;
			}

			bool disableCaches = isMigrationMode
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

			// We should notify the user that initial balance can be entered,
			// if there are now any errors on this box.
			// 
			if (isUnreleasedMigratedDocument &&
				string.IsNullOrEmpty(PXUIFieldAttribute.GetError<ARInvoice.curyInitDocBal>(cache, doc)))
			{
				cache.RaiseExceptionHandling<ARInvoice.curyInitDocBal>(doc, doc.CuryInitDocBal,
					new PXSetPropertyException(Messages.EnterInitialBalanceForUnreleasedMigratedDocument, PXErrorLevel.Warning));
			}

			#endregion
		}
		
		public virtual void SetStateToViewProformaInvoiceAction(ARInvoice doc)
		{
			inquiry.SetEnabled(PM.Messages.ViewProforma, doc?.ProformaExists == true);
		}

		public virtual void SetDocTypeList(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.overdueFinCharges>())
			{
				Dictionary<string, string> allowed = new ARInvoiceType.ListAttribute().ValueLabelDic;
				allowed.Remove(ARInvoiceType.FinCharge);
				PXStringListAttribute.SetList<ARInvoice.docType>(cache, e.Row, allowed.Keys.ToArray(), allowed.Values.ToArray());
			}
		}

		protected virtual void ARInvoice_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice row = e.Row as ARInvoice;
			if (row == null) return;
			
			bool isProject = ProjectDefaultAttribute.IsProject(this, row.ProjectID, out PMProject project);
			var transactions = new PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
					And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>>(this);

			foreach (ARTran tran in transactions.Select())
			{
				bool isUpdated = tran.ProjectID != row.ProjectID;
				
				tran.ProjectID = row.ProjectID;

				if (PXAccess.FeatureInstalled<FeaturesSet.costCodes>() && isProject)
					{
						if (project.BudgetLevel == BudgetLevels.Task)
						{
							int CostCodeID = CostCodeAttribute.GetDefaultCostCode();
							isUpdated = isUpdated || tran.CostCodeID != CostCodeID;
							tran.CostCodeID = CostCodeID;
						}
					}

				if (tran.LineType == SOLineType.Discount || tran.LineType == SOLineType.Freight)
				{
                    try
                    {
					tran.TaskID = isProject ? GetTaskByAccount(tran, project) : null;			
				}
                    catch (PXException exc)
                    {
                        PMProject prj = SelectFrom<PMProject>.Where<PMProject.contractID.IsEqual<@P.AsInt>>.View.Select(this, e.OldValue);
                        sender.RaiseExceptionHandling<ARInvoice.projectID>(
                            e.Row,
                            prj.ContractCD ?? e.OldValue,
                            new PXSetPropertyException(exc.MessageNoNumber));
                    }
				}

					if (isUpdated)
					{
						Transactions.Update(tran);
					}
				
				sender.SetDefaultExt<ARInvoice.defRetainagePct>(e.Row);
			}
		}

		bool isReverse = false;
		protected virtual void ARInvoice_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (isReverse)
				e.Cancel = true;
		}

		protected virtual void ARInvoice_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ARInvoice row = e.Row as ARInvoice;
			if (row != null && row.ProjectID != null && row.ProjectID != PM.ProjectDefaultAttribute.NonProject())
			{
				var selectReleased = new PXSelectJoin<PMBillingRecord,
				InnerJoin<PMBillingRecordEx, On<PMBillingRecord.projectID, Equal<PMBillingRecordEx.projectID>,
				And<PMBillingRecord.billingTag, Equal<PMBillingRecordEx.billingTag>,
				And<PMBillingRecord.recordID, Less<PMBillingRecordEx.recordID>,
				And<PMBillingRecordEx.proformaRefNbr, IsNotNull>>>>>,
				Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>,
				And<PMBillingRecord.aRDocType, Equal<Required<PMBillingRecord.aRDocType>>,
				And<PMBillingRecord.aRRefNbr, Equal<Required<PMBillingRecord.aRRefNbr>>>>>>(this);

				var resultset = selectReleased.Select(row.ProjectID, row.DocType, row.RefNbr);
				if (resultset.Count > 0)
				{
					StringBuilder sb = new StringBuilder();
					foreach (PXResult<PMBillingRecord, PMBillingRecordEx> res in resultset)
					{
						PMBillingRecordEx item = (PMBillingRecordEx)res;
						sb.AppendFormat("{0}-{1},", item.ARDocType, item.ARRefNbr);
					}

					string list = sb.ToString().TrimEnd(',');

					throw new PXException(Messages.ReleasedProforma, list);
				}
			}
		}

		protected virtual void ARInvoice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			UpdateARBalances(sender, null, e.Row);

			var select = new PXSelectJoin<PM.PMBillingRecord,
				LeftJoin<PM.PMProforma, On<PM.PMBillingRecord.proformaRefNbr, Equal<PM.PMProforma.refNbr>>>,
				Where<PM.PMBillingRecord.aRDocType, Equal<Required<PM.PMBillingRecord.aRDocType>>,
				And<PM.PMBillingRecord.aRRefNbr, Equal<Required<PM.PMBillingRecord.aRRefNbr>>>>>(this);

			var resultset = select.Select(((ARInvoice)e.Row).DocType, ((ARInvoice)e.Row).RefNbr);
			if (resultset.Count > 0)
			{
				PM.PMBillingRecord billingRecord = PXResult.Unwrap<PM.PMBillingRecord>(resultset[0]);
				if (billingRecord != null)
				{
					if (billingRecord.ProformaRefNbr != null)
					{
						billingRecord.ARDocType = null;
						billingRecord.ARRefNbr = null;
						ProjectBillingRecord.Update(billingRecord);

						PM.PMProforma proforma = PXResult.Unwrap<PM.PMProforma>(resultset[0]);
						if (proforma != null && !string.IsNullOrEmpty(proforma.RefNbr))
						{
							proforma.ARInvoiceDocType = null;
							proforma.ARInvoiceRefNbr = null;
							proforma.Released = false;
							proforma.Status = PM.ProformaStatus.Open;
							ProjectProforma.Update(proforma);
						}
					}
					else
					{
						ProjectBillingRecord.Delete(billingRecord);
					}
				}
			}
		}

		protected virtual void ARInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARInvoice row = (ARInvoice)e.Row;
			ARInvoice oldRow = (ARInvoice)e.OldRow;

			if (row.Released != true)
			{
				if (e.ExternalCall && 
					!sender.ObjectsEqual<ARInvoice.docDate, ARInvoice.retainageApply>(oldRow, row) && 
					row.OrigDocType == null && 
					row.OrigRefNbr == null)
				{
					ARDiscountEngine.AutoRecalculatePricesAndDiscounts
						(Transactions.Cache, Transactions, null, ARDiscountDetails, row.CustomerLocationID, row.DocDate, DiscountEngine.DefaultARDiscountCalculationParameters);
				}

				if (sender.GetStatus(row) != PXEntryStatus.Deleted && !sender.ObjectsEqual<ARInvoice.curyDiscTot>(oldRow, row))
				{
					if (!sender.Graph.IsImport)
					{
						AddDiscount(sender, row);
					}
					if (!ARDiscountEngine.IsInternalDiscountEngineCall && e.ExternalCall)
					{
						ARDiscountEngine.SetTotalDocDiscount(Transactions.Cache, Transactions, ARDiscountDetails,
							Document.Current.CuryDiscTot, DiscountEngine.DiscountCalculationOptions.DisableAPDiscountsCalculation);
						RecalculateTotalDiscount();
					}
				}

				if (ARSetup.Current.RequireControlTotal != true && !sender.Graph.IsCopyPasteContext)
				{
					if (row.CuryDocBal != row.CuryOrigDocAmt)
					{
						sender.SetValueExt<ARInvoice.curyOrigDocAmt>(row, row.CuryDocBal != null && row.CuryDocBal != 0m ? row.CuryDocBal : 0m);
					}
				}

				if (row.Hold != true)
				{
					if (row.CuryDocBal != row.CuryOrigDocAmt)
					{
						sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(row, row.CuryOrigDocAmt,
							new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (row.CuryOrigDocAmt < 0m)
					{
						if (ARSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(row, row.CuryOrigDocAmt,
								new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
						else
						{
							sender.RaiseExceptionHandling<ARInvoice.curyDocBal>(row, row.CuryDocBal,
								new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
					}
					else
					{
							sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(row, null, null);
							sender.RaiseExceptionHandling<ARInvoice.curyDocBal>(row, null, null);
					}
				}

				if (row.CustomerID != null && row.CuryDiscTot != null && row.CuryDiscTot > 0 && row.CuryLineTotal != null && row.CuryLineTotal > 0)
				{
					decimal discountLimit = ARDiscountEngine.GetDiscountLimit(sender, row.CustomerID);
					if ((row.CuryLineTotal / 100 * discountLimit) < row.CuryDiscTot)
					{
						PXUIFieldAttribute.SetWarning<ARInvoice.curyDiscTot>(sender, row,
                            PXMessages.LocalizeFormatNoPrefix(Messages.DocDiscountExceedLimit, discountLimit));
					}
				}
			}

			UpdateARBalances(sender, e.Row, e.OldRow);
		}

		protected virtual void ARInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateARBalances(sender, e.Row, null);
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

		protected virtual void ARTran_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null && row.ProjectID != null && !PM.ProjectDefaultAttribute.IsNonProject( row.ProjectID) && row.TaskID != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
				if (account != null && account.AccountGroupID == null)
				{
					sender.RaiseExceptionHandling<ARTran.accountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
				}
			}
		}

		protected virtual void ARTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null && row.TaskID == null)
			{
				sender.SetDefaultExt<ARTran.taskID>(e.Row);
			}
		}

		protected virtual void ARTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran != null && tran.AccountID != null && location.Current != null)
			{
				InventoryItem item = InventoryItemGetByID(tran.InventoryID);
				EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<ARTran.employeeID>>>>.SelectSingleBound(this, new object[] { e.Row });

				CRLocation companyloc =
					(CRLocation)PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<GL.Branch.branchID, Equal<Required<ARTran.branchID>>>>.Select(this, tran.BranchID);
				SalesPerson salesperson = (SalesPerson)PXSelect<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<ARTran.salesPersonID>>>>.SelectSingleBound(this, new object[] { e.Row });

				int? customer_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cSalesSubID>(location.Current);
				int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.salesSubID>(item);
				int? employee_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.salesSubID>(employee);
				int? company_SubID = (int?)Caches[typeof(CRLocation)].GetValue<CRLocation.cMPSalesSubID>(companyloc);
				int? salesperson_SubID = (int?)Caches[typeof(SalesPerson)].GetValue<SalesPerson.salesSubID>(salesperson);

				object value;
				try
				{
					value = SubAccountMaskAttribute.MakeSub<ARSetup.salesSubMask>(this, SalesSubMask,
						new object[] { customer_SubID, item_SubID, employee_SubID, company_SubID, salesperson_SubID },
						new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(EPEmployee.salesSubID), typeof(Location.cMPSalesSubID), typeof(SalesPerson.salesSubID) });

					sender.RaiseFieldUpdating<ARTran.subID>(e.Row, ref value);
				}
				catch (PXException)
				{
					value = null;
				}

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[ARTax(typeof(ARInvoice), typeof(ARTax), typeof(ARTaxTran), typeof(ARInvoice.branchID))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		protected virtual void ARTran_TaxCategoryID_CacheAttached(PXCache sender)
		{
		}

		[PXBool]
		[DRTerms.Dates(typeof(ARTran.dRTermStartDate), typeof(ARTran.dRTermEndDate), typeof(ARTran.inventoryID), typeof(ARTran.deferredCode))]
		protected virtual void ARTran_RequiresTerms_CacheAttached(PXCache sender) { }

		protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if
			(	TaxAttribute.GetTaxCalc<ARTran.taxCategoryID>(sender, e.Row) == TaxCalc.Calc &&
				taxzone.Current != null && !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID) &&
				((ARTran)e.Row).InventoryID == null &&
				!isReverse
			)
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

			if (row != null && row.InventoryID != null && row.UOM != null && row.ManualPrice != true)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;
				Location c = location.Select();

				if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
					customerPriceClass = c.CPriceClassID;

				DateTime date = Document.Current.DocDate.Value;

				if (row.TranType == ARDocType.CreditMemo && row.OrigInvoiceDate != null)
				{
					date = row.OrigInvoiceDate.Value;
				}
				e.NewValue = ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, ((ARTran)e.Row).CustomerID, ((ARTran)e.Row).InventoryID, ((ARTran)e.Row).SiteID, currencyinfo.Select(), ((ARTran)e.Row).UOM, ((ARTran)e.Row).Qty, date, row.CuryUnitPrice) ?? 0m;

				ARSalesPriceMaint.CheckNewUnitPrice<ARTran, ARTran.curyUnitPrice>(sender, row, e.NewValue);
			}
			else
			{
				e.NewValue = sender.GetValue<ARTran.curyUnitPrice>(e.Row);
				e.Cancel = e.NewValue != null;
				return;
			}
		}

		protected virtual void ARTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
		}

		protected virtual void ARTran_OrigInvoiceDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
		}

		protected virtual void ARTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				if (row.Qty == 0)
				{
					sender.SetValueExt<ARTran.curyDiscAmt>(row, decimal.Zero);
					sender.SetValueExt<ARTran.discPct>(row, decimal.Zero);
				}
				else
				{
					sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
				}
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

		protected virtual void ARTran_EmployeeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.subID>(e.Row);
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void ARTran_InventoryID_CacheAttached(PXCache sender)
        {
        }


		protected virtual void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran tran = e.Row as ARTran;

			Int32? accountID = tran.AccountID;
			sender.SetDefaultExt<ARTran.accountID>(e.Row);
			tran.AccountID = tran.AccountID ?? accountID;

			try
			{
				sender.SetDefaultExt<ARTran.subID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<ARTran.subID>(e.Row, null);
			}
			sender.SetDefaultExt<ARTran.taxCategoryID>(e.Row);
			sender.SetDefaultExt<ARTran.deferredCode>(e.Row);

			if (e.ExternalCall && tran != null)
				tran.CuryUnitPrice = 0m;

			sender.SetDefaultExt<ARTran.uOM>(e.Row);

			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
			IN.InventoryItem item = PXSelectorAttribute.Select<IN.InventoryItem.inventoryID>(sender, tran) as IN.InventoryItem;
			if (item != null && tran != null)
			{
				tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, "Descr", customer.Current?.LocaleName);
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

		protected virtual void ARTran_DefScheduleID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, ((ARTran)e.Row).DefScheduleID);
			if (sc != null)
			{
				ARTran defertran = PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>.Select(this, sc.DocType, sc.RefNbr, sc.LineNbr);

				if (defertran != null)
				{
					((ARTran)e.Row).DeferredCode = defertran.DeferredCode;
				}
			}
		}

		protected virtual void ARTran_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (e.ExternalCall && row != null)
			{
				ARDiscountEngine.UpdateManualLineDiscount(sender, Transactions, row, ARDiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.DocDate, DiscountEngine.DefaultARDiscountCalculationParameters);
			}
		}

		protected virtual void ARTran_DeferredCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;

			if (row == null)
			{
				return;
			}

				if (row.TranType == ARDocType.CreditMemo)
				{
					DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, e.NewValue);

				if (dc?.Method == DeferredMethodType.CashReceipt)
					{
						e.Cancel = true;
						if (sender.RaiseExceptionHandling<ARTran.deferredCode>(e.Row, e.NewValue, new PXSetPropertyException(Messages.InvalidCashReceiptDeferredCode)))
						{
							throw new PXSetPropertyException(Messages.InvalidCashReceiptDeferredCode);
						}
					}
				}
			}

		protected virtual void ARTran_DeferredCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (ARTran)e.Row;

			string prevDRCode = (string)e.OldValue;
			string newDRCode = row.DeferredCode;

			if(string.IsNullOrEmpty(prevDRCode) == true && string.IsNullOrEmpty(newDRCode) == false)
			{
				var document = Document.Current;
				document.DRSchedCntr++;
				Document.Update(document);
		}

			if (string.IsNullOrEmpty(prevDRCode) == false && string.IsNullOrEmpty(newDRCode) == true)
			{
				var document = Document.Current;
				document.DRSchedCntr--;
				Document.Update(document);
			}
		}

		protected virtual void ARTran_DiscPct_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
            if (row == null)
                return;

            e.NewValue = MinGrossProfitValidator<ARTran>.ValidateDiscountPct<ARTran.inventoryID, ARTran.uOM>(sender, row, row.UnitPrice, (decimal?)e.NewValue);
		}

		protected virtual void ARTran_CuryDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
            if (row == null)
                return;

            e.NewValue = MinGrossProfitValidator<ARTran>.ValidateDiscountAmt<ARTran.inventoryID, ARTran.uOM>(sender, row, row.UnitPrice, (decimal?)e.NewValue);
		}

		protected virtual void ARTran_CuryUnitPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
            if (row == null)
                return;

            e.NewValue = MinGrossProfitValidator<ARTran>.ValidateUnitPrice<ARTran.curyInfoID, ARTran.inventoryID, ARTran.uOM>(sender, row, (decimal?)e.NewValue);
		}

		protected virtual void ARTran_DRTermStartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var line = e.Row as ARTran;

			if (line != null && line.RequiresTerms == true)
			{
				e.NewValue = Document.Current.DocDate;
			}
		}

		protected virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;
			if (row != null)
			{
				if ((!sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.baseQty>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyTranAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyDiscAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.discPct>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.manualDisc>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.discountID>(e.Row, e.OldRow)) && row.LineType != SOLineType.Discount)
					RecalculateDiscounts(sender, row);

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

				TaxAttribute.Calculate<ARTran.taxCategoryID>(sender, e);

				//Validate that Sales Account <> Deferral Account:
				if (!sender.ObjectsEqual<ARTran.accountID, ARTran.deferredCode>(e.Row, e.OldRow))
				{
					if (!string.IsNullOrEmpty(row.DeferredCode))
					{
						DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, row.DeferredCode);
						if (defCode != null)
						{
							if (defCode.AccountID == row.AccountID)
							{
								sender.RaiseExceptionHandling<ARTran.accountID>(e.Row, row.AccountID,
									new PXSetPropertyException(Messages.AccountIsSameAsDeferred, PXErrorLevel.Warning));
							}
						}
					}
				}
			}
		}
		
		protected virtual void ARTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			PXParentAttribute.SetParent(sender, e.Row, typeof(ARRegister), this.Document.Current);
		}

		protected virtual void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			TaxAttribute.Calculate<ARTran.taxCategoryID>(sender, e);

			if (((ARTran)e.Row).SortOrder == null)
				((ARTran)e.Row).SortOrder = ((ARTran)e.Row).LineNbr;
		}

		protected virtual void ARTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			
			if (row == null) return;
			
			if (row.Released == true)
			{
				e.Cancel = true;
				throw new PXException(Messages.ARTransExist);
			}

		}

		protected virtual void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;

			if (row != null)
			{
				foreach (PM.PMTran pMRef in RefContractUsageTran.Select(row.TranType, row.RefNbr, row.LineNbr))
				{
					if (pMRef != null)
					{
						pMRef.ARRefNbr = null;
						pMRef.ARTranType = null;
						pMRef.RefLineNbr = null;
						if (Document.Current != null && Document.Current.ProformaExists != true)
						{
							pMRef.Billed = false;
							pMRef.BilledDate = null;
							pMRef.InvoicedQty = 0;
							pMRef.InvoicedAmount = 0;
							PM.RegisterReleaseProcess.AddToUnbilledSummary(this, pMRef);
						}
						
						RefContractUsageTran.Update(pMRef);
					}
				}
			}

			if (Document.Current != null && Document.Current.InstallmentNbr == null)
			{
				Document.Current.IsTaxValid = false;
				Document.Cache.MarkUpdated(Document.Current);
			}

			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted)
			{
				if (row.LineType != SOLineType.Discount)
				{
					ARDiscountEngine.RecalculateGroupAndDocumentDiscounts(
						sender, 
						Transactions, 
						null, 
						ARDiscountDetails, 
						Document.Current.BranchID, 
						Document.Current.CustomerLocationID, 
						Document.Current.DocDate,
						DiscountEngine.DefaultARDiscountCalculationParameters | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation);
				}

				RecalculateTotalDiscount();
			}
		}

		protected virtual void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARTran.defScheduleID>(sender, row, row.TranType == ARInvoiceType.CreditMemo || row.TranType == ARInvoiceType.DebitMemo);
				PXUIFieldAttribute.SetEnabled<ARTran.deferredCode>(sender, row, row.DefScheduleID == null);
					}

			#region Migration Mode Settings

			ARInvoice doc = Document.Current;

			if (doc != null &&
				doc.IsMigratedRecord == true &&
				doc.Released != true)
			{
				PXUIFieldAttribute.SetEnabled<ARTran.defScheduleID>(Transactions.Cache, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.deferredCode>(Transactions.Cache, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.dRTermStartDate>(Transactions.Cache, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.dRTermEndDate>(Transactions.Cache, null, false);
				}

			#endregion
		}

		protected virtual void ARTran_DiscountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<ARTran, ARTran.costCodeID> e)
		{
			PMProject project;
			if (CostCodeAttribute.UseCostCode() && ProjectDefaultAttribute.IsProject(this, e.Row.ProjectID, out project))
			{
				if (project.BudgetLevel == BudgetLevels.Task)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
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
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}

		protected virtual void ARTaxTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			PXParentAttribute.SetParent(sender, e.Row, typeof(ARRegister), this.Document.Current);
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

		#region ARAdjust2 Events
		protected virtual void ARAdjust2_AdjgRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust2 adj = (ARAdjust2)e.Row;

			if (adj?.AdjgRefNbr == null)
				return;

			var selectAdjgDocument = new PXSelectJoin<ARPayment,
				LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARPayment.docType>,
					And<ARAdjust.adjdRefNbr, Equal<ARPayment.refNbr>,
					And<ARAdjust.released, NotEqual<True>,
					And<ARAdjust.voided, NotEqual<True>>>>>,
				LeftJoin<ARAdjust2, On<ARAdjust2.adjgDocType, Equal<ARPayment.docType>,
					And<ARAdjust2.adjgRefNbr, Equal<ARPayment.refNbr>,
					And<ARAdjust2.released, NotEqual<True>,
					And<ARAdjust2.voided, NotEqual<True>,
					And<Where<ARAdjust2.adjdDocType, NotEqual<Required<ARAdjust2.adjdDocType>>,
						Or<ARAdjust2.adjdRefNbr, NotEqual<Required<ARAdjust2.adjdRefNbr>>>>>>>>>>>,
				Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
					And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
					And<ARPayment.released, Equal<True>,
					And<ARPayment.openDoc, Equal<True>,
					And<ARPayment.hold, NotEqual<True>,
					And<Where<ARAdjust.adjgRefNbr, IsNotNull,
						Or<ARAdjust2.adjdRefNbr, IsNotNull>>>>>>>>>(this);

			using (new PXFieldScope(selectAdjgDocument.View, 
				typeof(ARPayment.docType), 
				typeof(ARPayment.refNbr),
				typeof(ARAdjust.adjdDocType),
				typeof(ARAdjust.adjdRefNbr),
				typeof(ARAdjust2.adjgDocType),
				typeof(ARAdjust2.adjgRefNbr)))
			{
				PXResult<ARPayment, ARAdjust, ARAdjust2> res = 
					selectAdjgDocument.View.SelectSingle(adj.AdjdDocType, adj.AdjdRefNbr, adj.AdjgDocType, adj.AdjgRefNbr) 
						as PXResult<ARPayment, ARAdjust, ARAdjust2>;
				if (res != null)
				{
					ARPayment payment = res;
					ARAdjust adjusted = res;
					ARAdjust2 adjusting = res;

					string adjDocType = string.IsNullOrEmpty(adjusted.AdjgDocType) ? adjusting.AdjdDocType : adjusted.AdjgDocType;
					string adjRefNbr = string.IsNullOrEmpty(adjusted.AdjgRefNbr) ? adjusting.AdjdRefNbr : adjusted.AdjgRefNbr;
					ARDocType docTypes = new ARDocType();

					throw new PXSetPropertyException(
						Messages.ApplicationIsAlreadyApplied, docTypes.GetLabel(payment.DocType), payment.RefNbr, docTypes.GetLabel(adjDocType), adjRefNbr);
				}
			}
		}

		protected virtual void ARAdjust2_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust2 adj = (ARAdjust2)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<ARInvoice.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.CuryDocBal == null)
			{
				CalcBalancesFromInvoiceSide(adj, false, false);
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

		protected virtual void ARAdjust2_CuryAdjdAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust2 application = e.Row as ARAdjust2;
			CalcBalancesFromInvoiceSide(application, true, false);
		}

		protected virtual void ARAdjust2_CuryAdjdWOAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust2 application = e.Row as ARAdjust2;
			CalcBalancesFromInvoiceSide(application, true, false);
		}

		protected virtual void ARAdjust2_CuryAdjdWOAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust2 adj = e.Row as ARAdjust2;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
			{
				CalcBalancesFromInvoiceSide(adj, false, false);
			}

			if (adj.CuryDocBal == null || adj.CuryWhTaxBal == null)
			{
				sender.RaiseExceptionHandling<ARAdjust2.adjgRefNbr>(adj, adj.AdjdRefNbr,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, PXUIFieldAttribute.GetDisplayName<ARAdjust2.adjgRefNbr>(sender)));
				return;
			}

			// We should use absolute values here, because wo amount 
			// may have positive or negative sign.
			// 
			if ((decimal)adj.CuryWhTaxBal + Math.Abs((decimal)adj.CuryAdjdWOAmt) - Math.Abs((decimal)e.NewValue) < 0m)
			{
				throw new PXSetPropertyException(Messages.ApplicationWOLimitExceeded, ((decimal)adj.CuryWhTaxBal + Math.Abs((decimal)adj.CuryAdjdWOAmt)).ToString());
			}
		}

		protected virtual void ARAdjust2_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}

		protected virtual void ARAdjust2_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARAdjust2 adjustment = e.Row as ARAdjust2;

			#region Check AdjgRefNbr before save
			// We raising verifying event here to prevent 
			// situations when it is possible to apply the same
			// invoice twice due to read only invoice view.
			// For more details see AC-85468, AC-90600.
			object adjgRefNbr = adjustment.AdjgRefNbr;

			try
			{
				sender.RaiseFieldVerifying<ARAdjust2.adjgRefNbr>(adjustment, ref adjgRefNbr);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling<ARAdjust2.adjgRefNbr>(adjustment, adjgRefNbr, ex);
			}
			#endregion

			if (adjustment.CuryDocBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust2.curyAdjdAmt>(e.Row, adjustment.CuryAdjdAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (adjustment.CuryDiscBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust2.curyAdjdPPDAmt>(e.Row, adjustment.CuryAdjdPPDAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (adjustment.CuryWhTaxBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust2.curyAdjdWOAmt>(e.Row, adjustment.CuryAdjdWOAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (adjustment.CuryAdjdWOAmt != 0m && string.IsNullOrEmpty(adjustment.WriteOffReasonCode))
			{
				if (sender.RaiseExceptionHandling<ARAdjust2.writeOffReasonCode>(e.Row, null, 
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust2.writeOffReasonCode>(sender))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<ARAdjust2.writeOffReasonCode>(), null, ErrorMessages.FieldIsEmpty, 
						PXUIFieldAttribute.GetDisplayName<ARAdjust2.writeOffReasonCode>(sender));
				}
			}

			decimal currencyAdjustedBalanceDelta = BalanceCalculation.GetFullBalanceDelta(adjustment).CurrencyAdjustedBalanceDelta;

			if (adjustment.VoidAdjNbr == null && currencyAdjustedBalanceDelta < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust2.curyAdjdAmt>(adjustment, adjustment.CuryAdjdAmt,
					new PXSetPropertyException(Messages.RegularApplicationTotalAmountNegative));
			}

			if (adjustment.VoidAdjNbr != null && currencyAdjustedBalanceDelta > 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust2.curyAdjdAmt>(adjustment, adjustment.CuryAdjdAmt,
					new PXSetPropertyException(Messages.ReversedApplicationTotalAmountPositive));
			}
		}

		/// <summary>
		/// The method to calculate application
		/// balances in Invoice currency.
		/// </summary>
		protected void CalcBalancesFromInvoiceSide(
			ARAdjust2 adj,
			bool isCalcRGOL,
			bool DiscOnDiscDate)
		{
			foreach (ARPayment payment in PXSelectReadonly<
				ARPayment,
				Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
					And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
				.Select(this, adj.AdjgDocType, adj.AdjgRefNbr))
                {
				CalcBalancesFromInvoiceSide(adj, payment, isCalcRGOL, DiscOnDiscDate);
				return;
                }
                }

		/// <summary>
		/// The method to calculate application
		/// balances in Invoice currency. Only 
		/// payment document should be set.
		/// </summary>
		protected void CalcBalancesFromInvoiceSide(
			ARAdjust2 adj,
			ARPayment payment,
			bool isCalcRGOL,
			bool DiscOnDiscDate)
		{
			foreach (ARInvoice invoice in ARInvoice_CustomerID_DocType_RefNbr
				.Select(adj.AdjdCustomerID, adj.AdjdDocType, adj.AdjdRefNbr))
                {
				CalcBalancesFromInvoiceSide(adj, invoice, payment, isCalcRGOL, DiscOnDiscDate);
				return;
			}
                }

		/// <summary>
		/// The base method to calculate application
		/// balances in Invoice currency. Both invoice
		/// and payment documents should be set.
		/// </summary>
		protected void CalcBalancesFromInvoiceSide(
			ARAdjust2 adj,
			ARInvoice invoice,
			ARPayment payment,
			bool isCalcRGOL,
			bool DiscOnDiscDate)
		{
			if (invoice == null) return;

			InitBalancesFromInvoiceSide(CurrencyInfo_CuryInfoID, adj, invoice, payment);

			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount(adj.AdjgDocDate, invoice, adj);
			}

			PaymentEntry.AdjustBalance(CurrencyInfo_CuryInfoID, adj, adj.AdjdCuryInfoID, adj.CuryAdjdAmt, adj.CuryAdjdDiscAmt, adj.CuryAdjdWOAmt, false);

			if (isCalcRGOL && (adj.Voided != true))
			{
				CalcRGOLFromInvoiceSide(CurrencyInfo_CuryInfoID, payment, adj);
				adj.RGOLAmt = adj.ReverseGainLoss == true ? -1m * adj.RGOLAmt : adj.RGOLAmt;
			}
		}

		/// <summary>
		/// The method to initialize application
		/// balances in Invoice currency.
		/// </summary>
		protected void InitBalancesFromInvoiceSide(
			PXSelectBase<CurrencyInfo> currencyinfoselect,
			ARAdjust2 adj,
			ARInvoice invoice,
			ARPayment payment)
		{
			// Payment balance should be calculated 
			// in Invoice currency.
			//
			decimal curyDocBal;
			decimal docBal;

			PaymentEntry.CalcBalance(
				currencyinfoselect,
				adj.AdjdCuryInfoID,
				adj.AdjgCuryInfoID,
				payment.CuryInfoID,
				payment.CuryDocBal,
				payment.DocBal,
				out curyDocBal,
				out docBal);

			adj.CuryDocBal = curyDocBal;
			adj.DocBal = docBal;

			// Discount balance can be taken 
			// from the Invoice as is.
			//
			adj.CuryDiscBal = invoice.CuryDiscBal;
			adj.DiscBal = invoice.DiscBal;

			// WO balance should be taken from 
			// the customer in Invoice currency.
			//
			adj.CuryWhTaxBal = 0m;
			adj.WhTaxBal = 0m;

			invoice.CuryWhTaxBal = 0m;
			invoice.WhTaxBal = 0m;

			if (adj.AdjgDocType != ARDocType.Refund &&
				adj.AdjdDocType != ARDocType.CreditMemo)
			{
				Customer invoiceCustomer = PXSelect<
					Customer,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, adj.AdjdCustomerID);

				if (invoiceCustomer?.SmallBalanceAllow == true)
		{
					decimal invoice_smallbalancelimit;
					CurrencyInfo invoice_info = currencyinfoselect.Select(adj.AdjdCuryInfoID);
					PXDBCurrencyAttribute.CuryConvCury(currencyinfoselect.Cache, invoice_info, invoiceCustomer.SmallBalanceLimit ?? 0m, out invoice_smallbalancelimit);

					adj.CuryWhTaxBal = invoice_smallbalancelimit;
					adj.WhTaxBal = invoiceCustomer.SmallBalanceLimit;

					invoice.CuryWhTaxBal = invoice_smallbalancelimit;
					invoice.WhTaxBal = invoiceCustomer.SmallBalanceLimit;
				}
			}
		}

		/// <summary>
		/// The method to calculate application RGOL
		/// from the Invoice document side.
		/// </summary>
		protected void CalcRGOLFromInvoiceSide<TInvoice, TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect, 
			TInvoice document, 
			TAdjustment adj)

			where TInvoice : IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			if (adj.CuryAdjdAmt == null || adj.CuryAdjdDiscAmt == null || adj.CuryAdjdWhTaxAmt == null) return;
			
			CurrencyInfo invoice_info = currencyinfoselect.Select(adj.AdjdCuryInfoID);
			CurrencyInfo payment_info = currencyinfoselect.Select(adj.AdjgCuryInfoID);
			CurrencyInfo payment_originfo = currencyinfoselect.Select(document.CuryInfoID);

			decimal adj_rgol_amt;
			decimal disc_rgol_amt;
			decimal whtax_rgol_amt;
			decimal curyadjgamt;

			PaymentEntry.CalcRGOL(
				currencyinfoselect.Cache, 
				invoice_info, 
				payment_info, 
				payment_originfo, 
				adj.CuryAdjdDiscAmt, 
				adj.AdjDiscAmt, 
				out curyadjgamt, 
				out disc_rgol_amt);
			adj.CuryAdjgDiscAmt = curyadjgamt;

			PaymentEntry.CalcRGOL(
				currencyinfoselect.Cache, 
				invoice_info, 
				payment_info, 
				payment_originfo,
				adj.CuryAdjdWhTaxAmt, 
				adj.AdjWhTaxAmt, 
				out curyadjgamt, 
				out whtax_rgol_amt);
			adj.CuryAdjgWhTaxAmt = curyadjgamt;

			PaymentEntry.CalcRGOL(
				currencyinfoselect.Cache, 
				invoice_info, 
				payment_info, 
				payment_originfo,
				adj.CuryAdjdAmt, 
				adj.AdjAmt, 
				out curyadjgamt, 
				out adj_rgol_amt);
			adj.CuryAdjgAmt = curyadjgamt;

			adj.RGOLAmt = adj_rgol_amt + disc_rgol_amt + whtax_rgol_amt;
		}
		#endregion

		#region ARInvoiceDiscountDetail events

		protected virtual void ARInvoiceDiscountDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;
			if (discountDetail == null) return;

			if (Document?.Current != null)
		{
				Document.Cache.SetValueExt<ARInvoice.curyDocDisc>(Document.Current, ARDiscountEngine.GetTotalGroupAndDocumentDiscount(ARDiscountDetails, true));
			}
		}

		protected virtual void ARInvoiceDiscountDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;
			if (!ARDiscountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (discountDetail.DiscountID != null)
				{
					ARDiscountEngine.InsertManualDocGroupDiscount(Transactions.Cache, Transactions, ARDiscountDetails, discountDetail, discountDetail.DiscountID, null, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.DocDate, GetDefaultARDiscountCalculationOptions(Document.Current) | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation);
					RecalculateTotalDiscount();
				}

				if (ARDiscountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, ARDiscountDetails, discountDetail, null, DiscountEngine.DefaultARDiscountCalculationParameters))
					RecalculateTotalDiscount();
			}

			if (discountDetail != null && discountDetail.DiscountID != null && discountDetail.DiscountSequenceID != null && discountDetail.Description == null)
			{
				object description = null;
				sender.RaiseFieldDefaulting<ARInvoiceDiscountDetail.description>(discountDetail, out description);
				sender.SetValue<ARInvoiceDiscountDetail.description>(discountDetail, description);
			}
		}

		protected virtual void ARInvoiceDiscountDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;
			ARInvoiceDiscountDetail oldDiscountDetail = (ARInvoiceDiscountDetail)e.OldRow;
			if (!ARDiscountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (!sender.ObjectsEqual<ARInvoiceDiscountDetail.discountID>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARInvoiceDiscountDetail.discountSequenceID>(e.Row, e.OldRow))
				{
					ARDiscountEngine.UpdateManualDocGroupDiscount(Transactions.Cache, Transactions, ARDiscountDetails, discountDetail, discountDetail.DiscountID, sender.ObjectsEqual<ARInvoiceDiscountDetail.discountID>(e.Row, e.OldRow) ? discountDetail.DiscountSequenceID : null, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.DocDate.Value, DiscountEngine.DefaultARDiscountCalculationParameters | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation);
					RecalculateTotalDiscount();
				}
				if (!sender.ObjectsEqual<ARInvoiceDiscountDetail.skipDiscount>(e.Row, e.OldRow))
			{
					ARDiscountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, ARDiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.DocDate, discountDetail.Type != DiscountType.Document, DiscountEngine.DefaultARDiscountCalculationParameters | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation);
					RecalculateTotalDiscount();
				}

				if (ARDiscountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, ARDiscountDetails, discountDetail, oldDiscountDetail, DiscountEngine.DefaultARDiscountCalculationParameters))
					RecalculateTotalDiscount();
			}
		}

		protected virtual void ARInvoiceDiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;
			if (!ARDiscountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (discountDetail.IsOrigDocDiscount == true)
				{
					ARDiscountEngine.UpdateGroupAndDocumentDiscountRatesOnly(Transactions.Cache, Transactions, null, ARDiscountDetails, false);
				}
				else
			{
					ARDiscountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, ARDiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.DocDate, (discountDetail.Type != null && discountDetail.Type != DiscountType.Document && discountDetail.Type != DiscountType.ExternalDocument), DiscountEngine.DefaultARDiscountCalculationParameters | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation);
				}
			}
			RecalculateTotalDiscount();
		}

		protected virtual void ARInvoiceDiscountDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;

			bool isExternalDiscount = discountDetail.Type == DiscountType.ExternalDocument;

			PXDefaultAttribute.SetPersistingCheck<ARInvoiceDiscountDetail.discountID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<ARInvoiceDiscountDetail.discountSequenceID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}

		protected virtual void ARInvoiceDiscountDetail_DiscountSequenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}
		protected virtual void ARInvoiceDiscountDetail_DiscountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		public virtual DiscountEngine.DiscountCalculationOptions GetDefaultARDiscountCalculationOptions(ARInvoice doc)
		{
			return DiscountEngine.DefaultARDiscountCalculationParameters | ((doc != null && doc.DisableAutomaticDiscountCalculation == true) ? DiscountEngine.DiscountCalculationOptions.DisableAllAutomaticDiscounts : DiscountEngine.DiscountCalculationOptions.CalculateAll);
		}

		#endregion

		private InventoryItem InventoryItemGetByID(int? inventoryID)
		{
			return PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
		}

		private DRDeferredCode DeferredCodeGetByID(string deferredCodeID)
		{
			return PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, deferredCodeID);
		}

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "Transactions", true) == 0)
			{
				keys["tranType"] = Document.Current.DocType;
				keys["refNbr"] = Document.Current.RefNbr;
				if (DontUpdateExistRecords)
				{
					keys["lineNbr"] = Document.Current.LineCntr + 1;
				}
			}
			return true;
		}

		private static bool DontUpdateExistRecords
		{
			get
			{
				object dontUpdateExistRecords;
				return
					PX.Common.PXExecutionContext.Current.Bag.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS,
																		 out dontUpdateExistRecords) &&
																		 true.Equals(dontUpdateExistRecords);
			}
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		#region External Tax
		public virtual bool IsExternalTax(string taxZoneID)
			{
					return false;
				}

		public virtual ARInvoice CalculateExternalTax(ARInvoice invoice)
			{
			return invoice;
		}

		public virtual void RecalcUnbilledTax() { }

		public virtual ARInvoice RecalculateExternalTax(ARInvoiceEntry taxCalculationEntry, ARInvoice invoice) => invoice;

		#endregion

		public virtual void RecalculateDiscounts(PXCache sender, ARTran line)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && line.Qty != null && line.CuryTranAmt != null && line.IsFree != true)
			{
				DiscountEngine.DiscountCalculationOptions discountCalculationOptions = DiscountEngine.DefaultARDiscountCalculationParameters | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation;
				if (line.CalculateDiscountsOnImport == true)
					discountCalculationOptions = discountCalculationOptions | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport;

				ARDiscountEngine.SetDiscounts(
					sender,
					Transactions,
					line,
					ARDiscountDetails,
					Document.Current.BranchID,
					Document.Current.CustomerLocationID,
					Document.Current.CuryID,
					Document.Current.DocDate,
					recalcdiscountsfilter.Current,
					discountCalculationOptions);

				if (line.CuryTranAmt != null && line.IsFree != true)
			{
				RecalculateTotalDiscount();
		}
		}
			else if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && Document.Current != null)
			{
				ARDiscountEngine.CalculateDocumentDiscountRate(Transactions.Cache, Transactions, line, ARDiscountDetails);
			}
		}

		public virtual void RecalculateTotalDiscount()
		{
			if (Document.Current != null)
			{
				ARInvoice old_row = PXCache<ARInvoice>.CreateCopy(Document.Current);
				Document.Cache.SetValueExt<ARInvoice.curyDiscTot>(Document.Current, ARDiscountEngine.GetTotalGroupAndDocumentDiscount(ARDiscountDetails));
				Document.Cache.RaiseRowUpdated(Document.Current, old_row);
			}
		}

		private void CheckApplicationDateAndPeriod(PXCache sender, ARInvoice document, ARAdjust application)
		{
			if (document == null) throw new ArgumentNullException(nameof(document));
			if (application == null) throw new ArgumentNullException(nameof(application));

			if (application.AdjdDocDate > document.DocDate)
			{
				if (sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(
					application,
					application.AdjdRefNbr,
					new PXSetPropertyException(
						Messages.UnableToApplyDocumentApplicationDateEarlierThanDocumentDate,
						PXErrorLevel.RowError)))
				{
					throw new PXRowPersistingException(
						PXDataUtils.FieldName<ARAdjust.adjdDocDate>(),
						application.AdjdDocDate,
						Messages.UnableToApplyDocumentApplicationDateEarlierThanDocumentDate);
				}
			}

			if (application.AdjdTranPeriodID?.CompareTo(document.TranPeriodID) > 0)
			{
				if (sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(
					application,
					application.AdjdRefNbr,
					new PXSetPropertyException(
						Messages.UnableToApplyDocumentApplicationPeriodPrecedesDocumentPeriod,
						PXErrorLevel.RowError)))
				{
					throw new PXRowPersistingException(
						PXDataUtils.FieldName<ARAdjust.adjdFinPeriodID>(),
						application.AdjdFinPeriodID,
						Messages.UnableToApplyDocumentApplicationPeriodPrecedesDocumentPeriod);
				}
			}
		}

		public virtual void AddDiscount(PXCache sender, ARInvoice row)
		{
			ARTran discount = (ARTran)Discount_Row.Cache.CreateInstance();
			discount.LineType = SOLineType.Discount;
			discount.DrCr = (Document.Current.DrCr == DrCr.Debit) ? DrCr.Credit : DrCr.Debit;
			discount.FreezeManualDisc = true;
			discount = (ARTran)Discount_Row.Select() ?? (ARTran)Discount_Row.Cache.Insert(discount);

			ARTran old_row = (ARTran)Discount_Row.Cache.CreateCopy(discount);

			discount.CuryTranAmt = (decimal?)sender.GetValue<ARInvoice.curyDiscTot>(row);
			discount.TaxCategoryID = null;
			using (new PXLocaleScope(customer.Current.LocaleName))
				discount.TranDesc = PXMessages.LocalizeNoPrefix(Messages.DocDiscDescr);

			DefaultDiscountAccountAndSubAccount(discount);


			if (discount.TaskID == null && ProjectDefaultAttribute.IsProject(this, discount.ProjectID, out PMProject project))
					{
				if (project != null)
					discount.TaskID = GetTaskByAccount(discount, project);
			}

			if (CostCodeAttribute.UseCostCode() && discount.CostCodeID == null)
			{
				discount.CostCodeID = CostCodeAttribute.DefaultCostCode;
			}

			Discount_Row.Cache.MarkUpdated(discount);

			discount.ManualDisc = true; //escape SOManualDiscMode.RowUpdated
			Discount_Row.Cache.RaiseRowUpdated(discount, old_row);

			decimal auotDocDisc = ARDiscountEngine.GetTotalGroupAndDocumentDiscount(ARDiscountDetails);
			if (auotDocDisc == discount.CuryTranAmt)
			{
				discount.ManualDisc = false;
			}
		}

		private int? GetTaskByAccount(ARTran tran, PMProject project)
		{
			PMAccountTask task = PXSelect<PMAccountTask, Where<PMAccountTask.projectID, Equal<Required<PM.PMAccountTask.projectID>>, And<PMAccountTask.accountID, Equal<Required<PMAccountTask.accountID>>>>>.Select(this, tran.ProjectID, tran.AccountID);
			if (task != null)
			{
				return task.TaskID;
			}
			else
			{
                using (new PXReadDeletedScope())
                {
				Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, tran.AccountID);
				throw new PXException(Messages.AccountMappingNotConfigured, project.ContractCD, ac.AccountCD);
			}
		}
		}

		public object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return this.Caches[BqlCommand.GetItemType(typeof(Field))].GetValue(data, typeof(Field).Name);
		}

		public virtual void DefaultDiscountAccountAndSubAccount(ARTran tran)
		{
			Location customerloc = location.Current;
			//Location companyloc = (Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>, Where<Branch.branchID, Equal<Current<ARRegister.branchID>>>>.Select(this);

			object customer_LocationAcctID = GetValue<Location.cDiscountAcctID>(customerloc);
			//object company_LocationAcctID = GetValue<Location.cDiscountAcctID>(companyloc);


			if (customer_LocationAcctID != null)
			{
				tran.AccountID = (int?)customer_LocationAcctID;
				Discount_Row.Cache.RaiseFieldUpdated<ARTran.accountID>(tran, null);
			}

			if (tran.AccountID != null)
			{
				object customer_LocationSubID = GetValue<Location.cDiscountSubID>(customerloc);
				if (customer_LocationSubID != null)
				{
					tran.SubID = (int?)customer_LocationSubID;
					Discount_Row.Cache.RaiseFieldUpdated<ARTran.subID>(tran, null);
				}
			}
		}

		#region CreditMemo Application

	    [FinPeriodID(
	        branchSourceType: typeof(ARAdjust.adjgBranchID),
	        masterFinPeriodIDType: typeof(ARAdjust.adjgTranPeriodID),
	        headerMasterFinPeriodIDType: typeof(ARInvoice.tranPeriodID))]
        protected virtual void ARAdjust_AdjgFinPeriodID_CacheAttached(PXCache sender)
	    {
	    }

		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		protected virtual void ARAdjust_AdjNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBDate()]
		[PXDBDefault(typeof(ARInvoice.docDate))]
		protected virtual void ARAdjust_AdjgDocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		protected virtual void ARAdjust_CuryAdjgAmt_CacheAttached(PXCache sender)
		{
		}

		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjDiscAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void ARAdjust_CuryAdjgDiscAmt_CacheAttached(PXCache sender)
		{
		}

		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjWOAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void ARAdjust_CuryAdjgWOAmt_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<
				Case<Where<ARAdjust.adjType, Equal<ARAdjust.adjType.adjusted>>, ARAdjust.adjdDocType,
				Case<Where<ARAdjust.adjType, Equal<ARAdjust.adjType.adjusting>>, ARAdjust.adjgDocType>>>))]
		protected virtual void ARAdjust_DisplayDocType_CacheAttached(PXCache sender) {}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<
				Case<Where<ARAdjust.adjType, Equal<ARAdjust.adjType.adjusted>>, ARAdjust.adjdRefNbr,
				Case<Where<ARAdjust.adjType, Equal<ARAdjust.adjType.adjusting>>, ARAdjust.adjgRefNbr>>>))]
		protected virtual void ARAdjust_DisplayRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<ARAdjust.displayRefNbr, Selector<Standalone.ARRegister.customerID, BAccount.acctCD>>))]
		protected virtual void ARAdjust_DisplayCustomerID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<ARAdjust.displayRefNbr, Standalone.ARRegister.docDate>))]
		protected virtual void ARAdjust_DisplayDocDate_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<ARAdjust.displayRefNbr, Standalone.ARRegister.docDesc>))]
		protected virtual void ARAdjust_DisplayDocDesc_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(FormatPeriodID<FormatDirection.display, Selector<ARAdjust.displayRefNbr, Standalone.ARRegister.finPeriodID>>))]
		protected virtual void ARAdjust_DisplayFinPeriodID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<ARAdjust.displayRefNbr, Standalone.ARRegister.curyID>))]
		protected virtual void ARAdjust_DisplayCuryID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<ARAdjust.displayRefNbr, Standalone.ARRegister.status>))]
		protected virtual void ARAdjust_DisplayStatus_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(IIf<Where<ARAdjust.adjType, Equal<ARAdjust.adjType.adjusted>>, ARAdjust.curyAdjgAmt, ARAdjust.curyAdjdAmt>))]
		protected virtual void ARAdjust_DisplayCuryAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[CurrencyInfo]
		[PXFormula(typeof(IIf<Where<ARAdjust.adjType, Equal<ARAdjust.adjType.adjusted>>, ARAdjust.adjgCuryInfoID, ARAdjust.adjdCuryInfoID>))]
		protected virtual void ARAdjust_DisplayCuryInfoID_CacheAttached(PXCache sender) { }

		protected virtual void ARAdjust_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((ARAdjust)e.Row).AdjdCuryInfoID != ((ARAdjust)e.Row).AdjgCuryInfoID && ((ARAdjust)e.Row).AdjdCuryInfoID != ((ARAdjust)e.Row).AdjdOrigCuryInfoID && ((ARAdjust)e.Row).VoidAdjNbr == null)
			{
				foreach (CurrencyInfo info in CurrencyInfo_CuryInfoID.Select(((ARAdjust)e.Row).AdjdCuryInfoID))
				{
					currencyinfo.Delete(info);
				}
			}
		}

		protected virtual void ARAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARAdjust doc = (ARAdjust)e.Row;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			#region Check AdjdRefNbr before save
			// We raising verifying event here to prevent 
			// situations when it is possible to apply the same
			// invoice twice due to read only invoice view.
			// For more details see AC-85468, AC-90600.
			object adjdRefNbr = doc.AdjdRefNbr;

			try
			{
				sender.RaiseFieldVerifying<ARAdjust.adjdRefNbr>(doc, ref adjdRefNbr);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(doc, adjdRefNbr, ex);
			}
			#endregion

			if (Document.Current != null)
			{
				CheckApplicationDateAndPeriod(sender, Document.Current, doc);
			}

			if (doc.CuryDocBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust.curyAdjgAmt>(e.Row, doc.CuryAdjgAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}
		}

		protected virtual void ARAdjust_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				ARAdjust adj = (ARAdjust)e.Row;
				if (adj.AdjdCuryInfoID == null)
				{
					foreach (PXResult<ARInvoice, CurrencyInfo> res in PXSelectJoin<ARInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						ARAdjust_AdjdRefNbr_FieldUpdated<ARInvoice>(sender, res, adj);
						return;
					}
				}
			}
			catch (PXSetPropertyException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		private void ARAdjust_AdjdRefNbr_FieldUpdated<T>(PXCache cache, PXResult<T, CurrencyInfo> res, ARAdjust adj)
			where T : ARRegister, IInvoice, new()
		{
			CurrencyInfo info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
			info_copy.CuryInfoID = null;
			info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);
			T invoice = (T)res;

			//currencyinfo.Cache.SetValueExt<CurrencyInfo.curyEffDate>(info_copy, Document.Current.DocDate);
			info_copy.SetCuryEffDate(currencyinfo.Cache, Document.Current.DocDate);

			adj.CustomerID = Document.Current.CustomerID;
			adj.AdjgDocDate = Document.Current.DocDate;
			adj.AdjgCuryInfoID = Document.Current.CuryInfoID;
			adj.AdjdCuryInfoID = info_copy.CuryInfoID;
			adj.AdjdCustomerID = invoice.CustomerID;
			adj.AdjdOrigCuryInfoID = invoice.CuryInfoID;
			adj.AdjdBranchID = invoice.BranchID;
			adj.AdjdARAcct = invoice.ARAccountID;
			adj.AdjdARSub = invoice.ARSubID;
			adj.AdjdDocDate = invoice.DocDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<ARAdjust.adjdFinPeriodID>(cache, adj, invoice.TranPeriodID);
			adj.Released = false;

			CalcBalances<T>(adj, invoice, false, true);

			decimal? CuryApplAmt = adj.CuryDocBal;
			//TODO: accumulate Unapplied Balance
			decimal? CuryUnappliedBal = 0m;

			if (Document.Current != null && CuryUnappliedBal > 0m)
			{
				CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);
			}
			else if (Document.Current != null && CuryUnappliedBal <= 0m && Document.Current.CuryOrigDocAmt > 0)
			{
				CuryApplAmt = 0m;
			}

			adj.CuryAdjgAmt = CuryApplAmt;
			adj.CuryAdjgDiscAmt = 0m;
			adj.CuryAdjgWOAmt = 0m;

			CalcBalances<T>(adj, invoice, true, true);

		    PXCache<ARAdjust>.SyncModel(adj);
		}

		protected bool internalCall;

		protected virtual void ARAdjust_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			ARAdjust adj = e.Row as ARAdjust;

			if (!internalCall)
			{
				if (e.Row != null && adj.AdjdCuryInfoID != null && adj.CuryDocBal == null && sender.GetStatus(adj) != PXEntryStatus.Deleted)
				{
					CalcBalances(adj, false, false);
				}

				if (adj != null)
				{
					e.NewValue = adj.CuryDocBal;
				}
			}

			e.Cancel = true;
		}

		protected virtual void ARAdjust_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust adj = (ARAdjust)e.Row;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
			{
				CalcBalances(adj, false, false);
			}

			if (adj.CuryDocBal == null)
			{
				throw new PXSetPropertyException<ARAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust.adjdRefNbr>(sender));
			}

			if ((decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
			}
		}

		protected virtual void ARAdjust_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances((ARAdjust)e.Row, true, false);
		}

		protected void CalcBalances(ARAdjust adj, bool isCalcRGOL, bool DiscOnDiscDate)
		{
			foreach (ARInvoice invoice in ARInvoice_CustomerID_DocType_RefNbr.Select(adj.AdjdCustomerID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
				CalcBalances(adj, invoice, isCalcRGOL, DiscOnDiscDate);
				return;
			}
		}

		protected void CalcBalances<T>(ARAdjust adj, T invoice, bool isCalcRGOL, bool DiscOnDiscDate)
			where T : class, IBqlTable, IInvoice, new()
		{
			PaymentEntry.CalcBalances(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj);
			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount(adj.AdjgDocDate, invoice, adj);
			}

			adj.CuryWhTaxBal = 0m;
			adj.WhTaxBal = 0m;

			invoice.CuryWhTaxBal = 0m;
			invoice.WhTaxBal = 0m;

			PaymentEntry.AdjustBalance(CurrencyInfo_CuryInfoID, adj);
			if (isCalcRGOL && (adj.Voided != true))
			{
				PaymentEntry.CalcRGOL(CurrencyInfo_CuryInfoID, invoice, adj);
				adj.RGOLAmt = adj.ReverseGainLoss == true ? -1m * adj.RGOLAmt : adj.RGOLAmt;
			}
		}
		#endregion

		#region Project Related Methods [Obsolete] moved to ARInvoiceEntryExt
		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public virtual int? GetProjectedAccountGroup(ARTran line)
		{
			//TODO Remove in 2020R1 (0 references)
			
			int? projectedRevenueAccountGroupID = null;
			int? projectedRevenueAccount = line.AccountID;

			if (line.AccountID != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<ARTran.accountID>(Transactions.Cache, line, line.AccountID) as Account;
				if (revenueAccount != null)
				{
					if (revenueAccount.AccountGroupID == null)
						throw new PXException(PM.Messages.RevenueAccountIsNotMappedToAccountGroup, revenueAccount.AccountCD);

					projectedRevenueAccountGroupID = revenueAccount.AccountGroupID;
				}
			}

			return projectedRevenueAccountGroupID;
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public virtual void AddToInvoiced(ARTran line, int? revenueAccountGroup, int mult = 1)
		{
			//TODO Remove in 2020R1 (0 references)	

			if (line.TaskID == null)
				return;

			if (revenueAccountGroup == null)
				return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, line.ProjectID);
			if (project != null && project.NonProject != true)
			{
				int? inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				int? costCodeID = line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
				if (project.BudgetLevel == BudgetLevels.Item)
				{
					var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>,
									And<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>>>>>(this);
					var revenueBudget = selectRevenueBudget.Select(line.ProjectID, line.TaskID);

					foreach (PMRevenueBudget budget in revenueBudget)
					{
						if (budget.TaskID == line.TaskID && line.InventoryID == budget.InventoryID)
						{
							inventoryID = line.InventoryID;
						}
					}

				}
				else if (project.BudgetLevel == BudgetLevels.Task)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
				}

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = GL.AccountType.Income;
				invoiced.ProjectID = line.ProjectID;
				invoiced.ProjectTaskID = line.TaskID;
				invoiced.AccountGroupID = revenueAccountGroup;
				invoiced.InventoryID = inventoryID;
				invoiced.CostCodeID = costCodeID;

				invoiced = Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryInvoicedAmount += mult * (line.CuryTranAmt.GetValueOrDefault() + line.CuryRetainageAmt.GetValueOrDefault());
					invoiced.InvoicedAmount += mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
				else
				{
					invoiced.CuryInvoicedAmount += mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
					invoiced.InvoicedAmount += mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public virtual void SubtractAmountToInvoice(ARTran line, int? revenueAccountGroup, int mult = 1)
		{
			//TODO Remove in 2020R1 (0 references)
			if (line.TaskID == null)
				return;

			if (revenueAccountGroup == null)
				return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, line.ProjectID);
			if (project != null && project.NonProject != true)
			{
				int? inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				int? costCodeID = line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
				if (project.BudgetLevel == BudgetLevels.Item)
				{
					var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>,
									And<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>>>>>(this);
					var revenueBudget = selectRevenueBudget.Select(line.ProjectID, line.TaskID);

					foreach (PMRevenueBudget budget in revenueBudget)
					{
						if (budget.TaskID == line.TaskID && line.InventoryID == budget.InventoryID)
						{
							inventoryID = line.InventoryID;
						}
					}

				}
				else if (project.BudgetLevel == BudgetLevels.Task)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
				}

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = GL.AccountType.Income;
				invoiced.ProjectID = line.ProjectID;
				invoiced.ProjectTaskID = line.TaskID;
				invoiced.AccountGroupID = revenueAccountGroup;
				invoiced.InventoryID = inventoryID;
				invoiced.CostCodeID = costCodeID;
				invoiced.CuryInfoID = line.CuryInfoID;

				invoiced = Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryAmountToInvoice -= mult * (line.CuryTranAmt.GetValueOrDefault() + line.CuryRetainageAmt.GetValueOrDefault());
					invoiced.AmountToInvoice -= mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
				else
				{
					invoiced.CuryAmountToInvoice -= mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
					invoiced.AmountToInvoice -= mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
			}
		}
		#endregion

		#region PPDCreditMemo

		public virtual ARInvoice CreatePPDCreditMemo(ARPPDCreditMemoParameters filter, List<PendingPPDCreditMemoApp> list, ref int index)
		{
			bool firstApp = true;
			ARInvoice invoice = (ARInvoice)Document.Cache.CreateInstance();

			foreach (PendingPPDCreditMemoApp doc in list)
			{
				if (firstApp)
				{
					firstApp = false;
                    index = doc.Index.Value;

					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, doc.InvCuryInfoID);
					info.CuryInfoID = null;
					info = currencyinfo.Insert(info);

					invoice.DocType = ARDocType.CreditMemo;
					invoice.DocDate = filter.GenerateOnePerCustomer == true ? filter.CreditMemoDate : doc.AdjgDocDate;
					invoice.BranchID = doc.AdjdBranchID;

					string masterPeriodID = filter.GenerateOnePerCustomer == true
						? FinPeriodRepository.GetByID(filter.FinPeriodID, PXAccess.GetParentOrganizationID(filter.BranchID)).MasterFinPeriodID
						: doc.AdjgTranPeriodID;

					FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(Document.Cache, invoice, masterPeriodID);

					invoice = PXCache<ARInvoice>.CreateCopy(Document.Insert(invoice));

					invoice.CustomerID = doc.AdjdCustomerID;
					invoice.CustomerLocationID = doc.InvCustomerLocationID;
					invoice.CuryInfoID = info.CuryInfoID;
					invoice.CuryID = info.CuryID;
					Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
							.Select(this, doc.CustomerID);
					invoice.DocDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(ARSetup)], ARSetup.Current, nameof(AR.ARSetup.pPDCreditMemoDescr), customer?.LocaleName);
					invoice.ARAccountID = doc.AdjdARAcct;
					invoice.ARSubID = doc.AdjdARSub;
					invoice.TaxZoneID = doc.InvTaxZoneID;
					invoice.Hold = false;
					invoice.PendingPPD = true;

					invoice = Document.Update(invoice);

					invoice.DontPrint = true;
					invoice.DontEmail = true;
				}

				AddTaxesAndApplications(doc);
			}

			ARDiscountDetails.Select().RowCast<ARInvoiceDiscountDetail>().ForEach(discountDetail => ARDiscountDetails.Cache.Delete(discountDetail));

			if (ARSetup.Current.RequireControlTotal == true)
			{
				invoice.CuryOrigDocAmt = invoice.CuryDocBal;
				Document.Cache.Update(invoice);
			}

			Save.Press();
			string refNbr = invoice.RefNbr;
			list.ForEach(doc => PXUpdate<Set<ARAdjust.pPDCrMemoRefNbr, Required<ARAdjust.pPDCrMemoRefNbr>>, ARAdjust,
					Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
						And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
							And<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
								And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
									And<ARAdjust.released, Equal<True>,
										And<ARAdjust.voided, NotEqual<True>,
											And<ARAdjust.pendingPPD, Equal<True>>>>>>>>>
					.Update(this, refNbr, doc.AdjdDocType, doc.AdjdRefNbr, doc.AdjgDocType, doc.AdjgRefNbr));

			return invoice;
		}

		private static readonly Dictionary<string, string> DocTypes = new ARInvoiceType.AdjdListAttribute().ValueLabelDic;

		public virtual void AddTaxesAndApplications(PendingPPDCreditMemoApp doc)
		{
			ARTaxTran artaxMax = null;
			decimal? TaxTotal = 0m;
			decimal? InclusiveTotal = 0m;
			decimal? DiscountedTaxableTotal = 0m;
			decimal? DiscountedPriceTotal = 0m;
			decimal CashDiscPercent = (decimal)(doc.CuryAdjdPPDAmt / doc.InvCuryOrigDocAmt);

			PXResultset<ARTaxTran> taxes = PXSelectJoin<ARTaxTran,
				InnerJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>,
				Where<ARTaxTran.module, Equal<BatchModule.moduleAR>,
					And<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>,
					And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>,
					And<Tax.taxApplyTermsDisc, Equal<CSTaxTermsDiscount.toPromtPayment>>>>>>
				.Select(this, doc.AdjdDocType, doc.AdjdRefNbr);

			//add taxes
			foreach (PXResult<ARTaxTran, Tax> res in taxes)
			{
				Tax tax = res;
				ARTaxTran artax = PXCache<ARTaxTran>.CreateCopy(res);
				ARTaxTran artaxNew = Taxes.Search<ARTaxTran.taxID>(artax.TaxID);

				if (artaxNew == null)
				{
					artax.TranType = null;
					artax.RefNbr = null;
					artax.TaxPeriodID = null;
				    artax.FinPeriodID = null;
					artax.Released = false;
					artax.Voided = false;
					artax.CuryInfoID = Document.Current.CuryInfoID;

					TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.NoCalc);
					artaxNew = Taxes.Insert(artax);

					artaxNew.CuryTaxableAmt = 0m;
					artaxNew.CuryTaxAmt = 0m;
					artaxNew.TaxRate = artax.TaxRate;
				}

				bool isTaxable = ARPPDCreditMemoProcess.CalculateDiscountedTaxes(Taxes.Cache, artax, CashDiscPercent);
				DiscountedPriceTotal += artax.CuryDiscountedPrice;

				decimal? CuryTaxableAmt = artax.CuryTaxableAmt - artax.CuryDiscountedTaxableAmt;
				decimal? CuryTaxAmt = artax.CuryTaxAmt - artax.CuryDiscountedPrice;

				artaxNew.CuryTaxableAmt += CuryTaxableAmt;
				artaxNew.CuryTaxAmt += CuryTaxAmt;

				TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualCalc);
				Taxes.Update(artaxNew);

				if (isTaxable)
				{
					DiscountedTaxableTotal += artax.CuryDiscountedTaxableAmt;
					if (artaxMax == null || artaxNew.CuryTaxableAmt > artaxMax.CuryTaxableAmt)
					{
						artaxMax = artaxNew;
					}
				}

				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive) { InclusiveTotal += CuryTaxAmt; }
				else { TaxTotal += CuryTaxAmt; }
			}

			//adjust taxes according to parent ARInvoice
			decimal? DiscountedInvTotal = doc.InvCuryOrigDocAmt - doc.InvCuryOrigDiscAmt;
			decimal? DiscountedDocTotal = DiscountedTaxableTotal + DiscountedPriceTotal;

			if (doc.InvCuryOrigDiscAmt == doc.CuryAdjdPPDAmt &&
			    artaxMax != null &&
			    doc.InvCuryVatTaxableTotal + doc.InvCuryTaxTotal == doc.InvCuryOrigDocAmt &&
			    DiscountedDocTotal != DiscountedInvTotal)
			{
				artaxMax.CuryTaxableAmt += DiscountedDocTotal - DiscountedInvTotal;
				TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualCalc);
				Taxes.Update(artaxMax);
			}

			//add document details
			AddPPDCreditMemoDetails(doc, TaxTotal, InclusiveTotal, taxes);

			//add applications
			ARAdjust adj = new ARAdjust();
			adj.AdjdDocType = doc.AdjdDocType;
			adj.AdjdRefNbr = doc.AdjdRefNbr;
			adj = Adjustments_1.Insert(adj);

			adj.CuryAdjgAmt = doc.InvCuryDocBal;
			Adjustments_1.Update(adj);
		}

		public virtual void AddPPDCreditMemoDetails(PendingPPDCreditMemoApp doc, decimal? TaxTotal, decimal? InclusiveTotal, PXResultset<ARTaxTran> taxes)
		{
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, doc.AdjdCustomerID);
			ARTran tranNew = Transactions.Insert();

			tranNew.BranchID = doc.AdjdBranchID;
			using (new PXLocaleScope(customer.LocaleName))
				tranNew.TranDesc = string.Format("{0} {1}, {2} {3}", PXMessages.LocalizeNoPrefix(DocTypes[doc.AdjdDocType]), doc.AdjdRefNbr, PXMessages.LocalizeNoPrefix(Messages.Payment), doc.AdjgRefNbr);
			tranNew.CuryExtPrice = doc.CuryAdjdPPDAmt - TaxTotal;
			tranNew.CuryTaxableAmt = tranNew.CuryExtPrice - InclusiveTotal;
			tranNew.CuryTaxAmt = TaxTotal + InclusiveTotal;
			tranNew.AccountID = customer.DiscTakenAcctID;
			tranNew.SubID = customer.DiscTakenSubID;
			tranNew.TaxCategoryID = null;
			tranNew.IsFree = true;
			tranNew.ManualDisc = true;
			tranNew.CuryDiscAmt = 0m;
			tranNew.DiscPct = 0m;
			tranNew.GroupDiscountRate = 1m;
			tranNew.DocumentDiscountRate = 1m;

			if (taxes.Count == 1)
			{
				ARTaxTran artax = taxes[0];
				ARTran artran = PXSelectJoin<ARTran,
					InnerJoin<ARTax, On<ARTax.tranType, Equal<ARTran.tranType>,
						And<ARTax.refNbr, Equal<ARTran.refNbr>,
							And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>>,
					Where<ARTax.tranType, Equal<Required<ARTax.tranType>>,
						And<ARTax.refNbr, Equal<Required<ARTax.refNbr>>,
							And<ARTax.taxID, Equal<Required<ARTax.taxID>>>>>,
					OrderBy<Asc<ARTran.lineNbr>>>.SelectSingleBound(this, null, artax.TranType, artax.RefNbr, artax.TaxID);
				if (artran != null)
				{
					tranNew.TaxCategoryID = artran.TaxCategoryID;
				}
			}

			Transactions.Update(tranNew);
		}

		#endregion

	}
	
    public class ARInvoiceEntryProjectFieldVisibilityGraphExtension : PXGraphExtension<ARInvoiceEntry>
    {
        protected virtual void _(Events.RowSelected<ARInvoice> e)
        {
            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<ARInvoice.projectID>(e.Cache, e.Row, 
                PXAccess.FeatureInstalled<FeaturesSet.contractManagement>() || PM.ProjectAttribute.IsPMVisible(BatchModule.AR));
            PXUIFieldAttribute.SetDisplayName<ARInvoice.projectID>(e.Cache, GL.Messages.ProjectContract);
        }
    }

	[Serializable]
	public partial class DuplicateFilter : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected string _RefNbr;
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "New Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RefNbr
			{
			get { return this._RefNbr; }
			set { this._RefNbr = value; }
		}
		#endregion
	}

    //proposal for inherited mapping
    //public class SalesPrice : SalesPriceGraph<ARInvoiceEntry, ARInvoice>
    //{
    //    public class TDocument : Extensions.SalesPrice.Document
    //    {
    //        public abstract class fml : PX.Data.BQL.BqlString.Field<fml>
    //        {
    //        }
    //        public virtual string FML { get; set; }
    //    }
    //    protected class TDocumentMapping : DocumentMapping
    //    {
    //        public TDocumentMapping(Type table) : base(table)
    //        {
    //            _extension = typeof(TDocument);
    //        }

    //        public Type FML = typeof(TDocument.fml);
    //    }
    //    protected override DocumentMapping GetDocumentMapping()
    //    {
    //        return new TDocumentMapping(typeof(ARInvoice)) { FML = typeof(ARInvoice.drCr) };
    //    }
    //    //protected override DocumentMapping GetDocumentMapping()
    //    //{
    //    //    return new DocumentMapping(typeof(ARInvoice));
    //    //}
    //    protected override DetailMapping GetDetailMapping()
    //    {
    //        return new DetailMapping(typeof(ARTran)) { Descr = typeof(ARTran.tranDesc), Quantity = typeof(ARTran.qty) };
    //    }
    //    protected override PriceClassSourceMapping GetPriceClassSourceMapping()
    //    {
    //        return new PriceClassSourceMapping(typeof(Location)) { PriceClassID = typeof(Location.cPriceClassID) };
    //    }

    //    protected virtual void _(Events.RowSelected<TDocument> e)
    //    {

    //    }
    //}
}
