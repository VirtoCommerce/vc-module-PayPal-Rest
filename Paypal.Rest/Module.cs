using System;
using Microsoft.Practices.Unity;
using Paypal.Rest.Managers;
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

        #region IModule Members

        public override void PostInitialize()
        {
            var settings = _container.Resolve<ISettingsManager>().GetModuleSettings("Paypal.Rest");

            Func<PaypalRestPaymentMethod> paypalBankCardsExpressCheckoutPaymentMethodFactory = () => new PaypalRestPaymentMethod
            {
                Name = "Credit Card (Paypal/REST)",
                Description = "Paypal (REST)",
                LogoUrl = "https://raw.githubusercontent.com/VirtoCommerce/vc-module-Paypal-DirectPayments/master/Paypal.DirectPayments/Content/paypal_2014_logo.png",
                Settings = settings
            };

            _container.Resolve<IPaymentMethodsService>().RegisterPaymentMethod(paypalBankCardsExpressCheckoutPaymentMethodFactory);
        }

        #endregion
    }
}
