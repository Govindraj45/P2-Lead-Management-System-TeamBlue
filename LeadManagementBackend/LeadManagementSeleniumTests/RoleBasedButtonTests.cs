// FluentAssertions gives us readable checks like ".Should().BeTrue()"
using FluentAssertions;
// SeleniumFixture is our shared Chrome browser setup
using LeadManagementSeleniumTests.Fixtures;
// Selenium libraries to find and interact with web page elements
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests;

/// <summary>
/// Tests that buttons and actions are shown/hidden based on user role:
/// - Admin: can see Delete button on lead list, Convert button on Qualified leads
/// - SalesManager: can see Convert button, cannot see Delete button
/// - SalesRep: cannot see Delete or Convert buttons
/// </summary>
// This test class checks that each user role sees only the buttons they are allowed to use
public class RoleBasedButtonTests : IClassFixture<SeleniumFixture>
{
    // Store a reference to the shared browser fixture
    private readonly SeleniumFixture _fixture;
    // The name of a lead we create and advance to "Qualified" status for testing
    private static string? _qualifiedLeadName;
    // A lock to make sure only one thread sets up the test data
    private static readonly object _setupLock = new();

    // Constructor: sets up test data (a Qualified lead) before running any tests
    public RoleBasedButtonTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        EnsureTestData();
    }

    // Create a lead and advance it to "Qualified" status so we can test the Convert button
    private void EnsureTestData()
    {
        // Use a lock so this only runs once, even if multiple tests start at the same time
        lock (_setupLock)
        {
            if (_qualifiedLeadName != null) return;

            // Log in as Admin (who has all permissions)
            _fixture.LoginAsAdmin();

            var leadName = $"RoleTest {Guid.NewGuid():N}";

            // Create a new lead via the form
            _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
            _fixture.WaitForElement(By.CssSelector("input[name='name']"));

            _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(leadName);
            _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys($"roletest-{Guid.NewGuid():N}@test.com");
            new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']")))
                .SelectByText("Website");
            new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']")))
                .SelectByText("High");

            _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Wait for redirect to the leads list page
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.Url.TrimEnd('/').EndsWith("/leads"));
            _fixture.WaitForElement(By.CssSelector("table"));

            // Click the lead's name link to go to its detail page
            var leadLink = _fixture.WaitForElement(By.XPath($"//a[contains(text(), '{leadName}')]"));
            leadLink.Click();

            // Wait for the detail page to load (shows the lead's name in an h1 heading)
            wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains(leadName)));

            // Advance the lead from "New" to "Contacted" by clicking the transition button
            _fixture.WaitForElement(By.XPath("//button[contains(text(), 'Move to Contacted')]")).Click();
            wait.Until(d => d.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"))
                .Any(b => b.Text.Trim() == "Contacted"));

            // Advance the lead from "Contacted" to "Qualified" by clicking the next transition button
            _fixture.WaitForElement(By.XPath("//button[contains(text(), 'Move to Qualified')]")).Click();
            wait.Until(d => d.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"))
                .Any(b => b.Text.Trim() == "Qualified"));

            // Save the lead name so other tests can find it
            _qualifiedLeadName = leadName;
        }
    }

    // Helper: navigate to the detail page of our Qualified test lead
    private void NavigateToQualifiedLead()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        // Click the link with our lead's name
        var leadLink = _fixture.WaitForElement(By.XPath($"//a[contains(text(), '{_qualifiedLeadName}')]"));
        leadLink.Click();

        // Wait until the detail page header shows our lead's name
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains(_qualifiedLeadName!)));
    }

    // ──────────── ADMIN TESTS ────────────

    // TEST: Admin should see Delete buttons on the lead list page
    [Fact]
    public void Admin_CanSee_DeleteButton_OnLeadList()
    {
        _fixture.LoginAsAdmin();
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        // Look for any Delete buttons on the page
        var deleteButtons = _fixture.Driver.FindElements(By.XPath("//button[contains(text(), 'Delete')]"));
        deleteButtons.Count.Should().BeGreaterThan(0, "Admin should see Delete buttons on the lead list");
    }

    // TEST: Admin should see the "Convert Lead" button on a Qualified lead's detail page
    [Fact]
    public void Admin_CanSee_ConvertButton_OnQualifiedLead()
    {
        _fixture.LoginAsAdmin();
        NavigateToQualifiedLead();

        var convertButton = _fixture.ElementExists(By.XPath("//button[contains(text(), 'Convert Lead')]"));
        convertButton.Should().BeTrue("Admin should see Convert Lead button on Qualified lead");
    }

    // TEST: Admin should see the Edit button on a lead that hasn't been converted yet
    [Fact]
    public void Admin_CanSee_EditButton_OnNonConvertedLead()
    {
        _fixture.LoginAsAdmin();
        NavigateToQualifiedLead();

        var editLink = _fixture.ElementExists(By.XPath("//a[contains(text(), 'Edit')]"));
        editLink.Should().BeTrue("Admin should see Edit button on non-converted lead");
    }

    // ──────────── SALES MANAGER TESTS ────────────

    // TEST: SalesManager should see the "Convert Lead" button on a Qualified lead
    [Fact]
    public void SalesManager_CanSee_ConvertButton_OnQualifiedLead()
    {
        _fixture.LoginAsManager();
        NavigateToQualifiedLead();

        var convertButton = _fixture.ElementExists(By.XPath("//button[contains(text(), 'Convert Lead')]"));
        convertButton.Should().BeTrue("SalesManager should see Convert Lead button on Qualified lead");
    }

    // TEST: SalesManager should NOT see Delete buttons (only Admins can delete)
    [Fact]
    public void SalesManager_CannotSee_DeleteButton_OnLeadList()
    {
        _fixture.LoginAsManager();
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var deleteButtons = _fixture.Driver.FindElements(By.XPath("//button[contains(text(), 'Delete')]"));
        deleteButtons.Where(b => b.Displayed).Should().BeEmpty("SalesManager should NOT see Delete buttons");
    }

    // ──────────── SALES REP TESTS ────────────

    // TEST: SalesRep should NOT see Delete buttons on the lead list
    [Fact]
    public void SalesRep_CannotSee_DeleteButton_OnLeadList()
    {
        _fixture.LoginAsRep();
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table, p.text-gray-500"), 15);

        // Only check for Delete buttons if a table actually loaded
        if (_fixture.ElementExists(By.CssSelector("table")))
        {
            var deleteButtons = _fixture.Driver.FindElements(By.XPath("//button[contains(text(), 'Delete')]"));
            deleteButtons.Where(b => b.Displayed).Should().BeEmpty("SalesRep should NOT see Delete buttons");
        }
    }

    // TEST: SalesRep should NOT see the "Convert Lead" button (only Admin and Manager can convert)
    [Fact]
    public void SalesRep_CannotSee_ConvertButton_OnQualifiedLead()
    {
        _fixture.LoginAsRep();
        NavigateToQualifiedLead();

        var convertButton = _fixture.ElementExists(By.XPath("//button[contains(text(), 'Convert Lead')]"));
        convertButton.Should().BeFalse("SalesRep should NOT see Convert Lead button");
    }
}

/*
 * FILE SUMMARY:
 * RoleBasedButtonTests verifies that the UI correctly shows or hides buttons based on who
 * is logged in. Admins should see Delete, Edit, and Convert buttons; SalesManagers should
 * see Convert but not Delete; SalesReps should see neither Delete nor Convert. The tests
 * first create a lead and advance it to "Qualified" status, then log in as each role and
 * check which buttons are visible on the page.
 */
