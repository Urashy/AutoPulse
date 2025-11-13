namespace Api_c_sharp.DTO;

public class AnnonceDTO
{
    public int IdAnnonce { get; set; }
    public string Libelle { get; set; }
    public string Pseudo { get; set; }
    public string LibelleEtatAnnonce { get; set; }
    public string NumeroRue { get; set; }
    public string Rue { get; set; }
    public string Marque { get; set; }
    public string Modele { get; set; }
    public DateTime DatePublication { get; set; }
}