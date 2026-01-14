using Microsoft.Playwright;
using Xunit;

namespace BlazordAutoPulseTests;

public class CompteTest : BaseTest
{
    [Fact]
    public async Task TestConnexionEmailOuMdpIncorrect()
    {
        await Page.ClickAsync("[data-testid='btn-connexion-layout']");

        await Page.WaitForURLAsync("**/connexion*");
            
        await Page.FillAsync("[data-testid='email-connexion']", "test");
        await Page.FillAsync("[data-testid='password-connexion']", "test");
            
        await Page.ClickAsync("[data-testid='btn-connexion']");
        
        await Page.Locator("[data-testid='error-box']").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        }); 
            
        var errorBox = await Page.Locator("[data-testid='error-box']").AllInnerTextsAsync();
        Assert.Contains("Email ou mot de passe incorrect", errorBox);
    }
    
    [Fact]
    public async Task TestConnexionAucunChampsNonRempli()
    {
        await Page.ClickAsync("[data-testid='btn-connexion-layout']");

        await Page.WaitForURLAsync("**/connexion*");
            
        await Page.ClickAsync("[data-testid='btn-connexion']");
        
        await Page.Locator("[data-testid='error-box']").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        }); 
            
        var errorBox = await Page.Locator("[data-testid='error-box']").AllInnerTextsAsync();
        Assert.Contains("Veuillez remplir tous les champs", errorBox);
    }
    
    [Fact]
    public async Task TestInscriptionNonRempli()
    {
        await Page.ClickAsync("[data-testid='btn-connexion-layout']");

        await Page.WaitForURLAsync("**/connexion*");
            
        await Page.ClickAsync("[data-testid='btn-inscription']");
        
        await Page.WaitForURLAsync("**/creationcompte*");
        
        await Page.ClickAsync("[data-testid='btn-valide-inscription']");
        
        await Page.Locator("[data-testid='error-box']").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        }); 
            
        var errorBox = await Page.Locator("[data-testid='error-box']").AllInnerTextsAsync();
        Assert.Contains("Le pseudo est requis", errorBox);
    }
    
    [Fact]
    public async Task TestInscriptionMdpDifferent()
    {
        await Page.ClickAsync("[data-testid='btn-connexion-layout']");

        await Page.WaitForURLAsync("**/connexion*");
            
        await Page.ClickAsync("[data-testid='btn-inscription']");
        
        await Page.WaitForURLAsync("**/creationcompte*");
        
        await Page.FillAsync("[data-testid='pseudo-inscription']", "test");
        await Page.FillAsync("[data-testid='email-inscription']", "test@test.test");
        
        await Page.FillAsync("[data-testid='password-inscription']", "Testtest1*");
        await Page.FillAsync("[data-testid='password-confirmation-inscription']", "Testtest1");
        
        await Page.ClickAsync("[data-testid='btn-valide-inscription']");
        await Page.ClickAsync("[data-testid='btn-valide-inscription']");
        
        await Page.Locator("[data-testid='error-box']").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        }); 
            
        var errorBox = await Page.Locator("[data-testid='error-box']").AllInnerTextsAsync();
        Assert.Contains("Les mots de passe ne correspondent pas", errorBox);
    }
    
    [Fact]
    public async Task TestConnexionReussi()
    {
        await Page.ClickAsync("[data-testid='btn-connexion-layout']");

        await Page.WaitForURLAsync("**/connexion*");
        
        await Page.FillAsync("[data-testid='email-connexion']", "info@ecoauto.com");
        await Page.FillAsync("[data-testid='password-connexion']", "ouioui");
        
        await Page.ClickAsync("[data-testid='btn-connexion']");
        
        await Page.WaitForURLAsync("**/*");
        
        await Page.ClickAsync("[data-testid='btn-compte']");
        
        await Page.WaitForURLAsync("**/compte*");
        
        var emailCompte = await Page.Locator("[data-testid='email-compte']").AllInnerTextsAsync();
        
        Assert.Contains("info@ecoauto.com", emailCompte);
    }
}