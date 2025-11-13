using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models;

namespace Api_c_sharp.Models.Repository
{
    public class AutoPulseBdContext : DbContext
    {
        public AutoPulseBdContext(DbContextOptions<AutoPulseBdContext> options)
            : base(options)
        {
        }

        public DbSet<Adresse> Adresses { get; set; }
        public DbSet<Annonce> Annonces { get; set; }
        public DbSet<APourAdresse> APourAdresses { get; set; }
        public DbSet<APourConversation> APourConversations { get; set; }
        public DbSet<APourCouleur> APourCouleurs { get; set; }
        public DbSet<Avis> Avis { get; set; }
        public DbSet<BoiteDeVitesse> BoitesDeVitesses { get; set; }
        public DbSet<Carburant> Carburants { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Couleur> Couleurs { get; set; }
        public DbSet<EtatAnnonce> EtatAnnonces { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<Favori> Favoris { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Journal> Journaux { get; set; }
        public DbSet<Marque> Marques { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MiseEnAvant> MisesEnAvant { get; set; }
        public DbSet<Modele> Modeles { get; set; }
        public DbSet<ModeleBlender> ModelesBlender { get; set; }
        public DbSet<Motricite> Motricites { get; set; }
        public DbSet<MoyenPaiement> MoyensPaiements { get; set; }
        public DbSet<Pays> Pays { get; set; }
        public DbSet<Signalement> Signalements { get; set; }
        public DbSet<TypeCompte> TypesCompte { get; set; }
        public DbSet<TypeJournal> TypesJournal { get; set; }
        public DbSet<Ville> Villes { get; set; }
        public DbSet<Voiture> Voitures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //-----------------------------Adresse-----------------------------
            modelBuilder.Entity<Adresse>()
                .HasKey(a => a.IdAdresse); 

            modelBuilder.Entity<Adresse>()
                .HasOne(a => a.VilleAdresseNav)
                .WithMany(v => v.Adresses)
                .HasForeignKey(a => a.IdVille);

            modelBuilder.Entity<Adresse>()
                .HasMany(a => a.APourAdresses)
                .WithOne(ap => ap.AdresseAPourAdresseNav)
                .HasForeignKey(a => a.IdAdresse);

            //-----------------------------Annonce-----------------------------
            modelBuilder.Entity<Annonce>()
                .HasKey(a => a.IdAnnonce);

            modelBuilder.Entity<Annonce>()
                .HasOne(a => a.VoitureAnnonceNav)
                .WithMany(v => v.Annonces)
                .HasForeignKey(a => a.IdVoiture);

            modelBuilder.Entity<Annonce>()
                .HasOne(a => a.EtatAnnonceNavigation)
                .WithMany(e => e.Annonces)
                .HasForeignKey(a => a.IdEtatAnnonce);

            modelBuilder.Entity<Annonce>()
                .HasOne(a => a.AdresseAnnonceNav)
                .WithMany(ad => ad.Annonces)
                .HasForeignKey(a => a.IdAdresse);

            modelBuilder.Entity<Annonce>()
                .HasMany(a => a.Favoris)
                .WithOne(f => f.AnnonceFavoriNavigation)
                .HasForeignKey(f => f.IdAnnonce);

            modelBuilder.Entity<Annonce>()
                .HasOne(a => a.CompteAnnonceNav)
                .WithMany(c => c.Annonces)
                .HasForeignKey(a => a.IdCompte);

            modelBuilder.Entity<Annonce>()
                .HasOne(a => a.CommandeAnnonceNav)
                .WithOne(c => c.CommandeAnnonceNav)
                .HasForeignKey<Commande>(a => a.IdAnnonce);

            modelBuilder.Entity<Annonce>()
                .HasMany(a => a.Conversations)
                .WithOne(c => c.AnnonceConversationNav)
                .HasForeignKey(c => c.IdAnnonce);

            modelBuilder.Entity<Annonce>()

            //-----------------------------APourAdresse-----------------------------
            modelBuilder.Entity<APourAdresse>()
                .HasKey(e => new { e.IdAdresse, e.IdCompte });

            modelBuilder.Entity<APourConversation>()
                .HasKey(e => new { e.IdCompte, e.IdConversation });

            modelBuilder.Entity<APourCouleur>()
                .HasKey(e => new { e.IdCouleur, e.IdVoiture });


            modelBuilder.Entity<Favori>()
                .HasKey(e => new { e.IdAnnonce, e.IdCompte });

            modelBuilder.Entity<Compte>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Compte>()
                .HasIndex(e => e.Pseudo)
                .IsUnique();

            modelBuilder.Entity<Annonce>()
                .HasIndex(e => new { e.IdEtatAnnonce, e.DatePublication });

            modelBuilder.Entity<Avis>()
                .HasIndex(e => e.IdJugee);

            modelBuilder.Entity<Avis>()
                .HasIndex(e => e.IdJugeur);

            modelBuilder.Entity<Commande>()
                .HasIndex(e => e.IdVendeur);

            modelBuilder.Entity<Commande>()
                .HasIndex(e => e.IdAcheteur);

            modelBuilder.Entity<Ville>()
                .HasIndex(e => e.CodePostal);

            modelBuilder.Entity<Message>()
                .HasIndex(e => new { e.IdMessage, e.DateEnvoiMessage });

            modelBuilder.Entity<Journal>()
                .HasIndex(e => new { e.IdCompte, e.DateJournal });


            modelBuilder.Entity<MiseEnAvant>()
                .Property(e => e.PrixSemaine)
                .HasPrecision(10, 2);
        }
    }
}