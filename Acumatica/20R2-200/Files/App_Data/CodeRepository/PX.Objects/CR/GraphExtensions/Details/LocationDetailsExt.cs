using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.CR.Extensions.Relational;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.CR.Extensions
{
	public delegate bool ValidateAddressesDelegate();

	/// <summary>
	/// Represents the Locations grid
	/// </summary>
	public abstract class LocationDetailsExt<TGraph, TDefContactAddress, TMaster, TBAccountID> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TDefContactAddress : PXGraphExtension<TGraph>    // DefContactAddress graph ext
		where TMaster : BAccount, IBqlTable, new()
		where TBAccountID : class, IBqlField
	{
		#region Ctor

		public override void Initialize()
		{
			base.Initialize();

			Locations.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.accountLocations>();
		}

		#endregion

		#region Views

		[PXViewName(Messages.Locations)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		public PXSelectJoin<
				CRLocation,
			LeftJoin<Address,
				On<Address.addressID, Equal<CRLocation.defAddressID>>>,
			Where<
				CRLocation.bAccountID, Equal<Current<TBAccountID>>>>
			Locations;

		#endregion

		#region Actions
		public PXAction<TMaster> RefreshLocation;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual void refreshLocation()
		{
			Base.SelectTimeStamp();
			Base.Caches<Location>().Clear();
		}

		public PXAction<TMaster> NewLocation;
		[PXUIField(DisplayName = Messages.AddNewLocation)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual void newLocation()
		{
			var master = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (master == null || master.BAccountID == null) return;

			LocationMaint graph = GetLocationsGraph(master);

			CRLocation baccLocation = Locations.SelectSingle();
			var newLocation = (Location)graph.Location.Cache.CreateInstance();
			newLocation.BAccountID = master.BAccountID;

			string locType;
			switch (master.Type)
			{
				case BAccountType.VendorType:
					locType = LocTypeList.VendorLoc;
					break;
				case BAccountType.CustomerType:
				case BAccountType.EmpCombinedType:
					locType = LocTypeList.CustomerLoc;
					break;
				case BAccountType.CombinedType:
					locType = LocTypeList.CombinedLoc;
					break;
				default:
					locType = baccLocation.LocType;
					break;
			}
			newLocation.LocType = locType;

			graph.Location.Insert(newLocation);
			graph.Location.Cache.IsDirty = false;

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<TMaster> ViewLocation;
		[PXUIField(DisplayName = Messages.ViewLocation, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable viewLocation(PXAdapter adapter)
		{
			TMaster master = Base.Caches[typeof(TMaster)].Current as TMaster;

			if (this.Locations.Current != null && master != null && Base.Caches[typeof(TMaster)].GetStatus(master) != PXEntryStatus.Inserted)
			{
				CRLocation current = this.Locations.Current;

				LocationMaint graph = GetLocationsGraph(master);

				graph.Location.Current = graph.Location.Search<CRLocation.locationID>(current.LocationID, master.AcctCD);

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

		#region Events

		protected virtual void _(Events.RowSelected<CRLocation> e)
		{
			var row = e.Row as CRLocation;
			if (row == null) return;

			bool CustomerDetailsVisible = (row.LocType == LocTypeList.CustomerLoc || row.LocType == LocTypeList.CombinedLoc);

			PXUIFieldAttribute.SetEnabled<CRLocation.cTaxZoneID>(e.Cache, null, CustomerDetailsVisible);
			PXUIFieldAttribute.SetEnabled<CRLocation.cAvalaraCustomerUsageType>(e.Cache, null, CustomerDetailsVisible);
			PXUIFieldAttribute.SetEnabled<CRLocation.cBranchID>(e.Cache, null, CustomerDetailsVisible);
		}

		protected virtual void _(Events.RowSelected<TMaster> e)
		{
			var row = e.Row as TMaster;
			if (row == null) return;

			var isNotInserted = e.Cache.GetStatus(row) != PXEntryStatus.Inserted;

			var isCustomerOrCombined = row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType;
			var isCustomerOrProspect = row.Type == BAccountType.CustomerType || row.Type == BAccountType.ProspectType;
			var isCustomerOrProspectOrCombined = isCustomerOrProspect || row.Type == BAccountType.CombinedType;

			NewLocation.SetEnabled(isNotInserted);

			PXUIFieldAttribute.SetVisible(Locations.Cache, null, typeof(CRLocation.cPriceClassID).Name, isCustomerOrProspectOrCombined);
			PXUIFieldAttribute.SetVisible(Locations.Cache, null, typeof(CRLocation.cSalesAcctID).Name, isCustomerOrCombined);
			PXUIFieldAttribute.SetVisible(Locations.Cache, null, typeof(CRLocation.cSalesSubID).Name, isCustomerOrCombined);
		}

		#endregion

		#region Methods

		public virtual LocationMaint GetLocationsGraph(TMaster master)
		{
			LocationMaint graph;

			switch (master.Type)
			{
				case BAccountType.VendorType:
					graph = PXGraph.CreateInstance<VendorLocationMaint>();
					break;

				case BAccountType.CustomerType:
					graph = PXGraph.CreateInstance<CustomerLocationMaint>();
					break;

				default:

					if (typeof(Customer).IsAssignableFrom(Base.PrimaryItemType))
					{
						graph = PXGraph.CreateInstance<CustomerLocationMaint>();
					}
					else if (typeof(Vendor).IsAssignableFrom(Base.PrimaryItemType))
					{
						graph = PXGraph.CreateInstance<VendorLocationMaint>();
					}
					else
					{
						graph = PXGraph.CreateInstance<AccountLocationMaint>();
					}

					break;
			}

			return graph;
		}

		#endregion
	}
}
