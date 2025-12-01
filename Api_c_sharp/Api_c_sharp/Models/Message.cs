using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table ("t_e_message_mes")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mes_id")]
        public int IdMessage { get; set; }

        [Column("mes_contenu")]
        [Required]
        public string ContenuMessage { get; set; } = null!;

        [Column("mes_dateenvoi")]
        public DateTime DateEnvoiMessage { get; set; } = DateTime.Now;

        [Column("con_id")]
        [Required]
        public int IdConversation { get; set; }

        [Column("com_id")]
        [Required]
        public int IdCompte { get; set; }

        [ForeignKey(nameof(IdCompte))]
        [InverseProperty(nameof(Compte.Messages))]
        public virtual Compte MessageCompteNav { get; set; }

        [ForeignKey(nameof(IdConversation))]
        [InverseProperty(nameof(Conversation.Messages))]
        public virtual Conversation ConversationMessageNav { get; set; }
    }
}
