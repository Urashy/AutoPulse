using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_paiement_pmt")]
    public class Paiement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("pmt_id")]
        public int IdPaiement { get; set; }

        [Column("ann_id")]
        public int IdAnnonce { get; set; }

        [Column("cba_id")]
        public int? IdCarteBancaire { get; set; }

        [Column("mav_id")]
        public int? IdMiseEnAvant { get; set; }

        [Column("cmd_id")]
        public int? IdCommande { get; set; }

        [Column("com_id")]
        public int IdCompte { get; set; }

        [Column("pmpt_date")]
        public DateTime DatePaiement { get; set; }

        [ForeignKey(nameof(IdAnnonce))]
        [InverseProperty(nameof(Annonce.Paiements))]
        public Annonce PaiementAnnonceNav { get; set; }

        [ForeignKey(nameof(IdCarteBancaire))]
        [InverseProperty(nameof(CarteBancaire.Paiements))]
        public CarteBancaire PaiementCarteBancaireNav { get; set; }

        [ForeignKey(nameof(IdMiseEnAvant))]
        [InverseProperty(nameof(MiseEnAvant.Paiements))]
        public MiseEnAvant PaiementMiseEnAvantNav { get; set; }

        [ForeignKey(nameof(IdCommande))]
        [InverseProperty(nameof(Commande.Paiements))]
        public Commande PaiementCommandeNav { get; set; }

        [ForeignKey(nameof(IdCompte))]
        [InverseProperty(nameof(Compte.Paiements))]
        public Compte PaiementCompteNav { get; set; }
    }
}
