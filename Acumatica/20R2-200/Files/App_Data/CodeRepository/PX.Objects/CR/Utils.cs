using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public static class BAccountUtility
	{
		public static BAccount FindAccount(PXGraph graph, int? aBAccountID)
		{
			BAccount acct = null;
			if (aBAccountID.HasValue)
			{
				PXSelectBase<BAccount> sel = new PXSelectReadonly<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>(graph);
				acct = (BAccount)sel.View.SelectSingle(aBAccountID);
			}
			return acct;

		}
		public static void ViewOnMap(Address aAddr)
		{
			PX.Data.MapRedirector map = SitePolicy.CurrentMapRedirector;
			if (map != null && aAddr != null)
			{
				PXGraph graph = new PXGraph();
				Country country = PXSelectorAttribute.Select<Address.countryID>(graph.Caches[typeof(Address)], aAddr) as Country;
				map.ShowAddress(country != null ? country.Description : aAddr.CountryID, aAddr.State, aAddr.City, aAddr.PostalCode, aAddr.AddressLine1, aAddr.AddressLine2, aAddr.AddressLine3);
			}

		}

		public static void ViewOnMap(CRAddress aAddr)
		{
			PX.Data.MapRedirector map = SitePolicy.CurrentMapRedirector;
			if (map != null && aAddr != null)
			{
				PXGraph graph = new PXGraph();
				Country country = PXSelectorAttribute.Select<CRAddress.countryID>(graph.Caches[typeof(CRAddress)], aAddr) as Country;
				map.ShowAddress(country != null ? country.Description : aAddr.CountryID, aAddr.State, aAddr.City, aAddr.PostalCode, aAddr.AddressLine1, aAddr.AddressLine2, null);
			}
		}

		public static void ViewOnMap<TAddress, FCountryID>(CS.IAddress aAddr)
			where TAddress : class, IBqlTable, CS.IAddress, new()
			where FCountryID : IBqlField
		{
			PX.Data.MapRedirector map = SitePolicy.CurrentMapRedirector;
			if (map != null && aAddr != null)
			{
				PXGraph graph = new PXGraph();
				Country country = PXSelectorAttribute.Select<FCountryID>(graph.Caches[typeof(TAddress)], aAddr) as Country;
				map.ShowAddress(country != null ? country.Description : aAddr.CountryID, aAddr.State, aAddr.City, aAddr.PostalCode, aAddr.AddressLine1, aAddr.AddressLine2, null);
			}
		}
	}
}
