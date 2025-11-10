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
        public string ContenuMessage { get; set; } = null!;

        [Column("mes_dateenvoi")]
        public DateTime DateEnvoiMessage { get; set; }

        [Column("con_id")]

        [ForeignKey("IdConversation")]
        [InverseProperty(nameof(Conversation.IdConversation))]
        public virtual Conversation ConversationMessageNav { get; set; }
    }
}
