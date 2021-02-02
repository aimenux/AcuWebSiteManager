using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APReleaseChecks : PXGraph<APReleaseChecks>
	{
		public PXFilter<ReleaseChecksFilter> Filter;
		public PXCancel<ReleaseChecksFilter> Cancel;
		public PXAction<ReleaseChecksFilter> ViewDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (APPaymentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(APPaymentList.Cache, APPaymentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public ToggleCurrency<ReleaseChecksFilter> CurrencyView;
		[PXFilterable]
		public PXFilteredProcessing<APPayment, ReleaseChecksFilter, Where<boolTrue, Equal<boolTrue>>, OrderBy<Desc<APPayment.selected>>> APPaymentList;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		#region Actions for assign access rights
		public PXAction<ReleaseChecksFilter> Release;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ReleaseChecks, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		public IEnumerable release(PXAdapter a)
		{
			return a.Get();
		}
		public PXAction<ReleaseChecksFilter> Reprint;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ReprintChecks, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public IEnumerable reprint(PXAdapter a)
		{
			return a.Get();
		}
		public PXAction<ReleaseChecksFilter> VoidReprint;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ReprintChecksWithNewNumber, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		public IEnumerable voidReprint(PXAdapter a)
		{
			return a.Get();
		}
		#endregion

		#region Setups
		public PXSetup<APSetup> APSetup;
		public PXSetup<Company> Company;

		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Optional<ReleaseChecksFilter.payAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Optional<ReleaseChecksFilter.payTypeID>>>>> cashaccountdetail;
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[APDocType.ReleaseChecksList]
		protected virtual void APPayment_DocType_CacheAttached(PXCache sender) { }

		[Obsolete("The method is obsolete after AC-80359 fix")]
		public virtual void SetPreloaded(APPrintChecks graph)
		{
			ReleaseChecksFilter filter_copy = PXCache<ReleaseChecksFilter>.CreateCopy(Filter.Current);
			filter_copy.PayAccountID = graph.Filter.Current.PayAccountID;
			filter_copy.PayTypeID = graph.Filter.Current.PayTypeID;
			filter_copy.CuryID = graph.Filter.Current.CuryID;
			Filter.Cache.Update(filter_copy);

			foreach (APPayment seldoc in graph.APPaymentList.Cache.Updated)
			{
				seldoc.Passed = true;
				APPaymentList.Cache.Update(seldoc);
				APPaymentList.Cache.SetStatus(seldoc, PXEntryStatus.Updated);
			}
			APPaymentList.Cache.IsDirty = false;

			TimeStamp = graph.TimeStamp;
		}

		public APReleaseChecks()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled(APPaymentList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.selected>(APPaymentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(APPaymentList.Cache, null, true);

			PXUIFieldAttribute.SetVisible<APPayment.printCheck>(APPaymentList.Cache, null, false);
			PXUIFieldAttribute.SetVisibility<APPayment.printCheck>(APPaymentList.Cache, null, PXUIVisibility.Invisible);
		}

		private bool cleared;
		public override void Clear()
		{
			cleared = true;
			base.Clear();
		}

		protected virtual IEnumerable appaymentlist()
		{
			if (cleared)
			{
				foreach (APPayment doc in APPaymentList.Cache.Updated)
				{
					doc.Passed = false;
				}
			}

			IEnumerable<APPayment> checks = PXSelectJoin < APPayment,
				InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
				Where2<Where< APPayment.released, NotEqual<True>,
					And<APPayment.hold, NotEqual<True>,
					And<APPayment.printed, Equal<True>,
					And<APPayment.docType, NotEqual<APDocType.refund>,
					And<APPayment.cashAccountID, Equal<Current<ReleaseChecksFilter.payAccountID>>,
					And<APPayment.paymentMethodID, Equal<Current<ReleaseChecksFilter.payTypeID>>,
					And<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>,
					And<Where<APPayment.dontApprove, Equal<True>,
						Or<APPayment.approved, Equal<True>>>>>>
				.Select(this).RowCast<APPayment>();

			return checks;
		}

		public static void ReleasePayments(List<APPayment> list, string Action)
		{
			APReleaseChecks releaseChecksGraph = CreateInstance<APReleaseChecks>();
			APPaymentEntry pe = CreateInstance<APPaymentEntry>();
			CABatchEntry be = CreateInstance<CABatchEntry>();
			bool failed = false;
			bool successed = false;

			List<APRegister> docs = new List<APRegister>(list.Count);

			foreach (APPayment payment in list)
			{
				if (payment.Passed == true)
				{
					releaseChecksGraph.TimeStamp = pe.TimeStamp = payment.tstamp;
				}

				switch (Action)
				{
					case ReleaseChecksFilter.action.ReleaseChecks:
						payment.Printed = true;
						break;
					case ReleaseChecksFilter.action.ReprintChecks:			
					case ReleaseChecksFilter.action.VoidAndReprintChecks:
						payment.ExtRefNbr = null;
						payment.Printed = false;
						break;
					default:
						continue;
				}

				PXProcessing<APPayment>.SetCurrentItem(payment);

				if (Action == ReleaseChecksFilter.action.ReleaseChecks) 
				{
					try
					{
						pe.Document.Current = pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
						if (pe.Document.Current?.ExtRefNbr != payment.ExtRefNbr)
						{
							APPayment payment_copy = PXCache<APPayment>.CreateCopy(pe.Document.Current);
							payment_copy.ExtRefNbr = payment.ExtRefNbr;
							pe.Document.Update(payment_copy);
						}

						if (PaymentRefAttribute.PaymentRefMustBeUnique(pe.paymenttype.Current) && string.IsNullOrEmpty(payment.ExtRefNbr))
						{
							throw new PXException(ErrorMessages.FieldIsEmpty, typeof(APPayment.extRefNbr).Name);
						}

						payment.IsReleaseCheckProcess = true;

						if (payment.Prebooked != true) 
						{
							APPrintChecks.AssignNumbersWithNoAdditionalProcessing(pe, payment);
						}
						pe.Save.Press();

						object[] persisted = PXTimeStampScope.GetPersisted(pe.Document.Cache, pe.Document.Current);
						if (persisted == null || persisted.Length == 0)
						{
							//preserve timestamp which will be @@dbts after last record committed to db on previous Persist().
							//otherwise another process updated APAdjust.
							docs.Add(payment);
						}
						else
						{
							if (payment.Passed == true)
							{
								pe.Document.Current.Passed = true;
							}
							docs.Add(pe.Document.Current);
						}
						successed = true;
					}
					catch (PXException e)
					{
						PXProcessing<APPayment>.SetError(e);
						docs.Add(null);
						failed = true;
					}
				}

				if (Action == ReleaseChecksFilter.action.ReprintChecks || Action == ReleaseChecksFilter.action.VoidAndReprintChecks)				
				{
					try
					{
						payment.IsPrintingProcess = true;
						using (PXTransactionScope transactionScope = new PXTransactionScope())
						{

							#region Update CABatch if ReprintChecks
							CABatch caBatch = PXSelectJoin<CABatch,
							InnerJoin<CABatchDetail, On<CABatch.batchNbr, Equal<CABatchDetail.batchNbr>>>,
							Where<CABatchDetail.origModule, Equal<Required<APPayment.origModule>>,
								And<CABatchDetail.origDocType, Equal<Required<APPayment.docType>>,
								 And<CABatchDetail.origRefNbr, Equal<Required<APPayment.refNbr>>>>>>.
							 Select(be, payment.OrigModule, payment.DocType, payment.RefNbr);
							if (caBatch != null)
							{
								be.Document.Current = caBatch;
								int DetailCount = be.Details.Select().Count; // load
								CABatchDetail detail = be.Details.Locate(new CABatchDetail()
								{
									BatchNbr = be.Document.Current.BatchNbr,
									OrigModule = payment.OrigModule,
									OrigDocType = payment.DocType,
									OrigRefNbr = payment.RefNbr,
									OrigLineNbr = CABatchDetail.origLineNbr.DefaultValue,
								});
								if (detail != null)
								{
									// payment.Status is recalculated in CABatchEntry.Delete
									if (DetailCount == 1)
									{
										be.Document.Delete(be.Document.Current); // Details will delete by PXParent
									}
									else
									{
										be.Details.Delete(detail);  // recalculated batch totals
									}
								}
								be.Save.Press();
							}
							else
							{
								PXCache cacheAPPayment = releaseChecksGraph.APPaymentList.Cache;

								cacheAPPayment.SetValueExt<APPayment.printed>(payment, false);
								cacheAPPayment.SetValueExt<APPayment.hold>(payment, false);
								cacheAPPayment.SetValueExt<APPayment.extRefNbr>(payment, null);
								// APPayment.Status is recalculated by SetStatusCheckAttribute
								cacheAPPayment.MarkUpdated(payment);

								cacheAPPayment.PersistUpdated(payment);
								cacheAPPayment.Persisted(false);
							}
							#endregion
						// TODO: Need to rework. Use legal CRUD methods of caches!
						releaseChecksGraph.TimeStamp = PXDatabase.SelectTimeStamp();

						// delete check numbers only if Reprint (not Void and Reprint)
						PaymentMethod pm = pe.paymenttype.Select(payment.PaymentMethodID);
						if (pm.APPrintChecks == true && Action == ReleaseChecksFilter.action.ReprintChecks)
						{
							APPayment doc = payment;
							new HashSet<string>(pe.Adjustments_Raw.Select(doc.DocType, doc.RefNbr)
								.RowCast<APAdjust>()
								.Select(adj => adj.StubNbr))
									.ForEach(nbr => PaymentRefAttribute.DeleteCheck((int)doc.CashAccountID, doc.PaymentMethodID, nbr));

							// sync PaymentMethodAccount.APLastRefNbr with actual last CashAccountCheck number
							PaymentMethodAccount det = releaseChecksGraph.cashaccountdetail.SelectSingle(payment.CashAccountID, payment.PaymentMethodID);
							PaymentRefAttribute.LastCashAccountCheckSelect.Clear(releaseChecksGraph);
							CashAccountCheck cacheck = PaymentRefAttribute.LastCashAccountCheckSelect.SelectSingleBound(releaseChecksGraph, new object[] { det });
							det.APLastRefNbr = cacheck?.CheckNbr;
							releaseChecksGraph.cashaccountdetail.Cache.PersistUpdated(det);
							releaseChecksGraph.cashaccountdetail.Cache.Persisted(false);
						}
						// END TODO
						if (string.IsNullOrEmpty(payment.ExtRefNbr))
						{
							//try to get next number
							releaseChecksGraph.APPaymentList.Cache.SetDefaultExt<APPayment.extRefNbr>(payment);
						}
							transactionScope.Complete();
						}
					}
					catch (PXException e)
					{
						PXProcessing<APPayment>.SetError(e);
					}
					docs.Add(null);
				}
			}
			if (successed)
			{
				APDocumentRelease.ReleaseDoc(docs, true);
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
		}

		protected virtual void ReleaseChecksFilter_PayAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter.Cache.SetDefaultExt<ReleaseChecksFilter.curyID>(e.Row);
			APPaymentList.Cache.Clear();
		}

		protected virtual void ReleaseChecksFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//redefault to release action when saved values are populated in filter
			if (((ReleaseChecksFilter)e.OldRow).PayAccountID == null && ((ReleaseChecksFilter)e.OldRow).PayTypeID == null)
			{
				((ReleaseChecksFilter)e.Row).Action = ReleaseChecksFilter.action.ReleaseChecks;
			}
		}

		protected virtual void ReleaseChecksFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetVisible<ReleaseChecksFilter.curyID>(sender, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			ReleaseChecksFilter filter = e.Row as ReleaseChecksFilter;
			if (filter == null) return;

			PaymentMethod paymentMethod = PXSelect<PaymentMethod,
				Where<PaymentMethod.paymentMethodID, Equal<Required<ReleaseChecksFilter.payTypeID>>>>.Select(this, filter.PayTypeID);
			Reprint.SetEnabled(paymentMethod != null && paymentMethod.PrintOrExport == true);
			VoidReprint.SetEnabled(paymentMethod != null && paymentMethod.PrintOrExport == true);

			List<Tuple<string, string>> availableActions = new List<Tuple<string, PXAction>>
			{
					new Tuple<string, PXAction>(ReleaseChecksFilter.action.ReleaseChecks, Release),
					new Tuple<string, PXAction>(ReleaseChecksFilter.action.ReprintChecks, Reprint),
					new Tuple<string, PXAction>(ReleaseChecksFilter.action.VoidAndReprintChecks, VoidReprint)
			}
				.Select(t => new {ShortCut = t.Item1, State = t.Item2.GetState(filter) as PXButtonState})
				.Where(t => t.State?.Enabled == true)
				.Select(t => new Tuple<string, string>(t.ShortCut, t.State.DisplayName)).ToList();

			string[] actions = availableActions.Select(t => t.Item1).ToArray();
			PXStringListAttribute.SetLocalizable<ReleaseChecksFilter.action>(Filter.Cache, null, false);
			PXStringListAttribute.SetList<ReleaseChecksFilter.action>(Filter.Cache, null, actions, availableActions.Select(t => t.Item2).ToArray());
			PXUIFieldAttribute.SetEnabled<ReleaseChecksFilter.action>(Filter.Cache, filter, availableActions.Count > 1);

			if (availableActions.Count > 0)
			{
				if (filter.Action == null || !actions.Contains(filter.Action))
			{
					filter.Action = actions[0];
				}
			}
			else
			{
				filter.Action = null;
			}

			string action = filter.Action;
			APPaymentList.SetProcessEnabled(action != null);
			APPaymentList.SetProcessAllEnabled(action != null);

			APPaymentList.SetProcessDelegate(list => ReleasePayments(list, action));
		}

		protected virtual void ReleaseChecksFilter_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ReleaseChecksFilter.payAccountID>(e.Row);
		}

		protected virtual void APPayment_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			APPayment row = e.NewRow as APPayment;
			if (row != null && row.Selected == true)
			{
				PXFieldState state = (PXFieldState)sender.GetStateExt<APPayment.extRefNbr>(row);
				if (state != null && !string.IsNullOrEmpty(state.Error))
					row.Selected = false;
			}
		}

		protected virtual void APPayment_PrintCheck_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((APPayment)e.Row).Printed != true)
			{
				((APPayment)e.Row).Selected = true;
			}
		}
	}

	[Serializable]
	public partial class ReleaseChecksFilter : IBqlTable
	{
		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		protected String _PayTypeID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>>>))]		
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
		[CashAccount(null, typeof(Search2<CashAccount.cashAccountID,
						   InnerJoin<PaymentMethodAccount,
							   On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						   Where2<Match<Current<AccessInfo.userName>>,
						   And<CashAccount.clearingAccount, Equal<False>,
						   And<PaymentMethodAccount.paymentMethodID, Equal<Current<ReleaseChecksFilter.payTypeID>>,
						   And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search2<PaymentMethodAccount.cashAccountID,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<ReleaseChecksFilter.payTypeID>>,
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
		
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute(): base(
					new string[] { ReleaseChecks},
					new string[] { Messages.ReleaseChecks }) {}
		}

			public const string ReleaseChecks = "R";
			public const string ReprintChecks = "D";
			public const string VoidAndReprintChecks = "V";

			public class releaseChecks : PX.Data.BQL.BqlString.Constant<releaseChecks>
			{
				public releaseChecks() : base(ReleaseChecks) {}
			}

			public class reprintChecks : PX.Data.BQL.BqlString.Constant<reprintChecks>
			{
				public reprintChecks() : base(ReprintChecks) {}
			}

			public class voidAndReprintChecks : PX.Data.BQL.BqlString.Constant<voidAndReprintChecks>
			{
				public voidAndReprintChecks() : base(VoidAndReprintChecks) {}
			}
		}
		[PXDBString(10)]
		[PXUIField(DisplayName = "Action")]
		[action.List]
		public virtual string Action { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<ReleaseChecksFilter.payAccountID>>>>))]
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
		#region CashBalance
		public abstract class cashBalance : PX.Data.BQL.BqlDecimal.Field<cashBalance> { }
		protected Decimal? _CashBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(ReleaseChecksFilter.curyID))]
		[PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(ReleaseChecksFilter.payAccountID))]
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
		#region PayFinPeriodID
		public abstract class payFinPeriodID : PX.Data.BQL.BqlString.Field<payFinPeriodID> { }
		protected string _PayFinPeriodID;
		[FinPeriodID(typeof(AccessInfo.businessDate))]
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
		#region GLBalance
		public abstract class gLBalance : PX.Data.BQL.BqlDecimal.Field<gLBalance> { }
		protected Decimal? _GLBalance;

		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDBDecimal()]
		[PXDBCury(typeof(ReleaseChecksFilter.curyID))]
		[PXUIField(DisplayName = "GL Balance", Enabled = false)]
		[GLBalance(typeof(ReleaseChecksFilter.payAccountID), typeof(ReleaseChecksFilter.payFinPeriodID))]
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
