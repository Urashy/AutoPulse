using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_annonce_ann")]
public class Annonce
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ann_id")]
    public int IdAnnonce { get; set; }
    
    [Column("uti_id")]
    public int IdUtilisateur { get; set; }
    
    [Column("cmd_id")]
    public int IdCommande { get; set; }
    
    [Column("mea_id")]
    public int IdMiseEnAvant { get; set; }
    
    [Column("eta_id")]
    public int IdEtatAnnonce { get; set; }
    
    [Column("adr_id")]
    public int IdAdresseAnnonce { get; set; }
    
    [Column("voi_id")]
    public int IdVoiture { get; set; }
    
    [ForeignKey(nameof(IdEtatAnnonce))]
    [InverseProperty(nameof(EtatAnnonce.Annonces))]
    public virtual EtatAnnonce EtatAnnonceNavigation { get; set; }
    
    [ForeignKey(nameof(Favori.AnnonceFavoriNavigation))]
    public virtual ICollection<Favori>Favoris { get; set; } = new List<Favori>();
    
}