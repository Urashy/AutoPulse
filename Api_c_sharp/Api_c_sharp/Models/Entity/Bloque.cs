using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity
{
    [Table("t_e_bloque_blo")]
    public class Bloque
    {
        [Column("blo_id")]
        public int IdBloquant { get; set; }
        [Column("com_id")]
        public int IdBloque { get; set; }
        [Column("blo_date")]
        public DateTime DateBloque { get; set; }


        [ForeignKey(nameof(IdBloquant))]
        [InverseProperty(nameof(Compte.ComptesBloquants))]
        public virtual Compte CompteBloquantNav { get; set; } = null!;

        [ForeignKey(nameof(IdBloque))]
        [InverseProperty(nameof(Compte.ComptesBloqueurs))]
        public virtual Compte CompteBloqueNav { get; set; } = null!;
    }
}
