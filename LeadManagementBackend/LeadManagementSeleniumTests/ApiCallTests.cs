using FluentAssertions;
using LeadManagementSeleniumTests.Fixtures;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests;

/// <summary>
/// Tests end-to-end API calls by performing UI actions that trigger actual
/// backend API requests and verifying the resulting page state.
/// </summary>
public class ApiCallTests : IClassFixture<SeleniumFixture>
{
    private readonly SeleniumFixture _fixture;
    private static bool _loggedIn;

    public ApiCallTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        if (!_loggedIn)
        {
            _fixture.LoginAsAdmin();
            _loggedIn = true;
        }
    }

    [Fact]
    public void CreateLead_ApiCall_CreatesLeadAndRedirectsToList()
    {
        var uniqueName = $"API Test Lead {Guid.NewGuid():N}";

        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(uniqueName);
        _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys($"apitest-{Guid.NewGuid():N}@test.com");
        _fixture.Driver.FindElement(By.CssSelector("input[name='phone']")).SendKeys("555-0199");
        _fixture.Driver.FindElement(By.CssSelector("input[name='company']")).SendKeys("API Corp");

        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']")))
            .SelectByText("Website");
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']")))
            .SelectByText("High");

        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Should redirect to the leads list page
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.TrimEnd('/').EndsWith("/leads"));

        // The newly created lead should appear in the table
        _fixture.WaitForElement(By.CssSelector("table"));
        var pageSource = _fixture.Driver.PageSource;
        pageSource.Should().Contain(uniqueName, "newly created lead should appear on the leads list");
    }

    [Fact]
    public void GetLeads_ApiCall_LoadsLeadListPage()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table, p.text-gray-500"), 15);

        var hasTable = _fixture.ElementExists(By.CssSelector("table"));
        var hasEmptyMessage = _fixture.ElementExists(By.XPath("//*[contains(text(), 'No leads found')]"));

        (hasTable || hasEmptyMessage).Should().BeTrue("lead list page should show either leads table or empty message");
    }

    [Fact]
    public void Login_ApiCall_AuthenticatesAndRedirects()
    {
        // Logout first
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");
        ((IJavaScriptExecutor)_fixture.Driver).ExecuteScript("localStorage.clear();");
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");

        _fixture.WaitForElement(By.CssSelector("input[type='email']"));

        _fixture.Driver.FindElement(By.CssSelector("input[type='email']")).SendKeys("admin@leadcrm.com");
        _fixture.Driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("Admin@123");
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));
        wait.Until(d => d.Url.Contains("/leads"));

        _fixture.Driver.Url.Should().Contain("/leads");
    }

    [Fact]
    public void Login_InvalidCredentials_ShowsError()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");
        ((IJavaScriptExecutor)_fixture.Driver).ExecuteScript("localStorage.clear();");
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");

        _fixture.WaitForElement(By.CssSelector("input[type='email']"));

        _fixture.Driver.FindElement(By.CssSelector("input[type='email']")).SendKeys("wrong@example.com");
        _fixture.Driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("WrongPassword");
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(5));
        wait.Until(d =>
        {
            var errorElements = d.FindElements(By.CssSelector("[class*='text-red'], [class*='bg-red'], [role='alert']"));
            return errorElements.Any(e => e.Displayed);
        });

        _fixture.Driver.Url.Should().Contain("/login");
    }

    [Fact]
    public void DeleteLead_ApiCall_RemovesLeadFromList()
    {
        var uniqueName = $"Delete Me {Guid.NewGuid():N}";

        // Create a lead to delete
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(uniqueName);
        _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys($"deleteme-{Guid.NewGuid():N}@test.com");
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']")))
            .SelectByText("Partner");
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']")))
            .SelectByText("Low");

        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Wait for redirect to list
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.TrimEnd('/').EndsWith("/leads"));
        _fixture.WaitForElement(By.CssSelector("table"));

        // Find and click the Delete button for our lead
        var deleteButton = _fixture.WaitForElement(
            By.XPath($"//tr[contains(., '{uniqueName}')]//button[contains(text(), 'Delete')]"));
        deleteButton.Click();

        // Handle the window.confirm dialog
        wait.Until(d =>
        {
            try { d.SwitchTo().Alert(); return true; }
            catch (NoAlertPresentException) { return false; }
        });
        _fixture.Driver.SwitchTo().Alert().Accept();

        // Lead should disappear from the list
        wait.Until(d =>
        {
            var rows = d.FindElements(By.XPath($"//tr[contains(., '{uniqueName}')]"));
            return rows.Count == 0;
        });
    }
}
