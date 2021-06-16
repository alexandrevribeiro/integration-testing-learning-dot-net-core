using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TennisBookings.Web.IntegrationTests.Helpers;
using Xunit;

namespace TennisBookings.Web.IntegrationTests.Controllers
{
    public class AdminHomeControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public AdminHomeControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            // Disabling the default "redirect" behavior (which is following any rediorects
            // until it gets to a final resource), for the Test to assert on the response itself
            factory.ClientOptions.AllowAutoRedirect = false;            
            _factory = factory;
        }

        [Fact]
        public async Task Get_SecurePageIsForbiddenForAnUnauthenticatedUser()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/Admin");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("http://localhost/identity/account/login", response.Headers.Location.OriginalString.ToLower());
        }

        public static IEnumerable<object[]> RoleAccess => new List<object[]>
        {
            // 1st: The user role
            // 2nd: The expected response status code
            new object[] { "Member", HttpStatusCode.Forbidden },
            new object[] { "Admin", HttpStatusCode.OK }
        };

        [Theory]
        [MemberData(nameof(RoleAccess))]
        public async Task Get_SecurePageAccessibleOnlyByAdminUsers(string role, HttpStatusCode expected)
        {
            // Registering the Test Authentication Schema to be used in the request
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test",
                            options => options.Role = role);
                });
            }).CreateClient();

            // (Optional) We can also add an authorization header to the default headers for the
            // test client so that each request specifies the name of the Test scheme.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.GetAsync("/Admin");

            Assert.Equal(expected, response.StatusCode);
        }
    }
}
