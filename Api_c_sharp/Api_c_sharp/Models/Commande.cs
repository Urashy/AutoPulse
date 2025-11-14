using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_commande_cmd")]
public class Commande
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("cmd_id")]
    public int IdCommande { get; set; }

    [Column("fac_id")]
    public int? IdFacture { get; set; }

    [Required]
    [Column("com_id_vendeur")]
    public int IdVendeur { get; set; }

    [Required]
    [Column("com_id_acheteur")]
    public int IdAcheteur { get; set; }

    [Required]
    [Column("cmd_id_annonce")]
    public int IdAnnonce { get; set; }

    [Required]
    [Column("cmd_date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    [Column("cmd_moyen_paiement")]
    public int IdMoyenPaiement{ get; set; }
    
    [ForeignKey(nameof(IdMoyenPaiement))]
    [InverseProperty(nameof(MoyenPaiement.Commandes))]
    public virtual MoyenPaiement CommandeMoyenPaiementNav { get; set; }

    [ForeignKey(nameof(IdFacture))]
    [InverseProperty(nameof(Facture.CommandeFactureNavigation))]
    public virtual Facture CommandeFactureNavigation { get; set; }
    
    [InverseProperty(nameof(Avis.CommandeAvisNav))]
    public virtual ICollection<Avis> AvisListe { get; set; }

    [ForeignKey(nameof(IdAnnonce))]
    [InverseProperty(nameof(Annonce.CommandeAnnonceNav))]
    public virtual Annonce CommandeAnnonceNav { get; set; }
}