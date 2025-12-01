using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface IReinitialiseMdp: IService<ReinitialisationMdp>
{
    Task<bool> VerifCode(ReinitialisationMdp data);
    Task DeleteByNameAsync(string id);
}