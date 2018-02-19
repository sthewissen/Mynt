![Mynt](https://raw.githubusercontent.com/sthewissen/Mynt/master/myntlogo.png)

Finding this useful? Consider a donation!

| Coin | Address |
| ------- | ------ |
| BTC | 13myncc3ie6iGjSmJHCdzahwaKizX7NBB1 |
| ETH | 0x4398c958468bEDB41DdEF4C297eB543c6d26f440 |

# Mynt
This is an Azure Functions-based cryptocurrency trading bot. It uses the following cloud components to function:

- Azure Functions
- Azure Table Storage

A lot of the logic is based on the [Freqtrade] bot and was converted to C#. The bot currently supports trading on the Bittrex exchange. This software was primarily created for educational purposes only. Don't risk money which you are afraid to lose. The bot runs at a pre-defined interval of 5 minutes, since that matches the candle data it retrieves from the exchange. This can be changed by altering the CRON expression on the `TradeTimer` function. Do keep in mind that unless you also change the logic for retrieving the candles you're still looking at 5 minute intervals.

This bot was first mentioned in [one of my blogposts].

![Build status](https://sthewissen.visualstudio.com/_apis/public/build/definitions/c865956c-413b-4c44-b678-45d3026ae0b0/11/badge)

---

### Documentation

* [Configuration](https://github.com/sthewissen/Mynt/wiki/Configuration)
* [Indicators](https://github.com/sthewissen/Mynt/wiki/Indicators)
* [Strategies](https://github.com/sthewissen/Mynt/wiki/Strategies)
* [Notifications](https://github.com/sthewissen/Mynt/wiki/Notifications)
* [Backtesting](https://github.com/sthewissen/Mynt/wiki/Backtesting)

---

### Additional tools used

- Bittrex API wrapper - https://github.com/Coinio/Bittrex.Api.Client
- TA-Lib wrapper - https://www.nuget.org/packages/TA-Lib/

### Contributing

Feel like this bot is missing a feature? Pull requests are welcome! A few pointers for contributions:

- Create your PR against the develop branch, not master.
- If you are unsure, discuss the feature in an issue before a PR.

   [Freqtrade]: <https://github.com/gcarq/freqtrade>
   [one of my blogposts]: <https://www.thewissen.io/building-cryptocurrency-trading-bot-using-azure-part-1>
