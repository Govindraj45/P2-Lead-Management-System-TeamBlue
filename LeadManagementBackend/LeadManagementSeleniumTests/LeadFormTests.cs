// FluentAssertions gives us readable checks like ".Should().BeTrue()"
using FluentAssertions;
// SeleniumFixture is our shared Chrome browser setup
using LeadManagementSeleniumTests.Fixtures;
// Selenium library to find elements on a web page
using OpenQA.Selenium;

namespace LeadManagementSeleniumTests;

// This test class checks that the "Create Lead" form has all the right fields and behaves correctly
public class LeadFormTests : IClassFixture<SeleniumFixture>
{
    // Store a reference to the shared browser fixture
    private readonly SeleniumFixture _fixture;
    // Track whether we already logged in
    private static bool _loggedIn;

    // Constructor: logs in as Admin once before all tests in this class
    public LeadFormTests(SeleniumFixture fixture)
    {
        _fixture = fixture;
        if (!_loggedIn)
        {
            _fixture.LoginAsAdmin();
            _loggedIn = true;
        }
    }

    // TEST: Make sure the Create Lead form shows all input fields and the Submit button
    [Fact]
    public void LeadForm_Renders_AllFieldsAndSubmitButton()
    {
        // Go to the Create Lead page
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");

        // Check that the Name field exists and is visible
        var nameField = _fixture.WaitForElement(By.CssSelector("input[name='name']"));
        nameField.Should().NotBeNull();
        nameField.Displayed.Should().BeTrue();

        // Check that the Email field is visible
        var emailField = _fixture.Driver.FindElement(By.CssSelector("input[name='email']"));
        emailField.Displayed.Should().BeTrue();

        // Check that the Phone field is visible
        var phoneField = _fixture.Driver.FindElement(By.CssSelector("input[name='phone']"));
        phoneField.Displayed.Should().BeTrue();

        // Check that the Company field is visible
        var companyField = _fixture.Driver.FindElement(By.CssSelector("input[name='company']"));
        companyField.Displayed.Should().BeTrue();

        // Check that the Position field is visible
        var positionField = _fixture.Driver.FindElement(By.CssSelector("input[name='position']"));
        positionField.Displayed.Should().BeTrue();

        // Check that the Source dropdown is visible
        var sourceSelect = _fixture.Driver.FindElement(By.CssSelector("select[name='source']"));
        sourceSelect.Displayed.Should().BeTrue();

        // Check that the Priority dropdown is visible
        var prioritySelect = _fixture.Driver.FindElement(By.CssSelector("select[name='priority']"));
        prioritySelect.Displayed.Should().BeTrue();

        // Check that the Submit button is visible and says "Save Lead"
        var submitButton = _fixture.Driver.FindElement(By.CssSelector("button[type='submit']"));
        submitButton.Displayed.Should().BeTrue();
        submitButton.Text.Should().Contain("Save Lead");
    }

    // TEST: Make sure the Source dropdown has the correct options (Website, Referral, etc.)
    [Fact]
    public void LeadForm_SourceDropdown_ContainsExpectedOptions()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("select[name='source']"));

        // Get all the options from the Source dropdown
        var sourceSelect = _fixture.Driver.FindElement(By.CssSelector("select[name='source']"));
        var options = sourceSelect.FindElements(By.TagName("option"))
            .Select(o => o.Text).ToList();

        // Verify each expected source option is present
        options.Should().Contain("Website");
        options.Should().Contain("Referral");
        options.Should().Contain("ColdCall");
        options.Should().Contain("Event");
        options.Should().Contain("Partner");
    }

    // TEST: Make sure the Priority dropdown has Low, Medium, and High options
    [Fact]
    public void LeadForm_PriorityDropdown_ContainsExpectedOptions()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("select[name='priority']"));

        // Get all the options from the Priority dropdown
        var prioritySelect = _fixture.Driver.FindElement(By.CssSelector("select[name='priority']"));
        var options = prioritySelect.FindElements(By.TagName("option"))
            .Select(o => o.Text).ToList();

        // Verify each expected priority option is present
        options.Should().Contain("Low");
        options.Should().Contain("Medium");
        options.Should().Contain("High");
    }

    // TEST: Click Submit without filling anything in — validation errors should appear
    [Fact]
    public void LeadForm_ShowsValidationErrors_WhenSubmittedEmpty()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("button[type='submit']"));

        // Click Submit without entering any data
        var submitButton = _fixture.Driver.FindElement(By.CssSelector("button[type='submit']"));
        submitButton.Click();

        // Look for red error messages on the page (the "p.text-red-500" CSS class)
        var errorMessages = _fixture.Driver.FindElements(By.CssSelector("p.text-red-500"));
        errorMessages.Count.Should().BeGreaterThan(0, "validation errors should appear for required fields");
    }

    // TEST: Verify that the form shows labels for Name, Email, Source, and Priority
    [Fact]
    public void LeadForm_Labels_AreVisible()
    {
        _fixture.Driver.Navigate().GoToUrl($"{SeleniumFixture.BaseUrl}/leads/create");
        _fixture.WaitForElement(By.CssSelector("input[name='name']"));

        // Collect all label text on the page (converted to uppercase for easy matching)
        var labels = _fixture.Driver.FindElements(By.CssSelector("label"))
            .Select(l => l.Text.ToUpper()).ToList();

        // Check that key labels exist
        labels.Should().Contain(l => l.Contains("NAME"));
        labels.Should().Contain(l => l.Contains("EMAIL"));
        labels.Should().Contain(l => l.Contains("SOURCE"));
        labels.Should().Contain(l => l.Contains("PRIORITY"));
    }
}

/*
 * FILE SUMMARY:
 * LeadFormTests checks that the "Create Lead" form on the frontend renders correctly.
 * It verifies all input fields (name, email, phone, company, position) are visible,
 * that dropdown menus contain the right options, that labels are displayed, and that
 * submitting an empty form triggers validation error messages. These tests ensure the
 * form UI matches what the backend expects.
 */
