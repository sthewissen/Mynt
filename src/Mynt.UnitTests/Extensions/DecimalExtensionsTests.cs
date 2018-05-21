using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mynt.Core.Enums;
using Mynt.Core.Extensions;
using Should.Fluent;

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
			target.Count.Should().Equal(maxs.Count());
			maxs[0].Should().Equal(null);
			maxs[4].Should().Equal(21);
			maxs[8].Should().Equal(38);
			maxs[12].Should().Equal(38);
			maxs[20].Should().Equal(30);
		}

		[TestMethod]
		public void CheckLowestItems()
		{
			// Arrange
			var target = new List<decimal?> { null, 9, 8, 4, 21, 38, 19, 3, 30, 3, 1, 2, 3, 4, 6, 7, 8, 9, 4, 3, 2, 23, 6 };

			var mins = target.Lowest(13);

			// Assert
			target.Count.Should().Equal(mins.Count());
			mins[0].Should().Equal(null);
			mins[4].Should().Equal(4);
			mins[8].Should().Equal(3);
			mins[12].Should().Equal(1);
			mins[20].Should().Equal(1);
		}

		[TestMethod]
		public void CheckAvgItems()
		{
			// Arrange
			var target = new List<decimal?> { null, 9, 8, 4, 21, 38, 19, 3, 30, 3, 1, 2, 3, 4, 6, 7, 8, 9, 4, 3, 2, 23, 6 };

			var avgs = target.Avg(13);

			// Assert
			target.Count.Should().Equal(avgs.Count());
			avgs[0].Should().Equal(null);
			avgs[4].Should().Equal(10.5m);
			avgs[8].Should().Equal(16.5m);
			avgs[12].Should().Equal(11.75m);
			avgs[22].Should().Equal(6m);
		}
	}
}
