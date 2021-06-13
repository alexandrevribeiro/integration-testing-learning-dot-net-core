using System;
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
    public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public ProductsControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            factory.ClientOptions.BaseAddress = new Uri("http://localhost/api/products/");

            _client = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_ReturnsExpectedArrayOfProducts()
        {
            _factory.FakeCloudDatabase.ResetDefaultProducts(useCustomIfAvailable: false);

            var products = await _client.GetFromJsonAsync<ExpectedProductModel[]>("");

            Assert.NotNull(products);
            Assert.Equal(_factory.FakeCloudDatabase.Products.Count, products.Count());
        }

        [Fact]
        public async Task Get_ReturnsExpectedProduct()
        {
            var firstProduct = _factory.FakeCloudDatabase.Products.First();

            var product = await _client.GetFromJsonAsync<ExpectedProductModel>($"{firstProduct.Id}");

            Assert.NotNull(product);
            Assert.Equal(firstProduct.Name, product.Name);
        }
    }
}
