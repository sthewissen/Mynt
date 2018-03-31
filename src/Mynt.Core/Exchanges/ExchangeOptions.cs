using System;
using Mynt.Core.Configuration;
using Mynt.Core.Enums;

namespace Mynt.Core.Exchanges
{
    public class ExchangeOptions : BaseSettings
    {
        public Exchange Exchange { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string PassPhrase { get; set; }

        public ExchangeOptions(Exchange exchange)
        {
            Exchange = exchange;
            TrySetFromConfig(() => ApiKey = AppSettings.Get<string>(exchange.ToString() + nameof(ApiKey)));
            TrySetFromConfig(() => ApiSecret = AppSettings.Get<string>(exchange.ToString() + nameof(ApiSecret))); 
            TrySetFromConfig(() => PassPhrase = AppSettings.Get<string>(exchange.ToString() + nameof(PassPhrase))); 
        }
    }
}
