using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Exchanges;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Tests.TradeManagers
{
    [TestClass]
    public class PaperTradeManagerTests
    {
        private PaperTradeManager _tradeManager;

        [TestInitialize]
        public void Init()
        {
            var logger = new Serilog.LoggerConfiguration().CreateLogger();
            _tradeManager = new PaperTradeManager(new BaseExchange(new ExchangeOptions()), new TheScalper(), null, logger, new TradeOptions(), new InMemoryDataStore());
        }

        #region initialization tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePaperTradeManagerWithoutExchangeShouldThrowException()
        {
            // Arrange
            var target = new PaperTradeManager(null, new TheScalper(), null, null, new TradeOptions(), new InMemoryDataStore());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePaperTradeManagerWithoutLoggerShouldThrowException()
        {
            // Arrange
            var target = new PaperTradeManager(new BaseExchange(new ExchangeOptions()), new TheScalper(), null, null, new TradeOptions(), new InMemoryDataStore());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePaperTradeManagerWithoutStrategyShouldThrowException()
        {
            // Arrange
            var target = new PaperTradeManager(new BaseExchange(new ExchangeOptions()), null, null, null, new TradeOptions(), new InMemoryDataStore());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePaperTradeManagerWithoutSettingsShouldThrowException()
        {
            // Arrange
            var target = new PaperTradeManager(new BaseExchange(new ExchangeOptions()), new TheScalper(), null, null, null, new InMemoryDataStore());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePaperTradeManagerWithoutDataStoreShouldThrowException()
        {
            // Arrange
            var target = new PaperTradeManager(new BaseExchange(new ExchangeOptions()), new TheScalper(), null, null, new TradeOptions(), null);
        }

        #endregion
        

    
    }
}
