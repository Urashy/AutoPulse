namespace AutoPulse.Shared.DTO;
public class SignalementDTO
{
    public int IdSignalement { get; set; }
    public string? DescriptionSignalement { get; set; }
    public DateTime DateCreationSignalement { get; set; }

    // Informations sur le signalant
    public int IdCompteSignalant { get; set; }
    public string? PseudoSignalant { get; set; }

    // Informations sur la cible (peut être un compte OU une annonce)
    public int? IdCompteSignale { get; set; }
    public string? PseudoSignale { get; set; }

    public int? IdAnnonceSignale { get; set; }
    public string? LibelleAnnonceSignale { get; set; }

    // Type et état
    public int IdTypeSignalement { get; set; }
    public string? LibelleTypeSignalement { get; set; }

    public int IdEtatSignalement { get; set; }
    public string? LibelleEtatSignalement { get; set; }

    // Propriété helper pour savoir le type de signalement
    public string TypeCible => IdAnnonceSignale.HasValue ? "Annonce" : "Compte";
}