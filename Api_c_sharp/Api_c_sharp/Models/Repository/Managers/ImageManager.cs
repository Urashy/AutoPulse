
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class ImageManager : BaseManager<Image, int>
    {
        public ImageManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<Image?> GetByNameAsync(int id)
        {
            return await dbSet.FirstOrDefaultAsync(e => e.IdImage == id);
        }
    }
}
