using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_signalement_sig")]
    public class Signalement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("sig_id")]
        public int IdSignalement { get; set; }

        [Column("sig_description")]
        public string? DescriptionSignalement { get; set; }
    }
}
