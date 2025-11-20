namespace Api_c_sharp.DTO
{
    public class ImageDTO
    {
        public int IdImage { get; set; }
        public byte[]? Fichier { get; set; }
        public int? IdVoiture { get; set; }
        public int? IdCompte { get; set; }
    }
}
