using FluentAssertions;
using LeadManagementSeleniumTests.Fixtures;
using OpenQA.Selenium;

namespace LeadManagementSeleniumTests;

public class LeadFormTests : IClassFixture<SeleniumFixture>
{
    private readonly SeleniumFixture _fixture;
    private static bool _loggedIn;

    public LeadFormTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        if (!_loggedIn)
        {
            _fixture.LoginAsAdmin();
            _loggedIn = true;
        }
    }

    [Fact]
    public void LeadForm_Renders_AllFieldsAndSubmitButton()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");

        // Verify all form input fields are present
        var nameField = _fixture.WaitForElement(By.CssSelector("input[name='name']"));
        nameField.Should().NotBeNull();
        nameField.Displayed.Should().BeTrue();

        var emailField = _fixture.Driver.FindElement(By.CssSelector("input[name='email']"));
        emailField.Displayed.Should().BeTrue();

        var phoneField = _fixture.Driver.FindElement(By.CssSelector("input[name='phone']"));
        phoneField.Displayed.Should().BeTrue();

        var companyField = _fixture.Driver.FindElement(By.CssSelector("input[name='company']"));
        companyField.Displayed.Should().BeTrue();

        var positionField = _fixture.Driver.FindElement(By.CssSelector("input[name='position']"));
        positionField.Displayed.Should().BeTrue();

        // Verify dropdowns
        var sourceSelect = _fixture.Driver.FindElement(By.CssSelector("select[name='source']"));
        sourceSelect.Displayed.Should().BeTrue();

        var prioritySelect = _fixture.Driver.FindElement(By.CssSelector("select[name='priority']"));
        prioritySelect.Displayed.Should().BeTrue();

        // Verify submit button
        var submitButton = _fixture.Driver.FindElement(By.CssSelector("button[type='submit']"));
        submitButton.Displayed.Should().BeTrue();
        submitButton.Text.Should().Contain("Save Lead");
    }

    [Fact]
    public void LeadForm_SourceDropdown_ContainsExpectedOptions()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("select[name='source']"));

        var sourceSelect = _fixture.Driver.FindElement(By.CssSelector("select[name='source']"));
        var options = sourceSelect.FindElements(By.TagName("option"))
            .Select(o => o.Text).ToList();

        options.Should().Contain("Website");
        options.Should().Contain("Referral");
        options.Should().Contain("ColdCall");
        options.Should().Contain("Event");
        options.Should().Contain("Partner");
    }

    [Fact]
    public void LeadForm_PriorityDropdown_ContainsExpectedOptions()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("select[name='priority']"));

        var prioritySelect = _fixture.Driver.FindElement(By.CssSelector("select[name='priority']"));
        var options = prioritySelect.FindElements(By.TagName("option"))
            .Select(o => o.Text).ToList();

        options.Should().Contain("Low");
        options.Should().Contain("Medium");
        options.Should().Contain("High");
    }

    [Fact]
    public void LeadForm_ShowsValidationErrors_WhenSubmittedEmpty()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("button[type='submit']"));

        var submitButton = _fixture.Driver.FindElement(By.CssSelector("button[type='submit']"));
        submitButton.Click();

        // Validation error messages should appear (red text)
        var errorMessages = _fixture.Driver.FindElements(By.CssSelector("p.text-red-500"));
        errorMessages.Count.Should().BeGreaterThan(0, "validation errors should appear for required fields");
    }

    [Fact]
    public void LeadForm_Labels_AreVisible()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        var labels = _fixture.Driver.FindElements(By.CssSelector("label"))
            .Select(l => l.Text.ToUpper()).ToList();

        labels.Should().Contain(l => l.Contains("NAME"));
        labels.Should().Contain(l => l.Contains("EMAIL"));
        labels.Should().Contain(l => l.Contains("SOURCE"));
        labels.Should().Contain(l => l.Contains("PRIORITY"));
    }
}
