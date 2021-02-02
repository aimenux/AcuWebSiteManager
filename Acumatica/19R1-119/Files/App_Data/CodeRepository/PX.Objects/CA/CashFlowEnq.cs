using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CA
{
	[System.SerializableAttribute()]
	public class CashFlowEnq : PXGraph<CashFlowEnq>
	{
		#region Internal type definitions
		[Serializable]
		public partial class CashFlowFilter : PX.Data.IBqlTable
		{
			#region AccountID
			public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }		
			[CashAccount]
			public virtual int? CashAccountID
			{
				get;
				set;
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			protected DateTime? _StartDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
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
			#region IncludeUnassignedDocs
			public abstract class includeUnassignedDocs : PX.Data.BQL.BqlBool.Field<includeUnassignedDocs> { }
			protected Boolean? _IncludeUnassignedDocs;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include AP, AR Documents with no Cash Account Specified")]
			public virtual Boolean? IncludeUnassignedDocs
			{
				get
				{
					return this._IncludeUnassignedDocs;
				}
				set
				{
					this._IncludeUnassignedDocs = value;
				}
			}
			#endregion
			#region AllCashAccounts
			public abstract class allCashAccounts : PX.Data.BQL.BqlBool.Field<allCashAccounts> { }
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "All Cash Accounts")]
			public virtual bool? AllCashAccounts
			{ get; set; }
			#endregion
			#region DefaultAccountID
			public abstract class defaultAccountID : PX.Data.BQL.BqlInt.Field<defaultAccountID> { }
			protected Int32? _DefaultAccountID;
            [CashAccount(DisplayName = "Default Cash Account")]
			public virtual Int32? DefaultAccountID
			{
				get
				{
					return this._DefaultAccountID;
				}
				set
				{
					this._DefaultAccountID = value;
				}
			}
			#endregion
			#region IncludeUnreleased
			public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }
			protected Boolean? _IncludeUnreleased;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include Unreleased Documents")]
			public virtual Boolean? IncludeUnreleased
			{
				get
				{
					return this._IncludeUnreleased;
				}
				set
				{
					this._IncludeUnreleased = value;
				}
			}
			#endregion			
			#region IncludeUnapplied
			public abstract class includeUnapplied : PX.Data.BQL.BqlBool.Field<includeUnapplied> { }
			protected Boolean? _IncludeUnapplied;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include Unapplied Payments")]
			public virtual Boolean? IncludeUnapplied
			{
				get
				{
					return this._IncludeUnapplied;
				}
				set
				{
					this._IncludeUnapplied = value;
				}
			}
			#endregion			
			#region IncludeScheduled
			public abstract class includeScheduled : PX.Data.BQL.BqlBool.Field<includeScheduled> { }
			protected Boolean? _IncludeScheduled;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include Scheduled Documents")]
			public virtual Boolean? IncludeScheduled
			{
				get
				{
					return this._IncludeScheduled;
				}
				set
				{
					this._IncludeScheduled = value;
				}
			}
			#endregion			
			#region SummaryOnly
			public abstract class summaryOnly : PX.Data.BQL.BqlBool.Field<summaryOnly> { }
			protected Boolean? _SummaryOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Summary Only")]
			public virtual Boolean? SummaryOnly
			{
				get
				{
					return this._SummaryOnly;
				}
				set
				{
					this._SummaryOnly = value;
				}
			}
			#endregion			
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Convert To Currency",Required = false)]
			[PXDefault(typeof(Search<Account.curyID,Where<Account.accountID,Equal<Optional<CashFlowFilter.cashAccountID>>>>))]
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
			#region CuryRateTypeID
			public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
			protected String _CuryRateTypeID;
			[PXDBString(6, IsUnicode = true)]
			[PXDefault()]
			[PXSelector(typeof(CM.CurrencyRateType.curyRateTypeID))]
			[PXUIField(DisplayName = "Convert Curr. Rate Type")]
			public virtual String CuryRateTypeID
			{
				get
				{
					return this._CuryRateTypeID;
				}
				set
				{
					this._CuryRateTypeID = value;
				}
			}
			#endregion
			public bool StrictAccountCondition = false;
		}

		public enum EntityType 
		{
			BAccountID,
			CashForecastTranID
		}
		
		public class FlowKey : Quadplet<int, int, int,int> 
		{
			public FlowKey(CashFlowForecastRecordType.RecordType recordType, int acctID, EntityType entityType, int entityID)
				: base(acctID, (int)recordType, (int)entityType, entityID)
			{
			}
			public FlowKey(int acctID, APInvoice doc) : this(CashFlowForecastRecordType.RecordType.CashOut, acctID, EntityType.BAccountID, doc.VendorID.Value) { }
			public FlowKey(int acctID, ARInvoice doc) : this(CashFlowForecastRecordType.RecordType.CashIn, acctID, EntityType.BAccountID, doc.CustomerID.Value) { }
			public FlowKey(int acctID, Light.APInvoice doc) : this(CashFlowForecastRecordType.RecordType.CashOut, acctID, EntityType.BAccountID, doc.VendorID.Value) { }
			public FlowKey(int acctID, Light.ARInvoice doc) : this(CashFlowForecastRecordType.RecordType.CashIn, acctID, EntityType.BAccountID, doc.CustomerID.Value) { }
			[Obsolete("The constructor is obsolete. It will be removed in 2020R2. Please use FlowKey(APPayment, bool) instead.")]
			public FlowKey(APPayment doc) : this(CashFlowForecastRecordType.RecordType.CashOut, doc.CashAccountID.Value, EntityType.BAccountID, doc.VendorID.Value) { }
			public FlowKey(APPayment doc, bool isApplied) 
								 :this(isApplied ? CashFlowForecastRecordType.RecordType.CashOut : CashFlowForecastRecordType.RecordType.CashOutUnapplied,
																doc.CashAccountID.Value, EntityType.BAccountID, doc.VendorID.Value) { }
			public FlowKey(ARPayment doc, bool isApplied)
								: this(isApplied ? CashFlowForecastRecordType.RecordType.CashIn: CashFlowForecastRecordType.RecordType.CashInUnapplied, 
																doc.CashAccountID.Value, EntityType.BAccountID, doc.CustomerID.Value) { }
			public FlowKey(CashForecastTran doc)
				: this(doc.DrCr == DrCr.Credit ? CashFlowForecastRecordType.RecordType.CashIn : CashFlowForecastRecordType.RecordType.CashOut,
						doc.CashAccountID.Value, 
						EntityType.CashForecastTranID, 
						doc.TranID.Value) { } 
		}
		#endregion

		#region Buttons
		public PXCancel<CashFlowFilter> Cancel;
		public PXAction<CashFlowFilter> ViewReport;
		[PXUIField(DisplayName = Messages.ViewAsReport, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable viewReport(PXAdapter adapter)
		{
			PXReportResultset resultset = new PXReportResultset(typeof(CashFlowForecast));
			PXResultset<CashFlowForecast> set = this.CashFlow.Select();
			Dictionary<int, string> dict = new Dictionary<int, string>();
			PXSelectBase<CashAccount> acctSelect = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(this);
			
			set.Sort((PXResult<CashFlowForecast> o1, PXResult<CashFlowForecast> o2) =>
			{
				CashFlowForecast op2 = o2;
				CashFlowForecast op1 = o1;
				int acctCompare = (op1.CashAccountID.Value > op2.CashAccountID.Value) ? 1 :
									(op1.CashAccountID.Value < op2.CashAccountID.Value) ? -1 : 0;
				if( acctCompare !=0) 
				{
					string acctCD1 = String.Empty;
					string acctCD2 = String.Empty;
					if (op1.CashAccountID.HasValue) 
					{
						if( !dict.TryGetValue(op1.CashAccountID.Value, out acctCD1)) 
						{
							CashAccount acct = acctSelect.Select(op1.CashAccountID.Value);
							if(acct!= null)
                                acctCD1 = acct.CashAccountCD;
							dict.Add(op1.CashAccountID.Value, acctCD1);
						}
					}
					if (op2.CashAccountID.HasValue)
					{
						if (!dict.TryGetValue(op2.CashAccountID.Value, out acctCD2))
						{
							CashAccount acct = acctSelect.Select(op2.CashAccountID.Value);
							if (acct != null)
                                acctCD2 = acct.CashAccountCD;
							dict.Add(op2.CashAccountID.Value, acctCD2);
						}
					}
					acctCompare = String.Compare(acctCD1, acctCD2);
				}				
				int res = ( acctCompare !=0 ? acctCompare :
						(op1.RecordType.Value != op2.RecordType.Value ?
							(op1.RecordType == 0 ? -1 :
							(op2.RecordType.Value == 0 ? 1 : op2.RecordType.Value - op1.RecordType.Value)) :
							(op1.BAccountID.HasValue != op2.BAccountID.HasValue ?
								(op1.BAccountID.HasValue ? -1 : 1)
								: 0)));
				return res;
			});
			foreach (CashFlowForecast it in set)
			{
				resultset.Add(it);
			}
			throw new PXReportRequiredException(resultset, this.CashFlowReportName, Messages.CashForecastReport);
		}

		public PXAction<CashFlowFilter> ViewReport2;
		[PXUIField(DisplayName = Messages.ViewAsTabReport, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible=false)]
		[PXLookupButton()]
		public virtual IEnumerable viewReport2(PXAdapter adapter)
		{
			List<CashFlowForecast2> set = new List<CashFlowForecast2>();
			PXReportResultset resultset = new PXReportResultset(typeof(CashFlowForecast2));
			foreach (CashFlowForecast it in this.CashFlow.Select()) 
			{
				Convert(set, it,true);
			}
			set.Sort((CashFlowForecast2 op1, CashFlowForecast2 op2) =>
			{
				return ((op1.TranDate.Value > op2.TranDate.Value) ? 1 
					: ((op1.TranDate.Value < op2.TranDate.Value) ? -1 :
					((op1.CashAccountID.Value > op2.CashAccountID.Value) ? 1 :
						(op1.CashAccountID.Value < op2.CashAccountID.Value) ? -1 :
						(op1.RecordType.Value != op2.RecordType.Value ?
							(op1.RecordType == 0 ? -1 :
							(op2.RecordType.Value == 0 ? 1 : op2.RecordType.Value - op1.RecordType.Value)) :
							(op1.BAccountID.HasValue != op2.BAccountID.HasValue ?
								(op1.BAccountID.HasValue ? -1 : 1)
								: 0)))));					
			});
 
			foreach (CashFlowForecast2 it in set)
			{
				resultset.Add(it);
			} 

			throw new PXReportRequiredException(resultset, "CA660012", Messages.CashForecastReport);
		} 
		#endregion

		#region Ctor + Public Selects

		public CashFlowEnq() 
		{
			this.CashFlow.Cache.AllowDelete = false;
			this.CashFlow.Cache.AllowInsert = false;
			this.CashFlow.Cache.AllowUpdate = false;
		}
		public PXFilter<CashFlowFilter> Filter;		
		[PXFilterable]
		public PXSelect<CashFlowForecast> CashFlow;
		
		public PXSelect<Light.ARInvoice,Where<Light.ARInvoice.docType, Equal<Required<Light.ARInvoice.docType>>,And<Light.ARInvoice.refNbr,Equal<Required<Light.ARInvoice.refNbr>>>>> arInvoice;
		public PXSelect<Light.APInvoice, Where<Light.APInvoice.docType, Equal<Required<Light.APInvoice.docType>>, And<Light.APInvoice.refNbr, Equal<Required<Light.APInvoice.refNbr>>>>> apInvoice;
		public PXSelect<Schedule, Where<Schedule.scheduleID,NotEqual<Schedule.scheduleID>>> schedule;


		#endregion

		#region Events
		protected virtual void CashFlowFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CashFlowFilter row = (CashFlowFilter)e.Row;
			if (row != null)
			{
				if (row.StartDate.HasValue)
				{
					PXCache cache = this.Caches[typeof(CashFlowForecast)];
					bool showDetails = (row.SummaryOnly == false);
					const string fieldNameBase = "CuryAmountDay";
					for (int iCol = 0; iCol < this.AmountFieldsNumber; iCol++)
					{
						DateTime iDate = row.StartDate.Value.AddDays(iCol);
						string fieldName = fieldNameBase + iCol.ToString();
						string displayName = PXUIFieldAttribute.GetDisplayName(cache, fieldName);
						string newDisplayName = iDate.ToString(Messages.DateFormat);
						if (newDisplayName != displayName)
						{
							PXUIFieldAttribute.SetDisplayName(cache, fieldName, iDate.ToString(Messages.DateFormat));
							PXUIFieldAttribute.SetNeutralDisplayName(cache, fieldName, " ");

						}
						PXUIFieldAttribute.SetVisible(cache, fieldName, showDetails|| iCol==0);
					}
				}
				PXUIFieldAttribute.SetEnabled<CashFlowFilter.defaultAccountID>(sender, row, row.IncludeUnassignedDocs == true && !row.CashAccountID.HasValue);
				PXUIFieldAttribute.SetRequired<CashFlowFilter.defaultAccountID>(sender,row.IncludeUnassignedDocs == true && !row.CashAccountID.HasValue);
				bool convertToCury =(string.IsNullOrEmpty(row.CuryID) == false);
				PXUIFieldAttribute.SetEnabled<CashFlowFilter.curyRateTypeID>(sender, row, convertToCury);
				PXUIFieldAttribute.SetRequired<CashFlowFilter.curyRateTypeID>(sender, convertToCury);
				if (row.AllCashAccounts == true)
				{
					EnableAccountId(sender, row, false);
				}
				else
				{
					EnableAccountId(sender, row, true);
				}
			}
		}

		private void EnableAccountId(PXCache sender, CashFlowFilter row, bool enable)
		{
			PXUIFieldAttribute.SetEnabled<CashFlowFilter.cashAccountID>(sender, row, enable);
			PXUIFieldAttribute.SetRequired<CashFlowFilter.cashAccountID>(sender, enable);
		}

		protected virtual void CashFlowFilter_AllCashAccounts_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CashFlowFilter row = e.Row as CashFlowFilter;
			if (row.AllCashAccounts == true)
			{
				row.CashAccountID = null;
			}
		}

		protected virtual void CashFlowFilter_IncludeScheduled_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) 
		{
			CashFlowFilter row = (CashFlowFilter)e.Row;
			if ((bool)e.NewValue == true) 			
			{			
				this.startDate = row.StartDate ?? DateTime.Now;
				this.endDate = startDate.AddDays(this.AmountFieldsNumber);
				if(DetectPotentialScheduleBreak(endDate))
				{
					throw new PXSetPropertyException(Messages.FinPeriodsAreNotDefinedForDateRangeProvided);
				}
			}
		}

		protected virtual void CashFlowFilter_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e) 
		{
			CashFlowFilter row = (CashFlowFilter)e.Row;
			if (row != null && string.IsNullOrEmpty(row.CuryID) == false)
			{
				PXResult<CashAccount> max = null;
				foreach (PXResult<CashAccount> it in PXSelectGroupBy<CashAccount, Where<CashAccount.curyRateTypeID, IsNotNull>, Aggregate<GroupBy<CashAccount.curyRateTypeID, Count>>>.Select(this))
				{
					if (max == null || max.RowCount < it.RowCount) max = it;
				}
				if (max != null)
				{
					e.NewValue = ((CashAccount)max).CuryRateTypeID;
					e.Cancel = true;
				}
				else 
				{
					CMSetup cmSetup = PXSelect<CMSetup>.Select(this);
					e.NewValue = cmSetup != null ? cmSetup.CARateTypeDflt : null;
					e.Cancel = true;
				}
			}
			else 
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void CashFlowFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) 
		{
			sender.SetDefaultExt<CashFlowFilter.curyRateTypeID>(e.Row);
		}
		#endregion

		#region Select Delegate + Supporting functions
		public IEnumerable cashFlow()
		{
			CashFlowFilter filter = this.Filter.Current;

			if (filter == null || (filter.IncludeUnassignedDocs == true && filter.CashAccountID == null && filter.DefaultAccountID == null))
				yield break;
			if (filter.CashAccountID == null && filter.AllCashAccounts == false)
				yield break;
			this.startDate = filter.StartDate ?? DateTime.Now;
			this.endDate = startDate.AddDays(this.AmountFieldsNumber);
			if (filter.IncludeScheduled == true && this.DetectPotentialScheduleBreak(endDate))
			{
				yield break;
			}
			if (string.IsNullOrEmpty(filter.CuryID) == false && string.IsNullOrEmpty(filter.CuryRateTypeID))
				yield break;

			this.baseCurrency = PXSelectJoin<Currency, InnerJoin<Company, On<Currency.curyID, Equal<Company.baseCuryID>>>>.Select(this);
			this.defaultCashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, filter.CashAccountID ?? filter.DefaultAccountID);
		
			if(String.IsNullOrEmpty(filter.CuryID) == false)
			{
				this.convertToCurrency = PXSelectReadonly<Currency,Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(this, filter.CuryID);
				this.currencyRateType = filter.CuryRateTypeID; 
			}

			Dictionary<int, CashFlowForecast> accountBalances = new Dictionary<int, CashFlowForecast>(1);
			this.accountRates = new Dictionary<int, CurrencyRate>(1);
			this.currencyRates = new Dictionary<string, CurrencyRate>(1);

			CalcCashAccountBalance(startDate, filter, accountBalances);
			Dictionary<FlowKey, CashFlowForecast> incomingFlow = new Dictionary<FlowKey, CashFlowForecast>(1);

			RetrieveAPInvoices(incomingFlow, filter);
			
			if (filter.IncludeScheduled == true)
			{
				RetrieveAPScheduled(incomingFlow, filter);				
			}
			RetrieveAPPayments(incomingFlow, filter);

			RetrieveARInvoices(incomingFlow, filter);
			if (filter.IncludeScheduled == true)
			{
				RetrieveARScheduled(incomingFlow, filter);
			}
			RetrieveARPayments(incomingFlow, filter);

			RetriveCashForecasts(incomingFlow, filter);

			//Accumulate values
			RecalcSummmary(accountBalances, incomingFlow);	

			foreach (CashFlowForecast row in accountBalances.Values)
			{
				yield return row;
			}

			foreach (CashFlowForecast it in incomingFlow.Values)
			{
				if (it.IsZero()) continue;
				yield return it;
			}
		}

		protected virtual void RetrieveARInvoices(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{
			PXSelectBase<Light.ARInvoice> arInvSelect = new PXSelectReadonly2<Light.ARInvoice,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<Light.ARInvoice.curyInfoID>>,
							LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<Light.ARInvoice.cashAccountID>>>>,
							Where<Light.ARInvoice.dueDate, Less<Required<Light.ARInvoice.dueDate>>,
								And<Light.ARInvoice.docType, NotEqual<ARDocType.cashSale>,
								And<Light.ARInvoice.docType, NotEqual<ARDocType.cashReturn>,
								And<Light.ARInvoice.voided, Equal<False>,
								And<Light.ARInvoice.scheduled, Equal<False>,
								And<Light.ARInvoice.openDoc, Equal<True>>>>>>>>(this);
			if (filter.IncludeUnreleased != true)
			{
				arInvSelect.WhereAnd<Where<Light.ARInvoice.released, Equal<True>>>();
			}

			if (filter.CashAccountID.HasValue)
			{
				if (filter.StrictAccountCondition)
				{
					if (filter.IncludeUnassignedDocs == true)
					{
						arInvSelect.WhereAnd<Where<Where<Light.ARInvoice.cashAccountID, Equal<Required<Light.ARInvoice.cashAccountID>>,
												Or<Light.ARInvoice.cashAccountID, IsNull>>>>();
					}
					else
					{
						arInvSelect.WhereAnd<Where<Light.ARInvoice.cashAccountID, Equal<Required<Light.ARInvoice.cashAccountID>>>>();
					}
				}
				else
				{
					arInvSelect.WhereAnd<Where<Where<Light.ARInvoice.cashAccountID, Equal<Required<Light.ARInvoice.cashAccountID>>,
											Or<Light.ARInvoice.cashAccountID, IsNull>>>>();
				}
			}

			foreach (PXResult<Light.ARInvoice, CurrencyInfo, CashAccount> iRes in arInvSelect.Select(endDate, filter.CashAccountID))
			{
				Light.ARInvoice iDoc = iRes;
				CurrencyInfo iCuryInfo = iRes;
				CashFlowForecast rec = null;
				CashAccount iAccount = iRes;
				if (iAccount.AccountID.HasValue == false)
				{
					iAccount = findDefaultCashAccount(iDoc);
				}
				if ((iAccount == null || iAccount.AccountID == null))
				{
					if ((filter.IncludeUnassignedDocs == false || (filter.DefaultAccountID == null && filter.CashAccountID == null))) continue;
					iAccount = defaultCashAccount;
				}
				if (filter.CashAccountID.HasValue && filter.CashAccountID != iAccount.CashAccountID) continue; //If account specifeid - skip records which are defaulted to another account

				//if((iAccount == null || iAccount.AccountID == null) && (filter.IncludeUnassignedDocs == false || filter.AccountID == null)) continue;  
				FlowKey key = new FlowKey(iAccount.CashAccountID.Value, iDoc);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iAccount, iDoc);
					flow.Add(key, rec);
				}

				Decimal sign = (iDoc.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;
				Decimal? curyValue = iDoc.CuryDocBal;
				Decimal? value = iDoc.DocBal;		

				rec.CuryID = this.ConvertDocAmount(ref curyValue, ref value, iCuryInfo, iAccount, filter);
				AddAmount(rec, iDoc.DueDate.Value, curyValue, value, sign);
			}
		}
		protected virtual void RetrieveAPInvoices(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{
			PXSelectBase<Light.APInvoice> apInvSelect = new PXSelectReadonly2<Light.APInvoice,
									InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<Light.APInvoice.curyInfoID>>,
									LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<Light.APInvoice.payAccountID>>>>,
									Where<Light.APInvoice.docType, NotEqual<APDocType.quickCheck>,
										And<Light.APInvoice.docType, NotEqual<APDocType.voidQuickCheck>,
										And<Light.APInvoice.voided, Equal<False>,
										And<Light.APInvoice.scheduled, Equal<False>,
										And<Light.APInvoice.openDoc, Equal<True>,
										And<Where<Light.APInvoice.dueDate, Less<Required<Light.APInvoice.dueDate>>,
											Or<Light.APInvoice.payDate,Less<Required<Light.APInvoice.dueDate>>>>>>>>>>>(this);
			if (filter.IncludeUnreleased != true)
			{
				apInvSelect.WhereAnd<Where<Light.APInvoice.released, Equal<True>>>();
			}

			if (filter.CashAccountID.HasValue)
			{
				if (filter.StrictAccountCondition)
				{
					if (filter.IncludeUnassignedDocs == true)
					{

						apInvSelect.WhereAnd<Where<Where<Light.APInvoice.payAccountID, Equal<Required<Light.APInvoice.payAccountID>>,
												Or<Light.APInvoice.payAccountID, IsNull>>>>();
					}
					else
					{
						apInvSelect.WhereAnd<Where<Light.APInvoice.payAccountID, Equal<Required<Light.APInvoice.payAccountID>>>>();
					}
				}
				else
				{
					apInvSelect.WhereAnd<Where<Where<Light.APInvoice.payAccountID, Equal<Required<Light.APInvoice.payAccountID>>,
														Or<Light.APInvoice.payAccountID, IsNull>>>>();
				}
			}

			foreach (PXResult<Light.APInvoice, CurrencyInfo, CashAccount> iRes in apInvSelect.Select(endDate, endDate, filter.CashAccountID))
			{
				Light.APInvoice iDoc = iRes;
				CashFlowForecast rec = null;
				CurrencyInfo iCuryInfo = iRes;
				CashAccount iAccount = iRes;
				if (iAccount.AccountID.HasValue == false)
				{
					iAccount = findDefaultCashAccount(iDoc);
				}
				if ((iAccount == null || iAccount.AccountID == null))
				{
					if ((filter.IncludeUnassignedDocs == false || (filter.DefaultAccountID == null && filter.CashAccountID == null))) continue;
					iAccount = defaultCashAccount;
				}
				if (filter.CashAccountID.HasValue && filter.CashAccountID != iAccount.CashAccountID) continue; //If account specifeid - skip records which are defaulted to another account
				FlowKey key = new FlowKey(iAccount.CashAccountID.Value, iDoc);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iAccount, iDoc);

					flow.Add(key, rec);
				}
				Decimal? curyValue = iDoc.CuryDocBal;
				Decimal? value = iDoc.DocBal;
				Decimal sign = (iDoc.DrCr == DrCr.Debit) ? Decimal.One : Decimal.MinusOne;

				rec.CuryID = ConvertDocAmount(ref curyValue, ref value, iCuryInfo, iAccount, filter);
				AddAmount(rec, iDoc.PayDate.HasValue?iDoc.PayDate.Value:iDoc.DueDate.Value, curyValue, value, sign);
			}
		}

		

		protected virtual void RetrieveAPPayments(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{
			PXSelectBase<APPayment> apPaymentSelect = new PXSelectReadonly2<APPayment,
										LeftJoin<APAdjust,
										On<APAdjust.adjgDocType, Equal<APPayment.docType>,
											And<APAdjust.adjgRefNbr, Equal<APPayment.refNbr>,
											And<APAdjust.released, Equal<True>>>>>,
										Where<APPayment.docDate,
											GreaterEqual<Required<APPayment.docDate>>,
										And<APPayment.docDate, Less<Required<APPayment.docDate>>,
										And<APPayment.voided, Equal<False>,
										And<APPayment.scheduled, Equal<False>,
										And<APPayment.docType, NotEqual<APPaymentType.voidCheck>,
										And<APPayment.docType, NotEqual<APPaymentType.voidQuickCheck>>>>>>>>(this);
			if (filter.CashAccountID.HasValue)
			{
				apPaymentSelect.WhereAnd<Where<APPayment.cashAccountID, Equal<Required<APPayment.cashAccountID>>>>();
			}
			if (filter.IncludeUnreleased != true)
			{
				apPaymentSelect.WhereAnd<Where<APPayment.released, Equal<True>>>();
			}

			APPayment lastDoc = null;
			foreach (PXResult<APPayment, APAdjust> iRes in apPaymentSelect.Select(startDate, endDate, filter.CashAccountID))
			{
				APPayment iDoc = iRes;
				APAdjust iAdj = iRes;
				CashFlowForecast rec = null;
				if (iDoc.CashAccountID == null) continue;
				bool isCashSale = (iDoc.DocType == APDocType.QuickCheck || iDoc.DocType == APDocType.VoidQuickCheck);
				bool applied = isCashSale || (String.IsNullOrEmpty(iAdj.AdjdDocType) == false && String.IsNullOrEmpty(iAdj.AdjdRefNbr) == false);
				if (filter.IncludeUnapplied != true && !applied) continue; //Skip unapplied

				FlowKey key = new FlowKey(iDoc, applied);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iDoc, applied);
					flow.Add(key, rec);
				}
				AddAmount(rec, iDoc, iAdj);
				if (filter.IncludeUnapplied == true && iDoc.CuryDocBal != Decimal.Zero
					&& (lastDoc == null || lastDoc.DocType != iDoc.DocType || lastDoc.RefNbr != iDoc.RefNbr))
				{
					FlowKey key1 = new FlowKey(iDoc, false);
					if (!flow.TryGetValue(key1, out rec))
					{
						rec = Create(startDate, iDoc, false);
						flow.Add(key1, rec);
					}
					AddAmount(rec, iDoc);
				}
				lastDoc = iDoc;
			}
		}

		protected virtual void RetrieveARPayments(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{
			PXSelectBase<ARPayment> arPaymentSelect = new PXSelectReadonly2<ARPayment, LeftJoin<ARAdjust,
										On<ARAdjust.adjgDocType, Equal<ARPayment.docType>,
											And<ARAdjust.adjgRefNbr, Equal<ARPayment.refNbr>,
											And<ARAdjust.released, Equal<True>>>>>,
										Where<ARPayment.docDate,
											GreaterEqual<Required<ARPayment.docDate>>,
											And<ARPayment.voided, Equal<False>,
											And<ARPayment.scheduled, Equal<False>,
											And<ARPayment.docType, NotEqual<ARPaymentType.voidPayment>,
											And<ARPayment.docDate, Less<Required<ARPayment.docDate>>>>>>>>(this);
			if (filter.IncludeUnreleased != true)
			{
				arPaymentSelect.WhereAnd<Where<ARPayment.released, Equal<True>>>();
			}
			if (filter.CashAccountID.HasValue)
			{
				arPaymentSelect.WhereAnd<Where<ARPayment.cashAccountID, Equal<Required<ARPayment.cashAccountID>>>>();
			}

			ARPayment lastDoc = null;
			foreach (PXResult<ARPayment, ARAdjust> iRes in arPaymentSelect.Select(startDate, endDate, filter.CashAccountID))
			{
				ARPayment iDoc = iRes;
				ARAdjust iAdj = iRes;
				CashFlowForecast rec = null;
				if (iDoc.CashAccountID == null) continue;
				bool isCashSale = (iDoc.DocType == ARDocType.CashSale || iDoc.DocType == ARDocType.CashReturn);
				bool applied = isCashSale || (String.IsNullOrEmpty(iAdj.AdjdDocType) == false && String.IsNullOrEmpty(iAdj.AdjdRefNbr) == false);
				if (filter.IncludeUnapplied != true && !applied) continue; //Skip unapplied
				FlowKey key = new FlowKey(iDoc, applied);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iDoc, applied);
					flow.Add(key, rec);
				}
				//Unreleased applications
				AddAmount(rec, iDoc, iAdj);
				if (filter.IncludeUnapplied == true && iDoc.CuryDocBal != Decimal.Zero
					&& (lastDoc == null || lastDoc.DocType != iDoc.DocType || lastDoc.RefNbr != iDoc.RefNbr))
				{
					FlowKey key1 = new FlowKey(iDoc, false);
					if (!flow.TryGetValue(key1, out rec))
					{
						rec = Create(startDate, iDoc, false);
						flow.Add(key1, rec);
					}
					AddAmount(rec, iDoc);
				}
				lastDoc = iDoc;
			}
		}

		protected virtual void RetriveCashForecasts(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{
			PXSelectBase<CashForecastTran> cashForecastSelect = new PXSelectJoin<CashForecastTran, InnerJoin<CashAccount,
										On<CashAccount.cashAccountID, Equal<CashForecastTran.cashAccountID>>>,
										Where<CashForecastTran.tranDate,
											GreaterEqual<Required<APPayment.docDate>>,
										And<CashForecastTran.tranDate, Less<Required<CashForecastTran.tranDate>>>>>(this);
			if (filter.CashAccountID.HasValue)
			{
				cashForecastSelect.WhereAnd<Where<CashForecastTran.cashAccountID, Equal<Required<CashForecastTran.cashAccountID>>>>();
			}

			foreach (PXResult<CashForecastTran, CashAccount> iRes in cashForecastSelect.Select(startDate, endDate, filter.CashAccountID))
			{
				CashForecastTran iDoc = iRes;
				CashAccount iAccount = iRes;
				CashFlowForecast rec;
				FlowKey key = new FlowKey(iDoc);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iDoc);
					flow.Add(key, rec);
				}
				CurrencyRate acctRate;
				if (!this.accountRates.TryGetValue(iDoc.CashAccountID.Value, out acctRate))
				{
					acctRate = findCuryRate(iAccount.CuryID, baseCurrency.CuryID, iAccount.CuryRateTypeID, startDate);
					this.accountRates.Add(iAccount.CashAccountID.Value, acctRate);
				}
				AddAmount(rec, iDoc, acctRate, baseCurrency);
			}
		}

		protected virtual void RetrieveAPScheduled(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{

			PXSelectBase<Light.APInvoice> apScheduledInvSelect = new PXSelectReadonly2<Light.APInvoice,
									InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<Light.APInvoice.curyInfoID>>,
									LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<Light.APInvoice.payAccountID>>,
									InnerJoin<Schedule, On<Schedule.scheduleID, Equal<Light.APInvoice.scheduleID>,
										And<Schedule.active, Equal<True>>>,
									InnerJoin<CS.Terms, On<CS.Terms.termsID, Equal<Light.APInvoice.termsID>>>>>>,
									Where<Light.APInvoice.voided, Equal<False>,
										And<Light.APInvoice.scheduled, Equal<True>>>>(this);
			
			foreach (PXResult<Light.APInvoice, CurrencyInfo, CashAccount, Schedule, CS.Terms> iRes in apScheduledInvSelect.Select())
			{
				Light.APInvoice iDoc = iRes;
				CashFlowForecast rec = null;
				CurrencyInfo iCuryInfo = iRes;
				CashAccount iAccount = iRes;
				Schedule schedule = iRes;
				CS.Terms terms = iRes;
				if (iAccount.AccountID.HasValue == false)
				{
					iAccount = findDefaultCashAccount(iDoc);
				}

				if ((iAccount == null || iAccount.AccountID == null))
				{
					if ((filter.IncludeUnassignedDocs == false || (filter.DefaultAccountID == null && filter.CashAccountID == null))) continue;
					iAccount = this.defaultCashAccount;
				}

				if (filter.CashAccountID.HasValue && filter.CashAccountID != iAccount.CashAccountID) continue; //If account specifeid - skip records which are defaulted to another account


				List<ScheduleDet> toCreate = new List<ScheduleDet>();
				if (String.IsNullOrEmpty(schedule.ScheduleID) == false && (schedule.NextRunDate.HasValue && schedule.NextRunDate.Value <= endDate))
				{
					Light.APInvoice last = PXSelect<Light.APInvoice, 
										Where<Light.APInvoice.scheduled, Equal<False>, And<Light.APInvoice.scheduleID, Equal<Required<Light.APInvoice.scheduleID>>>>, OrderBy<Desc<Light.APInvoice.docDate>>>.Select(this, schedule.ScheduleID);
					DateTime? lastDate = (last!=null? last.DocDate: null); 
					Schedule copy = (Schedule)this.schedule.Cache.CreateCopy(schedule);
					try
					{
						List<ScheduleDet> schdDetails = new Scheduler(this).MakeSchedule(copy, short.MaxValue, endDate).ToList();
						toCreate = schdDetails.FindAll((ScheduleDet op) =>{
							if (lastDate.HasValue && op.ScheduledDate.Value < lastDate.Value) return false;
							DateTime? dueDate, discDate;
							CS.TermsAttribute.CalcTermsDates(terms, op.ScheduledDate, out dueDate, out discDate);
							return dueDate.HasValue && (dueDate.Value < endDate);
						});
					}
					catch (PXFinPeriodException e)
					{
						throw new PXException(e, Messages.CAFinPeriodRequiredForSheduledTransactionIsNotDefinedInTheSystem,GL.BatchModule.AP,iDoc.DocType, iDoc.RefNbr, schedule.ScheduleID); 
					}					
				}
				bool hasScheduled = (toCreate.Count > 0);
				if (!hasScheduled) continue;
				FlowKey key = new FlowKey(iAccount.CashAccountID.Value, iDoc);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iAccount, iDoc);
					flow.Add(key, rec);
				}

				Decimal? curyValue = iDoc.CuryDocBal;
				Decimal? value = iDoc.DocBal;
				Decimal sign = (iDoc.DrCr == DrCr.Debit) ? Decimal.One : Decimal.MinusOne;

				rec.CuryID = ConvertDocAmount(ref curyValue, ref value, iCuryInfo, iAccount, filter);
				foreach (ScheduleDet iSd in toCreate)
				{
					DateTime? dueDate, discDate;
					CS.TermsAttribute.CalcTermsDates(terms, iSd.ScheduledDate, out dueDate, out discDate);					
					AddAmount(rec, dueDate.Value, curyValue, value, sign);
				}
			}
		}

		protected virtual void RetrieveARScheduled(Dictionary<FlowKey, CashFlowForecast> flow, CashFlowFilter filter)
		{			
			PXSelectBase<Light.ARInvoice> apScheduledInvSelect = new PXSelectReadonly2<Light.ARInvoice,
									InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<Light.ARInvoice.curyInfoID>>,
									LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<Light.ARInvoice.cashAccountID>>,
									InnerJoin<Schedule, On<Schedule.scheduleID, Equal<Light.ARInvoice.scheduleID>,
										And<Schedule.active, Equal<True>>>,
									InnerJoin<CS.Terms, On<CS.Terms.termsID, Equal<Light.ARInvoice.termsID>>>>>>,
									Where<Light.ARInvoice.voided, Equal<False>,
										And<Light.ARInvoice.scheduled, Equal<True>>>>(this);
			foreach (PXResult<Light.ARInvoice, CurrencyInfo, CashAccount, Schedule, CS.Terms> iRes in apScheduledInvSelect.Select())
			{
				Light.ARInvoice iDoc = iRes;
				CashFlowForecast rec = null;
				CurrencyInfo iCuryInfo = iRes;
				CashAccount iAccount = iRes;
				Schedule schedule = iRes;
				CS.Terms terms = iRes;
				if (iAccount.AccountID.HasValue == false)
				{
					iAccount = findDefaultCashAccount(iDoc);
				}

				if ((iAccount == null || iAccount.AccountID == null))
				{
					if ((filter.IncludeUnassignedDocs == false || (filter.DefaultAccountID == null && filter.CashAccountID == null))) continue;
					iAccount = this.defaultCashAccount;
				}
				if (filter.CashAccountID.HasValue && filter.CashAccountID != iAccount.CashAccountID) continue; //If account specifeid - skip records which are defaulted to another account

				List<ScheduleDet> toCreate = new List<ScheduleDet>();
				if (String.IsNullOrEmpty(schedule.ScheduleID) == false && (schedule.NextRunDate.HasValue && schedule.NextRunDate.Value <= endDate))
				{
					Light.ARInvoice last = PXSelect<Light.ARInvoice,
										Where<Light.ARInvoice.scheduled, Equal<False>, And<Light.ARInvoice.scheduleID, Equal<Required<Light.ARInvoice.scheduleID>>>>, OrderBy<Desc<Light.ARInvoice.docDate>>>.Select(this, schedule.ScheduleID);
					DateTime? lastDate = (last != null ? last.DocDate : null);

					Schedule copy = (Schedule)this.schedule.Cache.CreateCopy(schedule);
					try
					{
						List<ScheduleDet> schdDetails = new Scheduler(this).MakeSchedule(copy, short.MaxValue, endDate).ToList();
						toCreate = schdDetails.FindAll((ScheduleDet op) =>
						{
							if (lastDate.HasValue && op.ScheduledDate.Value < lastDate.Value) return false;
							DateTime? dueDate, discDate;
							CS.TermsAttribute.CalcTermsDates(terms, op.ScheduledDate, out dueDate, out discDate);
							return dueDate.HasValue && (dueDate.Value < endDate);
						});
					}
					catch (PXFinPeriodException e)
					{
						throw new PXException(e, Messages.CAFinPeriodRequiredForSheduledTransactionIsNotDefinedInTheSystem, GL.BatchModule.AR, iDoc.DocType, iDoc.RefNbr, schedule.ScheduleID);
					}

				}
				bool hasScheduled = (toCreate.Count > 0);
				if (!hasScheduled) continue;
				FlowKey key = new FlowKey(iAccount.CashAccountID.Value, iDoc);
				if (!flow.TryGetValue(key, out rec))
				{
					rec = Create(startDate, iAccount, iDoc);
					flow.Add(key, rec);
				}

				Decimal? curyValue = iDoc.CuryDocBal;
				Decimal? value = iDoc.DocBal;
				Decimal sign = (iDoc.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;

				rec.CuryID = ConvertDocAmount(ref curyValue, ref value, iCuryInfo, iAccount, filter);

#if false
				if (iAccount != null && iAccount.AccountID.HasValue)
				{
					CurrencyRate acctRate = null;
					CurrencyRate docRate = findCuryRate(iDoc.CuryID, iCuryInfo.CuryID, iCuryInfo.CuryRateTypeID, startDate);
					Currency acctCurrency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(this, iAccount.CuryID);
					if (!this.accountRates.TryGetValue(iAccount.AccountID.Value, out acctRate))
					{
						acctRate = findCuryRate(iAccount.CuryID, baseCurrency.CuryID, iAccount.CuryRateTypeID, startDate);
						this.accountRates.Add(iAccount.AccountID.Value, acctRate);
					}
					ConvertAmount(ref curyValue, ref value, iDoc.CuryID, acctCurrency, baseCurrency, acctRate, docRate);
				} 
#endif

				foreach (ScheduleDet iSd in toCreate)
				{
					DateTime? dueDate, discDate;
					CS.TermsAttribute.CalcTermsDates(terms, iSd.ScheduledDate, out dueDate, out discDate);
					AddAmount(rec, dueDate.Value, curyValue, value, sign);
				}
			}
		}

		protected virtual void RecalcSummmary(Dictionary<int, CashFlowForecast> accountBalances, Dictionary<FlowKey, CashFlowForecast> flow)
		{
			foreach (CashFlowForecast row in accountBalances.Values)
			{
				Decimal curyValue = Decimal.Zero;
				Decimal baseValue = Decimal.Zero;
				for (int iCol = 1; iCol < this.AmountFieldsNumber+1; iCol++)
				{
					SetValue(row, iCol, curyValue, true);
					SetValue(row, iCol, baseValue, false);
				}
			}
			foreach (CashFlowForecast it in flow.Values)
			{
				if (it.CashAccountID.HasValue)
				{
					CashFlowForecast acctBalanceRow;
					if (!accountBalances.TryGetValue(it.CashAccountID.Value, out acctBalanceRow))
					{
						acctBalanceRow = new CashFlowForecast();
						acctBalanceRow.RecordType = (int)CashFlowForecastRecordType.RecordType.Balance;
						accountBalances.Add(it.CashAccountID.Value, acctBalanceRow);
					}
					for (int iCol = 0; iCol < (this.AmountFieldsNumber); iCol++)
					{
						Decimal? curyValue = GetValue(it, iCol, true);
						Decimal? baseValue = GetValue(it, iCol, false);
						decimal sign = it.RecordType < 0 ? Decimal.MinusOne : (it.RecordType > 0 ? Decimal.One : Decimal.Zero);
						AddValue(acctBalanceRow, iCol + 1, (curyValue ?? Decimal.Zero) * sign, true);
						AddValue(acctBalanceRow, iCol + 1, (baseValue ?? Decimal.Zero) * sign, false);
					}
				}
			}

			foreach (CashFlowForecast row in accountBalances.Values)
			{
				for (int iCol = 1; iCol < this.AmountFieldsNumber + 1; iCol++)
				{
					Decimal curyValue = GetValue(row, iCol - 1, true) ?? Decimal.Zero;
					Decimal baseValue = GetValue(row, iCol - 1, false) ?? Decimal.Zero;
				
					AddValue(row, iCol, curyValue, true);
					AddValue(row, iCol, baseValue, false);
				}
			}
		}

		protected bool DetectPotentialScheduleBreak(DateTime endDate) 
		{
			MasterFinPeriod fp = PXSelect<MasterFinPeriod,
				Where<MasterFinPeriod.startDate, Greater<Required<MasterFinPeriod.startDate>>,
				And<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>>, OrderBy<Asc<MasterFinPeriod.startDate>>>.Select(this, endDate);
			return fp == null;
		}

		
		#endregion

		#region Utility Fuctions
		private void AddValue(CashFlowForecast row, int offset, Decimal value, bool isCury)
		{
			const string fieldNameBase = "AmountDay";
			string fieldName = (isCury ? "Cury" : String.Empty) + fieldNameBase + offset.ToString();
			int fieldOrdinal = this.CashFlow.Cache.GetFieldOrdinal(fieldName);
			if (fieldOrdinal < 0)
			{
				//AddField(this.CashFlow.Cache, fieldName);
				fieldOrdinal = this.CashFlow.Cache.GetFieldOrdinal(fieldName);
			}
			Decimal? current = (Decimal?)this.CashFlow.Cache.GetValue(row, fieldOrdinal);
			current = (current ?? Decimal.Zero) + value;
			this.CashFlow.Cache.SetValue(row, fieldOrdinal, current);
		}

		private void SetValue(CashFlowForecast row, int offset, Decimal value, bool isCury)
		{
			const string fieldNameBase = "AmountDay";
			string fieldName = (isCury ? "Cury" : String.Empty) + fieldNameBase + offset.ToString();
			int fieldOrdinal = this.CashFlow.Cache.GetFieldOrdinal(fieldName);
			if (fieldOrdinal < 0)
			{
				//AddField(this.CashFlow.Cache, fieldName);
				fieldOrdinal = this.CashFlow.Cache.GetFieldOrdinal(fieldName);
			}			
			this.CashFlow.Cache.SetValue(row, fieldOrdinal, value);
		}
		private Decimal? GetValue(CashFlowForecast row, int offset, bool isCury)
		{
			const string fieldNameBase = "AmountDay";
			string fieldName = (isCury ? "Cury" : String.Empty) + fieldNameBase + offset.ToString();
			int fieldOrdinal = this.CashFlow.Cache.GetFieldOrdinal(fieldName);
			if (fieldOrdinal < 0)
			{
				//AddField(this.CashFlow.Cache, fieldName);
				fieldOrdinal = this.CashFlow.Cache.GetFieldOrdinal(fieldName);
			}
			Decimal? current = (Decimal?)this.CashFlow.Cache.GetValue(row, fieldOrdinal);
			return current;
		}

		public void CalcCashAccountBalance(DateTime startDate, CashFlowFilter filter, Dictionary<int, CashFlowForecast> result)
		{
			int? cashAccountID = filter.CashAccountID;
			CMSetup cmSetup = PXSelect<CMSetup>.Select(this);
			PXSelectBase<CashAccount> acctBalanceSelect = new PXSelectJoinGroupBy<CashAccount, 
																			LeftJoin<CADailySummary, On<CADailySummary.cashAccountID, Equal<CashAccount.cashAccountID>,
																				And<CADailySummary.tranDate, Less<Required<CADailySummary.tranDate>>>>,																				
																				CrossJoin<Company,
																				InnerJoin<Currency, On<Currency.curyID, Equal<Company.baseCuryID>>>>>,
																				Where<Match<CashAccount, Current<AccessInfo.userName>>>,
																				Aggregate<GroupBy<CashAccount.cashAccountID, 
																							Sum<CADailySummary.amtReleasedClearedCr, Sum<CADailySummary.amtReleasedUnclearedCr,
																							Sum<CADailySummary.amtUnreleasedUnclearedCr, Sum<CADailySummary.amtUnreleasedClearedCr,
																								Sum<CADailySummary.amtReleasedClearedDr, Sum<CADailySummary.amtReleasedUnclearedDr,
																								Sum<CADailySummary.amtUnreleasedUnclearedDr, Sum<CADailySummary.amtUnreleasedClearedDr
																								>>>>>>>>>>>(this);
			if (cashAccountID.HasValue)
				acctBalanceSelect.WhereAnd<Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>();			
			foreach (PXResult<CashAccount,CADailySummary, Company, Currency> iRes in acctBalanceSelect.Select(startDate, cashAccountID))
			{
				CashAccount account = iRes;
				CADailySummary balanceSummary = iRes;
				Currency baseCurrency = iRes;
				CashFlowForecast row;
				if (!result.TryGetValue(account.CashAccountID.Value,out row))
				{
					row = Create(startDate, balanceSummary, account);
					row.CuryID = account.CuryID;
					row.AcctCuryID = account.CuryID;
					if (!row.CashAccountID.HasValue)
					{
						row.CashAccountID = account.CashAccountID;
						row.CuryID = account.CuryID;
					}
					result.Add(account.CashAccountID.Value, row);					
				}
				bool needConversion = (account.CuryID != baseCurrency.CuryID);

				row.CuryAmountDay0 = ((balanceSummary.AmtReleasedClearedDr ?? Decimal.Zero) + (balanceSummary.AmtReleasedUnclearedDr ?? Decimal.Zero))
							- ((balanceSummary.AmtReleasedClearedCr ?? Decimal.Zero) + (balanceSummary.AmtReleasedUnclearedCr ?? Decimal.Zero));
				row.AmountDay0 = row.CuryAmountDay0;
				if (needConversion)
				{
					CurrencyRate rate = null;
					if(String.IsNullOrEmpty(account.CuryRateTypeID) && (cmSetup == null || string.IsNullOrEmpty(cmSetup.CARateTypeDflt)))
					{
						throw new PXException(Messages.CurrencyRateTypeInNotDefinedInCashAccountAndDafaultIsNotConfiguredInCMSetup, account.CashAccountCD); 
					}  
					if (!accountRates.TryGetValue(account.CashAccountID.Value, out rate))
					{
						string curyRateType = String.IsNullOrEmpty(account.CuryRateTypeID) ? cmSetup.CARateTypeDflt : account.CuryRateTypeID;
						rate = findCuryRate(account.CuryID, baseCurrency.CuryID, curyRateType, startDate);
						this.accountRates.Add(account.CashAccountID.Value, rate);
					}
					if (rate == null)
                        throw new PXException(Messages.CurrencyRateIsRequiredToConvertFromCuryToBaseCuryForAccount, account.CuryID, baseCurrency.CuryID, account.CashAccountCD); 
					Decimal? baseAmount;
					int basePecision = baseCurrency.DecimalPlaces ?? 0;
					PaymentEntry.CuryConvBase(row.CuryAmountDay0, out baseAmount, rate.CuryRate.Value, rate.CuryMultDiv, basePecision);
					row.AmountDay0 = baseAmount;
				}

				if (string.IsNullOrEmpty(filter.CuryID) == false && filter.CuryID != account.CuryID) 
				{
					row.CuryAmountDay0 = ConvertFromBase(row.AmountDay0, this.convertToCurrency);
					row.CuryID = convertToCurrency.CuryID;  
				}	
			}
		}

		public void CalcCashAccountBalance(DateTime startDate, CashFlowFilter filter, Dictionary<int, CashFlowForecast2> result, Dictionary<int, CurrencyRate> rates)
		{
			int? cashAccountID = filter.CashAccountID;
			CMSetup cmSetup = PXSelect<CMSetup>.Select(this);
			PXSelectBase<CashAccount> acctBalanceSelect = new PXSelectJoinGroupBy<CashAccount, LeftJoin<CADailySummary, On<CADailySummary.cashAccountID, Equal<CashAccount.cashAccountID>,
																				And<CADailySummary.tranDate, Less<Required<CADailySummary.tranDate>>>>,
																				CrossJoin<Company,
																				InnerJoin<Currency, On<Currency.curyID, Equal<Company.baseCuryID>>>>>,
																				Where<Match<CashAccount, Current<AccessInfo.userName>>>,
																				Aggregate<GroupBy<CADailySummary.cashAccountID, Sum<CADailySummary.amtReleasedClearedCr, Sum<CADailySummary.amtReleasedUnclearedCr,
																							Sum<CADailySummary.amtUnreleasedUnclearedCr, Sum<CADailySummary.amtUnreleasedClearedCr,
																								Sum<CADailySummary.amtReleasedClearedDr, Sum<CADailySummary.amtReleasedUnclearedDr,
																								Sum<CADailySummary.amtUnreleasedUnclearedDr, Sum<CADailySummary.amtUnreleasedClearedDr
																								>>>>>>>>>>>(this);
			if (cashAccountID.HasValue)
				acctBalanceSelect.WhereAnd<Where<CADailySummary.cashAccountID, Equal<Required<CADailySummary.cashAccountID>>>>();
			foreach (PXResult<CashAccount, CADailySummary, Company, Currency> iRes in acctBalanceSelect.Select(startDate, cashAccountID))
			{
				CADailySummary balanceSummary = iRes;
				CashAccount account = iRes;				
				Currency baseCurrency = iRes;				
				CashFlowForecast2 row;
				if (!result.TryGetValue(account.CashAccountID.Value,out row))
				{
					row = new CashFlowForecast2();
					row.RecordType = (int)CashFlowForecastRecordType.RecordType.Balance;
					row.CashAccountID = account.CashAccountID.Value;
					row.TranDate = startDate;
					row.CuryID = account.CuryID;					
					result[balanceSummary.CashAccountID.Value] = row;
				}
				bool needConversion = (account.CuryID != baseCurrency.CuryID);
				row.CuryAmountDay = ((balanceSummary.AmtReleasedClearedDr ?? Decimal.Zero) + (balanceSummary.AmtReleasedUnclearedDr ?? Decimal.Zero))
							- ((balanceSummary.AmtReleasedClearedCr ?? Decimal.Zero) + (balanceSummary.AmtReleasedUnclearedCr ?? Decimal.Zero));
				row.AmountDay = row.CuryAmountDay;
				if (needConversion)
				{
					CurrencyRate rate;
					if (String.IsNullOrEmpty(account.CuryRateTypeID) && (cmSetup == null || string.IsNullOrEmpty(cmSetup.CARateTypeDflt)))
					{
						throw new PXException(Messages.CurrencyRateTypeInNotDefinedInCashAccountAndDafaultIsNotConfiguredInCMSetup, account.CashAccountCD);
					}
					if (!this.accountRates.TryGetValue(account.CashAccountID.Value,out rate))
					{
						string curyRateType = String.IsNullOrEmpty(account.CuryRateTypeID) ? cmSetup.CARateTypeDflt : account.CuryRateTypeID;
						rate = findCuryRate(account.CuryID, baseCurrency.CuryID, account.CuryRateTypeID, startDate);
						this.accountRates.Add(account.CashAccountID.Value, rate);						 
					}
					if (rate == null)
                        throw new PXException(Messages.CurrencyRateIsRequiredToConvertFromCuryToBaseCuryForAccount, account.CuryID, baseCurrency.CuryID, account.CashAccountCD); 
					Decimal? baseAmount;
					int basePecision = baseCurrency.DecimalPlaces ?? 0;
					PaymentEntry.CuryConvBase(row.CuryAmountDay, out baseAmount, rate.CuryRate.Value, rate.CuryMultDiv, basePecision);
					row.AmountDay = baseAmount;
				}

				if (string.IsNullOrEmpty(filter.CuryID) == false && filter.CuryID != account.CuryID)
				{
					row.CuryAmountDay = ConvertFromBase(row.AmountDay, this.convertToCurrency);
					row.CuryID = this.convertToCurrency.CuryID;
				}
			}
		}

		protected CurrencyRate findCuryRate(string fromCuryID, string toCuryID, string aCuryRateType, DateTime aDate)
		{
			CurrencyRate curyRate = PXSelectReadonly<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
								And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
								And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
								And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
								OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(this, fromCuryID, toCuryID, aCuryRateType, aDate);
			if (curyRate == null)
			{
				curyRate = PXSelectReadonly<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
								And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
								And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
								And<CurrencyRate.curyEffDate, Greater<Required<CurrencyRate.curyEffDate>>>>>>,
								OrderBy<Asc<CurrencyRate.curyEffDate>>>.Select(this, fromCuryID, toCuryID, aCuryRateType, aDate);
			}
			return curyRate;
		}

		protected CashAccount findDefaultCashAccount(Light.ARInvoice aDoc)
		{
			CashAccount acct = null;
			PXCache cache = this.arInvoice.Cache;
			Light.ARInvoice copy = (Light.ARInvoice)cache.CreateCopy(aDoc);
			if (String.IsNullOrEmpty(aDoc.PaymentMethodID))
			{
				object newValue;
				cache.RaiseFieldDefaulting<Light.ARInvoice.paymentMethodID>(copy, out newValue);
				copy.PaymentMethodID = (string)newValue;
			}

			if (aDoc.PMInstanceID.HasValue == false)
			{
				object newValue;
				cache.RaiseFieldDefaulting<Light.ARInvoice.pMInstanceID>(copy, out newValue);
				copy.PMInstanceID = newValue as Int32?;
			}

			{
				object newValue;
				cache.RaiseFieldDefaulting<Light.ARInvoice.cashAccountID>(copy, out newValue);
				Int32? acctID = newValue as Int32?;
				if (acctID.HasValue)
				{
					acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, acctID);
				}
			}
			return acct;
		}

		protected CashAccount findDefaultCashAccount(Light.APInvoice aDoc)
		{
			CashAccount acct = null;
			PXCache cache = this.apInvoice.Cache;
			Light.APInvoice copy = (Light.APInvoice)cache.CreateCopy(aDoc);
			if (String.IsNullOrEmpty(aDoc.PayTypeID))
			{
				object newValue;
				cache.RaiseFieldDefaulting<Light.APInvoice.payTypeID>(copy, out newValue);
				copy.PayTypeID = (string)newValue;
			}

			{
				object newValue;
				cache.RaiseFieldDefaulting<Light.APInvoice.payAccountID>(copy, out newValue);
				Int32? acctID = newValue as Int32?;
				if (acctID.HasValue)
				{
					acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, acctID);
				}
			}
			return acct;
		}

		[Obsolete("This class has been deprecated and will be removed in Acumatica ERP 2019R2.")]
		protected CashFlowForecast Create(DateTime startDate, CashAccount aCashAccount, ARInvoice src)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.CustomerID;
			dst.StartingDate = startDate;
			dst.RecordType = (int)CashFlowForecastRecordType.RecordType.CashIn;
			dst.CashAccountID = aCashAccount.CashAccountID;
			dst.CuryID = aCashAccount.CuryID;
			dst.AcctCuryID = aCashAccount.CuryID;
			return dst;
		}

		
		protected CashFlowForecast Create(DateTime startDate, CashAccount aCashAccount, Light.ARInvoice src)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.CustomerID;
			dst.StartingDate = startDate;
			dst.RecordType = (int)CashFlowForecastRecordType.RecordType.CashIn;
			dst.CashAccountID = aCashAccount.CashAccountID;
			dst.CuryID = aCashAccount.CuryID;
			dst.AcctCuryID = aCashAccount.CuryID;
			return dst;
		}

		[Obsolete("This class has been deprecated and will be removed in Acumatica ERP 2019R2.")]
		protected CashFlowForecast Create(DateTime startDate, CashAccount aCashAccount, APInvoice src)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.VendorID;
			dst.StartingDate = startDate;
			dst.RecordType = (int)CashFlowForecastRecordType.RecordType.CashOut;
			dst.CashAccountID = aCashAccount.CashAccountID;
			dst.CuryID = aCashAccount.CuryID;
			dst.AcctCuryID = aCashAccount.CuryID;
			return dst;
		}

		protected CashFlowForecast Create(DateTime startDate, CashAccount aCashAccount, Light.APInvoice src)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.VendorID;
			dst.StartingDate = startDate;
			dst.RecordType = (int)CashFlowForecastRecordType.RecordType.CashOut;
			dst.CashAccountID = aCashAccount.CashAccountID;
			dst.CuryID = aCashAccount.CuryID;
			dst.AcctCuryID = aCashAccount.CuryID;
			return dst;
		}

		protected CashFlowForecast Create(DateTime startDate, APPayment src, bool applied)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.VendorID;
			dst.StartingDate = startDate;
			dst.RecordType = applied ? (int)CashFlowForecastRecordType.RecordType.CashOut : (int)CashFlowForecastRecordType.RecordType.CashOutUnapplied;
			dst.CashAccountID = src.CashAccountID;
			dst.CuryID = src.CuryID;
			dst.AcctCuryID = src.CuryID;
			return dst;
		}

		[Obsolete("The method is obsolete. It will be removed in 2020R2. Please use Create(DateTime, APPayment, bool) instead.")]
		protected CashFlowForecast Create(DateTime startDate, APPayment src)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.VendorID;
			dst.StartingDate = startDate;
			dst.RecordType = (int)CashFlowForecastRecordType.RecordType.CashOut;
			dst.CashAccountID = src.CashAccountID;
			dst.CuryID = src.CuryID;
			dst.AcctCuryID = src.CuryID;
			return dst;
		}

		protected CashFlowForecast Create(DateTime startDate, ARPayment src, bool applied)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.BAccountID = src.CustomerID;
			dst.StartingDate = startDate;
			dst.RecordType = applied ? (int)CashFlowForecastRecordType.RecordType.CashIn : (int)CashFlowForecastRecordType.RecordType.CashInUnapplied;
			dst.CashAccountID = src.CashAccountID;
			dst.CuryID = src.CuryID;
			dst.AcctCuryID = src.CuryID;
			return dst;
		}

		protected CashFlowForecast Create(DateTime startDate, CADailySummary src, CashAccount account)
		{
			CashFlowForecast dst = new CashFlowForecast();		
			dst.StartingDate = startDate;
			dst.RecordType = (int)CashFlowForecastRecordType.RecordType.Balance;
			dst.CashAccountID = src.CashAccountID;
			dst.CuryID = account.CuryID;
			dst.AcctCuryID = account.CuryID;
			return dst;
		}

		protected CashFlowForecast Create(DateTime startDate, CashForecastTran src)
		{
			CashFlowForecast dst = new CashFlowForecast();
			dst.StartingDate = startDate;
			dst.RecordType = (src.DrCr == DrCr.Debit) ? (int)CashFlowForecastRecordType.RecordType.CashIn : (int)CashFlowForecastRecordType.RecordType.CashOut;			
			dst.CashAccountID = src.CashAccountID;
			dst.EntryID = src.TranID;
			dst.CuryID = src.CuryID;
			dst.AcctCuryID = src.CuryID;
			return dst;
		}

		protected static CashFlowForecast2 Create(CashFlowForecast src, int offset) 
		{
			CashFlowForecast2 tran = new CashFlowForecast2();
			tran.RecordType = src.RecordType;
			tran.TranDate = src.StartingDate.Value.AddDays(offset);
			tran.BAccountID = src.BAccountID;
			tran.CashAccountID = src.CashAccountID;
			tran.EntryID = src.EntryID;
			tran.CuryID = src.CuryID;
			tran.AcctCuryID = src.AcctCuryID;
			return tran;
		}

		protected CashFlowForecast AddAmount(CashFlowForecast dst, ARPayment doc)
		{
			Decimal sign = (doc.DrCr == DrCr.Debit) ? Decimal.One : Decimal.MinusOne;
			Decimal? curyAmount = Decimal.Zero;
			Decimal? baseAmount = Decimal.Zero;			
			if (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn)
			{
				curyAmount = doc.CuryOrigDocAmt;
				baseAmount = doc.OrigDocAmt;				
			}
			else
			{
				curyAmount = doc.CuryDocBal;
				baseAmount = doc.DocBal;				
			}
			if (this.convertToCurrency != null && doc.CuryID != this.convertToCurrency.CuryID)
			{
				curyAmount = ConvertFromBase(curyAmount, convertToCurrency);
				dst.CuryID = convertToCurrency.CuryID;
			}
			AddAmount(dst, doc.DocDate.Value, curyAmount, baseAmount, sign);
			return dst;
		}

		protected CashFlowForecast AddAmount(CashFlowForecast dst, ARPayment doc, ARAdjust adj) 
		{
			int offset = (doc.DocDate.Value - dst.StartingDate.Value).Days;
			Decimal sign = (doc.DrCr == DrCr.Debit) ? Decimal.One : Decimal.MinusOne;
			Decimal? curyAmount = Decimal.Zero;
			Decimal? baseAmount = Decimal.Zero;
			if (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn)
			{
				curyAmount = doc.CuryOrigDocAmt;
				baseAmount = doc.OrigDocAmt;				
			}
			else
			{
				sign = adj.AdjgBalSign ?? 1m;
				curyAmount = adj.CuryAdjgAmt;
				baseAmount = adj.AdjAmt;	
			}
			if (this.convertToCurrency != null && doc.CuryID != this.convertToCurrency.CuryID)
			{
				curyAmount = ConvertFromBase(baseAmount, convertToCurrency);
				dst.CuryID = convertToCurrency.CuryID;
			}
			AddAmount(dst, doc.DocDate.Value, curyAmount, baseAmount, sign);
			return dst;
		}

		protected CashFlowForecast AddAmount(CashFlowForecast dst, APPayment doc, APAdjust adj)
		{
			int offset = (doc.DocDate.Value - dst.StartingDate.Value).Days;
			Decimal sign = (doc.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;
			Decimal? curyAmount = Decimal.Zero;
			Decimal? baseAmount = Decimal.Zero;
			if (doc.DocType == APDocType.QuickCheck || doc.DocType == APDocType.VoidQuickCheck)
			{
				curyAmount = doc.CuryOrigDocAmt;
				baseAmount = doc.OrigDocAmt;								
			}
			else
			{
				sign = adj.AdjgBalSign ?? 1m;
				curyAmount = adj.CuryAdjgAmt;
				baseAmount = adj.AdjAmt;	
			}
			if (this.convertToCurrency != null && doc.CuryID != this.convertToCurrency.CuryID)
			{
				curyAmount = ConvertFromBase(baseAmount, convertToCurrency);
				dst.CuryID = convertToCurrency.CuryID;
			}
			AddAmount(dst, doc.DocDate.Value, curyAmount, baseAmount, sign);
			return dst;
		}

		protected CashFlowForecast AddAmount(CashFlowForecast dst, APPayment doc)
		{
			Decimal sign = (doc.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;
			Decimal? curyAmount = Decimal.Zero;
			Decimal? baseAmount = Decimal.Zero;
			if (doc.DocType == APDocType.QuickCheck || doc.DocType == APDocType.VoidQuickCheck)
			{
				curyAmount = doc.CuryOrigDocAmt;
				baseAmount = doc.OrigDocAmt;
			}
			else
			{
				curyAmount = doc.CuryDocBal;
				baseAmount = doc.DocBal;
			}
			if (this.convertToCurrency != null && doc.CuryID != this.convertToCurrency.CuryID)
			{
				curyAmount = ConvertFromBase(baseAmount, convertToCurrency);
				dst.CuryID = convertToCurrency.CuryID;
			}
			AddAmount(dst, doc.DocDate.Value, curyAmount, baseAmount, sign);
			return dst;
		}

		protected CashFlowForecast AddAmount(CashFlowForecast dst, DateTime docDate, Decimal? curyValue, Decimal? baseValue, Decimal sign)
		{
			int offset = (docDate - dst.StartingDate.Value).Days;
			if (offset < 0) 
				offset = 0;
			AddValue(dst, offset, (curyValue ?? Decimal.Zero) * sign, true);
			AddValue(dst, offset, (baseValue ?? Decimal.Zero) * sign, false);
			return dst;
		}

		protected CashFlowForecast AddAmount(CashFlowForecast dst, CashForecastTran doc, CurrencyRate acctRate, Currency baseCurrency)
		{
			int offset = (doc.TranDate.Value - dst.StartingDate.Value).Days;
			Decimal sign = Decimal.One;
			Decimal? baseValue = doc.CuryTranAmt.Value;
			Decimal? curyAmount = doc.CuryTranAmt.Value;
			if (doc.CuryID != baseCurrency.CuryID)
			{
				if (acctRate == null) 
					throw new PXException(Messages.CurrencyRateIsRequiredToConvertFromCuryToBaseCuryForAccount, doc.CuryID, baseCurrency.CuryID, doc.CashAccountID);
				Decimal? curyValue = doc.CuryTranAmt.Value;
				PaymentEntry.CuryConvBase(curyValue, out baseValue, acctRate.CuryRate.Value, acctRate.CuryMultDiv, baseCurrency.DecimalPlaces.Value);	
			}

			if (this.convertToCurrency != null && doc.CuryID != this.convertToCurrency.CuryID)
			{
				curyAmount = ConvertFromBase(baseValue, convertToCurrency);
			}
			AddAmount(dst, doc.TranDate.Value, curyAmount, baseValue, sign);
			return dst;
		}

		protected static void ConvertAmount(ref Decimal? aCuryValue, ref Decimal? aValue, string aSrcCuryID, Currency destCurrency, Currency baseCurrency, CurrencyRate destRate, CurrencyRate srcRate)
		{
			Decimal? curyValue = aCuryValue;
			Decimal? value = aValue;
			curyValue = PaymentEntry.CalcBalances(curyValue, value, destCurrency.CuryID, aSrcCuryID, baseCurrency.CuryID,
								destRate != null ? destRate.CuryRate.Value : Decimal.One,
								destRate != null ? destRate.CuryMultDiv : "M",
								srcRate != null ? srcRate.CuryRate.Value : Decimal.One,
								srcRate != null ? srcRate.CuryMultDiv : "M",
								destCurrency.DecimalPlaces.Value, baseCurrency.DecimalPlaces.Value);

			if (destCurrency.CuryID != baseCurrency.CuryID)
			{
				Decimal? baseValue;
				PaymentEntry.CuryConvBase(curyValue, out baseValue, destRate.CuryRate.Value, destRate.CuryMultDiv, baseCurrency.DecimalPlaces.Value);
				aValue = baseValue ?? Decimal.Zero;
			}
			else 
			{
				aValue = curyValue;
			}
			aCuryValue = curyValue;
		}

		protected virtual Decimal? ConvertFromBase(Decimal? baseAmount, Currency convertToCurrency)
		{
			CurrencyRate convertToRate = null;
			string curyID = convertToCurrency.CuryID;
			if (curyID == this.baseCurrency.CuryID)
				return baseAmount;
			if (!this.currencyRates.TryGetValue(curyID, out convertToRate))
			{
				convertToRate = findCuryRate(curyID, this.baseCurrency.CuryID, this.currencyRateType, startDate);
				if (convertToRate == null)
				{
					throw new PXException(Messages.CurrencyRateIsNotDefined);
				}
				this.currencyRates[curyID] = convertToRate;
			}
			Decimal? convertedCuryAmount;
			int curyPecision = convertToCurrency.DecimalPlaces ?? 0;
			PaymentEntry.CuryConvCury(baseAmount, out convertedCuryAmount, convertToRate.CuryRate.Value, convertToRate.CuryMultDiv, curyPecision);
			return convertedCuryAmount;
		}

		protected virtual string ConvertDocAmount(ref Decimal? curyValue, ref Decimal? value, CurrencyInfo aDocCuryInfo, CashAccount aAccount, CashFlowFilter filter)
		{
			string curyID = filter.CuryID;
			bool convertToSingleCury = true;			
			if (string.IsNullOrEmpty(curyID))
			{
				if (aAccount != null && aAccount.AccountID.HasValue)
				{
					curyID = aAccount.CuryID;
				}
				convertToSingleCury = false;
			}

			CurrencyRate acctRate = null;
			CurrencyRate docRate = findCuryRate(aDocCuryInfo.CuryID, baseCurrency.CuryID, aDocCuryInfo.CuryRateTypeID, startDate);
			Currency acctCurrency = null;
			if (!convertToSingleCury)
			{
				
				acctCurrency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(this, curyID);
				if (!this.accountRates.TryGetValue(aAccount.CashAccountID.Value, out acctRate))				
				{
					if (curyID != baseCurrency.CuryID)
					{
						string curyRateType = (aAccount != null) ? aAccount.CuryRateTypeID : String.Empty;
						if (String.IsNullOrEmpty(curyRateType))
						{
							CMSetup cmSetup = PXSelect<CMSetup>.Select(this);
							if (cmSetup == null || String.IsNullOrEmpty(cmSetup.CARateTypeDflt))
							{
								throw new PXException(Messages.CurrencyRateTypeInNotDefinedInCashAccountAndDafaultIsNotConfiguredInCMSetup, aAccount.CashAccountCD);
							}
							else
							{
								curyRateType = cmSetup.CARateTypeDflt;
							}
						}
						acctRate = findCuryRate(aAccount.CuryID, baseCurrency.CuryID, curyRateType, startDate);
						this.accountRates[aAccount.CashAccountID.Value] = acctRate;
					}
				}				
			}
			else
			{
				acctCurrency = this.convertToCurrency;
				if (!this.currencyRates.TryGetValue(curyID, out acctRate))
				{
					acctRate = findCuryRate(curyID, baseCurrency.CuryID, this.currencyRateType, startDate);
					this.currencyRates[curyID] = acctRate;
				}
			}
			ConvertAmount(ref curyValue, ref value, aDocCuryInfo.CuryID, acctCurrency, baseCurrency, acctRate, docRate);
			return curyID;
		} 

		protected void Convert(List<CashFlowForecast2> dest, CashFlowForecast src, bool skipIfZero) 
		{
			for (int i = 0; i < AmountFieldsNumber; i++) 
			{
				Decimal? curyAmountDay = GetValue(src, i, true);
				Decimal? amountDay = GetValue(src, i, false);
				if (skipIfZero && (curyAmountDay ?? Decimal.Zero) == Decimal.Zero && (amountDay ?? Decimal.Zero) == Decimal.Zero) continue;
				CashFlowForecast2 tran = Create(src, i);
				tran.CuryAmountDay = curyAmountDay;
				tran.AmountDay = amountDay;				
				dest.Add(tran);
			}
		}

		
		#endregion

		#region Private variables
		//Constants
		protected readonly string CashFlowReportName = "CA658000";
		protected readonly int AmountFieldsNumber = 30;
		//Temporary values for the calculations
		protected DateTime startDate;
		protected DateTime endDate;
		protected Currency baseCurrency;
		protected Currency convertToCurrency;
		protected Dictionary<string, CurrencyRate> currencyRates;
		protected Dictionary<int, CurrencyRate> accountRates;
		protected string currencyRateType;
		protected CashAccount defaultCashAccount;
		#endregion

	}
}
