using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        [TestMethod]
        public async Task TestMethod1()
        {
            var napi = await NemApi.CreateApi();
            var uri = await napi.GetCityPlan<List<PolyZone>>(napi.Cities[0]);

            Assert.AreEqual(true, true);
        }
    }
}