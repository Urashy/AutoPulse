namespace AutoPulse.Shared.DTO;

public class CommandeCreateDTO
{
    public int IdCommande { get; set; }
    public int IdVendeur { get; set; }
    public int IdAcheteur { get; set; }
    public int IdAnnonce { get; set; }
    public int IdMoyenPaiement { get; set; }
    public DateTime Date { get; set; }
}