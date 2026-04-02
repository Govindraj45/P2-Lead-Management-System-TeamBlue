// These libraries let us control a Chrome browser from C# code
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace LeadManagementSeleniumTests.Fixtures;

// This class sets up a Chrome browser that all our Selenium tests share.
// "IDisposable" means it will clean up the browser when tests are done.
public class SeleniumFixture : IDisposable
{
    // The "Driver" is our remote control for the Chrome browser
    public IWebDriver Driver { get; }
    // The web address where our frontend app is running
    public const string BaseUrl = "http://localhost:80";
    // The web address where our backend API is running
    public const string ApiUrl = "http://localhost:5000";

    // This constructor runs once to set up Chrome before any tests start
    public SeleniumFixture()
    {
        // Configure Chrome to run in "headless" mode (no visible window, runs in the background)
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        // These flags make Chrome run smoothly in test/CI environments
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-background-networking");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-sync");
        options.AddArgument("--disable-translate");
        // Set the browser window size so elements are positioned consistently
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--remote-allow-origins=*");

        // Start the Chrome browser with our settings
        Driver = new ChromeDriver(options);
        // Wait up to 10 seconds when looking for an element on the page
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        // Wait up to 60 seconds for a page to fully load
        Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
    }

    // Log in to the app as a specific user by filling in the login form
    public void LoginAs(string email, string password)
    {
        // Go to the login page and clear any old login data from the browser
        Driver.Navigate().GoToUrl($"{BaseUrl}/login");
        try { ((IJavaScriptExecutor)Driver).ExecuteScript("localStorage.clear();"); } catch { }
        Driver.Navigate().GoToUrl($"{BaseUrl}/login");

        // Wait until the email input field appears on the page
        WaitForElement(By.CssSelector("input[type='email']"));

        // Find the email, password, and submit button on the login form
        var emailField = Driver.FindElement(By.CssSelector("input[type='email']"));
        var passwordField = Driver.FindElement(By.CssSelector("input[type='password']"));
        var loginButton = Driver.FindElement(By.CssSelector("button[type='submit']"));

        // Type the email and password into the form, then click the login button
        emailField.Clear();
        emailField.SendKeys(email);
        passwordField.Clear();
        passwordField.SendKeys(password);
        loginButton.Click();

        // Wait until the browser navigates to the /leads page (meaning login succeeded)
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
        wait.Until(d => d.Url.Contains("/leads"));
    }

    // Shortcut methods to log in as each of the three user roles
    public void LoginAsAdmin() => LoginAs("admin@leadcrm.com", "Admin@123");
    public void LoginAsManager() => LoginAs("manager@leadcrm.com", "Manager@123");
    public void LoginAsRep() => LoginAs("rep@leadcrm.com", "Rep@123");

    // Wait for a specific element to appear and be visible on the page, then return it
    public IWebElement WaitForElement(By by, int timeoutSeconds = 15)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var el = d.FindElement(by);
            return el.Displayed ? el : null;
        })!;
    }

    // Check if an element exists and is visible on the page (returns true/false)
    public bool ElementExists(By by)
    {
        try
        {
            // Use a short 2-second wait so this check is fast
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            var elements = Driver.FindElements(by);
            return elements.Count > 0 && elements[0].Displayed;
        }
        finally
        {
            // Reset the wait time back to the normal 10 seconds
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }
    }

    // Close and clean up the Chrome browser when all tests are finished
    public void Dispose()
    {
        try { Driver.Quit(); } catch { }
        try { Driver.Dispose(); } catch { }
    }
}

/*
 * FILE SUMMARY:
 * SeleniumFixture is the shared setup class for all Selenium browser tests.
 * It launches a headless Chrome browser, provides helper methods to log in as
 * different user roles (Admin, Manager, SalesRep), and offers utilities to wait
 * for elements or check if they exist on the page. Every test class uses this
 * fixture so they don't each have to start their own browser from scratch.
 */
