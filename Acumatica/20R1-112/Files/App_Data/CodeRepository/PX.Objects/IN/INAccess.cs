using System;
using System.Collections.Generic;
using PX.Data;
using System.Collections;

namespace PX.Objects.IN
{
	public class INAccessDetail : PX.SM.UserAccess
	{
		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(Search<INSite.siteCD, Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>), typeof(INSite.siteCD))]
        protected virtual void INSite_SiteCD_CacheAttached(PXCache sender)
		{
		}

		public INAccessDetail()
		{
			INSetup setup = INSetup.Current;
			Site.Cache.AllowDelete = false;
			Site.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Site.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INSite.siteCD>(Site.Cache, null);

			Views.Caches.Remove(typeof(PX.SM.RelationGroup));
			Views.Caches.Add(typeof(PX.SM.RelationGroup));
		}
		public PXSetup<INSetup> INSetup;

        public PXSelect<INSite, Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>> Site;

        public PXSave<INSite> SaveSite;
		public PXCancel<INSite> CancelSite;
		public PXFirst<INSite> FirstSite;
		public PXPrevious<INSite> PrevSite;
		public PXNext<INSite> NextSite;
		public PXLast<INSite> LastSite;

		protected override IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if ((group.SpecificModule == null || group.SpecificModule == typeof(InventoryItem).Namespace)
					&& (group.SpecificType == null || group.SpecificType == typeof(INSite).FullName)
					|| PX.SM.UserAccess.IsIncluded(getMask(), group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}

		protected override byte[] getMask()
		{
			byte[] mask = null;
			if (Site.Current != null)
			{
				mask = Site.Current.GroupMask;
			}
			return mask;
		}

		public override void Persist()
		{
			if (Site.Current != null)
			{
				PopulateNeighbours<INSite>(Site, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.GL.Account>();
			}
			else
			{
				return;
			}
			base.Persist();
		}
	}

	public class INAccess : PX.SM.BaseAccess
	{
		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDimensionSelector(SiteAttribute.DimensionName, typeof(Search<INSite.siteCD, Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>), typeof(INSite.siteCD))]
        protected virtual void INSite_SiteCD_CacheAttached(PXCache sender)
		{
		}

		public class INRelationGroupWarehouseSelectorAttribute : PXCustomSelectorAttribute
		{
			public INRelationGroupWarehouseSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return INAccess.GroupDelegate(_Graph, false);
			}
		}
		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
        [INRelationGroupWarehouseSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected virtual void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		public INAccess()
		{
			INSetup setup = INSetup.Current;
			Site.Cache.AllowDelete = false;
			Site.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Site.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INSite.included>(Site.Cache, null);
		}
		public PXSetup<INSetup> INSetup;

        public Int32? INTransitSiteID
        {
            get
            {
                if (INSetup.Current.TransitSiteID == null)
                    throw new PXException("Please fill transite site id in inventory preferences.");
                return INSetup.Current.TransitSiteID;
            }
        }

        public PXSelect<INSite, Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>> Site;
		protected virtual IEnumerable site(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				foreach (INSite item in PXSelect<INSite>
					.Select(this))
				{
                    if (item.SiteID == this.INTransitSiteID)
                        continue;
					Site.Current = item;
					yield return item;
				}
			}
			else
			{
				yield break;
			}
		}

		static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<warehouseType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(InventoryItem).Namespace)
					&& (group.SpecificType == null || group.SpecificType == typeof(INSite).FullName)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		protected virtual IEnumerable group()
		{
			return GroupDelegate(this, true);
		}

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(inventoryModule).Namespace;
			group.SpecificType = typeof(INSite).FullName;
		}

		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (group != null)
			{
				if (String.IsNullOrEmpty(group.GroupName))
				{
					Save.SetEnabled(false);
				}
				else
				{
					Save.SetEnabled(true);
				}
			}
		}
		protected virtual void INSite_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INSite site = e.Row as INSite;
			PX.SM.RelationGroup group = Group.Current;
			if (site != null && site.GroupMask != null && group != null && group.GroupMask != null
				&& sender.GetStatus(site) == PXEntryStatus.Notchanged)
			{
				for (int i = 0; i < site.GroupMask.Length && i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] != 0x00 && (site.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
					{
						site.Included = true;
					}
				}
			}
		}
		protected virtual void INSite_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			INSite site = e.Row as INSite;
			PX.SM.RelationGroup group = Group.Current;
			if (site != null && site.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (site.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = site.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					site.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (site.Included == true)
					{
						site.GroupMask[i] = (byte)(site.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						site.GroupMask[i] = (byte)(site.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}
		public override void Persist()
		{
			populateNeighbours<PX.SM.Users>(Users);
			populateNeighbours<INSite>(Site);
			populateNeighbours<PX.SM.Users>(Users);
			base.Persist();
			PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.IN.INSite>();
			PXDimensionAttribute.Clear();
		}

		#region Do not Check Control Accounts
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_SalesAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_InvtAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_COGSAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_StdCstRevAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_StdCstVarAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_PPVAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_POAccrualAcctID_CacheAttached(PXCache sender) { }
		[GL.Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void INSite_LCVarianceAcctID_CacheAttached(PXCache sender) { }
		#endregion
	}

	public class INAccessItem : PX.SM.BaseAccess
	{
		public class INRelationGroupSelectorAttribute : PXCustomSelectorAttribute
		{
			public INRelationGroupSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return INAccessItem.GroupDelegate(_Graph, false);
			}
		}
		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[INRelationGroupSelector(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected virtual void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryCD, Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>), typeof(InventoryItem.inventoryCD))]
		protected virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender)
		{
		}

		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
		protected virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender)
		{
		}

		[PXUIField(DisplayName = "Lot/Serial Class")]
		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_LotSerClassID_CacheAttached(PXCache sender)
		{
		}

		[PXUIField(DisplayName = "Posting Class")]
		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_PostClassID_CacheAttached(PXCache sender)
		{
		}

		public INAccessItem()
		{
			INSetup setup = INSetup.Current;
			Class.Cache.AllowDelete = false;
			Class.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Class.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INItemClass.included>(Class.Cache, null);
			PXUIFieldAttribute.SetEnabled(Item.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<InventoryItem.included>(Item.Cache, null);
			PXUIFieldAttribute.SetEnabled<InventoryItem.inventoryCD>(Item.Cache, null);

			PXDefaultAttribute.SetPersistingCheck<InventoryItem.valMethod>(Item.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<InventoryItem.cOGSAcctID>(Item.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<InventoryItem.cOGSSubID>(Item.Cache, null, PXPersistingCheck.Nothing);
		}
		public PXSetup<INSetup> INSetup;
		public PXSetup<CS.CommonSetup> CommoSetup;

		public PXSelect<InventoryItem> Item;
		protected virtual IEnumerable item(
		)
		{

			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (InventoryItem item in PXSelect<InventoryItem,
					Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or<Match<Required<InventoryItem.groupMask>>>>>>>>
					.Select(this, new object[] {new byte[0]}))
				{
					if (!inserted)
					{
						Item.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Item.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Item.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}

		public PXSelect<INItemClass> Class;
		protected virtual IEnumerable cLass(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				foreach (INItemClass cLass in PXSelect<INItemClass>
					.Select(this))
				{
					Class.Current = cLass;
					yield return cLass;
				}
			}
			else
			{
				yield break;
			}
		}

		static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<itemType>,
					Or<PX.SM.Neighbour.leftEntityType, Equal<itemClassType>>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(InventoryItem).Namespace)
					&& (group.SpecificType == null || group.SpecificType == typeof(CS.SegmentValue).FullName || group.SpecificType == typeof(InventoryItem).FullName)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		protected virtual IEnumerable group()
		{
			return GroupDelegate(this, true);
		}

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(inventoryModule).Namespace;
			group.SpecificType = typeof(InventoryItem).FullName;
		}
		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (group != null)
			{
				if (String.IsNullOrEmpty(group.GroupName))
				{
					Save.SetEnabled(false);
				}
				else
				{
					Save.SetEnabled(true);
				}
			}
		}

		protected virtual void INItemClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemClass cLass = e.Row as INItemClass;
			PX.SM.RelationGroup group = Group.Current;
			if (cLass != null && cLass.GroupMask != null && group != null && group.GroupMask != null
				&& sender.GetStatus(cLass) == PXEntryStatus.Notchanged)
			{
				for (int i = 0; i < cLass.GroupMask.Length && i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] != 0x00 && (cLass.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
					{
						cLass.Included = true;
					}
				}
			}
		}
		protected virtual void INItemClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			INItemClass cLass = e.Row as INItemClass;
			PX.SM.RelationGroup group = Group.Current;
			if (cLass != null && cLass.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (cLass.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = cLass.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					cLass.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (cLass.Included == true)
					{
						cLass.GroupMask[i] = (byte)(cLass.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						cLass.GroupMask[i] = (byte)(cLass.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}

		protected virtual void InventoryItem_InventoryCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelectorAttribute.Select<InventoryItem.inventoryCD>(sender, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, "InventoryCD");
			}
		}
		protected virtual void InventoryItem_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				InventoryItem item = PXSelectorAttribute.Select<InventoryItem.inventoryCD>(sender, e.Row) as InventoryItem;
				if (item != null)
				{
					item.Included = true;
					PXCache<InventoryItem>.RestoreCopy((InventoryItem)e.Row, item);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			InventoryItem item = e.Row as InventoryItem;
			if (item != null && sender.GetStatus(item) == PXEntryStatus.Notchanged)
			{
				if (item.GroupMask != null)
				{
					foreach (byte b in item.GroupMask)
					{
						if (b != 0x00)
						{
							item.Included = true;
							return;
						}
					}
				}
				else
				{
					item.Included = true;
				}
			}
		}
		protected virtual void InventoryItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			InventoryItem item = e.Row as InventoryItem;
			PX.SM.RelationGroup group = Group.Current;
			if (item != null && item.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (item.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = item.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					item.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (item.Included == true)
					{
						item.GroupMask[i] = (byte)(item.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						item.GroupMask[i] = (byte)(item.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}

		public override void Persist()
		{
			populateNeighbours<PX.SM.Users>(Users);
			populateNeighbours<InventoryItem>(Item);
			populateNeighbours<INItemClass>(Class);
			populateNeighbours<InventoryItem>(Item);
			populateNeighbours<PX.SM.Users>(Users);
			base.Persist();
			PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.IN.INItemClass>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.IN.InventoryItem>();
			PXDimensionAttribute.Clear();
		}
	}

	public class INAccessDetailItem : PX.SM.UserAccess
	{
		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(InventoryItem.inventoryCD), typeof(InventoryItem.inventoryCD))]
		protected virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender)
		{
		}

		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
		protected virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender)
		{
		}

		[PXUIField(DisplayName = "Lot/Serial Class")]
		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_LotSerClassID_CacheAttached(PXCache sender)
		{
		}

		[PXUIField(DisplayName = "Posting Class")]
		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_PostClassID_CacheAttached(PXCache sender)
		{
		}

		public INAccessDetailItem()
		{
			INSetup setup = INSetup.Current;

			Class.Cache.AllowDelete = false;
			Class.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Class.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INItemClass.itemClassCD>(Class.Cache, null);

			Item.Cache.AllowDelete = false;
			Item.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Item.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<InventoryItem.inventoryCD>(Item.Cache, null);

			Views.Caches.Remove(typeof(PX.SM.RelationGroup));
			Views.Caches.Add(typeof(PX.SM.RelationGroup));

			PXDefaultAttribute.SetPersistingCheck<InventoryItem.valMethod>(Item.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<InventoryItem.cOGSAcctID>(Item.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<InventoryItem.cOGSSubID>(Item.Cache, null, PXPersistingCheck.Nothing);
		}

		public PXSetup<INSetup> INSetup;

		public PXSelect<INItemClass> Class;

		public PXSelect<InventoryItem> Item;

		public PXSave<INItemClass> SaveClass;
		public PXCancel<INItemClass> CancelClass;
		public PXFirst<INItemClass> FirstClass;
		public PXPrevious<INItemClass> PrevClass;
		public PXNext<INItemClass> NextClass;
		public PXLast<INItemClass> LastClass;

		public PXSave<InventoryItem> SaveItem;
		public PXCancel<InventoryItem> CancelItem;
		public PXFirst<InventoryItem> FirstItem;
		public PXPrevious<InventoryItem> PrevItem;
		public PXNext<InventoryItem> NextItem;
		public PXLast<InventoryItem> LastItem;

		protected override IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if ((group.SpecificModule == null || group.SpecificModule == typeof(InventoryItem).Namespace)
					&& (Class.Current != null || group.SpecificType == null || group.SpecificType == typeof(CS.SegmentValue).FullName || group.SpecificType == typeof(InventoryItem).FullName)
					|| PX.SM.UserAccess.IsIncluded(getMask(), group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}

		protected override byte[] getMask()
		{
			byte[] mask = null;
			if (Item.Current != null)
			{
				mask = Item.Current.GroupMask;
			}
			else if (Class.Current != null)
			{
				mask = Class.Current.GroupMask;
			}
			return mask;
		}

		public override void Persist()
		{
			if (Item.Current != null)
			{
				PopulateNeighbours<InventoryItem>(Item, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.IN.InventoryItem>();
			}
			else if (Class.Current != null)
			{
				PopulateNeighbours<INItemClass>(Class, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.IN.INItemClass>();
			}
			else
			{
				return;
			}
			base.Persist();
		}
	}

	public class INAccessDetailByClass : INAccessDetailItem
	{
	}

	public class INAccessDetailByItem : INAccessDetailItem
	{
	}

}
