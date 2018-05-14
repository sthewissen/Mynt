using System;
using System.Collections.Generic;
using System.Linq;
using Mynt.Core.Models;

namespace Mynt.Core.Extensions
{
    public static class CandleExtensions
    {
        public static List<decimal> High(this List<Candle> source)
        {
            return source.Select(x => x.High).ToList();
        }

        public static List<decimal> Low(this List<Candle> source)
        {
            return source.Select(x => x.Low).ToList();
        }

        public static List<decimal> Open(this List<Candle> source)
        {
            return source.Select(x => x.Open).ToList();
        }

        public static List<decimal> Close(this List<Candle> source)
        {
            return source.Select(x => x.Close).ToList();
        }

        public static List<decimal> Hl2(this List<Candle> source)
        {
            return source.Select(x => (x.High + x.Low) / 2).ToList();
        }

        public static List<decimal> Hlc3(this List<Candle> source)
        {
            return source.Select(x => (x.High + x.Low + x.Close) / 3).ToList();
        }
    }
}
