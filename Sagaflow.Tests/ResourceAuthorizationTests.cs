using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleMvcExample;
using SimpleMvcExample.Areas.Identity.Data;

namespace Sagaflow.Tests;

[TestFixture]
public class Tests
{
    private TestScaffold _testScaffold;
    private TestWebApplicationFactory _webFactory;

    [SetUp]
    public void Setup()
    {
        _webFactory = new TestWebApplicationFactory();

        _testScaffold = new TestScaffold().UseIoc();
        //     .Given("I have a WebHost of the SimpleMvcExample with SagaFlow configured", ts =>
        //     {
        //         // Jobs Resource are resolved on startup of the WebApplication
        //         // ts.WithWebApplicationFactory<TestWebApplicationFactory, Program>(_webFactory);
        //
        //     })
        //     .And("the following user has been registered into the system", t =>
        //     {
        //         using var scope = t.ServiceProvider.CreateScope();
        //         var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        //
        //         var email = "peter.parker@email.com";
        //         var user = new ApplicationUser()
        //         {
        //             Id = Guid.NewGuid(),
        //             Name = "Peter Parker",
        //             UserName = email,
        //             Email = email,
        //             Password = "Password123!"
        //         };
        //
        //         var result = userManager.CreateAsync(user, user.Password).Result; 
        //         
        //         if(result.Succeeded)
        //             t.TestScaffoldContext.Set(user, "User");
        //     });
    }

    [TestCase("Admin", HttpStatusCode.OK, TestName ="I should receive an Ok response when using an authorized role")]
    [TestCase("Member", HttpStatusCode.Forbidden, TestName ="I should receive an Ok response when using an unauthorized role")]
    public void JobsController_WhenAuthorizingResourceByRole_ReturnsCorrectStatusCode(string role, HttpStatusCode expectedStatusCode)
    {
        _testScaffold
            .Given($"I am authenticated as a user with the role {role}", ts =>
            {
                var httpClient = _webFactory.WithWebHostBuilder(b => b.ConfigureServices(s =>
                {
                    s.AddAuthentication("Test")
                        .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options =>
                        {
                            options.Role = role;
                        });
                })).CreateClient();
                ts.TestScaffoldContext.Set(httpClient);

            })
            .When("I request the jobs resource", ts =>
            {
                var httpClient = ts.TestScaffoldContext.Get<HttpClient>();
                var response = httpClient.GetAsync("/api/jobs", CancellationToken.None).Result;
                ts.TestScaffoldContext.Set(response);
            })
            .Then($"I should receive an {expectedStatusCode} response", ts =>
            {
                var response = ts.TestScaffoldContext.Get<HttpResponseMessage>();
                response.StatusCode.Should().Be(expectedStatusCode);
            });
    }
    
    [Test]
    public void JobsController_WhenRequestingJobsWithAllowedRole_ReturnsOkWithResults_()
    {
        _testScaffold
            .Given("I am authenticated as a user with the restricted role", _ => throw new NotImplementedException())
            .When("I request the jobs resource", _ => throw new NotImplementedException())
            .Then("I should receive an unauthorized response", _ => throw new NotImplementedException());
    }
    
    
    public void GivenTheUserHasLoggedIn(TestScaffold testScaffold)
    {
        var user = testScaffold.TestScaffoldContext.Get<ApplicationUser?>("User");
        using var scope = testScaffold.ServiceProvider.CreateScope();

        var httpClient = testScaffold.GetWebApplicationHttpClient<TestWebApplicationFactory, Program>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        var usr = userStore.FindByNameAsync(user.Email.ToUpper(), CancellationToken.None).Result;
        var _signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        
        var signInResult = _signInManager.PasswordSignInAsync(
            user.Email, 
            user.Password,
            false, 
            lockoutOnFailure: false).Result;

        
        
        var response = httpClient.PostAsJsonAsync<LoginRequest>(
            "/Identity/Account/Login",
            new LoginRequest()
            {
                Email = user.Email, 
                Password = user.Password
            });
        
        var result = response.Result;
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}