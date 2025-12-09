using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Models.Repository.Interfaces;

public interface IPieceJointeRepository : WritableRepository<PieceJointe>
{
    Task<IEnumerable<PieceJointe>> GetByMessageIdAsync(int messageId);
    Task<PieceJointe?> GetByIdWithContentAsync(int id);
}