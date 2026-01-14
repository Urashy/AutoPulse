using Microsoft.Playwright;
using Xunit;

namespace BlazordAutoPulseTests
{
    public class SearchTests : BaseTest
    {
        [Fact]
        public async Task Recherche_Depuis_Home_Redirects_To_Search()
        {
            await Page.FillAsync("[data-testid='search-nom']", "Peugeot");
            await Page.ClickAsync("[data-testid='btn-search']");

            await Page.WaitForURLAsync("**/search*");

            Assert.Contains("/search", Page.Url);

            var results = Page.Locator("[data-testid='results-grid']");
            await results.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            var count = await results.Locator(".car-card").CountAsync();
            Assert.True(count > 0);
        }
    }
}
