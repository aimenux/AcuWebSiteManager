using System;

namespace PX.Data
{
    public class GoogleMapLatLongRedirector : GoogleMapRedirector
    {
        #region Methods
        public void ShowAddressByLocation(decimal? latitude, decimal? longitude)
        {
            string latLong           = "loc:";
            string countryDummy      = string.Empty;
            string stateDummy        = string.Empty;
            string cityDummy         = string.Empty;
            string postalCodeDummy   = string.Empty;
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