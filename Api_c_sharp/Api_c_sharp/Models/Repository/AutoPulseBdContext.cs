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
        public DbSet<Bloque> Bloques { get; set; }
        public DbSet<BoiteDeVitesse> BoitesDeVitesses { get; set; }
        public DbSet<Carburant> Carburants { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Couleur> Couleurs { get; set; }
        public DbSet<EtatAnnonce> EtatAnnonces { get; set; }
        public DbSet<EtatSignalement> EtatSignalements { get; set; }
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
        public DbSet<ReinitialisationMotDePasse> ReinitialisationMotDePasses { get; set; }

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
            
            modelBuilder.Entity<ReinitialisationMotDePasse>()
                .HasKey(r => r.IdReinitialisationMdp); 

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

            //-----------------------------Bloque-----------------------------
            modelBuilder.Entity<Bloque>()
                .HasKey(e => new { e.IdBloque, e.IdBloquant });

            //-----------------------------BoiteDeVitesse-----------------------------
            modelBuilder.Entity<BoiteDeVitesse>()
                .HasKey(e => e.IdBoiteDeVitesse);

            //-----------------------------Carburant-----------------------------
            modelBuilder.Entity<Carburant>()
                .HasKey(e => e.IdCarburant);

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

            //-----------------------------Compte-----------------------------
            modelBuilder.Entity<Compte>()
                .HasKey(e => e.IdCompte);

            modelBuilder.Entity<Compte>()
                .HasOne(c => c.TypeCompteCompteNav)
                .WithMany(t => t.Comptes)
                .HasForeignKey(c => c.IdTypeCompte);


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

            //-----------------------------EtatSignalement-----------------------------
            modelBuilder.Entity<EtatSignalement>()
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
                .HasForeignKey(i => i.IdVoiture);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.CompteImageNav)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.IdCompte);

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

            //-----------------------------Pays-----------------------------
            modelBuilder.Entity<Pays>()
                .HasKey(e => e.IdPays);

            //-----------------------------ReinitialisationMotDePasse-----------------------------
            modelBuilder.Entity<ReinitialisationMotDePasse>()
                .HasKey(e => e.IdReinitialisationMdp);

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
                .HasForeignKey(s => s.IdCompteSignale);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.TypeSignalementSignalementNav)
                .WithMany(t => t.Signalements)
                .HasForeignKey(s => s.IdTypeSignalement);

            modelBuilder.Entity<Signalement>()
                .HasOne(s => s.EtatSignalementNav)
                .WithMany(e => e.Signalements)
                .HasForeignKey(s => s.IdEtatSignalement);

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