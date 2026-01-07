using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_refresh_token_rft")]
public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("rft_id")]
    public int IdRefreshToken { get; set; }

    [Column("rft_token_hash")]
    [Required]
    [StringLength(64)]
    public string TokenHash { get; set; } = null!;

    [Column("com_id")]
    [Required]
    public int IdCompte { get; set; }

    [Column("rft_date_creation")]
    [Required]
    public DateTime DateCreation { get; set; }

    [Column("rft_date_expiration")]
    [Required]
    public DateTime DateExpiration { get; set; }

    [Column("rft_est_revoque")]
    public bool EstRevoque { get; set; } = false;

    [Column("rft_date_revocation")]
    public DateTime? DateRevocation { get; set; }
    
    [Column("rft_remember_me")]
    public bool RememberMe { get; set; } = false;

    [Column("rft_ip_creation")]
    [StringLength(45)]
    public string? IpCreation { get; set; }

    [Column("rft_user_agent")]
    [StringLength(500)]
    public string? UserAgent { get; set; }

    [ForeignKey(nameof(IdCompte))]
    [InverseProperty(nameof(Compte.RefreshTokens))]
    public virtual Compte CompteRefreshTokenNav { get; set; } = null!;
}