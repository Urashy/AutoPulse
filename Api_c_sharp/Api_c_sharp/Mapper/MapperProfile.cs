using AutoPulse.Shared.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Mapper;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        //---------------------------------Adresse---------------------------------

        CreateMap<Adresse, AdresseDTO>()
            .ForMember(dest => dest.LibellePays,
                opt => opt.MapFrom(src => src.PaysAdresseNav.Libelle))
            .ReverseMap();

        CreateMap<Adresse, AdresseCreateDTO>()
            .ReverseMap();

        CreateMap<Adresse, AdresseUpdateDTO>()
            .ReverseMap();

        //---------------------------------Annonce---------------------------------

        CreateMap<Annonce, AnnonceDTO>()
            .ForMember(dest => dest.PseudoVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.Pseudo))
            .ForMember(dest => dest.LibelleEtatAnnonce,
                opt => opt.MapFrom(src => src.EtatAnnonceNavigation.LibelleEtatAnnonce))
            .ForMember(dest => dest.Marque,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.MarqueVoitureNavigation.LibelleMarque))
            .ForMember(dest => dest.Modele,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.ModeleVoitureNavigation.LibelleModele))
            .ForMember(dest => dest.Annee,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Annee))
            .ForMember(dest => dest.Kilometrage,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Kilometrage))
            .ForMember(dest => dest.Carburant,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.CarburantVoitureNavigation.LibelleCarburant))
            .ForMember(dest => dest.Ville,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.LibelleVille))
            .ForMember(dest => dest.CodePostal,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.CodePostal))
            .ForMember(dest => dest.IdMiseEnAvant,
                opt => opt.MapFrom(src => src.IdMiseEnAvant)).ReverseMap();

        CreateMap<Annonce, AnnonceDetailDTO>()
            .ForMember(dest => dest.LibelleEtatAnnonce,
                opt => opt.MapFrom(src => src.EtatAnnonceNavigation.LibelleEtatAnnonce))
            .ForMember(dest => dest.EstMiseEnAvant,
                opt => opt.MapFrom(src => src.IdMiseEnAvant > 1))
            .ForMember(dest => dest.LibelleMiseEnAvant,
                opt => opt.MapFrom(src => src.MiseEnAvantAnnonceNav != null ? src.MiseEnAvantAnnonceNav.LibelleMiseEnAvant : null))
            .ForMember(dest => dest.Prix
            , // <--- AJOUTEZ CETTE LIGNE
                opt => opt.MapFrom(src => src.Prix))
            .ForMember(dest => dest.IdVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.IdCompte))
            .ForMember(dest => dest.PseudoVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.Pseudo))
            .ForMember(dest => dest.NomVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.Nom))
            .ForMember(dest => dest.PrenomVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.Prenom))
            .ForMember(dest => dest.BiographieVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.Biographie))
            .ForMember(dest => dest.DateInscriptionVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.DateCreation))
            .ForMember(dest => dest.TypeCompteVendeur,
                opt => opt.MapFrom(src => src.CompteAnnonceNav.TypeCompteCompteNav.Libelle))
            // Adresse
            .ForMember(dest => dest.NumeroRue,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.Numero.ToString()))
            .ForMember(dest => dest.Rue,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.Rue))
            .ForMember(dest => dest.Ville,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.LibelleVille))
            .ForMember(dest => dest.CodePostal,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.CodePostal))
            .ForMember(dest => dest.Pays,
                opt => opt.MapFrom(src => src.AdresseAnnonceNav.PaysAdresseNav.Libelle))
            // Voiture
            .ForMember(dest => dest.IdVoiture,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.IdVoiture))
            .ForMember(dest => dest.Marque,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.MarqueVoitureNavigation.LibelleMarque))
            .ForMember(dest => dest.Modele,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.ModeleVoitureNavigation.LibelleModele))

            .ForMember(dest => dest.Categorie,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.CategorieVoitureNavigation.LibelleCategorie))
            .ForMember(dest => dest.Couleur,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.APourCouleurs.FirstOrDefault().APourCouleurCouleurNav.LibelleCouleur ?? "Non spécifié"))
            .ForMember(dest => dest.Carburant,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.CarburantVoitureNavigation.LibelleCarburant))
            .ForMember(dest => dest.BoiteDeVitesse,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.BoiteVoitureNavigation.LibelleBoite))
            .ForMember(dest => dest.Motricite,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.MotriciteVoitureNavigation.LibelleMotricite))
            .ForMember(dest => dest.Kilometrage,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Kilometrage))
            .ForMember(dest => dest.Annee,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Annee))
            .ForMember(dest => dest.Puissance,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Puissance))
            .ForMember(dest => dest.Couple,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Couple))
            .ForMember(dest => dest.NbCylindres,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.NbCylindres))
            .ForMember(dest => dest.NbAirbag,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.NbAirbag))
            .ForMember(dest => dest.MiseEnCirculation,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.MiseEnCirculation))
            .ForMember(dest => dest.NbPlaces,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.NbPlace))
            .ForMember(dest => dest.NbPortes,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.NbPorte))
            .ForMember(dest => dest.InterieurCuire,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.InterieurCuire))
            .ForMember(dest => dest.CylindrerMoteur,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.CylindrerMoteur))
            .ForMember(dest => dest.PositionVolant,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.PositionVolant))
            .ForMember(dest => dest.NbVues,
                opt => opt.MapFrom(src => src.Vues.Count()))
            .ForMember(dest => dest.NbFavoris,
                opt => opt.MapFrom(src => src.Favoris.Count()))
            .ForMember(dest => dest.LienModeleBlender,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.ModeleBlenderNavigation != null
                    ? src.VoitureAnnonceNav.ModeleBlenderNavigation.Lien
                    : null)).ReverseMap();

        CreateMap<AnnonceCreateDTO, Annonce>().ReverseMap();
        CreateMap<AnnonceUpdateDTO, Annonce>().ReverseMap();

        //---------------------------------APourConversation---------------------------------

        CreateMap<APourConversation, APourConversationDTO>().ReverseMap();

        //---------------------------------APourCouleur---------------------------------

        CreateMap<APourCouleur, APourCouleurDTO>().ReverseMap();

        //---------------------------------Avis---------------------------------

        CreateMap<Avis, AvisListDTO>()
            .ForMember(dest => dest.PseudoJugeur,
                opt => opt.MapFrom(src => src.CompteJugeurNav.Pseudo)).ReverseMap();

        CreateMap<Avis, AvisDetailDTO>()
            .ForMember(dest => dest.PseudoJugee,
                opt => opt.MapFrom(src => src.CompteJugeeNav.Pseudo))
            .ForMember(dest => dest.PseudoJugeur,
                opt => opt.MapFrom(src => src.CompteJugeurNav.Pseudo)).ReverseMap();

        CreateMap<AvisCreateDTO, Avis>()
            .ForMember(dest => dest.DateAvis,
                opt => opt.MapFrom(src => DateTime.UtcNow)).ReverseMap();

        CreateMap<AvisUpdateDTO, Avis>()
            .ForMember(dest => dest.DateAvis,
                opt => opt.MapFrom(src => DateTime.UtcNow)).ReverseMap();

        //---------------------------------Bloque---------------------------------

        CreateMap<Bloque, BloqueDTO>().ReverseMap();

        //---------------------------------BoiteDeVitesse---------------------------------

        CreateMap<BoiteDeVitesse, BoiteDeVitesseDTO>().ReverseMap();

        //---------------------------------Carburant---------------------------------

        CreateMap<Carburant, CarburantDTO>().ReverseMap();

        //---------------------------------CarteBancaire---------------------------------

        CreateMap<CarteBancaire, CarteBancaireDTO>()
            .ForMember(dest => dest.DateExpiration, opt => opt.MapFrom(src => $"{src.DateExpiration.Month.ToString()}/{src.DateExpiration.Year.ToString().Substring(2, 2)}"))
            .ReverseMap();

        CreateMap<CarteBancaireCreateDTO, CarteBancaire>()
            .ForMember(dest => dest.DateExpiration,
                opt => opt.MapFrom(src => DateTime.SpecifyKind(src.DateExpiration, DateTimeKind.Local).ToUniversalTime()))
            .ReverseMap();

        CreateMap<CarteBancaireUpdateDTO, CarteBancaire>().ReverseMap();

        //---------------------------------Categorie---------------------------------

        CreateMap<Categorie, CategorieDTO>().ReverseMap();

        //---------------------------------Commande---------------------------------

        CreateMap<Commande, CommandeDTO>()
            .ForMember(dest => dest.PseudoVendeur,
                opt => opt.MapFrom(src => src.CommandeAnnonceNav.CompteAnnonceNav.Pseudo))
            .ForMember(dest => dest.PseudoAcheteur,
                opt => opt.MapFrom(src => src.AcheteurCommande.Pseudo))
            .ForMember(dest => dest.LibelleAnnonce,
                opt => opt.MapFrom(src => src.CommandeAnnonceNav.Libelle))
            .ForMember(dest => dest.MoyenPaiement,
                opt => opt.MapFrom(src => src.CommandeMoyenPaiementNav.TypePaiement));

        CreateMap<Commande, CommandeDetailDTO>()
            .ForMember(dest => dest.MoyenPaiement,
                opt => opt.MapFrom(src => src.CommandeMoyenPaiementNav.TypePaiement))
            .ForMember(dest => dest.PseudoVendeur,
                opt => opt.MapFrom(src => src.CommandeAnnonceNav.CompteAnnonceNav.Pseudo))
            .ForMember(dest => dest.PseudoAcheteur,
                opt => opt.MapFrom(src => src.AcheteurCommande.Pseudo))
            .ForMember(dest => dest.Annonce,
                opt => opt.MapFrom(src => src.CommandeAnnonceNav)).ReverseMap();

        CreateMap<CommandeCreateDTO, Commande>().ReverseMap();
        CreateMap<CommandeUpdateDTO, Commande>().ReverseMap();

        //---------------------------------Compte---------------------------------

        CreateMap<Compte, CompteGetDTO>()
            .ForMember(dest => dest.TypeCompte,
                 opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
            .ForMember(dest => dest.DateInscription,
                opt => opt.MapFrom(src => src.DateCreation))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.Email)).ReverseMap();

        CreateMap<Compte, CompteDetailDTO>()
            .ForMember(dest => dest.TypeCompte,
                opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
            .ForMember(dest => dest.Adresses,
                   opt => opt.MapFrom(src => src.Adresses))
            .ForMember(dest => dest.TypeCompte,
                 opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
            .ForMember(
                opt => opt.idImage,
                cfg => cfg.MapFrom(src => src.Images.FirstOrDefault().IdImage))
            .ForMember(dest => dest.EstSuspendu,
                opt => opt.MapFrom(src => src.IdEtatCompte == 2))
            .ReverseMap();

        CreateMap<Compte, CompteProfilPublicDTO>()
            .ForMember(dest => dest.DateInscription,
                opt => opt.MapFrom(src => src.DateCreation))
            .ForMember(dest => dest.ImageProfil,
                opt => opt.MapFrom(src => src.Images.Any()
                    ? Convert.ToBase64String(src.Images.First().Fichier)
                    : null))
            .ForMember(dest => dest.NombreAnnonces,
                opt => opt.MapFrom(src => src.Annonces.Count))
            .ForMember(dest => dest.NoteMoyenne,
                opt => opt.MapFrom(src => src.AvisJugees.Any()
                    ? src.AvisJugees.Average(a => a.NoteAvis)
                    : 0))
            .ForMember(dest => dest.NombreAvis,
                opt => opt.MapFrom(src => src.AvisJugees.Count)).ReverseMap();

        CreateMap<CompteCreateDTO, Compte>()
            .ForMember(dest => dest.IdEtatCompte,
                opt => opt.MapFrom(src => src.EstSuspendu ? 2 : 1));

        CreateMap<Compte, CompteCreateDTO>();



        CreateMap<CompteUpdateDTO, Compte>()
            .ForMember(dest => dest.IdEtatCompte,
                opt => opt.MapFrom(src => src.EstSuspendu ? 2 : 1));

        //---------------------------------Conversation---------------------------------

        CreateMap<ConversationCreateDTO, Conversation>()
            .ForMember(dest => dest.DateDernierMessage, opt => opt.MapFrom(src => DateTime.SpecifyKind(src.DateDernierMessage,DateTimeKind.Local).ToUniversalTime()))
            .ReverseMap();

        CreateMap<Conversation, ConversationUpdateDTO>()
            .ReverseMap();

        CreateMap<Conversation, ConversationListDTO>()
            .ForMember(dest => dest.LibelleAnnonce,
                opt => opt.MapFrom(src => src.AnnonceConversationNav.Libelle))
            .ForMember(dest => dest.DernierMessage,
                opt => opt.MapFrom(src => src.Messages.OrderByDescending(m => m.DateEnvoiMessage).FirstOrDefault().ContenuMessage))
            .ForMember(dest => dest.DateDernierMessage,
                opt => opt.MapFrom(src => src.Messages.OrderByDescending(m => m.DateEnvoiMessage).FirstOrDefault().DateEnvoiMessage))
            .ForMember(dest => dest.ParticipantPseudo,
                opt => opt.MapFrom(src => src.ApourConversations.Select(a => a.APourConversationCompteNav.Pseudo).ToList())).ReverseMap();

        CreateMap<Conversation, ConversationDetailDTO>()
            .ForMember(dest => dest.LibelleAnnonce,
                opt => opt.MapFrom(src => src.AnnonceConversationNav.Libelle))
            .ForMember(dest => dest.Messages,
                opt => opt.MapFrom(src => src.Messages))
            .ForMember(dest => dest.Participants,
                opt => opt.MapFrom(src => src.ApourConversations.Select(a => a.APourConversationCompteNav))).ReverseMap();

        //---------------------------------Couleur---------------------------------

        CreateMap<Couleur, CouleurDTO>().ReverseMap();

        //---------------------------------EtatAnnonce---------------------------------

        CreateMap<EtatAnnonce, EtatAnnonceDTO>().ReverseMap();

        //---------------------------------EtatCompte---------------------------------

        CreateMap<EtatCompte, EtatCompteDTO>().ReverseMap();

        //---------------------------------EtatSignalement---------------------------------

        CreateMap<EtatSignalementPlainte, EtatSignalementDTO>().ReverseMap();

        //---------------------------------Facture---------------------------------

        CreateMap<Facture, FactureDTO>().ReverseMap();

        //---------------------------------Favori---------------------------------

        CreateMap<Favori, FavoriDTO>().ReverseMap();

        //---------------------------------Image---------------------------------

        CreateMap<Image, ImageDTO>().ReverseMap();

        CreateMap<Image, ImageUploadDTO>().ReverseMap();

        //---------------------------------Journal---------------------------------

        CreateMap<Journal, JournalDTO>().ReverseMap();
        CreateMap<Journal, JournalCreateDTO>().ReverseMap();
        CreateMap<Journal, JournalUpdateDTO>().ReverseMap();

        //---------------------------------Marque---------------------------------

        CreateMap<Marque, MarqueDTO>().ReverseMap();

        //---------------------------------Message---------------------------------

        
        CreateMap<Message, MessageDTO>()
            .ForMember(dest => dest.PseudoCompte,
        opt => opt.MapFrom(src => src.MessageCompteNav.Pseudo))
            .ForMember(dest => dest.PiecesJointes, opt => opt.MapFrom(src =>
        src.PiecesJointes.Select(pj => new PieceJointeDTO
        {
            IdPieceJointe = pj.IdPieceJointe,
            IdMessage = pj.IdMessage,
            NomFichier = pj.NomFichier,
            TypeMime = pj.TypeMime,
            Extension = pj.Extension,
            TailleFichier = pj.TailleFichier,
            ContenuBase64 = Convert.ToBase64String(pj.Contenu)
        })))
            .ForMember(dest => dest.Offres, opt => opt.MapFrom(src => src.Offres));

        CreateMap<MessageCreateDTO, Message>().ReverseMap();
        CreateMap<MessageUpdateDTO, Message>().ReverseMap();

        //---------------------------------MiseEnAvant---------------------------------

        CreateMap<MiseEnAvant, MiseEnAvantDTO>().ReverseMap();

        //---------------------------------Modele---------------------------------

        CreateMap<Modele, ModeleDTO>().ReverseMap();

        //---------------------------------ModeleBlender---------------------------------

        CreateMap<ModeleBlender, ModeleBlenderDTO>().ReverseMap();

        //---------------------------------Motricite---------------------------------

        CreateMap<Motricite, MotriciteDTO>().ReverseMap();

        //---------------------------------MoyenPaiement---------------------------------

        CreateMap<MoyenPaiement, MoyenPaiementDTO>().ReverseMap();

        //---------------------------------Notification---------------------------------
        CreateMap<Notification, NotificationDTO>()
            .ForMember(dest => dest.IdAnnonce, opt => opt.MapFrom(src => src.AnnonceNotificationNav.IdAnnonce))
            .ForMember(dest => dest.LibelleAnnonce, opt => opt.MapFrom(src => src.AnnonceNotificationNav.Libelle))
            .ForMember(dest => dest.Reduction, opt => opt.MapFrom(src => src.NouveauPrix - src.AncienPrix))
            .ReverseMap();

        CreateMap<NotificationCreateDTO, Notification>()
            .ForMember(dest => dest.DateCreation, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EstLue, opt => opt.MapFrom(src => false))
            .ReverseMap();

        CreateMap<Notification, NotificationMarkReadDTO>()
            .ReverseMap();
        
        CreateMap<IEnumerable<Notification>, NotificationStatsDTO>()
            .ForMember(dest => dest.TotalNonLues,
                opt => opt.MapFrom(src => src.Count(n => !n.EstLue)))
            .ForMember(dest => dest.TotalPriceDrops,
                opt => opt.MapFrom(src => src.Count(n => n.Type == "pricedrop")))
            .ForMember(dest => dest.TotalMessages,
                opt => opt.MapFrom(src => src.Count()))
            .ReverseMap();
        
        CreateMap<Notification, NotificationUpdateDTO>()
            .ReverseMap();

        //---------------------------------Offre---------------------------------
        CreateMap<Offre, OffreDTO>()
            .ForMember(dest => dest.IdAnnonce, opt => opt.MapFrom(src => src.IdAnnonce))
        .ReverseMap();

        CreateMap<OffreUpdateDTO, Offre>()
            .ForMember(dest => dest.OffreAnnonceNav, opt => opt.Ignore())
            .ForMember(dest => dest.OffreMessageNav, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<OffreCreateDTO, Offre>()
            .ForMember(dest => dest.IdOffre, opt => opt.Ignore())
            .ForMember(dest => dest.DateOffre, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.EstAccepte, opt => opt.Ignore());


        //---------------------------------Pays---------------------------------

        CreateMap<Pays, PaysDTO>().ReverseMap();

        //---------------------------------PieceJointe---------------------------------

        CreateMap<PieceJointeDTO, PieceJointe>()
            .ForMember(dest => dest.Contenu, opt => opt.MapFrom(src => Convert.FromBase64String(src.ContenuBase64))).ReverseMap();

        CreateMap<PieceJointeUploadDTO, PieceJointe>()
            .ForMember(dest => dest.DateUpload, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Contenu, opt => opt.MapFrom(src => Convert.FromBase64String(src.ContenuBase64)))
            .ForMember(dest => dest.MessagePjNav, opt => opt.Ignore());

        CreateMap<PieceJointeCreateDTO, PieceJointe>()
            .ForMember(dest => dest.DateUpload, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Contenu, opt => opt.MapFrom(src => Convert.FromBase64String(src.ContenuBase64)))
            .ForMember(dest => dest.MessagePjNav, opt => opt.Ignore());

        //---------------------------------Plainte---------------------------------

        CreateMap<Plainte, PlainteDTO>()
            .ReverseMap();

        CreateMap<PlainteCreateDTO, Plainte>()
            .ForMember(dest => dest.DateCreation, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<PlainteUpdateDTO, Plainte>().ReverseMap();

        //---------------------------------ReinitialisationMotDePasse---------------------------------

        CreateMap<TokenEmail, TokenEmailDTO>().ReverseMap();
        CreateMap<TokenEmail, TokenEmailCreateDTO>().ReverseMap();

        //---------------------------------Signalement---------------------------------

        CreateMap<Signalement, SignalementDTO>()
            .ForMember(dest => dest.PseudoSignalant,
                opt => opt.MapFrom(src => src.CompteSignalantNav.Pseudo))
            .ForMember(dest => dest.PseudoSignale,
                opt => opt.MapFrom(src => src.CompteSignaleNav != null ? src.CompteSignaleNav.Pseudo : null))
            .ForMember(dest => dest.LibelleAnnonceSignale,
                opt => opt.MapFrom(src => src.AnnonceSignaleNav != null ? src.AnnonceSignaleNav.Libelle : null))
            .ForMember(dest => dest.LibelleTypeSignalement,
                opt => opt.MapFrom(src => src.TypeSignalementSignalementNav.LibelleTypeSignalement))
            .ForMember(dest => dest.LibelleEtatSignalement,
                opt => opt.MapFrom(src => src.EtatSignalementNav.LibelleEtatSignalement))
            .ForMember(dest => dest.PseudoSignalant,
                opt => opt.MapFrom(src => src.CompteSignalantNav.Pseudo))
            .ForMember(dest => dest.IdCompteSignale,
                opt => opt.MapFrom(src => src.IdCompteSignale))
            .ReverseMap();

        CreateMap<SignalementCreateDTO, Signalement>()
            .ForMember(dest => dest.DateCreationSignalement,
                opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<SignalementUpdateDTO, Signalement>()
            .ForMember(dest => dest.DateCreationSignalement,
                opt => opt.MapFrom(src => DateTime.UtcNow))
           .ReverseMap();

        //---------------------------------TypeCompte---------------------------------

        CreateMap<TypeCompte, TypeCompteDTO>().ReverseMap();

        //---------------------------------TypeJournal

        CreateMap<TypeJournal, TypeJournalDTO>().ReverseMap();

        //---------------------------------TypeSignalement---------------------------------

        CreateMap<TypeSignalement, TypeSignalementDTO>().ReverseMap();

        //---------------------------------Voiture---------------------------------

        CreateMap<Voiture, VoitureCreateDTO>().ReverseMap();
        CreateMap<Voiture, VoitureUpdateDTO>().ReverseMap();

        CreateMap<Voiture, VoitureDTO>()
            .ForMember(dest => dest.Marque,
                opt => opt.MapFrom(src => src.MarqueVoitureNavigation.LibelleMarque))
            .ForMember(dest => dest.Modele,
                opt => opt.MapFrom(src => "N/A")) // À mapper avec la vraie relation Modele si disponible
            .ForMember(dest => dest.Carburant,
                opt => opt.MapFrom(src => src.CarburantVoitureNavigation.LibelleCarburant))
            .ForMember(dest => dest.LibelleCouleur,
                opt => opt.MapFrom(src => src.APourCouleurs.FirstOrDefault().APourCouleurCouleurNav.LibelleCouleur ?? "Non spécifié")).ReverseMap()
            .ReverseMap();

        CreateMap<Voiture, VoitureDetailDTO>()
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images.Select(i => Convert.ToBase64String(i.Fichier)).ToList())).ReverseMap();

        //---------------------------------Vue---------------------------------

        CreateMap<Vue, VueDTO>().ReverseMap();

    }
}