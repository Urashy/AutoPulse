using BlazordAutoPulseTests;
using Microsoft.Playwright;
using Xunit;

public class PaginationTests : BaseTest
{
    [Fact]
    public async Task Pagination_Suivant_Change_La_Page()
    {
        // chargement de la page
        await Page.GotoAsync($"{BaseUrl}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstPageInfo = await Page.InnerTextAsync(".pagination-info");

        // Clique sur le button "suivant"
        var nextButton = Page.Locator("[data-testid='pagination-next']");
        await nextButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await nextButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var secondPageInfo = await Page.InnerTextAsync(".pagination-info");

        // Vérifie que les infos des pages 1 et 2 sont différente 
        Assert.NotEqual(firstPageInfo, secondPageInfo);
    }

    [Fact]
    public async Task Pagination_Precedent_Fonctionne()
    {
        // chargement de la page
        await Page.GotoAsync($"{BaseUrl}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstPageInfo = await Page.InnerTextAsync(".pagination-info");

        // Clique sur le button "suivant"
        var nextButton = Page.Locator("[data-testid='pagination-next']");
        await nextButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await nextButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var secondPageInfo = await Page.InnerTextAsync(".pagination-info");

        // Vérifie que les infos des pages 1 et 2 sont différente 
        Assert.NotEqual(firstPageInfo, secondPageInfo);

        // Clique sur le button "préceden"
        var prevButton = Page.Locator("[data-testid='pagination-prev']");
        await prevButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await prevButton.ClickAsync();

        var finalPageInfo = await Page.InnerTextAsync(".pagination-info");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Vérifie que les infos de la page 1 et de la page final sont les même 
        Assert.Equal(firstPageInfo, finalPageInfo);
    }

}
