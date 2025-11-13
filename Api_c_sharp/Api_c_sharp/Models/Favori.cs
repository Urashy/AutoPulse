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
    public virtual Annonce AnnonceFavoriNav { get; set; } = null!; 

    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.Favoris))]
    public virtual Compte CompteFavoriNav { get; set; } = null!;
}