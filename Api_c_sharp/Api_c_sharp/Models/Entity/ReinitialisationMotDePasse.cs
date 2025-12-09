using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_reinitialisationmotdepasse_rei")]
public class ReinitialisationMotDePasse
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("rei_id")]
    public int IdReinitialisationMdp { get; set; }
    
    [Column("com_id")]
    [Required]
    public int IdCompte { get; set; }
    
    [Column("com_email")]
    [Required]
    public string Email { get; set; } = null!;
    
    [Column("rei_token")]
    [Required]
    public string Token { get; set; }
    
    [Column("rei_expiration")]
    [Required]
    public DateTime Expiration { get; set; }
    
    [Column("rei_utilise")]
    [Required]
    public bool Utilise { get; set; }
}