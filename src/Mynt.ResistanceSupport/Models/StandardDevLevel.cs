using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.ResistanceSupport.Models
{
    public class StandardDevLevel
    {
		public decimal Price { get; internal set; }
		public List<Candle> Matches { get; internal set; }
        
		public int FirstTimeHit => Matches.Count > 0 ? (DateTime.UtcNow - Matches.First().Timestamp).Days : -1;
		public int LastTimeHit => Matches.Count > 0 ? (DateTime.UtcNow - Matches.Last().Timestamp).Days : -1;
		public int HitPeriod => FirstTimeHit - LastTimeHit;

		public StandardDevLevel(){
			Matches = new List<Candle>();
		}
    }
}
