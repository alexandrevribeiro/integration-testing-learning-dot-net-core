using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace TennisBookings.Web.IntegrationTests.Pages
{
    public class GeneralPageTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public GeneralPageTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        public static IEnumerable<object[]> ValidUrls => new List<object[]>
        {
            new object[] { "/" },
            new object[] { "/Index" },
            new object[] { "/About" },
            new object[] { "/Enquiry" },
        };

        [Theory]
        [MemberData(nameof(ValidUrls))]
        public async Task ValidUrls_ReturnsSuccessAndExpectedContentType(string path)
        {
            var expectedMediatype = new MediaTypeHeaderValue("text/html");
            var client = _factory.CreateClient();

            var response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();

            Assert.Equal(expectedMediatype.MediaType, response.Content.Headers.ContentType.MediaType);
        }
    }
}
