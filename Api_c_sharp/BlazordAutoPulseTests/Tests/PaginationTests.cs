using BlazordAutoPulseTests;
using Microsoft.Playwright;
using Xunit;

public class PaginationTests : BaseTest
{
    [Fact]
    public async Task Pagination_Suivant_Change_La_Page()
    {
        await Page.GotoAsync($"{BaseUrl}/search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstPageInfo = await Page.InnerTextAsync(".pagination-info");

        var nextButton = Page.Locator("[data-testid='pagination-next']");
        await nextButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await nextButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var secondPageInfo = await Page.InnerTextAsync(".pagination-info");

        Assert.NotEqual(firstPageInfo, secondPageInfo);
    }

    [Fact]
    public async Task Pagination_Precedent_Fonctionne()
    {
        await Page.GotoAsync($"{BaseUrl}/search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var nextButton = Page.Locator("[data-testid='pagination-next']");
        await nextButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await nextButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var prevButton = Page.Locator("[data-testid='pagination-prev']");
        await prevButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await prevButton.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var grid = Page.Locator(".car-card");
        Assert.True(await grid.CountAsync() > 0);
    }

}
