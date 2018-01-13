﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.IO;

namespace ParkPlacesTests.IO
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