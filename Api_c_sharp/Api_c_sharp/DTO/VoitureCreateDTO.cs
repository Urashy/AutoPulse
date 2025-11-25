namespace Api_c_sharp.DTO;

public class VoitureCreateDTO
{
    public int IdVoiture { get; set; }
    public int IdMarque { get; set; }
    public int IdModele { get; set; }
    public int IdMotricite { get; set; }
    public int IdCarburant { get; set; }
    public int IdBoiteDeVitesse { get; set; }
    public int IdCategorie { get; set; }
    public int NbPlace { get; set; }
    public int NbPorte { get; set; }
    public int Kilometrage { get; set; }
    public int Annee { get; set; }
    public int Puissance { get; set; }
    public int Couple { get; set; }
    public int NbCylindres { get; set; }
    public DateTime MiseEnCirculation { get; set; }
    public int? IdModeleBlender { get; set; }
    public int NbPlace { get; set; }
    public int NbPorte { get; set; }
}