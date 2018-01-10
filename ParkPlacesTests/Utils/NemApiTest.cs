using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.DotUtils.Extensions;
using ParkPlaces.DotUtils.Net;
using ParkPlaces.IO;

namespace ParkPlacesTests.Utils
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für NemApiTest
    /// </summary>
    [TestClass]
    public class NemApiTest
    {
        private NemApi _api;

        public NemApiTest()
        {
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var napi = await GetNem();
            var polyData = await napi.GetCityPlan<List<PolyZone>>(napi.Cities[0]);

            Assert.AreEqual(true, true);
        }

        /// <summary>
        /// Gets all results for all cities
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMethod2()
        {
            var napi = await GetNem();

            var lel = await napi.Cities.ParallelForEachTaskAsync(async x =>
            {
                var res = await napi.GetCityPlan<List<PolyZone>>(x);
                return res;
            });

            var polyData = await napi.GetCityPlan<List<PolyZone>>(napi.Cities[0]);

            Assert.AreEqual(true, true);
        }

        private async Task<NemApi> GetNem()
        {
            return _api ?? await NemApi.CreateApi();
        }
    }
}