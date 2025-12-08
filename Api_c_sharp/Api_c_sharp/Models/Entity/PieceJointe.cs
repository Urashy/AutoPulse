using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_piecejointe_pj")]
public class PieceJointe
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("pj_id")]
    public int IdPieceJointe { get; set; }

    [Column("mes_id")]
    [Required]
    public int IdMessage { get; set; }

    [Column("pj_nom_fichier")]
    [Required]
    [MaxLength(255)]
    public string NomFichier { get; set; } = null!;

    [Column("pj_type_mime")]
    [Required]
    [MaxLength(100)]
    public string TypeMime { get; set; } = null!;

    [Column("pj_extension")]
    [Required]
    [MaxLength(10)]
    public string Extension { get; set; } = null!;

    [Column("pj_taille_fichier")]
    [Required]
    public long TailleFichier { get; set; }

    [Column("pj_contenu")]
    [Required]
    public byte[] Contenu { get; set; } = null!;

    [Column("pj_date_upload")]
    public DateTime DateUpload { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdMessage))]
    [InverseProperty(nameof(Message.PiecesJointes))]
    public virtual Message MessagePjNav { get; set; } = null!;
}