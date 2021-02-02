using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PX.SM;
using PX.Data;

namespace PX.Objects.GL
{
	public class GLBudgetTreeMaint : PXGraph<GLBudgetTreeMaint>
	{
		public PXSelectOrderBy<GLBudgetTree, OrderBy<Asc<GLBudgetTree.sortOrder>>> Details;
		public PXSelect<GLBudgetTree, Where<GLBudgetTree.parentGroupID, Equal<Argument<Guid?>>>, OrderBy<Asc<GLBudgetTree.sortOrder>>> Tree;
		public PXFilter<AccountsPreloadFilter> PreloadFilter;
		public PXSetup<GLSetup> GLSetup;

		bool SubEnabled = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.subAccount>();
		
		public GLBudgetTreeMaint()
		{
			GLSetup setup = GLSetup.Current;

		}

		protected virtual IEnumerable tree(
			[PXGuid]
			Guid? groupID
		)
		{
			if (groupID == null)
			{
				yield return new GLBudgetTree()
				{
					GroupID = Guid.Empty,
					Description = PXSiteMap.RootNode.Title
				};

			}

			foreach (GLBudgetTree item in PXSelect<GLBudgetTree,
			 Where<GLBudgetTree.parentGroupID,
				Equal<Required<GLBudgetTree.groupID>>>>.Select(this, groupID))
			{
				if (!string.IsNullOrEmpty(item.Description) && (bool)item.IsGroup && SearchForMatchingChild(item.GroupID))
				{
					yield return item;
				}
			}
		}

		protected virtual IEnumerable details(
			[PXGuid]
			Guid? groupID
		)
		{
			if (groupID == null)
				groupID = this.Tree.Current != null
								? this.Tree.Current.GroupID
								: Guid.Empty;

			this.CurrentSelected.Group = groupID;

			GLBudgetTree parentNode = PXSelect<GLBudgetTree, Where<GLBudgetTree.groupID, Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group);
			if (parentNode != null)
			{
				this.CurrentSelected.AccountID = parentNode.AccountID != null ? parentNode.AccountID : Int32.MinValue;
				this.CurrentSelected.SubID = parentNode.SubID != null ? parentNode.SubID : Int32.MinValue;
				this.CurrentSelected.GroupMask = parentNode.GroupMask;
			}
			else
			{
				this.CurrentSelected.AccountID = Int32.MinValue;
				this.CurrentSelected.SubID = Int32.MinValue;
				this.CurrentSelected.GroupMask = null;
			}
			this.CurrentSelected.AccountMaskWildcard = SubCDUtils.CreateSubCDWildcard(parentNode != null ? parentNode.AccountMask : string.Empty, AccountAttribute.DimensionName);
			this.CurrentSelected.SubMaskWildcard = parentNode != null ? parentNode.SubMask : string.Empty;

			List<GLBudgetTree> nodes = new List<GLBudgetTree>();
			foreach (GLBudgetTree node in PXSelect<GLBudgetTree,
				Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>,
				And<GLBudgetTree.parentGroupID, NotEqual<GLBudgetTree.groupID>,
				And<Match<Current<AccessInfo.userName>>>>>>.Select(this, groupID))
			{
				if (node.GroupMask != null)
				{
					foreach (byte b in node.GroupMask)
					{
						if (b != 0x00)
						{
							node.Secured = true;
						}
					}
				}
				nodes.Add(node);
			}

			if (PXSelect<GLBudgetTree, Where<GLBudgetTree.groupID, Equal<Required<GLBudgetTree.groupID>>, And<Match<Current<AccessInfo.userName>>>>>.Select(this, groupID).Count == 0 && groupID != Guid.Empty)
			{
				Details.Cache.AllowInsert = false;
				Details.Cache.AllowDelete = false;
				Details.Cache.AllowUpdate = false;
			}
			else
			{
				Details.Cache.AllowInsert = true;
				Details.Cache.AllowDelete = true;
				Details.Cache.AllowUpdate = true;
			}

			return nodes;
		}

		private SelectedNode CurrentSelected
		{
			get
			{
				PXCache cache = this.Caches[typeof(SelectedNode)];
				if (cache.Current == null)
				{
					cache.Insert();
					cache.IsDirty = false;
				}
				return (SelectedNode)cache.Current;
			}
		}

		private bool SearchForMatchingChild(Guid? GroupID)
		{
			PXResultset<GLBudgetTree> childGroups = PXSelect<GLBudgetTree, Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>, And<GLBudgetTree.isGroup, Equal<True>>>>.Select(this, GroupID);
			if (PXSelect<GLBudgetTree, Where<GLBudgetTree.groupID, Equal<Required<GLBudgetTree.groupID>>, And<Match<Current<AccessInfo.userName>>>>>.Select(this, GroupID).Count == 0)
			{
				if (PXSelect<GLBudgetTree, Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>, And<Match<Current<AccessInfo.userName>>>>>.Select(this, GroupID).Count == 0)
				{
					foreach (GLBudgetTree childGroup in childGroups)
					{
						if (SearchForMatchingChild(childGroup.GroupID)) return true;
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

		#region Actions
		public PXSave<GLBudgetTree> Save;
		public PXCancel<GLBudgetTree> Cancel;
		public PXAction<GLBudgetTree> ShowPreload;
		public PXAction<GLBudgetTree> Preload;
		public PXAction<GLBudgetTree> ConfigureSecurity;
		public PXDelete<GLBudgetTree> delete;

		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
		public virtual IEnumerable Delete(PXAdapter adapter)
		{
			GLBudgetTree lineCopy = PXCache<GLBudgetTree>.CreateCopy(Details.Current);
			if (Details.Current.IsGroup == true && Details.Current.GroupID != Details.Current.ParentGroupID)
			{
				if (Details.Ask(Messages.BudgetTreeDeleteGroupTitle, Messages.BudgetTreeDeleteGroupMessage, MessageButtons.YesNo) == WebDialogResult.Yes)
				{
					deleteRecords(Details.Current.GroupID);
					Details.Cache.Delete(lineCopy);
				}
			}
			else
			{
				Details.Cache.Delete(lineCopy);
			}
			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = Messages.PreloadArticlesTree, MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable showPreload(PXAdapter adapter)
		{
			GLBudgetTree parentNode = PXSelect<GLBudgetTree, Where<GLBudgetTree.groupID, Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group);

			PXStringState subStrState = (PXStringState)Details.Cache.GetStateExt(null, typeof(GLBudgetTree.subID).Name);
			string subWildCard = new String('?', subStrState.InputMask.Length - 1);
			
			if (parentNode != null)
			{
				PreloadFilter.Current.AccountCDWildcard = SubCDUtils.CreateSubCDWildcard(parentNode != null ? parentNode.AccountMask : string.Empty, AccountAttribute.DimensionName);
				if (parentNode.AccountMask != null)
				{
					Account AcctFrom = PXSelect<Account, Where<Account.active, Equal<True>,
						And<Account.accountCD, Like<Required<SelectedNode.accountMaskWildcard>>>>,
						OrderBy<Asc<Account.accountCD>>>.SelectWindowed(this, 0, 1, PreloadFilter.Current.AccountCDWildcard);
					Account AcctTo = PXSelect<Account, Where<Account.active, Equal<True>,
						And<Account.accountCD, Like<Required<AccountsPreloadFilter.accountCDWildcard>>>>,
						OrderBy<Desc<Account.accountCD>>>.SelectWindowed(this, 0, 1, PreloadFilter.Current.AccountCDWildcard);
					PreloadFilter.Current.FromAccount = AcctFrom != null ? AcctFrom.AccountID : null;
					PreloadFilter.Current.ToAccount = AcctTo != null ? AcctTo.AccountID : null;
				}
				else
				{
					PreloadFilter.Current.FromAccount = null;
					PreloadFilter.Current.ToAccount = null;
				}
				PreloadFilter.Current.SubIDFilter = parentNode.SubMask ?? subWildCard;
			}
			else
			{
				PreloadFilter.Current.FromAccount = null;
				PreloadFilter.Current.ToAccount = null;
				PreloadFilter.Current.SubIDFilter = subWildCard;
			}
			if (PreloadFilter.Current != null)
			{
				if (parentNode != null)
				{
					if (parentNode.AccountMask != null && PreloadFilter.Current.FromAccount == null)
					{
						if (Details.Ask(Messages.BudgetTreePreloadArticlesTitle, String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetTreePreloadArticlesMessage), parentNode.AccountMask), MessageButtons.OK) == WebDialogResult.OK)
						{
							return adapter.Get();
						}
					}
				}
			}
			Details.AskExt();
			return adapter.Get();
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable preload(PXAdapter adapter)
		{
            if (PreloadFilter.Current.FromAccount == null)
			{
				PreloadFilter.Cache.RaiseExceptionHandling<AccountsPreloadFilter.fromAccount>(PreloadFilter.Current, PreloadFilter.Current.FromAccount, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				PreloadFilter.Cache.RaiseExceptionHandling<AccountsPreloadFilter.toAccount>(PreloadFilter.Current, PreloadFilter.Current.ToAccount, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
				return adapter.Get();
			}
			GLBudgetTree currentItem = ((GLBudgetTree)PXSelect<GLBudgetTree,
				Where<GLBudgetTree.groupID,
				Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group));
			PreloadFilter.Current.AccountCDWildcard = SubCDUtils.CreateSubCDWildcard(currentItem != null ? currentItem.AccountMask : string.Empty, AccountAttribute.DimensionName);

			GLBudgetTree LastItem = PXSelect<GLBudgetTree,
				Where<GLBudgetTree.parentGroupID,
				Equal<Required<GLBudgetTree.parentGroupID>>>, OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, this.CurrentSelected.Group);

			int LastSortNum = LastItem == null ? 1 : LastItem.SortOrder.Value + 1;
			bool noLinesPreloaded = true;
            List<GLBudgetTree> nodesToInsert = new List<GLBudgetTree>();
            foreach (PXResult<Account> account in PXSelect<Account, Where<Account.accountCD,
				GreaterEqual<Required<Account.accountCD>>,
				And<Account.active, Equal<True>,
				And<Account.accountCD, LessEqual<Required<Account.accountCD>>,
                And<Account.accountID, NotEqual<Current<GL.GLSetup.ytdNetIncAccountID>>>>>>>.Select(this,
					PreloadFilter.Current.FromAccount != null ?
					((Account)PXSelect<Account, Where<Account.accountID, Equal<Current<AccountsPreloadFilter.fromAccount>>>>.Select(this)).AccountCD :
					((Account)PXSelect<Account>.Select(this).First()).AccountCD,
					PreloadFilter.Current.ToAccount != null ?
					((Account)PXSelect<Account, Where<Account.accountID, Equal<Current<AccountsPreloadFilter.toAccount>>>>.Select(this)).AccountCD :
					((Account)PXSelect<Account>.Select(this).Last()).AccountCD))
			{
				foreach (PXResult<Sub> sub in PXSelect<Sub, Where<Sub.active, Equal<True>, And<Sub.subCD, Like<Current<AccountsPreloadFilter.subCDWildcard>>>>>.Select(this))
				{
					Account acct = account;
					Sub subAcct = sub;
					GLBudgetTree group = new GLBudgetTree();
					if (currentItem == null || acct.AccountID != currentItem.AccountID || subAcct.SubID != currentItem.SubID)
					{
						group.AccountID = acct.AccountID;
						group.SubID = subAcct.SubID;

						group.SortOrder = LastSortNum++;

						group.GroupMask = CurrentSelected.GroupMask;

                        nodesToInsert.Add(group);
						noLinesPreloaded = false;
					}
				}
			}
			if (noLinesPreloaded)
			{
				if (Details.Ask(Messages.BudgetTreePreloadArticlesTitle, Messages.BudgetTreePreloadArticlesNothingToPreload, MessageButtons.OK) == WebDialogResult.OK)
				{
					return adapter.Get();
				}
			}
            if (nodesToInsert.Count > 500)
            {
                if (Details.Ask(Messages.Confirmation, String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetTreePreloadArticlesTooManyMessage), nodesToInsert.Count), MessageButtons.OKCancel) == WebDialogResult.OK)
                {
					InsertPreloadedNodes(nodesToInsert);
                }
            }
            else
            {
				InsertPreloadedNodes(nodesToInsert);
            }
			return adapter.Get();
		}

		protected virtual void InsertPreloadedNodes(IEnumerable<GLBudgetTree> nodes)
		{
			bool hadError = false;

			foreach (GLBudgetTree node in nodes)
			{
				try
                {
                    Details.Insert(node);
                }
				catch (PXSetPropertyException)
				{
					hadError = true;
				}
            }

			if (hadError)
				throw new PXException(Messages.SubaccountSegmentValuesWereDeactivatedWontLoad);
		}

		[PXLookupButton]
		[PXUIField(DisplayName = Messages.ConfigureSecurity, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable configureSecurity(PXAdapter adapter)
		{
			GLBudgetTree group = PXSelect<GLBudgetTree,
				Where<GLBudgetTree.groupID, Equal<Current<GLBudgetTree.groupID>>>>.SelectSingleBound(this, new object[] { Details.Current });
			if (group != null)
			{
				GLAccessByBudgetNode graph = CreateInstance<GLAccessByBudgetNode>();
				graph.BudgetTree.Current = group;
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			else
			{
				throw new PXException(Messages.BudgetTreeNodeNotFound);
			}
		}

		public PXAction<GLBudgetTree> left;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowLeft)]
		public virtual IEnumerable Left(PXAdapter adapter)
		{
			GLBudgetTree currentItem = ((GLBudgetTree)PXSelect<GLBudgetTree,
					Where<GLBudgetTree.groupID,
					Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group));
			if (currentItem != null)
			{
				GLBudgetTree parentItem = PXSelect<GLBudgetTree,
					 Where<GLBudgetTree.isGroup, Equal<True>,
					  And<GLBudgetTree.groupID, Equal<Required<GLBudgetTree.groupID>>>>,
					 OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, currentItem.ParentGroupID);
				if (parentItem != null)
				{
					GLBudgetTree lastItem = PXSelect<GLBudgetTree,
					Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>>,
					OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, parentItem.ParentGroupID);
					if (lastItem != null)
					{
						currentItem.ParentGroupID = lastItem.ParentGroupID;
						currentItem.SortOrder = lastItem.SortOrder + 1;
						Tree.Update(currentItem);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<GLBudgetTree> right;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowRight)]
		public virtual IEnumerable Right(PXAdapter adapter)
		{
			GLBudgetTree currentItem = ((GLBudgetTree)PXSelect<GLBudgetTree,
					Where<GLBudgetTree.groupID,
					Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group));
			
			if (currentItem != null)
			{
				GLBudgetTree nextItem = PXSelect<GLBudgetTree,
					 Where<GLBudgetTree.isGroup, Equal<True>,
					 And<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>,
					 And<GLBudgetTree.sortOrder, Less<Required<GLBudgetTree.sortOrder>>>>>,
					 OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, currentItem.ParentGroupID, currentItem.SortOrder);
				if (nextItem != null)
				{
					if ((bool)nextItem.IsGroup && nextItem.AccountMask != null && nextItem.SubMask != null)
					{
						if (Details.Ask(Messages.BudgetTreeCannotMoveGroupTitle, Messages.BudgetTreeCannotMoveGroupAggregatingArticle, MessageButtons.OK) == WebDialogResult.OK)
						{
							return adapter.Get();
						}
					}
					GLBudgetTree lastChildItem = PXSelect<GLBudgetTree,
					Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>>,
					OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, nextItem.GroupID);
					if (lastChildItem != null)
					{
						currentItem.ParentGroupID = lastChildItem.ParentGroupID;
						currentItem.SortOrder = lastChildItem.SortOrder + 1;
						Tree.Update(currentItem);
					}
					else
					{
						currentItem.ParentGroupID = nextItem.GroupID;
						currentItem.SortOrder = 1;
						Tree.Update(currentItem);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<GLBudgetTree> up;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
		public virtual IEnumerable Up(PXAdapter adapter)
		{
			GLBudgetTree currentItem = ((GLBudgetTree)PXSelect<GLBudgetTree,
					Where<GLBudgetTree.groupID,
					Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group));
			if (currentItem != null)
			{
				GLBudgetTree nextItem = PXSelect<GLBudgetTree,
					 Where<GLBudgetTree.isGroup, Equal<True>,
					 And<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>,
					 And<GLBudgetTree.sortOrder, Less<Required<GLBudgetTree.sortOrder>>>>>,
					 OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, currentItem.ParentGroupID, currentItem.SortOrder);
				if (nextItem != null)
				{
					int orderNbr = currentItem.SortOrder.Value;
					currentItem.SortOrder = nextItem.SortOrder;
					Tree.Update(currentItem);
					nextItem.SortOrder = orderNbr;
					Tree.Update(nextItem);
				}
			}
			return adapter.Get();
		}

		public PXAction<GLBudgetTree> down;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
		public virtual IEnumerable Down(PXAdapter adapter)
		{
			GLBudgetTree currentItem = ((GLBudgetTree)PXSelect<GLBudgetTree,
					Where<GLBudgetTree.groupID,
					Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group));
			if (currentItem != null)
			{
				GLBudgetTree nextItem = PXSelect<GLBudgetTree,
					 Where<GLBudgetTree.isGroup, Equal<True>,
					 And<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>,
					 And<GLBudgetTree.sortOrder, Greater<Required<GLBudgetTree.sortOrder>>>>>,
					 OrderBy<Asc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, currentItem.ParentGroupID, currentItem.SortOrder);
				if (nextItem != null)
				{
					int orderNbr = currentItem.SortOrder.Value;
					currentItem.SortOrder = nextItem.SortOrder;
					Tree.Update(currentItem);
					nextItem.SortOrder = orderNbr;
					Tree.Update(nextItem);
				}
			}
			return adapter.Get();
		}

		public PXAction<GLBudgetTree> deleteGroup;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
		public virtual IEnumerable DeleteGroup(PXAdapter adapter)
		{
			if (Details.Ask(Messages.BudgetTreeDeleteGroupTitle, Messages.BudgetTreeDeleteGroupMessage, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
				if (Details.Current.GroupID != Details.Current.ParentGroupID)
				{
					deleteRecords(this.CurrentSelected.Group);
				}
				Tree.Cache.Delete((GLBudgetTree)PXSelect<GLBudgetTree,
					 Where<GLBudgetTree.groupID,
						Equal<Required<GLBudgetTree.groupID>>>>.Select(this, this.CurrentSelected.Group));
			}
			return adapter.Get();
		}

		#endregion

		private void deleteRecords(Guid? groupID)
		{
			foreach (GLBudgetTree item in PXSelect<GLBudgetTree,
				 Where<GLBudgetTree.parentGroupID,
					Equal<Required<GLBudgetTree.parentGroupID>>>>.Select(this, groupID))
			{
				if (item.GroupID != item.ParentGroupID && item.IsGroup == true)
				{
				deleteRecords(item.GroupID);
				}
				Tree.Cache.Delete(item);
			}
		}

		#region Events
		
		protected virtual void GLBudgetTree_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			GLBudgetTree row = (GLBudgetTree)e.Row;
			if (row == null) return;
			if (row.IsGroup != null)
			{
				PXUIFieldAttribute.SetEnabled<GLBudgetTree.accountID>(cache, row, !(bool)row.IsGroup);
				PXUIFieldAttribute.SetEnabled<GLBudgetTree.subID>(cache, row, !(bool)row.IsGroup);
                if ((row.Rollup != null && (bool)row.Rollup && !(bool)row.IsGroup && row.AccountID != null && row.SubID != null))
                {
                    PXUIFieldAttribute.SetEnabled<GLBudgetTree.isGroup>(cache, row, false);
                    PXUIFieldAttribute.SetEnabled<GLBudgetTree.accountMask>(cache, row, false);
                    PXUIFieldAttribute.SetEnabled<GLBudgetTree.subMask>(cache, row, false);
                }
                if ((bool)row.IsGroup)
                {
                    //Disable Account and Subaccount mask fields if node has child node
                    if ((PXSelect<GLBudgetTree, Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>>>.Select(this, row.GroupID).Count > 0))
                    {
                        PXUIFieldAttribute.SetEnabled<GLBudgetTree.accountMask>(cache, row, false);
                        PXUIFieldAttribute.SetEnabled<GLBudgetTree.subMask>(cache, row, false);
                    }
                    else
                    {
                        PXUIFieldAttribute.SetEnabled<GLBudgetTree.accountMask>(cache, row, true);
                        PXUIFieldAttribute.SetEnabled<GLBudgetTree.subMask>(cache, row, true);
                    }
                }
			}
			PXUIFieldAttribute.SetEnabled<GLBudgetTree.isGroup>(cache, row, !((row.AccountID != null || row.SubID != null) && SubEnabled) || ((row.AccountID != null) && !SubEnabled));

		}

		protected virtual void GLBudgetTree_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			GLBudgetTree row = e.Row as GLBudgetTree;
			if (row == null) return;

			/// Never persist the root node - it is always provided by the <see cref=tree /> delegate.
			///
			if (row.GroupID == Guid.Empty)
			{
				e.Cancel = true;
				return;
			}

			bool isGroup = (bool)row.IsGroup || row.GroupID == row.ParentGroupID;

			if ((row.AccountID == null || row.SubID == null) && !isGroup)
			{
				Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountID>(e.Row, row.AccountID,
								new PXSetPropertyException(PXMessages.LocalizeNoPrefix(Messages.AcctSubMayNotBeEmptyForNonGroup), PXErrorLevel.RowError));
				return;
			}
			if (((row.AccountMask == null || row.SubMask == null) && !isGroup && SubEnabled) || (row.AccountMask == null && !isGroup && !SubEnabled))
			{
				Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountID>(e.Row, row.AccountMask,
								new PXSetPropertyException(PXMessages.LocalizeNoPrefix(Messages.AcctSubMaskNotBeEmptyForNonGroup), PXErrorLevel.RowError));
				return;
			}

			if (SubEnabled && isGroup && row.AccountMask == null && row.SubMask != null)
			{
				Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountID>(e.Row, row.AccountMask,
								new PXSetPropertyException(PXMessages.LocalizeNoPrefix(Messages.AcctMaskMayNotBeEmptyForGroup), PXErrorLevel.RowError));
				return;
			}

			if (SubEnabled && isGroup && row.AccountMask != null && row.SubMask == null)
			{
				Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountID>(e.Row, row.AccountMask,
								new PXSetPropertyException(PXMessages.LocalizeNoPrefix(Messages.SubMaskMayNotBeEmptyForGroup), PXErrorLevel.RowError));
				return;
			}
		}

		private void VerifyParentChildMask(GLBudgetTree child, GLBudgetTree parent)
		{
			if (child.ParentGroupID == parent.GroupID && parent.AccountMask != null && parent.SubMask != null)
			{
				if (!MatchMask(child.AccountMask, parent.AccountMask ?? string.Empty, false))
				{
					Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountMask>(child, child.AccountMask, new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetTreeIncorrectAccountMask), parent.AccountMask), PXErrorLevel.Error));
				}
				else
				{
					if (!MatchMask(child.SubMask, parent.SubMask ?? string.Empty, false))
					{
						Details.Cache.RaiseExceptionHandling<GLBudgetTree.subMask>(child, child.SubMask, new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetTreeIncorrectSubMask), parent.SubMask), PXErrorLevel.Error));
					}
				}
			}
		}

        public override void Persist()
        {        
            PXResultset<GLBudgetTree> allNodes = new PXResultset<GLBudgetTree>();
            if (SubEnabled)
            {
                allNodes = PXSelect<GLBudgetTree, Where<GLBudgetTree.accountMask, IsNotNull,
                                    And<GLBudgetTree.subMask, IsNotNull>>>.Select(this);
            }
            else
            {
                allNodes = PXSelect<GLBudgetTree, Where<GLBudgetTree.accountMask, IsNotNull>>.Select(this);
            }
			
			var nodesList = allNodes.RowCast<GLBudgetTree>().ToList();
            Dictionary<GLBudgetTree, PXEntryStatus> allNodesDict = new Dictionary<GLBudgetTree, PXEntryStatus>();
			
            foreach (GLBudgetTree node in nodesList)
            {
                allNodesDict.Add(node, Details.Cache.GetStatus(node));
            }

			for (int i = 0; i < nodesList.Count; i++)
            {
				var row = nodesList[i];
				for(int j = i+1; j < nodesList.Count; j++)
                {
					var node = nodesList[j];
                    if (row.GroupID != node.GroupID && allNodesDict[node] != PXEntryStatus.Notchanged)
                    {
                        //Verifying Account-Subaccount pairs for duplicates
                        if ((SubEnabled && row.AccountID != null && row.SubID != null && node.AccountID != null && node.SubID != null
                            && node.AccountID == row.AccountID && node.SubID == row.SubID)
                            || (!SubEnabled && row.AccountID != null && node.AccountID != null && node.AccountID == row.AccountID))
                        {
                            Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountID>(node, node.AccountID,
                                new PXSetPropertyException(PXMessages.LocalizeNoPrefix(Messages.DuplicateAccountSubEntry), PXErrorLevel.RowError));
                        }

						VerifyParentChildMask(node, row);
						VerifyParentChildMask(row, node);

                        //Verifying masks for overlapping
                        if (node.ParentGroupID != row.GroupID && row.ParentGroupID != node.GroupID &&
                            (SubEnabled &&
                            (MatchMask(row.AccountMask, node.AccountMask) || MatchMask(node.AccountMask, row.AccountMask))
                            && (MatchMask(row.SubMask, node.SubMask) || MatchMask(node.SubMask, row.SubMask)))
                            || (!SubEnabled &&
                            (MatchMask(row.AccountMask, node.AccountMask) || MatchMask(node.AccountMask, row.AccountMask))))
                        {
                            bool raiseError = true;

                            if ((bool)row.IsGroup)
                            {
                                PXResultset<GLBudgetTree> childNodes = collectChildNodes(row.GroupID);

                                foreach (GLBudgetTree childNode in childNodes)
                                {
                                    if (node == childNode) raiseError = false;
                                }
                            }
                            if (raiseError)
                            {
                                Details.Cache.RaiseExceptionHandling<GLBudgetTree.accountMask>(node, node.AccountMask, new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetTreeOverlappingMask), row.AccountMask, row.SubMask), PXErrorLevel.Error));
                                Details.Cache.RaiseExceptionHandling<GLBudgetTree.subMask>(node, node.SubMask, new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetTreeOverlappingMask), row.AccountMask, row.SubMask), PXErrorLevel.Error));
                            }
                        }
                    }
                }
            }
            base.Persist();
        }

		protected virtual void GLBudgetTree_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			GLBudgetTree row = e.Row as GLBudgetTree;
			if (row == null) return;
			if (CurrentSelected.AccountID != null && CurrentSelected.SubID != null && CurrentSelected.AccountID > 0 && CurrentSelected.SubID > 0)
			{
				row.Rollup = true;
			}
			row.GroupMask = CurrentSelected.GroupMask;
			if (row.SortOrder == 0)
			{
				GLBudgetTree LastItem = PXSelect<GLBudgetTree,
					Where<GLBudgetTree.parentGroupID,
					Equal<Required<GLBudgetTree.parentGroupID>>>, OrderBy<Desc<GLBudgetTree.sortOrder>>>.SelectWindowed(this, 0, 1, this.CurrentSelected.Group);

				row.SortOrder = LastItem == null ? 1 : LastItem.SortOrder.Value + 1;
			}

			if ((row.Rollup != null && (bool)row.Rollup && !(bool)row.IsGroup))
			{
				PXUIFieldAttribute.SetEnabled<GLBudgetTree.isGroup>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<GLBudgetTree.accountMask>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<GLBudgetTree.subMask>(cache, row, false);
			}
		}

		protected virtual void GLBudgetTree_SubMask_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void AccountsPreloadFilter_SubIDFilter_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void GLBudgetTree_GroupID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			GLBudgetTree row = e.Row as GLBudgetTree;
			if (row == null) return;
			row.GroupID = Guid.NewGuid();
			row.ParentGroupID = this.CurrentSelected.Group == null ? Guid.Empty : this.CurrentSelected.Group;
		}

		protected virtual void GLBudgetTree_SubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null) return;

			if (!MatchMask(((Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, e.NewValue)).SubCD, CurrentSelected.SubMaskWildcard ?? String.Empty))
			{
				throw new PXSetPropertyException(String.Format(PXMessages.LocalizeNoPrefix(Messages.BudgetSubaccountNotAllowed), CurrentSelected.SubMaskWildcard));
			}
		}

		protected virtual bool MatchMask(string accountCD, string mask)
		{
			return MatchMask(accountCD, mask, true);
		}

		protected virtual bool MatchMask(string accountCD, string mask, bool replaceChars)
		{
			if (mask.Length > 0 && accountCD.Length > 0 && mask.Length > accountCD.Length)
			{
				mask = mask.Substring(0, accountCD.Length);
			}
			for (int i = 0; i < mask.Length; i++)
			{
				if (replaceChars)
				{
					if (mask[i] == '?' && accountCD[i] != '?')
					{
						char[] chars = mask.ToCharArray();
						chars[i] = accountCD[i];
						mask = new string(chars);
					}
					if (accountCD[i] == '?' && mask[i] != '?')
					{
						char[] chars = accountCD.ToCharArray();
						chars[i] = mask[i];
						accountCD = new string(chars);
					}
				}
				if (i >= accountCD.Length || mask[i] != '?' && mask[i] != accountCD[i])
				{
					return false;
				}
			}
			return true;
		}

		private PXResultset<GLBudgetTree> collectChildNodes(Guid? GroupID)
		{
			PXResultset<GLBudgetTree> childNodes = new PXResultset<GLBudgetTree>();
			PXResultset<GLBudgetTree> childGroups = PXSelect<GLBudgetTree,
				Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>>>.Select(this, GroupID);
			foreach (PXResult<GLBudgetTree> childGroup in childGroups)
			{
				childNodes.Add(childGroup);
				childNodes.AddRange(collectChildNodes(((GLBudgetTree)childGroup).GroupID));
			}
			return childNodes;
		}

		protected virtual void GLBudgetTree_SubID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLBudgetTree row = e.Row as GLBudgetTree;
			if (row == null) return;

			if (row.SubMask == null)
			{
				row.SubMask = ((Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, row.SubID)).SubCD;
			}

			if (!IsImport)
			{
				row.IsGroup = false;
			}
		}

		protected virtual void GLBudgetTree_IsGroup_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXStringState strState = (PXStringState)sender.GetStateExt(null, typeof(GLBudgetTree.accountID).Name);
			PXDBStringAttribute.SetInputMask(sender, typeof(GLBudgetTree.accountMask).Name, strState.InputMask.Replace('#', 'C'));
			strState = (PXStringState)sender.GetStateExt(null, typeof(GLBudgetTree.subID).Name);
			PXDBStringAttribute.SetInputMask(sender, typeof(GLBudgetTree.subMask).Name, strState.InputMask.Replace('A', 'C'));
		}

		protected virtual void GLBudgetTree_IsGroup_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			GLBudgetTree row = e.Row as GLBudgetTree;
			if (e.NewValue != null && row.IsGroup != null && (bool)row.IsGroup && !(bool)e.NewValue)
			{
				if (Details.Ask(Messages.BudgetTreeDeleteGroupTitle, Messages.BudgetTreeDeleteGroupMessage, MessageButtons.YesNo) == WebDialogResult.Yes)
				{
					deleteRecords(row.GroupID);
				}
				else
				{
					e.NewValue = true;
					e.Cancel = true;
				}
			}
		}

		protected virtual void GLBudgetTree_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLBudgetTree row = e.Row as GLBudgetTree;
			object defaultValue;
			sender.RaiseFieldDefaulting<GLBudgetTree.description>(e.Row, out defaultValue);
			if (defaultValue != null) row.Description = defaultValue.ToString();

			if (!IsImport)
			{
				row.IsGroup = false;
			}
		}

		protected virtual void GLBudgetTree_AccountMask_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
			{
				PXStringState strState = (PXStringState)sender.GetStateExt(null, typeof(GLBudgetTree.accountID).Name);
				e.NewValue = ((string)e.NewValue).PadRight(strState.InputMask.Length - 1, '?').Replace(' ', '?');
			}
		}

		protected virtual void GLBudgetTree_SubMask_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
			{
				PXStringState strState = (PXStringState)sender.GetStateExt(null, typeof(GLBudgetTree.subID).Name);
				e.NewValue = ((string)e.NewValue).PadRight(strState.InputMask.Length - 1, '?').Replace(' ', '?');
			}
		}

		#endregion
	}

	[Serializable]
    [PXHidden]
	public partial class SelectedNode : IBqlTable
	{
		#region Group
		public abstract class group : PX.Data.BQL.BqlGuid.Field<group> { }
		protected Guid? _Group;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? Group
		{
			get
			{
				return this._Group;
			}
			set
			{
				this._Group = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXInt()]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected int? _SubID;
		[PXInt()]
		public virtual int? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region AccountMask Wildcard
		public abstract class accountMaskWildcard : PX.Data.BQL.BqlString.Field<accountMaskWildcard> { };
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(SelectedGroup.WildcardAnything)]
		public virtual string AccountMaskWildcard { get; set; }
		#endregion
		#region SubMask Wildcard
		public abstract class subMaskWildcard : PX.Data.BQL.BqlString.Field<subMaskWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		[PXDefault(SelectedGroup.WildcardAnything)]
		public virtual string SubMaskWildcard { get; set; }
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		[PXDBGroupMask]
		public virtual byte[] GroupMask { get; set; }
		#endregion
	}

	[Serializable]
	public partial class AccountsPreloadFilter : IBqlTable
	{
		#region FromAccount
		public abstract class fromAccount : PX.Data.BQL.BqlInt.Field<fromAccount> { }
		[PXInt()]
		[PXDimensionSelector(AccountAttribute.DimensionName, (typeof(Search<Account.accountID, Where<Account.accountCD, Like<Current<SelectedNode.accountMaskWildcard>>>, OrderBy<Asc<Account.accountCD>>>)), typeof(Account.accountCD), DescriptionField=typeof(Account.description))]
		[PXUIField(DisplayName = "Account from", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual int? FromAccount { get; set; }
		#endregion

		#region ToAccount
		public abstract class toAccount : PX.Data.BQL.BqlInt.Field<toAccount> { }
		[PXInt()]
		[PXDimensionSelector(AccountAttribute.DimensionName, (typeof(Search<Account.accountID, Where<Account.accountCD, Like<Current<SelectedNode.accountMaskWildcard>>>, OrderBy<Asc<Account.accountCD>>>)), typeof(Account.accountCD), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account to", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual int? ToAccount { get; set; }
		#endregion

		#region SubIDFilter
		public abstract class subIDFilter : PX.Data.BQL.BqlString.Field<subIDFilter> { }
		[SubAccountRaw(DisplayName = "Subaccount Mask")]
		public virtual string SubIDFilter { get; set; }
		#endregion

		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
		[PXString(30, IsUnicode = true)]
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

	}
}