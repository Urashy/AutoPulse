namespace AutoPulse.Shared.DTO;

public class PieceJointeUploadDTO
{
    public int IdMessage { get; set; }
    public string NomFichier { get; set; } = null!;
    public string TypeMime { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public long TailleFichier { get; set; }
    public string ContenuBase64 { get; set; } = null!;
}