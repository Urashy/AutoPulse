namespace AutoPulse.Shared.DTO;

public class PieceJointeDTO
{
    public int IdPieceJointe { get; set; }
    public int IdMessage { get; set; }
    public string NomFichier { get; set; } = string.Empty;
    public string TypeMime { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TailleFichier { get; set; }
    public DateTime DateAjout { get; set; }
    public string? ContenuBase64 { get; set; }

    // ✅ CACHE : Calculer une seule fois au lieu de chaque render
    private string? _dataUrlCache;
    public string DataUrl
    {
        get
        {
            if (_dataUrlCache != null)
                return _dataUrlCache;

            if (string.IsNullOrEmpty(ContenuBase64))
                return string.Empty;

            _dataUrlCache = $"data:{TypeMime};base64,{ContenuBase64}";
            return _dataUrlCache;
        }
    }

    // ✅ CACHE : Propriétés calculées une seule fois
    private bool? _estImageCache;
    public bool EstImage
    {
        get
        {
            if (_estImageCache.HasValue)
                return _estImageCache.Value;

            _estImageCache = TypeMime?.StartsWith("image/") ?? false;
            return _estImageCache.Value;
        }
    }

    private string? _tailleFormateeCache;
    public string TailleFormatee
    {
        get
        {
            if (_tailleFormateeCache != null)
                return _tailleFormateeCache;

            _tailleFormateeCache = TailleFichier switch
            {
                < 1024 => $"{TailleFichier} o",
                < 1024 * 1024 => $"{TailleFichier / 1024.0:F1} Ko",
                < 1024 * 1024 * 1024 => $"{TailleFichier / (1024.0 * 1024):F1} Mo",
                _ => $"{TailleFichier / (1024.0 * 1024 * 1024):F1} Go"
            };
            return _tailleFormateeCache;
        }
    }

    private string? _iconeTypeCache;
    public string IconeType
    {
        get
        {
            if (_iconeTypeCache != null)
                return _iconeTypeCache;

            _iconeTypeCache = Extension?.ToLowerInvariant() switch
            {
                ".pdf" => "📄",
                ".doc" or ".docx" => "📝",
                ".xls" or ".xlsx" => "📊",
                ".zip" or ".rar" or ".7z" => "📦",
                ".jpg" or ".jpeg" or ".png" or ".gif" => "🖼️",
                ".mp4" or ".avi" or ".mov" => "🎬",
                ".mp3" or ".wav" or ".ogg" => "🎵",
                _ => "📎"
            };
            return _iconeTypeCache;
        }
    }
}