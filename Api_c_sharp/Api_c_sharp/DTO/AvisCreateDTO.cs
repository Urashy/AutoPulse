namespace Api_c_sharp.DTO;

public class AvisCreateDTO
{
    public int IdJugee { get; set; }
    public int IdCommande { get; set; }
    public string ContenuAvis { get; set; }
    public int NoteAvis { get; set; }
}