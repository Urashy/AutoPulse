using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class OffreDTO
    {
        public int IdOffre { get; set; }

        public int IdMessage { get; set; }

        public decimal Valeur { get; set; }

        public DateTime DateOffre { get; set; } = DateTime.UtcNow;
        public int IdAnnonce { get; set; }


        public bool? EstAccepte { get; set; }
    }
}
