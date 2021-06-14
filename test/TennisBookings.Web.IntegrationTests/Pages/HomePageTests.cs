using System.Threading.Tasks;
using TennisBookings.Web.IntegrationTests.Helpers;
using Xunit;

namespace TennisBookings.Web.IntegrationTests.Pages
{
    public class HomePageTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public HomePageTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }        

        [Fact]
        public async Task Get_ReturnsPageWithExpectedH1()
        {
            // IMPORTANT: This kind of tests comparing HTML should only be used for
            // HTML elements that are supposed to be rarely changed

            var client = _factory.CreateClient();
            var response = await client.GetAsync("/");

            response.EnsureSuccessStatusCode();

            using var content = await HtmlHelpers.GetDocumentAsync(response);

            var h1 = content.QuerySelector("h1");

            Assert.Equal("Welcome to Tennis by the Sea!", h1.TextContent);
        }
    }
}
