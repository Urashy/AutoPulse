namespace AutoPulse.Shared.DTO;

public class CommandeUpdateDTO
{
    public int IdCommande { get; set; }
    public int IdVendeur { get; set; }
    public int IdAcheteur { get; set; }
    public int IdAnnonce { get; set; }
    public int IdMoyenPaiement { get; set; }
    public int IdEtatCommande { get; set; }
    public int IdOffre { get; set; }
    public DateTime Date { get; set; }
}