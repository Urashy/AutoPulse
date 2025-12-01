using Microsoft.AspNetCore.Http;

namespace AutoPulse.Shared.DTO
{
    public class ImageUploadDTO
    {
        public int IdImage { get; set; }
        public int? IdVoiture { get; set; }
        public int? IdCompte { get; set; }

        public IFormFile File { get; set; }
    }

}
