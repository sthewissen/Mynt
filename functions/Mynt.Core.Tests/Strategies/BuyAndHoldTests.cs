using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Models;
using Mynt.Core.Strategies;

namespace Mynt.Core.Tests.Strategies
{
    [TestClass]
    public class BuyAndHoldTests
    {
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PrepareWithNullInputThrowsException()
        {
            // Arrange
            var target = new BuyAndHold();

            // Act
            target.Prepare(null);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void PrepareWithEmptyInputThrowsException()
        {
            // Arrange
            var target = new BuyAndHold();

            // Act
            target.Prepare(new List<Candle>());
        }
        
        [TestMethod]
        public void PrepareWithListReturnsExpectedPattern()
        {
            // Arrange
            var target = new BuyAndHold();

            var list = Enumerable.Range(1, 100).
                Select(_ => new Candle { Close = 2.0 * Math.Sin(_) * Math.Sin(_) }).ToList();

            // Act
            var result = target.Prepare(list);

            // Assert
            Assert.AreEqual(100, result.Count());
            Assert.AreEqual(1, result.First());
            for (int index =1; index< result.Count; index++)
            {
                Assert.AreEqual(0, result[index]);
            }
        }
    }
}
