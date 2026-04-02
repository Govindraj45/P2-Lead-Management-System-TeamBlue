using FluentAssertions;
using LeadManagementSeleniumTests.Fixtures;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests;

/// <summary>
/// Tests that buttons and actions are shown/hidden based on user role:
/// - Admin: can see Delete button on lead list, Convert button on Qualified leads
/// - SalesManager: can see Convert button, cannot see Delete button
/// - SalesRep: cannot see Delete or Convert buttons
/// </summary>
public class RoleBasedButtonTests : IClassFixture<SeleniumFixture>
{
    private readonly SeleniumFixture _fixture;
    private static string? _qualifiedLeadName;
    private static readonly object _setupLock = new();

    public RoleBasedButtonTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        EnsureTestData();
    }

    private void EnsureTestData()
    {
        lock (_setupLock)
        {
            if (_qualifiedLeadName != null) return;

            _fixture.LoginAsAdmin();

            var leadName = $"RoleTest {Guid.NewGuid():N}";

            // Create a lead
            _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
            _fixture.WaitForElement(By.CssSelector("input[name='name']"));

            _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(leadName);
            _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys($"roletest-{Guid.NewGuid():N}@test.com");
            new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']")))
                .SelectByText("Website");
            new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']")))
                .SelectByText("High");

            _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Wait for redirect to /leads list
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.Url.TrimEnd('/').EndsWith("/leads"));
            _fixture.WaitForElement(By.CssSelector("table"));

            // Navigate to the detail page by clicking the lead link
            var leadLink = _fixture.WaitForElement(By.XPath($"//a[contains(text(), '{leadName}')]"));
            leadLink.Click();

            wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains(leadName)));

            // Advance: New → Contacted
            _fixture.WaitForElement(By.XPath("//button[contains(text(), 'Move to Contacted')]")).Click();
            wait.Until(d => d.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"))
                .Any(b => b.Text.Trim() == "Contacted"));

            // Advance: Contacted → Qualified
            _fixture.WaitForElement(By.XPath("//button[contains(text(), 'Move to Qualified')]")).Click();
            wait.Until(d => d.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"))
                .Any(b => b.Text.Trim() == "Qualified"));

            _qualifiedLeadName = leadName;
        }
    }

    private void NavigateToQualifiedLead()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var leadLink = _fixture.WaitForElement(By.XPath($"//a[contains(text(), '{_qualifiedLeadName}')]"));
        leadLink.Click();

        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains(_qualifiedLeadName!)));
    }

    // ──────────── ADMIN TESTS ────────────

    [Fact]
    public void Admin_CanSee_DeleteButton_OnLeadList()
    {
        _fixture.LoginAsAdmin();
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var deleteButtons = _fixture.Driver.FindElements(By.XPath("//button[contains(text(), 'Delete')]"));
        deleteButtons.Count.Should().BeGreaterThan(0, "Admin should see Delete buttons on the lead list");
    }

    [Fact]
    public void Admin_CanSee_ConvertButton_OnQualifiedLead()
    {
        _fixture.LoginAsAdmin();
        NavigateToQualifiedLead();

        var convertButton = _fixture.ElementExists(By.XPath("//button[contains(text(), 'Convert Lead')]"));
        convertButton.Should().BeTrue("Admin should see Convert Lead button on Qualified lead");
    }

    [Fact]
    public void Admin_CanSee_EditButton_OnNonConvertedLead()
    {
        _fixture.LoginAsAdmin();
        NavigateToQualifiedLead();

        var editLink = _fixture.ElementExists(By.XPath("//a[contains(text(), 'Edit')]"));
        editLink.Should().BeTrue("Admin should see Edit button on non-converted lead");
    }

    // ──────────── SALES MANAGER TESTS ────────────

    [Fact]
    public void SalesManager_CanSee_ConvertButton_OnQualifiedLead()
    {
        _fixture.LoginAsManager();
        NavigateToQualifiedLead();

        var convertButton = _fixture.ElementExists(By.XPath("//button[contains(text(), 'Convert Lead')]"));
        convertButton.Should().BeTrue("SalesManager should see Convert Lead button on Qualified lead");
    }

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

    [Fact]
    public void SalesRep_CannotSee_DeleteButton_OnLeadList()
    {
        _fixture.LoginAsRep();
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table, p.text-gray-500"), 15);

        if (_fixture.ElementExists(By.CssSelector("table")))
        {
            var deleteButtons = _fixture.Driver.FindElements(By.XPath("//button[contains(text(), 'Delete')]"));
            deleteButtons.Where(b => b.Displayed).Should().BeEmpty("SalesRep should NOT see Delete buttons");
        }
    }

    [Fact]
    public void SalesRep_CannotSee_ConvertButton_OnQualifiedLead()
    {
        _fixture.LoginAsRep();
        NavigateToQualifiedLead();

        var convertButton = _fixture.ElementExists(By.XPath("//button[contains(text(), 'Convert Lead')]"));
        convertButton.Should().BeFalse("SalesRep should NOT see Convert Lead button");
    }
}
