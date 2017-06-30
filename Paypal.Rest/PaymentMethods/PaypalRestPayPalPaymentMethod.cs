using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using PayPal.Api;
using VirtoCommerce.Domain.Payment.Model;

namespace Paypal.Rest.PaymentMethods
{
    public class PaypalRestPayPalPaymentMethod : PaymentMethod
    {
        public PaypalRestPayPalPaymentMethod()
            : base("Paypal.Rest")
        {
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Paypal;

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentEvaluationContext context)
        {
            if (context.Store == null)
                throw new NullReferenceException("Store should not be null.");

            if (context.BankCardInfo == null)
                throw new NullReferenceException("BankCardInfo should not be null.");

            var retVal = new ProcessPaymentResult();

            var config = new Dictionary<string, string>
            {
                { "mode", GetSetting("PayPal.Rest.Mode") },
                { "clientId", GetSetting("PayPal.Rest.ClientId") },
                { "clientSecret", GetSetting("PayPal.Rest.ClientSecret") }
            };

            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            var apiContext = new APIContext(accessToken);

            var shippingAddress =
                context.Order.Addresses.FirstOrDefault(
                    a => a.AddressType == VirtoCommerce.Domain.Commerce.Model.AddressType.BillingAndShipping || a.AddressType == VirtoCommerce.Domain.Commerce.Model.AddressType.Shipping);
            if (shippingAddress == null)
            {
                throw new Exception("No shipping address available.");
            }

            var transaction = new Transaction
            {
                amount = new Amount
                {
                    currency = "USD",
                    total = context.Order.Total.ToString(CultureInfo.InvariantCulture),
                    details = new Details
                    {
                        shipping = context.Order.ShippingSubTotal.ToString(CultureInfo.InvariantCulture),
                        subtotal = context.Order.SubTotal.ToString(CultureInfo.InvariantCulture),
                        tax = context.Order.TaxTotal.ToString(CultureInfo.InvariantCulture),
                    }
                },
                description = $"Order from {context.Store.Name}.",
                item_list = new ItemList
                {
                    shipping_address = new ShippingAddress
                    {
                        city = shippingAddress.City,
                        country_code = shippingAddress.CountryCode,
                        line1 = shippingAddress.Line1,
                        line2 = shippingAddress.Line2,
                        phone = shippingAddress.Phone,
                        postal_code = shippingAddress.PostalCode,
                        state = shippingAddress.RegionId,
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
                                city = context.Payment.BillingAddress.City,
                                country_code = context.Payment.BillingAddress.CountryCode,
                                line1 = context.Payment.BillingAddress.Line1,
                                line2 = context.Payment.BillingAddress.Line2,
                                phone = context.Payment.BillingAddress.Phone,
                                postal_code = context.Payment.BillingAddress.PostalCode,
                                state = context.Payment.BillingAddress.RegionId,
                            },
                            cvv2 = context.BankCardInfo.BankCardCVV2,
                            expire_month = context.BankCardInfo.BankCardMonth,
                            expire_year = context.BankCardInfo.BankCardYear,
                            first_name = context.Payment.BillingAddress.FirstName,
                            last_name = context.Payment.BillingAddress.LastName,
                            number = context.BankCardInfo.BankCardNumber,
                            type = context.BankCardInfo.BankCardType
                        }
                    }
                },
                payer_info = new PayerInfo
                {
                    email = context.Order.CustomerId // Look up customer using this.
                }
            };
            var payment = new Payment
            {
                intent = "Sale",
                payer = payer,
                transactions = new List<Transaction> { transaction }
            };
            var createdPayment = payment.Create(apiContext);

            PaymentStatus newStatus;
            if (createdPayment.state == "failed")
            {
                context.Payment.IsApproved = false;
                var error = createdPayment.failure_reason;

                retVal.Error = error;
                context.Payment.VoidedDate = DateTime.UtcNow;
                newStatus = PaymentStatus.Voided;
            }
            else
            {
                context.Payment.IsApproved = true;

                retVal.OuterId = createdPayment.id;
                retVal.IsSuccess = true;
                context.Payment.CapturedDate = DateTime.UtcNow;
                newStatus = PaymentStatus.Paid;
            }

            retVal.NewPaymentStatus = context.Payment.PaymentStatus = newStatus;
            return retVal;
        }

        public override PostProcessPaymentResult PostProcessPayment(PostProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override VoidProcessPaymentResult VoidProcessPayment(VoidProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override CaptureProcessPaymentResult CaptureProcessPayment(CaptureProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override RefundProcessPaymentResult RefundProcessPayment(RefundProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(NameValueCollection queryString)
        {
            return new ValidatePostProcessRequestResult();
        }
    }
}