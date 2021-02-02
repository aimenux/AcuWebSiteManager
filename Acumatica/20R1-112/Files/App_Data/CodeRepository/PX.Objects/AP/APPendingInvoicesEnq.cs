using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.BQLConstants;
using PX.Objects.CS;

namespace PX.Objects.AP
{


	public class Pair<FT, ST> : IComparable<Pair<FT, ST>>, IEquatable<Pair<FT, ST>>
		where FT : IComparable<FT>
		where ST : IComparable<ST>
	{
		public Pair(FT aFirst, ST aSecond)
		{
			this.first = aFirst;
			this.second = aSecond;
		}
		public FT first;
		public ST second;

		#region IComparable Pair<FT, ST> Members
		public virtual int CompareTo(Pair<FT, ST> other)
		{
			int res = this.first.CompareTo(other.first);
			if (res == 0) return (this.second.CompareTo(other.second));
			return res;
		}

		#endregion

		#region IEquatable<Pair<FT,ST>> Members
		public override int GetHashCode()
		{
			return 0;
		} 

		public virtual bool Equals(Pair<FT, ST> other)
		{
			return (this.CompareTo(other)==0);
		}

		public override bool Equals(Object other)
		{
			Pair<FT, ST> tmp = other as Pair<FT, ST>;
			if(tmp!=null)
				return (this.CompareTo(tmp) == 0);
			return ((Object)this).Equals(other);
		}

		#endregion
	}

    public class Triplet<T1, T2, T3> : IComparable<Triplet<T1, T2, T3>>, IEquatable<Triplet<T1, T2, T3>>
        where T1 : IComparable<T1>
        where T2 : IComparable<T2>
        where T3 : IComparable<T3>
    {
        public Triplet(T1 aArg1, T2 aArg2, T3 aArg3)
        {
            this.first = aArg1;
            this.second = aArg2;
            this.third = aArg3;
        }
        public T1 first;
        public T2 second;
        public T3 third;

        #region IComparable<Triplet<T1,T2,T3>> Members
        public virtual int CompareTo(Triplet<T1,T2,T3> other)
        {
            int res = this.first.CompareTo(other.first);
            if (res == 0) 
                res = this.second.CompareTo(other.second);
            if (res == 0)
                return this.third.CompareTo(other.third);
            return res;
        }

        #endregion

        #region IEquatable<Triplet<T1,T2,T3>> Members
        public override int GetHashCode()
        {
            return 0;
        }

        public virtual bool Equals(Triplet<T1,T2,T3> other)
        {
            return (this.CompareTo(other) == 0);
        }

        public override bool Equals(Object other)
        {
            Triplet<T1, T2, T3> tmp = other as Triplet<T1, T2, T3>;
            if (tmp != null)
                return (this.CompareTo(tmp) == 0);
            return ((Object)this).Equals(other);
        }

        #endregion
    }

	public class Quadplet<T1, T2, T3, T4> : IComparable<Quadplet<T1, T2, T3, T4>>, IEquatable<Quadplet<T1, T2, T3, T4>>
		where T1 : IComparable<T1>
		where T2 : IComparable<T2>
		where T3 : IComparable<T3>
		where T4 : IComparable<T4>
	{
		public Quadplet(T1 aArg1, T2 aArg2, T3 aArg3, T4 aArg4)
		{
			this.first = aArg1;
			this.second = aArg2;
			this.third = aArg3;
			this.fourth = aArg4;
		}
		public T1 first;
		public T2 second;
		public T3 third;
		public T4 fourth;

		#region IComparable<Quadplet<T1,T2,T3,T4>> Members
		public virtual int CompareTo(Quadplet<T1, T2, T3, T4> other)
		{
			int res = this.first.CompareTo(other.first);
			if (res == 0)
				res = this.second.CompareTo(other.second);
			if (res == 0)
				res = this.third.CompareTo(other.third);
			if (res == 0)
				return this.fourth.CompareTo(other.fourth);
			return res;
		}

		#endregion

		#region IEquatable<Quadplet<T1,T2,T3>> Members
		public override int GetHashCode()
		{
			return 0;
		}

		public virtual bool Equals(Quadplet<T1, T2, T3,T4> other)
		{
			return (this.CompareTo(other) == 0);
		}

		public override bool Equals(Object other)
		{
			Quadplet<T1, T2, T3, T4> tmp = other as Quadplet<T1, T2, T3, T4>;
			if (tmp != null)
				return (this.CompareTo(tmp) == 0);
			return ((Object)this).Equals(other);
		}

		#endregion
	}

	[PX.Objects.GL.TableAndChartDashboardType]   
    [Serializable]
	public class APPendingInvoicesEnq : PXGraph<APPendingInvoicesEnq>
	{
		#region InternalTypes
        [Serializable]
		public partial class PendingInvoiceFilter: IBqlTable
		{
			#region PayDate
			public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
			protected DateTime? _PayDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Pay Date", Visibility = PXUIVisibility.Visible)]
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
			#region PayAccountID
			public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
			protected Int32? _PayAccountID;
			[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible)]
			[PXDefault()]
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
			[PXDefault()]
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,Where<PaymentMethod.useForAP,Equal<True>>>))]
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
			[PXDBCury(typeof(PendingInvoiceFilter.curyID))]
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

        [Serializable]
		public partial class PendingPaymentSummary: IBqlTable
		{
			public PendingPaymentSummary() 
			{
				this.ClearValues();
			}

			#region PayAccountID
			public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
			protected Int32? _PayAccountID;
			[CashAccount(DisplayName = "Cash Account", DescriptionField =typeof(CashAccount.descr),Visibility = PXUIVisibility.SelectorVisible)]
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
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.useForAP,Equal<True>>>), DescriptionField = typeof(CA.PaymentMethod.descr))]
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
			[PXCurrency(typeof(PendingPaymentSummary.curyInfoID), typeof(PendingPaymentSummary.docBal), BaseCalc = false)]
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
			[PXDBBaseCury()]
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
			#region CuryDiscBal
			public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
			protected Decimal? _CuryDiscBal;

			[PXCurrency(typeof(PendingPaymentSummary.curyInfoID), typeof(PendingPaymentSummary.discBal), BaseCalc = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Cash Discount Amount",Visible =false)]
			public virtual Decimal? CuryDiscBal
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
			public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
			protected Decimal? _DiscBal;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Cash Discount Amount",Visible =false)]
			public virtual Decimal? DiscBal
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
			[PXUIField(DisplayName = "Max. Pay Date")]
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
			[PXUIField(DisplayName = "Overdue Documents", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
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
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overdue Documents Amount", Visible = false)]
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
			[PXCurrency(typeof(PendingPaymentSummary.curyInfoID), typeof(PendingPaymentSummary.overdueDocBal), BaseCalc = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overdue Documents Amount", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
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
			#region ValidDiscCount
			public abstract class validDiscCount : PX.Data.BQL.BqlInt.Field<validDiscCount> { }
			protected int? _ValidDiscCount;
			[PXInt()]
			[PXUIField(DisplayName = "Valid Discount Documents", Visible = true)]
			public virtual int? ValidDiscCount
			{

				get
				{
					return this._ValidDiscCount;
				}
				set
				{
					this._ValidDiscCount = value;
				}
			}
			#endregion
			#region ValidDiscBal
			public abstract class validDiscBal : PX.Data.BQL.BqlDecimal.Field<validDiscBal> { }
			protected Decimal? _ValidDiscBal;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Discount Valid",Visible=false)]
			public virtual Decimal? ValidDiscBal
			{
				get
				{
					return this._ValidDiscBal;
				}
				set
				{
					this._ValidDiscBal = value;
				}
			}
			#endregion
			#region ValidCuryDiscBal
			public abstract class validCuryDiscBal : PX.Data.BQL.BqlDecimal.Field<validCuryDiscBal> { }
			protected Decimal? _ValidCuryDiscBal;
			[PXCurrency(typeof(PendingPaymentSummary.curyInfoID), typeof(PendingPaymentSummary.validDiscBal), BaseCalc = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Valid Discount Amount", Visible = true)]
			public virtual Decimal? ValidCuryDiscBal
			{
				get
				{
					return this._ValidCuryDiscBal;
				}
				set
				{
					this._ValidCuryDiscBal = value;
				}
			}
				#endregion
			#region LostDiscCount
			public abstract class lostDiscCount : PX.Data.BQL.BqlInt.Field<lostDiscCount> { }
			protected int? _LostDiscCount;
			[PXInt()]
			[PXUIField(DisplayName = "Lost Discount Documents", Visible = false)]
			public virtual int? LostDiscCount
			{

				get
				{
					return this._LostDiscCount;
				}
				set
				{
					this._LostDiscCount = value;
				}
			}
			#endregion
			#region LostDiscBal
			public abstract class lostDiscBal : PX.Data.BQL.BqlDecimal.Field<lostDiscBal> { }
			protected Decimal? _LostDiscBal;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Lost Discounts",Visible = false)]
			public virtual Decimal? LostDiscBal
			{
				get
				{
					return this._LostDiscBal;
				}
				set
				{
					this._LostDiscBal = value;
				}
			}
			#endregion
			#region LostCuryDiscBal
			public abstract class lostCuryDiscBal : PX.Data.BQL.BqlDecimal.Field<lostCuryDiscBal> { }
			protected Decimal? _LostCuryDiscBal;
			[PXCurrency(typeof(PendingPaymentSummary.curyInfoID), typeof(PendingPaymentSummary.lostDiscBal), BaseCalc = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Lost Discount Amount", Visible = true)]
			public virtual Decimal? LostCuryDiscBal
			{
				get
				{
					return this._LostCuryDiscBal;
				}
				set
				{
					this._LostCuryDiscBal = value;
				}
			}
#endregion
			protected virtual void ClearValues() 
			{
				this._DocCount = 0;
				this._CuryDocBal = 0m;
				this._DocBal = 0m;
				this._DiscBal = 0m;
				this._CuryDiscBal = 0m;

				this._LostDiscCount = 0;
				this._LostDiscBal = 0m;
				this._LostCuryDiscBal = 0m;
				
				this._OverdueDocCount = 0;
				this._OverdueDocBal = 0m;
				this._OverdueCuryDocBal = 0m;
				
				this._ValidDiscCount = 0;
				this._ValidDiscBal = 0m;
				this._ValidCuryDiscBal = 0m;
				
				
			}
		}
		public struct CashAcctKey : IComparable<CashAcctKey>
		{
			public CashAcctKey(int aAcctID, string aPayTypeID)
			{
				this.AccountID = aAcctID;
				this.PaymentMethodID = aPayTypeID;
			}

			public int AccountID;
			public string PaymentMethodID;

			#region IComparable<CashAcctKey> Members

			int IComparable<CashAcctKey>.CompareTo(CashAcctKey other)
			{
				if (this.AccountID == other.AccountID)
					return (this.PaymentMethodID.CompareTo(other.PaymentMethodID));
				return Math.Sign(this.AccountID - other.AccountID);
			}

			#endregion
		}
		public class APInvoiceKey : Pair<string, string> 
		{
			public APInvoiceKey(string aFirst, string aSecond) : base(aFirst, aSecond) { }
		}
			
		#endregion

		#region Views/Selects

		public PXFilter<PendingInvoiceFilter> Filter;
		[PXFilterable]
		public PXSelect<PendingPaymentSummary> Documents;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		public virtual IEnumerable filter()
		{
			if (this.Filter.Cache != null)
			{
				PendingInvoiceFilter locFilter = this.Filter.Cache.Current as PendingInvoiceFilter;
				locFilter.Balance = 0m;
				locFilter.CuryBalance = 0m;
				locFilter.CuryID = null;
				bool sameCurrency = true;
				bool isFirst = true;
				foreach (PendingPaymentSummary it in this.Documents.Select())
				{
					locFilter.Balance += it.DocBal;
					if (isFirst)
					{
						locFilter.CuryID = it.CuryID;
					}
					else
					{
						if (!((string.IsNullOrEmpty(locFilter.CuryID) && string.IsNullOrEmpty(it.CuryID)) ||
							locFilter.CuryID == it.CuryID))
						{
							sameCurrency = false;
						}
					}

					if (sameCurrency)
					{
						locFilter.CuryBalance += it.CuryDocBal;
					}

					isFirst = false;
				}
				bool hasCurrency = !(string.IsNullOrEmpty(locFilter.CuryID));
				PXUIFieldAttribute.SetVisible<PendingInvoiceFilter.curyID>(this.Filter.Cache, locFilter, sameCurrency && hasCurrency);
				PXUIFieldAttribute.SetVisible<PendingInvoiceFilter.curyBalance>(this.Filter.Cache, locFilter, sameCurrency && hasCurrency);
				PXUIFieldAttribute.SetVisible<PendingInvoiceFilter.balance>(this.Filter.Cache, locFilter, !(sameCurrency && hasCurrency));
			}
			yield return this.Filter.Cache.Current;
			this.Filter.Cache.IsDirty = false;
		}
		public virtual IEnumerable documents()
		{
			PendingInvoiceFilter filter = Filter.Current;
			Dictionary<CashAcctKey, PendingPaymentSummary> result = new Dictionary<CashAcctKey, PendingPaymentSummary>();
			if (filter == null && !filter.PayDate.HasValue)
			{
				return result.Values;
			}
			PXSelectBase<APInvoice> sel = new PXSelectJoin<APInvoice,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<APInvoice.payAccountID>>,
				LeftJoin<APAdjust, On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
						And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>, And<APAdjust.released, Equal<BitOff>>>>,
				CrossJoin<APSetup>>>>,
				Where<Where2<Where<APInvoice.paySel, Equal<BitOn>, Or<APSetup.requireApprovePayments, Equal<False>>>,
				And2<Where<APInvoice.released, Equal<True>, Or<APInvoice.prebooked, Equal<True>>>,
				And<APInvoice.openDoc, Equal<BitOn>>>>>,
				OrderBy<
					Asc<APInvoice.docType,
					Asc<APInvoice.refNbr>>>
				>(this);
			/*if(filter.CuryID != null) 
			{
				sel.WhereAnd<Where<APInvoice.curyID, Equal<Current<PendingInvoiceFilter.curyID>>>>();
			}*/
			if (filter.PayDate != null)
			{
				sel.WhereAnd<Where<APInvoice.payDate, LessEqual<Current<PendingInvoiceFilter.payDate>>>>();
			}

			if (filter.PayAccountID != null)
			{
				sel.WhereAnd<Where<APInvoice.payAccountID, Equal<Current<PendingInvoiceFilter.payAccountID>>>>();
			}

			if (filter.PayTypeID != null)
			{
				sel.WhereAnd<Where<APInvoice.payTypeID, Equal<Current<PendingInvoiceFilter.payTypeID>>>>();
			}

			APInvoiceKey lastInvoice = null;
			foreach (PXResult<APInvoice, CurrencyInfo, CashAccount, APAdjust> it in sel.Select())
			{
				APInvoice inv = (APInvoice)it;
				CashAccount acct = (CashAccount)it;
				APAdjust adjust = (APAdjust)it;
				CurrencyInfo info = it;
				CurrencyInfo_CuryInfoID.StoreCached(new PXCommandKey(new object[] { info.CuryInfoID }, null, null, null, 0, 1, null, false, null), new List<object> { info });
				if (adjust.AdjdDocType != null)
					continue; //Skip invoices, having unreleased payments
				APInvoiceKey invNbr = new APInvoiceKey(inv.DocType, inv.RefNbr);
				if (lastInvoice != null && lastInvoice.CompareTo(invNbr) == 0)
					continue; //Skip multiple entries for invoice
				//inv.DocCount = it.RowCount;
				lastInvoice = invNbr;
				CashAcctKey key = new CashAcctKey(inv.PayAccountID.Value, inv.PayTypeID);
				PendingPaymentSummary res = null;
				if (!result.ContainsKey(key))
				{
					res = new PendingPaymentSummary();
					res.PayAccountID = inv.PayAccountID;
					res.PayTypeID = inv.PayTypeID;
					res.CuryID = acct.CuryID;
					result[key] = res;
					//Assign new CyrrencyInfo - to do conersion correctly. RateTypeID must be taken from the Cash Account
					CurrencyInfo new_info = new CurrencyInfo();
					new_info.CuryID = res.CuryID;
					new_info.CuryRateTypeID = acct.CuryRateTypeID;
					new_info.CuryEffDate = filter.PayDate;
					new_info = this.CurrencyInfo_CuryInfoID.Insert(new_info);
					res.CuryInfoID = new_info.CuryInfoID;
				}
				else
				{
					res = result[key];
				}

				APAdjust adj = new APAdjust();
				adj.VendorID = inv.VendorID;
				adj.AdjdDocType = inv.DocType;
				adj.AdjdRefNbr = inv.RefNbr;
				adj.AdjgDocType = APDocType.Check;
				adj.AdjgRefNbr = AutoNumberAttribute.GetNewNumberSymbol<APPayment.refNbr>(Caches[typeof(APPayment)], new APPayment { DocType = APDocType.Check });
				try
				{
					PaymentEntry.CalcBalances<APInvoice, APAdjust>(this.CurrencyInfo_CuryInfoID, res.CuryInfoID, filter.PayDate, inv, adj);
				}
				catch (PXRateIsNotDefinedForThisDateException ex)
				{
					Documents.Cache.RaiseExceptionHandling<PendingPaymentSummary.curyID>(res, res.CuryID, new PXSetPropertyException(ex.Message, PXErrorLevel.RowError));
				}
				Aggregate(res, new PXResult<APAdjust, APInvoice>(adj, inv), filter.PayDate);
			}
			return result.Values;
		}
	
		#endregion
		
		#region Ctor + Overrides
		public APPendingInvoicesEnq() 
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

		#region Actions
		public PXCancel<PendingInvoiceFilter> Cancel;
		public PXAction<PendingInvoiceFilter> processPayment;

		[PXUIField(DisplayName = "Process Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable ProcessPayment(PXAdapter adapter)
		{
			if (this.Documents.Current != null && this.Filter.Current!= null)
			{
				PendingPaymentSummary res = this.Documents.Current;
				PendingInvoiceFilter currentFilter = this.Filter.Current;
				APPayBills graph = PXGraph.CreateInstance<APPayBills>();
				PayBillsFilter paymentFilter = graph.Filter.Current;
				paymentFilter.PayAccountID = res.PayAccountID;
				paymentFilter.PayTypeID = res.PayTypeID;
				paymentFilter.PayDate = currentFilter.PayDate;
				graph.Filter.Update(paymentFilter);
				throw new PXRedirectRequiredException(graph, "ProcessPayment");
			}
			return Filter.Select();
		}
		#endregion
		
		#region Utility Functions
		
		public static void Aggregate(PendingPaymentSummary aRes, PXResult<APAdjust, APInvoice> aSrc, DateTime? aPayDate)
		{
			aRes.DocBal += ((APAdjust)aSrc).DocBal;
			aRes.CuryDocBal += ((APAdjust)aSrc).CuryDocBal;
			aRes.DocCount ++;
			aRes.PayDate = aPayDate;

			if (((APInvoice)aSrc).DueDate < aPayDate)
			{
				aRes.OverdueDocCount ++;
				aRes.OverdueDocBal += ((APAdjust)aSrc).DocBal;
				aRes.OverdueCuryDocBal += ((APAdjust)aSrc).CuryDocBal;
			}

			if (((APInvoice)aSrc).DiscDate < aPayDate)
			{
				aRes.LostDiscCount++;
				aRes.LostDiscBal += ((APAdjust)aSrc).DiscBal;
				aRes.LostCuryDiscBal += ((APAdjust)aSrc).CuryDiscBal;
			}
			else
			{
				aRes.ValidDiscCount++;
				aRes.ValidDiscBal += ((APAdjust)aSrc).DiscBal;
				aRes.ValidCuryDiscBal += ((APAdjust)aSrc).CuryDiscBal;
			
			}
			aRes.DiscBal = aRes.LostDiscBal + aRes.ValidDiscBal;
			aRes.CuryDiscBal = aRes.LostCuryDiscBal + aRes.ValidCuryDiscBal;
			if (aRes.MaxPayDate == null || ((APInvoice)aSrc).PayDate > aRes.MaxPayDate)
				aRes.MaxPayDate = ((APInvoice)aSrc).PayDate;
			if (aRes.MinPayDate == null || ((APInvoice)aSrc).PayDate < aRes.MinPayDate)
				aRes.MinPayDate = ((APInvoice)aSrc).PayDate;
		}

		#endregion
	}
}
