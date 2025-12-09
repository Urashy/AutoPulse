using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager;

public class PieceJointeManager : WriteableReadableManager<PieceJointe>, IPieceJointeRepository
{
    public PieceJointeManager(AutoPulseBdContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PieceJointe>> GetByMessageIdAsync(int messageId)
    {
        return await dbSet
            .Where(pj => pj.IdMessage == messageId)
            .OrderBy(pj => pj.DateUpload)
            .ToListAsync();
    }
    
    public async Task<PieceJointe?> GetByIdWithContentAsync(int id)
    {
        return await dbSet
            .FirstOrDefaultAsync(pj => pj.IdPieceJointe == id);
    }
}