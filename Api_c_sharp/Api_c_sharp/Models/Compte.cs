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
        public string Pseudo { get; set; } = null!;

        [Column("com_mdp")]
        public string MotDePasse { get; set; } = null!;

        [Column("com_nom")]
        public string Nom { get; set; } = null!;

        [Column("com_prenom")]
        public string Prenom { get; set; } = null!;

        [Column("com_email")]
        public string Email { get; set; } = null!;

        [Column("com_date_creation")]
        public DateTime DateCreation { get; set; }

        [Column("com_date_derniere_connexion")]
        public DateTime? DateDerniereConnexion { get; set; }

        [Column("com_date_naissance")]
        public DateTime DateNaissance { get; set; }

        [Column("com_biographie")]
        public string? Biographie { get; set; }

        [Column("tco_id")]
        public int IdTypeCompte { get; set; }

        [ForeignKey("IdUtilisateur")]
        [InverseProperty(nameof(TypeCompte.Comptes))]
        public virtual TypeCompte TypeCompteCompteNav { get; set; } = null!;

        [InverseProperty(nameof(APourAdresse.CompteAPourAdresseNav))]
        public virtual ICollection<APourAdresse> APourAdresses { get; set; } = new List<APourAdresse>();

        [InverseProperty(nameof(APourConversation.APourConversationCompteNav))]
        public virtual ICollection<Conversation> ApourConversations { get; set; } = new List<Conversation>();    

        [InverseProperty(nameof(Signalement.CompteSignalantNav))]
        public virtual ICollection<Signalement> SignalementsFaits { get; set; } = new List<Signalement>();

        [InverseProperty(nameof(Signalement.CompteSignaleNav))]
        public virtual ICollection<Signalement> SignalementsRecus { get; set; } = new List<Signalement>();

        [InverseProperty(nameof(Journal.CompteJournauxNav))]
        public virtual ICollection<Journal> Journaux { get; set; } = new List<Journal>();

    }
}
