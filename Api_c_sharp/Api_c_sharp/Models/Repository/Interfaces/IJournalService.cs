using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Managers.Models_Manager;

namespace Api_c_sharp.Models.Repository.Interfaces
{
    /// <summary>
    /// Service de journalisation automatique des actions utilisateurs
    /// </summary>
    public interface IJournalService
    {
        Task LogActionAsync(int idCompte, int idTypeJournal, string contenu);

        Task LogConnexionAsync(int idCompte);

        Task LogDeconnexionAsync(int idCompte);

        Task LogCreationCompteAsync(int idCompte, string pseudo);

        Task LogModificationProfilAsync(int idCompte);

        Task LogPublicationAnnonceAsync(int idCompte, int idAnnonce, string titreAnnonce);

        Task LogModificationAnnonceAsync(int idCompte, int idAnnonce, string titreAnnonce);

        Task LogSuppressionAnnonceAsync(int idCompte, int idAnnonce, string titreAnnonce);

        Task LogAchatAsync(int idCompteAcheteur,int idCompteVendeur, int idCommande, int idAnnonce, int idMoyenPaiement);

        Task LogSignalementAsync(int idCompteSignalant, int idCompteSignale, int idSignalement,int idTypeSignalement, string description);

        Task LogDepotAvisAsync(int idCompteJugeur, int idCompteJuge, int idAvis, int note, string description);

        Task LogMiseFavorisAsync(int idCompte, int idAnnonce);

        Task LogEnvoiMessageAsync(int idCompte, int idConversation, string messagecontenu, int? idAnnonce = null);

        Task LogGenerationFactureAsync(int idCompte, int idFacture, int idCommande);

        Task LogBlocageUtilisateurAsync(int idCompteBloquer, int idCompteBloque);

        Task<IEnumerable<Journal>> GetJournalByType(int typeID);
    }
}
