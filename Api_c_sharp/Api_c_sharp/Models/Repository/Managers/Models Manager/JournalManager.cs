using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class JournalManager : WriteableReadableManager<Journal>, IJournalRepository, IJournalService
    {
        private readonly ILogger<JournalManager> _logger;

        public JournalManager(AutoPulseBdContext context, ILogger<JournalManager> logger) : base(context)
        {
            _logger = logger;
        }

        // Méthode existante
        public async Task<IEnumerable<Journal>> GetJournalByType(int typeID)
        {
            return await dbSet.Where(journal => journal.IdTypeJournal == typeID).OrderBy(j => j.DateJournal).ToListAsync();
        }

        public async Task LogActionAsync(int idCompte, int idTypeJournal, string contenu)
        {
            try
            {
                var journal = new Journal
                {
                    IdCompte = idCompte,
                    IdTypeJournal = idTypeJournal,
                    ContenuJournal = contenu,
                    DateJournal = DateTime.UtcNow
                };

                await AddAsync(journal);

                _logger.LogInformation(
                    "Journal créé - Compte: {IdCompte}, Type: {IdType}, Contenu: {Contenu}",
                    idCompte, idTypeJournal, contenu
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors de la création du journal pour le compte {IdCompte}",
                    idCompte
                );
            }
        }

        public async Task LogConnexionAsync(int idCompte)
        {
            var contenu = "Connexion au compte";
            await LogActionAsync(idCompte, 1, contenu);
        }

        public async Task LogDeconnexionAsync(int idCompte)
        {
            var contenu = "Déconnexion du compte";
            await LogActionAsync(idCompte, 2, contenu);
        }

        public async Task LogCreationCompteAsync(int idCompte, string pseudo)
        {
            var contenu = $"Création du compte avec le pseudo : {pseudo}";
            await LogActionAsync(idCompte, 3, contenu);
        }

        public async Task LogModificationProfilAsync(int idCompte)
        {
            var contenu = "Modification des informations du profil";
            await LogActionAsync(idCompte, 4, contenu);
        }

        public async Task LogPublicationAnnonceAsync(int idCompte, int idAnnonce, string titreAnnonce)
        {
            var contenu = $"Publication de l'annonce #{idAnnonce} : {titreAnnonce}";
            await LogActionAsync(idCompte, 5, contenu);
        }

        public async Task LogModificationAnnonceAsync(int idCompte, int idAnnonce, string titreAnnonce)
        {
            var contenu = $"Modification de l'annonce #{idAnnonce} : {titreAnnonce}";
            await LogActionAsync(idCompte, 6, contenu);
        }

        public async Task LogSuppressionAnnonceAsync(int idCompte, int idAnnonce, string titreAnnonce)
        {
            var contenu = $"Suppression de l'annonce #{idAnnonce} : {titreAnnonce}";
            await LogActionAsync(idCompte, 7, contenu);
        }

        public async Task LogAchatAsync(int idCompte, int idCommande, int idAnnonce, string titreAnnonce)
        {
            var contenu = $"Achat effectué - Commande #{idCommande} pour l'annonce #{idAnnonce} : {titreAnnonce}";
            await LogActionAsync(idCompte, 8, contenu);
        }

        public async Task LogSignalementAsync(int idCompteSignalant, int idCompteSignale, int idSignalement)
        {
            var contenu = $"Signalement effectué (#{idSignalement}) envers le compte #{idCompteSignale}";
            await LogActionAsync(idCompteSignalant, 9, contenu);
        }

        public async Task LogDepotAvisAsync(int idCompteJugeur, int idCompteJuge, int idAvis, int note)
        {
            var contenu = $"Dépôt d'un avis (#{idAvis}) avec la note {note}/5 pour le compte #{idCompteJuge}";
            await LogActionAsync(idCompteJugeur, 10, contenu);
        }

        public async Task LogMiseFavorisAsync(int idCompte, int idAnnonce, string titreAnnonce)
        {
            var contenu = $"Ajout de l'annonce #{idAnnonce} ({titreAnnonce}) aux favoris";
            await LogActionAsync(idCompte, 11, contenu);
        }

        public async Task LogEnvoiMessageAsync(int idCompte, int idConversation, int? idAnnonce = null)
        {
            var contenu = idAnnonce.HasValue
                ? $"Envoi d'un message dans la conversation #{idConversation} concernant l'annonce #{idAnnonce}"
                : $"Envoi d'un message dans la conversation #{idConversation}";
            await LogActionAsync(idCompte, 12, contenu);
        }

        public async Task LogGenerationFactureAsync(int idCompte, int idFacture, int idCommande)
        {
            var contenu = $"Génération de la facture #{idFacture} pour la commande #{idCommande}";
            await LogActionAsync(idCompte, 13, contenu);
        }

        public async Task LogBlocageUtilisateurAsync(int idCompteBloquer, int idCompteBloque)
        {
            var contenu = $"Blocage de l'utilisateur #{idCompteBloque}";
            await LogActionAsync(idCompteBloquer, 14, contenu);
        }
    }
}