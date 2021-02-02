using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CR;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AR.MigrationMode;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Extensions;

namespace PX.Objects.AR
{
	public class ARWriteOffType : ARDocType
	{
		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SmallBalanceWO, SmallCreditWO },
				new string[] { Messages.SmallBalanceWO, Messages.SmallCreditWO }) { }
		}
		public class DefaultDrCrAttribute : PXDefaultAttribute
		{
			protected readonly Type _DocType;
			public DefaultDrCrAttribute(Type DocType)
			{
				_DocType = DocType;
				this.PersistingCheck = PXPersistingCheck.Nothing;				
			}
			public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				string docType = (string)sender.GetValue(e.Row, _DocType.Name);
				if (docType != null)
					e.NewValue = ARWriteOffType.DrCr(docType);
			}
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case SmallBalanceWO:
					return GL.DrCr.Credit;
				case SmallCreditWO:
					return GL.DrCr.Debit;
				default:
					return null;
			}
		}
	}

	[Serializable]
	public partial class ARWriteOffFilter : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region WOType
		public abstract class woType : PX.Data.BQL.BqlString.Field<woType> { }
		protected String _WOType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault(ARWriteOffType.SmallBalanceWO)]
		[ARWriteOffType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual String WOType
		{
			get
			{
				return this._WOType;
			}
			set
			{
				this._WOType = value;
			}
		}
		#endregion
		#region WODate
		public abstract class wODate : PX.Data.BQL.BqlDateTime.Field<wODate> { }
		protected DateTime? _WODate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Doc. Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? WODate
		{
			get
			{
				return this._WODate;
			}
			set
			{
				this._WODate = value;
			}
		}
		#endregion
		#region WOFinPeriodID
		public abstract class wOFinPeriodID : PX.Data.BQL.BqlString.Field<wOFinPeriodID> { }
		protected string _WOFinPeriodID;
		[AROpenPeriod(
			typeof(ARWriteOffFilter.wODate), 
			branchSourceType: typeof(ARWriteOffFilter.branchID),
		    useMasterOrganizationIDByDefault: true)]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public virtual String WOFinPeriodID
		{
			get
			{
				return this._WOFinPeriodID;
			}
			set
			{
				this._WOFinPeriodID = value;
			}
		}
		#endregion
		#region WOLimit
		public abstract class wOLimit : PX.Data.BQL.BqlDecimal.Field<wOLimit> { }
		protected Decimal? _WOLimit;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Limit", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
		public virtual Decimal? WOLimit
		{
			get
			{
				return this._WOLimit;
			}
			set
			{
				this._WOLimit = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer()]
		[PXRestrictor(typeof(Where<Customer.status, Equal<BAccount.status.active>,
						Or<Customer.status, Equal<BAccount.status.oneTime>, 
						Or<Customer.status, Equal<BAccount.status.creditHold>>>>), Messages.CustomerIsInStatus, typeof(Customer.status))]
		[PXRestrictor(typeof(Where<Customer.smallBalanceAllow, Equal<True>>), Messages.CustomerSmallBalanceAllowOff)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDefault]
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where2<Where<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>, And<Current<ARWriteOffFilter.woType>, Equal<ARWriteOffType.smallBalanceWO>>>,
			Or<Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>, And<Current<ARWriteOffFilter.woType>, Equal<ARWriteOffType.smallCreditWO>>>>>>))]
		[PXUIField(DisplayName = "Reason Code", Visibility = PXUIVisibility.Visible)]
		public virtual String ReasonCode
		{
			get
			{
				return this._ReasonCode;
			}
			set
			{
				this._ReasonCode = value;
			}
		}
		#endregion
		#region SelTotal
		public abstract class selTotal : PX.Data.BQL.BqlDecimal.Field<selTotal> { }
		protected Decimal? _SelTotal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Selection Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
	}

	[TableAndChartDashboardType]
	public class ARCreateWriteOff : PXGraph<ARCreateWriteOff>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public PXCancel<ARWriteOffFilter> Cancel;
		public PXFilter<ARWriteOffFilter> Filter;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<ARWriteOffFilter> EditDetail;
		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof(ARRegisterEx.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXFilteredProcessingJoin<
			ARRegisterEx, 
			ARWriteOffFilter,
				InnerJoin<Customer, 
					On<Customer.bAccountID, Equal<ARRegisterEx.customerID>,
					And<Customer.smallBalanceAllow, Equal<True>>>,
				LeftJoin<ARAdjust, 
					On<ARAdjust.adjdDocType, Equal<ARRegisterEx.docType>,
					And<ARAdjust.adjdRefNbr, Equal<ARRegisterEx.refNbr>,
					And<ARAdjust.released, Equal<False>,
					And<ARAdjust.voided, Equal<False>>>>>,
				LeftJoin<ARAdjust2, 
					On<ARAdjust2.adjgDocType, Equal<ARRegisterEx.docType>, 
					And<ARAdjust2.adjgRefNbr, Equal<ARRegisterEx.refNbr>, 
					And<ARAdjust2.released, Equal<False>, 
					And<ARAdjust2.voided, Equal<False>>>>>>>>,
				Where<Where2<
					MatchWithBranch<ARRegisterEx.branchID>,
					And2<Match<Current<AccessInfo.userName>>,
					And<ARRegisterEx.released, Equal<True>,
					And<ARRegisterEx.hold, NotEqual<True>,
					And<ARRegisterEx.openDoc, Equal<True>,
					And<ARRegisterEx.pendingPPD, NotEqual<True>,
					And2<Where<
						ARRegisterEx.docBal, Greater<decimal0>,
						Or<ARRegisterEx.curyDocBal, Greater<decimal0>>>,
					And<ARRegisterEx.docBal, LessEqual<Current<ARWriteOffFilter.wOLimit>>,
					And2<Where<
						Current2<ARWriteOffFilter.branchID>, IsNull,
						Or<ARRegisterEx.branchID, Equal<Current<ARWriteOffFilter.branchID>>>>,
					And2<Where<Current<ARWriteOffFilter.woType>, Equal<ARDocType.smallBalanceWO>,
					And2<Where<
						ARRegisterEx.docType, Equal<ARDocType.invoice>,
						Or<ARRegisterEx.docType, Equal<ARDocType.debitMemo>,
						Or<ARRegisterEx.docType, Equal<ARDocType.finCharge>>>>,
					And<
						ARAdjust.adjgRefNbr, IsNull,
						Or<Current<ARWriteOffFilter.woType>, Equal<ARDocType.smallCreditWO>, 
					And2<Where<
						ARRegisterEx.docType, Equal<ARDocType.payment>,
						Or<ARRegisterEx.docType, Equal<ARDocType.creditMemo>, 
						Or<ARRegisterEx.docType, Equal<ARDocType.prepayment>>>>, 
					And<ARAdjust2.adjdRefNbr, IsNull>>>>>>,
					And<Where<
						Current<ARWriteOffFilter.customerID>, IsNull, 
						Or<Current<ARWriteOffFilter.customerID>, Equal<ARRegisterEx.customerID>>>>>>>>>>>>>>>> 
			ARDocumentList;

		public CMSetupSelect CMSetup;
		public PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Current<ARWriteOffFilter.customerID>>>> customer;
		public PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<ARSetup.dfltCustomerClassID>>>> customerclass;

		public ARSetupNoMigrationMode ARSetup;

		public PXSelect<Sub> subs;

		public Customer CUSTOMER
		{
			get
			{
				return customer.Select();
			}
		}

		public CustomerClass CUSTOMERCLASS
		{
			get
			{
				return customerclass.Select();
			}
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable editDetail(PXAdapter adapter)
		{
			if (ARDocumentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(ARDocumentList.Cache, ARDocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}


		public ARCreateWriteOff()
		{
			ARSetup setup = ARSetup.Current;
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName.ToLower() == "filter" && values != null)
			{
				values["SelTotal"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "FC Balance")]
		protected virtual void ARRegisterEx_CuryDocBal_CacheAttached(PXCache sender)
		{ }

		protected virtual void ARRegisterEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARRegisterEx row = e.Row as ARRegisterEx;
			if (row == null)
			{
				return;
			}

			bool isMultipleCalendarsSupportEnabled = PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();
			bool isNeedConvertToMaster = isMultipleCalendarsSupportEnabled && Filter.Current.BranchID != null;

			string masterPeriodID =
				isNeedConvertToMaster
					? FinPeriodRepository.
						GetByID(Filter.Current.WOFinPeriodID, PXAccess.GetParentOrganizationID(Filter.Current.BranchID)).
						MasterFinPeriodID
					: Filter.Current.WOFinPeriodID;

			bool WODate_GT_DocDate = DateTime.Compare((DateTime)Filter.Current.WODate, (DateTime)((ARRegisterEx)e.Row).DocDate) >= 0;
			bool WOPeriod_GT_DocPeriod = string.Compare(masterPeriodID, ((ARRegisterEx)e.Row).TranPeriodID) >= 0;

			PXUIFieldAttribute.SetEnabled<ARRegisterEx.selected>(sender, e.Row, WODate_GT_DocDate && WOPeriod_GT_DocPeriod);

			sender.RaiseExceptionHandling<ARRegisterEx.docDate>(e.Row, null, !WODate_GT_DocDate ? new PXSetPropertyException(Messages.WriteOff_ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<ARWriteOffFilter.wODate>(Filter.Cache)) : null);
			sender.RaiseExceptionHandling<ARRegisterEx.finPeriodID>(e.Row, null, !WOPeriod_GT_DocPeriod ? new PXSetPropertyException(Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<ARWriteOffFilter.wOFinPeriodID>(Filter.Cache)) : null);

			if (string.IsNullOrEmpty(row.ReasonCode))
				row.ReasonCode = Filter.Current.ReasonCode;
		}

		protected virtual void ARWriteOffFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(ARDocumentList.Cache, typeof(ARRegisterEx.reasonCode).Name, true);

			ARWriteOffFilter filter = (ARWriteOffFilter)e.Row;

			if (filter != null)
			{
				if (customer.Current != null && object.Equals(filter.CustomerID, customer.Current.BAccountID) == false)
				{
					customer.Current = null;
				}
				
				ARDocumentList.SetAutoPersist(true);
				ARDocumentList.SetProcessDelegate(
					delegate(List<ARRegisterEx> list)
					{
						List<ARRegister> docs = new List<ARRegister>(list.Count);
						foreach (ARRegister doc in list)
						{
							docs.Add(doc);
						}
						CreatePayments(docs, filter);
					}
				);
			}

			bool isMultiCurrencyEnabled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();

			PXUIFieldAttribute.SetVisible<ARRegisterEx.curyID>(ARDocumentList.Cache, null, isMultiCurrencyEnabled);
			PXUIFieldAttribute.SetVisible<ARRegisterEx.curyDocBal>(ARDocumentList.Cache, null, isMultiCurrencyEnabled);
		}

		protected virtual void ARWriteOffFilter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (customerclass.Current != null)
			{
				((ARWriteOffFilter)e.Row).WOLimit = customerclass.Current.SmallBalanceLimit;
				return;
			}
		}

		protected virtual void ARWriteOffFilter_WOType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARWriteOffFilter filter = (ARWriteOffFilter)e.Row;
			sender.SetDefaultExt<ARWriteOffFilter.reasonCode>(e.Row);

			if (customer.Current != null && customer.Current.BAccountID != filter.CustomerID)
			{
				customer.Current = null;
			}
		}

		protected virtual void ARWriteOffFilter_ReasonCode_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARWriteOffFilter filter = (ARWriteOffFilter)e.Row;
			if (filter.WOType == ARWriteOffType.SmallBalanceWO)
			{
				e.NewValue = ARSetup.Current.BalanceWriteOff;
			}
			else
			{
				e.NewValue = ARSetup.Current.CreditWriteOff;
			}
		}
		
		protected virtual void ARWriteOffFilter_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARWriteOffFilter_WOType_FieldUpdated(sender, e);

			if (customer.Current != null)
			{
				((ARWriteOffFilter)e.Row).WOLimit = customer.Current.SmallBalanceLimit;
				return;
			}

			if (customerclass.Current != null)
			{
				((ARWriteOffFilter)e.Row).WOLimit = customerclass.Current.SmallBalanceLimit;
				return;
			}
		}

		protected virtual void ARWriteOffFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARWriteOffFilter row = e.Row as ARWriteOffFilter;
			ARWriteOffFilter oldRow = e.OldRow as ARWriteOffFilter;
			if (row == null || oldRow == null) return;

			if (!sender.ObjectsEqual<ARWriteOffFilter.woType, ARWriteOffFilter.branchID, ARWriteOffFilter.customerID>(row, oldRow))
			{
				ARDocumentList.Cache.Clear();
				row.SelTotal = 0;
			}
			else if (row.ReasonCode != oldRow.ReasonCode)
			{
				foreach (ARRegisterEx item in ARDocumentList.Select() )
				{
					ARDocumentList.Cache.SetValue<ARRegisterEx.reasonCode>(item, row.ReasonCode);
				}
			}
		}
		
		public static void CreatePayments(List<ARRegister> list, ARWriteOffFilter filter)
		{
			if  ( string.IsNullOrEmpty(filter.ReasonCode) )
				throw new PXException(Messages.ReasonCodeIsRequired);

			bool failed = false;

			IARWriteOffEntry pe = null;
			if (filter.WOType == ARDocType.SmallBalanceWO)
			{
				pe = PXGraph.CreateInstance<ARSmallBalanceWriteOffEntry>();
			}
			else
			{
				pe = PXGraph.CreateInstance<ARSmallCreditWriteOffEntry>();
			}

			List<ARRegister> orig = list;
			list = new List<ARRegister>(orig);

			List<ARRegister> paylist = new List<ARRegister>();
			List<int> paybind = new List<int>();

			var cache = (pe as PXGraph).Caches[typeof(ARRegisterEx)];

			IFinPeriodRepository FinPeriodRepository = ((PXGraph) pe).GetService<IFinPeriodRepository>();

			list = list.OrderBy(doc => new Tuple<string, string, string, string, string>(
				(string)(cache.GetValueExt<ARRegisterEx.branchID>(doc) as PXFieldState).Value,
				doc.CuryID,
				(string)(cache.GetValueExt<ARRegisterEx.customerID>(doc) as PXFieldState).Value,
				doc.DocType,
				doc.RefNbr
			)).ToList();

			for (int i = 0; i < list.Count; i++)
			{
				ARRegisterEx doc = (ARRegisterEx) list[i];
				int idx = orig.IndexOf(doc);
				try
				{
					ReasonCode reasonCode = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Required<ReasonCode.reasonCodeID>>>>.Select( (PXGraph)pe, doc.ReasonCode ?? filter.ReasonCode);
					if ( reasonCode == null )
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ReasonCodeNotFound, filter.ReasonCode));

					Location customerLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
						And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select((PXGraph)pe, doc.CustomerID, doc.CustomerLocationID);
					
					CRLocation companyLocation = PXSelectJoin<CRLocation, 
						InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
						InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select((PXGraph)pe, doc.BranchID);
					
					object value = null;
					if (reasonCode.Usage == ReasonCodeUsages.BalanceWriteOff || reasonCode.Usage == ReasonCodeUsages.CreditWriteOff)
					{
						value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>((PXGraph)pe, reasonCode.SubMask,
							new object[] { reasonCode.SubID, customerLocation.CSalesSubID, companyLocation.CMPSalesSubID },
							new Type[] { typeof(ReasonCode.subID), typeof(Location.cSalesSubID), typeof(CRLocation.cMPSalesSubID) });
					}
					else
					{
						throw new PXException(Messages.InvalidReasonCode);
					}

					ARReleaseProcess.EnsureNoUnreleasedVoidPaymentExists((pe as PXGraph), doc, Common.Messages.ActionWrittenOff);

					bool isMultipleCalendarsSupportEnabled = PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();
					bool isNeedConvertToMaster = isMultipleCalendarsSupportEnabled && filter.BranchID != null;
					string masterPeriodID =
						isNeedConvertToMaster
							? FinPeriodRepository.
								GetByID(filter.WOFinPeriodID, PXAccess.GetParentOrganizationID(filter.BranchID)).
								MasterFinPeriodID
							: filter.WOFinPeriodID;

					pe.CreateWriteOff(reasonCode, value.ToString(), filter.WODate, masterPeriodID, doc);

					if (pe.ARDocument != null && !paylist.Contains(pe.ARDocument))
					{
						paylist.Add(pe.ARDocument);
						paybind.Add(idx);
					}
				}
				catch (Exception e)
				{
					PXProcessing<ARRegister>.SetError(idx, e);
					failed = true;
				}
			}
			
			if (paylist.Count > 0)
			{
				try
				{
					ARDocumentRelease.ReleaseDoc(paylist, false);
				}
				catch (PXMassProcessException e)
				{
					PXProcessing<ARRegister>.SetError(paybind[e.ListIndex], e.InnerException);
					failed = true;
				}
			}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}

		public PXAction<ARWriteOffFilter> ShowCustomer;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable showCustomer(PXAdapter adapter)
		{
			PXRedirectHelper.TryRedirect(customer.Cache, CUSTOMER, Messages.Customer, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		[Serializable()]
		public class ARRegisterEx : ARRegister
		{
			#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public override bool? Selected { get; set; }
			#endregion
			#region DocBal
			public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }

			[PXDBBaseCury()]
			[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			//Works only for totals, calculated on row updating
			[PXParent(typeof(Select<ARWriteOffFilter>), UseCurrent = true)]
			[PXUnboundFormula(typeof(Switch<Case<Where<ARRegisterEx.selected, Equal<True>>, ARRegisterEx.docBal>, decimal0>), typeof(SumCalc<ARWriteOffFilter.selTotal>))]
			public override decimal? DocBal { get; set; }
			#endregion
			#region ReasonCode
			public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
			protected String _ReasonCode;
			[PXString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
			[PXUIField(DisplayName = "Reason Code", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where2<Where<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>, And<Current<ARWriteOffFilter.woType>, Equal<ARWriteOffType.smallBalanceWO>>>,
			Or<Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>, And<Current<ARWriteOffFilter.woType>, Equal<ARWriteOffType.smallCreditWO>>>>>>))]
			public virtual String ReasonCode
			{
				get
				{
					return this._ReasonCode;
				}
				set
				{
					this._ReasonCode = value;
				}
			}
			#endregion
		}
	}

	public interface IARWriteOffEntry
	{
		void CreateWriteOff(ReasonCode reasonCode, string WOSubCD, DateTime? WODate, string masterPeriodID, ARRegister ardoc);
		ARRegister ARDocument
		{
			get;
		}
	}

	[PXHidden]
	public class ARSmallCreditWriteOffEntry : PXGraph<ARSmallCreditWriteOffEntry>, IARWriteOffEntry
	{
		public PXSelect<ARInvoice> Document;
		public PXSelect<ARAdjust> Adjustments;

		public PXSetup<ARSetup> ARSetup;
		public CMSetupSelect CMSetup;
		public PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>, And<Customer.smallBalanceAllow, Equal<True>>>> customer;
		public PXSelect<CurrencyInfo> currencyinfo;

		public PXSelect<ARPayment, Where<ARPayment.customerID, Equal<Required<ARPayment.customerID>>, And<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>> ARPayment_CustomerID_DocType_RefNbr;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		public PXSelect<ARBalances> arbalances;

		#region Cache Attached
		#region ARAdjust
		[PXDBInt()]
		[PXDefault()]
		protected virtual void ARAdjust_CustomerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjdCuryID")]
		protected virtual void ARAdjust_AdjdCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjdDocType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjdRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		protected virtual void ARAdjust_AdjNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjdOrigCuryID")]
		protected virtual void ARAdjust_AdjdOrigCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDate()]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgDocDate_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ARInvoice
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(ARWriteOffType.SmallCreditWO)]
		[ARWriteOffType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]		
		protected virtual void ARInvoice_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<ARInvoice.refNbr, Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>>>))]
		[AutoNumber(typeof(ARSetup.writeOffNumberingID), typeof(ARInvoice.docDate))]		
		protected virtual void ARInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}
		[Account(DisplayName = "Credit WO Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault()]		
		protected virtual void ARInvoice_ARAccountID_CacheAttached(PXCache sender)
		{
		}
		[SubAccount(typeof(ARInvoice.aRAccountID), DisplayName = "Credit WO Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		protected virtual void ARInvoice_ARSubID_CacheAttached(PXCache sender)
		{
		}
		[PXString(1, IsFixed = true)]
		[ARWriteOffType.DefaultDrCr(typeof(ARInvoice.docType))]
		protected virtual void ARInvoice_DrCr_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		public ARRegister ARDocument
		{
			get
			{
				return (ARRegister)this.Document.Current;
			}
		}

		protected Int32? _CustomerID;
		public Customer CUSTOMER
		{
			get
			{
				return customer.Select(_CustomerID);
			}
		}

		public virtual void CreateWriteOff(ReasonCode reasonCode, string WOSubCD, DateTime? WODate, string masterPeriodID, ARRegister ardoc)
		{
			Int32? WOAccountID = reasonCode.AccountID;
			if ( WOAccountID == null )
				throw new ArgumentNullException("WOAccountID");

			if ( WOSubCD == null )
				throw new ArgumentNullException("WOSubCD");


			this.Clear();
			customer.Current = null;
			_CustomerID = ardoc.CustomerID;

			ARInvoice payment = new ARInvoice();
			payment.BranchID = ardoc.BranchID;
			payment = PXCache<ARInvoice>.CreateCopy(this.Document.Insert(payment));
			payment.CustomerID = ardoc.CustomerID;
			payment.CustomerLocationID = ardoc.CustomerLocationID;
			payment.DocDate = WODate;

			FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(Document.Cache, payment, masterPeriodID);

			payment.CuryID = ardoc.CuryID;
			payment.ARAccountID = WOAccountID;
			payment.Hold = false;
			payment.AdjCntr = -1;
			payment.DocDesc = reasonCode.Descr;

			payment = this.Document.Update(payment);

			this.Document.Cache.SetValueExt<ARInvoice.aRSubID>(payment, WOSubCD);

			ARAddressAttribute.DefaultRecord<ARInvoice.billAddressID>(Document.Cache, payment);
			ARContactAttribute.DefaultRecord<ARInvoice.billContactID>(Document.Cache, payment);
			ARShippingAddressAttribute.DefaultRecord<ARInvoice.shipAddressID>(Document.Cache, payment);
			ARShippingContactAttribute.DefaultRecord<ARInvoice.shipContactID>(Document.Cache, payment);

			ARAdjust adj = new ARAdjust();
			adj.AdjgDocType = ardoc.DocType;
			adj.AdjgRefNbr = ardoc.RefNbr;
			adj.AdjNbr = payment.AdjCntr;
			adj.AdjdCuryInfoID = Document.Current.CuryInfoID;
			adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;

			adj = this.Adjustments.Insert(adj);

			Document.Current.CuryDocBal += (decimal)adj.CuryAdjdAmt;
			Document.Current.DocBal += (decimal)adj.AdjAmt;

			Document.Current.CuryOrigDocAmt += (decimal)adj.CuryAdjdAmt;
			Document.Current.OrigDocAmt += (decimal)adj.AdjAmt;

			ARReleaseProcess.UpdateARBalances(this, Document.Current, adj.AdjAmt);

			Document.Cache.MarkUpdated(Document.Current);

			this.Actions.PressSave();
		}

		protected virtual void ARAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current == null)
			{
				e.Cancel = true;
				return;
			}

			var adjust = e.Row as ARAdjust;

			adjust.AdjdDocType = Document.Current.DocType;
			adjust.AdjdRefNbr = Document.Current.RefNbr;
			adjust.AdjdBranchID = Document.Current.BranchID;
			adjust.AdjdCuryInfoID = Document.Current.CuryInfoID;
			adjust.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
		}

		protected virtual void ARAdjust_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var adjust = e.Row as ARAdjust;

			foreach (PXResult<ARPayment, CurrencyInfo> res in PXSelectJoin<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>, Where<ARPayment.customerID, Equal<Current<ARInvoice.customerID>>, And<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>>.Select(this, adjust.AdjgDocType, adjust.AdjgRefNbr))
			{
				PXCache<CurrencyInfo>.RestoreCopy(currencyinfo.Current, ((CurrencyInfo)res));
				currencyinfo.Current.CuryInfoID = Document.Current.CuryInfoID;

				ARPayment payment = res;

				adjust.CustomerID = Document.Current.CustomerID;
				//must be from SC for Trial Balance & Release
				adjust.AdjgDocDate = Document.Current.DocDate;
				adjust.AdjgFinPeriodID = Document.Current.FinPeriodID;
				adjust.AdjgTranPeriodID = Document.Current.TranPeriodID;
				adjust.AdjgCuryInfoID = payment.CuryInfoID;
				adjust.AdjgBranchID = payment.BranchID;
				adjust.AdjdCustomerID = Document.Current.CustomerID;
				adjust.AdjdARAcct = payment.ARAccountID;
				adjust.AdjdARSub = payment.ARSubID;
				adjust.AdjdDocDate = Document.Current.DocDate;
				adjust.AdjdTranPeriodID = Document.Current.TranPeriodID;
				adjust.AdjdFinPeriodID = Document.Current.FinPeriodID;

				adjust.Released = false;

				CalcBalances(e.Row, false, e.ExternalCall);

				decimal? CuryApplAmt = adjust.CuryDocBal;
				decimal? CuryApplDiscAmt = 0m;

				adjust.CuryAdjgAmt = CuryApplAmt;
				adjust.CuryAdjgDiscAmt = CuryApplDiscAmt;

				CalcBalances(e.Row, true, e.ExternalCall);
			}
		}

		private void CalcBalances(object row, bool isCalcRGOL, bool DiscOnDiscDate)
		{
			ARAdjust adj = (ARAdjust)row;

			PXCache cache = Caches[typeof(ARAdjust)];

			ARPayment payment = (ARPayment)ARPayment_CustomerID_DocType_RefNbr.Select(adj.CustomerID, adj.AdjgDocType, adj.AdjgRefNbr);
			if (payment == null)
			{
				return;
			}

			PaymentEntry.CalcBalances<ARPayment, ARAdjust>(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, payment, adj);

			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount<ARPayment, ARAdjust>(adj.AdjgDocDate, payment, adj);
			}

			PaymentEntry.AdjustBalance<ARAdjust>(CurrencyInfo_CuryInfoID, adj);
			if (isCalcRGOL)
			{
				PaymentEntry.CalcRGOL<ARPayment, ARAdjust>(CurrencyInfo_CuryInfoID, payment, adj);
				adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -1m * adj.RGOLAmt : adj.RGOLAmt;
			}
		}

	}

	[PXHidden]
	public class ARSmallBalanceWriteOffEntry : PXGraph<ARSmallBalanceWriteOffEntry>, IARWriteOffEntry
	{
		public PXSelect<ARPayment> Document;
		public PXSelect<ARAdjust> Adjustments;

		public PXSetup<ARSetup> ARSetup;
		public CMSetupSelect CMSetup;
		public PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>, And<Customer.smallBalanceAllow, Equal<True>>>> customer;
		public PXSelect<CurrencyInfo> currencyinfo;

		public PXSelect<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>> ARInvoice_CustomerID_DocType_RefNbr;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		public PXSelect<ARBalances> arbalances;

		#region Cache Attached
		#region ARAdjust
		[PXDBInt()]
		[PXDefault()]
		protected virtual void ARAdjust_CustomerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjdCuryID")]
		protected virtual void ARAdjust_AdjdCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjdDocType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		protected virtual void ARAdjust_AdjdRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		protected virtual void ARAdjust_AdjNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjdOrigCuryID")]
		protected virtual void ARAdjust_AdjdOrigCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDate()]
		[PXDefault()]
		protected virtual void ARAdjust_AdjgDocDate_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ARPayment
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(ARWriteOffType.SmallBalanceWO)]
		[ARWriteOffType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void ARPayment_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<ARPayment.refNbr, Where<ARPayment.docType, Equal<Current<ARPayment.docType>>>>))]
		[AutoNumber(typeof(ARSetup.writeOffNumberingID), typeof(ARPayment.docDate))]
		protected virtual void ARPayment_RefNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt()]
		protected virtual void ARPayment_CashAccountID_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt()]
		protected virtual void ARPayment_CashSubID_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		protected virtual void ARPayment_CATranID_CacheAttached(PXCache sender)
		{
		}
		[Account(DisplayName = "Balance WO Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault()]		
		protected virtual void ARPayment_ARAccountID_CacheAttached(PXCache sender)
		{
		}
		[SubAccount(typeof(ARPayment.aRAccountID), DisplayName = "Balance WO Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		protected virtual void ARPayment_ARSubID_CacheAttached(PXCache sender)
		{
		}
		[PXString(1, IsFixed = true)]
		[ARWriteOffType.DefaultDrCr(typeof(ARPayment.docType))]
		protected virtual void ARPayment_DrCr_CacheAttached(PXCache sender)
		{
		}		
		[PXDBBool()]
		[PXDefault(false)]
		protected virtual void ARPayment_DepositAsBatch_CacheAttached(PXCache sender)
		{
		}
		[PXDBBool()]
		[PXDefault(false)]		
		protected virtual void ARPayment_Deposited_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		public ARSmallBalanceWriteOffEntry()
		{
			ARSetup setup = ARSetup.Current;
		}

		public ARRegister ARDocument
		{
			get
			{
				return (ARRegister)this.Document.Current;
			}
		}

		protected Int32? _CustomerID;
		public Customer CUSTOMER
		{
			get
			{
				return customer.Select(_CustomerID);
			}
		}

		public virtual void CreateWriteOff(ReasonCode reasonCode, string WOSubCD, DateTime? WODate, string masterPeriodID, ARRegister ardoc)
		{
			Int32? WOAccountID = reasonCode.AccountID;
			if (WOAccountID == null)
				throw new ArgumentNullException("WOAccountID");

			if (WOSubCD == null)
				throw new ArgumentNullException("WOSubCD");

			this.Clear();
			customer.Current = null;
			_CustomerID = ardoc.CustomerID;

			ARPayment payment = new ARPayment();
			payment.BranchID = ardoc.BranchID;
			payment = PXCache<ARPayment>.CreateCopy(this.Document.Insert(payment));
			payment.CustomerID = ardoc.CustomerID;
			payment.CustomerLocationID = ardoc.CustomerLocationID;
			payment.AdjDate = WODate;
			FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.adjFinPeriodID>(Document.Cache, payment, masterPeriodID);
			payment.DocDate = WODate;
			FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.finPeriodID>(Document.Cache, payment, masterPeriodID);
			payment.CuryID = ardoc.CuryID;
			payment.ARAccountID = WOAccountID;
			payment.Hold = false;
			payment.DocDesc = reasonCode.Descr;

			payment = this.Document.Update(payment);
			this.Document.Cache.SetValueExt<ARPayment.aRSubID>(payment, WOSubCD);

			var trans = ardoc.PaymentsByLinesAllowed == true
				? PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
						And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(this, ardoc.DocType, ardoc.RefNbr)
				: new PXResultset<ARTran>() { null };

			foreach (ARTran tran in trans)
			{
				ARAdjust adj = new ARAdjust();
				adj.AdjdDocType = ardoc.DocType;
				adj.AdjdRefNbr = ardoc.RefNbr;
				adj.AdjdLineNbr = tran?.LineNbr ?? 0;

				adj = this.Adjustments.Insert(adj);

				Document.Current.CuryDocBal += (decimal)adj.CuryAdjgAmt;
				Document.Current.DocBal += (decimal)adj.AdjAmt;

				Document.Current.CuryOrigDocAmt += (decimal)adj.CuryAdjgAmt;
				Document.Current.OrigDocAmt += (decimal)adj.AdjAmt;

				ARReleaseProcess.UpdateARBalances(this, Document.Current, adj.AdjAmt);
			}

			Document.Cache.MarkUpdated(Document.Current);

			this.Actions.PressSave();
		}

		protected virtual void ARAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current == null)
			{
				e.Cancel = true;
				return;
			}

			var adjust = e.Row as ARAdjust;

			adjust.AdjgDocType = Document.Current.DocType;
			adjust.AdjgRefNbr = Document.Current.RefNbr;
			adjust.AdjgBranchID = Document.Current.BranchID;
			adjust.AdjgCuryInfoID = Document.Current.CuryInfoID;
			adjust.AdjgFinPeriodID = Document.Current.FinPeriodID;
			adjust.AdjgTranPeriodID = Document.Current.TranPeriodID;
		}

		protected virtual void ARAdjust_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var adjust = e.Row as ARAdjust;

			foreach (PXResult<ARInvoice, CurrencyInfo, ARTran> res in PXSelectJoin<ARInvoice, 
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>, 
				LeftJoin<ARTran, On<ARInvoice.paymentsByLinesAllowed, Equal<True>,
					And<ARTran.tranType, Equal<ARInvoice.docType>,
					And<ARTran.refNbr, Equal<ARInvoice.refNbr>,
					And<ARTran.lineNbr, Equal<Required<ARAdjust.adjdLineNbr>>>>>>>>,
				Where <ARInvoice.customerID, Equal<Current<ARPayment.customerID>>, 
					And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, 
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>>
				.Select(this, adjust.AdjdLineNbr, adjust.AdjdDocType, adjust.AdjdRefNbr))
			{
				ARTran tran = res;

				CurrencyInfo info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info_copy.CuryInfoID = null;
				info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);

				//currencyinfo.Cache.SetValueExt<CurrencyInfo.curyEffDate>(info_copy, Document.Current.DocDate);
				info_copy.SetCuryEffDate(currencyinfo.Cache, Document.Current.DocDate);

				ARInvoice invoice = res;

				adjust.CustomerID = Document.Current.CustomerID;
				adjust.AdjgDocDate = Document.Current.DocDate;
				adjust.AdjgCuryInfoID = Document.Current.CuryInfoID;
				adjust.AdjdCustomerID = invoice.CustomerID;
				adjust.AdjdCuryInfoID = info_copy.CuryInfoID;
				adjust.AdjdOrigCuryInfoID = invoice.CuryInfoID;
				adjust.AdjdBranchID = invoice.BranchID;
				adjust.AdjdARAcct = invoice.ARAccountID;
				adjust.AdjdARSub = invoice.ARSubID;
				adjust.AdjdDocDate = invoice.DocDate;
				adjust.AdjdTranPeriodID = invoice.TranPeriodID;
				adjust.AdjdFinPeriodID = invoice.FinPeriodID;
				adjust.Released = false;

				CalcBalances(e.Row, false, e.ExternalCall, tran);

				decimal? CuryApplAmt = adjust.CuryDocBal;
				decimal? CuryApplDiscAmt = 0m;

				adjust.CuryAdjgAmt = CuryApplAmt;
				adjust.CuryAdjgDiscAmt = CuryApplDiscAmt;

				CalcBalances(e.Row, true, e.ExternalCall, tran);
			}
		}

		private void CalcBalances(object row, bool isCalcRGOL, bool DiscOnDiscDate, ARTran tran)
		{
			ARAdjust adj = (ARAdjust)row;

			PXCache cache = Caches[typeof(ARAdjust)];

			ARInvoice invoice = (ARInvoice)ARInvoice_CustomerID_DocType_RefNbr.Select(adj.AdjdCustomerID, adj.AdjdDocType, adj.AdjdRefNbr);
			if (invoice == null)
			{
				return;
			}

			PaymentEntry.CalcBalances<ARInvoice, ARAdjust>(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj, tran);

			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount<ARInvoice, ARAdjust>(adj.AdjgDocDate, invoice, adj);
			}

			PaymentEntry.AdjustBalance<ARAdjust>(CurrencyInfo_CuryInfoID, adj);
			if (isCalcRGOL)
			{
				PaymentEntry.CalcRGOL<ARInvoice, ARAdjust>(CurrencyInfo_CuryInfoID, invoice, adj, tran);
				adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -1m * adj.RGOLAmt : adj.RGOLAmt;
			}
		}
	}
}
