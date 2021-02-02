using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public class WorkLocationsMaint : PXGraph<WorkLocationsMaint, PRLocation>
	{
		#region Views
		public PXSelect<PRLocation> Locations;
		public PXSelect<Address, Where<Address.addressID, Equal<Current<PRLocation.addressID>>>> Address;
		#endregion

		#region Events
		protected virtual void Address_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Address row = (Address)e.Row;
			if (row != null)
			{
				bool allowAddressEditing = (Locations.Current.BranchID == null);
				PXUIFieldAttribute.SetEnabled<Address.addressLine1>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.addressLine2>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.city>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.countryID>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.state>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.postalCode>(sender, row, allowAddressEditing);
			}
		}

		protected virtual void PRLocation_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			try
			{
				Address addr = new Address();
				addr = (Address)Address.Cache.Insert(addr);
			}
			finally
			{
				Address.Cache.IsDirty = false;
			}
		}

		protected virtual void Address_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null)
			{
				Locations.Current.AddressID = addr.AddressID;
			}
		}

		protected virtual void Address_CountryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Address addr = (Address)e.Row;
			if ((string)e.OldValue != addr.CountryID)
			{
				addr.State = null;
			}
		}

		protected virtual void PRLocation_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PRLocation;
			if (row.BranchID == null)
			{
				row.AddressID = null;
			}
			else
			{
				BAccount rec = PXSelectJoin<BAccount,
											InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>,
											Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
				row.AddressID = rec.DefAddressID;

			}
		}

		protected virtual void _(Events.RowPersisting<Address> e)
		{
			TaxLocationHelpers.AddressPersisting(e);
		}
		#endregion

		#region Buttons
		public PXAction<PRLocation> ViewOnMap;
		[PXUIField(DisplayName = "View On Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewOnMap(PXAdapter adapter)
		{
			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}
		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class WorkLocationsMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<WorkLocationsMaint, PRLocation, Address>
		{
			protected override string AddressView => nameof(Base.Address);
		}
		#endregion
	}
}