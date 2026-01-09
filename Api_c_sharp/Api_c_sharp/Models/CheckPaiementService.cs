using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Services;

public class CheckPaiementService: BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CheckPaiementService> _logger;
    
    public CheckPaiementService(
        IServiceProvider serviceProvider,
        ILogger<CheckPaiementService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    private static async Task WaitUntilScheduledTimeAsync(CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var scheduledTime = new TimeSpan(23, 50, 0);
        
        var nextRun = now.Date.Add(scheduledTime);
        
        if (now.TimeOfDay > scheduledTime)
        {
            nextRun = nextRun.AddDays(1);
        }
        
        var delay = nextRun - now;
        
        Console.WriteLine($"⏰ Prochaine vérification planifiée à {nextRun:yyyy-MM-dd HH:mm:ss} UTC (dans {delay.TotalHours:F2} heures)");
        
        await Task.Delay(delay, token);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("💶 Système de vérification de paiement démarré.");
    
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await WaitUntilScheduledTimeAsync(stoppingToken);
                
                _logger.LogInformation("🚀 Démarrage de la vérification des paiements...");
                
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

                _logger.LogInformation($"📊 {paiementsData.Count} paiement(s) à traiter");

                foreach (var paiement in paiementsData)
                {
                    await notificationManager.NotifPaiementMiseEnAvant(
                        paiement.IdAnnonce, 
                        paiement.IdCompte, 
                        paiement.IdMiseEnAvant
                    );
                    _logger.LogInformation($"✅ Paiement {paiement.IdPaiement}, notification envoyée");
                }
                
                _logger.LogInformation("✨ Vérification terminée avec succès");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Service de vérification de paiement arrêté");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du check des paiements attente de 5 minutes");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}