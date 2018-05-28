using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Enums;
using Mynt.Core.Models;
using Mynt.Core.Strategies;
<<<<<<< HEAD
using Mynt.Core.Strategies.Simple;
using Should.Fluent;

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
			var target = new SmaGoldenCross();
=======

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
>>>>>>> a8381648f34e683d38b45afc3ae6b1c0b7b4a985

			// Act
			target.Prepare(null);
		}

<<<<<<< HEAD
		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void PrepareGoldenCrossWithEmptyInputThrowsException()
		{
			// Arrange
			var target = new SmaGoldenCross();
=======
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void PrepareGoldenCrossWithEmptyInputThrowsException()
        {
            // Arrange
            var target = new GoldenCross();
>>>>>>> a8381648f34e683d38b45afc3ae6b1c0b7b4a985

			// Act
			target.Prepare(new List<Candle>());
		}

<<<<<<< HEAD
		[TestMethod]
		public void PrepareGoldenCrossWithNoCrossoversReturnsListOfHolds()
		{
			// Arrange
			var target = new SmaGoldenCross();
			var list = Enumerable.Range(1, 250).Select(_ => new Candle { Close = _ }).ToList();
=======
        [TestMethod]
        public void PrepareGoldenCrossWithNoCrossoversReturnsListOfHolds()
        {
            // Arrange
            var target = new GoldenCross();
            var list = Enumerable.Range(1, 250).Select(_ => new Candle { Close = _ }).ToList();
>>>>>>> a8381648f34e683d38b45afc3ae6b1c0b7b4a985

			// Act
			var result = target.Prepare(list);

			// Assert
			result.All(_ => _.Equals(TradeAdvice.Hold)).Should().Equal(true);
		}

<<<<<<< HEAD
		[TestMethod]
		public void PrepareGoldenCrossWithMultipleCrossoversReturnsExpectedPattern()
		{
			// Arrange
			var target = new SmaGoldenCross();
=======
        [TestMethod]
        public void PrepareGoldenCrossWithMultipleCrossoversReturnsExpectedPattern()
        {
            // Arrange
            var target = new GoldenCross();
>>>>>>> a8381648f34e683d38b45afc3ae6b1c0b7b4a985

			var list = Enumerable.Range(1, 250).
				Select(_ => new Candle { Close = 2.0m * (decimal)Math.Sin(_) * (decimal)Math.Sin(_) }).ToList();

			// Act
			var result = target.Prepare(list);

			// Assert
			result.Count().Should().Equal(250);
			result.Count(_ => _.Equals(TradeAdvice.Hold)).Should().Equal(218);
			result.Count(_ => _.Equals(TradeAdvice.Sell)).Should().Equal(16);
			result.Count(_ => _.Equals(TradeAdvice.Buy)).Should().Equal(16);
		}
	}
}
