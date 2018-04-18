using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Enums;
using Mynt.Core.Models;
using Mynt.Core.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mynt.UnitTests.Strategies
{
    [TestClass]
    public class BuyAndHoldTests
    {
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PrepareBuyAndHoldWithNullInputThrowsException()
        {
            // Arrange
            var target = new BuyAndHold();

            // Act
            target.Prepare(null);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void PrepareBuyAndHoldWithEmptyInputThrowsException()
        {
            // Arrange
            var target = new BuyAndHold();

            // Act
            target.Prepare(new List<Candle>());
        }

        [TestMethod]
        public void PrepareBuyAndHoldWithListReturnsExpectedPattern()
        {
            // Arrange
            var target = new BuyAndHold();

            var list = Enumerable.Range(1, 100).
                Select(_ => new Candle { Close = 2.0m * (decimal)Math.Sin(_) * (decimal)Math.Sin(_) }).ToList();

            // Act
            var result = target.Prepare(list);

            // Assert
            Assert.AreEqual(100, result.Count());
            Assert.AreEqual(TradeAdvice.Buy, result.First());
            for (int index = 1; index < result.Count; index++)
            {
                Assert.AreEqual(TradeAdvice.Hold, result[index]);
            }
        }
    }
}
