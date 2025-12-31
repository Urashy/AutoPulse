using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager;

public class TokenEmailManager : BaseManager<TokenEmail, string>
{
    public TokenEmailManager(AutoPulseBdContext context) : base(context)
    {
    }

    public override async Task<TokenEmail?> GetByNameAsync(string token)
    {
        return await dbSet.Where(t => t.Token == token).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Vérifie la validité d'un code pour un email et un type donné
    /// </summary>
    public virtual async Task<TokenEmail?> VerificationCode(string email, string code, string typeToken)
    {
        return await dbSet
            .Where(t => t.Email == email 
                        && t.Token == code 
                        && t.TypeToken == typeToken
                        && !t.Utilise
                        && t.Expiration > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Invalide tous les tokens d'un certain type pour un compte
    /// </summary>
    public virtual async Task InvaliderTokensParType(int idCompte, string typeToken)
    {
        var tokens = await dbSet
            .Where(t => t.IdCompte == idCompte && t.TypeToken == typeToken && !t.Utilise)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Utilise = true;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Supprime les tokens expirés
    /// </summary>
    public virtual async Task NettoyerTokensExpires()
    {
        var tokensExpires = await dbSet
            .Where(t => t.Expiration < DateTime.UtcNow || t.Utilise)
            .ToListAsync();

        dbSet.RemoveRange(tokensExpires);
        await context.SaveChangesAsync();
    }
}