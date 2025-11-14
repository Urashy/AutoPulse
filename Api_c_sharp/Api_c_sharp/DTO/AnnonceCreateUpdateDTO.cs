namespace Api_c_sharp.DTO;

public class AnnonceCreateUpdateDTO
{
    public string Libelle { get; set; }
    public int IdEtatAnnonce { get; set; }
    public int IdAdresseAnnonce { get; set; }
    public int IdVoiture { get; set; }
    public int? IdMiseEnAvant { get; set; }
}