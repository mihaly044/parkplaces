using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using ParkPlaces.Map_shapes;


namespace ParkPlacesTests.Utils
{
    [TestClass()]
    public class PolygonalUtilTests
    {
        private readonly List<PointLatLng> _path = new List<PointLatLng>(4)
        {
            new PointLatLng(47.454921311932566d, 19.03005838394165d),
            new PointLatLng(47.455240509746986d, 19.032289981842041d),
            new PointLatLng(47.455138947016209d, 19.032268524169922d),
            new PointLatLng(47.454848766704529d, 19.03007984161377d)
        };

        [TestMethod()]
        public void ToRadiansTest()
        {
            double degrees = 90d;
            double expected = 1.5707;
            double actual = PolygonalUtil.ToRadians(degrees);

            Assert.AreEqual(expected, actual, 4, "Inaccurate degrees to radians conversion");
        }

        [TestMethod()]
        public void ComputeSignedAreaTest()
        {
            double expected = -1612.1563d;
            double actual = PolygonalUtil.ComputeSignedArea(_path);

            Assert.AreEqual(expected, actual, 4, "Signed area calculation is inaccurate");
        }

        [TestMethod()]
        public void ComputeAreaTest()
        {
            double expected = 1612.1563d;
            double actual = PolygonalUtil.ComputeArea(_path);

            Assert.AreEqual(expected, actual, 4, "Signed area calculation is inaccurate");
        }
    }
}