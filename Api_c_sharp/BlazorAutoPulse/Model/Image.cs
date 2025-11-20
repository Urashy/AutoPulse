namespace BlazorAutoPulse.Model
{
    public class Image
    {
        public int IdImage { get; set; }
        public byte[]? Fichier { get; set; }
        public int? IdVoiture { get; set; }
        public int? IdCompte { get; set; }
    }
}
