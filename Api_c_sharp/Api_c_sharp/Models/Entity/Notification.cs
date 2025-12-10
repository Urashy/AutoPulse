using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_notification_not")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("not_id")]
        public int IdNotification { get; set; }

        [Column("com_id")]
        [Required]
        public int IdCompte { get; set; }

        [Column("not_titre")]
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = null!;

        [Column("not_message")]
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = null!;

        [Column("not_type")]
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "info"; // info, success, warning, error, pricedrop

        [Column("not_url_navigation")]
        [StringLength(500)]
        public string? UrlNavigation { get; set; }

        [Column("not_est_lue")]
        public bool EstLue { get; set; } = false;

        [Column("not_date_creation")]
        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Données spécifiques aux baisses de prix
        [Column("ann_id")]
        public int? IdAnnonce { get; set; }

        [Column("not_ancien_prix")]
        public decimal? AncienPrix { get; set; }

        [Column("not_nouveau_prix")]
        public decimal? NouveauPrix { get; set; }

        // Navigation properties
        [ForeignKey(nameof(IdCompte))]
        [InverseProperty(nameof(Compte.Notifications))]
        public virtual Compte CompteNotificationNav { get; set; } = null!;

        [ForeignKey(nameof(IdAnnonce))]
        [InverseProperty(nameof(Annonce.Notifications))]
        public virtual Annonce? AnnonceNotificationNav { get; set; }
    }
}