namespace BlazorAutoPulse.Model;

public class Annonce
{
    public int IdAnnonce { get; set; }
    public string Libelle { get; set; } = null!;
    public int IdCompte{ get; set; }
    public int? IdCommande { get; set; }
    public int IdEtatAnnonce { get; set; }
    public int IdAdresseAnnonce { get; set; }
    public int IdVoiture { get; set; }
    public int? IdMiseEnAvant { get; set; }
    public DateTime? DatePublication{ get; set; }
}