namespace AutoPulse.Shared.DTO;

public class AvisCreateDTO
{
    public int IdAvis { get; set; } 
    public int IdJugee { get; set; }
    public int IdJugeur { get; set; }
    public int IdCommande { get; set; }
    public string ContenuAvis { get; set; }
    public int NoteAvis { get; set; }
}