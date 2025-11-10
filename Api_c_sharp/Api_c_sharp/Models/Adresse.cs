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

        [Column("adr_numero")]
        public string Numero { get; set; } = null!;

        [Column("adr_rue")]
        public string Rue { get; set; } = null!;

        [Column("vil_id")]
        public int IdVille { get; set; }

        [ForeignKey("IdVille")]
        [InverseProperty(nameof(Ville.Adresses))]
        public virtual Ville VilleAdresseNav { get; set; } = null!;

        [InverseProperty(nameof(APourAdresse.AdresseAPourAdresseNav))]
        public virtual ICollection<APourAdresse> APourAdresses { get; set; } = new List<APourAdresse>();
    }
}
