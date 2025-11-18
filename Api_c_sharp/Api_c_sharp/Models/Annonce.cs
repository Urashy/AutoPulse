using Api_c_sharp.Models.Repository.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace Api_c_sharp.Models;

[Table("t_e_annonce_ann")]
public class Annonce
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ann_id")]
    public int IdAnnonce { get; set; }

    [Column("ann_nom")]
    [Required]
    public string Libelle { get; set; } = null!;

    [Column("com_id")]
    [Required]
    public int IdCompte{ get; set; }
   
        
    [Column("eta_id")]
    [Required]
    public int IdEtatAnnonce { get; set; }
    
    [Column("adr_id")]
    [Required]
    public int IdAdresse { get; set; }
    
    [Column("voi_id")]
    [Required]
    public int IdVoiture { get; set; }

    [Column("mav_id")]
    public int? IdMiseEnAvant { get; set; }

    [Column("ann_dat")]
    public DateTime? DatePublication{ get; set; }

    [Column("ann_pri")]
    public int Prix { get; set; }


    [ForeignKey(nameof(IdEtatAnnonce))]
    [InverseProperty(nameof(EtatAnnonce.Annonces))]
    public virtual EtatAnnonce EtatAnnonceNavigation { get; set; } = null!;

    [InverseProperty(nameof(Favori.AnnonceFavoriNav))]
    public virtual ICollection<Favori>Favoris { get; set; } = new List<Favori>();

    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.Annonces))]
    public virtual Compte CompteAnnonceNav { get; set; } =  null!;

    [InverseProperty(nameof(Conversation.AnnonceConversationNav))]
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    [InverseProperty(nameof(Commande.CommandeAnnonceNav))]
    public virtual ICollection<Commande> Commandes { get; set; }

    [ForeignKey(nameof(IdMiseEnAvant))]
    [InverseProperty(nameof(MiseEnAvant.Annonces))]
    public virtual MiseEnAvant MiseEnAvantAnnonceNav { get; set; } = null!;

    [ForeignKey(nameof(IdAdresse))]
    [InverseProperty(nameof(Adresse.Annonces))]
    public virtual Adresse AdresseAnnonceNav { get; set; } = null!;

    [ForeignKey(nameof(IdVoiture))]
    [InverseProperty(nameof(Voiture.Annonces))]
    public virtual Voiture VoitureAnnonceNav { get; set; } = null!;
}