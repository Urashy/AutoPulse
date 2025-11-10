using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_j_apourconversation_apc")]
    public class APourConversation
    {
        [Key]
        [Column("com_id")]
        public int IdCompte { get; set; }

        [Key]
        [Column("con_id")]
        public int IdConversation { get; set; }

        [ForeignKey("IdCompte")]
        [InverseProperty(nameof(Compte.APourConversationCompteNav))]
        public virtual Compte APourConversationCompteNav { get; set; } = null!;

        [ForeignKey("IdConversation")]
        [InverseProperty(nameof(Conversation.APourConversationConversationNav))]
        public virtual Conversation APourConversationConversationNav { get; set; } = null!;
    }
}
