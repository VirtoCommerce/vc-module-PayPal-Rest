namespace Paypal.Rest
{
    public class PayPalRestProcessResult
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public string PaymentId { get; set; }
    }
}