namespace Api_c_sharp.DTO;

public class FavoriDTO
{
    public int IdCompte { get; set; }
    public int IdAnnonce { get; set; }
    public AnnonceDTO Annonce { get; set; }
}