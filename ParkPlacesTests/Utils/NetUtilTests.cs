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

#pragma warning disable 618
            string actual = NetUtil.GetStringFromUrl(url);
#pragma warning restore 618
            Assert.AreEqual(expected, actual);
        }
    }
}