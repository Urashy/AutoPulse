using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class SignalementAnnonceCreateDTO
    {
        public int IdSignalement { get; set; }
        public string DescriptionSignalement { get; set; }
        public int IdCompteSignalant { get; set; }
        public int IdAnnonceSignale { get; set; }

        public int IdTypeSignalement { get; set; }
    }
}
