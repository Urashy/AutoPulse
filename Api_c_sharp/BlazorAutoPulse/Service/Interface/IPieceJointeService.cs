using AutoPulse.Shared.DTO;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAutoPulse.Service.Interface;

public interface IPieceJointeService
{
    Task<List<PieceJointeDTO>> UploadFilesAsync(int idMessage, IReadOnlyList<IBrowserFile> files);
    Task<PieceJointeDTO?> GetByIdAsync(int id);
    Task<IEnumerable<PieceJointeDTO>> GetByMessageAsync(int idMessage);
    Task<bool> DeleteAsync(int id);
}