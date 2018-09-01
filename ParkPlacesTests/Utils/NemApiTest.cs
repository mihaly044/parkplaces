using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.DotUtils.Extensions;
using ParkPlaces.DotUtils.Net;
using ParkPlaces.IO;
using PPNetLib.Prototypes;

namespace ParkPlacesTests.Utils
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für NemApiTest
    /// </summary>
    [TestClass]
    public class NemApiTest
    {
#pragma warning disable 649
        private NemApi _api;
#pragma warning restore 649

        /// <summary>
        /// Get results for a city
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestGetCityPlan()
        {
            var napi = await GetNem();
            var polyData = await napi.GetCityPlan<List<PolyZone>>(napi.Cities[0]);
            Assert.IsInstanceOfType(polyData, typeof(List<PolyZone>));
        }

        /// <summary>
        /// Gets all results for all cities
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestGetAllResults()
        {
            var napi = await GetNem();

            var polyData = await napi.Cities
                .ParallelForEachTaskAsync(async x => await napi.GetCityPlan<List<PolyZone>>(x));

            Assert.IsInstanceOfType(polyData, typeof(List<List<PolyZone>>));
        }

        private async Task<NemApi> GetNem()
        {
            return _api ?? await NemApi.CreateApi();
        }
    }
}