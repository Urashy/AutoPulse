using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models;

namespace Api_c_sharp.Models.Repository
{
    public class AutoPulseBdContext : DbContext
    {
        // Constructeur requis pour la configuration via le fichier Program.cs
        public AutoPulseBdContext(DbContextOptions<AutoPulseBdContext> options)
            : base(options)
        {
        }

        // =======================================================
        // Définition des DbSet (Vos tables de base de données)
        // =======================================================

        // Modules principaux (Annonce et ses dépendances directes)
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


        // =======================================================
        // API Fluent pour la configuration avancée
        // =======================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PostgreSQL est sensible aux noms de tables/colonnes en minuscules
            // et aux noms de colonnes/contraintes trop longs.
            // Le package Npgsql gère la majorité des conversions de noms (snake_case par défaut).

            // Si vous avez des problèmes de clés primaires/étrangères non-conventionnelles
            // ou des relations Many-to-Many sans classe intermédiaire, vous devez les configurer ici.

            // Exemple pour Annonce: 
            // Bien que [Key] dans Annonce.cs gère IdAnnonce, ceci peut être explicité:


            modelBuilder.Entity<APourCouleur>().HasKey(e => new { e.IdCouleur, e.IdVoiture});
            modelBuilder.Entity<APourConversation>().HasKey(e => new { e.IdCompte, e.IdConversation});
            modelBuilder.Entity<APourAdresse>().HasKey(e => new { e.IdAdresse, e.IdCompte});
            modelBuilder.Entity<Favori>().HasKey(e => new { e.IdAnnonce, e.IdCompte});



            base.OnModelCreating(modelBuilder);
        }
    }
}
