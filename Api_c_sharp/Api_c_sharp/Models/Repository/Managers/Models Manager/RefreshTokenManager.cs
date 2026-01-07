using System.Security.Cryptography;
using System.Text;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager;

public class RefreshTokenManager: WriteableReadableManager<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenManager(AutoPulseBdContext context) : base(context)
    {
    }

    public async Task<RefreshToken> StoreRefreshTokenAsync(
        int idCompte,
        string token,
        bool rememberMe,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var tokenHash = ComputeSha256Hash(token);

        var refreshToken = new RefreshToken
        {
            TokenHash = tokenHash,
            IdCompte = idCompte,
            DateCreation = DateTime.UtcNow,
            DateExpiration = rememberMe
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddDays(1),
            RememberMe = rememberMe,
            IpCreation = ipAddress,
            EstRevoque = false,
            UserAgent = userAgent
        };
        
        await dbSet.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        return refreshToken;
    }
    
    public async Task<Compte?> ValidateRefreshTokenAsync(string token)
    {
        var tokenHash = ComputeSha256Hash(token);

        var refreshToken = await dbSet
            .Include(rt => rt.CompteRefreshTokenNav)
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                !rt.EstRevoque &&
                rt.DateExpiration > DateTime.UtcNow);

        if (refreshToken == null)
        {
            return null;
        }

        return refreshToken.CompteRefreshTokenNav;
    }
    
    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var tokenHash = ComputeSha256Hash(token);

        var refreshToken = await dbSet
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (refreshToken == null)
        {
            return false;
        }

        refreshToken.EstRevoque = true;
        refreshToken.DateRevocation = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task RevokeAllUserTokensAsync(int idCompte)
    {
        var tokens = await dbSet
            .Where(rt => rt.IdCompte == idCompte && !rt.EstRevoque)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.EstRevoque = true;
            token.DateRevocation = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task<int> CleanupExpiredTokensAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        var expiredTokens = await dbSet
            .Where(rt =>
                rt.DateExpiration < DateTime.UtcNow ||
                (rt.EstRevoque && rt.DateRevocation < cutoffDate))
            .ToListAsync();

        dbSet.RemoveRange(expiredTokens);
        await context.SaveChangesAsync();

        return expiredTokens.Count;
    }

    public async Task<List<RefreshToken>> GetActiveUserTokensAsync(int idCompte)
    {
        return await dbSet
            .Where(rt =>
                rt.IdCompte == idCompte &&
                !rt.EstRevoque &&
                rt.DateExpiration > DateTime.UtcNow)
            .OrderByDescending(rt => rt.DateCreation)
            .ToListAsync();
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}