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

        public double Conviction => (AmountOfHitsScore + FirstTimeHitScore + LastTimeHitScore);

        public int AmountOfHits => Hits.Count;

        public int FirstTimeHit => (DateTime.UtcNow - Hits.First()).Days;

        public int LastTimeHit => (DateTime.UtcNow - Hits.Last()).Days;

        public int AmountOfHitsScore { get; set; }
        public int FractalHitsScore { get; set; }
        public int FirstTimeHitScore { get; set; }
        public int LastTimeHitScore { get; set; }
    }

    public enum LevelType
    {
        Support,
        Resistance
    }
}
