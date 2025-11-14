using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models
{
    [Table("t_e_voiture_voi")]
    public class Voiture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("voi_id")]
        public int IdVoiture{ get; set; }

        [Column("mar_id")]
        [Required]
        public int IdMarque { get; set; }

        [Column("mot_id")]
        [Required]
        public int IdMotricite{ get; set; }

        [Column("car_id")]
        [Required]
        public int IdCarburant { get; set; }

        [Column("boi_id")]
        [Required]
        public int IdBoiteDeVitesse{ get; set; }

        [Column("cou_id")]
        [Required]
        public int IdCouleur{ get; set; }

        [Column("cat_id")]
        [Required]
        public int IdCategorie{ get; set; }

        [Column("voi_nbplace")]
        [DefaultValue(5)]
        public int NbPlace { get; set; }

        [Column("voi_nbporte")]
        public int NbPorte { get; set; }

        [Column("voi_kilometrage")]
        [Required]
        public int Kilometrage { get; set; }

        [Column("voi_annee")]
        [Required]
        public int Annee { get; set; }

        [Column("voi_puissance")]
        [Required]
        public int Puissance { get; set; }

        [Column("voi_couple")]
        public int Couple { get; set; }

        [Column("voi_nbcylindres")]
        public int NbCylindres { get; set; }

        [Column("mob_id")]
        public int? IdModeleBlender { get; set; }

        [Column("voi_miseencirculation")]
        [Required]
        public DateTime MiseEnCirculation { get; set; }

        [Column("mod_id")]
        [Required]
        public int IdModele { get; set; }

        [ForeignKey(nameof(IdModele))]
        [InverseProperty(nameof(Modele.Voitures))]
        public virtual Modele? ModeleVoitureNavigation { get; set; } = null!;

        [ForeignKey(nameof(IdMarque))]
        [InverseProperty(nameof(Marque.Voitures))]
        public virtual Marque? MarqueVoitureNavigation { get; set; } = null!;

        [ForeignKey(nameof(IdCategorie))]
        [InverseProperty(nameof(Categorie.Voitures))]
        public virtual Categorie? CategorieVoitureNavigation { get; set; } = null!;

        [ForeignKey(nameof(IdMotricite))]
        [InverseProperty(nameof(Motricite.Voitures))]
        public virtual Motricite? MotriciteVoitureNavigation { get; set; } = null!;

        [ForeignKey(nameof(IdCarburant))]
        [InverseProperty(nameof(Carburant.Voitures))]
        public virtual Carburant? CarburantVoitureNavigation { get; set; } = null!;

        [ForeignKey(nameof(IdBoiteDeVitesse))]
        [InverseProperty(nameof(BoiteDeVitesse.Voitures))]
        public virtual BoiteDeVitesse? BoiteVoitureNavigation { get; set; } = null!;

        [InverseProperty(nameof(APourCouleur.APourCouleurVoitureNav))]
        public virtual ICollection<APourCouleur> APourCouleurs { get; set; } = new List<APourCouleur>();

        [InverseProperty(nameof(Annonce.VoitureAnnonceNav))]
        public virtual ICollection<Annonce> Annonces { get; set; } = new List<Annonce>();


        [InverseProperty(nameof(Image.VoitureImageNav))]
        public virtual ICollection<Image> Images { get; set; } = new List<Image>();

        [ForeignKey(nameof(IdModeleBlender))]
        [InverseProperty(nameof(ModeleBlender.Voitures))]
        public virtual ModeleBlender? ModeleBlenderNavigation { get; set; }

    }
}
