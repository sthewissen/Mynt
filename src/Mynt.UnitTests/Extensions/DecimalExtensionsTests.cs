using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;

namespace Mynt.UnitTests
{
    [TestClass]
    public class DecimalExtensionsTests
    {
        [TestMethod]
        public void CheckHighestItems()
        {
            // Arrange
            var target = new List<decimal?> { null, 9, 8, 4, 21, 38, 19, 3, 30, 3, 1, 2, 3, 4, 6, 7, 8, 9, 4, 3, 2, 23, 6 };

            var maxs = target.Highest(13);

            // Assert
            Assert.AreEqual(target.Count, maxs.Count());
            Assert.AreEqual(maxs[0], null);
            Assert.AreEqual(maxs[4], 21);
            Assert.AreEqual(maxs[8], 38);
            Assert.AreEqual(maxs[12], 38);
            Assert.AreEqual(maxs[20], 30);
        }

        [TestMethod]
        public void CheckLowestItems()
        {
            // Arrange
            var target = new List<decimal?> { null, 9, 8, 4, 21, 38, 19, 3, 30, 3, 1, 2, 3, 4, 6, 7, 8, 9, 4, 3, 2, 23, 6 };

            var mins = target.Lowest(13);

            // Assert
            Assert.AreEqual(target.Count, mins.Count());
            Assert.AreEqual(mins[0], null);
            Assert.AreEqual(mins[4], 4);
            Assert.AreEqual(mins[8], 3);
            Assert.AreEqual(mins[12], 1);
            Assert.AreEqual(mins[20], 1);
        }

        [TestMethod]
        public void CheckAvgItems()
        {
            // Arrange
            var target = new List<decimal?> { null, 9, 8, 4, 21, 38, 19, 3, 30, 3, 1, 2, 3, 4, 6, 7, 8, 9, 4, 3, 2, 23, 6 };

            var avgs = target.Avg(13);

            // Assert
            Assert.AreEqual(target.Count, avgs.Count());
            Assert.AreEqual(avgs[0], null);
            Assert.AreEqual(avgs[4], 10.5m);
            Assert.AreEqual(avgs[8], 16.5m);
            Assert.AreEqual(avgs[12], 11.75m);
            Assert.AreEqual(avgs[22], 6m);
        }
    }
}
