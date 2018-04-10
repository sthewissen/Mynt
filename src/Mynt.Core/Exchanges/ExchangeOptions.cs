using Mynt.Core.Enums;

namespace Mynt.Core.Exchanges
{
    public class ExchangeOptions
    {
        public Exchange Exchange { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string PassPhrase { get; set; }
    }
}