using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests.Fixtures;

public class SeleniumFixture : IDisposable
{
    public IWebDriver Driver { get; }
    public const string BaseUrl = "http://localhost:80";
    public const string ApiUrl = "http://localhost:5000";

    public SeleniumFixture()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-background-networking");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-sync");
        options.AddArgument("--disable-translate");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--remote-allow-origins=*");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
    }

    public void LoginAs(string email, string password)
    {
        // Clear storage to ensure clean login state
        Driver.Navigate().GoToUrl($"{BaseUrl}/login");
        try { ((IJavaScriptExecutor)Driver).ExecuteScript("localStorage.clear();"); } catch { }
        Driver.Navigate().GoToUrl($"{BaseUrl}/login");

        WaitForElement(By.CssSelector("input[type='email']"));

        var emailField = Driver.FindElement(By.CssSelector("input[type='email']"));
        var passwordField = Driver.FindElement(By.CssSelector("input[type='password']"));
        var loginButton = Driver.FindElement(By.CssSelector("button[type='submit']"));

        emailField.Clear();
        emailField.SendKeys(email);
        passwordField.Clear();
        passwordField.SendKeys(password);
        loginButton.Click();

        // Wait for redirect to /leads
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
        wait.Until(d => d.Url.Contains("/leads"));
    }

    public void LoginAsAdmin() => LoginAs("admin@leadcrm.com", "Admin@123");
    public void LoginAsManager() => LoginAs("manager@leadcrm.com", "Manager@123");
    public void LoginAsRep() => LoginAs("rep@leadcrm.com", "Rep@123");

    public IWebElement WaitForElement(By by, int timeoutSeconds = 15)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var el = d.FindElement(by);
            return el.Displayed ? el : null;
        })!;
    }

    public bool ElementExists(By by)
    {
        try
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            var elements = Driver.FindElements(by);
            return elements.Count > 0 && elements[0].Displayed;
        }
        finally
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }
    }

    public void Dispose()
    {
        try { Driver.Quit(); } catch { }
        try { Driver.Dispose(); } catch { }
    }
}
