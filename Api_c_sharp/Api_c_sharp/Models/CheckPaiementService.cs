using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Services;

public class CheckPaiementService: BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    
    public CheckPaiementService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("💶 Système de vérification de paiement démarré.");
    
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                PaiementManager paiementManager = scope.ServiceProvider.GetRequiredService<PaiementManager>();
                NotificationManager notificationManager = scope.ServiceProvider.GetRequiredService<NotificationManager>();
            
                List<Paiement> paiements = (await paiementManager.VerifPaiementAutoMiseEnAvant()).ToList();

                var paiementsData = paiements.Select(p => new 
                {
                    p.IdPaiement,
                    p.IdAnnonce,
                    p.IdCompte,
                    IdMiseEnAvant = (int)p.IdMiseEnAvant
                }).ToList();

                int nbIter = 0;
                foreach (var paiement in paiementsData)
                {
                    await notificationManager.NotifPaiementMiseEnAvant(
                        paiement.IdAnnonce, 
                        paiement.IdCompte, 
                        paiement.IdMiseEnAvant
                    );
                    _logger.LogInformation($"Paiement {paiement.IdPaiement}, notification envoyé");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du check des paiements");
            }
        
            await Task.Delay(TimeSpan.FromHours(10), stoppingToken);
        }
    }
}