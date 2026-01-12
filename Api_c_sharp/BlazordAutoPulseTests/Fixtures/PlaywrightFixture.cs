using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    [SetUpFixture]
    public class PlaywrightFixture
    {
        public static IPlaywright Playwright;
        public static IBrowser Browser;

        [OneTimeSetUp]
        public async Task Setup()
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }
