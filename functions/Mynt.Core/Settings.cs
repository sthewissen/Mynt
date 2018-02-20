using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core
{
    public static class Settings
    {
        public static string BinanceApiKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace( Constants.BinanceApiKey))
                {
                    return AppSettings.Get<string>("BinanceApiKey");
                }

                return Constants.BinanceApiKey;
            }
        }

        public static string BinanceApiSecret
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Constants.BinanceApiSecret))
                {
                    return AppSettings.Get<string>("BinanceApiSecret");
                }

                return Constants.BinanceApiSecret;
            }
        }
    }
}
