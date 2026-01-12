using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazordAutoPulseTests.Pages
{
    public class SearchPage
    {
        private readonly IPage _page;

        public SearchPage(IPage page)
        {
            _page = page;
        }

        public async Task NavigateAsync(string query = "")
        {
            await _page.GotoAsync($"/search{query}");
        }

        public ILocator Loader => _page.Locator(".loading");
        public ILocator CarCards => _page.Locator(".car-card");
        public ILocator NoResults => _page.Locator(".no-results");
        public ILocator PaginationNext => _page.Locator(".pagination-btn:has-text('Suivant')");
    }
}
