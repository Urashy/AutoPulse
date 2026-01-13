using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace BlazordAutoPulseTests
{
    [Trait("Category", "E2E")]
    public abstract class BaseTest : IAsyncLifetime
    {
        protected const string BaseUrl = "http://localhost:5296";

        protected IPlaywright Playwright { get; private set; }
        protected IBrowser Browser { get; private set; }
        protected IPage Page { get; private set; }

        // Avant chaque test
        public async Task InitializeAsync()
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true // ou false si tu veux voir le navigateur
            });

            Page = await Browser.NewPageAsync();
            await Page.GotoAsync(BaseUrl);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Après chaque test
        public async Task DisposeAsync()
        {
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }
}
