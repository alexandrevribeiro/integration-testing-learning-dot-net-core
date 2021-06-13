using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TennisBookings.Merchandise.Api.Data;
using TennisBookings.Merchandise.Api.Data.Dto;
using TennisBookings.Merchandise.Api.External.Database;
using TennisBookings.Merchandise.Api.IntegrationTests.Fakes;
using TennisBookings.Merchandise.Api.IntegrationTests.Models;
using TennisBookings.Merchandise.Api.IntegrationTests.TestHelpers.Serialization;
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

        [Fact]        
        public async Task Post_WithoutName_ReturnsBadRequest()
        {
            var productInputModel = GetValidProductInputModel().CloneWith(p => p.Name = null);

            var response = await _client.PostAsJsonAsync("", productInputModel, JsonSerializerHelper.DefaultSerialisationOptions);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_WithInvalidName_ReturnsExpectedProblemDetails()
        {
            var productInputModel = GetValidProductInputModel().CloneWith(p => p.Name = null);

            var response = await _client.PostAsJsonAsync("", productInputModel, JsonSerializerHelper.DefaultSerialisationOptions);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.Collection(problemDetails.Errors, kvp =>
            {
                Assert.Equal("Name", kvp.Key);
                var error = Assert.Single(kvp.Value);
                Assert.Equal("The Name field is required.", error);
            });
        }

        #region Helping methods

        private static TestProductInputModel GetValidProductInputModel(Guid? id = null)
        {
            return new TestProductInputModel
            {
                Id = id is object ? id.Value.ToString() : Guid.NewGuid().ToString(),
                Name = "Some Product",
                Description = "This is a description",
                Category = new CategoryProvider().AllowedCategories().First(),
                InternalReference = "ABC123",
                Price = 4.00m
            };
        }

        #endregion
    }
}
