using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;
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

        public static IEnumerable<object[]> ConfigVariations => new List<object[]>
        {
            // 1st param: Config "Features > WeatherForecasting > EnableWeatherForecast"
            // 2nd param: Config "Features > HomePage > EnableWeatherForecast"
            // 3rd param: whether the "Weather Forecast" should be displayed or not
            new object[] { false, false, false },
            new object[] { true, false, false },
            new object[] { false, true, false },
            new object[] { true, true, true }
        };

        [Theory]
        [MemberData(nameof(ConfigVariations))]
        public async Task HomePageIncludesForecast_ForExpectedConfigVariations(bool globalEnabled,
            bool pageEnabled, bool shouldDisplayForecast)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "Features:WeatherForecasting:EnableWeatherForecast", globalEnabled.ToString() },
                        { "Features:HomePage:EnableWeatherForecast", pageEnabled.ToString() }
                    });
                });
            }).CreateClient();

            var response = await client.GetAsync("/");
            using var content = await HtmlHelpers.GetDocumentAsync(response);

            var forecastDiv = content.All.SingleOrDefault(e => e.Id == "weather-forecast" &&
                e.LocalName == TagNames.Div);

            Assert.Equal(shouldDisplayForecast, forecastDiv != null);
        }
    }
}
