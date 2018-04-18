using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Enums;
using Mynt.Core.Models;
using Mynt.Core.Strategies;

namespace Mynt.UnitTests
{
    [TestClass]
    public class GoldenCrossTests
    {
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PrepareGoldenCrossWithNullInputThrowsException()
        {
            // Arrange
            var target = new GoldenCross();

            // Act
            target.Prepare(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void PrepareGoldenCrossWithEmptyInputThrowsException()
        {
            // Arrange
            var target = new GoldenCross();

            // Act
            target.Prepare(new List<Candle>());
        }

        [TestMethod]
        public void PrepareGoldenCrossWithNoCrossoversReturnsListOfHolds()
        {
            // Arrange
            var target = new GoldenCross();
            var list = Enumerable.Range(1, 250).Select(_ => new Candle { Close = _ }).ToList();

            // Act
            var result = target.Prepare(list);

            // Assert
            Assert.IsTrue(result.All(_ => _.Equals(TradeAdvice.Hold)));
        }

        [TestMethod]
        public void PrepareGoldenCrossWithMultipleCrossoversReturnsExpectedPattern()
        {
            // Arrange
            var target = new GoldenCross();

            var list = Enumerable.Range(1, 250).
                Select(_ => new Candle { Close = 2.0m * (decimal)Math.Sin(_) * (decimal)Math.Sin(_) }).ToList();

            // Act
            var result = target.Prepare(list);

            // Assert
            Assert.AreEqual(250, result.Count());
            Assert.AreEqual(218, result.Count(_ => _.Equals(TradeAdvice.Hold)));
            Assert.AreEqual(16, result.Count(_ => _.Equals(TradeAdvice.Sell)));
            Assert.AreEqual(16, result.Count(_ => _.Equals(TradeAdvice.Buy)));
        }
    }
}
