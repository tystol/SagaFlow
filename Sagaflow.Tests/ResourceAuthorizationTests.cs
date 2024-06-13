using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.Core;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Sagaflow.Tests;

public class Tests
{
    private TestScaffold _testScaffold;
    
    [SetUp]
    public void Setup()
    {
        var webFactory = new TestWebApplicationFactory();
    
        _testScaffold = new TestScaffold()
            .WithWebApplicationFactory<TestWebApplicationFactory, Program>(webFactory);

        _testScaffold
            .Given("I have the users", _ => throw new NotImplementedException())
            .And("I have a jobs resource provider", _ => throw new NotImplementedException())
            .And("I have the jobs", _ => throw new NotImplementedException())
            .And("The jobs resource provider has a restricted role", _ => throw new NotImplementedException());
    }

    [Test]
    public void JobsController_WhenRequestingJobsWithRestrictedRole_ReturnsUnAuthorized_()
    {
        _testScaffold
            .Given("I am authenticated as a user with the restricted role", _ => throw new NotImplementedException())
            .When("I request the jobs resource", _ => throw new NotImplementedException())
            .Then("I should receive an unauthorized response", _ => throw new NotImplementedException());
    }
}