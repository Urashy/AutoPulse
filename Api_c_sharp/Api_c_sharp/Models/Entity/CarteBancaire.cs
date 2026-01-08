using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_cartebancaire_cba")]
    public class CarteBancaire
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("cba_id")]
        public int IdCarteBancaire { get; set; }

        [Required]
        [Column("cba_numero")]
        public string NumeroCarte { get; set; } = null!;

        [Required]
        [Column("cba_code_securite")]
        public string CodeSecurite { get; set; } = null!;

        [Required]
        [Column("cba_date_expiration")]
        public DateTime DateExpiration { get; set; }

        [Required]
        [Column("com_idcompte")]
        public int IdCompte { get; set; }

        [Required]
        [Column("cba_typecarte")]
        public string TypeCarte { get; set; } = null!;

        [Column("cba_nomcarte")]
        public string? NomCarte { get; set; } 

        [ForeignKey(nameof(IdCompte))]
        [InverseProperty(nameof(Compte.CarteBancaires))]
        public virtual Compte? CompteCarteBancaireNav { get; set; }

        [InverseProperty(nameof(Paiement.PaiementCarteBancaireNav))]
        public virtual ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
    }
}
