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
                .WithOne(f => f.AnnonceFavoriNav)
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
                .HasOne(a => a.MiseEnAvantAnnonceNav)
                .WithMany(m => m.Annonces)
                .HasForeignKey(a => a.IdMiseEnAvant);


            //-----------------------------APourAdresse-----------------------------
            modelBuilder.Entity<APourAdresse>()
                .HasKey(e => new { e.IdAdresse, e.IdCompte });

            modelBuilder.Entity<APourAdresse>()
                .HasOne(ap => ap.AdresseAPourAdresseNav)
                .WithMany(a => a.APourAdresses)
                .HasForeignKey(ap => ap.IdAdresse);

            modelBuilder.Entity<APourAdresse>()
                .HasOne(ap => ap.CompteAPourAdresseNav)
                .WithMany(c => c.APourAdresses)
                .HasForeignKey(ap => ap.IdCompte);

            //-----------------------------APourConversation-----------------------------
            modelBuilder.Entity<APourConversation>()
                .HasKey(e => new { e.IdCompte, e.IdConversation });

            modelBuilder.Entity<APourConversation>()
                .HasOne(ac => ac.APourConversationCompteNav)
                .WithMany(c => c.ApourConversations)
                .HasForeignKey(ac => ac.IdCompte);

            modelBuilder.Entity<APourConversation>()
                .HasOne(ac => ac.APourConversationConversationNav)
                .WithMany(c => c.ApourConversations)
                .HasForeignKey(ac => ac.IdConversation);

            //-----------------------------APourCouleur-----------------------------
            modelBuilder.Entity<APourCouleur>()
                .HasKey(e => new { e.IdCouleur, e.IdVoiture });

            modelBuilder.Entity<APourCouleur>()
                .HasOne(ac => ac.APourCouleurCouleurNav)
                .WithMany(c => c.APourCouleurs)
                .HasForeignKey(ac => ac.IdCouleur);

            modelBuilder.Entity<APourCouleur>()
                .HasOne(ac => ac.APourCouleurVoitureNav)
                .WithMany(v => v.APourCouleurs)
                .HasForeignKey(ac => ac.IdVoiture);

            //-----------------------------Avis-----------------------------
            modelBuilder.Entity<Avis>()
                .HasKey(e => e.IdAvis);

            modelBuilder.Entity<Avis>()
                .HasOne(a => a.CompteJugeeNav)
                .WithMany(c => c.AvisJugees)
                .HasForeignKey(a => a.IdJugee);

            modelBuilder.Entity<Avis>()
                .HasOne(a => a.CompteJugeurNav)
                .WithMany(c => c.AvisJugeur)
                .HasForeignKey(a => a.IdJugeur);

           modelBuilder.Entity<Avis>()
                .HasOne(a => a.CommandeAvisNav)
                .WithMany(c => c.AvisListe)
                .HasForeignKey(a => a.IdCommande);

            //-----------------------------BoiteDeVitesse-----------------------------
            modelBuilder.Entity<BoiteDeVitesse>()
                .HasKey(e => e.IdBoiteDeVitesse);

            modelBuilder.Entity<BoiteDeVitesse>()
                .HasMany(b => b.Voitures)
                .WithOne(v => v.BoiteVoitureNavigation)
                .HasForeignKey(v => v.IdBoiteDeVitesse);
            //-----------------------------Carburant-----------------------------
            modelBuilder.Entity<Carburant>()
                .HasKey(e => e.IdCarburant);

            modelBuilder.Entity<Carburant>()
                .HasMany(c => c.Voitures)
                .WithOne(v => v.CarburantVoitureNavigation)
                .HasForeignKey(v => v.IdCarburant);

            //-----------------------------Categorie-----------------------------
            modelBuilder.Entity<Categorie>()
                .HasKey(e => e.IdCategorie);

            modelBuilder.Entity<Categorie>()
                .HasMany(c => c.Voitures)
                .WithOne(v => v.CategorieVoitureNavigation)
                .HasForeignKey(v => v.IdCategorie);

            //-----------------------------Commande-----------------------------
            modelBuilder.Entity<Commande>()
                .HasKey(e => e.IdCommande);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.CommandeAnnonceNav)
                .WithOne(a => a.CommandeAnnonceNav)
                .HasForeignKey<Annonce>(c => c.IdCommande);

            modelBuilder.Entity<Commande>()
                .HasMany(c => c.AvisListe)
                .WithOne(a => a.CommandeAvisNav)
                .HasForeignKey(c => c.IdCommande);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.CommandeFactureNavigation)
                .WithOne(f => f.CommandeFactureNavigation)
                .HasForeignKey<Facture>(c => c.IdFacture);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.CommandeMoyenPaiementNav)
                .WithMany(m => m.Commandes)
                .HasForeignKey(c => c.IdMoyenPaiement);

            //-----------------------------Compte-----------------------------
            modelBuilder.Entity<Compte>()
                .HasKey(e => e.IdCompte);

            modelBuilder.Entity<Compte>()
                .HasOne(c => c.TypeCompteCompteNav)
                .WithMany(t => t.Comptes)
                .HasForeignKey(c => c.IdTypeCompte);

            modelBuilder.Entity<Compte>()
                .HasMany(c => c.AvisJugees)
                .WithOne(a => a.CompteJugeeNav)
                .HasForeignKey(a => a.IdJugee);

            modelBuilder.Entity<Compte>()
                .HasMany(c => c.AvisJugeur)
                .WithOne(a => a.CompteJugeurNav)
                .HasForeignKey(a => a.IdJugeur);

            modelBuilder.Entity<Compte>()
                .HasMany(c => c.Annonces)
                .WithOne(a => a.CompteAnnonceNav)
                .HasForeignKey(a => a.IdCompte);

            modelBuilder.Entity<Compte>()
                .HasMany(c => c.Favoris)
                .WithOne(f => f.CompteFavoriNav)
                .HasForeignKey(f => f.IdCompte);

            //-----------------------------Favori-----------------------------
            modelBuilder.Entity<Favori>()
                .HasKey(e => new { e.IdAnnonce, e.IdCompte });

            modelBuilder.Entity<Favori>()
                .HasOne(f => f.AnnonceFavoriNav)
                .WithMany(a => a.Favoris)
                .HasForeignKey(f => f.IdAnnonce);

            modelBuilder.Entity<Favori>()
                .HasOne(f => f.CompteFavoriNav)
                .WithMany(c => c.Favoris)
                .HasForeignKey(f => f.IdCompte);



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