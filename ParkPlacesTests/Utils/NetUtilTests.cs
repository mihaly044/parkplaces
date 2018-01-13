using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.Utils;

namespace ParkPlacesTests.Utils
{
    [TestClass()]
    public class NetUtilTests
    {
        [TestMethod()]
        public void GetStringFromUrlTest()
        {
            string url = "https://pastebin.com/raw/apGeNVWT";
            string expected = "test";

            string actual = NetUtil.GetStringFromUrl(url);
            Assert.AreEqual(expected, actual);
        }
    }
}