using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.FA
{
	public class CalcDeprProcess : PXGraph<CalcDeprProcess>
	{
		public PXCancel<BalanceFilter> Cancel;
		public PXFilter<BalanceFilter> Filter;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<BalanceFilter> ViewAsset;
		public PXAction<BalanceFilter> ViewBook;
		public PXAction<BalanceFilter> ViewClass;

		[PXFilterable]
		public PXFilteredProcessingJoin<FABookBalance, BalanceFilter,
				InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
				InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>,
				LeftJoin<Account, On<Account.accountID, Equal<FixedAsset.fAAccountID>>>>>> Balances;

		public PXSetup<Company> company;
		public PXSetup<FASetup> fasetup;

		public CalcDeprProcess()
		{
			object setup = fasetup.Current;
		}

		#region CacheAttached
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<FixedAsset.assetID>),
			SubstituteKey = typeof(FixedAsset.assetCD), CacheGlobal = true, DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Fixed Asset", Enabled = false)]
		public virtual void FABookBalance_AssetID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributesAttribute(Method = MergeMethod.Append)]
		[PXSelector(typeof(FABook.bookID),
			SubstituteKey = typeof(FABook.bookCode),
			DescriptionField = typeof(FABook.description))]
		protected virtual void FABookBalance_BookID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		protected virtual IEnumerable balances()
		{
			BalanceFilter filter = Filter.Current;

			PXView view = new Select2<FABookBalance,
					InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
					InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>,
					LeftJoin<Account, On<Account.accountID, Equal<FixedAsset.fAAccountID>>>>>,
					Where<FABookBalance.depreciate, Equal<True>,
						And<FABookBalance.status, Equal<FixedAssetStatus.active>,
						And<FADetails.status, Equal<FixedAssetStatus.active>>>>>()
				.CreateView(this, mergeCache: PXView.MaximumRows > 1);

			if (filter.BookID != null)
			{
				view.WhereAnd<Where<FABookBalance.bookID, Equal<Current<BalanceFilter.bookID>>>>();
			}
			if (filter.ClassID != null)
			{
				view.WhereAnd<Where<FixedAsset.classID, Equal<Current<BalanceFilter.classID>>>>();
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() || filter.OrgBAccountID != null)
			{
				view.WhereAnd<Where<FixedAsset.branchID, Inside<Current<BalanceFilter.orgBAccountID>>>>();
			}
			if (!string.IsNullOrEmpty(filter.PeriodID))
			{
				view.WhereAnd<Where<FABookBalance.currDeprPeriod, LessEqual<Current<BalanceFilter.periodID>>>>();
			}
			if (filter.ParentAssetID != null)
			{
				view.WhereAnd<Where<FixedAsset.parentAssetID, Equal<Current<BalanceFilter.parentAssetID>>>>();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach (PXFilterRow f in PXView.Filters)
			{
				if (f.DataField.ToLower() == "status")
				{
					f.DataField = "FADetails__Status";
				}
				newFilters.Add(f);
			}
			List<object> list = view.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		protected virtual void FABookBalance_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;
			if (bal == null || PXLongOperation.Exists(UID)) return;

			try
			{
				AssetProcess.CheckUnreleasedTransactions(this, bal.AssetID);
			}
			catch (PXException exc)
			{
				PXUIFieldAttribute.SetEnabled<FABookBalance.selected>(sender, bal, false);
				sender.RaiseExceptionHandling<FABookBalance.selected>(bal, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowWarning));
			}
		}

		private static IEnumerable<FABookBalance> GetProcessableRecords(IEnumerable<FABookBalance> list)
		{
			PXGraph graph = CreateInstance<PXGraph>();
			return list.Where(balance => !AssetProcess.UnreleasedTransactionsExistsForAsset(graph, balance.AssetID));
		}

		private void SetProcessDelegate()
		{
			BalanceFilter filter = Filter.Current;
			bool depreciate = filter.Action == BalanceFilter.action.Depreciate;

			Balances.SetProcessDelegate(delegate(List<FABookBalance> list)
			{
				if (PXLongOperation.GetTaskList().Where(_ => _.Screen == "FA.50.20.00").ToArray().Length > 1)
				{
					throw new PXException(Messages.AnotherDeprRunning);
				}

				IEnumerable<FABookBalance> balances = GetProcessableRecords(list);

				bool success = depreciate
					? AssetProcess.DepreciateAsset(balances, null, filter.PeriodID, true)
					: AssetProcess.CalculateAsset(balances, filter.PeriodID);
				if (!success)
				{
					throw new PXOperationCompletedWithErrorException();
				}
			});

			bool canDepreciate = !string.IsNullOrEmpty(filter.PeriodID) && fasetup.Current.UpdateGL == true;
			PXUIFieldAttribute.SetEnabled<BalanceFilter.action>(Filter.Cache, filter, canDepreciate);
			if (!canDepreciate)
			{
				filter.Action = BalanceFilter.action.Calculate;
			}
		}

		protected virtual void BalanceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			SetProcessDelegate();
		}

		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable actionsFolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable viewAsset(PXAdapter adapter)
		{
			if (Balances.Current != null)
			{
				AssetMaint graph = CreateInstance<AssetMaint>();
				graph.CurrentAsset.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.assetID>>>>.Select(this);
				if (graph.CurrentAsset.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewAsset") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewBook(PXAdapter adapter)
		{
			if (Balances.Current != null)
			{
				BookMaint graph = CreateInstance<BookMaint>();
				graph.Book.Current = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>.Select(this);
				if (graph.Book.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewBook") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewClass, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewClass(PXAdapter adapter)
		{
			if (Balances.Current != null)
			{
				AssetClassMaint graph = CreateInstance<AssetClassMaint>();
				graph.CurrentAssetClass.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.classID>>>>.Select(this);
				if (graph.CurrentAssetClass.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewClass") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}
	}

	[Serializable]
	public partial class BalanceFilter : ProcessAssetFilter
	{
		#region OrganizationID
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
		public override int? OrganizationID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[BranchOfOrganization(
			organizationFieldType: typeof(BalanceFilter.organizationID),
			onlyActive: true,
			featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
			IsDetail = false, 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public override int? BranchID
		{
			get;
			set;
		}
		#endregion
		
		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXUIField(DisplayName = "To Period")]
		[FABookPeriodOpenInGLSelector(
			dateType: typeof(AccessInfo.businessDate),
			organizationSourceType: typeof(BalanceFilter.organizationID),
			branchSourceType: typeof(BalanceFilter.branchID),
			bookSourceType: typeof(BalanceFilter.bookID),
			isBookRequired: false)]
		public virtual string PeriodID
		{
			get;
			set;
		}
		#endregion
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
				new[] { Calculate, Depreciate },
				new[] { Messages.Calculate, Messages.Depreciate })
				{ }
			}

			public const string Calculate = "C";
			public const string Depreciate = "D";

			public class calculate : PX.Data.BQL.BqlString.Constant<calculate>
			{
				public calculate() : base(Calculate) { }
			}
			public class depreciate : PX.Data.BQL.BqlString.Constant<depreciate>
			{
				public depreciate() : base(Depreciate) { }
			}
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BalanceFilter.action.Calculate)]
		[BalanceFilter.action.List]
		[PXUIField(DisplayName = "Action", Required = true)]
		public virtual string Action { get; set; }
		#endregion
	}
}
