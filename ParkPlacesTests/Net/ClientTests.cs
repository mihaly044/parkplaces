using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.Net.Tests
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