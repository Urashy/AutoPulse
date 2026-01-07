using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_commande_cmd")]
public class Commande
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("cmd_id")]
    public int IdCommande { get; set; }

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

    [Required]
    [Column("etc_id")]
    public int IdEtatCommande { get; set; }

    [Column("moy_moyenpaiement")]
    public int IdMoyenPaiement{ get; set; }

    [Required]
    [Column("off_idoffre")]
    public int IdOffre { get; set; }

    [ForeignKey(nameof(IdMoyenPaiement))]
    [InverseProperty(nameof(MoyenPaiement.Commandes))]
    public virtual MoyenPaiement? CommandeMoyenPaiementNav { get; set; }

    [InverseProperty(nameof(Facture.CommandeFactureNav))]
    public virtual ICollection<Facture> Factures { get; set; } = new List<Facture>();

    [InverseProperty(nameof(Avis.CommandeAvisNav))]
    public virtual ICollection<Avis> AvisListe { get; set; } = new List<Avis>();

    [ForeignKey(nameof(IdAnnonce))]
    [InverseProperty(nameof(Annonce.Commandes))]
    public virtual Annonce? CommandeAnnonceNav { get; set; }

    [ForeignKey(nameof(IdAcheteur))]
    [InverseProperty(nameof(Compte.CommandeAcheteur))]
    public virtual Compte? AcheteurCommande {  get; set; }

    [ForeignKey(nameof(IdVendeur))]
    [InverseProperty(nameof(Compte.CommandeVendeur))]
    public virtual Compte? VendeurCommande { get; set; }

    [ForeignKey(nameof(IdOffre))]
    [InverseProperty(nameof(Offre.CommandeOffre))]
    public virtual Offre? Offrecommande { get; set; }

    [ForeignKey(nameof(IdEtatCommande))]
    [InverseProperty(nameof(EtatCommande.Commandes))]
    public virtual EtatCommande EtatCommandeCommandeNav { get; set; } = null!;

    [InverseProperty(nameof(Paiement.PaiementCommandeNav))]
    public virtual ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
}