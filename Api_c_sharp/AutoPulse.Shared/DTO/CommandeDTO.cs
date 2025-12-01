namespace Api_c_sharp.DTO;

public class CommandeDTO
{
    public int IdCommande { get; set; }
    public DateTime Date { get; set; }
    public string PseudoVendeur { get; set; }
    public string PseudoAcheteur { get; set; }
    public string LibelleAnnonce { get; set; }
    public string MoyenPaiement { get; set; }
}