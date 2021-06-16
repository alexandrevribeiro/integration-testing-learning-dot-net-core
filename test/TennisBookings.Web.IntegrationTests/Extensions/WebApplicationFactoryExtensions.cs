using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using TennisBookings.Web.Data;
using TennisBookings.Web.IntegrationTests.Pages;

namespace TennisBookings.Web.IntegrationTests.Extensions
{
    public static class WebApplicationFactoryExtensions
    {
        public static HttpClient CreateClientWithMemberAndDbSetup<TLogCategoryName>(this WebApplicationFactory<Startup> factory,
            Action<TennisBookingDbContext> configure)
        {
            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.WithMemberUser().ConfigureTestDatabase<TLogCategoryName>(configure);
            }).CreateClient();

            // (Optional) We can also add an authorization header to the default headers for the
            // test client so that each request specifies the name of the Test scheme.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            return client;
        }
    }
}

