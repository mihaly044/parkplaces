﻿using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ParkPlaces.IO;
//
//    var data = Dto2Object.FromJson(jsonString);

namespace ParkPlaces.IO
{
    public partial class Dto2Object
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("zones")]
        public List<PolyZone> Zones { get; set; }
    }

    public partial class PolyZone
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

    public partial class Geometry
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public partial class Dto2Object
    {
        public static Dto2Object FromJson(string json) => JsonConvert.DeserializeObject<Dto2Object>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Dto2Object self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
