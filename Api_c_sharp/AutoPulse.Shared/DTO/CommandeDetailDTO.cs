namespace Api_c_sharp.DTO;

public class CommandeDetailDTO
{
    public int IdCommande { get; set; }
    public DateTime Date { get; set; }
    public string MoyenPaiement { get; set; }
    
    // Vendeur
    public int IdVendeur { get; set; }
    public string PseudoVendeur { get; set; }
    
    // Acheteur
    public int IdAcheteur { get; set; }
    public string PseudoAcheteur { get; set; }
    
    // Annonce liée
    public AnnonceDTO Annonce { get; set; }
    
    // Facture
    public int? IdFacture { get; set; }
}