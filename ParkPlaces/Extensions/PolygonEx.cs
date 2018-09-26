using System;
using Newtonsoft.Json;

using ParkPlaces.Map_shapes;
using PPNetClient;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;

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
            Client.Instance.Send(new UpdateZoneReq() { Zone = JsonConvert.SerializeObject(polygon.GetZoneInfo())});
        }

        public static int GetId(this Polygon polygon)
        {
            if(polygon.Tag is PolyZone zone)
                return int.Parse(zone.Id);
            return -1;
        }

        public static Geometry ToGeometry(this GMap.NET.PointLatLng input, int id)
        {
            return new Geometry(input.Lat, input.Lng, id) { IsModified = true };
        }
    }
}
