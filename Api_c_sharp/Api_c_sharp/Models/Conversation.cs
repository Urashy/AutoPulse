using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_conversation_con")]
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("con_id")]
        public int IdConversation { get; set; }

        [Column("ann_id")]
        [Required]
        public int IdAnnonce { get; set; }

        [Column("con_date_dernier_message")]
        [Required]
        public DateTime DateDernierMessage { get; set; } = DateTime.Now;

        [ForeignKey(nameof(IdAnnonce))]
        [InverseProperty(nameof(Annonce.Conversations))]
        public virtual Annonce AnnonceConversationNav { get; set; }

        [InverseProperty(nameof(APourConversation.APourConversationConversationNav))]
        public virtual ICollection<APourConversation> ApourConversations { get; set; } = new List<APourConversation>();

        [InverseProperty(nameof(Message.ConversationMessageNav))]
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();


    }
}
