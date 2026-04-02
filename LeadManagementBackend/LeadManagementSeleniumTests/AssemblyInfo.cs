// xUnit is the testing framework we use to run our tests
using Xunit;

// This tells xUnit to run tests ONE AT A TIME (not in parallel).
// Selenium tests share a single browser, so running them at the same time would cause conflicts.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

/*
 * FILE SUMMARY:
 * AssemblyInfo.cs contains project-wide settings that apply to ALL test files in this project.
 * Its main job here is to disable parallel test execution so that Selenium tests don't
 * fight over the same browser instance. Without this, multiple tests might try to click
 * buttons or navigate pages at the same time, causing random failures.
 */
