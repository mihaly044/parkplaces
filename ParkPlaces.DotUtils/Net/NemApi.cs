using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParkPlaces.DotUtils.Extensions;

namespace ParkPlaces.DotUtils.Net
{
    public class NemApi
    {
        private const string BaseUri = "https://www.nemzetimobilfizetes.hu/parking_purchases";
        private const string ContentUri = BaseUri + "/zonainfo/content";
        private const string CityInfoUri = BaseUri + "/geocode/?search={0}";
        private const string ParkingApiUri = BaseUri + "/search_parking_zones/?search={0}&time={1}";    //BUDAPEST - 2018-01-10 15:56:07

        private static Regex _regexCity = new Regex("option value=\"\\d+\">(.*?)<", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public List<string> Cities { get; private set; }

        private NemApi()
        {
        }

        /// <summary>
        /// Creates an instance of NemApi
        /// </summary>
        /// <param name="cities">Allows you to bypass initialisation. May be faster</param>
        /// <returns></returns>
        public static async Task<NemApi> CreateApi(List<string> cities = null)
        {
            var res = new NemApi();

            if (cities == null)
            {
                await res.Initialize();
            }
            else
            {
                res.Cities = cities.ToList();//creating a copy
            }

            return res;
        }

        private async Task Initialize()
        {
            var content = await DownloadString(ContentUri);

            Cities = _regexCity.Matches(content).Cast<Match>()
                .Where(m => m.Groups.Count >= 2)
                .Select(m => m.Groups[1].Value)
                .ToList();
        }

        /// <summary>
        /// If <see cref="T"/> is string, it will return the raw data, else it will deserialize the data to the required Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="city"></param>
        /// <param name="reqdt"></param>
        /// <returns></returns>
        public async Task<T> GetCityPlan<T>(string city, DateTime reqdt = default(DateTime)) where T : class
        {
            var cPlan = await InternalGetCityPlan(city, reqdt);

            if (typeof(T) == typeof(string))
            {
                return cPlan as T;
            }

            return JsonConvert.DeserializeObject<T>(cPlan, new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
            });
        }

        private async Task<string> InternalGetCityPlan(string city, DateTime reqdt = default(DateTime))
        {
            reqdt = !reqdt.IsDefault() ? reqdt : DateTime.Now;
            var rUri = string.Format(ParkingApiUri, city, reqdt.ToString("yyyy-MM-dd HH:mm:ss"));

            return await DownloadString(rUri);
        }

        private async Task<string> DownloadString(string uri)
        {
            using (var wc = WebClientFactory.GetClient())
            {
                wc.Headers.Add("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
                var content = await wc.DownloadStringTaskAsync(uri);
                return content;
            }
        }
    }
}