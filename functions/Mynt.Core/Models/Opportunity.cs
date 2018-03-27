using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Models
{
    public class Opportunity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
