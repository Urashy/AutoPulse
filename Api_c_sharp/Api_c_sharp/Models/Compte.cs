using Api_c_sharp.Models.Repository.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_compte_com")]
    public class Compte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("com_id")]
        public int IdCompte { get; set; }

        [Column("com_pseudo")]
        [Required]  
        public string Pseudo { get; set; } = null!;

        [Column("com_mdp")]
        [Required]
        public string MotDePasse { get; set; } = null!;

        [Column("com_nom")]
        [Required]
        public string Nom { get; set; } = null!;

        [Column("com_prenom")]
        [Required]
        public string Prenom { get; set; } = null!;

        [Column("com_email")]
        [Required]
        public string Email { get; set; } = null!;

        [Column("com_date_creation")]
        [Required]
        public DateTime DateCreation { get; set; }

        [Column("com_date_derniere_connexion")]
        [Required]
        public DateTime? DateDerniereConnexion { get; set; }

        [Column("com_date_naissance")]
        [Required]
        public DateTime DateNaissance { get; set; }

        [Column("com_biographie")]
        public string? Biographie { get; set; }

        [Column("tco_id")]
        [Required]
        public int IdTypeCompte { get; set; }

        [Column("cpr_siret")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "Le numéro SIRET doit contenir exactement 14 chiffres")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "Le SIRET doit contenir uniquement des chiffres")]
        public string? NumeroSiret { get; set; } 

        [Column("cpr_raison_sociale")]
        public string? RaisonSociale { get; set; }
        
        [Column("com_google_id")]
        public string? GoogleId { get; set; }

        [Column("com_auth_provider")]
        public string? AuthProvider { get; set; }

        [ForeignKey("IdCompte")]
        [InverseProperty(nameof(TypeCompte.Comptes))]
        public virtual TypeCompte TypeCompteCompteNav { get; set; } = null!;

        [InverseProperty(nameof(Adresse.CompteAdresseNav))]
        public virtual ICollection<Adresse> Adresses { get; set; } = new List<Adresse>();

        [InverseProperty(nameof(APourConversation.APourConversationCompteNav))]
        public virtual ICollection<APourConversation> ApourConversations { get; set; } = new List<APourConversation>();    

        [InverseProperty(nameof(Signalement.CompteSignalantNav))]
        public virtual ICollection<Signalement> SignalementsFaits { get; set; } = new List<Signalement>();

        [InverseProperty(nameof(Signalement.CompteSignaleNav))]
        public virtual ICollection<Signalement> SignalementsRecus { get; set; } = new List<Signalement>();

        [InverseProperty(nameof(Journal.CompteJournauxNav))]
        public virtual ICollection<Journal> Journaux { get; set; } = new List<Journal>();

        [InverseProperty(nameof(Avis.CompteJugeeNav))]
        public virtual ICollection<Avis> AvisJugees { get; set; } = new List<Avis>();

        [InverseProperty(nameof(Avis.CompteJugeurNav))]
        public virtual ICollection<Avis> AvisJugeur { get; set; } = new List<Avis>();

        [InverseProperty(nameof(Annonce.CompteAnnonceNav))]
        public virtual ICollection<Annonce> Annonces { get; set; } = new List<Annonce>();

        [InverseProperty(nameof(Favori.CompteFavoriNav))]
        public virtual ICollection<Favori> Favoris { get; set; } = new List<Favori>();

        [InverseProperty(nameof(Image.CompteImageNav))]
        public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    }
}
