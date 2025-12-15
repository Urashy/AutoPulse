using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_offre_off")]
    public class Offre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("off_id")]
        public int IdOffre { get; set; }

        [Column("ann_id")]
        [Required]
        public int IdAnnonce { get; set; }

        [Column("mes_id")]
        [Required]
        public int IdMessage { get; set; }

        [Column("off_valeur")]
        [Required]
        public decimal Valeur { get; set; }

        [Column("off_date")]
        [Required]
        public DateTime DateOffre { get; set; } = DateTime.UtcNow;

        [Column("off_estaccepte")]
        public bool? EstAccepte { get; set; }

        // Navigation properties
        [ForeignKey(nameof(IdAnnonce))]
        [InverseProperty(nameof(Annonce.Offres))]
        public virtual Annonce OffreAnnonceNav { get; set; } = null!;

        [ForeignKey(nameof(IdMessage))]
        [InverseProperty(nameof(Message.Offres))]
        public virtual Message OffreMessageNav { get; set; } = null!;
    }
}