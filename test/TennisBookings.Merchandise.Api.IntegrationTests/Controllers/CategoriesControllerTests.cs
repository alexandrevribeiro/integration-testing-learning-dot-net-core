using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TennisBookings.Merchandise.Api.IntegrationTests.Models;
using TennisBookings.Merchandise.Api.IntegrationTests.TestHelpers.Serialization;
using Xunit;

namespace TennisBookings.Merchandise.Api.IntegrationTests.Controllers
{
    public class CategoriesControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _httpClient;

        public CategoriesControllerTests(WebApplicationFactory<Startup> factory)
        {
            // Centralizing the base API endpoint address

            // Option 1) Use "CreateDefaultClient" providing the base address
            // _httpClient = factory.CreateDefaultClient(new Uri("http://localhost/api/categories"));

            // Option 2) Use "CreateHttpClient" (which follows redirects and handles cookies), and is configured
            // by using "factory.ClientOptions"
            factory.ClientOptions.BaseAddress = new Uri("http://localhost/api/categories");
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedJson()
        {
            var expectedCategories = new List<string> { "Accessories", "Bags", "Balls", "Clothing", "Rackets" };

            // By using the "System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<>" extension method,
            // it already covers the following cases:
            // - Uses case-insensitive deserialization 
            // - Validates response has success status code
            // - Validates Content-Type header
            // - Validates response includes content

            var model = await _httpClient.GetFromJsonAsync<ExpectedCategoriesModel>("");

            Assert.NotNull(model?.AllowedCategories);
            Assert.Equal(expectedCategories.OrderBy(c => c), model.AllowedCategories.OrderBy(c => c));
        }

        #region Previous 4 tests simplified into the new "GetAll_ReturnsExpectedJson" above:
        /* 

        [Fact]
        public async Task GetAll_ReturnsSuccessStatusCode()
        {
            var response = await _httpClient.GetAsync("");

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedMediaType()
        {
            var response = await _httpClient.GetAsync("");

            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task GetAll_ReturnsContent()
        {
            var response = await _httpClient.GetAsync("");

            Assert.NotNull(response.Content);
            Assert.True(response.Content.Headers.ContentLength > 0);
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedJson()
        {
            var expectedCategories = new List<string> { "Accessories", "Bags", "Balls", "Clothing", "Rackets" };

            var responseStream = await _httpClient.GetStreamAsync("");

            var model = await JsonSerializer.DeserializeAsync<ExpectedCategoriesModel>(responseStream,
                JsonSerializerHelper.DefaultDeserialisationOptions);

            Assert.NotNull(model?.AllowedCategories);
            Assert.Equal(expectedCategories.OrderBy(c => c), model.AllowedCategories.OrderBy(c => c));
        } 
        
        */
        #endregion
    }
}
