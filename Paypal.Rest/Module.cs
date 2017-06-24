﻿using System;
using Microsoft.Practices.Unity;
using Paypal.Rest.PaymentMethods;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace Paypal.Rest
{
    public class Module : ModuleBase
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void PostInitialize()
        {
            var settings = _container.Resolve<ISettingsManager>().GetModuleSettings("Paypal.Rest");

            Func<PaypalRestCreditCardPaymentMethod> paypalRestCreditCardPaymentMethod = () => new PaypalRestCreditCardPaymentMethod
            {
                Name = "Credit Card (Paypal/REST)",
                Description = "Process credit cards using PayPal's REST interface.",
                LogoUrl = "https://github.com/montanehamilton/vc-module-PayPal-Rest/raw/master/Paypal.Rest/Content/paypal_2014_logo.png",
                Settings = settings
            };

            Func<PaypalRestPayPalPaymentMethod> paypalRestPayPalPaymentMethod = () => new PaypalRestPayPalPaymentMethod()
            {
                Name = "PayPal (Paypal/REST)",
                Description = "Process PayPal using PayPal's REST interface.",
                LogoUrl = "https://github.com/montanehamilton/vc-module-PayPal-Rest/raw/master/Paypal.Rest/Content/paypal_2014_logo.png",
                Settings = settings
            };

            _container.Resolve<IPaymentMethodsService>().RegisterPaymentMethod(paypalRestCreditCardPaymentMethod);
            _container.Resolve<IPaymentMethodsService>().RegisterPaymentMethod(paypalRestPayPalPaymentMethod);
        }
    }
}
