using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class CarteBancaireCreateDTO
    {
        public string? NumeroCarte { get; set; }
        public DateTime DateExpiration { get; set; }
        public string? CodeSecurite { get; set; }
        public int IdCompte { get; set; }
    }
}
