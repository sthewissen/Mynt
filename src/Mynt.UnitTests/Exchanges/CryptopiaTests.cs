using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Enums;
using Mynt.Core.Exchanges;
using Should;
using Should.Fluent;

namespace Mynt.UnitTests
{
	[TestClass]
	public class CryptopiaTests
	{
		[TestMethod]
		public async Task GetMarketSummaries()
		{
			// Arrange
			var exchange = new BaseExchange(new ExchangeOptions { Exchange = Exchange.Cryptopia, ApiKey = "NONE", ApiSecret = "NONE", PassPhrase = "NONE" });

			// Act
			var markets = await exchange.GetMarketSummaries("BTC");

			// Assert
			markets.Count.ShouldBeGreaterThan(0);
		}
	}
}
