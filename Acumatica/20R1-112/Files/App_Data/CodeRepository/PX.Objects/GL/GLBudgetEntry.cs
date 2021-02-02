using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Descriptor;
using PX.SM;

namespace PX.Objects.GL
{
	public class GLBudgetEntry : PXGraph<GLBudgetEntry>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		#region Selects

		public PXFilter<BudgetFilter> Filter;
		public PXFilter<BudgetDistributeFilter> DistrFilter;
		public PXFilter<BudgetPreloadFilter> PreloadFilter;
		public PXFilter<ManageBudgetDialog> ManageDialog;

		[PXImport(typeof(BudgetFilter))]
		public PXSelect<GLBudgetLine,
			Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
				And<GLBudgetLine.subID, Like<Current<BudgetFilter.subCDWildcard>>>>>>,
				OrderBy<Asc<GLBudgetLine.sortOrder>>> BudgetArticles;

		public PXSelect<GLBudgetLineDetail,
			Where<GLBudgetLineDetail.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
			And<GLBudgetLineDetail.branchID, Equal<Current<BudgetFilter.branchID>>,
			And<GLBudgetLineDetail.finYear, Equal<Optional<BudgetFilter.finYear>>,
			And<GLBudgetLineDetail.groupID, Equal<Required<GLBudgetLineDetail.groupID>>>
				>>>> Allocations;

		public PXSelect<GLBudgetLine, Where<GLBudgetLine.parentGroupID, Equal<Argument<Guid?>>>,
			OrderBy<Asc<GLBudgetLine.treeSortOrder>>> Tree;

		public PXSelect<Neighbour> Neighbour;

		bool SubEnabled = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.subAccount>();

		#endregion

		#region Helper
		bool IsNullOrEmpty(Guid? _guid) => _guid == null || Guid.Empty.Equals(_guid);
		bool IsEmpty(Guid? _guid) => Guid.Empty.Equals(_guid);

		protected void HoldNotchanged(PXCache cache)
		{
			foreach (var o in cache.Cached)
			{
				if (cache.GetStatus(o) == PXEntryStatus.Notchanged)
					cache.SetStatus(o, PXEntryStatus.Held);
			}
		}
		protected void HoldNotchanged(PXCache cache, object o)
		{
			if (cache.GetStatus(o) == PXEntryStatus.Notchanged)
				cache.SetStatus(o, PXEntryStatus.Held);
		}

		#endregion

		#region Data structures for improve performance

		protected GLBudgetEntryActionType _CurrentAction = GLBudgetEntryActionType.None;  // Action which is now being executed
		private bool IndexesIsPrepared = false;

		private GLBudgetLineIdx _ArticleIndex = null;
		private GLBudgetLineIdx ArticleIndex
		{
			get 
			{
				if (!IndexesIsPrepared && IsImport)
					PrepareIndexes();

				return _ArticleIndex;
			}
		}

		private GLBudgetLineDetailIdx _AllocationIndex = null;
		private GLBudgetLineDetailIdx AllocationIndex
		{
			get 
			{
				if (!IndexesIsPrepared && IsImport)
					PrepareIndexes();

				return _AllocationIndex;
			}
		}

		private PXResultset<GLBudgetLine> _ArticleGroups = null;
		private PXResultset<GLBudgetLine> ArticleGroups
		{
			get 
			{
				if (!IndexesIsPrepared && IsImport)
					PrepareIndexes();

				return _ArticleGroups;
			}
		}
		#endregion

		#region Ctor

		private readonly int _Periods;
		private readonly string _PrefixPeriodField;
		private int _PeriodsInCurrentYear;

		public PXSetup<GLSetup> GLSetup;

		public GLBudgetEntry()
		{
			GLSetup setup = GLSetup.Current;
			_PrefixPeriodField = "Period";

			MasterFinYear maxFinYear =
				PXSelectOrderBy<
					MasterFinYear,
					OrderBy<
						Desc<MasterFinYear.finPeriods>>>
					.SelectSingleBound(this, null);
			_Periods = maxFinYear?.FinPeriods ?? 0;

			OrganizationFinYear currentYear = PXSelect<
				OrganizationFinYear, 
				Where<OrganizationFinYear.year, Equal<Required<OrganizationFinYear.year>>,
					And<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>>>>
				.Select(this, Filter.Current.FinYear, PXAccess.GetParentOrganizationID(Filter.Current.BranchID));
			if (currentYear != null)
			{
				_PeriodsInCurrentYear = (int)currentYear.FinPeriods;
			}
			else
			{
				_PeriodsInCurrentYear = _Periods;
			}

			for (int i = 1; i <= _Periods; i++)
			{
				int j = i;
				string fieldName = _PrefixPeriodField + i;
				BudgetArticles.Cache.Fields.Add(fieldName);
				FieldSelecting.AddHandler(typeof(GLBudgetLine), fieldName, (sender, e) => AllocationFieldSelecting(sender, e, j));
				FieldUpdating.AddHandler(typeof(GLBudgetLine), fieldName, (sender, e) => AllocationFieldUpdating(sender, e, j));
			}

			if (IsImport)
			{
				Filter.Current.ShowTree = false;
				_BudgetArticlesIndex = null;
			}
		}

		#endregion

		private SelectedGroup CurrentSelected
		{
			get
			{
				PXCache cache = this.Caches[typeof(SelectedGroup)];
				if (cache.Current == null)
				{
					cache.Insert();
					cache.IsDirty = false;
				}
				return (SelectedGroup)cache.Current;
			}
		}

		protected virtual IEnumerable tree(
			[PXGuid]
			Guid? GroupID
		)
		{
			if (GroupID == null)
			{
				yield return new GLBudgetLine()
				{
					GroupID = Guid.Empty,
					Description = PXSiteMap.RootNode.Title
				};

			}

			foreach (GLBudgetLine article in PXSelect<GLBudgetLine,
			 Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>,
				And<GLBudgetLine.isGroup, Equal<True>,
				And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>>>.Select(this, GroupID))
			{
				if (SearchForMatchingGroupMask(article.GroupID))
				{
					if (Filter.Current.TreeNodeFilter != null)
					{
						if (article.Description.Contains(Filter.Current.TreeNodeFilter))
						{
							yield return article;
						}
						else
						{
							if (SearchForMatchingChild(article.GroupID))
							{
								yield return article;
							}
							else if (SearchForMatchingParent(article.ParentGroupID))
							{
								yield return article;
							}
						}
					}
					else
					{
						yield return article;
					}
				}
			}
		}

		private GLBudgetLineIdx _BudgetArticlesIndex = null;

		protected virtual IEnumerable budgetarticles(
			[PXGuid]
			Guid? groupID
		)
		{
			bool useIdx = IsImport && Filter.Current.ShowTree == false && (groupID == null || groupID == Guid.Empty);
			if (useIdx && _BudgetArticlesIndex != null)
			{
				return _BudgetArticlesIndex.GetAll();
			}

			if (groupID == null)
			{
				if (CurrentSelected.Group == null)
				{
					groupID = Tree.Current?.GroupID ?? Guid.Empty;
					CurrentSelected.Group = Filter.Current.ShowTree == true ? groupID : Guid.Empty;
				}
				else
				{
					groupID = CurrentSelected.Group;
				}
			}
			else
			{
				CurrentSelected.Group = Filter.Current.ShowTree == true ? groupID : Guid.Empty;
			}

			this.CurrentSelected.Group = (this.Filter.Current.ShowTree ?? true) ? groupID : Guid.Empty;

			GLBudgetLine parentNode = PXSelect<GLBudgetLine, Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>>>.Select(this, this.CurrentSelected.Group);
			if (parentNode != null)
			{
				this.CurrentSelected.AccountID = parentNode.AccountID != null ? parentNode.AccountID : Int32.MinValue;
				this.CurrentSelected.SubID = parentNode.SubID != null ? parentNode.SubID : Int32.MinValue;
			}
			else
			{
				this.CurrentSelected.AccountID = Int32.MinValue;
				this.CurrentSelected.SubID = Int32.MinValue;
			}
			this.CurrentSelected.AccountMaskWildcard = SubCDUtils.CreateSubCDWildcard(parentNode != null ? parentNode.AccountMask : string.Empty, AccountAttribute.DimensionName);
			this.CurrentSelected.AccountMask = parentNode != null ? parentNode.AccountMask : string.Empty;
			this.CurrentSelected.SubMaskWildcard = SubCDUtils.CreateSubCDWildcard(parentNode != null ? parentNode.SubMask : string.Empty, SubAccountAttribute.DimensionName);
			this.CurrentSelected.SubMask = parentNode != null ? parentNode.SubMask : string.Empty;

			BudgetArticles.Cache.AllowInsert = Filter.Current != null && Filter.Current.BranchID != null && Filter.Current.LedgerID != null && Filter.Current.FinYear != null
				 && (Filter.Current.CompareToBranchID == null || Filter.Current.CompareToFinYear == null || Filter.Current.CompareToLedgerId == null);
			BudgetArticles.Cache.AllowDelete = Filter.Current != null && Filter.Current.BranchID != null && Filter.Current.LedgerID != null && Filter.Current.FinYear != null
				 && (Filter.Current.CompareToBranchID == null || Filter.Current.CompareToFinYear == null || Filter.Current.CompareToLedgerId == null);

			List<GLBudgetLine> Articles = new List<GLBudgetLine>();
			int SortOrder = 0;
			//Adding logical groups to the grid
			if (Filter.Current.ShowTree==true)
			{
				foreach (GLBudgetLine logicalGroup in PXSelect<GLBudgetLine,
						Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
							And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
							And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
							And<GLBudgetLine.accountID, IsNull,
							And<Match<Current<AccessInfo.userName>>>>>>>,
							OrderBy<Asc<GLBudgetLine.treeSortOrder>>>.Select(this))
				{
					if (logicalGroup.ParentGroupID == groupID)
					{
						logicalGroup.SortOrder = SortOrder++;
						Articles.Add(logicalGroup);
						if (Filter.Current.CompareToBranchID != null && Filter.Current.CompareToLedgerId != null && !string.IsNullOrEmpty(Filter.Current.CompareToFinYear))
						{
							Articles.Add(ComparisonRow(logicalGroup, SortOrder++));
						}
					}
				}
			}

			foreach (PXResult<GLBudgetLine, Account, Sub> result in PXSelectJoin<GLBudgetLine,
					InnerJoin<Account, On<Account.accountID, Equal<GLBudgetLine.accountID>>,
					LeftJoin<Sub, On<Sub.subID, Equal<GLBudgetLine.subID>>>>,
					Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
						And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
						And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
						And<Account.active, Equal<True>,
						And2<Where<Sub.active, Equal<True>,
								And<Sub.subCD, Like<Current<BudgetFilter.subCDWildcard>>, Or<Sub.subID, IsNull>>>,
						And<Match<Current<AccessInfo.userName>>>>>>>>,
							OrderBy<Asc<Account.accountCD,
							Asc<GLBudgetLine.subID>>>>.Select(this))
			{
				GLBudgetLine article = result;

				if (article.Comparison != null && article.Comparison == true) break;

				if (Filter.Current.ShowTree == true)
				{
					if ((article.GroupID == null || article.ParentGroupID == groupID) || (article.ParentGroupID == groupID && groupID == Guid.Empty)) //|| article.GroupID == groupID 
					{
						article.SortOrder = SortOrder++;
						Articles.Add(article);
						if (Filter.Current.CompareToBranchID != null && Filter.Current.CompareToLedgerId != null && !string.IsNullOrEmpty(Filter.Current.CompareToFinYear))
						{
							Articles.Add(ComparisonRow(article, SortOrder++));
						}
					}
				}
				else
				{
					if (article.Rollup == null || !(bool)article.Rollup)
					{
						article.SortOrder = SortOrder++;
						Articles.Add(article);
						if (Filter.Current.CompareToBranchID != null && Filter.Current.CompareToLedgerId != null && !string.IsNullOrEmpty(Filter.Current.CompareToFinYear))
						{
							Articles.Add(ComparisonRow(article, SortOrder++));
						}
					}
				}
			}

			if (useIdx)
			{
				_BudgetArticlesIndex = new GLBudgetLineIdx(Articles);
			}

			return Articles;
		}

		#region Functions

		protected virtual decimal ParseAmountValue(object obj)
		{
			if (string.IsNullOrWhiteSpace(obj?.ToString()))
			{
				return 0;
			}

			int precision = Filter.Current.Precision ?? 2;

			return (decimal)Math.Round(Convert.ToDouble(obj), precision, MidpointRounding.AwayFromZero);
		}

		private bool SearchForMatchingGroupMask(Guid? GroupID)
		{
			PXResultset<GLBudgetLine> childGroups = PXSelect<GLBudgetLine, Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>, And<GLBudgetLine.isGroup, Equal<True>>>>.Select(this, GroupID);
			if (PXSelect<GLBudgetLine, Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>, And<Match<Current<AccessInfo.userName>>>>>.Select(this, GroupID).Count == 0)
			{
				if (PXSelect<GLBudgetLine, Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>, And<Match<Current<AccessInfo.userName>>>>>.Select(this, GroupID).Count == 0)
				{
					foreach (GLBudgetLine childGroup in childGroups)
					{
						if (SearchForMatchingGroupMask(childGroup.GroupID)) return true;
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
			return false;
		}

		private bool SearchForMatchingChild(Guid? GroupID)
		{
			PXResultset<GLBudgetLine> childGroups = PXSelect<GLBudgetLine, Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>, And<GLBudgetLine.isGroup, Equal<True>>>>.Select(this, GroupID);
			foreach (GLBudgetLine childGroup in childGroups)
			{
				if (!childGroup.Description.Contains(Filter.Current.TreeNodeFilter))
				{
					if (SearchForMatchingChild(childGroup.GroupID)) return true;
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		private bool SearchForMatchingParent(Guid? ParentGroupID)
		{
			GLBudgetLine parentGroup = PXSelect<GLBudgetLine, Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>>>.Select(this, ParentGroupID);
			if (parentGroup != null)
			{
				if (!parentGroup.Description.Contains(Filter.Current.TreeNodeFilter))
				{
					if (SearchForMatchingParent(parentGroup.ParentGroupID)) return true;
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		protected virtual bool MatchMask(string accountCD, string mask)
		{
			if (mask.Length == 0 && accountCD.Length > 0)
			{
				for (int i = 0; i == accountCD.Length; i++)
				{
					mask += "?";
				}
			}
			if (mask.Length > 0 && accountCD.Length > 0 && mask.Length > accountCD.Length)
			{
				mask = mask.Substring(0, accountCD.Length);
			}
			for (int i = 0; i < mask.Length; i++)
			{
				if (i >= accountCD.Length || mask[i] != '?' && mask[i] != accountCD[i])
				{
					return false;
				}
			}
			return true;
		}

		private GLBudgetLine ComparisonRow(GLBudgetLine article, int sortOrder)
		{
			GLBudgetLine compare = new GLBudgetLine
			{
				IsGroup = article.IsGroup,
				Released = false,
				WasReleased = false,
				BranchID = Filter.Current.CompareToBranchID,
				LedgerID = Filter.Current.CompareToLedgerId,
				FinYear = Filter.Current.CompareToFinYear,
				AccountID = article.AccountID,
				SubID = article.SubID,

				GroupID = article.GroupID,
				ParentGroupID = article.ParentGroupID,

				Description = Messages.BudgetArticleDescrCompared,
				Rollup = article.Rollup,
				Comparison = true,
				Compared = article.Compared,
				Amount = 0m,
				SortOrder = sortOrder
			};
			PXUIFieldAttribute.SetEnabled(BudgetArticles.Cache, compare, false);
			if (article.Compared != null)
			{
				foreach (decimal t in article.Compared)
				{
					compare.Amount += t;
				}
			}
			//All comparison values are taken from GLHistory
			compare.AllocatedAmount = compare.Amount;
			compare.ReleasedAmount = compare.Amount;
			HoldNotchanged(BudgetArticles.Cache, compare);
			return compare;
		}

		protected virtual void UpdateAlloc(decimal value, GLBudgetLine article, int fieldNbr)
		{
			GLBudgetLineDetail alloc = GetAlloc(article, fieldNbr);
			decimal? delta;
			if (alloc != null)
			{
				delta = value - alloc.Amount;
				if (delta != 0m)
				{
					alloc.Amount = value;
					Allocations.Update(alloc);
					article.Allocated = null;
				}
			}
			else
			{
				delta = value;
				if (delta != 0m)
				{
					OrganizationFinPeriod period = PXSelect<
						OrganizationFinPeriod,
						Where<OrganizationFinPeriod.finYear, Equal<Required<OrganizationFinPeriod.finYear>>,
							And<OrganizationFinPeriod.periodNbr, Equal<Required<OrganizationFinPeriod.periodNbr>>,
							And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>>
						.Select(this, article.FinYear, fieldNbr.ToString("00"), PXAccess.GetParentOrganizationID(article.BranchID));
					if (period != null)
					{
						alloc = new GLBudgetLineDetail
						{
							GroupID = article.GroupID,
							BranchID = article.BranchID,
							LedgerID = article.LedgerID,
							FinYear = article.FinYear,
							AccountID = article.AccountID,
							SubID = article.SubID,
							FinPeriodID = period.FinPeriodID,
							Amount = value
						};
						Allocations.Insert(alloc);
						article.Allocated = null;
					}
				}
			}
			RollupAllocation(article, fieldNbr, delta);
		}

		protected virtual GLBudgetLineDetail GetAlloc(GLBudgetLine article, int fieldNbr)
		{
			string period = article.FinYear + fieldNbr.ToString("00");
			if (article.GroupID == null) return null;
			if (AllocationIndex != null)
			{
				return AllocationIndex.Get(article.GroupID.Value, period);
			}
			else
			{  // TODO: Bag perfomance - try to use _AllocationIndex
				foreach (GLBudgetLineDetail alloc in Allocations.Select(article.FinYear, article.GroupID))
				{
					if (alloc.FinPeriodID == period)
					{
						return alloc;
					}
				}
			}
			return null;
		}

		protected virtual void EnsureAlloc(GLBudgetLine article)
		{
			if (_CurrentAction == GLBudgetEntryActionType.PreloadBudgetTree) return;

			if (article.Allocated != null) return;
			article.Allocated = new decimal[_Periods];
			int idx;
			if (AllocationIndex != null)
			{
				if (article.GroupID != null)
				{
					foreach (GLBudgetLineDetail alloc in AllocationIndex.GetList(article.GroupID.Value))
					{
						idx = int.Parse(alloc.FinPeriodID.Substring(4)) - 1;
						article.Allocated[idx] = alloc.Amount ?? 0m;
					}
				}
				return;
			}
			foreach (GLBudgetLineDetail alloc in Allocations.Select(article.FinYear, article.GroupID))
			{
				idx = int.Parse(alloc.FinPeriodID.Substring(4)) - 1;
				article.Allocated[idx] = alloc.Amount ?? 0m;
			}
		}

		protected bool suppressIDs;
		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			IEnumerable ret = base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			suppressIDs = viewName == nameof(BudgetArticles);
			if (IsImport && viewName == nameof(Tree) && Filter.Current.ShowTree == true && ret is IList && ((IList)ret).Count == 1)
			{
				CurrentSelected.Group = Tree.Current.GroupID;
			}
			return ret;
		}

		protected virtual GLBudgetLine GetPrevArticle(GLBudgetLine article)
		{
			return article.FinYear != null ? PXSelect<GLBudgetLine,
				Where<GLBudgetLine.branchID, Equal<Current<GLBudgetLine.branchID>>,
					And<GLBudgetLine.ledgerID, Equal<Current<GLBudgetLine.ledgerID>>,
					And<GLBudgetLine.accountID, Equal<Current<GLBudgetLine.accountID>>,
					And<GLBudgetLine.subID, Equal<Current<GLBudgetLine.subID>>,
					And<GLBudgetLine.finYear, Equal<Required<GLBudgetLine.finYear>>>>>>>>.SelectSingleBound(this,
					new object[] { article }, (int.Parse(article.FinYear) - 1).ToString(CultureInfo.InvariantCulture)) : null;
		}

		protected virtual void PopulateComparison(int? branchID, int? ledgerID, string finYear)
		{
			foreach (GLBudgetLine article in BudgetArticles.Cache.Cached)
			{
				article.Compared = new decimal[_Periods];
				HoldNotchanged(BudgetArticles.Cache, article);
			}
			if (branchID == null || ledgerID == null || String.IsNullOrEmpty(finYear)) return;

			Ledger ledger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<BudgetFilter.compareToLedgerID>>>>.Select(this, ledgerID);

			bool BudgetArticlesCacheStatus = BudgetArticles.Cache.IsDirty;

			Dictionary<Guid, GLBudgetLine> parentNodes =
				PXSelect<GLBudgetLine,
				Where<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
					And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
					And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
					And<GLBudgetLine.isGroup, Equal<True>,
					And2<Exists<Select<GL.Standalone.GLBudgetLine2, Where<GL.Standalone.GLBudgetLine2.parentGroupID, Equal<GLBudgetLine.groupID>>>>,
					And<Match<Current<AccessInfo.userName>>>>>>>>>
					.Select(this)
					.RowCast<GLBudgetLine>()
					.ToDictionary(_ => _.GroupID.Value, _ => _);

			PXSelectBase<GLBudgetLine> cmdBudgetLine =
				new PXSelectReadonly2<GLBudgetLine,
				InnerJoin<MasterFinPeriod, On<True, Equal<True>>,
				InnerJoin<Account, On<Account.accountID, Equal<GLBudgetLine.accountID>>,
				LeftJoin<GLHistory, On<GLHistory.accountID, Equal<GLBudgetLine.accountID>,
					And<GLHistory.subID, Equal<GLBudgetLine.subID>,
					And<GLHistory.finPeriodID, Equal<MasterFinPeriod.finPeriodID>,
					And<GLHistory.ledgerID, Equal<Required<BudgetFilter.compareToLedgerID>>,
					And<GLHistory.branchID, Equal<Required<BudgetFilter.compareToBranchID>>>>>>>>>>,
				Where<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
					And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
					And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
					And<MasterFinPeriod.finYear, Equal<Required<BudgetFilter.compareToFinYear>>,
					And<Match<Current<AccessInfo.userName>>>>>>>>
				(this);

			PXSelectBase<GLHistory> cmdHistory =
				new PXSelectReadonly2<GLHistory,
				LeftJoin<Account,
					On<GLHistory.accountID, Equal<Account.accountID>>,
				LeftJoin<Sub,
					On<GLHistory.subID, Equal<Sub.subID>>,
				InnerJoin<MasterFinPeriod,
					On<True, Equal<True>,
					And<GLHistory.finPeriodID, Equal<MasterFinPeriod.finPeriodID>,
					And<GLHistory.ledgerID, Equal<Required<BudgetFilter.compareToLedgerID>>,
					And<GLHistory.branchID, Equal<Required<BudgetFilter.compareToBranchID>>>>>>>>>,
				Where<MasterFinPeriod.finYear, Equal<Required<BudgetFilter.compareToFinYear>>,
					And<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>,
					And<Account.accountCD, Like<Required<Account.accountCD>>,
					And<Sub.subCD, Like<Required<Sub.subCD>>>>>>>(this);

			PXCache<GLBudgetLine> cacheGLBudgetLine = this.Caches<GLBudgetLine>();

			List<Type> cmdBudgetLineFields = new List<Type>();
			cmdBudgetLineFields.Add(typeof(GLBudgetLine));
			cmdBudgetLineFields.Add(typeof(GLHistory.curyFinPtdDebit));
			cmdBudgetLineFields.Add(typeof(GLHistory.curyFinPtdCredit));
			cmdBudgetLineFields.Add(typeof(GLHistory.curyFinPtdCredit));
			cmdBudgetLineFields.Add(typeof(GLHistory.curyFinPtdDebit));
			cmdBudgetLineFields.Add(typeof(Account.type));
			cmdBudgetLineFields.Add(typeof(MasterFinPeriod.finPeriodID));

			using (new PXFieldScope(cmdBudgetLine.View, cmdBudgetLineFields))
			foreach (PXResult<GLBudgetLine, MasterFinPeriod, Account, GLHistory> result in cmdBudgetLine.Select(ledgerID, branchID, finYear))
			{
				GLBudgetLine articleOrig = result;
				GLBudgetLine article = (GLBudgetLine)cacheGLBudgetLine.Locate(articleOrig); // Cache must have the row
				GLHistory hist = result;
				Account acct = result;
				MasterFinPeriod period = result;
				int iFinPeriodID = int.Parse(period.FinPeriodID.Substring(4)) - 1;

				if (iFinPeriodID + 1 > _Periods)
				{
					break;
				}

				decimal histExpense = (hist.CuryFinPtdDebit ?? 0m) - (hist.CuryFinPtdCredit ?? 0m);
				decimal histIncome = (hist.CuryFinPtdCredit ?? 0m) - (hist.CuryFinPtdDebit ?? 0m);

				//Collecting amounts for aggregating articles
				if (article.AccountID != null && article.SubID != null && (article.AccountMask.Contains('?') || article.SubMask.Contains('?')) && ledger.BalanceType != LedgerBalanceType.Budget)
				{
					histExpense = 0;
					histIncome = 0;

					using (new PXFieldScope(cmdHistory.View, 
							typeof(GLHistory.curyFinPtdDebit), 
							typeof(GLHistory.curyFinPtdCredit), 
							typeof(GLHistory.curyFinPtdCredit),
							typeof(GLHistory.curyFinPtdDebit)))
					foreach (PXResult<GLHistory, Account, Sub, MasterFinPeriod> aggregatingResult in cmdHistory
								.Select(ledgerID, branchID, finYear, period.FinPeriodID,
								SubCDUtils.CreateSubCDWildcard(article.AccountMask, AccountAttribute.DimensionName),
								SubCDUtils.CreateSubCDWildcard(article.SubMask, SubAccountAttribute.DimensionName)))
					{
						GLHistory histAggr = aggregatingResult;
						histExpense += (histAggr.CuryFinPtdDebit ?? 0m) - (histAggr.CuryFinPtdCredit ?? 0m);
						histIncome += (histAggr.CuryFinPtdCredit ?? 0m) - (histAggr.CuryFinPtdDebit ?? 0m);
					}
				}

				if (article.Compared == null)
				{
					article.Compared = new decimal[_Periods];
				}

				if (article.GroupID != null && parentNodes.ContainsKey(article.GroupID.Value))
				{
					SaveRolledUpBudgetLines(parentNodes);
				}

				if (acct.Type == AccountType.Asset || acct.Type == AccountType.Expense)
				{
					article.Compared[iFinPeriodID] = histExpense;
					RollupComparison(article, iFinPeriodID, histExpense, parentNodes);
				}
				else
				{
					article.Compared[iFinPeriodID] = histIncome;
					RollupComparison(article, iFinPeriodID, histIncome, parentNodes);
				}
			}

			SaveRolledUpBudgetLines(parentNodes);

			if (!BudgetArticlesCacheStatus)
			{
				BudgetArticles.Cache.IsDirty = false;
			}
		}

		private void RollupComparison(GLBudgetLine article, int period, decimal? delta, Dictionary<Guid, GLBudgetLine> parentNodes)
		{
			if (IsNullOrEmpty(article.ParentGroupID)) return;

			GLBudgetLine parentNode = parentNodes[article.ParentGroupID.Value];

			if (parentNode != null)
			{
				if (parentNode.Compared == null)
				{
					parentNode.Compared = new decimal[_Periods];
				}
				parentNode.Compared[period] += (decimal)delta;
				parentNode.IsRolledUp = true;
				RollupComparison(parentNode, period, delta, parentNodes);
			}
		}

		private void SaveRolledUpBudgetLines(Dictionary<Guid, GLBudgetLine> parentNodes)
		{
			foreach(var line in parentNodes.Values.Where(_ => _.IsRolledUp == true))
			{
				BudgetArticles.Update(line);
				line.IsRolledUp = false;
			}
		}

		private void RollupArticleAmount(GLBudgetLine article, decimal? delta)
		{
			if (IsNullOrEmpty(article.ParentGroupID) || delta == 0m) return;
			GLBudgetLine parentNode = GetArticle(article.BranchID, article.LedgerID, article.FinYear, article.ParentGroupID);
			if (parentNode != null)
			{
				parentNode.Amount = parentNode.Amount ?? 0;
				parentNode.Amount += delta;
				BudgetArticles.Update(parentNode);
			}
		}

		protected virtual void RollupAllocation(GLBudgetLine article, int fieldNbr, decimal? delta)
		{
			if (IsNullOrEmpty(article.ParentGroupID) || delta == 0m) return;

			GLBudgetLine rollupArticle = GetArticleByCurrentFilter(article.ParentGroupID);
			if (rollupArticle != null)
			{
				GLBudgetLineDetail rollupAlloc = GetAlloc(rollupArticle, fieldNbr);
				if (rollupAlloc != null && rollupAlloc.LedgerID != null)
				{
					rollupAlloc.Amount += delta;
					rollupAlloc = Allocations.Update(rollupAlloc);
				}
				else
				{
					rollupAlloc = new GLBudgetLineDetail();
					rollupAlloc.GroupID = rollupArticle.GroupID;
					rollupAlloc.BranchID = rollupArticle.BranchID;
					rollupAlloc.LedgerID = rollupArticle.LedgerID;
					rollupAlloc.FinYear = rollupArticle.FinYear;
					rollupAlloc.AccountID = rollupArticle.AccountID;
					rollupAlloc.SubID = rollupArticle.SubID;
					rollupAlloc.FinPeriodID = rollupArticle.FinYear + fieldNbr.ToString("00");
					rollupAlloc.Amount = delta;
					if (AllocationIndex != null)
						AllocationIndex.Add(rollupAlloc);  // It's need before GLBudgetLine_RowUpdated
					rollupAlloc = Allocations.Insert(rollupAlloc); // GLBudgetLine_RowUpdated raise before GLBudgetLineDetail_RowInserted because PXParent on GroupID. Is it ok?
				}
				rollupArticle.Allocated = null;
				rollupArticle.Released = false;
				rollupArticle.WasReleased = false;
				RollupAllocation(rollupArticle, fieldNbr, delta);
			}
		}

		private GLBudgetLine PutInGroup(string AccountID, string SubID, GLBudgetLine parentArticle)
		{
			GLBudgetLine foundLine = new GLBudgetLine();
			foreach (GLBudgetLine child in PXSelect<GLBudgetLine,
				Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>,
				And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
				And<GLBudgetLine.isGroup, Equal<True>>>>>>>.Select(this, parentArticle.GroupID))
			{
				if (MatchMask(SubID, child.SubMask ?? String.Empty) && MatchMask(AccountID, child.AccountMask ?? String.Empty))
				{
					GLBudgetLine tmpLine = PutInGroup(AccountID, SubID, child);
					if (tmpLine.GroupID != null || tmpLine.ParentGroupID != null)
					{
						foundLine = tmpLine;
					}
					else
					{
						foundLine.ParentGroupID = (Guid)child.GroupID;
						foundLine.GroupMask = child.GroupMask;
					}
				}
			}
			return foundLine;
		}

		private GLBudgetLine PutIntoInnerGroup(GLBudgetLine newArticle, GLBudgetLine parentArticle)
		{
			GLBudgetLine returnArticle = parentArticle;
			if (parentArticle.IsGroup == true)
				foreach (GLBudgetLine child in PXSelect<GLBudgetLine,
					Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>,
					And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
					And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
					And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>>.Select(this, parentArticle.GroupID))
				{
					GetArticleByCurrentFilter(parentArticle.GroupID);
					if (MatchMask(((Account)PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>
					.Select(this, newArticle.AccountID)).AccountCD, child.AccountMask ?? String.Empty)
					&&
					MatchMask(((Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>
					.Select(this, newArticle.SubID)).SubCD, child.SubMask ?? String.Empty))
					{
						returnArticle = child;
						returnArticle = PutIntoInnerGroup(newArticle, child);
					}
				}
			return returnArticle;
		}

		private GLBudgetLine LocateInBudgetArticlesCache(PXResultset<GLBudgetLine> existingArticles, GLBudgetLine line, bool IncludeGroups)
		{              // TODO: Bad Performance
			PXEntryStatus status;
			foreach (GLBudgetLine article in existingArticles)
			{
				status = BudgetArticles.Cache.GetStatus(article);
				if (status != PXEntryStatus.Deleted &&	status != PXEntryStatus.InsertedDeleted && article.GroupID != line.GroupID)
				{
					if (article.BranchID.Equals(line.BranchID) &&
						article.LedgerID.Equals(line.LedgerID) &&
						article.FinYear.Equals(line.FinYear) &&
						article.AccountID.Equals(line.AccountID) &&
						article.SubID.Equals(line.SubID))
					{
						if (IncludeGroups)
						{
							return article;
						}
						else
						{
							if (!(bool)article.IsGroup)
							{
								return article;
							}
						}
					}
				}
			}
			return null;
		}

		protected GLBudgetLine GetArticle(int? branchID, int? ledgerID, string finyear, Guid? groupID)
		{
			if (IsNullOrEmpty(groupID)) return null;
			GLBudgetLine article = (GLBudgetLine)BudgetArticles.Cache.Locate(new GLBudgetLine
			{
				BranchID = branchID,
				LedgerID = ledgerID,
				FinYear = finyear,
				GroupID = groupID
			});

			return article ??
			 PXSelect<GLBudgetLine,
			 Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>,
			 And<GLBudgetLine.branchID, Equal<Required<BudgetFilter.branchID>>,
			 And<GLBudgetLine.ledgerID, Equal<Required<BudgetFilter.ledgerID>>,
			 And<GLBudgetLine.finYear, Equal<Required<BudgetFilter.finYear>>>>>>>
			 .Select(this, groupID, branchID, ledgerID, finyear);
		}
		protected GLBudgetLine GetArticleByCurrentFilter(Guid? groupID)
		{
			return GetArticle(Filter.Current.BranchID, Filter.Current.LedgerID, Filter.Current.FinYear, groupID);
		}
		
		// Fill tree structure. Root element is Guid.Empty
		private bool setArticleToParentGroup(Dictionary<Guid, HashSet<GLBudgetLine>> articlesByParentGroup, GLBudgetLine article)
		{
			HashSet<GLBudgetLine> articles = null;
			if (!articlesByParentGroup.TryGetValue(article.ParentGroupID.Value, out articles))
			{
				articles = new HashSet<GLBudgetLine>();
				articlesByParentGroup.Add(article.ParentGroupID.Value, articles);
			}
			if (!articles.Contains<GLBudgetLine>(article))
			{
				articles.Add(article);
				return true;
			}
			return false;
		}

		private Guid PutIntoNewInnerGroup(Guid groupID, GLBudgetLine article, IEnumerable<GLBudgetLine> articleGroups)
		{
			Guid parentGroupID = groupID;
			foreach (GLBudgetLine child in (groupID == Guid.Empty ? articleGroups : articleGroups.Where(x => x.ParentGroupID == groupID)))
			{
				if (article.GroupID != child.GroupID && (bool)child.IsGroup)   // если это группа и ее GroupID не совпадает с GroupID статьи, то 
				{
					// try to find the group using AccountMask and SubMask
					if (child.AccountMask != null && child.SubMask != null && MatchMask(article.AccountMask, child.AccountMask) && MatchMask(article.SubMask, child.SubMask))
					{
						parentGroupID = (Guid)child.GroupID;
						parentGroupID = PutIntoNewInnerGroup(parentGroupID, article, articleGroups);
					}
				}
			}
			return parentGroupID;
		}

		private PXResultset<GLBudgetLine> collectChildNodes(Guid? GroupID)
		{
			PXResultset<GLBudgetLine> childNodes = new PXResultset<GLBudgetLine>();
			PXResultset<GLBudgetLine> childGroups = PXSelect<GLBudgetLine,
				Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>,
				And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
					And<Match<Current<AccessInfo.userName>>>>>>>>.Select(this, GroupID);
			GLBudgetLine glBudgetLine;
			foreach (PXResult<GLBudgetLine> childGroup in childGroups)
			{
				glBudgetLine = childGroup;
				childNodes.Add(childGroup);
				if (glBudgetLine.IsGroup == true)
				{
					childNodes.AddRange(collectChildNodes(glBudgetLine.GroupID));
				}
			}
			return childNodes;
		}

		private void CheckBudgetLinesForDuplicates()
		{
			var cache = BudgetArticles.Cache;

			var listCached = new HashSet<GLBudgetLineKey>();
			foreach (GLBudgetLine item in cache.Cached)
			{
				if (item.IsGroup == false && 
					cache.GetStatus(item) != PXEntryStatus.Deleted &&
					item.Comparison != true)
				{
					GLBudgetLineKey line = new GLBudgetLineKey();
					line.BranchID = (int)item.BranchID;
					line.LedgerID = (int)item.LedgerID;
					line.FinYear = item.FinYear;
					line.AccountID = item.AccountID ?? 0;
					line.SubID = item.SubID ?? 0;

					if (listCached.Contains(line))
					{
						BudgetArticles.Cache.RaiseExceptionHandling<GLBudgetLine.accountID>(item, item.AccountID,
							new PXSetPropertyException(Messages.DuplicateAccountSubEntry, PXErrorLevel.RowError));

						throw new PXException(Messages.DuplicateAccountSubEntry);
					}
					else
					{
						listCached.Add(line);
					}
				}
			}
		}

		private void CheckBudgetLinesForComparisonData()
		{
			PXCache cache = BudgetArticles.Cache;

			foreach (GLBudgetLine item in BudgetArticles.Select())
			{
				if (item.Comparison != null && (bool)item.Comparison)
				{
					BudgetArticles.Cache.RaiseExceptionHandling<GLBudgetLine.accountID>(item, item.AccountID,
						new PXSetPropertyException(Messages.AlreadyContainsDataForComparison, PXErrorLevel.RowError));

					throw new PXException(Messages.AlreadyContainsDataForComparison);
				}
			}
		}

		#endregion

		#region Implementation of IPXProcess
		public void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			_CurrentAction = GLBudgetEntryActionType.None;
			return;
		}
		#endregion

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (!IndexesIsPrepared)
				PrepareIndexes();

			GLBudgetLineIdx.checkGLBudgetLine checkGLBudgetLine = delegate (GLBudgetLine line, GLBudgetLine sourceLine)
			{
				if (line == null) return null;
				if (IsNullOrEmpty(line.GroupID)) return null;
				PXEntryStatus status = BudgetArticles.Cache.GetStatus(line);
				if (status == PXEntryStatus.Deleted || status == PXEntryStatus.InsertedDeleted)
					return null;
				return line;
			};

			string val;
			int? accountID = null;
			int? subID = null;
			GLBudgetLine article = null;

			val = values[nameof(GLBudgetLine.AccountID)] as string;
			if (!String.IsNullOrEmpty(val))
				accountID = AccountDefinition.getID(val);

			val = values[nameof(GLBudgetLine.SubID)] as string;
			if (String.IsNullOrEmpty(val))
				val = SubDefinition.DefaultCD;

			subID = SubDefinition.getID(val);

			if ((accountID != null && (subID != null || SubDimensionDefinition.Validate == false) && SubEnabled) || (accountID != null && !SubEnabled))
			{
				article = ArticleIndex.GetByKey(new GLBudgetLineKey()
				{
					BranchID = Filter.Current.BranchID ?? 0,
					LedgerID = Filter.Current.LedgerID ?? 0,
					FinYear = Filter.Current.FinYear,
					AccountID = accountID ?? 0,
					SubID = subID ?? 0
				}, checkGLBudgetLine);

				if (article?.SubID != null)
				{
					if ((bool)article.IsGroup == true)
					{
						return false;
					}

					values.Add(nameof(GLBudgetLine.IsUploaded), true);
					values.Add(nameof(GLBudgetLine.GroupID), article.GroupID);
					values.Add(nameof(GLBudgetLine.ParentGroupID), article.ParentGroupID);
					values.Add(nameof(GLBudgetLine.GroupMask), article.GroupMask);

					keys[nameof(GLBudgetLine.GroupID)] = article.GroupID;
					keys[nameof(GLBudgetLine.ParentGroupID)] = article.ParentGroupID;
				}
				else
				{
					Guid groupID = Guid.NewGuid();
					values.Add(nameof(GLBudgetLine.GroupID), groupID);
					keys[nameof(GLBudgetLine.GroupID)] = groupID;

					Guid? parentID = CurrentSelected?.Group ?? Guid.Empty;
					values.Add(nameof(GLBudgetLine.ParentGroupID), parentID);
					keys[nameof(GLBudgetLine.ParentGroupID)] = parentID;
					values.Add(nameof(GLBudgetLine.IsUploaded), true);

					foreach (GLBudgetLine group in ArticleGroups)
					{
						if ((SubEnabled && group.AccountMask != null && group.AccountMask != string.Empty && group.SubMask != null && group.SubMask != String.Empty)
							|| (!SubEnabled && group.AccountMask != null && group.AccountMask != string.Empty && group.SubMask != null))
						{
							if ((SubEnabled && MatchMask(values[nameof(GLBudgetLine.AccountID)].ToString().Trim(), group.AccountMask) && MatchMask(values[nameof(GLBudgetLine.SubID)].ToString().Trim(), group.SubMask))
								|| (!SubEnabled && MatchMask(values[nameof(GLBudgetLine.AccountID)].ToString().Trim(), group.AccountMask)))
							{
								GLBudgetLine foundLine = PutInGroup(values[nameof(GLBudgetLine.AccountID)].ToString().Trim(), (values[nameof(GLBudgetLine.SubID)] ?? String.Empty).ToString().Trim(), group);
								values[nameof(GLBudgetLine.ParentGroupID)] = foundLine.ParentGroupID ?? group.GroupID;
								keys[nameof(GLBudgetLine.ParentGroupID)] = values[nameof(GLBudgetLine.ParentGroupID)];
								if (group.GroupMask != null)
								{
									if (!values.Contains(nameof(GLBudgetLine.GroupMask))) values.Add(nameof(GLBudgetLine.GroupMask), foundLine.GroupMask ?? group.GroupMask);
									else values[nameof(GLBudgetLine.GroupMask)] = foundLine.GroupMask ?? group.GroupMask;
								}
							}
						}
					}
				}

				for (int i = 1; i <= _Periods; i++)
				{
					string fieldName = _PrefixPeriodField + i;
					if (values.Contains(fieldName))
					{
						try
						{
							values[fieldName] = ParseAmountValue(values[fieldName]);
						}
						catch (Exception) { }
					}
				}

				if (values.Contains(nameof(GLBudgetLine.Amount)))
				{
					try
					{
						values[nameof(GLBudgetLine.Amount)] = ParseAmountValue(values[nameof(GLBudgetLine.Amount)]);
					}
					catch (Exception) { }
				}
			}
			else
			{
				return false;
			}
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		#endregion

		#region Actions
		public PXSave<BudgetFilter> Save;
		public PXCancel<BudgetFilter> Cancel;
		public PXDelete<BudgetFilter> Delete;
		public PXAction<BudgetFilter> First;
		public PXAction<BudgetFilter> Prev;
		public PXAction<BudgetFilter> Next;
		public PXAction<BudgetFilter> WNext;
		public PXAction<BudgetFilter> Last;
		public PXAction<BudgetFilter> ShowPreload;
		public PXAction<BudgetFilter> Preload;
		public PXAction<BudgetFilter> Distribute;
		public PXAction<BudgetFilter> DistributeOK;
		public PXAction<BudgetFilter> ShowManage;
		public PXAction<BudgetFilter> ManageOK;


		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXCancelButton]
		public virtual IEnumerable cancel(PXAdapter adapter)
		{
			BudgetFilter oldfilter = Filter.Current;
			BudgetPreloadFilter oldPreloadFilter = PreloadFilter.Current;
			oldfilter.CompareToFinYear = null;
			oldfilter.SubIDFilter = null;
			Clear();
			Filter.Cache.RestoreCopy(Filter.Current, oldfilter);
			PreloadFilter.Cache.RestoreCopy(PreloadFilter.Current, oldPreloadFilter);
			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXDeleteButton]
		public virtual IEnumerable delete(PXAdapter adapter)
		{
			bool deleteAllowed = true;
			PXResultset<GLBudgetLine> budget = PXSelect<GLBudgetLine,
							Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
							And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
							And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this);
			foreach (GLBudgetLine budgetLine in budget)
			{
				if (budgetLine.ReleasedAmount > 0)
				{
					deleteAllowed = false;
					break;
				}
			}
			if (deleteAllowed)
			{
				foreach (GLBudgetLine budgetLine in budget)
				{
					BudgetArticles.Delete(budgetLine);
				}
				Filter.Current.FinYear = null;
				this.Save.Press();
			}
			else
			{
				BudgetArticles.Ask(Messages.BudgetDeleteTitle, Messages.BudgetDeleteMessage, MessageButtons.OK);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton(ConfirmationMessage = null)]
		public virtual IEnumerable prev(PXAdapter adapter)
		{
			if (!BudgetArticles.Cache.IsDirty)
			{
				GLBudgetLine article = PXSelectGroupBy<GLBudgetLine,
					Where<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>, And<GLBudgetLine.finYear, Less<Current<BudgetFilter.finYear>>>>,
					Aggregate<Max<GLBudgetLine.finYear>>>
					.Select(this);
				if (article == null || article.FinYear == null)
				{
					return Last.Press(adapter);
				}
				Filter.Current.FinYear = article.FinYear;
				Filter.Update(Filter.Current);
			}
			else
			{
				BudgetArticles.Ask(Messages.BudgetPendingChangesTitle, Messages.BudgetPendingChangesMessage, MessageButtons.OK);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton(ConfirmationMessage = null)]
		public virtual IEnumerable next(PXAdapter adapter)
		{
			if (!BudgetArticles.Cache.IsDirty)
			{
				GLBudgetLine article = PXSelectGroupBy<GLBudgetLine,
					Where<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
					And<GLBudgetLine.finYear, Greater<Required<BudgetFilter.finYear>>>>,
					Aggregate<Min<GLBudgetLine.finYear>>>
					.Select(this, Filter.Current.FinYear ?? "");
				if (article == null || article.FinYear == null)
				{
					return First.Press(adapter);
				}
				Filter.Current.FinYear = article.FinYear;
				Filter.Update(Filter.Current);
			}
			else
			{
				BudgetArticles.Ask(Messages.BudgetPendingChangesTitle, Messages.BudgetPendingChangesMessage, MessageButtons.OK);
			}
			return adapter.Get();
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable wnext(PXAdapter adapter)
		{
			bool errorHappened = false;
			if (PreloadFilter.Current.LedgerID == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.ledgerID>(PreloadFilter.Current, PreloadFilter.Current.LedgerID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				errorHappened = true;
			}
			if (PreloadFilter.Current.FinYear == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.finYear>(PreloadFilter.Current, PreloadFilter.Current.FinYear,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				errorHappened = true;
			}

			if (PreloadFilter.Current.ChangePercent == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.changePercent>(PreloadFilter.Current, PreloadFilter.Current.ChangePercent,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				errorHappened = true;
			}

			if (!MatchMask(PreloadFilter.Current.AccountIDFilter ?? String.Empty, CurrentSelected.AccountMask ?? String.Empty))
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.accountIDFilter>(PreloadFilter.Current, PreloadFilter.Current.AccountIDFilter,
					new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetAccountNotAllowed), CurrentSelected.AccountMask)));
				errorHappened = true;
			}

			if (!MatchMask(PreloadFilter.Current.SubIDFilter ?? String.Empty, CurrentSelected.SubMask ?? String.Empty))
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.subIDFilter>(PreloadFilter.Current, PreloadFilter.Current.SubIDFilter,
					new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetSubaccountNotAllowed), CurrentSelected.SubMask)));
				errorHappened = true;
			}

			if (errorHappened)
			{
				return adapter.Get();
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.First, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXFirstButton(ConfirmationMessage = null)]
		public virtual IEnumerable first(PXAdapter adapter)
		{
			if (!BudgetArticles.Cache.IsDirty)
			{
				GLBudgetLine article = PXSelectGroupBy<GLBudgetLine,
					Where<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>>,
					Aggregate<Min<GLBudgetLine.finYear>>>
					.Select(this);
				if (article != null && article.FinYear != null)
				{
					Filter.Current.FinYear = article.FinYear;
					Filter.Update(Filter.Current);
				}
			}
			else
			{
				BudgetArticles.Ask(Messages.BudgetPendingChangesTitle, Messages.BudgetPendingChangesMessage, MessageButtons.OK);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.Last, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLastButton(ConfirmationMessage = null)]
		public virtual IEnumerable last(PXAdapter adapter)
		{
			if (!BudgetArticles.Cache.IsDirty)
			{
				GLBudgetLine article = PXSelectGroupBy<GLBudgetLine, 
					Where<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>>, 
					Aggregate<Max<GLBudgetLine.finYear>>>.Select(this);
				if (article != null && article.FinYear != null)
				{
					Filter.Current.FinYear = article.FinYear;
					Filter.Update(Filter.Current);
				}
			}
			else
			{
				BudgetArticles.Ask(Messages.BudgetPendingChangesTitle, Messages.BudgetPendingChangesMessage, MessageButtons.OK);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.Distribute, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void distribute()
		{
			DistrFilter.AskExt();
		}

		[PXUIField(DisplayName = "Load", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void distributeOK()
		{
			BudgetDistributeFilter filter = DistrFilter.Current;

			PXResultset<GLBudgetLine> articlesToDistribute;

			OrganizationFinYear currentYear = PXSelect<
				OrganizationFinYear,
				Where<OrganizationFinYear.year, Equal<Required<OrganizationFinYear.year>>,
					And<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>>>>
				.Select(this, Filter.Current.FinYear, PXAccess.GetParentOrganizationID(Filter.Current.BranchID));

			int periodsToDistribute = _Periods > (int)currentYear.FinPeriods ? (int)currentYear.FinPeriods : _Periods;
			int periodsTotal = periodsToDistribute;
			bool hasAdjustmentPeriod = false;

			PXResultset<OrganizationFinPeriod> finPeriods = PXSelect<
				OrganizationFinPeriod, 
				Where<OrganizationFinPeriod.finYear, Equal<Required<OrganizationFinPeriod.finYear>>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>
				.Select(this, Filter.Current.FinYear, PXAccess.GetParentOrganizationID(Filter.Current.BranchID));
			foreach (OrganizationFinPeriod finPeriod in finPeriods)
			{
				if (finPeriod.EndDate == finPeriod.StartDate && Int32.Parse(finPeriod.PeriodNbr) == periodsToDistribute)
				{
					periodsToDistribute--;
					hasAdjustmentPeriod = true;
					break;
				}
			}

			if (filter.ApplyToAll != null && filter.ApplyToAll.Value == true)
			{
				PXResultset<GLBudgetLine> articlesInGroup = PXSelect<GLBudgetLine,
				Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.parentGroupID>>,
				And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
				And<GLBudgetLine.isGroup, Equal<False>,
				And<Match<Current<AccessInfo.userName>>>>>>>>>.Select(this, BudgetArticles.Current.ParentGroupID);
				articlesToDistribute = articlesInGroup;
				if (filter.ApplyToSubGroups != null && filter.ApplyToSubGroups.Value == true)
				{
					PXResultset<GLBudgetLine> allChilds = collectChildNodes(BudgetArticles.Current.ParentGroupID);
					articlesToDistribute = allChilds;
				}
			}
			else
			{
				articlesToDistribute = PXSelect<GLBudgetLine,
				Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>,
				And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
				And<GLBudgetLine.isGroup, Equal<False>>>>>>>.Select(this, BudgetArticles.Current.GroupID); ;
			}

			foreach (GLBudgetLine article in articlesToDistribute)
			{
				if (!(bool)article.IsGroup)
				{
					decimal amt = article.Amount ?? 0m;
					int prec = Filter.Current.Precision ?? 2;
					decimal minunit = (decimal)(1 / Math.Pow(10, (prec)));

					switch (filter.Method)
					{
						case BudgetDistributeFilter.method.Evenly:
							decimal periodAmt = (decimal)Math.Round((double)amt / periodsToDistribute, prec, MidpointRounding.AwayFromZero);
							decimal delta = amt - periodAmt * periodsToDistribute;
							if (delta < 0m)
							{
								periodAmt -= minunit;
								delta = amt - periodAmt * periodsToDistribute;
							}
							for (int i = 0; i < periodsTotal; i++)
							{
								decimal val = periodAmt;
								if (delta > 0)
								{
									val += minunit;
									delta -= minunit;
								}
								if (hasAdjustmentPeriod && i + 1 == periodsTotal)
								{
									val = 0;
								}
								UpdateAlloc(val, article, i + 1);
							}
							break;
						case BudgetDistributeFilter.method.PreviousYear:
							{
								GLBudgetLine prev = GetPrevArticle(article);
								if (prev == null) break;

								decimal allocated = 0m;
								int max_idx = 0;
								prev.Allocated = null;
								EnsureAlloc(prev);
								for (int i = 0; i < periodsTotal; i++)
								{
									decimal val = (decimal)Math.Round((double)(article.Amount * prev.Allocated[i] / prev.AllocatedAmount), prec, MidpointRounding.AwayFromZero);
									allocated += val;
									if (prev.Allocated[i] > prev.Allocated[max_idx]) max_idx = i;
									UpdateAlloc(val, article, i + 1);
								}
								GLBudgetLineDetail alloc = GetAlloc(article, max_idx + 1);
								if (alloc != null)
									UpdateAlloc((alloc.Amount ?? 0m) + amt - allocated, article, max_idx + 1);
							}
							break;
						case BudgetDistributeFilter.method.ComparedValues:
							{
								decimal allocated = 0m;
								int max_idx = 0;
								decimal compared = article.Compared.Sum();
								if (compared == 0) break;
								for (int i = 0; i < periodsTotal; i++)
								{
									decimal val = (decimal)Math.Round((double)(article.Amount * article.Compared[i] / compared), prec, MidpointRounding.AwayFromZero);
									allocated += val;
									if (article.Compared[i] > article.Compared[max_idx]) max_idx = i;
									UpdateAlloc(val, article, i + 1);
								}
								GLBudgetLineDetail alloc = GetAlloc(article, max_idx + 1);
								if (alloc != null)
									UpdateAlloc((alloc.Amount ?? 0m) + amt - allocated, article, max_idx + 1);
							}
							break;
					}
				}
			}
		}

		[PXButton]
		[PXUIField(DisplayName = Messages.PreloadArticles, MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable showPreload(PXAdapter adapter)
		{
			CheckBudgetLinesForComparisonData();

			PreloadFilter.Select();
			GLBudgetLine parentNode = PXSelect<GLBudgetLine, Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>>>.Select(this, this.CurrentSelected.Group);

			PXStringState acctStrState = (PXStringState)PreloadFilter.Cache.GetStateExt(null, typeof(BudgetPreloadFilter.fromAccount).Name);
			string acctWildCard = new String('?', acctStrState.InputMask.Length - 1);
			PXStringState subStrState = (PXStringState)BudgetArticles.Cache.GetStateExt(null, typeof(GLBudgetLine.subID).Name);
			string subWildCard = new String('?', subStrState.InputMask.Length - 1);

			if (parentNode != null)
			{
				PreloadFilter.Current.AccountCDWildcard = SubCDUtils.CreateSubCDWildcard(parentNode != null ? parentNode.AccountMask : string.Empty, AccountAttribute.DimensionName);
				if (parentNode.AccountMask != null)
				{
					Account AcctFrom = PXSelect<Account, Where<Account.active, Equal<True>,
						And<Account.accountCD, Like<Required<SelectedGroup.accountMaskWildcard>>>>,
						OrderBy<Asc<Account.accountCD>>>.SelectWindowed(this, 0, 1, PreloadFilter.Current.AccountCDWildcard);
					Account AcctTo = PXSelect<Account, Where<Account.active, Equal<True>,
						And<Account.accountCD, Like<Required<BudgetPreloadFilter.accountCDWildcard>>>>,
						OrderBy<Desc<Account.accountCD>>>.SelectWindowed(this, 0, 1, PreloadFilter.Current.AccountCDWildcard);
					PreloadFilter.Current.FromAccount = AcctFrom != null ? AcctFrom.AccountID : null;
					PreloadFilter.Current.ToAccount = AcctTo != null ? AcctTo.AccountID : null;
				}
				else
				{
					PreloadFilter.Current.FromAccount = null;
					PreloadFilter.Current.ToAccount = null;
				}
				PreloadFilter.Current.AccountIDFilter = parentNode.AccountMask ?? acctWildCard;
				PreloadFilter.Current.SubIDFilter = parentNode.SubMask ?? subWildCard;
			}
			else
			{
				PreloadFilter.Current.FromAccount = null;
				PreloadFilter.Current.ToAccount = null;
				PreloadFilter.Current.AccountIDFilter = acctWildCard;
				PreloadFilter.Current.SubIDFilter = subWildCard;
			}
			if (PreloadFilter.Current.BranchID == null) PreloadFilter.Current.BranchID = Filter.Current.CompareToBranchID;
			if (PreloadFilter.Current.LedgerID == null) PreloadFilter.Current.LedgerID = Filter.Current.CompareToLedgerId;
			if (PreloadFilter.Current.FinYear == null) PreloadFilter.Current.FinYear = Filter.Current.CompareToFinYear;
			BudgetArticles.AskExt();
			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = "Manage Budget", MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable showManage(PXAdapter adapter)
		{
			ManageDialog.AskExt();
			return adapter.Get();
		}

		[PXUIField(DisplayName = "ManageAction", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void manageOK()
		{
			ManageDialog.Select();
			ManageBudgetDialog manageDialog = ManageDialog.Current;
			switch (manageDialog.Method)
			{
				case ManageBudgetDialog.method.RollbackBudget:
					{
						_CurrentAction = GLBudgetEntryActionType.RollbackBudget; // it's need here for Persist()
						GLBudgetEntry process = PrepareGraphForLongOperation();
						PXLongOperation.StartOperation(this, (PXToggleAsyncDelegate)(() =>
						{
							process.RollbackBudget();
							PXLongOperation.SetCustomInfo(process);
						}));

						break;
					}
				case ManageBudgetDialog.method.ConvertBudget:
					{
						if (Filter.Ask(Messages.BudgetUpdateTitle, Messages.BudgetUpdateMessage, MessageButtons.OKCancel) == WebDialogResult.OK)
						{
							_CurrentAction = GLBudgetEntryActionType.ConvertBudget;  // it's need here for Persist()
							GLBudgetEntry process = PrepareGraphForLongOperation();
							PXLongOperation.StartOperation(this.UID, (PXToggleAsyncDelegate)(() =>
							{
								process.ConvertBudget();
								PXLongOperation.SetCustomInfo(process);
							}));

						}
						break;
					}
			}
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable preload(PXAdapter adapter)
		{
			bool errorHappened = false;
			if (PreloadFilter.Current.LedgerID == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.ledgerID>(PreloadFilter.Current, PreloadFilter.Current.LedgerID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				errorHappened = true;
			}
			if (PreloadFilter.Current.FinYear == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.finYear>(PreloadFilter.Current, PreloadFilter.Current.FinYear, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				errorHappened = true;
			}
			if (PreloadFilter.Current.ChangePercent == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<BudgetPreloadFilter.changePercent>(PreloadFilter.Current, PreloadFilter.Current.ChangePercent, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				errorHappened = true;
			}
			if (errorHappened)
			{
				return adapter.Get();
			}

			GLBudgetEntry process = PrepareGraphForLongOperation();
			PXLongOperation.StartOperation(this.UID, (PXToggleAsyncDelegate)(() =>
			{
				process.DoPreload();
				PXLongOperation.SetCustomInfo(process);
			}));

			return adapter.Get();
		}

		#endregion

		#region Action Function
		protected GLBudgetEntry PrepareGraphForLongOperation()
		{
			HoldNotchanged(this.Caches<GLBudgetLine>());
			HoldNotchanged(this.Caches<GLBudgetLineDetail>());
			Persist();
			//SelectTimeStamp();	// moved to Persist()
			GLBudgetEntry process = this.Clone();
			PXCache cache = process.Caches<SelectedGroup>();
			cache.RestoreCopy(process.CurrentSelected, this.CurrentSelected);
			return process;
		}
		
		private void PreloadBudgetTree()
		{
			_CurrentAction = GLBudgetEntryActionType.PreloadBudgetTree;
			foreach (Neighbour neighbour in Neighbour.Select())
			{
				if (neighbour.LeftEntityType.Contains(nameof(GLBudgetTree)) || neighbour.RightEntityType.Contains(nameof(GLBudgetTree)))
				{
					neighbour.LeftEntityType = neighbour.LeftEntityType.Replace(nameof(GLBudgetTree), nameof(GLBudgetLine));
					neighbour.RightEntityType = neighbour.RightEntityType.Replace(nameof(GLBudgetTree), nameof(GLBudgetLine));
					Neighbour.Update(neighbour);
				}
			}

			CurrentSelected.Group = Guid.Empty;

			PXResultset<GLBudgetTree> BudgetTree =
			PXSelectJoinOrderBy<GLBudgetTree,
				LeftJoin<Account, On<GLBudgetTree.accountID, Equal<Account.accountID>>,
				LeftJoin<Sub, On<GLBudgetTree.subID, Equal<Sub.subID>>>>,
			OrderBy<Desc<GLBudgetTree.isGroup, Asc<GLBudgetTree.sortOrder>>>>.Select(this);

			List<GLBudgetLine> articlesToBeInserted = new List<GLBudgetLine>(BudgetTree.RowCount ?? 10);
			Dictionary<Guid, GLBudgetLine> articteGroups = new Dictionary<Guid, GLBudgetLine>();
			List<GLBudgetLine> articleLines = new List<GLBudgetLine>(BudgetTree.RowCount ?? 10);
			List<GLBudgetLine> orphanLines = new List<GLBudgetLine>();

			Dictionary<Guid, HashSet<GLBudgetLine>> articlesByParentGroupIdx = new Dictionary<Guid, HashSet<GLBudgetLine>>();

			#region fill articlesToBeInserted, articleGroups, articleLines
			// BudgetTree is template for new Budget
			bool isGroup = false, isLine = false;
			foreach (PXResult<GLBudgetTree, Account, Sub> n in BudgetTree)
			{
				GLBudgetTree node = n;
				Account acc = n;
				Sub sub = n;
				if (node.GroupID != node.ParentGroupID)   // anytime is True in DB
				{
					isGroup = ((bool)node.IsGroup && node.AccountID == null);
					isLine = (node.AccountID != null && node.SubID != null);
					if (isLine || isGroup)
					{
						GLBudgetLine article = new GLBudgetLine();
						article.BranchID = Filter.Current.BranchID;
						article.LedgerID = Filter.Current.LedgerID;
						article.FinYear = Filter.Current.FinYear;
						article.GroupID = node.GroupID;
						article.ParentGroupID = node.ParentGroupID;

						article.IsGroup = node.IsGroup;

						article.IsPreloaded = true;

						article.AccountID = node.AccountID;
						if (article.AccountID.HasValue)
						{
							PXSelectorAttribute.StoreCached<GLBudgetLine.accountID>(Caches[typeof(GLBudgetLine)], article, acc);
						}
						article.SubID = node.SubID;
						if (article.SubID.HasValue)
						{
							PXSelectorAttribute.StoreCached<GLBudgetLine.subID>(Caches[typeof(GLBudgetLine)], article, sub);
						}
						article.TreeSortOrder = node.SortOrder;
						article.Description = node.Description;

						article.Rollup = node.Rollup;
						article.AccountMask = node.AccountMask;
						article.SubMask = node.SubMask;
						article.GroupMask = node.GroupMask;

						articlesToBeInserted.Add(article);

						if (isGroup)
						{
							articteGroups.Add(article.GroupID.Value, article);
						}
						if (isLine)
						{
							articleLines.Add(article);
						}

						// Fill tree structure. Root element is Guid.Empty
						setArticleToParentGroup(articlesByParentGroupIdx, article);
					}
				}
			}
			#endregion

			#region orphan lines
			//Search for orphan lines.  (no parent & GroupID != Guid.Empty)
			foreach (GLBudgetLine article in articlesToBeInserted) // is orphan only lines, not group?
			{
				if (article.ParentGroupID != Guid.Empty)
				{
					if (!articteGroups.ContainsKey(article.ParentGroupID.Value))
					{
						article.Rollup = true;
						orphanLines.Add(article);
						// it's not real group, remove it
						articlesByParentGroupIdx.Remove(article.ParentGroupID.Value);
					}
				}
			}

			//Put orphan articles into groups if possible
			foreach (GLBudgetLine article in orphanLines)
			{
				if (article.AccountID != null && article.SubID != null && !(bool)article.IsGroup) // if it's line of budget 
				{
					article.ParentGroupID = PutIntoNewInnerGroup(Guid.Empty, article, articteGroups.Values);
					setArticleToParentGroup(articlesByParentGroupIdx, article);
				}
			}
			#endregion

			#region Generate new Guids
			HashSet<GLBudgetLine> hsArticles = null;
			Guid newGuid = Guid.Empty;
			// for article lines with parent group = Guid.Empty (00000000-0000-0000-0000-000000000000)
			articlesByParentGroupIdx.TryGetValue(Guid.Empty, out hsArticles);
			if (hsArticles != null)
			{
				foreach (var a in hsArticles)
				{
					if (a.IsGroup == false)// not groups, only lines, group in next foreach
						a.GroupID = Guid.NewGuid();
				}
			}
			// for each group and the group's childs change group.GroupID and child.ParentGroupID
			foreach (GLBudgetLine articleGroup in articteGroups.Values)
			{
				hsArticles = null;
				articlesByParentGroupIdx.TryGetValue(articleGroup.GroupID.Value, out hsArticles);

				newGuid = Guid.NewGuid();
				articleGroup.GroupID = newGuid;
				if (hsArticles != null)
				{
					foreach (var article in hsArticles)
					{
						article.ParentGroupID = newGuid;
						if (article.IsGroup == false)
						{
							article.GroupID = Guid.NewGuid();
						}
					}
				}
			}
			#endregion

			foreach (GLBudgetLine article in articlesToBeInserted)
			{
				BudgetArticles.Insert(article);
			}
			_CurrentAction = GLBudgetEntryActionType.None;
		}

		protected virtual void RollbackBudget()
		{
			_CurrentAction = GLBudgetEntryActionType.RollbackBudget;
			SubDimensionDefinition.Fill();
			AccountDefinition.Fill();
			SubDefinition.Fill();

			PXResultset<GLBudgetLine> allArticles = PXSelect<GLBudgetLine,
			  Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
			  And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
			  And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this);
			PXResultset<GLBudgetLineDetail> allAllocations = PXSelect<GLBudgetLineDetail,
			  Where<GLBudgetLineDetail.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
			  And<GLBudgetLineDetail.branchID, Equal<Current<BudgetFilter.branchID>>,
			  And<GLBudgetLineDetail.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this);

			GLBudgetLineDetailIdx allAllocationsIndex = new GLBudgetLineDetailIdx(allAllocations);
			_AllocationIndex = allAllocationsIndex;
			GLBudgetLineIdx allArticlesIndex = new GLBudgetLineIdx(allArticles);
			_ArticleIndex = allArticlesIndex;

			foreach (GLBudgetLine article in allArticles)
			{
				if (article.ReleasedAmount == 0)
				{
					if (!(bool)article.IsGroup && !(bool)article.IsPreloaded && !(bool)article.Rollup)
					{
						RollupArticleAmount(article, -article.Amount);

						foreach (GLBudgetLineDetail alloc in allAllocationsIndex.GetList(article.GroupID.Value))
						{
							RollupAllocation(article, int.Parse(alloc.FinPeriodID.Substring(4)), -alloc.Amount);
						}
						BudgetArticles.Delete(article);
					}
				}
				else if (article.ReleasedAmount != article.AllocatedAmount || article.ReleasedAmount != article.Amount)
				{
					foreach (GLBudgetLineDetail allocation in allAllocationsIndex.GetList(article.GroupID.Value))
					{
						decimal? delta = allocation.ReleasedAmount - allocation.Amount;
						allocation.Amount = allocation.ReleasedAmount;
						Allocations.Update(allocation);
						RollupAllocation(article, int.Parse(allocation.FinPeriodID.Substring(4)), delta);
					}
					article.Amount = article.ReleasedAmount;
					article.Released = true;
					article.Allocated = null;
					EnsureAlloc(article);
					BudgetArticles.Update(article);
				}
				else if (article.ReleasedAmount == article.AllocatedAmount && article.ReleasedAmount == article.Amount && article.Released != true)
				{
					bool released = true;

					foreach (GLBudgetLineDetail allocation in allAllocationsIndex.GetList(article.GroupID.Value))
					{
						if (allocation.ReleasedAmount != allocation.Amount)
							released = false;
					}
					if (released)
					{
						article.Released = true;
						BudgetArticles.Update(article);
					}
				}
			}
			BudgetArticles.View.RequestRefresh();
		}

		private void ConvertBudget()
		{
			// TODO: rewrite to using indexes

			SubDimensionDefinition.Fill();
			AccountDefinition.Fill();
			SubDefinition.Fill();

			PXResultset<GLBudgetLine> articles = PXSelect<GLBudgetLine,
				Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
				And<GLBudgetLine.released, Equal<False>,
				And<GLBudgetLine.isGroup, Equal<False>>>>>>>.Select(this);
			_ArticleIndex = new GLBudgetLineIdx(articles);

			PXResultset<GLBudgetLineDetail> allocations = PXSelect<GLBudgetLineDetail,
			  Where<GLBudgetLineDetail.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
			  And<GLBudgetLineDetail.branchID, Equal<Current<BudgetFilter.branchID>>,
			  And<GLBudgetLineDetail.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this);

			foreach (GLBudgetLine article in articles)
			{
				decimal allocatedAmt = 0;
				// TODO: Bag perfomance - GLBudgetLineDetailIdx
				foreach (GLBudgetLineDetail allocation in Allocations.Select(article.FinYear, article.GroupID))
				{
					allocatedAmt += (decimal)allocation.Amount;
				}
				article.AllocatedAmount = allocatedAmt;
				BudgetArticles.Update(article);
			}

			CurrentSelected.AccountMaskWildcard = SelectedGroup.WildcardAnything;
			CurrentSelected.AccountMask = "";
			CurrentSelected.SubMask = "";
			CurrentSelected.SubMaskWildcard = SelectedGroup.WildcardAnything;

			List<GLBudgetLine> oldArticles = new List<GLBudgetLine>();
			List<GLBudgetLineDetail> oldAllocations = new List<GLBudgetLineDetail>();

			foreach (GLBudgetLine budgetLine in PXSelect<GLBudgetLine,
			Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
			And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
			And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this))
			{
				if (!(bool)budgetLine.IsGroup || ((bool)budgetLine.IsGroup && budgetLine.AccountID != null && budgetLine.SubID != null))
				{
					Guid tempGuid = Guid.NewGuid();
					foreach (GLBudgetLineDetail budgetLineDetail in Allocations.Select(Filter.Current.FinYear, budgetLine.GroupID))
					{
						GLBudgetLineDetail tmpAllocation = Allocations.Cache.CreateCopy(budgetLineDetail) as GLBudgetLineDetail;
						tmpAllocation.GroupID = tempGuid;
						oldAllocations.Add(tmpAllocation);
					}
					GLBudgetLine tmpArticle = BudgetArticles.Cache.CreateCopy(budgetLine) as GLBudgetLine;
					tmpArticle.GroupID = tempGuid;
					tmpArticle.NoteID = null;
					oldArticles.Add(tmpArticle);
				}
			}

			//Check for conflicting lines
			foreach (GLBudgetLine oldArticle in oldArticles)
			{
				foreach (GLBudgetTree treeNode in PXSelect<GLBudgetTree>.Select(this))
				{
					if ((bool)treeNode.IsGroup && treeNode.AccountID != null && treeNode.SubID != null)
					{
						string oldAccountCD = ((Account)PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, oldArticle.AccountID)).AccountCD;
						string newAccountCD = ((Account)PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, treeNode.AccountID)).AccountCD;
						string oldSubCD = ((Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, oldArticle.SubID)).SubCD;
						string newSubCD = ((Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, treeNode.SubID)).SubCD;
						if (MatchMask(oldAccountCD, newAccountCD) && MatchMask(oldSubCD, newSubCD))
						{
							throw new PXException(Messages.BudgetUpdateConflictMessage, oldAccountCD, oldSubCD);
						}
					}
				}
			}

			foreach (GLBudgetLine budgetLine in PXSelect<GLBudgetLine,
			Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
			And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
			And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this))
			{
				BudgetArticles.Delete(budgetLine);
			}

			PreloadBudgetTree();

			PXResultset<GLBudgetLine> existingGroups = PXSelect<GLBudgetLine,
				Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this);

			//Put old articles into new groups
			foreach (GLBudgetLine oldArticle in oldArticles)
			{
				bool groupFound = false;
				foreach (GLBudgetLine newGroup in existingGroups)
				{
					if (newGroup.AccountMask != null && (newGroup.AccountMask != String.Empty || newGroup.SubMask != String.Empty))
					{
						if (MatchMask(((Account)PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, oldArticle.AccountID)).AccountCD, newGroup.AccountMask ?? String.Empty)
							&& MatchMask(((Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, oldArticle.SubID)).SubCD, newGroup.SubMask ?? String.Empty))
						{
							groupFound = true;
							if ((bool)newGroup.IsGroup)
							{
								oldArticle.ParentGroupID = newGroup.GroupID;
								oldArticle.GroupMask = newGroup.GroupMask;
								oldArticle.ParentGroupID = PutIntoInnerGroup(oldArticle, newGroup).GroupID;
								if (newGroup.AccountID != null && newGroup.SubID != null)
								{
									oldArticle.Rollup = true;
								}
							}
						}
					}
				}
				if (!groupFound)
				{
					oldArticle.ParentGroupID = Guid.Empty;
				}
			}
			foreach (GLBudgetLine oldArticle in oldArticles)
			{
				GLBudgetLine locatedArticle = LocateInBudgetArticlesCache(existingGroups, oldArticle, true);
				if (!(bool)oldArticle.IsGroup && locatedArticle == null)
				{
					foreach (GLBudgetLineDetail oldAllocation in oldAllocations.Where(x => x.GroupID == oldArticle.GroupID))
					{
						Allocations.Insert(oldAllocation);
						RollupAllocation(oldArticle, int.Parse(oldAllocation.FinPeriodID.Substring(4)), oldAllocation.Amount);
					}
					BudgetArticles.Insert(oldArticle);
				}
				else if (!(bool)oldArticle.IsGroup && locatedArticle != null)
				{
					foreach (GLBudgetLineDetail oldAllocation in oldAllocations.Where(x => x.GroupID == oldArticle.GroupID))
					{
						oldAllocation.GroupID = locatedArticle.GroupID;
						Allocations.Insert(oldAllocation);
						RollupAllocation(locatedArticle, int.Parse(oldAllocation.FinPeriodID.Substring(4)), oldAllocation.Amount);
					}
					locatedArticle.Amount = oldArticle.Amount;
					locatedArticle.Released = oldArticle.Released;
					locatedArticle.ReleasedAmount = oldArticle.ReleasedAmount;
					locatedArticle.WasReleased = locatedArticle.WasReleased == true || oldArticle.WasReleased == true;
					BudgetArticles.Update(locatedArticle);
				}
			}
			foreach (GLBudgetLine oldArticle in oldArticles)
			{
				GLBudgetLine locatedArticle = LocateInBudgetArticlesCache(existingGroups, oldArticle, true);
				if ((bool)oldArticle.IsGroup && locatedArticle != null)
				{
					locatedArticle.Released = oldArticle.Released;
					locatedArticle.WasReleased = locatedArticle.WasReleased == true || oldArticle.WasReleased == true;
					BudgetArticles.Update(locatedArticle);
				}
			}
		}

		protected virtual void DoPreload()
		{
			_CurrentAction = GLBudgetEntryActionType.Preload;
			#region Prepare Data

			SubDimensionDefinition.Fill();
			AccountDefinition.Fill();
			SubDefinition.Fill();

			Account AccountFrom = PXSelect<Account, Where<Account.accountID, Equal<Current<BudgetPreloadFilter.fromAccount>>>>.Select(this);
			Account AccountTo = PXSelect<Account, Where<Account.accountID, Equal<Current<BudgetPreloadFilter.toAccount>>>>.Select(this);
			if (AccountFrom == null)
				AccountFrom = PXSelectOrderBy<Account, OrderBy<Asc<Account.accountCD>>>.SelectWindowed(this, 0, 1);

			if (AccountTo == null)
				AccountTo = PXSelectOrderBy<Account, OrderBy<Desc<Account.accountCD>>>.SelectWindowed(this, 0, 1);

			#region Select rows from GLHistory and fill articlesFromHistory and allocationsFromHistory
			int? Account = null;
			int? Subaccount = null;
			Guid? groupID = null;

			List<GLBudgetLine> articlesFromHistory = new List<GLBudgetLine>();
			List<GLBudgetLineDetail> allocationsFromHistory = new List<GLBudgetLineDetail>();

			foreach (PXResult<GLHistory, Branch, OrganizationFinPeriod, Sub, Account> result in PXSelectJoin<
				GLHistory,
				InnerJoin<Branch,
					On<GLHistory.branchID, Equal<Branch.branchID>>,
				InnerJoin<OrganizationFinPeriod,
					On<OrganizationFinPeriod.finPeriodID, Equal<GLHistory.finPeriodID>,
					And<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>>>,
				InnerJoin<Sub, 
					On<Sub.subID, Equal<GLHistory.subID>>,
				InnerJoin<Account, 
					On<Account.accountID, Equal<GLHistory.accountID>>>>>>,
				Where<GLHistory.ledgerID, Equal<Current<BudgetPreloadFilter.ledgerID>>,
					And<GLHistory.branchID, Equal<Current<BudgetPreloadFilter.branchID>>,
					And<OrganizationFinPeriod.finYear, Equal<Current<BudgetPreloadFilter.finYear>>,
					And<Account.accountCD, GreaterEqual<Required<Account.accountCD>>,
					And<Account.accountCD, LessEqual<Required<Account.accountCD>>,
					And<Account.accountCD, Like<Required<Account.accountCD>>,
					And<Sub.subCD, Like<Current<BudgetPreloadFilter.subCDWildcard>>,
					And2<
						Where<OrganizationFinPeriod.status, Equal<FinPeriod.status.open>,
							Or<OrganizationFinPeriod.status, Equal<FinPeriod.status.closed>>>,
						And<GLHistory.accountID, NotEqual<Current<GL.GLSetup.ytdNetIncAccountID>>>>>>>>>>>, 
				OrderBy<
					Asc<Account.accountID>>>
				.Select(this,
						AccountFrom.AccountCD,
						AccountTo.AccountCD,
						SubCDUtils.CreateSubCDWildcard(PreloadFilter.Current.AccountIDFilter != null
														? PreloadFilter.Current.AccountIDFilter 
														: string.Empty, AccountAttribute.DimensionName)))
			{
				GLHistory history = result;
				Account acct = result;
				OrganizationFinPeriod period = result;
				if (period.PeriodNbr != null && Int32.Parse(period.PeriodNbr) <= _PeriodsInCurrentYear)
				{
					if (history.AccountID != Account || history.SubID != Subaccount)
					{
						GLBudgetLine newArticle = new GLBudgetLine();
						newArticle.BranchID = Filter.Current.BranchID;
						newArticle.LedgerID = Filter.Current.LedgerID;
						newArticle.FinYear = Filter.Current.FinYear;
						newArticle.AccountID = history.AccountID;
						newArticle.SubID = history.SubID;
						newArticle.Released = false;
						newArticle.WasReleased = false;
						newArticle.ParentGroupID = CurrentSelected.Group ?? Guid.Empty;
						newArticle.GroupID = Guid.NewGuid();
						newArticle.Amount = 0;

						Account = history.AccountID;
						Subaccount = history.SubID;
						groupID = newArticle.GroupID;

						articlesFromHistory.Add(newArticle);
					}
					GLBudgetLineDetail alloc = new GLBudgetLineDetail();
					alloc.GroupID = groupID;
					alloc.BranchID = Filter.Current.BranchID;
					alloc.LedgerID = Filter.Current.LedgerID;
					alloc.FinYear = Filter.Current.FinYear;
					alloc.AccountID = history.AccountID;
					alloc.SubID = history.SubID;
					alloc.FinPeriodID = history.FinPeriodID.Replace(history.FinYear, Filter.Current.FinYear);
					//! Should be account-type dependant. Double check
					if (acct.Type == AccountType.Asset || acct.Type == AccountType.Expense)
					{
						alloc.Amount = Math.Round((decimal)((history.CuryFinPtdDebit - history.CuryFinPtdCredit) * PreloadFilter.Current.ChangePercent / 100), Filter.Current.Precision ?? 2);
					}
					else
					{
						alloc.Amount = Math.Round((decimal)((history.CuryFinPtdCredit - history.CuryFinPtdDebit) * PreloadFilter.Current.ChangePercent / 100), Filter.Current.Precision ?? 2);
					}
					allocationsFromHistory.Add(alloc);
				}
			}
			#endregion

			#region Seelct rows from GLBudgetLine and GLBudgetLineDetail. (existingArticles, existingHiddenArticles, existingAllocations, articlesInCurrentGroup, articlesNotInCurrentGroup)
			var existingArticles = PXSelect<GLBudgetLine,
				Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
					And<Match<Current<AccessInfo.userName>>>>>>>.Select(this, 0).AsEnumerable();

			var existingHiddenArticles = PXSelect<GLBudgetLine,
				 Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
				 And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				 And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
					  And<Not<Match<Current<AccessInfo.userName>>>>>>>>.Select(this, 0).AsEnumerable();

			PXResultset<GLBudgetLineDetail> existingAllocations = PXSelect<GLBudgetLineDetail,
				Where<GLBudgetLineDetail.branchID, Equal<Current<BudgetFilter.branchID>>,
				And<GLBudgetLineDetail.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
				And<GLBudgetLineDetail.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this);

			//Collecting child nodes of the selected group
			PXResultset<GLBudgetLine> articlesInCurrentGroup = new PXResultset<GLBudgetLine>();
			if (CurrentSelected.Group != Guid.Empty)
			{
				articlesInCurrentGroup.Add(PXSelect<GLBudgetLine, Where<GLBudgetLine.groupID, Equal<Required<GLBudgetLine.groupID>>>>.Select(this, CurrentSelected.Group));
			}
			articlesInCurrentGroup.AddRange(collectChildNodes(CurrentSelected.Group));

			//Collecting not-in-selected-group nodes
			PXResultset<GLBudgetLine> articlesNotInCurrentGroup = new PXResultset<GLBudgetLine>();
			articlesNotInCurrentGroup.AddRange(existingArticles.Except(articlesInCurrentGroup, new PXResultGLBudgetLineComparer()));

			#region Prepare data for fast search

			_ArticleIndex = new GLBudgetLineIdx(existingArticles.Count() + existingHiddenArticles.Count(), true);
			_ArticleIndex.Add(existingArticles);
			_ArticleIndex.Add(existingHiddenArticles);

			GLBudgetLineIdx existingArticlesIdx = new GLBudgetLineIdx(existingArticles);
			GLBudgetLineIdx existingHiddenArticlesIdx = new GLBudgetLineIdx(existingHiddenArticles);
			GLBudgetLineIdx articlesInCurrentGroupIdx = new GLBudgetLineIdx(articlesInCurrentGroup);

			GLBudgetLineDetailIdx allocationsFromHistoryIdx = new GLBudgetLineDetailIdx(allocationsFromHistory);
			GLBudgetLineDetailIdx existingAllocationsIdx = new GLBudgetLineDetailIdx(existingAllocations);
			_AllocationIndex = existingAllocationsIdx;

			GLBudgetLineIdx.checkGLBudgetLine checkGLBudgetLineIncludeGroups = delegate (GLBudgetLine line, GLBudgetLine sourceLine)
			{
				if (line == null) return null;
				GLBudgetLine ret = null;
				PXEntryStatus status = BudgetArticles.Cache.GetStatus(line);
				if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					ret = line;
					if (sourceLine != null)
						ret = (line.GroupID != sourceLine.GroupID) ? line : null;
				}
				return ret;
			};

			GLBudgetLineIdx.checkGLBudgetLine checkGLBudgetLineNotIncludeGroups = delegate (GLBudgetLine line, GLBudgetLine sourceLine)
			{
				if (line == null) return null;
				GLBudgetLine ret = null;

				PXEntryStatus status = BudgetArticles.Cache.GetStatus(line);
				if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					ret = line;
					if (sourceLine != null)
						ret = (line.GroupID != sourceLine.GroupID) ? line : null;

					ret = (line.IsGroup != true) ? line : null;
				}
				return ret;
			};

			GLBudgetLineDetailIdx.checkGLBudgetLineDetail checkGLBudgetLineDetail = delegate (GLBudgetLineDetail alloc, GLBudgetLineDetail sourceAlloc)
			{
				if (alloc == null) return null;
				GLBudgetLineDetail ret = null;
				PXEntryStatus status = Allocations.Cache.GetStatus(alloc);
				if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					if (sourceAlloc != null)
						ret = (alloc.GroupID != sourceAlloc.GroupID) ? alloc : null;
					ret = alloc;
				}
				return ret;
			};

			#endregion
			#endregion 

			#region  Put new articles into existing groups if possible
			foreach (GLBudgetLine newArticle in articlesFromHistory)
			{
				bool skipArticle = false;
				//Do not update/add articles that are not in the selected group
				foreach (GLBudgetLine existingArticle in articlesNotInCurrentGroup)
				{
					if (existingArticle.AccountMask != null)
					{
						if (MatchMask(AccountDefinition.getCD(newArticle.AccountID.Value), existingArticle.AccountMask ?? String.Empty)
							&& MatchMask(SubDefinition.getCD(newArticle.SubID.Value), existingArticle.SubMask ?? String.Empty))
						{
							skipArticle = true;
						}
					}
				}
				foreach (GLBudgetLine existingArticle in articlesInCurrentGroup)
				{
					if (existingArticle.AccountMask != null)
					{
						if (MatchMask(AccountDefinition.getCD(newArticle.AccountID.Value), existingArticle.AccountMask ?? String.Empty)
							&& MatchMask(SubDefinition.getCD(newArticle.SubID.Value), existingArticle.SubMask ?? String.Empty))
						{
							skipArticle = false;
							if ((bool)existingArticle.IsGroup || !String.IsNullOrEmpty(existingArticle.AccountMask) || !String.IsNullOrEmpty(existingArticle.SubMask))
							{
								GLBudgetLine innerArticle = PutIntoInnerGroup(newArticle, existingArticle);
								if ((innerArticle.AccountID == newArticle.AccountID && innerArticle.SubID == newArticle.SubID) ||
									 (!String.IsNullOrEmpty(innerArticle.AccountMask) || !String.IsNullOrEmpty(innerArticle.SubMask)))
								{
									newArticle.ParentGroupID = innerArticle.GroupID;
									newArticle.GroupMask = innerArticle.GroupMask;
								}
								if (innerArticle.AccountID != null && innerArticle.SubID != null)
								{
									newArticle.Rollup = true;
								}
							}
							else
							{
								newArticle.ParentGroupID = existingArticle.ParentGroupID;
								newArticle.GroupMask = existingArticle.GroupMask;
							}
						}
					}
				}
				if (skipArticle)
				{
					foreach (GLBudgetLineDetail allocLine in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
					{
						allocationsFromHistoryIdx.Delete(allocLine.GroupID.Value, allocLine.FinPeriodID);
						allocLine.GroupID = Guid.Empty;
					}

					newArticle.GroupID = Guid.Empty;
				}
			}

			//Remove all articles/allocations that should not be processed
			articlesFromHistory.RemoveAll(x => x.GroupID == Guid.Empty);
			allocationsFromHistory.RemoveAll(x => x.GroupID == Guid.Empty);

			#endregion

			#endregion

			#region Process Data
			// articlesFromHistory from GLHistory
			// allocationsFromHistory from GLBudgetLineDetail 
			// existingArticles from GLBudgetLine
			// existingAllocations from GLBudgetLineDetail
			switch (PreloadFilter.Current.PreloadAction)
			{
				//Reload all
				case 0:
					{
						foreach (GLBudgetLine article in articlesInCurrentGroup)
						{
							if (!(bool)article.IsPreloaded && article.ReleasedAmount == 0)
							{
								BudgetArticles.Delete(article);
							}
						}
						foreach (GLBudgetLine newArticle in articlesFromHistory)
						{
							if (_ArticleIndex.GetByKey(newArticle, checkGLBudgetLineIncludeGroups) == null)
							{
								foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
								{
									if (existingAllocationsIdx.GetByKey(newAllocation, checkGLBudgetLineDetail) == null)
									{
										newArticle.Amount += newAllocation.Amount;
										Allocations.Insert(newAllocation);
										RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
									}
								}
								BudgetArticles.Insert(newArticle);
							}
						}
						break;
					}
				//Update existing
				case 1:
					{
						GLBudgetLine locatedArticle = null; ;
						foreach (GLBudgetLine newArticle in articlesFromHistory)
						{
							locatedArticle = articlesInCurrentGroupIdx.GetByKey(newArticle, checkGLBudgetLineNotIncludeGroups);
							if (locatedArticle != null && !(bool)locatedArticle.IsGroup && !locatedArticle.AccountMask.Contains('?') && !locatedArticle.SubMask.Contains('?'))
							{
								locatedArticle.Allocated = null;
								newArticle.ParentGroupID = locatedArticle.ParentGroupID;

								foreach (GLBudgetLineDetail allocation in existingAllocationsIdx.GetList(locatedArticle.GroupID.Value))
								{
									RollupAllocation(locatedArticle, int.Parse(allocation.FinPeriodID.Substring(4)), -allocation.Amount);
									allocation.Amount = 0;
									Allocations.Update(allocation);
								}

								foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
								{
									GLBudgetLineDetail locatedAlloc = existingAllocationsIdx.GetByKey(newAllocation, checkGLBudgetLineDetail);
									
									if (locatedAlloc != null)
									{
										locatedAlloc.Amount = newAllocation.Amount;
										newArticle.Amount += locatedAlloc.Amount;
										Allocations.Update(locatedAlloc);
										RollupAllocation(locatedArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
									}
									else
									{
										newAllocation.GroupID = locatedArticle.GroupID;
										newArticle.Amount += newAllocation.Amount;
										Allocations.Insert(newAllocation);
										RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
									}
								}
								locatedArticle.Amount = newArticle.Amount;
								BudgetArticles.Update(locatedArticle);
							}
							//Updating amounts on Aggregating lines
							else if (newArticle.Rollup != null && (bool)newArticle.Rollup)
							{
								GLBudgetLine parentNode = GetArticleByCurrentFilter(newArticle.ParentGroupID);
								if (parentNode != null && parentNode.Cleared == null)
								{
									parentNode.Amount = 0;
									parentNode.Cleared = true;
									BudgetArticles.Update(parentNode);

									foreach (GLBudgetLineDetail allocation in existingAllocationsIdx.GetList(parentNode.GroupID.Value))
									{
										RollupAllocation(parentNode, int.Parse(allocation.FinPeriodID.Substring(4)), -allocation.Amount);
										allocation.Amount = 0;
										Allocations.Update(allocation);
									}
								}
								foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
								{
									newArticle.Amount += newAllocation.Amount;
									Allocations.Insert(newAllocation);
									RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
								}
								BudgetArticles.Insert(newArticle);
							}
						}
						//Removing rollup lines
						foreach (GLBudgetLine article in BudgetArticles.Cache.Cached)
						{
							if (article.Rollup != null && (bool)article.Rollup)
							{
								BudgetArticles.Delete(article);
							}
							if (article.Cleared != null)
							{
								article.Cleared = null;
								article.Amount = article.AllocatedAmount;
								BudgetArticles.Update(article);
							}
						}
						break;
					}
				//Update and Add
				case 2:
					{
						GLBudgetLine locatedArticle = null;
						GLBudgetLineDetail locatedAllocation = null;
						foreach (GLBudgetLine newArticle in articlesFromHistory)
						{
							locatedArticle = articlesInCurrentGroupIdx.GetByKey(newArticle, checkGLBudgetLineIncludeGroups);
							if (locatedArticle != null && !locatedArticle.AccountMask.Contains('?') && !locatedArticle.SubMask.Contains('?')) // Update GLBudgetLineDetail
							{
								if (locatedArticle.IsGroup == false)
								{
									if (locatedArticle.Rollup != null && (bool)locatedArticle.Rollup)
									{
										BudgetArticles.Delete(locatedArticle);
									}
									else
									{
										locatedArticle.Allocated = null;
										newArticle.ParentGroupID = locatedArticle.ParentGroupID;
										foreach (GLBudgetLineDetail allocation in existingAllocationsIdx.GetList(locatedArticle.GroupID.Value))
										{
											RollupAllocation(locatedArticle, int.Parse(allocation.FinPeriodID.Substring(4)), -allocation.Amount);
											allocation.Amount = 0;
											Allocations.Update(allocation);
										}
									}
									foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
									{
										locatedAllocation = existingAllocationsIdx.GetByKey(newAllocation, checkGLBudgetLineDetail);
										if (locatedAllocation != null)
										{
											locatedAllocation.Amount = newAllocation.Amount;
											newArticle.Amount += locatedAllocation.Amount;
											Allocations.Update(locatedAllocation);
											RollupAllocation(locatedArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
										}
										else
										{
											newAllocation.GroupID = locatedArticle.GroupID;
											newArticle.Amount += newAllocation.Amount;
											Allocations.Insert(newAllocation);
											RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
										}
									}
									locatedArticle.Amount = newArticle.Amount;
									BudgetArticles.Update(locatedArticle);
								}
							}
							//Updating amounts on Aggregating lines
							else if (newArticle.Rollup != null && (bool)newArticle.Rollup)// Add new GLBudgetLineDetail
							{								
								GLBudgetLine parentNode;
								parentNode = ArticleIndex.Get(newArticle.ParentGroupID);

								if (parentNode != null && parentNode.Cleared == null)
								{
									parentNode.Amount = 0;
									parentNode.Cleared = true;
									BudgetArticles.Update(parentNode);

									foreach (GLBudgetLineDetail allocation in existingAllocationsIdx.GetList(parentNode.GroupID.Value))
									{
										RollupAllocation(parentNode, int.Parse(allocation.FinPeriodID.Substring(4)), -allocation.Amount);
										allocation.Amount = 0;
										Allocations.Update(allocation);
									}
								}
								foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
								{
									newArticle.Amount += newAllocation.Amount;
									Allocations.Insert(newAllocation);
									RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
								}
								BudgetArticles.Insert(newArticle);
							}
							else if (existingHiddenArticlesIdx.GetByKey(newArticle, checkGLBudgetLineIncludeGroups) == null) // Add new GLBudgetLineDetail
							{
								foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
								{
									newArticle.Amount += newAllocation.Amount;
									Allocations.Insert(newAllocation);
									RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
								}
								BudgetArticles.Insert(newArticle);
							}
						}
						//Removing rollup lines
						foreach (GLBudgetLine article in BudgetArticles.Cache.Cached)
						{
							if (article.Rollup != null && (bool)article.Rollup)
							{
								BudgetArticles.Delete(article);
							}
							if (article.Cleared != null)
							{
								article.Cleared = null;
								article.Amount = article.AllocatedAmount;
								BudgetArticles.Update(article);
							}
						}
						break;
					}
				//Add
				case 3:
					{
						foreach (GLBudgetLine newArticle in articlesFromHistory)
						{
							if (_ArticleIndex.GetByKey(newArticle, checkGLBudgetLineIncludeGroups) == null && (newArticle.Rollup == null || !(bool)newArticle.Rollup))
							{
								foreach (GLBudgetLineDetail newAllocation in allocationsFromHistoryIdx.GetList(newArticle.GroupID.Value))
								{
									newArticle.Amount += newAllocation.Amount;
									Allocations.Insert(newAllocation);
									RollupAllocation(newArticle, int.Parse(newAllocation.FinPeriodID.Substring(4)), newAllocation.Amount);
								}
								BudgetArticles.Insert(newArticle);
							}
						}
						break;
					}
			}
			#endregion
			_CurrentAction = GLBudgetEntryActionType.None;
		}

		#endregion

		public override void Persist()
		{
			switch (_CurrentAction)
			{
				case GLBudgetEntryActionType.PreloadBudgetTree:
				case GLBudgetEntryActionType.Preload:
				case GLBudgetEntryActionType.ConvertBudget:
				case GLBudgetEntryActionType.RollbackBudget:
					break;
				default:
					CheckBudgetLinesForDuplicates();
					break;
			}
			this.SelectTimeStamp();
			base.Persist();
		}

		#region Events

		#region GLBudgetLineDetail

		protected virtual void AllocationFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, int fieldNbr)
		{
			GLBudgetLine article = (GLBudgetLine)e.Row;
			PXDecimalState state = (PXDecimalState)PXDecimalState.CreateInstance(e.ReturnState, Filter.Current?.Precision, _PrefixPeriodField + fieldNbr, false, 0, decimal.MinValue, decimal.MaxValue);
			e.ReturnState = (object)state;
			state.DisplayName = String.Format(PXMessages.LocalizeNoPrefix(Messages.PeriodFormatted), fieldNbr);
			state.Enabled = true;
			state.Visible = true;
			state.Visibility = PXUIVisibility.Visible;
			if (article != null)
			{
				decimal val;
				if (article.IsGroup != null && (bool)article.IsGroup)
				{
					state.Enabled = false;
				}
				if (article.Comparison == true && article.Compared != null && fieldNbr <= article.Compared.Length)
				{
					state.Enabled = false;
					val = article.Compared[fieldNbr - 1];
				}
				else
				{
					EnsureAlloc(article);
					val = article.Allocated[fieldNbr - 1];
				}
				e.ReturnValue = val;
			}
			if (fieldNbr > _PeriodsInCurrentYear)
			{
				state.Visible = false;
			}
		}

		protected virtual void AllocationFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e, int fieldNbr)
		{
			GLBudgetLine article = e.Row as GLBudgetLine;
			// Parses and rounds to appropriate precision
			object NewValue = e.NewValue;

			decimal val = 0;
			if (e.NewValue is string)   // when import from excel
			{
				if (Decimal.TryParse((string)e.NewValue, out val))
				{
					NewValue = val;
				}
				else if (!Decimal.TryParse(((string)e.NewValue).Replace(',', '.'), out val))
				{
					Allocations.Cache.RaiseFieldUpdating<GLBudgetLineDetail.amount>(
						new GLBudgetLineDetail { GroupID = article.GroupID, LedgerID = article.LedgerID, BranchID = article.BranchID, FinYear = article.FinYear }
					, ref NewValue);
				}
			}

			if (IsImport)
			{
				if (article.GroupID == null)
				{
					article.GroupID = Guid.NewGuid();
				}
				if (IsNullOrEmpty(article.ParentGroupID))
				{
					article.ParentGroupID = CurrentSelected.Group;
				}
			}

			UpdateAlloc((NewValue as decimal?) ?? 0m, article, fieldNbr);

			article.Released = false;
		}
		#endregion

		#region Filters
		#region BudgetDistributeFilter
		protected virtual void BudgetDistributeFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			BudgetFilter f = Filter.Current;
			GLBudgetLine article = BudgetArticles.Current;
			if (article == null) return;

			Dictionary<string, string> allowed = new BudgetDistributeFilter.method.ListAttribute().ValueLabelDic;
			if (f == null || f.CompareToBranchID == null || f.CompareToLedgerId == null || string.IsNullOrEmpty(f.CompareToFinYear) || article.Compared == null || article.Compared.Sum() == 0m)
			{
				allowed.Remove(BudgetDistributeFilter.method.ComparedValues);
			}

			GLBudgetLine prev = GetPrevArticle(article);
			if (prev == null || prev.AllocatedAmount == 0m)
			{
				allowed.Remove(BudgetDistributeFilter.method.PreviousYear);
			}

			PXStringListAttribute.SetList<BudgetDistributeFilter.method>(sender, e.Row, allowed.Keys.ToArray(), allowed.Values.ToArray());

			BudgetDistributeFilter row = e.Row as BudgetDistributeFilter;
			PXUIFieldAttribute.SetEnabled<BudgetDistributeFilter.applyToSubGroups>(sender, row, row.ApplyToAll != null && (bool)row.ApplyToAll);
			if (row.ApplyToAll == null || !(bool)row.ApplyToAll) row.ApplyToSubGroups = false;
		}
		#endregion
		#region BudgetFilter
		protected virtual void BudgetFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			BudgetFilter row = (BudgetFilter)e.Row;
			this.ShowPreload.SetEnabled(row != null && row.BranchID != null && row.LedgerID != null && row.FinYear != null);
			this.ShowManage.SetEnabled(row != null && row.BranchID != null && row.LedgerID != null && row.FinYear != null);
			BudgetArticles.Cache.AllowInsert = row != null && row.BranchID != null && row.LedgerID != null && row.FinYear != null;
			if (row != null && row.Precision == null)
			{
				Currency currency = PXSelectJoin<Currency,
					InnerJoin<Ledger, On<Ledger.baseCuryID, Equal<Currency.curyID>>>,
					Where<Ledger.ledgerID, Equal<Current<BudgetFilter.ledgerID>>>>
					.Select(this);
				row.Precision = currency != null ? currency.DecimalPlaces : 2;
			}
			bool hasGLBudgetTree = PXSelect<GLBudgetTree>.Select(this).Any();
			//Hide "Show Tree" when Budget Configuration is empty
			PXUIFieldAttribute.SetVisible(sender, typeof(BudgetFilter.showTree).Name, hasGLBudgetTree);
			if (row != null)
			{
				if (!hasGLBudgetTree)
				{
					row.ShowTree = false;
				}
				PXUIFieldAttribute.SetVisible(BudgetArticles.Cache, typeof(GLBudgetLine.isGroup).Name, row.ShowTree==true);
			}
			if (row.LedgerID != null && row.CompareToLedgerId != null &&
				((Ledger)PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Current<BudgetFilter.ledgerID>>>>
				.Select(this)).BaseCuryID !=
				((Ledger)PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<BudgetFilter.ledgerID>>>>
				.Select(this, row.CompareToLedgerId)).BaseCuryID)
			{
				PXUIFieldAttribute.SetWarning<BudgetFilter.compareToLedgerID>(sender, row, Messages.BudgetDifferentCurrency);
			}
			else
			{
				PXUIFieldAttribute.SetWarning<BudgetFilter.compareToLedgerID>(sender, row, null);
			}
		}

		protected virtual void BudgetFilter_SubIDFilter_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void BudgetFilter_FinYear_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			BudgetFilter row = (BudgetFilter)e.Row;
			if (row == null) return;

			OrganizationFinYear finYear =
				PXSelect<
					OrganizationFinYear,
					Where<OrganizationFinYear.year, Equal<Required<OrganizationFinYear.year>>,
						And<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>>>>
					.SelectSingleBound(this, null, row.FinYear, PXAccess.GetParentOrganizationID(row.BranchID));
			_PeriodsInCurrentYear = finYear?.FinPeriods ?? 0;
		}

		protected virtual void BudgetFilter_ShowTree_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsImport)
			{
				e.NewValue = false;
			}
		}

		protected virtual void BudgetFilter_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			BudgetFilter row = (BudgetFilter)e.Row;
			BudgetFilter newRow = (BudgetFilter)e.NewRow;

			if ((newRow.BranchID != row.BranchID || newRow.LedgerID != row.LedgerID || newRow.FinYear != row.FinYear) && BudgetArticles.Cache.IsDirty)
			{
				newRow.BranchID = row.BranchID;
				newRow.LedgerID = row.LedgerID;
				newRow.FinYear = row.FinYear;
				BudgetArticles.Ask(Messages.BudgetPendingChangesTitle, Messages.BudgetPendingChangesMessage, MessageButtons.OK);
			}
		}

		protected virtual void BudgetFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			BudgetFilter row = (BudgetFilter)e.Row;
			BudgetFilter old = (BudgetFilter)e.OldRow;
			if (row == null) return;

			if (Filter.Current.BranchID != null && Filter.Current.LedgerID != null && Filter.Current.FinYear != null)
			{
				if (PXSelect<GLBudgetTree>.Select(this).Count != 0 && PXSelect<GLBudgetLine,
				Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
					And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
					And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this).Count == 0)
				{

					WebDialogResult result = Filter.Ask(Messages.BudgetArticlesPreloadFromConfigurationTitle, Messages.BudgetArticlesPreloadFromConfiguration, MessageButtons.YesNo);
					Filter.ClearDialog();
					if (result == WebDialogResult.Yes)
					{
						_CurrentAction = GLBudgetEntryActionType.PreloadBudgetTree; // it's need here for Persist()
						GLBudgetEntry process = PrepareGraphForLongOperation();
						PXLongOperation.StartOperation(this.UID, (PXToggleAsyncDelegate)(() => {
							process.PreloadBudgetTree();
							process._CurrentAction = GLBudgetEntryActionType.PreloadBudgetTree; // it's need here for Persist()
							process.Persist();
							//process.SelectTimeStamp();
							PXLongOperation.SetCustomInfo(process);
						}));
					}
					else
					{
						Filter.Current.FinYear = null;
					}
				}

				if (IsImport)
				{
					var flt = Filter.Current;
					if (flt.BranchID != null && flt.FinYear != null && flt.LedgerID != null &&
						(flt.BranchID != old.BranchID || flt.FinYear != old.FinYear || flt.LedgerID != old.LedgerID)
					)
					{
						PrepareIndexes();
						_BudgetArticlesIndex = null;
					}
				}
			}
			if (old.CompareToBranchID != Filter.Current.CompareToBranchID || old.CompareToLedgerId != Filter.Current.CompareToLedgerId || old.CompareToFinYear != Filter.Current.CompareToFinYear || Filter.Current.CompareToFinYear != null)
			{
				PopulateComparison(Filter.Current.CompareToBranchID, Filter.Current.CompareToLedgerId, Filter.Current.CompareToFinYear);
			}
		}
		#endregion
		#region BudgetPreloadFilter

		protected virtual void BudgetFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<BudgetFilter.ledgerID>(e.Row);
		}

		protected virtual void BudgetPreloadFilter_AccountIDFilter_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXStringState strState = (PXStringState)sender.GetStateExt(null, typeof(BudgetPreloadFilter.fromAccount).Name);
			PXDBStringAttribute.SetInputMask(sender, typeof(BudgetPreloadFilter.accountIDFilter).Name, strState.InputMask.Replace('#', 'C'));
		}

		protected virtual void BudgetPreloadFilter_AccountIDFilter_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
			{
				PXStringState strState = (PXStringState)sender.GetStateExt(null, typeof(BudgetPreloadFilter.fromAccount).Name);
				e.NewValue = ((string)e.NewValue).PadRight(strState.InputMask.Length - 1, '?').Replace(' ', '?');
			}
		}

		protected virtual void BudgetPreloadFilter_SubIDFilter_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void BudgetPreloadFilter_SubIDFilter_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PXStringState strState = (PXStringState)sender.GetStateExt(null, typeof(BudgetPreloadFilter.subIDFilter).Name);
			if (e.NewValue != null)
			{
				e.NewValue = ((string)e.NewValue).PadRight(strState.InputMask.Length - 1, '?').Replace(' ', '?');
			}
			else
			{
				e.NewValue = string.Empty;
				e.NewValue = ((string)e.NewValue).PadRight(strState.InputMask.Length - 1, '?').Replace(' ', '?');
			}
		}

		protected virtual void BudgetPreloadFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetValueExt<BudgetPreloadFilter.ledgerID>(e.Row, null);
		}

		protected virtual void BudgetPreloadFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			BudgetPreloadFilter row = (BudgetPreloadFilter)e.Row;
			if (row == null) return;
			if (Filter.Current.LedgerID != null && row.LedgerID != null
				&& ((Ledger)PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Current<BudgetFilter.ledgerID>>>>.Select(this)).BaseCuryID !=
					((Ledger)PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<BudgetPreloadFilter.ledgerID>>>>.Select(this, row.LedgerID)).BaseCuryID)
			{
				PXUIFieldAttribute.SetWarning<BudgetPreloadFilter.ledgerID>(cache, row, Messages.BudgetDifferentCurrency);
			}
			else
			{
				PXUIFieldAttribute.SetWarning<BudgetPreloadFilter.ledgerID>(cache, row, null);
			}
		}
		#endregion
		#endregion

		#region GLBudgetLine
		private bool CheckReleasedInGroup(GLBudgetLine article)
		{
			if (article.IsGroup != true) return false;

			// find childs of the group			
			foreach (GLBudgetLine line in PXSelect<GLBudgetLine,
				Where<GLBudgetLine.parentGroupID, Equal<Required<GLBudgetLine.groupID>>,
					And<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
					And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
					And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>>
				.Select(this, article.GroupID))
			{
				if (line.IsGroup == true && CheckReleasedInGroup(line) == true)
				{
					return true;
				}
				else if (line.IsGroup == false && line.Released == true)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void GLBudgetLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (_CurrentAction == GLBudgetEntryActionType.PreloadBudgetTree) return;

			GLBudgetLine article = (GLBudgetLine)e.Row;
			if (article == null) return;
			Distribute.SetEnabled(article.Comparison == null ? true : false);

			bool wasNeverReleased = article.WasReleased != true;
			bool isNotGroup = article.IsGroup != true;
			bool wasNotPreloaded = article.IsPreloaded != true;

			PXUIFieldAttribute.SetEnabled<GLBudgetLine.accountID>(sender, article, wasNeverReleased && isNotGroup && wasNotPreloaded);
			PXUIFieldAttribute.SetEnabled<GLBudgetLine.subID>(sender, article, wasNeverReleased && isNotGroup && wasNotPreloaded);
			PXUIFieldAttribute.SetEnabled<GLBudgetLine.description>(sender, article, wasNeverReleased);
			PXUIFieldAttribute.SetEnabled<GLBudgetLine.amount>(sender, article, isNotGroup);

			sender.RaiseExceptionHandling<GLBudgetLine.allocatedAmount>(article, article.AllocatedAmount, article.Amount != article.AllocatedAmount ?
				(new PXSetPropertyException(Messages.BudgetLineAmountNotEqualAllocated, PXErrorLevel.RowWarning)) : null);
		}
		protected virtual void GLBudgetLine_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
		}

		protected virtual void GLBudgetLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLBudgetLine article = (GLBudgetLine)e.Row;
			if (article == null) return;

			var detailsSelect = new PXSelect<GLBudgetLineDetail, Where<GLBudgetLineDetail.groupID, Equal<Required<GLBudgetLineDetail.groupID>>>>(this);

			//Update amounts in all parent nodes
			if (article.ParentGroupID != Guid.Empty)
			{
				GLBudgetLine oldRow = (GLBudgetLine)e.OldRow;
				RollupArticleAmount(article, article.Amount - oldRow.Amount);
			}
			if ((bool)article.IsGroup) EnsureAlloc(article);

			if (sender.ObjectsEqual<GLBudgetLine.accountID, GLBudgetLine.subID>(article, e.OldRow) == false)
			{
				if (AllocationIndex != null) // use AllocationIndex if it posible
				{
					foreach (GLBudgetLineDetail detail in AllocationIndex.GetList(article.GroupID.Value))
					{
						if (detail.AccountID != article.AccountID || detail.SubID != article.SubID)
						{
							detail.AccountID = article.AccountID;
							detail.SubID = article.SubID;
							Allocations.Update(detail);
						}
					}
				}
				else
				{
					foreach (GLBudgetLineDetail detail in detailsSelect.Select(article.GroupID))
					{
						if (detail.AccountID != article.AccountID || detail.SubID != article.SubID)
						{
							detail.AccountID = article.AccountID;
							detail.SubID = article.SubID;
							Allocations.Update(detail);
						}
					}
				}
			}
		}

		protected virtual void GLBudgetLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLBudgetLine article = (GLBudgetLine)e.Row;
			if (article.Comparison != null && (bool)article.Comparison)
			{
				e.Cancel = true;
				BudgetArticles.Cache.SetStatus(article, PXEntryStatus.Held);
			}
			if ((bool)article.IsGroup)
			{
				PXDefaultAttribute.SetPersistingCheck<GLBudgetLine.accountID>(sender, e.Row, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLBudgetLine.subID>(sender, e.Row, PXPersistingCheck.Nothing);
			}
			if (article.AccountMask == null && article.SubMask == null && article.AccountID != null && article.SubID != null)
			{
				string accountCD = AccountDefinition.getCD(article.AccountID.Value);
				string subCD = SubDefinition.getCD(article.SubID.Value);

				article.AccountMask = accountCD;
				article.SubMask = subCD;
			}
		}

		protected virtual void GLBudgetLine_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (_CurrentAction == GLBudgetEntryActionType.PreloadBudgetTree) { return; }
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (article == null) return;

			if (!(bool)article.IsGroup && article.IsUploaded != null && article.AccountID == null)
			{
				e.Cancel = true;
			}

			article.GroupID = article.GroupID ?? Guid.NewGuid();

			if (CurrentSelected.AccountID != null && CurrentSelected.AccountID != Int32.MinValue && CurrentSelected.SubID != null && CurrentSelected.SubID != Int32.MinValue)
			{
				article.Rollup = true;
			}

			if (IsNullOrEmpty(article.ParentGroupID))
			{
				article.ParentGroupID = CurrentSelected.Group ?? Guid.Empty;
			}

			if (article.GroupMask == null) article.GroupMask = new byte[0];
			if (!IsEmpty(article.ParentGroupID) && article.GroupMask.Length == 0)
			{
				GLBudgetLine parentNode = GetArticle(article.BranchID, article.LedgerID, article.FinYear, article.ParentGroupID);
				if (parentNode != null)
					article.GroupMask = parentNode.GroupMask;
			}
		}

		protected virtual void GLBudgetLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_CurrentAction == GLBudgetEntryActionType.PreloadBudgetTree) return;
			GLBudgetLine article = e.Row as GLBudgetLine;

			// foreach (GLBudgetLineDetail alloc in Allocations.Select(article.FinYear, article.GroupID)) Allocations.Update(alloc); // TODO: is it realy need?
			PXFormulaAttribute.CalcAggregate<GLBudgetLineDetail.amount>(Allocations.Cache, article);  // Calculate article.AllocatedAmount by GLBudgetLineDetail.amount 

			if (ArticleIndex != null)
				ArticleIndex.Add(article);

			//Update amounts in all parent nodes 
			if (article.ParentGroupID != Guid.Empty)
			{
				RollupArticleAmount(article, article.Amount);
				sender.Current = article;
			}
		}

		protected virtual void GLBudgetLine_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (article == null) return;

			//Released articles cannot be deleted
			if (e.ExternalCall)
			{
				if (AllocationIndex != null)
				{
					foreach (GLBudgetLineDetail alloc in AllocationIndex.GetList(article.GroupID.Value))
					{
						if (alloc.ReleasedAmount != 0m)
							throw new PXException(Messages.ReleasedBudgetArticleCanNotBeDeleted);
					}
				}
				else
				{
					foreach (GLBudgetLineDetail alloc in Allocations.Select(article.FinYear, article.GroupID))
					{
						if (alloc.ReleasedAmount != 0m)
							throw new PXException(Messages.ReleasedBudgetArticleCanNotBeDeleted);
					}
				}
			}
			//Show error when trying to delete comparison line
			if (article.Comparison != null && (bool)article.Comparison)
			{
				throw new PXException(Messages.ComparisonLinesCanNotBeDeleted);
			}

			//Update amounts in all parent nodes
			if (e.ExternalCall)
			{
				if (article.ParentGroupID != Guid.Empty)
				{
					RollupArticleAmount(article, -article.Amount);
					if (AllocationIndex != null)
					{
						foreach (GLBudgetLineDetail alloc in AllocationIndex.GetList(article.GroupID.Value))
						{
							RollupAllocation(article, int.Parse(alloc.FinPeriodID.Substring(4)), -alloc.Amount);
						}
					}
					else
					{
						foreach (GLBudgetLineDetail alloc in Allocations.Select(article.FinYear, article.GroupID))
						{
							RollupAllocation(article, int.Parse(alloc.FinPeriodID.Substring(4)), -alloc.Amount);
						}
					}
				}
			}

			//To check released child in the node
			if (e.ExternalCall && article.IsGroup == true && CheckReleasedInGroup(article))
			{
				throw new PXException(Messages.BudgetNodeDeleteMessage);
			}
		}

		protected virtual void GLBudgetLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (ArticleIndex != null)
				ArticleIndex.Delete(article);
		}

		protected virtual void GLBudgetLine_AccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (suppressIDs && article != null && article.Comparison == true)
				e.ReturnValue = null;
		}

		protected virtual void GLBudgetLine_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLBudgetLine.description>(e.Row);
		}

		protected virtual void GLBudgetLine_SubID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (suppressIDs && article != null && article.Comparison == true)
				e.ReturnValue = null;
		}

		protected virtual void GLBudgetLine_Amount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLBudgetLine article = (GLBudgetLine)e.Row;
			if (article == null) return;
			article.Released = false;
		}

		protected virtual void GLBudgetLine_AccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (_CurrentAction == GLBudgetEntryActionType.PreloadBudgetTree) return;
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (article == null) return;
			if ((article.IsGroup != null && (bool)article.IsGroup) || (article.IsPreloaded != null && (bool)article.IsPreloaded))
				e.Cancel = true;
		}

		protected virtual void GLBudgetLine_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (_CurrentAction == GLBudgetEntryActionType.PreloadBudgetTree) return;
			GLBudgetLine article = e.Row as GLBudgetLine;
			if (article == null) return;
			if (article.IsGroup != null && (bool)article.IsGroup)
				e.Cancel = true;

			string subCD = null;
			if (e.NewValue is int) 
			{
				subCD = SubDefinition.getCD((int)e.NewValue);
			}

			if (subCD != null && !MatchMask(subCD, CurrentSelected.SubMask ?? String.Empty))
			{
				throw new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetSubaccountNotAllowed), CurrentSelected.SubMask));
			}
		}
		#endregion

		#region GLBudgetLineDetail
		protected virtual void GLBudgetLineDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLBudgetLineDetail row = (GLBudgetLineDetail)e.Row;
			if (row.AccountID == null || row.SubID == null)
			{
				PXDefaultAttribute.SetPersistingCheck<GLBudgetLineDetail.accountID>(sender, e.Row, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLBudgetLineDetail.subID>(sender, e.Row, PXPersistingCheck.Nothing);
			}
		}
		protected virtual void GLBudgetLineDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLBudgetLineDetail alloc = e.Row as GLBudgetLineDetail;
			return;
		}
		protected virtual void GLBudgetLineDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			GLBudgetLineDetail alloc = e.Row as GLBudgetLineDetail;
			if (AllocationIndex != null)
				if (alloc.GroupID != null)
					AllocationIndex.Add(alloc);
		}
		protected virtual void GLBudgetLineDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			GLBudgetLineDetail alloc = e.Row as GLBudgetLineDetail;
			if (AllocationIndex != null)
				if (alloc.GroupID != null)
					AllocationIndex.Delete(alloc.GroupID.Value, alloc.FinPeriodID);
		}		
		#endregion

		#region ManageBudgetDialog
		protected virtual void ManageBudgetDialog_Message_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = PXMessages.LocalizeNoPrefix(Messages.BudgetRollbackMessage);
		}
		protected virtual void ManageBudgetDialog_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ManageBudgetDialog row = (ManageBudgetDialog)e.Row;
			if (row == null) return;
			switch (row.Method)
			{
				case ManageBudgetDialog.method.RollbackBudget:
					{
						row.Message = PXMessages.LocalizeNoPrefix(Messages.BudgetRollbackMessage);
						break;
					}
				case ManageBudgetDialog.method.ConvertBudget:
					{
						row.Message = PXMessages.LocalizeNoPrefix(Messages.BudgetConvertMessage);
						break;
					}
			}
		}
		#endregion
		#endregion

		#region internal Types definition

		protected enum GLBudgetEntryActionType : int { None = 0, PreloadBudgetTree = 1, Preload = 2, ImportExcel = 3, ConvertBudget = 4, RollbackBudget = 5 };

		protected class PXResultGLBudgetLineComparer : IEqualityComparer<PXResult<GLBudgetLine>>
		{
			public bool Equals(PXResult<GLBudgetLine> x, PXResult<GLBudgetLine> y)
			{
				GLBudgetLine _x = x;
				GLBudgetLine _y = y;
				if (Object.ReferenceEquals(x, y))
					return true;

				if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
					return false;
				return _x.GroupID == _y.GroupID;
			}

			public int GetHashCode(PXResult<GLBudgetLine> x)
			{
				if (Object.ReferenceEquals(x, null))
					return 0;

				GLBudgetLine _x = x;
				return _x.GroupID.GetHashCode();
			}
		}

		protected struct GLBudgetLineDetailKey
		{
			public int BranchID;
			public int LedgerID;
			public int AccountID;
			public int SubID;
			public string FinYear;
			public string FinPeriodID;

			public override bool Equals(object o)
			{
				if (o is GLBudgetLineDetailKey)
				{
					GLBudgetLineDetailKey d = (GLBudgetLineDetailKey)o;
					return (
						BranchID.Equals(d.BranchID) &&
						LedgerID.Equals(d.LedgerID) &&
						FinYear.Equals(d.FinYear) &&
						FinPeriodID.Equals(d.FinPeriodID) &&
						AccountID.Equals(d.AccountID) &&
						SubID.Equals(d.SubID)
					);
				}
				return false;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = 37;
					result *= 397;
					result += BranchID.GetHashCode();
					result *= 397;
					result += LedgerID.GetHashCode();
					result *= 397;
					result += FinYear.GetHashCode();
					result *= 397;
					result += FinPeriodID.GetHashCode();
					result *= 397;
					result += AccountID.GetHashCode();
					result *= 397;
					result += SubID.GetHashCode();
					return result;
				}
			}
		}

		/// <summary>
		/// Index for GLBudgetLineDetail by (GroupID, FinPeriodID) and keyfields (BranchID, LedgerID, FinYear, AccountID, SubID, FinPeriodID)
		/// </summary>
		protected class GLBudgetLineDetailIdx : IDisposable
		{
			private Dictionary<Guid, Dictionary<string, GLBudgetLineDetail>> all;
			//private Dictionary<GLBudgetLineDetailKey, Dictionary<Guid, GLBudgetLineDetail>> keys; // For future, if should be need
			private Dictionary<GLBudgetLineDetailKey, GLBudgetLineDetail> keys;

			public delegate GLBudgetLineDetail checkGLBudgetLineDetail(GLBudgetLineDetail line, GLBudgetLineDetail sourceLine = null);

			public GLBudgetLineDetailIdx() : this(100) { }
			public GLBudgetLineDetailIdx(int capacity) : this(capacity, false) { }
			public GLBudgetLineDetailIdx(bool withKeys) : this(100, withKeys) { }
			public GLBudgetLineDetailIdx(int capacity, bool withKeys)
			{
				all = new Dictionary<Guid, Dictionary<string, GLBudgetLineDetail>>(capacity);
				if(withKeys)
					keys = new Dictionary<GLBudgetLineDetailKey, GLBudgetLineDetail>(capacity);
			}
			public GLBudgetLineDetailIdx(PXResultset<GLBudgetLineDetail> allocations) : this(allocations.Count, true)
			{
				Add(allocations);
			}
			public GLBudgetLineDetailIdx(IEnumerable<GLBudgetLineDetail> allocations) : this(allocations.Count(), true)
			{
				Add(allocations);
			}

			public bool Add(GLBudgetLineDetail alloc)
			{
				Guid groupID = alloc.GroupID.Value;
				string finPeriodId = alloc.FinPeriodID;
				bool ret = false;

				Dictionary<string, GLBudgetLineDetail> i = null;
				if (!all.TryGetValue(groupID, out i))
				{
					i = new Dictionary<string, GLBudgetLineDetail>();
					all.Add(groupID, i);
				}
				i[finPeriodId] = alloc;

				if (keys != null)
				{
					GLBudgetLineDetailKey key = new GLBudgetLineDetailKey()
					{
						BranchID = alloc.BranchID ?? 0,
						LedgerID = alloc.LedgerID ?? 0,
						FinYear = alloc.FinYear,
						FinPeriodID = alloc.FinPeriodID,
						AccountID = alloc.AccountID ?? 0,
						SubID = alloc.SubID ?? 0
					};
					keys[key] = alloc;
				}
				return ret;
			}
			public void Add(IEnumerable<GLBudgetLineDetail> allocations)
			{
				foreach (GLBudgetLineDetail alloc in allocations)
					Add(alloc);
			}
			public void Add(PXResultset<GLBudgetLineDetail> allocations)
			{
				foreach (GLBudgetLineDetail alloc in allocations)
					Add(alloc);
			}

			public bool Delete(GLBudgetLineDetail alloc)
			{
				if (alloc == null) return false;
				return Delete(alloc.GroupID.Value, alloc.FinPeriodID);
			}
			public bool Delete(Guid GroupID, string FinPeriodId)
			{
				Dictionary<string, GLBudgetLineDetail> i = null;
				bool ret = false;
				if (this.all.TryGetValue(GroupID, out i))
				{
					ret = i.Remove(FinPeriodId);
					if (i.Count == 0)
					{
						this.all.Remove(GroupID);
					}
				}
				return ret;
			}

			public void Clear()
			{
				all.Clear();
				if (keys!=null)
					keys.Clear();
			}

			public IEnumerable<GLBudgetLineDetail> Get(Guid GroupID)
			{
				Dictionary<string, GLBudgetLineDetail> i;
				if (all.TryGetValue(GroupID, out i)) return i.Values;
				return null;
			}
			public IEnumerable<GLBudgetLineDetail> GetList(Guid GroupID)
			{
				Dictionary<string, GLBudgetLineDetail> i;
				if (all.TryGetValue(GroupID, out i)) return i.Values;
				return new List<GLBudgetLineDetail>();
			}
			public GLBudgetLineDetail Get(Guid GroupID, string FinPeriodId, checkGLBudgetLineDetail check = null)
			{
				Dictionary<string, GLBudgetLineDetail> i = null;
				GLBudgetLineDetail a = null;
				if (all.TryGetValue(GroupID, out i))
				{
					if (i.TryGetValue(FinPeriodId, out a))
						return a;
				}
				if (a == null && check != null)
				{
					a = check(a);
				}
				return a;
			}

			public GLBudgetLineDetail GetByKey(GLBudgetLineDetailKey key, checkGLBudgetLineDetail check = null)
			{
				if (keys == null) return null;
				GLBudgetLineDetail alloc = null;
				if (keys.TryGetValue(key, out alloc))
				{
					if (alloc != null && check != null)
						return check(alloc);
					return alloc;
				}
				return null;
			}

			public GLBudgetLineDetail GetByKey(GLBudgetLineDetail keySource, checkGLBudgetLineDetail check = null)
			{
				if (keys == null) return null;
				GLBudgetLineDetailKey key = new GLBudgetLineDetailKey()
				{
					BranchID = keySource.BranchID ?? 0,
					LedgerID = keySource.LedgerID ?? 0,
					FinYear = keySource.FinYear,
					FinPeriodID = keySource.FinPeriodID,
					AccountID = keySource.AccountID ?? 0,
					SubID = keySource.SubID ?? 0
				};
				GLBudgetLineDetail alloc = GetByKey(key, null);
				if (alloc != null && check != null)
					return check(alloc, keySource);
				return alloc;
			}

			public void Dispose()
			{
				this.Clear();
			}
		}

		protected struct GLBudgetLineKey
		{
			public int BranchID;
			public int LedgerID;
			public int AccountID;
			public int SubID;
			public string FinYear;

			public override bool Equals(object o)
			{
				if (o is GLBudgetLineKey)
				{
					GLBudgetLineKey d = (GLBudgetLineKey)o;
					return (
						BranchID.Equals(d.BranchID) &&
						LedgerID.Equals(d.LedgerID) &&
						FinYear.Equals(d.FinYear) &&
						AccountID.Equals(d.AccountID) &&
						SubID.Equals(d.SubID)
					);
				}
				return false;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = 37;
					result *= 397;
					result += BranchID.GetHashCode();
					result *= 397;
					result += LedgerID.GetHashCode();
					result *= 397;
					result += FinYear.GetHashCode();
					result *= 397;
					result += AccountID.GetHashCode();
					result *= 397;
					result += SubID.GetHashCode();
					return result;
				}
			}
		}

		/// <summary>
		/// Index for GLBudgetLine by GroupID and keyfields (BranchID, LedgerID, FinYear, AccountID, SubID)
		/// </summary>
		protected class GLBudgetLineIdx {
			private Dictionary<Guid, GLBudgetLine> all;
			private Dictionary<Guid, GLBudgetLine> groups;
			private Dictionary<GLBudgetLineKey, Dictionary<Guid, GLBudgetLine>> keys;

			public Dictionary<Guid, GLBudgetLine> All { get { return all; } }
			public Dictionary<Guid, GLBudgetLine> Groups { get { return groups; } }

			public bool IsEmpty { get { return all.Count == 0; } }

			public delegate GLBudgetLine checkGLBudgetLine(GLBudgetLine line, GLBudgetLine sourceLine=null);

			public GLBudgetLineIdx():this(100){}

			public GLBudgetLineIdx(int capacity):this(capacity, false){}
			public GLBudgetLineIdx(bool withKeys) : this(100, withKeys){}
			public GLBudgetLineIdx(int capacity, bool withKeys)
			{
				all = new Dictionary<Guid, GLBudgetLine>(capacity);
				groups = new Dictionary<Guid, GLBudgetLine>();
				if(withKeys)
					keys = new Dictionary<GLBudgetLineKey, Dictionary<Guid, GLBudgetLine>>(capacity);
			}
			public GLBudgetLineIdx(IEnumerable<PXResult<GLBudgetLine>> lines):this(lines.Count(),true)
			{
				Add(lines);
			}
			public GLBudgetLineIdx(PXResultset<GLBudgetLine> lines) : this(lines.Count, true)
			{
				Add(lines);
			}

			public GLBudgetLineIdx(IEnumerable<GLBudgetLine> lines) : this(lines.Count(), false)
			{
				Add(lines);
			}

			public bool Add(GLBudgetLine line) {
				if (line?.GroupID == null) return false;
				Guid id = line.GroupID.Value;
				all[id] = line;
				if (line.IsGroup == true)
					groups[id] = line;

				if (keys != null)
				{
					Dictionary<Guid,GLBudgetLine> dkey;
					GLBudgetLineKey key = new GLBudgetLineKey()
					{
						BranchID = line.BranchID ?? 0,
						LedgerID = line.LedgerID ?? 0,
						FinYear = line.FinYear,
						AccountID = line.AccountID ?? 0,
						SubID = line.SubID ?? 0
					};
					if (!keys.TryGetValue(key, out dkey))
					{
						dkey = new Dictionary<Guid, GLBudgetLine>();
						keys.Add(key, dkey);
					}
					dkey[id] = line;
				}
				return true;
			}
			public int Add(IEnumerable<GLBudgetLine> lines)
			{
				int count = 0;
				foreach (GLBudgetLine l in lines)
				{
					if (Add(l)) count++;
				}
				return count;
			}
			public int Add(IEnumerable<PXResult<GLBudgetLine>> lines)
			{
				int count = 0;
				foreach (GLBudgetLine l in lines)
				{
					if(Add(l)) count++;
				}
				return count; 
			}
			public int Add(PXResultset<GLBudgetLine> lines)
			{
				int count = 0;
				foreach (GLBudgetLine l in lines)
				{
					if (Add(l)) count++;
				}
				return count;
			}
			public bool Delete(Guid? GroupID)
			{
				if (GroupID == null) return false;
				Guid id = GroupID.Value;
				if (keys != null)
				{
					GLBudgetLine line;
					all.TryGetValue(id, out line);
					if (line!=null)
					{
						Dictionary<Guid, GLBudgetLine> dkey;
						GLBudgetLineKey key = new GLBudgetLineKey()
						{
							BranchID = line.BranchID ?? 0,
							LedgerID = line.LedgerID ?? 0,
							FinYear = line.FinYear,
							AccountID = line.AccountID ?? 0,
							SubID = line.SubID ?? 0
						};
						if (keys.TryGetValue(key, out dkey))
						{
							dkey.Remove(id);
							if (dkey.Count == 0)
							{
								keys.Remove(key);
							}
						}
						
					}
				}
				groups.Remove(id);
				return all.Remove(id);
			}
			public bool Delete(GLBudgetLine line)
			{
				if (line?.GroupID == null) return false;
				return Delete(line.GroupID.Value);
			}
			public void Clear()
			{
				all.Clear();
				groups.Clear();
				if(keys!=null)
					keys.Clear();
			}

			public GLBudgetLine GetGroup(Guid? GroupID, checkGLBudgetLine check = null)
			{
				if (GroupID == null) return null;
				GLBudgetLine line = null;
				groups.TryGetValue(GroupID.Value, out line);
				if (line != null && check != null)
					return check(line);

				return line;
			}
			public IEnumerable<GLBudgetLine> GetGroups()
			{
				return groups.Values;
			}
			public IEnumerable<GLBudgetLine> GetAll()
			{
				return all.Values;
			}
			public GLBudgetLine Get(Guid? GroupID, checkGLBudgetLine check=null)
			{
				if (GroupID == null) return null;
				GLBudgetLine line=null;
				all.TryGetValue(GroupID.Value, out line);
				if (line != null && check != null)
					return check(line);

				return line;
			}
			public GLBudgetLine GetByKey(GLBudgetLine keySource, checkGLBudgetLine check = null) {
				GLBudgetLineKey key = new GLBudgetLineKey()
				{
					BranchID = keySource.BranchID ?? 0,
					LedgerID = keySource.LedgerID ?? 0,
					FinYear = keySource.FinYear,
					AccountID = keySource.AccountID ?? 0,
					SubID = keySource.SubID ?? 0
				};
				GLBudgetLine line= GetByKey(key, null);
				if (line!=null && check != null)
					return check(line, keySource);
				return line;
			}
			public GLBudgetLine GetByKey(GLBudgetLineKey key, checkGLBudgetLine check = null)
			{
				if (keys == null) return null;
				Dictionary<Guid, GLBudgetLine> lines = null;
				GLBudgetLine  line=null;
				if (keys.TryGetValue(key, out lines))
				{
					line=lines.Values.FirstOrDefault();
					if (line != null && check != null)
						return check(line);
					return line;
				}
				return null;
			}
			public IEnumerable<GLBudgetLine> GetList(GLBudgetLineKey key)
			{
				if (keys == null) return null;
				Dictionary<Guid, GLBudgetLine> line = null;
				keys.TryGetValue(key, out line);
				return line.Values;
			}
		}

		private void PrepareIndexes(bool preload=true)
		{
			var flt = Filter.Current;
			if (preload && (flt.BranchID != null && flt.FinYear != null && flt.LedgerID != null))
			{
				if (_ArticleIndex?.IsEmpty ?? true)
				{
					_ArticleGroups = PXSelect<GLBudgetLine,
						Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
						And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
						And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>,
						And<GLBudgetLine.isGroup, Equal<True>>>>>>.Select(this);

					_ArticleIndex = new GLBudgetLineIdx(PXSelect<GLBudgetLine,
						Where<GLBudgetLine.branchID, Equal<Current<BudgetFilter.branchID>>,
						And<GLBudgetLine.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
						And<GLBudgetLine.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this));

					_AllocationIndex = new GLBudgetLineDetailIdx(
						PXSelect<GLBudgetLineDetail,
						Where<GLBudgetLineDetail.ledgerID, Equal<Current<BudgetFilter.ledgerID>>,
						And<GLBudgetLineDetail.branchID, Equal<Current<BudgetFilter.branchID>>,
						And<GLBudgetLineDetail.finYear, Equal<Current<BudgetFilter.finYear>>>>>>.Select(this));
				}
			}
			else
			{
				_ArticleIndex = new GLBudgetLineIdx();
				_AllocationIndex = new GLBudgetLineDetailIdx();
			}
			IndexesIsPrepared = true;
		}

		#endregion

		#region IPrefetchable
		private class SubDimensionDefinition : IPrefetchable
		{
			private bool? _validate;

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<CS.Dimension>(
					new PXDataField<CS.Dimension.validate>(),
					new PXDataFieldValue(typeof(CS.Dimension.dimensionID).Name, PXDbType.NVarChar, SubAccountAttribute.DimensionName)))
				{
					if (record != null)
						_validate = record.GetBoolean(0);
				}
			}

			public static bool Validate 
			{
				get { return PXDatabase.GetSlot<SubDimensionDefinition>(typeof(SubDimensionDefinition).FullName)?._validate == true; }
			}

			public static void Fill() 
			{
				PXDatabase.GetSlot<SubDimensionDefinition>(typeof(SubDimensionDefinition).FullName);
			}
		}

		private class AccountDefinition : IPrefetchable
		{
			private Dictionary<int, string> _IDCD = new Dictionary<int, string>();
			private Dictionary<string, int> _CDID = new Dictionary<string, int>();

			public void Prefetch()
			{
				foreach (PXDataRecord record in PXDatabase.SelectMulti<Account>(new PXDataField<Account.accountID>(),	new PXDataField<Account.accountCD>()))
				{
					int? accountID = record.GetInt32(0);
					string accountCD = record.GetString(1);
					_IDCD.Add(accountID??0, accountCD);
					_CDID.Add(accountCD, accountID??0);
				}
			}

			public static void Fill()
			{
				PXDatabase.GetSlot<AccountDefinition>(typeof(AccountDefinition).FullName, typeof(Account));
			}

			public static string getCD(int id)
			{
				string cd = null;
				var instance = PXDatabase.GetSlot<AccountDefinition>(typeof(AccountDefinition).FullName,typeof(Account));
				instance._IDCD.TryGetValue(id, out cd);
				return cd;
			}
			public static int? getID(string cd)
			{
				int id = -1;
				var instance = PXDatabase.GetSlot<AccountDefinition>(typeof(AccountDefinition).FullName, typeof(Account));
				instance._CDID.TryGetValue(cd, out id);
				return id >= 0 ?(System.Nullable<int>)id:null;
			}
		}

		protected class SubDefinition : IPrefetchable
		{
			private Dictionary<int, string> _IDCD = new Dictionary<int, string>();
			private Dictionary<string, int> _CDID = new Dictionary<string, int>();
			private static int? _defaultID;
			private static string _defaultCD;

			public static int? DefaultID 
			{ 
				get 
				{
					var instance = PXDatabase.GetSlot<SubDefinition>(typeof(SubDefinition).FullName);
					return _defaultID; 
				} 
			}
			public static string DefaultCD 
			{ 
				get 
				{
					var instance = PXDatabase.GetSlot<SubDefinition>(typeof(SubDefinition).FullName);
					return _defaultCD; 
				} 
			}

			public void Prefetch()
			{
				foreach (PXDataRecord record in PXDatabase.SelectMulti<Sub>(new PXDataField<Sub.subID>(), new PXDataField<Sub.subCD>()))
				{
					int? subID = record.GetInt32(0);
					string subCD = record.GetString(1);
					_IDCD.Add(subID ?? 0, subCD);
					_CDID.Add(subCD, subID ?? 0);
				}
				_defaultID = SubAccountAttribute.TryGetDefaultSubID();
				_IDCD.TryGetValue(_defaultID??0, out _defaultCD);
			}

			public static void Fill()
			{
				PXDatabase.GetSlot<SubDefinition>(typeof(SubDefinition).FullName, typeof(Sub));
			}

			public static string getCD(int id)
			{
				string cd = null;
				var instance = PXDatabase.GetSlot<SubDefinition>(typeof(SubDefinition).FullName, typeof(Sub));
				instance._IDCD.TryGetValue(id, out cd);
				return cd;
			}
			public static int? getID(string cd)
			{
				int id = -1;
				var instance = PXDatabase.GetSlot<SubDefinition>(typeof(SubDefinition).FullName, typeof(Sub));
				instance._CDID.TryGetValue(cd, out id);
				return id >= 0 ? (System.Nullable<int>)id : null;
			}
		}
		#endregion
	}

	[Serializable]
	[PXHidden]
	public partial class SelectedGroup : IBqlTable
	{
		#region Group
		public abstract class group : PX.Data.BQL.BqlGuid.Field<group> { }
		[PXDBGuid(IsKey = true)]
		public virtual Guid? Group	{ get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXInt()]
		public virtual Int32? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[PXInt()]
		public virtual int? SubID { get; set; }
		#endregion
		#region AccountMask
		public abstract class accountMask : PX.Data.BQL.BqlString.Field<accountMask> { };
		[PXDBString(10, IsUnicode = true)]
		public virtual string AccountMask { get; set; }
		#endregion
		#region SubMask
		public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual string SubMask { get; set; }
		#endregion
		#region AccountMask Wildcard
		public const string WildcardAnything = "%";

		public abstract class accountMaskWildcard : PX.Data.BQL.BqlString.Field<accountMaskWildcard> { };
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(WildcardAnything)]
		public virtual string AccountMaskWildcard { get; set; }
		#endregion
		#region SubMask Wildcard
		public abstract class subMaskWildcard : PX.Data.BQL.BqlString.Field<subMaskWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		[PXDefault(WildcardAnything)]
		public virtual string SubMaskWildcard { get; set; }
		#endregion
	}

	[Serializable]
	public partial class BudgetDistributeFilter : IBqlTable
	{
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
				new[] { Evenly, PreviousYear, ComparedValues },
				new[] { Messages.Evenly, Messages.PreviousYear, Messages.ComparedValues }) { }
			}

			public class NonComparedListAttribute : PXStringListAttribute
			{
				public NonComparedListAttribute()
					: base(
				new[] { Evenly, PreviousYear },
				new[] { Messages.Evenly, Messages.PreviousYear }) { }
			}

			public const string Evenly = "E";
			public const string PreviousYear = "P";
			public const string ComparedValues = "C";

			public class evenly : PX.Data.BQL.BqlString.Constant<evenly>
			{
				public evenly() : base(Evenly) { }
			}
			public class previousYear : PX.Data.BQL.BqlString.Constant<previousYear>
			{
				public previousYear() : base(PreviousYear) { }
			}
			public class comparedValues : PX.Data.BQL.BqlString.Constant<comparedValues>
			{
				public comparedValues() : base(ComparedValues) { }
			}
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(method.Evenly)]
		[method.List]
		[PXUIField(DisplayName = "Distribution Method")]
		public virtual String Method { get; set; }
		#endregion
		#region ApplyToAll
		public abstract class applyToAll : PX.Data.BQL.BqlBool.Field<applyToAll> { }
		[PXUIField(DisplayName = "Apply to All Articles in This Node")]
		[PXBool]
		public virtual bool? ApplyToAll { get; set; }
		#endregion
		#region ApplyToSubGroups
		public abstract class applyToSubGroups : PX.Data.BQL.BqlBool.Field<applyToSubGroups> { }
        [PXUIField(DisplayName = "Apply to Subarticles", Enabled = false)]
		[PXBool]
		public virtual bool? ApplyToSubGroups { get; set; }
		#endregion
	}

	[Serializable]
	public partial class BudgetPreloadFilter : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(BudgetFilter.branchID), Required = true)]
		public virtual int? BranchID { get; set; }
		#endregion

		#region LedgerId
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Ledger", Required = true)]
		[PXSelector(typeof(
				Search5<Ledger.ledgerID,
					LeftJoin<OrganizationLedgerLink,
						On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
					LeftJoin<Branch,
							On<Branch.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
					Where2<Where<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
								And<Branch.branchID, Equal<Current2<BudgetPreloadFilter.branchID>>,
								And<Current2<BudgetPreloadFilter.branchID>, IsNotNull>>>,
							Or<Ledger.balanceType, Equal<LedgerBalanceType.budget>>>,
					Aggregate<GroupBy<Ledger.ledgerID>>>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			DescriptionField = typeof(Ledger.descr))]
		[PXDefault(typeof(BudgetFilter.compareToLedgerID))]
		public virtual int? LedgerID { get; set; }
		#endregion

		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		[PXUIField(DisplayName = "Financial Year", Required = true)]
		[PXDBString(4)]
		[GenericFinYearSelector(typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.year>>>),
			null,
			branchSourceType: typeof(BudgetPreloadFilter.branchID))]
		[PXDefault(typeof(BudgetFilter.compareToFinYear))]
		public virtual String FinYear { get; set; }
		#endregion

		#region ChangePercent
		public abstract class changePercent : PX.Data.BQL.BqlShort.Field<changePercent> { }
		[PXShort()]
		[PXDefault((short)100)]
		[PXUIField(DisplayName = "Multiplier (in %)", Required = true)]
		public virtual short? ChangePercent { get; set; }
		#endregion

		#region FromAccount
		public abstract class fromAccount : PX.Data.BQL.BqlInt.Field<fromAccount> { }
		[PXDBInt]
		[PXDimensionSelector(AccountAttribute.DimensionName, (typeof(Search<Account.accountID, Where<Account.accountCD, Like<Current<SelectedGroup.accountMaskWildcard>>>, OrderBy<Asc<Account.accountCD>>>)), typeof(Account.accountCD), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account From", Visibility = PXUIVisibility.Visible)]
		public virtual int? FromAccount { get; set; }
		#endregion

		#region ToAccount
		public abstract class toAccount : PX.Data.BQL.BqlInt.Field<toAccount> { }
		[PXDBInt]
		[PXDimensionSelector(AccountAttribute.DimensionName, (typeof(Search<Account.accountID, Where<Account.accountCD, Like<Current<SelectedGroup.accountMaskWildcard>>>, OrderBy<Asc<Account.accountCD>>>)), typeof(Account.accountCD), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account To", Visibility = PXUIVisibility.Visible)]
		public virtual int? ToAccount { get; set; }
		#endregion

		#region AccountIDFilter
		public abstract class accountIDFilter : PX.Data.BQL.BqlString.Field<accountIDFilter> { }
		[AccountRaw(DisplayName = "Account Mask")]
		public virtual string AccountIDFilter { get; set; }
		#endregion
		
		#region SubIDFilter
		public abstract class subIDFilter : PX.Data.BQL.BqlString.Field<subIDFilter> { }
		[SubAccountRaw(DisplayName = "Subaccount Mask")]
		public virtual string SubIDFilter { get; set; }
		#endregion

		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual string SubCDWildcard
		{
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this.SubIDFilter, SubAccountAttribute.DimensionName);
			}
		}
		#endregion

		#region AccountCD Wildcard
		public abstract class accountCDWildcard : PX.Data.BQL.BqlString.Field<accountCDWildcard> { };
		[PXDBString(10, IsUnicode = true)]
		public virtual string AccountCDWildcard { get; set; }
		#endregion

		#region PreloadAction
		[PXUIField()]
		[PXDefault((short)2)]
		[PXShort]
		public short? PreloadAction
		{
			get { return this._PreloadAction; }
			set { if (value != null) this._PreloadAction = value; }
		}
		protected short? _PreloadAction;
		#endregion

		#region Strategy
		public abstract class strategy : PX.Data.BQL.BqlString.Field<strategy>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
				new[] { UpdateExisting, UpdateAndLoad, LoadNotExisting },
				new[] { Messages.UpdateExisting, Messages.UpdateAndLoad, Messages.LoadNotExisting }) { }
			}

			public const string ReloadAll = "R";
			public const string UpdateExisting = "U";
			public const string UpdateAndLoad = "F";
			public const string LoadNotExisting = "L";

			public class reloadAll : PX.Data.BQL.BqlString.Constant<reloadAll>
			{
				public reloadAll() : base(ReloadAll) { }
			}
			public class updateExisting : PX.Data.BQL.BqlString.Constant<updateExisting>
			{
				public updateExisting() : base(UpdateExisting) { }
			}
			public class updateAndLoad : PX.Data.BQL.BqlString.Constant<updateAndLoad>
			{
				public updateAndLoad() : base(UpdateAndLoad) { }
			}
			public class loadNotExisting : PX.Data.BQL.BqlString.Constant<loadNotExisting>
			{
				public loadNotExisting() : base(LoadNotExisting) { }
			}
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(strategy.UpdateAndLoad)]
		[strategy.List]
		public virtual String Strategy { get; set; }
		#endregion
	}

	[Serializable]
	public partial class BudgetFilter : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(Required = true)]
		public virtual int? BranchID { get; set; }
		#endregion

		#region LedgerId
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Ledger", Required = true)]
		[PXSelector(typeof(
				Search5<Ledger.ledgerID,
					LeftJoin<OrganizationLedgerLink,
						On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
						LeftJoin<Branch,
							On<Branch.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
					Where2<Where<Ledger.balanceType, Equal<LedgerBalanceType.budget>,
							Or<Ledger.balanceType, Equal<LedgerBalanceType.statistical>>>,
						And<Branch.branchID, Equal<Current2<BudgetFilter.branchID>>>>,
					Aggregate<GroupBy<Ledger.ledgerID>>>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			DescriptionField = typeof(Ledger.descr))]
		[PXDefault(typeof(
				Search5<Ledger.ledgerID,
				LeftJoin<OrganizationLedgerLink,
					On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
					LeftJoin<Branch,
						On<Branch.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
				Where2<Where<Ledger.balanceType, Equal<LedgerBalanceType.budget>,
						Or<Ledger.balanceType, Equal<LedgerBalanceType.statistical>>>,
					And<Branch.branchID, Equal<Current2<BudgetFilter.branchID>>>>,
				Aggregate<GroupBy<Ledger.ledgerID>>>))]
		public virtual int? LedgerID { get; set; }
		#endregion

		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		[PXUIField(DisplayName = "Financial Year", Required = true)]
		[PXDBString(4)]
		[GenericFinYearSelector(typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.year>>>),
			null,
			branchSourceType: typeof(BudgetFilter.branchID))]
		public virtual String FinYear { get; set; }
		#endregion

		#region ShowTree
		public abstract class showTree : PX.Data.BQL.BqlBool.Field<showTree> { }
		[PXBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Tree View")]
		public virtual Boolean? ShowTree { get; set; }
		#endregion

		#region CompareToBranchID
		public abstract class compareToBranchID : PX.Data.BQL.BqlInt.Field<compareToBranchID> { }
		[Branch(DisplayName = "Compare to Branch", Required = false)]
		public virtual int? CompareToBranchID { get; set; }
		#endregion

		#region CompareToLedgerID
		public abstract class compareToLedgerID : PX.Data.BQL.BqlInt.Field<compareToLedgerID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Compare to Ledger")]
		[PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, NotEqual<LedgerBalanceType.report>>>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			DescriptionField = typeof(Ledger.descr))]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? CompareToLedgerId { get; set; }
		#endregion

		#region CompareToFinYear
		public abstract class compareToFinYear : PX.Data.BQL.BqlString.Field<compareToFinYear> { }
		[PXUIField(DisplayName = "Compare to Year")]
		[PXDBString(4)]
		[GenericFinYearSelector(typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.year>>>),
			null,
			branchSourceType: typeof(BudgetFilter.compareToBranchID),
			useMasterOrganizationIDByDefault: true)]
		public virtual string CompareToFinYear { get; set; }
		#endregion

		#region SubIDFilter
		public abstract class subIDFilter : PX.Data.BQL.BqlString.Field<subIDFilter> { }
		[SubAccountRaw(DisplayName = "Subaccount Filter")]
		public virtual string SubIDFilter { get; set; }
		#endregion

		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual string SubCDWildcard
		{
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this.SubIDFilter, SubAccountAttribute.DimensionName);
			}
		}
		#endregion

		#region Tree Node Filter
		public abstract class treeNodeFilter : PX.Data.BQL.BqlString.Field<treeNodeFilter> { };
		[PXUIField(DisplayName = "Tree Node Filter")]
		[PXDBString(30, IsUnicode = true)]
		public virtual string TreeNodeFilter { get; set; }
		#endregion

		public short? Precision;
	}

	[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
	public class BudgetLedger : PX.Data.BQL.BqlString.Constant<BudgetLedger>
	{
		public BudgetLedger() : base("B"){}
	}

	[Serializable]
	public partial class ManageBudgetDialog : IBqlTable
	{
		#region Message
		[PXString(IsUnicode = true)]
		[PXUIField(Enabled = false)]
		public virtual String Message { get; set; }
		#endregion
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
				new[] { RollbackBudget, ConvertBudget },
				new[] { Messages.BudgetRollback, Messages.BudgetConvert }) { }
			}

			public const string RollbackBudget = "R";
			public const string ConvertBudget = "C";

			public class rollbackBudget : PX.Data.BQL.BqlString.Constant<rollbackBudget>
			{
				public rollbackBudget() : base(RollbackBudget) { }
			}
			public class convertBudget : PX.Data.BQL.BqlString.Constant<convertBudget>
			{
				public convertBudget() : base(ConvertBudget) { }
			}
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(method.RollbackBudget)]
		[method.List]
		[PXUIField(DisplayName = Messages.BudgetManageAction)]
		public virtual String Method { get; set; }
		#endregion
	}
}
