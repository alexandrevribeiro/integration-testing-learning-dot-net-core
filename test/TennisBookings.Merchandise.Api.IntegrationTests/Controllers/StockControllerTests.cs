using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TennisBookings.Merchandise.Api.Data.Dto;
using TennisBookings.Merchandise.Api.External.Database;
using TennisBookings.Merchandise.Api.IntegrationTests.Fakes;
using TennisBookings.Merchandise.Api.IntegrationTests.Models;
using Xunit;

namespace TennisBookings.Merchandise.Api.IntegrationTests.Controllers
{
    public class StockControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _httpClient;
        private readonly WebApplicationFactory<Startup> _factory;

        public StockControllerTests(WebApplicationFactory<Startup> factory)
        {
            factory.ClientOptions.BaseAddress = new Uri("http://localhost/api/stock/");
            _httpClient = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task GetStockTotal_ReturnsExpectedJson()
        {
            var model = await _httpClient.GetFromJsonAsync<ExpectedStockTotalOutputModel>("total");

            Assert.NotNull(model);
            Assert.True(model.StockItemTotal > 0);            
        }

        [Fact]
        public async Task GetStockTotal_ReturnsExpectedStockQuantity()
        {
            var cloudDatabase = new FakeCloudDatabase(new []
            {
                new ProductDto { StockCount = 200 },
                new ProductDto { StockCount = 500 },
                new ProductDto { StockCount = 300 }
            });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<ICloudDatabase>(cloudDatabase);
                });
            }).CreateClient();

            var model = await client.GetFromJsonAsync<ExpectedStockTotalOutputModel>("total");

            Assert.Equal(1000, model.StockItemTotal);
        }

        #region Previous tests simplified into the new "GetStockTotal_ReturnsExpectedJson" above:

        /*
        [Fact]
        public async Task GetStockTotal_ReturnsSuccessStatusCode()
        {
            var response = await _httpClient.GetAsync("total");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetStockTotal_ReturnsExpectedJsonContentString()
        {
            var response = await _httpClient.GetStringAsync("total");

            Assert.Equal("{\"stockItemTotal\":100}", response);
        }

        [Fact]
        public async Task GetStockTotal_ReturnsExpectedContentType()
        {
            var response = await _httpClient.GetAsync("total");

            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        }
         */
        #endregion
    }
}
