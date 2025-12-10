namespace AutoPulse.Shared.DTO;

public class NotificationUpdateDTO
{
    public int IdNotification { get; set; }
    public int IdCompte { get; set; }
    public string Titre { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "info";
    public string? UrlNavigation { get; set; }
    public int? IdAnnonce { get; set; }
    public decimal? AncienPrix { get; set; }
    public decimal? NouveauPrix { get; set; }
}