using System;
using Mynt.Helpers;

namespace Mynt
{
    public class Constants
    {
        public static string ApiBaseUrl => $"https://{Settings.FunctionRoot}.azurewebsites.net";
        public const string BlockChainApiRoot = "https://blockchain.info";
        public const string BittrexApiRoot = "https://www.bittrex.com";
    }
}
