using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPNetClient;

namespace ParkPlacesTests.Net
{
    [TestClass()]
    public class ClientTests
    {
        [TestMethod()]
        public void ConnectShouldThrowException()
        {
            var c = new Client();
            var shouldThrowException = true;

            Assert.ThrowsException<ArgumentNullException>(() => {
                c.Connect(shouldThrowException);
            });
        }
    }
}