using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vex472.Erebus.Core.Tests
{
    [TestClass]
    public class ErebusAddressTests
    {
        [TestMethod]
        public void ParseTest()
        {
            var addr = new ErebusAddress("{22-123A--DD9-8}");
            Assert.AreEqual(new ErebusAddress(0x22, 0x123a, 0, 0, 0, 0, 0xdd9, 0x8), addr);
            try
            {
                ErebusAddress.Parse("{22-123A--DD9--F}");
                ErebusAddress.Parse("{22---123A}");
                Assert.Fail("Did not crash on double skip");
            }
            catch { }
        }

        [TestMethod]
        public void ToStringTest() => Assert.AreEqual("{22-123A--DD9-8}", new ErebusAddress(0x22, 0x123a, 0, 0, 0, 0, 0xdd9, 0x8).ToString());
    }
}