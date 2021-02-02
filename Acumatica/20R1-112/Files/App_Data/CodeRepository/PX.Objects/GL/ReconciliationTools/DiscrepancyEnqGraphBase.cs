using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace ReconciliationTools
{
	#region Internal Types

	public interface IDiscrepancyEnqResult
	{
		decimal? GLTurnover { get; set; }
		decimal? XXTurnover { get; set; }
		decimal? Discrepancy { get; }
	}

	[Serializable]
	public partial class DiscrepancyEnqFilter : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false, null, Required = false)]
		public int? OrganizationID { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[BranchOfOrganization(typeof(DiscrepancyEnqFilter.organizationID), false)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region UseMasterCalendar
		public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

		[PXBool]
		public bool? UseMasterCalendar { get; set; }
		#endregion
		#region PeriodFrom
		public abstract class periodFrom : PX.Data.BQL.BqlString.Field<periodFrom> { }

		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(DiscrepancyEnqFilter.branchID),
			organizationSourceType: typeof(DiscrepancyEnqFilter.organizationID),
			useMasterCalendarSourceType: typeof(DiscrepancyEnqFilter.useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXDefault]
		[PXUIField(DisplayName = "From Period")]
		public virtual string PeriodFrom
		{
			get;
			set;
		}
		#endregion
		#region PeriodTo
		public abstract class periodTo : PX.Data.BQL.BqlString.Field<periodTo> { }

		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(DiscrepancyEnqFilter.branchID),
			organizationSourceType: typeof(DiscrepancyEnqFilter.organizationID),
			useMasterCalendarSourceType: typeof(DiscrepancyEnqFilter.useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXDefault]
		[PXUIField(DisplayName = "To Period")]
		public virtual string PeriodTo
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[Account(null, DisplayName = "Account", DescriptionField = typeof(Account.description))]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubCD
		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Invisible, FieldClass = SubAccountAttribute.DimensionName)]
		[PXDimension("SUBACCOUNT", ValidComboRequired = false)]
		public virtual string SubCD
		{
			get;
			set;
		}
		#endregion
		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { }

		[PXDBString(30, IsUnicode = true)]
		public virtual string SubCDWildcard
		{
			get
			{
				return SubCDUtils.CreateSubCDWildcard(SubCD, SubAccountAttribute.DimensionName);
			}
		}
		#endregion

		#region ShowOnlyWithDiscrepancy
		public abstract class showOnlyWithDiscrepancy : PX.Data.BQL.BqlBool.Field<showOnlyWithDiscrepancy> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Show Only Documents with Discrepancy")]
		public virtual bool? ShowOnlyWithDiscrepancy
		{
			get;
			set;
		}
		#endregion

		#region TotalGLAmount
		public abstract class totalGLAmount : PX.Data.BQL.BqlDecimal.Field<totalGLAmount> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total GL Amount", Enabled = false)]
		public virtual decimal? TotalGLAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalXXAmount
		public abstract class totalXXAmount : PX.Data.BQL.BqlDecimal.Field<totalXXAmount> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Module Amount", Enabled = false)]
		public virtual decimal? TotalXXAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalDiscrepancy
		public abstract class totalDiscrepancy : PX.Data.BQL.BqlDecimal.Field<totalDiscrepancy> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Discrepancy", Enabled = false)]
		public virtual decimal? TotalDiscrepancy
		{
			get;
			set;
		}
		#endregion
		#region FilterDetails		
		public abstract class filterDetails : IBqlField { };
		[PXResultStorage]
		public byte[][] FilterDetails { get; set; }
		#endregion
	}

	[Serializable]
	public partial class DiscrepancyByAccountEnqResult : GLTranR, IDiscrepancyEnqResult
	{
		#region GLTurnover
		public abstract class gLTurnover : PX.Data.BQL.BqlDecimal.Field<gLTurnover> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "GL Turnover")]
		public virtual decimal? GLTurnover
		{
			get;
			set;
		}
		#endregion
		#region XXTurnover
		public abstract class aPTurnover : IBqlField { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Module Turnover")]
		public virtual decimal? XXTurnover
		{
			get;
			set;
		}
		#endregion
		#region NonXXTrans
		public abstract class nonXXTrans : PX.Data.BQL.BqlDecimal.Field<nonXXTrans> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Non-Module Transactions")]
		public virtual decimal? NonXXTrans
		{
			get;
			set;
		}
		#endregion
		#region Discrepancy
		public abstract class discrepancy : PX.Data.BQL.BqlDecimal.Field<discrepancy> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discrepancy")]
		public virtual decimal? Discrepancy
		{
			get
			{
				return GLTurnover - XXTurnover - NonXXTrans;
			}
		}
		#endregion
	}

	public class DiscrepancyByAccountEnqResultKey
	{
		public string FinPeriodID;
		public int? AccountID;
		public int? SubID;

		public DiscrepancyByAccountEnqResultKey(GLTran tran)
		{
			FinPeriodID = tran.FinPeriodID;
			AccountID = tran.AccountID;
			SubID = tran.SubID;
		}

		public override int GetHashCode()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.subAccount>()
				? Tuple.Create(FinPeriodID, AccountID, SubID).GetHashCode()
				: Tuple.Create(FinPeriodID, AccountID).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			DiscrepancyByAccountEnqResultKey value = (DiscrepancyByAccountEnqResultKey)obj;

			return PXAccess.FeatureInstalled<FeaturesSet.subAccount>()
				? FinPeriodID == value.FinPeriodID && AccountID == value.AccountID && SubID == value.SubID
				: FinPeriodID == value.FinPeriodID && AccountID == value.AccountID;
		}
	}

	#endregion

	[TableAndChartDashboardType]
	public class DiscrepancyEnqGraphBase<TGraph, TEnqFilter, TEnqResult> : PXGraph<TGraph>
		where TGraph : PXGraph
		where TEnqFilter : DiscrepancyEnqFilter, new()
		where TEnqResult : class, IBqlTable, IDiscrepancyEnqResult, new()
	{
		public PXFilter<TEnqFilter> Filter;

		[PXFilterable]
		public PXSelect<TEnqResult> Rows;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public IEnumerable Refresh(PXAdapter adapter)
		{
			this.Filter.Current.FilterDetails = null;
			return adapter.Get();
		}
		protected virtual IEnumerable filter()
		{
			PXCache cache = Caches[typeof(TEnqFilter)];
			if (cache != null)
			{
				DiscrepancyEnqFilter filter = cache.Current as DiscrepancyEnqFilter;
				if (filter != null)
				{
					filter.TotalGLAmount = 0m;
					filter.TotalXXAmount = 0m;
					filter.TotalDiscrepancy = 0m;

					foreach (TEnqResult res in Rows.Select())
					{
						filter.TotalGLAmount += (res.GLTurnover ?? 0m);
						filter.TotalXXAmount += (res.XXTurnover ?? 0m);
						filter.TotalDiscrepancy += (res.Discrepancy ?? 0m);
					}
				}
			}

			yield return cache.Current;
			cache.IsDirty = false;
		}

		protected IEnumerable rows()
		{
			PXCache cache = Caches[typeof(TEnqFilter)];
			DiscrepancyEnqFilter header = cache.Current as DiscrepancyEnqFilter;
			if (header.FilterDetails != null)
			{
				PXFieldState state = Filter.Cache.GetStateExt<DiscrepancyEnqFilter.filterDetails>(header) as PXFieldState;
				if (state?.Value is IEnumerable value)
					return value;
			}
			List<TEnqResult> result = SelectDetails();
			Filter.Cache.SetValueExt<DiscrepancyEnqFilter.filterDetails>(header, result);
			return result;
		}

		protected virtual List<TEnqResult> SelectDetails()
		{
			return null;
		}
	
		#region Ctor + Overrides

		public DiscrepancyEnqGraphBase()
		{
			Rows.Cache.AllowDelete = false;
			Rows.Cache.AllowInsert = false;
			Rows.Cache.AllowUpdate = false;
			RowUpdated.AddHandler(typeof(TEnqFilter), FilterRowUpdated);
			FieldUpdated.AddHandler(typeof(TEnqFilter), typeof(DiscrepancyEnqFilter.periodFrom).Name, PeriodFromFieldUpdated);
			FieldUpdated.AddHandler(typeof(TEnqFilter), typeof(DiscrepancyEnqFilter.periodTo).Name, PeriodToFieldUpdated);
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Actions

		public PXAction<TEnqFilter> previousPeriod;
		public PXAction<TEnqFilter> nextPeriod;

		[PXUIField(
			DisplayName = "",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousPeriod(PXAdapter adapter)
		{
			DiscrepancyEnqFilter filter = Filter.Current as DiscrepancyEnqFilter;

			filter.UseMasterCalendar = filter.OrganizationID == null && filter.BranchID == null;
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod prevPeriodFrom = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.PeriodFrom, looped: true);
			filter.PeriodFrom = prevPeriodFrom != null ? prevPeriodFrom.FinPeriodID : null;

			FinPeriod prevPeriodTo = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.PeriodTo, looped: true);
			filter.PeriodTo = prevPeriodTo != null ? prevPeriodTo.FinPeriodID : null;
			filter.FilterDetails = null;
			return adapter.Get();
		}

		[PXUIField(
			DisplayName = "",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			DiscrepancyEnqFilter filter = Filter.Current as DiscrepancyEnqFilter;

			filter.UseMasterCalendar = filter.OrganizationID == null && filter.BranchID == null;
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod nextPeriodFrom = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.PeriodFrom, looped: true);
			filter.PeriodFrom = nextPeriodFrom != null ? nextPeriodFrom.FinPeriodID : null;

			FinPeriod nextPeriodTo = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.PeriodTo, looped: true);
			filter.PeriodTo = nextPeriodTo != null ? nextPeriodTo.FinPeriodID : null;
			filter.FilterDetails = null;
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		protected virtual void PeriodFromFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DiscrepancyEnqFilter row = (DiscrepancyEnqFilter)e.Row;

			if (string.CompareOrdinal(row.PeriodFrom, row.PeriodTo) > 0)
			{
				cache.SetValue<DiscrepancyEnqFilter.periodTo>(e.Row, row.PeriodFrom);
			}
		}

		protected virtual void PeriodToFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DiscrepancyEnqFilter row = (DiscrepancyEnqFilter)e.Row;

			if (string.CompareOrdinal(row.PeriodFrom, row.PeriodTo) > 0)
			{
				cache.SetValue<DiscrepancyEnqFilter.periodFrom>(e.Row, row.PeriodTo);
			}
		}

		protected virtual void FilterRowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			DiscrepancyEnqFilter row = (DiscrepancyEnqFilter)e.Row;
			row.FilterDetails = null;
		}
	

		#endregion

		#region Utility functions

		protected virtual string GetSubCD(int? subID)
		{
			Sub sub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>
				.SelectSingleBound(this, null, subID);
			return sub?.SubCD;
		}

		protected virtual decimal CalcGLTurnover(GLTran tran)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}