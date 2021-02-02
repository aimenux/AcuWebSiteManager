using System;

namespace PX.Objects.FS
{
    public static class Haversine
    {
        public const double Rm = 3960; // means radius of the earth (miles) at 39 degrees from the equator
        public const double Rk = 6371; // means radius of the earth (km) at 39 degrees from the equator

        public enum DistanceUnit { Miles, Kilometers };

        public static double calculate(LatLng from, LatLng to, DistanceUnit unit)
        {
            return calculate(from.Latitude, from.Longitude, to.Latitude, to.Longitude, unit);
        }

        public static double calculate(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit)
        {
            double R = (unit == DistanceUnit.Miles) ? Rm : Rk;
            double dLat = toRadians(lat2 - lat1);
            double dLon = toRadians(lon2 - lon1);
            double rlat1 = toRadians(lat1);
            double rlat2 = toRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rlat1) * Math.Cos(rlat2);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }

        public static double toRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
