namespace AutoPulse.Shared.DTO;

public class PieceJointeUploadDTO
{
    public int IdMessage { get; set; }
    public string NomFichier { get; set; } = string.Empty;
    public string TypeMime { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TailleFichier { get; set; }
    public string ContenuBase64 { get; set; } = string.Empty;
}