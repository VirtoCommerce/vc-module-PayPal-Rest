using System.Collections.Generic;

namespace Paypal.Rest
{
    public class PayPalRestConfiguration
    {
        public string Mode { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public int Timeout { get; set; } = 30000;

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                { "mode", Mode },
                { "clientId", ClientId },
                { "clientSecret", ClientSecret },
                { "connectionTimeout", Timeout.ToString() }
            };
        }
    }
}