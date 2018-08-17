using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.IO;
using System.Threading.Tasks;

namespace ParkPlacesTests.IO
{
    [TestClass()]
    public class IoHandlerTests
    {
        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            var result = await IoHandler.Instance.UpdateAsync(false, true);
            Assert.IsNotNull(result);
        }
    }
}