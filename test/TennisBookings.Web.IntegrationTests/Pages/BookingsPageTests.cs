using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TennisBookings.Web.Data;
using TennisBookings.Web.IntegrationTests.Extensions;
using TennisBookings.Web.IntegrationTests.Helpers;
using Xunit;

namespace TennisBookings.Web.IntegrationTests.Pages
{
    public class BookingsPageTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public BookingsPageTests(CustomWebApplicationFactory<Startup> factory)
        {
            factory.ClientOptions.BaseAddress = new Uri("http://localhost/bookings");
            _factory = factory;
        }

        [Fact]
        public async Task NoBookingsTableOnPage_WhenUserHasNoBookings()
        {
            // Creates a client registering a Member (Test Authentication Schema) and
            // setting up the DbContext to reset the DB for this Test
            var client = _factory.CreateClientWithMemberAndDbSetup<BookingsPageTests>(db => 
                DatabaseHelper.ResetDbForTests(db));

           var response = await client.GetAsync("");

            response.AssertOk();

            using var content = await HtmlHelpers.GetDocumentAsync(response);
            var table = content.QuerySelector("table");

            Assert.Null(table);
        }

        [Fact]
        public async Task ExpectedBookingsTableRowsOnPage_WhenUserHasBooking()
        {
            var startDate = FixedDateTime.UtcNow.AddDays(5);

            // Creates a client registering a Member (Test Authentication Schema) and
            // setting up the DbContext for this Test
            var client = _factory.CreateClientWithMemberAndDbSetup<BookingsPageTests>(db =>
            {
                DatabaseHelper.ResetDbForTests(db);

                var member = db.Members.Find(1);
                var court = db.Courts.Find(1);

                db.CourtBookings.Add(new CourtBooking
                {
                    Court = court,
                    Member = member,
                    StartDateTime = startDate,
                    EndDateTime = startDate.AddHours(2)
                });

                db.SaveChanges();
            });

            var response = await client.GetAsync("");

            response.AssertOk();

            using var content = await HtmlHelpers.GetDocumentAsync(response);
            var table = content.QuerySelector("table");
            var tableBody = table.QuerySelector("tbody");
            var rows = tableBody.QuerySelectorAll("tr");

            Assert.Single(rows);

            // This following approach provides more asurance of the content provided in the oage,
            // BUT it's more coupled to the HTML output, which may require more work to maintain
            // So it's a tradeoff we're making
            Assert.Collection(rows, r =>
            {
                var cells = r.QuerySelectorAll("td");

                Assert.Equal(startDate.ToString("D"), cells[0].TextContent);
                Assert.Equal("Court 1", cells[1].TextContent);
                Assert.Equal("10 am to 12 pm", cells[2].TextContent);
            });
        }

        #region Previous test methods before refactoring them

        /*

        [Fact]
        public async Task NoBookingsTableOnPage_WhenUserHasNoBookings()
        {
            // Registering the Test Authentication Schema to be used in the request
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test",
                            options => options.Role = "Member");

                    // Setting up the DbContext to reset the DB for this Test
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<TennisBookingDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<BookingsPageTests>>();

                    try
                    {
                        // Reset the data
                        DatabaseHelper.ResetDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                                            "database with test data. Error: {Message}", ex.Message);
                    }
                });
            }).CreateClient();

            // (Optional) We can also add an authorization header to the default headers for the
            // test client so that each request specifies the name of the Test scheme.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.GetAsync("");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var content = await HtmlHelpers.GetDocumentAsync(response);
            var table = content.QuerySelector("table");

            Assert.Null(table);
        }

        [Fact]
        public async Task ExpectedBookingsTableRowsOnPage_WhenUserHasBooking()
        {
            var startDate = FixedDateTime.UtcNow.AddDays(5);

            // Registering the Test Authentication Schema to be used in the request
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test",
                            options => options.Role = "Member");

                    // Setting up some data for the Test
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<TennisBookingDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<BookingsPageTests>>();

                    try
                    {
                        // Reset the data
                        DatabaseHelper.ResetDbForTests(db);

                        // Adding the Test data
                        var member = db.Members.Find(1);
                        var court = db.Courts.Find(1);

                        db.CourtBookings.Add(new CourtBooking
                        {
                            Court = court,
                            Member = member,
                            StartDateTime = startDate,
                            EndDateTime = startDate.AddHours(2)
                        });

                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                                            "database with test data. Error: {Message}", ex.Message);
                    }
                });
            }).CreateClient();

            // (Optional) We can also add an authorization header to the default headers for the
            // test client so that each request specifies the name of the Test scheme.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response = await client.GetAsync("");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var content = await HtmlHelpers.GetDocumentAsync(response);
            var table = content.QuerySelector("table");
            var tableBody = table.QuerySelector("tbody");
            var rows = tableBody.QuerySelectorAll("tr");

            Assert.Single(rows);

            // This following approach provides more asurance of the content provided in the oage,
            // BUT it's more coupled to the HTML output, which may require more work to maintain
            // So it's a tradeoff we're making
            Assert.Collection(rows, r =>
            {
                var cells = r.QuerySelectorAll("td");

                Assert.Equal(startDate.ToString("D"), cells[0].TextContent);
                Assert.Equal("Court 1", cells[1].TextContent);
                Assert.Equal("10 am to 12 pm", cells[2].TextContent);
            });
        }

        */
        #endregion
    }
}

