using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.CA;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class CCTransactionsHistoryEnq : PXGraph<CCTransactionsHistoryEnq>
	{
		#region Internal Types
		public static class CardSearchOption
		{

			public const string PartialNumber = "P";
			public const string FullNumber = "F";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { PartialNumber, FullNumber },
					new string[] { Messages.PMInstanceSearchByPartialNumber, Messages.PMInstanceSearchByFullNumber })
				{ }

			}
		}
		[Serializable]
		public partial class CCTransactionsHistoryFilter : IBqlTable
		{
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			protected String _PaymentMethodID;
			[PXDBString(10, IsUnicode = true)]
			[PXDefault()]
			//[PXDefault(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>>>))]
			[PXUIField(DisplayName = "Payment Method")]//, TabOrder =1)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>>>), DescriptionField = typeof(PaymentMethod.descr))]
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

			#region CardSearchType
			public abstract class cardSearchType : PX.Data.BQL.BqlString.Field<cardSearchType> { }
			protected String _CardSearchType;
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Search By", Visibility = PXUIVisibility.Visible)]//,TabOrder=4)]
			[PXDefault(CardSearchOption.PartialNumber)]
			[CardSearchOption.List()]
			public virtual String CardSearchType
			{
				get
				{
					return this._CardSearchType;
				}
				set
				{
					this._CardSearchType = value;
				}
			}
			#endregion
			#region CardNumber
			public abstract class cardNumber : PX.Data.BQL.BqlString.Field<cardNumber> { }
			protected String _CardNumber;
#if true
			[PXDBStringWithMask(255, typeof(Search<PaymentMethodDetail.entryMask, Where<PaymentMethodDetail.paymentMethodID,
												Equal<Current<CCTransactionsHistoryFilter.paymentMethodID>>,
												And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
												And<PaymentMethodDetail.isIdentifier, Equal<True>>>>>), IsUnicode = true)]
#else
			[PXDBString(30)]
#endif
			[PXUIField(DisplayName = "Card Number")]//,TabOrder =6)]
			public virtual String CardNumber
			{
				get
				{
					return this._CardNumber;
				}
				set
				{
					this._CardNumber = value;
				}
			}
			#endregion
			#region PartialCardNumber
			public abstract class partialCardNumber : PX.Data.BQL.BqlString.Field<partialCardNumber> { }
			protected String _PartialCardNumber;

			[PXDBStringWithMask(255, typeof(Search<PaymentMethodDetail.displayMask, Where<PaymentMethodDetail.paymentMethodID,
												Equal<Current<CCTransactionsHistoryFilter.paymentMethodID>>,
												And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
												And<PaymentMethodDetail.isIdentifier, Equal<True>>>>>), IsUnicode = true)]
			[PXUIField(DisplayName = "Card Number - Partial")]//,TabOrder=7)]
			public virtual String PartialCardNumber
			{
				get
				{
					return this._PartialCardNumber;
				}
				set
				{
					this._PartialCardNumber = value;
				}
			}
			#endregion
			#region NameOnCard
			public abstract class nameOnCard : PX.Data.BQL.BqlString.Field<nameOnCard> { }
			protected String _NameOnCard;
			[PXDBString(255)]
			[PXUIField(DisplayName = "Customer Name")]//,TabOrder =9)]
			public virtual String NameOnCard
			{
				get
				{
					return this._NameOnCard;
				}
				set
				{
					this._NameOnCard = value;
				}
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

			protected DateTime? _StartDate;

			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date")]//,TabOrder = 12)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date")]//,TabOrder = 15)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
			#region EndDatePlusOne
			public abstract class endDatePlusOne : PX.Data.BQL.BqlDateTime.Field<endDatePlusOne> { }
			[PXDBDate()]
			public virtual DateTime? EndDatePlusOne
			{
				get
				{
					if (this._EndDate.HasValue)
						return this._EndDate.Value.AddDays(1);
					return this._EndDate;
				}
				set
				{
				}
			}
			#endregion
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

			protected Decimal? _Amount;
			[PXDBDecimal(4)]
			[PXUIField(DisplayName = "Amount")]//,TabOrder = 18)]
			public virtual Decimal? Amount
			{
				get
				{
					return this._Amount;
				}
				set
				{
					this._Amount = value;
				}
			}
			#endregion
			#region ProcessingCenterID
			public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
			protected String _ProcessingCenterID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>))]
			[PXUIField(DisplayName = "Proc. Center ID")]//,TabOrder=20)]
			public virtual String ProcessingCenterID
			{
				get
				{
					return this._ProcessingCenterID;
				}
				set
				{
					this._ProcessingCenterID = value;
				}
			}
			#endregion
			#region PMInstanceID
			public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
			protected Int32? _PMInstanceID;
			[PXDBInt()]
			[PXDefault()]
			[PXSelector(typeof(Search2<CustomerPaymentMethod.pMInstanceID,
								InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>>,
								Where<CustomerPaymentMethod.paymentMethodID, Equal<Current<CCTransactionsHistoryFilter.paymentMethodID>>,
								And<CustomerPaymentMethod.descr, Like<Current<CCTransactionsHistoryFilter.maskedCardNumber>>,
								And<Customer.acctName, Like<Current<CCTransactionsHistoryFilter.dBSearchName>>,
								And<CustomerPaymentMethod.isActive, Equal<True>>>>>>),
							new Type[]{typeof(CustomerPaymentMethod.descr),
							typeof(Customer.acctCD),
							typeof(Customer.acctName)}, DescriptionField = typeof(CustomerPaymentMethod.descr))]
			[PXUIField(DisplayName = "Card Number", Required = true)]//,TabOrder = 14)]
			public virtual Int32? PMInstanceID
			{
				get
				{
					return this._PMInstanceID;
				}
				set
				{
					this._PMInstanceID = value;
				}
			}
			#endregion
			#region MaskedCardNumber
			public abstract class maskedCardNumber : PX.Data.BQL.BqlString.Field<maskedCardNumber> { }
			protected String _MaskedCardNumber;
			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Masked Card Number", Enabled = false)]
			public virtual String MaskedCardNumber
			{
				get
				{
					return this._MaskedCardNumber;
				}
				set
				{
					this._MaskedCardNumber = value;
				}
			}
			#endregion
			#region NumberOfCards
			public abstract class numberOfCards : PX.Data.BQL.BqlInt.Field<numberOfCards> { }
			protected Int32? _NumberOfCards;
			[PXDBInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Null)]
			[PXUIField(DisplayName = "Number Of Matches", Enabled = false)]
			public virtual Int32? NumberOfCards
			{
				get
				{
					return this._NumberOfCards;
				}
				set
				{
					this._NumberOfCards = value;
				}
			}
			#endregion
			#region DBSearchName
			public abstract class dBSearchName : PX.Data.BQL.BqlString.Field<dBSearchName> { }
			[PXDBString(255)]
			public virtual String DBSearchName
			{
				get
				{
					return this._NameOnCard + PXDatabase.Provider.SqlDialect.WildcardAnything;
				}
			}
			#endregion
		}
		#endregion

		#region Ctor + Member Decalaration
		public PXFilter<CCTransactionsHistoryFilter> Filter;
		public PXCancel<CCTransactionsHistoryFilter> Cancel;
		[PXFilterable]
		public PXSelectJoin<CCProcTran,
			LeftJoin<Customer, On<True, Equal<True>>, LeftJoin<CustomerPaymentMethod, On<True, Equal<True>>>> //for aspx validation
			> CCTrans;
		public PXSetup<ARSetup> ARSetup;
		public PXAction<CCTransactionsHistoryFilter> ViewDocument;
		public PXAction<CCTransactionsHistoryFilter> ViewCustomer;
		public PXAction<CCTransactionsHistoryFilter> ViewPaymentMethod;
		public PXAction<CCTransactionsHistoryFilter> ViewExternalTransaction;

		protected string CardNumber_DetailID = string.Empty;
		protected string NameOnCard_DetailID = string.Empty;
		public CCTransactionsHistoryEnq()
		{
			ARSetup setup = ARSetup.Current;
			this.CCTrans.Cache.AllowInsert = false;
			this.CCTrans.Cache.AllowUpdate = false;
			this.CCTrans.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetRequired<ARPayment.customerID>(Caches[typeof(ARPayment)], false);
			PXUIFieldAttribute.SetRequired<Customer.acctName>(Caches[typeof(Customer)], false);
			PXUIFieldAttribute.SetRequired<CCProcTran.processingCenterID>(CCTrans.Cache, false);
			PXUIFieldAttribute.SetRequired<CCProcTran.startTime>(CCTrans.Cache, false);
		}
		#endregion

		#region Selection Delegates
		public virtual IEnumerable cCTrans()
		{
			List<PXResult<CCProcTran, CustomerPaymentMethod, Customer, ARPayment>> res = new List<PXResult<CCProcTran, CustomerPaymentMethod, Customer, ARPayment>>();

			CCTransactionsHistoryFilter filter = (CCTransactionsHistoryFilter)this.Filter.Current;


			if (filter != null && string.IsNullOrEmpty(filter.PaymentMethodID) == false &&
				filter.StartDate.HasValue && filter.EndDate.HasValue && filter.PMInstanceID != null)
			{
				GetInternalData(filter.PaymentMethodID);

				PXSelectBase<CCProcTran> sel = new PXSelectJoin<CCProcTran,
					InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<CCProcTran.pMInstanceID>>,
					InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>,
					LeftJoin<ARPayment, On<ARPayment.docType, Equal<CCProcTran.docType>, And<ARPayment.refNbr, Equal<CCProcTran.refNbr>>>>>>>(this);

				if (string.IsNullOrEmpty(filter.PaymentMethodID) == false)
				{
					sel.WhereAnd<Where<CustomerPaymentMethod.paymentMethodID, Equal<Current<CCTransactionsHistoryFilter.paymentMethodID>>>>();
				}
				if (string.IsNullOrEmpty(filter.ProcessingCenterID) == false)
				{
					sel.WhereAnd<Where<CCProcTran.processingCenterID, Equal<Current<CCTransactionsHistoryFilter.processingCenterID>>>>();
				}
				if (filter.StartDate.HasValue)
				{
					sel.WhereAnd<Where<CCProcTran.startTime, GreaterEqual<Current<CCTransactionsHistoryFilter.startDate>>>>();
				}
				if (filter.EndDate.HasValue)
				{
					sel.WhereAnd<Where<CCProcTran.startTime, LessEqual<Current<CCTransactionsHistoryFilter.endDatePlusOne>>>>();
				}

				if (filter.PMInstanceID.HasValue)
				{
					sel.WhereAnd<Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<CCTransactionsHistoryFilter.pMInstanceID>>>>();
				}

				foreach (PXResult<CCProcTran, CustomerPaymentMethod, Customer, ARPayment> it in sel.Select(CardNumber_DetailID, NameOnCard_DetailID))
				{
					ARPayment payment = it;
					CCProcTran ccTran = it;
					if (ccTran.TranType == CCTranTypeCode.Credit || ccTran.TranType == CCTranTypeCode.VoidTran)
					{
						ARPayment voiding = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
														And<ARPayment.docType, Equal<ARDocType.voidPayment>>>>.Select(this, payment.RefNbr);
						if (voiding != null)
						{
							((ARPayment)it).DocType = ARDocType.VoidPayment;
							((CCProcTran)it).DocType = ARDocType.VoidPayment;
						}
					}
					yield return it;
				}
			}
			yield break;
		}

		private void GetInternalData(string paymentMethodID)
		{
			PaymentMethodDetail paymentMethodDetail = PXSelectReadonly<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
					And<PaymentMethodDetail.isIdentifier, Equal<True>>>>>.Select(this, paymentMethodID);
			if (paymentMethodDetail != null)
			{
				CardNumber_DetailID = paymentMethodDetail.DetailID;
				//CardNumber_IsEncrypted = (paymentMethodDetail.IsEncrypted == true);
			}

			paymentMethodDetail = PXSelectReadonly<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
					And<PaymentMethodDetail.isOwnerName, Equal<True>>>>>.Select(this, paymentMethodID);
			if (paymentMethodDetail != null)
			{
				NameOnCard_DetailID = paymentMethodDetail.DetailID;
				//NameOnCard_IsEncrypted = (paymentMethodDetail.IsEncrypted == true);
			}
		}

		#endregion

		#region Actions

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			CCProcTran tran = this.CCTrans.Current;
			if (tran != null)
			{
				PXGraph target = FindSourceDocumentGraph(tran.DocType, tran.RefNbr, tran.OrigDocType, tran.OrigRefNbr);
				if (target != null)
					throw new PXRedirectRequiredException(target, true, Messages.ViewDocument) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewCustomer(PXAdapter adapter)
		{
			if (this.CCTrans.Current != null)
			{
				CCProcTran row = this.CCTrans.Current;
				CustomerPaymentMethod pmInstance = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(this, row.PMInstanceID);
				if (pmInstance != null)
				{
					CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
					graph.BAccount.Current = graph.BAccount.Search<Customer.bAccountID>(pmInstance.BAccountID);
					if (graph.BAccount.Current != null)
					{
						throw new PXRedirectRequiredException(graph, true, Messages.ViewCustomer) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}
			return adapter.Get();
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewPaymentMethod(PXAdapter adapter)
		{
			if (this.CCTrans.Current != null)
			{
				CustomerPaymentMethod payMethod = PXSelect<CustomerPaymentMethod,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
					.Select(this, this.CCTrans.Current.PMInstanceID);
				CustomerPaymentMethodMaint graph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
				graph.CustomerPaymentMethod.Current = payMethod;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewPaymentMethod) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewExternalTransaction, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewExternalTransaction(PXAdapter adapter)
		{
			if (this.CCTrans.Current != null)
			{
				CCProcTran row = this.CCTrans.Current;
				ExternalTransactionMaint graph = PXGraph.CreateInstance<ExternalTransactionMaint>();
				graph.CurrentTransaction.Current = graph.CurrentTransaction.Search<ExternalTransaction.transactionID>(row.TransactionID);
				if (graph.CurrentTransaction.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, Messages.ViewExternalTransaction) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return Filter.Select();
		}

		#endregion

		#region Filter Events Handler


		public virtual void CCTransactionsHistoryFilter_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				PXCache.TryDispose(sender.GetAttributes<CCTransactionsHistoryFilter.cardNumber>(e.Row));
			}
		}

		public virtual void CCTransactionsHistoryFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			CCTransactionsHistoryFilter row = (CCTransactionsHistoryFilter)e.Row;
			PXUIFieldAttribute.SetEnabled<CCTransactionsHistoryFilter.cardNumber>(sender, e.Row, row.CardSearchType == CardSearchOption.FullNumber);
			PXUIFieldAttribute.SetEnabled<CCTransactionsHistoryFilter.partialCardNumber>(sender, e.Row, row.CardSearchType == CardSearchOption.PartialNumber);

			PXUIFieldAttribute.SetVisible<CCTransactionsHistoryFilter.cardNumber>(sender, e.Row, row.CardSearchType == CardSearchOption.FullNumber);
			PXUIFieldAttribute.SetVisible<CCTransactionsHistoryFilter.partialCardNumber>(sender, e.Row, row.CardSearchType == CardSearchOption.PartialNumber);

			PXUIFieldAttribute.SetEnabled<CCTransactionsHistoryFilter.partialCardNumber>(sender, e.Row, !string.IsNullOrEmpty(row.PaymentMethodID));
			PXUIFieldAttribute.SetEnabled<CCTransactionsHistoryFilter.pMInstanceID>(sender, e.Row, !string.IsNullOrEmpty(row.PaymentMethodID));
			PXUIFieldAttribute.SetEnabled<CCTransactionsHistoryFilter.nameOnCard>(sender, e.Row, !string.IsNullOrEmpty(row.PaymentMethodID));
		}

		public virtual void CCTransactionsHistoryFilter_CardSearchType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CCTransactionsHistoryFilter row = (CCTransactionsHistoryFilter)e.Row;
			if (row.CardSearchType == CardSearchOption.FullNumber)
			{
				row.PartialCardNumber = String.Empty;
			}
			else
			{
				row.CardNumber = String.Empty;
			}
		}

		public virtual void CCTransactionsHistoryFilter_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CCTransactionsHistoryFilter row = (CCTransactionsHistoryFilter)e.Row;
			row.PMInstanceID = null;
			row.PartialCardNumber = string.Empty;
		}

		public virtual void CCTransactionsHistoryFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CCTransactionsHistoryFilter row = (CCTransactionsHistoryFilter)e.Row;
			CCTransactionsHistoryFilter oldRow = (CCTransactionsHistoryFilter)e.OldRow;
			if (oldRow.PaymentMethodID != row.PaymentMethodID || oldRow.CardNumber != row.CardNumber || oldRow.PartialCardNumber != row.PartialCardNumber)
			{
				this.RecalcRow(row);
			}
			if (row.MaskedCardNumber != oldRow.MaskedCardNumber || row.DBSearchName != oldRow.DBSearchName)
			{

				PXResult<CustomerPaymentMethod> cnt = PXSelectJoinGroupBy<CustomerPaymentMethod, InnerJoin<Customer,
															On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>>,
															Where<CustomerPaymentMethod.paymentMethodID, Equal<Current<CCTransactionsHistoryFilter.paymentMethodID>>,
																And<CustomerPaymentMethod.descr, Like<Optional<CCTransactionsHistoryFilter.maskedCardNumber>>,
																And<Customer.acctName, Like<Optional<CCTransactionsHistoryFilter.dBSearchName>>,
																And<CustomerPaymentMethod.isActive, Equal<True>>>>>,
															Aggregate<Count<CustomerPaymentMethod.pMInstanceID>>>.Select(this, row.MaskedCardNumber, row.DBSearchName);
				int count = 0;
				if (cnt != null && cnt.RowCount != null)
				{
					count = (int)cnt.RowCount;
					row.NumberOfCards = cnt.RowCount;
				}
				row.PMInstanceID = count == 1 ? ((CustomerPaymentMethod)cnt).PMInstanceID : null;
			}
		}

		protected virtual void RecalcRow(CCTransactionsHistoryFilter aRow)
		{
			if (!string.IsNullOrEmpty(aRow.PaymentMethodID))
			{
				PaymentMethodDetail def = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID,
									Equal<Optional<CCTransactionsHistoryFilter.paymentMethodID>>,
									And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
									And<PaymentMethodDetail.isIdentifier, Equal<True>>>>>.Select(this, aRow.PaymentMethodID);
				if (def != null)
				{
					aRow.MaskedCardNumber = CustomerPaymentMethodMaint.FormatDescription(aRow.PaymentMethodID, CustomerPaymentMethodMaint.IDObfuscator.RestoreToMasked(aRow.PartialCardNumber, def.DisplayMask, SqlDialect.WildcardAnySingle, false));
				}
			}
		}

		static bool IsNameMatch(string aName, string aTestName)
		{
			return aName.Trim().StartsWith(aTestName.Trim(), StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		public static PXGraph FindSourceDocumentGraph(string docType, string refNbr, string origDocType, string origRefNbr)
		{
			PXGraph target = null;
			if (docType == ARDocType.Payment || docType == ARDocType.VoidPayment)
			{
				ARPaymentEntry graph = PXGraph.CreateInstance<ARPaymentEntry>();
				graph.Document.Current = graph.Document.Search<ARPayment.refNbr>(refNbr, docType);
				if (graph.Document.Current != null)
					target = graph;
			}
			if (docType == ARDocType.CashSale)
			{
				ARCashSaleEntry graph = PXGraph.CreateInstance<ARCashSaleEntry>();
				graph.Document.Current = graph.Document.Search<Standalone.ARCashSale.refNbr>(refNbr, docType);
				if (graph.Document.Current != null)
					target = graph;
			}
			if (docType == ARDocType.Invoice)
			{
				SO.SOInvoiceEntry graph = PXGraph.CreateInstance<SO.SOInvoiceEntry>();
				graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(refNbr, docType);
				if (graph.Document.Current != null)
					target = graph;
			}
			if (target == null && !string.IsNullOrEmpty(origRefNbr))
			{
				SO.SOOrderEntry graph = PXGraph.CreateInstance<SO.SOOrderEntry>();
				graph.Document.Current = graph.Document.Search<SO.SOOrder.orderNbr>(origRefNbr, origDocType);
				if (graph.Document.Current != null)
					target = graph;
			}
			return target;
		}
	}
}