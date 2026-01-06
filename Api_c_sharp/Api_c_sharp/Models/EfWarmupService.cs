using Api_c_sharp.Models.Repository;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Services;

public class EfWarmupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public EfWarmupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("🔥 Warm-up EF Core...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AutoPulseBdContext>();

        try
        {
            await context.Comptes
                .Include(c => c.Images)
                .Include(c => c.TypeCompteCompteNav)
                .Where(c => c.IdCompte == -1)
                .FirstOrDefaultAsync(cancellationToken);

            sw.Stop();
            Console.WriteLine($"✅ Warm-up terminé en {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erreur warm-up: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}