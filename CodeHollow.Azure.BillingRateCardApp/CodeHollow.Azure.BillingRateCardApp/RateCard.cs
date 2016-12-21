using System.Collections.Generic;

namespace CodeHollow.Azure.BillingRateCardApp
{
    public class RateCard
    {
        public List<object> OfferTerms { get; set; }
        public List<Meter> Meters { get; set; }
        public string Currency { get; set; }
        public string Locale { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
