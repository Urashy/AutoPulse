
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class ImageManager : WriteableReadableManager<Image>, IImageRepository
    {
        public ImageManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<IEnumerable<int>> GetAllImagesByVoitureId(int voitureId)
        {
            var list = await dbSet
                .Where(img => img.IdVoiture == voitureId)
                .ToListAsync();

            List<int> ret = new List<int>();

            foreach (Image element in list)
            {
                ret.Add(element.IdImage);
            }

            return ret;
        }

        public virtual async Task<Image> GetFirstImageByVoitureID(int idvoiture)
        {
            return await dbSet.FirstOrDefaultAsync(img => img.IdVoiture == idvoiture);
        }

        public virtual async Task<Image> GetImageByCompteID(int idcompte)
        {
            return await dbSet.FirstOrDefaultAsync(img => img.IdCompte == idcompte);
        }
    }
}
