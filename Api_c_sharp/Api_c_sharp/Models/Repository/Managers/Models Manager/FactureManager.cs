using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class FactureManager : WritableManager<Facture>, ReadableRepository<Facture>
    {
        public FactureManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<IEnumerable<Facture>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<Facture?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public byte[]? GenererPdfFactureParCommande(int commandeId)
        {
            // ✅ CORRECTION : Utiliser _context au lieu de dbSet
            var commande = _context.Commandes
                .Include(c => c.AcheteurCommande)
                .Include(c => c.VendeurCommande)
                .Include(c => c.CommandeAnnonceNav)
                    .ThenInclude(a => a.VoitureAnnonceNav)
                        .ThenInclude(v => v.MarqueVoitureNavigation)
                .Include(c => c.CommandeAnnonceNav)
                    .ThenInclude(a => a.VoitureAnnonceNav)
                        .ThenInclude(v => v.ModeleVoitureNavigation)
                .Include(c => c.CommandeAnnonceNav)
                    .ThenInclude(a => a.VoitureAnnonceNav)
                        .ThenInclude(v => v.CarburantVoitureNavigation)
                .Include(c => c.CommandeAnnonceNav)
                    .ThenInclude(a => a.AdresseAnnonceNav)
                        .ThenInclude(a => a.PaysAdresseNav)
                .Include(c => c.CommandeMoyenPaiementNav)
                .Include(c => c.EtatCommandeCommandeNav)
                .Include(c => c.Offrecommande)
                .FirstOrDefault(c => c.IdCommande == commandeId);

            if (commande == null) return null;

            // Configuration de la licence QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // Génération du document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(2, Unit.Centimetre);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // En-tête
                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("AutoPulse")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                col.Item().Text("Plateforme de vente automobile")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"FACTURE N° {commande.IdCommande:D6}")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                col.Item().Text($"Date : {commande.Date:dd/MM/yyyy}")
                                    .FontSize(10);
                            });
                        });

                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    // Contenu
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        // Section Vendeur et Acheteur
                        column.Item().Row(row =>
                        {
                            // Vendeur
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Column(col =>
                                {
                                    col.Item().Text("VENDEUR").Bold().FontSize(12)
                                        .FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Text(commande.VendeurCommande.Pseudo ?? "")
                                        .Bold();
                                    col.Item().Text($"{commande.VendeurCommande.Nom} {commande.VendeurCommande.Prenom}");
                                    col.Item().Text(commande.VendeurCommande.Email ?? "");

                                    if (!string.IsNullOrEmpty(commande.VendeurCommande.NumeroSiret))
                                    {
                                        col.Item().PaddingTop(5).Text($"SIRET : {commande.VendeurCommande.NumeroSiret}");
                                        col.Item().Text($"Raison sociale : {commande.VendeurCommande.RaisonSociale}");
                                    }
                                });

                            // Acheteur
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Column(col =>
                                {
                                    col.Item().Text("ACHETEUR").Bold().FontSize(12)
                                        .FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Text(commande.AcheteurCommande.Pseudo ?? "")
                                        .Bold();
                                    col.Item().Text($"{commande.AcheteurCommande.Nom} {commande.AcheteurCommande.Prenom}");
                                    col.Item().Text(commande.AcheteurCommande.Email ?? "");
                                });
                        });

                        // Adresse de livraison
                        if (commande.CommandeAnnonceNav?.AdresseAnnonceNav != null)
                        {
                            var adresse = commande.CommandeAnnonceNav.AdresseAnnonceNav;
                            column.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Column(col =>
                                {
                                    col.Item().Text("ADRESSE DE RÉCUPÉRATION").Bold().FontSize(12)
                                        .FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Text($"{adresse.Numero} {adresse.Rue}");
                                    col.Item().Text($"{adresse.CodePostal} {adresse.LibelleVille}");
                                    col.Item().Text(adresse.PaysAdresseNav?.Libelle ?? "");
                                });
                        }

                        // Détails du véhicule
                        column.Item().PaddingTop(20).Text("DÉTAILS DU VÉHICULE")
                            .Bold().FontSize(14).FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            // En-tête du tableau
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3)
                                    .Padding(8).Text("Description").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3)
                                    .Padding(8).Text("Détails").Bold();
                                header.Cell().Background(Colors.Blue.Lighten3)
                                    .Padding(8).AlignRight().Text("Montant").Bold();
                            });

                            var voiture = commande.CommandeAnnonceNav?.VoitureAnnonceNav;
                            var annonce = commande.CommandeAnnonceNav;

                            // Ligne du véhicule
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(8).Column(col =>
                                {
                                    col.Item().Text(annonce?.Libelle ?? "Véhicule").Bold();
                                    col.Item().Text($"{voiture?.MarqueVoitureNavigation?.LibelleMarque} {voiture?.ModeleVoitureNavigation?.LibelleModele}");
                                    col.Item().PaddingTop(5).Text($"Année : {voiture?.Annee}");
                                });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(8).Column(col =>
                                {
                                    col.Item().Text($"Kilométrage : {voiture?.Kilometrage:N0} km");
                                    col.Item().Text($"Carburant : {voiture?.CarburantVoitureNavigation?.LibelleCarburant}");
                                    col.Item().Text($"Puissance : {voiture?.Puissance} ch");
                                });

                            var montant = commande.Offrecommande?.Valeur ?? annonce?.Prix ?? 0;
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(8).AlignRight().Text($"{montant:N2} €").FontSize(12);
                        });

                        // Totaux
                        column.Item().PaddingTop(15).AlignRight().Column(col =>
                        {
                            var montant = commande.Offrecommande?.Valeur ?? commande.CommandeAnnonceNav?.Prix ?? 0;

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Sous-total :").Bold();
                                row.ConstantItem(120).AlignRight().Text($"{montant:N2} €");
                            });

                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("TVA (20%) :").Bold();
                                row.ConstantItem(120).AlignRight().Text($"{montant * 0.20m:N2} €");
                            });

                            col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Colors.Blue.Darken2);

                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL TTC :").Bold().FontSize(14)
                                    .FontColor(Colors.Blue.Darken2);
                                row.ConstantItem(120).AlignRight().Text($"{montant * 1.20m:N2} €")
                                    .Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                            });
                        });

                        // Informations de paiement
                        column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(10).Column(col =>
                            {
                                col.Item().Text("INFORMATIONS DE PAIEMENT").Bold().FontSize(12)
                                    .FontColor(Colors.Blue.Darken1);
                                col.Item().PaddingTop(5).Text($"Moyen de paiement : {commande.CommandeMoyenPaiementNav?.TypePaiement}");
                                col.Item().Text($"Statut : {commande.EtatCommandeCommandeNav?.Libelle}");
                            });

                        // Mentions légales
                        column.Item().PaddingTop(20).Text(text =>
                        {
                            text.Span("Mentions légales : ").Bold().FontSize(9);
                            text.Span("Cette facture est émise dans le cadre d'une transaction entre particuliers sur la plateforme AutoPulse. " +
                                     "En cas de litige, merci de contacter notre service client.")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // Pied de page
                    page.Footer().AlignCenter().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("AutoPulse - contact@autopulse.fr - ").FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}