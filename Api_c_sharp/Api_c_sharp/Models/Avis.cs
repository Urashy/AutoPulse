using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models;

[Table("t_e_avis_avi")]
public class Avis
{
    [Key]
    [Column("avi_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdAvis { get; set; }

    [Required]
    [Column("uti_id_jugee")]
    public int IdJugee { get; set; }

    [Required]
    [Column("uti_id_jugeur")]
    public int IdJugeur { get; set; }

    [Required]
    [Column("cmd_id_commande")]
    public int IdCommande { get; set; }

    [Required]
    [Column("avi_date")]
    public DateTime DateAvis { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(1000, ErrorMessage = "Le contenu de l'avis ne doit pas dépasser 1000 caractères.")]
    [Column("avi_libelle")]
    public string ContenuAvis { get; set; }

    [Required]
    [Range(0, 5, ErrorMessage = "La note doit être comprise entre 0 et 5.")]
    [Column("avi_note")]
    public int NoteAvis { get; set; }
    
    [ForeignKey(nameof(IdAvis))]
    [InverseProperty(nameof(Commande.AvisListe))]
    public virtual Commande? CommandeNavigation { get; set; }
}