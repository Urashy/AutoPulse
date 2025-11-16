namespace BlazorAutoPulse.Model;

public class Annonce
{
    // Informations annonce
    public int IdAnnonce { get; set; }
    public string Libelle { get; set; }
    public string LibelleEtatAnnonce { get; set; }
    public DateTime DatePublication { get; set; }
    public bool EstMiseEnAvant { get; set; }
    public string LibelleMiseEnAvant { get; set; }
    
    // Informations vendeur
    public int IdVendeur { get; set; }
    public string PseudoVendeur { get; set; }
    public string NomVendeur { get; set; }
    public string PrenomVendeur { get; set; }
    public string BiographieVendeur { get; set; }
    public DateTime DateInscriptionVendeur { get; set; }
    public string TypeCompteVendeur { get; set; }
    
    // Adresse complète
    public string NumeroRue { get; set; }
    public string Rue { get; set; }
    public string Ville { get; set; }
    public string CodePostal { get; set; }
    public string Pays { get; set; }
    
    // Informations voiture détaillées
    public int IdVoiture { get; set; }
    public string Marque { get; set; }
    public string Modele { get; set; }
    public string Categorie { get; set; }
    public string Couleur { get; set; }
    public string Carburant { get; set; }
    public string BoiteDeVitesse { get; set; }
    public string Motricite { get; set; }
    public int Kilometrage { get; set; }
    public int Annee { get; set; }
    public int Puissance { get; set; }
    public int Couple { get; set; }
    public int NbCylindres { get; set; }
    public DateTime MiseEnCirculation { get; set; }
    public int NbPlaces { get; set; }
    public int NbPortes { get; set; }
    
    // Images
    public List<string> Images { get; set; } = new List<string>();
    
    // Modèle 3D
    public string LienModeleBlender { get; set; }
}