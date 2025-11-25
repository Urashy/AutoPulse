namespace Api_c_sharp.DTO;

public class AnnonceDTO
{
    public int IdAnnonce { get; set; }
    public string Libelle { get; set; }
    public string PseudoVendeur { get; set; }
    public string LibelleEtatAnnonce { get; set; }
    public DateTime DatePublication { get; set; }
    
    // Informations voiture essentielles
    public string Marque { get; set; }
    public string Modele { get; set; }
    public int IdVoiture { get; set; }
    public int Annee { get; set; }
    public int Kilometrage { get; set; }
    public string Carburant { get; set; }
    public decimal? Prix { get; set; } // Si vous avez un prix
    
    // Localisation
    public string Ville { get; set; }
    public string CodePostal { get; set; }
    
    // Image principale
    public string ImagePrincipale { get; set; } // Base64 ou URL
    
    // Mise en avant
    public int IdMiseEnAvant { get; set; }
}