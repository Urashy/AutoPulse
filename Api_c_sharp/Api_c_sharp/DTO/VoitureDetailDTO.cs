namespace Api_c_sharp.DTO;

public class VoitureDetailDTO
{
    public int IdVoiture { get; set; }
    public string LibelleMarque { get; set; }
    public string LibelleModele { get; set; }
    public string LibelleMotricite { get; set; }
    public string LibelleCarburant { get; set; }
    public string LibelleBoite { get; set; }
    public IEnumerable<string> LibelleCouleur { get; set; }
    public string LibelleCategorie { get; set; }
    public int Kilometrage { get; set; }
    public int Annee { get; set; }
    public int Puissance { get; set; }
    public int Couple { get; set; }
    public int NbCylindres { get; set; }
    public DateTime MiseEnCirculation { get; set; }
    public int NbPlaces { get; set; }
    public int NbPortes { get; set; }
    public string LienModeleBlender { get; set; }
    public List<string> Images { get; set; } = new List<string>();
}