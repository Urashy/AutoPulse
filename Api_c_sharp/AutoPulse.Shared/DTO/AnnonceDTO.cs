namespace AutoPulse.Shared.DTO;

public class AnnonceDTO
{
    public int IdAnnonce { get; set; }
    public string? Libelle { get; set; }
    public string? PseudoVendeur { get; set; }
    public string? LibelleEtatAnnonce { get; set; }
    public DateTime DatePublication { get; set; }
    
    public string? Marque { get; set; }
    public string? Modele { get; set; }
    public int IdVoiture { get; set; }
    public int Annee { get; set; }
    public int Kilometrage { get; set; }
    public string? Carburant { get; set; }
    public decimal? Prix { get; set; }

    public string? Ville { get; set; }
    public string? CodePostal { get; set; }

    public int IdMiseEnAvant { get; set; }
    public int NbConversations { get; set; }
}