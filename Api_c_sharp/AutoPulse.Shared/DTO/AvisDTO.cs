namespace Api_c_sharp.DTO;

public class AvisListDTO
{
    public int IdAvis { get; set; }
    public string PseudoJugeur { get; set; }
    public DateTime DateAvis { get; set; }
    public string ContenuAvis { get; set; }
    public int NoteAvis { get; set; }
}