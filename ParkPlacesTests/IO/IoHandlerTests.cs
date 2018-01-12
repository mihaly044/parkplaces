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
    public class IoHandlerTests
    {
        [TestMethod()]
        public async void UpdateAsyncTest()
        {
            var result = await IoHandler.Instance.UpdateAsync(false, true);
            Assert.IsNotNull(result);
        }
    }
}