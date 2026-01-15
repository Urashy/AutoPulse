using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_tokenemail_tke")]
public class TokenEmail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("tke_id")]
    public int IdTokenEmail { get; set; }
    
    [Column("com_id")]
    public int? IdCompte { get; set; }
    
    [Column("com_email")]
    [Required]
    public string Email { get; set; } = null!;
    
    [Column("tke_token")]
    [Required]
    public string Token { get; set; } = null!;
    
    [Column("tke_expiration")]
    [Required]
    public DateTime Expiration { get; set; }
    
    [Column("tke_utilise")]
    [Required]
    public bool Utilise { get; set; }
    
    [Column("tke_type")]
    [Required]
    [StringLength(50)]
    public string TypeToken { get; set; } = null!;
    
    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.TokensEmail))]
    public virtual Compte CompteTokenNav { get; set; } = null!;
}