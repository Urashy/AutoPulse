using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_j_apouradresse_apa")]
    public class APourAdresse
    {
        [Key]
        [Column("adr_id")]
        public int IdAdresse { get; set; }

        [Key]
        [Column("com_id")]
        public int IdCompte { get; set; }

        [ForeignKey("IdAdresse")]
        [InverseProperty(nameof(Adresse.APourAdresses))]
        public virtual Adresse AdresseAPourAdresseNav { get; set; } = null!;

        [ForeignKey("IdCompte")]
        [InverseProperty(nameof(Compte.APourAdresses))]
        public virtual Compte CompteAPourAdresseNav { get; set; } = null!;
    }
}
