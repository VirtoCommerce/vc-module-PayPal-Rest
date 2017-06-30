using System;
using System.Collections.Generic;
using System.Linq;
using PayPal.Api;
using VirtoCommerce.Domain.Payment.Model;

namespace Paypal.Rest
{
    public class PayPalService
    {
        private readonly PayPalRestConfiguration _configuration;

        public PayPalService(PayPalRestConfiguration configuration)
        {
            _configuration = configuration;
            _configuration.Timeout = 60000;
        }

        public PayPalRestProcessResult ProcessCreditCard(ProcessPaymentEvaluationContext context)
        {
            var shippingAddress =
                context.Order.Addresses.FirstOrDefault(
                    a => a.AddressType == VirtoCommerce.Domain.Commerce.Model.AddressType.BillingAndShipping ||
                         a.AddressType == VirtoCommerce.Domain.Commerce.Model.AddressType.Shipping);



            if (shippingAddress == null)
            {
                throw new Exception("No shipping address available.");
            }

            var billingAddress = context.Order.Addresses.FirstOrDefault(
                                     a => a.AddressType == VirtoCommerce.Domain.Commerce.Model.AddressType
                                              .BillingAndShipping ||
                                          a.AddressType == VirtoCommerce.Domain.Commerce.Model.AddressType.Billing) ??
                                 shippingAddress;

            var transaction = new Transaction
            {
                amount = new Amount
                {
                    currency = context.Order.Currency,
                    total = context.Order.Total.ToString("N2"),
                    details = new Details
                    {
                        shipping = context.Order.ShippingSubTotal.ToString("N2"),
                        subtotal = context.Order.SubTotal.ToString("N2"),
                        tax = context.Order.TaxTotal.ToString("N2"),
                    }
                },
                description = $"Order from {context.Store.Name}.",
                item_list = new ItemList
                {
                    shipping_address = new ShippingAddress
                    {
                        city = shippingAddress.City,
                        country_code = GetPayPalCountryCode(shippingAddress.CountryCode),
                        line1 = shippingAddress.Line1,
                        line2 = shippingAddress.Line2,
                        phone = shippingAddress.Phone,
                        postal_code = shippingAddress.PostalCode,
                        state = shippingAddress.RegionId,
                        recipient_name = FormattedName(shippingAddress)
                    }
                },
                invoice_number = Guid.NewGuid().ToString()
            };

            var payer = new Payer
            {
                payment_method = "credit_card",
                funding_instruments = new List<FundingInstrument>
                {
                    new FundingInstrument
                    {
                        credit_card = new CreditCard
                        {
                            billing_address = new Address
                            {
                                city = billingAddress.City,
                                country_code = GetPayPalCountryCode(billingAddress.CountryCode),
                                line1 = billingAddress.Line1,
                                line2 = billingAddress.Line2,
                                phone = billingAddress.Phone,
                                postal_code = billingAddress.PostalCode,
                                state = billingAddress.RegionId,
                            },
                            cvv2 = context.BankCardInfo.BankCardCVV2,
                            expire_month = context.BankCardInfo.BankCardMonth,
                            expire_year = context.BankCardInfo.BankCardYear,
                            first_name = billingAddress.FirstName,
                            last_name = billingAddress.LastName,
                            number = context.BankCardInfo.BankCardNumber,
                            type = GetPayPalCreditCardType(context.BankCardInfo.BankCardType)
                        }
                    }
                },
                payer_info = new PayerInfo
                {
                    email = billingAddress.Email
                }
            };

            var payment = new Payment
            {
                intent = "Sale",
                payer = payer,
                transactions = new List<Transaction> {transaction}
            };

            try
            {
                var accessToken = new OAuthTokenCredential(_configuration.ToDictionary()).GetAccessToken();
                var apiContext = new APIContext(accessToken) {Config = _configuration.ToDictionary()};
                var createdPayment = payment.Create(apiContext);

                if (createdPayment.state == "failed")
                {
                    return new PayPalRestProcessResult
                    {
                        Succeeded = false,
                        Error = createdPayment.failure_reason
                    };
                }

                return new PayPalRestProcessResult
                {
                    Succeeded = true,
                    Error = string.Empty,
                    PaymentId = createdPayment.id
                };
            }
            catch (Exception e)
            {
                return new PayPalRestProcessResult
                {
                    Succeeded = false,
                    Error = e.Message,
                    PaymentId = string.Empty
                };
            }
        }

        public string GetPayPalCreditCardType(string virtoCommerceType)
        {
            switch (virtoCommerceType.ToUpperInvariant())
            {
                case "VISA":
                    return "visa";
                case "DISCOVER":
                    return "discover";
                case "MASTERCARD":
                    return "mastercard";
                case "AMEX":
                    return "amex";
            }

            throw new ArgumentException("$'{virtoCommerceType}' is not a valid credit card type.");
        }

        private string GetPayPalCountryCode(string virtoCommerceCountryCode)
        {
            if (virtoCommerceCountryCode.ToUpperInvariant() == "USA")
            {
                return "US";
            }

            return string.Empty;
        }

        private string FormattedName(VirtoCommerce.Domain.Commerce.Model.Address address)
        {
            var name = $"{address.FirstName} {address.MiddleName} {address.LastName}";
            return name.Replace("  ", " ");
        }
    }
}
