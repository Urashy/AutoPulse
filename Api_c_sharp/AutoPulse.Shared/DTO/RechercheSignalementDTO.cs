using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class RechercheSignalementDTO
    {
        public string Recherche { get; set; } = string.Empty;

        // "Annonce" ou "Compte" ou "all"
        public string TypeCible { get; set; } = "all";

        // 0 ou -1 = Tous, 1 = En attente, 2 = Traité, 3 = Rejeté
        public int IdEtatSignalement { get; set; } = 0;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string ToQueryString()
        {
            var paramsList = new List<string>();
            if (!string.IsNullOrWhiteSpace(Recherche)) paramsList.Add($"recherche={Uri.EscapeDataString(Recherche)}");
            if (!string.IsNullOrWhiteSpace(TypeCible)) paramsList.Add($"typeCible={TypeCible}");
            if (IdEtatSignalement > 0) paramsList.Add($"idEtat={IdEtatSignalement}");
            paramsList.Add($"pageNumber={PageNumber}");
            paramsList.Add($"pageSize={PageSize}");
            return string.Join("&", paramsList);
        }
    }
}
