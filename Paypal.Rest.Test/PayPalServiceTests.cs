using System.Collections.Generic;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Model;

namespace Paypal.Rest.Test
{
    [TestClass]
    public class PayPalServiceTests
    {
        private PayPalService _payPalService;
        private Mock<ILog> _logger;

        [TestInitialize]
        public void TestInitialize()
        {
            var configuration = new PayPalRestConfiguration
            {
                ClientId = "AaRbKe2riATpDsXJy0x9ut3_qfE23e5ftPeGQmdl3j7HUFMaLnFArVe7QqC8chhvcVilQb9-QnXNzNWr",
                ClientSecret = "EAdr_JFKnrP-qdYehzOxfflmxCAvqgMcRrWgf4yOgFv-pNdmE49e92ETopqGNnjgiSJlFTe5cu5GehMh",
                Mode = "sandbox"
            };

            _logger = new Mock<ILog>();
            _payPalService = new PayPalService(configuration, _logger.Object);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ProcessCreditCardWhenEverythingIsPerfect()
        {
            var context = new ProcessPaymentEvaluationContext
            {
                BankCardInfo = new BankCardInfo
                {
                    BankCardNumber = "4012888888881881",
                    BankCardCVV2 = "000",
                    BankCardMonth = 11,
                    BankCardYear = 2018,
                    BankCardType = "VISA"
                },
                Order = new CustomerOrder
                {
                    Addresses = new List<Address>
                    {
                        new Address
                        {
                            AddressType = AddressType.Shipping,
                            City = "Lehi",
                            CountryCode = "USA",
                            CountryName = "United States",
                            Email = "lehi@vcPaypal.com",
                            FirstName = "Sam",
                            LastName = "Smith",
                            Line1 = "123 Main St.",
                            Line2 = string.Empty,
                            MiddleName = string.Empty,
                            RegionId = "UT",
                            Phone = "8889637845",
                            PostalCode = "84043",
                            Name = "Sam Smith",
                            Organization = string.Empty,
                            RegionName = "Utah",
                            Zip = "84043"
                        }
                    }, Currency = "USD", Items = new List<LineItem>
                    {
                        new LineItem
                        {
                            Currency = "USD", Price = 12.50M, Quantity = 1, TaxPercentRate = 1.0M
                        }
                    }, Shipments = new List<Shipment>
                    {
                        new Shipment
                        {
                            Currency = "USD", Price = 10.50M, TaxPercentRate = 0.0M, 
                        }
                    }
                },
                Payment = new PaymentIn
                {
                    BillingAddress = new Address
                    {
                        AddressType = AddressType.Billing,
                        City = "Lehi",
                        CountryCode = "USA",
                        CountryName = "United States",
                        Email = "lehi@vcPaypal.com",
                        FirstName = "Sam",
                        LastName = "Smith",
                        Line1 = "123 Main St.",
                        Line2 = string.Empty,
                        MiddleName = string.Empty,
                        RegionId = "UT",
                        Phone = "8889637845",
                        PostalCode = "84043",
                        Name = "Sam Smith",
                        Organization = string.Empty,
                        RegionName = "Utah",
                        Zip = "84043"
                    }
                },
                Store = new Store
                {
                    Name = "My Store"
                }
            };

            var result = _payPalService.ProcessCreditCard(context);

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Error);
            Assert.IsNotNull(result.PaymentId);
            Assert.AreEqual(true, result.Succeeded);
        }
    }
}
