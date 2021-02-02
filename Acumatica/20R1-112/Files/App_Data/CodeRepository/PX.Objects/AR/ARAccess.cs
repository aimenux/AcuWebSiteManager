using System;
using System.Collections;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	public class ARAccessDetail : PX.SM.UserAccess
	{
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(CustomerAttribute.DimensionName,
			typeof(Search2<Customer.acctCD,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, And<Contact.contactID, Equal<Customer.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defAddressID>>>>>>),
			typeof(Customer.acctCD),
			typeof(Customer.acctCD), typeof(Customer.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
		   )]
		protected virtual void Customer_AcctCD_CacheAttached(PXCache sender)
		{
		}

		public ARAccessDetail()
		{
			ARSetup setup = ARSetup.Current;
			Customer.Cache.AllowDelete = false;
			Customer.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Customer.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Customer.acctCD>(Customer.Cache, null);
			Views.Caches.Remove(Groups.GetItemType());
			Views.Caches.Add(Groups.GetItemType());
		}

		public PXSelect<Customer> Customer;

		public PXSave<Customer> SaveCustomer;
		public PXCancel<Customer> CancelCustomer;
		public PXFirst<Customer> FirstCustomer;
		public PXPrevious<Customer> PrevCustomer;
		public PXNext<Customer> NextCustomer;
		public PXLast<Customer> LastCustomer;

		public PXSetup<ARSetup> ARSetup;

		protected override IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if (group.SpecificModule == null || group.SpecificModule == typeof(Customer).Namespace
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
			else if (Customer.Current != null)
			{
				mask = Customer.Current.GroupMask;
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
			else if (Customer.Current != null)
			{
				PopulateNeighbours<Customer>(Customer, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.AR.Customer>();
			}
			else
			{
				return;
			}
			base.Persist();
		}
	}

	public class ARAccess : PX.SM.BaseAccess
	{
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(CustomerAttribute.DimensionName,
			typeof(Search2<Customer.acctCD,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, And<Contact.contactID, Equal<Customer.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defAddressID>>>>>>),
			typeof(Customer.acctCD),
			typeof(Customer.acctCD), typeof(Customer.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
		   )]
		protected virtual void Customer_AcctCD_CacheAttached(PXCache sender)
		{
		}

		public class ARRelationGroupCustomerSelectorAttribute : PXCustomSelectorAttribute
		{
			public ARRelationGroupCustomerSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return ARAccess.GroupDelegate(_Graph, false);
			}
		}
		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[ARRelationGroupCustomerSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected virtual void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		public ARAccess()
		{
			ARSetup setup = ARSetup.Current;
			Customer.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Customer.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Customer.included>(Customer.Cache, null);
			PXUIFieldAttribute.SetEnabled<Customer.acctCD>(Customer.Cache, null);
		}
		public PXSelect<Customer> Customer;
		protected virtual IEnumerable customer(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (Customer item in PXSelect<Customer,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or<Match<Required<Customer.groupMask>>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						Customer.Current = item;
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
								Customer.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Customer.Current = item;
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
		
		public PXSetup<ARSetup> ARSetup;

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(accountsReceivableModule).Namespace;
		}
		
		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (group != null)
			{
				if (String.IsNullOrEmpty(group.GroupName))
				{
					Save.SetEnabled(false);
					Customer.Cache.AllowInsert = false;
				}
				else
				{
					Save.SetEnabled(true);
					Customer.Cache.AllowInsert = true;
				}
			}
		}
		protected virtual void Customer_AcctCD_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void Customer_AcctCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelectorAttribute.Select<Customer.acctCD>(sender, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, "CustomerID");
			}
		}
		protected virtual void Customer_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				Customer cust = PXSelectorAttribute.Select<Customer.acctCD>(sender, e.Row) as Customer;
				if (cust != null)
				{
					cust.Included = true;
					PXCache<Customer>.RestoreCopy((Customer)e.Row, cust);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void Customer_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Customer cust = e.Row as Customer;
			if (cust != null && sender.GetStatus(cust) == PXEntryStatus.Notchanged)
			{
				if (cust.GroupMask != null)
				{
					foreach (byte b in cust.GroupMask)
					{
						if (b != 0x00)
						{
							cust.Included = true;
							return;
						}
					}
				}
				else
				{
					cust.Included = true;
				}
			}
		}
		protected virtual void Customer_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Customer cust = e.Row as Customer;
			PX.SM.RelationGroup group = Group.Current;
			if (cust != null && cust.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (cust.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = cust.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					cust.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (cust.Included == true)
					{
						cust.GroupMask[i] = (byte)(cust.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						cust.GroupMask[i] = (byte)(cust.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}
		public override void Persist()
		{
			populateNeighbours<PX.SM.Users>(Users);
			populateNeighbours<Customer>(Customer);
			populateNeighbours<PX.SM.Users>(Users);
			base.Persist();
			PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.AR.Customer>();
		}

		static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<customerType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(Customer).Namespace)
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
