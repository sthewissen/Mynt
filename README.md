<img src="https://raw.githubusercontent.com/sthewissen/Mynt/master/img/myntlogo.png" alt="Mynt" width="400" />

## Donating to support further development

Finding this useful? Consider a donation!

[![Donate with Bitcoin](https://en.cryptobadges.io/badge/small/13myncc3ie6iGjSmJHCdzahwaKizX7NBB1)](https://en.cryptobadges.io/donate/13myncc3ie6iGjSmJHCdzahwaKizX7NBB1)

[![Donate with Ethereum](https://en.cryptobadges.io/badge/small/0x4398c958468bEDB41DdEF4C297eB543c6d26f440)](https://en.cryptobadges.io/donate/0x4398c958468bEDB41DdEF4C297eB543c6d26f440)

## Get in touch!

Want to have a chat? [Come find us on Slack!]

<a href="https://join.slack.com/t/mynt-bot/signup"><img src="https://upload.wikimedia.org/wikipedia/commons/b/b9/Slack_Technologies_Logo.svg" alt="Join us on Slack!" width="100" /></a>

## About the project...
This is an Azure Functions-based cryptocurrency trading bot. It uses the following cloud components to function:

- Azure Functions
- Azure Table Storage

A lot of the logic is based on the [Freqtrade] bot and was converted to C#. The bot currently supports trading on the Bittrex exchange. This software was primarily created for educational purposes only. Don't risk money which you are afraid to lose. This bot was first mentioned in [one of my blogposts].

![Build status](https://sthewissen.visualstudio.com/_apis/public/build/definitions/c865956c-413b-4c44-b678-45d3026ae0b0/11/badge)

---

### Documentation

* [Installation (Azure)](https://github.com/sthewissen/Mynt/wiki/Installation-(Azure))
* [Installation (Local)](https://github.com/sthewissen/Mynt/wiki/Installation-(Local))
* [Configuration](https://github.com/sthewissen/Mynt/wiki/Configuration)
* [Indicators](https://github.com/sthewissen/Mynt/wiki/Indicators)
* [Strategies](https://github.com/sthewissen/Mynt/wiki/Strategies)
* [Notifications](https://github.com/sthewissen/Mynt/wiki/Notifications)
* [Backtesting](https://github.com/sthewissen/Mynt/wiki/Backtesting)

---

### Additional tools used

- Binance API wrapper - https://github.com/JKorf/Binance.Net
- Bittrex API wrapper - https://github.com/Coinio/Bittrex.Api.Client
- TA-Lib wrapper - https://www.nuget.org/packages/TA-Lib/

### Contributing

Feel like this bot is missing a feature? Pull requests are welcome! A few pointers for contributions:

- Create your PR against the develop branch, not master.
- If you are unsure, discuss the feature in an issue before a PR.

   [Freqtrade]: <https://github.com/gcarq/freqtrade>
   [Come find us on Slack!]: <https://join.slack.com/t/mynt-bot/shared_invite/enQtMzI3ODgzNTE1OTg3LTMyMGQyNTUxNTg2ODEwMjBjMDE0YzI5NDU3ZGI0MzVjMjBhYzBlNWE5MTMwMzIyZTViNmM2YTUxYzZhYjcyMTA>
   [one of my blogposts]: <https://www.thewissen.io/building-cryptocurrency-trading-bot-using-azure-part-1>
