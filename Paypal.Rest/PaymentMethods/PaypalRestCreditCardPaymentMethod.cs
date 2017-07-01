using System;
using System.Collections.Specialized;
using Common.Logging;
using VirtoCommerce.Domain.Payment.Model;

namespace Paypal.Rest.PaymentMethods
{
    public class PaypalRestCreditCardPaymentMethod : PaymentMethod
    {
        private readonly ILog _logger;

        public PaypalRestCreditCardPaymentMethod(ILog logger)
            : base("Paypal.Rest")
        {
            _logger = logger;
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.BankCard;

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentEvaluationContext context)
        {
            if (context.Store == null)
                throw new NullReferenceException("Store should not be null.");

            if (context.BankCardInfo == null)
                throw new NullReferenceException("BankCardInfo should not be null.");

            var retVal = new ProcessPaymentResult();
            
            var payPalService = new PayPalService(GetConfiguration(), _logger);
            var result = payPalService.ProcessCreditCard(context);

            PaymentStatus newStatus;
            if (result.Succeeded)
            {
                context.Payment.IsApproved = true;
                retVal.OuterId = result.PaymentId;
                retVal.IsSuccess = true;
                context.Payment.CapturedDate = DateTime.UtcNow;
                newStatus = PaymentStatus.Paid;
            }
            else
            {
                context.Payment.IsApproved = false;
                retVal.Error = result.Error;
                retVal.IsSuccess = false;
                context.Payment.VoidedDate = DateTime.UtcNow;
                newStatus = PaymentStatus.Voided;
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

        private PayPalRestConfiguration GetConfiguration()
        {
            return new PayPalRestConfiguration
            {
                ClientSecret = GetSetting("PayPal.Rest.ClientSecret"),
                ClientId = GetSetting("PayPal.Rest.ClientId"),
                Mode = GetSetting("PayPal.Rest.Mode")
            };
        }
    }
}