using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Paypal.Rest.Test
{
    [TestClass]
    public class PayPalRestConfigurationTests
    {
        [TestMethod]
        public void ReturnSandboxAsDefault()
        {
            var configuration = new PayPalRestConfiguration();

            var dictionary = configuration.ToDictionary();

            Assert.AreEqual("sandbox", dictionary["mode"]);
        }

        [TestMethod]
        public void ReturnModeIfSet()
        {
            var configuration = new PayPalRestConfiguration {Mode = "SANDBOX"};

            var dictionary = configuration.ToDictionary();

            Assert.AreEqual("sandbox", dictionary["mode"]);
        }
    }
}
