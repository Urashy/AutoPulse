using Microsoft.EntityFrameworkCore;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Models.Repository
{
    public partial class AutoPulseBdContext : DbContext
    {
        public AutoPulseBdContext()
        {
        }
        public AutoPulseBdContext(DbContextOptions<AutoPulseBdContext> options)
            : base(options)
        {
        }

        public DbSet<Adresse> Adresses { get; set; }
        public DbSet<Annonce> Annonces { get; set; }
        public DbSet<APourConversation> APourConversations { get; set; }
        public DbSet<APourCouleur> APourCouleurs { get; set; }
        public DbSet<Avis> Avis { get; set; }
        public virtual DbSet<BenchmarkIA> BenchmarksIA { get; set; }
        public DbSet<Bloque> Bloques { get; set; }
        public DbSet<BoiteDeVitesse> BoitesDeVitesses { get; set; }
        public DbSet<Carburant> Carburants { get; set; }
        public DbSet<CarteBancaire> CarteBancaires { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Couleur> Couleurs { get; set; }
        public DbSet<EtatAnnonce> EtatAnnonces { get; set; }
        public DbSet<EtatCommande> EtatCommandes { get; set; }
        public DbSet<EtatCompte> EtatComptes { get; set; }
        public DbSet<EtatSignalementPlainte> EtatSignalementsPlaintes { get; set; }
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Offre> Offres { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Pays> Pays { get; set; }
        public DbSet<PieceJointe> PiecesJointes { get; set; }
        public DbSet<Plainte> Plaintes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TokenEmail> TokenEmails { get; set; }
        public DbSet<Signalement> Signalements { get; set; }
        public DbSet<TypeCompte> TypesCompte { get; set; }
        public DbSet<TypeJournal> TypesJournal { get; set; }
        public DbSet<TypeSignalement> TypesSignalement { get; set; }
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Vue> Vues { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");

            //-----------------------------Adresse-----------------------------
            modelBuilder.Entity<Adresse>()
                .HasKey(a => a.IdAdresse); 
            
            modelBuilder.Entity<TokenEmail>()
                .HasKey(r => r.IdTokenEmail); 

            modelBuilder.Entity<Adresse>()
                .HasOne(a => a.PaysAdresseNav)
                .WithMany(v => v.Adresses)
                .HasForeignKey(a => a.IdPays);

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
                .HasOne(a => a.CompteAnnonceNav)
                .WithMany(c => c.Annonces)
                .HasForeignKey(a => a.IdCompte);

            modelBuilder.Entity<Annonce>()
                .HasOne(a => a.MiseEnAvantAnnonceNav)
                .WithMany(m => m.Annonces)
                .HasForeignKey(a => a.IdMiseEnAvant);
            
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
           
           //-----------------------------BenchmarkIA-----------------------------
           modelBuilder.Entity<BenchmarkIA>()
               .HasKey(e => e.IdBenchmark);

           modelBuilder.Entity<BenchmarkIA>()
               .HasIndex(e => e.BenchmarkId);

           modelBuilder.Entity<BenchmarkIA>()
               .HasIndex(e => new { e.ModelType, e.Timestamp });

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.AvgInferenceTimeMs)
               .HasPrecision(18, 6);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.MinInferenceTimeMs)
               .HasPrecision(18, 6);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.MaxInferenceTimeMs)
               .HasPrecision(18, 6);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.StdInferenceTimeMs)
               .HasPrecision(18, 6);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.PredictionsPerSecond)
               .HasPrecision(18, 6);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.TotalTimeSeconds)
               .HasPrecision(18, 6);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.SuccessRatePercent)
               .HasPrecision(5, 2);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.MemoryTotalGb)
               .HasPrecision(10, 2);

           modelBuilder.Entity<BenchmarkIA>()
               .Property(e => e.MemoryAvailableGb)
               .HasPrecision(10, 2);

            //-----------------------------Bloque-----------------------------
            modelBuilder.Entity<Bloque>()
                .HasKey(e => new { e.IdBloque, e.IdBloquant });

            modelBuilder.Entity<Bloque>()
                .HasOne(b => b.CompteBloqueNav)
                .WithMany(c => c.ComptesBloqueurs)
                .HasForeignKey(b => b.IdBloque);

            modelBuilder.Entity<Bloque>()
                .HasOne(b => b.CompteBloquantNav)
                .WithMany(c => c.ComptesBloquants)
                .HasForeignKey(b => b.IdBloquant);

            //-----------------------------BoiteDeVitesse-----------------------------
            modelBuilder.Entity<BoiteDeVitesse>()
                .HasKey(e => e.IdBoiteDeVitesse);

            //-----------------------------Carburant-----------------------------
            modelBuilder.Entity<Carburant>()
                .HasKey(e => e.IdCarburant);

            //-----------------------------CarteBancaire-----------------------------
            modelBuilder.Entity<CarteBancaire>()
                .HasKey(e => e.IdCarteBancaire);

            modelBuilder.Entity<CarteBancaire>()
                .HasOne(b => b.CompteCarteBancaireNav)
                .WithMany(c => c.CarteBancaires)
                .HasForeignKey(b => b.IdCompte);

            //-----------------------------Categorie-----------------------------
            modelBuilder.Entity<Categorie>()
                .HasKey(e => e.IdCategorie);

            //-----------------------------Commande-----------------------------
            modelBuilder.Entity<Commande>()
                .HasKey(e => e.IdCommande);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.CommandeAnnonceNav)
                .WithMany(a => a.Commandes)
                .HasForeignKey(c => c.IdAnnonce);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.CommandeMoyenPaiementNav)
                .WithMany(m => m.Commandes)
                .HasForeignKey(c => c.IdMoyenPaiement);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.AcheteurCommande)
                .WithMany(a => a.CommandeAcheteur)
                .HasForeignKey(c => c.IdAcheteur);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.VendeurCommande)
                .WithMany(v => v.CommandeVendeur)
                .HasForeignKey(c => c.IdVendeur);

            modelBuilder.Entity<Commande>()
                .HasOne(c => c.EtatCommandeCommandeNav)
                .WithMany(e => e.Commandes)
                .HasForeignKey(c => c.IdEtatCommande);

            //-----------------------------Compte-----------------------------
            modelBuilder.Entity<Compte>()
                .HasKey(e => e.IdCompte);

            modelBuilder.Entity<Compte>()
                .HasOne(c => c.TypeCompteCompteNav)
                .WithMany(t => t.Comptes)
                .HasForeignKey(c => c.IdTypeCompte);

            modelBuilder.Entity<Compte>()
                .HasOne(c => c.EtatCompteNav)
                .WithMany(e => e.Comptes)
                .HasForeignKey(c => c.IdEtatCompte);

            modelBuilder.Entity<Compte>()
                .HasIndex(e => e.Pseudo)
                .IsUnique();

            modelBuilder.Entity<Compte>()
                .HasIndex(e => e.Email)
                .IsUnique(); 

            //-----------------------------Conversation-----------------------------
            modelBuilder.Entity<Conversation>()
                .HasKey(e => e.IdConversation);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.AnnonceConversationNav)
                .WithMany(a => a.Conversations)
                .HasForeignKey(c => c.IdAnnonce);

            //-----------------------------Couleur-----------------------------
            modelBuilder.Entity<Couleur>()
                .HasKey(e => e.IdCouleur);

            //-----------------------------EtatAnnonce-----------------------------
            modelBuilder.Entity<EtatAnnonce>()
                .HasKey(e => e.IdEtatAnnonce);

            //-----------------------------EtatCommande-----------------------------
            modelBuilder.Entity<EtatCommande>()
                .HasKey(e => e.IdEtatCommande);

            //-----------------------------EtatCompte-----------------------------
            modelBuilder.Entity<EtatCompte>()
                .HasKey(e => e.IdEtatCompte);

            //-----------------------------EtatSignalement-----------------------------
            modelBuilder.Entity<EtatSignalementPlainte>()
                .HasKey(e => e.IdEtatSignalement);

            //-----------------------------Facture-----------------------------

            modelBuilder.Entity<Facture>()
                .HasKey(e => e.IdFacture);

            modelBuilder.Entity<Facture>()
                .HasOne(f => f.CommandeFactureNav)
                .WithMany(c => c.Factures)
                .HasForeignKey(f => f.IdFacture);

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

            //-----------------------------Image-----------------------------
            modelBuilder.Entity<Image>()
                .HasKey(e => e.IdImage);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.VoitureImageNav)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.IdVoiture)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.CompteImageNav)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.IdCompte)
                .OnDelete(DeleteBehavior.Cascade);

            //-----------------------------Journal-----------------------------
            modelBuilder.Entity<Journal>()
                .HasKey(e => e.IdJournal);

            modelBuilder.Entity<Journal>()
                .HasOne(j => j.TypeJournauxJournauxNav)
                .WithMany(t => t.Journaux)
                .HasForeignKey(j => j.IdTypeJournal);

            modelBuilder.Entity<Journal>()
                .HasOne(j => j.CompteJournauxNav)
                .WithMany(c => c.Journaux)
                .HasForeignKey(j => j.IdCompte);

            //-----------------------------Marque-----------------------------
            modelBuilder.Entity<Marque>()
                .HasKey(e => e.IdMarque);

            //-----------------------------Message-----------------------------
            modelBuilder.Entity<Message>()
                .HasKey(e => e.IdMessage);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ConversationMessageNav)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.IdConversation);

            //-----------------------------MiseEnAvant-----------------------------
            modelBuilder.Entity<MiseEnAvant>()
                .HasKey(e => e.IdMiseEnAvant);

            modelBuilder.Entity<MiseEnAvant>()
                .Property(e => e.PrixSemaine)
                .HasPrecision(10, 2);

            //-----------------------------Modele-----------------------------
            modelBuilder.Entity<Modele>()
                .HasKey(e => e.IdModele);

            modelBuilder.Entity<Modele>()
                .HasOne(m => m.MarqueModeleNavigation)
                .WithMany(ma => ma.Modeles)
                .HasForeignKey(v => v.IdMarque);

            //-----------------------------ModeleBlender-----------------------------
            modelBuilder.Entity<ModeleBlender>()
                .HasKey(e => e.IdModeleBlender);

            //-----------------------------Motricite-----------------------------
            modelBuilder.Entity<Motricite>()
                .HasKey(e => e.IdMotricite);

            //-----------------------------MoyenPaiement-----------------------------
            modelBuilder.Entity<MoyenPaiement>()
                .HasKey(e => e.IdMoyenPaiement);

            //-----------------------------Notification-----------------------------
            modelBuilder.Entity<Notification>()
                .HasKey(e => e.IdNotification);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.CompteNotificationNav)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.IdCompte);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.AnnonceNotificationNav)
                .WithMany(s => s.Notifications)
                .HasForeignKey(n => n.IdAnnonce);

            //-----------------------------Offre-----------------------------
            modelBuilder.Entity<Offre>()
                .HasKey(e => e.IdOffre);

            modelBuilder.Entity<Offre>()
                .HasOne(o => o.OffreAnnonceNav)
                .WithMany(a => a.Offres)
                .HasForeignKey(o => o.IdAnnonce);

            modelBuilder.Entity<Offre>()
                .HasOne(o => o.OffreMessageNav)
                .WithMany(c => c.Offres)
                .HasForeignKey(o => o.IdMessage);

            //-----------------------------Paiement-----------------------------
            modelBuilder.Entity<Paiement>()
                .HasKey(e => e.IdPaiement);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.PaiementAnnonceNav)
                .WithMany(a => a.Paiements)
                .HasForeignKey(p => p.IdAnnonce);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.PaiementCarteBancaireNav)
                .WithMany(c => c.Paiements)
                .HasForeignKey(p => p.IdCarteBancaire);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.PaiementMiseEnAvantNav)
                .WithMany(m => m.Paiements)
                .HasForeignKey(p => p.IdMiseEnAvant);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.PaiementCommandeNav)
                .WithMany(c => c.Paiements)
                .HasForeignKey(p => p.IdCommande);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.PaiementCompteNav)
                .WithMany(c => c.Paiements)
                .HasForeignKey(p => p.IdCompte);


            //-----------------------------Pays-----------------------------
            modelBuilder.Entity<Pays>()
                .HasKey(e => e.IdPays);
            
            //-----------------------------Piece jointe-----------------------------
            modelBuilder.Entity<PieceJointe>()
                .HasKey(e => e.IdPieceJointe);

            modelBuilder.Entity<PieceJointe>()
                .HasOne(p => p.MessagePjNav)
                .WithMany(m => m.PiecesJointes)
                .HasForeignKey(p => p.IdMessage)
                .OnDelete(DeleteBehavior.Cascade);

            //-----------------------------Plainte-----------------------------
            modelBuilder.Entity<Plainte>()
                .HasKey(e => e.IdPlainte);

            modelBuilder.Entity<Plainte>()
                .HasOne(p => p.SignalementPlainteNav)
                .WithMany(s => s.Plaintes)
                .HasForeignKey(p => p.IdSignalement);

            modelBuilder.Entity<Plainte>()
                .HasOne(p => p.ComptePlainteNav)
                .WithMany(c => c.Plaintes)
                .HasForeignKey(p => p.IdCompte);

            modelBuilder.Entity<Plainte>()
                .HasOne(p => p.EtatSignalementPlaintePlainteNav)
                .WithMany(t => t.Plaintes)
                .HasForeignKey(p => p.IdEtat);
            
            //-----------------------------Refresh Token-----------------------------
            modelBuilder.Entity<RefreshToken>()
                .HasKey(e => e.IdRefreshToken);
            
            modelBuilder.Entity<RefreshToken>()
                .HasOne(p => p.CompteRefreshTokenNav)
                .WithMany(c => c.RefreshTokens)
                .HasForeignKey(p => p.IdCompte);

            //-----------------------------ReinitialisationMotDePasse-----------------------------
            modelBuilder.Entity<TokenEmail>()
                .HasKey(e => e.IdTokenEmail);

            //-----------------------------Signalement-----------------------------
            modelBuilder.Entity<Signalement>()
                 .HasKey(e => e.IdSignalement);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.CompteSignalantNav)
                .WithMany(c => c.SignalementsFaits)
                .HasForeignKey(s => s.IdCompteSignalant);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.CompteSignaleNav)
                .WithMany(c => c.SignalementsRecus)
                .HasForeignKey(s => s.IdCompteSignale)
                .IsRequired(false);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.TypeSignalementSignalementNav)
                .WithMany(t => t.Signalements)
                .HasForeignKey(s => s.IdTypeSignalement);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.EtatSignalementNav)
                .WithMany(e => e.Signalements)
                .HasForeignKey(s => s.IdEtatSignalement);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.AnnonceSignaleNav)
                .WithMany(a => a.SignalementsRecus)
                .HasForeignKey(s => s.IdAnnonceSignale);

            //-----------------------------TypeCompte-----------------------------
            modelBuilder.Entity<TypeCompte>()
                .HasKey(e => e.IdTypeCompte);

            //-----------------------------TypeJournal-----------------------------
            modelBuilder.Entity<TypeJournal>()
                .HasKey(e => e.IdTypeJournaux);

            //-----------------------------TypeSignalement-----------------------------
            modelBuilder.Entity<TypeSignalement>()
                .HasKey(e => e.IdTypeSignalement);

            //-----------------------------Voiture-----------------------------
            modelBuilder.Entity<Voiture>()
                .HasKey(e => e.IdVoiture);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.ModeleVoitureNavigation)
                .WithMany(m => m.Voitures)
                .HasForeignKey(v => v.IdModele);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.MarqueVoitureNavigation)
                .WithMany(m => m.Voitures)
                .HasForeignKey(v => v.IdMarque);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.CategorieVoitureNavigation)
                .WithMany(c => c.Voitures)
                .HasForeignKey(v => v.IdCategorie);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.MotriciteVoitureNavigation)
                .WithMany(m => m.Voitures)
                .HasForeignKey(v => v.IdMotricite);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.CarburantVoitureNavigation)
                .WithMany(c => c.Voitures)
                .HasForeignKey(v => v.IdCarburant);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.BoiteVoitureNavigation)
                .WithMany(b => b.Voitures)
                .HasForeignKey(v => v.IdBoiteDeVitesse);

            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.ModeleBlenderNavigation)
                .WithMany(m => m.Voitures)
                .HasForeignKey(v => v.IdModeleBlender);

            //-----------------------------Vues-----------------------------
            modelBuilder.Entity<Vue>()
                .HasKey(e => new { e.IdCompte, e.IdAnnonce });

            modelBuilder.Entity<Vue>()
                .HasOne(v => v.AnnonceVueNav)
                .WithMany(a => a.Vues)
                .HasForeignKey(v => v.IdAnnonce);

            modelBuilder.Entity<Vue>()
                .HasOne(v => v.CompteVueNav)
                .WithMany(c => c.Vues)
                .HasForeignKey(v => v.IdCompte);

            //-----------------------------Indexes-----------------------------
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

            modelBuilder.Entity<Message>()
                .HasIndex(e => new { e.IdMessage, e.DateEnvoiMessage });

            modelBuilder.Entity<Journal>()
                .HasIndex(e => new { e.IdCompte, e.DateJournal });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}