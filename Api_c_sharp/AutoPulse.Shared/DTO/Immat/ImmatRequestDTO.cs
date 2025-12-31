using System.ComponentModel.DataAnnotations;

namespace AutoPulse.Shared.DTO.Immat
{
    public class ImmatRequestDTO
    {
        [Required(ErrorMessage = "La plaque d'immatriculation est requise")]
        [RegularExpression(@"^[A-Z]{2}-\d{3}-[A-Z]{2}$", ErrorMessage = "Format de plaque invalide (ex: AB-123-CD)")]
        public string PlateNumber { get; set; } = string.Empty;
    }
}