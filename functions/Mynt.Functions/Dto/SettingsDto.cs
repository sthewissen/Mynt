using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Functions.Dto
{
    public class SettingsDto
    {
        public int AmountOfWorkers { get; set; }
        public double StakePerWorker { get; set; }
        public double StopLossPercentage { get; set; }
        public int MinimumAmountOfVolume { get; set; }
    }
}
