namespace AutoPulse.Shared.DTO;

public class CommandeCreateDTO
{
    public int IdVendeur { get; set; }
    public int IdAcheteur { get; set; }
    public int IdAnnonce { get; set; }
    public int IdMoyenPaiement { get; set; }
    public int IdEtatCommande { get; set; }
    public DateTime Date { get; set; }
    public int IdOffre { get; set; }
    public int IdEtatCommande { get; set; }
}