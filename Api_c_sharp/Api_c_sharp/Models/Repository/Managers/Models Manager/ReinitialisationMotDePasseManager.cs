using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager;

public class ReinitialisationMotDePasseManager: BaseManager<ReinitialisationMotDePasse, string>, IReinitialisationMotDePasse
{
    public ReinitialisationMotDePasseManager(AutoPulseBdContext context) : base(context)
    {
    }

    public virtual async Task<ReinitialisationMotDePasse> VerificationCode(string email, string code)
    {
        ReinitialisationMotDePasse entity = await dbSet.FirstOrDefaultAsync(r => r.Email == email && r.Token == code && r.Expiration > DateTime.UtcNow);
        return entity;
    }

    public override async Task<ReinitialisationMotDePasse?> GetByNameAsync(string name)
    {
        return await dbSet.Where(r => r.Token == name).FirstOrDefaultAsync();
    }
}