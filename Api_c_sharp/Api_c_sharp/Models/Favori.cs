using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_j_favori_fav")]
public class Favori
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("com_id")]
    public int IdCompte { get; set; }
    
    [Required]
    [Column("ann_id")]
    public int IdAnnonce { get; set; }
    
    [ForeignKey(nameof(IdAnnonce))]
    [InverseProperty(nameof(Annonce.Favoris))]
    public virtual Annonce AnnonceFavoriNavigation { get; set; }

    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.Favoris))]
    public virtual Annonce CompteFavoriNav { get; set; }
}