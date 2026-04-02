// FluentAssertions gives us readable checks like ".Should().BeTrue()"
using FluentAssertions;
// SeleniumFixture is our shared Chrome browser setup
using LeadManagementSeleniumTests.Fixtures;
// Selenium libraries to find and interact with web page elements
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests;

// This test class checks that status badges (colored labels like "New", "Contacted") display correctly
public class StatusBadgeTests : IClassFixture<SeleniumFixture>
{
    // Store a reference to the shared browser fixture
    private readonly SeleniumFixture _fixture;
    // Track whether we already logged in
    private static bool _loggedIn;

    // Constructor: logs in as Admin once before all tests in this class
    public StatusBadgeTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        if (!_loggedIn)
        {
            _fixture.LoginAsAdmin();
            _loggedIn = true;
        }
    }

    // TEST: After creating a lead, the leads list page should show a "New" status badge
    [Fact]
    public void StatusBadge_RendersCorrectly_OnLeadListPage()
    {
        // Create a fresh lead so we know at least one "New" badge exists
        CreateTestLead("StatusBadge List Lead", $"statuslist-{Guid.NewGuid():N}@test.com", "Website", "High");

        // Navigate to the leads list and wait for the table to appear
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        // Find all status badge elements on the page (styled as rounded pill shapes)
        var badges = _fixture.Driver.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"));
        badges.Count.Should().BeGreaterThan(0, "at least one status badge should be displayed");

        // Check that one of the badges says "New"
        var badgeTexts = badges.Select(b => b.Text.Trim()).ToList();
        badgeTexts.Should().Contain("New", "newly created lead should show 'New' status badge");
    }

    // TEST: The lead detail page should also show the status badge with a colored dot
    [Fact]
    public void StatusBadge_DisplaysOnDetailPage()
    {
        // Create a lead and go to its detail page
        CreateTestLead("BadgeDetail Lead", $"badgedetail-{Guid.NewGuid():N}@test.com", "Referral", "Medium");

        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        // Click the lead's name to open the detail page
        var leadLink = _fixture.WaitForElement(By.XPath("//a[contains(text(), 'BadgeDetail Lead')]"));
        leadLink.Click();

        // Wait for the detail page to load (the lead name appears in a heading)
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains("BadgeDetail Lead")));

        // Verify the badge says "New" and the colored dot inside it is visible
        var badge = _fixture.Driver.FindElement(By.CssSelector("span.inline-flex.items-center.rounded-full"));
        badge.Text.Trim().Should().Be("New");

        var dot = badge.FindElement(By.CssSelector("span.rounded-full"));
        dot.Displayed.Should().BeTrue();
    }

    // TEST: When we change a lead's status from "New" to "Contacted", the badge should update
    [Fact]
    public void StatusBadge_UpdatesAfterStatusChange()
    {
        // Create a lead that starts as "New"
        CreateTestLead("StatusChange Lead", $"statuschange-{Guid.NewGuid():N}@test.com", "Event", "Low");

        // Navigate to the leads list and open the lead's detail page
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var leadLink = _fixture.WaitForElement(By.XPath("//a[contains(text(), 'StatusChange Lead')]"));
        leadLink.Click();

        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains("StatusChange Lead")));

        // Click the "Move to Contacted" button to change the lead's status
        var transitionButton = _fixture.WaitForElement(By.XPath("//button[contains(text(), 'Move to Contacted')]"));
        transitionButton.Click();

        // Wait until the badge text changes from "New" to "Contacted"
        wait.Until(d =>
        {
            var badges = d.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"));
            return badges.Any(b => b.Text.Trim() == "Contacted");
        });

        // Confirm the badge now says "Contacted"
        var updatedBadge = _fixture.Driver.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"))
            .First(b => b.Text.Trim() == "Contacted");
        updatedBadge.Should().NotBeNull();
    }

    // Helper method: creates a new lead by filling out the form in the browser
    private void CreateTestLead(string name, string email, string source, string priority)
    {
        // Navigate to the Create Lead form
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        // Fill in all the form fields
        _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(name);
        _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys(email);
        _fixture.Driver.FindElement(By.CssSelector("input[name='phone']")).SendKeys("555-0100");
        _fixture.Driver.FindElement(By.CssSelector("input[name='company']")).SendKeys("Test Corp");

        // Select the source and priority from their dropdowns
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']"))).SelectByText(source);
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']"))).SelectByText(priority);

        // Submit the form
        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Wait until the app redirects back to the leads list page
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
        {
            var url = d.Url.TrimEnd('/');
            return url.EndsWith("/leads");
        });
    }
}

/*
 * FILE SUMMARY:
 * StatusBadgeTests verifies that the colored status badges (like "New", "Contacted") display
 * correctly throughout the app. It checks that badges appear on both the lead list page and
 * the lead detail page, and that the badge text updates in real time when a lead's status is
 * changed. Each test creates a fresh lead so the expected badge state is predictable.
 */
