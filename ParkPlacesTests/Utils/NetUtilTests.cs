using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.Utils.Tests
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