using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace BlazorAutoPulse.Model
{
    public class ImageUpload
    {
        public int IdImage { get; set; }
        public int? IdVoiture { get; set; }
        public int? IdCompte { get; set; }

        public IBrowserFile File { get; set; }
    }
}
