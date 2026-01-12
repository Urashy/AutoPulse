using BlazordAutoPulseTests.Pages;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazordAutoPulseTests.Tests
{
    [TestFixture]
    public class SearchPageTests
    {
        private IBrowserContext _context;
        private IPage _page;
        private SearchPage _searchPage;

        [SetUp]
        public async Task Setup()
        {
            _context = await PlaywrightFixture.Browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true, // <-- crucial
                 BaseURL = "https://localhost:7297"
            });
            _page = await _context.NewPageAsync();
            _searchPage = new SearchPage(_page);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.CloseAsync();
        }

        [Test]
        public async Task SearchPage_WithResults_ShouldDisplayCars()
        {
            // Arrange
            await _searchPage.NavigateAsync("?marque=55&prixmax=40000");
            Console.WriteLine(_page.Url);

            Assert.IsTrue(_page.Url.Contains("/search"), "Playwright n'est pas sur /search !");

            // Attendre la fin du chargement
            await _searchPage.Loader.WaitForAsync(new()
            {
                State = WaitForSelectorState.Detached
            });

            await _page.Locator(".car-card").WaitForAsync(new LocatorWaitForOptions
            {
                Timeout = 5000 // 5 secondes max
            });

            // Assert
            var count = await _searchPage.CarCards.CountAsync();
            Assert.That(count, Is.GreaterThan(0));
        }

        [Test]
        public async Task SearchPage_NoResults_ShouldDisplayMessage()
        {
            await _searchPage.NavigateAsync("?order=4&prixmax=6000");

            await _searchPage.Loader.WaitForAsync(new()
            {
                State = WaitForSelectorState.Detached
            });
            await _page.Locator(".no-results").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            Assert.That(await _searchPage.NoResults.IsVisibleAsync(), Is.True);
        }

        [Test]
        public async Task Pagination_ShouldGoToNextPage()
        {
            await _searchPage.NavigateAsync("?order=4");

            await _searchPage.Loader.WaitForAsync(new()
            {
                State = WaitForSelectorState.Detached
            });

            await _searchPage.PaginationNext.ClickAsync();

            // On attend un nouveau chargement
            await _page.WaitForTimeoutAsync(500);

            Assert.That(await _searchPage.CarCards.CountAsync(), Is.GreaterThan(0));
        }
    }
}
