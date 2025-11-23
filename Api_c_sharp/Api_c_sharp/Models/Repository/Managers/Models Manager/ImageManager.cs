
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class ImageManager : WriteableReadableManager<Image>, IImageRepository
    {
        public ImageManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Image>> GetImagesByVoitureId(int voitureId)
        {
            return await dbSet.Where(img => img.IdVoiture == voitureId).ToListAsync();
        }
    }
}
