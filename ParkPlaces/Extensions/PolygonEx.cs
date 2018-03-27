﻿using System;
using ParkPlaces.IO;
using ParkPlaces.IO.Database;
using ParkPlaces.Map_shapes;

namespace ParkPlaces.Extensions
{
    public static class PolygonEx
    {
        /// <summary>
        /// Return the Id of a zone that is the same
        /// as in the database
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static int GetZoneId(this Polygon polygon)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException(nameof(polygon));
            }

            if (polygon.Tag is PolyZone zone)
                return int.Parse(zone.Id);
            return -1;
        }

        /// <summary>
        /// Return the info object associated with a polygon
        /// shown on the map
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static PolyZone GetZoneInfo(this Polygon polygon)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException(nameof(polygon));
            }

            if (polygon.Tag is PolyZone zone)
                return zone;
            return null;
        }

        public static void UpdateZoneInfo(this Polygon polygon)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException(nameof(polygon));
            }
            Sql.Instance.UpdateZoneInfo(polygon.GetZoneInfo());
        }
    }
}
