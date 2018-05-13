using System;
namespace Mynt.ResistanceSupport.Models
{
    public class PivotPoint
    {
		public DateTime Timestamp { get; set; }
		public decimal Price { get; set; }
		public PivotType Type { get; set; }
    }
}
