﻿using System.Collections.Generic;
using Newtonsoft.Json;

// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ParkPlaces.IO;
//
//    var data = Dto2Object.FromJson(jsonString);

namespace PPNetLib.Prototypes
{
    public partial class Dto2Object
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("zones")]
        public List<PolyZone> Zones { get; set; }
    }

    public class PolyZone
    {
        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("distance")]
        public long Distance { get; set; }

        [JsonProperty("geometry")]
        public List<Geometry> Geometry { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("telepules")]
        public string Telepules { get; set; }

        [JsonProperty("zoneid")]
        public string Zoneid { get; set; }

        [JsonProperty("fee")]
        public long Fee { get; set; }

        [JsonProperty("service_na")]
        public string ServiceNa { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("timetable")]
        public string Timetable { get; set; }
    }

    public class Geometry
    {
        public int Id { get; set; }

        [JsonProperty("lat")]
        public double Lat
        {
            get => _internalPoint.Lat;
            set
            {
                _internalPoint.Lat = value;
                IsModified = true;
            }
        }

        [JsonProperty("long")]
        public double Lng
        {
            get => _internalPoint.Lng;
            set
            {
                _internalPoint.Lng = value;
                IsModified = true;
            }
        }

        /// <summary>
        /// Indicates whether this point has been modified and needs to be updated
        /// in the database
        /// </summary>
        public bool IsModified { get; set; }

        private PointLatLng _internalPoint;

        [JsonConstructor]
        public Geometry(int id=0)
        {
            Id = id;
            _internalPoint = new PointLatLng();
        }

        public Geometry(double lat, double lng, int id)
        {
            _internalPoint = new PointLatLng(lat, lng);
            Id = id;
        }

        public static Geometry FromLatLng(PointLatLng input)
        {
            return new Geometry(input.Lat, input.Lng, 0);
        }
    }

    public partial class Dto2Object
    {
        public static Dto2Object FromJson(string json) => JsonConvert.DeserializeObject<Dto2Object>(json, Converter.Settings);

        public void Reset()
        {
            foreach (var zone in Zones)
            {
                zone.Geometry.Clear();
                zone.Geometry.TrimExcess();
            }

            Zones.Clear();
            Zones.TrimExcess();
        }
    }

    public static class Serialize
    {
        public static string ToJson(this Dto2Object self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None
        };
    }
}