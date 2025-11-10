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

        // =======================================================
        // DbSets - Tables de base de données
        // =======================================================

        // Modules principaux
        public DbSet<Annonce> Annonces { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<ComptePro> ComptesPro { get; set; }
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Adresse> Adresses { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Facture> Factures { get; set; }

        // Entités de Configuration et de Référence
        public DbSet<Avis> Avis { get; set; }
        public DbSet<BoiteDeVitesse> BoitesDeVitesse { get; set; }
        public DbSet<Carburant> Carburants { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Couleur> Couleurs { get; set; }
        public DbSet<EtatAnnonce> EtatAnnonces { get; set; }
        public DbSet<Marque> Marques { get; set; }
        public DbSet<MiseEnAvant> MisesEnAvant { get; set; }
        public DbSet<Modele> Modeles { get; set; }
        public DbSet<ModeleBlender> ModelesBlender { get; set; }
        public DbSet<Motricite> Motricites { get; set; }
        public DbSet<MoyenPaiement> MoyensPaiement { get; set; }
        public DbSet<Pays> Pays { get; set; }
        public DbSet<TypeCompte> TypesCompte { get; set; }
        public DbSet<TypeJournal> TypesJournal { get; set; }
        public DbSet<Ville> Villes { get; set; }

        // Entités de Journalisation et de Relation
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Favori> Favoris { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Journal> Journaux { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Signalement> Signalements { get; set; }

        // Tables de jointure
        public DbSet<APourAdresse> APourAdresses { get; set; }
        public DbSet<APourConversation> APourConversations { get; set; }
        public DbSet<APourCouleur> APourCouleurs { get; set; }

        // =======================================================
        // Configuration Fluent API
        // =======================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =======================================================
            // Configuration des clés composites
            // =======================================================

            modelBuilder.Entity<APourCouleur>()
                .HasKey(e => new { e.IdCouleur, e.IdVoiture });

            modelBuilder.Entity<APourConversation>()
                .HasKey(e => new { e.IdCompte, e.IdConversation });

            modelBuilder.Entity<APourAdresse>()
                .HasKey(e => new { e.IdAdresse, e.IdCompte });

            modelBuilder.Entity<Favori>()
                .HasKey(e => new { e.IdAnnonce, e.IdCompte });

            // =======================================================
            // Configuration des index pour optimisation
            // =======================================================

            // Index uniques sur Compte
            modelBuilder.Entity<Compte>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Compte>()
                .HasIndex(e => e.Pseudo)
                .IsUnique();

            // Index unique sur ComptePro
            modelBuilder.Entity<ComptePro>()
                .HasIndex(e => e.NumeroSiret)
                .IsUnique();

            // Index pour optimiser les recherches d'annonces
            modelBuilder.Entity<Annonce>()
                .HasIndex(e => new { e.IdEtatAnnonce, e.DatePublication });

            // Index pour optimiser les recherches d'avis
            modelBuilder.Entity<Avis>()
                .HasIndex(e => e.IdJugee);

            modelBuilder.Entity<Avis>()
                .HasIndex(e => e.IdJugeur);

            // Index pour optimiser les recherches de commandes
            modelBuilder.Entity<Commande>()
                .HasIndex(e => e.IdVendeur);

            modelBuilder.Entity<Commande>()
                .HasIndex(e => e.IdAcheteur);

            // Index sur code postal
            modelBuilder.Entity<Ville>()
                .HasIndex(e => e.CodePostal);

            // Index pour optimiser les messages par conversation
            modelBuilder.Entity<Message>()
                .HasIndex(e => new { e.IdMessage, e.DateEnvoiMessage });

            // Index pour optimiser les journaux par compte
            modelBuilder.Entity<Journal>()
                .HasIndex(e => new { e.IdCompte, e.DateJournaux });

            // =======================================================
            // Configuration de la précision décimale
            // =======================================================

            modelBuilder.Entity<MiseEnAvant>()
                .Property(e => e.PrixSemaine)
                .HasPrecision(10, 2);
        }
    }
}