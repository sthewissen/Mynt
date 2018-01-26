using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Models;
using Mynt.Core.Strategies;

namespace Mynt.Core.Tests.Strategies
{
    [TestClass]
    public class SmaCrossoverTests
    {
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PrepareWithNullInputThrowsException()
        {
            // Arrange
            var target = new SmaCrossover();

            // Act
            target.Prepare(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void PrepareWithEmptyInputThrowsException()
        {
            // Arrange
            var target = new SmaCrossover();

            // Act
            target.Prepare(new List<Candle>());
        }

        [TestMethod]
        public void PrepareWithNoCrossoversReturnsListOfZeros()
        {
            // Arrange
            var target = new SmaCrossover();
            var list = Enumerable.Range(1, 100).Select(_ => new Candle { Close = _ }).ToList();

            // Act
            var result = target.Prepare(list);

            // Assert
            Assert.IsTrue(result.All(_ => _.Equals(0)));
        }

        [TestMethod]
        public void PrepareWithMultipleCrossoversReturnsExpectedPattern()
        {
            // Arrange
            var target = new SmaCrossover();

            var list = Enumerable.Range(1, 100).
                Select(_ => new Candle { Close = 2.0 * Math.Sin(_) * Math.Sin(_) }).ToList();

            // Act
            var result = target.Prepare(list);

            // Assert
            Assert.AreEqual(100, result.Count());
            Assert.AreEqual(67, result.Count(_ => _.Equals(0)));
            Assert.AreEqual(16, result.Count(_ => _.Equals(-1)));
            Assert.AreEqual(17, result.Count(_ => _.Equals(1)));
        }
    }
}
