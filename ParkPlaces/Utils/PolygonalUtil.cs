using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMap.NET;

namespace ParkPlaces.Utils
{
    /// <summary>
    /// Reference: https://github.com/googlemaps/android-maps-utils/blob/9085396a1cb5703db568241937ffa330ff099d4e/library/src/com/google/maps/android/SphericalUtil.java
    /// </summary>
    public static class PolygonalUtil
    {
        private const double EarthRadius = 6378137;

        public static double ToRadians(double d) => (d * Math.PI) / 180;

        private static double PolarTriangleArea(double tan1, double lng1, double tan2, double lng2)
        {
            var deltaLng = lng1 - lng2;
            var t = tan1 * tan2;
            return 2 * Math.Atan2(t * Math.Sin(deltaLng), 1 + t * Math.Cos(deltaLng));
        }

        public static double ComputeSignedArea(List<PointLatLng> path, double radius = EarthRadius)
        {
            var size = path.Count;
            if (size < 3) { return 0; }
            double total = 0;
            var prev = path[size - 1];
            var prevTanLat = Math.Tan((Math.PI / 2 - ToRadians(prev.Lat)) / 2);
            var prevLng = ToRadians(prev.Lng);
            // For each edge, accumulate the signed area of the triangle formed by the North Pole
            // and that edge ("polar triangle").
            foreach (var point in path)
            {
                var tanLat = Math.Tan((Math.PI / 2 - ToRadians(point.Lat)) / 2);
                var lng = ToRadians(point.Lng);
                total += PolarTriangleArea(tanLat, lng, prevTanLat, prevLng);
                prevTanLat = tanLat;
                prevLng = lng;
            }
            return total * (radius * radius);
        }

        public static double ComputeArea(List<PointLatLng> path)
        {
            return Math.Abs(ComputeSignedArea(path));
        }
    }
}