using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;

using PX.Data;

using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.AP.MigrationMode;
using PX.Objects.Common.Utility;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APPayBills : PXGraph<APPayBills>
	{
		public PXFilter<PayBillsFilter> Filter;
		public PXCancel<PayBillsFilter> Cancel;
		public PXAction<PayBillsFilter> ViewDocument;

		[PXFilterable]
		public PXFilteredProcessingJoin<APAdjust, PayBillsFilter, 
			InnerJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
	        LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
	                And<APTran.tranType, Equal<APAdjust.adjdDocType>,
	                And<APTran.refNbr, Equal<APAdjust.adjdRefNbr>,
	                And<APTran.lineNbr, Equal<APAdjust.adjdLineNbr>>>>>>>> APDocumentList;

		public PXSelectJoin<APAdjust, 
			InnerJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
					And<APTran.tranType, Equal<APAdjust.adjdDocType>,
					And<APTran.refNbr, Equal<APAdjust.adjdRefNbr>,
					And<APTran.lineNbr, Equal<APAdjust.adjdLineNbr>>>>>>>> APExceptionsList;

		public ToggleCurrency<PayBillsFilter> CurrencyView;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		public PXSelect<APInvoice> Invoice;
		
		#region Setups
		public PXSetup<GL.Company> Company;

		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<PayBillsFilter.payAccountID>>>> cashaccount;

		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Current<PayBillsFilter.payAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Current<PrintChecksFilter.payTypeID>>>>> cashaccountdetail;

		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PayBillsFilter.payTypeID>>>> paymenttype;

		public APSetupNoMigrationMode APSetup;

		#endregion
		
        [InjectDependency]
        public IFinPeriodRepository FinPeriodRepository { get; set; }

		public APPayBills()
		{
			APSetup setup = APSetup.Current;

			APDocumentList.SetSelected<APAdjust.selected>();
			APDocumentList.SetProcessCaption(Messages.Process);
			APDocumentList.SetProcessAllCaption(Messages.ProcessAll);

			APDocumentList.Cache.AllowInsert = true;
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdDocType>(APDocumentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdRefNbr>(APDocumentList.Cache, null, true);
		    PXUIFieldAttribute.SetEnabled<APAdjust.adjdLineNbr>(APDocumentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgAmt>(APDocumentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgDiscAmt>(APDocumentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APAdjust.separateCheck>(APDocumentList.Cache, null, true);

			PXUIFieldAttribute.SetVisible<PayBillsFilter.curyID>(Filter.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			this.APExceptionsList.AllowInsert = false;
			this.APExceptionsList.AllowUpdate = false;
			this.APExceptionsList.AllowDelete = false;

			PXUIFieldAttribute.SetDisplayName<APInvoice.origRefNbr>(Caches[typeof(APInvoice)], Messages.OrigRefNbr);
		}

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);			
			Caches.AddCacheMappingsWithInheritance(this, typeof(Vendor));						
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		public override void Clear()
		{
			Filter.Current.CuryInfoID = null;
			Filter.Current.CurySelTotal = 0m;
			Filter.Current.SelTotal = 0m;
			Filter.Current.SelCount = 0;
			base.Clear();
		}
		public PXAction<PayBillsFilter> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (APExceptionsList.Current != null)
			{
				APInvoice invoice = PXSelectJoin<APInvoice,
						InnerJoin<APAdjust,
							On<Current<APAdjust.adjdDocType>, Equal<APInvoice.docType>,
								And<Current<APAdjust.adjdRefNbr>, Equal<APInvoice.refNbr>>>>>.SelectSingleBound(this, null);

				APInvoiceEntry graph = PXGraph.CreateInstance<APInvoiceEntry>();
				PXRedirectHelper.TryRedirect(graph, invoice, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<PayBillsFilter> viewOriginalDocument;
		[PXButton]
		public virtual IEnumerable ViewOriginalDocument(PXAdapter adapter)
		{
			if (APDocumentList.Current != null)
			{
				APInvoice invoice = PXSelectJoin<APInvoice,
						InnerJoin<APAdjust,
							On<Current<APAdjust.adjdDocType>, Equal<APInvoice.docType>,
								And<Current<APAdjust.adjdRefNbr>, Equal<APInvoice.refNbr>>>>>.SelectSingleBound(this, null);

				RedirectionToOrigDoc.TryRedirect(invoice.OrigDocType, invoice.OrigRefNbr, invoice.OrigModule);
			}
			return adapter.Get();
		}
		
        protected virtual void PayBillsFilter_Days_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            e.ReturnValue = PXLocalizer.Localize(e.ReturnValue as string);
        }

		protected virtual void PayBillsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null && cashaccount.Current != null && object.Equals(cashaccount.Current.CashAccountID, ((PayBillsFilter)e.Row).PayAccountID) == false)
			{
				cashaccount.Current = null;
			}

			if (e.Row != null && cashaccountdetail.Current != null && (object.Equals(cashaccountdetail.Current.CashAccountID, ((PayBillsFilter)e.Row).PayAccountID) == false || object.Equals(cashaccountdetail.Current.PaymentMethodID, ((PayBillsFilter)e.Row).PayTypeID) == false))
			{
				cashaccountdetail.Current = null;
			}

			if (e.Row != null && paymenttype.Current != null && (object.Equals(paymenttype.Current.PaymentMethodID, ((PayBillsFilter)e.Row).PayTypeID) == false))
			{
				paymenttype.Current = null;
			}

			PayBillsFilter filter = e.Row as PayBillsFilter;
			if (filter != null)
			{
				CurrencyInfo info = CurrencyInfo_CuryInfoID.Select(filter.CuryInfoID);
				PaymentMethod paytype = paymenttype.Current;
				APDocumentList.SetProcessDelegate(
					delegate(List<APAdjust> list)
					{
						var graph = PXGraph.CreateInstance<APPayBills>();
						graph.CreatePayments(list, filter, info, paytype);
					}
				);
			}

			PXUIFieldAttribute.SetDisplayName<APAdjust.vendorID>(APDocumentList.Cache, Messages.VendorID);
			PXUIFieldAttribute.SetVisible<APAdjust.vendorID>(APDocumentList.Cache,null,true);

			bool errorsOnForm = PXUIFieldAttribute.GetErrors(sender, null, PXErrorLevel.Error, PXErrorLevel.RowError).Count > 0;

			APDocumentList.SetProcessEnabled(!errorsOnForm);
			APDocumentList.SetProcessAllEnabled(!errorsOnForm);
		}
		protected virtual void APAdjust_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			if (string.IsNullOrEmpty(adj.AdjdRefNbr))
			{
				e.Cancel = true;
			    return;
			}

			bool manual = adj.VendorID == null;


			foreach (PXResult<APInvoice, APTran, CurrencyInfo> res in
                PXSelectJoin<APInvoice,
                    LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
                        And<APTran.tranType, Equal<APInvoice.docType>,
                        And<APTran.refNbr, Equal<APInvoice.refNbr>,
                        And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>,
                    InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>>,
                Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
                    And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(this, adj.AdjdLineNbr, adj.AdjdDocType, adj.AdjdRefNbr))
            {
                CurrencyInfo info = res;
                CurrencyInfo info_copy = null;
                APInvoice invoice = res;
                APTran tran = res;
                if (invoice.PaymentsByLinesAllowed != true)
                {
                    adj.AdjdLineNbr = 0;
                }
                else if(adj.AdjdLineNbr == 0)
                {
                    adj.AdjdLineNbr = null;
                }

                if(adj.AdjdLineNbr == null)
                {
                    e.Cancel = true;
                    return;
                }

				//place select tail in cache to merge correctly
				CurrencyInfo_CuryInfoID.Cache.SetStatus(info, PXEntryStatus.Notchanged);

                try
                {
                    info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
                    info_copy.CuryInfoID = null;
                    info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);

                    info_copy.SetCuryEffDate(currencyinfo.Cache, Filter.Current.PayDate);
                }
                catch (PXRateIsNotDefinedForThisDateException ex)
                {
                    APDocumentList.Cache.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(adj, 0m, new PXSetPropertyException(ex.Message, PXErrorLevel.RowError));
                }

                adj.VendorID = invoice.VendorID;
                adj.AdjgDocDate = Filter.Current.PayDate;
                adj.AdjgFinPeriodID = Filter.Current.PayFinPeriodID;
                adj.AdjgCuryInfoID = Filter.Current.CuryInfoID;
                adj.AdjdCuryInfoID = info_copy.CuryInfoID;
                adj.AdjdOrigCuryInfoID = info.CuryInfoID;
                adj.AdjdBranchID = invoice.BranchID;
                adj.AdjdAPAcct = invoice.APAccountID;
                adj.AdjdAPSub = invoice.APSubID;
                adj.AdjdDocDate = invoice.DocDate;
                adj.AdjdFinPeriodID = invoice.FinPeriodID;
                adj.Released = false;
				adj.SeparateCheck = adj.SeparateCheck ?? invoice.SeparateCheck;

                CurrencyInfo_CuryInfoID.View.Clear();
                CurrencyInfo_CuryInfoID.StoreCached(new PXCommandKey(new object[] { adj.AdjdOrigCuryInfoID }, null, null, null, 0,1, null, false,null), new List<object> { info });
                CurrencyInfo_CuryInfoID.StoreCached(new PXCommandKey(new object[] { adj.AdjdCuryInfoID }, null, null, null, 0, 1, null, false, null), new List<object> { info_copy });

                CalcBalances(adj, invoice, tran, false);
                if (manual)
                {
	                adj.Selected = true;
					// Acuminator disable once PX1048 RowChangesInEventHandlersAllowedForArgsOnly [Required fo correct merge cache]
					invoice.ManualEntry = true;
	                Invoice.Cache.SetStatus(invoice, PXEntryStatus.Inserted);
                }

				if (adj.CuryWhTaxBal >= 0m && adj.CuryDiscBal >= 0m && adj.CuryDocBal - adj.CuryWhTaxBal - adj.CuryDiscBal <= 0m)
                {
                    //no amount suggestion is possible
                    break;
                }
				if(adj.CuryAdjgAmt == 0)				
					adj.CuryAdjgAmt = adj.CuryDocBal - adj.CuryWhTaxBal - adj.CuryDiscBal;

				if(adj.CuryAdjgDiscAmt == 0)
					adj.CuryAdjgDiscAmt = adj.CuryDiscBal;	            

				adj.CuryAdjgWhTaxAmt = adj.CuryWhTaxBal;

                CalcBalances(adj, invoice, tran, true);
                break;
            }

		}

		private bool CheckIfRowNotApprovedForPayment(APAdjust row) 
		{
			if (APSetup.Current.RequireApprovePayments != true)
				return false;

			var invoice = (APInvoice)PXSelectorAttribute.Select<APAdjust.adjdRefNbr>(APDocumentList.Cache, row);
			return invoice != null && !(invoice.DocType == APDocType.DebitAdj || invoice.PaySel == true);
		}

		protected virtual void APAdjust_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || PXLongOperation.Exists(UID)) return;
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdDocType>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdRefNbr>(sender, e.Row, false);
		    PXUIFieldAttribute.SetEnabled<APAdjust.adjdLineNbr>(sender, e.Row, false);
			if (CheckIfRowNotApprovedForPayment((APAdjust)e.Row))
				APDocumentList.Cache.RaiseExceptionHandling<APAdjust.curyAdjgAmt>((APAdjust)e.Row, 0m, new PXSetPropertyException(Messages.DocumentNotApprovedNotProceed, PXErrorLevel.RowWarning));

			PXException ex = null;
			if (APSetup.Current.EarlyChecks == false && ((APAdjust)e.Row).AdjdDocDate > Filter.Current.PayDate)
			{
				ex = new PXSetPropertyException(Messages.ApplDate_Less_DocDate, PXErrorLevel.RowWarning, PXUIFieldAttribute.GetDisplayName<PayBillsFilter.payDate>(Filter.Cache));
			}

			if (APSetup.Current.EarlyChecks == false && !string.IsNullOrEmpty(Filter.Current.PayFinPeriodID) && string.Compare(((APAdjust)e.Row).AdjdFinPeriodID, Filter.Current.PayFinPeriodID) > 0)
			{
				ex = new PXSetPropertyException(Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowWarning, PXUIFieldAttribute.GetDisplayName<PayBillsFilter.payFinPeriodID>(Filter.Cache));
			}

			sender.RaiseExceptionHandling<APAdjust.selected>(e.Row, false, ex);
			PXUIFieldAttribute.SetEnabled<APAdjust.selected>(sender, e.Row, ex == null && ((APAdjust)e.Row).CuryDocBal != null && !string.IsNullOrEmpty(Filter.Current.PayFinPeriodID));
			
		}

		Dictionary<object, object> _copies = new Dictionary<object, object>();

		protected virtual IEnumerable aPExceptionsList()
		{
			PayBillsFilter filter = Filter.Current;
			PXResultMapper mapper = new PXResultMapper(this,
					new Dictionary<Type, Type>()
					{
						{typeof(APAdjust.adjdDocType), typeof(APInvoice.docType)},
						{typeof(APAdjust.adjdRefNbr), typeof(APInvoice.refNbr)},
						{typeof(APAdjust.adjdLineNbr), typeof(APTran.lineNbr)},
					},
					typeof(APAdjust), typeof(APInvoice), typeof(APTran)
				);

			mapper.ExtFilters.AddRange(new Type[]
			{
				typeof(APAdjust.separateCheck),
				typeof(APAdjust.curyAdjgAmt),
				typeof(APAdjust.curyDocBal),
				typeof(APAdjust.curyAdjgDiscAmt),
				typeof(APAdjust.curyDiscBal)
			});
			mapper.SuppressSorts.AddRange(new Type[]
			{
				typeof(APAdjust.adjgDocType),
				typeof(APAdjust.adjgRefNbr),
				typeof(APAdjust.adjNbr)
			});
			bool extSorts = mapper.SortColumns.Any(field => mapper.ExtFilters.Contains(APDocumentList.Cache.GetBqlField(field)));
			PXDelegateResult result = mapper.CreateDelegateResult(!extSorts);

			if (filter?.PayDate == null || filter.PayTypeID == null || filter.PayAccountID == null)
				return result;

			DateTime PayInLessThan = ((DateTime)filter.PayDate).AddDays(filter.PayInLessThan.GetValueOrDefault());
			DateTime DueInLessThan = ((DateTime)filter.PayDate).AddDays(filter.DueInLessThan.GetValueOrDefault());
			DateTime DiscountExpiresInLessThan = ((DateTime)filter.PayDate).AddDays(filter.DiscountExpiresInLessThan.GetValueOrDefault());

			int startRow = result.IsResultTruncated ? PXView.StartRow : 0;
			int maxRow = result.IsResultTruncated ? PXView.MaximumRows : 0;
			int totalRows = 0;

			PXSelectBase<APInvoice> cmd = new PXSelectJoin<
				APInvoice,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
					InnerJoin<Vendor,
						On<Vendor.bAccountID, Equal<APInvoice.vendorID>,
						And<Where<
							Vendor.status, Equal<BAccount.status.active>,
							Or<Vendor.status, Equal<BAccount.status.oneTime>>>>>,
					InnerJoin<APAdjust,
						On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
						And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
						And<APAdjust.released, Equal<False>>>>,
					LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
						And<APTran.tranType, Equal<APInvoice.docType>,
						And<APTran.refNbr, Equal<APInvoice.refNbr>,
						And<APTran.lineNbr, Equal<APAdjust.adjdLineNbr>>>>>,
					InnerJoin<APPayment,
						On<APPayment.docType, Equal<APAdjust.adjgDocType>,
						And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>>>>>>,
				Where<
					APInvoice.openDoc, Equal<True>,
					And2<Where<
						APInvoice.released, Equal<True>,
						Or<APInvoice.prebooked, Equal<True>>>,
					And2<Where<
						APInvoice.paySel, Equal<True>,
						Or2<Where<Current<APSetup.requireApprovePayments>, Equal<False>>,
						Or<APInvoice.docType, Equal<APDocType.debitAdj>>>>,
					And2<Where<
						APInvoice.vendorID, Equal<Current<PayBillsFilter.vendorID>>,
						Or<Current<PayBillsFilter.vendorID>, IsNull>>,
					And<APInvoice.payAccountID, Equal<Current<PayBillsFilter.payAccountID>>,
					And<APInvoice.payTypeID, Equal<Current<PayBillsFilter.payTypeID>>,
					And2<Where<APPayment.status, Equal<APDocStatus.hold>,
								Or2<Where<APPayment.status, Equal<APDocStatus.pendingApproval>>,
								Or<APPayment.status, Equal<APDocStatus.rejected>>>>,
					And2<Where2<Where2<
						Where<
							Current<PayBillsFilter.showPayInLessThan>, Equal<True>,
							And<APInvoice.payDate, LessEqual<Required<APInvoice.payDate>>>>,
						Or2<Where<
							Current<PayBillsFilter.showDueInLessThan>, Equal<True>,
							And<Where<
								APInvoice.dueDate, LessEqual<Required<APInvoice.dueDate>>,
								Or<APInvoice.dueDate, IsNull>>>>,
						Or<Where<
							Current<PayBillsFilter.showDiscountExpiresInLessThan>, Equal<True>,
							And<Where<
								APInvoice.discDate, LessEqual<Required<APInvoice.discDate>>,
								Or<APInvoice.discDate, IsNull>>>>>>>,
						Or<Where<
							Current<PayBillsFilter.showPayInLessThan>, Equal<False>,
							And<Current<PayBillsFilter.showDueInLessThan>, Equal<False>,
							And<Current<PayBillsFilter.showDiscountExpiresInLessThan>, Equal<False>>>>>>,
					And<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>>>
					(this);
			object[] searches = mapper.Searches;
			string[] sorts = mapper.SortColumns;
			int index = sorts.FindIndex(field => field == typeof(APTran).Name + "__" + typeof(APTran.lineNbr).Name);
			if(index > 0 && (int?)searches[index] == 0)
			{
				searches[index] = null;
			}
			
			List<object> invoices = cmd.View.Select(new[] { filter },
				new[] { PayInLessThan, DueInLessThan, (object)DiscountExpiresInLessThan },
				searches,
				sorts,
				mapper.Descendings,
				mapper.Filters,
				ref startRow,
				maxRow,
				ref totalRows);
			foreach (PXResult<APInvoice, CurrencyInfo, Vendor, APAdjust, APTran> res in invoices)
			{
				APInvoice doc = res;
				APTran tran = res;
				APAdjust adj = new APAdjust();
				CurrencyInfo cury = res;
				adj.VendorID = doc.VendorID;
				adj.AdjdDocType = doc.DocType;
				adj.AdjdRefNbr = doc.RefNbr;
				adj.AdjdLineNbr = tran?.LineNbr ?? 0;
				adj.AdjgDocType = APDocType.Check;
				adj.AdjgRefNbr = AutoNumberAttribute.GetNewNumberSymbol<APPayment.refNbr>(Caches[typeof(APPayment)], new APPayment { DocType = APDocType.Check });
				adj.SeparateCheck = doc.SeparateCheck;

				APAdjust locatedAdjust = APExceptionsList.Locate(adj);
				if (locatedAdjust == null)
				{
					PXSelectJoin<APInvoice, 
							LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
									And<APTran.tranType, Equal<APInvoice.docType>,
										And<APTran.refNbr, Equal<APInvoice.refNbr>,
											And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>,
								InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>>,
							Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
								And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>
						.StoreCached(this,
							new PXCommandKey(new object[] { adj.AdjdLineNbr, adj.AdjdDocType, adj.AdjdRefNbr }),
							new List<object> { new PXResult<APInvoice, APTran, CurrencyInfo>(doc, tran, cury) });

					PXSelectorAttribute.StoreCached<APAdjust.adjdRefNbr>(APExceptionsList.Cache, adj, doc);

					if (adj.AdjdLineNbr != 0)
					{
						PXSelectorAttribute.StoreCached<APAdjust.adjdLineNbr>(APExceptionsList.Cache, adj, tran);
					}
					result.Add(new PXResult<APAdjust, APInvoice, APTran>(APExceptionsList.Insert(adj), doc, tran));
				}
				else
				{
					result.Add(new PXResult<APAdjust, APInvoice, APTran>(locatedAdjust, doc, tran));
				}
			}
			if(result.IsResultTruncated)
				PXView.StartRow = 0;

			APExceptionsList.Cache.IsDirty = false;
			return result;
		}
		protected virtual IEnumerable apdocumentlist()
		{
			PayBillsFilter filter = Filter.Current;
			PXSelectBase<APInvoice> cmd = ComposeBQLCommandForAPDocumentListSelect();
			
			PXResultMapper mapper = new PXResultMapper(this,
					new Dictionary<Type, Type>()
					{
						{typeof(APAdjust.adjdDocType), typeof(APInvoice.docType)},
						{typeof(APAdjust.adjdRefNbr), typeof(APInvoice.refNbr)},
						{typeof(APAdjust.adjdLineNbr), typeof(APTran.lineNbr)},
					},
					typeof(APAdjust), typeof(APInvoice), typeof(APTran)
				);

			mapper.ExtFilters.AddRange(new Type[]
			{
				typeof(APAdjust.separateCheck),
				typeof(APAdjust.curyAdjgAmt),
				typeof(APAdjust.curyDocBal),
				typeof(APAdjust.curyAdjgDiscAmt),
				typeof(APAdjust.curyDiscBal)
			});
			mapper.SuppressSorts.AddRange(new Type[]
			{
				typeof(APAdjust.adjgDocType),
				typeof(APAdjust.adjgRefNbr),
				typeof(APAdjust.adjNbr)
			});			
			bool extSorts = mapper.SortColumns.Any(field => mapper.ExtFilters.Contains(APDocumentList.Cache.GetBqlField(field)));
			PXDelegateResult result = mapper.CreateDelegateResult(!extSorts);
			

			int startRow = result.IsResultTruncated ? PXView.StartRow : 0;
			int maxRow = result.IsResultTruncated ? PXView.MaximumRows: 0;
			int totalRows = 0;

			object[] searches = mapper.Searches;
			string[] sorts = mapper.SortColumns;
			int index = sorts.FindIndex(field => field == typeof(APTran).Name + "__" + typeof(APTran.lineNbr).Name);
			if (index > 0 && (int?)searches[index] == 0)
			{
				searches[index] = null;
			}
			foreach (PXResult<APInvoice, APTran, CurrencyInfo> res in
				cmd.View.Select(new[] { filter },
					ComposeParametersForAPDocumentListSelect(),
					searches,
					sorts,
					mapper.Descendings,
					mapper.Filters,
					ref startRow,
					maxRow,
					ref totalRows))
			{
				APInvoice doc = res;
			    APTran tran = res;
			    CurrencyInfo cury = res;
				APAdjust adj = new APAdjust();
				adj.VendorID = doc.VendorID;
				adj.AdjdDocType = doc.DocType;
				adj.AdjdRefNbr = doc.RefNbr;
				adj.AdjgDocType = APDocType.Check;
				adj.AdjgRefNbr = AutoNumberAttribute.GetNewNumberSymbol<APPayment.refNbr>(Caches[typeof(APPayment)], new APPayment { DocType = APDocType.Check });
			    adj.AdjdLineNbr = tran?.LineNbr ?? 0;
			    adj.Selected = false;

				adj.SeparateCheck = doc.SeparateCheck;

				if (APDocumentList.Locate(adj) == null)
				{
					PXSelectJoin<APInvoice,
						LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
								And<APTran.tranType, Equal<APInvoice.docType>,
								And<APTran.refNbr, Equal<APInvoice.refNbr>,
								And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>>,
						Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
							And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>
					.StoreCached(this,
						new PXCommandKey(new object[] {adj.AdjdLineNbr, adj.AdjdDocType, adj.AdjdRefNbr}),
						new List<object> {new PXResult<APInvoice, APTran, CurrencyInfo>(doc, tran, cury)});

					PXSelectorAttribute.StoreCached<APAdjust.adjdRefNbr>(APDocumentList.Cache, adj, doc);

					if (adj.AdjdLineNbr != 0)
					{						
						PXSelectorAttribute.StoreCached<APAdjust.adjdLineNbr>(APDocumentList.Cache, adj, tran);					
					}

					result.Add( new PXResult<APAdjust, APInvoice, APTran>(APDocumentList.Insert(adj), doc, tran) );
					_copies.Add(adj, PXCache<APAdjust>.CreateCopy(adj));
				}
				else
				{
					adj = APDocumentList.Locate(adj);
					adj.SeparateCheck = (adj.SeparateCheck ?? doc.SeparateCheck);

					result.Add(new PXResult<APAdjust, APInvoice, APTran>(adj, doc, tran));
					if (_copies.ContainsKey(adj))
					{
						_copies.Remove(adj);
					}
					_copies.Add(adj, PXCache<APAdjust>.CreateCopy(adj));
				}
			}

			APDocumentList.Cache.IsDirty = false;
			if (result.IsResultTruncated)
			{
				PXView.StartRow = 0;
			}

			return result;
		}

		/// <summary>
		/// Composes parameters for the BQL command created by 
		/// <see cref="ComposeBQLCommandForAPDocumentListSelect"/> method.
		/// This method can be overridden along with <see cref="ComposeBQLCommandForAPDocumentListSelect"/>
		/// to modify filtering conditions.
		/// </summary>
		/// <returns>The method returns an array of objects that contain required parameters 
		/// for BQL select.</returns>
		public virtual object[] ComposeParametersForAPDocumentListSelect()
		{
			PayBillsFilter filter = Filter.Current;
			DateTime payInLessThan = ((DateTime)filter.PayDate).AddDays(filter.PayInLessThan.GetValueOrDefault());
			DateTime dueInLessThan = ((DateTime)filter.PayDate).AddDays(filter.DueInLessThan.GetValueOrDefault());
			DateTime discountExpiresInLessThan = ((DateTime)filter.PayDate).AddDays(filter.DiscountExpiresInLessThan.GetValueOrDefault());
			return new[] { (object)payInLessThan, (object)dueInLessThan, (object)discountExpiresInLessThan };
		}

		/// <summary>
		/// Composes a BQL query for the <see cref="apdocumentlist"/> 
		/// select delegate. 
		/// The <see cref="ComposeParametersForAPDocumentListSelect"/>
		/// method provides parameters for the query.
		/// </summary>
		public virtual PXSelectBase<APInvoice> ComposeBQLCommandForAPDocumentListSelect()
		{
			return new PXSelectJoin<
				APInvoice,
					LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
							And<APTran.tranType, Equal<APInvoice.docType>,
							And<APTran.refNbr, Equal<APInvoice.refNbr>,
							And<APTran.curyTranBal, Greater<decimal0>>>>>,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
					InnerJoin<Vendor, 
						On<Vendor.bAccountID, Equal<APInvoice.vendorID>,
						And<Where<
							Vendor.status, Equal<BAccount.status.active>,
							Or<Vendor.status, Equal<BAccount.status.oneTime>>>>>,
					LeftJoin<APAdjust, 
						On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
						And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
						And<APAdjust.released, Equal<False>>>>,
					LeftJoin<APAdjust2, 
						On<APAdjust2.adjgDocType, Equal<APInvoice.docType>,
						And<APAdjust2.adjgRefNbr, Equal<APInvoice.refNbr>,
						And<APAdjust2.released, Equal<False>>>>,
					LeftJoin<APPayment, 
						On<APPayment.docType, Equal<APInvoice.docType>,
						And<APPayment.refNbr, Equal<APInvoice.refNbr>,
						And<APPayment.docType, Equal<APDocType.prepayment>>>>>>>>>>,
				Where<APInvoice.manualEntry, Equal<True>, 
				Or<Where<
					APInvoice.openDoc, Equal<True>,
					And<APInvoice.hold, Equal<False>,
					And<APInvoice.curyDocBal, NotEqual<decimal0>,
					And2<Where<
						APInvoice.released, Equal<True>,
						Or<APInvoice.prebooked, Equal<True>>>,
					And2<Where<
						APInvoice.paySel, Equal<True>, 
						Or2<Where<Current<APSetup.requireApprovePayments>, Equal<False>>,
						Or<APInvoice.docType, Equal<APDocType.debitAdj>>>>,
					And2<Where<
						APInvoice.vendorID, Equal<Current<PayBillsFilter.vendorID>>,
						Or<Current<PayBillsFilter.vendorID>, IsNull>>,
					And<APInvoice.payAccountID, Equal<Current<PayBillsFilter.payAccountID>>,
					And<APInvoice.payTypeID, Equal<Current<PayBillsFilter.payTypeID>>,
					And<APAdjust.adjgRefNbr, IsNull,
					And<APAdjust2.adjdRefNbr, IsNull,
					And<APPayment.refNbr, IsNull,
					And2<Where2<Where2<
						Where<
							Current<PayBillsFilter.showPayInLessThan>, Equal<True>,
							And<APInvoice.payDate, LessEqual<Required<APInvoice.payDate>>>>,
						Or2<Where<
							Current<PayBillsFilter.showDueInLessThan>, Equal<True>,
							And<Where<
								APInvoice.dueDate, LessEqual<Required<APInvoice.dueDate>>, 
								Or<APInvoice.dueDate, IsNull>>>>,
						Or<Where<
							Current<PayBillsFilter.showDiscountExpiresInLessThan>, Equal<True>,
							And<Where<
								APInvoice.discDate, LessEqual<Required<APInvoice.discDate>>, 
								Or<APInvoice.discDate, IsNull>>>>>>>,
						Or<Where<
							Current<PayBillsFilter.showPayInLessThan>, Equal<False>,
							And<Current<PayBillsFilter.showDueInLessThan>, Equal<False>,
							And<Current<PayBillsFilter.showDiscountExpiresInLessThan>, Equal<False>>>>>>,
					And<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>>>>>>>>>(this);
		}

		public virtual void CreatePayments(List<APAdjust> list, PayBillsFilter filter, CurrencyInfo info, PaymentMethod paymenttype)
		{
			if (RunningFlagScope<APPayBills>.IsRunning)
				throw new PXSetPropertyException(Messages.AnotherPayBillsRunning, PXErrorLevel.Warning);
			
			using (new RunningFlagScope<APPayBills>())
			{
				_createPayments(list, filter, info, paymenttype);
			}
		}

		protected virtual void _createPayments(List<APAdjust> list, PayBillsFilter filter, CurrencyInfo info, PaymentMethod paymenttype)
		{			
			foreach (APAdjust adj in list)
			{
				adj.AdjgDocDate = filter.PayDate;
			    adj.AdjgBranchID = filter.BranchID;
			    FinPeriodIDAttribute.CalcMasterPeriodID<APAdjust.adjgFinPeriodID>(Caches[typeof(APAdjust)], adj);
                adj.AdjgFinPeriodID = filter.PayFinPeriodID;
			    adj.AdjgTranPeriodID = FinPeriodRepository.GetByID(adj.AdjgFinPeriodID, PXAccess.GetParentOrganizationID(adj.AdjgBranchID)).FinPeriodID;
            }

			bool failed = false;

			//check amount is always sum of its applications
			PXRowSelecting del = delegate(PXCache cache, PXRowSelectingEventArgs e)
			{
				APPayment doc = e.Row as APPayment;
				if (doc != null)
				{
					doc.CuryApplAmt = doc.CuryOrigDocAmt;
					doc.CuryUnappliedBal = 0m;
				}
			};

			APPaymentEntry pe = CreateInstance<APPaymentEntry>();
			pe.RowSelecting.RemoveHandler<APPayment>(pe.APPayment_RowSelecting);
			pe.RowSelecting.AddHandler<APPayment>(del);
			
			List<PXResult<APPayment, Vendor>> createdPayments = new List<PXResult<APPayment, Vendor>>();

			list.Sort((a, b) =>
			{
				int aSortOrder = 0;
				int bSortOrder = 0;

				aSortOrder += (1 + ((IComparable)a.VendorID).CompareTo(b.VendorID)) / 2 * 1000;
				bSortOrder += (1 - ((IComparable)a.VendorID).CompareTo(b.VendorID)) / 2 * 1000;

				aSortOrder += (a.AdjdDocType == APDocType.DebitAdj ? 100 : 0);
				bSortOrder += (b.AdjdDocType == APDocType.DebitAdj ? 100 : 0);

				aSortOrder += (1 + ((IComparable)a.AdjdRefNbr).CompareTo(b.AdjdRefNbr)) / 2 * 10;
				bSortOrder += (1 - ((IComparable)a.AdjdRefNbr).CompareTo(b.AdjdRefNbr)) / 2 * 10;

				var aObj = PXResult.Unwrap<APAdjust>(a).AdjdLineNbr ?? 0;
				var bObj = PXResult.Unwrap<APAdjust>(b).AdjdLineNbr ?? 0;
				aSortOrder += (1 + ((IComparable)aObj).CompareTo(bObj)) / 2;
				bSortOrder += (1 - ((IComparable)aObj).CompareTo(bObj)) / 2;
				return aSortOrder.CompareTo(bSortOrder);
			});
			var docs = new Dictionary<string, APAdjust>();
			foreach (APAdjust adj in list)
			{
				string docKey = $"{adj.AdjdDocType}_{adj.AdjdRefNbr}";
				if (docs.TryGetValue(docKey, out var parent))
				{
					if (parent.SeparateCheck != true && adj.SeparateCheck == true)
					{
						parent.SeparateCheck = true;
					}
					adj.SeparateCheck = false;
				}
				else
				{
					docs[docKey] = adj;
				}
			}

			foreach (APAdjust adj in list)
			{
				PXProcessing<APAdjust>.SetCurrentItem(adj);
				try
				{
					if (CheckIfRowNotApprovedForPayment(adj))
					{
						throw new PXSetPropertyException(Messages.DocumentNotApprovedNotProceed, PXErrorLevel.RowError);
					}

					var rec = (PXResult<APInvoice, APTran>)pe.APInvoice_VendorID_DocType_RefNbr
					    .Select(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr);
				    APInvoice apdoc = rec;
					apdoc.PayAccountID = filter.PayAccountID;
					apdoc.PayTypeID = filter.PayTypeID;
					pe.APInvoice_VendorID_DocType_RefNbr.StoreCached(
						new PXCommandKey(new object[] { adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr }),
						new List<object> { rec });

					pe.TakeDiscAlways = filter.TakeDiscAlways == true;
					pe.CreatePayment(adj, info, false);

					Vendor vendor = pe.vendor.Current;

					//always update to reflect changes in amount
					byte[] timestamp = pe.TimeStamp;
					pe.Clear();
					pe.Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
					APPayment seldoc = PXCache<APPayment>.CreateCopy(pe.Document.Current);

					seldoc.tstamp = timestamp;

					createdPayments.Add(new PXResult<APPayment, Vendor>(seldoc, vendor));

					PXProcessing<APAdjust>.SetProcessed();
				}
				catch (PXException exc)
				{
					PXSetPropertyException exception = exc as PXSetPropertyException;
					if (exception != null && exception.ErrorLevel == PXErrorLevel.Warning)
					{
						PXProcessing<APAdjust>.SetWarning(exception);
					}
					else
					{
						PXProcessing<APAdjust>.SetError(exc);
						failed = true;

						pe.Clear();
					}
				}
			}

			List<PXResult<APPayment, Vendor>> resultPayments = new List<PXResult<APPayment, Vendor>>();

			foreach (var paymentVendor in createdPayments)
			{
				APPayment payment = paymentVendor;
				Vendor vendor = paymentVendor;

				try
				{
					pe.Clear();
					pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
					APPayment seldoc = PXCache<APPayment>.CreateCopy(pe.Document.Current);

					if (seldoc.Hold != false)
					{
						seldoc.Hold = false;
						seldoc = pe.Document.Update(seldoc);
						pe.Save.Press();
					}

					payment.Hold = seldoc.Hold;
					payment.Status = seldoc.Status;

					resultPayments.Add(new PXResult<APPayment, Vendor>(payment, vendor));
				}
				catch (PXException exc)
				{
					PXSetPropertyException exception = exc as PXSetPropertyException;
					if (exception != null && exception.ErrorLevel == PXErrorLevel.Warning)
					{
						PXProcessing<APAdjust>.SetWarning(exception);
					}
					else
					{
						PXProcessing<APAdjust>.SetError(exc);
						failed = true;

						pe.Clear();
					}
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
			else if (pe.created.Count > 0)
			{
				RedirectToResult(resultPayments, paymenttype, filter);
			}
		}

		protected virtual void RedirectToResult(List<PXResult<APPayment, Vendor>> payments, PaymentMethod paymentMethod, PayBillsFilter filter)
		{
			bool dontApprove = false;
			foreach (PXResult<APPayment, Vendor> result in payments)
			{
				APPayment payment = result;
				if (payment.Status != APDocStatus.PendingApproval)
				{
					dontApprove = true;
					break;
				}
			}

			if (!dontApprove)
				return;

			PXGraph targetGraph = null;
            PXSelectBase<APPayment> paymentView = null;
			if (paymentMethod?.PrintOrExport == true)
			{
				APPrintChecks printChecksGraph = CreateInstance<APPrintChecks>();

				PrintChecksFilter filterCopy = PXCache<PrintChecksFilter>.CreateCopy(printChecksGraph.Filter.Current);
				filterCopy.BranchID = filter.BranchID;
				filterCopy.PayTypeID = filter.PayTypeID;
				filterCopy.PayAccountID = filter.PayAccountID;
				filterCopy.CuryID = filter.CuryID;
				printChecksGraph.Filter.Cache.Update(filterCopy);

				targetGraph = printChecksGraph;
                paymentView = printChecksGraph.APPaymentList;

            }
			else
			{
				APReleaseChecks releaseChecksGraph = CreateInstance<APReleaseChecks>();

				ReleaseChecksFilter filterCopy = PXCache<ReleaseChecksFilter>.CreateCopy(releaseChecksGraph.Filter.Current);
				filterCopy.PayTypeID = filter.PayTypeID;
				filterCopy.PayAccountID = filter.PayAccountID;
				filterCopy.CuryID = filter.CuryID;
				releaseChecksGraph.Filter.Cache.Update(filterCopy);

				targetGraph = releaseChecksGraph;
                paymentView = releaseChecksGraph.APPaymentList;
            }

			if (targetGraph != null && paymentView != null)
            {
                PXCache paymentCache = paymentView.Cache;
                foreach (PXResult<APPayment, Vendor> result in payments)
				{
					APPayment payment = result;
					Vendor vendor = result;
                    payment = paymentView.Search<APPayment.docType, APPayment.refNbr>(payment.DocType, payment.RefNbr);
                    if (payment != null)
                    {
                        targetGraph.Caches[typeof(Vendor)].Current = vendor;

                        payment.Selected = true;
                        payment.Passed = true;

                        paymentCache.Update(payment);
                    }
				}

				paymentCache.IsDirty = false;
				targetGraph.TimeStamp = TimeStamp;

				throw new PXRedirectRequiredException(targetGraph, "NextProcessing");
			}
		}

		protected virtual void PayBillsFilter_PayAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PayBillsFilter.curyInfoID>>>>.Select(this, null))
			{
				currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyRateTypeID>(info);
			}
			sender.SetDefaultExt<PayBillsFilter.payDate>(e.Row);
			
		}

		protected virtual void PayBillsFilter_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<PayBillsFilter.payDate>(e.Row);
			sender.SetDefaultExt<PayBillsFilter.payAccountID>(e.Row);
		}

		protected virtual void PayBillsFilter_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<PayBillsFilter.payDate>(e.Row);
		}

		protected virtual void PayBillsFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Invoice.Cache.Clear();
			APDocumentList.Cache.Clear();
			APExceptionsList.Cache.Clear();
			Filter.Current.CurySelTotal = 0m;
			Filter.Current.SelTotal = 0m;
			Filter.Current.SelCount = 0;
			if (!sender.ObjectsEqual<PayBillsFilter.payAccountID>(e.OldRow, e.Row))
				Filter.Cache.SetDefaultExt<PayBillsFilter.curyID>(e.Row);
		}

		protected virtual void PayBillsFilter_PayDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			sender.RaiseExceptionHandling<PayBillsFilter.payDate>(e.Row, e.NewValue, null);
		}

		protected virtual void PayBillsFilter_PayDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PayBillsFilter.curyInfoID>>>>.Select(this, null))
			{
				currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyEffDate>(info);
			}
			APDocumentList.Cache.Clear();
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			foreach (PayBillsFilter filter in Filter.Cache.Inserted)
			{
				e.NewValue = filter.PayDate;
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				foreach (PayBillsFilter filter in Filter.Cache.Inserted)
				{
					if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryRateTypeID))
					{
						e.NewValue = cashaccount.Current.CuryRateTypeID;
						e.Cancel = true;
					}
				}
			}
		}

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDimensionSelector(SubAccountAttribute.DimensionName, typeof(Sub.subCD), typeof(Sub.subCD))]
		[PXRemoveBaseAttribute(typeof(SubAccountAttribute))]
		protected virtual void APAdjust_AdjdAPSub_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(PayBillsFilter.curyInfoID), CuryIDField = "AdjgCuryID")]
		protected virtual void APAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(APDocType.Check)]
		protected virtual void APAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		protected virtual void APAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(APDocType.Invoice)]
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.List()]
		protected virtual void APAdjust_AdjdDocType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdRefNbr(typeof(Search2<
			APInvoice.refNbr,
			InnerJoin<BAccount,
				On<BAccount.bAccountID, Equal<APInvoice.vendorID>,
				And<Where<
					BAccount.status, Equal<BAccount.status.active>,
					Or<BAccount.status, Equal<BAccount.status.oneTime>>>>>,
			LeftJoin<APAdjust,
				On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
				And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
				And<APAdjust.released, Equal<False>,
				And<Where<
					APAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>,
					Or<APAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>,
			LeftJoin<APPayment,
				On<APPayment.docType, Equal<APInvoice.docType>,
				And<APPayment.refNbr, Equal<APInvoice.refNbr>,
				And<APPayment.docType, Equal<APDocType.prepayment>>>>>>>,
			Where<
				APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>,
				And2<Where<
					APInvoice.released, Equal<True>,
					Or<APInvoice.prebooked, Equal<True>>>,
				And<APInvoice.openDoc, Equal<True>,
				And<APAdjust.adjgRefNbr, IsNull,
				And<APPayment.refNbr, IsNull>>>>>>),
			Filterable = true)]
		protected virtual void APAdjust_AdjdRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXInt()]
		protected virtual void APAdjust_AdjdWhTaxAcctID_CacheAttached(PXCache sender)
		{
		}

		[PXInt()]
		protected virtual void APAdjust_AdjdWhTaxSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		protected virtual void APAdjust_VendorID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void APAdjust_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && ((APAdjust)e.Row).AdjdCuryInfoID != null && ((APAdjust)e.Row).CuryDocBal == null)
			{
				CalcBalances((APAdjust)e.Row, false);
			}
			if (e.Row != null)
			{
				e.NewValue = ((APAdjust)e.Row).CuryDocBal;
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APAdjust.adjdLineNbr>(e.Row);
		}
		protected virtual void APAdjust_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && ((APAdjust)e.Row).AdjdCuryInfoID != null && ((APAdjust)e.Row).CuryDiscBal == null)
			{
				CalcBalances((APAdjust)e.Row, false);
			}
			if (e.Row != null)
			{
				e.NewValue = ((APAdjust)e.Row).CuryDiscBal;
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj.CuryDocBal == null)
			{
				CalcBalances(adj, false);
			}

			if (adj.CuryAdjgAmt == null)
			{
				adj.CuryAdjgAmt = 0m;
			}

			if (adj.CuryAdjgDiscAmt == null)
			{
				adj.CuryAdjgDiscAmt = 0m;
			}

			if (adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal?)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
			}
		}

		protected virtual void APAdjust_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.OldValue != null && ((APAdjust)e.Row).CuryDocBal == 0m && ((APAdjust)e.Row).CuryAdjgAmt < (decimal)e.OldValue)
			{
				((APAdjust)e.Row).CuryAdjgDiscAmt = 0m;
			}
			CalcBalances((APAdjust)e.Row, true);
		}

		protected virtual void APAdjust_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj.CuryDiscBal == null)
			{
				CalcBalances(adj, false);
			}

			if (adj.CuryAdjgAmt == null)
			{
				adj.CuryAdjgAmt = 0m;
			}

			if (adj.CuryAdjgDiscAmt == null)
			{
				adj.CuryAdjgDiscAmt = 0m;
			}

			if (adj.CuryDiscBal + adj.CuryAdjgDiscAmt == 0 && (decimal?)e.NewValue > 0)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_EQ, 0.ToString());
			}
			else if (adj.CuryDiscBal + adj.CuryAdjgDiscAmt - (decimal?)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
			}

			if (adj.CuryAdjgAmt != null && (sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
			{
				if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
				}
			}
		}

		protected virtual void APAdjust_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances((APAdjust)e.Row, true);
		}

		private void CalcBalances(APAdjust row, bool isCalcRGOL)
		{
			APAdjust adj = (APAdjust)row;
			foreach (PXResult<APInvoice,APTran> record in 
			    PXSelectJoin<APInvoice, 
		            LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
		                And<APTran.tranType, Equal<APInvoice.docType>,
		                    And<APTran.refNbr, Equal<APInvoice.refNbr>,
		                        And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>>,
                    Where<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>, 
			          And<APInvoice.docType, Equal<Required<APInvoice.docType>>, 
			          And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>>
			        .Select(this, adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
			    APInvoice voucher = record;
                APTran tran = record;

                CalcBalances(adj, voucher, tran, isCalcRGOL);
				return;
			}
		}

		protected virtual void CalcBalances(APAdjust adj, APInvoice voucher, bool isCalcRGOL)
		{
			CalcBalances(adj, voucher, null, isCalcRGOL);
		}

		protected virtual void CalcBalances(APAdjust adj, APInvoice voucher, APTran tran, bool isCalcRGOL)
		{
			try
			{
				APPaymentEntry.CalcBalances<APInvoice>(CurrencyInfo_CuryInfoID, adj, voucher, isCalcRGOL, Filter.Current.TakeDiscAlways == false, tran);
			}
			catch (PXRateIsNotDefinedForThisDateException ex)
			{
				APDocumentList.Cache.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(adj, 0m, new PXSetPropertyException(ex.Message, PXErrorLevel.RowError));
			}
		}

		protected virtual void APAdjust_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PayBillsFilter filter = Filter.Current;
			if (filter != null)
			{
				{
					APAdjust new_row = e.Row as APAdjust;
					filter.CurySelTotal += new_row.Selected == true ? new_row.AdjgBalSign * new_row.CuryAdjgAmt : 0m;
					filter.SelTotal += new_row.Selected == true ? new_row.AdjgBalSign * new_row.AdjAmt : 0m;
					filter.SelCount += new_row.Selected == true ? 1 : 0;
				}
			}

			if (CheckIfRowNotApprovedForPayment((APAdjust)e.Row))
				APDocumentList.Cache.RaiseExceptionHandling<APAdjust.curyAdjgAmt>((APAdjust)e.Row, 0m, new PXSetPropertyException(Messages.DocumentNotApprovedNotProceed, PXErrorLevel.RowWarning));

		}

		protected virtual void APAdjust_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PayBillsFilter filter = Filter.Current;
			if (filter != null)
			{
				object OldRow = e.OldRow;
				if (object.ReferenceEquals(e.Row, e.OldRow) && !_copies.TryGetValue(e.Row, out OldRow))
				{
					decimal? curyval = 0m;
					decimal? val = 0m;
					int? count = 0;

					foreach (APAdjust res in APDocumentList.Select((object)null))
					{
						if (res.Selected == true)
						{
							curyval += res.AdjgBalSign * res.CuryAdjgAmt ?? 0m;
							val += res.AdjgBalSign * res.AdjAmt ?? 0m;
							count++;
						}
					}

					filter.CurySelTotal = curyval;
					filter.SelTotal = val;
					filter.SelCount = count;
				}
				else
				{
					APAdjust old_row = OldRow as APAdjust;
					APAdjust new_row = e.Row as APAdjust;
					filter.CurySelTotal -= old_row.Selected == true ? old_row.AdjgBalSign * old_row.CuryAdjgAmt : 0m;
					filter.CurySelTotal += new_row.Selected == true ? new_row.AdjgBalSign * new_row.CuryAdjgAmt : 0m;

					filter.SelTotal -= old_row.Selected == true ? old_row.AdjgBalSign * old_row.AdjAmt : 0m;
					filter.SelTotal += new_row.Selected == true ? new_row.AdjgBalSign * new_row.AdjAmt : 0m;

					filter.SelCount -= old_row.Selected == true ? 1 : 0;
					filter.SelCount += new_row.Selected == true ? 1 : 0;
				}
			}
		}

		protected virtual void CurrencyInfo_SampleCuryRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (Filter.Current != null && ((CurrencyInfo)e.Row).CuryInfoID == Filter.Current.CuryInfoID && ((CurrencyInfo)CurrencyInfo_CuryInfoID.Select(Filter.Current.CuryInfoID)) != null)
			{
				foreach (PXResult<APAdjust, APInvoice, APTran> row in APDocumentList.Select())
				{
					CalcBalances(row, row, row,true);
				}
				APDocumentList.View.RequestRefresh();
			}
		}

		protected virtual void CurrencyInfo_SampleRecipRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (Filter.Current != null && ((CurrencyInfo)e.Row).CuryInfoID == Filter.Current.CuryInfoID && ((CurrencyInfo)CurrencyInfo_CuryInfoID.Select(Filter.Current.CuryInfoID)) != null)
			{
				foreach (PXResult<APAdjust, APInvoice, APTran> row in APDocumentList.Select())
				{
					CalcBalances(row, row, row, true);
				}
				APDocumentList.View.RequestRefresh();
			}
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "Filter" &&
				Filter.Current != null &&
				Filter.Current.SelCount > 0 &&
				Filter.View.Ask(viewName, Messages.AskUpdatePayBillsFilter, MessageButtons.YesNo) == WebDialogResult.No) return 0;

			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		protected class APDocumentListViewExecuteParamsBuilder
		{
			protected readonly string APInvoiceViewPrefix = $"{nameof(APInvoice)}__";

			protected Dictionary<string, string> AdjFieldsToSubstituteByName;

			protected string DocTypeFieldName;
			protected string RefNbrFieldName;

			protected string AdjdDocTypeFieldName;
			protected string AdjdRefNbrFieldName;

			public APDocumentListViewExecuteParamsBuilder()
			{
				DocTypeFieldName = typeof(APInvoice.docType).Name.Capitalize();
				RefNbrFieldName = typeof(APInvoice.refNbr).Name.Capitalize();

				AdjdDocTypeFieldName = typeof(APAdjust.adjdDocType).Name.Capitalize();
				AdjdRefNbrFieldName = typeof(APAdjust.adjdRefNbr).Name.Capitalize();

				AdjFieldsToSubstituteByName = new Dictionary<string, string>()
				{
					{AdjdDocTypeFieldName, DocTypeFieldName},
					{AdjdRefNbrFieldName, RefNbrFieldName}
				};
			}

			public virtual ViewExecutingParams BuildViewExecutingParams(PXView view)
			{
				ViewExecutingParams viewExecutingParams = BuildSortingAndDescendings(view);

				viewExecutingParams.FilterRows = BuildFilters(view);

				return viewExecutingParams;
			}

			protected virtual ViewExecutingParams BuildSortingAndDescendings(PXView view)
			{
				ViewExecutingParams viewExecutingParams = new ViewExecutingParams()
				{
					Sorts = new List<string>(),
					Descendings = new List<bool>(),
					FilterRows = new List<PXFilterRow>()
				};


				string[] externalSorts = view.GetExternalSorts();
				bool[] externalDescendings = view.GetExternalDescendings();

				if (externalSorts != null)
				{
					for (int i = 0; i < externalSorts.Length; i++)
					{
						string sortField = externalSorts[i];

						if (AdjFieldsToSubstituteByName.ContainsKey(sortField))
						{
							viewExecutingParams.Sorts.Add(AdjFieldsToSubstituteByName[sortField]);
						}
						else if (sortField.Contains(APInvoiceViewPrefix))
						{
							viewExecutingParams.Sorts.Add(ConvertToFieldNameWithoutPrefix(sortField, APInvoiceViewPrefix));
						}
						else
						{
							viewExecutingParams.Sorts.Add(sortField);
						}

						viewExecutingParams.Descendings.Add(externalDescendings[i]);
					}
				}

				string[] keySortingFields = new string[] { DocTypeFieldName, RefNbrFieldName };

				foreach (var keyField in keySortingFields)
				{
					if (!viewExecutingParams.Sorts.Contains(keyField))
					{
						viewExecutingParams.Sorts.Add(keyField);
						viewExecutingParams.Descendings.Add(false);
					}
				}
				return viewExecutingParams;
			}

			protected virtual List<PXFilterRow> BuildFilters(PXView view)
			{
				List<PXFilterRow> filters = view.GetExternalFilters()?.ToList() ?? new List<PXFilterRow>();

				foreach (var externalFilter in filters)
				{
					if (AdjFieldsToSubstituteByName.ContainsKey(externalFilter.DataField))
					{
						externalFilter.DataField = AdjFieldsToSubstituteByName[externalFilter.DataField];
					}
					else if (externalFilter.DataField.Contains(APInvoiceViewPrefix))
					{
						externalFilter.DataField = ConvertToFieldNameWithoutPrefix(externalFilter.DataField, APInvoiceViewPrefix);
					}
				}

				return filters;
			}

			private string ConvertToFieldNameWithoutPrefix(string fieldNameWithPrefix, string prefix)
			{
				char firstLetter = char.ToUpper(fieldNameWithPrefix[prefix.Length]);

				return string.Concat(firstLetter,
					fieldNameWithPrefix.Substring(prefix.Length + 1, fieldNameWithPrefix.Length - prefix.Length - 1));
			}

			public class ViewExecutingParams
			{
				public List<string> Sorts;

				public List<bool> Descendings;

				public List<PXFilterRow> FilterRows;
			}
		}
	}
	
	[Serializable]
	public partial class PayBillsFilter : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		[Branch]
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
		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		protected String _PayTypeID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>))]
		
		public virtual String PayTypeID
		{
			get
			{
				return this._PayTypeID;
			}
			set
			{
				this._PayTypeID = value;
			}
		}
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;

		[CashAccount(typeof(PayBillsFilter.branchID), typeof(Search2<CashAccount.cashAccountID,
							InnerJoin<PaymentMethodAccount, 
								On<PaymentMethodAccount.cashAccountID,Equal<CashAccount.cashAccountID>>>,
							Where2<Match<Current<AccessInfo.userName>>, 
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID,Equal<Current<PayBillsFilter.payTypeID>>,
							And<PaymentMethodAccount.useForAP,Equal<True>>>>>>), Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search2<PaymentMethodAccount.cashAccountID, 
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PayBillsFilter.payTypeID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
							And<PaymentMethodAccount.aPIsDefault, Equal<True>,
							And<CashAccount.branchID, Equal<Current<AccessInfo.branchID>>>>>>>))]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion
		#region PayDate
		public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
		protected DateTime? _PayDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? PayDate
		{
			get
			{
				return this._PayDate;
			}
			set
			{
				this._PayDate = value;
			}
		}
		#endregion
		#region PayFinPeriodID
		public abstract class payFinPeriodID : PX.Data.BQL.BqlString.Field<payFinPeriodID> { }
		protected string _PayFinPeriodID;
		[APOpenPeriod(
		    typeof(payDate), 
		    branchSourceType: typeof(branchID))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public virtual String PayFinPeriodID
		{
			get
			{
				return this._PayFinPeriodID;
			}
			set
			{
				this._PayFinPeriodID = value;
			}
		}
		#endregion
		#region PayInLessThan
		public abstract class payInLessThan : PX.Data.BQL.BqlShort.Field<payInLessThan> { }
		protected Int16? _PayInLessThan;
		[PXDBShort()]
		[PXUIField(Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(APSetup.paymentLeadTime), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int16? PayInLessThan
		{
			get
			{
				return this._PayInLessThan;
			}
			set
			{
				this._PayInLessThan = value;
			}
		}
		#endregion
		#region ShowPayInLessThan
		public abstract class showPayInLessThan : PX.Data.BQL.BqlBool.Field<showPayInLessThan> { }
		protected Boolean? _ShowPayInLessThan;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Pay Date Within", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowPayInLessThan
		{
			get
			{
				return this._ShowPayInLessThan;
			}
			set
			{
				this._ShowPayInLessThan = value;
			}
		}
		#endregion
		#region DueInLessThan
		public abstract class dueInLessThan : PX.Data.BQL.BqlShort.Field<dueInLessThan> { }
		protected Int16? _DueInLessThan;
		[PXDBShort()]
		[PXUIField(Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(APSetup.paymentLeadTime), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int16? DueInLessThan
		{
			get
			{
				return this._DueInLessThan;
			}
			set
			{
				this._DueInLessThan = value;
			}
		}
		#endregion
		#region ShowDueInLessThan
		public abstract class showDueInLessThan : PX.Data.BQL.BqlBool.Field<showDueInLessThan> { }
		protected Boolean? _ShowDueInLessThan;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Due Date Within", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowDueInLessThan
		{
			get
			{
				return this._ShowDueInLessThan;
			}
			set
			{
				this._ShowDueInLessThan = value;
			}
		}
		#endregion
		#region DiscountExpiredInLessThan
		public abstract class discountExpiresInLessThan : PX.Data.BQL.BqlShort.Field<discountExpiresInLessThan> { }
		protected Int16? _DiscountExpiresInLessThan;
		[PXDBShort()]
		[PXUIField(Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(APSetup.paymentLeadTime), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int16? DiscountExpiresInLessThan
		{
			get
			{
				return this._DiscountExpiresInLessThan;
			}
			set
			{
				this._DiscountExpiresInLessThan = value;
			}
		}
		#endregion
		#region ShowDiscountExpiresInLessThan
		public abstract class showDiscountExpiresInLessThan : PX.Data.BQL.BqlBool.Field<showDiscountExpiresInLessThan> { }
		protected Boolean? _ShowDiscountExpiresInLessThan;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cash Discount Expires Within", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowDiscountExpiresInLessThan
		{
			get
			{
				return this._ShowDiscountExpiresInLessThan;
			}
			set
			{
				this._ShowDiscountExpiresInLessThan = value;
			}
		}
		#endregion
		#region Balance
		public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
		protected Decimal? _Balance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDBDecimal(4)]
		[PXDBCury(typeof(PayBillsFilter.curyID))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<PayBillsFilter.payAccountID>>>>))]
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
		[CurrencyInfo(ModuleCode = BatchModule.AP)]
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
		#region Days
		public abstract class days : PX.Data.BQL.BqlString.Field<days> { }
		protected String _Days;
		[PXDBString(IsUnicode = true)]
		[PXUIField]
		[PXDefault(Messages.Days, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Days
		{
			get
			{
				return this._Days;
			}
			set
			{
				this._Days = value;
			}
		}
		#endregion

		#region CurySelTotal
		public abstract class curySelTotal : PX.Data.BQL.BqlDecimal.Field<curySelTotal> { }
		protected Decimal? _CurySelTotal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDBCury(typeof(PayBillsFilter.curyID))]
		[PXDBCurrency(typeof(PayBillsFilter.curyInfoID), typeof(PayBillsFilter.selTotal), BaseCalc = false)]
		[PXUIField(DisplayName = "Selection Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CurySelTotal
		{
			get
			{
				return this._CurySelTotal;
			}
			set
			{
				this._CurySelTotal = value;
			}
		}
		#endregion
		#region SelTotal
		public abstract class selTotal : PX.Data.BQL.BqlDecimal.Field<selTotal> { }
		protected Decimal? _SelTotal;
		[PXDBDecimal(4)]
		public virtual Decimal? SelTotal
		{
			get
			{
				return this._SelTotal;
			}
			set
			{
				this._SelTotal = value;
			}
		}
		#endregion
		#region SelCount
		public abstract class selCount : PX.Data.BQL.BqlInt.Field<selCount> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Number of Rows Selected", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual int? SelCount { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[VendorActive(
			Visibility = PXUIVisibility.SelectorVisible, 
			Required = false, 
			DescriptionField = typeof(Vendor.acctName))]
		[PXDefault]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region TakeDiscAlways
		public abstract class takeDiscAlways : PX.Data.BQL.BqlBool.Field<takeDiscAlways> { }
		protected Boolean? _TakeDiscAlways;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Always Take Cash Discount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? TakeDiscAlways
		{
			get
			{
				return this._TakeDiscAlways;
			}
			set
			{
				this._TakeDiscAlways = value;
			}
		}
		#endregion
		#region CashBalance
		public abstract class cashBalance : PX.Data.BQL.BqlDecimal.Field<cashBalance> { }
		protected Decimal? _CashBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDBDecimal()]
		[PXDBCury(typeof(PayBillsFilter.curyID))]
		[PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(PayBillsFilter.payAccountID))]
		public virtual Decimal? CashBalance
		{
			get
			{
				return this._CashBalance;
			}
			set
			{
				this._CashBalance = value;
			}
		}
		#endregion
		#region GLBalance
		public abstract class gLBalance : PX.Data.BQL.BqlDecimal.Field<gLBalance> { }
		protected Decimal? _GLBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDBDecimal()]
		[PXDBCury(typeof(PayBillsFilter.curyID))]
		[PXUIField(DisplayName = "GL Balance", Enabled = false)]
		[GLBalance(typeof(PayBillsFilter.payAccountID), typeof(PayBillsFilter.payFinPeriodID))]
		public virtual Decimal? GLBalance
		{
			get
			{
				return this._GLBalance;
			}
			set
			{
				this._GLBalance = value;
			}
		}
		#endregion
	}
}
