using System;
using System.Collections;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AP
{
	public class APAccessDetail : PX.SM.UserAccess
	{
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Vendor ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(VendorAttribute.DimensionName,
			typeof(Search2<Vendor.acctCD,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<Vendor.bAccountID>, And<Contact.contactID, Equal<Vendor.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<Vendor.bAccountID>, And<Address.addressID, Equal<Vendor.defAddressID>>>>>>),
			typeof(Vendor.acctCD),
			typeof(Vendor.acctCD), typeof(Vendor.acctName), typeof(Vendor.vendorClassID), typeof(Vendor.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
		   )]
		protected virtual void Vendor_AcctCD_CacheAttached(PXCache sender)
		{
		}

		public APAccessDetail()
		{
			APSetup setup = APSetup.Current;
			Vendor.Cache.AllowDelete = false;
			Vendor.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Vendor.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Vendor.acctCD>(Vendor.Cache, null);
			Views.Caches.Remove(Groups.GetItemType());
			Views.Caches.Add(Groups.GetItemType());
		}

		public PXSelect<Vendor> Vendor;

		public PXSave<Vendor> SaveVendor;
		public PXCancel<Vendor> CancelVendor;
		public PXFirst<Vendor> FirstVendor;
		public PXPrevious<Vendor> PrevVendor;
		public PXNext<Vendor> NextVendor;
		public PXLast<Vendor> LastVendor;

		public PXSetup<APSetup> APSetup;

		protected override IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if (group.SpecificModule == null || group.SpecificModule == typeof(Vendor).Namespace
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
			if (User.Current != null)
			{
				mask = User.Current.GroupMask;
			}
			else if (Vendor.Current != null)
			{
				mask = Vendor.Current.GroupMask;
			}
			return mask;
		}

		public override void Persist()
		{
			if (User.Current != null)
			{
				PopulateNeighbours<PX.SM.Users>(User, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			}
			else if (Vendor.Current != null)
			{
				PopulateNeighbours<Vendor>(Vendor, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.AP.Vendor>();
			}
			else
			{
				return;
			}
			base.Persist();
		}
	}

	public class APAccess : PX.SM.BaseAccess
	{
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Vendor ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(VendorAttribute.DimensionName,
			typeof(Search2<Vendor.acctCD,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<Vendor.bAccountID>, And<Contact.contactID, Equal<Vendor.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<Vendor.bAccountID>, And<Address.addressID, Equal<Vendor.defAddressID>>>>>>),
			typeof(Vendor.acctCD),
			typeof(Vendor.acctCD), typeof(Vendor.acctName), typeof(Vendor.vendorClassID), typeof(Vendor.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
		   )]
		protected virtual void Vendor_AcctCD_CacheAttached(PXCache sender)
		{
		}

		public class APRelationGroupVendorSelectorAttribute : PXCustomSelectorAttribute
		{
			public APRelationGroupVendorSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return APAccess.GroupDelegate(_Graph, false);
			}
		}
		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[APRelationGroupVendorSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected virtual void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		public APAccess()
		{
			APSetup setup = APSetup.Current;
			Vendor.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Vendor.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Vendor.included>(Vendor.Cache, null);
			PXUIFieldAttribute.SetEnabled<Vendor.acctCD>(Vendor.Cache, null);
			PXUIFieldAttribute.SetVisible<PX.SM.Users.username>(Caches[typeof(PX.SM.Users)], null);
		}
		public PXSelect<Vendor> Vendor;
		protected virtual IEnumerable vendor(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (Vendor item in PXSelect<Vendor,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or<Match<Required<Vendor.groupMask>>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						Vendor.Current = item;
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
								Vendor.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Vendor.Current = item;
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

		public PXSetup<APSetup> APSetup;
		
		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(accountsPayableModule).Namespace;
		}
		
		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (group != null)
			{
				if (String.IsNullOrEmpty(group.GroupName))
				{
					Save.SetEnabled(false);
					Vendor.Cache.AllowInsert = false;
				}
				else
				{
					Save.SetEnabled(true);
					Vendor.Cache.AllowInsert = true;
				}
			}
		}
		protected virtual void Vendor_AcctCD_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void Vendor_AcctCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelectorAttribute.Select<Vendor.acctCD>(sender, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, "VendorID");
			}
		}
		protected virtual void Vendor_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				Vendor vend = PXSelectorAttribute.Select<Vendor.acctCD>(sender, e.Row) as Vendor;
				if (vend != null)
				{
					vend.Included = true;
					PXCache<Vendor>.RestoreCopy((Vendor)e.Row, vend);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void Vendor_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Vendor vend = e.Row as Vendor;
			if (vend != null && sender.GetStatus(vend) == PXEntryStatus.Notchanged)
			{
				if (vend.GroupMask != null)
				{
					foreach (byte b in vend.GroupMask)
					{
						if (b != 0x00)
						{
							vend.Included = true;
							return;
						}
					}
				}
				else
				{
					vend.Included = true;
				}
			}
		}
		protected virtual void Vendor_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Vendor vend = e.Row as Vendor;
			PX.SM.RelationGroup group = Group.Current;
			if (vend != null && vend.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (vend.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = vend.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					vend.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (vend.Included == true)
					{
						vend.GroupMask[i] = (byte)(vend.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						vend.GroupMask[i] = (byte)(vend.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}
		public override void Persist()
		{
			populateNeighbours<PX.SM.Users>(Users);
			populateNeighbours<Vendor>(Vendor);
			populateNeighbours<PX.SM.Users>(Users);
			base.Persist();
			PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.AP.Vendor>();
		}

		static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<vendorType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(Vendor).Namespace)
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
	}
}
