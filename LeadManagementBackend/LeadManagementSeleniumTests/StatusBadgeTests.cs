using FluentAssertions;
using LeadManagementSeleniumTests.Fixtures;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests;

public class StatusBadgeTests : IClassFixture<SeleniumFixture>
{
    private readonly SeleniumFixture _fixture;
    private static bool _loggedIn;

    public StatusBadgeTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        if (!_loggedIn)
        {
            _fixture.LoginAsAdmin();
            _loggedIn = true;
        }
    }

    [Fact]
    public void StatusBadge_RendersCorrectly_OnLeadListPage()
    {
        CreateTestLead("StatusBadge List Lead", $"statuslist-{Guid.NewGuid():N}@test.com", "Website", "High");

        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var badges = _fixture.Driver.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"));
        badges.Count.Should().BeGreaterThan(0, "at least one status badge should be displayed");

        var badgeTexts = badges.Select(b => b.Text.Trim()).ToList();
        badgeTexts.Should().Contain("New", "newly created lead should show 'New' status badge");
    }

    [Fact]
    public void StatusBadge_DisplaysOnDetailPage()
    {
        CreateTestLead("BadgeDetail Lead", $"badgedetail-{Guid.NewGuid():N}@test.com", "Referral", "Medium");

        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var leadLink = _fixture.WaitForElement(By.XPath("//a[contains(text(), 'BadgeDetail Lead')]"));
        leadLink.Click();

        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains("BadgeDetail Lead")));

        var badge = _fixture.Driver.FindElement(By.CssSelector("span.inline-flex.items-center.rounded-full"));
        badge.Text.Trim().Should().Be("New");

        var dot = badge.FindElement(By.CssSelector("span.rounded-full"));
        dot.Displayed.Should().BeTrue();
    }

    [Fact]
    public void StatusBadge_UpdatesAfterStatusChange()
    {
        CreateTestLead("StatusChange Lead", $"statuschange-{Guid.NewGuid():N}@test.com", "Event", "Low");

        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads");
        _fixture.WaitForElement(By.CssSelector("table"));

        var leadLink = _fixture.WaitForElement(By.XPath("//a[contains(text(), 'StatusChange Lead')]"));
        leadLink.Click();

        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("h1")).Any(h => h.Text.Contains("StatusChange Lead")));

        var transitionButton = _fixture.WaitForElement(By.XPath("//button[contains(text(), 'Move to Contacted')]"));
        transitionButton.Click();

        wait.Until(d =>
        {
            var badges = d.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"));
            return badges.Any(b => b.Text.Trim() == "Contacted");
        });

        var updatedBadge = _fixture.Driver.FindElements(By.CssSelector("span.inline-flex.items-center.rounded-full"))
            .First(b => b.Text.Trim() == "Contacted");
        updatedBadge.Should().NotBeNull();
    }

    private void CreateTestLead(string name, string email, string source, string priority)
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        _fixture.Driver.FindElement(By.CssSelector("input[name='name']")).SendKeys(name);
        _fixture.Driver.FindElement(By.CssSelector("input[name='email']")).SendKeys(email);
        _fixture.Driver.FindElement(By.CssSelector("input[name='phone']")).SendKeys("555-0100");
        _fixture.Driver.FindElement(By.CssSelector("input[name='company']")).SendKeys("Test Corp");

        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='source']"))).SelectByText(source);
        new SelectElement(_fixture.Driver.FindElement(By.CssSelector("select[name='priority']"))).SelectByText(priority);

        _fixture.Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // App redirects to /leads list page after creation
        var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
        {
            var url = d.Url.TrimEnd('/');
            return url.EndsWith("/leads");
        });
    }
}
