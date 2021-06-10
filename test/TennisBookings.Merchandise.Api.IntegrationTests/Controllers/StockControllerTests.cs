using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TennisBookings.Merchandise.Api.IntegrationTests.Controllers
{
    public class StockControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _httpClient;

        public StockControllerTests(WebApplicationFactory<Startup> factory)
        {
            //factory.ClientOptions.BaseAddress = new Uri("http://localhost/api/stock");

            _httpClient = factory.CreateDefaultClient(new Uri("http://localhost/api/stock/"));
        }

        [Fact]
        public async Task GetStockTotal_ReturnsSuccessStatusCode()
        {
            var response = await _httpClient.GetAsync("total");
            response.EnsureSuccessStatusCode();
        }
    }
}
