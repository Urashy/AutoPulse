using Api_c_sharp.Models.Repository.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_annonce_ann")]
public class Annonce : SearchableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ann_id")]
    public int IdAnnonce { get; set; }

    [Column("ann_nom")]
    public string Nom{ get; set; }

    [Column("com_id")]
    public int IdCompte{ get; set; }
    
    [Column("cmd_id")]
    public int IdCommande { get; set; }
        
    [Column("eta_id")]
    public int IdEtatAnnonce { get; set; }
    
    [Column("adr_id")]
    public int IdAdresseAnnonce { get; set; }
    
    [Column("voi_id")]
    public int IdVoiture { get; set; }

    [Column("mav_id")]
    public int? IdMiseEnAvant { get; set; }

    [Column("ann_dat")]
    public DateTime? DatePublication{ get; set; }

    [ForeignKey(nameof(IdEtatAnnonce))]
    [InverseProperty(nameof(EtatAnnonce.Annonces))]
    public virtual EtatAnnonce EtatAnnonceNavigation { get; set; }
    
    [ForeignKey(nameof(Favori.AnnonceFavoriNavigation))]
    [InverseProperty(nameof(Favori.AnnonceFavoriNavigation))]
    public virtual ICollection<Favori>Favoris { get; set; } = new List<Favori>();

    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.Annonces))]
    public virtual Compte CompteAnnonceNav { get; set; }

    [InverseProperty(nameof(Conversation.AnnonceConversationNav))]
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    [ForeignKey(nameof(IdCommande))]
    [InverseProperty(nameof(Commande.CommandeAnnonceNav))]
    public virtual Commande CommandeAnnonceNav { get; set; }

    [ForeignKey(nameof(IdMiseEnAvant))]
    [InverseProperty(nameof(MiseEnAvant.Annonces))]
    public virtual MiseEnAvant MiseEnAvantNavigation { get; set; }

    [ForeignKey(nameof(IdAdresseAnnonce))]
    [InverseProperty(nameof(Adresse.Annonces))]
    public virtual Adresse AdresseAnnonceNav { get; set; }

    [ForeignKey(nameof(IdVoiture))]
    [InverseProperty(nameof(Voiture.Annonces))]
    public virtual Voiture VoitureAnnonceNav { get; set; }

}