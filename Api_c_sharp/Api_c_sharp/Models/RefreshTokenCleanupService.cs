using Api_c_sharp.Models.Repository.Managers.Models_Manager;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🧹 Service de nettoyage des refresh tokens démarré");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Attendre 24h (ou au démarrage, attendre 1h)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var refreshTokenManager = scope.ServiceProvider
                    .GetRequiredService<RefreshTokenManager>();

                var deletedCount = await refreshTokenManager.CleanupExpiredTokensAsync();

                _logger.LogInformation("✅ Nettoyage effectué : {Count} token(s) supprimé(s)", 
                    deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du nettoyage des tokens");
            }
        }
    }
}