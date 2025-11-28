namespace BlazorAutoPulse.Model;

public class Annonce
{
    public int IdAnnonce { get; set; }
    public string Libelle { get; set; }
    public int IdCompte { get; set; }
    public int IdEtatAnnonce { get; set; }
    public int IdAdresse { get; set; }
    public int IdVoiture { get; set; }
    public int? IdMiseEnAvant { get; set; }
    public DateTime? DatePublication { get; set; }
    public int Prix { get; set; }
    public int? Annee { get; set; }
    public int? Kilometrage { get; set; }
}