using Microsoft.Playwright;
using Xunit;

namespace BlazordAutoPulseTests
{
    public class FiltersTests : BaseTest
    {
        [Fact]
        public async Task Filtre_Marque_Retourne_Des_Resultats()
        {
            await Page.SelectOptionAsync("[data-testid='select-marque']", "Alpine");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.Locator(".car-card").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            var annonces = Page.Locator(".car-card");
            await annonces.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            var count = await annonces.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var nom = await annonces.Nth(i).Locator("[data-testid='title']").InnerTextAsync();
                Assert.Contains("Alpine", nom);
            }
        }

        [Fact]
        public async Task Filtre_Prix_Entre_5000_Et_10000_Fonctionne()
        {
            await Page.GotoAsync($"{BaseUrl}/search");

            // ⏳ Attendre que les sliders existent réellement
            await Page.WaitForSelectorAsync("#prix-slider-min");
            await Page.WaitForSelectorAsync("#prix-slider-max");

            // Définir le prix min = 5000
            await Page.EvaluateAsync(
                @"(value) => {
            const minSlider = document.getElementById('prix-slider-min');
            minSlider.value = value;
            minSlider.dispatchEvent(new Event('input', { bubbles: true }));
        }",
                5000
            );

            // Définir le prix max = 10000
            await Page.EvaluateAsync(
                @"(value) => {
            const maxSlider = document.getElementById('prix-slider-max');
            maxSlider.value = value;
            maxSlider.dispatchEvent(new Event('input', { bubbles: true }));
        }",
                10000
            );

            // Lancer la recherche
            await Page.ClickAsync("[data-testid='btn-search']");

            // Attendre les résultats
            var cards = Page.Locator(".car-card");
            await cards.First.WaitForAsync();

            var prices = Page.Locator("[data-testid='prix']");
            int count = await prices.CountAsync();

            Assert.True(count > 0);

            for (int i = 0; i < count; i++)
            {
                var priceText = await prices.Nth(i).InnerTextAsync();

                // "12 500 €" → 12500
                var numeric = priceText
                    .Replace("€", "")
                    .Replace("\u202F", "")
                    .Replace("\u00A0", "")
                    .Replace(" ", "")
                    .Trim();

                int price = int.Parse(numeric);
                Assert.InRange(price, 5000, 10000);
            }
        }


        [Fact]
        public async Task Recherche_Par_Nom_Fonctionne()
        {
            await Page.GotoAsync($"{BaseUrl}");
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await Page.FillAsync("[data-testid='search-nom']", "Audi");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var annonces = Page.Locator(".car-card");
            await annonces.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            var count = await annonces.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var nom = await annonces.Nth(i).Locator("[data-testid='title']").InnerTextAsync();
                Assert.Contains("Audi", nom);
            }
        }

            [Fact]
        public async Task Reset_Reinitialise_Les_Filtres()
        {
            await Page.FillAsync("[data-testid='search-nom']", "BMW");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForURLAsync("**/search*");

            await Page.ClickAsync("[data-testid='btn-reset']");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var value = await Page.InputValueAsync("[data-testid='search-nom']");
            Assert.True(string.IsNullOrEmpty(value));
        }

        [Fact]
        public async Task Recherche_Genere_QueryString()
        {
            await Page.FillAsync("[data-testid='search-nom']", "Toyota");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForURLAsync("**/search*");

            Assert.Contains("nom=Toyota", Page.Url);
        }

    }
}