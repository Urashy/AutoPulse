using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_adresse_adr")]
    public class Adresse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("adr_id")]
        public int IdAdresse { get; set; }

        [Column("adr_nom")]
        [Required]
        public string Nom { get; set; } = null!;

        [Column("adr_numero")]
        [Required]
        public int Numero { get; set; }

        [Column("adr_rue")]
        [Required]
        public string Rue { get; set; } = null!;

        [Column("adr_libelleville")]
        [Required]
        public int LibelleVille { get; set; }

        [Column("adr_codepostal")]
        [Required]
        public string CodePostal { get; set; } = null!;

        [Column("com_id")]
        [Required]
        public int IdCompte { get; set; }

        [Column("pays_id")]
        [Required]
        public int IdPays { get; set; }

        [ForeignKey(nameof(IdCompte))]
        [InverseProperty(nameof(Compte.Adresses))]
        public virtual Compte CompteAdresseNav { get; set; } = null!;

        [InverseProperty(nameof(Annonce.AdresseAnnonceNav))]
        public virtual ICollection<Annonce> Annonces { get; set; } = new List<Annonce>();

        [ForeignKey(nameof(IdPays))]
        [InverseProperty(nameof(Pays.Adresses))]
        public virtual Pays PaysAdresseNav { get; set; } = null!;
    }
}
