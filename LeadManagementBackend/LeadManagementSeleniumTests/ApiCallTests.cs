// FluentAssertions gives us readable checks like ".Should().BeTrue()"
using FluentAssertions;
// SeleniumFixture is our shared Chrome browser setup
using LeadManagementSeleniumTests.Fixtures;
// Selenium libraries to find and interact with web page elements
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests;

/// <summary>
/// Tests end-to-end API calls by performing UI actions that trigger actual
/// backend API requests and verifying the resulting page state.
/// </summary>
// This test class uses SeleniumFixture to get a shared Chrome browser
public class ApiCallTests : IClassFixture<SeleniumFixture>
{
    // Store a reference to the shared browser fixture
    private readonly SeleniumFixture _fixture;
    // Track whether we already logged in (so we only log in once for all tests)
    private static bool _loggedIn;

    // Constructor: runs before each test. Logs in as Admin the first time.
    public ApiCallTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        if (!_loggedIn)
        {
            _fixture.LoginAsAdmin();
            _loggedIn = true;
        }
    }

    // TEST: Fill out the "Create Lead" form and verify the new lead appears in the list
    [Fact]
    public void CreateLead_ApiCall_CreatesLeadAndRedirectsToList()
    {
        // Generate a unique lead name so we can find it later
        var uniqueName = $"API Test Lead {Guid.NewGuid():N}";

        // Navigate to the "Create Lead" page and wait for the form to load
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        // Fill in the form fields: name, email, phone, company
        _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(uniqueName);
        _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys($"apitest-{Guid.NewGuid():N}@test.com");
        _fixture.Driver.FindElement(By.CssSelector("input[name='phone']")).SendKeys("555-0199");
        _fixture.Driver.FindElement(By.CssSelector("input[name='company']")).SendKeys("API Corp");

        // Pick "Website" from the Source dropdown and "High" from the Priority dropdown
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']")))
            .SelectByText("Website");
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']")))
            .SelectByText("High");

        // Click the submit button to create the lead
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Wait until the browser redirects back to the leads list page
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.TrimEnd('/').EndsWith("/leads"));

        // Check that our new lead's name appears somewhere on the page
        _fixture.WaitForElement(By.CssSelector("table"));
        var pageSource = _fixture.Driver.PageSource;
        pageSource.Should().Contain(uniqueName, "newly created lead should appear on the leads list");
    }

    // TEST: Navigate to the leads page and verify it shows either a table of leads or an "empty" message
    [Fact]
    public void GetLeads_ApiCall_LoadsLeadListPage()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table, p.text-gray-500"), 15);

        // Check if there's a table (leads exist) or a "No leads found" message
        var hasTable = _fixture.ElementExists(By.CssSelector("table"));
        var hasEmptyMessage = _fixture.ElementExists(By.XPath("//*[contains(text(), 'No leads found')]"));

        (hasTable || hasEmptyMessage).Should().BeTrue("lead list page should show either leads table or empty message");
    }

    // TEST: Log in with valid Admin credentials and verify we get redirected to /leads
    [Fact]
    public void Login_ApiCall_AuthenticatesAndRedirects()
    {
        // First, clear any existing login by wiping localStorage
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");
        ((IJavaScriptExecutor)_fixture.Driver).ExecuteScript("localStorage.clear();");
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");

        // Wait for the login form to appear
        _fixture.WaitForElement(By.CssSelector("input[type='email']"));

        // Type in valid admin credentials and click login
        _fixture.Driver.FindElement(By.CssSelector("input[type='email']")).SendKeys("admin@leadcrm.com");
        _fixture.Driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("Admin@123");
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Verify the browser redirected to the /leads page (login was successful)
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));
        wait.Until(d => d.Url.Contains("/leads"));

        _fixture.Driver.Url.Should().Contain("/leads");
    }

    // TEST: Try logging in with wrong credentials and verify an error message appears
    [Fact]
    public void Login_InvalidCredentials_ShowsError()
    {
        // Clear any existing login session
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");
        ((IJavaScriptExecutor)_fixture.Driver).ExecuteScript("localStorage.clear();");
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/login");

        _fixture.WaitForElement(By.CssSelector("input[type='email']"));

        // Enter wrong email and password
        _fixture.Driver.FindElement(By.CssSelector("input[type='email']")).SendKeys("wrong@example.com");
        _fixture.Driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("WrongPassword");
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Wait for a red error message to appear on the page
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(5));
        wait.Until(d =>
        {
            var errorElements = d.FindElements(By.CssSelector("[class*='text-red'], [class*='bg-red'], [role='alert']"));
            return errorElements.Any(e => e.Displayed);
        });

        // We should still be on the login page (not redirected to /leads)
        _fixture.Driver.Url.Should().Contain("/login");
    }

    // TEST: Create a lead, then delete it, and verify it disappears from the list
    [Fact]
    public void DeleteLead_ApiCall_RemovesLeadFromList()
    {
        var uniqueName = $"Delete Me {Guid.NewGuid():N}";

        // Step 1: Create a new lead that we will delete
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(uniqueName);
        _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys($"deleteme-{Guid.NewGuid():N}@test.com");
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']")))
            .SelectByText("Partner");
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']")))
            .SelectByText("Low");

        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Wait for the leads list page to load after creating the lead
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.TrimEnd('/').EndsWith("/leads"));
        _fixture.WaitForElement(By.CssSelector("table"));

        // Step 2: Find the Delete button in the same row as our lead and click it
        var deleteButton = _fixture.WaitForElement(
            By.XPath($"//tr[contains(., '{uniqueName}')]//button[contains(text(), 'Delete')]"));
        deleteButton.Click();

        // Step 3: A browser confirmation dialog ("Are you sure?") pops up — click OK
        wait.Until(d =>
        {
            try { d.SwitchTo().Alert(); return true; }
            catch (NoAlertPresentException) { return false; }
        });
        _fixture.Driver.SwitchTo().Alert().Accept();

        // Step 4: Verify the lead's row is gone from the table
        wait.Until(d =>
        {
            var rows = d.FindElements(By.XPath($"//tr[contains(., '{uniqueName}')]"));
            return rows.Count == 0;
        });
    }
}

/*
 * FILE SUMMARY:
 * ApiCallTests verifies that real API calls work correctly by automating browser actions.
 * It tests creating a lead, loading the lead list, logging in with valid and invalid
 * credentials, and deleting a lead. Each test uses Selenium to fill forms and click
 * buttons just like a real user, then checks that the page updates correctly afterward.
 */
