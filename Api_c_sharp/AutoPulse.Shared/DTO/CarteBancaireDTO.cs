using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class CarteBancaireDTO
    {
        public int IdCarteBancaire { get; set; }
        public string? NumeroCarte { get; set; }
        public string? CodeSecurite { get; set; }
        public string? DateExpiration { get; set; }
        public int IdCompte { get; set; }
    }
}
