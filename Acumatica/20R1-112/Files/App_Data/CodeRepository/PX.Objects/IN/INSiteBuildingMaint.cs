using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System.Collections;

namespace PX.Objects.IN
{
	public class INSiteBuildingMaint : PXGraph<INSiteBuildingMaint, INSiteBuilding>
	{
		public INSiteBuildingMaint()
		{
			Sites.Cache.AllowInsert = Sites.Cache.AllowUpdate = false;
		}

		#region Views
		public SelectFrom<INSiteBuilding>.View Buildings;
		public
			SelectFrom<INSite>.
			Where<
				INSite.FK.Building.SameAsCurrent.
				And<INSite.siteID.IsNotEqual<SiteAttribute.transitSiteID>>.
				And<Match<AccessInfo.userName.FromCurrent>>>.
			View Sites;
		public PXSetup<Branch>.Where<Branch.branchID.IsEqual<INSiteBuilding.branchID.AsOptional>> Branch;
		public
			SelectFrom<Address>.
			Where<
				Address.bAccountID.IsEqual<Branch.bAccountID.FromCurrent>.
				And<Address.addressID.IsEqual<INSiteBuilding.addressID.FromCurrent>>>.
			View Address;
		#endregion

		#region Buttons
		public PXChangeID<INSiteBuilding, INSiteBuilding.buildingCD> changeID;

		public PXAction<INSiteBuilding> validateAddresses;
		[PXButton, PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			if (Buildings.Current != null)
			{
				Address address = Address.Current;
				if (address != null && address.IsValidated == false)
					PXAddressValidator.Validate(this, address, true);
			}
			return adapter.Get();
		}

		public PXAction<INSiteBuilding> viewOnMap;
		[PXButton, PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{
			BAccountUtility.ViewOnMap(Address.Current);
			return adapter.Get();
		}
		#endregion

		#region Cache Attached
		[PXDefault(typeof(SearchFor<Branch.countryID>.In<SelectFrom<Branch>.Where<Branch.branchID.IsEqual<INSiteBuilding.branchID.FromCurrent>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Address.countryID> e) { }
		#endregion

		#region Event Handlers
		#region INSiteBuilding
		protected virtual void _(Events.RowInserted<INSiteBuilding> e)
		{
			try
			{
				Address.Cache.Insert(new Address { BAccountID = Branch.Current?.BAccountID });
			}
			finally
			{
				Address.Cache.IsDirty = false;
			}
		}

		protected virtual void _(Events.RowUpdated<INSiteBuilding> e)
		{
			if (!e.Cache.ObjectsEqual<INSiteBuilding.branchID>(e.Row, e.OldRow))
			{
				bool found = false;
				foreach (Address record in Address.Cache.Inserted)
				{
					record.BAccountID = Branch.Current?.BAccountID;
					record.CountryID = Branch.Current?.CountryID;
					found = true;
				}

				if (!found)
				{
					object old_branch = Branch.View.SelectSingleBound(new object[] { e.OldRow });
					Address addr = (Address)Address.View.SelectSingleBound(new object[] { old_branch, e.OldRow }) ?? new Address();

					addr.BAccountID = Branch.Current.BAccountID;
					addr.CountryID = Branch.Current.CountryID;
					addr.AddressID = null;
					Address.Cache.Insert(addr);
				}
				else
				{
					Address.Cache.Normalize();
				}
			}
		}

		protected virtual void _(Events.RowDeleted<INSiteBuilding> e)
		{
			Address.Cache.Delete(Address.Current);
		}
		#endregion

		#region INSite
		protected virtual void _(Events.RowDeleting<INSite> e)
		{
			if (e.Row != null)
			{
				e.Row.BuildingID = null;
				e.Cache.Update(e.Row);
				e.Cancel = true;
				Sites.View.RequestRefresh();
			}
		}
		#endregion

		#region Address
		protected virtual void _(Events.RowInserted<Address> e)
		{
			if (e.Row != null)
				Buildings.Current.AddressID = e.Row.AddressID;
		}

		protected virtual void _(Events.FieldDefaulting<Address, Address.bAccountID> e)
		{
			if (Branch.Current != null)
			{
				e.NewValue = Branch.Current.BAccountID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<Address, Address.countryID> e)
		{
			e.Row.State = null;
			e.Row.PostalCode = null;
		}
		#endregion
		#endregion
	}
}