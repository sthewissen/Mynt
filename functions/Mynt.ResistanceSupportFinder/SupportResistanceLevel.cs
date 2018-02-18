using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.ResistanceSupportFinder
{
    public class SupportResistanceLevel
    {
        public decimal Price { get; set; }
        public LevelType Type { get; set; }

        public List<DateTime> Hits { get; set; }

        public double Conviction => (double) Price;

        public int HitScore => Hits.Count;

        public int FirstTime => (DateTime.UtcNow - Hits.First()).Days;

        public int LastTime => (DateTime.UtcNow - Hits.Last()).Days;

        public int TimeScore
        {
            get
            {
                var dates = Hits.OrderBy(x => x).ToList();
                int total = 0;

                for (int i = 0; i < dates.Count; i++)
                {
                    if (i > 0)
                    {
                        total += (dates[i] - dates[i - 1]).Days;
                    }
                }

                return total;
            }
        }
    }

    public enum LevelType
    {
        Support,
        Resistance
    }
}
