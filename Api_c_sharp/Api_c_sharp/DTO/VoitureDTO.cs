namespace Api_c_sharp.DTO
{
    public class VoitureDTO
    {
        public int IdVoiture { get; set; }
        public string LibelleMarque { get; set; }
        public string LibelleMotricite { get; set; }
        public string LibelleCarburant { get; set; }
        public string LibelleBoite { get; set; }
        public string LibelleCouleur { get; set; }
        public string LibelleCategorie { get; set; }
        public int Kilometrage { get; set; }
        public int Annee { get; set; }
        public int Puissance { get; set; }
        public DateTime MiseEnCirculation { get; set; }
    }
}
