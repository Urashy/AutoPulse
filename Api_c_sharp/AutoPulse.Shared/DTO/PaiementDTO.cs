using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class PaiementDTO
    {
        public int NbSemaineMiseEnAvantOr {  get; set; }
        public decimal PrixMiseEnAvantOr { get; set; } = 0;
        public int NbSemaineMiseEnAvantPlatine { get; set; }
        public decimal PrixMisedAvantPlatine { get; set; } = 0;
        public int NbSemaineMiseEnAvantDiamant { get; set; }
        public decimal PrixMiseEnAvantDiamant { get; set; } = 0;

    }
}
