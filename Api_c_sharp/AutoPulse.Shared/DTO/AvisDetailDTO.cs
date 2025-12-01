namespace AutoPulse.Shared.DTO;

public class AvisDetailDTO
{
    public int IdAvis { get; set; }
    public string PseudoJugee { get; set; }
    public string PseudoJugeur { get; set; }
    public int IdCommande { get; set; }
    public DateTime DateAvis { get; set; }
    public string ContenuAvis { get; set; }
    public int NoteAvis { get; set; }
}