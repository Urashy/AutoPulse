
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_j_vue_vue")]
public class Vue
{
    [Key]
    [Column("com_id")]
    public int IdCompte { get; set; }

    [Key]
    [Column("ann_id")]
    public int IdAnnonce { get; set; }

    [ForeignKey(nameof(IdAnnonce))]
    [InverseProperty(nameof(Annonce.Vues))]
    public virtual Annonce AnnonceVueNav { get; set; } = null!;

    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.Vues))]
    public virtual Compte CompteVueNav { get; set; } = null!;
}

