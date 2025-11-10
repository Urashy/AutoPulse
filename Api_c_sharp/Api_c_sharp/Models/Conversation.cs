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

        [InverseProperty(nameof(APourConversation.APourConversationConversationNav))]
        public virtual ICollection<APourConversation> ApourConversations { get; set; } = new List<APourConversation>();

    }
}
