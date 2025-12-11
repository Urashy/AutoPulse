using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class AdminSignalement
    {
        public int Id { get; set; }
        public string TypeSignalement { get; set; } = "";
        public string TypeCible { get; set; } = "";
        public int IdCible { get; set; }
        public string PseudoSignalant { get; set; } = "";
        public string? PseudoCible { get; set; }
        public string? TitreCible { get; set; }
        public string? Description { get; set; }
        public DateTime DateSignalement { get; set; }
        public string Statut { get; set; } = "";
        public int IdStatut { get; set; }
    }
}
