using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParkPlaces.IO;
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
    }
}
