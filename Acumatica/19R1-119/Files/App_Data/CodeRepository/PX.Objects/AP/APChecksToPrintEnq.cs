using System;
using System.Collections.Generic;
using System.Collections;

using PX.Data;

using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]  
    [Serializable]
	public class APChecksToPrintEnq : PXGraph<APChecksToPrintEnq>
	{
		#region InternalTypes
        [Serializable]
		public partial class DocFilter: IBqlTable
		{
			#region PayTypeID
			public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
			protected string _PayTypeID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
			[PXRestrictor(typeof(Where<PaymentMethod.useForAP, Equal<True>>), Messages.PaymentTypeCantBeUsedInAP, typeof(PaymentMethod.paymentMethodID))]
			[PXRestrictor(typeof(Where<PaymentMethod.isActive, Equal<True>>), Messages.PaymentTypeIsInactive, typeof(PaymentMethod.paymentMethodID))]
			[PXRestrictor(typeof(Where<PaymentMethod.aPPrintChecks, Equal<True>,
				Or<PaymentMethod.aPCreateBatchPayment, Equal<True>>>),
			Messages.PaymentTypeNoPrintCheck, typeof(PaymentMethod.paymentMethodID))]
			public virtual string PayTypeID
			{
				get
				{
					return _PayTypeID;
				}
				set
				{
					_PayTypeID = value;
				}
			}
			#endregion
			#region PayAccountID
			public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
			protected int? _PayAccountID;
			[CashAccount(null, typeof(Search2<CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount,
					On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
				Where2<Match<Current<AccessInfo.userName>>,
					And<CashAccount.clearingAccount, Equal<False>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<DocFilter.payTypeID>>,
					And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), Visibility = PXUIVisibility.Visible)]
			public virtual int? PayAccountID
			{
				get
				{
					return _PayAccountID;
				}
				set
				{
					_PayAccountID = value;
				}
			}
			#endregion
			#region PayDate
			public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
			protected DateTime? _PayDate;
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Pay Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? PayDate
			{
				get
				{
					return _PayDate;
				}
				set
				{
					_PayDate = value;
				}
			}
			#endregion
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
			protected Decimal? _Balance;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBBaseCury()]
			[PXUIField(DisplayName = "Total Due", Enabled=false)]
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
			#region CuryBalance
			public abstract class curyBalance : PX.Data.BQL.BqlDecimal.Field<curyBalance> { }

			protected Decimal? _CuryBalance;
		
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCury(typeof(DocFilter.curyID))]
			[PXUIField(DisplayName = "Total Due", Enabled = false)]
			public virtual Decimal? CuryBalance
			{
				get
				{
					return this._CuryBalance;
				}
				set
				{
					this._CuryBalance = value;
				}
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Currency", Enabled=false)]
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
		}
		[PXSubstitute(GraphType = typeof(APChecksToPrintEnq))]
        [Serializable]
        [PXHidden]
		public partial class APPaymentExt : APPayment
		{
			#region DocDate
			public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			#endregion
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region OpenDoc
			public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			#endregion
			#region Hold
			public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
			#endregion
			
			#region MinPayDate
			public abstract class minPayDate : PX.Data.BQL.BqlDateTime.Field<minPayDate> { }
			protected DateTime? _MinPayDate;
			[PXDBDate(BqlField = typeof(APRegister.docDate))]
			[PXUIField(DisplayName = "Min. Pay Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? MinPayDate
			{
				get
				{
					return this._MinPayDate;
				}
				set
				{
					this._MinPayDate = value;
				}
			}
			#endregion
			#region DocCount
			public abstract class docCount : PX.Data.BQL.BqlInt.Field<docCount> { }
			protected int? _DocCount;
			[PXInt()]
			[PXUIField(DisplayName = "Documents", Visible = true)]
			public virtual int? DocCount			
			{
				get
				{
					return this._DocCount;
				}
				set 
				{
					this._DocCount = value;
				}
			}
			#endregion
		}
        [Serializable]
		public partial class CheckSummary: IBqlTable
		{
			public CheckSummary() 
			{
				ClearValues();
			}

			#region PayAccountID
			public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
			protected Int32? _PayAccountID;
			[CashAccount(DescriptionField = typeof(CashAccount.descr), Visibility = PXUIVisibility.SelectorVisible, IsKey = true)]
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
			#region PayTypeID
			public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
			protected String _PayTypeID;
			[PXDBString(10, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,Where<PaymentMethod.useForAP,Equal<True>>>),DescriptionField=typeof(CA.PaymentMethod.descr))]
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
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Currency", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(typeof(Search<Company.baseCuryID>))]
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
			#region CuryDocBal
			public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
			protected Decimal? _CuryDocBal;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXCurrency(typeof(CheckSummary.curyInfoID), typeof(CheckSummary.docBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Amount", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Decimal? CuryDocBal
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
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			protected Decimal? _DocBal;
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Amount",Visible = false)]
			public virtual Decimal? DocBal
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
			#region MinPayDate
			public abstract class minPayDate : PX.Data.BQL.BqlDateTime.Field<minPayDate> { }
			protected DateTime? _MinPayDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Min. Pay Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? MinPayDate
			{
				get
				{
					return this._MinPayDate;
				}
				set
				{
					this._MinPayDate = value;
				}
			}
			#endregion
			#region PayDate
			public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
			protected DateTime? _PayDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Pay Date")]
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
			#region MaxPayDate
			public abstract class maxPayDate : PX.Data.BQL.BqlDateTime.Field<maxPayDate> { }
			protected DateTime? _MaxPayDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Max.Pay Date")]
			public virtual DateTime? MaxPayDate
			{
				get
				{
					return this._MaxPayDate;
				}
				set
				{
					this._MaxPayDate = value;
				}
			}
			#endregion
			#region DocCount
			public abstract class docCount : PX.Data.BQL.BqlInt.Field<docCount> { }
			protected int? _DocCount;
			[PXInt()]
			[PXUIField(DisplayName = "Documents", Visible = true)]
			public virtual int? DocCount
			{
				get
				{
					return this._DocCount;
				}
				set
				{
					this._DocCount = value;
				}
			}
				#endregion
			#region OverdueDocCount
			public abstract class overdueDocCount : PX.Data.BQL.BqlInt.Field<overdueDocCount> { }
			protected int? _OverdueDocCount;
			[PXInt()]
			[PXUIField(DisplayName = "Overdue Docs", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual int? OverdueDocCount
			{
				get
				{
					return this._OverdueDocCount;
				}
				set
				{
					this._OverdueDocCount = value;
				}
			}
			#endregion
			#region OverdueDocBal
			public abstract class overdueDocBal : PX.Data.BQL.BqlDecimal.Field<overdueDocBal> { }
			protected Decimal? _OverdueDocBal;
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overdue Docs. Amount",Visible = false)]
			public virtual Decimal? OverdueDocBal
			{
				get
				{
					return this._OverdueDocBal;
				}
				set
				{
					this._OverdueDocBal = value;
				}
			}
			#endregion
			#region OverdueCuryDocBal
			public abstract class overdueCuryDocBal : PX.Data.BQL.BqlDecimal.Field<overdueCuryDocBal> { }
			protected Decimal? _OverdueCuryDocBal;
			[PXCurrency(typeof(CheckSummary.curyInfoID), typeof(CheckSummary.overdueDocBal), BaseCalc = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overdue Docs. Amount", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Decimal? OverdueCuryDocBal
			{
				get
				{
					return this._OverdueCuryDocBal;
				}
				set
				{
					this._OverdueCuryDocBal = value;
				}
			}
			#endregion
			protected void ClearValues() 
			{
				DocCount = 0;
				CuryDocBal = 0m;
				DocBal = 0m;
				
				OverdueDocCount = 0;
				OverdueDocBal = 0m;
				OverdueCuryDocBal = 0m;
			}
		}
		protected struct CashAcctKey : IComparable<CashAcctKey>
		{
			public CashAcctKey(int aAccountID, string aPayTypeID)
			{
				AccountID = aAccountID;
				PaymentTypeID = aPayTypeID;
			}

			public int AccountID;
			public string PaymentTypeID;

			#region IComparable<CashAcctKey> Members

			int IComparable<CashAcctKey>.CompareTo(CashAcctKey other)
			{
				if (this.AccountID == other.AccountID)
					return (this.PaymentTypeID.CompareTo(other.PaymentTypeID));
				return Math.Sign(this.AccountID - other.AccountID);
			}

			#endregion
		}
		protected class APPaymentKey : Pair<string, string> 
		{
			public APPaymentKey(string aFirst, string aSecond) : base(aFirst, aSecond) { }
		}
			
		#endregion
		#region Ctor + Overrides
		public APChecksToPrintEnq() 
		{
			APSetup setup = APSetup.Current;
			this.Documents.Cache.AllowDelete = false;
			this.Documents.Cache.AllowInsert = false;
			this.Documents.Cache.AllowUpdate = false;
		}
		public PXSetup<APSetup> APSetup;
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		#endregion
		#region Buttons
		public PXCancel<DocFilter> Cancel;
		
		#region Navigation Button
		public PXAction<DocFilter> processPayment;
		[PXUIField(DisplayName = "Print Checks", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable ProcessPayment(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				CheckSummary res = Documents.Current;
				APPrintChecks graph = CreateInstance<APPrintChecks>();
				PrintChecksFilter paymentFilter = graph.Filter.Current;
				paymentFilter.PayAccountID = res.PayAccountID;
				paymentFilter.PayTypeID = res.PayTypeID;
				graph.Filter.Update(paymentFilter);
				throw new PXRedirectRequiredException(graph, "ProcessPayment");
			}

			return adapter.Get();
		}

		public PXAction<DocFilter> viewCashAccount;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCashAccount(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				CashAccountMaint graph = CreateInstance<CashAccountMaint>();
				graph.CashAccount.Current = graph.CashAccount.Search<CashAccount.cashAccountID>(Documents.Current.PayAccountID);
				throw new PXRedirectRequiredException(graph, true, "CashAccount") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		#endregion
		#endregion
		#region Public Selects
		public PXFilter<DocFilter> Filter;
		[PXFilterable]
		public PXSelect<CheckSummary> Documents;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		#endregion
		#region Events
		protected virtual void DocFilter_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter.Cache.SetDefaultExt<DocFilter.payAccountID>(e.Row);
		}
		#endregion
		#region  Delegates
		public virtual IEnumerable filter()
		{
			DocFilter locFilter = Filter.Cache.Current as DocFilter;
			if (locFilter != null)
			{
				locFilter.Balance = 0m;
				locFilter.CuryBalance = 0m;
				locFilter.CuryID = null;
				bool sameCurrency = true;
				bool isFirst = true;
				foreach (CheckSummary it in Documents.Select())
				{
					locFilter.Balance += it.DocBal;
					if (isFirst)
					{
						locFilter.CuryID = it.CuryID;
					}
					else 
					{
						if ( !((string.IsNullOrEmpty(locFilter.CuryID) && string.IsNullOrEmpty(it.CuryID)) || 
							locFilter.CuryID == it.CuryID)) 
						{
							sameCurrency = false;
						}
					}

					if (sameCurrency)
					{
						locFilter.CuryBalance += it.CuryDocBal;
					}
				
					isFirst=false;
				}
				bool hasCurrency = !(string.IsNullOrEmpty(locFilter.CuryID));
				PXUIFieldAttribute.SetVisible<DocFilter.curyID>(Filter.Cache, locFilter, sameCurrency && hasCurrency);
				PXUIFieldAttribute.SetVisible<DocFilter.curyBalance>(Filter.Cache, locFilter, sameCurrency && hasCurrency);
				PXUIFieldAttribute.SetVisible<DocFilter.balance>(Filter.Cache, locFilter, !(sameCurrency && hasCurrency)); 
			}
			yield return Filter.Cache.Current;
			Filter.Cache.IsDirty = false;
		}
		public virtual IEnumerable documents() 
		{
			DocFilter filter = Filter.Current;
			Dictionary<CashAcctKey,CheckSummary> result = new Dictionary<CashAcctKey, CheckSummary>();
			if (filter?.PayDate == null)
			{
				return result.Values;
			}
			PXSelectBase<APPaymentExt> sel = new PXSelectJoin<APPaymentExt,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<APPaymentExt.cashAccountID>>,
				InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>,				
				InnerJoin<PaymentMethod,On<PaymentMethod.paymentMethodID,Equal<APPaymentExt.paymentMethodID>>,
				LeftJoin<CABatchDetail,On<CABatchDetail.origModule,Equal<GL.BatchModule.moduleAP>,
						And<CABatchDetail.origDocType,Equal<APPayment.docType>,
						And<CABatchDetail.origRefNbr,Equal<APPayment.refNbr>>>>>>>>,				
				Where<APPaymentExt.released, Equal<False>,				
				And<APPayment.docType, NotEqual<APDocType.prepayment>, 
						And<APPayment.docType, NotEqual<APDocType.refund>,
				
				And<APPaymentExt.openDoc, Equal<True>,
				And2<Match<Vendor, Current<AccessInfo.userName>>,
				And<Where2<Where<PaymentMethod.aPCreateBatchPayment,Equal<True>, And<CABatchDetail.batchNbr,IsNull>>,
                        Or<Where<PaymentMethod.aPCreateBatchPayment, Equal<False>,
                                    And<PaymentMethod.aPPrintChecks, Equal<True>,
										And<APPayment.printed, Equal<False>>>>>>>>>>>>,
				OrderBy<
					Asc<APPaymentExt.docType,
					Asc<APPaymentExt.refNbr>>> 
				>(this);

			if (filter.PayDate != null)
			{
				sel.WhereAnd<Where<APPaymentExt.docDate, LessEqual<Current<DocFilter.payDate>>>>();
			}

			if (filter.PayAccountID != null) 
			{
				sel.WhereAnd<Where<APPaymentExt.cashAccountID, Equal<Current<DocFilter.payAccountID>>>>();
			}

			if (filter.PayTypeID != null)
			{
				sel.WhereAnd<Where<APPaymentExt.paymentMethodID, Equal<Current<DocFilter.payTypeID>>>>();
			}

			APPaymentKey lastInvoice = null;
			foreach (PXResult<APPaymentExt, CashAccount, Vendor, PaymentMethod, CABatchDetail> it in sel.Select()) 
			{
				APPaymentExt inv =  (APPaymentExt) it;
				CashAccount acct = (CashAccount)it;
				APPaymentKey invNbr = new APPaymentKey(inv.DocType, inv.RefNbr);
				if (lastInvoice != null && lastInvoice.CompareTo(invNbr) == 0) 
					continue; //Skip multiple entries for invoice
				//inv.DocCount = it.RowCount;
				lastInvoice = invNbr;
				CashAcctKey key = new CashAcctKey(inv.CashAccountID.Value, inv.PaymentMethodID);
				CheckSummary res = null;
				if (!result.ContainsKey(key)) 
				{
					res = new CheckSummary();
					res.PayAccountID = inv.CashAccountID;
					res.PayTypeID = inv.PaymentMethodID;
					res.CuryID = acct.CuryID;
					res.CuryInfoID = inv.CuryInfoID;
					result[key] = res;					
				}
				else 
				{
					res = result[key];
				}
				Aggregate(res, inv, filter.PayDate);
			}
			return result.Values;
		}
	
		#endregion
		#region Utility Functions
		
		protected static void Aggregate(CheckSummary aRes, APPaymentExt aSrc, DateTime? aPayDate)
		{
			aRes.DocBal += aSrc.OrigDocAmt;
			aRes.CuryDocBal += aSrc.CuryOrigDocAmt;
			aRes.DocCount ++;
			aRes.PayDate = aPayDate;

			if (aSrc.DocDate< aPayDate)
			{
				aRes.OverdueDocCount ++;
				aRes.OverdueDocBal += aSrc.OrigDocAmt;
				aRes.OverdueCuryDocBal += aSrc.CuryOrigDocAmt;
			}

			if (aRes.MaxPayDate == null || aSrc.DocDate > aRes.MaxPayDate)
				aRes.MaxPayDate = aSrc.DocDate;
			if (aRes.MinPayDate == null || aSrc.DocDate < aRes.MinPayDate)
				aRes.MinPayDate = aSrc.DocDate;
		}
		#endregion
	}
}
