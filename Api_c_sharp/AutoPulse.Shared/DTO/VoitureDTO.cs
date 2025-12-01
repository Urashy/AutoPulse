namespace AutoPulse.Shared.DTO
{
    public class VoitureDTO
    {
        public int IdVoiture { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public int Annee { get; set; }
        public int Kilometrage { get; set; }
        public string Carburant { get; set; }
        public IEnumerable<string> LibelleCouleur { get; set; }

    }
}
