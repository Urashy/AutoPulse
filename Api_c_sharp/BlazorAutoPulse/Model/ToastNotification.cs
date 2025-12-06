namespace BlazorAutoPulse.Model;

/// <summary>
/// Modèle de notification toast
/// </summary>
public class ToastNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? NavigationUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DurationMs { get; set; } = 3000;
}
    
/// <summary>
/// Types de notifications avec icônes et couleurs associées
/// </summary>
public enum NotificationType
{
    Success,    // ✓ Vert
    Info,       // ℹ Bleu
    Warning,    // ⚠ Orange
    Error,      // ✕ Rouge
    Message     // 💬 Cyan
}