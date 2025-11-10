using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_image_img")]
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("img_id")]
        public int IdImage { get; set; }

        [Column("img_fichier",TypeName ="bytea")]
        public byte[] Fichier { get; set; } = null!;



    }
}
