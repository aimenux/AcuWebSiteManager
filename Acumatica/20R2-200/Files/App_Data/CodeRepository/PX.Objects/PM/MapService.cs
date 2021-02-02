using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class MapService
    {
        private readonly PXGraph graph;

        public MapService(PXGraph graph)
        {
            this.graph = graph;
        }

        public virtual void viewAddressOnMap(IAddressLocation location, string siteAddress)
        {
            var mapRedirector = SitePolicy.CurrentMapRedirector;
            if (mapRedirector == null)
            {
                return;
            }
            if (location.Latitude == null && location.Longitude == null)
            {
                ShowLocationByAddress(mapRedirector, location, siteAddress);
            }
            else
            {
                ShowLocationByCoordinates(location.Latitude, location.Longitude);
            }
        }

        private void ShowLocationByAddress(MapRedirector mapRedirector, IAddressLocation location, string siteAddress)
        {
            var country = GetCountry(location.CountryID)?.Description ?? location.CountryID;
            mapRedirector.ShowAddress(country, location.State, location.City, location.PostalCode, siteAddress, null,
                null);
        }

        private Country GetCountry(string code)
        {
            return SelectFrom<Country>
                .Where<Country.countryID.IsEqual<P.AsString>>.View
                .Select(graph, code);
        }

        private static void ShowLocationByCoordinates(decimal? latitude, decimal? longitude)
        {
            new GoogleMapLatLongRedirector().ShowAddressByLocation(latitude, longitude);
        }

        public class GoogleMapLatLongRedirector : GoogleMapRedirector
        {
            #region Methods
            public void ShowAddressByLocation(decimal? latitude, decimal? longitude)
            {
                string latLong = "loc:";
                string countryDummy = string.Empty;
                string stateDummy = string.Empty;
                string cityDummy = string.Empty;
                string postalCodeDummy = string.Empty;
                string addressLine1Dummy = string.Empty;
                string addressLine2Dummy = string.Empty;

                if (latitude == null)
                {
                    latitude = 0;
                }

                if (longitude == null)
                {
                    longitude = 0;
                }

                latLong += Convert.ToString(latitude) + "+" + Convert.ToString(longitude);

                ShowAddress(countryDummy, stateDummy, cityDummy, postalCodeDummy, addressLine1Dummy, addressLine2Dummy, latLong);
            }
            #endregion
        }
    }
}
