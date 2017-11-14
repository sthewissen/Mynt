# Mynt
This is an Azure Functions-based cryptocurrency trading bot. It uses the following cloud components to function:

- Azure Functions
- Azure Table Storage

A lot of the logic is based on the [Freqtrade] bot and was converted to C#. The bot currently supports trading on the Bittrex exchange. This software was primarily created for educational purposes only. Don't risk money which you are afraid to lose. The bot runs at a pre-defined interval of 5 minutes, since that matches the candle data it retrieves from the exchange. This can be changed by altering the CRON expression on the `TradeTimer` function. Do keep in mind that unless you also change the logic for retrieving the candles you're still looking at 5 minute intervals.

This bot was first mentioned in [one of my blogposts].

### Configuration

There are a few settings you can configure to alter the behavior of the bot. These settings are stored in the `Constants.cs` file in the **Core** project. Azure Functions does not yet support some sort of configuration file that you can use locally and deploy to Azure so for now this will have to do.

| Setting | Description |
| ------- | ------ |
| `IsDryRunning` | This variable defines whether or not the bot is performing actual live trades. When dry-running the entire process is handled as it would be in a live scenario, but there is no actual communication with an exchange happening. |
| `BittrexApiKey` | The API key to use to communicate with Bittrex. |
| `BittrexApiSecret` | The secret key to use to communicate with Bittrex. |
| `MaxNumberOfConcurrentTrades` | The maximum number of concurrent trades the trader will perform. |
| `AmountOfBtcToInvestPerTrader` | The amount of BTC each trader has at its disposal to invest. |
| `TransactionFeePercentage` | The transaction fee percentage for the exchange we're using, for Bittrex this is 0,25%. |
| `StopLossPercentage` | The amount of profit at which we mitigate our losses and stop a trade (e.g. `-0.03` for a loss of 3%). |
| `MinimumAmountOfVolume` | Setting this to `0` means we will not look at volume and only look at our `AlwaysTradeList`. Setting this to any value higher than `0` means we will get a list of markets currently trading a volume above this value and analyze those for buy signals. |
| `AskLastBalance` | Sets the bidding price. A value of 0.0 will use the ask price, 1.0 will use the last price and values between those interpolate between ask and last price. Using the ask price will guarantee quick success in bid, but the bot will also end up paying more then would probably have been necessary. |
| `ReturnOnInvestment` | A list of duration and profit pairs. The duration is a value in minutes and the profit is a double containing a percentage. This list is used to define constraints such as "Sell when 5 minutes have passed and profit is at 3%". |
| `MarketBlackList` | A list of market names to never trade on (e.g. "BTC-XVG"). |
| `AlwaysTradeList` | A list of market names to always trade on (e.g. "BTC-OMG"). |
| `StopLossAnchors` | A list of percentages at which we want to lock in profit. Basically these function as a trailing stop loss. When profit reaches one of these percentages the stop loss is adjusted to this value. That way when profit drops below that we immediately sell. |
| `AzureStorageConnection` | The connection to the Azure Table Storage used to store our data. |
| `OrderTableName` | The table name for the Order table. |
| `BalanceTableName` | The table name for the Balance table. |

### Strategies

At the heart of this bot sit the strategies. These are all implementations of the `ITradingStrategy` interface and contain a `Prepare()` method that expects a `List<Candle>` objects. This method returns a list of integer values containing any of these three values:

- `-1` - This is a sell signal.
- `1` - This is a buy signal
- `0` - This is the signal to do absolutely nothing.

Within this preparation method you are free to use any type of indicator you want to determine what action to take at a specific moment in time. A sample strategy that uses a Simple Moving Average crossover looks like this:

    public class SmaCross : ITradingStrategy
    {
        public string Name => "SMA Cross";
        public List<Candle> Candles { get; set; }

        public SmaCross()
        {
            this.Candles = new List<Candle>();
        }

        public List<int> Prepare()
        {
            var result = new List<int>();

            var sma12 = Candles.Sma(12);
            var sma26 = Candles.Sma(26);

            for (int i = 0; i < Candles.Count; i++)
            {
                // Since we look back 1 candle, the first candle can never be a signal.
                if (i == 0)
                    result.Add(0);
                // When the fast SMA moves above the slow SMA, we have a positive cross-over
                else if (sma12[i] < sma26[i] && sma12[i - 1] > sma26[i])
                    result.Add(1);
                // When the slow SMA moves above the fast SMA, we have a negative cross-over
                else if (sma12[i] > sma26[i] && sma12[i - 1] < sma26[i])
                    result.Add(-1);
                else
                    result.Add(0);
            }

            return result;
        }
    }

### Indicators

All indicators use a .NET wrapper of the TA-Lib library. They are implemented as extension methods on `List<Candle>`. These candles can be used for every type of technical analysis because they also contain high, low, close, open and volume indicators for that moment in time. Not all indicators that are present in TA-Lib are implemented however. You can always submit a PR for a new indicator if you need it.

| Indicator | Default values | Extension method |
| ------ | ------ | ------ |
| Average Directional Movement Index | Period: 14 | Adx()
| Awesome Oscillator | Return RAW data: false | AwesomeOscillator()
| Bollinger Bands | Period: 5, Deviation up: 2, Deviation down: 2 | Bbands()
| Bear/Bull | N/A | BearBull()
| Commodity Channel Index | Period: 14 | Cci()
| Chande Momentum Oscillator | Period: 14 | Cmo()
| Derivative Oscillator | | DerivativeOscillator()
| Exponential Moving Average | Period: 30, Candle variable: Close | Ema()
| Fisher Ehlers | Period: 10 | Fisher()
| Moving Average Convergence/Divergence | Fast period: 12, Slow period: 26, Signal period: 9 | Macd()
| MESA Adaptive Moving Average | Fast period: 12, Slow period: 26, Signal period: 9 | Mama()
| Momentum Flow Index | Period: 14 | Mfi()
| Minus Directional Indicator | Period: 14 | MinusDI()
| Momentum | Period: 10 | Mom()
| Plus Directional Indicator | Period: 14 | PlusDI()
| Relative Strength Index | Period: 14 | Rsi()
| SAR | Acceleration factor: 0.02, Max. acceleration factor: 0.2 | Sar()
| Simple Moving Average | Period: 30, Candle variable: Close | Sma()
| Stochastics | Fast K period: 5, Slow K period: 3, Slow MA type: SMA, Slow D period: 3, Slow D MA type: SMA | Stoch()
| Stochastics Fast | Fast K period: 5, Fast D period: 3, Fast D MA type: SMA | StochFast()
| Stochastic RSI | Period: 14, Candle variable: Close, Fast K period: 3, Fast D period: 3, Fast D MA type: SMA | StochRsi()
| Triple Exponential Moving Average | Period: 20, Candle variable: Close | Tema()
| Weighted Moving Average | Period: 30, Candle variable: Close | Wma()

### Notifications
You can send/receive notifications using an implementation of the `INotificationManager` interface. Currently the default implementation uses Azure Notification Hubs to send these notifications to all devices registered within the configured Notification Hub. To implement your own custom notifications such as e.g. receiving an e-mail you can implement the `INotificationManager` interface and pass it into the `TradeManager` instance within the `TradeTimer` Azure Function.

### Backtesting
The project also contains a console application that can be used to backtest your strategies. It uses the 5 minute candle data for 10 popular crypto currencies. The data is distributed over a 20 day period and was gathered using the public Bittrex API. If you want more data or want to backtest using additional currencies you can use this API to retrieve the data.

The console application contains a few of the same variables (such as stoploss percentage, RoI) as the Azure Function that handles trading so you can also tweak these to change the results of your backtest.

### Additional tools used

- Bittrex API wrapper - https://github.com/Coinio/Bittrex.Api.Client
- TA-Lib wrapper - https://www.nuget.org/packages/TA-Lib/

### Contributing

Feel like this bot is missing a feature? Pull requests are welcome! A few pointers for contributions:

- Create your PR against the develop branch, not master.
- If you are unsure, discuss the feature in an issue before a PR.

   [Freqtrade]: <https://github.com/gcarq/freqtrade>
   [one of my blogposts]: <https://www.thewissen.io/building-cryptocurrency-trading-bot-using-azure-part-1>
