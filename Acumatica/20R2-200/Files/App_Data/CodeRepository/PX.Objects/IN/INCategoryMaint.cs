using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.Api.Export;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.SM;
using PX.Web.UI;

namespace PX.Objects.IN
{
    [Serializable]
    [NonOptimizable(IgnoreOptimizationBehavior = true)]
    public class INCategoryMaint : PXGraph<INCategoryMaint>
    {
        public static class AddItemsTypesList
        {
            public class ListAttribute : PXStringListAttribute
            {
				public ListAttribute() : base(
					new[]
					{
						Pair(AddAllItems, "All Items"),
						Pair(AddItemsByClass, "By Class"),
					}) {}
            }

            public const string AddAllItems = "A";
            public const string AddItemsByClass = "I";
        }

        #region DAC
        [Serializable]
        public partial class ClassFilter : IBqlTable
        {
            #region PriceBasis
            public abstract class addItemsTypes : PX.Data.BQL.BqlString.Field<addItemsTypes> { }
            protected String _AddItemsTypes;
            [PXString(1, IsFixed = true)]
            [PXUIField(DisplayName = ActionsMessages.AddItems)]
            [AddItemsTypesList.List()]
            [PXDefault(AddItemsTypesList.AddItemsByClass)]
            public virtual String AddItemsTypes
            {
                get
                {
                    return this._AddItemsTypes;
                }
                set
                {
                    this._AddItemsTypes = value;
                }
            }
            #endregion

            #region ItemClassID
            public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
            protected int? _ItemClassID;
            [PXInt]
            [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
			public virtual int? ItemClassID
            {
                get
                {
                    return this._ItemClassID;
                }
                set
                {
                    this._ItemClassID = value;
                }
            }
            #endregion
        }

        [Serializable]
        [PXHidden]
        public partial class SelectedNode : IBqlTable
        {
            #region FolderID
            public abstract class folderID : PX.Data.BQL.BqlInt.Field<folderID> { }
            protected int? _FolderID;
            [PXInt()]
            [PXUIField(Visible = false)]
            public virtual int? FolderID
            {
                get
                {
                    return this._FolderID;
                }
                set
                {
                    this._FolderID = value;
                }
            }
            #endregion

            #region CategoryID
            public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
            protected int? _CategoryID;
            [PXInt()]
            [PXUIField(Visible = false)]
            public virtual int? CategoryID
            {
                get
                {
                    return this._CategoryID;
                }
                set
                {
                    this._CategoryID = value;
                }
            }
            #endregion
        }

        [Serializable]
        public class INFolderCategory : INCategory { }

        [Serializable]
        public class INCategoryCurrent : INCategory { }

        #endregion

        #region Select
        public PXFilter<ClassFilter> ClassInfo;
        public PXFilter<SelectedNode> SelectedFolders;
        public PXSelectOrderBy<INCategory, OrderBy<Asc<INCategory.sortOrder>>> Folders;
	    protected virtual IEnumerable folders([PXInt] int? categoryID) => GetFolders(categoryID);

		public PXSelect<INCategory, Where<INCategory.categoryID, Equal<Current<INCategory.categoryID>>>> CurrentCategory;
	    protected virtual IEnumerable currentCategory() => GetCurrentCategory();

		public PXSelectJoin<INItemCategory,
                LeftJoin<InventoryItem, On<INItemCategory.FK.InventoryItem>>,
                Where<INItemCategory.categoryID, Equal<Current<INCategory.categoryID>>>>
            Members;
	    protected virtual IEnumerable members() => GetMembers();


		public PXSelectOrderBy<INFolderCategory, OrderBy<Asc<INFolderCategory.sortOrder>>> ParentFolders;
	    protected virtual IEnumerable parentFolders([PXInt] int? categoryID) => GetParentFolders(categoryID);


		public PXFilter<INItemCategoryBuffer> Buffer;

		[PXHidden]
		public PXSelect<InventoryItem,
			Where<InventoryItem.inventoryID, Equal<Optional<INItemCategory.inventoryID>>>> RelatedInventoryItem;

        #endregion

        #region delegates
        private IEnumerable<INCategory> GetFolders(int? categoryID)
        {
            if (categoryID == null)
            {
                yield return new INCategory
                {
                    CategoryID = 0,
                    Description = PXSiteMap.RootNode.Title
                };
            }
	        foreach (INCategory item in CategoryCache<INCategory>.GetChildren(this, categoryID))
	        {
		        if (!string.IsNullOrEmpty(item.Description))
			        yield return item;
	        }
        }

        private IEnumerable<INCategory> GetCurrentCategory()
        {
            if (Folders.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<INCategory.description>(Caches[typeof(INCategory)], null, Folders.Current.ParentID != null);
                PXUIFieldAttribute.SetEnabled<INCategory.parentID>(Caches[typeof(INCategory)], null, Folders.Current.ParentID != null);
                Caches[typeof(INItemCategory)].AllowInsert = Folders.Current.ParentID != null;
                Caches[typeof(INItemCategory)].AllowDelete = Folders.Current.ParentID != null;
                Caches[typeof(INItemCategory)].AllowUpdate = Folders.Current.ParentID != null;
                Actions["Copy"].SetEnabled(Folders.Current.ParentID != null);
                Actions["Cut"].SetEnabled(Folders.Current.ParentID != null);
                Actions["Paste"].SetEnabled(Folders.Current.ParentID != null);
                Actions["AddItemsbyClass"].SetEnabled(Folders.Current.ParentID != null);

                foreach (INCategory item in PXSelect<INCategory,
                Where<INCategory.categoryID, Equal<Required<INCategory.categoryID>>>>.
                Select(this, Folders.Current.CategoryID))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable GetMembers()
        {
            if (Folders.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<INCategory.description>(Caches[typeof(INCategory)], null, Folders.Current.ParentID != null);
                PXUIFieldAttribute.SetEnabled<INCategory.parentID>(Caches[typeof(INCategory)], null, Folders.Current.ParentID != null);
                Caches[typeof(INItemCategory)].AllowInsert = Folders.Current.ParentID != null;
                Caches[typeof(INItemCategory)].AllowUpdate = Folders.Current.ParentID != null;
                Actions["Copy"].SetEnabled(Folders.Current.ParentID != null);
                Actions["Cut"].SetEnabled(Folders.Current.ParentID != null);
                Actions["Paste"].SetEnabled(Folders.Current.ParentID != null);
                Actions["AddItemsbyClass"].SetEnabled(Folders.Current.ParentID != null);

                PXSelectBase<INItemCategory> cmd;
                cmd = new PXSelectJoin<INItemCategory,
                    LeftJoin<InventoryItem, On<INItemCategory.FK.InventoryItem>>,
                Where<INItemCategory.categoryID, Equal<Required<INCategory.categoryID>>>>(this);

                int startRow = PXView.StartRow;
                int totalRows = 0;

                foreach (
                    var res in
                        cmd.View.Select(PXView.Currents, new object[] { Folders.Current.CategoryID }, PXView.Searches, PXView.SortColumns, PXView.Descendings,
                            PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
                {
                    yield return res;
                    PXView.StartRow = 0;
                }
            }
        }

        private IEnumerable<INFolderCategory> GetParentFolders(int? categoryID)
        {
            if (categoryID == null)
            {
                yield return new INFolderCategory
                {
                    CategoryID = 0,
                    Description = PXSiteMap.RootNode.Title
                };
            }

	        foreach (INFolderCategory item in CategoryCache<INFolderCategory>.GetChildren(this, categoryID))
	        {
		        if (!string.IsNullOrEmpty(item.Description) && item.CategoryID != Folders.Current.CategoryID)
			        yield return item;
	        }
        }

		protected class CategoryCache<TCategory> : IPrefetchable<PXGraph>
			where TCategory : INCategory, new()
		{
			private ILookup<int?, TCategory> _lookup;

			public void Prefetch(PXGraph parameter)
			{
				_lookup =
					PXSelectOrderBy<TCategory, OrderBy<Asc<INCategory.parentID, Asc<INCategory.sortOrder>>>>
						.Select(parameter)
						.RowCast<TCategory>()
						.ToLookup(r => r.ParentID);
			}

			private static CategoryCache<TCategory> GetInstance(PXGraph graph)
			{
				return PXDatabase.GetLocalizableSlot<CategoryCache<TCategory>, PXGraph>(typeof(CategoryCache<TCategory>).FullName, graph, typeof(INCategory));
			}

			public static IEnumerable<TCategory> GetChildren(PXGraph graph, int? categoryID)
			{
				CategoryCache<TCategory> instance = GetInstance(graph);
				IEnumerable<TCategory> children = instance._lookup[categoryID];

				var cache = graph.Caches<TCategory>();
				var cached = cache.Cached.RowCast<TCategory>().Where(c => c.ParentID == categoryID && cache.GetStatus(c).IsIn(PXEntryStatus.Inserted, PXEntryStatus.Updated, PXEntryStatus.Deleted, PXEntryStatus.Modified)).ToArray();
				if (cached.Length > 0)
				{
					var untouchedShared = children.Except(cached, cache.GetComparer()).ToArray();
					var aliveLocal = cached.Where(c => cache.GetStatus(c) != PXEntryStatus.Deleted).ToArray();
					children = untouchedShared.Concat(aliveLocal).ToArray();
				}

				return children;
			}

			public static void Clear(PXGraph graph) => GetInstance(graph).Prefetch(graph);
		}
        #endregion

        #region Actions
        public PXSave<SelectedNode> Save;
        public PXCancel<SelectedNode> Cancel;

        public PXAction<SelectedNode> AddCategory;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable addCategory(PXAdapter adapter)
        {
            int ParentID = (int)Folders.Current.CategoryID;
            var inserted = (INCategory) Caches[typeof(INCategory)].Insert(new INCategory
            {
                Description = PXMessages.LocalizeNoPrefix(Messages.NewKey),
                ParentID = ParentID,
            });
            inserted.TempChildID = inserted.CategoryID;
            inserted.TempParentID = ParentID;
            INCategory previous = PXSelect<INCategory,
                Where<INCategory.parentID, Equal<Required<INCategory.parentID>>>,
                OrderBy<Desc<INCategory.sortOrder>>>.SelectSingleBound(this, null, ParentID);

            int sortOrder = (int)previous.SortOrder;
            sortOrder = sortOrder + 1;
            inserted.SortOrder = previous != null ? sortOrder : 1;
            Folders.Cache.ActiveRow = inserted;
            return adapter.Get();
        }

        public PXAction<SelectedNode> DeleteCategory;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable deleteCategory(PXAdapter adapter)
        {
            // ToDo recursive delete 
            Caches[typeof(INCategory)].Delete(Folders.Current);
            return adapter.Get();
        }



        public PXAction<SelectedNode> down;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageUrl = "~/Icons/NavBar/xps_collapse.gif", DisabledImageUrl = "~/Icons/NavBar/xps_collapseD.gif")]
        public virtual IEnumerable Down(PXAdapter adapter)
        {
            INCategory curr = Folders.Current;
            INCategory next = PXSelect<INCategory,
                Where<INCategory.parentID, Equal<Required<INCategory.parentID>>,
                    And<INCategory.sortOrder, Greater<Required<INCategory.parentID>>>>,
                OrderBy<Asc<INCategory.sortOrder>>>.SelectSingleBound(this, null, Folders.Current.ParentID,
                    Folders.Current.SortOrder);

            if (next != null && curr != null)
            {
                int temp = (int)curr.SortOrder;
                curr.SortOrder = next.SortOrder;
                next.SortOrder = temp;
                Caches[typeof(INCategory)].Update(next);
                Caches[typeof(INCategory)].Update(curr);
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> up;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageUrl = "~/Icons/NavBar/xps_expand.gif", DisabledImageUrl = "~/Icons/NavBar/xps_expandD.gif")]
        public virtual IEnumerable Up(PXAdapter adapter)
        {
            INCategory curr = Folders.Current;
            INCategory prev = PXSelect<INCategory,
                Where<INCategory.parentID, Equal<Required<INCategory.parentID>>,
                    And<INCategory.sortOrder, Less<Required<INCategory.parentID>>>>,
                OrderBy<Desc<INCategory.sortOrder>>>.SelectSingleBound(this, null, Folders.Current.ParentID,
                    Folders.Current.SortOrder);

            if (prev != null && curr != null)
            {
                int temp = (int)curr.SortOrder;
                curr.SortOrder = prev.SortOrder;
                prev.SortOrder = temp;
                Caches[typeof(INCategory)].Update(prev);
                Caches[typeof(INCategory)].Update(curr);
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> Copy;
        [PXButton(ImageKey = Sprite.Main.Copy, Tooltip = ActionsMessages.ttipCopyRec)]
        [PXUIField(DisplayName = ActionsMessages.CopyRec, Enabled = false)]
        public IEnumerable copy(PXAdapter adapter)
        {
            Buffer.Cache.Clear();
            foreach (INItemCategory pxResult in PXSelect<INItemCategory,
            Where<INItemCategory.categoryID, Equal<Required<INItemCategory.categoryID>>>>.
            Select(this, Folders.Current.CategoryID))
            {
                if (pxResult.CategorySelected == true)
                {
                    INItemCategoryBuffer insertnode = Buffer.Cache.CreateInstance() as INItemCategoryBuffer;
                    insertnode.InventoryID = pxResult.InventoryID;
                    Buffer.Cache.Insert(insertnode);
                }
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> Cut;
        [PXButton(ImageKey = Sprite.Main.Cut, Tooltip = ActionsMessages.ttipCut)]
        [PXUIField(DisplayName = ActionsMessages.Cut, Enabled = false)]
        internal IEnumerable cut(PXAdapter adapter)
        {
            Buffer.Cache.Clear();
            var delbuffer = new List<INItemCategory>();
            foreach (INItemCategory pxResult in PXSelect<INItemCategory,
            Where<INItemCategory.categoryID, Equal<Required<INItemCategory.categoryID>>>,
            OrderBy<Asc<InventoryItem.inventoryCD>>>.Select(this, Folders.Current.CategoryID))
            {
                if (pxResult.CategorySelected == true)
                {
                    INItemCategoryBuffer insertnode = Buffer.Cache.CreateInstance() as INItemCategoryBuffer;
                    insertnode.InventoryID = pxResult.InventoryID;
                    Buffer.Cache.Insert(insertnode);
                    delbuffer.Add(pxResult);
                }
            }

            foreach (INItemCategory pxResult in delbuffer)
            {
                Members.Cache.Delete(pxResult);
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> Paste;
        [PXButton(ImageKey = Sprite.Main.Paste, Tooltip = ActionsMessages.ttipPaste)]
        [PXUIField(DisplayName = ActionsMessages.Paste, Enabled = false)]
        internal IEnumerable paste(PXAdapter adapter)
        {
            foreach (INItemCategoryBuffer pxResult in Buffer.Cache.Cached)
            {
                INItemCategory insertnode = Members.Cache.CreateInstance() as INItemCategory;
                insertnode.InventoryID = pxResult.InventoryID;
                Members.Cache.Insert(insertnode);
            }
            return adapter.Get();
        }

        public PXAction<SelectedNode> AddItemsbyClass;
        [PXButton(Tooltip = ActionsMessages.AddItems)]
        [PXUIField(DisplayName = ActionsMessages.AddItems, Enabled = false)]
        internal IEnumerable addItemsbyClass(PXAdapter adapter)
        {
            if (ClassInfo.AskExt() == WebDialogResult.OK)
            {
                if (ClassInfo.Current.AddItemsTypes == AddItemsTypesList.AddAllItems)
                {
                    foreach (InventoryItem pxResult in PXSelectReadonly<InventoryItem, 
						Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>.Select(this))
                    {
                        INItemCategory insertnode = Members.Cache.CreateInstance() as INItemCategory;
                        insertnode.InventoryID = pxResult.InventoryID;
                        insertnode.CategorySelected = false;
                        Members.Cache.Update(insertnode);

                    }
                }
                else
                {
                    foreach (InventoryItem pxResult in PXSelectReadonly<InventoryItem, 
						Where<InventoryItem.itemClassID, Equal<Required<InventoryItem.itemClassID>>, 
						And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>>.Select(this, ClassInfo.Current.ItemClassID))
                    {
                        INItemCategory insertnode = Members.Cache.CreateInstance() as INItemCategory;
                        insertnode.InventoryID = pxResult.InventoryID;
                        insertnode.CategorySelected = false;
                        Members.Cache.Update(insertnode);
                    }
                }
            }
            Actions.PressSave();
            return adapter.Get();
        }

        public PXAction<SelectedNode> ViewDetails;
        [PXButton()]
        [PXUIField(DisplayName = "Inventory Details", Visible = false)]
        public virtual IEnumerable viewDetails(PXAdapter adapter)
        {
            if (Members.Current != null)
            {
                InventoryItem _inventoryItem = InventoryItem.PK.Find(this, Members.Current.InventoryID);

                if (_inventoryItem != null)
                {
                    if (_inventoryItem.StkItem == true)
                    {
                        InventoryItemMaint graph = PXGraph.CreateInstance<InventoryItemMaint>();
                        PXResult result = graph.Item.Search<InventoryItem.inventoryID>(Members.Current.InventoryID);
                        if (result != null)
                        {
                            InventoryItem _ninventoryItem = result[typeof(InventoryItem)] as InventoryItem;
                            graph.Item.Current = _ninventoryItem;
                            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
                        }
                    }

                    else
                    {
                        NonStockItemMaint graph = PXGraph.CreateInstance<NonStockItemMaint>();
                        PXResult result = graph.Item.Search<InventoryItem.inventoryID>(Members.Current.InventoryID);
                        if (result != null)
                        {
                            InventoryItem _sinventoryItem = result[typeof(InventoryItem)] as InventoryItem;
                            graph.Item.Current = _sinventoryItem;
                            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
                        }
                    }
                }
            }
            return adapter.Get();
        }
        #endregion

        #region Event Handler
        protected virtual void INCategory_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            INCategory row = e.Row as INCategory;
            if (row?.CategoryID == null) return;
            deleteRecurring(row);
        }


        protected virtual void INItemCategory_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            INItemCategory row = (INItemCategory) e.Row;
            if (row != null)
            {
                PXDefaultAttribute.SetDefault<INItemCategory.categorySelected>(cache, row, false);
            }
        }

        protected virtual void INItemCategoryBuffer_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void ClassFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            ClassFilter row = (ClassFilter) e.Row;
            if (row == null) return;
            if (row.AddItemsTypes == AddItemsTypesList.AddAllItems)
            {
                row.ItemClassID = null;
            }
            PXUIFieldAttribute.SetEnabled<ClassFilter.itemClassID>(cache, row, row.AddItemsTypes != AddItemsTypesList.AddAllItems);
        }

        public override void Persist()
        {
            Buffer.Cache.Clear();
            base.Persist();
            foreach (INCategory item in Caches[typeof(INCategory)].Cached)
            {
                if (item.TempParentID < 0)
                {
                    foreach (INCategory item2 in Caches[typeof(INCategory)].Cached)
                    {
                        if (item2.TempChildID == item.TempParentID)
                        {
                            item.ParentID = item2.CategoryID;
                            item.TempParentID = item2.CategoryID;
                            Caches[typeof(INCategory)].SetStatus(item, PXEntryStatus.Updated);
                        }
                    }
                }
            }
            base.Persist();
            Members.View.RequestRefresh();
	        CategoryCache<INCategory>.Clear(this);
	        CategoryCache<INFolderCategory>.Clear(this);
        }

        private void deleteRecurring(INCategory map)
        {
            if (map != null)
            {
                foreach (INCategory child in PXSelect<INCategory, Where<INCategory.parentID, Equal<Required<INCategory.categoryID>>>>.Select(this, map.CategoryID))
                    deleteRecurring(child);
	            Caches[typeof(INCategory)].Delete(map);
            }
        }

		protected virtual void _(Events.RowInserted<INItemCategory> eventArgs)
		{
			if (eventArgs.Row?.InventoryID == null)
				return;

			var inventory = RelatedInventoryItem.SelectSingle();
			RelatedInventoryItem.Cache.MarkUpdated(inventory);
		}

		protected virtual void _(Events.RowUpdated<INItemCategory> eventArgs)
		{
			int? oldInventoryID = (int?)eventArgs.OldRow?.InventoryID;
			if (eventArgs.Row == null || eventArgs.Row.InventoryID == oldInventoryID)
				return;

			if (eventArgs.Row.InventoryID != null)
			{
				var inventory = RelatedInventoryItem.SelectSingle();
				RelatedInventoryItem.Cache.MarkUpdated(inventory);
			}
			if (oldInventoryID != null)
			{
				var inventory = RelatedInventoryItem.SelectSingle(oldInventoryID);
				RelatedInventoryItem.Cache.MarkUpdated(inventory);
			}
		}

		protected virtual void _(Events.RowDeleted<INItemCategory> eventArgs)
		{
			if (eventArgs.Row?.InventoryID == null)
				return;

			var inventory = RelatedInventoryItem.SelectSingle();
			RelatedInventoryItem.Cache.MarkUpdated(inventory);
		}
		#endregion
	}
}