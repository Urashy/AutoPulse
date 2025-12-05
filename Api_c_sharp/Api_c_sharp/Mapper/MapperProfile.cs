using AutoPulse.Shared.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Mapper;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // ============================================
        // MAPPERS SIMPLES (Reference Data)
        // ============================================
        
        CreateMap<Marque, MarqueDTO>().ReverseMap();
        CreateMap<Bloque, BloqueDTO>().ReverseMap();

        CreateMap<Modele, ModeleDTO>().ReverseMap();
        CreateMap<APourConversation, APourConversationDTO>().ReverseMap();
        CreateMap<APourCouleur, APourCouleurDTO>().ReverseMap();

        CreateMap<Carburant, CarburantDTO>().ReverseMap();
        CreateMap<BoiteDeVitesse, BoiteDeVitesseDTO>().ReverseMap();
        CreateMap<Categorie, CategorieDTO>().ReverseMap();
        CreateMap<Couleur, CouleurDTO>().ReverseMap();
        CreateMap<EtatAnnonce, EtatAnnonceDTO>().ReverseMap();
        CreateMap<TypeCompte, TypeCompteDTO>().ReverseMap();
        CreateMap<MoyenPaiement, MoyenPaiementDTO>().ReverseMap();
        CreateMap<Motricite, MotriciteDTO>().ReverseMap();
        CreateMap<MiseEnAvant, MiseEnAvantDTO>().ReverseMap();
        CreateMap<TypeJournal, TypeJournalDTO>().ReverseMap();

        CreateMap<TypeSignalement, TypeSignalementDTO>()
            .ReverseMap();

        CreateMap<EtatSignalement, EtatSignalementDTO>()
            .ReverseMap();

        CreateMap<Journal, JournalDTO>()
            .ReverseMap();

        CreateMap<Facture, FactureDTO>()
            .ReverseMap();

        CreateMap<Conversation, ConversationCreateDTO>()
            .ReverseMap();

        CreateMap<ModeleBlender, ModeleBlenderDTO>()
            .ReverseMap();

        // ============================================
        // MAPPERS ADRESSE
        // ============================================

        CreateMap<Adresse, AdresseDTO>()
            .ReverseMap();
  




        // ============================================
        // MAPPERS PAYS
        // ============================================

        CreateMap<Pays, PaysDTO>().ReverseMap();
        
        // ============================================
        // MAPPERS VOITURE
        // ============================================
        
        CreateMap<Voiture, VoitureCreateDTO>().ReverseMap();
        
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
            .ForMember(dest => dest.LibelleMarque, 
                opt => opt.MapFrom(src => src.MarqueVoitureNavigation.LibelleMarque))
            .ForMember(dest => dest.LibelleModele, 
                opt => opt.MapFrom(src => "N/A")) // À mapper avec la vraie relation
            .ForMember(dest => dest.LibelleMotricite, 
                opt => opt.MapFrom(src => src.MotriciteVoitureNavigation.LibelleMotricite))
            .ForMember(dest => dest.LibelleCarburant, 
                opt => opt.MapFrom(src => src.CarburantVoitureNavigation.LibelleCarburant))
            .ForMember(dest => dest.LibelleBoite, 
                opt => opt.MapFrom(src => src.BoiteVoitureNavigation.LibelleBoite))
            .ForMember(dest => dest.LibelleCouleur, 
                opt => opt.MapFrom(src => src.APourCouleurs.FirstOrDefault().APourCouleurCouleurNav.LibelleCouleur ?? "Non spécifié"))
            .ForMember(dest => dest.LibelleCategorie, 
                opt => opt.MapFrom(src => src.CategorieVoitureNavigation.LibelleCategorie))
            .ForMember(dest => dest.LienModeleBlender, 
                opt => opt.MapFrom(src => src.ModeleBlenderNavigation != null ? src.ModeleBlenderNavigation.Lien : null))
            .ForMember(dest => dest.Images, 
                opt => opt.MapFrom(src => src.Images.Select(i => Convert.ToBase64String(i.Fichier)).ToList())).ReverseMap();
        
        // ============================================
        // MAPPERS ANNONCE
        // ============================================

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
            .ForMember(dest => dest.ImagePrincipale,
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Images.Any()
                    ? Convert.ToBase64String(src.VoitureAnnonceNav.Images.First().Fichier)
                    : null))
            .ForMember(dest => dest.IdMiseEnAvant,
                opt => opt.MapFrom(src => src.IdMiseEnAvant)).ReverseMap();
        
        CreateMap<Annonce, AnnonceDetailDTO>()
            .ForMember(dest => dest.LibelleEtatAnnonce, 
                opt => opt.MapFrom(src => src.EtatAnnonceNavigation.LibelleEtatAnnonce))
            .ForMember(dest => dest.EstMiseEnAvant, 
                opt => opt.MapFrom(src => src.IdMiseEnAvant.HasValue))
            .ForMember(dest => dest.LibelleMiseEnAvant, 
                opt => opt.MapFrom(src => src.MiseEnAvantAnnonceNav != null ? src.MiseEnAvantAnnonceNav.LibelleMiseEnAvant : null))
            .ForMember(dest => dest.Prix
            , // <--- AJOUTEZ CETTE LIGNE
                opt => opt.MapFrom(src => src.Prix))
            // Vendeur
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
            .ForMember(dest => dest.MiseEnCirculation, 
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.MiseEnCirculation))
            .ForMember(dest => dest.NbPlaces, 
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.NbPlace)) 
            .ForMember(dest => dest.NbPortes, 
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.NbPorte))
            .ForMember(dest => dest.Images, 
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.Images.Select(i => Convert.ToBase64String(i.Fichier)).ToList()))
            .ForMember(dest => dest.LienModeleBlender, 
                opt => opt.MapFrom(src => src.VoitureAnnonceNav.ModeleBlenderNavigation != null 
                    ? src.VoitureAnnonceNav.ModeleBlenderNavigation.Lien 
                    : null)).ReverseMap();
        
        CreateMap<AnnonceCreateUpdateDTO, Annonce>().ReverseMap();
        
        // ============================================
        // MAPPERS COMPTE
        // ============================================
        
        CreateMap<Compte, CompteGetDTO>()
            .ForMember(dest => dest.TypeCompte, 
                opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
            .ForMember(dest => dest.DateInscription, 
                opt => opt.MapFrom(src => src.DateCreation)).ReverseMap();
        
        CreateMap<Compte, CompteDetailDTO>()
            .ForMember(dest => dest.TypeCompte, 
                opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
            .ForMember(dest => dest.Adresses, 
                opt => opt.MapFrom(src => src.Adresses.Select(a => a.CompteAdresseNav)))
            .ForMember(dest => dest.TypeCompte,
                 opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
            .ForMember(
                opt => opt.idImage,
                cfg => cfg.MapFrom(src => src.Images.FirstOrDefault().IdImage))
            .ReverseMap();
        
        CreateMap<Compte, CompteProfilPublicDTO>()
            .ForMember(dest => dest.DateInscription, 
                opt => opt.MapFrom(src => src.DateCreation))
            .ForMember(dest => dest.TypeCompte, 
                opt => opt.MapFrom(src => src.TypeCompteCompteNav.Libelle))
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
            .ReverseMap();
        
        CreateMap<CompteUpdateDTO, Compte>()
            .ReverseMap();
        
        // ============================================
        // MAPPERS AVIS
        // ============================================
        
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

        // ============================================
        // MAPPERS COMMANDE
        // ============================================

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

        CreateMap<CommandeCreateDTO, Commande>();

        // ============================================
        // MAPPERS FAVORI
        // ============================================

        CreateMap<Favori, FavoriDTO>().ReverseMap();
        
        // ============================================
        // MAPPERS CONVERSATION
        // ============================================
        
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
        
        CreateMap<Message, MessageDTO>()
            .ForMember(dest => dest.PseudoCompte, 
                opt => opt.MapFrom(src => src.MessageCompteNav.Pseudo)).ReverseMap();
        
        CreateMap<MessageCreateDTO, Message>().ReverseMap();
        
        // ============================================
        // MAPPERS SIGNALEMENT
        // ============================================
        
        CreateMap<Signalement, SignalementDTO>()
            .ForMember(dest => dest.PseudoSignalant, 
                opt => opt.MapFrom(src => src.CompteSignalantNav.Pseudo))
            .ForMember(dest => dest.PseudoSignale, 
                opt => opt.MapFrom(src => src.CompteSignaleNav.Pseudo))
            .ForMember(dest => dest.LibelleTypeSignalement, 
                opt => opt.MapFrom(src => src.TypeSignalementSignalementNav.LibelleTypeSignalement)).ReverseMap();
        
        CreateMap<SignalementCreateDTO, Signalement>()
            .ForMember(dest => dest.DateCreationSignalement, 
                opt => opt.MapFrom(src => DateTime.Now)).ReverseMap();

        // ============================================
        // MAPPERS IMAGE
        // ============================================
        CreateMap<Image, ImageDTO>()
            .ReverseMap();
        
        CreateMap<Image, ImageUploadDTO>()
            .ReverseMap();
        
        // ============================================
        // MAPPERS REINITIALISATION MDP
        // ============================================
        CreateMap<ReinitialisationMotDePasse, ReinitialiseMdpDTO>()
            .ReverseMap();
    }
}