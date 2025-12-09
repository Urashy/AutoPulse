namespace AutoPulse.Shared.DTO;

public class PieceJointeDTO
{
    public int IdPieceJointe { get; set; }
    public int IdMessage { get; set; }
    public string NomFichier { get; set; } = null!;
    public string TypeMime { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public long TailleFichier { get; set; }
    
    /// <summary>
    /// Contenu encodé en Base64 pour le transport
    /// </summary>
    public string? ContenuBase64 { get; set; }
    
    public DateTime DateUpload { get; set; }
    
    // Propriétés helpers
    public string TailleFormatee => FormatFileSize(TailleFichier);
    public bool EstImage => TypeMime.StartsWith("image/");
    public bool EstPdf => TypeMime == "application/pdf";
    public bool EstDocument => TypeMime.Contains("word") || TypeMime.Contains("document");
    
    /// <summary>
    /// Data URL complète pour affichage direct dans le HTML
    /// </summary>
    public string DataUrl => !string.IsNullOrEmpty(ContenuBase64) 
        ? $"data:{TypeMime};base64,{ContenuBase64}" 
        : string.Empty;

    /// <summary>
    /// Icône en fonction du type de fichier
    /// </summary>
    public string IconeType => Extension.ToLowerInvariant() switch
    {
        ".pdf" => "📄",
        ".doc" or ".docx" => "📝",
        ".txt" => "📃",
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "🖼️",
        _ => "📎"
    };

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}