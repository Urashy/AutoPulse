namespace AutoPulse.Shared.DTO;

public class NotificationDTO
{
    public int IdNotification { get; set; }
    public int IdCompte { get; set; }
    public string Titre { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "info";
    public string? UrlNavigation { get; set; }
    public bool EstLue { get; set; }
    public DateTime DateCreation { get; set; }
        
    // Données spécifiques aux baisses de prix
    public int? IdAnnonce { get; set; }
    public string? LibelleAnnonce { get; set; }
    public double? AncienPrix { get; set; }
    public double? NouveauPrix { get; set; }
    public double? Reduction { get; set; }
    
    // Données spécifiques aux offre
    public string? PseudoAcheteurOffre { get; set; }
    public double? ValeurOffre { get; set; }
}