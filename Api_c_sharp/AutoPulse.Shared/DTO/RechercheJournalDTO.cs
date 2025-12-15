using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class RechercheJournalDTO
    {
        public int IdType { get; set; }
        public int Order { get; set; }
        public DateTime? DebutIntervalle { get; set; }
        
        public DateTime? FinIntervalle { get; set; }
    }
}
