using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.IO.Tests
{
    [TestClass()]
    public class Dto2ObjectTests
    {
        [TestMethod()]
        public void FromURISyncTest()
        {
            Dto2Object d = Dto2Object.FromURISync("https://www.nemzetimobilfizetes.hu/parking_purchases/search_parking_zones/?search=szeged");
            

            Assert.Fail();
        }
    }
}