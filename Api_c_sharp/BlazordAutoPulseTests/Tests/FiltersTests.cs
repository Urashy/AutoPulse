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
            Assert.True(await annonces.CountAsync() > 0);
        }

        [Fact]
        public async Task Filtre_Carburant_Fonctionne()
        {
            await Page.SelectOptionAsync("[data-testid='select-carburant']", "2");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.Locator(".car-card").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.True(await Page.Locator(".car-card").CountAsync() > 0);
        }

        [Fact]
        public async Task Recherche_Par_Nom_Fonctionne()
        {
            await Page.FillAsync("[data-testid='search-nom']", "Audi");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var annonces = Page.Locator(".car-card");
            await annonces.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            var count = await annonces.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var nom = await annonces.Nth(i).Locator(".title").InnerTextAsync();
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