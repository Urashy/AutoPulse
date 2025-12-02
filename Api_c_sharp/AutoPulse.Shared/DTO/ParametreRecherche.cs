// File: Api_c_sharp/BlazorAutoPulse/Model/ParametreRecherche.cs

namespace AutoPulse.Shared.DTO
{
    public class ParametreRecherche
    {
        public int Id { get; set; } = 0;
        public int IdCarburant { get; set; } = 0;
        public int IdMarque { get; set; } = 0;
        public int IdModele { get; set; } = 0;
        public int PrixMin { get; set; } = 0;
        public int PrixMax { get; set; } = 0;
        public int IdTypeVoiture { get; set; } = 0;
        public int IdTypeVendeur { get; set; } = 0;
        public string Nom { get; set; } = string.Empty;
        public int KmMin { get; set; } = 0;
        public int KmMax { get; set; } = 0;
        public string Departement { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 21;

        public string ToQueryString()
        {
            var parameters = new List<string>();

            if (Id > 0) parameters.Add($"id={Id}");
            if (IdCarburant > 0) parameters.Add($"idcarburant={IdCarburant}");
            if (IdMarque > 0) parameters.Add($"idmarque={IdMarque}");
            if (IdModele > 0) parameters.Add($"idmodele={IdModele}");
            if (PrixMin > 0) parameters.Add($"prixmin={PrixMin}");
            if (PrixMax > 0) parameters.Add($"prixmax={PrixMax}");
            if (IdTypeVoiture > 0) parameters.Add($"idtypevoiture={IdTypeVoiture}");
            if (IdTypeVendeur > 0) parameters.Add($"idtypevendeur={IdTypeVendeur}");
            if (!string.IsNullOrEmpty(Nom)) parameters.Add($"nom={Uri.EscapeDataString(Nom)}");
            if (KmMin > 0) parameters.Add($"kmmin={KmMin}");
            if (KmMax > 0) parameters.Add($"kmmax={KmMax}");
            if (!string.IsNullOrEmpty(Departement)) parameters.Add($"departement={Uri.EscapeDataString(Departement)}");

            // Ajout des paramètres de pagination si non par défaut
            if (PageNumber > 1) parameters.Add($"pageNumber={PageNumber}");
            if (PageSize != 21) parameters.Add($"pageSize={PageSize}");

            return string.Join("&", parameters);
        }
    }
}